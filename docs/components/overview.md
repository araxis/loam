---
title: Components overview
---

# Components

Loam is a pure-C# control library for Avalonia — no XAML, no markup compiler, just types you
construct and configure in code. This page is the catalog: every component in the current release is
grouped by area below, and each linked page documents the real public API (properties, events, static
helpers) with copy-paste C# examples. Start here when you're not sure which control to reach for, then
follow the link into the area page for the details.

```csharp
using Loam;          // Variant, LoamColor, LoamSize, Typo, Icons
using Loam.Controls; // Button, TextField, Card, DataGrid<T>, …
```

> Types live in `Loam.Controls`; shared enums (`LoamColor`, `Variant`, `LoamSize`, `Typo`) live in
> `Loam`; theming (`LoamTheme`, `LoamTokens`) in `Loam.Theming`.

::: tip Mental model
Loam is built from three layers. **Primitives** (`Text`, `Icon`, `Chip`, `Avatar`) are the smallest
display pieces. **Controls** (`Button`, `TextField`, `Select`, `DataGrid<T>`) combine primitives into
interactive widgets. **Surfaces & layout** (`Card`, `Stack`, `Grid`, the `Layout`/`AppBar`/`Drawer`
app shell) arrange everything on the page. Most controls share the same `Variant` / `Color` / `Size`
knobs, so once you've learned one you've learned the pattern for the rest.
:::

## By area

| Area | What's inside |
| --- | --- |
| [Display primitives](./display) | `Text`, `Icon`, `Divider`, `Chip` / `ChipSet`, `Badge`, `Avatar` / `AvatarGroup` |
| [Buttons & menus](./buttons) | `Button`, `IconButton`, `ToggleIconButton`, `ButtonGroup`, `Fab`, `Menu` |
| [Surfaces & layout](./layout) | `Paper`, `Card` family, `Container`, `Grid`/`Item`, `Stack`, `Spacer`, `Hidden`, `ScrollToTop`, app shell (`Layout`/`AppBar`/`Drawer`/`MainContent`) |
| [Form inputs](./inputs) | `Field`, `TextField`, `NumericField`, `MaskedTextField`, `Select`, `Autocomplete`, `CheckBox`, `Switch`, `Radio`, `Slider`, `Rating`, `ToggleGroup`, `FileUpload`, `Form` |
| [Pickers](./pickers) | `DatePicker`, `TimePicker`, `ColorPicker`, `DateRangePicker`, `MonthCalendar` |
| [Data display](./data-display) | `List`, `SimpleTable`, `DataGrid<T>`, `TreeView`, `Tabs`, `ExpansionPanels`, `Timeline`, `Carousel`, `Pagination`, `Stepper` |
| [Navigation](./navigation) | `Link`, `Breadcrumbs`, `NavMenu`/`NavLink`/`NavGroup` |
| [Overlays & feedback](./overlays) | `DialogService`, `SnackbarService`, `Overlay`, `Popover`, `Tooltip`, `Alert`, `ProgressLinear`/`ProgressCircular`, `Skeleton`, `Collapse` |
| [Charts & effects](./charts) | `PieChart`, `BarChart`, `LineChart`, `RadialGauge`, `Sparkline`, `RadarChart`, `Ripple` |

## How to read these pages

Every area page follows the same shape, so once you've read one you can skim the rest:

- A short **intro and mental model** explaining what the area covers and how its controls relate.
- A **"Choosing / which one when"** decision table when several controls overlap, with a one-line
  "Use it when" on each control so you can pick fast.
- A **Properties** table per control listing the real public API — types, defaults, and a description.
  Inherited members (for example, Avalonia's `Click` and `Command` on button subclasses) are noted as
  such rather than re-explained.
- One or more **C# examples** you can paste directly, plus at least one composed **Recipe** showing the
  controls working together.
- An **Accessibility & keyboard** section grounded in the control's actual behavior.

::: tip New to Loam?
If you're just getting started, read [Getting started](/guide/getting-started) and
[C#-only UI](/guide/csharp-ui) first — they cover the construct-and-configure pattern these examples
assume. [Theming](/guide/theming) explains how the shared knobs below resolve to colors and sizes.
:::

## Common parameters

Most controls accept the same familiar knobs:

- **`Variant`** — `Filled` · `Outlined` · `Text`
- **`Color` (`LoamColor`)** — `Primary` · `Secondary` · `Tertiary` · `Info` · `Success` · `Warning` ·
  `Error` · `Dark` · `Default` · `Inherit` · `Transparent`
- **`Size` (`LoamSize`)** — `ExtraSmall` · `Small` · `Medium` · `Large` · `ExtraLarge`
- **`Elevation`**, **`Dense`**, **`Square`**, **`Outlined`**, **`FullWidth`** where applicable

See [Theming](/guide/theming) for how these map to tokens and how to customize them.

::: details Not every knob applies to every control
The list above is the *common* vocabulary, not a guarantee. A control only exposes the knobs that make
sense for it — `Fab` is always filled, `Text` uses `Typo` rather than `Variant`, and layout surfaces
lean on `Elevation` / `Square` / `Outlined`. The per-control Properties tables on each area page are the
source of truth for what a given control actually accepts.
:::

## See also

- [Getting started](/guide/getting-started) — install Loam and stand up your first window.
- [C#-only UI](/guide/csharp-ui) — the construct-and-configure pattern behind every example here.
- [Theming](/guide/theming) — how `Variant`, `Color`, `Size`, and `Typo` resolve to tokens.
- [Buttons & menus](./buttons) — a good first area page to see the shared knobs in action.
