# Component Inventory & MudBlazor → Loam Mapping (master tracker)

The authoritative catalog of what Loam ships, which phase delivers it, and live status.
**Update the Status column whenever a slice moves.** Component names verify against MudBlazor v8/v9
source before implementation (source-first, ADR-0007).

**Status legend:** ⬜ Not started · 🟦 In progress · ✅ Done · ⏸️ Deferred/stretch · ❌ Dropped (note why)

> Loam type lives in `Loam.Controls` (no `Mud` prefix). Parameters keep MudBlazor names where they
> translate. "Notes" records intentional divergences.

## Phase 2 — Theming & design system (backbone)

| MudBlazor | Loam | Status | Notes |
| --- | --- | --- | --- |
| `MudTheme` | `LoamTheme` + `LoamThemeData` | ✅ | Styles-derived; projects data → resources; `SetData`/`SetPalette`/`SetPrimary`. |
| `Palette`/`PaletteLight`/`PaletteDark` | `LoamPalette` (Default Light/Dark) | ✅ | Full faithful palette, both variants; projected to `ThemeDictionaries` by reflection. |
| `Typography` | `LoamTypography` | ✅ | 14-style Material scale (Default, H1–H6, Subtitle1/2, Body1/2, Button, Caption, Overline). |
| `Shadows` | `LoamShadows` (elevation 0–25) | ✅ | CSS→`BoxShadows` converter (`ParseCss`); faithful MUI table. |
| `LayoutProperties` | `LoamLayout` | ✅ | radius 4, drawer 240/56, appbar 64. |
| `ZIndex` | `LoamZIndex` | ✅ | drawer/popover/appbar/dialog/snackbar/tooltip (1100–1600). |
| `MudThemeProvider` | `LoamTheme` setup + runtime API | ✅ | one-line `App.Styles` add; runtime variant + palette swap. |
| (color math) | `LoamColors` | ✅ | alpha/lighten/darken/contrast/luminance derivations. |

## Phase 3 — Core primitives

| MudBlazor | Loam | Status | Notes |
| --- | --- | --- | --- |
| `MudText` | `Text` | ✅ | `Typo`, `Color`, `GutterBottom` (`: TextBlock`, token-driven). `Align` pending. |
| `MudIcon` | `Icon` | ✅ | custom-drawn; `Data` (path), `Color`, `Size`, `ViewBox`; inherits `Foreground`. |
| `MudButton` | `Button` | 🟦 | `Variant`×`Color`×`Size`×`FullWidth` + `StartIcon`/`EndIcon` + hover/disabled done. Deferred: ripple, `:pressed`. |
| `MudIconButton` | `IconButton` | ✅ | circular icon-only; reuses shared `ButtonStyles` matrix. |
| `MudButtonGroup` | `ButtonGroup` | 🟦 | Connected `Items` (Buttons) with merged borders (−1px overlap) + shared outer corners; pushes group `Variant`/`Color`/`Size` onto children (`OverrideChildStyles`); `Vertical`. Local per-button `CornerRadius` overrides the theme setter. |
| `MudToggleIconButton` | `ToggleIconButton` | 🟦 | `: IconButton` (reuses its theme via inherited style key); two-way `Toggled` swaps the glyph (`Icon` off ↔ `ToggledIcon` on); `OnClick` flips it. `ToggledColor` ⬜. |
| `MudFab` | `Fab` | ✅ | pill, filled, elevated; `Label` + `StartIcon`. |
| `MudPaper` | `Paper` | ✅ | `Elevation` (0–25 shadow tokens), `Square`, `Outlined`. Replaced the Phase-1 `Surface` smoke control. |
| `MudCard` (+Header/Content/Actions/Media) | `Card` (+ parts) | ✅ | `Card`/`CardContent`/`CardActions` (Paper/Decorator). `CardHeader` (`Avatar`/`Title`/`Subtitle`/`Action`, theme-bound slot visibility). `CardMedia` (`Source` `IImage` + `MediaHeight`, gray placeholder band). |
| `MudDivider` | `Divider` | ✅ | `Vertical`, `Light`, `DividerType` (`: Border`). |
| `MudChip`/`MudChipSet` | `Chip`/`ChipSet` | ✅ | `Chip` (Text/Icon/variant/color/size/closeable/Label). `ChipSet` (`Items`, `Selectable`/`Mandatory`, two-way `SelectedIndex` → selected Filled, others Outlined). Multi-select ⬜. |
| `MudBadge` | `Badge` | ✅ | `Value`/`Dot`/`Color`/`Origin`/`Overlap`/`Bordered`/`Max`/`Visible`. |
| `MudAvatar`/`MudAvatarGroup` | `Avatar`/`AvatarGroup` | ✅ | `Avatar` (Variant/Color/Size/Square/Rounded). `AvatarGroup` (`Items`, `Max` + "+N" surplus, `Spacing` overlap). |
| (ripple effect) | `Ripple` | 🟦 | `: Decorator`; on press animates a translucent circle from the press point (`Animation` on `Progress`, `Render` draws it), `ClipToBounds`. Pure `Ripple.MaxReach`. Per-button auto-wiring ⬜. |

