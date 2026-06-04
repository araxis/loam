# Component Inventory & reference → Loam Mapping (master tracker)

The authoritative catalog of what Loam ships, which phase delivers it, and live status.
**Update the Status column whenever a slice moves.** Component names verify against reference v8/v9
source before implementation (source-first, ADR-0007).

**Status legend:** ⬜ Not started · 🟦 In progress · ✅ Done · ⏸️ Deferred/stretch · ❌ Dropped (note why)

> Loam type lives in `Loam.Controls` (no legacy prefix). Parameters keep reference names where they
> translate. "Notes" records intentional divergences.

## Phase 2 — Theming & design system (backbone)

| reference | Loam | Status | Notes |
| --- | --- | --- | --- |
| `Theme` | `LoamTheme` + `LoamThemeData` | ✅ | Styles-derived; projects data → resources; `SetData`/`SetPalette`/`SetPrimary`. |
| `Palette`/`PaletteLight`/`PaletteDark` | `LoamPalette` (Default Light/Dark) | ✅ | Full faithful palette, both variants; projected to `ThemeDictionaries` by reflection. |
| `Typography` | `LoamTypography` | ✅ | 14-style Material scale (Default, H1–H6, Subtitle1/2, Body1/2, Button, Caption, Overline). |
| `Shadows` | `LoamShadows` (elevation 0–25) | ✅ | CSS→`BoxShadows` converter (`ParseCss`); faithful reference table. |
| `LayoutProperties` | `LoamLayout` | ✅ | radius 4, drawer 240/56, appbar 64. |
| `ZIndex` | `LoamZIndex` | ✅ | drawer/popover/appbar/dialog/snackbar/tooltip (1100–1600). |
| `ThemeProvider` | `LoamTheme` setup + runtime API | ✅ | one-line `App.Styles` add; runtime variant + palette swap. |
| (color math) | `LoamColors` | ✅ | alpha/lighten/darken/contrast/luminance derivations. |

## Phase 3 — Core primitives

| reference | Loam | Status | Notes |
| --- | --- | --- | --- |
| `Text` | `Text` | ✅ | `Typo`, `Color`, `GutterBottom`, `Align` (`: TextBlock`, token-driven). |
| `Icon` | `Icon` | ✅ | custom-drawn; `Data` (path), `Color`, `Size`, `ViewBox`; inherits `Foreground`. |
| `Button` | `Button` | ✅ | `Variant`×`Color`×`Size`×`FullWidth` + `StartIcon`/`EndIcon` + hover/disabled + automatic ripple host. |
| `IconButton` | `IconButton` | ✅ | circular icon-only; reuses shared `ButtonStyles` matrix. |
| `ButtonGroup` | `ButtonGroup` | 🟦 | Connected `Items` (Buttons) with merged borders (−1px overlap) + shared outer corners; pushes group `Variant`/`Color`/`Size` onto children (`OverrideChildStyles`); `Vertical`. Local per-button `CornerRadius` overrides the theme setter. |
| `ToggleIconButton` | `ToggleIconButton` | ✅ | `: IconButton` (reuses its theme via inherited style key); two-way `Toggled` swaps the glyph (`Icon` off ↔ `ToggledIcon` on); `ToggledColor` colors the on glyph; `OnClick` flips it. |
| `Fab` | `Fab` | ✅ | pill, filled, elevated; `Label` + `StartIcon`. |
| `Paper` | `Paper` | ✅ | `Elevation` (0–25 shadow tokens), `Square`, `Outlined`. Replaced the Phase-1 `Surface` smoke control. |
| `Card` (+Header/Content/Actions/Media) | `Card` (+ parts) | ✅ | `Card`/`CardContent`/`CardActions` (Paper/Decorator). `CardHeader` (`Avatar`/`Title`/`Subtitle`/`Action`, theme-bound slot visibility). `CardMedia` (`Source` `IImage` + `MediaHeight`, gray placeholder band). |
| `Divider` | `Divider` | ✅ | `Vertical`, `Light`, `DividerType` (`: Border`). |
| `Chip`/`ChipSet` | `Chip`/`ChipSet` | ✅ | `Chip` (Text/Icon/variant/color/size/closeable/Label). `ChipSet` (`Items`, `Selectable`/`Mandatory`, `MultiSelect`, two-way `SelectedIndex`, `SelectedIndexes` → selected Filled, others Outlined). |
| `Badge` | `Badge` | ✅ | `Value`/`Dot`/`Color`/`Origin`/`Overlap`/`Bordered`/`Max`/`Visible`. |
| `Avatar`/`AvatarGroup` | `Avatar`/`AvatarGroup` | ✅ | `Avatar` (Variant/Color/Size/Square/Rounded). `AvatarGroup` (`Items`, `Max` + "+N" surplus, `Spacing` overlap). |
| (ripple effect) | `Ripple` | ✅ | `: Decorator`; on press animates a translucent circle from the press point (`Animation` on `Progress`, `Render` draws it), `ClipToBounds`. Pure `Ripple.MaxReach`. Button/IconButton templates auto-wire it. |

