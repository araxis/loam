---
title: Navigation
---

# Navigation

Loam provides five controls for in-app navigation, each mapping directly to its reference counterpart. All controls live in `Loam.Controls`; enums (`LoamColor`) live in `Loam`.

---

## Link

A clickable hyperlink that mirrors the reference API's `Link`. Extends `Text` (itself a `TextBlock`) and defaults its `Color` to `LoamColor.Primary`. The text underlines on pointer-over, or always when `Underline` is `true`. Clicking invokes `OnClick` and, if `Href` is set, opens the URL in the default browser via `TopLevel.Launcher`.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` *(inherited)* | `string?` | `null` | Display text (from `TextBlock`). |
| `Color` *(inherited)* | `LoamColor` | `LoamColor.Primary` | Semantic foreground color (from `Text`). |
| `Typo` *(inherited)* | `Typo` | `Typo.Body1` | Typographic scale (from `Text`). |
| `Underline` | `bool` | `false` | Always underline the text; otherwise underline only on hover. |
| `Href` | `string?` | `null` | URL opened on click. Must be an absolute URI. |
| `OnClick` | `Action?` | `null` | Callback invoked when the link is clicked (left button, enabled). |

```csharp
using Loam;
using Loam.Controls;

var link = new Link
{
    Text = "View details",
    Color = LoamColor.Secondary,
    Underline = true,
    Href = "https://example.com",
    OnClick = () => Console.WriteLine("clicked"),
};
```

---

## Breadcrumbs

A horizontal breadcrumb trail that mirrors the reference API's `Breadcrumbs`. Renders `Items` separated by `Separator`; every entry except the last is rendered as a `Link`, while the last entry is shown as the current (non-interactive) page. Items are supplied via `ObservableCollection<BreadcrumbItem>` and the trail rebuilds automatically when the collection or separator changes.

### BreadcrumbItem

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string?` | `null` | Display text for this entry. |
| `OnClick` | `Action?` | `null` | Callback invoked when a non-current entry is clicked. |
| `Href` | `string?` | `null` | URL opened on click. |
| `Disabled` | `bool` | `false` | Renders the entry as non-interactive text. |

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `ObservableCollection<BreadcrumbItem>` | `(empty)` | The breadcrumb entries, root first. |
| `Separator` | `string` | `"/"` | Text drawn between consecutive entries. Mirrors the reference API's `Separator`. |

```csharp
using Loam;
using Loam.Controls;

var breadcrumbs = new Breadcrumbs
{
    Separator = ">",
    Items =
    {
        new BreadcrumbItem("Home", onClick: () => GoHome()),
        new BreadcrumbItem("Settings", onClick: () => GoSettings()),
        new BreadcrumbItem("Profile"),   // last item — shown as current page
    },
};
```

---

## NavMenu

A vertical container for `NavLink` and `NavGroup` entries that mirrors the reference API's `NavMenu`. Extends `StackPanel` directly; all standard Avalonia layout properties apply.

### Properties

`NavMenu` exposes no additional properties beyond `StackPanel`. Add `NavLink` or `NavGroup` instances to its `Children`.

```csharp
using Loam.Controls;

var navMenu = new NavMenu
{
    Children =
    {
        new NavLink { Content = "Dashboard" },
        new NavLink { Content = "Reports", Icon = Icons.Chart },
    },
};
```

---

## NavLink

A clickable navigation row that mirrors the reference API's `NavLink`. Extends `ContentControl` and shows an optional leading `Icon` alongside a content label. When `IsActive` is `true`, the row background and text are tinted in `Color`; otherwise a subtle hover highlight is applied. Clicking invokes `OnClick` and, if set, opens `Href` in the default browser.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Content` *(inherited)* | `object?` | `null` | Label content (text string or any control). |
| `Icon` | `string?` | `null` | Leading icon path data. Hidden when `null` or empty. |
| `IsActive` | `bool` | `false` | Highlights the row as the current page. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Active accent color for background and text. Mirrors the reference API's active/icon color. |
| `OnClick` | `Action?` | `null` | Callback invoked on left-button click when enabled. |
| `Href` | `string?` | `null` | URL opened on click. Must be an absolute URI. |

```csharp
using Loam;
using Loam.Controls;

var navLink = new NavLink
{
    Content = "Dashboard",
    Icon = Icons.Home,
    IsActive = true,
    Color = LoamColor.Primary,
    OnClick = () => NavigateToDashboard(),
};
```

