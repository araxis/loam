---
title: Charts & effects
---

# Charts & effects

Loam provides five custom-drawn chart controls (`PieChart`, `BarChart`, `LineChart`, `RadialGauge`, `Sparkline`), a static math helper (`Charts`), and a click-ripple effect (`Ripple`). Chart controls are located in `Loam.Controls`; enums and palette types live in `Loam`. Colors use `Avalonia.Media.Color`.

The charts are not wrappers around a third-party plotting engine — each one overrides `Render` and draws its slices, bars, or polyline directly with an Avalonia `DrawingContext`. That keeps them light and theme-aware, but it also means there is no XAML template to restyle: you shape a chart entirely through its C# properties. Every chart projects its data into one shared `ChartPoint` snapshot that feeds rendering, hover, tooltips, data labels, accessibility text, and the legend — so what you see, what a screen reader speaks, and what a bound `ChartLegend` shows can never drift apart.

```csharp
using Loam;          // LoamColor, Typo, enums
using Loam.Controls; // PieChart, BarChart, LineChart, ChartLegend, Ripple, Charts
using Avalonia.Media; // Color
```

> **Package (since 3.1).** The chart controls ship in the **`Loam.Charts`** satellite package. Add the
> package reference and register its themes with `Styles.Add(new LoamCharts())` after `LoamTheme`.
> Namespaces are unchanged (`Loam.Controls`). See the [v3 → v3.1 migration guide](/migration/v3-to-v3.1).

::: tip Mental model
Pick the chart by the **shape of the comparison**: `PieChart` for parts of a whole, `BarChart` for
comparing discrete categories, `LineChart` for a trend across an ordered sequence. Then layer features
in the same order on all three — give it data (`Values`, or bind `ItemsSource`), turn on `ShowAxes` for
quantitative reading, add `ShowDataLabels`/`ShowTooltip` for precision, and attach a `ChartLegend` to
name the series. `Ripple` is unrelated to charts; it is a feedback decorator that happens to ship in the
same package family.
:::

## Choosing a chart

