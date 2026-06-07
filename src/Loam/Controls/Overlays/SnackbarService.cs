using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Default <see cref="ISnackbar"/>: stacks auto-dismissing toasts at the bottom-right of the window's overlay layer.</summary>
public sealed class SnackbarService : ISnackbar
{
    private const string HostRootName = "PART_LoamSnackbarRoot";
    private const string HostName = "PART_LoamSnackbarHost";
    private static readonly AttachedProperty<Action?> DismissActionProperty =
        AvaloniaProperty.RegisterAttached<SnackbarService, Control, Action?>("DismissAction");

    private readonly TopLevel _topLevel;
    private StackPanel? _host;

    /// <summary>Creates a service targeting the given window/top level.</summary>
    public SnackbarService(TopLevel topLevel) => _topLevel = topLevel ?? throw new ArgumentNullException(nameof(topLevel));

    /// <summary>Maximum number of visible toasts kept by default.</summary>
    public int MaxVisible { get; set; } = 3;

    /// <summary>Default position for snackbar stacks.</summary>
    public SnackbarPosition Position { get; set; } = SnackbarPosition.BottomRight;

    /// <summary>Creates a service for the window hosting <paramref name="visual"/>.</summary>
    public static SnackbarService For(Visual visual) =>
        new(TopLevel.GetTopLevel(visual) ?? throw new InvalidOperationException("The visual is not attached to a window."));

    /// <inheritdoc />
    public void Add(string message, LoamColor severity = LoamColor.Info, TimeSpan? duration = null) =>
        Add(new SnackbarOptions(message) { Severity = severity, Duration = duration });

    /// <inheritdoc />
    public void Add(SnackbarOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var host = EnsureHost();
        if (host is null)
        {
            return;
        }

        ApplyHostPosition(host, options.Position ?? Position);
        var (content, message, actionButton, dismissButton) = BuildContent(options);
        var position = options.Position ?? Position;
        var toast = new Border
        {
            Child = content,
            MinWidth = 280,
            MaxWidth = 672,
            MinHeight = 48,
            Padding = new Thickness(16, 4, actionButton is null && dismissButton is null ? 16 : 8, 4),
            Margin = new Thickness(0, 8, 0, 0),
            Focusable = true,
        };
        toast.Bind(Border.BackgroundProperty, toast.GetResourceObservable(LoamTokens.ColorScheme(nameof(LoamColorScheme.InverseSurface))));
        toast.Bind(Border.CornerRadiusProperty, toast.GetResourceObservable(LoamTokens.ShapeExtraSmall));
        message.Bind(TextBlock.ForegroundProperty, toast.GetResourceObservable(LoamTokens.ColorScheme(nameof(LoamColorScheme.InverseOnSurface))));
        InteractionAssist.SetAutomationName(toast, options.Message);
        AutomationProperties.SetHelpText(toast, BuildHelpText(options, position));

        DispatcherTimer? timer = null;
        var dismissed = false;
        void Dismiss()
        {
            if (dismissed)
            {
                return;
            }

            dismissed = true;
            timer?.Stop();
            SetDismissAction(toast, null);
            host.Children.Remove(toast);
        }

        SetDismissAction(toast, Dismiss);
        toast.KeyDown += (_, args) =>
        {
            if (args.Key == Key.Escape)
            {
                Dismiss();
                args.Handled = true;
            }
        };

        if (actionButton is not null)
        {
            actionButton.Click += (_, _) =>
            {
                if (dismissed)
                {
                    return;
                }

                options.Action?.Invoke();
                Dismiss();
            };
            actionButton.Bind(TemplatedControl.ForegroundProperty, toast.GetResourceObservable(LoamTokens.ColorScheme(nameof(LoamColorScheme.InversePrimary))));
        }

        if (dismissButton is not null)
        {
            dismissButton.Click += (_, _) => Dismiss();
            dismissButton.Bind(TemplatedControl.ForegroundProperty, toast.GetResourceObservable(LoamTokens.ColorScheme(nameof(LoamColorScheme.InversePrimary))));
        }

        host.Children.Add(toast);
        TrimVisible(host, Math.Max(1, options.MaxVisible ?? MaxVisible));

        var duration = options.Duration ?? TimeSpan.FromSeconds(4);
        if (duration == Timeout.InfiniteTimeSpan)
        {
            return;
        }

        timer = new DispatcherTimer { Interval = duration };
        timer.Tick += (_, _) => Dismiss();
        timer.Start();
    }

