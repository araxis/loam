---
title: Charts & effects
---

# Charts & effects

Loam provides three custom-drawn chart controls (`PieChart`, `BarChart`, `LineChart`), a static math helper (`Charts`), and a click-ripple effect (`Ripple`). Chart controls are located in `Loam.Controls`; enums and palette types live in `Loam`. Colors use `Avalonia.Media.Color`.

> **Package (since 3.1).** The chart controls ship in the **`Loam.Charts`** satellite package. Add the
> package reference and register its themes with `Styles.Add(new LoamCharts())` after `LoamTheme`.
> Namespaces are unchanged (`Loam.Controls`). See the [v3 → v3.1 migration guide](/migration/v3-to-v3.1).

Chart visuals are theme-aware by default. If `Colors` is `null`, series colors resolve from the active light/dark role tokens. Supplying `Colors` overrides theme roles for that chart. `Charts.Palette` remains available as a compatibility fallback for custom math/rendering scenarios.

---

## PieChart

Draws one filled slice per positive value, sized by its share of the positive-value total. Negative values are clamped to zero. Set `Donut = true` to punch a center hole; control the hole size with `HoleRatio`. Empty and zero-only charts render a tokenized `No data` state instead of a blank surface.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>` | `[]` | Data values; positive entries become slices and negative entries are clamped to zero. |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit series colors. When `null`, defaults resolve from theme role tokens; `Charts.Palette` is only the compatibility fallback. |
| `Donut` | `bool` | `false` | When `true`, renders a center hole to produce a donut chart. |
| `HoleRatio` | `double` | `0.6` | Hole radius as a fraction of the chart radius. Clamped to 0–0.95. |

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
```

---

## BarChart

Renders a vertical bar per value, scaled against the largest positive value in the series. Negative values are clamped to zero. The default measured size is 320 x 180. Bars are drawn with tokenized grid lines and rounded corners. Empty and zero-only charts render the shared `No data` state.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>` | `[]` | Data values; each entry becomes one bar and negative values are clamped to zero. |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit series colors. When `null`, defaults resolve from theme role tokens; `Charts.Palette` is only the compatibility fallback. |

### Example

```csharp
new BarChart
{
    Width = 320,
    Height = 180,
    Values = new[] { 12d, 48d, 30d, 65d, 22d },
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

Plots values as a connected polyline with a dot at each data point, scaled against the largest positive value. Negative values are clamped to zero. Set `Area = true` to fill the region beneath the line with a tokenized translucent wash. Empty and zero-only charts render the shared `No data` state; a single positive value renders as one centered dot.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>` | `[]` | Data values plotted left-to-right; negative values are clamped to zero. |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit series colors. Index 0 is used for the line and fill. When `null`, defaults resolve from theme role tokens; `Charts.Palette` is only the compatibility fallback. |
| `Area` | `bool` | `false` | When `true`, fills the area beneath the line at 18 % opacity. |

### Example

```csharp
new LineChart
{
    Width = 320,
    Height = 180,
    Values = new[] { 10d, 35d, 20d, 55d, 40d, 70d },
    Area = true,
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

### Example

```csharp
// Compute sweep angles for a custom pie renderer
var sweeps = Charts.SliceSweeps(new[] { 40d, 25d, 20d, 15d });
// sweeps → [144, 90, 72, 54] degrees

// Scale bar data to a 160 px plot height
var heights = Charts.BarHeights(new[] { 20d, 80d, 50d }, maxPixels: 160d);
// heights → [40, 160, 100] pixels

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
