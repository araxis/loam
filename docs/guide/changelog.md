---
title: Changelog
---

# Changelog

Notable changes per release. Dates are when the work landed on the development branch; v3 is shipping as
previews while the phases complete.

## 3.0.0 (preview)

Loam v3 ("vNext") — a major version that keeps the v2 authoring model and fixes the rough edges found
building real apps on v2. It **may break source compatibility, but never silently**: every rename ships
first as an `[Obsolete]` alias with a stable `LOAMxxxx` diagnostic. See the
[migration guide](/migration/v2-to-v3) for the rename map.

### Theme consistency (Phase 1)

- **Base-chrome bridge** so stray Avalonia controls — scrollbars, tooltips, menus, window background,
  text selection, expanders — read as Material in both light and dark, not Fluent.
- `SystemAccentColor*` mapped to the Loam primary so accent-driven built-ins match.

### Material You (Phase 2)

- `LoamTheme.SetSeed(color)` generates a full light **and** dark scheme from one seed color
  (CIELAB tonal palettes, accessible by construction).
- `LoamContrast` high-contrast variant; one-call density via `LoamDensity.Compact` + `SetDensity`.

### Naming & ergonomics (Phase 3)

- `Loam.Controls.Grid` → **`ResponsiveGrid`**, `Loam.Controls.Item` → **`Col`** (`LOAM0001`/`LOAM0002`)
  — the responsive layout no longer shadows `Avalonia.Controls.Grid`.
- `AppBar.CustomActions` slot; explicit generated-vs-custom content precedence (with a debug warning);
  a global-usings collision aid.

### Components & data (Phases 4–5)

- New shell controls: **`NavigationRail`**, **`BottomNavigation`**, **`CommandPalette`** (searchable
  command overlay).
- `Loam.Controls.Stack` deprecated in favor of `StackPanel` (`LOAM0003`); `DataGrid<T>` is the
  recommended table (ADR-0013).
- **`DataGrid<T>` maturity:** grouping (`GroupBy`), collapsible groups, group aggregates
  (`GroupAggregate`), an empty state (`EmptyText`/`EmptyContent`), a column-width API
  (`DataGridColumn<T>.Width`), and **frozen columns** (`FrozenColumns` two-pane layout with
  `RowHeight`), on top of the existing sort/filter/paging/virtualize/inline-edit/selection.

### Still planned

- The **package split** into `Loam.Charts` / `Loam.Pickers` / `Loam.Data` satellites (Phase 4, ADR-0009).
- Release polish: visual-regression coverage and the final `3.0.0` cut (Phase 6).

## 2.0.0

The v2 rebaseline: role-based light/dark color schemes, expanded foundation tokens, and tokenized state
feedback across the full component catalog.