    private StackPanel? EnsureHost()
    {
        if (_host is { Parent: not null })
        {
            return _host;
        }

        var layer = OverlayLayer.GetOverlayLayer(_topLevel);
        if (layer is null)
        {
            return null;
        }

        var existingRoot = layer.Children.OfType<Panel>().FirstOrDefault(child => child.Name == HostRootName);
        if (existingRoot?.Children.OfType<StackPanel>().FirstOrDefault(child => child.Name == HostName) is { } existingHost)
        {
            _host = existingHost;
            return _host;
        }

        _host = new StackPanel
        {
            Name = HostName,
            Margin = new Thickness(0, 0, 24, 24),
        };
        ApplyHostPosition(_host, Position);

        var root = new Panel { Name = HostRootName, Children = { _host } };
        InteractionAssist.ApplyZIndex(root, LoamTokens.ZIndex(nameof(LoamZIndex.Snackbar)), LoamZIndex.Default.Snackbar);
        root.Bind(Layoutable.WidthProperty, layer.GetObservable(Visual.BoundsProperty, b => b.Width));
        root.Bind(Layoutable.HeightProperty, layer.GetObservable(Visual.BoundsProperty, b => b.Height));
        layer.Children.Add(root);
        return _host;
    }

    private static (Control Content, Text Message, Button? ActionButton, Button? DismissButton) BuildContent(SnackbarOptions options)
    {
        var message = new Text
        {
            Text = options.Message,
            Color = LoamColor.Inherit,
            Typo = Typo.Body2,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
        };

        if (string.IsNullOrWhiteSpace(options.ActionText) && string.IsNullOrWhiteSpace(options.DismissText))
        {
            return (message, message, null, null);
        }

        Button? action = null;
        if (!string.IsNullOrWhiteSpace(options.ActionText))
        {
            action = new Button
            {
                Content = options.ActionText,
                Variant = Variant.Text,
                Color = LoamColor.Primary,
                Size = LoamSize.Medium,
                VerticalAlignment = VerticalAlignment.Center,
            };
            InteractionAssist.SetAutomationName(action, options.ActionText);
        }

        Button? dismiss = null;
        if (!string.IsNullOrWhiteSpace(options.DismissText))
        {
            dismiss = new Button
            {
                Content = options.DismissText,
                Variant = Variant.Text,
                Color = LoamColor.Primary,
                Size = LoamSize.Medium,
                VerticalAlignment = VerticalAlignment.Center,
            };
            InteractionAssist.SetAutomationName(dismiss, options.DismissText);
        }

        message.Margin = new Thickness(0, 0, 24, 0);

        var layout = new global::Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
            Children = { message },
        };
        if (action is not null)
        {
            global::Avalonia.Controls.Grid.SetColumn(action, 1);
            layout.Children.Add(action);
        }

        if (dismiss is not null)
        {
            global::Avalonia.Controls.Grid.SetColumn(dismiss, 2);
            layout.Children.Add(dismiss);
        }

        return (layout, message, action, dismiss);
    }

    private static void TrimVisible(StackPanel host, int maxVisible)
    {
        while (host.Children.Count > maxVisible)
        {
            if (host.Children[0] is Control control && GetDismissAction(control) is { } dismiss)
            {
                dismiss();
            }
            else
            {
                host.Children.RemoveAt(0);
            }
        }
    }

    private static string BuildHelpText(SnackbarOptions options, SnackbarPosition position)
    {
        var parts = new List<string> { $"Snackbar at {position}", "Escape dismissible" };
        if (!string.IsNullOrWhiteSpace(options.ActionText))
        {
            parts.Add($"Action {options.ActionText}");
        }

        if (!string.IsNullOrWhiteSpace(options.DismissText))
        {
            parts.Add($"Dismiss {options.DismissText}");
        }

        return string.Join(", ", parts);
    }

    private static void SetDismissAction(Control control, Action? action) =>
        control.SetValue(DismissActionProperty, action);

    private static Action? GetDismissAction(Control control) =>
        control.GetValue(DismissActionProperty);

    private static void ApplyHostPosition(StackPanel host, SnackbarPosition position)
    {
        host.HorizontalAlignment = position switch
        {
            SnackbarPosition.BottomLeft or SnackbarPosition.TopLeft => HorizontalAlignment.Left,
            SnackbarPosition.BottomCenter or SnackbarPosition.TopCenter => HorizontalAlignment.Center,
            _ => HorizontalAlignment.Right,
        };
        host.VerticalAlignment = position is SnackbarPosition.TopLeft or SnackbarPosition.TopRight or SnackbarPosition.TopCenter
            ? VerticalAlignment.Top
            : VerticalAlignment.Bottom;
        host.Margin = position switch
        {
            SnackbarPosition.TopLeft => new Thickness(24, 24, 0, 0),
            SnackbarPosition.TopRight => new Thickness(0, 24, 24, 0),
            SnackbarPosition.TopCenter => new Thickness(24, 24),
            SnackbarPosition.BottomLeft => new Thickness(24, 0, 0, 24),
            SnackbarPosition.BottomCenter => new Thickness(24),
            _ => new Thickness(0, 0, 24, 24),
        };
    }
}