## Phase 4 — Layout & app shell

| MudBlazor | Loam | Status | Notes |
| --- | --- | --- | --- |
| `MudContainer` | `Container` | ✅ | `MaxWidthBreakpoint`/`Gutters`; caps + centers (`: Decorator`). |
| `MudGrid`/`MudItem` | `Grid`/`Item` | ✅ | responsive 12-col custom panel; container-query breakpoints. Qualify vs `Avalonia.Controls.Grid`. |
| `MudStack` | `Stack` | ✅ | `Row` + spacing (`: StackPanel`). `Justify`/`Wrap`/`Reverse` ⬜. |
| `MudSpacer` | `Spacer` | 🟦 | `: Control`, stretch; fills as the `LastChildFill` child of a `DockPanel` (or star `Grid` cell) to push docked siblings to the edges. |
| `MudHidden` | `Hidden` | 🟦 | `: Decorator`; tracks host-window width → hides `Child` when current `Breakpoints` bucket satisfies `Mode` (Down/Up/Only) vs `Breakpoint`. Pure `IsHiddenAt` for the rule. |
| Breakpoint service | `Breakpoint` enum + `Breakpoints` helper | ✅ | xs–xxl thresholds; container-width based. |
| `MudLayout` | `Layout` | ✅ | `AppBar`/`Drawer` slots + content; DockPanel template. |
| `MudAppBar` | `AppBar` | ✅ | `Color`/`Elevation`/`Dense`; app-bar palette default. |
| `MudDrawer`(+Header/Container) | `Drawer` | ✅ | `Open`/`Mini`/`DrawerWidth`/`MiniWidth`; slides. Responsive/temporary/overlay variants ⬜. |
| `MudMainContent` | `MainContent` | ✅ | padded scroll viewer. |
| `MudScrollToTop` | `ScrollToTop` | 🟦 | `: Decorator` (default up-arrow `Fab`); watches a `Target` `ScrollViewer.ScrollChanged`, shows once `Offset.Y > VisibleOffset`, scrolls home on click. |

## Phase 5 — Forms & inputs

