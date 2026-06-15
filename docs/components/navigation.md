---
title: Navigation
---

# Navigation

Navigation controls answer two different questions for the user: *"where am I?"* and *"where can I go?"*
Loam splits that work across seven controls, each mapping directly to its reference counterpart. A
[`Link`](#link) is an inline jump; [`Breadcrumbs`](#breadcrumbs) show the trail you took to get here; the
[`NavMenu`](#navmenu) / [`NavLink`](#navlink) / [`NavGroup`](#navgroup) family is the persistent side list
of an app; and the Material 3 [`NavigationRail`](#navigationrail) and [`BottomNavigation`](#bottomnavigation)
are compact, single-select destination switchers for the edges of a layout. All controls live in
`Loam.Controls`; the shared enums (`LoamColor`, `Typo`) and the `Icons` glyph set live in `Loam`.

```csharp
using Loam;          // LoamColor, Typo, Icons
using Loam.Controls; // Link, Breadcrumbs, NavMenu, NavLink, NavGroup, NavigationRail, …
```

::: tip Mental model
Pick by *scope*. For a jump inside running text, use a **`Link`**. For the full destination list down the
side of a desktop app, build a **`NavMenu`** of **`NavLink`** rows (group related rows with **`NavGroup`**).
For a compact medium-width shell, use a **`NavigationRail`**; for mobile width, a **`BottomNavigation`** bar.
Layer **`Breadcrumbs`** across the top to show the path. The rail and bar own their own single selection;
the `NavMenu` family does not — you set `IsActive` yourself.
:::

## Choosing a navigation control

| Use | When | Reach for |
| --- | --- | --- |
| Inline jump | A word or phrase in body text links elsewhere | [`Link`](#link) |
| Path / trail | Show the hierarchy that led to the current page | [`Breadcrumbs`](#breadcrumbs) |
| Full side list | A desktop app shell with many destinations | [`NavMenu`](#navmenu) + [`NavLink`](#navlink) |
| Collapsible section | A side list with nested, foldable groups | [`NavGroup`](#navgroup) |
| Compact side rail | 3–7 top-level destinations, medium width | [`NavigationRail`](#navigationrail) |
| Bottom bar | Top-level destinations on a mobile-width screen | [`BottomNavigation`](#bottomnavigation) |

`Color` and `Typo` mean the same thing here as everywhere else — see
[Components overview → common parameters](./overview#common-parameters) and [Theming](/guide/theming) for
how they resolve to tokens.

::: warning Active state is yours to manage in the `NavMenu` family
`NavLink.IsActive` is a plain styled property — Loam never sets it for you. When the user navigates, clear
the old active row and set the new one (or bind `IsActive` to your router/view-model). The
[`NavigationRail`](#navigationrail) and [`BottomNavigation`](#bottomnavigation) are the exception: they own a
`SelectedIndex` and set each item's `IsActive` themselves.
:::

---

## Link

A clickable hyperlink that mirrors the reference API's `Link`. Extends `Text` (itself a `TextBlock`) and defaults its `Color` to `LoamColor.Primary`. The text underlines on pointer-over, or always when `Underline` is `true`. Clicking invokes `OnClick` and, if `Href` is set, opens the URL in the default browser via `TopLevel.Launcher`.

**Use it when** a destination lives inside running prose ("see the [docs](#)"), or you need a lightweight
text-styled action. For a full-width tappable row in a side list, use [`NavLink`](#navlink) instead.

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

::: tip `Href` must be absolute
`Link` only launches `Href` when it parses as an *absolute* URI. A relative path is silently ignored — but
`OnClick` still fires, so use `OnClick` for in-app routing and `Href` for external URLs. The two can coexist:
`OnClick` runs first, then the URL launches.
:::

---

## Breadcrumbs

A horizontal breadcrumb trail that mirrors the reference API's `Breadcrumbs`. Renders `Items` separated by `Separator`; every entry except the last is rendered as a `Link`, while the last entry is shown as the current (non-interactive) page. Items are supplied via `ObservableCollection<BreadcrumbItem>` and the trail rebuilds automatically when the collection or separator changes.

**Use it when** the user is somewhere deep in a hierarchy and needs both orientation ("you are here") and a
one-click path back up. The trail is data-driven — mutate `Items` and it redraws.

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

::: details When is an entry a link vs. plain text?
The trail renders an entry as a clickable `Link` only when it is *not* the last item, *not* `Disabled`, and
has either an `OnClick` or an `Href`. The final entry is always drawn as the current page (full-opacity,
non-interactive); any earlier entry with no handler renders as muted plain text. So a middle item with
neither `OnClick` nor `Href` is shown but is not clickable.
:::

---

## NavMenu

A vertical container for `NavLink` and `NavGroup` entries that mirrors the reference API's `NavMenu`. Extends `StackPanel` directly; all standard Avalonia layout properties apply.

**Use it when** you want the persistent destination list of a desktop or wide-layout app, typically hosted
inside a [`Drawer`](./layout#drawer). It is a thin `StackPanel` subclass: a vertical orientation, a small
inter-row spacing, and an automation name — nothing more, so you stay in plain Avalonia layout territory.

### Properties

`NavMenu` exposes no additional properties beyond `StackPanel`. Add `NavLink` or `NavGroup` instances to its `Children`.

```csharp
using Loam;
using Loam.Controls;

var navMenu = new NavMenu
{
    Children =
    {
        new NavLink { Content = "Dashboard", Icon = Icons.Material.Filled.Dashboard, IsActive = true },
        new NavLink { Content = "Reports", Icon = Icons.Material.Filled.BarChart },
    },
};
```

---

## NavLink

A clickable navigation row that mirrors the reference API's `NavLink`. Extends `ContentControl` and shows an optional leading `Icon` alongside a content label. When `IsActive` is `true`, the row background and text are tinted in `Color`; otherwise a subtle hover highlight is applied. Clicking invokes `OnClick` and, if set, opens `Href` in the default browser.

**Use it when** you need a full-width, focusable destination row inside a [`NavMenu`](#navmenu). Set
`IsActive` on the current row yourself (or bind it) — see the warning at the top of the page.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Content` *(inherited)* | `object?` | `null` | Label content (text string or any control). |
| `Icon` | `string?` | `null` | Leading icon path data. Hidden when `null` or empty. |
| `IsActive` | `bool` | `false` | Highlights the row as the current page. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Active accent color for background and text. Mirrors the reference API's active/icon color. |
| `Label` | `string?` | `null` | Accessible name used by assistive tech when `Content` is not descriptive text (e.g. a custom control). Takes precedence over `Content` for the automation name. |
| `OnClick` | `Action?` | `null` | Callback invoked on left-button click when enabled. |
| `Href` | `string?` | `null` | URL opened on click. Must be an absolute URI. |

```csharp
using Loam;
using Loam.Controls;

var navLink = new NavLink
{
    Content = "Dashboard",
    Icon = Icons.Material.Filled.Home,
    IsActive = true,
    Color = LoamColor.Primary,
    OnClick = () => NavigateToDashboard(),
};
```

::: tip Set `Label` when `Content` is not text
`NavLink` derives its accessible name from `Label` first, then `Content`, then `Href`. When the row's
`Content` is a plain string you usually need nothing extra — but if you put a custom control (an icon-only
badge, say) in `Content`, set `Label` so screen readers still announce the destination.
:::

---

## NavGroup

A collapsible group of navigation entries that mirrors the reference API's `NavGroup`. Extends `TemplatedControl` and renders a clickable, focusable header row (with optional `Icon`, `Title`, and a chevron) that toggles `Expanded`. The nested `Items` (`ObservableCollection<Control>`) are rendered indented beneath the header and are only visible when `Expanded` is `true`. Enter or Space toggles the group, the chevron rotates 180° when open, and the reveal uses `Collapse`. `Expanded` uses two-way binding by default.

**Use it when** a side list has enough destinations that folding related ones under a labelled header keeps
it scannable ("Reports", "Admin"). Nest `NavLink` rows in `Items`; each is auto-indented under the header.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string?` | `null` | Header label text. Mirrors the reference API's `Title`. |
| `Icon` | `string?` | `null` | Leading icon path data in the header. Hidden when `null` or empty. |
| `Expanded` | `bool` | `false` | Whether the group is open. Two-way bindable. |
| `Items` | `ObservableCollection<Control>` | `(empty)` | Nested entries; typically `NavLink` instances. |

```csharp
using Loam;
using Loam.Controls;

var navGroup = new NavGroup
{
    Title = "Reports",
    Icon = Icons.Material.Filled.BarChart,
    Expanded = true,
    Items =
    {
        new NavLink { Content = "Monthly" },
        new NavLink { Content = "Annual" },
    },
};
```

---

## NavigationRail

A Material 3 **navigation rail** — a compact vertical strip of top-level destinations for the side of an app shell (best for 3–7 destinations and medium-width layouts; use `NavMenu` inside a `Drawer` for the full list). Each `NavigationRailItem` shows a centered icon in an active-indicator pill above a label; the rail manages single selection.

**Use it when** the app has a handful of top-level sections and you want a slim, always-visible switcher that
tracks its own selection — no manual `IsActive` bookkeeping.

### NavigationRail properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `IList<NavigationRailItem>` | `(empty)` | The destinations. |
| `SelectedIndex` | `int` | `0` | The selected destination index. Two-way bindable; `-1` selects nothing. |
| `SelectedItem` | `NavigationRailItem?` | — | Read-only; the item at `SelectedIndex`, or `null` when none is selected. |
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
| `Selected` | event | — | Raised when the item is activated by click or keyboard. |

The active item uses the secondary-container indicator pill with on-surface label; inactive items use on-surface-variant. Activation works by click and keyboard (Enter/Space).

```csharp
using Loam;
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

::: tip `Header` for a top action
The optional `Header` slot sits above the destinations — a natural home for a menu button or a `Fab` (see
[Buttons → Fab](./buttons#fab)). It does not participate in selection; it is just content you own.
:::

---

## BottomNavigation

A Material 3 **bottom navigation bar** — a horizontal strip of equal-width destinations for the bottom of a compact (mobile-width) layout. `BottomNavigationItem` shares the icon-over-label, active-indicator-pill anatomy of `NavigationRailItem`; the bar manages single selection.

**Use it when** you are targeting mobile or narrow widths and want the same top-level destinations the rail
would carry, anchored to the bottom of the screen.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `IList<BottomNavigationItem>` | `(empty)` | The destinations, laid out in equal-width cells. |
| `SelectedIndex` | `int` | `0` | The selected destination index. Two-way bindable; `-1` selects nothing. |
| `SelectedItem` | `BottomNavigationItem?` | — | Read-only; the item at `SelectedIndex`, or `null` when none is selected. |
| `SelectionChanged` | event | — | Raised when `SelectedIndex` changes. |

`BottomNavigationItem` derives from `NavigationRailItem`, so it has the same `Icon` / `Label` / `IsActive` / `Value` / `OnClick` members (and the `Selected` event).

```csharp
using Loam;
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

---

## Recipe: an app-shell side menu

A common shell — a flat destination, a collapsible group, and a footer link — built entirely from the
`NavMenu` family. The host tracks which row is active and clears the others on each navigation, since
`NavLink.IsActive` is not managed for you. Drop the resulting `NavMenu` inside a [`Drawer`](./layout#drawer)
for a full side panel.

```csharp
using Avalonia.Layout;
using Loam;
using Loam.Controls;

var dashboard = new NavLink { Content = "Dashboard", Icon = Icons.Material.Filled.Dashboard, IsActive = true };
var monthly   = new NavLink { Content = "Monthly" };
var annual    = new NavLink { Content = "Annual" };
var settings  = new NavLink { Content = "Settings", Icon = Icons.Material.Filled.Settings };

var rows = new[] { dashboard, monthly, annual, settings };

void Activate(NavLink target)
{
    foreach (var row in rows)
    {
        row.IsActive = ReferenceEquals(row, target);
    }
}

foreach (var row in rows)
{
    var captured = row;
    row.OnClick = () => Activate(captured);
}

var menu = new NavMenu
{
    Children =
    {
        dashboard,
        new NavGroup
        {
            Title = "Reports",
            Icon = Icons.Material.Filled.BarChart,
            Expanded = true,
            Items = { monthly, annual },
        },
        settings,
    },
};
```

## Accessibility & keyboard

Every interactive control here is focusable and keyboard-operable, and each carries an automation name so
assistive tech can announce it:

- **Focus** — `Link`, `NavLink`, `NavGroup`, `NavigationRailItem`, and `BottomNavigationItem` are all in the tab order (`Focusable = true`); <kbd>Tab</kbd> / <kbd>Shift</kbd>+<kbd>Tab</kbd> move between them and a focused control shows a focus highlight.
- **Activation** — <kbd>Enter</kbd> and <kbd>Space</kbd> activate `Link` and `NavLink` (firing `OnClick`, then launching `Href`), toggle a `NavGroup` open/closed, and select a rail or bottom-nav destination. Mouse activation is left-button only.
- **Disabled** — setting `IsEnabled = false` dims the control to the theme's disabled opacity and blocks both pointer and keyboard activation.
- **Names** — `Link` and `NavLink` derive their accessible name from their text (`NavLink` prefers `Label`, then `Content`, then `Href`); `Breadcrumbs`, `NavMenu`, `NavigationRail`, and `BottomNavigation` set descriptive container names ("Breadcrumbs", "Navigation menu", "Navigation rail", "Bottom navigation"); each rail/bar item is named from its `Label`.

::: tip Always give rail and bar items a `Label`
A `NavigationRailItem` or `BottomNavigationItem` takes its accessible name from `Label`. An icon-only
destination with no `Label` reads as nothing to a screen reader — set `Label` even if you are tempted to
show the icon alone.
:::

## See also

- [Buttons & menus](./buttons) — `Menu` for firing actions (vs. navigating), and `Fab` for a rail `Header`.
- [Surfaces & layout → Drawer](./layout#drawer) — the panel that typically hosts a `NavMenu`.
- [Display primitives → Icon](./display#icon) — the glyph renderer behind every `Icon` property here.
- [Components overview → common parameters](./overview#common-parameters) — how `Color` and `Typo` behave.
- [Theming](/guide/theming) — how `Color` and `Typo` resolve to tokens.
