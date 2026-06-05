---
title: Theming
---

# Theming

Loam's theme engine centers on **`LoamTheme`** (an Avalonia `Styles`) and projects a
**`LoamThemeData`** into Avalonia resources, with separate **light** and **dark** dictionaries.
The theme data includes role-based color schemes, compatibility palettes, typography, shadows,
shape, spacing, stroke, density, state layers, motion, field metrics and z-indices. Every control
resolves colors, fonts, spacing, stroke, elevation and motion from these tokens, so theme and variant
changes restyle the whole tree automatically.

## The theme object

```csharp
using Loam.Theming;

var theme = new LoamTheme();          // default light + dark schemes
Styles.Add(theme);                    // in your App.Initialize()
```

`LoamThemeData` is an immutable record you can customize and pass in:

```csharp
var data = LoamThemeData.Default with
{
    ColorSchemeLight = LoamColorScheme.DefaultLight with { Primary = Color.Parse("#0A7E8C") },
    Shape = LoamShape.Default with { Medium = new CornerRadius(10) },
    Spacing = LoamSpacing.Default with { Large = 18 },
    Density = LoamDensity.Default with { InteractiveMedium = 44 },
    FieldMetrics = LoamFieldMetrics.Default with { OutlinedHeight = 56 },
    StateLayer = LoamStateLayer.Default with { HoverOpacity = 0.1 },
};
Styles.Add(new LoamTheme(data));
```

## Light & dark

Loam ships both variants. Flip them with Avalonia's theme variant:

```csharp
Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;   // or Light
```

Because colors come from per-variant `ThemeDictionaries`, controls re-resolve instantly — no manual
restyle.

## Runtime changes

`LoamTheme` exposes runtime setters that rebuild the resource dictionaries in place:

| Method | Description |
| --- | --- |
| `SetPrimary(Color color)` | Recolor the primary (and its contrast text) for both variants. |
| `SetPalette(LoamPalette light, LoamPalette dark)` | Swap compatibility palettes and derive matching schemes. |
| `SetData(LoamThemeData data)` | Replace the entire theme data. |

```csharp
theme.SetPrimary(Colors.Indigo);
```

## Semantic colors & variants

Loam controls use the same theme-aware knobs:

- **`Color` (`LoamColor`)** — semantic roles: `Primary`, `Secondary`, `Tertiary`, `Info`, `Success`,
  `Warning`, `Error`, `Dark`, plus `Default`/`Inherit`/`Transparent`.
- **`Variant`** — `Filled`, `Outlined`, `Text`.
- **`Size` (`LoamSize`)** — `Small`, `Medium`, `Large`.
- **`Typo`** — display/headline/title/body/label roles in large/medium/small sizes. The older
  `H1`–`H6`, `Subtitle1/2`, `Body1/2`, `Button`, `Caption`, and `Overline` values remain aliases.

```csharp
new Button { Content = "Save",   Variant = Variant.Filled,   Color = LoamColor.Primary };
new Button { Content = "Cancel", Variant = Variant.Outlined, Color = LoamColor.Default };
new Alert  { Content = "Saved",  Color   = LoamColor.Success };
new Text   { Text = "Heading", Typo = Typo.H4 };
```

## Tokens

All values are exposed as Avalonia dynamic resources keyed by **`LoamTokens`** (e.g.
`LoamTokens.ColorPrimary`, `LoamTokens.ColorSurfaceContainer`, `LoamTokens.Palette(name)`,
`LoamTokens.TonalElevation(level)`, `LoamTokens.Elevation(level)`,
`LoamTokens.TypographyFontSize(name)`, field metrics, shape, spacing, stroke, density, state,
and motion tokens). Custom controls can bind to
them the same way Loam's do:

```csharp
border.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Surface));
```

Common additive token groups:

| Data record | Token examples |
| --- | --- |
| `LoamColorScheme` | `LoamTokens.ColorPrimary`, `LoamTokens.ColorOnPrimary`, `LoamTokens.ColorSurfaceContainer`, `LoamTokens.ColorOutline` |
| `LoamShape` | `LoamTokens.ShapeSmall`, `LoamTokens.ShapeMedium`, `LoamTokens.ShapeFull` |
| `LoamSpacing` | `LoamTokens.SpacingSmall`, `LoamTokens.SpacingLarge`, `LoamTokens.Spacing(name)` |
| `LoamStroke` | `LoamTokens.StrokeThin`, `LoamTokens.StrokeFocus` |
| `LoamDensity` | `LoamTokens.DensityInteractiveMedium`, `LoamTokens.DensityDataCellPadding` |
| `LoamStateLayer` | `LoamTokens.StateHoverOpacity`, `LoamTokens.StateDisabledOpacity` |
| `LoamMotion` | `LoamTokens.MotionDurationShort3`, `LoamTokens.MotionDuration(name)`, `LoamTokens.MotionEasing(name)` |
| `LoamFieldMetrics` | `LoamTokens.FieldOutlinedHeight`, `LoamTokens.FieldOutlinedPadding` |

## Elevation

`Paper`, `Card` and other surfaces take an `Elevation` (0–25) that maps to the built-in shadow table
and to tonal surface levels:

```csharp
new Paper { Elevation = 8, Content = /* … */ };
```