| Use | When | Reach for |
| --- | --- | --- |
| Parts of a whole | A handful of values that sum to a meaningful total (share, mix, split) | [`PieChart`](#piechart) |
| KPI total + breakdown | A donut whose hole carries the headline number | [`PieChart`](#piechart) with `Donut` |
| Compare categories | Discrete, unordered items measured on one scale (revenue by month, count by type) | [`BarChart`](#barchart) |
| Diverging / signed data | Values that go above and below zero (P&L, variance, net flow) | [`BarChart`](#barchart) with `AllowNegative` |
| Trend over a sequence | An ordered series where the *direction* matters (time, steps) | [`LineChart`](#linechart) |
| Several series together | More than one series across the same categories | [`BarChart`](#barchart) / [`LineChart`](#linechart) with `Series` |
| One value against a range | A KPI dial — utilization, score, progress toward a target | [`RadialGauge`](#radialgauge) |
| Trend in a tiny space | An inline mini-chart for a table cell, list row, or KPI tile | [`Sparkline`](#sparkline) |
| Press feedback on any control | A Material ripple when the user clicks | [`Ripple`](#ripple) |

`BarChart` and `LineChart` share a `CartesianChartBase`, so axes, `Series`, `Min`/`Max`, and the
`StackMode`/multi-line behaviors below apply to both. `PieChart` is its own shape and ignores axes.

::: tip Theme first, colors second
Chart visuals are theme-aware by default. If `Colors` is `null`, series colors resolve from the active light/dark role tokens. Supplying `Colors` overrides theme roles for that chart. `Charts.Palette` remains available as a compatibility fallback for custom math/rendering scenarios.
:::

### Per-point labels and snapshot (all charts)

All chart controls share two members on their `ChartBase`:

| Member | Type | Default | Description |
|---|---|---|---|
| `Labels` | `IReadOnlyList<string>?` | `null` | Optional per-point labels aligned by index to `Values`. When set, they enrich the chart's accessibility help text (e.g. `"3 values: Web, Direct, Mobile"`). |
| `ShowDataLabels` | `bool` | `false` | When `true`, draws per-point value annotations on the chart, with responsive thinning that drops colliding labels rather than overlapping them. |
| `DataLabelFormat` | `Func<ChartPoint, string>?` | `null` | Formats each data label from its `ChartPoint`. When `null`, a per-chart default is used (value for bars/lines, percentage for pie slices). |

`ChartPoint` is a `readonly record struct (int Index, double Value, double Percent, string? Label, Color Color)`, where `Percent` is the value's share of the positive total (0 for non-positive values). It is the single projection shared by rendering, accessibility, data labels, and (in later releases) tooltips and legends, so visible and spoken output never drift.

```csharp
// Bars/lines label with the value; pie slices label with the percentage by default.
new BarChart { Values = new[] { 12d, 19d, 8d }, ShowDataLabels = true };

// Override the format from the ChartPoint.
new PieChart { Donut = true, Values = data, ShowDataLabels = true, DataLabelFormat = p => $"{p.Percent:P0}" };
```

### Data binding (all charts)

Instead of assembling `Values`/`Labels`/`Colors` by hand, bind a collection and project each item:

| Member | Type | Default | Description |
|---|---|---|---|
| `ItemsSource` | `IEnumerable?` | `null` | Bound source; when set, items are projected into the chart and an `INotifyCollectionChanged` source (e.g. `ObservableCollection<T>`) refreshes it live. |
| `ValueSelector` | `Func<object, double>?` | `null` | Item → numeric value. |
| `LabelSelector` | `Func<object, string>?` | `null` | Item → label (optional). |
| `ColorSelector` | `Func<object, Color?>?` | `null` | Item → color; return `null` to use the theme series color for that point. |

```csharp
var orders = new ObservableCollection<Order>(initial);
var chart = new BarChart
{
    ItemsSource = orders,
    ValueSelector = o => ((Order)o).Total,
    LabelSelector = o => ((Order)o).Month,
    ColorSelector = o => ((Order)o).IsLate ? Colors.Red : (Color?)null,
    ShowAxes = true,
};
orders.Add(next); // chart updates automatically
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

::: tip Comparable scales across charts
Two charts only read as comparable if they share a scale. When you place several charts side by side, pin
the same `Min`/`Max` on each — otherwise each chart auto-fits its own data and a smaller series can look
just as "tall" as a larger one.
:::

### Multiple series (bar & line)

Set `Series` (a list of `ChartSeries`) to plot several series. It overrides `Values`; categories come
from `Labels`, and each series gets one color (its `ChartSeries.Color`, or a theme color by index).

| Member | Type | Default | Description |
|---|---|---|---|
| `Series` | `IReadOnlyList<ChartSeries>?` | `null` | Multiple series; `ChartSeries(IReadOnlyList<double> Values, string? Name, Color? Color)`. |
| `StackMode` (BarChart) | `BarStackMode` | `Grouped` | `Grouped` (side-by-side), `Stacked`, or `StackedPercent` (each category normalized to 100%). |

`LineChart` draws one line per series. The Y-axis domain spans all series (and stacked totals). The
per-point snapshot, hover/tooltips, and hit-testing all carry a `ChartPoint.SeriesIndex`. The pure
`Charts.StackedBarHeights` helper backs stacking. (Multi-series data labels and signed/negative
multi-series bars are not drawn in this release.)

```csharp
var series = new[]
{
    new ChartSeries(new[] { 30d, 45d, 28d, 60d }, "Web"),
    new ChartSeries(new[] { 18d, 22d, 35d, 30d }, "Mobile"),
};
new BarChart { Labels = new[] { "Q1", "Q2", "Q3", "Q4" }, Series = series, ShowAxes = true };               // grouped
new BarChart { Labels = quarters, Series = series, StackMode = BarStackMode.Stacked, ShowAxes = true };     // stacked
new LineChart { Labels = quarters, Series = series, ShowAxes = true };                                      // multi-line
```

**Bound legend.** Set `ChartLegend.Source` to a chart and the legend derives its rows automatically —
one per series (name + color) for multi-series charts, otherwise one per category — and refreshes when
the chart's data changes. No manual `Labels`/`Colors` to keep in sync:

```csharp
var chart = new BarChart { Labels = quarters, Series = series };
var legend = new ChartLegend { Source = chart };
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

**Use it when** a few values add up to a meaningful whole and the *share* of each is the story — a traffic
split, a budget mix. With more than five or six slices, prefer a `BarChart`; thin slices are hard to read.

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

::: details How the donut hole picks its text
The hole shows `CenterText` if you set it. Otherwise, if `CenterValueFormat` is set, it formats
`CenterValue` — or the positive-value total when `CenterValue` is `null`. `CenterSubText` is the caption
drawn beneath either. So a donut summing to 1,240 with `CenterValueFormat = "N0"` and
`CenterSubText = "sessions"` reads "1,240 / sessions" without you computing the total.
:::

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

**Use it when** you are comparing discrete categories on one scale and the exact magnitudes matter. Add
`ShowAxes` so the values can be read off the gutter, and `Series` when each category carries more than one
measure.

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

::: warning Signed bars are single-series only
`AllowNegative` draws below-baseline bars only for a single `Values` series. Multi-series bars (via
`Series`) are positive-stacked or grouped in this release — negative values in a `Series` are treated as
zero. Keep diverging data on a single-series `BarChart`.
:::

---

## LineChart

Plots values as a connected polyline with a dot at each data point, scaled against the largest positive value. By default negative values are clamped to zero; set `AllowNegative = true` to plot them below a zero baseline. Set `Area = true` to fill the region beneath the line with a tokenized translucent wash (filled to the zero baseline when `AllowNegative` is set). Empty and zero-only charts render the shared `No data` state; a single positive value renders as one centered dot.

**Use it when** the data is an ordered sequence and the *shape* of the change — rising, dipping,
recovering — is what you want the reader to see. Reach for `Area` to emphasize cumulative volume, and
`Series` for several trends on one axis.

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

## RadialGauge

A single value drawn as a filled arc over a track, with an optional center readout — a KPI dial.
`RadialGauge` subclasses `ChartBase`, so it shares the theme palette, tooltip, hover/click, and empty state.
The fill is the resolved series color (override with `Colors`); the track uses the theme grid color.

**Use it when** you have one number to show against a range (utilization, a score, progress toward a
target). For several values, reach for [`BarChart`](#barchart); for a part-of-whole split, [`PieChart`](#piechart).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `double` | `0` | The value to display; clamped to `[Minimum, Maximum]` for the fill. |
| `Minimum` | `double` | `0` | The low end of the range. |
| `Maximum` | `double` | `100` | The high end of the range. When `Maximum <= Minimum` the gauge shows the empty state. |
| `StartAngle` | `double` | `135` | Arc start angle in degrees, clockwise from due-east (135 = lower-left). |
| `SweepAngle` | `double` | `270` | Total arc length in degrees (270 = a bottom-open speedometer arc; use 180 for a top half-circle). |
| `Thickness` | `double` | `14` | Ring thickness in pixels. |
| `Format` | `string?` | `null` | .NET numeric format for the readout (e.g. `"0.0"`, `"P0"`); when null, a compact default is used. |
| `CenterText` | `string?` | `null` | Explicit readout text; when null, `Value` formatted by `Format` is shown. |
| `Caption` | `string?` | `null` | Optional caption drawn beneath the readout (e.g. the metric name). |

`Colors`, `ShowTooltip`, `TooltipFormat`, `HoveredIndex`, `HoverChanged`, and `PointClicked` are inherited from `ChartBase`.

### Example

```csharp
using Loam.Controls;

var gauge = new RadialGauge
{
    Width   = 180,
    Height  = 160,
    Value   = 72,
    Maximum = 100,
    CenterText = "72%",
    Caption = "CPU load",
};
```

---

## Sparkline

A compact, inline chart for tables, lists, and KPI tiles: it draws `Values` as a small line or bar strip
with no axes or data labels, and no tooltip by default. `Sparkline` subclasses `ChartBase`, so it still
resolves theme colors and supports hover/click when you opt in. Values are shown by magnitude (non-positive
values read as zero) — for signed data use [`LineChart`](#linechart).

**Use it when** you want a trend glance in a tight space. For a full, axis-labeled chart, use
[`LineChart`](#linechart) or [`BarChart`](#barchart).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Mode` | `SparklineMode` | `Line` | Draw a connected `Line` or one `Bar` per value. |
| `ShowTooltip` | `bool` | `false` | Off by default (inherited from `ChartBase`); set `true` to enable hover tooltips. |

`Values`, `Colors`, `ItemsSource`, and the hover/click members are inherited from `ChartBase`.

### Example

```csharp
using Loam.Controls;

var spark = new Sparkline { Width = 160, Height = 32, Values = new[] { 4d, 7d, 5d, 9d, 6d, 11d } };
var bars  = new Sparkline { Width = 160, Height = 32, Mode = SparklineMode.Bar, Values = new[] { 3d, 6d, 2d, 8d } };
```

---

## ChartLegend

A vertical list of legend rows — a color swatch plus a caption — that pairs with the charts above. It can
read its rows from a bound chart (`Source`), or you can drive them manually with `Labels`/`Colors`.
`ChartLegend` is a `StackPanel`, so you place it like any panel (beside or beneath the chart).

**Use it when** a chart has more than one series, or its slices/bars need named keys. Prefer the bound
form (`Source`) so the legend rebuilds itself whenever the chart's data changes.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Source` | `ChartBase?` | `null` | Chart to derive rows from — one per series for multi-series charts, otherwise one per category. Refreshes automatically and overrides manual `Labels`/`Colors`. |
| `Labels` | `AvaloniaList<string>` | `[]` | Manual legend captions (used when `Source` is `null`). |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit swatch colors; when omitted, swatches bind to theme role tokens in the same order charts use. |
| `ShowSwatches` | `bool` | `true` | When `false`, renders captions only (no color swatch). |

```csharp
// Bound to a chart — rows follow the data
var chart = new BarChart { Labels = quarters, Series = series };
var legend = new ChartLegend { Source = chart };

// Manual rows when there is no single source chart
var manual = new ChartLegend
{
    Labels = { "Desktop", "Browser", "Mobile" },
};
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
| `GaugeFraction(value, min, max)` | `double` | The clamped 0..1 position of `value` within `[min, max]` — the fill fraction for a [`RadialGauge`](#radialgauge). Returns 0 when the range is non-positive. |
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

::: tip These helpers are pure
Everything on `Charts` (besides `Palette`) is a side-effect-free function over numbers. That makes them
unit-testable on their own and reusable from a custom `Control.Render` override when you need a chart
shape the built-in controls do not draw.
:::

---

## Ripple

A click-ripple `Decorator`. Wraps a child control and, on each pointer press, animates a translucent circle that expands from the press point to the farthest corner and fades out. `ClipToBounds` is enabled automatically.

`Ripple` ships in the core **Loam** package (namespace `Loam.Controls`), not the `Loam.Charts` satellite —
it is grouped here only because it is the library's other custom-drawn visual. Loam's own buttons already
host a ripple internally, so reach for `Ripple` to add the same feedback to a custom surface (a card, a
list row, a tile).

**Use it when** you build a clickable surface that is not already a Loam button and want Material-style
press feedback on it.

### Properties / Members

| Member | Type | Default | Description |
|---|---|---|---|
| `Progress` | `double` (styled property) | `0` | Current ripple animation progress from 0 to 1. Driven by the internal animation; triggers `Render` via `AffectsRender`. |
| `RippleOpacity` | `double` (styled property) | `0.12` | Maximum opacity of the ripple at its strongest, before it fades out. |
| `Duration` | `TimeSpan` (styled property) | `150 ms` | How long the expand-and-fade animation runs. |
| `RippleBrush` | `IBrush?` (styled property) | `null` | Color of the ripple. When unset (or not a solid brush) it falls back to black. |
| `Child` | `Control?` | `null` | The wrapped content (inherited from `Decorator`). |
| `MaxReach(origin, size)` | `static double` | — | Returns the distance from `origin` to the farthest corner of `size` — the radius the ripple expands to. |

### Example

```csharp
new Ripple
{
    Child = new Button { Content = "Click me" },
}

// A tinted, slower ripple on a custom surface
new Ripple
{
    RippleBrush = new SolidColorBrush(Color.Parse("#6750A4")),
    RippleOpacity = 0.2,
    Duration = TimeSpan.FromMilliseconds(250),
    Child = myCard,
}

// Using the static helper directly in a custom renderer
double reach = Ripple.MaxReach(pressPoint, controlBounds.Size);
```

---

## Recipe: a KPI card with chart, legend, and live readout

A common dashboard tile — a titled card holding a multi-series chart, a bound legend beside it, and a
status line that updates as the user hovers. Everything is plain C#; the chart, legend, and text stay in
sync through the bound `Source` and the `HoverChanged` event. Lay the pieces out with the surfaces in
[Surfaces & layout](./layout) and the typography in [Display primitives](./display#text).

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

var quarters = new[] { "Q1", "Q2", "Q3", "Q4" };
var series = new[]
{
    new ChartSeries(new[] { 30d, 45d, 28d, 60d }, "Web"),
    new ChartSeries(new[] { 18d, 22d, 35d, 30d }, "Mobile"),
};

var chart = new BarChart
{
    Labels   = quarters,
    Series   = series,
    ShowAxes = true,
    Height   = 200,
};

var readout = new Text { Typo = Typo.Caption };
chart.HoverChanged += (_, e) =>
    readout.Text = e.Point is { } p ? $"{p.Label}: {p.Value:N0}" : "Hover a bar";

var card = new Card
{
    Elevation = 2,
    Content = new CardContent
    {
        Child = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                new Text { Text = "Sessions by quarter", Typo = Typo.H6 },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 16,
                    Children =
                    {
                        new Border { Child = chart, Width = 360 },
                        new ChartLegend { Source = chart },
                    },
                },
                readout,
            },
        },
    },
};
```

## Accessibility & keyboard

The charts are custom-drawn `Control`s, so they are not focusable or keyboard-operable on their own — they
are read by assistive technology rather than navigated. Build accordingly:

- **Automation name** — each chart sets a default name (`"Pie chart"`, `"Bar chart"`, `"Line chart"`, `"Gauge"`, `"Sparkline"`). Override it with `AutomationProperties.SetName(chart, "Sessions by quarter")` so screen readers announce the chart's purpose, not just its type.
- **`RadialGauge` summary** — the gauge keeps a help text describing its readout and range (e.g. `"CPU load: 72% (0 to 100)"`); set `Caption` and `Format`/`CenterText` so it reads well.
- **Spoken summary** — the chart keeps an automation *help text* in sync with its data: the count of positive values, plus the `Labels` when you provide them (e.g. `"3 values: Web, Direct, Mobile"`). Always set `Labels` so the summary is meaningful.
- **Don't rely on color alone** — series colors come from theme roles; pair the chart with a `ChartLegend` (and consider `ShowDataLabels`) so the data is legible without distinguishing hues.
- **Interaction is pointer-based** — `HoverChanged` and `PointClicked` fire on pointer move/press. If a datapoint must be actionable from the keyboard, mirror that action on a focusable control (a `Button`, a list row) elsewhere in the view.
- **`Ripple`** is decoration only; it adds no semantics. Keep the real action on the `Child` control, which carries its own focus and activation.

::: tip Name the chart and label its points
```csharp
using Avalonia.Automation;

var chart = new PieChart { Values = data, Labels = new[] { "Web", "Direct", "Mobile" } };
AutomationProperties.SetName(chart, "Traffic sources");
// Help text becomes "3 values: Web, Direct, Mobile"
```
:::

## See also

- [Surfaces & layout](./layout) — `Card`, `Paper`, and the panels that frame a chart tile.
- [Display primitives → Text](./display#text) — `Text`/`Typo` for chart titles, captions, and readouts.
- [Buttons & menus](./buttons) — focusable, keyboard-operable actions to pair with a chart's `PointClicked`.
- [v3 → v3.1 migration](/migration/v3-to-v3.1) — how the charts moved into the `Loam.Charts` package.
- [Theming](/guide/theming) — how series colors resolve from `LoamColor` role tokens when `Colors` is `null`.
