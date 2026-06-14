---
title: Charts & effects
---

# Charts & effects

Loam provides three custom-drawn chart controls (`PieChart`, `BarChart`, `LineChart`), a static math helper (`Charts`), and a click-ripple effect (`Ripple`). Chart controls are located in `Loam.Controls`; enums and palette types live in `Loam`. Colors use `Avalonia.Media.Color`.

> **Package (since 3.1).** The chart controls ship in the **`Loam.Charts`** satellite package. Add the
> package reference and register its themes with `Styles.Add(new LoamCharts())` after `LoamTheme`.
> Namespaces are unchanged (`Loam.Controls`). See the [v3 → v3.1 migration guide](/migration/v3-to-v3.1).

Chart visuals are theme-aware by default. If `Colors` is `null`, series colors resolve from the active light/dark role tokens. Supplying `Colors` overrides theme roles for that chart. `Charts.Palette` remains available as a compatibility fallback for custom math/rendering scenarios.

### Per-point labels and snapshot (all charts)

All chart controls share two members on their `ChartBase`:

| Member | Type | Default | Description |
|---|---|---|---|
| `Labels` | `IReadOnlyList<string>?` | `null` | Optional per-point labels aligned by index to `Values`. When set, they enrich the chart's accessibility help text (e.g. `"3 values: Web, Direct, Mobile"`). |
| `ResolvedPoints` | `IReadOnlyList<ChartPoint>` | — | The current per-point snapshot, rebuilt whenever `Values`, `Colors`, or `Labels` change. |
| `ShowDataLabels` | `bool` | `false` | When `true`, draws per-point value annotations on the chart, with responsive thinning that drops colliding labels rather than overlapping them. |
| `DataLabelFormat` | `Func<ChartPoint, string>?` | `null` | Formats each data label from its `ChartPoint`. When `null`, a per-chart default is used (value for bars/lines, percentage for pie slices). |

`ChartPoint` is a `readonly record struct (int Index, double Value, double Percent, string? Label, Color Color)`, where `Percent` is the value's share of the positive total (0 for non-positive values). It is the single projection shared by rendering, accessibility, data labels, and (in later releases) tooltips and legends, so visible and spoken output never drift.

```csharp
// Bars/lines label with the value; pie slices label with the percentage by default.
new BarChart { Values = new[] { 12d, 19d, 8d }, ShowDataLabels = true };

// Override the format from the ChartPoint.
new PieChart { Donut = true, Values = data, ShowDataLabels = true, DataLabelFormat = p => $"{p.Percent:P0}" };
```

### Hover, tooltips, and clicks (all charts)

All charts hit-test the pointer against their datapoints (slice / bar / nearest line point) and surface it:

| Member | Type | Default | Description |
|---|---|---|---|
| `ShowTooltip` | `bool` | `true` | Draws a tokenized tooltip near the pointer for the hovered datapoint. |
| `TooltipFormat` | `Func<ChartPoint, string>?` | `null` | Formats the tooltip text; when `null`, a per-chart default is used (value, plus the percentage for pie slices). |
| `HoveredIndex` | `int` | `-1` | Index of the datapoint under the pointer, or `-1`. |
| `HoverChanged` | `event EventHandler<ChartPointEventArgs>` | — | Raised when the hovered datapoint changes (`Index` is `-1` when the pointer leaves all datapoints). |
| `PointClicked` | `event EventHandler<ChartPointEventArgs>` | — | Raised when a datapoint is clicked. |

`ChartPointEventArgs` carries the `Index` and the `ChartPoint? Point` (null when none). The hovered element is lightly emphasized while under the pointer.

### Axes (bar & line)

`BarChart` and `LineChart` share a `CartesianChartBase` that can draw a numeric Y-axis and a category
X-axis. Axes are **off by default** (existing charts are unchanged); set `ShowAxes = true` to enable them.

| Member | Type | Default | Description |
|---|---|---|---|
| `ShowAxes` | `bool` | `false` | Draws a nice-number Y-axis (ticks + labels in a left gutter) and category X-axis (from `Labels`, in a bottom gutter); bars/lines scale to the resulting domain. |
| `Min` / `Max` | `double?` | `null` | Explicit value-axis bounds; when null, derived from the data (and zero). Useful for comparable cross-chart scaling. |
| `YAxisTickCount` | `int` | `4` | Approximate number of Y-axis tick intervals (input to nice-number rounding). |
| `YAxisFormat` | `Func<double, string>?` | `null` | Formats Y-axis tick labels (e.g. `v => $"${v:N0}k"`); compact numeric default otherwise. |

