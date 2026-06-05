using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A Material surface, mirroring the reference API's <c>Paper</c>. Hosts content on a token-driven
/// background with an elevation shadow, optionally squared corners or an outline. Supersedes the
/// Phase-1 <c>Surface</c> smoke control.
/// </summary>
public class Paper : ContentControl
{
    /// <summary>Identifies the <see cref="Elevation"/> property.</summary>
    public static readonly StyledProperty<int> ElevationProperty =
        AvaloniaProperty.Register<Paper, int>(nameof(Elevation), defaultValue: 1);

    /// <summary>Identifies the <see cref="Square"/> property.</summary>
    public static readonly StyledProperty<bool> SquareProperty =
        AvaloniaProperty.Register<Paper, bool>(nameof(Square));

    /// <summary>Identifies the <see cref="Outlined"/> property.</summary>
    public static readonly StyledProperty<bool> OutlinedProperty =
        AvaloniaProperty.Register<Paper, bool>(nameof(Outlined));

    private Border? _root;
    private IDisposable? _backgroundBinding;
    private IDisposable? _cornerBinding;
    private IDisposable? _shadowBinding;
    private IDisposable? _borderBinding;

    /// <summary>Shadow depth (0–25). Mirrors the reference API's <c>Elevation</c>.</summary>
    public int Elevation
    {
        get => GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>Removes corner rounding. Mirrors the reference API's <c>Square</c>.</summary>
    public bool Square
    {
        get => GetValue(SquareProperty);
        set => SetValue(SquareProperty, value);
    }

    /// <summary>Draws a 1px outline and removes the shadow. Mirrors the reference API's <c>Outlined</c>.</summary>
    public bool Outlined
    {
        get => GetValue(OutlinedProperty);
        set => SetValue(OutlinedProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Paper);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find<Border>("PART_Root");
        ApplyVisual();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ElevationProperty ||
            change.Property == SquareProperty ||
            change.Property == OutlinedProperty)
        {
            ApplyVisual();
        }
    }

    private void ApplyVisual()
    {
        if (_root is null)
        {
            return;
        }

        _backgroundBinding?.Dispose();
        _backgroundBinding = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(
            InteractionAssist.TonalSurfaceToken(Elevation, Outlined)));

        _cornerBinding?.Dispose();
        if (Square)
        {
            _cornerBinding = null;
            _root.CornerRadius = new CornerRadius(0);
        }
        else
        {
            _cornerBinding = _root.Bind(Border.CornerRadiusProperty, this.GetResourceObservable(LoamTokens.ShapeMedium));
        }

        _shadowBinding?.Dispose();
        var level = Outlined ? 0 : Elevation;
        _shadowBinding = _root.Bind(Border.BoxShadowProperty, this.GetResourceObservable(LoamTokens.Elevation(level)));

        _borderBinding?.Dispose();
        if (Outlined)
        {
            _root.BorderThickness = new Thickness(1);
            _borderBinding = _root.Bind(Border.BorderBrushProperty, this.GetResourceObservable(LoamTokens.ColorOutlineVariant));
        }
        else
        {
            _borderBinding = null;
            _root.BorderThickness = default;
        }
    }
}
