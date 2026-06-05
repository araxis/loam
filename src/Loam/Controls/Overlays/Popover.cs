using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A floating panel anchored to a target, mirroring the reference API's <c>Popover</c>. Wraps an Avalonia
/// <see cref="Popup"/>: set <see cref="Content"/>, anchor it to a <see cref="Target"/> with
/// <see cref="Placement"/>, and toggle the two-way <see cref="Open"/>. Light-dismiss closes it.
/// </summary>
public class Popover : Decorator
{
    /// <summary>Identifies the <see cref="Content"/> property.</summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Popover, object?>(nameof(Content));

    /// <summary>Identifies the <see cref="Open"/> property.</summary>
    public static readonly StyledProperty<bool> OpenProperty =
        AvaloniaProperty.Register<Popover, bool>(nameof(Open), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Placement"/> property.</summary>
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.Register<Popover, PlacementMode>(nameof(Placement), PlacementMode.Bottom);

    /// <summary>Identifies the <see cref="Target"/> property.</summary>
    public static readonly StyledProperty<Control?> TargetProperty =
        AvaloniaProperty.Register<Popover, Control?>(nameof(Target));

    private readonly Popup _popup;
    private IInputElement? _restoreFocus;

    /// <summary>Creates the popover.</summary>
    public Popover()
    {
        _popup = new Popup
        {
            IsLightDismissEnabled = true,
            OverlayDismissEventPassThrough = true,
            Placement = Placement,
        };
        _popup.Closed += (_, _) => SetCurrentValue(OpenProperty, false);
        _popup.Bind(Popup.IsOpenProperty, this.GetObservable(OpenProperty));
        Child = _popup;
        InteractionAssist.SetAutomationName(this, "Popover");
    }

    /// <summary>The popover body (wrapped in an elevated <see cref="Paper"/>).</summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>Whether the popover is shown (two-way). Mirrors the reference API's <c>Open</c>.</summary>
    public bool Open
    {
        get => GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    /// <summary>Where the popover sits relative to the target. Mirrors the reference API's anchor/transform origin.</summary>
    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>The control the popover is anchored to (defaults to the popover's parent).</summary>
    public Control? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplyPlacementTarget();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            _popup.Child = Content is null ? null : BuildSurface(Content);
        }
        else if (change.Property == OpenProperty)
        {
            ApplyOpenState();
        }
        else if (change.Property == PlacementProperty)
        {
            _popup.Placement = Placement;
        }
        else if (change.Property == TargetProperty)
        {
            ApplyPlacementTarget();
        }
    }

    private Paper BuildSurface(object content)
    {
        var paper = new Paper
        {
            Elevation = 8,
            Padding = new Thickness(12),
            Content = content,
            Focusable = true,
        };
        paper.KeyDown += (_, args) =>
        {
            if (args.Key == Key.Escape)
            {
                Open = false;
                args.Handled = true;
            }
        };
        InteractionAssist.ApplyZIndex(paper, LoamTokens.ZIndex(nameof(LoamZIndex.Popover)), LoamZIndex.Default.Popover);
        InteractionAssist.SetAutomationName(paper, "Popover");
        return paper;
    }

    private void ApplyOpenState()
    {
        var topLevel = TopLevel.GetTopLevel(Target ?? this);
        if (Open)
        {
            _restoreFocus = topLevel?.FocusManager?.GetFocusedElement();
            if (_popup.Child is Control child)
            {
                child.Focus();
            }
        }
        else if (topLevel is not null)
        {
            InteractionAssist.RestoreFocus(topLevel, _restoreFocus);
            _restoreFocus = null;
        }
    }

    private void ApplyPlacementTarget()
    {
        _popup.PlacementTarget = Target ?? Parent as Control;
    }
}
