using System.Globalization;
using System.Text;
using Avalonia.Media;

namespace Loam.Theming;

/// <summary>
/// Material elevation shadows, levels 0–25, mirroring the reference API's <c>Shadow</c> table (the MUI
/// elevation set, verified against Material Design v9.5.0). The source values are CSS multi-layer
/// box-shadows; <see cref="ParseCss"/> converts them to Avalonia <see cref="BoxShadows"/>.
/// </summary>
public sealed class LoamShadows
{
    private readonly IReadOnlyList<BoxShadows> _elevations;

    private LoamShadows(IReadOnlyList<BoxShadows> elevations) => _elevations = elevations;

    /// <summary>Highest supported elevation level.</summary>
    public int MaxElevation => _elevations.Count - 1;

    /// <summary>Returns the shadow for an elevation level (clamped to 0..<see cref="MaxElevation"/>).</summary>
    public BoxShadows this[int level] => _elevations[Math.Clamp(level, 0, MaxElevation)];

    /// <summary>The Loam default elevation set.</summary>
    public static LoamShadows Default { get; }

    // Static ctor runs after the Css field initializer regardless of textual order.
    static LoamShadows() => Default = new LoamShadows(Css.Select(ParseCss).ToArray());

    /// <summary>
    /// Converts a CSS box-shadow string (one or more comma-separated <c>x y blur spread rgba()</c>
    /// layers) to Avalonia <see cref="BoxShadows"/>. An empty string yields "no shadow".
    /// </summary>
    public static BoxShadows ParseCss(string css)
    {
        if (string.IsNullOrWhiteSpace(css))
        {
            return default;
        }

        var layers = SplitTopLevel(css).Select(ParseLayer).ToArray();
        return layers.Length == 1
            ? new BoxShadows(layers[0])
            : new BoxShadows(layers[0], layers[1..]);
    }

    private static BoxShadow ParseLayer(string layer)
    {
        layer = layer.Trim();
        var colorStart = layer.IndexOf("rgba", StringComparison.OrdinalIgnoreCase);
        var metrics = layer[..colorStart].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var color = ParseRgba(layer[colorStart..]);

        return new BoxShadow
        {
            OffsetX = Px(metrics[0]),
            OffsetY = Px(metrics[1]),
            Blur = Px(metrics[2]),
            Spread = Px(metrics[3]),
            Color = color,
        };
    }

    private static Color ParseRgba(string rgba)
    {
        var inner = rgba[(rgba.IndexOf('(') + 1)..rgba.IndexOf(')')];
        var p = inner.Split(',', StringSplitOptions.TrimEntries);
        var a = (byte)Math.Clamp(Math.Round(double.Parse(p[3], CultureInfo.InvariantCulture) * 255), 0, 255);
        return Color.FromArgb(a, byte.Parse(p[0], CultureInfo.InvariantCulture),
            byte.Parse(p[1], CultureInfo.InvariantCulture), byte.Parse(p[2], CultureInfo.InvariantCulture));
    }

    private static double Px(string token) =>
        double.Parse(token.Replace("px", string.Empty), CultureInfo.InvariantCulture);

    private static List<string> SplitTopLevel(string css)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var depth = 0;
        foreach (var ch in css)
        {
            switch (ch)
            {
                case '(':
                    depth++;
                    sb.Append(ch);
                    break;
                case ')':
                    depth--;
                    sb.Append(ch);
                    break;
                case ',' when depth == 0:
                    result.Add(sb.ToString());
                    sb.Clear();
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }

        if (sb.Length > 0)
        {
            result.Add(sb.ToString());
        }

        return result;
    }

