using Avalonia.Media;

namespace Loam.Theming;

/// <summary>
/// Color math used to build palettes and derive interaction states (hover/ripple/disabled
/// overlays, on-color contrast). Mirrors the kinds of derivations Material Design does with
/// <c>Color</c>, kept as small pure functions on Avalonia's <see cref="Color"/>.
/// </summary>
public static class LoamColors
{
    /// <summary>Black at the given alpha (0–1). Material Design uses this for light-theme text/lines.</summary>
    public static Color BlackAlpha(double alpha) => Color.FromArgb(ToByte(alpha), 0, 0, 0);

    /// <summary>White at the given alpha (0–1). Used for dark-theme text/lines.</summary>
    public static Color WhiteAlpha(double alpha) => Color.FromArgb(ToByte(alpha), 255, 255, 255);

    /// <summary>Returns the color with its alpha replaced (0–1).</summary>
    public static Color WithAlpha(this Color color, double alpha) =>
        Color.FromArgb(ToByte(alpha), color.R, color.G, color.B);

    /// <summary>Lightens the color toward white by <paramref name="amount"/> (0–1).</summary>
    public static Color Lighten(this Color color, double amount) => Lerp(color, Colors.White, amount);

    /// <summary>Darkens the color toward black by <paramref name="amount"/> (0–1).</summary>
    public static Color Darken(this Color color, double amount) => Lerp(color, Colors.Black, amount);

    /// <summary>
    /// Returns black (87% alpha) or white depending on which reads better on top of
    /// <paramref name="background"/> (WCAG relative-luminance threshold).
    /// </summary>
    public static Color ContrastText(this Color background) =>
        RelativeLuminance(background) > 0.5 ? Color.FromArgb(0xDE, 0, 0, 0) : Colors.White;

    /// <summary>Relative luminance (0–1) using the sRGB coefficients.</summary>
    public static double RelativeLuminance(this Color color)
    {
        static double Channel(byte v)
        {
            var c = v / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
    }

    private static Color Lerp(Color from, Color to, double t)
    {
        t = Math.Clamp(t, 0, 1);
        static byte Mix(byte a, byte b, double t) => (byte)Math.Round(a + ((b - a) * t));
        return Color.FromArgb(from.A, Mix(from.R, to.R, t), Mix(from.G, to.G, t), Mix(from.B, to.B, t));
    }

    private static byte ToByte(double alpha) => (byte)Math.Clamp(Math.Round(alpha * 255), 0, 255);
}
