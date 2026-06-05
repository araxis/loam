using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// Attaches a Loam-styled tooltip to a control, mirroring the reference API's <c>Tooltip</c>. Wraps
/// Avalonia's <see cref="ToolTip"/> with a small elevated <see cref="Paper"/>.
/// </summary>
public static class Tooltip
{
    /// <summary>Sets a text tooltip on <paramref name="control"/>.</summary>
    public static void Set(Control control, string text)
    {
        AutomationProperties.SetHelpText(control, text);
        var paper = new Paper
        {
            Elevation = 4,
            Padding = new Thickness(8, 4),
            Content = new Text { Text = text, Typo = Typo.Caption },
        };
        InteractionAssist.ApplyZIndex(paper, LoamTokens.ZIndex(nameof(LoamZIndex.Tooltip)), LoamZIndex.Default.Tooltip);
        ToolTip.SetTip(control, paper);
    }
}
