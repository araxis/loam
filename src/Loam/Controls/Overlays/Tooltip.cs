using Avalonia;
using Avalonia.Controls;
using Loam;

namespace Loam.Controls;

/// <summary>
/// Attaches a Loam-styled tooltip to a control, mirroring the reference API's <c>Tooltip</c>. Wraps
/// Avalonia's <see cref="ToolTip"/> with a small elevated <see cref="Paper"/>.
/// </summary>
public static class Tooltip
{
    /// <summary>Sets a text tooltip on <paramref name="control"/>.</summary>
    public static void Set(Control control, string text) =>
        ToolTip.SetTip(control, new Paper
        {
            Elevation = 4,
            Padding = new Thickness(8, 4),
            Content = new Text { Text = text, Typo = Typo.Caption },
        });
}
