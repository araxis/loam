using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;

namespace Loam.Controls;

/// <summary>
/// A circular (or square) content holder for initials, icons or images, mirroring the reference API's
/// <c>Avatar</c>. Token-driven background/foreground per <see cref="Variant"/> and
/// <see cref="Color"/>; circular by default, <see cref="Square"/>/<see cref="Rounded"/> alter the shape.
/// </summary>
public class Avatar : ContentControl
{
    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<Avatar, Variant>(nameof(Variant), Loam.Variant.Filled);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Avatar, LoamColor>(nameof(Color), LoamColor.Default);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<Avatar, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="Square"/> property.</summary>
    public static readonly StyledProperty<bool> SquareProperty =
        AvaloniaProperty.Register<Avatar, bool>(nameof(Square));

    /// <summary>Identifies the <see cref="Rounded"/> property.</summary>
    public static readonly StyledProperty<bool> RoundedProperty =
        AvaloniaProperty.Register<Avatar, bool>(nameof(Rounded));

    private Border? _root;
    private IDisposable? _backgroundBinding;
    private IDisposable? _foregroundBinding;
    private IDisposable? _borderBinding;

    /// <summary>Creates the avatar.</summary>
    public Avatar() => InteractionAssist.SetAutomationName(this, "Avatar");

    /// <summary>Visual style. Mirrors the reference API's <c>Variant</c>.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Semantic color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Avatar size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Square corners. Mirrors the reference API's <c>Square</c>.</summary>
    public bool Square
    {
        get => GetValue(SquareProperty);
        set => SetValue(SquareProperty, value);
    }

    /// <summary>Rounded-rectangle corners (between circle and square). Mirrors the reference API's <c>Rounded</c>.</summary>
    public bool Rounded
    {
        get => GetValue(RoundedProperty);
        set => SetValue(RoundedProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Avatar);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find("PART_Root") as Border;
        ApplyVisual();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == VariantProperty || change.Property == ColorProperty ||
            change.Property == SizeProperty || change.Property == SquareProperty ||
            change.Property == RoundedProperty)
        {
            ApplyVisual();
        }
        else if (change.Property == ContentProperty)
        {
            ApplyAutomationName();
        }
    }

    private void ApplyVisual()
    {
        var px = Size switch
        {
            LoamSize.ExtraSmall => 24d,
            LoamSize.Small => 32d,
            LoamSize.Large => 56d,
            LoamSize.ExtraLarge => 72d,
            _ => 40d,
        };
        Width = px;
        Height = px;

        if (_root is null)
        {
            return;
        }

        _root.CornerRadius = Square ? new CornerRadius(0)
            : Rounded ? new CornerRadius(px * 0.2)
            : new CornerRadius(px / 2);

        var tokens = SemanticColor.Resolve(Color);

        _backgroundBinding?.Dispose();
        _foregroundBinding?.Dispose();
        _borderBinding?.Dispose();
        _borderBinding = null;

        if (Variant == Loam.Variant.Filled)
        {
            _backgroundBinding = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));
            _foregroundBinding = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.FillText));
            _root.BorderThickness = default;
        }
        else
        {
            _root.Background = Brushes.Transparent;
            _backgroundBinding = null;
            _foregroundBinding = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.Accent));
            if (Variant == Loam.Variant.Outlined)
            {
                _root.BorderThickness = new Thickness(1);
                _borderBinding = _root.Bind(Border.BorderBrushProperty, this.GetResourceObservable(tokens.Border));
            }
            else
            {
                _root.BorderThickness = default;
            }
        }
    }

    private void ApplyAutomationName() => InteractionAssist.SetAutomationName(this, Content, "Avatar");
}
