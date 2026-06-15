---
title: Data display
---

# Data display

Controls for presenting structured information: lists, tables, grids, trees, tabs, timelines, carousels, paginators, and step wizards. All controls live in the `Loam.Controls` namespace; enums (`LoamColor`, `LoamSize`, `Variant`, `Typo`) live in the `Loam` namespace. (Column alignment uses Avalonia's own `HorizontalAlignment` from `Avalonia.Layout`.)

Where the [button family](./buttons) is about committing to an action, this page is about *showing* data — turning a model into something a user can scan, expand, sort, and step through. Most of these controls follow one of two shapes: a **container with an `ObservableCollection` of plain item objects** (`Tabs.Items` of `TabItem`, `Timeline.Items` of `TimelineItem`, `Stepper.Steps` of `Step`), or a **panel you fill with real controls** (`List` is a `StackPanel`; `SimpleTable.Rows` hold strings or any `Control`). Once you know which shape a control uses, populating it is the same gesture every time.

```csharp
using Loam;          // LoamColor, LoamSize, Variant, Typo, Icons
using Loam.Controls; // List, DataGrid<T>, Tabs, Stepper, …
```

::: tip Mental model
Pick by **how much the data does**. Static rows you control by hand → `SimpleTable`. Data-shaped rows the user sorts/pages/filters/selects → `DataGrid<T>`. A vertical menu of tappable rows → `List`. Hierarchy → `TreeView`. Switching between views of *the same* region → `Tabs`. Optional detail you collapse → `ExpansionPanels`. A sequence (history or a wizard) → `Timeline` / `Stepper`. Rotating featured content → `Carousel`. Splitting a long result set across pages → `Pagination`.
:::

> **Package (since 3.1).** `DataGrid<T>`, `SimpleTable`, `TreeView`/`TreeViewItem`, and `Pagination` ship
> in the **`Loam.Data`** satellite package — add the package reference and register its themes with
> `Styles.Add(new LoamData())` after `LoamTheme`. The remaining controls on this page (List, Tabs,
> Stepper, Timeline, Carousel, ExpansionPanel) stay in the core `Loam` package and need no extra
> reference. Namespaces are unchanged (`Loam.Controls`). See the
> [v3 → v3.1 migration guide](/migration/v3-to-v3.1).

> **Name collision note.** `TreeView`, `TreeViewItem`, and `Carousel` exist in both Loam and `Avalonia.Controls`. Qualify Loam types explicitly — `Loam.Controls.TreeView`, `Loam.Controls.TreeViewItem`, `Loam.Controls.Carousel` — when both namespaces are in scope.

> **Choosing a table.** Reach for **`DataGrid<T>`** by default — it's the recommended table for any
> data-shaped content (sorting, paging, filtering, selection, editing). Use **`SimpleTable`** only for a
> handful of static, non-interactive rows you'd otherwise hand-build with a `Grid`. See
> [ADR-0013](https://github.com/araxis/loam/blob/main/memory/decisions/0013-table-strategy.md).

## Choosing a control

| Use | When | Reach for |
| --- | --- | --- |
| Vertical menu of rows | Tappable items, optional icon/secondary line, grouped by subheaders | [`List`](#list-listitem-listsubheader) |
| Static table | A few non-interactive rows you'd otherwise hand-build with a `Grid` | [`SimpleTable`](#simpletable) |
| Interactive table | Sorting, paging, filtering, selection, editing over typed rows | [`DataGrid<T>`](#datagrid-t) |
| Hierarchy | Nested, expandable nodes (file tree, org chart) | [`TreeView`](#loam-controls-treeview-loam-controls-treeviewitem) |
| Switch views in place | One region, several panels; only one visible at a time | [`Tabs`](#tabs) |
| Progressive disclosure | Optional sections the user expands (FAQ, settings groups) | [`ExpansionPanels`](#expansionpanels-expansionpanel) |
| Chronological history | Ordered events down (or across) a connector line | [`Timeline`](#timeline) |
| Rotating content | One featured slide at a time, with optional auto-play | [`Carousel`](#loam-controls-carousel) |
| Page a long result set | Jump between pages of an external/manual data source | [`Pagination`](#pagination) |
| Guide through steps | A linear, ordered wizard with Back/Next | [`Stepper`](#stepper) |

`Color`, `Size`, and `Variant` mean the same thing here as everywhere else — see
[Components overview → common parameters](./overview#common-parameters) and [Theming](/guide/theming).

---

## List / ListItem / ListSubheader

`List` is a vertical container (`StackPanel` subclass) that holds `ListItem` rows, mirroring the reference API's `List` / `ListItem`. Each `ListItem` is a `ContentControl` that optionally shows a leading icon and highlights on hover. `ListSubheader` (a `Text` subclass) provides a muted, semibold section label with list-aligned padding.

**Use it when** you need a vertical menu of tappable rows — a navigation rail, a settings list, an inbox. For *tabular* data with columns, use [`SimpleTable`](#simpletable) or [`DataGrid<T>`](#datagrid-t); for *nested* rows, use [`TreeView`](#loam-controls-treeview-loam-controls-treeviewitem).

### ListItem properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Icon` | `string?` | `null` | SVG path for the leading icon. Set to `null` or empty to hide the icon. Mirrors the reference API's `Icon`. |
| `Content` | `object?` | `null` | Row content (inherited from `ContentControl`). |
| `SecondaryText` | `string?` | `null` | Optional supporting line rendered below the main content. Folded into the row's automation name. |
| `Action` | `object?` | `null` | Optional trailing visual, usually an `IconButton` or status `Chip`. |
| `IsSelected` | `bool` | `false` | Whether the row is shown in the selected state. |
| `Activated` *(event)* | `EventHandler<RoutedEventArgs>` | — | Raised (bubbling) when the row is activated by pointer or by <kbd>Enter</kbd>/<kbd>Space</kbd> while focused. |

### ListSubheader

`ListSubheader` has no additional properties beyond `Text`. Typography (`Typo.Caption`), weight (`SemiBold`), padding, and secondary foreground are applied in the constructor and cannot be overridden per-instance via properties.

```csharp
var list = new List
{
    Children =
    {
        new ListSubheader { Text = "Tasks" },
        new ListItem { Icon = Icons.Material.Filled.Check, Content = new Text { Text = "Ready for review" } },
        new ListItem { Icon = Icons.Material.Filled.Star,  Content = new Text { Text = "Pinned milestone" } },
        new ListSubheader { Text = "Archive" },
        new ListItem { Content = new Text { Text = "Older releases" } },
    },
};
```

`List` itself is a plain `StackPanel`, so rows are not auto-selected as a group — wire `Activated` on each `ListItem` (and set `IsSelected` yourself) when you want single-select menu behavior:

```csharp
foreach (var item in list.Children.OfType<ListItem>())
{
    item.Activated += (sender, _) =>
    {
        foreach (var row in list.Children.OfType<ListItem>())
        {
            row.IsSelected = ReferenceEquals(row, sender);
        }
    };
}
```

---

## SimpleTable

A lightweight data table hosted on an elevated `Paper` surface, mirroring the reference API's `SimpleTable`. Populate `Headers` with column labels and `Rows` with `TableRow` instances. Cell values may be plain strings or any `Control`.

**Use it when** you have a small, fixed set of rows that never sort, page, or filter — a spec sheet, a summary, a comparison you assemble by hand. The moment the data is dynamic or the user wants to sort it, switch to [`DataGrid<T>`](#datagrid-t).

### TableRow

| Member | Type | Description |
|--------|------|-------------|
| `Cells` | `IList<object?>` | Left-to-right cell values. A value may be a `string` (rendered as `Text`) or any `Control`. |
| `TableRow(params object?[] cells)` | constructor | Convenience constructor. |

### SimpleTable properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Headers` | `ObservableCollection<string>` | empty | Column header labels. |
| `Rows` | `ObservableCollection<TableRow>` | empty | Data rows. |
| `Striped` | `bool` | `false` | Alternating row shading. Mirrors the reference API's `Striped`. |
| `Hover` | `bool` | `false` | Row highlight on pointer-over. Mirrors the reference API's `Hover`. |
| `Bordered` | `bool` | `false` | Cell grid lines. Mirrors the reference API's `Bordered`. |
| `Dense` | `bool` | `false` | Compact cell padding. Mirrors the reference API's `Dense`. |
| `Elevation` | `int` | `1` | Paper surface elevation. Mirrors the reference API's `Elevation`. |

```csharp
var table = new SimpleTable
{
    Striped   = true,
    Hover     = true,
    Bordered  = false,
    Dense     = false,
    Elevation = 2,
};

table.Headers.Add("Name");
table.Headers.Add("Role");
table.Headers.Add("Status");

table.Rows.Add(new TableRow("Alice", "Admin",  "Active"));
table.Rows.Add(new TableRow("Bob",   "Viewer", "Inactive"));
// Mix strings and controls in the same row:
table.Rows.Add(new TableRow("Carol", "Editor", new Chip { Text = "Pending", Color = LoamColor.Warning }));
```

---

## DataGrid&lt;T&gt;

A typed data grid that renders `Items` across strongly typed `DataGridColumn<T>` definitions with clickable sort headers, optional paging, row striping/hover, and single-row selection. Mirrors the reference API's `DataGrid`. Because `DataGrid<T>` is generic it is implemented as a `Decorator`; sort/paging statics live on the companion `DataGrids` class.

**Use it when** the table is the workhorse of a screen: many rows, a typed model, and users who expect to sort, filter, page, select, edit cells, group, freeze columns, or export. It is the recommended default for any data-shaped content (see the note above). Bind `Items` to an `ObservableCollection<T>` and the grid tracks add/remove/reset for you.

### DataGrid&lt;T&gt; properties

| Property / Member | Type | Default | Description |
|-------------------|------|---------|-------------|
| `Columns` | `ObservableCollection<DataGridColumn<T>>` | empty | Column definitions. |
| `Items` | `IEnumerable<T>?` | `null` | Source rows. When the source implements `INotifyCollectionChanged` (e.g. `ObservableCollection<T>`), the grid observes it and refreshes on add/remove/reset — no need to reassign `Items`. |
| `ObserveItemChanges` | `bool` | `false` | When true and rows implement `INotifyPropertyChanged`, the grid also refreshes when a row raises a property change. |
| `Refresh()` | `void` | — | Forces a refresh; call after mutating a non-observable source in place. |
| `ExportCsv()` / `ExportTsv()` | `string` | — | The current view (filtered + sorted, all pages) as CSV / TSV, using each column's display text with RFC-4180 quoting. Backed by the static `DataGrids.ToDelimited<T>(rows, columns, separator)` helper. |
| `CopyToClipboardAsync()` | `Task<string?>` | — | Copies the selected rows — or the whole current view when nothing is selected — to the system clipboard as TSV and returns the copied text; returns `null` when no clipboard is available. Also wired to **Ctrl+C** / **Cmd+C** when focus is within the grid. |
| `SelectionMode` | `DataGridSelectionMode` | `Single` | Row selection: `None` (disabled), `Single`, or `Multiple`. A plain click (or Space/Enter on the focused row) selects, replacing any existing selection. In `Multiple`, **Ctrl**-click / **Ctrl+Space** toggles a row and **Shift**-click / **Shift+Space** selects a range from the anchor. A disabled grid is not selectable. |
| `SelectedItem` | `T?` | `null` | The selected row (the primary/last-affected one in `Multiple`). Two-way friendly; assigning replaces any existing selection and raises `SelectionChanged` when the value changes. |
| `SelectedItems` | `IReadOnlyList<T>` | empty | A snapshot of the selected rows in view order. |
| `SelectionChanged` | `event Action<T?>` | — | Raised when the selection changes; the argument is the primary selected item (or default when empty). |
| `IsLoading` | `bool` | `false` | Shows a skeleton loading body (state precedence: Error > Loading > Empty > data). |
| `ErrorText` / `ErrorContent` | `string?` / `Control?` | `null` | Shows an error body instead of rows; `ErrorContent` overrides `ErrorText`. |
| `OnRetry` | `Action?` | `null` | When set, the error body shows a **Retry** button that invokes this. |
| `SkeletonRowCount` | `int` | `6` | Number of skeleton rows in the loading state. |
| `ShowFooter` | `bool` | `false` | Renders a footer row of per-column aggregates (over the current filtered rows, all pages), aligned to the column layout. |
| `PageSize` | `int` | `0` | Rows per page; `0` disables paging. Mirrors the reference API's `RowsPerPage`. |
| `Page` | `int` | `1` | Current 1-based page. |
| `FilterText` | `string?` | `null` | Text passed to the filter pipeline before sorting/paging. |
| `Filter` | `Func<T, string, bool>?` | `null` | Custom row predicate for `FilterText`; defaults to searching rendered cell values. |
| `Virtualize` | `bool` | `false` | Limits unpaged rendering to `MaxRenderedRows`. |
| `MaxRenderedRows` | `int` | `200` | Maximum rows rendered when `Virtualize` is enabled and paging is off. |
| `Striped` | `bool` | `true` | Alternating row shading. Mirrors the reference API's `Striped`. |
| `Hover` | `bool` | `true` | Row hover highlight. Mirrors the reference API's `Hover`. |
| `Dense` | `bool` | `false` | Compact cell padding. Mirrors the reference API's `Dense`. |
| `Elevation` | `int` | `1` | Host paper elevation. Mirrors the reference API's `Elevation`. |
| `GroupBy` | `Func<T, object?>?` | `null` | Groups rows by key with a group-header row (key + count) above each group, in first-appearance order (follows the current sort). Applies within the rendered page. |
| `CollapsibleGroups` | `bool` | `true` | When grouped, lets the user click (or keyboard-activate) a group header to collapse/expand its rows. Collapsed state is keyed by group key and survives re-renders. |
| `GroupAggregate` | `Func<IReadOnlyList<T>, string>?` | `null` | Optional text appended to each group header, computed from the group's items (e.g. a sum or average). |
| `EmptyText` | `string` | `"No data"` | Text shown below the header when there are no rows to display after filtering. |
| `EmptyContent` | `Control?` | `null` | Custom empty-state content; overrides `EmptyText` when set. |
| `FrozenColumns` | `int` | `0` | Number of leading columns to pin while the rest scroll horizontally. Ignored while grouped, or if not less than the column count. Frozen layouts size every column by pixel width. |
| `RowHeight` | `double` | `0` | Fixed body-row height in px (`0` = auto). Guarantees row alignment across the frozen/scrollable panes for custom-height cells. |

### DataGridColumn&lt;T&gt; properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `string` | _(required)_ | Column header text. |
| `Value` | `Func<T, object?>` | _(required)_ | Projects a row to its cell value (used for display and sorting). |
| `Format` | `string?` | `null` | Optional .NET format string applied to the cell value (e.g. `"N2"`). |
| `Sortable` | `bool` | `true` | Whether clicking the header sorts by this column. |
| `Align` | `HorizontalAlignment` | `Left` | Cell content alignment. |
| `Width` | `double?` | `null` | Fixed pixel width; `null` sizes with star (shares remaining space). In a frozen-column layout, columns without a width get a default pixel width. |
| `CellTemplate` | `Func<T, Control>?` | `null` | Custom cell content. |
| `Editable` | `bool` | `false` | Renders a text editor for this column when `SetText` is provided. |
| `SetText` | `Action<T, string?>?` | `null` | Applies edited text back to the row. |
| `Summary` | `Func<IReadOnlyList<T>, string>?` | `null` | Custom footer text for this column (shown when the grid's `ShowFooter` is on). |
| `SummaryKind` | `DataGridSummary?` | `null` | Built-in footer aggregate when `Summary` is null: `Sum`/`Average`/`Min`/`Max` over the column's numeric values, or `Count`. Honors `Format`. |

The two-argument constructor — `new DataGridColumn<T>(header, value)` — is the only way to set the required `Header` and `Value`; everything else is an `init` property set in the object initializer.

### DataGrids static helpers

| Method | Signature | Description |
|--------|-----------|-------------|
| `Sort<T>` | `(IReadOnlyList<T> items, DataGridColumn<T>? column, bool descending) → IReadOnlyList<T>` | Sorts `items` by `column.Value`; returns original order when `column` is `null`. |
| `PageCount` | `(int count, int pageSize) → int` | Total page count for `count` rows at `pageSize` (`0` = 1 page). |
| `Filter<T>` | `(IReadOnlyList<T> items, string? text, Func<T, string, bool> predicate) → IReadOnlyList<T>` | Returns matching rows when `text` has content; otherwise returns the original rows. |
| `Group<T>` | `(IReadOnlyList<T> items, Func<T, object?> selector) → IReadOnlyList<DataGridGroup<T>>` | Groups rows by key in first-appearance order; a `null` key forms its own group. |

### Keyboard

Rows are focusable. When a row has focus:

| Key | Action |
|-----|--------|
| <kbd>↑</kbd> / <kbd>↓</kbd> | Move focus to the previous / next rendered row. |
| <kbd>Home</kbd> / <kbd>End</kbd> | Move focus to the first / last rendered row. |
| <kbd>Space</kbd> / <kbd>Enter</kbd> | Select the focused row (toggles it in `Multiple`). |
| <kbd>Shift</kbd> + <kbd>↑</kbd>/<kbd>↓</kbd>/<kbd>Home</kbd>/<kbd>End</kbd> | Extend the selection to the focused row (`Multiple`). |
| <kbd>Ctrl</kbd> + <kbd>A</kbd> | Select every rendered row — the current page, expanded groups only (`Multiple`). |
| <kbd>Esc</kbd> | Clear the selection. |
| <kbd>Ctrl</kbd> + <kbd>C</kbd> / <kbd>Cmd</kbd> + <kbd>C</kbd> | Copy the selection (or the whole view) as TSV. |

In `Single` mode the selection follows focus as you move; navigation stays within the current page and never wraps.

```csharp
class Employee
{
    public string Name { get; set; } = "";
    public string Department { get; set; } = "";
    public decimal Salary { get; set; }
}

var grid = new DataGrid<Employee>
{
    Striped   = true,
    Hover     = true,
    PageSize  = 20,
    FilterText = searchText,
    Filter = (employee, text) =>
        employee.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
        employee.Department.Contains(text, StringComparison.OrdinalIgnoreCase),
    Elevation = 1,
};

grid.Columns.Add(new DataGridColumn<Employee>("Name", e => e.Name)
{
    Editable = true,
    SetText = (employee, text) => employee.Name = text ?? "",
});
grid.Columns.Add(new DataGridColumn<Employee>("Department", e => e.Department));
grid.Columns.Add(new DataGridColumn<Employee>("Salary",     e => e.Salary)
{
    Format  = "C2",
    Align   = HorizontalAlignment.Right,
});

grid.SelectionChanged += emp => Console.WriteLine($"Selected: {emp?.Name}");

grid.Items = employees; // IEnumerable<Employee>
```

::: details Loading, empty, and error states
The grid renders a single body state at a time, in the precedence **Error > Loading > Empty > data**. Drive them from your view model rather than swapping the grid out:

```csharp
grid.IsLoading = true;                 // skeleton rows while fetching
// …on failure:
grid.IsLoading = false;
grid.ErrorText = "Couldn't load employees.";
grid.OnRetry   = () => ViewModel.Reload();   // shows a Retry button
// …on success with no rows:
grid.ErrorText = null;
grid.EmptyText = "No employees match your filter.";
```
:::

---

## Loam.Controls.TreeView / Loam.Controls.TreeViewItem

A hierarchical tree that mirrors the reference API's `TreeView` / `TreeViewItem`. Root nodes are added to `TreeView.Items`; each `TreeViewItem` may have its own `Items` collection for nested children. Clicking a node selects it and updates `TreeView.SelectedItem`. Nodes with children show a chevron that toggles `Expanded`. Tree rows are focusable; Enter selects a row and Space toggles expandable rows.

**Use it when** the data is genuinely nested — a file system, a category hierarchy, an org chart. For a flat menu, prefer [`List`](#list-listitem-listsubheader); for flat-but-groupable rows, prefer [`DataGrid<T>`](#datagrid-t) with `GroupBy`.

### Loam.Controls.TreeView properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<Loam.Controls.TreeViewItem>` | empty | Root nodes. |
| `SelectedItem` | `Loam.Controls.TreeViewItem?` | `null` | The selected node (two-way). |

### Loam.Controls.TreeViewItem properties

| Property / Member | Type | Default | Description |
|-------------------|------|---------|-------------|
| `Text` | `string?` | `null` | Node label. Mirrors the reference API's `Text`. |
| `Icon` | `string?` | `null` | Leading icon path. Mirrors the reference API's `Icon`. |
| `Expanded` | `bool` | `false` | Whether children are shown (two-way). |
| `IsSelected` | `bool` | `false` | Whether this node is the selected node. |
| `Items` | `ObservableCollection<Loam.Controls.TreeViewItem>` | empty | Child nodes. |
| `ItemSelected` | `event EventHandler<RoutedEventArgs>` | — | Raised (bubbling) when this node's row is clicked. |

```csharp
var tree = new Loam.Controls.TreeView();

var parent = new Loam.Controls.TreeViewItem
{
    Text     = "Documents",
    Icon     = Icons.Material.Filled.Article,
    Expanded = true,
};
parent.Items.Add(new Loam.Controls.TreeViewItem { Text = "Report.pdf",  Icon = Icons.Material.Filled.Article });
parent.Items.Add(new Loam.Controls.TreeViewItem { Text = "Budget.xlsx", Icon = Icons.Material.Filled.Table });

tree.Items.Add(parent);
tree.Items.Add(new Loam.Controls.TreeViewItem { Text = "Downloads", Icon = Icons.Material.Filled.CloudUpload });
```

---

## Tabs

A tab strip with switchable content, mirroring the reference API's `Tabs`. Add `TabItem` instances to `Items`; the active header is underlined in the accent `Color`.

**Use it when** several views share one screen region and the user looks at one at a time — Overview / Analytics / Settings. Tabs *switch* content in place; they are not navigation between top-level destinations (use [NavMenu](./navigation) for that) and not a linear sequence (use [`Stepper`](#stepper)).

### TabItem properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `string?` | `null` | Tab header text. Mirrors the reference API's `TabPanel` label. |
| `Content` | `Control?` | `null` | Content shown when the tab is selected. |

### Tabs properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<TabItem>` | empty | The tabs. |
| `SelectedIndex` | `int` | `0` | Index of the active tab. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Underline accent color for the active tab. Mirrors the reference API's `Color`. |

```csharp
var tabs = new Tabs
{
    Color         = LoamColor.Primary,
    SelectedIndex = 0,
};

tabs.Items.Add(new TabItem("Overview",  new Text { Text = "Overview content" }));
tabs.Items.Add(new TabItem("Analytics", new Text { Text = "Analytics content" }));
tabs.Items.Add(new TabItem("Settings",  new Text { Text = "Settings content" }));
```

---

## ExpansionPanels / ExpansionPanel

A stacked accordion of collapsible sections, mirroring the reference API's `ExpansionPanels` / `ExpansionPanel`. By default, expanding one panel collapses the others (accordion mode); set `MultiExpansion` to allow several open at once. Headers are focusable, expose an automation name from `Header`, and toggle with Enter or Space.

**Use it when** the page has more sections than fit comfortably and each is optional — a FAQ, grouped settings, an order's collapsible detail blocks. If switching is exclusive and the content is *peer* views (not optional detail), prefer [`Tabs`](#tabs).

### ExpansionPanel properties

`ExpansionPanel` extends `HeaderedContentControl`.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `object?` | `null` | Panel header (inherited from `HeaderedContentControl`). |
| `Content` | `object?` | `null` | Revealed body content (inherited from `ContentControl`). |
| `IsExpanded` | `bool` | `false` | Whether the panel is open (two-way). Mirrors the reference API's `IsExpanded`. |

### ExpansionPanels properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Panels` | `ObservableCollection<ExpansionPanel>` | empty | The contained panels. |
| `MultiExpansion` | `bool` | `false` | Allow multiple panels open simultaneously. Mirrors the reference API's `MultiExpansion`. |

```csharp
var accordion = new ExpansionPanels { MultiExpansion = false };

accordion.Panels.Add(new ExpansionPanel
{
    Header    = "Shipping",
    Content   = new Text { Text = "Ships within 2–3 business days." },
    IsExpanded = true,
});
accordion.Panels.Add(new ExpansionPanel
{
    Header  = "Returns",
    Content = new Text { Text = "Free returns within 30 days." },
});
accordion.Panels.Add(new ExpansionPanel
{
    Header  = "Warranty",
    Content = new Text { Text = "12-month manufacturer warranty." },
});
```

::: warning Accordion vs. multi-expand
With `MultiExpansion = false` (the default), setting `IsExpanded = true` on more than one panel up front is contradictory — opening any panel will close the others as soon as the user interacts. Set `MultiExpansion = true` when you genuinely want several sections open at once.
:::

---

## Timeline

A timeline that renders `TimelineItem` entries down (or across) a connector line, each with a colored dot beside a `Paper` content card. Mirrors the reference API's `Timeline` / `TimelineItem`. Implemented as a `Decorator` (no `ControlTheme` required).

**Use it when** the data is an ordered sequence of events you want to *show* — order history, an audit trail, a changelog. It is read-only chronology; for a sequence the user *advances through*, use [`Stepper`](#stepper).

### TimelineItem properties

| Property / Member | Type | Default | Description |
|-------------------|------|---------|-------------|
| `Content` | `object?` | `null` | Entry content (string or any `Control`). When set, it wins over the generated layout below. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Dot color. Mirrors the reference API's `Color`. |
| `Title` | `string?` | `null` | Used by the generated card layout when `Content` is empty. |
| `Subtitle` | `string?` | `null` | Supporting text in the generated layout. |
| `TimeText` | `string?` | `null` | Optional time/metadata line rendered above the title in the generated layout. |
| `TimelineItem(object? content, LoamColor color = Primary)` | constructor | — | Content + dot color. |
| `TimelineItem(string title, string? subtitle, string? timeText = null, LoamColor color = Primary)` | constructor | — | Generated title/subtitle/time layout. |

### Timeline properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<TimelineItem>` | empty | Entries displayed in order. |
| `Orientation` | `Orientation` | `Vertical` | Lay entries top-to-bottom (`Vertical`) or left-to-right (`Horizontal`). |

```csharp
var timeline = new Timeline();

timeline.Items.Add(new TimelineItem("Order placed",    LoamColor.Primary));
timeline.Items.Add(new TimelineItem("Payment confirmed", LoamColor.Success));
timeline.Items.Add(new TimelineItem("Dispatched",      LoamColor.Info));
timeline.Items.Add(new TimelineItem("Out for delivery", LoamColor.Warning));
```

Use the title/subtitle/time constructor for a richer entry without building a control yourself:

```csharp
using Avalonia.Layout;

var history = new Timeline { Orientation = Orientation.Vertical };
history.Items.Add(new TimelineItem("Order placed", "Confirmation emailed", "09:14", LoamColor.Primary));
history.Items.Add(new TimelineItem("Dispatched",   "Left the warehouse",   "13:02", LoamColor.Info));
```

---

## Loam.Controls.Carousel

A slideshow that displays one `CarouselItem` at a time with optional prev/next arrows and clickable bullet indicators. Mirrors the reference API's `Carousel` / `CarouselItem`. Navigation wraps around.

**Use it when** you have a small set of equally important, rotating items — a featured-content banner, an onboarding tour, an image gallery. For long lists the user scans linearly, a [`List`](#list-listitem-listsubheader) is clearer; for paging through records, use [`Pagination`](#pagination).

### CarouselItem properties

| Property / Member | Type | Default | Description |
|-------------------|------|---------|-------------|
| `Content` | `object?` | `null` | Slide content (string or any `Control`). When set, it wins over the generated layout. |
| `Title` | `string?` | `null` | Title used by the generated slide layout when `Content` is empty. |
| `Subtitle` | `string?` | `null` | Supporting text in the generated slide layout. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Semantic color for the generated slide surface. |
| `CarouselItem(object? content)` | constructor | — | Slide from any content. |
| `CarouselItem(string title, string? subtitle, LoamColor color = Primary)` | constructor | — | Generated title/subtitle slide. |

### Loam.Controls.Carousel properties

| Property / Member | Type | Default | Description |
|-------------------|------|---------|-------------|
| `Items` | `ObservableCollection<CarouselItem>` | empty | The slides. |
| `SelectedIndex` | `int` | `0` | Visible slide index (two-way). |
| `ShowArrows` | `bool` | `true` | Whether prev/next arrow buttons are shown. Mirrors the reference API's `ShowArrows`. |
| `ShowBullets` | `bool` | `true` | Whether bullet indicators are shown. Mirrors the reference API's `ShowBullets`. |
| `AutoPlay` | `bool` | `false` | Advances automatically while attached, enabled, and showing at least two slides. |
| `AutoPlayInterval` | `TimeSpan` | `4s` | How often `AutoPlay` advances. |
| `SelectedIndexChanged` | `event EventHandler<int>` | — | Raised after the visible slide changes. |
| `Next()` / `Previous()` | `void` | — | Advance / go back one slide, wrapping around. |
| `GoTo(int index)` | `void` | — | Jump to a slide, clamped to the available range. |

```csharp
var carousel = new Loam.Controls.Carousel
{
    ShowArrows  = true,
    ShowBullets = true,
};

carousel.Items.Add(new CarouselItem(new Image { Source = new Bitmap("slide1.png") }));
carousel.Items.Add(new CarouselItem(new Image { Source = new Bitmap("slide2.png") }));
carousel.Items.Add(new CarouselItem(new Text  { Text = "Coming soon" }));
```

::: tip Auto-play, accessibly
`AutoPlay` is off by default — and that's usually right. Motion that the user can't pause is a real accessibility problem. If you enable it, keep `ShowArrows`/`ShowBullets` on so there's always a manual control, and lean on a calm `AutoPlayInterval` (the 4-second default) rather than a snappy one.
:::

---

## Pagination

A page navigator that renders boundary pages, a configurable window of pages around the selection, ellipsis gaps, and prev/next arrows. Mirrors the reference API's `Pagination`. Also used internally by `DataGrid<T>` when `PageSize > 0`.

**Use it when** you page through an *external or manual* data source (server-side paging, a non-grid layout) and want a standalone pager. Inside a [`DataGrid<T>`](#datagrid-t), set `PageSize` instead — the grid builds and wires its own pager for you.

### Pagination properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Count` | `int` | `1` | Total number of pages. Mirrors the reference API's `Count`. |
| `Selected` | `int` | `1` | Current 1-based page (two-way). Mirrors the reference API's `Selected`. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Selected page button color. Mirrors the reference API's `Color`. |
| `BoundaryCount` | `int` | `1` | Pages shown at each end. Mirrors the reference API's `BoundaryCount`. |
| `MiddleCount` | `int` | `3` | Pages shown around the selection. Mirrors the reference API's `MiddleCount`. |
| `ShowFirstLast` | `bool` | `false` | Adds first-page and last-page boundary buttons flanking the arrows. |
| `ShowRange` | `bool` | `false` | With `PageSize`/`TotalItems`, shows a "Showing X–Y of N" summary. |
| `PageSize` / `TotalItems` | `int` | `0` | Rows per page and total item count, used for the range summary. |

`DataGrid<T>`'s built-in pager enables `ShowFirstLast` and `ShowRange` automatically.

### Static helper

| Method | Signature | Description |
|--------|-----------|-------------|
| `BuildPages` | `(int count, int selected, int boundary, int middle) → IReadOnlyList<int>` | Returns the page layout as a list of 1-based page numbers; `0` marks an ellipsis gap. |

```csharp
var pager = new Pagination
{
    Count         = 20,
    Selected      = 1,
    Color         = LoamColor.Primary,
    BoundaryCount = 1,
    MiddleCount   = 3,
};

pager.GetObservable(Pagination.SelectedProperty).Subscribe(page =>
{
    Console.WriteLine($"Navigated to page {page}");
});
```

---

## Stepper

A linear step wizard that displays numbered `Step` entries with connector lines, the active step's content, and Back / Next (Finish) navigation. Mirrors the reference API's `Stepper` / `Step`. Advancing through the last step marks it complete and fires `OnCompleted`.

**Use it when** a task is genuinely sequential and the order matters — checkout, account setup, a multi-page form. The Next button reads "Finish" on the last step. For peer views the user can visit in any order, use [`Tabs`](#tabs); for showing (not advancing) a sequence, use [`Timeline`](#timeline).

### Step properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | `string?` | `null` | Step header title. |
| `Content` | `object?` | `null` | Step body content (string or any `Control`). |
| `Completed` | `bool` | `false` | Whether the step is marked complete (set automatically by `Next()`). |

### Stepper properties

| Property / Member | Type | Default | Description |
|-------------------|------|---------|-------------|
| `Steps` | `ObservableCollection<Step>` | empty | The wizard steps. |
| `ActiveIndex` | `int` | `0` | The active step index (two-way). |
| `OnCompleted` | `Action?` | `null` | Invoked when the final step is finished. |
| `Next()` | `void` | — | Advances to the next step, marking the current one complete; calls `OnCompleted` on the last step. |
| `Previous()` | `void` | — | Returns to the previous step. |

```csharp
var stepper = new Stepper
{
    OnCompleted = () => Console.WriteLine("Wizard finished"),
};

stepper.Steps.Add(new Step("Account",  new Text { Text = "Enter your email and password." }));
stepper.Steps.Add(new Step("Profile",  new Text { Text = "Tell us about yourself." }));
stepper.Steps.Add(new Step("Confirm",  new Text { Text = "Review and submit." }));
```

---

## Recipe: a paged list with a header tab

A small dashboard slice — [`Tabs`](#tabs) switching views, a [`List`](#list-listitem-listsubheader) of rows, and a standalone [`Pagination`](#pagination) below it driving which page of rows is shown. Everything is plain C#; lay it out with a `StackPanel` (see [Surfaces & layout](./layout)).

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

string[] allItems = Enumerable.Range(1, 42).Select(i => $"Message {i}").ToArray();
const int pageSize = 10;

var list = new List();
var pager = new Pagination
{
    Count      = DataGrids.PageCount(allItems.Length, pageSize),
    Selected   = 1,
    ShowRange  = true,
    PageSize   = pageSize,
    TotalItems = allItems.Length,
};

void ShowPage(int page)
{
    list.Children.Clear();
    foreach (var text in allItems.Skip((page - 1) * pageSize).Take(pageSize))
    {
        list.Children.Add(new ListItem
        {
            Icon    = Icons.Material.Filled.Article,
            Content = new Text { Text = text },
        });
    }
}

pager.GetObservable(Pagination.SelectedProperty).Subscribe(ShowPage);
ShowPage(1);

var inbox = new StackPanel { Spacing = 8, Children = { list, pager } };

var tabs = new Tabs { Color = LoamColor.Primary };
tabs.Items.Add(new TabItem("Inbox",   inbox));
tabs.Items.Add(new TabItem("Archive", new Text { Text = "Nothing archived yet." }));
```

## Accessibility & keyboard

Every interactive control on this page is keyboard-operable and carries an automation name out of the box. Across the family, <kbd>Enter</kbd> and <kbd>Space</kbd> are the activation keys, disabled controls drop out of the tab order, and arrow keys navigate *within* a control once it has focus.

- **`List` / `ListItem`** — rows are focusable; <kbd>Tab</kbd> moves between them and <kbd>Enter</kbd>/<kbd>Space</kbd> raise `Activated`. `SecondaryText` is folded into the row's announced name.
- **`Tabs`** — headers are focusable; <kbd>Enter</kbd>/<kbd>Space</kbd> select the focused header, <kbd>←</kbd>/<kbd>↓</kbd> move to the previous tab and <kbd>→</kbd>/<kbd>↑</kbd> to the next. The strip announces "Tab N of M".
- **`ExpansionPanel`** — headers are focusable and announce "Expanded"/"Collapsed"; <kbd>Enter</kbd>/<kbd>Space</kbd> toggle the panel.
- **`TreeView`** — nodes are focusable; <kbd>Enter</kbd> selects, <kbd>Space</kbd> toggles an expandable node (or selects a leaf), <kbd>→</kbd> expands or steps into children, <kbd>←</kbd> collapses or steps to the parent, and <kbd>↑</kbd>/<kbd>↓</kbd> move through the visible enabled nodes.
- **`Carousel`** — when the carousel has focus, <kbd>←</kbd>/<kbd>→</kbd> move to the previous/next slide; the arrows and bullets are individually focusable and activate with <kbd>Enter</kbd>/<kbd>Space</kbd>. Each bullet announces "Slide N" and "Selected"/"Not selected".
- **`Pagination`** — the arrows and page numbers are buttons: <kbd>Tab</kbd> to a page, <kbd>Enter</kbd>/<kbd>Space</kbd> to go there. The selected page announces "Page N, selected".
- **`DataGrid<T>`** — see the dedicated [Keyboard](#keyboard) table above.
- **`Stepper`** — drive the wizard with the focusable **Back** / **Next** (named "Previous step" / "Next step", "Finish steps" on the last step) buttons; the numbered markers are decorative and not focusable.

::: tip Name your icon-only rows
A `ListItem` whose `Content` is just an icon, or a `TreeViewItem` with only an `Icon`, has no text for assistive technology to read. Give it readable text — set the node's `Text`, the row's `Content`/`SecondaryText`, or an explicit automation name:

```csharp
using Avalonia.Automation;

var row = new ListItem { Icon = Icons.Material.Filled.Star };
AutomationProperties.SetName(row, "Pinned");
```
:::

## See also

- [Display primitives](./display) — `Text`, `Icon`, `Chip`, and the glyph set behind every `Icon` property here.
- [Buttons & menus](./buttons) — the `Button`/`IconButton` family that powers `Pagination`, `Stepper`, and `Carousel` chrome.
- [Form inputs](./inputs) — pair a [TextField](./inputs#textfield) with `DataGrid<T>.FilterText`, or host a [Form](./inputs#form) inside a `Stepper` step.
- [Navigation](./navigation) — for moving between top-level destinations rather than switching views with `Tabs`.
- [Surfaces & layout](./layout) — `Paper`, `StackPanel`, and the layout pieces these recipes compose.
- [Theming](/guide/theming) — how `Color`, `Size`, and `Variant` resolve to tokens.
- [v3 → v3.1 migration guide](/migration/v3-to-v3.1) — moving the `Loam.Data` controls into the satellite package.
