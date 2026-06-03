---
title: Navigation
---

# Navigation

Loam provides five controls for in-app navigation, each mapping directly to its MudBlazor counterpart. All controls live in `Loam.Controls`; enums (`LoamColor`) live in `Loam`.

---

## Link

A clickable hyperlink that mirrors MudBlazor's `MudLink`. Extends `Text` (itself a `TextBlock`) and defaults its `Color` to `LoamColor.Primary`. The text underlines on pointer-over, or always when `Underline` is `true`. Clicking invokes `OnClick` and, if `Href` is set, opens the URL in the default browser via `TopLevel.Launcher`.

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

A horizontal breadcrumb trail that mirrors MudBlazor's `MudBreadcrumbs`. Renders `Items` separated by `Separator`; every entry except the last is rendered as a `Link`, while the last entry is shown as the current (non-interactive) page. Items are supplied via `ObservableCollection<BreadcrumbItem>` and the trail rebuilds automatically when the collection or separator changes.

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
| `Separator` | `string` | `"/"` | Text drawn between consecutive entries. Mirrors MudBlazor's `Separator`. |

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

A vertical container for `NavLink` and `NavGroup` entries that mirrors MudBlazor's `MudNavMenu`. Extends `StackPanel` directly; all standard Avalonia layout properties apply.

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

A clickable navigation row that mirrors MudBlazor's `MudNavLink`. Extends `ContentControl` and shows an optional leading `Icon` alongside a content label. When `IsActive` is `true`, the row background and text are tinted in `Color`; otherwise a subtle hover highlight is applied. Clicking invokes `OnClick` and, if set, opens `Href` in the default browser.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Content` *(inherited)* | `object?` | `null` | Label content (text string or any control). |
| `Icon` | `string?` | `null` | Leading icon path data. Hidden when `null` or empty. |
| `IsActive` | `bool` | `false` | Highlights the row as the current page. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Active accent color for background and text. Mirrors MudBlazor's active/icon color. |
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

A collapsible group of navigation entries that mirrors MudBlazor's `MudNavGroup`. Extends `TemplatedControl` and renders a clickable header row (with optional `Icon`, `Title`, and a chevron) that toggles `Expanded`. The nested `Items` (`ObservableCollection<Control>`) are rendered indented beneath the header and are only visible when `Expanded` is `true`. The chevron rotates 180° when the group is open. `Expanded` uses two-way binding by default.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | `null` | Header label text. Mirrors MudBlazor's `Title`. |
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
