using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// Overlays a small indicator (count or dot) on its <see cref="ContentControl.Content"/>, mirroring
/// the reference API's <c>Badge</c>. The wrapped element goes in <c>Content</c>; the badge value is
/// <see cref="Value"/> (or a <see cref="Dot"/>).
/// </summary>
public class Badge : ContentControl
{
    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<Badge, object?>(nameof(Value));

    /// <summary>Identifies the <see cref="Dot"/> property.</summary>
    public static readonly StyledProperty<bool> DotProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(Dot));

    /// <summary>Identifies the <see cref="Overlap"/> property.</summary>
    public static readonly StyledProperty<bool> OverlapProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(Overlap));

    /// <summary>Identifies the <see cref="Bordered"/> property.</summary>
    public static readonly StyledProperty<bool> BorderedProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(Bordered));

    /// <summary>Identifies the <see cref="Origin"/> property.</summary>
    public static readonly StyledProperty<BadgeOrigin> OriginProperty =
        AvaloniaProperty.Register<Badge, BadgeOrigin>(nameof(Origin), BadgeOrigin.TopRight);

    /// <summary>Identifies the <see cref="Max"/> property.</summary>
    public static readonly StyledProperty<int> MaxProperty =
        AvaloniaProperty.Register<Badge, int>(nameof(Max), 99);

    /// <summary>Identifies the <see cref="Visible"/> property.</summary>
    public static readonly StyledProperty<bool> VisibleProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(Visible), true);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Badge, LoamColor>(nameof(Color), LoamColor.Primary);

    private Border? _badge;
    private TextBlock? _badgeText;
    private IDisposable? _backgroundBinding;
    private IDisposable? _foregroundBinding;
    private IDisposable? _borderBinding;

    /// <summary>The badge value (number or text). Mirrors the reference API's <c>Content</c>.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Show a small dot instead of a value. Mirrors the reference API's <c>Dot</c>.</summary>
    public bool Dot
    {
        get => GetValue(DotProperty);
        set => SetValue(DotProperty, value);
    }

    /// <summary>Pull the badge inward over the content. Mirrors the reference API's <c>Overlap</c>.</summary>
    public bool Overlap
    {
        get => GetValue(OverlapProperty);
        set => SetValue(OverlapProperty, value);
    }

    /// <summary>Draw a surface-colored ring around the badge. Mirrors the reference API's <c>Bordered</c>.</summary>
    public bool Bordered
    {
        get => GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
    }

    /// <summary>Corner placement. Mirrors the reference API's <c>Origin</c>.</summary>
    public BadgeOrigin Origin
    {
        get => GetValue(OriginProperty);
        set => SetValue(OriginProperty, value);
    }

    /// <summary>Cap for numeric values (e.g. <c>99+</c>). Mirrors the reference API's <c>Max</c>.</summary>
    public int Max
    {
        get => GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>Whether the badge is shown. Mirrors the reference API's <c>Visible</c>.</summary>
    public bool Visible
    {
        get => GetValue(VisibleProperty);
        set => SetValue(VisibleProperty, value);
    }

    /// <summary>Badge color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Badge);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _badge = e.NameScope.Find("PART_Badge") as Border;
        _badgeText = e.NameScope.Find("PART_BadgeText") as TextBlock;
        ApplyBadge();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || change.Property == DotProperty ||
            change.Property == OverlapProperty || change.Property == BorderedProperty ||
            change.Property == OriginProperty || change.Property == MaxProperty ||
            change.Property == VisibleProperty || change.Property == ColorProperty)
        {
            ApplyBadge();
        }
    }

    private void ApplyBadge()
    {
        if (_badge is null)
        {
            return;
        }

        var size = Dot ? 10d : 18d;
        _badge.Height = size;
        _badge.MinWidth = size;
        _badge.CornerRadius = new CornerRadius(size / 2);
        _badge.Padding = Dot ? default : new Thickness(5, 0);

        var tokens = SemanticColor.Resolve(Color);
        _backgroundBinding?.Dispose();
        _backgroundBinding = _badge.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));

        var label = FormatValue();
        if (_badgeText is not null)
        {
            _foregroundBinding?.Dispose();
            _foregroundBinding = _badgeText.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(tokens.FillText));
            _badgeText.IsVisible = !Dot;
            _badgeText.Text = label;
        }

        _badge.IsVisible = Visible && (Dot || !string.IsNullOrEmpty(label));

        _borderBinding?.Dispose();
        _borderBinding = null;
        if (Bordered)
        {
            _badge.BorderThickness = new Thickness(2);
            _borderBinding = _badge.Bind(Border.BorderBrushProperty, this.GetResourceObservable(LoamTokens.Surface));
        }
        else
        {
            _badge.BorderThickness = default;
        }

        var (h, v, sx, sy) = OriginParams(Origin);
        _badge.HorizontalAlignment = h;
        _badge.VerticalAlignment = v;
        var offset = Overlap ? size * 0.25 : size * 0.5;
        _badge.RenderTransform = new TranslateTransform(sx * offset, sy * offset);
    }

    private string FormatValue()
    {
        if (Dot)
        {
            return string.Empty;
        }

        return Value switch
        {
            int n => n > Max ? $"{Max}+" : n.ToString(),
            null => string.Empty,
            _ => Value.ToString() ?? string.Empty,
        };
    }

    private static (HorizontalAlignment H, VerticalAlignment V, double Sx, double Sy) OriginParams(BadgeOrigin origin) =>
        origin switch
        {
            BadgeOrigin.TopLeft => (HorizontalAlignment.Left, VerticalAlignment.Top, -1, -1),
            BadgeOrigin.BottomLeft => (HorizontalAlignment.Left, VerticalAlignment.Bottom, -1, 1),
            BadgeOrigin.BottomRight => (HorizontalAlignment.Right, VerticalAlignment.Bottom, 1, 1),
            _ => (HorizontalAlignment.Right, VerticalAlignment.Top, 1, -1),
        };
}
