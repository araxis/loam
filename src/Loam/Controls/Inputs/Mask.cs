using System.Text;

namespace Loam.Controls;

/// <summary>
/// Input-mask formatting, mirroring the reference API's <c>Mask</c>. <see cref="Apply"/> projects raw input
/// onto a pattern whose placeholders are <c>#</c> (digit), <c>A</c> (letter), <c>*</c> (letter or digit);
/// any other character is a literal inserted automatically (e.g. <c>(###) ###-####</c>).
/// </summary>
public static class Mask
{
    /// <summary>Formats <paramref name="raw"/> input onto <paramref name="pattern"/>; trailing literals are omitted once the raw input runs out.</summary>
    public static string Apply(string? raw, string? pattern)
    {
        if (string.IsNullOrEmpty(raw) || string.IsNullOrEmpty(pattern))
        {
            return raw ?? string.Empty;
        }

        var result = new StringBuilder();
        var ri = 0;
        foreach (var p in pattern)
        {
            if (ri >= raw.Length)
            {
                break;
            }

            if (IsPlaceholder(p))
            {
                while (ri < raw.Length && !Matches(p, raw[ri]))
                {
                    ri++;
                }

                if (ri < raw.Length)
                {
                    result.Append(raw[ri]);
                    ri++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                // Literal: emit it (auto-inserting), consuming the raw char if the user already typed it.
                result.Append(p);
                if (raw[ri] == p)
                {
                    ri++;
                }
            }
        }

        return result.ToString();
    }

    private static bool IsPlaceholder(char p) => p is '#' or 'A' or '*';

    private static bool Matches(char placeholder, char c) => placeholder switch
    {
        '#' => char.IsDigit(c),
        'A' => char.IsLetter(c),
        _ => char.IsLetterOrDigit(c),
    };
}