| MudBlazor | Loam | Status | Notes |
| --- | --- | --- | --- |
| `MudForm` | `Form` | ✅ | `Validate()` aggregates `TextField` validation; `IsValid`. (Full `INotifyDataErrorInfo` later.) |
| `MudField` | `Field` | 🟦 | chrome (label/helper/error/variant) built into `TextField` for now; extract a shared `Field` base later. |
| `MudTextField` | `TextField` | ✅ | `Text`(two-way)/`Label`/`HelperText`/`Placeholder`/`Variant`/`Color`/`Error`/`ReadOnly`. Floating label + adornments ⬜. |
| `MudNumericField` | `NumericField` | 🟦 | Shares `TextField` chrome + vertical spinner (`PART_Up`/`PART_Down`). Two-way `Value` clamped to `Minimum`/`Maximum`; `Step`; `Format`; text↔value parse (current culture). Generic `T` numeric type ⬜ (double-only for now). |
| `MudSelect`/`MudSelectItem` | `Select`/`SelectItem` | 🟦 | Outlined box + chevron opens a `Flyout` of `ListItem` rows; two-way `Value`/`Label`/`Placeholder`. Multi-select, custom item templates, `ToStringFunc` ⬜. |
| `MudAutocomplete` | `Autocomplete` | 🟦 | Composes a `TextField` (`PART_Field`, chrome forwarded) + a `Flyout` of `ListItem` matches (case-insensitive contains, `MaxItems`). Two-way `Value`. Generic `T`/`SearchFunc`/templated items ⬜. |
| `MudCheckBox` | `CheckBox` | ✅ | `Color`/`Size`; Material box + check (`: Avalonia CheckBox`, tri-state inherited). |
| `MudSwitch` | `Switch` | ✅ | `Color`/`Size`; sliding track + thumb (`: ToggleButton`). |
| `MudRadioGroup`/`MudRadio` | `RadioGroup`/`Radio` | ✅ | `Radio` (`: RadioButton`) ring+dot; `RadioGroup` (`: Decorator`) two-way `Value`. |
| `MudSlider` | `Slider` | ✅ | custom draggable track/fill/thumb. |
| `MudRating` | `Rating` | 🟦 | Star strip (`Icons…Star`); two-way `SelectedValue`, `MaxValue`, `Color` (default gold/Warning), `Size`, `ReadOnly`; live hover preview. Half-ratings + custom icons ⬜. |
| `MudToggleGroup`/`MudToggleItem` | `ToggleGroup`/`ToggleItem` | 🟦 | Segmented single-select: connected `Border` segments in a rounded outline; selected (== two-way `SelectedValue`) fills `Color` + contrast text. Multi-select/icons ⬜. |
| `MudFileUpload` | `FileUpload` | 🟦 | Upload button opens the platform picker via `TopLevel.StorageProvider.OpenFilePickerAsync` (`AllowMultiple`); picked `Files` (`IStorageFile`) exposed + names shown as chips + `FilesSelected` event. `ShowSelection`/`Clear`. Drag-drop + per-file remove/progress ⬜. |
| `MudMask` | `Mask` | 🟦 | `Mask.Apply(raw, pattern)` static formatter (`#`=digit, `A`=letter, `*`=any, literals inline) + `MaskedTextField : TextField` that reformats on input. |

## Phase 6 — Overlays & feedback

| MudBlazor | Loam | Status | Notes |
| --- | --- | --- | --- |
| `MudPopover`(+Provider) | `Popover` + overlay layer | ✅ | overlay layer + Avalonia `Flyout` (Menu/Select/pickers); standalone **`Popover`** (`: Decorator` wrapping Avalonia `Popup`): `Content` in elevated `Paper`, two-way `Open`, `Placement`, `Target`, light-dismiss. |
| `MudOverlay` | `Overlay` | 🟦 | `: ContentControl`; parent-filling scrim (`DarkBackground`), centered content, two-way `Visible`, `AutoClose` + `OnClick` (scrim-only hit). |
| `MudTooltip` | `Tooltip` | ✅ | `Tooltip.Set(control, text)` static helper over Avalonia `ToolTip`. |
| `MudMenu`/`MudMenuItem` | `Menu`/`MenuItem` | ✅ | `Menu` (`: Button`) opens a `Flyout` of `MenuItem` rows. Qualify vs `Avalonia.Controls.Menu`. |
| `MudDialog` + `IDialogService` + `MudDialogProvider` | `IDialogService`/`DialogService` | ✅ | overlay-layer scrim + `Paper`; `ShowAsync`/`ConfirmAsync`, `DialogResult`. No provider component. |
| `MudMessageBox` | `MessageBox` | ✅ | `DialogService.MessageBoxAsync(title, message, yes, no?, cancel?)` → `bool?` (yes/no/cancel); omitting no/cancel hides those buttons. |
| `ISnackbar` + `MudSnackbarProvider` | `ISnackbar`/`SnackbarService` | ✅ | overlay-layer auto-dismiss toasts. Actions/queue limits ⬜. |
| `MudAlert` | `Alert` | ✅ | severity `Color` + Filled/Outlined/Text-tint + icon. |
| `MudProgressCircular` | `ProgressCircular` | 🟦 | Custom `Render` arc (`: Control`); determinate sweep from `Value` + spinning `Indeterminate` (default, `Animation` on `SpinAngle`). `Color`/`Size`/`StrokeWidth`. Faint `Divider` track on determinate. |
| `MudProgressLinear` | `ProgressLinear` | ✅ | determinate fill. Indeterminate animation pending. |
| `MudSkeleton` | `Skeleton` | ✅ | placeholder block + `Circle`. Shimmer animation pending. |
| `MudCollapse` | `Collapse` | 🟦 | `: Decorator`; two-way `Expanded` clips `Child` (MaxHeight 0↔∞) with `ClipToBounds`. Slide animation ⬜. |