## Phase 4 — Layout & app shell

| reference | Loam | Status | Notes |
| --- | --- | --- | --- |
| `Container` | `Container` | ✅ | `MaxWidthBreakpoint`/`Gutters`; caps + centers (`: Decorator`). |
| `Grid`/`Item` | `Grid`/`Item` | ✅ | responsive 12-col custom panel; container-query breakpoints. Qualify vs `Avalonia.Controls.Grid`. |
| `Stack` | `Stack` | ✅ | `Row` + spacing (`: StackPanel`). `Justify`/`Wrap`/`Reverse` ⬜. |
| `Spacer` | `Spacer` | 🟦 | `: Control`, stretch; fills as the `LastChildFill` child of a `DockPanel` (or star `Grid` cell) to push docked siblings to the edges. |
| `Hidden` | `Hidden` | 🟦 | `: Decorator`; tracks host-window width → hides `Child` when current `Breakpoints` bucket satisfies `Mode` (Down/Up/Only) vs `Breakpoint`. Pure `IsHiddenAt` for the rule. |
| Breakpoint service | `Breakpoint` enum + `Breakpoints` helper | ✅ | xs–xxl thresholds; container-width based. |
| `Layout` | `Layout` | ✅ | `AppBar`/`Drawer` slots + content; drawer-aware body panel supports docked space reservation and temporary overlay mode. |
| `AppBar` | `AppBar` | ✅ | `Color`/`Elevation`/`Dense`; app-bar palette default. |
| `Drawer`(+Header/Container) | `Drawer` | ✅ | `Open`/`Mini`/`DrawerWidth`/`MiniWidth`; slides. `DrawerMode.Docked`/`Temporary`, `ShowScrim`, and `CloseOnScrimClick` in `Layout`. |
| `MainContent` | `MainContent` | ✅ | padded scroll viewer. |
| `ScrollToTop` | `ScrollToTop` | 🟦 | `: Decorator` (default up-arrow `Fab`); watches a `Target` `ScrollViewer.ScrollChanged`, shows once `Offset.Y > VisibleOffset`, scrolls home on click. |

## Phase 5 — Forms & inputs

