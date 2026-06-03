using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A vector icon, mirroring MudBlazor's <c>MudIcon</c>. Renders an SVG path (e.g. from
/// <see cref="Loam.Icons"/>) scaled from its <see cref="ViewBox"/> to a size-driven box and filled
/// with a token color. <see cref="LoamColor.Inherit"/> uses the ambient <see cref="Foreground"/>,
/// so icons inside a <see cref="Button"/> automatically pick up the button's text color.
/// </summary>
public class Icon : Control
{
    /// <summary>Identifies the <see cref="Data"/> property.</summary>
    public static readonly StyledProperty<string?> DataProperty =
        AvaloniaProperty.Register<Icon, string?>(nameof(Data));

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Icon, LoamColor>(nameof(Color), LoamColor.Inherit);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<Icon, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="ViewBox"/> property.</summary>
    public static readonly StyledProperty<string> ViewBoxProperty =
        AvaloniaProperty.Register<Icon, string>(nameof(ViewBox), "0 0 24 24");

    /// <summary>The fill brush. Inherited, so icons pick up the ambient text color by default.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<Icon>();

    private Geometry? _geometry;
    private Rect _viewBox = new(0, 0, 24, 24);
    private IDisposable? _colorBinding;

    static Icon()
    {
        AffectsMeasure<Icon>(DataProperty, SizeProperty);
        AffectsRender<Icon>(DataProperty, ForegroundProperty);
    }

    /// <summary>Creates the icon.</summary>
    public Icon() => ApplyColor();

    /// <summary>SVG path data (MudBlazor's <c>MudIcon.Icon</c>; renamed to avoid the type/member clash).</summary>
    public string? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>Semantic color. Mirrors MudBlazor's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Icon size. Mirrors MudBlazor's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Path coordinate space. Mirrors MudBlazor's <c>ViewBox</c> (default <c>0 0 24 24</c>).</summary>
    public string ViewBox
    {
        get => GetValue(ViewBoxProperty);
        set => SetValue(ViewBoxProperty, value);
    }

    /// <summary>The fill brush (inherited).</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>The pixel dimension for a <see cref="LoamSize"/>.</summary>
    public static double PixelSize(LoamSize size) => size switch
    {
        LoamSize.Small => 20,
        LoamSize.Large => 32,
        _ => 24,
    };

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataProperty)
        {
            _geometry = string.IsNullOrEmpty(Data) ? null : Geometry.Parse(Data);
        }
        else if (change.Property == ViewBoxProperty)
        {
            _viewBox = ParseViewBox(ViewBox);
        }
        else if (change.Property == ColorProperty)
        {
            ApplyColor();
        }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        if (_geometry is null)
        {
            return default;
        }

        var size = PixelSize(Size);
        return new Size(size, size);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        if (_geometry is null || Foreground is null || _viewBox.Width <= 0 || _viewBox.Height <= 0)
        {
            return;
        }

        var size = PixelSize(Size);
        var transform = Matrix.CreateTranslation(-_viewBox.X, -_viewBox.Y)
            * Matrix.CreateScale(size / _viewBox.Width, size / _viewBox.Height);
        using (context.PushTransform(transform))
        {
            context.DrawGeometry(Foreground, null, _geometry);
        }
    }

    private void ApplyColor()
    {
        _colorBinding?.Dispose();
        _colorBinding = null;

        var key = Color switch
        {
            LoamColor.Inherit or LoamColor.Transparent => null,
            LoamColor.Default => LoamTokens.Palette(nameof(LoamPalette.ActionDefault)),
            _ => LoamTokens.Palette(Color.ToPaletteName()!),
        };

        if (key is not null)
        {
            _colorBinding = this.Bind(ForegroundProperty, this.GetResourceObservable(key));
        }
    }

    private static Rect ParseViewBox(string value)
    {
        var parts = value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 4 &&
            double.TryParse(parts[0], out var x) && double.TryParse(parts[1], out var y) &&
            double.TryParse(parts[2], out var w) && double.TryParse(parts[3], out var h))
        {
            return new Rect(x, y, w, h);
        }

        return new Rect(0, 0, 24, 24);
    }
}
