# Progress Log

Newest entries on top. One entry per meaningful unit of work. Keep it factual and dated.

Format: `## YYYY-MM-DD — Phase N — short title` → What was done · Decisions · Verified facts ·
Next.

---

## 2026-06-15 — 3.23 — Charts: RadarChart (spider)

Second charts roadmap item. **RadarChart** (`src/Loam.Charts/RadarChart.cs`): one axis per category arranged radially,
each series a polygon at `value/max`; `Max`/`Levels`/`Filled`; single-series via `Values`, multi via `Series`; category names
from `Labels`; needs ≥3 axes (else no-data); transparent-fill hit-test; flat vertex array (series-major `s*categories+c`)
matching `ResolvedPoints`. New internal pure helper `Charts.RadarPoints(values,max,center,maxRadius)` (angle `-90+i*360/n`,
radius clamped).

**Refactor:** extracted **`MultiSeriesChartBase : ChartBase`** holding the Series machinery (`Series`, `HasSeries`, `SeriesList`,
`SeriesCategoryCount`, `SeriesColor`, `GetLegendEntries`, `BuildPoints`) out of `CartesianChartBase` (which now derives from it).
BarChart/LineChart inherit transitively — unchanged. RadarChart extends `MultiSeriesChartBase` directly (no Cartesian axes).
This avoids duplicating ~60 lines of series logic. Existing 33 ChartTests stayed green through the refactor (no regression).

**Verified:** solution 0/0; full suite **622 → 626 (+4)** — RadarPoints math (axes/radius/clamp/empty), render-without-throwing
(single/multi/<3-axes/zero), multi-series snapshot series-index, vertex hover+click+payload; gallery (single/series+legend/empty)
+ acceptance green; docs (charts.md RadarChart section + Choosing row + intro "six" + a11y name), overview, changelog 3.23.0,
Directory.Build.props → 3.23.0.

**Adversarial review (12 agents, 5/9 confirmed) → fixes:** (major) multi-series charts announced "No data" to screen
readers because `UpdateAutomation` counted `Values` (empty for Series-driven charts) → now counts the `_points` snapshot and
de-dupes labels, fixing radar AND bar/line multi-series a11y (+ test). (minor) added a multi-series radar hover test (series-1
vertex → series-major flat index 4, SeriesIndex 1) and legend assertions. (nit) removed an unreachable `verts.Count < 3` guard
that could have desynced the flat index. (nit) empty-state now also requires a positive value, so an explicit positive `Max`
with all-zero data shows "No data" instead of a collapsed polygon. (nit) reworded changelog/notes — `RadarPoints` is internal,
not a public helper. Net suite **626 → 627**; all green, docs build clean.

**Next (charts roadmap):** 3.24 Scatter/Bubble (numeric X axis via XYChartBase). Then StackedArea / Heatmap.

---

## 2026-06-15 — 3.22 — Charts: RadialGauge + Sparkline

First charts milestone after the roadmap plan (user picked Gauge+Sparkline as the lowest-risk pair on `ChartBase`).
**RadialGauge** (`src/Loam.Charts/RadialGauge.cs`): a single `Value` over `Minimum`/`Maximum` drawn as a filled arc
(`StartAngle`/`SweepAngle`/`Thickness`) on a theme grid track, with a center readout (`Format`/`CenterText`/`Caption`).
Dedicated `Value` (not `ChartBase.Values`); overrides `BuildPoints` to emit one `ChartPoint`; sets automation help text
in `RefreshGauge` after `RefreshData`; empty state when `Maximum<=Minimum`. **Sparkline** (`Sparkline.cs`): compact inline
`Line`/`Bar` strip (`Mode`), chrome+tooltip off by default (`ShowTooltip=false` in ctor), reuses `Charts.LinePoints`/
`BarHeights`; magnitude-only (non-positive→0). New pure helper `Charts.GaugeFraction(value,min,max)` (clamped 0..1).

**Key discovery — hit-testability:** custom-drawn `ChartBase` controls are hit-tested by *rendered content*; the gauge's
*stroked* arc wasn't registering pointer events (BarChart works because its bars are filled). Fix: both Render methods
`FillRectangle(Brushes.Transparent, Bounds)` so the whole control receives hover/click. (Headless caveat: `hovered.Point`
snapshot capture is timing-flaky, so the hover test asserts the deterministic `HoveredIndex`/`Index` only.)

**Verified:** solution 0/0; full suite **616 → 621 (+5)** — GaugeFraction clamp, gauge+sparkline render-without-throwing
(incl. empty range / empty / bar), gauge automation name+help text, gauge arc hover hit-test, sparkline tooltip-off default;
gallery acceptance 37 green; docs (charts.md RadialGauge/Sparkline sections + Choosing table + GaugeFraction row + a11y),
overview, changelog 3.22.0, Directory.Build.props → 3.22.0.

**Adversarial review (15 agents, 6/12 confirmed, all minor/nit) → fixes:** (1) gauge HitTest used the raw `SweepAngle` while
ArcGeometry clamps to 359.999 → clamp `_arcSweep = Math.Clamp(SweepAngle,0,359.999)` so the hit band matches the drawn arc.
(2,4) empty-range gauge announced a numeric readout while showing "No data" → `GaugeHelpText` returns "No data" when
`Maximum<=Minimum` (+ test). (3) hover tests didn't check the `ChartPoint` payload → gauge test now asserts `Point.Value`/`Label`
and `PointClicked`, plus a new `Sparkline_hover_hit_tests_a_point`; the earlier "Point null" flake was a duplicate-line bug, not
timing. (5) docs intro "three chart controls" → "five". (6) NaN `Value` surfaced "NaN" → `FormatValue` guards non-finite to
`Minimum`. Net suite **621 → 622**; all green, docs build clean.

**Next (charts roadmap):** 3.23 RadarChart; 3.24 Scatter/Bubble (numeric X axis via XYChartBase). Then StackedArea/Heatmap.

---

## 2026-06-15 — 3.21 — DataGrid keyboard navigation (completes row selection)

Natural follow-on to 3.20: focused rows now navigate by keyboard. Grid-level `OnKeyDown` (after the Ctrl+C block) handles
Up/Down (move focus), Home/End (first/last rendered), Shift+Up/Down/Home/End (extend range in Multiple), Ctrl+A (select the
rendered view in Multiple), Esc (clear). Single mode: selection follows focus. Gated on `IsRowFocused(e.Source)` — e.Source is
a Control whose automation name starts with "Row " — so cell editors and headers keep their own keys. New state: `_focusedItem`
(set in row GotFocus), `_renderedRows` (List<T>) + `_rowFocusTargets` (List<Border>) filled incrementally in
`AddRowBackgroundTo` when `isFocusTarget` (single grid + LEFT frozen pane; right pane=false) so index↔row maps in VISUAL order
across grouping/frozen/paging; both cleared atop `RebuildCore`. `MoveRowFocus`/`ExtendSelectionTo`/`SelectViewRows` pass list
COPIES (GetRange/ToList) since SetSelection→Rebuild clears `_renderedRows`. Navigation is page-scoped and does not wrap.

**Decision — focus after rebuild:** a synchronous `Focus()` on a row created by the selection-driven rebuild did NOT stick
(tests showed IsFocused false only after a rebuild). Fix: `FocusRow` sets `_focusedItem` synchronously (so chained nav indexes
correctly) but posts `target.Focus()` via `Dispatcher.UIThread.Post` so it lands after the new tree is attached+laid out. Required
`using Avalonia.Threading;`.

**Verified:** solution build 0/0; full suite **611 (+13)** — arrow down/up(no-focus)/home/end, plain-arrow-moves-focus-only
(Multiple), Shift extend + shrink, Ctrl+A (Multiple) + no-op (Single), Escape clears, None moves focus only, header arrow ignored,
End stays within page, frozen both-panes highlight. Docs: data-display.md Keyboard table + SelectionMode note; changelog 3.21.0;
Directory.Build.props → 3.21.0. Gallery: selection sample caption + status mention arrows/Ctrl+A.

**Adversarial review (20 agents, 6/17 confirmed) → fixes:** (1 major) keyboard Shift-extend collapsed cross-page selections —
rerouted through `SelectRow(target, Shift)` (same all-pages path as Shift-click; seeds anchor on the focused row for the first
extend), removing the page-scoped `ExtendSelectionTo`. (2 major) navigation snapped to the first value-equal row (records/structs)
— now track `_focusedIndex` directly (captured per rendered border in GotFocus) instead of FindIndex-by-value; `FocusedRowIndex`
is range-checked. (3 major) disabled grid was keyboard-selectable — nav block now gated on `IsEnabled`. (4 minor) `IsRowFocused`
keyed off the "Row " name prefix (locale/custom-content fragile) — now structural via a `_rowBorders` HashSet. (5 minor) docs/
changelog/notes said Ctrl+A selects "the current view" — corrected to "rendered rows (current page; expanded groups)". (6 minor)
added Shift+Home and Ctrl+A→Escape tests. Net tests **611 → 616 (+5)**: index-exact nav over value-equal rows, cross-page
Shift-extend, disabled-grid inert, Shift+Home, Ctrl+A→Escape. Suite 616 green, build 0/0.

**Next:** AutomationPeers (SelectionItemPattern) / header context-menu / true virtualization for DataGrid; HSV editor; CalendarView.

---

## 2026-06-14 — 3.20 — DataGrid row selection (single + multiple)

Built on an existing single-select foundation (`_selectedItem`, `SelectRow`, `SelectionChanged`,
`RowVisual.Selected`, `ApplyRowBackgrounds` PaletteSelected highlight, rows already wiring click + activation).
Added: `DataGridSelectionMode` enum (None/Single/Multiple); fields `_selectionMode` (default **Single**, to
preserve the shipped always-select behavior — deviates from the "None default" ask deliberately), `_selection`
(List<T>, ordered, identity-deduped), `_anchorItem`. Public `SelectionMode`, `SelectedItems`
(IReadOnlyList<T> snapshot); reworked `SelectedItem` setter to drive `_selection`. `SelectRow(item, modifiers)`:
None→noop; Single→replace; Multiple→Shift range from anchor (CurrentViewRows order), Ctrl toggle, plain replace.
`SetSelection` dedupes, no-ops when unchanged (only moves anchor), sets primary `_selectedItem`, fires
`SelectionChanged`, Rebuilds. Row PointerPressed passes `e.KeyModifiers`; row KeyDown: Multiple plain Space →
Control (toggle), Shift+Space → range — so range/toggle are keyboard-testable (a11y win). Rebuild prunes
filtered-out items from `_selection` (fires SelectionChanged on change) so selection survives sort/filter/page
by identity (EqualityComparer<T>.Default). Ctrl+C now copies `RowsToCopy()` = selected-in-view-order when a
selection exists, else the whole view (3.19 path).

**Verified:** build 0/0 (solution); full suite **587** (+10): single-select via Space + highlight; None no-op;
Multiple Space toggle; multi-row; Shift+Space range; SelectionChanged primary; survives Rebuild (Striped toggle)
by identity + highlight reapplied; filtered-out clears + fires default; programmatic SelectedItem highlights;
Ctrl+C copies only selected rows. Fix: CA1720 on enum member `Single` (type-name) → SuppressMessage (matches
WPF/Avalonia SelectionMode.Single convention). Gallery: "Row selection" DataGrid (Multiple) sample. Docs:
data-display.md SelectionMode/SelectedItem/SelectedItems/SelectionChanged rows + updated CopyToClipboardAsync;
changelog 3.20.0.

**Adversarial review (30 agents, 21 confirmed) → hardening:** (1) Shift-range when the anchor was filtered out
silently degraded to a plain click — now falls back to the surviving primary selection (`_anchorItem ?? _selectedItem`).
(2) `SelectedItem` setter didn't raise `SelectionChanged` and didn't replace a multi-selection — rerouted through
`SetSelection` (fires on change, no-ops on same value, replaces). (3) Disabled grid was still pointer-selectable —
`SelectRow` now early-returns when `!IsEnabled`. (4) Re-entrancy: a `SelectionChanged` handler mutating the grid mid-
rebuild (e.g. during prune) could restructure a half-built tree — `Rebuild()` now guards with `_rebuilding` and
coalesces a re-entrant request into one follow-up pass (body moved to `RebuildCore`). (5) a11y: selected rows now set
`AutomationProperties.ItemStatus = "Selected"` (full SelectionItemPattern peer left as a follow-up). Plus frozen
shared-`RowVisual` and `RowsToCopy` (sorted, not grouped) clarifying comments. Tests **587 → 598 (+11)**: real
pointer Ctrl/Shift/plain clicks (new `PointerArgs` helper synthesizing `PointerPressedEventArgs`), shift-reanchor,
shift-after-anchor-filtered, survives sort reorder, setter replaces multi + fires, single re-select no-op, frozen
multi-select both panes, ItemStatus, prune-time re-entrant reassign. Docs: removed two stale duplicate rows
(`SelectedItem`/`SelectionChanged`), documented plain-click + disabled. Gallery: live selection-count status line.

**Next:** AutomationPeers (SelectionItemPattern) / header context-menu / true virtualization for DataGrid; HSV editor; CalendarView.

---

## 2026-06-14 — 3.19 — DataGrid clipboard copy (closes the 3.6 loose end)

Switched off the Pickers track. `DataGrid<T>` gains `public async Task<string?> CopyToClipboardAsync()` —
copies the current view as TSV (reuses `ExportTsv()`) via `TopLevel.GetTopLevel(this)?.Clipboard`, returns the
copied text or null if no clipboard; plus an `OnKeyDown` override binding Ctrl+C **and** Cmd+C (Meta, for macOS).

**Resolved the 3.6 deferral:** the original block ("IClipboard lacks SetTextAsync") was an Avalonia-12 API move,
not a removal. Verified empirically via a throwaway probe test: Avalonia 12 `IClipboard` is now a data-transfer
model (`SetDataAsync(IAsyncDataTransfer)` / `TryGetDataAsync`), and the text convenience lives in
`Avalonia.Input.Platform.ClipboardExtensions.SetTextAsync/TryGetTextAsync` (needs `using Avalonia.Input.Platform;`
— the missing using was the whole problem). Also confirmed the **headless platform exposes a real, readable
in-memory clipboard**, so the copy is fully round-trip testable (not just the text-generation).

**Verified:** build 0/0 (solution); full suite **577** (+5): CopyToClipboardAsync returns ExportTsv and round-
trips through the headless clipboard (read back via TryGetTextAsync); Ctrl+C and Cmd+C (Meta) copy the view +
mark Handled; copy returns null when not attached (no TopLevel); editable-cell Ctrl+C is NOT hijacked (the
inner TextBox handles it → grid's `!e.Handled` guard skips). Async `[AvaloniaFact] Task` tests work on the
headless dispatcher. A focused single-agent adversarial review confirmed the design (fire-and-forget best-effort,
cross-platform modifier, no hijack); its flagged editable-cell case is now tested-clean. Docs: data-display.md
CopyToClipboardAsync row; changelog 3.19.0. No gallery (behavioral/clipboard).

**Next:** DataGrid row selection (then copy-selection); HSV spectrum editor; CalendarView. Loose ends now: the
DataGrid selection/AutomationPeers/header-menu/virtualization cluster.

Tenth Pickers milestone. `ColorPicker` gains `Validation` (`Func<AvaColor,string?>`) + public `Validate()`,
mirroring the 3.17 pattern but WITHOUT `Required` (Value is non-null). Self-gating (Validation null → return
ErrorText, no touch). Wired: `OnPropertyChanged` calls Validate on ValueProperty/ValidationProperty change;
editable `CommitText` and the palette `SelectColor` call Validate explicitly (same-value coverage). Editable
hex parse error still precedes (CommitText returns before setting Value on invalid). Completes validation
across all four field pickers (Date/Time/Range have Required+Validation; Color has Validation).

**Verified:** build 0/0 (solution); full suite **572** (+5). Tests: Validation sets/clears on value change;
self-gating preserves manual error; editable commit runs business validation; editable parse error precedes
validation; palette select (black swatch via keyboard Enter) runs validation. Gallery: "Validation" ColorPicker
sample (rejects near-black). Docs: ColorPicker Validation/Validate() rows + intro note updated; changelog 3.18.0.
Review: focused single-agent adversarial pass (close mirror of reviewed-clean 3.17) — confirmed no re-entrancy/
precedence/self-gating/ordering bugs; its one flagged gap (palette-select validation untested) was closed with
a keyboard-driven test.

**Next:** HSV spectrum editor for ColorPicker (visual plane); CalendarView state machine. Loose end:
Avalonia 12 clipboard API + DataGrid copy.

---

## 2026-06-14 — 3.17 — Field-picker validation hooks (Required + Validation)

Ninth Pickers milestone. `DatePicker`, `TimePicker`, `DateRangePicker` each gain `Required` (bool) +
`RequiredText` (default "Required") + `Validation` (Func returning an error message; signatures: DatePicker
`Func<DateTime?,string?>`, TimePicker `Func<TimeSpan?,string?>`, DateRangePicker `Func<DateTime?,DateTime?,string?>`)
+ a public `Validate()`, mirroring `TextField.Validate()`. **Self-gating:** if neither Required nor Validation
is set, `Validate()` returns the current ErrorText WITHOUT touching Error/ErrorText (so manual error state is
preserved) — a deliberate improvement over TextField (which relies on the caller gating). Wired centrally:
`OnPropertyChanged` calls `Validate()` when the value OR Required/Validation/RequiredText change (covers
programmatic, flyout OK, Clear), and the editable `CommitText` + flyout OK call `Validate()` explicitly too
(covers same-value commits where OnPropertyChanged doesn't fire). Editable parse/range error still takes
precedence (CommitText returns before setting the value on parse failure, so Validate isn't reached).

**Adversarial review (workflow, 3 dims × verify):** validation-correctness and parity dims came back CLEAN
(no loops, self-gating, precedence, same-value, init-ordering, three-way consistency all verified). 6 confirmed
test/doc parity gaps closed: Time+Range self-gating (preserve-manual-error) tests; Time+Range editable
parse-error-precedence tests; DatePicker same-value re-commit re-runs Validate; Time+Range flyout-OK runs
Validate; and the Required/RequiredText/Validation/Validate() rows added to the Time+Range doc tables. No
production bugs.

**Verified:** build 0/0 (solution); full suite **567** (+14). Tests: Required flags missing value + custom
RequiredText + clears when set; Required error returns after Clear(); Validation func sets/clears; self-gating
preserves manual error; editable commit of a parseable-but-business-invalid date shows the validation error;
TimePicker + DateRangePicker Required+Validation combined (range validation uses start/end span). Gallery:
"Required & validation" DatePicker sample (Required + weekday-only). Docs: shared validation intro note +
DatePicker table rows (note states Time/Range share the same members); changelog 3.17.0. ColorPicker omitted
(non-null Value → Required N/A); could add a Validation-only overload later.

**Next:** ColorPicker Validation (Func<Color,string?>); HSV spectrum editor; CalendarView state machine.
Loose end: Avalonia 12 clipboard API + DataGrid copy.

---

## 2026-06-14 — 3.16 — ColorPicker editable hex entry (editable across all 4 field pickers)

Eighth Pickers milestone. `ColorPicker` gains `Editable` + `InvalidHexText` and a public static
`TryParseColor(text, out AvaColor)` (wraps `AvaColor.TryParse`; empty/whitespace → false). Theme: box
restructured from a horizontal StackPanel to a DockPanel (swatch Dock.Left + textLayer Grid holding PART_Hex +
new PART_Input TextBox). The control mirrors the editable pattern with carried fixes, adapted to ColorPicker's
specifics: Value is NON-NULL (no empty state) → empty text reverts to the current value with no error (not a
clear). No trailing icon exists, so the **swatch is the palette opener** in editable mode (PointerPressed →
Open, "Open color palette" automation name) — and since the swatch isn't focusable it doesn't blur the input
(no premature commit); Alt+Down also opens. CommitText parses, honors ShowAlpha (forces opaque when alpha not
shown), sets Value (ValueChanged auto-raised via OnPropertyChanged) + reformats; invalid → Error +
InvalidHexText. UpdateEditMode hides PART_Hex + shows PART_Input. IsActive includes input focus; IBeam cursor;
OnKeyDown activation guarded by !Editable; box PointerPressed focuses input when Editable.

**Adversarial review (workflow, 3 dims × verify):** the correctness dim came back CLEAN (parse, ShowAlpha
forcing, empty-reverts, ValueChanged semantics, swatch-opener interplay all sound). One production finding
fixed: the floating label wasn't inset past the leading 30px swatch (pre-existing, now user-facing with the
editable textbox) → `UpdateLabel` now passes a `SwatchLeadingInset (30)` to `ApplyLabelLayout`, matching the
DatePicker leading-icon pattern. Closed test gaps: ShowAlpha=false forces typed #AARRGGBB opaque; lowercase/
same-value commit still reformats to upper-case (exercises the explicit UpdateDisplay-after-set); empty revert
raises no ValueChanged; non-editable Space still opens. Skipped nits: swatch pointer-open (automation-name +
Alt+Down adequate; synthetic PointerPressed too fragile), minor doc clarifications.

**Verified:** build 0/0 (solution); full suite **553** (+11). Gallery: "Editable hex entry" sample. Docs:
ColorPicker note + Editable/InvalidHexText/TryParseColor rows; changelog 3.16.0.

**Next:** CalendarView state machine; HSV spectrum editor for ColorPicker (visual plane); per-picker
validation hooks (Required/Validation). Loose end: Avalonia 12 clipboard API + DataGrid copy.

---

## 2026-06-14 — 3.15 — DateRangePicker editable text entry (completes the editable trio)

Seventh Pickers milestone. `DateRangePicker` gains `Editable` + `InvalidRangeText` and a public static
`TryParseRange(text, format, out start, out end)`: empty→(null,null); single date→start only; two dates split
on `RangeSeparators` ("–", " to ", " - ") via `SplitRange`, each half parsed by **reusing
`DatePicker.TryParseDate`**, then auto-ordered. Theme: `PART_Input` TextBox + calendar `Icon`→`IconButton`
`PART_CalendarButton` ("Open calendar"). Control mirrors Date/Time editable exactly with all carried fixes
(same-value reformat via explicit UpdateDisplay, `_flyoutOpening` tunnel guard + CommitText skips when
flyout open/opening, flyout OK clears Error + reformats, input automation name, IBeam cursor, IsActive
includes `_input.IsFocused`, OnKeyDown activation guarded by `!Editable`, box PointerPressed focuses input).
CommitText validates both endpoints via `IsOutOfRange` (compares `.Date` vs Min/Max).

**Adversarial review (workflow, 3 dims × verify):** range-parse and parity dims came back CLEAN (separator scan,
single-date, auto-order, empty-half, and all carried-over fixes verified). 5 confirmed minor test-coverage gaps
closed: " - " separator case, single-date round-trip reformat assertion, non-editable activation opens flyout,
editable+ShowPresets coexistence (typed range commits + rail renders). No production bugs.

**Verified:** build 0/0 (solution); full suite **542** (+12). Metrics test simplified: all three field pickers
now expose an IconButton calendar/clock affordance ("Open calendar"/"Open clock"). Tests: TryParseRange table
(empty/single/pair/" to "/reversed-auto-order/invalid); typed range Enter commits + RangeSelected; single date
= start only; invalid→Error + ErrorText==InvalidRangeText unchanged; out-of-range rejected; empty clears +
sync; "to"+reversed reformats to ordered en-dash; calendar button opens + OK syncs; Alt+Down; non-editable
hides input. Gallery: "Editable text entry" sample. Docs: pickers.md note generalized to all three +
Editable/InvalidRangeText/TryParseRange rows; changelog 3.15.0.

**Next:** CalendarView state machine; ColorPicker HSV editor; per-picker validation hooks. Loose end:
Avalonia 12 clipboard API + DataGrid copy.

---

## 2026-06-14 — 3.14 — TimePicker editable text entry

Sixth Pickers milestone. Extended the proven DatePicker 3.13 editable pattern to `TimePicker`: `Editable` +
`InvalidTimeText` StyledProperties; public static `TryParseTime(text, format, out TimeSpan?)` (empty→valid
null; TryParseExact(format).TimeOfDay → TryParse(culture).TimeOfDay → TimeSpan.TryParse; else false). Theme:
`PART_Input` TextBox added to textLayer; Schedule `Icon` → `IconButton` `PART_ClockButton` ("Open clock").
Control mirrors DatePicker exactly, carrying ALL the 3.13 review fixes from the start: same-value reformat
(explicit UpdateDisplay after Time=parsed), clock-button commit guard (`_flyoutOpening` set on tunnel
PointerPressed + CommitText skips when `_flyoutOpen || _flyoutOpening`, Click is Open-only), flyout OK clears
Error + reformats, `_input` automation name, IBeam cursor, IsActive includes `_input.IsFocused`, OnKeyDown
activation guarded by `!Editable`, box PointerPressed focuses `_input` when Editable. No Min/Max (time has no
range) → invalid = unparseable only; no IsOutOfRange.

**Verified:** build 0/0 (solution); full suite **530**. Tests: TryParseTime table; typed time on Enter commits +
TimeSelected; invalid→Error + ErrorText==InvalidTimeText, Time unchanged; empty clears + sync from Time; loose
"9:05 AM"→"09:05" reformat; clock button opens + OK syncs input; Alt+Down opens; label floats on focus;
non-editable hides input. Gallery: "Editable text entry" sample. Docs: pickers.md note generalized to Date+Time
+ Editable/InvalidTimeText/TryParseTime rows; changelog 3.14.0.

**Adversarial review (workflow, 3 dims × verify) — caught a CRITICAL bug:** `TimeSpan.TryParse` fallback in
TryParseTime accepted bare numbers as DAYS ("5"→5 days) and spans ≥24h, silently corrupting Time (display
masked it via DateTime.Today.Add + time-of-day formatting). Fixed: the TimeSpan branch is now guarded to
`span >= TimeSpan.Zero && span < TimeSpan.FromDays(1)` (rejects "5", "25:00", negatives). +2 tests (TryParseTime
rejects "5"/"25:00"; editable rejects bare "5", Time unchanged). Minor (documented, not changed): typed entry
accepts any minute, not snapped to MinuteStep — intentional asymmetry, noted on the Editable XML summary.
Parity dim confirmed all 9 carried-over DatePicker fixes present; other findings were verified-correct nits.

**Next:** DateRangePicker editable (two-date parsing — trickier); CalendarView state machine; ColorPicker HSV
editor. Loose end: Avalonia 12 clipboard API + DataGrid copy.

---

## 2026-06-14 — 3.13 — DatePicker editable text entry

Fifth Pickers milestone (XL). `DatePicker` gains `Editable` (opt-in) + `InvalidDateText` (default "Invalid
date") StyledProperties and a public static `TryParseDate(text, format, out DateTime?)` (empty→valid null;
TryParseExact(format) then TryParse(culture); else false). Theme: added a chrome-stripped `PART_Input`
TextBox in the textLayer Grid (hidden unless Editable) and converted the trailing calendar `Icon` to an
`IconButton` `PART_CalendarButton` (Size Small) so it can open the flyout when box-click now focuses the
text. Control: finds the new parts; `FieldChrome.ResetInnerTextBox(_input)` (re-applied on focus per house
pattern); `_input` KeyDown Enter→`CommitText`, Alt+Down→`Open`; LostFocus→`CommitText`; box PointerPressed
focuses `_input` when Editable (else Focus+Open as before); `_calendarButton.Click`→Focus+Open ("Open
calendar" automation name). `CommitText` parses, validates `IsOutOfRange` (compares `.Date` vs Min/Max),
sets Error+InvalidDateText on failure (keeps text), else Error=false + Date + DateSelected. `UpdateDisplay`
syncs `_input.Text` from Date; `UpdateLabel` hides `_display` when Editable, sets `_input.PlaceholderText`
(not obsolete Watermark) only when label not resting, and floats the label on typed text; `IsActive`
includes `_input.IsFocused`; cursor is IBeam in Editable. OnKeyDown activation guarded by `!Editable`.

**Decisions:** DatePicker-only this milestone (Time/Range later). Calendar→IconButton is DatePicker-scoped;
updated the shared metrics test to assert the DatePicker calendar as an IconButton ("Open calendar") while
Time/Range keep the plain Icon@12px. Parse keeps user text on failure (no clobber).

**Verified:** build 0/0 (solution); full suite **520** (+12 editable tests). Fix: TextBox.Watermark obsolete →
PlaceholderText. Gallery: "Editable text entry" sample (free + constrained). Docs: pickers.md intro note +
Editable/InvalidDateText/TryParseDate rows; changelog 3.13.0.

**Adversarial review (workflow, 3 dims × verify) — found REAL bugs (first milestone with production fixes):**
13 confirmed. Fixed: (1) same-value commit didn't reformat (Avalonia SetValue no-ops when unchanged so
OnPropertyChanged→UpdateDisplay never ran) → CommitText now calls UpdateDisplay() explicitly after Date=parsed;
(2) clicking the calendar button blurred the input and auto-committed/errored partial text → calendar Click is
now Open()-only plus a `_flyoutOpening` guard set on the button's tunnel PointerPressed (and CommitText skips
when `_flyoutOpen || _flyoutOpening`); (3) flyout OK now clears Error (stale typed-error survived picking a
date); (4) a11y — `_input` now gets AutomationProperties.SetName (Label/Placeholder/"Date"). Rejected the
reviewer's IsHitTestVisible=false on PART_Input (would break editable clicking; non-editable is already inert
via IsVisible=false). Test gaps closed: loose-parse reformats to DateFormat ("July 4, 2026"); flyout selection
updates the text box; Alt+Down opens; label floats on focus; non-editable activation still opens; ErrorText ==
InvalidDateText on both error paths.

**Next:** extend editable entry to TimePicker/DateRangePicker; CalendarView state machine; ColorPicker HSV
editor. Loose end: Avalonia 12 clipboard API + DataGrid copy.

---

## 2026-06-14 — 3.12 — Field-picker leading adornment icons

Fourth Pickers milestone. `DatePicker`, `TimePicker`, `DateRangePicker` gain `AdornmentIcon`
(StyledProperty<string?>, a glyph from `Loam.Icons`). Each `*Theme.cs` adds a `PART_Adornment` `Icon`
(Small, `IsVisible=false`) docked Left at the start of the box DockPanel `{ icon, clear, adornment,
textLayer }` — collapsed when unset, so zero layout impact and the no-adornment path is unchanged. Each
control finds `PART_Adornment`, `UpdateAdornment()` sets `Data` + visibility from `AdornmentIcon`, and
`OnPropertyChanged(AdornmentIconProperty)` re-runs it + `UpdateLabel()`.

**Layout insight (the crux):** the value/resting label sit INSIDE the box DockPanel, so a Left-docked icon
shifts them automatically. But the floating label is a SIBLING overlay positioned by an absolute
`metrics.LabelX` margin — it does NOT move with box content. Fix: `FieldChrome.ApplyLabelLayout` gained an
optional `leadingInset` (default 0, so other inputs unaffected) added to the label's X; pickers pass
`FieldChrome.LeadingAdornmentInset(this)` = `Icon.PixelSize(Small) + IconSpacing` = 20 + 8 = **28px** when
an adornment is set. New public `FieldChrome.LeadingAdornmentInset(Control)` helper.

**Decisions:** ColorPicker excluded (already has a leading swatch). Scope = the three field pickers only;
no change to text inputs.

**Verified:** build 0/0 (solution); full suite **508**. Gallery: "Leading icon"
sample on all three pages (Person/Edit, Notifications, Favorite). Docs: pickers.md intro note + AdornmentIcon
rows; changelog 3.12.0.

