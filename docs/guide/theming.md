---
title: Theming
---

# Theming

Loam's theme engine centers on **`LoamTheme`** (an Avalonia `Styles`) and projects a
**`LoamThemeData`** — palette, typography, shadows, layout, shape, state, motion, field metrics and z-indices — into Avalonia
resources, with separate **light** and **dark** dictionaries. Every control resolves its colors, fonts
and elevations from these tokens, so theme and variant changes restyle the whole tree automatically.

## The theme object

```csharp
using Loam.Theming;

var theme = new LoamTheme();          // default light + dark palettes
Styles.Add(theme);                    // in your App.Initialize()
```

`LoamThemeData` is an immutable record you can customize and pass in:

```csharp
var data = LoamThemeData.Default with
{
    PaletteLight = LoamPalette.DefaultLight with { Primary = Color.Parse("#0A7E8C") },
    Shape = LoamShape.Default with { Medium = new CornerRadius(10) },
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
| `SetPalette(LoamPalette light, LoamPalette dark)` | Swap the whole light/dark palettes. |
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
- **`Typo`** — the built-in type scale (`H1`–`H6`, `Subtitle1/2`, `Body1/2`, `Button`, `Caption`,
  `Overline`).

```csharp
new Button { Content = "Save",   Variant = Variant.Filled,   Color = LoamColor.Primary };
new Button { Content = "Cancel", Variant = Variant.Outlined, Color = LoamColor.Default };
new Alert  { Content = "Saved",  Color   = LoamColor.Success };
new Text   { Text = "Heading", Typo = Typo.H4 };
```

## Tokens

All values are exposed as Avalonia dynamic resources keyed by **`LoamTokens`** (e.g.
`LoamTokens.Primary`, `LoamTokens.Surface`, `LoamTokens.Palette(name)`,
`LoamTokens.Elevation(level)`, `LoamTokens.TypographyFontSize(name)`, field metrics,
shape, state, and motion tokens). Custom controls can bind to
them the same way Loam's do:

```csharp
border.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Surface));
```

Common additive token groups:

| Data record | Token examples |
| --- | --- |
| `LoamShape` | `LoamTokens.ShapeSmall`, `LoamTokens.ShapeMedium`, `LoamTokens.ShapeFull` |
| `LoamStateLayer` | `LoamTokens.StateHoverOpacity`, `LoamTokens.StateDisabledOpacity` |
| `LoamMotion` | `LoamTokens.MotionDurationShort`, `LoamTokens.MotionEasingStandard` |
| `LoamFieldMetrics` | `LoamTokens.FieldOutlinedHeight`, `LoamTokens.FieldOutlinedPadding` |

## Elevation

`Paper`, `Card` and other surfaces take an `Elevation` (0–25) that maps to the built-in shadow table:

```csharp
new Paper { Elevation = 8, Content = /* … */ };
```
