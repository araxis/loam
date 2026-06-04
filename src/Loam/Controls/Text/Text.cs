using Avalonia;
using Avalonia.Controls;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// Typography-aware text, mirroring the reference API's <c>Text</c>. A <see cref="TextBlock"/> whose
/// font and color are driven by theme tokens via <see cref="Typo"/> and <see cref="Color"/>, so it
/// re-styles automatically on theme/variant changes. Set text via the inherited
/// <see cref="TextBlock.Text"/> (or <see cref="TextBlock.Inlines"/>).
/// </summary>
public class Text : TextBlock
{
    /// <summary>Identifies the <see cref="Typo"/> property.</summary>
    public static readonly StyledProperty<Typo> TypoProperty =
        AvaloniaProperty.Register<Text, Typo>(nameof(Typo), Typo.Body1);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Text, LoamColor>(nameof(Color), LoamColor.Default);

    /// <summary>Identifies the <see cref="GutterBottom"/> property.</summary>
    public static readonly StyledProperty<bool> GutterBottomProperty =
        AvaloniaProperty.Register<Text, bool>(nameof(GutterBottom));

    private IDisposable? _fontSizeBinding;
    private IDisposable? _fontWeightBinding;
    private IDisposable? _foregroundBinding;

    /// <summary>Creates the control and wires its font family to the theme.</summary>
    public Text()
    {
        this.Bind(FontFamilyProperty, this.GetResourceObservable(LoamTokens.FontFamily));
        ApplyTypo();
        ApplyColor();
    }

    /// <summary>The typographic role. Mirrors the reference API's <c>Typo</c>.</summary>
    public Typo Typo
    {
        get => GetValue(TypoProperty);
        set => SetValue(TypoProperty, value);
    }

    /// <summary>The semantic text color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Adds bottom spacing after the text. Mirrors the reference API's <c>GutterBottom</c>.</summary>
    public bool GutterBottom
    {
        get => GetValue(GutterBottomProperty);
        set => SetValue(GutterBottomProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Text);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TypoProperty)
        {
            ApplyTypo();
        }
        else if (change.Property == ColorProperty)
        {
            ApplyColor();
        }
        else if (change.Property == GutterBottomProperty)
        {
            Margin = GutterBottom ? new Thickness(0, 0, 0, 8) : default;
        }
    }

    private void ApplyTypo()
    {
        _fontSizeBinding?.Dispose();
        _fontWeightBinding?.Dispose();

        if (Typo == Typo.Inherit)
        {
            _fontSizeBinding = null;
            _fontWeightBinding = null;
            return;
        }

        var name = Typo.ToString();
        _fontSizeBinding = this.Bind(FontSizeProperty, this.GetResourceObservable(LoamTokens.TypographyFontSize(name)));
        _fontWeightBinding = this.Bind(FontWeightProperty, this.GetResourceObservable(LoamTokens.TypographyFontWeight(name)));
    }

    private void ApplyColor()
    {
        _foregroundBinding?.Dispose();

        var key = Color switch
        {
            LoamColor.Default or LoamColor.Inherit => LoamTokens.TextPrimary,
            LoamColor.Transparent => null,
            _ => LoamTokens.Palette(Color.ToPaletteName()!),
        };

        _foregroundBinding = key is null
            ? null
            : this.Bind(ForegroundProperty, this.GetResourceObservable(key));
    }
}