**Adversarial review (workflow, 3 dims × verify):** correctness-layout and regression-api dims came back CLEAN
(inset math, DockPanel order, zero-footprint-when-unset, optional-param non-impact on other ApplyLabelLayout
callers all verified). 6 confirmed test-coverage gaps closed — the indent test now (a) runs across Outlined/
Filled/Text variants and (b) asserts the value-text region itself shifts 28px (via PART_Display's parent
`Bounds.X`, since `TranslatePoint` isn't on `Text` in Avalonia 12), plus new tests for runtime set→unset
(reverts to zero-space) and clear-button + adornment coexistence (also exercises the gallery's Person glyph).
Dismissed: docs-accuracy and 28px-delta-brittleness (both already correct).

**Next:** Pickers — CalendarView state machine / editable entry / validation; ColorPicker HSV editor; or
the adornment-on-text-inputs follow-up. Loose end: Avalonia 12 clipboard API + DataGrid copy.

---

## 2026-06-14 — 3.11 — TimePicker auto-scroll to selected time

Third Pickers milestone. When the TimePicker flyout opens, the hour and minute `ScrollViewer` columns
now center the selected (or closest, per MinuteStep) value instead of starting at the top. `Open()`
captures both column ScrollViewers (`_hourScroll`/`_minuteScroll` via a capture callback on `BuildColumn`),
and `ScheduleScrollToSelection` centers the target row: if already laid out it centers immediately,
otherwise it hooks a one-shot `EffectiveViewportChanged` and centers once `Viewport`/`Bounds` are valid
(offset = rowCenter − viewport/2, clamped to [0, extent−viewport]). Row `GotFocus` now calls
`BringIntoView()` so keyboard navigation keeps the active row visible.

**Root-cause fix (the enabler):** the columns used `VerticalScrollBarVisibility = Disabled`, which in
Avalonia pins content to the viewport — i.e. the columns were never actually scrollable (Extent == Viewport,
offset always clamped to 0; rows past ~3.6 were clipped/unreachable). Changed to `Hidden` (scrollable, no
visible scrollbar). This is the real correctness win; the auto-centering rides on top.

**Verified:** build 0/0 (solution); full suite **503** (+6 TimePicker tests). Updated 1 pre-existing test that
asserted the old `Disabled` value → `Hidden`. Headless gotcha confirmed: with `Disabled` the
offset/`BringIntoView` were no-ops (Extent==Viewport); `Hidden` made them work in headless too. Gallery:
"Opens at selected time" sample (22:55 + 18:45 quarter-hour). Docs: pickers.md TimePicker behavioral note;
changelog 3.11.0.

**Adversarial review (workflow, 3 dims × verify):** correctness-timing and regression dims came back CLEAN
(centering math, Disabled→Hidden, static handler with no `this`-capture all verified sound). 3 confirmed
test-quality gaps fixed: (a) tests only checked `Offset.Y>0` → added `ShouldBeWithinViewport`/`ShouldBeCenteredIn`
(mid-list 12:30 lands centered ±3px; clamped 22:55 stays visible) + `ShouldOverlapViewport` for the
BringIntoView keyboard case; (b) `TimeColumns` index helper was child-order-fragile → replaced with
`TimeColumn(paper, heading)` matching the column's sibling heading; (c) added null-`Time` default test and a
MinuteStep-rounding test (18:50 step 15 → row 45 visible). Dismissed: offset-flakiness (math deterministic;
22:55 gives large offsets) and a claimed MinuteStep "out-of-bounds" bug (code correct — value always present).

**Next:** Pickers — leading adornment icons across field pickers; then CalendarView state machine / editable
entry / validation; ColorPicker HSV editor. Loose end: Avalonia 12 clipboard API + DataGrid copy.

---

## 2026-06-14 — 3.10 — DateRangePicker quick-select preset rail

Second Pickers milestone. `DateRangePicker` gains `ShowPresets` (StyledProperty<bool>, opt-in), a mutable
`Presets` (`AvaloniaList<DateRangePreset>`, mirrors ColorPicker.Palette house style), and a static
`DefaultPresets` (`IReadOnlyList<DateRangePreset>`: Today, Yesterday, Last 7 days, Last 30 days, This
month, Last month, This year). New public type `DateRangePreset(string Label, Func<DateTime,(DateTime
Start,DateTime End)> Resolve)` in its own file. In `Open()`, when `ShowPresets`, a left-docked rail
(`BuildPresetRail`) of Text-variant buttons sits beside the calendar inside a DockPanel; the flyout paper
widens by `PresetRailWidth (168) + 16`. A preset click resolves against `DateTime.Today`, auto-orders,
clamps to `MinDate`/`MaxDate` (no-op if fully out of range), sets the **pending** range, moves
`calendar.DisplayMonth`, and calls `SyncPendingDisplay()` — OK still commits. Presets read at Open() time
(flyout rebuilt per open), so no CollectionChanged plumbing needed.

**Decisions:** presets stage pending (not instant-commit) to compose with the two-step OK/Cancel model;
`Presets` non-empty replaces `DefaultPresets` (not merge); `ShowPresets` is the single visibility gate.
`DefaultPresets` lambdas are `static` (capture nothing) to satisfy analyzers; the apply-closure captures
Open()'s pending locals.

**Verified:** build 0/0 (solution); full suite **497** (+8 picker tests). Initial 5: pure DefaultPresets.Resolve
table for a fixed anchor; rail stages pending then OK commits Last-7-days = 6-day span; rail absent without
ShowPresets; custom Presets replace defaults; preset clamps to MinDate. Gallery: "Quick-select presets"
sample (defaults + custom) on the DateRangePicker page. Docs: pickers.md table rows + "Quick-select presets"
subsection; changelog 3.10.0. Fix: CA1859 on a test helper (List<string?> not IReadOnlyList).

**Adversarial review (workflow, 4 dims × verify) → 6 confirmed fixes applied:** (1) title MaxWidth was pinned
to PickerWidth(360) so it stayed narrow on the 544-wide preset paper → `PopupSurface.PickerContent` now takes
an optional `width` and DateRangePicker passes `paperWidth`; (2) rail could overflow for many custom presets →
`BuildPresetRail` now returns a `ScrollViewer` (Width=168, MaxHeight=360, Auto vertical scrollbar) so 7 defaults
fit but long lists scroll; (3) redundant `AutomationProperties.SetName` on text buttons removed (Content is the
accessible name; house pattern reserves SetName for icon-only buttons); (4) +test preset buttons keyboard-focusable;
(5) +test MaxDate clamp (custom far-future preset clamps end down to MaxDate); (6) +test fully-out-of-bounds preset
stages nothing (calendar RangeStart/End stay null). Two findings dismissed on verification: record-vs-class (value
vs reference-type pattern) and "missing divider" (4px margin spacing is adequate).

**Next:** Pickers — TimePicker auto-scroll to selected; adornment (leading) icons; then CalendarView
state machine / editable entry / validation. Loose end: Avalonia 12 clipboard API + DataGrid copy.

---

## 2026-06-14 — 3.9 — Pickers clearable fields (inline × affordance)

Opened the **Pickers track**. `DatePicker`, `TimePicker`, and `DateRangePicker` gain an opt-in
`Clearable` StyledProperty. Each `*Theme.cs` adds a `PART_Clear` `IconButton` (Close glyph, Small,
docked Right before the calendar/clock icon inside the field's DockPanel `{ icon, clear, textLayer }`,
hidden by default). In `OnApplyTemplate` the control finds `PART_Clear`, sets its automation name
("Clear date"/"Clear time"/"Clear dates"), and wires `Click` to `Clear()` + the change event with null.
`UpdateClearButton()` toggles visibility on `Clearable && hasValue`, re-evaluated in `OnPropertyChanged`
for the value + `Clearable` properties. Because `IconButton : Button` consumes the pointer, clicking the
clear button does **not** bubble to `PART_Box` and open the flyout.

**Decisions:** surface the existing `Clear()` API rather than add new clearing logic; off by default
(additive, zero behavior change for existing usage). ColorPicker deferred (no nullable value — `Value`
is a non-null `Color`).

**Verified:** build 0/0 (solution); full suite **489 passing** (+3: DatePicker clear resets value & does
not open the flyout + hides once empty; clear hidden without Clearable or value; TimePicker +
DateRangePicker clear reset). Gallery: "Clearable" sample added to all three picker pages. Docs:
pickers.md intro note + `Clearable` rows; changelog 3.9.0.

**Next:** Pickers quick wins — TimePicker auto-scroll to selected, adornment icons, date-range preset
rail; then CalendarView state machine, editable entry, validation. Loose end: Avalonia 12 clipboard API
+ DataGrid copy.

---

## 2026-06-14 — 3.8 — DataGrid footer aggregates (totals row)

`DataGridColumn<T>` gains `Summary` (Func<IReadOnlyList<T>,string>?) and `SummaryKind` enum
(Sum/Average/Min/Max/Count, honoring Format) + `SummaryText(rows)` (numeric via an `AsNumber` switch).
`DataGrid<T>.ShowFooter` computes footer values over the filtered `sorted` set in Rebuild and passes a
`string?[]` to the grid builders; both `BuildSingleGrid` and `BuildFrozenGrid` (left + right panes) render
a footer row (`BuildFooterCell`: Subtitle2, column-aligned, top-border separator) at `rowIndex` when there
are data rows. Footer skipped in empty/loading/error states.

**Verified:** build 0/0; full suite **486 passing** (+2: column SummaryText aggregates, grid footer shows
"2 people"/"60"). Visually confirmed offscreen: "4 regions / 435 / $35,500" totals row, right-aligned,
currency-formatted, separated by a top border. Gallery: DataGrid "Footer totals" sample.

**Next:** sticky header + footer pinning; then the selection / AutomationPeers / header-menu /
virtualization cluster. Loose end: Avalonia 12 clipboard API + copy.

---

## 2026-06-14 — 3.7 — DataGrid async states + Pagination polish

**Async states:** `DataGrid<T>` gains `IsLoading` (skeleton body via `Loam.Controls.Skeleton` rows),
`ErrorText`/`ErrorContent` (+ `OnRetry` → Retry button), `SkeletonRowCount`. Precedence
Error>Loading>Empty>data, implemented by forcing an empty body (so the existing column-spanning empty
cell renders) and branching `BuildEmptyRow` on state; pagination hidden during states. Reuses the
empty-state plumbing — minimal change.

**Pagination polish:** `ShowFirstLast` (first/last `IconButton`s, new `Icons.Material.Filled.FirstPage`/
`LastPage` glyphs added to core), `ShowRange` + `PageSize`/`TotalItems` → "Showing X–Y of N". DataGrid's
internal pager enables both (TotalItems = filtered+sorted count).

**Verified:** build 0/0; full suite **484 passing** (+3: loading skeleton hides rows, error+Retry,
pagination first/last+range). Gallery: DataGrid "Async states" sample (Loading/Error/Ready buttons);
existing paged sample now shows the enhanced pager.

**Next (3.8+):** footer aggregates + sticky header; then multi-select → AutomationPeers → header-menu →
virtualization. Also: verify the Avalonia 12 clipboard API and add copy.

---

## 2026-06-14 — 3.6 — DataGrid liveness & egress (live binding + CSV/TSV export)

Starts the Data track (value-first "liveness & egress" milestone).

**Live binding:** `DataGrid<T>.Items` now observes its source: if it implements
`INotifyCollectionChanged` (e.g. `ObservableCollection<T>`), add/remove/reset refresh the grid without
reassigning Items. Opt-in `ObserveItemChanges` watches per-row `INotifyPropertyChanged`; `Refresh()` for
non-observable sources. Subscriptions managed in the Items setter + `OnAttachedToVisualTree`, torn down
in `OnDetachedFromVisualTree` (DataGrid is a `Decorator`). Non-observable sources behave exactly as
before. Gallery: DataGrid "Live data" sample (ObservableCollection + Add/Remove buttons).

**Export:** pure `DataGrids.ToDelimited<T>(rows, columns, separator)` (header + per-column `Display`,
RFC-4180 quoting) + instance `ExportCsv()`/`ExportTsv()` over the current filtered+sorted view (all
pages). **Clipboard copy deferred:** Avalonia 12's `IClipboard` has no `SetTextAsync` (compile error) —
needs API verification before adding `CopyToClipboardAsync`.

**Verified:** build 0/0; full suite **481 passing** (+4: collection-change refresh, row-property-change
refresh, RFC-4180 quoting, export covers current view).

**Next (3.7):** pagination first/last + page-size + range; footer aggregates; async loading/error states;
then the selection/a11y/header-tooling cluster.

---

## 2026-06-14 — 3.5 (sub-slice 2) — Chart-bound legend

`ChartLegend.Source : ChartBase?` auto-derives rows from the chart — one per series (name + per-series
color) for multi-series, else one per category — and refreshes on the chart's new internal
`SnapshotChanged` event (raised in `RebuildPoints`). Backed by `ChartBase.GetLegendEntries()` (internal
virtual; `CartesianChartBase` overrides for series). Subscription managed across attach/detach; manual
`Labels`/`Colors` still work when `Source` is null. Gallery multi-series samples now use `Source`.
Verified: build 0/0, full suite **477 passing** (+1: legend rows/colors/labels derived from series).
Deferred (documented): interactive series toggle + hover-highlight sync, and time-series X mapping.

---

## 2026-06-14 — 3.5 (sub-slice 1) — Charts multi-series (grouped/stacked/percent bars, multi-line)

**Done**
- `ChartSeries(IReadOnlyList<double> Values, string? Name, Color? Color)` record; `BarStackMode` enum;
  `ChartPoint.SeriesIndex` (non-positional init prop, default 0 — non-breaking).
- `CartesianChartBase.Series` (overrides Values when set). Snapshot generalized via a virtual
  `ChartBase.BuildPoints()` override that emits flat **series-major** points (s*categories + c) with
  per-series color and category labels. `Charts.StackedBarHeights` pure helper.
- BarChart `RenderSeries`: grouped (sub-bars per category), Stacked, StackedPercent (per-category
  normalize to 100). LineChart `RenderSeries`: one polyline+dots per series. Both reserve axis gutters,
  span the domain across all series, and feed `_barRects`/`_pointPositions` flat indices so hit-testing,
  tooltips, and hover emphasis generalize automatically.
- **Single-series path untouched** (Series==null → existing render + base.BuildPoints). Gallery: BarChart
  "Grouped series"/"Stacked series", LineChart "Multiple series". Docs: charts.md multi-series section.

**Decisions / limits**
- Multi-series colors are per-series (vs single-series per-category) via the snapshot; drawing reads
  `ResolvedPoints[flat].Color`. Multi-series data labels and signed/negative multi-series bars are not
  drawn this release (documented).

**Verified:** build 0/0; full suite **476 passing** (+4: StackedBarHeights, multi-series snapshot/
SeriesIndex/color, render-without-throw across all 4 modes, grouped-bar hover). Visually confirmed
offscreen: grouped, stacked, 100%-stacked bars, and multi-line — all with axes and per-series colors.

**Next:** 3.5 sub-slice 2 — chart-bound interactive legend (`ChartLegend.Source`).

---

## 2026-06-14 — 3.4 (sub-slice 2) — Charts ItemsSource binding

`ChartBase` gains `ItemsSource` + `ValueSelector`/`LabelSelector`/`ColorSelector`. When set, items project
into Values/Labels/Colors and an `INotifyCollectionChanged` source refreshes live (subscribe in setter +
`OnAttached`, unsubscribe in `OnDetached`; re-project in `RefreshVisuals` so color nulls resolve to fresh
theme colors). ColorSelector null → theme series color for that point. Gallery: BarChart "Bound data"
sample (ObservableCollection + Add-point button). Verified: build 0/0, full suite **472 passing** (+1
live-update test). Next: sub-slice 3 — multi-series/stacked (XL).

---

## 2026-06-14 — 3.4 (sub-slice 1) — Charts axes + nice-number scaling

First of four 3.4 sub-slices (axes → ItemsSource → multi-series → bound legend).

**Done**
- `Charts.NiceScale(min,max,ticks)` + zero-based overload (pure, 1/2/5×10ⁿ rounding via NiceNum); new
  Rect-based `Charts.ScaledLinePoints(values, Rect, min, max)` overload.
- New `CartesianChartBase : ChartBase` (shared by Bar/Line): `ShowAxes` (default false), `Min`/`Max`,
  `YAxisTickCount` (4), `YAxisFormat`; `ResolveDomain` (applies overrides + NiceScale), `MeasureYGutter`/
  `MeasureXGutter`, `DrawYAxis` (ticks+labels+gridlines), `DrawXAxisLabels` (category labels, thinned).
- **Unified Bar/Line rendering** over a value-axis domain: both now scale via `SignedBarLayout`/
  `ScaledLinePoints` over `[min,max]` (the signed helpers generalize — `(0,max)` reproduces the old
  BarHeights/LinePoints output exactly), reserve left/bottom gutters when `ShowAxes`, draw axes vs the
  4-line grid. Zero baseline drawn only when `min < 0`. BarChart's two branches collapsed into one.
- Gallery: Bar/Line "Axes" samples. Docs: charts.md axes table + example, changelog 3.4.0.

**Decisions**
- Axes default OFF to stay additive (existing samples/tests unchanged); the unification is behavior-
  preserving for the non-axis path (verified: full suite unchanged at 468 before new tests).

**Verified:** Release build 0/0; full suite **471 passing** (468 + 3: NiceScale rounding, Rect line-point
mapping, axes render-without-throw). Visually confirmed offscreen: bar with $0k–$80k Y-axis + Q1–Q5
X-axis; line with 0–80 Y-axis + Jan–Jun X-axis, both correctly scaled.

**Next:** 3.4 sub-slice 2 — `ItemsSource` binding (value/label/color selectors + INotifyCollectionChanged).

---

## 2026-06-14 — 3.3 — Charts interactivity: hit-testing + hover tooltips

**Done**
- `ChartBase`: `protected abstract int HitTest(Point local)`; pointer overrides (Moved/Exited/Pressed)
  maintaining `HoveredIndex` (invalidate only on change; reposition tooltip on same-element move);
  `HoverChanged`/`PointClicked` events with new `ChartPointEventArgs(int Index, ChartPoint? Point)`.
- Per-chart HitTest using geometry recorded during Render: PieChart by angle+radius against slice
  ranges (records `_center`/`_radius`/`_holeRadius`/`_sliceRanges`); BarChart by bar rect (`_barRects`,
  both signed and non-signed branches); LineChart by nearest point within 16px (`_pointPositions`).
- Hover emphasis: a 2px Surface outline on the hovered slice/bar, a larger outlined dot on the hovered
  line point.
- Tooltips: self-drawn rounded box (EmptySurface fill + Outline stroke + Text), pointer-following,
  edge-flipped/clamped into bounds. `ShowTooltip` (default true) + `TooltipFormat`; `DefaultTooltip`
  shows label+value (PieChart override adds the percentage). Drawn last in each chart's Render.
- Gallery: BarChart "Interactive" sample wiring HoverChanged/PointClicked to a caption. Docs: charts.md
  interactivity table + example, changelog 3.3.0.

**Decisions**
- Geometry recorded during `Render` (UI thread) and read in pointer handlers (UI thread) — no threading
  issue with Avalonia's record-then-replay rendering. Changed `_points` field to `ChartPoint[]` (CA1859);
  use `.Length`.

**Verified:** Release build 0/0; full suite **468 passing** (+2: headless pointer test confirms hover
hit-tests bar index 1 and `PointClicked` fires; tooltips render without throwing). Visually confirmed via
headless `CaptureRenderedFrame`: "Thu: 22" bar tooltip and "Alpha: 45 (45%)" donut-slice tooltip, both
with the hovered element emphasized.

**Next:** 3.4 — analytical depth (axes, ItemsSource binding, multi-series/stacked, bound legend).

---

## 2026-06-14 — 3.2 — Charts enrichment: on-chart data labels (completes 3.2)

**Done**
- `ChartBase.ShowDataLabels` (bool) + `DataLabelFormat` (Func<ChartPoint,string>?). Shared helpers:
  `ResolveDataLabel`, `DefaultDataLabel` (virtual; value via `0.##`), `DataLabelText` (tokenized
  FormattedText), `ContrastBrush` (luminance-based near-black/white for on-fill text).
- PieChart: overrides `DefaultDataLabel` → percentage; draws labels at slice centroids (skips slices
  < 16°), contrast-colored on the slice fill.
- BarChart: value above positive bars / below negative bars (signed branch too); LineChart: value above
  each point. Both thin left-to-right (skip when a label would overlap the previous) and clamp x into the
  chart bounds so edge labels aren't clipped.
- Gallery: +3 samples (PieChart "Slice percentages", Bar/Line "Data labels"). Docs: charts.md shared
  table + example, changelog. 1 new test (render-without-throw across all three + signed).

**Verified:** Release build 0/0; full suite **466 passing**. Visually confirmed via the offscreen Skia
harness: bar values above bars, donut slice % at centroids (white-on-fill), line values above points
with the first/last no longer clipping after the x-clamp.

**Next:** 3.3 — chart hit-testing + hover tooltips (the snapshot + this label infra feed it).

---

## 2026-06-14 — 3.2 — Charts enrichment, first slice (snapshot + donut center text + signed values)

First slice of the value/demand-first enrichment roadmap ([satellite-enrichment-roadmap.md](../plans/satellite-enrichment-roadmap.md)).
All additive in `src/Loam.Charts/Charts.cs`; no breaking changes.

**Done**
- **Per-point snapshot:** `public readonly record struct ChartPoint(int Index, double Value, double Percent,
  string? Label, Color Color)`; `ChartBase.Labels` + `protected internal ResolvedPoints`, rebuilt on
  Values/Colors/Labels/visuals change. `UpdateAutomation` now appends positive-point labels when present
  (kept the bare "{n} value(s)"/"No data" format when no labels — existing tests rely on it).
- **Donut center text:** `PieChart.CenterText`/`CenterSubText`/`CenterValue`/`CenterValueFormat` drawn in
  the hole with `FormattedText` (MaxTextWidth + ellipsis trimming), only when `Donut`.
- **Signed values:** opt-in `AllowNegative` on `BarChart`/`LineChart` (default false → unchanged clamping).
  New pure helpers in static `Charts`: `SignedDomain`, `ZeroBaselineOffset` (public), `SignedBarLayout`
  (public), `ScaledLinePoints` (internal). Bars grow up/down from a drawn zero baseline; signed lines plot
  around it and Area fills to it.
- Gallery: +3 samples (PieChart "Donut with center total", Bar/Line "Signed values"). Docs: charts.md
  property tables + examples, changelog 3.2.0 entry. 7 new xUnit/headless tests in ChartTests.cs.

**Decisions**
- Kept default clamping behavior (AllowNegative defaults false) so existing all-positive callers and the
  `Charts_math_clamps_negative_values` test are unaffected — signed math lives in *new* helpers, not in
  `BarHeights`/`SliceSweeps`/`LinePoints`.
- Hoisted inline `string[]` label literals to locals to satisfy CA1861 under TreatWarningsAsErrors.

**Verified:** Release build 0/0; full suite **465 passing** (458 + 7). Visually confirmed: signed
LineChart in the live gallery (PrintWindow capture); donut center text ("1,240 / sessions", centered)
and signed BarChart (bars up/down from a zero baseline) via an offscreen Skia `RenderTargetBitmap`
harness. (Live-gallery nav was blocked by Windows foreground-lock contention, so the offscreen render
was used for the two pages that couldn't be reached interactively.)

**Next:** 3.3 — chart hit-testing + hover tooltips (builds on the snapshot).

---

## 2026-06-14 — 3.1 cycle — Package split Phase C (versioning, CI, guard test, docs)

**Done**
- Hoisted shared package metadata + `Version=3.1.0` to `Directory.Build.props` (all four packages
  version in lockstep); added `Directory.Build.targets` to pack README + icon into every packable
  project (conditioned `IsPackable != false`). Stripped the now-shared props from `src/Loam/Loam.csproj`.
- `ci.yml` + `package.yml` now `dotnet pack Loam.slnx` → produces all four nupkgs (publish loop already
  iterates `*.nupkg`); the tag-driven `/p:PackageVersion` override applies to all four.
- Added `tests/Loam.Tests/PackagingTests.cs` (33 cases): each moved control/registrar lives in its
  satellite assembly and is absent from core; core keeps `LoamTheme`.
- Docs: getting-started (satellite install + registrars), changelog (3.1.0 entry + corrected stale
  3.0.0 "still planned"), new `migration/v3-to-v3.1.md` (+ sidebar/nav wiring; v2→v3 package-split row
  flipped to ✅ 3.1.0), component callouts on charts/pickers/data-display, README (status, modular
  bullet, install, catalog tags, repo layout, pack cmd), ADR-0009 status → implemented.
- **Decision:** kept `ChartBase.TryChartResource` `OfType<LoamTheme>()` fallback — still compiles (core
  is referenced) and guards token resolution before the chart is attached to the visual tree.

**Verified**
- `dotnet pack Loam.slnx -c Release` → `Loam`, `Loam.Charts`, `Loam.Pickers`, `Loam.Data` all `3.1.0`;
  satellite nuspec depends on `Loam 3.1.0` + `Avalonia 12.0.4`, carries MIT/icon/readme/release-notes.
- Release build 0 warnings / 0 errors; full suite **458 passing** (was 425 + 33 new guards).

**Next:** PR `work/3.1` → main, then tag `v3.1.0` to publish all four packages (outward-facing — awaiting
go-ahead). Optionally a GitHub Release mirroring the 3.0.0 one.

---

## 2026-06-14 — 3.1 cycle — Package split Phase B (physical split into 3 satellite projects)

**Done**
- Created `src/Loam.Charts`, `src/Loam.Pickers`, `src/Loam.Data` SDK projects (net8.0, Avalonia via CPM,
  `ProjectReference` → core, `IsPackable` + `PackageId`/`Description`/`PackageTags`).
- `git mv` the mapped files (history preserved, namespaces unchanged): Charts (Charts.cs, ChartLegend.cs,
  LoamCharts.cs); Pickers (Date/Time/Color/DateRange +Themes, MonthCalendar, LoamPickers.cs); Data
  (DataGrid, DataGridColumn, SimpleTable+Theme, TreeView+Theme, TreeViewItem+Theme, Pagination+Theme,
  LoamData.cs). Core keeps Tabs/Stepper/Carousel/Timeline/ExpansionPanel(s).
- `InternalsVisibleTo` from **core** → the 3 satellites (they use internal `FieldChrome`/`PopupSurface`/
  `InteractionAssist`/`TemplateScope`), and from **each satellite** → `Loam.Tests` (ChartTests etc. read
  internal chart members — this was the only build break, fixed).
- Registered the 3 projects in `Loam.slnx`; gallery + tests `ProjectReference` all three satellites.

**Verified**
- `dotnet build Loam.slnx -c Release` — 0/0 across all 6 projects (`Loam.Charts/Pickers/Data.dll` build as
  separate assemblies). Full suite **425 passed** — controls render themed via the registrars across the
  assembly boundary; charts intact. **Core has no reference to any satellite.**

**Next:** Phase C — hoist shared version/packaging metadata to `Directory.Build.props`; `package.yml`/
`ci.yml` pack all 4 nupkgs (publish loop already handles many); CI guard test (core lacks moved types);
docs/migration + `v3.1.0`. Optional cleanup: drop the `ChartBase` `OfType<LoamTheme>` fallback.

---

## 2026-06-14 — 3.1 cycle — Package split Phase A (decouple theme registration in-place)

Branch `work/3.1`. First step of the package split (plan: `memory/plans/package-split-3.1.md`, ADR-0009).

**Done**
- Added per-satellite `Styles` registrars in core: `LoamPickers` (DatePicker/TimePicker/ColorPicker/
  DateRangePicker), `LoamData` (SimpleTable/Pagination/TreeView/TreeViewItem), `LoamCharts` (empty —
  charts self-render; shipped for a uniform "add the styles" story) in `src/Loam/Theming/`.
- Removed the 8 satellite `typeof` registrations from `LoamTheme.RegisterControlThemes` (the only
  core→satellite coupling). Core no longer references the picker/data control themes.
- Wired `samples/Loam.Gallery/App.cs` + `tests/Loam.Tests/TestApp.cs` to add the three registrars next
  to `LoamTheme`.
- Registrars live in core for now (same assembly → use the internal `*Theme.Create()` directly), so no
  `InternalsVisibleTo` needed yet. Deferred to Phase B: `InternalsVisibleTo` for the satellite assemblies
  and dropping the `ChartBase` `OfType<LoamTheme>` fallback (it also guards the not-yet-attached case).

**Verified**
- `dotnet build Loam.slnx -c Release` — 0/0. Full suite **425 passed** (the test app adds the registrars,
  so picker/data control rendering/metric assertions still pass — confirms the registration mechanism).

**Next:** Phase B — physical split into `src/Loam.Charts`/`Loam.Pickers`/`Loam.Data` projects (move files,
project refs, `InternalsVisibleTo`, slnx), then Phase C (packaging/CI + docs + `v3.1.0`).

---

## 2026-06-13 — v3 — Gallery per-sample layout rolled out to all groups

**Done:** finished the per-sample gallery layout across every component group, so each multi-variant
page now renders **each sample as its own preview card immediately followed by its own C# snippet**
(via `PageWithSamples` + `Sample(caption, BuildXxxVariant)` + per-sample builder methods). Genuinely
single-cohesive-demo pages stay as `Page(...)` (one preview + one code block) — correct, since they
have no separate captioned variants.

Converted (≈35 pages): Display (all 8), DataGrid, Inputs (RadioGroup/Slider/ToggleGroup),
Feedback (ProgressCircular/ProgressLinear/Skeleton/Popover/Tooltip), Data (SimpleTable/Tabs/
ExpansionPanels/Collapse/Timeline/Carousel/Stepper/Pagination), Navigation (Breadcrumbs/NavMenu),
Layout (Container/ResponsiveGrid/Spacer), Charts (PieChart/BarChart/LineChart), plus Button/IconButton.
Left as single-demo `Page(...)`: pickers, text inputs, Shell, Surfaces, TreeView, NavLink/NavGroup/rails,
Col/Hidden/ScrollToTop, etc. (no multi-captioned variants).

**How:** executed as a multi-agent **workflow** (one agent per group, sequential because all builders
live in one 7000-line file; per-group self-commit for durability). The first attempt died on a session
suspension and a later one half-converted Data (registrations referencing methods it never created — was
discarded); the hardened rerun (create methods **before** swapping each registration) finished cleanly.

**Verified:** `dotnet build Loam.slnx -c Release` 0/0; full suite **424 passed**; live spot-check of the
Inputs/Slider page confirms per-sample preview+code interleaving. Commits `1945d54`/`5be0a18`/`6c2fe14`/
`8168c8d`/`3186c57`/`4e3a9f9`/`54021a0` on `work/vnext`.

**Next:** optional — convert the remaining caption-less multi-variant pages (e.g. AppBar's 3 variants,
Buttons' ToggleIconButton/ButtonGroup/Fab) if per-sample splitting is wanted there too.

---

## 2026-06-07 — v3 — Gallery per-sample layout (infra + DataGrid flagship)

**Maintainer ask:** wider previews; **each sample interleaved with its own C# snippet** (sample → its
code → next sample) instead of all previews then one big code dump; do it for all pages.

**Architecture before:** each `GalleryPage` had one `BuilderMethod`; `page.Code` was that whole
method's source (regex-extracted from `ComponentsView.cs` at runtime); `BuildArticle` = header + one
Preview card + one `CodeSampleView`.

**Done (infrastructure, applied to DataGrid as the verified flagship)**
- New `GallerySample(Caption, Build, Code)` + `GalleryPage.Samples` (init, default empty). Helpers
  `Sample(caption, build)` (captures `build.Method.Name` → per-sample code via `GallerySourceCode`)
  and `PageWithSamples(group, title, desc, params samples)` (page `Code` = join of sample codes so the
  metadata asserts still hold; `Build` = all samples stacked for the expected-components test;
  `BuilderMethod` = first sample's method).
- `BuildArticle` now interleaves: when `Samples` is non-empty it renders, per sample, a Preview `Paper`
  (caption header + control) **followed by that sample's `CodeSampleView`**. Single-builder pages keep
  the old one-preview-one-code path (back-compat) so the rollout is incremental.
- Converted the **DataGrid** page: split the monolithic `BuildDataGrid` into `SampleDesserts()`,
  `AddDessertColumns()`, and six per-sample methods (`BuildDataGridPaged/Grouped/Frozen/Editable/
  Virtualized/Empty`), each its own snippet; widened grids (520→720, frozen 460→560).
- Relaxed the gallery test that asserted exactly one `CodeSampleView` per article → now ≥1 (we
  intentionally render one per sample).

**Verified**
- `dotnet build Loam.slnx -c Release` — 0/0; full suite **424** green. Ran the desktop gallery: the
  DataGrid page now shows each sample's preview card immediately followed by its own highlighted C#
  snippet, then the next sample — exactly the requested structure.

**Next:** roll the `PageWithSamples` pattern out to the remaining pages (large mechanical pass — split
each multi-variant builder into per-sample methods). Infra + flagship are in; the rest is repetition.

---

## 2026-06-07 — v3 — Gallery DataGrid page: visual cleanup (ran the app to verify)

**Maintainer feedback:** "the ui of grid sample is awful." Ran the desktop gallery and screenshotted
the DataGrid page to see it. Real problems found: (1) the shared `Labeled` helper is a fixed **96px**
side caption, so the new long captions truncated ("Grouped + aggr", "Frozen column") and floated
mid-height beside tall grids; (2) the Paged sample used **editable** cells whose `TextBox` forced tall
rows, misaligning the name vs the centered number columns.

**Done**
- Reworked `BuildDataGrid` to a clean **stacked** layout: a local `Section(title, control)` puts a
  primary-colored heading **above** each grid (full captions, no truncation), replacing the side
  `Labeled` for this page. Samples: Sortable·filtered·paged · Grouped+aggregate · Frozen first column ·
  Editable cells · Virtualized · Empty. Paged is now non-editable (tight, aligned); inline-edit moved
  to its own "Editable cells" sample.
- **Product fix:** editable `DataGrid` cells now align with text cells — the cell `TextBox` gets
  `MinHeight = 0` + `VerticalContentAlignment = Center` so it no longer inflates row height.
- Re-ran the app: headings sit above each grid, rows align, frozen column pins with a working
  horizontal scrollbar, editable rows match the others. Eyesore resolved.

**Verified**
- `dotnet build Loam.slnx -c Release` — 0/0. Full suite **424** green. Visually confirmed in the
  running desktop gallery.

---

## 2026-06-07 — v3 Phase 5/6 — Gallery: demo the new DataGrid features

**Gap found (by maintainer):** the Phase 5 `DataGrid<T>` features had tests + docs but no **gallery**
demo (only the new nav controls got gallery pages). Fixed.

**Done**
- Extended `BuildDataGrid` in `Loam.Gallery`: added **Grouped + aggregate** (`GroupBy` Indulgent/Light
  with `GroupAggregate` avg-calories), **Frozen columns** (`FrozenColumns = 1` with explicit column
  `Width`s + two derived columns so the scrollable pane actually scrolls), and **Empty** (custom
  `EmptyText` with a filter that excludes all) — alongside the existing Paged + Virtual samples. This
  also makes the page's long-declared `Empty` acceptance criterion truthful.
- Audited the rest: Material You / high-contrast / `SetDensity` are already demoed via the header
  `BuildSeedPicker`; the new nav controls had pages. The one other untouched Phase 3 item —
  **`AppBar.CustomActions`** — had no gallery demo either, so added a third AppBar sample showing
  arbitrary controls (a filled button + icon button) in that slot.

**Verified**
- `dotnet build Loam.slnx -c Release` — 0/0. GalleryAcceptanceTests (37) green; full suite **424** green.

---

## 2026-06-07 — v3 Phase 6 — Release prep (docs, positioning, version)

**Done**
- New docs: **`guide/why-loam.md`** ("Why Loam vs plain Avalonia" — side-by-side code, comparison table,
  honest "use plain Avalonia when…") and **`guide/changelog.md`** (v3 preview release notes by phase).
  Wired both into the Guide sidebar; refreshed the stale `guide/introduction.md` Status section
  (v2 → v3 preview) and links.
- Refreshed the **README** component catalog (`Grid`/`Item`/`Stack` → `ResponsiveGrid`/`Col`; added
  `NavigationRail`/`BottomNavigation`/`CommandPalette`; "deeper DataGrid grouping" → "richer inline-edit").
- **Version bump** `3.0.0-preview.1` → `3.0.0-preview.2`; added `PackageReleaseNotes` + a `material`
  tag; updated version refs in README + migration guide.
- **Release-readiness check:** `dotnet pack -c Release` produced `Loam.3.0.0-preview.2.nupkg` cleanly
  (README + icon + license metadata all present); docs build passed.

**Decision (visual-regression):** deferring pixel-snapshot tests — cross-machine font rendering makes
them flaky, which conflicts with the zero-flake gate. The existing `GalleryAcceptanceTests` already
render every page headlessly (a stable render-smoke guard); pixel snapshots, if added later, should be
an opt-in CI job, not part of the default suite.

**Verified**
- `npm run docs:build` passed; `dotnet pack -c Release` succeeded. Suite unchanged at **424**.

**Next:** the final `3.0.0` cut (drop the preview suffix) when ready; the deferred package split
(ADR-0009); optional richer inline-edit / CAM16-HCT.

---

## 2026-06-07 — v3 Phase 5 — DataGrid frozen columns + group aggregates

**Done**
- **Column-width API:** `DataGridColumn<T>.Width` (pixel; `null` = star). Single-grid mode now honors
  mixed fixed/star columns.
- **Frozen columns:** `DataGrid<T>.FrozenColumns` pins the leading N columns in a left `Grid` while the
  rest render in a right `Grid` inside a horizontal `ScrollViewer` (two-pane `BuildFrozenGrid`). Row
  hover/selection are **synced across panes** via a shared `RowVisual` (refactored the old per-border
  closure state into `RowVisual` + `AddRowBackgroundTo` + `ApplyRowBackgrounds`). `RowHeight` (px, 0=auto)
  guarantees cross-pane row alignment for custom-height cells. Frozen layouts size all columns by pixel
  width (default 140 when unset). Frozen is **ignored while grouped** (falls back to single grid) to
  avoid the group-header-spanning-panes problem — documented limitation.
- **Group aggregates:** `GroupAggregate` (`Func<IReadOnlyList<T>, string>?`) appends computed text
  (sum/avg/etc.) to each group header.
- Added `DataDisplayTests`: two-pane render + horizontal scroller, frozen row activation selects,
  frozen-ignored-while-grouped, group-aggregate text. Docs (`data-display.md`) + tracker updated.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors (fixed CA1859 by typing
  the builders as `Grid`).
- Full suite — **424 passed**, 0 failed.

**This completes the named Phase 5 data-maturity items** (grouping, collapsible groups, empty state,
frozen columns, group aggregates). **Next:** Phase 6 release prep (visual-regression snapshots,
positioning docs, `3.0.0`), or richer inline-edit. Package split still deferred (ADR-0009).

---

## 2026-06-07 — v3 Phase 5 — DataGrid empty state

**Done**
- `DataGrid<T>` now renders a proper empty state (below the header) when there are no rows to display
  after filtering: `EmptyText` (default "No data") or a custom `EmptyContent` control, spanning all
  columns. Detected via "no data/group rows rendered" so collapsed-but-present groups don't trigger it;
  custom `EmptyContent` is detached from its prior parent on rebuild (reparenting-safe).
- Added two `DataDisplayTests` (empty `Items` shows "No data"; filter excluding all shows custom text).

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **420 passed**, 0 failed.

**Next:** frozen columns is the remaining named Phase 5 item but needs a column-width API + a
synced-horizontal-scroll two-pane layout (row-height alignment) — a larger change; deferred with that
rationale. Otherwise Phase 6 release prep. Package split still deferred (ADR-0009).

---

## 2026-06-07 — v3 Phase 5 — Collapsible DataGrid groups

**Done**
- Group headers in `DataGrid<T>` are now collapsible: a chevron (ExpandLess/ExpandMore) + clickable,
  keyboard-activatable header toggles showing/hiding that group's rows. Collapsed state is tracked by
  group key (`HashSet<object>` with a null-key sentinel) and **survives re-renders** (sort/filter/page);
  the header keeps the full group count when collapsed. New `CollapsibleGroups` opt-out (default on);
  `GroupBy` changes clear collapsed state.
- Added a `DataDisplayTests` case (collapse hides rows + keeps header/other groups; re-expand restores).

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **418 passed**, 0 failed.

**Next:** optional Phase 5 (frozen columns) or Phase 6 release prep (visual-regression snapshots,
positioning docs, `3.0.0`). Package split still deferred (ADR-0009).

---

## 2026-06-07 — v3 Phase 5 — DataGrid grouping

**Done**
- Added grouping to the self-rendering generic `DataGrid<T>`: a `GroupBy` selector renders a
  group-header row (`key (count)`, surface-container-high, on its own full-width row) above each
  group's data rows, integrated with the existing filter→sort→page→render pipeline (groups apply within
  the rendered page, in first-appearance order so they follow the current sort).
- Added the pure, testable `DataGrids.Group<T>(items, selector)` + `DataGridGroup<T>` record
  (first-appearance order; `null` keys form their own group). Refactored `BuildGrid`'s row loop into a
  shared `AddDataRow` local fn used by both grouped and ungrouped paths; added `BuildGroupHeader`.
- Added `DataDisplayTests` (pure group order + null keys; end-to-end group headers render) and
  documented `GroupBy`/`Group` in `docs/components/data-display.md` + the tracker.
- Scope note: grouping is per-page; cross-page group continuity, collapsible groups, frozen columns,
  and richer inline-edit remain future expansion.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **417 passed**, 0 failed.

**Next:** more Phase 5 (collapsible groups / frozen columns) or Phase 6 release prep
(visual-regression snapshots, positioning docs, `3.0.0`). Package split still deferred (ADR-0009).

---

## 2026-06-07 — v3 Phase 6 — Migration-guide accuracy pass (deferring package split)

**Decision:** per maintainer direction, defer the Phase 4 **package split** (`Loam.Charts`/
`Loam.Pickers`/`Loam.Data`) — it's the biggest, riskiest v3 change and has a theme-registration
coupling to design first. Keep the single assembly for now and consolidate (Phase 6).

**Done**
- Brought `docs/migration/v2-to-v3.md` in sync with reality (it's a living record): phase tags in
  "What v3 is about" now read Phases 1–3 ✅ done, Phase 4 in progress (split deferred). Replaced the
  stale "Coming in later phases" list with an accurate **Delivered in this preview** summary
  (theme bridge, Material You + high-contrast + density, AppBar slot/content-precedence/global-usings,
  NavigationRail/BottomNavigation/CommandPalette) plus a trimmed **Coming** list (package split,
  DataGrid maturity, release).
- Refreshed the README status/test count (377 → **414**) and v3 framing.

**Verified**
- `npm run docs:build` passed. Docs-only change; suite unchanged at 414.

**Next:** Phase 5 — DataGrid maturity (grouping is the main remaining gap; sort/filter/page/virtualize/
edit already exist), or continue Phase 6 (visual-regression snapshots, positioning docs) before the
`3.0.0` release. Package split remains deferred (ADR-0009).

---

## 2026-06-07 — v3 Phase 4 — CommandPalette (additive)

**Done**
- Added `CommandPalette` + `CommandPaletteItem`: a searchable command list (search `TextField` over
  live-filtered `ListItem` rows on an elevated `Paper`) with keyboard nav (Down/Up move, Enter runs,
  Escape closes), two-way `FilterText`/`IsOpen`, `Invoked`/`Closed` events, and a **pure testable**
  `Filter(commands, query)` (case-insensitive title/keyword match).
- Added a gallery page (`Feedback/CommandPalette`) + icon, `CommandPaletteTests` (pure filter, live
  filter + selection reset, keyboard invoke + close), docs, and the tracker row.
- Scope note: hosting the palette on the window overlay layer (a `DialogService`-style
  `CommandPalette.For(...)`) is a deferred follow-up; the control works inline / in an `Overlay`/dialog.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **414 passed**, 0 failed.

**This completes Phase 4's additive controls** (`NavigationRail`, `BottomNavigation`, `CommandPalette`).
**Next:** the Phase 4 package split — extract `Loam.Charts`/`Loam.Pickers`/`Loam.Data` satellites
(ADR-0009), the last and biggest Phase 4 item.

---

## 2026-06-07 — v3 Phase 4 — BottomNavigation (additive shell control)

**Done**
- Added Material 3 `BottomNavigation` + `BottomNavigationItem`. `BottomNavigation` (`: Decorator` +
  `UniformGrid` Rows=1 for equal-width cells, surface-container background, two-way `SelectedIndex`,
  `SelectedItem`, `SelectionChanged`). `BottomNavigationItem : NavigationRailItem` — reuses the
  icon-over-label active-indicator-pill anatomy and activation (zero duplication).
- Made `NavigationRailItem` fully tappable (transparent stretch hit-target wrapper around the centered
  content) — benefits wide bottom-nav cells and the rail alike.
- Added a gallery page (`Navigation/BottomNavigation`) + icon, `BottomNavigationTests`, docs, and the
  tracker row.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **411 passed**, 0 failed.

**Next:** Phase 4 remaining — `CommandPalette` (searchable command overlay), then the package split
(`Loam.Charts`/`Loam.Pickers`/`Loam.Data`, ADR-0009).

---

## 2026-06-07 — v3 Phase 4 — NavigationRail (additive shell control)

**Done**
- Added Material 3 `NavigationRail` + `NavigationRailItem` (new Loam controls). Self-composed
  (`: Decorator`, no ControlTheme boilerplate): item = centered icon in a secondary-container
  active-indicator pill above a label, with hover/focus state layers and click + keyboard (Enter/Space)
  activation; rail = `Items` + optional `Header` + two-way `SelectedIndex` + `SelectedItem` +
  `SelectionChanged`, single-selection management.
- Token-bound (Surface rail bg; SecondaryContainer/OnSecondaryContainer/OnSurface/OnSurfaceVariant
  roles), so it re-themes with Material You / variant swaps. Icons tinted via `Icon.Foreground`
  (`Color = Inherit`).
- Added a gallery page (`Navigation/NavigationRail`) + icon, and `NavigationRailTests` (default
  selection, `SelectedIndex` updates, keyboard activation, active-indicator color). Documented in
  `docs/components/navigation.md` and the component tracker.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **409 passed**, 0 failed.

**Next:** Phase 4 remaining — `BottomNavigation`, `CommandPalette` (additive), then the package split
(`Loam.Charts`/`Loam.Pickers`/`Loam.Data`, ADR-0009).

---

## 2026-06-07 — v3 Phase 4 — Deprecate Stack + table strategy

**Done**
- Marked `Stack` `[Obsolete]` (`LOAM0003`) → use `Avalonia.Controls.StackPanel`. Migrated every gallery
  + test usage to `StackPanel` (`Row = true` → `Orientation = Orientation.Horizontal`; default
  `Spacing = 8` set explicitly). Removed the gallery's dedicated Stack page (StackPanel is framework
  standard) and its acceptance test; kept a scoped back-compat unit test
  (`#pragma warning disable LOAM0003`) for the shim.
- Self-references inside the obsolete `Stack` (its own `Register`/`nameof`) don't warn — obsolete
  usage within an obsolete type is exempt, so the library stays warning-clean under
  `TreatWarningsAsErrors`.
- **Table strategy (ADR-0013):** `DataGrid<T>` is the recommended table; `SimpleTable` is kept as the
  minimal static option (not deprecated, positioned secondary). Added "Choosing a table" guidance to
  `docs/components/data-display.md`; updated the migration guide rename map and the component tracker.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **405 passed**, 0 failed.

**Next:** Phase 4 remaining — additive shell controls (`NavigationRail`, `BottomNavigation`,
`CommandPalette`), then the package split into `Loam.Charts`/`Loam.Pickers`/`Loam.Data` (ADR-0009,
the big restructure).

---

## 2026-06-07 — v3 Phase 3 — Collision tooling: global-usings snippet (Phase 3 core complete)

**Done**
- Added a documented one-file **`GlobalUsings.cs`** recipe to `docs/guide/csharp-ui.md`: a single
  `global using Button = Loam.Controls.Button;` (etc.) makes the bare restyle names resolve to Loam
  project-wide, removing the per-file `using LoamX = …` friction REVIEW flagged. Documented the
  trade-off (qualify Avalonia's control in the rare file that needs it) and that net-new concepts
  (`ResponsiveGrid`/`Col`/`Paper`/`Chip`) never clash.
- This satisfies the PLAN's "GlobalUsings snippet" collision aid. A full Roslyn rename/collision
  analyzer remains an optional, heavier future item (ADR-0008).

**Verified**
- Docs-only change; `npm run docs:build` passed. (No code/test impact; suite stays at 405.)

**Phase 3 core is complete:** `AppBar` custom-actions slot, explicit generated-vs-custom content
precedence (+ debug warning), and the global-usings collision aid. Remaining Phase 3 is the optional
analyzer.

**Next:** Phase 4 — component churn & packaging: drop thin wrappers (`Stack` → `StackPanel`),
consolidate the table story (`SimpleTable` vs `DataGrid<T>`), and extract `Loam.Charts`/`Loam.Pickers`/
`Loam.Data` satellites (ADR-0009); plus add `NavigationRail`/`BottomNavigation`/`CommandPalette`.

---

## 2026-06-07 — v3 Phase 3 — Generated-vs-custom content precedence

**Done**
- Made the dual-mode content rule explicit: **custom `Content` always wins** over the generated
  anatomy (`Title`/`Subtitle`/`Body`/…) on `Paper`, `Card`, and `Drawer`. Documented it on the shared
  `Internal.DualContent` helper, in the control docs, and in `docs/components/layout.md`.
- Added a **debug-only** diagnostic (`DualContent.WarnIfConflicting`, `[Conditional("DEBUG")]`) that
  logs when both custom `Content` and generated props are set on one instance — compiled out entirely
  in Release (call + arg evaluation elided), so zero cost and no Release warnings. Wired at both
  conflict points (Content set with generated props present; generated prop set with custom Content
  present) in Paper/Card/Drawer.
- Added `PrimitivesTests` covering the precedence contract in both orderings (custom set after generated
  wins; generated set after custom is ignored).

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **405 passed**, 0 failed. (Warning itself is debug-only, so tested via the behavioral
  precedence contract, not the log output — tests run in Release.)

**Next:** Phase 3 remaining — collision tooling: a `GlobalUsings` snippet / analyzer so consumers
don't hand-alias the restyle names (`Button`/`Text`/`Card`/`Menu`/…) per file. (`Form`'s Child-based
dual mode could get the same warning later.)

---

## 2026-06-07 — v3 Phase 3 — AppBar custom-actions slot

**Done**
- Added `AppBar.CustomActions` (`AvaloniaList<Control>`) — a trailing slot for arbitrary live controls
  (toggles, search fields, stateful actions), rendered before the icon-only `Actions`. Solves the
  REVIEW finding that `AppBar.Actions` only accepted immutable `AppBarAction`.
- Re-host safety: the trailing strip is now a stable instance panel, detached from its previous parent
  and cleared on each rebuild, so live `CustomActions` controls re-host without reparenting errors
  (mirrors `MainContent`'s header-row approach). `MainContent.Actions` was already `Control`-typed.
- Documented `Actions` vs `CustomActions` vs `Content` in `docs/components/layout.md`.
- Added `ShellTests`: custom + icon actions render together; live controls survive repeated rebuilds.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **403 passed**, 0 failed.

**Next:** Phase 3 remaining — generated-vs-custom content precedence (explicit slots + debug warning
when both `Content` and generated props are set on `Paper`/`Card`/`Drawer`); collision tooling
(GlobalUsings snippet / analyzer for the restyle names).

---

## 2026-06-07 — v3 Phase 2 — High-contrast variant (Phase 2 complete)

**Done**
- Added a `LoamContrast` { Standard, Medium, High } level threaded through
  `LoamColorScheme.FromSeed(seed, dark, contrast)`, `LoamThemeData.FromSeed(seed, contrast)`, and
  `LoamTheme.SetSeed(seed, contrast)`. Standard reproduces the Material 3 tones exactly (existing tests
  unchanged); higher levels push role tones toward the extremes (accents, on-roles, surfaces, outlines)
  for stronger separation.
- Gallery: added a **High contrast** `Switch` to the theme playground; the playground now tracks the
  current seed + contrast via closures so seed swatches and the contrast toggle compose
  (`SetSeed(seed, contrast)`). `SeedSwatch` takes a callback.
- Documented high contrast in `docs/guide/theming.md`.
- Added `MaterialYouTests`: Standard overload equals the 2-arg default; High increases separation over
  Standard; High meets WCAG AAA (≥ 7:1) on the main text pairs across 6 seeds × light/dark.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **401 passed**, 0 failed.

**Phase 2 is complete:** Material You seed→scheme generator, gallery seed picker, one-call density
switch, and a high-contrast variant. `LoamTheme` now exposes `SetSeed`/`SetPrimary`/`SetPalette`/
`SetDensity`/`SetData`.

**Next:** Phase 3 — naming & ergonomics refactor: `AppBar` custom-actions slot (accept arbitrary
`Control`s, not just immutable `AppBarAction`), generated-vs-custom content precedence (explicit slots
+ debug warning), and collision tooling (a `GlobalUsings` snippet / analyzer for the restyle names).

---

## 2026-06-07 — v3 Phase 2 — One-call density switch (compact mode)

**Done**
- Added `LoamDensity.Compact` preset (reduced interactive targets, button heights/padding, icon-button
  and tabular padding) and a runtime `LoamTheme.SetDensity(LoamDensity)` entry point (keeps
  colors/typography). Density tokens already flow through `ProjectSharedTokens`, so the switch updates
  `Loam.Density.*` at runtime.
- Gallery: turned the header seed flyout into a small "Theme playground" — seed swatches + a
  **Compact density** `Switch` (calls `SetDensity`) + Reset (restores `LoamThemeData.Default` and
  unchecks compact).
- Documented `SetDensity` in `docs/guide/theming.md` runtime-setters table.
- Added `ThemingTests`: Compact metrics < Default; `SetDensity` updates density tokens at runtime.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **398 passed**, 0 failed.

**Next:** Phase 2 remaining — high-contrast theme variant. (Then Phase 3: naming/ergonomics refactor —
`AppBar` custom-actions slot, generated-vs-custom content precedence, collision tooling.)

---

## 2026-06-07 — v3 Phase 2 — Gallery Material You seed playground

**Done**
- Added a live **seed picker** to the gallery header (palette `IconButton` → `Flyout` of seed swatches
  + Reset). Clicking a swatch calls `LoamTheme.SetSeed` on the app's theme instance, recoloring the
  whole gallery at runtime (base controls follow via the Phase-1 Fluent bridge); Reset restores
  `LoamThemeData.Default`. Found via `Application.Current.Styles.OfType<LoamTheme>()`.
- Noted: `DesignSystemView` (its old `SetPrimary` swatches) is orphaned — `MainWindow` only shows
  `ComponentsView`, so the playground was added to the real shell header.
- Documented `SetSeed`/Material You in `docs/guide/theming.md` (runtime setters table + a section,
  incl. the accessibility-by-construction note and the gallery seed picker).
- Added a gallery acceptance test for the seed picker (present in the header, palette icon, has a
  flyout). Kept the existing header test green (seed is a plain `IconButton`; the "Toggle theme"
  `ToggleIconButton` stays uniquely identifiable).

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors (CA1859 fixed:
  `BuildSeedFlyout` returns the concrete `StackPanel`).
- Full suite — **396 passed**, 0 failed.
- `npm run docs:build` — passed.

**Next:** Phase 2 remaining — high-contrast theme variant, one-call compact/density switch. (Optional:
wire `DesignSystemView` into the shell or remove it; CAM16/HCT upgrade for exact M3 fidelity.)

---

## 2026-06-07 — v3 Phase 2 — Material You seed→scheme generator (first slice)

**Done**
- Implemented one-seed → complete light + dark `LoamColorScheme` generation (the headline
  "customizable" feature). New `LoamLab` (sRGB↔Lab↔LCh, tone = CIE L\*, gamut-clamped chroma),
  `LoamTonalPalette` (hue + chroma, sampled by tone), `LoamColorScheme.FromSeed(seed, dark)` mapping
  every role to standard M3 tones.
- Runtime API: `LoamTheme.SetSeed(color)` and `LoamThemeData.FromSeed(color)` — regenerate both
  schemes + compatibility palettes, keeping typography/shape/etc. The Phase-1 Fluent accent bridge
  follows the new seed automatically.
- Added **ADR-0012** (CIELAB tonal palettes as a tractable, accessible approximation of CAM16/HCT;
  full HCT deferred as an optional upgrade).
- Added `MaterialYouTests`: accessibility across 6 seeds × light/dark × 11 text pairs, tone ordering,
  gamut clamping at extremes, `FromSeed`/`SetSeed` runtime updates.

**Verified**
- Key insight (and why generated schemes are accessible by construction): WCAG luminance == XYZ Y, and
  L\* is a function of Y alone, so tone-gap contrast is deterministic and matches M3 — independent of
  the seed's hue/chroma. 132 contrast assertions pass WCAG AA (≥ 4.5).
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **395 passed**, 0 failed.

**Next:** Phase 2 remaining — gallery theme playground (live seed picker), high-contrast variant, a
one-call compact/density switch. Optional: CAM16/HCT upgrade for exact Material You fidelity.

---

## 2026-06-07 — v3 Phase 1 — Expander bridge + consolidate FluentBridge + ADR-0011

**Done**
- Themed the base Avalonia `Expander`: header on the tonal container ramp (rest/hover/press),
  `OnSurface` header text, outline-variant edges, `Surface` content, neutral chevron. Size/padding/
  alignment keys left to Fluent.
- **Refactor:** extracted the five inline `BridgeFluent*` methods out of `LoamTheme` into a dedicated
  `Loam.Theming.FluentBridge` static helper (now six bridges + accent shade math), invoked once per
  variant via `FluentBridge.Apply(dict, scheme, stateLayer)`. `LoamTheme` is back to token projection
  + control-theme registration.
- Added **ADR-0011** documenting the bridge approach (override brush keys not colors; colors only,
  leave geometry to Fluent; source-verified + version-coupled; retired when FluentTheme is dropped).
- Added Expander `FluentBridgeTests` (projection + end-to-end) and renamed the test helper to
  `BrushColor` (now general across all bridges).
- **Visual:** user confirmed the demo app looks good in both light and dark themes — closes the
  Phase 1 visual-verification gap for accent/scrollbar/tooltip/menu/window/selection/expander.

**Verified**
- Source-checked Avalonia 12.0.4 `Controls/Expander.xaml` for the exact brush keys.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **390 passed**, 0 failed.

**Next:** Avalonia `DataGrid` is the last Phase 1 item, but the core package doesn't reference
`Avalonia.Controls.DataGrid` (its theme ships separately) — defer to the `Loam.Data` satellite
(ADR-0009) or document a consumer opt-in. Then Phase 2 (Material You seed→scheme generator).

---

## 2026-06-07 — v3 Phase 1 — Theme consistency: Window background + text selection

**Done**
- Added `LoamTheme.BridgeFluentWindowAndText`: the bare Window region background now reads as the
  Material app background (`SystemRegionBrush` → scheme `Background`), and base text selection uses
  Loam primary instead of Fluent blue (`TextControlSelectionHighlightColor` → primary @ 0.4). The
  selection fix flows into every base `TextBox`, including those hosted inside Loam `Field`/`TextField`.
  Per variant, runtime-swappable.
- Added two `FluentBridgeTests` (per-variant projection + end-to-end selection resolution).

**Verified**
- Source-checked Avalonia 12.0.4 `Controls/Window.xaml` (`SystemRegionBrush`) and `Controls/TextBox.xaml`
  (`TextControlSelectionHighlightColor`, `CaretBrush` = `TextControlForeground`).
- Left caret/text foreground to Fluent for now (caret follows `TextControlForeground`; overriding it
  would broadly change base text color — a separate decision).
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **388 passed**, 0 failed.

**Next:** continue Phase 1 — `Expander`, Avalonia `DataGrid`. Then consider extracting the five
`BridgeFluent*` methods into a dedicated `FluentBridge` helper + an ADR, and a visual gallery pass
(light/dark) to confirm the look (selection opacity 0.4 and scrollbar thumb opacities are tunable).

---

## 2026-06-07 — v3 Phase 1 — Theme consistency: ContextMenu / MenuFlyout bridge

**Done**
- Added `LoamTheme.BridgeFluentMenu`: base Avalonia context menus, menu flyouts, and plain flyouts now
  read as Material — `SurfaceContainer` surface (no border), `OnSurface` item text, OnSurface state
  layers on hover/press (using the theme's state-layer opacities), muted `OnSurfaceVariant` shortcut
  text and submenu chevrons, with disabled states at the disabled opacity. Per variant,
  runtime-swappable.
- Overrides the Fluent menu brush keys: `MenuFlyoutPresenterBackground/BorderBrush`,
  `FlyoutPresenterBackground`, `MenuFlyoutItem{Background,Foreground}*`,
  `MenuFlyoutItemKeyboardAcceleratorTextForeground*`, `MenuFlyoutSubItemChevron*`. Size/margin/corner
  keys left to Fluent.
- Added two `FluentBridgeTests` (per-variant projection + end-to-end menu-surface resolution).

**Verified**
- Source-checked Avalonia 12.0.4 `Controls/{MenuItem,ContextMenu,MenuFlyoutPresenter,FlyoutPresenter}.xaml`
  for exact keys; confirmed chevrons are `Fill` brushes and accelerators are `Foreground` brushes.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **386 passed**, 0 failed.

**Next:** continue Phase 1 — `Window` background, text selection/caret, `Expander`, Avalonia `DataGrid`.
Visual gallery pass (light/dark) still pending.

---

## 2026-06-07 — v3 Phase 1 — Theme consistency: ToolTip bridge

**Done**
- Added `LoamTheme.BridgeFluentToolTip`: base Avalonia tooltips now use the Material inverse-surface
  container with inverse-on-surface text and no border. Overrides `ToolTipBackground`,
  `ToolTipForeground`, `ToolTipBorderBrush` (per variant, runtime-swappable); geometry/size/corner keys
  left to Fluent.
- Added two `FluentBridgeTests` (projection per variant + end-to-end background resolution).

**Verified**
- Source-checked Avalonia 12.0.4 `Controls/ToolTip.xaml` for the exact brush keys.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **384 passed**, 0 failed.

**Next:** continue Phase 1 — `ContextMenu`/`MenuFlyout`, `Window` background, text selection/caret,
`Expander`, Avalonia `DataGrid`. Visual gallery pass (light/dark) still pending.

---

## 2026-06-07 — v3 Phase 1 — Theme consistency: ScrollBar bridge

**Done**
- Added `LoamTheme.BridgeFluentScrollBar` so base Avalonia ScrollBars read as Material: a subtle
  on-surface thumb (rest/hover/pressed/disabled) on a transparent track, with neutral line-button
  chrome. Per variant, runtime-swappable. Scrollbars are intentionally neutral, not accent-colored.
- Overrides the Fluent ScrollBar brush keys (`ScrollBarPanningThumbBackground`,
  `ScrollBarThumbFill{PointerOver,Pressed,Disabled}`, `ScrollBarTrackFill/Stroke`,
  `ScrollBarBackground/Border/Foreground`, and the `ScrollBarButton*` set). Geometry/size keys left to
  Fluent. The template resolves these via DynamicResource from the control's scope, so LoamTheme wins.
- Added two `FluentBridgeTests`: per-variant projection of the neutral thumb/track tokens, and
  end-to-end resolution of `ScrollBarPanningThumbBackground` through a live control.

**Verified**
- Source-checked Avalonia 12.0.4 `Controls/ScrollBar.xaml` (`gh api` at tag `12.0.4`) for the exact
  brush keys, thumb rest/hover/pressed states, and line-button chrome.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **382 passed**, 0 failed.

**Next:** continue Phase 1 — `ToolTip`, then `ContextMenu`/`MenuFlyout`, `Window` background, text
selection/caret, `Expander`, Avalonia `DataGrid`. Visual gallery pass (light/dark) still pending; the
thumb opacities (0.45/0.70/0.72) are reasonable defaults to confirm visually.

---

## 2026-06-07 — v3 Phase 1 — Theme consistency: Fluent accent bridge (first slice)

**Done**
- Added `LoamTheme.BridgeFluentAccent` so base Fluent controls with no Loam ControlTheme adopt Loam's
  primary instead of Fluent blue. Runs per variant inside `BuildVariantDictionary`, so it is
  runtime-swappable (`SetPrimary`/`SetPalette`/`SetData`) and light/dark correct.
- Overrides the `SystemAccentColor*` Color keys (base + six HSL-derived shades, mirroring Avalonia
  12.0.4's `SystemAccentColors` shade math) AND the `SystemControl*AccentBrush` brush keys — the
  brushes are the part that actually retints stray controls (see finding below).
- Added `FluentBridgeTests`: per-variant projection, runtime `SetPrimary` update, and an end-to-end
  test through the live `TestApp` (FluentTheme under LoamTheme) proving a stray
  `SystemControlHighlightAccentBrush` resolves to Loam primary.

**Verified**
- Source-checked Avalonia 12.0.4 (`gh api` at tag `12.0.4`): `Accents/SystemAccentColors.cs`,
  `Accents/BaseResources.xaml` — exact keys, shade formula, and per-key opacities.
- Discovered (and recorded in `findings/2026-06-07-fluent-accent-bridge.md`) that overriding only the
  `SystemAccentColor` color does NOT cascade — Fluent's accent brushes resolve it in FluentTheme's own
  scope; the brush keys must be overridden. Caught by the end-to-end test failing first (`#0078d7`),
  then passing after the brush override (`#6750a4`).
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — 0 warnings, 0 errors.
- Full suite — **380 passed**, 0 failed (added 3 bridge tests).

**Next:** continue Phase 1 — theme the residual base primitives via Loam ControlThemes / resource
bridges: `ScrollBar` (most visible), `ToolTip`, `ContextMenu`/`MenuFlyout`, `Window` background,
text selection/caret, `Expander`, Avalonia `DataGrid`. Then a visual pass of the gallery in light/dark
(not yet run — headless only). Consider an ADR for the base-chrome bridging approach as it grows.

---

## 2026-06-07 — v3 Phase 0 — Decide & scaffold (kickoff)

**Done**
- Started v3 ("vNext") on branch `work/vnext`; bumped `Loam.csproj` `<Version>` to `3.0.0-preview.1`.
- Locked three ADRs: ADR-0008 (naming & Avalonia collision strategy + rename map), ADR-0009 (package
  split: lean core + `Loam.Charts`/`Loam.Pickers`/`Loam.Data` satellites, deferred to Phase 4),
  ADR-0010 (v3 versioning & deprecation policy / breaking-change budget).
- Renamed the responsive layout: new canonical `ResponsiveGrid` (was `Grid`) and `Col` (was `Item`),
  behaviour-identical (carried automation names "Grid layout"/"Grid item" verbatim).
- Kept `Grid`/`Item` as `[Obsolete]` subclasses with stable diagnostic ids `LOAM0001`/`LOAM0002` and a
  migration URL (warning, not error).
- Migrated internal + sample + test call sites off the deprecated names (`ColorPicker`, `LayoutView`,
  `ShellView`, `ComponentsView`, `CodeSampleView`, `LayoutTests`, `GalleryAcceptanceTests`); renamed
  the gallery's `Layout/Grid`+`Layout/Item` pages to `Layout/ResponsiveGrid`+`Layout/Col`.
- Added a back-compat `LayoutTests` case proving the deprecated aliases still construct, resolve spans,
  lay out, and report the same automation names (scoped `#pragma warning disable LOAM0001, LOAM0002`).
- Scaffolded `docs/migration/v2-to-v3.md` (status, breaking-change policy, canonical rename map,
  diagnostic-id registry, step-by-step for the done renames, per-phase "coming soon"); wired it into
  the VitePress nav/sidebar and added a "Project" menu linking `PLAN.md`/`REVIEW.md`.
- Updated `docs/components/layout.md` (ResponsiveGrid/Col sections + deprecation notes; Avalonia `Grid`
  qualified in the fixed-2D example), README status, memory README, and the component tracker.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` — **0 warnings, 0 errors** (custom obsolete
  diagnostics `LOAM0001`/`LOAM0002` emit correctly under `TreatWarningsAsErrors`).
- `dotnet test … Loam.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` — **377 passed**, 0 failed (was 376 + 1 new
  back-compat test).

**Next:** Phase 1 — theme consistency: bridge Loam tokens to base Avalonia chrome (ScrollBar,
ToolTip, ContextMenu/MenuFlyout, Window, text selection/caret, Expander, Avalonia DataGrid) and map
`SystemAccentColor*` → Loam primary. Also run the docs site build to confirm the new page renders.

---

## 2026-06-07 — v2.0 — Gallery header and docs refresh

**Done**
- Moved the gallery theme control to the right side of the top bar and changed it to an icon-only `ToggleIconButton`.
- Replaced the header's read-only status chips with local token-bound status pills so theme switching cannot clip the outlines in the tight top-bar layout.
- Added light/dark mode icon paths and a warning icon to the curated built-in icon catalog.
- Updated docs examples and size descriptions so public examples match the current icon catalog and five-size control scale.
- Rebuilt the docs site; no extra image asset was added because the current docs issues were API accuracy issues, not missing visual explanation.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused gallery acceptance tests passed: 36 tests.
- `git diff --check` passed.
- `npm run docs:build` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 376 tests.
- `dotnet pack src\Loam\Loam.csproj -c Release --no-build` created `Loam.2.0.0.nupkg`; archive inspection showed only `Loam.dll`, `Loam.xml`, `README.md`, and package metadata.

**Next:** relaunch the Release gallery and visually confirm the top bar in light and dark; after that, continue with final release staging only after explicit approval.

---

## 2026-06-07 — v2.0 — Release readiness gate

**Done**
- Refreshed README and docs visible status text from the old test counts to the current verified suite count.
- Replaced stale `v1` catalog wording in the docs overview/introduction/homepage with current v2.0 baseline language.
- Ran a neutral-name/status scan over README/docs; remaining hits are technical API names such as `Icons.Material.*`, package-lock hashes, or literal color examples.
- Built the release package artifact without publishing it.
- Inspected the package archive and nuspec metadata.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 375 tests.
- `dotnet pack src\Loam\Loam.csproj -c Release --no-build` created `Loam.2.0.0.nupkg`.
- Package contents include `Loam.dll`, `Loam.xml`, `README.md`, and `Loam.nuspec`; nuspec reports version `2.0.0`, MIT license expression, repository metadata, project URL, and only the Avalonia dependency.
- `npm run docs:build` passed; `npm ci` was not needed because `docs/node_modules` was already present.
- Generated package/docs output stayed ignored and did not appear in `git ls-files --others --exclude-standard`.

**Next:** relaunch the Release gallery for a final visual pass; if that passes, the next step is an explicit release commit/push/publish decision.

---

## 2026-06-07 — v2.0 — Tooltip acceptance panel

**Done**
- Added Tooltip as a dedicated `Feedback / Tooltip` gallery page instead of burying it inside Menu samples.
- Extended `TooltipOptions` with placement, offsets, show-delay, between-show-delay, show-on-disabled, and service-enabled settings while preserving `Tooltip.Set(...)`.
- Added `Tooltip.Clear(...)` to remove the attached tooltip and automation help text.
- Removed Tooltip from the Menu page expected component metadata so Menu acceptance stays focused on menu behavior.
- Added Tooltip regression tests for tokenized `Paper` content, title/padding/elevation/help text, attached placement/delay/disabled-service options, and clear behavior.
- Added gallery acceptance checks for the Tooltip page and updated the component adaptation audit.

**Verified**
- Initial Release build was blocked by stale gallery PID `88736` locking `Loam.dll`; stopped the process and reran.
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Tooltip/gallery acceptance tests passed: 6 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 375 tests.

**Next:** relaunch Release gallery and visually check `Feedback / Tooltip`; if it passes, continue with the next visible issue or release-readiness gate.

---

## 2026-06-07 — v2.0 — ToggleGroup button-segment fix

**Done**
- Rebuilt `ToggleGroup` to render each option as a real Loam `Button` segment instead of custom borders, labels, state layers, and a bespoke equal-segment panel.
- Reused the working connected-button approach from `ButtonGroup`: merged adjacent borders, pill outer corners, intrinsic strip measurement, and minimum width/height enforcement under constrained parents.
- Updated ToggleGroup regression tests to assert real button segments, selected/outlined variants, size/typography propagation, automation help text, disabled keyboard suppression, and constrained-parent rendering.
- Updated the component adaptation audit so ToggleGroup no longer claims the old custom segment-panel implementation.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused `InputTests` passed: 83 tests.
- Focused ButtonGroup/gallery acceptance tests passed: 39 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 374 tests.
- Release gallery launched for visual verification, PID `63212`.

**Next:** visually confirm `ToggleGroup` on its component page and on `Start / Sizes`; if it passes, continue with Tooltip acceptance.

---

## 2026-06-07 — v2.0 — Sizes page layout fix

**Done**
- Fixed `ToggleGroup` itself to compute and enforce intrinsic group minimum width/height from segment metrics, so valid labels do not clip under constrained parents.
- Changed `ToggleGroup` to use a dedicated equal-segment layout panel across each group, so selected segments do not squeeze neighboring labels.
- Added constrained-parent and equal-segment-width regression tests for extra-large `ToggleGroup` labels.
- Reworked the `Start / Sizes` gallery page so each component family renders in a named wrapped sample lane instead of a cramped row of fixed cells.
- Removed the framed sample-card treatment from size lanes so controls such as `ButtonGroup` render like their component pages instead of inside extra chrome.
- Replaced the size lanes with a stable five-column comparison matrix, `Auto` columns, and local horizontal scrolling, so the page is driven by component desired sizes without fake component chrome.
- Added `ToggleGroup` to the Sizes page expected component metadata.
- Tightened gallery acceptance checks for the size matrix header plus `ButtonGroup` and `ToggleGroup` rows.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Input/gallery tests passed: 118 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 374 tests.
- Release gallery launched for visual verification, latest PID `91788`.

**Next:** visually verify `Start / Sizes`, then continue with Tooltip acceptance.

---

## 2026-06-07 — v2.0 — ToggleGroup size clipping fix

**Done**
- Fixed `ToggleGroup` segment sizing to measure actual label text instead of using approximate glyph widths.
- Updated the gallery size matrix so size-aware controls use minimum cells and can wrap instead of clipping wide valid controls.
- Tightened the `ToggleGroup` unclipped-label regression test across all five sizes with the Day/Week/Month sample.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Input/gallery tests passed: 118 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 372 tests.
- Release gallery launched for visual verification, PID `73468`.

**Next:** visually verify the `ToggleGroup` size matrix, then continue with Tooltip acceptance.

---

## 2026-06-07 — v2.0 — Snackbar service acceptance

**Done**
- Hardened `SnackbarService` so action, dismiss, Escape, timer, and queue trimming all share one idempotent cleanup path.
- Queue trimming now invokes the toast cleanup callback instead of raw-removing visuals, so old timers are stopped deterministically.
- Added snackbar automation help text for stack position, Escape dismissal, action text, and dismiss text.
- Expanded the SnackbarService gallery panel with standard, action, persistent, positioned, and queue-limit examples.
- Updated overlay docs, gallery acceptance checks, focused component tests, and the component adaptation audit.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused SnackbarService/gallery tests passed: 63 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 372 tests.
- Release gallery launched for visual verification, PID `87144`.

**Next:** visually verify SnackbarService in the Release gallery, then continue with Tooltip acceptance.

---

## 2026-06-07 — v2.0 — Dialog service acceptance

**Done**
- Hardened `DialogInstance.Close` so repeated close attempts keep the first result and dismiss only once.
- Extended `DialogOptions` with minimum width, maximum height, outer margin, and autofocus while preserving existing options.
- Added dialog/backdrop/modal-layer automation names and help text for Escape and scrim dismissal behavior.
- Moved built-in confirm/message-box actions to text-button styling and scheduled first enabled child focus after attach.
- Expanded the DialogService gallery panel with confirm, message box, custom sized content, and persistent dismissal examples.
- Updated overlay docs, gallery acceptance checks, focused component tests, and the component adaptation audit.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused DialogService/gallery tests passed: 62 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 371 tests.
- Release gallery launched for visual verification, PID `31676`.

**Next:** visually verify DialogService in the Release gallery, then continue with SnackbarService acceptance.

---

## 2026-06-07 — v2.0 — Menu popup row acceptance

**Done**
- Hardened `Menu` so disabled triggers do not open and repeated opens replace the previous flyout cleanly.
- Added open/closed automation help text, enabled-only first-row focus, Escape dismissal, and Up/Down keyboard navigation that skips disabled rows.
- Rendered `ShortcutText` as trailing row content instead of secondary body text.
- Added tokenized pressed state feedback to `ListItem`, preserving hover/focus/selected state ordering.
- Expanded the Menu gallery panel with filled, outlined, persistent, disabled, divider, shortcut, disabled-row, and tooltip examples.
- Updated Menu docs, gallery acceptance checks, focused component tests, and the component adaptation audit.

**Verified**
- `git diff --check` passed before full verification.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Menu/gallery tests passed: 7 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 371 tests.

**Next:** visually verify Menu in the Release gallery, then continue with DialogService acceptance.

---

## 2026-06-07 — v2.0 — Popover live acceptance

**Done**
- Hardened `Popover` open/closed automation help text and Escape closing from the control/surface.
- Suppressed trigger toggling while the trigger or popover is disabled, including direct disabled `Open = true` attempts.
- Reattached trigger handlers when a popover re-enters the visual tree.
- Lowered the generated popover surface to compact popup elevation and shape tokens.
- Rebuilt the gallery panel with live trigger, open/close, disabled-trigger, and controlled-open button examples using direct component APIs.
- Updated popover docs, gallery acceptance, focused component tests, and the component adaptation audit.

**Verified**
- `git diff --check` passed before verification.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Popover/gallery tests passed: 7 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 369 tests.

**Next:** visually verify Popover in the Release gallery, then continue with Menu popup row/hover/focus acceptance.

---

## 2026-06-07 — v2.0 — Overlay live acceptance

**Done**
- Hardened `Overlay` automation help text for visible/hidden and auto-close/manual modes.
- Suppressed scrim and Escape auto-close callbacks while the overlay is disabled.
- Rebuilt the gallery panel with separate live light auto-close, dark auto-close, manual close, and disabled auto-close examples.
- Updated overlay docs, gallery acceptance, and the component adaptation audit.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused overlay/gallery tests passed: 25 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 367 tests.

**Next:** visually verify Overlay in the Release gallery, then continue with Popover.

---

## 2026-06-07 — v2.0 — Collapse reduced-motion acceptance

**Done**
- Hardened `Collapse` so disabled, `Animated = false`, and zero-duration states resolve immediately without a height transition.
- Updated automation help text to expose both expanded/collapsed and animated/static state.
- Expanded the `Collapse` gallery panel with live animated, static, custom-duration, and zero-duration toggles, plus a disabled-static example.
- Updated docs, gallery acceptance tests, and the component adaptation audit.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Collapse/gallery tests passed: 9 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 366 tests.

**Next:** visually verify Collapse in the Release gallery, then continue with the next surface/feedback acceptance pass.

---

## 2026-06-07 — v2.0 — Progress sample cleanup and Skeleton acceptance

**Done**
- Removed leaked `CircularCase` and `LinearCase` gallery helpers from source-linked progress
  samples so copyable code shows direct component construction.
- Hardened `Skeleton` with public size metrics, disabled/static shimmer suppression, and refreshed
  automation help text when `Animate` or `IsEnabled` changes.
- Expanded the Skeleton gallery panel with presets, composition, five-size, static, disabled, and
  custom circular examples using public component APIs.
- Added Skeleton to the global Sizes page and updated docs plus the component audit.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused progress helper/Skeleton/gallery tests passed: 17 tests across the two focused runs.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 365 tests.

**Next:** visually verify ProgressCircular, ProgressLinear, Skeleton, and Sizes in the Release
gallery, then continue with Collapse.

---

## 2026-06-07 — v2.0 — ProgressLinear size and state acceptance

**Done**
- Added `ProgressLinear.Size` with five-size track metrics and named track/fill anatomy.
- Tokenized active, track, and disabled brushes in the control and made disabled indeterminate
  rendering static instead of animated.
- Expanded the ProgressLinear gallery panel with determinate, indeterminate, custom value text,
  disabled, disabled-indeterminate, and all five size examples using source-linked C#.
- Added `ProgressLinear` to the global Sizes page and updated docs plus the component audit.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused ProgressLinear/gallery tests passed: 7 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 363 tests.

**Next:** visually verify ProgressLinear in the Release gallery, then continue with Skeleton.

---

## 2026-06-06 — v2.0 — ProgressCircular size and state acceptance

**Done**
- Rebased `ProgressCircular` to a 48 px medium baseline with five-size diameter coverage and
  size-resolved default stroke width.
- Added disabled static rendering with disabled-role brushes and no indeterminate animation while
  disabled.
- Expanded the ProgressCircular gallery panel with states, all five sizes, disabled, custom value
  text, and static/custom-stroke examples using source-linked C#.
- Updated ProgressCircular docs and component adaptation audit notes.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused ProgressCircular/gallery tests passed: 5 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 360 tests.

**Next:** visually verify ProgressCircular in the Release gallery, then continue with ProgressLinear.

---

## 2026-06-06 — v2.0 — Alert close affordance acceptance

**Done**
- Replaced Alert's generated close affordance internals with a real `IconButton`, preserving
  `Closeable`, `CloseIcon`, `Close()`, and `Closed`.
- Ensured generated close actions have a proper hit target, focusability, automation name/help text,
  and button-family state feedback.
- Added disabled close suppression coverage and expanded the Alert gallery panel with a disabled state.
- Refreshed Alert docs to describe generated title, message, action, close, and compatibility content
  paths.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Alert/gallery tests passed: 4 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 359 tests.

**Next:** visually verify Alert in the Release gallery, then continue with ProgressCircular.

---

## 2026-06-06 — v2.0 — MonthCalendar keyboard and state acceptance

**Done**
- Added day-cell arrow and PageUp/PageDown keyboard navigation, including focus movement across
  displayed month boundaries while respecting min/max disabled bounds.
- Clipped day-cell state layers to the circular 40px target and removed the inactive month dropdown
  chevron from the header.
- Added automation names/help text for previous/next month actions.
- Expanded the MonthCalendar gallery panel to show selected, range, and constrained states with
  source-linked C#.
- Updated picker docs to describe first-day-of-week, public month navigation, keyboard navigation,
  range states, and disabled day behavior.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused MonthCalendar/gallery tests passed: 8 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 359 tests.

**Next:** visually verify MonthCalendar in the Release gallery, then continue with the next
component panel.

---

## 2026-06-06 — v2.0 — DatePicker generated API consistency

**Done**
- Added `PickerTitle`, `CancelText`, `OkText`, `OpenPicker()`, `ClosePicker()`, `Clear()`, and
  `DateSelected` to `DatePicker` so it matches the generated picker-family API pattern.
- Hardened disabled DatePicker behavior so pointer, keyboard activation, and `OpenPicker()` no longer
  open the generated calendar flyout when disabled.
- Updated the DatePicker gallery panel and picker docs to show generated action labels and the pending
  Cancel/OK commit workflow.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused DatePicker/gallery tests passed: 6 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 357 tests.

**Next:** visually verify DatePicker in the Release gallery, then continue with the next
picker/control panel.

---

## 2026-06-06 — v2.0 — TimePicker commit actions acceptance

**Done**
- Replaced the generated TimePicker popup's single Close action with Cancel and OK actions.
- Changed hour/minute row selection to update pending popup state first; OK commits to `Time` and
  raises `TimeSelected`, while Cancel closes without changing the committed value.
- Added `PickerTitle`, `CancelText`, `OkText`, `OpenPicker()`, `ClosePicker()`, `Clear()`, and disabled
  open suppression so TimePicker matches the picker-family API pattern.
- Updated the TimePicker gallery panel and picker docs to show generated action labels and the pending
  commit workflow.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused TimePicker/gallery tests passed: 7 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 356 tests.

**Next:** visually verify TimePicker in the Release gallery, then continue with the next
picker/control panel.

---

## 2026-06-06 — v2.0 — DateRangePicker disabled interaction acceptance

**Done**
- Hardened `DateRangePicker` disabled behavior so pointer, keyboard activation, and `OpenPicker()`
  no longer open the generated flyout when the control is disabled.
- Kept programmatic range updates intact and kept the existing pending-selection OK/Cancel workflow.
- Refreshed DateRangePicker docs to describe variants, picker title/actions, open/close/clear methods,
  `RangeSelected`, pending commit behavior, and disabled opening behavior.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused DateRangePicker/gallery tests passed: 7 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 354 tests.

**Next:** visually verify DateRangePicker in the Release gallery, then continue with the next
picker/control panel.

---

## 2026-06-06 — v2.0 — ColorPicker palette interaction acceptance

**Done**
- Hardened `ColorPicker` so disabled controls suppress pointer, keyboard, and `OpenPicker()` flyout
  opening while preserving programmatic value updates.
- Made generated palette swatches focusable, automation-named, keyboard selectable, and backed by
  tokenized hover/focus/pressed state layers.
- Refreshed ColorPicker picker docs to describe variants, custom palettes, open/close methods, value
  change notification, and disabled opening behavior without implying hand-built internals.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused ColorPicker/gallery tests passed: 10 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 353 tests.

**Next:** visually verify ColorPicker in the Release gallery, then continue with the next
picker/control panel.

---

## 2026-06-06 — v2.0 — Form generated-state acceptance

**Done**
- Hardened generated `Form` submit/reset actions with named action parts, disabled-state propagation,
  disabled handler suppression, and action help text.
- Expanded the Form gallery panel to show generated default, validation-error, ready, disabled, and
  five action-size states with source-linked C#.
- Updated Form docs for spacing, action styling, alignment, submit/reset events, and disabled
  generated-action behavior.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Form/gallery tests passed: 13 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 351 tests.

**Next:** visually verify the Form panel in the Release gallery, then continue with ColorPicker as
the next picker/input component panel.

---

## 2026-06-06 — v2.0 — FileUpload size and disabled acceptance

**Done**
- Hardened generated `FileUpload` selection UI so selected-file chips and the generated clear action
  inherit `FileUpload.Size`.
- Disabled generated picker, chip remove, and clear actions when `FileUpload.IsEnabled` is false
  while preserving programmatic `ShowSelection()` and `Clear()` behavior.
- Expanded the FileUpload gallery panel with explicit five-size coverage and updated docs for the
  public file upload API surface.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused FileUpload/gallery tests passed: 7 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 350 tests.

**Next:** visually verify the FileUpload panel in the Release gallery, then continue with Form as
the next input component panel.

---

## 2026-06-06 — v2.0 — ToggleGroup state-layer and size acceptance

**Done**
- Added `ToggleGroup.Size` and projected all five size values through the shared button-family
  density and typography tokens.
- Rebuilt ToggleGroup segments with named `PART_Segment` and clipped `PART_StateLayer` surfaces
  for hover, focus, pressed, and keyboard activation feedback.
- Added disabled interaction suppression, selected/unselected segment help text, dynamic group
  automation help text, and group-level keyboard navigation.
- Fixed segment text rendering and alignment regressions by using stable native text measurement,
  segment-specific padding, no label trimming/wrapping, and size-aware segment/text minimum widths
  so labels cannot collapse or clip in the gallery or consumer apps.
- Expanded the ToggleGroup gallery panel and the global Sizes page to show all five sizes.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused ToggleGroup tests passed: 6 tests, including segment label/width/clipping regression coverage.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 348 tests.

**Next:** visually verify the ToggleGroup panel in the Release gallery, then continue with
FileUpload as the next input component.

---

## 2026-06-06 — v2.0 — Rating state-layer acceptance

**Done**
- Rebuilt `Rating` stars as stable 40 px interaction targets with centered icons and per-star
  state layers for hover, focus, pressed, and keyboard activation feedback.
- Added dynamic rating automation help text plus selected/unselected help text for individual
  star targets.
- Hardened disabled and read-only paths so keyboard changes and state-layer feedback are
  suppressed without changing the public `Rating` API.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Rating tests passed: 4 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 344 tests.

**Next:** visually verify the Rating panel in the Release gallery, then continue with ToggleGroup
as the next selection panel.

---

## 2026-06-06 — v2.0 — Slider state-layer acceptance

**Done**
- Added a named thumb-following `Slider` state layer so hover/focus/pressed feedback appears at
  the active handle position.
- Added value/range automation help text that refreshes when `Value`, `Minimum`, or `Maximum`
  changes.
- Added regression coverage for focus feedback, disabled state-layer clearing, disabled keyboard
  suppression, and value/range automation.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Slider tests passed: 4 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 342 tests.

**Next:** visually verify the Slider panel in the Release gallery, then continue with Rating as
the next selection panel.

---

## 2026-06-06 — v2.0 — Radio and RadioGroup state-layer acceptance

**Done**
- Added a named mark-centered `Radio` state layer so hover/focus/pressed feedback appears around
  the radio ring without changing label spacing.
- Added content/value automation names and selected/unselected help text for `Radio`.
- Hardened `RadioGroup` with focusability, stable automation help text, child rewiring after
  content changes, and deterministic value-to-radio synchronization.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Radio/RadioGroup tests passed: 5 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 340 tests.

**Next:** visually verify the Radio and RadioGroup panels in the Release gallery, then continue
with Slider as the next selection panel.

---

## 2026-06-06 — v2.0 — Switch state-layer acceptance

**Done**
- Added a named thumb-following `Switch` state layer so hover/focus/pressed feedback moves with
  the handle while preserving the existing track/label layout.
- Bound selected feedback through existing palette state tokens and unselected feedback through the
  neutral on-surface state token, with disabled state-layer suppression.
- Added automation naming/help text from switch content and on/off state, plus endpoint size
  coverage for extra-small through extra-large geometry.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Switch tests passed: 3 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 336 tests.

**Next:** visually verify the Switch panel in the Release gallery, then continue with Radio and
RadioGroup as the next selection panels.

---

## 2026-06-06 — v2.0 — CheckBox state-layer acceptance

**Done**
- Added a named mark-centered `CheckBox` state layer so hover/focus/pressed feedback appears around
  the checkbox glyph without widening the label spacing.
- Bound selected state feedback through existing palette state tokens and unselected feedback through
  the neutral on-surface state token.
- Added automation naming/help text from checkbox content, with focused tests for focus feedback,
  disabled suppression, and content updates.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused CheckBox tests passed: 5 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 334 tests.

**Next:** visually verify the CheckBox panel in the Release gallery, then continue with Switch as
the next selection panel.

---

## 2026-06-06 — v2.0 — Autocomplete automation acceptance

**Done**
- Added root automation name/help text for `Autocomplete`, sourced from label/helper text and
  refreshed to error text when invalid.
- Added automation names/help text for suggestion rows so popup options expose stable accessible
  labels.
- Added regression coverage for root automation, suggestion-row automation, and error help text
  refresh.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Autocomplete tests passed: 14 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 332 tests.

**Next:** visually verify the Autocomplete panel in the Release gallery, then continue with Select
as the next input panel.

---

## 2026-06-06 — v2.0 — MaskedTextField live input masking

**Done**
- Hardened `MaskedTextField` live editor synchronization so focused text input is projected through
  the active mask pattern after Avalonia completes the keypress caret update.
- Kept the mask reentrancy guard exception-safe and normalized caret/selection to the end after
  inserted literals such as phone/date separators.
- Added regression coverage for live editor input, digit-by-digit keyboard typing, visible editor
  text, public `Text`, and caret state.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused MaskedTextField tests passed: 6 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 331 tests.

**Next:** visually verify live typing on the MaskedTextField page in the Release gallery, then
continue with Autocomplete as the next input panel.

---

## 2026-06-06 — v2.0 — MaskedTextField automation and gallery acceptance

**Done**
- Reworked the MaskedTextField gallery panel into a compact wrapping matrix of real
  `MaskedTextField` instances covering phone, postal code, date, access code, product key,
  partial input, error, and disabled states.
- Added MaskedTextField automation name/help text so the root exposes its label and mask pattern,
  including error text when the field is invalid.
- Added focused component and gallery acceptance coverage for mask projection, pattern automation,
  source-linked state samples, filled/text/outlined variants, secondary color, error, disabled, and
  corrected outlined notch spacing.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused MaskedTextField/gallery tests passed: 4 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 329 tests.

**Next:** visually verify the MaskedTextField page in the Release gallery, then continue with
Autocomplete as the next input panel.

---

## 2026-06-06 — v2.0 — NumericField interaction and gallery acceptance

**Done**
- Reworked the NumericField gallery panel into a compact wrapping matrix of real `NumericField`
  instances covering outlined, filled, text/underline, fractional step, max-bound, negative range,
  error, and disabled states.
- Hardened NumericField keyboard behavior so Up/Down adjust the value from the inner editor while
  preserving min/max clamping and step behavior.
- Added automation names/help text for the NumericField root and spinner controls, and disabled the
  spinner targets when the control is disabled.
- Added focused component and gallery acceptance coverage for keyboard adjustment, spinner
  automation, source-linked state samples, variants, formatting, bounds, error, disabled, and
  corrected outlined notch spacing.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused NumericField/gallery tests passed: 6 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 327 tests.

**Next:** visually verify the NumericField page in the Release gallery, then continue with
MaskedTextField as the next input panel.

---

## 2026-06-06 — v2.0 — TextField gallery acceptance

**Done**
- Reworked the TextField gallery panel into a compact wrapping matrix of real `TextField`
  instances covering outlined, filled, text/underline, adornments, always-floated label,
  read-only, required error, and disabled states.
- Kept the existing TextField public API unchanged because variants, label/helper/error text,
  validation flags, adornments, read-only, disabled, shrink-label, and placeholder behavior already
  cover the needed anatomy.
- Added focused gallery acceptance coverage proving the panel uses real TextField labels and state
  properties, and that outlined floated labels keep the corrected notch spacing.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused TextField/gallery tests passed: 11 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 325 tests.

**Next:** visually verify the TextField page in the Release gallery, then continue with
NumericField as the next input panel.

---

## 2026-06-06 — v2.0 — Field gallery acceptance

**Done**
- Expanded the Field gallery panel with real custom content states for phone entry, accent
  swatch, grouped options, text/underline quick filter, error amount, and disabled token display.
- Kept the existing Field public API unchanged because label, helper, error, variant, adornments,
  inner padding, content, and disabled state already cover the needed anatomy.
- Tightened focused component and gallery acceptance coverage for outlined notch spacing,
  source-linked sample content, filled/text/outlined variants, error, disabled, adornments, and
  chromeless focused inner TextBox behavior.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused Field/gallery tests passed: 2 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 324 tests.

**Next:** visually verify the Field page in the Release gallery, then continue with the next input
or picker page that still needs richer component-panel acceptance.

---

## 2026-06-06 — v2.0 — TimePicker gallery ergonomics

**Done**
- Updated the TimePicker gallery panel so state labels such as Outlined, Filled,
  Text / underline, Empty, Selected, 24-hour format, Minute step, Error, and Disabled are real
  `TimePicker.Label` values instead of external sample text wrappers.
- Kept the existing TimePicker public API unchanged because it already exposes label, helper, error,
  variant, time format, minute step, selected time, and disabled state.
- Added focused component and gallery acceptance coverage for label/helper/error rendering,
  automation naming, source-linked state samples, and rejection of the old wrapper labels.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused TimePicker/gallery tests passed: 5 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 324 tests.

**Next:** visually verify the TimePicker page in the Release gallery, then continue with the next
input or picker page that still shows sample-only anatomy.

---

## 2026-06-06 — v2.0 — Field floating-label alignment

**Done**
- Fixed shared outlined field floating-label alignment by reserving top notch space on the outlined
  field border while keeping the label inside the control's layout bounds.
- Restored the positive floating-label top offset so labels are not clipped, while filled and text
  variants keep non-notched label positioning.
- Added focused TextField and DatePicker coverage so field-style controls keep the corrected
  floating-label position.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused field/picker/theme tests passed: 5 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 323 tests.

**Next:** visually verify DatePicker and TextField labels in the Release gallery, then continue
with TimePicker gallery ergonomics.

---

## 2026-06-06 — v2.0 — DatePicker gallery ergonomics

**Done**
- Updated the DatePicker gallery panel so state labels such as Outlined, Filled,
  Text / underline, Empty, Selected, Error, and Disabled are real `DatePicker.Label`
  values instead of external sample text wrappers.
- Kept the existing DatePicker public API unchanged because it already exposes label, helper,
  error, variant, date format, constraints, and pending commit/cancel behavior.
- Added focused component and gallery acceptance coverage for label/helper/error rendering,
  automation naming, source-linked state samples, and rejection of the old wrapper labels.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused DatePicker/gallery tests passed: 5 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 323 tests.

**Next:** visually verify the DatePicker page in the Release gallery, then continue with TimePicker
because it still has external sample labels in its state matrix.

---

## 2026-06-06 — v2.0 — DateRangePicker gallery ergonomics

**Done**
- Updated the DateRangePicker gallery panel so state labels such as Outlined, Filled,
  Text / underline, Empty, Selected, Error, and Disabled are real `DateRangePicker.Label`
  values instead of external sample text wrappers.
- Kept the existing DateRangePicker public API unchanged because it already exposes label, helper,
  error, variants, constraints, custom action text, and pending range commit/cancel behavior.
- Added focused component and gallery acceptance coverage for label/helper/error rendering,
  automation naming, source-linked state samples, and rejection of the old wrapper labels.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused DateRangePicker/gallery tests passed: 6 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 322 tests.

**Next:** visually verify the DateRangePicker page in the Release gallery, then continue with the
next picker/input page that still uses external sample labels or helper text.

---

## 2026-06-06 — v2.0 — ColorPicker gallery ergonomics

**Done**
- Updated the ColorPicker gallery panel so state labels such as Outlined, Filled, Text, Alpha,
  Error, and Disabled are real `ColorPicker.Label` values instead of external sample text wrappers.
- Kept the existing ColorPicker public API unchanged because it already exposes label, helper, error,
  alpha, value, variant, and custom palette anatomy.
- Added focused component and gallery acceptance coverage for label/helper/error rendering,
  automation naming, and source-linked state samples.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused ColorPicker/gallery tests passed: 8 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 321 tests.

**Next:** visually verify the ColorPicker page in the Release gallery, then continue with the next
picker/input page that still uses external sample labels or helper text.

---

## 2026-06-06 — v2.0 — FileUpload API ergonomics

**Done**
- Added generated `FileUpload` label, helper text, empty status text, selected status format, and
  configurable upload button icon.
- Updated the FileUpload gallery panel so labels/status/helper text live inside `FileUpload`
  instances instead of external sample stacks.
- Updated input docs for the generated FileUpload anatomy, remove/clear events, and selected status
  formatting.
- Added focused component and gallery acceptance coverage for label/helper/status rendering, button
  icon binding, removable chips, clear action, and state variants.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused FileUpload/gallery tests passed: 5 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 320 tests.

**Next:** visually verify the FileUpload page in the Release gallery, then continue the
component-by-component ergonomics sweep with ColorPicker or the next input component whose sample still
builds labels/status externally.

---

## 2026-06-06 — v2.0 — Form API ergonomics

**Done**
- Added generated `Form` title/subtitle, helper text, success/error status text, and submit/reset
  action icons while preserving raw `Child` compatibility.
- Updated generated submit/reset behavior so validation and reset refresh the built-in status line.
- Updated the Form gallery panel and input docs to use generated form anatomy instead of external
  title/status stacks.
- Tightened focused form and gallery acceptance coverage for generated anatomy, status switching,
  action icons, field metrics, and reset behavior.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused form/gallery tests passed: 2 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 319 tests.

**Next:** visually verify the Form page in the Release gallery, then continue the
component-by-component ergonomics sweep with the next input component whose sample still builds core
anatomy outside the control.

---

## 2026-06-06 — v2.0 — Drawer generated content overlap fix

**Done**
- Wrapped generated `Drawer.Items` navigation in a vertical scroll viewport so compact drawer frames
  cannot paint nav rows under the generated footer.
- Increased the Drawer gallery preview frames so title, subtitle, navigation rows, and footer text
  have enough room in the standard sample.
- Added a focused regression test for generated drawer header/nav/footer layout in a compact height.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused shell/gallery tests passed: 22 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 319 tests.

**Next:** visually verify the Drawer page in the Release gallery, then continue with form/input
composition ergonomics if the shell samples pass.

---

## 2026-06-06 — v2.0 — Shell API ergonomics

**Done**
- Added generated `Drawer` title, subtitle, and footer text anatomy while preserving custom
  `Header`, `Footer`, and raw `Content` compatibility.
- Added generated `MainContent` page header anatomy with title, subtitle, custom header, custom
  header actions, primary/secondary action text, action events, and automation help text.
- Updated Layout, Drawer, and MainContent gallery panels to use generated shell APIs instead of
  hand-built header/title/footer rows.
- Added focused shell and gallery acceptance coverage for drawer header/footer generation,
  mini-drawer hiding, main-content header actions, and generated shell samples.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused shell/gallery tests passed: 22 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 318 tests.

**Next:** visually verify Layout, Drawer, and MainContent in the Release gallery, then continue the
component-by-component ergonomics sweep with the next component whose sample still builds core anatomy
outside the control.

---

## 2026-06-06 — v2.0 — Surface container API ergonomics

**Done**
- Added generated `Paper` anatomy for title, subtitle, body, compact padding, semantic color, and
  shape tokens while preserving custom `Content` compatibility.
- Added generated `Card` anatomy for body text plus primary/secondary action text and click events,
  so standard cards no longer require hand-built internal rows.
- Updated the Paper and Card gallery panels to use the generated APIs in source-linked code.
- Added focused component and gallery acceptance coverage for generated surface/card anatomy, square
  surfaces, no cast shadows, generated actions, and automation text.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed with zero warnings.
- Focused surface/gallery tests passed: 29 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 316 tests.

**Next:** visually verify Paper and Card in the Release gallery, then continue the
component-by-component ergonomics sweep with shell surfaces or the next page that still forces users
to assemble component internals by hand.

---

## 2026-06-06 — v2.0 — Loading feedback API ergonomics

**Done**
- Added generated `ProgressLinear` label/value text support with value formatting, explicit value text,
  and automation help text.
- Added generated `ProgressCircular` label/value text support, centered determinate value rendering, and
  automation help text.
- Added `SkeletonPreset`, `Skeleton.Preset`, `Size`, `Label`, and static factories for text lines,
  avatars, buttons, thumbnails, and cards while preserving the old custom `Width`/`Height`/`Circle`
  path.
- Updated ProgressCircular, ProgressLinear, and Skeleton gallery panels to use the generated APIs in
  source-linked code.
- Added focused component and gallery acceptance coverage for the loading APIs.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- Focused loading/gallery tests passed: 16 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 314 tests.

**Next:** visually verify ProgressCircular, ProgressLinear, and Skeleton in the Release gallery, then
continue the component-by-component ergonomics sweep with surface containers (`Paper`/`Card`) or the
next gallery page that still feels hand-built.

---

## 2026-06-06 — v2.0 — Feedback API ergonomics

**Done**
- Added generated `Alert` anatomy for title, message, trailing action, and close affordance while
  preserving raw `Content` compatibility.
- Added `Alert.Closeable`, `CloseIcon`, `Action`, `Title`, `Message`, `Close()`, and `Closed` so
  standard alert layouts do not require hand-built rows.
- Updated the Alert gallery panel to show generated anatomy, action, closeable alerts, variants,
  semantic colors, and the raw content compatibility path in source-linked code.
- Added focused alert component and gallery acceptance coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- Focused alert/gallery tests passed: 13 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 310 tests.

**Next:** visually verify the Alert page in the Release gallery, then continue the component-by-component
ergonomics sweep with the next weakest feedback/loading component API.

---

## 2026-06-06 — v2.0 — Data display API ergonomics

**Done**
- Added generated `TimelineItem` anatomy for title, subtitle, time/metadata text, and semantic color
  while preserving custom `Content`.
- Added generated `CarouselItem` slide anatomy for title, subtitle, and semantic color while preserving
  custom `Content`.
- Added `Carousel.GoTo` and `SelectedIndexChanged` so carousel navigation can be driven without
  reaching into internal chrome.
- Added `ExpansionPanels.AddPanel`, `ExpandPanel`, `CollapsePanel`, `ExpandAll`, and `CollapseAll`
  helpers for common accordion/multi-expansion workflows.
- Updated Timeline, Carousel, and ExpansionPanels gallery samples to use the new component APIs in
  source-linked code.
- Added focused data display and gallery acceptance coverage for the new APIs.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- Focused data/gallery tests passed: 18 tests.
- `git diff --check` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 308 tests.

**Next:** visually verify Timeline, Carousel, and ExpansionPanels in the Release gallery, then continue
the component-by-component ergonomics sweep with the next control family that still needs generated
anatomy or simpler public hooks.

---

## 2026-06-06 — v2.0 — Input and picker API ergonomics

**Done**
- Added `FileUpload` accept filters, selected-file chip removal, optional clear action, and removal/clear
  events while preserving the existing `ButtonText`, `AllowMultiple`, `Files`, and `ShowSelection` APIs.
- Added generated `Form` action styling knobs for size, submit/reset variants, colors, and action-row
  alignment so standard forms do not need custom action footers.
- Added `ColorPicker.Palette`, `OpenPicker`, `ClosePicker`, and value-change notification; an empty
  palette continues to use the built-in default colors.
- Changed `DateRangePicker` to use pending selection with generated cancel/OK actions before committing
  `Start`/`End`, and added title/action text plus open/close/clear helpers.
- Added `MonthCalendar.FirstDayOfWeek`, `PreviousMonth`, and `NextMonth`.
- Updated FileUpload, Form, ColorPicker, DateRangePicker, and MonthCalendar gallery panels to show the
  new real component APIs in source-linked samples.
- Added focused component and gallery acceptance tests for the new behavior.

**Verified**
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- Focused input/picker/gallery tests passed: 26 tests.
- `git diff --check` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 305 tests.

**Next:** visually verify FileUpload, Form, ColorPicker, DateRangePicker, and MonthCalendar in the
Release gallery, then continue the component-by-component API ergonomics sweep with the next weakest
remaining sample/component family.

---

## 2026-06-06 — v2.0 — Overlay component API ergonomics

**Done**
- Added menu row options for disabled rows, divider rows, shortcut text, menu width, and public
  `OpenMenu`/`CloseMenu` methods.
- Added `Popover.Trigger` so standard trigger-driven popovers no longer require manual click wiring.
- Added snackbar positioning and explicit dismiss button options through `SnackbarPosition` and
  `SnackbarOptions`.
- Added `TooltipOptions` for title, elevation, padding, typography, color, and help text while keeping
  `Tooltip.Set(control, text)` compatible.
- Added dialog options for Escape dismissal, max width, and padding.
- Updated Menu, Popover, DialogService, SnackbarService, and tooltip gallery samples to use the new
  component APIs.
- Added focused overlay component and gallery acceptance coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~OverlayTests|FullyQualifiedName~Overlay_gallery_pages_use_component_options"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 23 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 301 tests.

**Next:** visually verify Menu, Popover, DialogService, SnackbarService, and tooltip behavior, then
continue with input/picker API ergonomics for `FileUpload`, `Form`, `ColorPicker`, and range/calendar
helpers.

---

## 2026-06-06 — v2.0 — Drawer generated-content toggle crash fix

**Done**
- Fixed generated `Drawer` content so `Open` toggles no longer rebuild header/footer content.
- Changed generated drawer updates to retain the generated surface and update nav/header/footer in
  place, avoiding already-parented control crashes when header/footer are controls.
- Added regression coverage for temporary drawer open/close toggles, mini rebuilds with header/footer,
  and the gallery Drawer page's `Toggle temporary` button.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~Drawer_toggle_open|FullyQualifiedName~Drawer_generated_content_rebuild|FullyQualifiedName~Shell_gallery_pages_use_generated_component_apis"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 3 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 295 tests.

**Next:** retry the Drawer gallery toggle visually, then continue with overlay/component API
ergonomics: `Menu`, `Popover`, `DialogService`, `SnackbarService`, and `Tooltip`.

---

## 2026-06-06 — v2.0 — Shell component API ergonomics

**Done**
- Added `AppBar.Subtitle` so standard generated app bars can show a two-line title stack without
  custom toolbar grids.
- Extended `AppBarAction` with `IsEnabled`, `Variant`, `Color`, and `Size` for generated trailing
  actions.
- Added stable shell automation names for `Layout` and `MainContent`.
- Added `DrawerItem.IsEnabled`, `Label`, and `Color`, plus `NavLink.Label`, so generated drawer
  items support disabled states and accessible mini-drawer labels.
- Updated Shell/Layout, AppBar, Drawer, and MainContent gallery samples to use generated component
  APIs, including navigation toggle callbacks, subtitles, disabled actions, disabled drawer items,
  footer/header content, and mini labels.
- Added focused shell API and gallery acceptance coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~ShellTests|FullyQualifiedName~Shell_gallery_pages_use_generated_component_apis|FullyQualifiedName~MainContent_gallery_sample_shows_shell_context"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 18 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 293 tests.

**Next:** visually verify the shell pages, then continue with overlay/component API ergonomics:
`Menu`, `Popover`, `DialogService`, `SnackbarService`, and `Tooltip`.

---

## 2026-06-06 — v2.0 — Timeline orientation and carousel automation fix

**Done**
- Added `Timeline.Orientation` with vertical default and horizontal rendering support.
- Added horizontal Timeline gallery coverage with scrollable overflow for narrow surfaces.
- Added `Carousel.AutoPlay` and `Carousel.AutoPlayInterval` so auto-advancing slides are a first-class
  component capability.
- Fixed Carousel previous/next arrow activation by wiring real button click events instead of relying
  on pointer handling for template buttons.
- Added auto-play, arrow-click, horizontal-timeline, and gallery acceptance coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~Carousel_prev_next|FullyQualifiedName~Carousel_auto_play|FullyQualifiedName~Timeline_horizontal|FullyQualifiedName~Timeline_carousel_gallery_pages_show_data_motion_states"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 4 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 290 tests.

**Next:** visually verify Timeline horizontal mode and Carousel arrow/auto behavior, then continue
with shell component API ergonomics: `AppBar`, `Drawer`, `Layout`, and `MainContent`.

---

## 2026-06-06 — v2.0 — Timeline and carousel state coverage

**Done**
- Added `Timeline` empty-state rendering and item-count automation help text from construction time.
- Added timeline item-position help text and disabled opacity handling.
- Hardened `Carousel` disabled behavior so programmatic, keyboard, and bullet navigation are
  suppressed while disabled.
- Added `Carousel` empty-slide rendering, selected-slide automation help text, bullet selected help
  text, and chrome disabled state updates.
- Replaced the Timeline and Carousel gallery pages with component-specific panels for default,
  rich-content, empty, disabled, hidden-chrome, clamped-index, and selected-slide states.
- Added focused component and gallery acceptance coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~Carousel|FullyQualifiedName~Timeline|FullyQualifiedName~Timeline_carousel_gallery_pages_show_data_motion_states"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 6 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 287 tests.

**Next:** visually verify the Timeline and Carousel pages, then continue with shell component API
ergonomics: `AppBar`, `Drawer`, `Layout`, and `MainContent`.

---

## 2026-06-06 — v2.0 — Tabs and reveal control coverage

**Done**
- Hardened `Tabs` selected-index clamping for invalid indexes and item removal.
- Suppressed disabled tab header activation and added selected-tab automation help text.
- Updated `ExpansionPanels` automation so accordion and multi-expansion state changes stay current.
- Added `ExpansionPanel` and `Collapse` expanded/collapsed automation help text.
- Replaced the Tabs, ExpansionPanels, and Collapse gallery pages with component-specific panels for
  selected, secondary, clamped, disabled, empty, accordion, multi-expansion, static, animated, and
  custom-duration states.
- Added focused component and gallery acceptance coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~Tabs_clamp_selection|FullyQualifiedName~ExpansionPanels_multi_mode|FullyQualifiedName~ExpansionPanel_help_text|FullyQualifiedName~Collapse_exposes_automation_state|FullyQualifiedName~Tabs_expansion_collapse_gallery_pages_show_reveal_states"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 5 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 284 tests.

**Next:** visually verify the Tabs, ExpansionPanels, and Collapse pages, then continue with
secondary data motion panels: `Timeline` and `Carousel`.

---

## 2026-06-06 — v2.0 — Stepper and pagination workflow coverage

**Done**
- Added `Pagination` automation name and page-count help text.
- Hardened disabled `Pagination` behavior so direct page/arrow click events cannot change selection.
- Added selected-page automation naming and kept selected page clamping deterministic.
- Added `Stepper` automation name and step-position help text.
- Hardened `Stepper` active-index clamping for invalid indexes and step removal, disabled next/back
  navigation, and empty-step action state.
- Replaced the Stepper and Pagination gallery pages with component-specific panels for boundary,
  windowed, secondary, clamped, empty, disabled, active, completed, and invalid-index states.
- Added focused component and gallery acceptance coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~DataDisplayTests.Pagination|FullyQualifiedName~DataDisplayTests.Stepper|FullyQualifiedName~Stepper_pagination_gallery_pages_show_workflow_navigation_states"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 9 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 279 tests.

**Next:** visually verify the Stepper and Pagination pages, then continue with reveal/data
interaction panels: `Tabs`, `ExpansionPanels`, and `Collapse`.

---

## 2026-06-06 — v2.0 — Navigation primitive component panels

**Done**
- Added default automation naming for `Breadcrumbs` and `NavMenu`.
- Fixed `NavGroup` so disabled groups no longer toggle from keyboard or header activation paths.
- Replaced the navigation primitive gallery pages with component-specific panels covering breadcrumb
  separators, href/disabled breadcrumb items, link colors/underline/href/disabled states, simple and
  grouped menus, active/secondary/text-only/href/disabled nav links, and expanded/collapsed/disabled
  nav groups.
- Added focused navigation behavior and gallery acceptance coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~NavigationTests|FullyQualifiedName~Navigation_gallery_pages_show_component_specific_panels"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 12 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 276 tests.

**Next:** visually verify the Navigation pages, then continue the component-by-component pass with
data workflow controls: `Stepper` and `Pagination`.

---

## 2026-06-06 — v2.0 — Layout primitive component panels

**Done**
- Added default automation names for `Container`, `Grid`, `Item`, `Stack`, `Spacer`, `Hidden`, and
  `ScrollToTop`, including the default scroll-to-top FAB.
- Replaced the layout primitive gallery pages with component-specific panels for breakpoint caps,
  gutters, fixed/responsive grid spans, item breakpoint fallback, vertical/row/custom stacks,
  star/dock spacers, hidden breakpoint modes, and scroll-to-top behavior.
- Added layout and gallery acceptance coverage for automation naming, item span fallback/clamping,
  source-linked layout examples, and per-page component coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~LayoutTests|FullyQualifiedName~Layout_gallery_pages_show_component_specific_panels"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 16 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 273 tests.

**Next:** visually verify the Layout pages, then continue the component-by-component pass with
navigation primitives: `Breadcrumbs`, `Link`, `NavMenu`, `NavLink`, and `NavGroup`.

---

## 2026-06-06 — v2.0 — Text, icon, and divider display coverage

**Done**
- Added automation naming for `Text`, `Icon`, and `Divider`.
- Fixed `Text` and `Icon` color inheritance so switching from an explicit semantic color back to
  `Inherit` clears the local foreground binding instead of keeping the old color.
- Replaced the Text gallery page with display, headline, title, body, label, legacy alias, color,
  alignment, and wrapping examples.
- Replaced the Icon gallery page with color, five-size, and common-glyph examples.
- Replaced the Divider gallery page with horizontal/vertical, full-width, inset, middle, and light
  divider examples.
- Added primitive and gallery acceptance coverage for the new states.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~PrimitivesTests.Text|FullyQualifiedName~PrimitivesTests.Icon|FullyQualifiedName~PrimitivesTests.Divider|FullyQualifiedName~GalleryAcceptanceTests.Text_icon_divider"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 11 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 270 tests.

**Next:** visually verify Text, Icon, and Divider pages, then continue with layout primitives:
Container, Grid, Item, Stack, Spacer, Hidden, and ScrollToTop.

---

## 2026-06-06 — v2.0 — Badge and avatar display coverage

**Done**
- Added automation naming for `Badge`, `Avatar`, and `AvatarGroup` so passive display surfaces
  expose useful names in headless/accessibility checks.
- Replaced the Badge gallery page with values, dot, capped value, origins, overlap, bordered, and
  hidden badge states.
- Replaced the Avatar gallery page with separate variant, color, shape, icon, and five-size panels.
- Replaced the AvatarGroup gallery page with overflow, compact, relaxed spacing, rounded, square,
  and five-size group examples.
- Added component and gallery acceptance coverage for the new states.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~DisplayTests.Badge|FullyQualifiedName~DisplayTests.Avatar|FullyQualifiedName~GalleryAcceptanceTests.Badge_avatar_gallery"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 9 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 266 tests.

**Next:** visually verify Badge, Avatar, and AvatarGroup pages, then continue with Text, Icon, and
Divider display surfaces.

---

## 2026-06-06 — v2.0 — Chip sizing regression correction

**Done**
- Fixed the Chip template overlay so the state layer no longer makes chips measure as full-width
  controls inside wrapping gallery layouts.
- Added regression coverage that verifies chips stay content-sized inside a `WrapPanel`.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~DisplayTests.Chip|FullyQualifiedName~GalleryAcceptanceTests.Chip_gallery"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 10 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 262 tests.

**Next:** visually verify Chip and ChipSet again, then continue with Badge, Avatar, and AvatarGroup.

---

## 2026-06-06 — v2.0 — Chip and ChipSet state coverage

**Done**
- Added a token-driven state-layer overlay to `Chip`, with focus/hover/pressed feedback clipped to
  the chip shape and cleared when disabled.
- Replaced the Chip gallery page with a richer matrix covering variants, colors, icon, closeable,
  label shape, all five sizes, and disabled states.
- Replaced the ChipSet gallery page with single mandatory, multi-select, optional selection, and
  disabled set examples.
- Added component regression coverage for Chip state layers and gallery acceptance coverage for
  Chip and ChipSet panels.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~GalleryAcceptanceTests.Chip_gallery_pages_show_component_state_matrices|FullyQualifiedName~DisplayTests.Chip_focus_uses_token_state_layer_and_disabled_clears_it|FullyQualifiedName~DisplayTests.ChipSet"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 6 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 261 tests.

**Next:** visually verify Chip and ChipSet pages, then continue with the next display surfaces:
Badge, Avatar, and AvatarGroup.

---

## 2026-06-06 — v2.0 — Selection input gallery coverage

**Done**
- Replaced the CheckBox, Switch, Radio, RadioGroup, Slider, ToggleGroup, and Rating gallery pages
  with richer state panels covering selected/unselected, disabled, colors, grouping, range values,
  read-only/static states, and size matrices where the component exposes `Size`.
- Fixed CheckBox indeterminate rendering so the mixed state uses the selected fill with a dash mark
  instead of visually matching an unchecked box.
- Added gallery acceptance coverage for the selection input panels and regression coverage for the
  CheckBox indeterminate visual state.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~GalleryAcceptanceTests.Selection_input_gallery_pages_show_state_matrices|FullyQualifiedName~InputTests.CheckBox_indeterminate_uses_filled_box_and_dash_mark"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 2 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 259 tests.

**Next:** visually verify the selection input pages in the gallery, then continue with display
selection surfaces such as Chip and ChipSet.

---

## 2026-06-06 — v2.0 — Color picker and file upload coverage

**Done**
- Added `Variant` support to `ColorPicker` so it can render outlined, filled, and text/underline
  field chrome like the other field-based pickers.
- Added `Variant`, `Color`, and `Size` to `FileUpload` and bound those properties to the internal
  upload button, so users can style the component without rebuilding its internals.
- Replaced the ColorPicker and FileUpload gallery pages with copyable state matrices covering
  variants, selected/default values, alpha, multiple files, size examples, error where applicable,
  and disabled states.
- Added control-level and gallery acceptance coverage for the new states.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~PickerTests.ColorPicker|FullyQualifiedName~InputTests.FileUpload|FullyQualifiedName~GalleryAcceptanceTests.ColorPicker_gallery|FullyQualifiedName~GalleryAcceptanceTests.FileUpload_gallery"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 9 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 257 tests.

**Next:** visually verify ColorPicker and FileUpload in the gallery, then continue with remaining
input components that still need richer state panels.

---

## 2026-06-06 — v2.0 — Time picker selected row style correction

**Done**
- Fixed TimePicker popup row state layers so hover/focus overlays use the same rounded pill shape as
  selected rows instead of drawing square blocks inside the selected pill.
- Hid the time-column scrollbars while preserving scrollable hour/minute columns, removing the
  visible vertical tracks beside selected values.
- Added regression coverage for hidden TimePicker column scrollbars, clipped rows, and rounded row
  state layers.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~PickerTests.TimePicker|FullyQualifiedName~GalleryAcceptanceTests.TimePicker_gallery"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 4 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 253 tests.

**Next:** visually verify the TimePicker selected row in the gallery, then continue with ColorPicker
and FileUpload state/variant coverage.

---

## 2026-06-06 — v2.0 — Time picker variants and sample coverage

**Done**
- Added `Variant` support to `TimePicker` so it can render outlined, filled, and text/underline
  field chrome like the other field-based picker controls.
- Replaced the TimePicker gallery sample's three-control demo with a copyable state matrix:
  outlined, filled, text/underline, empty, selected, 24-hour format, minute step, floating label,
  error, and disabled.
- Added control-level variant coverage and gallery acceptance coverage for the TimePicker state
  matrix.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~GalleryAcceptanceTests.TimePicker_gallery|FullyQualifiedName~GalleryAcceptanceTests.DateRangePicker_gallery|FullyQualifiedName~GalleryAcceptanceTests.DatePicker_gallery|FullyQualifiedName~PickerTests.TimePicker|FullyQualifiedName~PickerTests.DateRangePicker_variant|FullyQualifiedName~PickerTests.DatePicker_variant|FullyQualifiedName~PickerTests.Date_and_time|FullyQualifiedName~OverlayTests.PopupSurface"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 14 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 253 tests.

**Next:** visually verify TimePicker variants in the gallery, then continue with ColorPicker and
FileUpload state/variant coverage.

---

## 2026-06-06 — v2.0 — Date range picker variants and sample coverage

**Done**
- Added `Variant` support to `DateRangePicker` so it can render outlined, filled, and text/underline
  field chrome like the other field-based picker controls.
- Replaced the DateRangePicker gallery sample's two-control demo with a copyable state matrix:
  outlined, filled, text/underline, empty, selected, custom format, constrained, floating label,
  error, and disabled.
- Added control-level variant coverage and gallery acceptance coverage for the DateRangePicker
  state matrix.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed after stopping the running gallery
  process that had locked `Loam.dll`.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~GalleryAcceptanceTests.DateRangePicker_gallery|FullyQualifiedName~GalleryAcceptanceTests.DatePicker_gallery|FullyQualifiedName~PickerTests.DateRangePicker|FullyQualifiedName~PickerTests.DatePicker_variant|FullyQualifiedName~PickerTests.Date_and_time|FullyQualifiedName~OverlayTests.PopupSurface"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 12 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 251 tests.

**Next:** visually verify DateRangePicker variants in the gallery, then continue with TimePicker
variant/state coverage.

---

## 2026-06-06 — v2.0 — Date picker sample copyability

**Done**
- Removed the local `DateCase` helper from the DatePicker gallery source-linked sample.
- Rewrote the sample with normal inline `StackPanel`, `Text`, and `DatePicker` object initializers
  so copied code does not imply a private gallery helper is part of the component library.
- Added gallery acceptance coverage that the DatePicker source sample does not contain `DateCase`.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~GalleryAcceptanceTests.DatePicker_gallery|FullyQualifiedName~PickerTests.DatePicker|FullyQualifiedName~PickerTests.Date_and_time|FullyQualifiedName~OverlayTests.PopupSurface"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 10 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 249 tests.

**Next:** visually verify DatePicker source sample readability, then continue with DateRangePicker.

---

## 2026-06-06 — v2.0 — Picker popup shape correction

**Done**
- Reduced the shared picker popup shape from the extra-large dialog radius to the large surface
  radius so anchored picker flyouts read less inflated beside their trigger fields.
- Kept picker width, field variants, and calendar geometry unchanged.
- Existing popup surface tests continue to assert the rendered picker paper root consumes the shared
  shape token.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~GalleryAcceptanceTests.DatePicker_gallery|FullyQualifiedName~PickerTests.DatePicker|FullyQualifiedName~PickerTests.Date_and_time|FullyQualifiedName~OverlayTests.PopupSurface"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 10 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 249 tests.

**Next:** visually verify DatePicker popup radius, then continue with DateRangePicker variants.

---

## 2026-06-06 — v2.0 — Date picker visual variants

**Done**
- Added `Variant` to `DatePicker`, matching the existing field API for outlined, filled, and
  text/underline chrome.
- Wired DatePicker label layout and field chrome through `FieldChrome` for all variants.
- Updated the DatePicker gallery matrix to show outlined, filled, and text/underline variants before
  state examples.
- Added regression coverage for DatePicker variant chrome and the gallery variant matrix.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~GalleryAcceptanceTests.DatePicker_gallery|FullyQualifiedName~PickerTests.DatePicker|FullyQualifiedName~PickerTests.Date_and_time|FullyQualifiedName~OverlayTests.PopupSurface"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 10 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 249 tests.

**Next:** visually verify DatePicker variants in the gallery, then continue with DateRangePicker.

---

## 2026-06-06 — v2.0 — Date picker gallery state matrix

**Done**
- Expanded the DatePicker gallery page from a short vertical sample into a wrapped state matrix.
- Added empty, selected, custom format, constrained, floating-label, error, and disabled DatePicker
  examples, each using the actual live control.
- Aligned adjacent picker gallery page widths to the 360px picker field width.
- Added gallery acceptance coverage for the DatePicker state matrix and source-linked code.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~GalleryAcceptanceTests.DatePicker_gallery|FullyQualifiedName~PickerTests|FullyQualifiedName~OverlayTests.PopupSurface"
  --blame-hang --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false`
  passed: 25 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 248 tests.

**Next:** visually verify the DatePicker state matrix, then continue with DateRangePicker.

---

## 2026-06-06 — v2.0 — Date picker input-mode removal

**Done**
- Removed the invented DatePicker popup input mode and nested `TextField`.
- Kept the calendar picker anatomy with label, headline, divider, calendar body, and Cancel/OK
  actions.
- Added regression coverage that the DatePicker popup contains no nested `TextField` and that picker
  triggers keep outlined field chrome instead of underline-style chrome.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~PickerTests|FullyQualifiedName~OverlayTests.PopupSurface" --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 24 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 247 tests.

**Next:** visually verify the DatePicker trigger and popup, then continue with DateRangePicker.

---

## 2026-06-06 — v2.0 — Picker width and shape tightening

**Done**
- Made the shared picker shape an explicit `PopupSurface` token and added coverage that the rendered
  `Paper` root actually receives that corner radius.
- Aligned DatePicker and DateRangePicker default field widths to the 360px picker surface so the
  standalone field/picker pairing reads as one component instead of mismatched widths.
- Kept the picker surface at 360px because the seven-column calendar grid needs that width even
  when the field is used in a narrower layout.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~PickerTests|FullyQualifiedName~OverlayTests.PopupSurface" --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 24 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 247 tests.

**Next:** visually verify DatePicker field/picker width and radius, then continue with DateRangePicker
and TimePicker anatomy.

---

## 2026-06-06 — v2.0 — Date picker dialog anatomy correction

**Done**
- Reworked the actual `DatePicker` popup from a compact calendar dropdown into a full picker
  surface with helper label, selected-date headline, input/calendar mode toggle, divider, calendar
  body, and Cancel/OK actions.
- Changed date selection to a pending value: choosing a day updates the popup headline, `OK` commits,
  and `Cancel` closes without changing the field value.
- Updated the month grid header to use a month selector row with previous/next actions on the right,
  larger weekday/day typography, and the corrected extra-large picker container shape.
- Added regression coverage for the full picker structure and pending Cancel/OK behavior.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~PickerTests|FullyQualifiedName~OverlayTests.PopupSurface" --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 24 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 247 tests.

**Next:** visually verify DatePicker against the reference anatomy, then continue with DateRangePicker
and TimePicker.

---

## 2026-06-06 — v2.0 — Picker calendar layout alignment

**Done**
- Removed the extra shared picker paper padding and moved title/action spacing into the picker
  content so the 360px popup surface does not squeeze calendar content.
- Updated `MonthCalendar` to use a centered 336px calendar body, 56px month header, 48px weekday/day
  slots, 40px circular day targets, and 42 stable day-grid cells.
- Added regression coverage for picker surface spacing and calendar grid metrics.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~PickerTests|FullyQualifiedName~OverlayTests.PopupSurface" --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 23 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 246 tests.

**Next:** verify the picker popup in the Release gallery, then continue the component-by-component
input/picker audit.

---

## 2026-06-06 — v2.0 — Picker row and calendar interaction states

**Done**
- Added focusability, automation names, keyboard activation, and tokenized hover/focus state layers
  to `MonthCalendar` day cells.
- Disabled out-of-range calendar days are now explicitly non-focusable, have disabled help text,
  and do not respond to keyboard activation.
- Hardened `TimePicker` hour/minute rows with stable 48px hit targets, automation names, selected
  styling, and separate hover/focus state-layer overlays.
- Added regression coverage for calendar keyboard selection, disabled days, real picker flyout
  surfaces, and time row keyboard selection/state layers.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~PickerTests|FullyQualifiedName~OverlayTests.PopupSurface" --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 22 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 245 tests.

**Next:** visually inspect DatePicker, DateRangePicker, MonthCalendar, and TimePicker popup states in
the Release gallery, then continue to ColorPicker/FileUpload or begin release-readiness cleanup.

---

## 2026-06-06 — v2.0 — Picker popup surface standardization

**Done**
- Added a shared picker popup contract in `PopupSurface`: 360px fixed width, 24/20/24/20 padding,
  large shape, clipped paper, and a consistent title/body/actions stack.
- Moved `DatePicker`, `DateRangePicker`, and `TimePicker` onto the shared picker popup surface
  instead of one-off title, width, and padding layouts.
- Added coverage that opens the real date, range, and time picker flyouts and verifies the shared
  paper width, padding, clipping, and title/body structure.
- Added coverage for shared picker content and updated the shared picker paper expectations.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~PickerTests|FullyQualifiedName~OverlayTests.PopupSurface" --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 19 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 242 tests.

**Next:** visually inspect DatePicker, DateRangePicker, and TimePicker popups in the Release gallery,
then tighten picker row/cell hover, selected, and keyboard states if needed.

---

## 2026-06-06 — v2.0 — Autocomplete popup blink correction

**Done**
- Fixed the remaining Autocomplete blink/shrink while typing by keeping one popup paper, scroller,
  and list alive while the popup is open.
- Updating suggestions now replaces only row children; it no longer swaps the whole popup surface
  on every keystroke.
- Kept the open popup height stable while results narrow, so typing from many matches to fewer
  matches does not visually shrink the dropdown during the same open session.
- Added regression coverage that typing `a` then narrowing to `app` preserves the same popup paper,
  same scroller, and 320px open height.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 13 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 240 tests.

**Next:** visually retest Autocomplete typing in the Release gallery. The list should update
without row text resizing and without popup shrink/flash during the same open session.

---

## 2026-06-06 — v2.0 — Autocomplete fixed suggestion text metrics

**Done**
- Hard-locked Autocomplete suggestion row text metrics in the control: 16px font size, 24px line
  height, normal weight, zero margin, and fixed 48px row height.
- Stopped simple suggestion text from relying on typography resource rebinding while rows are
  recreated during typing.
- Updated regression coverage to assert actual row font metrics instead of checking a typography
  enum value.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 13 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 240 tests.

**Next:** visually retest Autocomplete typing in the Release gallery; if row text still appears to
resize, capture the exact typed text and moment so the remaining issue can be isolated to rendering
scale rather than component metrics.

---

## 2026-06-06 — v2.0 — Autocomplete keyboard typing filter correction

**Done**
- Investigated `auto.mp4`; the failure was that after typing `app`, the popup could still show
  the wider one-letter `a` result set.
- Hardened Autocomplete to listen to the inner editor's `TextChanged` event and reattach if the
  wrapped text editor is recreated by template application.
- Removed the custom row template from the gallery Autocomplete demo so the sample validates the
  component's own suggestion-row anatomy.
- Added a keyboard-input regression using the headless text-input API: typing `a`, then `pp`, must
  narrow the list to `Apple` and `Pineapple`.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 13 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 240 tests.

**Next:** visually retest the gallery Autocomplete by typing `a`, `ap`, and `app`; the result list
must narrow immediately and row text must stay stable.

---

## 2026-06-06 — v2.0 — Autocomplete suggestion typography stability

**Done**
- Fixed Autocomplete suggestion text jumping between row text sizes while typing.
- Normalized simple suggestion templates (`Text`/`TextBlock`) to the component's dropdown row
  typography so row text stays consistent across filtered result updates.
- Updated the gallery Autocomplete examples to use the same row typography as the component.
- Added regression coverage for template-provided smaller text being normalized in suggestion rows.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 12 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 239 tests.

**Next:** visually re-check Autocomplete typing in the Release gallery, especially `a`, `ap`,
`App`, and `Gra`, then continue with DatePicker/TimePicker popup anatomy.

---

## 2026-06-06 — v2.0 — Autocomplete current-text filtering correction

**Done**
- Fixed the case where typing `ap` could still show the previous one-letter `a` suggestion set.
- Autocomplete now listens to the inner editor text as the source of truth, keeps the wrapper field
  synchronized, and rejects stale queued suggestion renders if the editor text has changed.
- Pending searches are invalidated when the popup closes so old results cannot reopen over a newer
  empty, closed, or selected state.
- Added a regression test matching the gallery data: `a` expands to many fruit rows, then `ap`
  narrows to `Apple`, `Apricot`, `Grape`, and `Pineapple`.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 11 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 238 tests.

**Next:** visually re-check Autocomplete with `a`, `ap`, `App`, `Gra`, and item selection in the
Release gallery, then continue with DatePicker/TimePicker popup anatomy.

---

## 2026-06-06 — v2.0 — Autocomplete opaque popup row correction

**Done**
- Fixed the `App` stacked Autocomplete case where the Country field text could still show through
  the Fruit suggestions.
- Changed suggestion rows to paint an opaque tonal popup surface by default, with hover/focus drawn
  as a separate tokenized state-layer overlay instead of replacing the whole row background.
- Added a regression test for Fruit `App` suggestions above a filled Country Autocomplete with
  `Sweden` below it.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 10 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 237 tests.

**Next:** visually re-check Fruit `App`, `Gra`, and selecting an item in the Release gallery, then
continue with the next input/picker component panel.

---

## 2026-06-06 — v2.0 — Autocomplete popup stacking correction

**Done**
- Raised an Autocomplete to the popover z-layer while its suggestion popup is open, then restored
  its previous z-index when the popup closes.
- Raised the suggestion surface itself to the popover layer so stacked controls below it cannot draw
  their text through or over the menu.
- Added a stacked Fruit/Country regression case covering the bleed-through scenario.
- Disabled test collection parallelization for the headless UI test assembly, because the suite
  creates Avalonia windows/popups on shared UI infrastructure and was racing across classes.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 9 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 236 tests.

**Next:** visually inspect the Autocomplete page by typing `Gra` in Fruit with Country below it,
then continue with the next input/picker component panel.

---

## 2026-06-06 — v2.0 — Autocomplete menu row anatomy correction

**Done**
- Stopped Autocomplete from opening a full suggestion menu on empty focus; suggestions now open
  after meaningful typed text.
- Replaced the suggestion popup's `ListItem` rows with compact internal menu rows so list-item
  margins/templates cannot overflow or paint text outside the popup surface.
- Kept the same token-driven hover/focus state layers, explicit 48px row height, field-width
  surface, and bounded clipped scroll region.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 8 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 235 tests.

**Next:** visually inspect Autocomplete after typing one or more characters, then continue with the
next input/picker component panel.

---

## 2026-06-06 — v2.0 — Autocomplete popup anatomy correction

**Done**
- Replaced the loose autocomplete flyout with an anchored template popup, matching the field's
  placement model used by other dropdown controls.
- Forced suggestion surfaces, scroll area, and rows to the field width, with a bounded vertical
  scroller instead of a small detached rectangle.
- Locked suggestion row height, popup height, and scroll clipping so rows cannot paint outside the
  menu surface when many suggestions are shown.
- Added dismissal on the actual inner text editor losing focus so suggestions do not remain as
  stray top-level surfaces after leaving the field or app context.
- Added regression coverage for popup width, popup height, selection close, caret placement,
  focus-loss close, and many-row overflow clipping.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 7 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 234 tests.

**Next:** visually inspect Autocomplete in the Release gallery for popup alignment, width, close
behavior, hover, and selection, then continue with the next input/picker panel.

---

## 2026-06-06 — v2.0 — Autocomplete text synchronization correction

**Done**
- Replaced the autocomplete inner field's two-way binding loop with guarded text/value
  synchronization, so typing a partial suggestion updates `Value` without recursively rewriting
  the field.
- Updated suggestion selection to set the completed text, close the flyout, restore focus, and move
  the inner text box caret/selection to the end of the chosen value.
- Added regression coverage for partial typing, suggestion row activation, and caret placement after
  selection.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release /nodeReuse:false` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests.Autocomplete" --blame-hang --blame-hang-timeout 120s
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 5 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 232 tests
  after `dotnet build-server shutdown` cleared stale build nodes.

**Next:** visually inspect the Autocomplete page in the Release gallery, then continue with the next
input/picker component panel.

---

## 2026-06-06 — v2.0 — Dropdown row state-layer correction

**Done**
- Added color-role state-layer resources for hover, focus, pressed, and selected states so
  controls can bind neutral content overlays instead of reusing table colors.
- Updated `ListItem` hover/focus rendering to use `OnSurface` state layers; Select, Menu, and
  Autocomplete dropdown rows inherit the corrected hover behavior through the shared row control.
- Added regression coverage proving hovered list/menu rows no longer use the table hover color.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~ThemingTests|FullyQualifiedName~FeedbackTests|FullyQualifiedName~InputTests"
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 67 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 230 tests.

**Next:** visually inspect Select, Menu, and Autocomplete dropdown hover states in the Release
gallery, then continue with the selection-control panels.

---

## 2026-06-06 — v2.0 — Field and popup surface adoption

**Done**
- Standardized menu/select/autocomplete popups through the shared popup-surface helper:
  compact menu shape, no extra paper padding, deterministic tonal elevation, and clipped rounded
  content.
- Added a shared picker popup surface for date, date-range, time, and color pickers so these
  controls no longer hand-build divergent popup chrome.
- Hardened Select, Menu, and Autocomplete item picking with explicit row pointer activation, so a
  clicked Select item updates value and closes reliably while keyboard activation remains intact.
- Updated Select item anatomy so selected options use a real check icon plus selected row state
  instead of decorating the display text.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter
  "FullyQualifiedName~InputTests|FullyQualifiedName~OverlayTests|FullyQualifiedName~PickerTests"
  -p:UseSharedCompilation=false /nodeReuse:false` passed: 75 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 229 tests.

**Next:** visually inspect Select, Menu, Autocomplete, DatePicker, TimePicker, DateRangePicker, and
ColorPicker in the Release gallery, then continue with the selection-control component panels.

---

## 2026-06-06 — v2.0 — Component adoption button pilot

**Done**
- Added `ExtraSmall` and `ExtraLarge` to the public size scale and projected explicit
  five-size interactive, button-container, button-padding, and icon-button-padding density
  tokens.
- Introduced shared button size metrics and applied them across `Button`, `IconButton`,
  `ToggleIconButton`, `ButtonGroup`, and `Fab` so the button family has consistent height,
  padding, icon spacing, shape, and minimum hit target behavior.
- Corrected standard `Button` defaults to a restrained desktop scale
  (`32/36/46/54/64`) instead of oversized expressive display heights; `Fab` keeps its
  separate larger action scale.
- Mapped button and floating action label typography by size so large sizes no longer reuse the
  compact label scale.
- Bound button ripple feedback to the resolved control foreground so press feedback is visible
  across filled, outlined, text, tonal, icon, and floating action variants.
- Extended size-aware display and selection controls plus the gallery size matrix to render all
  five sizes, added a darker button configuration rail for visual scale review, and added neutral
  source-reference acceptance metadata for every catalog page.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 226 tests.
- Release gallery was launched for visual inspection.

**Next:** use this button-family pattern for the next component panels: fields/pickers,
selection controls, surfaces, shell, data, and charts should each get the same token, state,
size, source-reference, and copyable-code treatment.

---

## 2026-06-05 — v2.0 — Full component design audit acceptance

**Done**
- Added internal gallery acceptance criteria for every component catalog page covering anatomy,
  color roles, typography, shape, state layers, focus, press/ripple, disabled, selected/active,
  error, open/dismiss, loading, empty, keyboard, automation, responsive behavior, density, size,
  motion, light/dark rendering, and source-linked code where applicable.
- Added gallery acceptance tests so high-risk families must carry the relevant state criteria:
  buttons, inputs, pickers, data, navigation, shell, feedback, surfaces, and charts.
- Strengthened foundation tests so every color-scheme role projects to resources in light and dark,
  and every content/on-role pair meets the text contrast baseline.
- Updated the component adaptation audit so the remaining field, action, surface, feedback, and
  navigation rows are tracked as complete under the acceptance matrix.

**Verified**
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 210 tests.

**Next:** keep visual QA focused on individual pages that may still feel weak despite passing
acceptance, especially dense forms, overlays, and picker popups.

---

## 2026-06-05 — v2.0 — Gallery form sample polish

**Done**
- Reworked the `Form` gallery page from a bare two-field stack into a framed form preview with a
  title, guidance text, three realistic fields, fixed field width, validate/reset actions, and
  status feedback.
- Added a gallery acceptance test so the form sample keeps realistic field widths instead of
  collapsing to natural label size.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 206 tests.
- Visual QA captured the `Form` gallery page; the preview now reads as a real compact form and the
  source sample matches it.

**Next:** continue visual spot-checking individual pages for samples that feel too thin or
toy-like even when they are technically correct.

---

## 2026-06-05 — v2.0 — Gallery page-specific samples

**Done**
- Split duplicated gallery routes into page-specific preview/source builders so sidebar pages no
  longer reuse the same live content under different component names.
- Added focused samples for Avatar vs AvatarGroup, CheckBox vs Switch, text-field variants,
  selection controls, picker variants, feedback loaders, dialogs/snackbars, tabs/menu,
  navigation controls, layout/shell controls, spacer, and PieChart/BarChart/LineChart.
- Tightened gallery acceptance coverage so catalog pages must use distinct builder methods.
- Expanded code-sample highlighting for the additional component types now exposed in focused
  snippets.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 205 tests.
- Visual QA captured AvatarGroup, Switch, PieChart, BarChart, and LineChart pages; each now shows
  focused preview content and matching source.

**Next:** review remaining broad overview pages for depth, not duplication, and add richer states
where a focused page still feels too thin.

---

## 2026-06-05 — v2.0 — Gallery code copy action

**Done**
- Added a copy icon action to every gallery code sample header so source-linked C# samples can be
  copied directly from the gallery.
- Wired the action through Avalonia's clipboard API with transient copied/failed feedback and an
  accessible automation name.
- Added the built-in `ContentCopy` icon path and gallery acceptance coverage for the code-copy
  button.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 205 tests.
- Visual QA captured the `Field` gallery page; the code sample header keeps the filename, centered
  `C#` badge, and copy icon action aligned.

**Next:** consider whether code samples should also support direct text selection in addition to
the whole-sample copy action.

---

## 2026-06-05 — v2.0 — Gallery size matrix and custom field chrome

**Done**
- Promoted the custom field `TextBox` reset path to public `FieldEditor.MakeChromeless(TextBox)`
  and updated the gallery/docs to use the shared helper instead of duplicating field chrome reset
  logic.
- Removed redundant `PART_SwitchArea` naming and avoided highlighting bare `Icon` property names as
  types in gallery code samples.
- Fixed filled/text field floating labels so they no longer draw the outlined-field notch backing;
  the `Price` filled numeric field now renders without the visible label patch.
- Removed the repeated gallery header badges (`group`, `Tokenized`, `Sample`) so component pages
  keep focus on the live preview and source sample.
- Added a gallery `Sizes` page that renders all five public size examples for every
  size-aware control surface: buttons, icon buttons, toggle icon buttons, button groups, FAB,
  icons, avatars, avatar groups, chips, checkboxes, switches, radios, ratings, and circular
  progress.
- Removed the native focused border from the custom phone editor inside the `Field` gallery sample
  so only the shared field chrome provides focus/error outline.
- Made existing `Size` APIs visibly effective for `Switch`, `Chip`, and `Fab`.
- Updated gallery code highlighting for the newly surfaced size-sample types.
- Widened size-sample cells so connected controls such as `ButtonGroup` do not clip labels.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 204 tests.
- `npm run docs:build` in `docs` passed.
- Visual QA captured the `Sizes` and `Field` gallery pages; size rows are readable and the custom
  field sample no longer shows the stray native focused border.
- Visual QA captured the `TextField` gallery page; filled labels render without the notch patch.

**Next:** decide whether individual component pages should also duplicate their size rows, or keep
the catalog-level `Sizes` page as the single acceptance surface.

---

## 2026-06-05 — v2.0 — Release readiness and packaging

**Done**
- Refreshed README and chart docs for the current test count, source-compatible package command,
  theme-role chart colors, explicit chart color overrides, visible no-data chart states, negative
  value clamping, and `Charts.Palette` compatibility behavior.
- Updated docs metadata and component trackers so chart/data-display release status no longer reads
  as pending for the v2.0 baseline.
- Verified package contents for version `2.0.0`, `Loam.dll`, XML docs, README, MIT license
  expression, repository metadata, and no gallery/test/build artifacts inside the package.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 196 tests.
- `dotnet pack src\Loam\Loam.csproj -c Release --no-build` produced
  `src\Loam\bin\Release\Loam.2.0.0.nupkg`.
- `npm run docs:build` in `docs` passed.
- Release gallery visual QA checked Overview, Design System, Charts, DataGrid, TextField,
  Dialog/Snackbar, and Shell pages.

**Next:** tag/publish decision can be handled separately; no registry publish was performed here.

---

## 2026-06-05 — v2.0 — Chart theming and acceptance

**Done**
- Hardened `PieChart`, `BarChart`, and `LineChart` with theme-role default series colors,
  explicit color override preservation, tokenized grid/outline/surface/text rendering, visible
  empty states, automation names/help text, and deterministic negative-value clamping.
- Kept existing chart APIs source-compatible while adding internal chart visual resolution and
  test visibility for focused acceptance coverage.
- Expanded gallery chart samples with themed pie/donut/bar/line examples, legend rows, an
  explicit-color sample, and a visible no-data sample while keeping source-linked code.
- Added headless tests for chart math, light/dark role colors, explicit overrides, empty states,
  automation text, and value/property update behavior.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 196 tests.
- Release gallery visual QA checked Overview plus PieChart, BarChart, and LineChart pages in
  light and dark, including explicit-color and no-data chart examples.

**Next:** final packaging/docs scan and release readiness pass.

---

## 2026-06-05 — v2.0 — Data display hardening

**Done**
- Hardened `DataGrid` row and header behavior with focusable/named rows, keyboard row selection,
  keyboard sorting, tokenized selected/focus/hover/striped layers, deterministic page clamping,
  and selected-item cleanup when filtered rows disappear.
- Hardened `SimpleTable`, `TreeView`, `ListItem`, `ExpansionPanel`, `Timeline`, and `Carousel`
  with clearer automation names, keyboard behavior, disabled/selected/focus states, empty-state
  rendering, clamped carousel selection, and tokenized surface feedback.
- Expanded gallery examples for paged and virtual data grids, dense/bordered/empty tables,
  selected and disabled tree/list states, disabled expansion panels, timeline cards, and carousel
  navigation while preserving source-linked samples.
- Added focused headless coverage for data-grid selection/sorting/clamping, simple-table empty
  states, tree expansion/navigation, list-item activation, expansion-panel disabled behavior,
  timeline automation, and carousel keyboard navigation.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 191 tests.
- Release gallery visual QA checked DataGrid, SimpleTable, TreeView, List/ListItem,
  ExpansionPanels, Timeline, and Carousel with captured desktop surfaces.

**Next:** chart theming remains the next deferred phase; deeper data-grid grouping remains future
expansion.

---

## 2026-06-05 — v2.0 — Shell and overlay hardening

**Done**
- Hardened shell behavior for `AppBar`, `Drawer`, and `Layout`, including tokenized app-bar height,
  visual stacking, temporary drawer scrim color, disabled opacity, focusability, automation naming,
  and Escape close behavior.
- Hardened overlay and feedback controls for `DialogService`, `Overlay`, `Popover`, `Menu`,
  `SnackbarService`, `Tooltip`, `Alert`, progress controls, `Skeleton`, and `Collapse`, including
  Escape/light-dismiss paths, focus return, automation names, tokenized scrims/z-order, deterministic
  snackbar dismissal, and token-backed motion defaults.
- Expanded gallery samples for docked/mini/temporary shell states, initially visible overlay scrim,
  open popover, snackbar/dialog actions, and static feedback examples while keeping source-linked
  sample panels.
- Added headless coverage for temporary drawer Escape/scrim tokens, overlay Escape auto-close,
  dialog Escape cancellation, popover Escape, snackbar Escape dismissal, tooltip help text, feedback
  automation names, and disabled alert opacity.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 183 tests.
- Release gallery visual QA checked Shell/Layout docked, mini, and temporary drawer examples;
  Feedback progress/skeleton/static examples; Overlay scrim; Popover open state; and Alert surfaces.

**Next:** data-grid/table/tree polish and chart theming remain follow-up phases.

---

## 2026-06-05 — v2.0 — Component interaction audit

**Done**
- Hardened selection controls with tokenized hit targets, disabled opacity, keyboard adjustment or
  activation, focusable template surfaces, and automation names for Slider, Rating, ToggleGroup, Chip,
  and ChipSet while preserving existing component APIs.
- Tightened navigation/data interaction behavior for Link, Breadcrumbs, NavLink, NavGroup, Tabs,
  Stepper, and Pagination, including keyboard activation, selected/active state handling, automation
  names, and selected-page clamping.
- Expanded gallery samples for the touched families with selected, disabled, read-only, and grouped
  states while keeping source-linked samples.
- Added focused headless tests for keyboard selection/activation, focusable surfaces, automation
  names, selected/disabled/clamped states, and gallery article coverage.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 175 tests.
- Release gallery launched from `samples\Loam.Gallery\bin\Release\net8.0\Loam.Gallery.exe`; live
  visual captures checked CheckBox/Switch disabled states and ToggleGroup selected/disabled states.
- Gallery acceptance coverage rendered all catalog articles in light and dark, including Navigation,
  Tabs, and ScrollToTop pages.

**Next:** shell/navigation surfaces and overlay/data polish.

---

## 2026-06-05 — v2.0 — Gallery acceptance audit

**Done**
- Added internal gallery catalog metadata for routes, builder methods, expected component names, and
  single-component versus family samples.
- Exposed the catalog and article builder internally for headless acceptance coverage without changing
  the Loam public component API.
- Added gallery acceptance tests for route uniqueness, source-linked non-fallback code, shared-family
  coverage, light/dark article rendering, and expected component surface coverage.
- Split the `ScrollToTop` page from the `Hidden` sample so its preview and source now use a dedicated
  live `BuildScrollToTop` builder.

**Verified**
- `git diff --check` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 164 tests.
- Release gallery visual QA covered Overview, Badge, ButtonGroup, TextField, DatePicker, Alert,
  DataGrid, ScrollToTop, and LineChart.

**Next:** commit and push the acceptance-audit batch.

---

## 2026-06-05 — v2.0 — Gallery source-linked samples

**Done**
- Replaced the gallery's hand-written title-to-snippet table with source-linked samples that read the
  actual preview builder method used by each component page.
- Added source extraction with brace matching so the code panel stays related to the live preview when
  the gallery sample implementation changes.
- Cached loaded source and extracted snippets to keep gallery startup and page switching responsive.

**Verified**
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 159 tests.
- Visual QA confirmed representative pages display source-linked builder methods for Badge, Button,
  ButtonGroup, TextField, and AvatarGroup.

**Next:** commit and push the stabilization batch.

---

## 2026-06-05 — v2.0 — Filled button focus state correction

**Done**
- Changed filled button focus styling from an inner 2px border to the filled state-layer background,
  avoiding the false underline artifact when a child button in a connected group receives focus.
- Applied the same filled focus treatment to filled icon/FAB-style buttons through the shared button
  style matrix.
- Added focused coverage for filled `ButtonGroup` child focus behavior.

**Verified**
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --filter "FullyQualifiedName~PrimitivesTests"`
  passed: 19 tests.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 120s -p:UseSharedCompilation=false /nodeReuse:false` passed: 159 tests.
- Visual QA confirmed a clicked/focused filled `ButtonGroup` child uses the state-layer background
  without drawing a selected-looking underline.

**Next:** commit and push the stabilization batch.

---

## 2026-06-05 — v2.0 — Badge overlay layout fix

**Done**
- Changed `Badge` positioning so the indicator reserves overlay space inside the control's measured
  layout slot instead of relying on a render transform outside bounds.
- Kept the same corner/origin behavior while preventing top/right clipping in tight or clipped
  parent surfaces.
- Added headless coverage for top-right badge placement and the absence of out-of-bounds transform.

**Verified**
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --filter "FullyQualifiedName~DisplayTests"`
  passed: 32 tests.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 60s -p:UseSharedCompilation=false /nodeReuse:false` passed: 158 tests.

**Next:** visually confirm the relaunched gallery badge preview no longer clips indicator content.

---

## 2026-06-05 — v2.0 — Field hover background fix

**Done**
- Neutralized the inner Avalonia `TextBox` hover/focus background resources used inside Loam field
  chrome so outlined fields no longer show a too-dark inner rectangle in dark mode.
- Re-applied the transparent inner chrome after pointer and focus state changes.
- Added dark-theme headless coverage for hover and focus transparency on `TextField`.

**Verified**
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --filter "FullyQualifiedName~InputTests"`
  passed: 35 tests.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 60s -p:UseSharedCompilation=false /nodeReuse:false` passed: 157 tests.

**Next:** visually confirm the relaunched gallery field hover/focus state in dark mode.

---

## 2026-06-05 — v2.0 — Gallery navigation icon polish

**Done**
- Expanded the curated built-in icon catalog with reusable glyphs for display, inputs, feedback,
  data, navigation, layout, surfaces, and charts.
- Replaced generic gallery sidebar fallbacks with group/page-specific icons so component rows no
  longer repeat stars and checks.
- Updated the gallery top-bar mark from a generic star to the component-grid glyph.
- Added headless coverage that parses every built-in icon path as Avalonia geometry.

**Verified**
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --filter "FullyQualifiedName~PrimitivesTests"`
  passed: 18 tests.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 60s -p:UseSharedCompilation=false /nodeReuse:false` passed: 156 tests.

**Next:** visually review the relaunched gallery sidebar and tune any individual glyph choices that
feel too close semantically.

---

## 2026-06-05 — v2.0 — Scroll-to-top activation fix

**Done**
- Wired `ScrollToTop` to handle routed button clicks from its default FAB, so pressing the visible
  affordance scrolls the target viewer home.
- Kept the pointer-release fallback for custom non-button children, forced the target offset to
  zero, and re-evaluated visibility immediately after activation.
- Unsubscribed from the watched `ScrollViewer` on visual-tree detach to avoid stale scroll handlers.
- Added headless regression coverage for click activation and hide-after-scroll behavior.

**Verified**
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --filter "FullyQualifiedName~LayoutTests"`
  passed: 13 tests.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test Loam.slnx -c Release --no-build` passed: 155 tests.

**Next:** visually confirm the gallery FAB scrolls the current page to the top after launch.

---

## 2026-06-05 — v2.0 — Gallery page and code sample polish

**Done**
- Added an editor-style `CodeSampleView` for every component page, with normalized indentation,
  line numbers, a file header, and lightweight C# token coloring.
- Reworked gallery component pages with a cleaner masthead, token/state chips, and a calmer preview
  panel header so live controls and samples scan as one acceptance surface.
- Kept the implementation C#-only and local to the gallery app.

**Verified**
- `dotnet build Loam.slnx -c Release --no-restore` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 60s -p:UseSharedCompilation=false /nodeReuse:false` passed: 154 tests.
- `git diff --check` passed.

**Next:** visually review the gallery after launch and continue filling deeper per-component state
examples where the sample page still shares a family preview.

---

## 2026-06-05 — v2.0 — Field label notch polish

**Done**
- Changed shared field floating-label host backgrounds from the base surface token to the surface
  container token so outlined labels blend with tonal page/container backgrounds instead of showing a
  white patch.
- Added focused input coverage for the floating label host background.

**Verified**
- `dotnet build Loam.slnx -c Release --no-restore` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --filter InputTests`
  passed: 34 tests.

**Next:** continue reviewing remaining field and picker states against tonal surfaces.

---

## 2026-06-05 — v2.0 — Design-system rebaseline foundation

**Done**
- Added role-based `LoamColorScheme` for light/dark defaults and kept `LoamPalette` as the
  compatibility adapter, including a legacy migration preset.
- Expanded foundations with typography role aliases, shape scale, spacing, stroke, density,
  tonal elevation, and richer motion tokens.
- Reprojected `LoamTheme` so color roles, compatibility palette keys, tonal surfaces, spacing,
  stroke, density, elevation mapping, state, motion, and field metrics are live resources.
- Updated high-impact component paths: semantic color resolution, button/fab/icon-button state
  layers and ripple tokens, field metrics/chrome, paper tonal surfaces, table/grid density, and the
  design-system gallery page.
- Bumped package version to `2.0.0` and updated docs/memory for the new baseline.

**Verified**
- `dotnet build Loam.slnx -c Release --no-restore` passed.
- `dotnet build Loam.slnx -c Release` passed.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 60s -p:UseSharedCompilation=false /nodeReuse:false` passed: 154 tests.
- `dotnet pack src\Loam\Loam.csproj -c Release --no-build` created
  `src\Loam\bin\Release\Loam.2.0.0.nupkg`.
- Docs/gallery neutral-name scan only found historical entries and SVG path-command false positives;
  the updated gallery launched and stayed alive as `Loam Gallery`.

**Next:** continue the component-family audit for navigation, overlays, pickers, selection controls,
and chart legend/readability polish.

---

## 2026-06-04 — Unreleased — Component adaptation tokens and fields

**Done**
- Bumped the package version to `1.3.0` and started branch `work/component-adaptation`.
- Added additive theme data for shape, state layers, motion, and field metrics, and projected them
  through `LoamTokens`.
- Updated shared field chrome to consume field metrics, shape, and disabled-state opacity tokens.
- Brought `Select`, `Autocomplete`, and the field-style pickers onto the shared field-state surface:
  resting/floating labels, helper/error text, shrink labels, focus/error chrome, and keyboard close.
- Updated gallery samples and docs for the new field APIs; added an audit tracker for the broader
  component adaptation pass.

**Verified**
- `dotnet build Loam.slnx -c Release --no-restore` passed after the source changes.
- Focused test additions cover theme token projection, select field states, autocomplete forwarding,
  and picker shared chrome; final full test run remains next.

**Next:** run the full release checks, gallery launch, hygiene scans, then commit and push.

---

## 2026-06-04 — Unreleased — Field and gallery navigation

**Done**
- Added standalone `Field` for custom input-like content with label, helper/error text, variants,
  semantic focus color, start/end adornments, `InnerPadding`, and automation names.
- Extracted shared field chrome so `Field`, `TextField`, and `NumericField` use consistent border,
  focus, error, filled, outlined, and text-variant behavior.
- Explicitly reset inner `TextBox` chrome in field-style input templates so wrapped inputs do not
  draw a second native border/background inside Loam chrome.
- Reworked the gallery components tab into a side-menu catalog with focused pages, including a
  dedicated `Field` page and direct pages for form validation and the month calendar.

**Verified**
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-restore` passed: 142 tests.
- `dotnet build Loam.slnx -c Release --no-restore` passed.

**Next:** update docs build output, run final hygiene scans, and publish the implementation branch.

---

## 2026-06-04 — v1.2 — Polish release

**Done**
- Public API polish: `Text.Align`, `ToggleIconButton.ToggledColor`, and `ChipSet.MultiSelect` /
  `SelectedIndexes` with single-select compatibility preserved.
- Interaction/accessibility polish: `Select`, date/time/range/color pickers, `ExpansionPanel`,
  `NavGroup`, and tree rows are focusable where needed; Enter/Space activation, Escape flyout close,
  and basic automation names are covered by headless tests.
- Feedback/motion polish: `ProgressLinear.Indeterminate`, `Skeleton.Animate`, and animated
  `Collapse` with configurable `Duration`; expansion and nav reveal now use `Collapse`.
- Release maintenance: package version moved to `1.2.0`; workflows opt into the Node 24 action
  runtime; docs, gallery, inventory, and README were updated with neutral wording.

**Verified**
- `dotnet build Loam.slnx -c Release -p:UseSharedCompilation=false /nodeReuse:false` passed with
  isolated NuGet caches.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release -p:UseSharedCompilation=false
  /nodeReuse:false` passed: 139 tests.

**Next:** run docs build, pack, hygiene scans, then merge/tag `v1.2.0` for package publishing.

---

## 2026-06-04 — v1.1 — Full component hardening pass

**Done**
- Inputs: `TextField` gained start/end adornments and optional floating-label behavior; `Select`
  gained multi-select, selected-values display, formatter, and item-template hooks; `Autocomplete`
  gained sync/async search delegates and templated suggestion rows.
- Pickers: `DatePicker`, `DateRangePicker`, and `MonthCalendar` gained min/max constraints; range
  calendars now highlight the selected span; `ColorPicker` gained alpha display plus HSV conversion
  helpers.
- Data display: `DataGrid<T>` gained filtering, custom filter predicates, unpaged render limiting,
  custom cell templates, and editable text cells.
- Shell/primitives: `Drawer` gained docked/temporary modes with scrim support in `Layout`; button and
  icon-button templates now include ripple hosts automatically.
- Docs, gallery examples, component inventory, and tests were updated for the new surface.

**Verified**
- `dotnet build Loam.slnx -c Release -p:UseSharedCompilation=false /nodeReuse:false` passed with
  isolated NuGet caches.
- `dotnet test tests\Loam.Tests\Loam.Tests.csproj -c Release --no-build --blame-hang
  --blame-hang-timeout 60s` passed: 127 tests.

**Next:** publish v1.1.0 through the repository pipeline once the branch is merged.

---

## 2026-06-04 — v1.1 — Snackbar actions and limits

**Done**
- `SnackbarOptions` added for action callbacks, action text, custom duration, severity, and per-toast
  visible-count limits.
- `SnackbarService` keeps a shared overlay host per window, so repeated `SnackbarService.For(...)`
  calls participate in the same visible queue.
- Gallery and docs show an undo-style action toast.

**Verified**
- Added headless tests for action invocation/dismissal and visible-count trimming across service
  instances.

**Next:** continue hardening: input chrome reuse, picker constraints/range visuals, and data-grid
filtering/virtualization.

---

## 2026-06-03 — Cross-cutting — Ripple + DateRangePicker; Table & CSS-utils resolved — INVENTORY 100%

**Done** (build green; PickerTests+LayoutTests 18/18 isolated)
- **`Ripple`** (`: Decorator`, `ClipToBounds`) — on `OnPointerPressed`, captures the press point and
  animates `Progress` 0→1 (infinite-free `Animation`, `AffectsRender`); `Render` draws a translucent
  black circle expanding to `MaxReach` and fading. Pure static `Ripple.MaxReach(origin, size)`
  (farthest-corner distance). CA1001 suppressed (CTS disposed on detach, like `ProgressCircular`).
- **`DateRangePicker`** (`: TemplatedControl`) — DatePicker-style outlined box showing `Start – End`
  (two-way); flyout `MonthCalendar` where 1st click = start, 2nd = end (auto-ordered, swaps if
  earlier). Pure static `Format(start, end, fmt)`.
- **Inventory cleanup:** `Table` → marked ✅ as **covered by `SimpleTable` + `DataGrid<T>`** (no
  redundant control). **CSS utility classes** → marked ❌ **intentional** (Avalonia is property-based:
  `Margin`/`Padding`/`Spacing`/`Paper.Elevation`; no utility-class layer).
- Tests: `Ripple.MaxReach` (3-4-5 etc.), `DateRangePicker.Format` (partial/full), range display.
  Gallery: a Ripple surface + two date-range pickers.

**MILESTONE — every row of the component inventory is now resolved** (✅ done / 🟦 done-with-noted-
enhancements / ❌ intentional-non-map). No ⬜ remain. Per-row "⬜ enhancement" notes (animations,
StackedBar/TimeSeries, DataGrid filter/group/edit, picker clock-face/HSV, full range highlight) are
deliberate v1 scope cuts, all documented.

---

## 2026-06-03 — Phase 9 — Chart family (Pie/Donut/Bar/Line) 🟦 (ChartTests 2/2) — ALL COMPONENTS MAPPED

**Done** (build green; ChartTests 2/2 isolated)
- **`Charts`** static — categorical `Palette` + pure `SliceSweeps(values)` (→ degrees summing 360) and
  `BarHeights(values, maxPixels)` (scale to max). Unit-tested.
- **`ChartBase`** (`: Control`) — `Values`/`Colors` with render invalidation.
- **`PieChart`** — `Render` draws a `StreamGeometry` slice per value (center→arc→close); `Donut` overlays
  a `Surface`-colored hole (`HoleRatio`, resolved via `AnonObserver` on the `Surface` token).
- **`BarChart`** — scaled vertical rounded bars. **`LineChart`** — polyline + dots, optional `Area` fill
  (translucent). New reusable `Loam.Internal.AnonObserver<T>`.
- Tests: `SliceSweeps`/`BarHeights` math + all three charts render without throwing. Gallery: a
  "Charts" wrap (pie/donut/bar/line).

**Milestone:** with the chart family in, **every reference component on the master inventory is now
mapped** (most ✅/🟦; remaining work is documented per-row enhancements, e.g. StackedBar/TimeSeries,
DataGrid filter/group/edit, picker HSV/clock-face, animations). Charts live in `Loam.Controls` for now;
extracting a separate `Loam.Charts` package is the only structural follow-up.

**Learnings:** Custom charts are just `Control.Render` + `StreamGeometry`/`DrawRectangle`/`DrawLine`;
keep the *math* (`SliceSweeps`/`BarHeights`) in a pure static for testability and the *drawing* trivial.
A render-smoke `[AvaloniaFact]` (show + `RunJobs` + assert `Bounds`) catches geometry/throw regressions
cheaply.

---

## 2026-06-03 — Phase 6 — Popover (standalone) ✅ (OverlayTests 6/6)

**Done** (build green; OverlayTests 6/6 isolated)
- **`Popover`** (`: Decorator`) — wraps an Avalonia `Popup`: `Content` (wrapped in an elevated `Paper`),
  two-way `Open` (bound to `Popup.IsOpen`, with `Popup.Closed → Open=false` for light-dismiss),
  `Placement` (`PlacementMode`), `Target` (`PlacementTarget`). The `Popup` is the Decorator's `Child`
  (zero inline footprint). No theme needed. Completes the Popover row ✅.
- Test: `Open=true` materializes the content in the headless overlay (popup → `OverlayPopupHost` is a
  visual descendant). Gallery: a button toggling a titled popover.

**Learnings:** `PlacementMode` lives in **`Avalonia.Controls`** (not `…Primitives` or
`…PopupPositioning`). Avalonia `Popup` is the building block for anchored floating UI — host it as a
control's `Child`, bind `IsOpen`/`Placement`/`PlacementTarget`, and hook `Closed` for two-way
light-dismiss. Headless renders popups via `OverlayPopupHost` into the TopLevel, so their content is
assertable in the visual tree.

**Status:** all non-stretch reference components are now mapped. Only the **`Chart` family** (Phase 9
stretch, separate `Loam.Charts` package) remains ⏸️.

---

## 2026-06-03 — Phase 5 — Mask + MaskedTextField 🟦 (InputTests 21/21)

**Done** (build green; InputTests 21/21 isolated)
- **`Mask`** (static) — `Apply(raw, pattern)` projects raw input onto a pattern: `#`=digit, `A`=letter,
  `*`=letter/digit, everything else a literal auto-inserted; trailing literals are dropped once raw
  runs out, and an already-typed literal is consumed (so re-masking is idempotent). Pure + unit-tested.
- **`MaskedTextField`** (`: TextField`) — adds a `Pattern` and reformats `Text` through `Mask.Apply`
  on every change (a `_masking` reentrancy guard stops the self-write from looping). Reuses the
  `TextField` chrome via the inherited style key (no new theme/registration).
- Tests: `Mask.Apply` (phone/letters/literals/empty) + `MaskedTextField` reformats `1234567890` →
  `(123) 456-7890`. Gallery: a phone `MaskedTextField` added to "Text fields". `Mask` row was `⏸️`,
  now 🟦.

**Next:** standalone `Popover`, `Chart` family — then all non-stretch items are done.

---

## 2026-06-03 — Phase 5 — FileUpload 🟦 (InputTests 19/19)

**Done** (build green; InputTests 19/19 incl. FileUpload, isolated)
- **`FileUpload`** (`: TemplatedControl`) — an upload `Button` (`PART_Button`, `CloudUpload` icon)
  opens the platform picker via `TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync`
  (`AllowMultiple`); picked `Files` (`IStorageFile`) are exposed, their names shown as `Chip`s in
  `PART_Files`, and `FilesSelected` fires. `ShowSelection(names)`/`Clear()` public (also the seam used
  to test the chip display without a real `StorageProvider`). New `CloudUpload` icon. Registered.
- Test: `ShowSelection` renders a chip per name + reveals the strip. Gallery: an attach-files button
  with a count label.

**Learnings:** Platform file picking (`IStorageProvider`) isn't available in headless tests, so split
the control into a testable display seam (`ShowSelection(IReadOnlyList<string>)`) + the async picker
that calls it — the gallery exercises the real picker, the headless test exercises the display.

**Verification note:** full-suite `dotnet test` keeps **hanging environmentally** in this sandbox
(testhost deadlock under process contention), so this slice was verified by running `InputTests` in
isolation (`--filter`, 19/19, 990 ms). Library + gallery build clean.

**Next:** `Mask`, standalone `Popover`, `Chart` family.

---

## 2026-06-03 — Phase 7 — DataGrid<T> 🟦 (102 tests green)

**Done** (build + 102 tests green, Release)
- **`DataGrid<T>`** (`: Decorator`) + **`DataGridColumn<T>`** (`Header`/`Value`/`Format`/`Align`/
  `Sortable`) — typed data grid: clickable sort headers (toggle asc/desc, arrow indicator), `PageSize`
  paging (embeds a `Pagination` wired two-way to `Page`), striping/hover/`Dense`, single-row selection
  (`SelectedItem` + `SelectionChanged`), hosted in an elevated `Paper`. Rebuilds its own `Grid`
  (`AvaGrid`). Pure static `DataGrids.Sort`/`PageCount` for testable logic.
- Tests: `Sort`/`PageCount` helpers; grid renders headers + body cells; row selection. Gallery: a
  paged (4/page), sortable desserts grid.

**Learnings (generic Avalonia controls):** A generic control **cannot** register `StyledProperty`
(Avalonia analyzer **AVP1002**) nor key a `ControlTheme` by closed type, and **CA1000** forbids public
statics on generic types. Solution: build `DataGrid<T>` as a self-rendering `Decorator` with **plain CLR
properties** (setter → `Rebuild()`), and move the static `Sort`/`PageCount` helpers to a **non-generic**
`DataGrids` class (where they're also independently unit-testable). This is the standard pattern for
typed, data-bound Loam controls going forward.

**Next:** `FileUpload`, standalone `Popover`, `Mask`, `Chart` family.

---

## 2026-06-03 — Phase 8 — ColorPicker (palette) 🟦 (99 tests green)

**Done** (build + 99 tests green, Release)
- **`ColorPicker`** (`: TemplatedControl`) — palette mode: outlined box shows the current `Value`
  (`Color`, two-way) as a bordered swatch (`PART_Swatch`) + `#RRGGBB` hex (`PART_Hex`); clicking opens
  a `UniformGrid` flyout of preset Material swatches (`DefaultPalette`, 20 hues+neutrals) that set
  `Value`. `ToHex` static (`#RRGGBB`, upper-case). Registered. Completes the picker trio for now (HSV
  square/hue/alpha deferred).
- Tests: `ToHex` formatting; `Value` drives swatch brush + hex text. Gallery: theme/accent color
  pickers.

**Learnings:** Picker box chrome is now a repeated shape (Select/DatePicker/TimePicker/ColorPicker all
share the outlined `PART_Box` + display + trailing icon). A shared `PickerBox` template helper is a
worthwhile future refactor (tracked) once a 5th picker appears.

**Next:** `DataGrid`, `FileUpload`, standalone `Popover`, `Mask`.

---

## 2026-06-03 — Phase 8 — TimePicker 🟦 (97 tests green)

**Done** (build + 97 tests green, Release — single clean run, no flakiness)
- **`TimePicker`** (`: TemplatedControl`) — same Select/DatePicker outlined box (`PART_Box` +
  `PART_Display`) showing the two-way `Time` (`TimeSpan?`) formatted via
  `DateTime.Today.Add(Time).ToString(TimeFormat)` (so standard time formats / culture AM-PM work).
  Clicking opens a flyout with two scrollable columns (hours 0–23, minutes 0–59 by `MinuteStep`) of
  clickable rows; the selected row in each column is live-highlighted (Primary fill), and picking
  updates `Time` immediately. Reuses one static `HandCursor`. Registered (qualified vs
  `Avalonia.Controls.TimePicker`).
- Test: placeholder → formatted time on `Time` set. Gallery: reminder + 15-min-step standup pickers
  added to the renamed "Date & time pickers" section.

**Next:** `ColorPicker`, `DataGrid`, `FileUpload`, standalone `Popover`.

---

## 2026-06-03 — Phase 8 — DatePicker (self-contained calendar) 🟦 (96 tests green)

**Done** (build + 96 tests green, Release)
- **`MonthCalendar`** (`: Decorator`) — a **self-contained** Material month grid so Loam needs no
  FluentTheme `Calendar`. Header (‹ MMMM yyyy ›) with prev/next month nav, weekday row, and a
  `UniformGrid` (7 cols) of day cells with leading blanks; today gets a Primary outline, the
  `SelectedDate` a Primary fill; clicking a day raises `DateSelected`. Rebuilds on
  `SelectedDate`/`DisplayMonth` change.
- **`DatePicker`** (`: TemplatedControl`) — Select-style outlined box (`PART_Box`) showing the two-way
  `Date` formatted by `DateFormat` (placeholder when null) + a `CalendarToday` icon; clicking opens a
  `MonthCalendar` flyout that sets `Date` on pick. New `CalendarToday`/`Schedule` icons. Registered
  (qualified vs `Avalonia.Controls.DatePicker`).
- Tests: DatePicker placeholder → formatted date; MonthCalendar renders 28 cells for Feb-2026 / 31 for
  Jan. Gallery: empty + preset date pickers.

**Learnings:** Avalonia's `Calendar`/`DatePicker` rely on FluentTheme templates, which would couple a
LoamTheme-only consumer to Fluent — so picker popups are **custom-built** from primitives
(`UniformGrid` + `Border` day cells) to stay self-contained. `UniformGrid { Columns = 7 }` is the clean
month-grid layout (leading blanks = empty `Control`s). `DatePicker`/`Carousel`/`TreeView` all clash
with `Avalonia.Controls` — qualify in `Loam.Theming`.

**Test-infra fix (important):** `ThemingTests` are plain `[Fact]`s that call `new LoamTheme()` with no
headless app. `ListItemTheme`/`NavLinkTheme` created `new Cursor(StandardCursorType.Hand)` **eagerly**
as `ControlTheme` Setter values — the `Cursor` ctor resolves `ICursorFactory`, which only exists once
some `[AvaloniaFact]` has initialized the platform process-wide. So those theming tests passed or
failed by **execution order** (latent, pre-existing). Fix: moved both cursors off the Setter and onto
the `PART_Root` `Border` **inside the template lambda** (runs lazily under a live app), so
`new LoamTheme()` now creates **zero** eager cursors and needs no platform. Rule: never construct
`Cursor` (or other platform-backed objects) at `ControlTheme`-build time — do it in the template
factory. `MonthCalendar` also now reuses one static `HandCursor` instead of one per day cell.

**Next:** `TimePicker`, `ColorPicker`, `DataGrid`, `FileUpload`.

---

## 2026-06-03 — Phase 4 — Hidden + ScrollToTop 🟦 (94 tests green)

**Done** (build + 94 tests green, Release)
- **`Hidden`** (`: Decorator`) — tracks the host window's `Bounds` width (`TopLevel` observable) and
  hides its `Child` when the current `Breakpoints.FromWidth` bucket satisfies the `Mode`
  (Down/Up/Only) rule vs `Breakpoint`. Pure static `IsHiddenAt` holds the rule (unit-tested without
  a tree). `HiddenMode` enum added.
- **`ScrollToTop`** (`: Decorator`, default up-arrow `Fab`) — subscribes to a `Target`
  `ScrollViewer.ScrollChanged`, shows itself once `Offset.Y > VisibleOffset`, and `ScrollToHome()`s on
  click (`OnPointerReleased`). Both need no theme (Decorators).
- Tests: `IsHiddenAt` per mode; ScrollToTop hidden until `scroll.Offset` passes the threshold. Gallery:
  a "Responsive (Hidden)" chip that hides at Sm-and-down, plus a real ScrollToTop wired to the
  Components page's own `ScrollViewer`.

**Learnings:** `TopLevel.GetTopLevel(this).GetObservable(BoundsProperty)` gives the live window-size
stream for breakpoint-reactive controls; wrap it in a small `IObserver<Rect>` (same pattern as the
brush observer). `ScrollViewer` exposes `ScrollChanged` (event) + `ScrollToHome()` + a settable
`Offset` (`Vector`), so scroll-aware UI needs no custom plumbing.

**Next:** pickers (`DatePicker`/`TimePicker`/`ColorPicker`), `DataGrid`, `FileUpload`, `Mask`.

---

## 2026-06-03 — Phase 7 — Carousel 🟦 (92 tests green)

**Done** (build + 92 tests green, Release)
- **`Carousel`** (`: TemplatedControl`) + `CarouselItem` (`Content`) — z-stacked `Panel` template:
  `PART_Content` slide under overlay `PART_Prev`/`PART_Next` arrows and a bottom-center `PART_Bullets`
  strip. Two-way `SelectedIndex`; `Next`/`Previous` wrap modulo count; bullets are clickable, active =
  `Primary` (others `GrayLight`); `ShowArrows`/`ShowBullets` toggle chrome. Registered (qualified vs
  `Avalonia.Controls.Carousel`).
- Test: Next 0→1 swaps content; two Previous wraps 1→0→2. Gallery: a 3-slide carousel.

**Learnings:** A plain `Panel` (z-stack) is the simplest template for overlay UI (content + floating
arrows + bullets) — children overlap by render order, aligned via `Horizontal/VerticalAlignment`, no
Grid needed. `Carousel` clashes with `Avalonia.Controls.Carousel` (qualify in `Loam.Theming`).

**Tooling note:** overlapping background `dotnet test` runs locked `Loam.dll` (MSB3027); kill stray
`testhost`/`dotnet` test processes before re-running. Prefer a single foreground test invocation.

**Next:** `Hidden`, `ScrollToTop`, pickers (`DatePicker`/`TimePicker`/`ColorPicker`), `DataGrid`.

---

## 2026-06-03 — Phase 4/7 — ListSubheader + Spacer (91 tests green)

**Done** (build + 91 tests green, Release)
- **`ListSubheader`** (`: Text`) — muted SemiBold caption with list-aligned padding; binds
  `Foreground` to `TextSecondary` in the ctor (overrides `Text`'s default after its `ApplyColor`).
  Completes the List family ✅.
- **`Spacer`** (`: Control`) — empty stretch control; as the `LastChildFill` child of a `DockPanel`
  (or a star `Grid` cell) it eats the remaining space, pushing docked siblings to the edges. No flex
  panel needed.
- Tests: Spacer fills 340px of a 400px DockPanel after a 60px docked item; ListSubheader is Caption
  with `TextSecondary` (alpha 0x8A light). Gallery: a mailbox List (subheaders + items) and a
  Spacer-driven toolbar row.

**Learnings:** `Spacer`'s flex-grow has no StackPanel analog, but the `DockPanel` fill child is the
idiomatic Avalonia equivalent — a stretch `Control` as the last (fill) child does the same job.

**Next:** `Hidden`, `ScrollToTop`, `Carousel`, pickers.

---

## 2026-06-03 — Phase 6 — MessageBox ✅ (89 tests green)

**Done** (build + 89 tests green, Release)
- **`MessageBoxAsync`** added to `IDialogService`/`DialogService` — `MessageBoxAsync(title, message,
  yes, no?, cancel?)` returns `Task<bool?>` (true=yes, false=no, null=cancel/dismiss). Builds on the
  existing overlay-layer dialog; omitting `noText`/`cancelText` hides those buttons (Cancel=Text,
  No=Text/Primary, Yes=Filled/Primary). Resolves via `instance.Ok(true/false)`/`Cancel()`.
- Tests: Yes → true, No → false. Gallery: a "Message box" button (Save/Discard/Cancel → snackbar).

**Learnings:** `bool?` tri-state maps cleanly onto the existing `DialogResult` —
`result.Canceled ? null : (bool?)result.Data` — reusing `Ok(data)`/`Cancel()` without new plumbing.

**Next:** `ListSubheader`, `Spacer`/`Hidden`/`ScrollToTop`, `Carousel`, pickers.

---

## 2026-06-03 — Phase 3 — CardHeader + CardMedia ✅ (88 tests green)

**Done** (build + 88 tests green, Release)
- **`CardHeader`** (`: TemplatedControl`) — `DockPanel` header: optional leading `Avatar`
  (`PART_Avatar`), trailing `Action` (`PART_Action`, e.g. an icon button), and a
  `Title`/`Subtitle` stack. All slot content + visibility are bound declaratively in the theme via
  `GetObservable(Prop, selector)` (e.g. `a => a is not null`, `t => !string.IsNullOrEmpty(t)`), so the
  control is just property defs.
- **`CardMedia`** (`: TemplatedControl`) — fixed `MediaHeight` band (`PART_Root`) with a
  `BackgroundGray` placeholder behind a `UniformToFill` `Image` (`PART_Image`) bound to `Source`
  (`IImage`). Both registered. Card family now ✅.
- Tests: CardHeader title/subtitle text + absent avatar hidden; CardMedia root height = `MediaHeight`.
  Gallery: a full Card (header w/ avatar+settings action → media band → content → text actions).

**Learnings:** For slot-based controls, bind both `ContentPresenter.Content` AND `IsVisibleProperty`
in the theme with `GetObservable(Prop, selector)` — keeps the control class declarative (no
`OnApplyTemplate`/`OnPropertyChanged` plumbing). `Layoutable.HeightProperty` (Avalonia.Layout) is the
target for binding a control's height in a template.

**Next:** `ListSubheader`, `Spacer`, `MessageBox`, `Carousel`, pickers.

---

## 2026-06-03 — Phase 3 — ButtonGroup + ToggleIconButton 🟦 (86 tests green)

**Done** (build + 86 tests green, Release)
- **`ButtonGroup`** (`: TemplatedControl`) — connects `Items` (Buttons) in a `PART_Items` stack:
  −1px overlap merges adjacent borders, per-button `CornerRadius` rounds only the outer edges, and
  (when `OverrideChildStyles`) the group's `Variant`/`Color`/`Size` are pushed onto each child.
  `Vertical` switches orientation/corners. Registered `typeof(ButtonGroup)`.
- **`ToggleIconButton`** (`: IconButton`) — reuses the IconButton control theme via the **inherited
  `StyleKeyOverride`** (no new theme/registration). Two-way `Toggled` swaps the displayed glyph
  (`Icon` off ↔ `ToggledIcon` on) via the captured `PART_Icon`; overrides `OnClick()` to flip
  `Toggled`. New `FavoriteBorder` icon.
- Tests: ButtonGroup applies shared Variant/Color + outer-only corners (4,0,0,4 / 0 / 0,4,4,0);
  ToggleIconButton swaps glyph on `Toggled`. Gallery: outlined + filled groups and a favorite toggle.

**Learnings:** A control theme's `CornerRadius` is a *setter* — setting `button.CornerRadius` directly
on the instance (local value) overrides it, which is how ButtonGroup reshapes children without a new
theme. Subclassing a Loam control and **not** overriding `StyleKeyOverride` makes it inherit the base's
style key, so `ToggleIconButton` transparently reuses `IconButton`'s theme. Overriding Avalonia
`Button.OnClick()` is the clean hook for click behavior (raising `ClickEvent` manually does *not* call it).

**Next:** `ToggleIconButton` ToggledColor, `Carousel`, pickers, `MessageBox`, `FileUpload`.

---

## 2026-06-03 — Phase 7 — TreeView 🟦 (84 tests green)

**Done** (build + 84 tests green, Release)
- **`TreeViewItem`** (`: TemplatedControl`) — node row (`PART_Row`): expander `PART_Chevron`
  (rotates 0/−90; `Opacity` 0 for leaves so labels stay aligned) + optional `PART_Icon` + `PART_Text`,
  over an indented `PART_Children` stack. `Expanded` toggles children; hover/`IsSelected` tint the row.
  Chevron click toggles expand (`Handled` so it doesn't select); row click raises the bubbling
  `ItemSelectedEvent`.
- **`TreeView`** (`: TemplatedControl`) — hosts root `Items`, `AddHandler(ItemSelectedEvent)` to set
  `SelectedItem` and `ApplySelection` (walk descendants, single highlight). Both registered (qualified
  vs `Avalonia.Controls.TreeView`/`TreeViewItem`).
- Test: collapsed children hidden → expand shows them; selecting a child highlights only it. Gallery:
  a file-tree (src › Components/Theming) with the root expanded.

**Learnings:** Bubbling `RoutedEvent` is the clean way for a container to learn which descendant was
activated — child raises `RaiseEvent(new RoutedEventArgs(SomeEvent))`, parent `AddHandler`s it and
reads `e.Source`. `TreeView`/`TreeViewItem` clash with `Avalonia.Controls` (qualify, like `Button`).

**Next:** `ButtonGroup`, `Carousel`, `ToggleIconButton`, pickers.

---

## 2026-06-03 — Phase 6 — Overlay 🟦 (83 tests green)

**Done** (build + 83 tests green, Release)
- **`Overlay`** (`: ContentControl`) — parent-filling translucent scrim (`PART_Scrim`, darker when
  `DarkBackground`: 0x99 vs 0x22 black) wrapping centered `PART_Content`. Two-way `Visible` drives the
  control's `IsVisible`; `AutoClose` + `OnClick` fire only when the *scrim itself* is clicked (source
  check ignores bubbling from centered content). Registered `typeof(Overlay)`.
- Test: `Visible` toggles `IsVisible`; dark scrim alpha = 0x99. Gallery: a 320×160 region with a
  "Show overlay" button revealing a spinner-in-scrim (click to dismiss).

**Learnings:** For a click-to-dismiss scrim wrapping interactive content, gate the close on
`ReferenceEquals(e.Source, scrim)` so clicks on the centered content (which bubble up) don't dismiss.
Use `GetObservable(Prop, selector)` to map a bool property to a brush directly in a template binding.

**Next:** `ButtonGroup`, `TreeView`, `Carousel`, `ToggleIconButton`.

---

## 2026-06-03 — Phase 6 — Collapse 🟦 (82 tests green)

**Done** (build + 82 tests green, Release)
- **`Collapse`** (`: Decorator`) — two-way `Expanded` toggles `MaxHeight` between `∞` and `0` with
  `ClipToBounds`, hiding/revealing the `Child` while keeping it measured. No theme (Decorator). Slide
  animation deferred (consistent with other "animation pending" notes).
- Test: `MaxHeight==0` collapsed, `∞` when `Expanded`. Gallery: a button toggling a Collapse'd Paper.

**Next:** `ButtonGroup`, `TreeView`, `Carousel`, `Overlay`.

---

## 2026-06-03 — Phase 3 — ChipSet ✅ (81 tests green)

**Done** (build + 81 tests green, Release)
- **`ChipSet`** (`: TemplatedControl`) — wraps `Items` (`Chip`s) in a `PART_Items` `WrapPanel`. When
  `Selectable`, clicking a chip sets two-way `SelectedIndex` (toggles off unless `Mandatory`) and
  re-renders selected = `Variant.Filled`, others `Variant.Outlined`. Chip click handlers hooked once
  per chip (`HashSet` guard) with index resolved live via `Items.IndexOf`. Registered
  `typeof(ChipSet)`. Chip family now ✅.
- Test: `SelectedIndex=1` → chip 1 Filled, others Outlined. Gallery: a mandatory selectable filter
  set (All/Active/Archived/Draft).

**Learnings:** When re-adding the same control instances to a panel on every rebuild, guard event
subscriptions with a `HashSet<T>` of already-hooked instances and resolve the index *inside* the
handler (`Items.IndexOf`) so reorders/insertions stay correct without re-subscribing.

**Next:** `ButtonGroup`, `Collapse`, `TreeView`, `Carousel`.

---

## 2026-06-03 — Phase 3 — AvatarGroup ✅ (80 tests green)

**Done** (build + 80 tests green, Release)
- **`AvatarGroup`** (`: TemplatedControl`) — overlapping avatar cluster: shows up to `Max` of `Items`
  with a negative-`Spacing` overlap; the remainder collapse into a trailing "+N" surplus `Avatar`
  (inheriting the first avatar's `Size`/`Square`/`Rounded`). Registered `typeof(AvatarGroup)`. Avatar
  family now ✅.
- Test: 6 avatars / `Max=4` → 5 children, last is "+2". Gallery: a 5-avatar group (`Max=3`) added to
  the Avatars section.

**Next:** `ChipSet`, `ButtonGroup`, `Collapse`, `TreeView`.

---

## 2026-06-03 — Phase 7 — Pagination 🟦 (79 tests green)

**Done** (build + 79 tests green, Release)
- **`Pagination`** (`: TemplatedControl`) — pure static `BuildPages(count, selected, boundary, middle)`
  computes the layout: boundary pages at each end + a centered `MiddleCount` window (shifted to stay
  in range near edges), with `0` marking ellipsis gaps and single hidden pages filled rather than
  ellipsed. `Rebuild` renders prev/next `IconButton` arrows (disabled at bounds) + page `Button`s
  (selected → Filled `Color`) + `…` text. Two-way `Selected`. New `ArrowForward` icon. Registered
  `typeof(Pagination)`.
- Tests: `BuildPages` window/ellipsis/single-gap cases + prev disabled on page 1 / next advances.
  Gallery: two paginations (10 & 12 pages).

**Learnings:** Keeping the page-layout math in a pure static (`BuildPages`) makes the tricky
windowing fully unit-testable without any visual tree; the control's `Rebuild` is then a thin render
of that list. Private builder methods with no `this` access trip **CA1822** → mark `static`.

**Next:** `Collapse`, `TreeView`, `Carousel`, `ButtonGroup`.

---

## 2026-06-03 — Phase 5 — ToggleGroup 🟦 (77 tests green)

**Done** (build + 77 tests green, Release)
- **`ToggleGroup`** (`: TemplatedControl`) + `ToggleItem` (`Text`/`Value`) — segmented single-select:
  connected `Border` segments (left-divider between) inside a rounded, clipped `PART_Root` outline.
  The segment whose value equals the two-way `SelectedValue` fills `Color` (`Palette`) with contrast
  text (`PaletteContrast`); others are transparent with `TextPrimary`. Click sets `SelectedValue`.
  Registered `typeof(ToggleGroup)`.
- Test: selected segment fills `#594AE2`, moves on `SelectedValue` change (prior goes transparent).
  Gallery: Day/Week/Month "Toggle group".

**Next:** `Collapse`, `TreeView`, `Carousel`/`Pagination`, `ButtonGroup`.

---

## 2026-06-03 — Phase 8 — Stepper 🟦 (76 tests green)

**Done** (build + 76 tests green, Release)
- **`Stepper`** (`: TemplatedControl`) + `Step` (`Title`/`Content`/`Completed`) — numbered marker
  header (`PART_Header`) with connector lines; active/completed markers fill Primary (check icon when
  done, number otherwise), future markers are `ActionDisabledBackground`/secondary. Active step body
  in `PART_Content`; `PART_Back`/`PART_Next` buttons drive `Previous()`/`Next()` (Next label flips to
  "Finish" on the last step, firing `OnCompleted`). Two-way `ActiveIndex`. Registered `typeof(Stepper)`.
- Tests: Next advances + marks completed + swaps content, Previous goes back; finish on last step
  fires `OnCompleted`. Gallery: 3-step "Stepper" wizard.

**Learnings:** `Step` trips **CA1716** (VB `For…Step` keyword) → `[SuppressMessage]` like `Select`
(running list of VB-keyword type names: `Select`, `Step`). Inside `Loam.Controls`, bare `Button`
resolves to `Loam.Controls.Button` (no qualification needed in same-namespace theme files).

**Next:** `Collapse`, `TreeView`, `Carousel`/`Pagination`, `ToggleGroup`.

---

## 2026-06-03 — Phase 8 — NavGroup 🟦 (74 tests green)

**Done** (build + 74 tests green, Release)
- **`NavGroup`** (`: TemplatedControl`) — collapsible nav section: a clickable `PART_Header`
  (icon + `PART_Title` + rotating `PART_Chevron`) toggles `Expanded`, which shows/hides the indented
  nested `PART_Items` (`Items` collection of `Control`/`NavLink`, left-indented 16px on rebuild).
  Registered `typeof(NavGroup)`. Completes the `NavMenu`/`NavLink`/`NavGroup` trio.
- Test: nested items count + visibility toggles with `Expanded`. Gallery: an "Admin" group with
  Users/Roles links inside the NavMenu.

**Next:** `Stepper`, `Collapse`, `TreeView`, `Carousel`/`Pagination`.

---

## 2026-06-03 — Phase 7 — Timeline 🟦 (73 tests green)

**Done** (build + 73 tests green, Release)
- **`Timeline`** (`: Decorator`) + `TimelineItem` (`Content`/`Color`) — builds an
  `Avalonia.Controls.Grid` (marker column + content column): a continuous `Divider` connector line
  spans all rows behind per-item colored dots (`SemanticColor.Resolve(Color).Fill`), each beside a
  `Paper` content card. No `ControlTheme` (Decorator hosts the built grid as `Child`).
- Test: 2 items → 2×2 grid, 2 cards, content text present. Gallery: "Timeline" (order-status trail).

**Learnings:** `Decorator` is the lightest base for a control that *builds* its own visual tree
(no template/`PART_` wiring, no `ControlTheme` registration) — set `Child` on rebuild. Same `AvaGrid`
alias caveat as `SimpleTable` for the layout `Grid` inside `Loam.Controls`.

**Next:** `NavGroup`, `Stepper`, `Collapse`, `TreeView`.

---

## 2026-06-03 — Phase 5 — Autocomplete 🟦 (72 tests green)

**Done** (build + 72 tests green, Release)
- **`Autocomplete`** (`: TemplatedControl`) — **composes** a `TextField` (`PART_Field`) instead of
  duplicating chrome: the theme forwards `Label`/`Placeholder`/`Variant`/`Color` via bindings and the
  control two-way binds `TextField.Text ↔ Value`. Typing filters `Items`
  (`Filter` = case-insensitive `Contains`, capped at `MaxItems`) and shows a reused `Flyout` of
  `ListItem` matches; choosing one sets `Value` (guarded by `_selecting` so it doesn't reopen).
  Registered `typeof(Autocomplete)`.
- Tests: `Filter` static (contains/cap/empty) + `Value` fills the inner field. Gallery: fruit
  autocomplete added to "Text fields".

**Learnings:** Composition beats copying for field-style controls — hosting a `TextField` in the
template and forwarding props via `GetObservable` bindings avoided a *4th* copy of the chrome logic.
Driving filtering off the control's own two-way `Value` change (`OnPropertyChanged`) sidesteps
observer plumbing on the inner field's `TextProperty`; a `_selecting` reentrancy flag stops the
selection write from reopening the flyout. `GetObservable` lives in the `Avalonia` namespace.

**Next:** `NavGroup`, `Stepper`, `Timeline`, `Collapse`/`Overlay`.

---

## 2026-06-03 — Phase 8 — NavMenu + NavLink 🟦 (70 tests green)

**Done** (build + 70 tests green, Release)
- **`NavLink`** (`: ContentControl`) — clickable row: leading `Icon` + content label in a rounded
  `PART_Root`. `IsActive` → accent background (`PaletteHover`) + accent foreground/icon (`Color`,
  default Primary); inactive tints `LinesDefault` on hover. Click invokes `OnClick` + launches `Href`.
  Foreground driven on the `ContentPresenter` via `TextElement.ForegroundProperty`; icon color set
  via the `Color` enum. Registered `typeof(NavLink)`.
- **`NavMenu`** (`: StackPanel`) — vertical nav container (mirrors `List`).
- Tests: active tints icon (Primary) + non-transparent background; inactive icon is Default. Gallery:
  NavMenu (Dashboard/People/Settings, click-to-activate) added to the Navigation section.

**Learnings:** `NavGroup` (collapsible) deferred. For a custom control, drive inherited text/icon
color by binding `TextElement.ForegroundProperty` on the inner `ContentPresenter` — child `Icon`s with
`Color.Inherit` pick it up; an `Icon` with an explicit `Color` enum must be set directly (it doesn't
inherit once a non-Inherit color is bound).

**Next:** `Autocomplete`, `NavGroup`, `Stepper`, `Timeline`.

---

## 2026-06-03 — Phase 5 — Rating 🟦 (68 tests green)

**Done** (build + 68 tests green, Release)
- **`Rating`** (`: TemplatedControl`) — builds `MaxValue` star `Icon`s into `PART_Stars`. Two-way
  `SelectedValue` fills stars (accent `Color`, default gold/`Warning`); empties are `Default` at 0.3
  opacity. Live hover preview (`PointerEntered` per star sets a transient `_hover`; `PointerExited`
  on the strip clears it). `Size`, `ReadOnly` (no hover/click, default cursor). Registered
  `typeof(Rating)`.
- Test: fill reflects `SelectedValue` and updates on change. Gallery: "Rating" section (interactive,
  6-star, read-only small).

**Learnings:** Hover-preview pattern for icon strips — keep a transient `_hover` index separate from
the bound value; `effective = _hover > 0 ? _hover : SelectedValue`; clear `_hover` on the *container's*
`PointerExited` (fires once when leaving the whole strip) rather than per-star exit.

**Next:** `Autocomplete`, `NavMenu`/`NavLink`, `Stepper`, `Timeline`.

---

## 2026-06-03 — Phase 8 — Navigation: Link + Breadcrumbs 🟦 (67 tests green)

**Done** (build + 67 tests green, Release)
- **`Link`** (`: Text`) — reuses `Text` rendering; `Color` defaults to Primary, hand cursor, hover
  underline (always when `Underline`). Click (`OnPointerReleased`, left only) invokes `OnClick` and
  launches `Href` via `TopLevel.GetTopLevel(this).Launcher.LaunchUriAsync`. No `ControlTheme` (inherits
  `Text`'s style key).
- **`Breadcrumbs`** (`: TemplatedControl`) + `BreadcrumbItem` (`Text`/`OnClick`/`Href`/`Disabled`) —
  fills a horizontal `PART_Items` strip: non-last entries become `Link`s, the last is the muted
  current page, joined by a faint `Separator` (default `/`). Registered `typeof(Breadcrumbs)`.
- Tests: Link default color + underline toggle; Breadcrumbs child layout (5 children for 3 items,
  2 links, current tail Text). Gallery: "Navigation (breadcrumbs / link)" section.

**Learnings:** Deriving an interactive control from `Text` (a `TextBlock`) is a clean way to get a
styled clickable label — override `OnPointerReleased`/`OnPointerEntered`/`OnPointerExited` and set
`TextDecorations`. `TopLevel.GetTopLevel(this)?.Launcher` is the Avalonia way to open URLs (works on
desktop; no-op where unsupported). `TopLevel` lives in `Avalonia.Controls`.

**Next:** `Rating`, `Autocomplete`, `NavMenu`/`NavLink`, `Stepper`.

---

## 2026-06-03 — Phase 5 — NumericField 🟦 (65 tests green)

**Done** (build + 65 tests green, Release)
- **`NumericField`** (`: TemplatedControl`) — reuses the `TextField` Material chrome
  (label/variant border/helper/error, copied for now) and docks a vertical spinner
  (`PART_Up`/`PART_Down`, small chevron `Border`s) right of a borderless `TextBox`. Two-way `Value`
  clamped to `Minimum`/`Maximum`; `Step` (spinners + `Bump`); `Format` string; text↔value parse via
  `double.TryParse`/`ToString` (current culture). Reformats on blur. New `ExpandLess` icon for the up
  spinner. Registered `typeof(NumericField)` in `LoamTheme`.
- Tests: `Clamp` static, value clamps + text reflects, text parses into value. Gallery: two numeric
  fields (qty + price w/ `Format`) added to "Text fields".

**Learnings:** Reentrancy guard pattern for two-way text/value sync — a `_updatingText` bool gates
both `OnTextChanged` (text→value) and `UpdateText` (value→text) so neither re-triggers the other.
The chrome logic is now duplicated between `TextField` and `NumericField`; extract a shared `Field`
base when a 3rd field-style control appears (tracked in inventory).

**Next:** `Autocomplete`, `Breadcrumbs`/`Link`, `NavMenu`/`NavLink`, `Rating`.

---

## 2026-06-03 — Phase 7 — ExpansionPanels (accordion) 🟦 (62 tests green)

**Done** (build + 62 tests green, Release)
- **`ExpansionPanel`** (`: HeaderedContentControl`) — reuses `Header`/`Content`; adds two-way
  `IsExpanded`. Template: clickable `PART_Header` (header presenter + `PART_Chevron` that rotates
  180° on expand) over a `PART_Content` presenter whose `IsVisible` follows `IsExpanded`; bottom
  `Divider` rule.
- **`ExpansionPanels`** (`: TemplatedControl`) — hosts `Panels` in a `PART_Stack` inside an elevated
  `Paper`. `MultiExpansion` (default false) → accordion: subscribes each panel's `PropertyChanged`
  and collapses siblings when one expands. Both registered in `LoamTheme`.
- Tests: single-panel content visibility toggle + accordion sibling-collapse. Gallery:
  "Expansion panels" section.

**Learnings:** `HeaderedContentControl` (Avalonia.Controls) gives `Header`+`Content` for free — ideal
base for header/body controls. Cross-control coordination is clean via the public
`AvaloniaObject.PropertyChanged` event + `e.GetNewValue<bool>()` (no observer plumbing needed);
remember to unsubscribe stale panels on rebuild.

**Next:** `NumericField`, `Autocomplete`, `Breadcrumbs`/`Link`, `NavMenu`.

---

## 2026-06-03 — Phase 6 — ProgressCircular 🟦 (60 tests green)

**Done** (build + 60 tests green, Release)
- **`ProgressCircular`** (`: Control`, custom `Render`) — draws an arc via `StreamGeometry`/`ArcTo`.
  Determinate: sweep `Fraction(Value,Min,Max)*360` from top over a faint `Divider` track.
  Indeterminate (default): a 90° arc spun by an infinite `Animation` on a `SpinAngle` styled
  property (`AffectsRender`). `Color` (semantic), `Size` (24/40/56), `StrokeWidth`. No `ControlTheme`
  needed (plain `Control`). Brushes pulled from token observables via a small `IObserver` adapter.
- Tests: `Fraction`/`Diameter` statics + measures-to-diameter. Gallery: "Progress" section
  (spinners + determinate rings + linear bars).

**Learnings:** `Animation.RunAsync(this, token)` is the Avalonia 12 signature (no `IClock` arg).
A `CancellationTokenSource` field trips **CA1001** (control isn't `IDisposable`) → suppress
(lifecycle disposes it in `OnDetachedFromVisualTree`). `GetResourceObservable` yields `object?`
(brush) — wrap in a tiny `IObserver<object?>` to push into a render field + `InvalidateVisual`.

**Next:** `ExpansionPanels`, `NumericField`, `Autocomplete`, `Breadcrumbs`/`Link`.

---

## 2026-06-03 — Phase 7 — SimpleTable 🟦 (58 tests green)

**Done** (build + 58 tests green, Release)
- **`SimpleTable`** (`: TemplatedControl`) + `TableRow` (`Cells: IList<object?>`) — data-driven
  `Headers`/`Rows`; cells are strings (→ `Text`) or any `Control` (hosted). Builds an
  `Avalonia.Controls.Grid` (star columns) hosted in an elevated `Paper` (`PART_Paper`). Bools:
  `Striped` (alternate rows → `TableStriped`), `Hover` (pointer-over → `TableHover` via per-row
  background `Border`), `Bordered` (`TableLines` gridlines + header rule), `Dense` (compact padding),
  `Elevation`. Registered `typeof(SimpleTable)` in `LoamTheme`.
- Test: 2 headers + 2 rows → 2 cols / 3 grid rows, header+cell text present. Gallery: "Table"
  section (striped + hover desserts table).

**Learnings:** Inside `Loam.Controls`, bare `Grid` binds to **`Loam.Controls.Grid`** (our layout
control), so the layout `Grid` needs alias `using AvaGrid = Avalonia.Controls.Grid;` — including the
static `AvaGrid.SetRow/SetColumn/SetColumnSpan`. Table row backgrounds get a transparent `Border`
spanning all columns (added before cells) so they hit-test for hover.

**Next:** `ExpansionPanels`, `ProgressCircular`, `NumericField`, `Autocomplete`.

---

## 2026-06-03 — Phase 5 — Select 🟦 (57 tests green)

**Done** (build + 57 tests green, Release)
- **`Select`** (`: TemplatedControl`) + `SelectItem` (`Text`/`Value`) — outlined box (`PART_Box`,
  border bound to `LinesInputs`) showing the selected option (`PART_Display`) + a chevron
  (`Icons.Material.Filled.ExpandMore`, new). Click opens a `Flyout` (`Paper` elevation 8) of
  `ListItem` rows; choosing one sets two-way `Value` and hides the flyout. `Label`/`Placeholder`
  supported; display foreground swaps `TextPrimary` (selected) / `TextSecondary` (placeholder).
- Registered `typeof(Select)` in `LoamTheme.RegisterControlThemes`.
- Test: placeholder → selected text on `Value` change. Gallery: "Select" section (country + size).

**Learnings:** `Select` trips **CA1716** (VB reserved keyword) → `[SuppressMessage]` on the type
(deliberate, mirrors `Select`). Icon path constants are nested under
`Icons.Material.Filled.*`, not flat on `Icons`. `Select` is clash-free vs `Avalonia.Controls`
(their dropdown is `ComboBox`).

**Next:** `Table`/`SimpleTable`, then `ExpansionPanels`, `ProgressCircular`, `NumericField`.

---

## 2026-06-03 — Phase 6/7 — Tabs, Menu, Tooltip 🟦 (56 tests green)

**Done** (build + 56 tests green, Debug & Release)
- **`Tabs`** (`: TemplatedControl`) + `TabItem` — header strip (active underline in `Color`) + content
  switch; tested (selection swaps content).
- **`Menu`** (`: Button`) + `MenuItem` — opens an Avalonia `Flyout` of icon+label rows (`OnClick`).
- **`Tooltip`** static helper — `Tooltip.Set(control, text)` → styled `Paper` via `ToolTip.SetTip`.
- Gallery: "Tabs / Menu / Tooltip" section.

**Name clashes (learnings):** `TabItem`, `Menu`, `MenuItem` also exist in `Avalonia.Controls` →
qualify `Loam.Controls.*` in mixed files (joins Button/Grid/CheckBox/Slider). `Tabs` is clash-free.

**Next:** `Select` (flyout dropdown), `Table`/`SimpleTable`, `ExpansionPanels`, `ProgressCircular`.

---

## 2026-06-03 — Phase 6 — Dialog + Snackbar (overlay services) 🟦 (54 tests green)

**Done** (build + 54 tests green, Debug & Release)
- Verified `OverlayLayer : Canvas` (measures full-size, arranges Canvas-style → modal root must be
  sized to the layer; bind to `BoundsProperty`). Recipe recorded here.
- **`IDialogService`/`DialogService`** (`DialogService.For(visual)`): renders scrim + centered
  `Paper` dialog in the window's `OverlayLayer`. `ShowAsync(title, Func<DialogInstance,Control>)`
  returns `Task<DialogResult>`; `DialogInstance.Ok/Cancel/Close` closes + resolves. `ConfirmAsync`
  convenience. `DialogResult`/`DialogOptions`. No provider component needed (uses overlay layer).
- **`ISnackbar`/`SnackbarService`** (`SnackbarService.For(visual)`): `Add(message, severity, duration)`
  stacks auto-dismissing `Alert` toasts bottom-right in the overlay layer (DispatcherTimer).
- Tested: dialog shows in overlay + closes with typed result + clears overlay; Confirm OK→true;
  snackbar adds toast. Gallery: "Overlays" section (confirm dialog + snackbar buttons).

**Learning:** to fill the overlay, bind the modal root's `Width`/`Height` to
`layer.GetObservable(Visual.BoundsProperty, b => b.Width/Height)` (OverlayLayer/Canvas doesn't stretch).

**Next:** `Menu`/`Popover`/`Tooltip` (reuse overlay), `Select` (popup), `Tabs`, `Table`.

---

## 2026-06-03 — Phase 5 finish + Phase 6/7 start 🟢 (51 tests green)

Autonomous batch run (user: "continue, don't stop until full implementation"). Commits
`ddb807a` (Phase 5 part 3) and `3850904` (feedback/structure).

**Phase 5 inputs — complete enough:**
- `Radio` (`: RadioButton`) + `RadioGroup` (`: Decorator`, two-way `Value` selection).
- `Slider` (`: TemplatedControl`) — custom draggable track/fill/thumb (pointer capture).
- `Form` (`: Decorator`) `Validate()` aggregating field validation; `TextField.Required`/
  `Validation`/`Validate()` + self-validate on blur.

**Phase 6 feedback start:** `Alert` (severity color + variants + icon), `ProgressLinear`
(determinate fill), `Skeleton` (placeholder).

**Phase 7 structure start:** `Card`/`CardContent`/`CardActions` (Paper-based), `List`/`ListItem`
(`: ContentControl`, hover highlight + icon).

**Deferred (big remaining surface):** `Dialog`+`IDialogService`, `Snackbar`+`ISnackbar` (overlay
infra), `Select`/`Autocomplete`/`NumericField`, `Tabs`/`ExpansionPanels`/`Table`/`DataGrid`/
`TreeView`, `Menu`/`Popover`/`Tooltip`/`ProgressCircular`, navigation (`NavMenu`/`Breadcrumbs`/
`Stepper`), pickers (`Date`/`Time`/`Color`), `Timeline`/`Carousel`/`Pagination`, charts, ripple,
`ChipSet`/`AvatarGroup`/`ButtonGroup`, `Hidden`/`Spacer`/`ScrollToTop`. These are the bulk of the
remaining multi-session effort.

**Verification gap** — gallery GUI not launched here.

---

## 2026-06-03 — Phase 5 (part 2) — TextField 🟦

**Done** (build + 44 tests green, Debug & Release)
- Source-checked TextField params (v9.5.0).
- **`TextField`** (`: TemplatedControl`) wrapping a borderless Avalonia `TextBox` with built-in
  Field chrome: `Label`, `HelperText`/`ErrorText`, `Placeholder`, `Text` (two-way), `Variant`
  (Text/Filled/Outlined), `Color` focus accent, `Error`, `ReadOnly`. Focus + error recolor the
  border; error text replaces helper text. Tested: two-way binding, outlined border + label, error color.
- Gallery: "Text fields" section (4 variants incl. error state).

**Gotcha (learnings):** Avalonia 12 renamed `TextBox.Watermark` → `PlaceholderText` (obsolete = error
under warnings-as-errors).

**Deferred to Phase 5 part 3+**: `NumericField`, `Select`/`Autocomplete`, `Radio`/`RadioGroup`,
`Slider`, `Rating`, `Form` + validation engine, `FileUpload`, floating-label animation, adornments.

**Verification gap** — gallery GUI not launched here; `dotnet run --project samples/Loam.Gallery`.

---

## 2026-06-03 — Phase 5 (part 1) — Boolean inputs (CheckBox, Switch) 🟦

**Done** (build + 41 tests green, Debug & Release)
- Source-checked CheckBox/Switch params (v9.5.0).
- **`CheckBox`** (`: Avalonia CheckBox`) — `Color`/`Size`; Material box + check `Path`, token-filled
  when checked, outlined when not; disabled dims. Tested.
- **`Switch`** (`: ToggleButton`) — `Color`/`Size`; track + thumb that slides on checked; thumb
  elevation. Tested (thumb alignment per state).
- Gallery: "Selection" section (checkboxes + switches).

**Gotchas (learnings):** `ToggleSwitch` expects built-in knob template parts → subclass
`ToggleButton` instead. `Path` clashes with `System.IO.Path` → alias `AvaPath`. `Loam.Controls.CheckBox`
clashes with `Avalonia.Controls.CheckBox` → qualify.

**Deferred to Phase 5 part 2+**: `TextField`/`Field` (+ floating label, variants), `NumericField`,
`Select`/`Autocomplete`, `RadioGroup`/`Radio`, `Slider`, `Rating`, `Form` + validation, `FileUpload`.

**Verification gap** — gallery GUI not launched here; `dotnet run --project samples/Loam.Gallery`.

---

## 2026-06-03 — Phase 4 (part 2) — App shell (Layout, AppBar, Drawer, MainContent) 🟦

**Done** (build + 38 tests green, Debug & Release)
- Source-checked AppBar/Drawer params (v9.5.0).
- **`AppBar`** (`: ContentControl`) — `Color`/`Elevation`/`Dense`; default uses app-bar palette,
  semantic color overrides; elevation shadow. Imperative token color.
- **`Drawer`** (`: ContentControl`) — `Open`/`Mini`/`DrawerWidth`/`MiniWidth`; **slides** via a Width
  transition; drawer-colored panel with a right divider edge. `ResolveWidth` is a pure, tested helper.
- **`MainContent`** (`: ContentControl`) — padded scroll viewer.
- **`Layout`** (`: ContentControl`) — `AppBar`/`Drawer` slots + content; DockPanel template (app bar
  top, drawer left, content fills).
- Gallery: **App Shell** tab — working menu button toggles the drawer over a dashboard.

**Deferred**: `Spacer`, `Hidden`, `ScrollToTop`, drawer responsive/temporary/overlay variants, ripple,
`Card`, `ChipSet`, `ButtonGroup`. (Phase 4 layout + shell foundation now in place.)

**Verification gap** — gallery GUI not launched here; `dotnet run --project samples/Loam.Gallery`.

---

## 2026-06-02 — Phase 4 (part 1) — Layout primitives 🟦

**Done** (build + 33 tests green, Debug & Release)
- Source-checked Container/Grid/Item/Stack params (v9.5.0).
- **`Breakpoint`** enum + **`Breakpoints`** helper (xs–xxl thresholds; container-width / container-query
  based, not a global viewport — documented divergence).
- **`Grid`** (`: Panel`) + **`Item`** (`: Decorator`, `Xs`–`Xxl` spans) — responsive 12-column layout
  via custom `MeasureOverride`/`ArrangeOverride`; tested (half-items share a row at md, stack full
  width at xs).
- **`Container`** (`: Decorator`) — `MaxWidthBreakpoint`/`Gutters`; caps + centers content.
- **`Stack`** (`: StackPanel`) — `Row` toggles orientation; default spacing 8.
- Gallery: new **Layout** tab (responsive grid, stacks, container).

**Deferred to Phase 4 part 2**: `Spacer`, `Hidden`, app-shell (`Layout`/`AppBar`/`Drawer`/
`MainContent`), `ScrollToTop`.

**Verification gap** — gallery GUI not launched here; `dotnet run --project samples/Loam.Gallery`.

---

## 2026-06-02 — Phase 3 (part 3) — Display primitives (Avatar, Chip, Badge) 🟦

**Done** (build + 28 tests green, Debug & Release)
- Source-checked Avatar/Chip/Badge params (v9.5.0).
- Extracted `SemanticColor.Resolve` (color→token mapping) into a shared internal helper; ButtonStyles
  now delegates to it. Reused by the display primitives.
- **`Avatar`** (`: ContentControl`) — `Variant`/`Color`/`Size`/`Square`/`Rounded`; circular by
  default; imperative token color.
- **`Chip`** (`: TemplatedControl`) — `Text`/`Icon`/`Variant`/`Color`/`Size`/`Label`, optional close
  button (`Closeable` → `Closed` event); pill or label shape.
- **`Badge`** (`: ContentControl`) — overlays `Value`/`Dot` on `Content`; `Color`/`Origin`/`Overlap`/
  `Bordered`/`Max`/`Visible`; `BadgeOrigin` enum (4 corners). `150`+`Max=99` → `99+`.
- Registered Avatar/Chip/Badge themes; gallery sections for each.

**Deferred to Phase 4 / later**: `ChipSet`, animated `Ripple`, `:pressed`, `ButtonGroup`,
`ToggleIconButton`, `Card`(+parts). (Phase 3 core primitives are now broadly covered.)

**Verification gap** — gallery GUI not launched here; `dotnet run --project samples/Loam.Gallery`.

---

## 2026-06-02 — Phase 3 (part 2) — Icon & button family 🟦

**Done** (build + 24 tests green, Debug & Release)
- Source-checked Icon/IconButton/Fab params (v9.5.0); verified `Geometry.Parse`.
- **`Icon`** (`: Control`, custom-drawn) — `Data` (path), `Color`, `Size`, `ViewBox`; inherits
  `Foreground` so it picks up button text color. ADR-0006 finalized.
- **`Icons.Material.Filled.*`** — curated core set (~13 glyphs) mirroring the reference API's structure.
- **`Button.StartIcon`/`EndIcon`** — leading/trailing icons (imperative part wiring).
- **`IconButton`** (`: Button`) — circular icon-only button, reuses the shared color matrix.
- **`Fab`** (`: Button`) — pill, filled, elevated; `Label` + inherited `StartIcon`.
- **Refactor**: extracted shared `ButtonStyles` (color matrix, disabled, icon-content template)
  reused by Button/IconButton/Fab. Registered IconButton/Fab themes.
- Gallery: icons row, icon buttons, FABs, icon+text buttons.

**Deferred to Phase 3 part 3** (next): animated `Ripple`, `:pressed` feedback, `ButtonGroup`,
`ToggleIconButton`, `Card`(+parts), `Chip`/`ChipSet`, `Badge`, `Avatar`/`AvatarGroup`, full
`Loam.Icons` pack.

**Verification gap** — gallery GUI not launched here; `dotnet run --project samples/Loam.Gallery`.

---

## 2026-06-02 — Phase 3 (part 1) — Core Primitives 🟦

**Done** (build + 19 tests green, Debug & Release)
- Source-checked reference v9.5.0 enums/params (Variant/Size/Color/Typo/Align; Text/Button/
  Paper/Divider).
- **Shared API vocabulary**: `Variant`, `LoamColor`, `LoamSize`, `Typo`, `Align`, `DividerType`
  enums (ADR-0007 naming: `LoamColor`/`LoamSize` avoid `Avalonia.Color`/`Avalonia.Size` clashes;
  property names stay `Color`/`Size`). `LoamColorExtensions.ToPaletteName`.
- **Derived interaction tokens**: per semantic color `.Hover` (color@hoverOpacity) + `.Darken`
  brushes, projected by `LoamTheme`.
- **`Text`** (`: TextBlock`) — `Typo`/`Color`/`GutterBottom`, font+foreground from tokens.
- **`Paper`** (`: ContentControl`) — `Elevation`/`Square`/`Outlined`; replaces the Phase-1 `Surface`
  smoke control (Surface + its test removed).
- **`Divider`** (`: Border`) — `Vertical`/`Light`/`DividerType`.
- **`Button`** (`: Avalonia Button`) — `Variant`×`Color`×`Size`×`FullWidth` + hover/disabled states,
  generated in C# loops via `ControlTheme` nested styles (`PropertyEquals` + pseudo-classes) and
  `DynamicResourceExtension` setters. **Verified working end-to-end.**
- Gallery: `ComponentsView` (Text scale, full button matrix, Paper, Divider) + tabbed `MainWindow`.
- Verified + recorded the Avalonia 12 control-theme C# recipe →
  `findings/2026-06-02-avalonia-controltheme-csharp.md`.

**Deferred to Phase 3 part 2** (next): `Icon` + `Loam.Icons` (ADR-0006), `Button` `StartIcon`/
`EndIcon`, animated `Ripple`, `IconButton`/`Fab`/`ButtonGroup`/`ToggleIconButton`, `Card`(+parts),
`Chip`/`ChipSet`, `Badge`, `Avatar`/`AvatarGroup`, `:pressed` button feedback.

**Verification gap** — gallery GUI not launched here; `dotnet run --project samples/Loam.Gallery`.

---

## 2026-06-02 — Phase 2 — Design System & Theming Engine ✅

**Done**
- Sourced reference **v9.5.0** theme defaults via GitHub API → `findings/2026-06-02-reference-theme-defaults.md`.
- Expanded `LoamTheme` into a full design system (pure C#):
  - `LoamPalette` (record) — full semantic palette, both variants, faithful reference values;
    dark derived via `with`.
  - `LoamTypography` — 14-style Material scale (rem→px, weights, line heights).
  - `LoamShadows` — elevation 0–25 with a CSS→Avalonia `BoxShadows` converter (`ParseCss`).
  - `LoamLayout`, `LoamZIndex`, `LoamColors` (alpha/lighten/darken/contrast/luminance).
  - `LoamThemeData` aggregate (pure data) + `LoamTheme` projection into Light/Dark
    `ThemeDictionaries` (palette) and shared resources (typography/shadows/layout/z-index).
  - Runtime API: `SetData` / `SetPalette` / `SetPrimary` (re-projects, fires resource change).
  - `LoamTokens` expanded; palette projected by reflection over `LoamPalette` props.
- Gallery: `DesignSystemView` — palette swatches, full typography scale, elevation samples, app-bar,
  **light/dark toggle + live primary-color presets** (runtime `SetPrimary`).
- Tests: **14 pass** (Debug) — palette per variant, typography/layout/z-index, elevation,
  `SetPrimary` runtime, custom-data flow, `LoamColors` + `LoamShadows.ParseCss` units.
- Added `.gitattributes` (LF normalization).

**Decisions / fixes**
- Test projection by querying `theme.Resources` directly (control-tree resolution already covered by
  SurfaceThemeTests). See learnings: `control.TryGetResource` vs `theme.Resources`.
- Fixed static-init ordering (LoamShadows `Default` vs `Css`) via static ctor; renamed
  `LoamPalette.Light/Dark` factories to `DefaultLight/DefaultDark` (clash with `Dark` color prop).

**Verified facts** — `findings/2026-06-02-reference-theme-defaults.md`.

**Verification gap** — gallery GUI not launched here; `dotnet run --project samples/Loam.Gallery`.

**Next (Phase 3 — Core Primitives)**
1. `Text` (typography-aware), `Icon` (+ ADR-0006 icon decision).
2. `Button`/`IconButton`/`Fab` with `Variant`/`Color`/`Size` + ripple.
3. `Paper`/`Surface` finish (elevation→shadow-token mapping), `Divider`, `Chip`, `Badge`, `Avatar`.

---

## 2026-06-02 — Phase 1 — Solution Foundation & Tooling ✅

**Done**
- `git init` (branch `main`); added `.gitignore`, `LICENSE` (MIT + non-affiliation notice),
  `global.json` (SDK 10.0.300), `.editorconfig`, `Directory.Build.props`
  (nullable, warnings-as-errors, analyzers, central package mgmt), `Directory.Packages.props`
  (Avalonia 12.0.4 pinned).
- Created `Loam.slnx` + projects: `src/Loam` (net8.0 lib), `samples/Loam.Gallery` (net8.0 desktop),
  `tests/Loam.Tests` (net8.0, xunit v3 headless).
- Built the **fluent template helper seed** (`Internal/Templating/TemplateScope.Named`).
- Implemented the **theming seed**: `LoamTheme` (Styles-derived, Light/Dark ThemeDictionaries,
  reference-default palette tokens), `LoamTokens` (resource keys).
- Implemented the **smoke control** `Surface` (`ContentControl`) + `SurfaceTheme` — a pure-C#
  `ControlTheme` + `FuncControlTemplate`, all visuals token-bound via `GetResourceObservable`.
- Gallery shows the Surface + a live light/dark toggle (FluentTheme is temporary shell scaffolding).
- **2 headless tests pass** (Debug + Release): control theme applies; Surface background resolves
  `#FFFFFF` (Light) / `#373740` (Dark) and **re-resolves on runtime variant swap**.
- Added GitHub Actions CI (`.github/workflows/ci.yml`): restore → build → headless test on ubuntu.
- **`dotnet build` + `dotnet test` green in Debug and Release, 0 warnings.**

**Decisions / adjustments**
- Test stack moved to **xunit v3** (Avalonia.Headless.XUnit 12.x requires it); test project is `Exe`.
- Gallery TFM simplified to **net8.0** (ADR-0004 updated).
- Dynamic-resource-in-code mechanism chosen: **`GetResourceObservable`** (not DynamicResourceExtension
  in setters). See `findings/2026-06-02-phase1-spikes.md`.

**Verified facts** — `findings/2026-06-02-phase1-spikes.md` (pinned versions + C# patterns + gotchas).

**Verification gap** — live GUI launch of the gallery not run here (no display);
`dotnet run --project samples/Loam.Gallery` to view.

**Next (Phase 2 — Design System & Theming Engine)**
1. Expand `LoamTheme` model: full `Palette` (all reference colors), `Typography`, `Shadows` 0–24,
   `LayoutProperties`, `ZIndex`.
2. Token catalog + `LoamColor` derivations (hover/ripple/disabled, on-color contrast).
3. Runtime palette-edit demo in the gallery; headless tests for palette resolution + variant + swap.

---

## 2026-06-02 — Phase 0 — Discovery & planning ✅

**Done**
- Reviewed source-first and architecture references.
- Verified environment: .NET 10/11 SDKs installed; repo not yet under git; only local workspace
  notes present.
- Researched & recorded foundations (`findings/2026-06-02-foundations-research.md`): Avalonia 12.x,
  reference v8/v9, pure-C# ControlTheme feasibility, prior art (Material.Avalonia, Semi.Avalonia).
- Captured 3 owner decisions via clarifying questions → ADR-0001 (name **Loam**), ADR-0002
  (**pure C#, no XAML**), ADR-0003 (**balanced fidelity**); plus engineering ADR-0004…0007.
- Authored `DEVELOPMENT_PLAN.md` (Phases 0–10, per-phase + per-component Definition of Done).
- Built the memory system: README, decisions, findings, component inventory, learnings, this log.

**Decisions** — see `decisions/` ADR-0001…0007.

**Verified facts** — see `findings/2026-06-02-foundations-research.md`.

**Next (Phase 1 — Solution Foundation & Tooling)**
1. `git init`; add `.gitignore`, license, root `README`.
2. Create `Loam.sln` + projects: `src/Loam`, `samples/Loam.Gallery`, `tests/Loam.Tests`.
3. Central package management, `Directory.Build.props`, `.editorconfig`, nullable + warnings-as-errors.
4. Pin exact Avalonia 12.0.x; record in `findings/`.
5. Spike: confirm C# `TemplateBinding`, `ThemeDictionaries` population, dynamic-resource setters.
6. Smoke control (`Surface`) themed purely in C# + a headless render test → prove the pipeline.
7. CI (GitHub Actions): build + test.
