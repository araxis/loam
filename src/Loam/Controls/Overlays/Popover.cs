using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    /// <summary>Identifies the <see cref="Trigger"/> property.</summary>
    public static readonly StyledProperty<Control?> TriggerProperty =
        AvaloniaProperty.Register<Popover, Control?>(nameof(Trigger));

    private readonly Popup _popup;
    private IInputElement? _restoreFocus;
    private Control? _attachedTrigger;
    private bool _isAttached;
    private bool _syncingPopup;

    /// <summary>Creates the popover.</summary>
    public Popover()
    {
        _popup = new Popup
        {
            IsLightDismissEnabled = true,
            OverlayDismissEventPassThrough = true,
            Placement = Placement,
        };
        _popup.Closed += (_, _) =>
        {
            if (!_syncingPopup)
            {
                SetCurrentValue(OpenProperty, false);
            }
        };
        Child = _popup;
        Focusable = true;
        InteractionAssist.SetAutomationName(this, "Popover");
        ApplyAutomationState();
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

    /// <summary>Optional control that toggles the popover when clicked or keyboard-activated.</summary>
    public Control? Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        AttachTrigger();
        ApplyPlacementTarget();
        SyncPopupOpen();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            _popup.Child = Content is null ? null : BuildSurface(Content);
            if (Open)
            {
                SyncPopupOpen();
            }
        }
        else if (change.Property == OpenProperty)
        {
            if (Open && !IsEnabled)
            {
                SetCurrentValue(OpenProperty, false);
                return;
            }

            SyncPopupOpen();
        }
        else if (change.Property == PlacementProperty)
        {
            _popup.Placement = Placement;
            ApplyAutomationState();
            if (Open)
            {
                SyncPopupOpen();
            }
        }
        else if (change.Property == TargetProperty)
        {
            ApplyPlacementTarget();
            if (Open)
            {
                SyncPopupOpen();
            }
        }
        else if (change.Property == TriggerProperty)
        {
            AttachTrigger();
            ApplyPlacementTarget();
            if (Open)
            {
                SyncPopupOpen();
            }
        }
        else if (change.Property == IsEnabledProperty)
        {
            if (!IsEnabled && Open)
            {
                SetCurrentValue(OpenProperty, false);
            }

            ApplyAutomationState();
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        SetPopupOpen(false);
        DetachTrigger();
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled || e.Handled || !Open || e.Key != Key.Escape)
        {
            return;
        }

        Open = false;
        e.Handled = true;
    }

    private Paper BuildSurface(object content)
    {
        var paper = new Paper
        {
            Elevation = 3,
            Padding = new Thickness(16),
            Shape = SurfaceShape.ExtraSmall,
            ClipToBounds = true,
            Content = content,
            Focusable = true,
        };
        paper.KeyDown += (_, args) =>
        {
            if (IsEnabled && args.Key == Key.Escape)
            {
                Open = false;
                args.Handled = true;
            }
        };
        InteractionAssist.ApplyZIndex(paper, LoamTokens.ZIndex(nameof(LoamZIndex.Popover)), LoamZIndex.Default.Popover);
        InteractionAssist.SetAutomationName(paper, "Popover");
        AutomationProperties.SetHelpText(paper, "Popover content, Escape closes");
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
        _popup.PlacementTarget = Target ?? Trigger ?? Parent as Control;
    }

    private void SyncPopupOpen()
    {
        ApplyAutomationState();
        if (!_isAttached)
        {
            return;
        }

        ApplyPlacementTarget();
        SetPopupOpen(Open);
        ApplyOpenState();
    }

    private void SetPopupOpen(bool open)
    {
        if (_popup.IsOpen == open)
        {
            return;
        }

        _syncingPopup = true;
        try
        {
            _popup.IsOpen = open;
        }
        finally
        {
            _syncingPopup = false;
        }
    }

    private void AttachTrigger()
    {
        DetachTrigger();

        _attachedTrigger = Trigger;
        if (_attachedTrigger is null)
        {
            return;
        }

        if (_attachedTrigger is global::Avalonia.Controls.Button button)
        {
            button.Click += OnTriggerClick;
        }
        else
        {
            _attachedTrigger.PointerReleased += OnTriggerPointerReleased;
        }

        _attachedTrigger.KeyDown += OnTriggerKeyDown;
    }

    private void DetachTrigger()
    {
        if (_attachedTrigger is null)
        {
            return;
        }

        if (_attachedTrigger is global::Avalonia.Controls.Button oldButton)
        {
            oldButton.Click -= OnTriggerClick;
        }
        else
        {
            _attachedTrigger.PointerReleased -= OnTriggerPointerReleased;
        }

        _attachedTrigger.KeyDown -= OnTriggerKeyDown;
        _attachedTrigger = null;
    }

    private void OnTriggerClick(object? sender, RoutedEventArgs e)
    {
        if (CanToggle(sender))
        {
            Open = !Open;
        }
    }

    private void OnTriggerPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left && CanToggle(sender))
        {
            Open = !Open;
            e.Handled = true;
        }
    }

    private void OnTriggerKeyDown(object? sender, KeyEventArgs e)
    {
        if (InteractionAssist.IsActivationKey(e.Key) && CanToggle(sender))
        {
            Open = !Open;
            e.Handled = true;
        }
    }

    private bool CanToggle(object? sender) => IsEnabled && sender is Control { IsEnabled: true };

    private void ApplyAutomationState() =>
        AutomationProperties.SetHelpText(this, $"{(Open ? "Open" : "Closed")}, {Placement} placement");
}
