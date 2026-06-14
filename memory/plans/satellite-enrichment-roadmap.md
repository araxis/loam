# Satellite package enrichment roadmap — Loam.Charts → Loam.Pickers → Loam.Data

Created 2026-06-14. Source: source-grounded audit of each package + three-lens feature design
(peer-parity, Material/UX/a11y, app-developer) + synthesis. 3.1.0 (the package split) is already
shipped, so suggested version numbers below start at 3.2.

## Through-line

Three pieces of shared infrastructure are built once (in Charts) and reused across all three packages:

1. **Per-element data snapshot** — a value-type projection (`ChartPoint` in Charts; the row/cell model
   in DataGrid) rebuilt on data/label/color change, driving rendering, automation, tooltips, keyboard
   nav, peers, and export from one source of truth so spoken and visible output never drift.
2. **Custom `AutomationPeer` pattern** — charts (per-datapoint) → DataGrid (grid/row/cell) → TreeView
   (tri-state). Establish the peer-lifecycle-vs-full-rebuild discipline once; verify against a real
   screen reader, not just headless peer-tree assertions.
3. **Data egress / clipboard** — `RenderToBitmap`/PNG/clipboard (Charts) and `DataGrids.ToDelimited` +
   CSV/TSV/clipboard (Data), plus the `TopLevel.GetTopLevel(this).Clipboard` access path. No
   `RenderTargetBitmap`/Clipboard usage exists in the repo yet.

The hard architectural tension across all three: `ChartBase`, `MonthCalendar`, and `DataGrid<T>` all
**rebuild their whole visual tree on every property change**. This is fundamentally at odds with
recycling virtualization (the last, riskiest Data item) and complicates incremental update / focus
preservation everywhere.

---

## Loam.Charts

**Today:** three correct-but-static, single-series, self-rendering charts (`ChartBase : Control`,
`Render(DrawingContext)`), an unbound `ChartLegend`, pure-tested static `Charts` math, an empty
`LoamCharts` registrar. Read-only: no axes, no hit-testing/tooltips, no keyboard/per-point a11y,
single-series only, negatives silently clamped to zero (a correctness bug), no data binding,
hand-synced legend.

| # | Feature | Target | Value | Effort | Horizon |
|---|---|---|---|---|---|
| 1 | **Per-point snapshot** (`Labels` + projected `ChartPoint` list) | ChartBase | high | M | near |
| 2 | **Pointer hit-testing** (slice/bar/point index under pointer) | ChartBase | high | L | near |
| 3 | **Donut center text** (`CenterText`/`CenterSubText`/`CenterValueFormat`) | PieChart | high | S | near |
| 4 | **Signed-value support** + zero baseline (correctness fix) | static Charts + Bar/Line | high | M | near |
| 5 | Value tooltips on hover | ChartBase | high | M | near |
| 6 | Keyboard nav + per-point screen-reader announcements | ChartBase | high | L | mid |
| 7 | Per-datapoint `AutomationPeers` | ChartBase | medium | M | mid |
| 8 | On-chart data labels + responsive thinning | ChartBase | medium | M | mid |
| 9 | **Numeric Y-axis + category X-axis** (tick labels, nice-number scaling) | Bar/Line | high | L | mid |
| 10 | **`ItemsSource` binding** (value/label/color selectors) | ChartBase | high | L | mid |
| 11 | **Multi-series**: grouped + stacked bars, multiple lines | Bar/Line | high | XL | long |
| 12 | Legend bound to chart (auto labels/colors, toggle, hover highlight) | ChartLegend | medium | M | long |
| 13 | Animated data-change transitions + reduced-motion guard | ChartBase | medium | M | long |
| 14 | Loading / error states (beyond empty) | ChartBase | medium | M | long |
| 15 | Rendering variants: spline/stepped lines, horizontal bars, markers | Line/Bar | medium | M | long |
| 16 | Time-series / categorical X mapping | LineChart | medium | M | long |
| 17 | Size/density variants + themeable ControlTheme (populate `LoamCharts`) | LoamCharts + ChartBase | medium | M | long |
| 18 | Export/copy: render to PNG + clipboard | ChartBase | medium | M | long |

