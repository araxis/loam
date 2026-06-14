using Avalonia.Media;

namespace Loam.Theming;

/// <summary>
/// A Material-style tonal palette: a fixed hue + chroma, sampled by tone (CIE L*, 0–100). Used by the
/// Material You seed → scheme generator (ADR-0012).
/// </summary>
internal readonly struct LoamTonalPalette
{
    private readonly double _hue;
    private readonly double _chroma;

    private LoamTonalPalette(double hue, double chroma)
    {
        _hue = hue;
        _chroma = chroma;
    }

    /// <summary>A palette at a fixed hue (degrees) and chroma.</summary>
    public static LoamTonalPalette FromHueChroma(double hue, double chroma) => new(hue, chroma);

    /// <summary>A palette taking its hue and chroma from <paramref name="color"/>.</summary>
    public static LoamTonalPalette FromColor(Color color)
    {
        var (_, chroma, hue) = LoamLab.ToLch(color);
        return new LoamTonalPalette(hue, chroma);
    }

    /// <summary>The color at the given tone (L*), gamut-clamped to sRGB.</summary>
    public Color Tone(double tone) => LoamLab.ToneColor(tone, _chroma, _hue);
}
