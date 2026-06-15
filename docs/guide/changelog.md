---
title: Changelog
---

# Changelog

Notable changes per release. Dates are when the work landed on the development branch.

## 3.24.0

**`Loam.Charts` scatter and bubble charts.** Two new controls plot `(X, Y)` data on **numeric** axes (the first
charts with a numeric X axis, not a category index): **`ScatterChart`** draws evenly sized markers, and
**`BubbleChart`** sizes each marker by a third value (`ScatterPoint.Size`, area-proportional). They share a new
`XYChartBase` with nice-number X/Y domains (`XMin`/`XMax`/`YMin`/`YMax`, `AxisTickCount`, `XAxisFormat`/`YAxisFormat`,
`ShowAxes`). Data is a `ScatterPoint(X, Y, Size, Label)` list via `Points`, or multiple `ScatterSeries` via `Series`;
hover/click resolve the datapoint and the tooltip shows the `(x, y)` pair. `ChartPoint` gains `X`/`Size`; new public
helpers `Charts.MapLinear`, `Charts.Extent`, and `Charts.BubbleRadius`. Additive.

## 3.23.0

**`Loam.Charts` radar chart.** A new **`RadarChart`** (spider chart) draws one axis per category radially, with
each series as a polygon whose vertices sit at `value / max`. It introduces a shared **`MultiSeriesChartBase`**
(extracted from `CartesianChartBase`, which now derives from it) so radar reuses the same multi-`ChartSeries`
collection, legend, theme palette, tooltip, hover/click, and empty state as bar/line — without inheriting
Cartesian axes. Single-series via `Values`, multi-series via `Series`; category names from `Labels`; `Max`,
`Levels`, and `Filled` tune the scale, grid rings, and fill. Backed by an internal `Charts.RadarPoints` mapping
helper. Needs at least three axes. Additive.

## 3.22.0

**`Loam.Charts` gauge and sparkline.** Two new chart controls join `PieChart`/`BarChart`/`LineChart` on the
shared `ChartBase`: **`RadialGauge`** draws a single value as a filled arc over a track with an optional
center readout (`Value`/`Minimum`/`Maximum`, `StartAngle`/`SweepAngle`/`Thickness`, `Format`/`CenterText`/
`Caption`), and **`Sparkline`** is a compact inline line/bar strip (`Mode`) for tables and KPI tiles, with
chrome and tooltips off by default. Both inherit the theme palette, hover/click, and empty state, and expose
their value to assistive technology. Adds the `Charts.GaugeFraction` helper. Additive.

## 3.21.0

**`Loam.Data` DataGrid keyboard navigation.** `DataGrid<T>` rows now support full keyboard navigation when focused:
**↑/↓** move row focus, **Home/End** jump to the first/last rendered row, **Shift**+↑/↓/Home/End extend the selection
to the focused row in `Multiple` mode, **Ctrl+A** selects the rendered rows (current page), and **Esc** clears the selection. In
`Single` mode the selection follows focus; navigation stays within the current page and does not wrap. Cell editors
and column headers keep their own keys (navigation only engages while a row is focused). Builds on 3.20 row selection.
Additive.

## 3.20.0

**`Loam.Data` DataGrid row selection.** `DataGrid<T>` gains opt-in `SelectionMode` (`None` / `Single` /
`Multiple`), `SelectedItem`, `SelectedItems`, and a `SelectionChanged` event. Click selects; in `Multiple`,
Ctrl-click (or Ctrl+Space) toggles and Shift-click (or Shift+Space) selects a range; selected rows are
highlighted and the selection survives sort/filter/page rebuilds by item identity (items filtered out of the
view are dropped). Ctrl+C now copies the **selected** rows when a selection exists (otherwise the whole view,
as before). Assigning `SelectedItem` replaces the selection and raises `SelectionChanged`; a disabled grid is
not selectable; selected rows expose their state to assistive technology (`AutomationProperties.ItemStatus`).
`SelectionMode` defaults to `Single`, preserving the prior click-to-select behavior. Additive.

## 3.19.0

