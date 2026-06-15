using Avalonia.Styling;

namespace Loam.Theming;

/// <summary>
/// Styles entry point for Loam's chart controls (<c>PieChart</c>, <c>BarChart</c>, <c>LineChart</c>,
/// <c>RadialGauge</c>, <c>Sparkline</c>, <c>RadarChart</c>).
/// The charts self-render and read tokens from the active <see cref="LoamTheme"/> at runtime, so this
/// registers no control themes today. It is provided for a uniform "add the styles" story across the
/// satellite packages and as a forward-compatible hook:
/// <code>Styles.Add(new LoamTheme()); Styles.Add(new LoamCharts());</code>
/// </summary>
public sealed class LoamCharts : Styles
{
}
