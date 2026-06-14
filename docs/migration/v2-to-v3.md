---
title: Migrating from v2 to v3
---

# Migrating from Loam v2 to v3

> **Status: released.** Loam **v3** is published and the current package version is **`3.1.0`**. This
> guide is the living record of every v2 → v3 breaking change. The 3.1 package split has its own
> [v3 → v3.1 guide](/migration/v3-to-v3.1).

The v3 plan and the independent review that drives it live at the repository root:
[`PLAN.md`](https://github.com/araxis/loam/blob/main/PLAN.md) and
[`REVIEW.md`](https://github.com/araxis/loam/blob/main/REVIEW.md).

## What v3 is about

v3 keeps the things people like about Loam — pure-C# authoring, runtime theming, a familiar
component API — and fixes the rough edges found while building real apps on v2:

1. **Theme consistency end-to-end** so base Avalonia chrome (scrollbars, tooltips, menus, window,
   text selection) reads as Material, not Fluent. _(Phase 1 — ✅ done.)_
2. **Material You** tonal scheme generation from a single seed color. _(Phase 2 — ✅ done.)_
3. **Naming & ergonomics**: stop shadowing `Avalonia.Controls` for net-new concepts, and smooth the
   daily collision friction. _(Phase 3 — ✅ done; see the rename map below.)_
4. **Modular packaging**: a lean core plus optional satellite packages. _(Phase 4 — ✅ done; new shell
   controls landed in 3.0, the package split shipped in 3.1 — see the
   [v3 → v3.1 guide](/migration/v3-to-v3.1).)_

## Breaking-change & deprecation policy

v3 is a major version and **may break source compatibility**, but never silently:

- Every renamed or removed public type ships first as an **`[Obsolete]` alias** with a stable
  **diagnostic id** (`LOAMxxxx`) so the rename is a *compiler warning*, not a build break — until the
  alias is removed in a later release.
- Each diagnostic id is listed in the [rename map](#rename-map) with its replacement.
- Renames are documented here before the old name is removed; nothing disappears without a deprecation
  window across at least one preview.

To temporarily silence a specific rename warning while you migrate, suppress its id:

```csharp
#pragma warning disable LOAM0001 // Grid -> ResponsiveGrid
// ... legacy code ...
#pragma warning restore LOAM0001
```

…or project-wide via `<NoWarn>LOAM0001;LOAM0002</NoWarn>` in your `.csproj`. Prefer fixing the call
site — the warnings are there to find them.

## Rename map

| v2 | v3 | Diagnostic | Status | Notes |
| --- | --- | --- | --- | --- |
| `Loam.Controls.Grid` | `Loam.Controls.ResponsiveGrid` | `LOAM0001` | ✅ Done | Net-new responsive concept; no longer shadows `Avalonia.Controls.Grid`. |
| `Loam.Controls.Item` | `Loam.Controls.Col` | `LOAM0002` | ✅ Done | The responsive grid’s column child. |
| `Loam.Controls.Stack` | `Avalonia.Controls.StackPanel` | `LOAM0003` | ✅ Done | Thin wrapper over `StackPanel`; deprecated. Use `StackPanel` (`Orientation` = `Horizontal` for the old `Row = true`; set `Spacing`, which defaulted to `8`). |
| `SimpleTable` + `DataGrid<T>` | `DataGrid<T>` (recommended) | n/a | ✅ Guidance (ADR-0013) | `DataGrid<T>` is the recommended table; `SimpleTable` is kept for trivial static tables. |
| Charts / Pickers / heavy Data controls | satellite packages (`Loam.Charts`, `Loam.Pickers`, `Loam.Data`) | n/a | ✅ Done (3.1.0) | Moved out of the core package; namespaces unchanged. See the [v3 → v3.1 guide](/migration/v3-to-v3.1). |

## Done in this preview

### `Grid` → `ResponsiveGrid`, `Item` → `Col`

Loam’s responsive 12-column layout is a **different concept** from `Avalonia.Controls.Grid` (which is
fixed 2D placement). In v2 it shared the name `Grid`, which forced an alias (`using LoamGrid = …`) in
every file that also used Avalonia layout. v3 gives the concept its own name.

> **Rule of thumb:** use `Avalonia.Controls.Grid` for fixed 2D layout, and Loam’s `ResponsiveGrid`
> for breakpoint-based reflow.

The old names still work (as `[Obsolete]` aliases) for one preview, so you can migrate incrementally.

**Before (v2):**

```csharp
using LoamGrid = Loam.Controls.Grid;
using Loam.Controls;

var grid = new LoamGrid
{
    Spacing = 16,
    Children =
    {
        new Item { Xs = 12, Md = 6, Child = card1 },
        new Item { Xs = 12, Md = 6, Child = card2 },
    },
};
```

**After (v3):**

```csharp
using Loam.Controls; // no alias needed — ResponsiveGrid does not collide with Avalonia

var grid = new ResponsiveGrid
{
    Spacing = 16,
    Children =
    {
        new Col { Xs = 12, Md = 6, Child = card1 },
        new Col { Xs = 12, Md = 6, Child = card2 },
    },
};
```

Mechanical migration:

| Find | Replace |
| --- | --- |
| `Loam.Controls.Grid` | `Loam.Controls.ResponsiveGrid` |
| `new Item {` / `new Item(` (the grid child) | `new Col {` / `new Col(` |
| `using LoamGrid = Loam.Controls.Grid;` | delete the alias, use `ResponsiveGrid` directly |

`Spacing`, the `Xs`…`Xxl` span properties, `ResolveSpan(...)`, and the automation names are all
unchanged — `ResponsiveGrid`/`Col` are behaviour-identical to the v2 `Grid`/`Item`.

## Delivered in this preview

Beyond the renames above, the `3.0.0` previews also ship these (no migration needed — additive or
internal; see the docs site for each):

- **Base-chrome theme bridge** so stray Avalonia controls (scrollbars, tooltips, menus, window
  background, text selection, expanders) read as Material in light & dark _(Phase 1)_.
- **Material You** seed → light+dark scheme generation (`LoamTheme.SetSeed`), a **high-contrast**
  variant (`LoamContrast`), and a one-call **density** switch (`LoamDensity.Compact` + `SetDensity`)
  _(Phase 2)_.
- **`AppBar.CustomActions`** slot, explicit generated-vs-custom **content precedence** (+ debug
  warning), and a **global-usings** collision aid _(Phase 3)_.
- New shell controls: **`NavigationRail`**, **`BottomNavigation`**, and **`CommandPalette`** _(Phase 4)_.

## Coming in later phases

Tracked in [`PLAN.md`](https://github.com/araxis/loam/blob/main/PLAN.md):

- **Package split** into `Loam.Charts` / `Loam.Pickers` / `Loam.Data` satellites _(Phase 4, ADR-0009)_.
- **DataGrid maturity**: grouping, inline edit, virtualization, frozen columns _(Phase 5)_.
- **Docs, migration polish, visual-regression tests, and the `3.0.0` release** _(Phase 6)_.