| reference | Loam | Status | Notes |
| --- | --- | --- | --- |
| `Form` | `Form` | ✅ | `Validate()` aggregates `TextField` validation; `IsValid`. (Full `INotifyDataErrorInfo` later.) |
| `Field` | `Field` | 🟦 | chrome (label/helper/error/variant) built into `TextField` for now; extract a shared `Field` base later. |
| `TextField` | `TextField` | ✅ | `Text`(two-way)/`Label`/`HelperText`/`Placeholder`/`Variant`/`Color`/`Error`/`ReadOnly`; `StartAdornment`/`EndAdornment`; optional `FloatingLabel`. |
| `NumericField` | `NumericField` | 🟦 | Shares `TextField` chrome + vertical spinner (`PART_Up`/`PART_Down`). Two-way `Value` clamped to `Minimum`/`Maximum`; `Step`; `Format`; text↔value parse (current culture). Generic `T` numeric type ⬜ (double-only for now). |
| `Select`/`SelectItem` | `Select`/`SelectItem` | ✅ | Outlined box + chevron opens a `Flyout` of `ListItem` rows; two-way `Value`/`Label`/`Placeholder`; `MultiSelect` + `SelectedValues`; `DisplayTextFunc`; `ItemTemplate`; focusable with Enter/Space open, Escape close, automation name from label/display text. |
| `Autocomplete` | `Autocomplete` | ✅ | Composes a `TextField` (`PART_Field`, chrome forwarded) + a `Flyout` of `ListItem` matches. Two-way `Value`; static `Filter`; `SearchFunc`/`SearchAsync`; templated string rows. Generic `T` remains a future expansion. |
| `CheckBox` | `CheckBox` | ✅ | `Color`/`Size`; Material box + check (`: Avalonia CheckBox`, tri-state inherited). |
| `Switch` | `Switch` | ✅ | `Color`/`Size`; sliding track + thumb (`: ToggleButton`). |
| `RadioGroup`/`Radio` | `RadioGroup`/`Radio` | ✅ | `Radio` (`: RadioButton`) ring+dot; `RadioGroup` (`: Decorator`) two-way `Value`. |
| `Slider` | `Slider` | ✅ | custom draggable track/fill/thumb. |
| `Rating` | `Rating` | 🟦 | Star strip (`Icons…Star`); two-way `SelectedValue`, `MaxValue`, `Color` (default gold/Warning), `Size`, `ReadOnly`; live hover preview. Half-ratings + custom icons ⬜. |
| `ToggleGroup`/`ToggleItem` | `ToggleGroup`/`ToggleItem` | 🟦 | Segmented single-select: connected `Border` segments in a rounded outline; selected (== two-way `SelectedValue`) fills `Color` + contrast text. Multi-select/icons ⬜. |
| `FileUpload` | `FileUpload` | 🟦 | Upload button opens the platform picker via `TopLevel.StorageProvider.OpenFilePickerAsync` (`AllowMultiple`); picked `Files` (`IStorageFile`) exposed + names shown as chips + `FilesSelected` event. `ShowSelection`/`Clear`. Drag-drop + per-file remove/progress ⬜. |
| `Mask` | `Mask` | 🟦 | `Mask.Apply(raw, pattern)` static formatter (`#`=digit, `A`=letter, `*`=any, literals inline) + `MaskedTextField : TextField` that reformats on input. |

## Phase 6 — Overlays & feedback

| reference | Loam | Status | Notes |
| --- | --- | --- | --- |
| `Popover`(+Provider) | `Popover` + overlay layer | ✅ | overlay layer + Avalonia `Flyout` (Menu/Select/pickers); standalone **`Popover`** (`: Decorator` wrapping Avalonia `Popup`): `Content` in elevated `Paper`, two-way `Open`, `Placement`, `Target`, light-dismiss. |
| `Overlay` | `Overlay` | 🟦 | `: ContentControl`; parent-filling scrim (`DarkBackground`), centered content, two-way `Visible`, `AutoClose` + `OnClick` (scrim-only hit). |
| `Tooltip` | `Tooltip` | ✅ | `Tooltip.Set(control, text)` static helper over Avalonia `ToolTip`. |
| `Menu`/`MenuItem` | `Menu`/`MenuItem` | ✅ | `Menu` (`: Button`) opens a `Flyout` of `MenuItem` rows. Qualify vs `Avalonia.Controls.Menu`. |
| `Dialog` + `IDialogService` + `DialogProvider` | `IDialogService`/`DialogService` | ✅ | overlay-layer scrim + `Paper`; `ShowAsync`/`ConfirmAsync`, `DialogResult`. No provider component. |
| `MessageBox` | `MessageBox` | ✅ | `DialogService.MessageBoxAsync(title, message, yes, no?, cancel?)` → `bool?` (yes/no/cancel); omitting no/cancel hides those buttons. |
| `ISnackbar` + `SnackbarProvider` | `ISnackbar`/`SnackbarService` | ✅ | overlay-layer auto-dismiss toasts with optional action callbacks and visible-count limits. |
| `Alert` | `Alert` | ✅ | severity `Color` + Filled/Outlined/Text-tint + icon. |
| `ProgressCircular` | `ProgressCircular` | 🟦 | Custom `Render` arc (`: Control`); determinate sweep from `Value` + spinning `Indeterminate` (default, `Animation` on `SpinAngle`). `Color`/`Size`/`StrokeWidth`. Faint `Divider` track on determinate. |
| `ProgressLinear` | `ProgressLinear` | ✅ | Determinate fill plus `Indeterminate` moving fill; `Color`/`Value`/`Minimum`/`Maximum`. |
| `Skeleton` | `Skeleton` | ✅ | Placeholder block + `Circle` + `Animate` shimmer toggle (default on). |
| `Collapse` | `Collapse` | ✅ | `: Decorator`; two-way `Expanded` clips `Child` with `ClipToBounds`; `Animated` + `Duration` reveal/collapse. |