**Quick wins:** #3 donut center text (S), #4 signed values (M, correctness), #1 snapshot (M, unblocks
everything), #8 data labels (M).

---

## Loam.Pickers

**Today:** five solid self-contained Material controls (no FluentTheme Calendar dependency), but
**picker-only display fields**: every value chosen via popup, `MonthCalendar` pages one month at a time
(no year/decade jump), `TimePicker` is 24h-only and opens scrolled to top, `ColorPicker` is palette-only
(no hex/spectrum), `DateRangePicker` has no presets or dual-month, no visible Clear, no validation hook,
no per-control Culture, and `MonthCalendar` is the lone un-themable `Decorator`.

| # | Feature | Target | Value | Effort | Horizon |
|---|---|---|---|---|---|
| 1 | **MonthCalendar `CalendarView` state machine** (Days/Months/Years) + `OpenTo` | MonthCalendar | high | L | mid |
| 2 | **Editable text entry** + culture-aware parsing | Date/Time/Range | high | L | mid |
| 3 | **Visible Clear / None** affordance | all field pickers | high | S | near |
| 4 | TimePicker auto-scroll-to-selection + arrow stepping | TimePicker | medium | S | near |
| 5 | **Quick-preset rail** (Today, Last 30 days, This quarter…) | Date/Range | high | M | near |
| 6 | Predicate-disabled days + per-day decorators + week numbers | MonthCalendar | high | M | mid |
| 7 | **Dual-month side-by-side** range view | DateRangePicker | high | L | mid |
| 8 | Customizable leading/trailing adornment icons | all field pickers | medium | S | near |
| 9 | **Explicit `Culture`** (decoupled from CurrentCulture) + FirstDayOfWeek + RTL | all | medium | M | mid |
| 10 | **Validation integration** (INotifyDataErrorInfo/DataAnnotations) + `Required` | field pickers | high | M | mid |
| 11 | TimePicker 12h/AM-PM, seconds, Min/Max bounds | TimePicker | high | L | mid |
| 12 | **ColorPicker HSV spectrum editor** (canvas/sliders + hex/RGB) | ColorPicker | high | XL | long |
| 13 | ColorPicker recent-colors + No-color | ColorPicker | low | S | long |
| 14 | Promote MonthCalendar to themable registered ControlTheme (+ multi-select) | MonthCalendar | low | L | long |

**Quick wins:** #3 visible Clear (S, API already exists), #4 TimePicker auto-scroll (S), #8 adornment
icons (S), #5 preset rail (M, headline dashboard feature). Note: #2 editable entry depends on #9 Culture
(for deterministic parsing).

---

## Loam.Data

**Today:** a mature single-row table/tree/pagination package; self-rendering `DataGrid<T>` already does
sort/filter/page/group/freeze/stripe/inline-edit/empty-state. Four structural gaps block real LOB/a11y
use: (1) **liveness** — `Items` is `.ToList()`-snapshotted in `Rebuild()` and never observes its source;
(2) **selection breadth** — one `SelectedItem` only, no multi/checkbox/range; (3) **a11y depth** — bare
Border/Grid with only `SetName`, no 2D table model; (4) **scalability** — `Virtualize` merely truncates
to `MaxRenderedRows`.