    // Levels 0–25. Index 0 = none. Values verified against Material Design v9.5.0 Shadow.cs.
    private static readonly string[] Css =
    [
        string.Empty,
        "0px 2px 1px -1px rgba(0,0,0,0.2),0px 1px 1px 0px rgba(0,0,0,0.14),0px 1px 3px 0px rgba(0,0,0,0.12)",
        "0px 3px 1px -2px rgba(0,0,0,0.2),0px 2px 2px 0px rgba(0,0,0,0.14),0px 1px 5px 0px rgba(0,0,0,0.12)",
        "0px 3px 3px -2px rgba(0,0,0,0.2),0px 3px 4px 0px rgba(0,0,0,0.14),0px 1px 8px 0px rgba(0,0,0,0.12)",
        "0px 2px 4px -1px rgba(0,0,0,0.2),0px 4px 5px 0px rgba(0,0,0,0.14),0px 1px 10px 0px rgba(0,0,0,0.12)",
        "0px 3px 5px -1px rgba(0,0,0,0.2),0px 5px 8px 0px rgba(0,0,0,0.14),0px 1px 14px 0px rgba(0,0,0,0.12)",
        "0px 3px 5px -1px rgba(0,0,0,0.2),0px 6px 10px 0px rgba(0,0,0,0.14),0px 1px 18px 0px rgba(0,0,0,0.12)",
        "0px 4px 5px -2px rgba(0,0,0,0.2),0px 7px 10px 1px rgba(0,0,0,0.14),0px 2px 16px 1px rgba(0,0,0,0.12)",
        "0px 5px 5px -3px rgba(0,0,0,0.2),0px 8px 10px 1px rgba(0,0,0,0.14),0px 3px 14px 2px rgba(0,0,0,0.12)",
        "0px 5px 6px -3px rgba(0,0,0,0.2),0px 9px 12px 1px rgba(0,0,0,0.14),0px 3px 16px 2px rgba(0,0,0,0.12)",
        "0px 6px 6px -3px rgba(0,0,0,0.2),0px 10px 14px 1px rgba(0,0,0,0.14),0px 4px 18px 3px rgba(0,0,0,0.12)",
        "0px 6px 7px -4px rgba(0,0,0,0.2),0px 11px 15px 1px rgba(0,0,0,0.14),0px 4px 20px 3px rgba(0,0,0,0.12)",
        "0px 7px 8px -4px rgba(0,0,0,0.2),0px 12px 17px 2px rgba(0,0,0,0.14),0px 5px 22px 4px rgba(0,0,0,0.12)",
        "0px 7px 8px -4px rgba(0,0,0,0.2),0px 13px 19px 2px rgba(0,0,0,0.14),0px 5px 24px 4px rgba(0,0,0,0.12)",
        "0px 7px 9px -4px rgba(0,0,0,0.2),0px 14px 21px 2px rgba(0,0,0,0.14),0px 5px 26px 4px rgba(0,0,0,0.12)",
        "0px 8px 9px -5px rgba(0,0,0,0.2),0px 15px 22px 2px rgba(0,0,0,0.14),0px 6px 28px 5px rgba(0,0,0,0.12)",
        "0px 8px 10px -5px rgba(0,0,0,0.2),0px 16px 24px 2px rgba(0,0,0,0.14),0px 6px 30px 5px rgba(0,0,0,0.12)",
        "0px 8px 11px -5px rgba(0,0,0,0.2),0px 17px 26px 2px rgba(0,0,0,0.14),0px 6px 32px 5px rgba(0,0,0,0.12)",
        "0px 9px 11px -5px rgba(0,0,0,0.2),0px 18px 28px 2px rgba(0,0,0,0.14),0px 7px 34px 6px rgba(0,0,0,0.12)",
        "0px 9px 12px -6px rgba(0,0,0,0.2),0px 19px 29px 2px rgba(0,0,0,0.14),0px 7px 36px 6px rgba(0,0,0,0.12)",
        "0px 10px 13px -6px rgba(0,0,0,0.2),0px 20px 31px 3px rgba(0,0,0,0.14),0px 8px 38px 7px rgba(0,0,0,0.12)",
        "0px 10px 13px -6px rgba(0,0,0,0.2),0px 21px 33px 3px rgba(0,0,0,0.14),0px 8px 40px 7px rgba(0,0,0,0.12)",
        "0px 10px 14px -6px rgba(0,0,0,0.2),0px 22px 35px 3px rgba(0,0,0,0.14),0px 8px 42px 7px rgba(0,0,0,0.12)",
        "0px 11px 14px -7px rgba(0,0,0,0.2),0px 23px 36px 3px rgba(0,0,0,0.14),0px 9px 44px 8px rgba(0,0,0,0.12)",
        "0px 11px 15px -7px rgba(0,0,0,0.2),0px 24px 38px 3px rgba(0,0,0,0.14),0px 9px 46px 8px rgba(0,0,0,0.12)",
        "0 5px 5px -3px rgba(0,0,0,.06),0 8px 10px 1px rgba(0,0,0,.042),0 3px 14px 2px rgba(0,0,0,.036)",
    ];
}
