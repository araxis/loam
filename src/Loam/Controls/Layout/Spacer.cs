using Avalonia.Controls;
using Avalonia.Layout;

namespace Loam.Controls;

/// <summary>
/// A flexible gap, mirroring MudBlazor's <c>MudSpacer</c>. An empty stretch control: placed as the
/// fill child of a <see cref="DockPanel"/> (or a star <see cref="Grid"/> cell) it takes the remaining
/// space, pushing its docked siblings to the edges (e.g. left/right groups in an app bar).
/// </summary>
public class Spacer : Control
{
    /// <summary>Creates the spacer.</summary>
    public Spacer()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
    }
}
