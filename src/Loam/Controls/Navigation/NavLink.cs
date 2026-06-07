using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A navigation entry, mirroring the reference API's <c>NavLink</c>. A clickable row with an optional
/// leading <see cref="Icon"/> and content label that highlights in <see cref="Color"/> when
/// <see cref="IsActive"/>, tints on hover otherwise. Clicking invokes <see cref="OnClick"/> and
/// launches <see cref="Href"/> if set.
/// </summary>
public class NavLink : ContentControl
{
    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<NavLink, string?>(nameof(Icon));

    /// <summary>Identifies the <see cref="IsActive"/> property.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<NavLink, bool>(nameof(IsActive));

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<NavLink, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<NavLink, string?>(nameof(Label));

    private Border? _root;
    private Icon? _iconPart;
    private ContentPresenter? _presenter;
    private bool _hover;
    private IDisposable? _background;
    private IDisposable? _foreground;

    /// <summary>Creates the navigation link.</summary>
    public NavLink()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
        GotFocus += (_, _) => ApplyState();
        LostFocus += (_, _) => ApplyState();
    }

    /// <summary>Leading icon path. Mirrors the reference API's <c>Icon</c>.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Whether this is the current entry. Mirrors the reference API's active state.</summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>Active accent color. Mirrors the reference API's <c>IconColor</c>/active color.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Accessible label used when visible content is hidden or not descriptive enough.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Invoked when the entry is clicked. Mirrors the reference API's <c>OnClick</c>.</summary>
    public Action? OnClick { get; set; }

    /// <summary>An optional URL launched on click. Mirrors the reference API's <c>Href</c>.</summary>
    public string? Href { get; set; }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NavLink);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find("PART_Root") as Border;
        _iconPart = e.NameScope.Find("PART_Icon") as Icon;
        _presenter = e.NameScope.Find("PART_ContentPresenter") as ContentPresenter;
        if (_root is not null)
        {
            _root.Focusable = true;
            _root.GotFocus += (_, _) => ApplyState();
            _root.LostFocus += (_, _) => ApplyState();
        }

        UpdateIcon();
        ApplyState();
        UpdateAutomation();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            UpdateIcon();
        }
        else if (change.Property == IsActiveProperty || change.Property == ColorProperty || change.Property == IsEnabledProperty)
        {
            ApplyState();
        }
        else if (change.Property == ContentProperty || change.Property == LabelProperty)
        {
            UpdateAutomation();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _hover = true;
        ApplyState();
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _hover = false;
        ApplyState();
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEnabled)
        {
            return;
        }

        Focus();
        Activate();
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsEnabled && InteractionAssist.IsActivationKey(e.Key))
        {
            Activate();
            e.Handled = true;
        }
    }

    private void Activate()
    {
        OnClick?.Invoke();
        if (!string.IsNullOrWhiteSpace(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
        {
            TopLevel.GetTopLevel(this)?.Launcher.LaunchUriAsync(uri);
        }
    }

    private void UpdateIcon()
    {
        if (_iconPart is not null)
        {
            _iconPart.Data = Icon;
            _iconPart.IsVisible = !string.IsNullOrEmpty(Icon);
        }
    }

    private void ApplyState()
    {
        var root = _root;
        if (root is null)
        {
            return;
        }

        var accentName = Color is LoamColor.Default or LoamColor.Inherit
            ? nameof(LoamPalette.Primary)
            : Color.ToPaletteName()!;
        var focused = IsFocused || root.IsFocused;

        _background?.Dispose();
        _foreground?.Dispose();
        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);

        if (IsActive)
        {
            _background = root.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.PaletteHover(accentName)));
            _foreground = _presenter?.Bind(TextElement.ForegroundProperty, this.GetResourceObservable(LoamTokens.Palette(accentName)));
            if (_iconPart is not null)
            {
                _iconPart.Color = Color is LoamColor.Default or LoamColor.Inherit ? LoamColor.Primary : Color;
            }
        }
        else
        {
            _background = focused
                ? root.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.PaletteFocus(accentName)))
                : _hover
                ? root.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.PaletteHover(accentName)))
                : null;
            if (!_hover && !focused)
            {
                root.Background = Brushes.Transparent;
            }

            _foreground = _presenter?.Bind(TextElement.ForegroundProperty, this.GetResourceObservable(LoamTokens.TextPrimary));
            if (_iconPart is not null)
            {
                _iconPart.Color = LoamColor.Default;
            }
        }
    }

    private void UpdateAutomation() => InteractionAssist.SetAutomationName(this, Label, Content, Href);
}

/// <summary>A vertical container of <see cref="NavLink"/>s, mirroring the reference API's <c>NavMenu</c>.</summary>
public class NavMenu : StackPanel
{
    /// <summary>Creates the navigation menu.</summary>
    public NavMenu()
    {
        Orientation = Avalonia.Layout.Orientation.Vertical;
        Spacing = 2;
        AutomationProperties.SetName(this, "Navigation menu");
    }
}
