---
title: Data display
---

# Data display

Controls for presenting structured information: lists, tables, grids, trees, tabs, timelines, carousels, paginators, and step wizards. All controls live in the `Loam.Controls` namespace; enums (`LoamColor`, `LoamSize`, `Variant`, `Typo`, `HorizontalAlignment`) are in the `Loam` namespace.

> **Name collision note.** `TreeView`, `TreeViewItem`, and `Carousel` exist in both Loam and `Avalonia.Controls`. Qualify Loam types explicitly — `Loam.Controls.TreeView`, `Loam.Controls.TreeViewItem`, `Loam.Controls.Carousel` — when both namespaces are in scope.

---

## List / ListItem / ListSubheader

`List` is a vertical container (`StackPanel` subclass) that holds `ListItem` rows, mirroring MudBlazor's `MudList` / `MudListItem`. Each `ListItem` is a `ContentControl` that optionally shows a leading icon and highlights on hover. `ListSubheader` (a `Text` subclass) provides a muted, semibold section label with list-aligned padding.

### ListItem properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Icon` | `string?` | `null` | SVG path for the leading icon. Set to `null` or empty to hide the icon. Mirrors MudBlazor's `Icon`. |
| `Content` | `object?` | `null` | Row content (inherited from `ContentControl`). |

### ListSubheader

`ListSubheader` has no additional properties beyond `Text`. Typography (`Typo.Caption`), weight (`SemiBold`), padding, and secondary foreground are applied in the constructor and cannot be overridden per-instance via properties.

```csharp
var list = new List
{
    Children =
    {
        new ListSubheader { Text = "Fruits" },
        new ListItem { Icon = Icons.Material.Filled.Apple,  Content = new Text { Text = "Apple" } },
        new ListItem { Icon = Icons.Material.Filled.Grass,  Content = new Text { Text = "Mango" } },
        new ListSubheader { Text = "Vegetables" },
        new ListItem { Content = new Text { Text = "Carrot" } },
    },
};
```

---

## SimpleTable

A lightweight data table hosted on an elevated `Paper` surface, mirroring MudBlazor's `MudSimpleTable`. Populate `Headers` with column labels and `Rows` with `TableRow` instances. Cell values may be plain strings or any `Control`.

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
| `Striped` | `bool` | `false` | Alternating row shading. Mirrors MudBlazor's `Striped`. |
| `Hover` | `bool` | `false` | Row highlight on pointer-over. Mirrors MudBlazor's `Hover`. |
| `Bordered` | `bool` | `false` | Cell grid lines. Mirrors MudBlazor's `Bordered`. |
| `Dense` | `bool` | `false` | Compact cell padding. Mirrors MudBlazor's `Dense`. |
| `Elevation` | `int` | `1` | Paper surface elevation. Mirrors MudBlazor's `Elevation`. |

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

A typed data grid that renders `Items` across strongly typed `DataGridColumn<T>` definitions with clickable sort headers, optional paging, row striping/hover, and single-row selection. Mirrors MudBlazor's `MudDataGrid`. Because `DataGrid<T>` is generic it is implemented as a `Decorator`; sort/paging statics live on the companion `DataGrids` class.

### DataGrid&lt;T&gt; properties

| Property / Member | Type | Default | Description |
|-------------------|------|---------|-------------|
| `Columns` | `ObservableCollection<DataGridColumn<T>>` | empty | Column definitions. |
| `Items` | `IEnumerable<T>?` | `null` | Source rows. Mirrors MudBlazor's `Items`. |
| `PageSize` | `int` | `0` | Rows per page; `0` disables paging. Mirrors MudBlazor's `RowsPerPage`. |
| `Page` | `int` | `1` | Current 1-based page. |
| `SelectedItem` | `T?` | `default` | Selected row; row click updates this. Mirrors MudBlazor's `SelectedItem`. |
| `Striped` | `bool` | `true` | Alternating row shading. Mirrors MudBlazor's `Striped`. |
| `Hover` | `bool` | `true` | Row hover highlight. Mirrors MudBlazor's `Hover`. |
| `Dense` | `bool` | `false` | Compact cell padding. Mirrors MudBlazor's `Dense`. |
| `Elevation` | `int` | `1` | Host paper elevation. Mirrors MudBlazor's `Elevation`. |
| `SelectionChanged` | `event Action<T?>?` | — | Raised when a row is clicked and `SelectedItem` changes. |