## Phase 7 — Data display

| reference | Loam | Status | Notes |
| --- | --- | --- | --- |
| `List`/`ListItem`/`ListSubheader` | `List`/`ListItem`/`ListSubheader` | ✅ | `List` (`: StackPanel`), `ListItem` (`: ContentControl`, icon + hover), `ListSubheader` (`: Text`, muted SemiBold caption). Selection/nested ⬜. |
| `SimpleTable` | `SimpleTable` | 🟦 | Data-driven `Headers`/`Rows` (`TableRow` of cells; string→`Text`, else hosted `Control`) into a `Grid` inside an elevated `Paper`. `Striped`/`Hover`/`Bordered`/`Dense`/`Elevation`. Content-child (`<tr>`-style) API ⬜. |
| `Table` | `Table` | ✅ | covered by `SimpleTable` (simple data table) + `DataGrid<T>` (typed sort/page/select). No separate redundant control built — intentional. |
| `DataGrid`/`Column` | `DataGrid<T>`/`DataGridColumn<T>` | ✅ | Typed, self-rendering `: Decorator` (generics can't host `StyledProperty`/`ControlTheme`): sort headers, `PageSize` paging, striping/hover/`Dense`, single-row selection, `FilterText`/`Filter`, `Virtualize`/`MaxRenderedRows`, editable text cells via `SetText`, custom `CellTemplate`. Pure `DataGrids.Sort`/`PageCount`/`Filter`. Grouping remains future expansion. |
| `TreeView`/`TreeViewItem` | `TreeView`/`TreeViewItem` | 🟦 | `TreeViewItem` (`Text`/`Icon`/`Items`/`Expanded`/`IsSelected`; expander chevron, indented children, hover/select highlight; bubbling `ItemSelectedEvent`; focusable row with Enter select / Space toggle and automation name). `TreeView` coordinates single selection (`SelectedItem`). Qualify vs `Avalonia.Controls.TreeView`. Checkboxes/lazy-load ⬜. |
| `ExpansionPanels`/`ExpansionPanel` | `ExpansionPanels`/`ExpansionPanel` | ✅ | `ExpansionPanel` (`: HeaderedContentControl`, `IsExpanded` two-way, focusable header + rotating chevron + `Collapse`-based content reveal + automation name from header). `ExpansionPanels` container (`Panels`, `MultiExpansion`; accordion via `PropertyChanged`). |
| `Tabs`/`TabPanel`/`DynamicTabs` | `Tabs`/`TabItem` | 🟦 | `Tabs` + `TabItem` (header strip + content switch). `DynamicTabs` (closeable) ⬜. |
| `Timeline`/`TimelineItem` | `Timeline`/`TimelineItem` | 🟦 | `: Decorator`; vertical `Items` down a `Divider` connector line, each a colored dot beside a `Paper` content card. Alternating/horizontal modes + `TimelineAlign` ⬜. |
| `Carousel`/`CarouselItem` | `Carousel`/`CarouselItem` | 🟦 | Z-stacked slide (`PART_Content`) + overlay prev/next arrows + clickable bottom bullets; two-way `SelectedIndex`, `Next`/`Previous` wrap; `ShowArrows`/`ShowBullets`. Auto-cycle + transitions ⬜. Qualify vs `Avalonia.Controls.Carousel`. |
| `Pagination` | `Pagination` | 🟦 | `BuildPages` (boundary + centered `MiddleCount` window, edge-shifted, single-gap fill, `0`=ellipsis) → prev/next arrows + page buttons (selected filled `Color`); two-way `Selected`. First/last buttons ⬜. |

## Phase 8 — Navigation & pickers

| reference | Loam | Status | Notes |
| --- | --- | --- | --- |
| `NavMenu`/`NavLink`/`NavGroup` | `NavMenu`/`NavLink`/`NavGroup` | 🟦 | `NavLink` (`: ContentControl`, `Icon`/`IsActive`/`Color`/`OnClick`/`Href`; active = accent tint, hover otherwise). `NavMenu` (`: StackPanel`). `NavGroup` (`: TemplatedControl`; `Title`/`Icon`/`Expanded` focusable header + indented `Collapse`-revealed `Items`, automation name from title). |
| `Breadcrumbs` | `Breadcrumbs` | 🟦 | `BreadcrumbItem` (`Text`/`OnClick`/`Href`/`Disabled`); non-last entries are `Link`s, last is the muted current page; `Separator` (default `/`). Icon/maxitems-collapse ⬜. |
| `Link` | `Link` | 🟦 | `: Text`; `Color` (default Primary), hover underline + `Underline` (always), `OnClick`, `Href` (launches via `TopLevel.Launcher`). |
| `Stepper` | `Stepper` | 🟦 | `: TemplatedControl` + `Step` (`Title`/`Content`/`Completed`). Numbered marker header w/ connectors (active/completed = Primary, check icon when done), active `Content`, Back/Next(Finish) nav; two-way `ActiveIndex`, `OnCompleted`. Non-linear/vertical/validation ⬜. |
| `DatePicker` | `DatePicker` | ✅ | Outlined box (Select-style) + calendar icon opens a **self-contained** `MonthCalendar` flyout (no FluentTheme `Calendar` dep): month nav, weekday row, day grid w/ today/selected highlight and min/max disabling. Two-way `Date`, `DateFormat`, `MinDate`, `MaxDate`; focusable with Enter/Space open, Escape close, automation name. Views remain future expansion. Qualify vs `Avalonia.Controls.DatePicker`. |
| `TimePicker` | `TimePicker` | 🟦 | Outlined box (Select-style) + clock icon opens a flyout with scrollable hour/minute columns (`MinuteStep`), live-highlighted selection. Two-way `Time` (`TimeSpan`), `TimeFormat`; focusable with Enter/Space open, Escape close, automation name. Clock-face UI / AM-PM toggle ⬜. Qualify vs `Avalonia.Controls.TimePicker`. |
| `DateRangePicker` | `DateRangePicker` | ✅ | Outlined box shows `Start – End` (two-way), opens a `MonthCalendar` flyout: 1st click = start, 2nd = end (auto-ordered), min/max disabling, range highlight. `Format` static; focusable with Enter/Space open, Escape close, automation name. |
| `ColorPicker` | `ColorPicker` | ✅ | Palette mode: outlined box (swatch + hex) opens a `UniformGrid` flyout of preset swatches (`DefaultPalette`) that set two-way `Value` (`Color`). `ShowAlpha`, `ToHex`, `ToHexWithAlpha`, `ToHsv`, `FromHsv`; focusable with Enter/Space open, Escape close, automation name. Full HSV editor remains future expansion. |

## Phase 9 — Charts (stretch / optional `Loam.Charts`)

| reference | Loam | Status | Notes |
| --- | --- | --- | --- |
| `Chart` (Line/Bar/Pie/Donut/StackedBar/TimeSeries) | `PieChart`/`BarChart`/`LineChart` (+`ChartBase`/`Charts`) | 🟦 | Custom `Render` charts (`: Control`): Pie + Donut (`HoleRatio`), vertical Bar, Line (`Area` fill). `Values`/`Colors` (default `Charts.Palette`); pure `Charts.SliceSweeps`/`BarHeights`. In `Loam.Controls` for now (separate `Loam.Charts` package + StackedBar/TimeSeries/axes/legend/tooltips ⬜). |

## Cross-cutting (not single components)

| Concern | Loam piece | Status | Notes |
| --- | --- | --- | --- |
| CSS utility classes (`pa-4`, `d-flex`, `elevation-N`…) | (intentionally not mapped) | ❌ | reference CSS utilities don't translate to Avalonia, which is property-based (`Margin`/`Padding`/`Spacing` on panels, `Paper.Elevation`). No utility-class layer — use control properties directly. Decision, not a gap. |
| `Icons.Material.*` | `Icons.Material.Filled.*` (core) + future `Loam.Icons` pack | 🟦 | Curated ~13-glyph core set shipped (ADR-0006); full generated pack deferred to a separate package. |
| Color/opacity derivations | `LoamColors` + derived tokens | ✅ | alpha/lighten/darken/contrast (Phase 2); per-color `.Hover`/`.Darken` tokens (Phase 3). |
| Shared enums / API vocabulary | `Variant`, `LoamColor`, `LoamSize`, `Typo`, `Align`, `DividerType` + `LoamColorExtensions` | ✅ | mirrors reference enums (ADR-0007). |
