# Loam — Independent Review

**Library:** Loam 2.0.1 — pure-C#, no-XAML Material Design 3 control library for Avalonia
(`nuget: Loam`, repo: `github.com/araxis/loam`).
**Targets:** Avalonia 12.x, .NET 8 library.
**Reviewer basis:** This review was written while integrating Loam into the Nodely sample apps
(Avalonia 12.0.4). It is grounded in (a) reading Loam's theming system and ~20 control sources
directly, and (b) building a real Material shell + responsive drawer + home dashboard (cards/chips) +
dialogs + snackbars + runtime accent re-theming. A few points are explicitly flagged as *inference*
(not verified against a stress test).

---

## TL;DR

For a **pure-C#, code-first, Material Design 3** Avalonia app, Loam is genuinely capable and pleasant.
I stood up a real Material shell, responsive navigation, a card dashboard, dialogs, snackbars, and
live accent re-theming in a few hours, no XAML, and it looked right. It is strong where most app
chrome lives — **theming, shell/layout, navigation, surfaces, inputs, overlays, accessibility**. It
shows its edges on **heavy-data scenarios** (DataGrid maturity, virtualization), a **small curated
icon set**, a few **control ergonomics** (generated-vs-custom content, AppBar action slot), and
**name collisions with Avalonia**.

> **Is it enough to design apps?** Yes for LOB / dashboard / tool / content apps. For data-grid-heavy
> or highly bespoke visual systems you'll supplement with raw Avalonia primitives — which compose fine,
> because Loam is built directly on Avalonia (its `Button` *is* an `Avalonia.Controls.Button`).

### Scorecard

| Area | Rating | Note |
| --- | --- | --- |
| Theming / tokens | ★★★★★ | Role-based MD3 scheme, deep `LoamThemeData`, runtime `SetPrimary`/`SetPalette`. |
| Shell & layout | ★★★★☆ | `Layout`/`AppBar`/`Drawer`/`MainContent` + responsive grid; a couple of slot limits. |
| Navigation | ★★★★☆ | `NavMenu`/`NavLink`/`NavGroup`, drawer mini-rail; no dedicated rail / bottom-nav yet. |
| Surfaces & primitives | ★★★★☆ | `Paper`/`Card`/`Chip`/`Badge`/`Avatar`/`Divider`; generated-content precedence is subtle. |
| Inputs & forms | ★★★★☆ | Broad set incl. self-contained pickers; declarative validation surface would help. |
| Overlays & feedback | ★★★★★ | Provider-less `DialogService`/`SnackbarService` via the window overlay layer. |
| Data display | ★★★☆☆ | `DataGrid<T>`/`SimpleTable`/`TreeView`/`Tabs` exist; grouping/edit/virtualization are gaps. |
| Charts | ★★★☆☆ | Pie/bar/line + donut; no stacked / time-series yet. |
| Accessibility | ★★★★☆ | `AutomationProperties` throughout, keyboard activation, dialog focus capture/restore. |
| Code ergonomics | ★★★☆☆ | Type-name collisions with `Avalonia.Controls` force aliasing in every file. |

---

## What's good (real strengths)

- **Token-driven runtime theming is the standout.** `LoamTheme.SetPrimary(color)` recolored an entire
  running app — app bar, nav, cards — because controls bind to tokens via `GetResourceObservable`.
  `LoamColorScheme` is a *proper* MD3 role set (primary/secondary/tertiary + containers, the full
  surface-container ramp, outline, inverse, fixed), and `LoamThemeData` is a deep, swappable record
  (color schemes, typography, elevation, shape, spacing, density, motion, state layers, field metrics).
- **Pure C#, object-initializer API.** No XAML; predictable parameters (`Variant`/`Color`/`Size`/
  `Elevation`/`Dense`). Matches a code-first workflow exactly.
- **"Generated anatomy" slots are fast.** `Card { Title, Subtitle, HeaderAvatar, Body, PrimaryActionText }`,
  `Drawer { Title, Items, Footer }`, `AppBar`, `MainContent` — standard Material layouts in a few lines,
  then drop to `Content` for full custom.
- **Accessibility is baked in,** not bolted on — `AutomationProperties` set across controls, keyboard
  activation on `NavLink`/`Chip`, focus capture & restore in `DialogService`.
- **Provider-less overlays.** `DialogService.For(this)` / `SnackbarService.For(this)` use the window's
  `OverlayLayer` — no provider component to register.
- **Container-query responsive grid.** `Grid`/`Item` pick the breakpoint from the grid's *own* width,
  so layouts reflow correctly inside any container (ahead of typical Avalonia patterns).
- **Composes on Avalonia Fluent.** Loam controls subclass Avalonia controls, so `Command`, routed
  events, styles, and bindings keep working. You're never trapped.

## What's bad (honest)

- **Name collisions with `Avalonia.Controls`.** `Loam.Controls` defines `Grid`, `Button`, `Text`,
  `Menu`, `Card`… all of which collide with Avalonia. In a code-first file you must alias
  (`using LoamGrid = …`) or fully-qualify — this was the single biggest day-to-day friction.
- **`AppBar.Actions` only accepts `AppBarAction`** (init-only, rendered internally as an `IconButton`).
  You can't drop an arbitrary control (a toggle, a menu, a search field) into the bar without replacing
  the whole `Content`. And because `AppBarAction` is immutable, reflecting a toggle's state requires
  *rebuilding* the action (I swapped a sun/moon icon by replacing `Actions[0]`).