### DataGridColumn&lt;T&gt; properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `string` | _(required)_ | Column header text. |
| `Value` | `Func<T, object?>` | _(required)_ | Projects a row to its cell value (used for display and sorting). |
| `Format` | `string?` | `null` | Optional .NET format string applied to the cell value (e.g. `"N2"`). |
| `Sortable` | `bool` | `true` | Whether clicking the header sorts by this column. |
| `Align` | `HorizontalAlignment` | `Left` | Cell content alignment. |

### DataGrids static helpers

| Method | Signature | Description |
|--------|-----------|-------------|
| `Sort<T>` | `(IReadOnlyList<T> items, DataGridColumn<T>? column, bool descending) → IReadOnlyList<T>` | Sorts `items` by `column.Value`; returns original order when `column` is `null`. |
| `PageCount` | `(int count, int pageSize) → int` | Total page count for `count` rows at `pageSize` (`0` = 1 page). |

```csharp
record Employee(string Name, string Department, decimal Salary);

var grid = new DataGrid<Employee>
{
    Striped   = true,
    Hover     = true,
    PageSize  = 20,
    Elevation = 1,
};

grid.Columns.Add(new DataGridColumn<Employee>("Name",       e => e.Name));
grid.Columns.Add(new DataGridColumn<Employee>("Department", e => e.Department));
grid.Columns.Add(new DataGridColumn<Employee>("Salary",     e => e.Salary)
{
    Format  = "C2",
    Align   = HorizontalAlignment.Right,
});

grid.SelectionChanged += emp => Console.WriteLine($"Selected: {emp?.Name}");

grid.Items = employees; // IEnumerable<Employee>
```

---

## Loam.Controls.TreeView / Loam.Controls.TreeViewItem

A hierarchical tree that mirrors MudBlazor's `MudTreeView` / `MudTreeViewItem`. Root nodes are added to `TreeView.Items`; each `TreeViewItem` may have its own `Items` collection for nested children. Clicking a node selects it and updates `TreeView.SelectedItem`. Nodes with children show a chevron that toggles `Expanded`.

### Loam.Controls.TreeView properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<Loam.Controls.TreeViewItem>` | empty | Root nodes. |
| `SelectedItem` | `Loam.Controls.TreeViewItem?` | `null` | The selected node (two-way). |

### Loam.Controls.TreeViewItem properties

| Property / Member | Type | Default | Description |
|-------------------|------|---------|-------------|
| `Text` | `string?` | `null` | Node label. Mirrors MudBlazor's `Text`. |
| `Icon` | `string?` | `null` | Leading icon path. Mirrors MudBlazor's `Icon`. |
| `Expanded` | `bool` | `false` | Whether children are shown (two-way). |
| `IsSelected` | `bool` | `false` | Whether this node is the selected node. |
| `Items` | `ObservableCollection<Loam.Controls.TreeViewItem>` | empty | Child nodes. |
| `ItemSelected` | `event EventHandler<RoutedEventArgs>` | — | Raised (bubbling) when this node's row is clicked. |

```csharp
var tree = new Loam.Controls.TreeView();

var parent = new Loam.Controls.TreeViewItem
{
    Text     = "Documents",
    Icon     = Icons.Material.Filled.Folder,
    Expanded = true,
};
parent.Items.Add(new Loam.Controls.TreeViewItem { Text = "Report.pdf",  Icon = Icons.Material.Filled.PictureAsPdf });
parent.Items.Add(new Loam.Controls.TreeViewItem { Text = "Budget.xlsx", Icon = Icons.Material.Filled.TableChart });

tree.Items.Add(parent);
tree.Items.Add(new Loam.Controls.TreeViewItem { Text = "Downloads", Icon = Icons.Material.Filled.Folder });
```

---

## Tabs

A tab strip with switchable content, mirroring MudBlazor's `MudTabs`. Add `TabItem` instances to `Items`; the active header is underlined in the accent `Color`.

### TabItem properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `string?` | `null` | Tab header text. Mirrors MudBlazor's `MudTabPanel` label. |
| `Content` | `Control?` | `null` | Content shown when the tab is selected. |

