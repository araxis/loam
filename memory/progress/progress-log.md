# Progress Log

Newest entries on top. One entry per meaningful unit of work. Keep it factual and dated.

Format: `## YYYY-MM-DD — Phase N — short title` → What was done · Decisions · Verified facts ·
Next.

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