## Phase 7 — Data display

| MudBlazor | Loam | Status | Notes |
| --- | --- | --- | --- |
| `MudList`/`MudListItem`/`MudListSubheader` | `List`/`ListItem`/`ListSubheader` | ✅ | `List` (`: StackPanel`), `ListItem` (`: ContentControl`, icon + hover), `ListSubheader` (`: Text`, muted SemiBold caption). Selection/nested ⬜. |
| `MudSimpleTable` | `SimpleTable` | 🟦 | Data-driven `Headers`/`Rows` (`TableRow` of cells; string→`Text`, else hosted `Control`) into a `Grid` inside an elevated `Paper`. `Striped`/`Hover`/`Bordered`/`Dense`/`Elevation`. Content-child (`<tr>`-style) API ⬜. |
| `MudTable` | `Table` | ✅ | covered by `SimpleTable` (simple data table) + `DataGrid<T>` (typed sort/page/select). No separate redundant control built — intentional. |
| `MudDataGrid`/`Column` | `DataGrid<T>`/`DataGridColumn<T>` | 🟦 | Typed, self-rendering `: Decorator` (generics can't host `StyledProperty`/`ControlTheme`): sort headers (toggle asc/desc + arrow), `PageSize` paging (embeds `Pagination`), striping/hover/`Dense`, single-row selection (`SelectedItem`/`SelectionChanged`). `DataGridColumn<T>` = `Header`/`Value`/`Format`/`Align`/`Sortable`. Pure `DataGrids.Sort`/`PageCount`. Filter/group/edit/virtualize ⬜. |
| `MudTreeView`/`MudTreeViewItem` | `TreeView`/`TreeViewItem` | 🟦 | `TreeViewItem` (`Text`/`Icon`/`Items`/`Expanded`/`IsSelected`; expander chevron, indented children, hover/select highlight; bubbling `ItemSelectedEvent`). `TreeView` coordinates single selection (`SelectedItem`). Qualify vs `Avalonia.Controls.TreeView`. Checkboxes/lazy-load ⬜. |
| `MudExpansionPanels`/`MudExpansionPanel` | `ExpansionPanels`/`ExpansionPanel` | 🟦 | `ExpansionPanel` (`: HeaderedContentControl`, `IsExpanded` two-way, clickable header + rotating chevron + collapsible content). `ExpansionPanels` container (`Panels`, `MultiExpansion`; accordion via `PropertyChanged`). Expand animation ⬜. |
| `MudTabs`/`MudTabPanel`/`MudDynamicTabs` | `Tabs`/`TabItem` | 🟦 | `Tabs` + `TabItem` (header strip + content switch). `DynamicTabs` (closeable) ⬜. |
| `MudTimeline`/`MudTimelineItem` | `Timeline`/`TimelineItem` | 🟦 | `: Decorator`; vertical `Items` down a `Divider` connector line, each a colored dot beside a `Paper` content card. Alternating/horizontal modes + `TimelineAlign` ⬜. |
| `MudCarousel`/`MudCarouselItem` | `Carousel`/`CarouselItem` | 🟦 | Z-stacked slide (`PART_Content`) + overlay prev/next arrows + clickable bottom bullets; two-way `SelectedIndex`, `Next`/`Previous` wrap; `ShowArrows`/`ShowBullets`. Auto-cycle + transitions ⬜. Qualify vs `Avalonia.Controls.Carousel`. |
| `MudPagination` | `Pagination` | 🟦 | `BuildPages` (boundary + centered `MiddleCount` window, edge-shifted, single-gap fill, `0`=ellipsis) → prev/next arrows + page buttons (selected filled `Color`); two-way `Selected`. First/last buttons ⬜. |

## Phase 8 — Navigation & pickers

| MudBlazor | Loam | Status | Notes |
| --- | --- | --- | --- |
| `MudNavMenu`/`MudNavLink`/`MudNavGroup` | `NavMenu`/`NavLink`/`NavGroup` | 🟦 | `NavLink` (`: ContentControl`, `Icon`/`IsActive`/`Color`/`OnClick`/`Href`; active = accent tint, hover otherwise). `NavMenu` (`: StackPanel`). `NavGroup` (`: TemplatedControl`; `Title`/`Icon`/`Expanded` header + indented collapsible `Items`). Expand animation ⬜. |
| `MudBreadcrumbs` | `Breadcrumbs` | 🟦 | `BreadcrumbItem` (`Text`/`OnClick`/`Href`/`Disabled`); non-last entries are `Link`s, last is the muted current page; `Separator` (default `/`). Icon/maxitems-collapse ⬜. |
| `MudLink` | `Link` | 🟦 | `: Text`; `Color` (default Primary), hover underline + `Underline` (always), `OnClick`, `Href` (launches via `TopLevel.Launcher`). |
| `MudStepper` | `Stepper` | 🟦 | `: TemplatedControl` + `Step` (`Title`/`Content`/`Completed`). Numbered marker header w/ connectors (active/completed = Primary, check icon when done), active `Content`, Back/Next(Finish) nav; two-way `ActiveIndex`, `OnCompleted`. Non-linear/vertical/validation ⬜. |
| `MudDatePicker` | `DatePicker` | 🟦 | Outlined box (Select-style) + calendar icon opens a **self-contained** `MonthCalendar` flyout (no FluentTheme `Calendar` dep): month nav, weekday row, day grid w/ today/selected highlight. Two-way `Date`, `DateFormat`. Range/min-max/views ⬜. Qualify vs `Avalonia.Controls.DatePicker`. |
| `MudTimePicker` | `TimePicker` | 🟦 | Outlined box (Select-style) + clock icon opens a flyout with scrollable hour/minute columns (`MinuteStep`), live-highlighted selection. Two-way `Time` (`TimeSpan`), `TimeFormat`. Clock-face UI / AM-PM toggle ⬜. Qualify vs `Avalonia.Controls.TimePicker`. |
| `MudDateRangePicker` | `DateRangePicker` | 🟦 | Outlined box shows `Start – End` (two-way), opens a `MonthCalendar` flyout: 1st click = start, 2nd = end (auto-ordered). `Format` static. Full-span calendar highlight ⬜. |
| `MudColorPicker` | `ColorPicker` | 🟦 | Palette mode: outlined box (swatch + `#RRGGBB` hex) opens a `UniformGrid` flyout of preset Material swatches (`DefaultPalette`) that set two-way `Value` (`Color`). `ToHex` static. HSV square/hue slider/alpha ⬜. |

## Phase 9 — Charts (stretch / optional `Loam.Charts`)

| MudBlazor | Loam | Status | Notes |
| --- | --- | --- | --- |
| `MudChart` (Line/Bar/Pie/Donut/StackedBar/TimeSeries) | `PieChart`/`BarChart`/`LineChart` (+`ChartBase`/`Charts`) | 🟦 | Custom `Render` charts (`: Control`): Pie + Donut (`HoleRatio`), vertical Bar, Line (`Area` fill). `Values`/`Colors` (default `Charts.Palette`); pure `Charts.SliceSweeps`/`BarHeights`. In `Loam.Controls` for now (separate `Loam.Charts` package + StackedBar/TimeSeries/axes/legend/tooltips ⬜). |

## Cross-cutting (not single components)

| Concern | Loam piece | Status | Notes |
| --- | --- | --- | --- |
| CSS utility classes (`pa-4`, `d-flex`, `mud-elevation-N`…) | (intentionally not mapped) | ❌ | MudBlazor CSS utilities don't translate to Avalonia, which is property-based (`Margin`/`Padding`/`Spacing` on panels, `Paper.Elevation`). No utility-class layer — use control properties directly. Decision, not a gap. |
| `Icons.Material.*` | `Icons.Material.Filled.*` (core) + future `Loam.Icons` pack | 🟦 | Curated ~13-glyph core set shipped (ADR-0006); full generated pack deferred to a separate package. |
| Color/opacity derivations | `LoamColors` + derived tokens | ✅ | alpha/lighten/darken/contrast (Phase 2); per-color `.Hover`/`.Darken` tokens (Phase 3). |
| Shared enums / API vocabulary | `Variant`, `LoamColor`, `LoamSize`, `Typo`, `Align`, `DividerType` + `LoamColorExtensions` | ✅ | mirrors MudBlazor enums (ADR-0007). |
