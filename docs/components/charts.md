---
title: Charts & effects
---

# Charts & effects

Loam provides three custom-drawn chart controls (`PieChart`, `BarChart`, `LineChart`), a static math helper (`Charts`), and a Material-style click-ripple effect (`Ripple`). All chart controls mirror the MudBlazor `MudChart` component and are located in `Loam.Controls`; enums and palette types live in `Loam`. Colors use `Avalonia.Media.Color`.

---

## PieChart

Draws one filled slice per value, sized by its share of the total. Mirrors the MudBlazor `MudChart` Pie/Donut variant. Set `Donut = true` to punch a center hole; control the hole size with `HoleRatio`.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>` | `[]` | Data values; each entry becomes one slice. |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit series colors. Falls back to `Charts.Palette`. |
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
```

---

## BarChart

Renders a vertical bar per value, scaled against the largest value in the series. Mirrors the MudBlazor `MudChart` Bar variant. The default measured size is 320 × 180. Bars are drawn with 2 px rounded corners and an 8 px gap between slots.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>` | `[]` | Data values; each entry becomes one bar. |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit series colors. Falls back to `Charts.Palette`. |

### Example

```csharp
new BarChart
{
    Width = 320,
    Height = 180,
    Values = new[] { 12d, 48d, 30d, 65d, 22d },
}
```

---

## LineChart

Plots values as a connected polyline with a dot at each data point, scaled against the maximum value. Mirrors the MudBlazor `MudChart` Line variant. Set `Area = true` to fill the region beneath the line with a translucent wash (18 % opacity). Requires at least two values to render.

### Properties

| Member | Type | Default | Description |
|---|---|---|---|
| `Values` | `IReadOnlyList<double>` | `[]` | Data values plotted left-to-right. Minimum 2 values required to render. |
| `Colors` | `IReadOnlyList<Color>?` | `null` | Optional explicit series colors. Index 0 is used for the line and fill. Falls back to `Charts.Palette`. |
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

Static class containing the default series color palette and math helpers shared by all chart controls. Mirrors the helper utilities from MudBlazor's `MudChart`.

### Members

| Member | Type | Description |
|---|---|---|
| `Palette` | `IReadOnlyList<Color>` | Eight Material 500-range categorical colors used as the default series palette. |
| `SliceSweeps(values)` | `IReadOnlyList<double>` | Converts a list of values to per-slice sweep angles in degrees summing to 360. Returns empty when the total is ≤ 0. |
| `BarHeights(values, maxPixels)` | `IReadOnlyList<double>` | Scales values to pixel heights proportional to the largest value. Returns all zeros when the maximum is ≤ 0. |

### Example

```csharp
// Compute sweep angles for a custom pie renderer
var sweeps = Charts.SliceSweeps(new[] { 40d, 25d, 20d, 15d });
// sweeps → [144, 90, 72, 54] degrees

// Scale bar data to a 160 px plot height
var heights = Charts.BarHeights(new[] { 20d, 80d, 50d }, maxPixels: 160d);
// heights → [40, 160, 100] pixels

// Access the built-in palette
Color first = Charts.Palette[0]; // #2196F3
```

---

## Ripple

A Material Design click-ripple `Decorator`. Wraps a child control and, on each pointer press, animates a translucent dark circle that expands from the press point to the farthest corner and fades out over 450 ms. Mirrors MudBlazor's ripple effect. `ClipToBounds` is enabled automatically.

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
