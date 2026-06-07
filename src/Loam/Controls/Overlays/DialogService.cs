using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// Default <see cref="IDialogService"/>: renders a scrim + centered <see cref="Paper"/> dialog in the
/// window's <see cref="OverlayLayer"/>.
/// </summary>
public sealed class DialogService : IDialogService
{
    private readonly TopLevel _topLevel;

    /// <summary>Creates a service targeting the given window/top level.</summary>
    public DialogService(TopLevel topLevel) => _topLevel = topLevel ?? throw new ArgumentNullException(nameof(topLevel));

    /// <summary>Creates a service for the window hosting <paramref name="visual"/>.</summary>
    public static DialogService For(Visual visual) =>
        new(TopLevel.GetTopLevel(visual) ?? throw new InvalidOperationException("The visual is not attached to a window."));

    /// <inheritdoc />
    public Task<DialogResult> ShowAsync(string? title, Func<DialogInstance, Control> content, DialogOptions? options = null)
    {
        options ??= DialogOptions.Default;
        var layer = OverlayLayer.GetOverlayLayer(_topLevel)
            ?? throw new InvalidOperationException("No overlay layer is available.");

        var completion = new TaskCompletionSource<DialogResult>();
        Panel? root = null;
        var restoreFocus = _topLevel.FocusManager?.GetFocusedElement();
        var instance = new DialogInstance(completion, () =>
        {
            if (root is not null)
            {
                layer.Children.Remove(root);
            }

            InteractionAssist.RestoreFocus(_topLevel, restoreFocus);
        });

        var dialog = BuildDialog(title, content(instance), options);
        InteractionAssist.SetAutomationName(dialog, title, "Dialog");
        AutomationProperties.SetHelpText(dialog, options.DismissOnEscape ? "Modal dialog, Escape dismissible" : "Modal dialog, Escape disabled");

        var scrim = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Focusable = true,
        };
        AutomationProperties.SetName(scrim, "Dialog backdrop");
        AutomationProperties.SetHelpText(scrim, options.DismissOnScrimClick ? "Click to dismiss" : "Dismiss disabled");
        scrim.Bind(Border.BackgroundProperty, _topLevel.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.OverlayDark))));
        if (options.DismissOnScrimClick)
        {
            scrim.PointerPressed += (_, args) =>
            {
                instance.Cancel();
                args.Handled = true;
            };
        }

        root = new Panel { Focusable = true, Children = { scrim, dialog } };
        AutomationProperties.SetName(root, title ?? "Dialog");
        AutomationProperties.SetHelpText(root, options.DismissOnEscape ? "Modal layer, Escape dismissible" : "Modal layer, Escape disabled");
        root.KeyDown += (_, args) =>
        {
            if (options.DismissOnEscape && args.Key == Key.Escape)
            {
                instance.Cancel();
                args.Handled = true;
            }
        };
        InteractionAssist.ApplyZIndex(root, LoamTokens.ZIndex(nameof(LoamZIndex.Dialog)), LoamZIndex.Default.Dialog);
        root.Bind(Layoutable.WidthProperty, layer.GetObservable(Visual.BoundsProperty, b => b.Width));
        root.Bind(Layoutable.HeightProperty, layer.GetObservable(Visual.BoundsProperty, b => b.Height));
        layer.Children.Add(root);
        FocusInitial(dialog, root, options.AutoFocus);
        if (options.AutoFocus)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (root.Parent is not null)
                {
                    FocusInitial(dialog, root, autoFocus: true);
                }
            });
        }

        return completion.Task;
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmAsync(string title, string message, string okText = "OK", string cancelText = "Cancel")
    {
        var result = await ShowAsync(title, instance => BuildConfirm(instance, message, okText, cancelText));
        return !result.Canceled;
    }

    /// <inheritdoc />
    public async Task<bool?> MessageBoxAsync(string title, string message, string yesText = "OK", string? noText = null, string? cancelText = null)
    {
        var result = await ShowAsync(title, instance => BuildMessageBox(instance, message, yesText, noText, cancelText));
        return result.Canceled ? null : (bool?)result.Data;
    }

    private static StackPanel BuildMessageBox(DialogInstance instance, string message, string yesText, string? noText, string? cancelText)
    {
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        if (!string.IsNullOrEmpty(cancelText))
        {
            var cancel = new Button { Content = cancelText, Variant = Variant.Text, Color = LoamColor.Primary };
            cancel.Click += (_, _) => instance.Cancel();
            actions.Children.Add(cancel);
        }

        if (!string.IsNullOrEmpty(noText))
        {
            var no = new Button { Content = noText, Variant = Variant.Text, Color = LoamColor.Primary };
            no.Click += (_, _) => instance.Ok(false);
            actions.Children.Add(no);
        }

        var yes = new Button { Content = yesText, Variant = Variant.Text, Color = LoamColor.Primary };
        yes.Click += (_, _) => instance.Ok(true);
        actions.Children.Add(yes);

        return new StackPanel
        {
            Spacing = 20,
            Children = { new Text { Text = message, Typo = Typo.Body1 }, actions },
        };
    }

    private static Paper BuildDialog(string? title, Control body, DialogOptions options)
    {
        var stack = new StackPanel { Spacing = 16 };
        if (!string.IsNullOrEmpty(title))
        {
            stack.Children.Add(new Text { Text = title, Typo = Typo.HeadlineSmall });
        }

        stack.Children.Add(body);

        var paper = new Paper
        {
            Elevation = 8,
            Margin = options.Margin,
            Padding = options.Padding,
            Content = stack,
            MinWidth = options.MinWidth,
            MaxWidth = options.MaxWidth,
            MaxHeight = options.MaxHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (options.Width is { } width)
        {
            paper.Width = width;
        }

        return paper;
    }

    private static StackPanel BuildConfirm(DialogInstance instance, string message, string okText, string cancelText)
    {
        var cancel = new Button { Content = cancelText, Variant = Variant.Text, Color = LoamColor.Primary };
        cancel.Click += (_, _) => instance.Cancel();

        var ok = new Button { Content = okText, Variant = Variant.Text, Color = LoamColor.Primary };
        ok.Click += (_, _) => instance.Ok(true);

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = { cancel, ok },
        };

        return new StackPanel
        {
            Spacing = 20,
            Children = { new Text { Text = message, Typo = Typo.Body1 }, actions },
        };
    }

    private static void FocusInitial(Control dialog, InputElement fallback, bool autoFocus)
    {
        if (autoFocus)
        {
            foreach (var candidate in dialog.GetVisualDescendants().OfType<Control>())
            {
                if (candidate.Focusable && candidate.IsEnabled && candidate.IsVisible)
                {
                    candidate.Focus();
                    return;
                }
            }
        }

        fallback.Focus();
    }
}