`Charts.NiceScale(min, max, targetTicks)` (and the zero-based `NiceScale(max, targetTicks)`) is the pure
helper behind the scaling — it returns a rounded `(Min, Max, Step)` using 1/2/5×10ⁿ steps.

```csharp
new BarChart
{
    Values = new[] { 30d, 45d, 28d, 60d, 42d },
    Labels = new[] { "Q1", "Q2", "Q3", "Q4", "Q5" },
    ShowAxes = true,
    YAxisFormat = v => $"${v:N0}k",
};
```

```csharp
var chart = new BarChart { Values = new[] { 12d, 19d, 8d }, Labels = new[] { "Mon", "Tue", "Wed" } };
chart.HoverChanged += (_, e) => status.Text = e.Point is { } p ? $"{p.Label}: {p.Value:N0}" : "";
chart.PointClicked += (_, e) => OpenDetails(e.Index);
chart.TooltipFormat = p => $"{p.Label}: {p.Value:C0}"; // customize, or set ShowTooltip = false to disable
```

---

## PieChart

Draws one filled slice per positive value, sized by its share of the positive-value total. Negative values are clamped to zero. Set `Donut = true` to punch a center hole; control the hole size with `HoleRatio`, and fill the hole with a KPI total via the `Center*` properties. Empty and zero-only charts render a tokenized `No data` state instead of a blank surface.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>` | `[]` | Data values; positive entries become slices and negative entries are clamped to zero. |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit series colors. When `null`, defaults resolve from theme role tokens; `Charts.Palette` is only the compatibility fallback. |
| `Donut` | `bool` | `false` | When `true`, renders a center hole to produce a donut chart. |
| `HoleRatio` | `double` | `0.6` | Hole radius as a fraction of the chart radius. Clamped to 0–0.95. |
| `CenterText` | `string?` | `null` | Primary text drawn in the donut hole. Ignored unless `Donut` is `true`. |
| `CenterSubText` | `string?` | `null` | Secondary caption drawn under `CenterText`. |
| `CenterValue` | `double?` | `null` | Value formatted by `CenterValueFormat`; when `null`, the positive-value total is used. |
| `CenterValueFormat` | `string?` | `null` | .NET numeric format string (e.g. `"C0"`, `"N0"`) rendered in the hole when `CenterText` is not set. |

### Example

```csharp
new PieChart
{
    Width = 180,
    Height = 180,
    Values = new[] { 40d, 25d, 20d, 15d },
}

// Donut variant
new PieChart
{
    Width = 180,
    Height = 180,
    Values = new[] { 40d, 25d, 20d, 15d },
    Donut = true,
    HoleRatio = 0.55,
}

// Explicit colors override theme roles
new PieChart
{
    Width = 180,
    Height = 180,
    Values = new[] { 40d, 25d, 20d, 15d },
    Donut = true,
    Colors = new[]
    {
        Color.Parse("#355C7D"),
        Color.Parse("#6C5B7B"),
        Color.Parse("#C06C84"),
        Color.Parse("#F67280"),
    },
}

// Donut with a KPI total in the hole
new PieChart
{
    Width = 180,
    Height = 180,
    Donut = true,
    Values = new[] { 540d, 320d, 380d },
    Labels = new[] { "Desktop", "Browser", "Mobile" },
    CenterValueFormat = "N0", // formats the positive-value total (1,240)
    CenterSubText = "sessions",
}
```

---

## BarChart

Renders a vertical bar per value, scaled against the largest positive value in the series. By default negative values are clamped to zero; set `AllowNegative = true` to draw them as bars below a zero baseline (for P&L, variance, net-flow, and similar diverging data). The default measured size is 320 x 180. Bars are drawn with tokenized grid lines and rounded corners. Empty and zero-only charts render the shared `No data` state.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>` | `[]` | Data values; each entry becomes one bar. Negative values are clamped to zero unless `AllowNegative` is set. |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit series colors. When `null`, defaults resolve from theme role tokens; `Charts.Palette` is only the compatibility fallback. |
| `AllowNegative` | `bool` | `false` | When `true`, negative values render as bars below a tokenized zero baseline instead of being clamped to zero. |

### Example

```csharp
new BarChart
{
    Width = 320,
    Height = 180,
    Values = new[] { 12d, 48d, 30d, 65d, 22d },
}

// Signed (diverging) data with a zero baseline
new BarChart
{
    Width = 320,
    Height = 180,
    AllowNegative = true,
    Values = new[] { 12d, -5d, 8d, -3d, 15d },
    Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" },
}

// Visible empty state
new BarChart
{
    Width = 320,
    Height = 180,
    Values = new[] { 0d, -2d, 0d },
}
```