| # | Feature | Target | Value | Effort | Horizon |
|---|---|---|---|---|---|
| 1 | **Live data binding** (observe INotifyCollectionChanged/INPC) | DataGrid | high | M | near |
| 2 | Pagination first/last + rows-per-page + "X–Y of N" | Pagination | medium | S | near |
| 3 | **Footer summary / aggregate row** | DataGrid + Column | high | M | near |
| 4 | **Clipboard copy + CSV/TSV export** | DataGrid | high | S | near |
| 5 | Async states: loading skeleton / error / empty + live regions | DataGrid | medium | S | near |
| 6 | **Multi-row selection** (mode, SelectedItems, checkbox col, range, select-all) | DataGrid | high | L | mid |
| 7 | **Real AutomationPeers** (grid/row/cell roles + selection) | DataGrid | high | L | mid |
| 8 | **Per-column header menu**: multi-sort + priority badges + column filtering | DataGrid + Column | high | L | mid |
| 9 | **Sticky header** + frozen row band on vertical scroll | DataGrid | high | L | mid |
| 10 | Expandable row details / master-detail | DataGrid | medium | M | mid |
| 11 | Validated commit/cancel inline editing with typed editors | Column | medium | L | mid |
| 12 | TreeView checkbox tri-state selection + parent rollup | TreeView | medium | L | mid |
| 13 | TreeView lazy/on-demand child loading + node states | TreeViewItem | medium | L | mid |
| 14 | TreeView incremental search / type-ahead + filter highlight | TreeView | medium | M | mid |
| 15 | Cell-level keyboard nav + Ctrl+C | DataGrid + SimpleTable | medium | M | long |
| 16 | Resizable/reorderable/hideable columns + chooser + persistable layout | DataGrid + Column | medium | L | long |
| 17 | **True UI virtualization** (recycling, replaces MaxRenderedRows) | DataGrid | high | XL | long |

**Quick wins:** #1 live binding (M, removes the most surprising production gap), #2 pagination (S),
#4 export (S), #5 async states (S), #3 footer (M).

---

## Release roadmap — value/demand-first, weighted to dashboards/charts

Re-sequenced 2026-06-14 per user direction: optimize for **value/demand-first** (ship the most-wanted
features earliest, accept cross-package context-switching) with **dashboards/charts** as the primary use
case. This supersedes the original risk-first (charts→pickers→data, package-cohesive) order, which is
preserved in the git history of this file. Dependency edges and the "build shared patterns once" order
still hold (snapshot in M1, export in M3, peers in M7).

| Version | Theme | Headline features |
|---|---|---|
| **3.2 (M1)** | Charts: dashboard foundation | per-point snapshot, donut center text, signed values (fix), data labels |
| **3.3 (M2)** | Charts: interactivity | hit-testing, hover tooltips |
| **3.4 (M3)** | Live dashboard data | DataGrid live binding, footer aggregates, CSV/export+copy, async states, pagination; **Chart `ItemsSource`** |
| **3.5 (M4)** | Charts: analytical depth | ✅ axes + nice-number scaling and chart `ItemsSource` shipped in **v3.4.0**; remaining: multi-series/stacked, bound interactive legend, time-series |
| **3.6 (M5)** | Dashboard filtering + chart polish | Pickers: preset rail, visible Clear, adornment icons, TimePicker auto-scroll. Charts: loading/error, animated transitions |
| **3.7 (M6)** | Grid power | multi-select, header-menu (multi-sort + filtering), sticky header |
| **3.8 (M7)** | Accessibility pass | chart keyboard nav + per-point peers, DataGrid AutomationPeers |
| **3.9 (M8)** | Charts finish + grid richness | rendering variants, density theming, PNG export; DataGrid master-detail, typed editing |
| **3.10 (M9)** | Forms-grade pickers | CalendarView state machine, Culture, editable parsing, validation, dual-month, disabled-days, TimePicker 12h/bounds |
| **3.11 (M10)** | Tree + heavy editors + scalability | TreeView checkbox/lazy/search; ColorPicker HSV editor; cell-nav, column resize/reorder/chooser, **true virtualization** |

Sequencing rationale: charts interactivity stays front-loaded (primary use case), but the highest-demand
**DataGrid** wins (live binding, export, footer totals, async) jump forward to M3 — paired with chart
`ItemsSource` so grids and charts bind to the same live source. Forms-oriented picker work (editable
entry, validation, HSV editor) drops to M9–M10 as least relevant to dashboards; only the
dashboard-relevant picker slice (date-range presets + visible Clear) is pulled up to M5. Virtualization
(XL, risky, dependency-bound on live binding + sticky header + multi-select) stays last.

**Tradeoffs of this order vs the risk-first original:**
- More cross-package context-switching (charts ↔ data interleave in M1–M5).
- **Accessibility (peers/keyboard) consolidates into M7 (3.8)** rather than shipping with each package —
  pull M7 earlier if you have a WCAG/Section-508/procurement deadline.
- `MonthCalendar`'s fragile refactors (CalendarView, dual-month) land in M9 with less "pattern maturity"
  runway than the original gave them — budget extra test time there.