**`Loam.Data` DataGrid clipboard copy.** `DataGrid<T>` gains `CopyToClipboardAsync()`, which copies the current
view (filtered + sorted, all pages) to the system clipboard as TSV (reusing `ExportTsv()`) and returns the
copied text — also wired to **Ctrl+C** / **Cmd+C** when focus is within the grid. Uses Avalonia 12's
`TopLevel.Clipboard` (`ClipboardExtensions.SetTextAsync`); returns `null` when no clipboard is available. This
closes the clipboard loose end deferred since 3.6 (the Avalonia 12 clipboard API had moved). Additive.

## 3.18.0

**`Loam.Pickers` ColorPicker validation.** `ColorPicker` gains a `Validation` delegate (`Func<Color, string?>`)
and a public `Validate()`, completing validation across all four field pickers. It runs on every value change
(palette select, editable hex commit, programmatic) and drives `Error`/`ErrorText`, self-gates to preserve a
manually-set error when unset, and lets an editable parse error take precedence. There is no `Required` because
`Value` is non-null. Additive and off by default.

## 3.17.0

**`Loam.Pickers` field-picker validation hooks.** `DatePicker`, `TimePicker`, and `DateRangePicker` gain
`Required` (with `RequiredText`), a `Validation` delegate (returns an error message or `null`), and a public
`Validate()` — mirroring `TextField`. Validation runs automatically whenever the value changes (flyout OK,
editable commit, `Clear()`, programmatic) and drives `Error`/`ErrorText`. It self-gates so a manually-set
`Error`/`ErrorText` is preserved when neither `Required` nor `Validation` is configured, and in editable mode a
parse/format error takes precedence over business validation. Additive and off by default.

## 3.16.0

**`Loam.Pickers` ColorPicker editable hex entry.** Set `Editable` on `ColorPicker` to type or paste a hex
color (`#RRGGBB`, or `#AARRGGBB` when `ShowAlpha`) directly into the field — the full 24-bit space rather than
just the curated palette. The text is committed on Enter or focus loss and parsed via Avalonia's color parser;
unparseable input leaves `Value` unchanged and shows `InvalidHexText` (empty reverts, since a color has no
empty state). In editable mode the swatch (or `Alt+Down`) opens the palette flyout, which stays in sync. A
public static `ColorPicker.TryParseColor(text, out color)` exposes the parse rule. This brings editable entry
to all four field pickers. Additive and off by default.

## 3.15.0

**`Loam.Pickers` DateRangePicker editable text entry.** Set `Editable` on `DateRangePicker` to type a range
into the field, completing editable support across all three field pickers. The text is committed on Enter or
focus loss: a `"start – end"` pair (also `" to "` / `" - "` separators) is parsed by reusing
`DatePicker.TryParseDate` for each half and auto-ordered, a single date sets only `Start`, and both endpoints
are validated against `MinDate`/`MaxDate` (`InvalidRangeText` on failure); empty clears both and committed text
is normalized to the canonical en-dash form. The calendar icon becomes a button (and `Alt+Down` works) so the
flyout still opens and stays in sync. A public static `DateRangePicker.TryParseRange(text, format, out start,
out end)` exposes the parse rule. Additive and off by default.

## 3.14.0

**`Loam.Pickers` TimePicker editable text entry.** Set `Editable` on `TimePicker` to type a time into the
field (mirroring `DatePicker`). The text is committed on Enter or focus loss — parsed exactly against
`TimeFormat`, then loosely against the current culture or `TimeSpan` — and `InvalidTimeText` is shown when it
cannot be parsed; empty clears the value and committed text is normalized to `TimeFormat`. The clock icon
becomes a button (and `Alt+Down` works) so the flyout still opens and stays in sync. A public static
`TimePicker.TryParseTime(text, format, out value)` exposes the parse rule. Additive and off by default.

## 3.13.0

**`Loam.Pickers` DatePicker editable text entry.** Set `Editable` on `DatePicker` to let the user type a
date into the field. The text is committed on Enter or focus loss — parsed exactly against `DateFormat`,
then loosely against the current culture — and validated against `MinDate`/`MaxDate`; unparseable or
out-of-range input leaves `Date` unchanged and surfaces `InvalidDateText` in the error slot. The trailing
calendar icon becomes a button (and `Alt+Down` works) so the flyout still opens and stays in sync. A public
static `DatePicker.TryParseDate(text, format, out value)` exposes the parse rule. Additive and off by default.