### Tabs properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<TabItem>` | empty | The tabs. |
| `SelectedIndex` | `int` | `0` | Index of the active tab. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Underline accent color for the active tab. Mirrors MudBlazor's `Color`. |

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

A stacked accordion of collapsible sections, mirroring MudBlazor's `MudExpansionPanels` / `MudExpansionPanel`. By default, expanding one panel collapses the others (accordion mode); set `MultiExpansion` to allow several open at once.

### ExpansionPanel properties

`ExpansionPanel` extends `HeaderedContentControl`.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Header` | `object?` | `null` | Panel header (inherited from `HeaderedContentControl`). |
| `Content` | `object?` | `null` | Revealed body content (inherited from `ContentControl`). |
| `IsExpanded` | `bool` | `false` | Whether the panel is open (two-way). Mirrors MudBlazor's `IsExpanded`. |

### ExpansionPanels properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Panels` | `ObservableCollection<ExpansionPanel>` | empty | The contained panels. |
| `MultiExpansion` | `bool` | `false` | Allow multiple panels open simultaneously. Mirrors MudBlazor's `MultiExpansion`. |

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

---

## Timeline

A vertical timeline that renders `TimelineItem` entries down a connector line, each with a colored dot beside a `Paper` content card. Mirrors MudBlazor's `MudTimeline` / `MudTimelineItem`. Implemented as a `Decorator` (no `ControlTheme` required).

### TimelineItem properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Content` | `object?` | `null` | Entry content (string or any `Control`). |
| `Color` | `LoamColor` | `LoamColor.Primary` | Dot color. Mirrors MudBlazor's `Color`. |

### Timeline properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<TimelineItem>` | empty | Entries displayed top to bottom. |

```csharp
var timeline = new Timeline();

timeline.Items.Add(new TimelineItem("Order placed",    LoamColor.Primary));
timeline.Items.Add(new TimelineItem("Payment confirmed", LoamColor.Success));
timeline.Items.Add(new TimelineItem("Dispatched",      LoamColor.Info));
timeline.Items.Add(new TimelineItem("Out for delivery", LoamColor.Warning));
```

---

## Loam.Controls.Carousel

A slideshow that displays one `CarouselItem` at a time with optional prev/next arrows and clickable bullet indicators. Mirrors MudBlazor's `MudCarousel` / `MudCarouselItem`. Navigation wraps around.

### CarouselItem properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Content` | `object?` | `null` | Slide content (string or any `Control`). |

### Loam.Controls.Carousel properties

| Property / Member | Type | Default | Description |
|-------------------|------|---------|-------------|
| `Items` | `ObservableCollection<CarouselItem>` | empty | The slides. |
| `SelectedIndex` | `int` | `0` | Visible slide index (two-way). |
| `ShowArrows` | `bool` | `true` | Whether prev/next arrow buttons are shown. Mirrors MudBlazor's `ShowArrows`. |
| `ShowBullets` | `bool` | `true` | Whether bullet indicators are shown. Mirrors MudBlazor's `ShowBullets`. |
| `Next()` | `void` | — | Advances to the next slide, wrapping to the first. |
| `Previous()` | `void` | — | Returns to the previous slide, wrapping to the last. |

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

---

## Pagination

A page navigator that renders boundary pages, a configurable window of pages around the selection, ellipsis gaps, and prev/next arrows. Mirrors MudBlazor's `MudPagination`. Also used internally by `DataGrid<T>` when `PageSize > 0`.

### Pagination properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Count` | `int` | `1` | Total number of pages. Mirrors MudBlazor's `Count`. |
| `Selected` | `int` | `1` | Current 1-based page (two-way). Mirrors MudBlazor's `Selected`. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Selected page button color. Mirrors MudBlazor's `Color`. |
| `BoundaryCount` | `int` | `1` | Pages shown at each end. Mirrors MudBlazor's `BoundaryCount`. |
| `MiddleCount` | `int` | `3` | Pages shown around the selection. Mirrors MudBlazor's `MiddleCount`. |

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

A linear step wizard that displays numbered `Step` entries with connector lines, the active step's content, and Back / Next (Finish) navigation. Mirrors MudBlazor's `MudStepper` / `MudStep`. Advancing through the last step marks it complete and fires `OnCompleted`.

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
