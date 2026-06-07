---
title: Migrating from v2 to v3
---

# Migrating from Loam v2 to v3

> **Status: in progress (preview).** Loam v3 (“vNext”) is under active development on the
> `work/vnext` branch. This guide is the living record of every breaking change. It is filled in
> phase by phase as the work lands, so sections marked _Planned_ describe decisions that are agreed
> but not yet shipped. The current package version is **`3.0.0-preview.1`**.

The v3 plan and the independent review that drives it live at the repository root:
[`PLAN.md`](https://github.com/araxis/loam/blob/main/PLAN.md) and
[`REVIEW.md`](https://github.com/araxis/loam/blob/main/REVIEW.md).

## What v3 is about

v3 keeps the things people like about Loam — pure-C# authoring, runtime theming, a familiar
component API — and fixes the rough edges found while building real apps on v2:

1. **Theme consistency end-to-end** so base Avalonia chrome (scrollbars, tooltips, menus, window,
   text selection) reads as Material, not Fluent. _(Phase 1 — planned.)_
2. **Material You** tonal scheme generation from a single seed color. _(Phase 2 — planned.)_
3. **Naming & ergonomics**: stop shadowing `Avalonia.Controls` for net-new concepts, and smooth the
   daily collision friction. _(Phase 3 — in progress; see the rename map below.)_
4. **Modular packaging**: a lean core plus optional satellite packages. _(Phase 4 — planned.)_

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
| `Loam.Controls.Stack` | `Avalonia.Controls.StackPanel` | _TBD_ | 🔜 Planned (Phase 4) | Thin wrapper over `StackPanel` (adds `Row` + default `Spacing`); slated for removal. |
| `SimpleTable` + `DataGrid<T>` | one consolidated table API | _TBD_ | 🔜 Planned (Phase 4) | Consolidating the two table paths. |
| Charts / Pickers / heavy Data controls | satellite packages (`Loam.Charts`, `Loam.Pickers`, `Loam.Data`) | n/a | 🔜 Planned (Phase 4) | Moves out of the core package; namespaces unchanged. |

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

## Coming in later phases

These are tracked in [`PLAN.md`](https://github.com/araxis/loam/blob/main/PLAN.md); this guide will gain
a concrete section for each as it ships.

- **Theme bridge** for base Avalonia primitives + `SystemAccentColor*` mapping _(Phase 1)_.
- **Material You** seed → full light/dark scheme generator _(Phase 2)_.
- **`AppBar` custom-actions slot**, generated-vs-custom content precedence, collision tooling
  (global-usings / analyzer) _(Phase 3)_.
- **`Stack` removal**, table consolidation, and the **package split** into `Loam.Charts` /
  `Loam.Pickers` / `Loam.Data` _(Phase 4)_.
- **DataGrid maturity**: grouping, inline edit, virtualization, frozen columns _(Phase 5)_.