## 3.12.0

**`Loam.Pickers` leading adornment icons.** `DatePicker`, `TimePicker`, and `DateRangePicker` gain an
opt-in `AdornmentIcon` (a glyph from `Loam.Icons`) rendered at the start of the field. The value text,
resting label, and floating label all indent to the icon's right so nothing overlaps — across the
outlined, filled, and text variants. Additive and off by default; the leading slot takes zero space when
unset.

## 3.11.0

**`Loam.Pickers` TimePicker auto-scrolls to the selected time.** When the flyout opens, the hour and
minute columns now scroll to center the selected (or closest) value instead of starting at the top, so a
late time like `22:55` is visible immediately. The focused row is also kept in view during keyboard
navigation. This required making the columns actually scrollable (the vertical scroll mode was previously
`Disabled`, which pinned content to the viewport); they are now `Hidden` — scrollable with no visible
scrollbar.

## 3.10.0

**`Loam.Pickers` DateRangePicker quick-select presets.** Set `ShowPresets` to add a one-click shortcut
rail beside the calendar in the flyout. Clicking a preset stages a **pending** range (highlighted on the
calendar) that the user still confirms with OK, so presets compose with the two-click commit model. The
resolved range is auto-ordered and clamped to `MinDate`/`MaxDate`. `DefaultPresets` supplies Today,
Yesterday, Last 7 days, Last 30 days, This month, Last month, and This year; add `DateRangePreset` items
to `Presets` to supply your own. Additive and off by default.

## 3.9.0

**`Loam.Pickers` clearable fields.** `DatePicker`, `TimePicker`, and `DateRangePicker` gain an opt-in
`Clearable` flag. When set and the field holds a value, an inline trailing × button appears; clicking it
clears the value, raises the change event (`DateSelected`/`TimeSelected`/`RangeSelected`) with `null`,
and does not open the flyout. The button hides automatically when the field is empty. Surfaces the
existing `Clear()` API; additive and off by default.

## 3.8.0

**`Loam.Data` DataGrid footer aggregates.** Opt-in `ShowFooter` renders a totals row aligned to the
column layout (single-grid and frozen-pane), computed over the current filtered rows (all pages).
`DataGridColumn<T>` gains `Summary` (custom `Func<IReadOnlyList<T>, string>`) and a `SummaryKind` preset
(`Sum`/`Average`/`Min`/`Max`/`Count`, honoring `Format`). Additive.

## 3.7.0

**`Loam.Data` async states & pagination polish:**

- **DataGrid async states.** `IsLoading` shows a skeleton body; `ErrorText`/`ErrorContent` (+ `OnRetry`
  Retry button) show an error body; precedence is Error > Loading > Empty > data. `SkeletonRowCount`
  tunes the skeleton. Reuses the empty-state plumbing.
- **Pagination polish.** `ShowFirstLast` adds first/last boundary buttons; `ShowRange` (with `PageSize`/
  `TotalItems`) shows a "Showing X–Y of N" summary. `DataGrid<T>`'s built-in pager enables both.

## 3.6.0

**`Loam.Data` liveness & egress.** `DataGrid<T>` becomes live and exportable:

- **Live binding.** When `Items` implements `INotifyCollectionChanged` (e.g. `ObservableCollection<T>`),
  add/remove/reset refresh the grid automatically — no more reassigning `Items`. Subscriptions are
  managed across attach/detach to avoid leaks. Opt-in `ObserveItemChanges` refreshes when a row raises
  `INotifyPropertyChanged`; `Refresh()` forces a refresh for non-observable sources. Behavior is
  unchanged for plain sources.
- **Export.** `ExportCsv()`/`ExportTsv()` return the current view (filtered + sorted, all pages) using
  each column's display text with RFC-4180 quoting, backed by the pure `DataGrids.ToDelimited` helper.

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
