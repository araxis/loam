using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Loam;

namespace Loam.Controls;

/// <summary>Default <see cref="ISnackbar"/>: stacks auto-dismissing <see cref="Alert"/> toasts at the bottom-right of the window's overlay layer.</summary>
public sealed class SnackbarService : ISnackbar
{
    private const string HostRootName = "PART_LoamSnackbarRoot";
    private const string HostName = "PART_LoamSnackbarHost";

    private readonly TopLevel _topLevel;
    private StackPanel? _host;

    /// <summary>Creates a service targeting the given window/top level.</summary>
    public SnackbarService(TopLevel topLevel) => _topLevel = topLevel ?? throw new ArgumentNullException(nameof(topLevel));

    /// <summary>Maximum number of visible toasts kept by default.</summary>
    public int MaxVisible { get; set; } = 3;

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

        var toast = new Alert
        {
            Color = options.Severity,
            Variant = Variant.Filled,
            Content = BuildContent(options),
            MinWidth = 280,
            Margin = new Thickness(0, 8, 0, 0),
        };

        DispatcherTimer? timer = null;
        void Dismiss()
        {
            timer?.Stop();
            host.Children.Remove(toast);
        }

        if (toast.Content is Panel panel)
        {
            var actionButton = panel.Children.OfType<Button>().FirstOrDefault();
            if (actionButton is not null)
            {
                actionButton.Click += (_, _) =>
                {
                    options.Action?.Invoke();
                    Dismiss();
                };
                actionButton.Bind(TemplatedControl.ForegroundProperty, toast.GetObservable(TemplatedControl.ForegroundProperty));
            }
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
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 24, 24),
        };

        var root = new Panel { Name = HostRootName, Children = { _host } };
        root.Bind(Layoutable.WidthProperty, layer.GetObservable(Visual.BoundsProperty, b => b.Width));
        root.Bind(Layoutable.HeightProperty, layer.GetObservable(Visual.BoundsProperty, b => b.Height));
        layer.Children.Add(root);
        return _host;
    }

    private static Control BuildContent(SnackbarOptions options)
    {
        var message = new Text
        {
            Text = options.Message,
            Color = LoamColor.Inherit,
            VerticalAlignment = VerticalAlignment.Center,
        };

        if (string.IsNullOrWhiteSpace(options.ActionText))
        {
            return message;
        }

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children =
            {
                message,
                new Button
                {
                    Content = options.ActionText,
                    Variant = Variant.Text,
                    Color = options.Severity,
                    Size = LoamSize.Small,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };
    }

    private static void TrimVisible(StackPanel host, int maxVisible)
    {
        while (host.Children.Count > maxVisible)
        {
            host.Children.RemoveAt(0);
        }
    }
}
