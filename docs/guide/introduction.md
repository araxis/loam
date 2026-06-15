---
title: Introduction
---

# Introduction

**Loam** gives [Avalonia](https://avaloniaui.net) applications a complete themed control
set with a familiar, compact API — written entirely in C#, with **no XAML**.

If you've built application UI before, Loam should feel like home: you construct controls with object
initializers, configure them through a small shared vocabulary (`Variant`, `Color`, `Size`, `Dense`,
`Elevation`), and let central theme tokens handle the look. Nothing here replaces Avalonia — you still
write Avalonia views and use Avalonia layout. Loam sits on top as a control layer so you spend less time
hand-styling primitives and more time assembling screens.

```csharp
using Loam;          // Variant, LoamColor, LoamSize, Typo, Icons
using Loam.Controls; // Button, TextField, Card, DataGrid<T>, …
```

::: tip Mental model
Think of Loam as three layers stacked on Avalonia. At the bottom, **`LoamTheme`** projects a palette,
typography scale, and shadow set into Avalonia resources. In the middle, **control themes** (also pure
C#) style each control from those tokens. At the top, **the controls** you place expose a small, shared
set of knobs. Learn the knobs once and they mean the same thing everywhere — see
[Components → common parameters](/components/overview#common-parameters).
:::

## Why Loam?

Avalonia is a superb cross-platform UI framework. Loam adds a component layer with
the compact vocabulary teams expect in application UI: `Variant`, `Color`, `Dense`, `Elevation`, and
similar knobs.

- **Familiar API.** Component parameters use predictable names and consistent semantics.
- **Polished look.** Role-based colors, elevation/shadows, ripple, and typography all resolve from
  central theme tokens.
- **Pure C# authoring.** Controls, `ControlTheme`s, templates and bindings are built with Avalonia's
  code-only APIs — no `.axaml`. This keeps the whole UI in one language and one toolchain.
- **Self-contained.** The pickers (date / time / color) and the month calendar are custom-built, so a
  LoamTheme-only application doesn't need to pull in additional control packages.

## What Loam is *not*

- **Not a drop-in Razor port.** You still build Avalonia views and use Avalonia layout. Loam shrinks
  the *mental* gap, not the framework gap.
- **Not a framework wrapper.** Loam is an independent control library built directly on Avalonia.

## The packages

Loam ships as a core package plus three opt-in satellites (since 3.1). The core carries the theme and
the everyday controls; each satellite adds one heavier control group and depends on the core, so you
never reference `Loam` twice. Namespaces don't change across the split — everything stays under
`Loam.Controls` — so adding a satellite never touches your using-directives.

| Package | Add it when you need | What's inside |
| --- | --- | --- |
| **`Loam`** (core) | Always — it carries the theme and core controls | Buttons, inputs, surfaces, navigation, overlays, `Text`, `Icon`, and `LoamTheme` itself |
| **`Loam.Charts`** | You're drawing simple data visualizations | `PieChart`, `BarChart`, `LineChart` |
| **`Loam.Pickers`** | You need date / time / color entry | `DatePicker`, `TimePicker`, `ColorPicker`, `DateRangePicker`, `MonthCalendar` |
| **`Loam.Data`** | You're showing tabular or hierarchical data | `DataGrid<T>`, `SimpleTable`, `TreeView`, `Pagination` |

Each satellite exposes a styles registrar in the `Loam.Theming` namespace — `LoamCharts`, `LoamPickers`,
`LoamData` — that you add to `Application.Styles` *after* `LoamTheme`. The registrar contributes its
package's control themes and reuses the tokens `LoamTheme` already projected:

```csharp
using Avalonia.Themes.Fluent;
using Loam.Theming;

Styles.Add(new FluentTheme());   // base templates for the shell + built-in controls
Styles.Add(new LoamTheme());     // palette, typography, shadows + core control themes
Styles.Add(new LoamCharts());    // from Loam.Charts  — omit if unused
Styles.Add(new LoamPickers());   // from Loam.Pickers — omit if unused
Styles.Add(new LoamData());      // from Loam.Data    — omit if unused
```

::: warning Register before you render
A satellite control rendered without its registrar falls back to unthemed defaults. Add the registrar
for every satellite package you reference, and keep it after `LoamTheme` so it can read the tokens the
core already projected. The full walkthrough — including the `App` skeleton — is in
[Getting started → register the theme](./getting-started#_2-register-the-theme).
:::

## Status

Loam is shipping **v3 ("vNext") as previews**: end-to-end theme consistency, Material You scheme
generation, naming/ergonomics fixes, new shell controls (`NavigationRail`, `BottomNavigation`,
`CommandPalette`), and a matured `DataGrid<T>` (grouping, frozen columns, aggregates) have all landed.
The released baseline is **v2.0**. See the **[changelog](./changelog)** for what each phase delivered and
the **[migration guide](/migration/v2-to-v3)** for breaking changes.

The library targets **Avalonia 12** on **.NET 8**, with **xUnit + Avalonia.Headless** behavior tests.

## How the docs are organized

- **[Why Loam vs plain Avalonia](./why-loam)** — when to reach for Loam, and when not to.
- **[Getting Started](./getting-started)** — install, register the theme, build your first screen.
- **[Theming](./theming)** — palettes, light/dark, runtime recoloring.
- **[Authoring UI in C#](./csharp-ui)** — the code-only patterns Loam uses and that you'll use too.
- **[Components](/components/overview)** — every control, grouped, with properties and C# examples.
- **[Changelog](./changelog)** — notable changes per release.

::: tip Where to go next
New here? Read [Getting started](./getting-started) end-to-end, then skim the
[Components overview](/components/overview) to see what's available. If you're weighing Loam against
hand-rolling controls on plain Avalonia, [Why Loam](./why-loam) makes that call concrete.
:::

## See also

- [Getting started](./getting-started) — install the packages, register the theme, build a first screen.
- [Why Loam vs plain Avalonia](./why-loam) — the case for the control layer, and its limits.
- [Authoring UI in C#](./csharp-ui) — the construct-and-configure patterns every example assumes.
- [Theming](./theming) — how `Variant`, `Color`, and `Size` resolve to tokens, plus light/dark and runtime recoloring.
- [Components overview](/components/overview) — the full catalog, grouped by area.
- [Migration: v2 → v3](/migration/v2-to-v3) — breaking changes in the current preview line.