## Cross-cutting investments (build once, reuse)

- Per-element data snapshot pattern (Charts → Data).
- Custom AutomationPeer pattern + real-AT verification (Charts → Data → Tree).
- Data egress/clipboard helper + `TopLevel` access path with detached/headless fallback.
- Self-drawn overlay + pointer-interaction substrate (hit-test + hovered/focused index + clipped draw).
- Density + ControlTheme token vocabulary for the empty/under-themed registrars (`LoamCharts`,
  `MonthCalendar`).
- INotifyCollectionChanged/INPC subscription lifecycle reusing the existing `AnonObserver` +
  `OnDetachedFromVisualTree` teardown; scope per-item subscriptions to rendered rows.
- Culture-aware formatting/parsing routed through an explicit `Culture` property (precondition for
  deterministic headless tests of parsing and CSV).
- Test + gallery scaffolding: three lanes per feature — pure-math/serializer xUnit, Avalonia headless
  (peers/keyboard/selection), gallery sample + XML-doc page.

## Top risks

- **Self-render-vs-recycling tension** — biggest architectural risk; true DataGrid virtualization (XL)
  most likely to slip / require a partial ItemsRepeater rewrite. Keep last, budget heavily.
- **AutomationPeer lifecycle + verification cost**; Avalonia 12.0.4 `IGridProvider`/`ISelectionProvider`
  is under-documented — verify the exact API early.
- **MonthCalendar fragility** (whole-tree rebuild + focus-by-automation-name) multiplied by views,
  predicates, dual-month, week-numbers, RTL; a focus/automation regression breaks both date pickers.
- **Fixed-width picker layouts** (336px calendar, 360px PickerPaper) must absorb preset rails,
  dual-month, week-numbers, spectrum editor — decide a responsive/narrow fallback before 3.6–3.9.
- **Default-on visual changes** (axes, animations, AllowNegative) can break existing charts — gate
  behind opt-in or one-version-opt-in.
- **Culture/parse + CSV round-trips** are culture-sensitive — enforce Culture-before-parsing.
- **Combinatorial Data test matrix** (multi-sort × filter × group × frozen × selection × sticky ×
  virtualization) — invest in headless interaction tests early.
- **Two-way feedback loops** in editors (ColorPicker hex/RGB↔canvas; DataGrid commit/cancel vs live
  SetText) — known clobber/infinite-loop hazards.

## Recommended first slice — 3.2 (Charts foundations + quick wins)

> **Status (2026-06-14):** all four 3.2 items — snapshot, donut center text, signed values, **and
> on-chart data labels** — are **implemented and visually verified** on the `work/charts-3.2` branch
> (full suite 466 green). Milestone 3.2 is feature-complete.

Implement in this order, all additive and breaking nothing:

1. **Per-point snapshot on `ChartBase`** — `public readonly record struct ChartPoint(int Index, double
   Value, double Percent, string? Label, Color Color)`; a `Labels : IReadOnlyList<string>?` plain-CLR
   prop mirroring `Values` (setter → `RebuildPoints()` + `UpdateAutomation()` + `InvalidateVisual()`);
   `protected internal IReadOnlyList<ChartPoint> ResolvedPoints` rebuilt on Values/Colors/Labels change
   (reuse `ResolvedSeriesColors` for color, positive-total for Percent). Upgrade `UpdateAutomation()`
   help text from "{n} values" to optionally listing labels. Zero-dependency substrate.
2. **Donut center text on PieChart** (parallel) — `CenterText`/`CenterSubText`/`CenterValueFormat` drawn
   with `FormattedText` in the existing hole ellipse, clipped to hole radius, only when `Donut`.
3. **Signed-value correctness fix** (parallel) — pure `SignedDomain` + `SignedBarLayout` helpers in
   static `Charts`; opt-in `AllowNegative` (default false to preserve all-positive callers) so Bar/Line
   stop `Math.Max(0, …)`-zeroing negatives.

Land each with pure-math xUnit tests (alongside `ChartTests.cs`) and a `Loam.Gallery` sample. This slice
establishes the snapshot + test/gallery patterns the rest of the program reuses.