---

## LineChart

Plots values as a connected polyline with a dot at each data point, scaled against the largest positive value. By default negative values are clamped to zero; set `AllowNegative = true` to plot them below a zero baseline. Set `Area = true` to fill the region beneath the line with a tokenized translucent wash (filled to the zero baseline when `AllowNegative` is set). Empty and zero-only charts render the shared `No data` state; a single positive value renders as one centered dot.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>` | `[]` | Data values plotted left-to-right. Negative values are clamped to zero unless `AllowNegative` is set. |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit series colors. Index 0 is used for the line and fill. When `null`, defaults resolve from theme role tokens; `Charts.Palette` is only the compatibility fallback. |
| `Area` | `bool` | `false` | When `true`, fills the area beneath the line at 18 % opacity. |
| `AllowNegative` | `bool` | `false` | When `true`, negative values plot below a tokenized zero baseline instead of being clamped to zero. |

### Example

```csharp
new LineChart
{
    Width = 320,
    Height = 180,
    Values = new[] { 10d, 35d, 20d, 55d, 40d, 70d },
    Area = true,
}

// Signed values around a zero baseline
new LineChart
{
    Width = 320,
    Height = 180,
    AllowNegative = true,
    Area = true,
    Values = new[] { 4d, -2d, 6d, -1d, 3d, -4d },
}
```

---

## Charts (static helper)

Static class containing the compatibility series color palette and math helpers shared by all chart controls.

### Members

| Member | Type | Description |
|---|---|---|
| `Palette` | `IReadOnlyList<Color>` | Eight categorical colors retained as a compatibility fallback and for custom code. Built-in charts prefer theme roles when `Colors` is `null`. |
| `SliceSweeps(values)` | `IReadOnlyList<double>` | Converts positive values to per-slice sweep angles in degrees summing to 360. Negative values count as zero; returns empty when the positive total is less than or equal to zero. |
| `BarHeights(values, maxPixels)` | `IReadOnlyList<double>` | Scales positive values to pixel heights proportional to the largest value. Negative values count as zero; returns all zeros when the maximum is less than or equal to zero. |
| `SignedDomain(values)` | `(double Min, double Max)` | The value-axis domain for signed data, always including zero (`Min ≤ 0 ≤ Max`) so positive and negative values share one scale. |
| `ZeroBaselineOffset(min, max, plotHeight)` | `double` | The pixel offset of the zero baseline from the top of a plot for a signed `min..max` domain. |
| `SignedBarLayout(values, min, max, plotHeight)` | `IReadOnlyList<(double Y, double Height)>` | Lays out signed bars: each `(Y, Height)` is the bar's top offset from the plot top and its pixel height, growing up from the zero baseline for positive values and down for negative. |

### Example

```csharp
// Compute sweep angles for a custom pie renderer
var sweeps = Charts.SliceSweeps(new[] { 40d, 25d, 20d, 15d });
// sweeps → [144, 90, 72, 54] degrees

// Scale bar data to a 160 px plot height
var heights = Charts.BarHeights(new[] { 20d, 80d, 50d }, maxPixels: 160d);
// heights → [40, 160, 100] pixels

// Lay out signed (diverging) bars around a zero baseline
var (min, max) = Charts.SignedDomain(new[] { 10d, -5d, 0d }); // (-5, 10)
var layout = Charts.SignedBarLayout(new[] { 10d, -5d, 0d }, min, max, plotHeight: 150d);
// layout → [(0, 100), (100, 50), (100, 0)]  (Y, Height) from the plot top

// Access the compatibility palette for custom code
Color first = Charts.Palette[0]; // #2196F3
```

---

## Ripple

A click-ripple `Decorator`. Wraps a child control and, on each pointer press, animates a translucent circle that expands from the press point to the farthest corner and fades out. `ClipToBounds` is enabled automatically.

### Properties / Members

| Member | Type | Description |
|---|---|---|
| `Progress` | `double` (styled property) | Current ripple animation progress from 0 to 1. Driven by the internal animation; triggers `Render` via `AffectsRender`. |
| `Child` | `Control?` | The wrapped content (inherited from `Decorator`). |
| `MaxReach(origin, size)` | `static double` | Returns the distance from `origin` to the farthest corner of `size` — the radius the ripple expands to. |

### Example

```csharp
new Ripple
{
    Child = new Button { Content = "Click me" },
}

// Using the static helper directly in a custom renderer
double reach = Ripple.MaxReach(pressPoint, controlBounds.Size);
```
