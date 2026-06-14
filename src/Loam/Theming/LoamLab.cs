using Avalonia.Media;

namespace Loam.Theming;

/// <summary>
/// CIELAB color math for tonal-palette generation. "Tone" (Material's sense) is CIE L* (0–100).
/// </summary>
/// <remarks>
/// Tonal palettes (ADR-0012) fix a hue + chroma and vary tone = L*, reducing chroma per tone to stay
/// inside the sRGB gamut. Because WCAG relative luminance equals the XYZ Y channel and L* is a
/// function of Y alone, a color at a given tone has a fixed luminance regardless of hue/chroma — so
/// tone-gap contrast is deterministic, which is what makes generated schemes accessible by
/// construction. This is a tractable approximation of Material You's CAM16/HCT (a CAM16/HCT upgrade is
/// a tracked follow-up).
/// </remarks>
internal static class LoamLab
{
    private const double Xn = 0.95047, Yn = 1.0, Zn = 1.08883; // D65 reference white.
    private const double Epsilon = 216.0 / 24389.0;
    private const double Kappa = 24389.0 / 27.0;

    /// <summary>Lightness (L*, 0–100), chroma, and hue (degrees) of a color.</summary>
    public static (double L, double C, double H) ToLch(Color color)
    {
        var (l, a, b) = ToLab(color);
        var c = Math.Sqrt((a * a) + (b * b));
        var h = Math.Atan2(b, a) * 180.0 / Math.PI;
        if (h < 0)
        {
            h += 360.0;
        }

        return (l, c, h);
    }

    /// <summary>
    /// The sRGB color at the given tone (L*), hue (degrees), and requested chroma — with chroma
    /// reduced just enough (bisection) to fall inside the sRGB gamut. L* is preserved exactly.
    /// </summary>
    public static Color ToneColor(double tone, double chroma, double hue)
    {
        tone = Math.Clamp(tone, 0, 100);

        if (TryLch(tone, chroma, hue, out var full))
        {
            return full;
        }

        // L*,0,0 is always achromatic and in-gamut, so this is a safe lower bound.
        TryLch(tone, 0, hue, out var result);

        double lo = 0, hi = chroma;
        for (var i = 0; i < 24; i++)
        {
            var mid = (lo + hi) / 2;
            if (TryLch(tone, mid, hue, out var candidate))
            {
                result = candidate;
                lo = mid;
            }
            else
            {
                hi = mid;
            }
        }

        return result;
    }

    private static bool TryLch(double l, double c, double hueDeg, out Color color)
    {
        var hueRad = hueDeg * Math.PI / 180.0;
        return TryLabToColor(l, c * Math.Cos(hueRad), c * Math.Sin(hueRad), out color);
    }

    private static (double L, double A, double B) ToLab(Color color)
    {
        var r = ToLinear(color.R);
        var g = ToLinear(color.G);
        var b = ToLinear(color.B);

        var x = ((0.4124564 * r) + (0.3575761 * g) + (0.1804375 * b)) / Xn;
        var y = ((0.2126729 * r) + (0.7151522 * g) + (0.0721750 * b)) / Yn;
        var z = ((0.0193339 * r) + (0.1191920 * g) + (0.9503041 * b)) / Zn;

        var fx = Pivot(x);
        var fy = Pivot(y);
        var fz = Pivot(z);
        return ((116 * fy) - 16, 500 * (fx - fy), 200 * (fy - fz));
    }

    private static bool TryLabToColor(double l, double a, double b, out Color color)
    {
        var fy = (l + 16) / 116;
        var fx = fy + (a / 500);
        var fz = fy - (b / 200);

        var x = InversePivot(fx) * Xn;
        var y = InversePivot(fy) * Yn;
        var z = InversePivot(fz) * Zn;

        var r = (3.2404542 * x) - (1.5371385 * y) - (0.4985314 * z);
        var g = (-0.9692660 * x) + (1.8760108 * y) + (0.0415560 * z);
        var bl = (0.0556434 * x) - (0.2040259 * y) + (1.0572252 * z);

        const double tolerance = 0.0002;
        if (InGamut(r, tolerance) && InGamut(g, tolerance) && InGamut(bl, tolerance))
        {
            color = Color.FromRgb(Encode(r), Encode(g), Encode(bl));
            return true;
        }

        color = default;
        return false;
    }

    private static double Pivot(double t) => t > Epsilon ? Math.Cbrt(t) : ((Kappa * t) + 16) / 116;

    private static double InversePivot(double f)
    {
        var cube = f * f * f;
        return cube > Epsilon ? cube : ((116 * f) - 16) / Kappa;
    }

    private static bool InGamut(double linear, double tolerance) => linear >= -tolerance && linear <= 1 + tolerance;

    private static double ToLinear(byte v)
    {
        var c = v / 255.0;
        return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }

    private static byte Encode(double linear)
    {
        var c = Math.Clamp(linear, 0, 1);
        var encoded = c <= 0.0031308 ? c * 12.92 : (1.055 * Math.Pow(c, 1 / 2.4)) - 0.055;
        return (byte)Math.Clamp(Math.Round(encoded * 255), 0, 255);
    }
}
