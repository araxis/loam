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

There are two halves to remember: **data** and **projection**. `LoamThemeData` is pure, immutable data
(no Avalonia `Styles`) — trivially constructed, customized with `with`, and unit-tested. `LoamTheme`
takes that data and projects it into Avalonia's resource system as dynamic resources keyed by
[`LoamTokens`](#tokens). Controls never hard-code a color or a size; they bind a token. That single
indirection is what makes a variant flip, a seed change, or a density switch re-style the entire app at
runtime with no manual restyle pass.

```csharp
using Loam.Theming;   // LoamTheme, LoamThemeData, LoamTokens, LoamPalette, LoamColorScheme
using Avalonia.Media; // Color, Colors
```

::: tip Mental model
You almost never touch tokens by hand. Reach for **`LoamTheme`** once in `App.Initialize()`. After
that, control authors think in three knobs — [`Variant`](#semantic-colors-variants),
[`Color` (`LoamColor`)](#semantic-colors-variants), and [`Size` (`LoamSize`)](#semantic-colors-variants) —
and the theme decides what those resolve to. To recolor the whole app, change the **data**
([`SetSeed`](#material-you-generate-a-theme-from-one-seed) / [`SetPrimary`](#runtime-changes)), not the
controls.
:::

## The theme object

`LoamTheme` is the only piece you add to `Application.Styles`. It depends on Avalonia's `FluentTheme`
underneath it (which supplies the base templates Loam composes) — see
[Getting started → register the theme](./getting-started#_2-register-the-theme) for the full layering and
order.

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

::: details What `LoamThemeData` holds
A `LoamThemeData` record bundles every design facet, each its own small record with a `.Default`:

| Slot | Type | Covers |
| --- | --- | --- |
| `ColorSchemeLight` / `ColorSchemeDark` | `LoamColorScheme` | Role-based Material 3 colors per variant. |
| `PaletteLight` / `PaletteDark` | `LoamPalette` | Compatibility palette for reference-shaped APIs. |
| `Typography` | `LoamTypography` | Font family and the full type scale. |
| `Shadows` / `Elevation` | `LoamShadows` / `LoamElevation` | Box-shadow table and tonal/shadow mapping. |
| `Layout` | `LoamLayout` | Drawer widths, app-bar height, default radius. |
| `Spacing` | `LoamSpacing` | Spacing scale (`Small` 8, `Large` 16, `Section` 40…). |
| `Stroke` | `LoamStroke` | Stroke widths. |
| `Density` | `LoamDensity` | Hit targets, button heights/padding, table padding. |
| `Shape` | `LoamShape` | Corner radii (`Small` 8, `Medium` 12, `Full` 999…). |
| `StateLayer` | `LoamStateLayer` | Hover/focus/pressed/selected/disabled opacities. |
| `Motion` | `LoamMotion` | Durations and easing curves. |
| `FieldMetrics` | `LoamFieldMetrics` | Input heights, padding, floating-label geometry. |
| `ZIndex` | `LoamZIndex` | Overlay stacking order. |

Because every slot is an immutable record, `with` lets you override one value and inherit the rest — no
need to reconstruct a full theme to nudge one corner radius.
:::

## Light & dark

Loam ships both variants. Flip them with Avalonia's theme variant:

```csharp
Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;   // or Light
```

Because colors come from per-variant `ThemeDictionaries`, controls re-resolve instantly — no manual
restyle.

::: tip Color is per-variant; everything else is shared
Only the color scheme and palette live in the light/dark dictionaries. Typography, shape, spacing,
density, motion, field metrics and z-index are projected once as shared tokens, so they stay identical
across variants. Flipping the variant only re-resolves color — which is exactly why it's instant.
:::

## Choosing a customization entry point

Several methods recolor or reshape the app. Pick by *how much* you want to change and *where the values
come from*:

| Goal | Reach for | Scope |
| --- | --- | --- |
| Recolor everything from one brand color | [`SetSeed(seed)`](#material-you-generate-a-theme-from-one-seed) | Full light + dark scheme, accessible by construction |
| Boost contrast for low-vision users | [`SetSeed(seed, LoamContrast.High)`](#material-you-generate-a-theme-from-one-seed) | Same, with stronger tone separation |
| Change just the accent, keep the rest | [`SetPrimary(color)`](#runtime-changes) | Primary + its contrast/container roles, both variants |
| Apply your own compatibility palettes | [`SetPalette(light, dark)`](#runtime-changes) | Swaps palettes and derives matching schemes |
| Go information-dense (or comfortable) | [`SetDensity(density)`](#runtime-changes) | Hit targets, button/icon padding, table padding |
| Change anything else (shape, spacing, type…) | [`SetData(data)`](#runtime-changes) | Replaces the entire spec; build it with `with` |

All of them rebuild the resource dictionaries in place, so bound controls re-resolve immediately — there
is no "rebuild the app" step.

## Runtime changes

`LoamTheme` exposes runtime setters that rebuild the resource dictionaries in place:

| Method | Description |
| --- | --- |
| `SetSeed(Color seed)` | **Material You** — generate a complete light + dark scheme from one seed color. |
| `SetPrimary(Color color)` | Recolor the primary (and its contrast text) for both variants. |
| `SetPalette(LoamPalette light, LoamPalette dark)` | Swap compatibility palettes and derive matching schemes. |
| `SetDensity(LoamDensity density)` | Switch component density — `LoamDensity.Compact` for dense apps, `LoamDensity.Default` for comfortable. |
| `SetData(LoamThemeData data)` | Replace the entire theme data. |

```csharp
theme.SetPrimary(Colors.Indigo);
```

::: tip Hold the instance
These are instance methods on `LoamTheme`, so keep a reference to the one you added to `Styles` (a field
on your `App`) if you plan to change the theme at runtime. `SetData` and friends mutate that instance's
projected resources — they do not add a second theme.
:::

### Material You — generate a theme from one seed

`SetSeed` derives tonal palettes (primary/secondary/tertiary/neutral/neutral-variant/error) from a
single seed color and maps every role to the standard Material 3 tone for each variant — producing a
complete, **accessible** light + dark scheme at runtime. Build one up front with
`LoamThemeData.FromSeed(seed)`, or apply it live:

```csharp
// Recolor the whole app — base Avalonia controls follow via the Fluent bridge.
theme.SetSeed(Color.Parse("#006A6A"));

// Boost contrast for low-vision users (targets WCAG AAA on the main text pairs).
theme.SetSeed(Color.Parse("#006A6A"), LoamContrast.High);
```

Schemes are accessible by construction: a color's WCAG luminance is fixed by its tone (L\*), so the
Material 3 tone pairs (e.g. on-primary on primary) always clear WCAG AA regardless of the seed. The
gallery's header has a **seed picker** (the palette icon) that drives this live.

The contrast argument has three levels (`LoamContrast`); `Standard` is the exact Material 3 mapping,
and the higher levels push role tones toward the extremes for stronger separation:

| Level | Intent |
| --- | --- |
| `LoamContrast.Standard` | Default Material 3 tones. |
| `LoamContrast.Medium` | Increased contrast. |
| `LoamContrast.High` | Maximum contrast; targets WCAG AAA on the main text pairs. |

::: tip `FromSeed` for build-time, `SetSeed` for runtime
`LoamThemeData.FromSeed(seed)` returns a fresh data record you can pass to `new LoamTheme(data)` at
startup — handy when the seed is known up front (a per-tenant brand color, say). `theme.SetSeed(seed)`
does the same derivation but applies it to a live theme. Both keep the current typography, shape, spacing
and the rest; only color is regenerated.
:::

## Semantic colors & variants

Loam controls use the same theme-aware knobs:

- **`Color` (`LoamColor`)** — semantic roles: `Primary`, `Secondary`, `Tertiary`, `Info`, `Success`,
  `Warning`, `Error`, `Dark`, plus `Default`/`Inherit`/`Transparent`.
- **`Variant`** — `Filled`, `Outlined`, `Text`.
- **`Size` (`LoamSize`)** — `ExtraSmall`, `Small`, `Medium`, `Large`, `ExtraLarge`.
- **`Typo`** — display/headline/title/body/label roles in large/medium/small sizes. The older
  `H1`–`H6`, `Subtitle1/2`, `Body1/2`, `Button`, `Caption`, and `Overline` values remain aliases.

```csharp
new Button { Content = "Save",   Variant = Variant.Filled,   Color = LoamColor.Primary };
new Button { Content = "Cancel", Variant = Variant.Outlined, Color = LoamColor.Default };
new Alert  { Content = "Saved",  Color   = LoamColor.Success };
new Text   { Text = "Heading", Typo = Typo.H4 };
```

### How a knob becomes a token

The three knobs are *intent*, not literal values — the theme resolves them:

- **`Color`** selects which scheme/palette role a control reads. `LoamColor.Primary` resolves to the
  `Primary` role (`LoamTokens.ColorPrimary` and the matching `Loam.Palette.Primary`), so changing the
  seed or calling `SetPrimary` instantly retints every `Color = Primary` control. `Default` follows the
  neutral text/surface roles; `Inherit` takes the ambient color; `Transparent` paints nothing.
- **`Variant`** decides *how* that role is painted — `Filled` uses the role as a background with its
  `On*` contrast text, `Outlined` draws a border in the role with transparent fill, `Text` uses the role
  for the label only.
- **`Size`** maps to density metrics (`LoamTokens.Density…`) — heights, padding, and hit targets — so a
  `SetDensity` swap rescales every sized control at once.

::: warning Don't hard-code colors in custom controls
The whole engine depends on controls binding tokens, not literal brushes. A `Border` painted with a
fixed `Color.Parse(...)` won't follow a variant flip, a seed change, or `SetPrimary`. Bind the token
(see [Tokens](#tokens)) so your control re-styles with everyone else's.
:::

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

Keys are namespaced `Loam.<group>.<name>`. Most groups expose both ready-made constants for the
frequently bound tokens *and* a string-builder method for the rest — `LoamTokens.Palette(name)`,
`LoamTokens.Spacing(name)`, `LoamTokens.Density(name)`, `LoamTokens.MotionDuration(name)` and so on — so
a token resolves even without a dedicated constant. Pass a property name (e.g.
`LoamTokens.Spacing(nameof(LoamSpacing.Large))`) to stay refactor-safe.

::: details Color-scheme role tokens come with state-layer variants
For every color-scheme role, `LoamTheme` also projects hover/focus/pressed/selected overlay brushes —
the role color at the matching state opacity. Reach for them via
`LoamTokens.ColorSchemeStateLayer(role, state)`, e.g.
`LoamTokens.ColorSchemeStateLayer("Primary", "Hover")`. Tonal surface levels are exposed too, via
`LoamTokens.TonalElevation(level)` (0–5), which back the `Elevation` property on surfaces.
:::

## Elevation

`Paper`, `Card` and other surfaces take an `Elevation` (0–25) that maps to the built-in shadow table
and to tonal surface levels:

```csharp
new Paper { Elevation = 8, Content = /* … */ };
```

## Recipe: a live theme switcher

A small toolbar that drives the theme at runtime — a light/dark toggle, a couple of seed swatches, and a
density switch — wired to a `LoamTheme` instance you hold on your `App`. Everything is plain C#; lay it
out with a `StackPanel` (see [Surfaces & layout](../components/layout)).

```csharp
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Media;
using Loam;
using Loam.Controls;
using Loam.Theming;

// Held on your App so runtime setters target the live instance.
var theme = new LoamTheme();
Styles.Add(theme);

var switcher = new StackPanel
{
    Orientation = Orientation.Horizontal,
    Spacing = 8,
    Children =
    {
        new IconButton
        {
            Icon    = Icons.Material.Filled.DarkMode,
            Variant = Variant.Outlined,
        },
        new Button
        {
            Content   = "Teal",
            Variant   = Variant.Filled,
            Color     = LoamColor.Primary,
            StartIcon = Icons.Material.Filled.Palette,
        },
        new Button
        {
            Content = "Compact",
            Variant = Variant.Outlined,
            StartIcon = Icons.Material.Filled.Tune,
        },
    },
};

// Wire the actions to the live theme.
var toggle  = (IconButton)switcher.Children[0];
var teal    = (Button)switcher.Children[1];
var compact = (Button)switcher.Children[2];

toggle.Click += (_, _) =>
    Application.Current!.RequestedThemeVariant =
        Application.Current.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;

teal.Click    += (_, _) => theme.SetSeed(Color.Parse("#006A6A"));
compact.Click += (_, _) => theme.SetDensity(LoamDensity.Compact);
```

::: tip Read the resolved variant, not the requested one
`RequestedThemeVariant` can be `Default` (follow the OS). When you need to know what's actually showing —
to flip a toggle, say — read `ActualThemeVariant`, as the handler above does.
:::

## Accessibility & keyboard

Theming is where most of Loam's accessibility guarantees live, because color and size are resolved here
rather than per control:

- **Contrast by construction** — seed-generated schemes
  ([`SetSeed`](#material-you-generate-a-theme-from-one-seed) / `LoamThemeData.FromSeed`) fix each color's
  WCAG luminance via its Material 3 tone, so the `On*`-on-role pairs (e.g. on-primary on primary) clear
  WCAG AA for *any* seed. You don't have to re-check contrast after rebranding.
- **High-contrast mode** — `LoamContrast.High` pushes tones toward the extremes, targeting WCAG AAA on
  the main text pairs for low-vision users.
- **Honor the OS theme** — leave `RequestedThemeVariant = ThemeVariant.Default` to follow the platform
  light/dark setting; controls re-resolve when the OS flips.
- **Hit targets via density** — `Size` resolves to density metrics, and the default `LoamDensity` keeps
  interactive targets at comfortable sizes (`InteractiveMedium` is 48). Prefer `LoamDensity.Compact` only
  for genuinely dense, pointer-first surfaces.
- **Disabled emphasis** — the disabled state uses `LoamTokens.StateDisabledOpacity` (0.38) consistently,
  so disabled controls read as disabled to sighted users everywhere.

::: warning Keep your overrides accessible
When you override scheme roles by hand (`ColorSchemeLight with { Primary = … }`) you bypass the tone-based
contrast guarantee — set a matching `OnPrimary` and re-check the pair. `SetPrimary` does this for you
(it recomputes a readable contrast text); raw `with` edits do not.
:::

## See also

- [Getting started → register the theme](./getting-started#_2-register-the-theme) — the `FluentTheme` →
  `LoamTheme` → satellites layering and order.
- [Components overview → common parameters](../components/overview#common-parameters) — how `Variant`,
  `Color`, and `Size` read across the library.
- [Buttons & menus](../components/buttons) — the three shared knobs on a concrete control family.
- [Surfaces & layout](../components/layout) — `Paper`/`Card` and the `Elevation` scale.
- [Why Loam](./why-loam) — the pure-C#, token-driven design rationale.