---

## NavGroup

A collapsible group of navigation entries that mirrors the reference API's `NavGroup`. Extends `TemplatedControl` and renders a clickable, focusable header row (with optional `Icon`, `Title`, and a chevron) that toggles `Expanded`. The nested `Items` (`ObservableCollection<Control>`) are rendered indented beneath the header and are only visible when `Expanded` is `true`. Enter or Space toggles the group, the chevron rotates 180° when open, and the reveal uses `Collapse`. `Expanded` uses two-way binding by default.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | `null` | Header label text. Mirrors the reference API's `Title`. |
| `Icon` | `string?` | `null` | Leading icon path data in the header. Hidden when `null` or empty. |
| `Expanded` | `bool` | `false` | Whether the group is open. Two-way bindable. |
| `Items` | `ObservableCollection<Control>` | `(empty)` | Nested entries; typically `NavLink` instances. |

```csharp
using Loam.Controls;

var navGroup = new NavGroup
{
    Title = "Reports",
    Icon = Icons.FolderOpen,
    Expanded = true,
    Items =
    {
        new NavLink { Content = "Monthly" },
        new NavLink { Content = "Annual" },
    },
};
```

## NavigationRail

A Material 3 **navigation rail** — a compact vertical strip of top-level destinations for the side of an app shell (best for 3–7 destinations and medium-width layouts; use `NavMenu` inside a `Drawer` for the full list). Each `NavigationRailItem` shows a centered icon in an active-indicator pill above a label; the rail manages single selection.

### NavigationRail properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `IList<NavigationRailItem>` | `(empty)` | The destinations. |
| `SelectedIndex` | `int` | `0` | The selected destination index. Two-way bindable; `-1` selects nothing. |
| `Header` | `object?` | `null` | Optional content above the destinations (e.g. a menu button or `Fab`). |
| `SelectionChanged` | event | — | Raised when `SelectedIndex` changes. |

### NavigationRailItem properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Icon` | `string?` | `null` | Destination icon path data. |
| `Label` | `string?` | `null` | Label shown under the icon. |
| `IsActive` | `bool` | `false` | Whether this is the selected destination (set by the rail). |
| `Value` | `object?` | `null` | Opaque value carried by the destination. |
| `OnClick` | `Action?` | `null` | Invoked when the destination is activated. |

The active item uses the secondary-container indicator pill with on-surface label; inactive items use on-surface-variant. Activation works by click and keyboard (Enter/Space).

```csharp
using Loam.Controls;

var rail = new NavigationRail
{
    SelectedIndex = 0,
    Items =
    {
        new NavigationRailItem { Icon = Icons.Material.Filled.Home, Label = "Home" },
        new NavigationRailItem { Icon = Icons.Material.Filled.Dashboard, Label = "Dashboard" },
        new NavigationRailItem { Icon = Icons.Material.Filled.Settings, Label = "Settings" },
    },
};
rail.SelectionChanged += (_, _) => Navigate(rail.SelectedIndex);
```

## BottomNavigation

A Material 3 **bottom navigation bar** — a horizontal strip of equal-width destinations for the bottom of a compact (mobile-width) layout. `BottomNavigationItem` shares the icon-over-label, active-indicator-pill anatomy of `NavigationRailItem`; the bar manages single selection.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `IList<BottomNavigationItem>` | `(empty)` | The destinations, laid out in equal-width cells. |
| `SelectedIndex` | `int` | `0` | The selected destination index. Two-way bindable; `-1` selects nothing. |
| `SelectionChanged` | event | — | Raised when `SelectedIndex` changes. |

`BottomNavigationItem` has the same `Icon` / `Label` / `IsActive` / `Value` / `OnClick` members as `NavigationRailItem`.

```csharp
using Loam.Controls;

var bar = new BottomNavigation
{
    SelectedIndex = 0,
    Items =
    {
        new BottomNavigationItem { Icon = Icons.Material.Filled.Home, Label = "Home" },
        new BottomNavigationItem { Icon = Icons.Material.Filled.Search, Label = "Search" },
        new BottomNavigationItem { Icon = Icons.Material.Filled.Settings, Label = "Settings" },
    },
};
bar.SelectionChanged += (_, _) => Navigate(bar.SelectedIndex);
```
