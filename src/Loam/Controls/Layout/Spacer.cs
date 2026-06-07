using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Loam.Controls;

/// <summary>
/// A flexible gap, mirroring the reference API's <c>Spacer</c>. An empty stretch control: placed as the
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
        AutomationProperties.SetName(this, "Spacer");
    }
}
