---
title: Changelog
---

# Changelog

Notable changes per release. Dates are when the work landed on the development branch.

## 3.6.0 (in progress)

**`Loam.Data` liveness.** `DataGrid<T>` now observes its source:

- When `Items` implements `INotifyCollectionChanged` (e.g. `ObservableCollection<T>`), add/remove/reset
  refresh the grid automatically — no more reassigning `Items`. Subscriptions are managed across
  attach/detach to avoid leaks.
- Opt-in `ObserveItemChanges` refreshes when a row raises `INotifyPropertyChanged`; `Refresh()` forces a
  refresh for non-observable sources. Behavior is unchanged for plain (non-observable) sources.

## 3.5.0

**`Loam.Charts` multi-series.** Set `Series` (a list of `ChartSeries`) on bar/line charts:

- **Bar:** `StackMode` of `Grouped` (side-by-side), `Stacked`, or `StackedPercent` (per-category 100%),
  backed by the pure `Charts.StackedBarHeights` helper.
- **Line:** one line per series.
- The Y-axis domain spans all series (and stacked totals); the per-point snapshot, hover/tooltips, and
  hit-testing carry a `ChartPoint.SeriesIndex`. The single-series path is unchanged.
- **Bound legend:** `ChartLegend.Source` derives its rows (one per series, or per category) from a chart
  and refreshes automatically — no hand-syncing `Labels`/`Colors`.

**`Loam.Charts` analytical depth** — axes and data binding (additive; no breaking changes):

- **Axes.** `BarChart`/`LineChart` share a new `CartesianChartBase` with an opt-in `ShowAxes` that draws a
  nice-number numeric Y-axis (ticks + labels) and a category X-axis (from `Labels`), with `Min`/`Max`,
  `YAxisTickCount`, and `YAxisFormat` controls. Off by default, so existing charts are unchanged. New pure
  `Charts.NiceScale` helper (1/2/5×10ⁿ rounding); bar/line scaling unified over a value-axis domain (the
  signed-value helpers generalized), so axes, signed data, and explicit ranges share one path.
- **`ItemsSource` binding** (all charts): project items via `ValueSelector`/`LabelSelector`/`ColorSelector`;
  an `INotifyCollectionChanged` source (e.g. `ObservableCollection<T>`) refreshes the chart live.

_Multi-series/stacked charts and the chart-bound interactive legend are the next charts milestone._

## 3.3.0

**`Loam.Charts` interactivity** — charts become pointer-aware (additive; default-on tooltips):

- **Hit-testing.** Each chart resolves the datapoint under the pointer (pie slice by angle/radius, bar by
  rect, line by nearest point), exposed as `HoveredIndex` with `HoverChanged`/`PointClicked` events
  (`ChartPointEventArgs`). The hovered element is lightly emphasized.
- **Hover tooltips.** A self-drawn, tokenized tooltip follows the pointer for the hovered datapoint —
  `ShowTooltip` (on by default) and `TooltipFormat` control it.

## 3.2.0

First milestone of the **`Loam.Charts` enrichment** roadmap — all additive, no breaking changes:

- **Per-point snapshot.** `ChartBase` gains `Labels` and a `ResolvedPoints` projection (`ChartPoint`
  record struct: index, value, percent-of-positive-total, label, color). Labels enrich the chart's
  accessibility help text; the snapshot is the shared source for rendering, automation, and later
  tooltips/legends.
- **Donut center text.** `PieChart` can fill the hole with a KPI via `CenterText`/`CenterSubText` or an
  auto-formatted `CenterValue`/`CenterValueFormat` (defaults to the positive-value total).
- **Signed values (correctness).** `BarChart`/`LineChart` gain an opt-in `AllowNegative` that draws
  negatives below a zero baseline instead of silently clamping them to zero; new pure
  `Charts.SignedDomain`/`ZeroBaselineOffset`/`SignedBarLayout` helpers back it. Default behavior is
  unchanged.
- **On-chart data labels.** Opt-in `ShowDataLabels` (+ optional `DataLabelFormat`) annotates each
  datapoint — value above bars/line points, percentage at pie-slice centroids (with a contrast-aware
  color) — with responsive thinning that drops colliding labels instead of overlapping them.

## 3.1.0

**Modular packaging (ADR-0009).** The chart, picker, and heavy data controls move out of the core
`Loam` package into three opt-in satellite packages, so apps that don't use them ship a leaner core:

- **`Loam.Charts`** — `PieChart`, `BarChart`, `LineChart`.
- **`Loam.Pickers`** — `DatePicker`, `TimePicker`, `ColorPicker`, `DateRangePicker`, `MonthCalendar`.
- **`Loam.Data`** — `DataGrid<T>`, `SimpleTable`, `TreeView`/`TreeViewItem`, `Pagination`.

Each satellite depends on the core package and ships its own `Styles` registrar
(`LoamCharts`/`LoamPickers`/`LoamData`, in the `Loam.Theming` namespace). **Breaking for these control
groups only:** add the package reference *and* register its registrar after `LoamTheme`. **Namespaces
are unchanged** — every type stays under `Loam.Controls`, so call sites and using-directives don't move.
All four packages version in lockstep. See the [v3 → v3.1 migration guide](/migration/v3-to-v3.1).

The remaining controls (Tabs, Stepper, Carousel, Timeline, ExpansionPanel, lists, and the rest) stay in
the core package and need no change.

## 3.0.0

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

### Followed in 3.1.0

- The **package split** into `Loam.Charts` / `Loam.Pickers` / `Loam.Data` satellites (ADR-0009) shipped
  in [3.1.0](#_3-1-0).

## 2.0.0

The v2 rebaseline: role-based light/dark color schemes, expanded foundation tokens, and tokenized state
feedback across the full component catalog.
