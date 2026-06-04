using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
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

    private Border? _root;
    private Icon? _iconPart;
    private ContentPresenter? _presenter;
    private bool _hover;
    private IDisposable? _background;
    private IDisposable? _foreground;

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
        UpdateIcon();
        ApplyState();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            UpdateIcon();
        }
        else if (change.Property == IsActiveProperty || change.Property == ColorProperty)
        {
            ApplyState();
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
        if (_root is null)
        {
            return;
        }

        var accentName = Color is LoamColor.Default or LoamColor.Inherit
            ? nameof(LoamPalette.Primary)
            : Color.ToPaletteName()!;

        _background?.Dispose();
        _foreground?.Dispose();

        if (IsActive)
        {
            _background = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.PaletteHover(accentName)));
            _foreground = _presenter?.Bind(TextElement.ForegroundProperty, this.GetResourceObservable(LoamTokens.Palette(accentName)));
            if (_iconPart is not null)
            {
                _iconPart.Color = Color is LoamColor.Default or LoamColor.Inherit ? LoamColor.Primary : Color;
            }
        }
        else
        {
            _background = _hover
                ? _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.LinesDefault))
                : null;
            if (!_hover)
            {
                _root.Background = Brushes.Transparent;
            }

            _foreground = _presenter?.Bind(TextElement.ForegroundProperty, this.GetResourceObservable(LoamTokens.TextPrimary));
            if (_iconPart is not null)
            {
                _iconPart.Color = LoamColor.Default;
            }
        }
    }
}

/// <summary>A vertical container of <see cref="NavLink"/>s, mirroring the reference API's <c>NavMenu</c>.</summary>
public class NavMenu : StackPanel
{
}