- **Dual-mode "generated vs custom content" is subtle.** `Paper`/`Card`/`Drawer` flip between generated
  anatomy and your `Content` based on which properties are set, via internal
  `_usingGeneratedContent`/`_hasCustomContent` flags. It works, but the precedence isn't obvious and
  setting both can surprise. Needs documenting.
- **Curated icon set is small (~60 Material Filled).** Save / FolderOpen / ZoomIn / Fit weren't present
  (I mapped to CloudUpload/OpenInNew/GridView/HorizontalRule). `Icon.Data` *does* accept raw path data,
  so you're not blocked — but the built-in set is thin.
- **`Grid`/`Item` don't equalize row height** — each item is sized to its own content, so equal-height
  card rows need a manual `MinHeight`. An align-items/stretch option would be the Material-correct default.
- **Data layer is the weakest area.** `DataGrid<T>` exists but grouping/inline-edit are roadmap, and I
  did not see virtualization on lists/grids (*inference — verify before a data-heavy app*). This is
  where Avalonia's `DataGrid`/`TreeDataGrid` still win.
- **Charts are basic** (pie/bar/line + donut; no stacked/time-series yet).

## Is it customizable enough?

**Yes — at the token/theme level, that's its strength.** You can swap the entire `LoamThemeData`, both
palettes, the primary color, typography, shape, and density at runtime. **Per-instance** customization
is also good (`Variant`/`Color`/`Size`/`Outlined`/`Elevation`/`Shape`). The gap is **structural**
customization of a few "smart" controls (the AppBar action slot; generated-content precedence) — there
you occasionally fight the control instead of configuring it.

---

## Design question: does Loam need its own `Grid`?

**No — not one that competes with Avalonia's `Grid`. And the thing Loam currently *calls* `Grid` is
mis-named.**

- **Avalonia `Grid`** (WPF-inspired) is precise **2D fixed placement**: `RowDefinitions`/
  `ColumnDefinitions` with `*`/Auto/pixel/star-weights, `Grid.Row/Column/Span`, shared-size groups.
  It is excellent and Loam should simply rely on it. Duplicating it adds nothing.
- **Loam `Grid`/`Item`** is a different animal: a **responsive 12-column flow** where `Item` declares a
  column span per breakpoint (`Xs`/`Sm`/`Md`/`Lg`/…) and wraps to new rows, with the breakpoint derived
  from container width. Avalonia genuinely lacks this — `WrapPanel` wraps by item *size* (no declarative
  spans) and `UniformGrid` needs fixed equal cells. So the **capability is a real value-add worth keeping.**

The issue is purely the **name**:

1. `Loam.Controls.Grid` **shadows** `Avalonia.Controls.Grid`, forcing an alias everywhere.
2. It **misdescribes the concept** — it's a responsive row/flow, not a WPF grid.

Contrast with Loam's other collisions: `Button`/`Text`/`Card` *are* intentional MD3 **restyles** of the
Avalonia equivalents (same concept, themed), so a parallel name is at least defensible. Loam's `Grid` is
**not** a restyle of Avalonia's Grid — it's a different layout algorithm — so the shared name provides
zero "drop-in replacement" benefit and only confuses.

**Recommendation**

- Keep the responsive layout; **drop the `Grid` name.** Rename to `ResponsiveGrid` (or Bootstrap-style
  `Row` + `Col`, renaming `Item` → `Col`). Consider an `AlignItems`/stretch option for equal-height rows.
- Do **not** build a second 2D grid; document the split:
  > *Use Avalonia `Grid` for fixed 2D layout. Use Loam `ResponsiveGrid` for breakpoint-based reflow.*
- Bonus: this is a good model for the wider collision problem — where Loam adds a **new** concept
  (responsive grid), give it a **new** name; where it **restyles** an Avalonia concept, the parallel
  name is the trade-off to weigh against alias friction.

---

## Good to have (roadmap, prioritized)

1. **Material You tonal-palette generator from one seed color (HCT).** You already have `SetPrimary`
   with contrast recompute — extend it to generate the full tonal scheme from a single seed. One
   feature that would make "customizable" a headline.
2. **Fix the daily ergonomics:** rename the responsive grid (above); resolve/triage the type-name
   collisions (distinct names or an official alias-set guidance); give `AppBar`/`MainContent` a custom
   actions slot that accepts `Control`s; document generated-vs-custom content precedence.
3. **DataGrid maturity:** sorting/filtering UI, grouping, inline edit, frozen columns, virtualization;
   plus a virtualized `List`/`TreeView` for large data.
4. **Shell breadth:** a dedicated `NavigationRail` and `BottomNavigation`, a `CommandPalette`, and
   shared-element / page transitions for navigation (you already ship `Ripple`).
5. **Forms:** a declarative validation surface bound to `ObservableValidator` / DataAnnotations, and a
   `Form` builder.
6. **Icons:** a larger and/or pluggable icon set (Outlined/Round variants) and a visible-tooltip
   convention on `IconButton`/`AppBarAction`.
7. **A live theme playground in the gallery,** a one-call "compact app" switch (you model `LoamDensity`
   already), a high-contrast theme variant, and an RTL/localization audit.

---

## Bottom line

Loam punches above its size for app chrome and theming, and the **no-XAML + runtime-theming** combo is a
real differentiator in the Avalonia space. Tighten a few control ergonomics, rename the responsive grid,
and grow the **data** and **icon** stories, and it becomes a serious general-purpose MD3 toolkit for
Avalonia.
