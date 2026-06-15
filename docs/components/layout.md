---
title: Surfaces & layout
---

# Surfaces & layout

This page documents Loam's surface and layout controls. Surface controls (`Paper`, `Card` family) host content on token-driven backgrounds. Layout controls (`Container`, `ResponsiveGrid`, `Stack`, `Spacer`, `Hidden`, `ScrollToTop`) handle spacing, responsive columns, and visibility. Shell controls (`Layout`, `AppBar`, `Drawer`, `MainContent`) compose the application frame.

All controls live in `Loam.Controls`. Enums such as `Breakpoint`, `LoamColor`, and `HiddenMode` live in the `Loam` namespace. The responsive grid is named `ResponsiveGrid` (renamed from `Grid` in v3) so it no longer collides with `Avalonia.Controls.Grid` — no alias needed. The old `Grid`/`Item` names remain as deprecated aliases; see the [v2 → v3 migration guide](../migration/v2-to-v3).

```csharp
using Loam;          // Breakpoint, LoamColor, HiddenMode, Icons
using Loam.Controls; // Paper, Card, Container, ResponsiveGrid, Layout, …
```

::: tip Mental model
Think in three layers, from the inside out. **Surfaces** (`Paper`, `Card`) give content a background, an
elevation, and a shape. **Layout** controls (`Container`, `ResponsiveGrid`/`Col`, `Stack`, `Spacer`,
`Hidden`) decide where surfaces sit and how they reflow. **Shell** controls (`Layout`, `AppBar`,
`Drawer`, `MainContent`) frame the whole window. You rarely reach below the layer you're working in —
compose downward, not sideways.
:::

Two cross-cutting ideas show up repeatedly below, so learn them once:

- **Breakpoints are container-relative.** `ResponsiveGrid` resolves a column's span from *its own*
  available width, not the window — so a grid nested in a narrow column behaves like a phone even on a
  desktop. The thresholds are `Sm 600`, `Md 960`, `Lg 1280`, `Xl 1920`, `Xxl 2560` (dip); anything
  below `Sm` is `Xs`. `Hidden` is the exception — it watches the host window width.
- **Generated anatomy vs custom content.** Surfaces and `Drawer` can either build a layout from typed
  properties (`Title`, `Subtitle`, `Body`, …) **or** host your own `Content`. Custom `Content` always
  wins; the precedence note under [Paper](#paper) spells this out.

## Choosing a surface or layout control

| Need | Reach for |
| --- | --- |
| A plain elevated background for any content | [`Paper`](#paper) |
| A content + media + actions block (the standard card anatomy) | [`Card`](#card) |
| Center page content and cap its width | [`Container`](#container) |
| Reflow blocks into responsive columns | [`ResponsiveGrid`](#responsivegrid) + [`Col`](#col) |
| Fixed 2D row/column placement | `Avalonia.Controls.Grid` (not a Loam control) |
| A simple spaced row or column of children | `StackPanel` (or the deprecated [`Stack`](#stack)) |
| Push siblings apart inside a `DockPanel` | [`Spacer`](#spacer) |
| Show/hide a region by screen size | [`Hidden`](#hidden) |
| A "back to top" affordance on a long scroll | [`ScrollToTop`](#scrolltotop) |
| The whole app frame (bar + drawer + content) | [`Layout`](#layout) + [`AppBar`](#appbar) + [`Drawer`](#drawer) + [`MainContent`](#maincontent) |

::: tip Loam grid vs Avalonia grid
`ResponsiveGrid` is for *breakpoint reflow* — twelve columns that wrap as width shrinks. For a fixed
table-like arrangement (a settings form, a labelled field row), use Avalonia's own `Grid` with explicit
`RowDefinitions`/`ColumnDefinitions`. They solve different problems; reach for the one that matches the
job. The [`ScrollToTop`](#scrolltotop) example below uses Avalonia's `Grid` for exactly this reason.
:::

---

## Paper

Equivalent of the reference API's `Paper`. A `ContentControl` that renders on a token-driven surface background with an elevation shadow. Optionally removes corner rounding (`Square`) or replaces the shadow with a 1 px outline (`Outlined`).

**Use it when** you need a neutral elevated background for arbitrary content and don't need the
title/media/actions anatomy a [`Card`](#card) adds.

> **Generated anatomy vs custom content.** `Paper` (and `Card`, `Drawer`) can either build a generated
> layout from typed properties (`Title`, `Subtitle`, `Body`, …) **or** host your own `Content`. The
> precedence is explicit: **custom `Content` always wins.** If you set both on one instance the
> generated properties are ignored, and a Debug build logs a warning. Pick one mode per instance.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Elevation` | `int` | `1` | Shadow depth (0–25). |
| `Square` | `bool` | `false` | Removes corner rounding. |
| `Outlined` | `bool` | `false` | Draws a 1 px border and suppresses the shadow. |

### Example

```csharp
using Loam.Controls;

var paper = new Paper
{
    Elevation = 2,
    Content = new TextBlock { Text = "Hello from Paper" },
};

// Outlined variant — no shadow, 1 px border
var outlined = new Paper
{
    Outlined = true,
    Square = true,
    Content = new TextBlock { Text = "Outlined, square" },
};
```

---

## Card

Equivalent of the reference API's `Card`. Inherits `Paper` and applies card-specific styling. Compose children using `CardHeader`, `CardMedia`, `CardContent`, and `CardActions`.

**Use it when** a block of content reads as a discrete unit — a list tile, a dashboard widget, a preview
with a header and actions. Use the sub-parts for layout, or set `Content` directly for a fully custom body.

### Properties

Inherits all `Paper` properties (`Elevation`, `Square`, `Outlined`). No additional properties.

### CardContent

Equivalent of `CardContent`. A `Decorator` that wraps the card body with default padding (`16 px` on all sides).

### CardActions

Equivalent of `CardActions`. A `Decorator` for hosting action buttons at the bottom of a card, with default padding (`8 px` on all sides).

### CardHeader

Equivalent of `CardHeader`. A `TemplatedControl` that lays out an optional leading avatar, a title/subtitle text stack, and a trailing action slot.

| Property | Type | Default | Description |
|---|---|---|---|
| `Avatar` | `object?` | `null` | Leading visual (typically an `Avatar` control). |
| `Title` | `string?` | `null` | Primary header text. |
| `Subtitle` | `string?` | `null` | Secondary header text. |
| `Action` | `object?` | `null` | Trailing visual (typically an icon button). |

### CardMedia

Equivalent of `CardMedia`. A `TemplatedControl` that shows an image stretched to fill a fixed-height band, with a neutral placeholder background when no image is set.

| Property | Type | Default | Description |
|---|---|---|---|
| `Source` | `Avalonia.Media.IImage?` | `null` | The image to display. |
| `MediaHeight` | `double` | `180` | Height of the media band in pixels. |

### Example

```csharp
using Avalonia.Media.Imaging;
using Loam.Controls;

var card = new Card
{
    Elevation = 2,
    Content = new StackPanel
    {
        Children =
        {
            new CardHeader
            {
                Title = "Avalonia Card",
                Subtitle = "Loam surface component",
            },
            new CardMedia
            {
                Source = new Bitmap("cover.png"),
                MediaHeight = 200,
            },
            new CardContent
            {
                Child = new TextBlock { Text = "Body copy goes here." },
            },
            new CardActions
            {
                Child = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new Button { Content = "Share" },
                        new Button { Content = "Learn more" },
                    },
                },
            },
        },
    },
};
```

::: details Composing by hand vs the generated card anatomy
The example above composes `CardHeader`/`CardMedia`/`CardContent`/`CardActions` explicitly into a
`StackPanel` and sets `Content` — full control, predictable order. `Card` *also* exposes a typed
shorthand for the same shape: set `Title`, `Subtitle`, `HeaderAvatar`, `HeaderAction`, `BodyText` (or
`Body`), `MediaSource`/`MediaHeight`/`ShowMedia`, and `PrimaryActionText`/`SecondaryActionText` (with
`ActionColor` and the `PrimaryActionClick`/`SecondaryActionClick` events), or add live controls to the
`Actions` collection. Leave `Content` unset to use that generated path. Remember the precedence rule:
set one or the other on a given card, never both.
:::

---

## Container

Equivalent of the reference API's `Container`. A `Decorator` that centers its child and caps its width at a responsive breakpoint. Optional `Gutters` add `16 px` horizontal padding on each side.

**Use it when** a page's content would otherwise stretch uncomfortably wide on large screens — wrap the
body of a [`MainContent`](#maincontent) in a `Container` capped at `Md` or `Lg` for a readable measure.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `MaxWidthBreakpoint` | `Breakpoint` | `Breakpoint.Lg` | The breakpoint whose max-width caps the content. |
| `Gutters` | `bool` | `true` | Adds 16 px horizontal padding inside the width cap. |

The cap maps to the breakpoint's lower bound — `Md` caps at `960 dip`, `Lg` at `1280 dip`, and so on.
`Breakpoint.None` (and `Always`) leave the width uncapped.

### Example

```csharp
using Loam;
using Loam.Controls;

var container = new Container
{
    MaxWidthBreakpoint = Breakpoint.Md,
    Gutters = true,
    Child = new TextBlock { Text = "Centered, width-capped content" },
};
```

---

## ResponsiveGrid

A `Panel` that arranges `Col` children (or arbitrary controls, treated as full-width) in a responsive 12-column grid. Column spans are resolved from the grid's own available width (container-query style), not the window width.

**Use it when** a set of blocks should sit side by side on wide screens and stack on narrow ones — a card
gallery, a dashboard, a two-up form.

> **Renamed in v3.** This control was called `Grid` in v2. It now has a distinct name so it no longer shadows `Avalonia.Controls.Grid` — use Avalonia's `Grid` for fixed 2D placement and `ResponsiveGrid` for breakpoint reflow. The old `Grid` name remains as a deprecated alias (diagnostic `LOAM0001`); see the [migration guide](../migration/v2-to-v3).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Spacing` | `double` | `8` | Gutter in pixels between columns and rows. |

### Col

A `Decorator` child of `ResponsiveGrid` that declares how many of the 12 columns it occupies at each breakpoint. Span resolution cascades down to the nearest smaller breakpoint that has a value set; defaults to `12` (full row) when nothing is set. (Renamed from `Item` in v3; the old name remains as a deprecated alias, diagnostic `LOAM0002`.)

| Property | Type | Default | Description |
|---|---|---|---|
| `Xs` | `int` | `0` | Columns at the extra-small breakpoint. |
| `Sm` | `int` | `0` | Columns at the small breakpoint. |
| `Md` | `int` | `0` | Columns at the medium breakpoint. |
| `Lg` | `int` | `0` | Columns at the large breakpoint. |
| `Xl` | `int` | `0` | Columns at the extra-large breakpoint. |
| `Xxl` | `int` | `0` | Columns at the extra-extra-large breakpoint. |

A value of `0` means "not set" — span cascades to the next smaller breakpoint that is set.

::: tip Set the smallest breakpoint that matters
Because spans cascade *upward* from the nearest smaller value that's set, you usually only set `Xs` (the
mobile baseline) plus the one or two breakpoints where the layout actually changes. `Xs = 12, Md = 6`
reads as "full width on phones, half width from medium up" — no need to also fill in `Sm`, `Lg`, `Xl`.
A span is clamped to `1–12`.
:::

### Example

```csharp
using Loam.Controls;

var grid = new ResponsiveGrid
{
    Spacing = 16,
    Children =
    {
        new Col { Xs = 12, Md = 6, Child = new TextBlock { Text = "Left half on md+" } },
        new Col { Xs = 12, Md = 6, Child = new TextBlock { Text = "Right half on md+" } },
        new Col { Xs = 12, Md = 4, Child = new TextBlock { Text = "Third A" } },
        new Col { Xs = 12, Md = 4, Child = new TextBlock { Text = "Third B" } },
        new Col { Xs = 12, Md = 4, Child = new TextBlock { Text = "Third C" } },
    },
};
```

---

## Stack

Equivalent of the reference API's `Stack`. Extends `StackPanel` with a `Row` toggle and a sensible default `Spacing` of `8 px`. Vertical by default; set `Row = true` for horizontal layout.

**Use it when** maintaining v2 code. For new code, prefer `StackPanel` directly — `Stack` is deprecated
(see below).

> **Deprecated in v3 (`LOAM0003`).** `Stack` is a thin wrapper over `Avalonia.Controls.StackPanel` and will be removed in a future release. Use `StackPanel` directly: set `Orientation = Orientation.Horizontal` for the old `Row = true`, and set `Spacing` (Loam's `Stack` defaulted it to `8`). See the [migration guide](../migration/v2-to-v3).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Row` | `bool` | `false` | Lays children out horizontally instead of vertically. |
| `Spacing` | `double` | `8` | Inherited from `StackPanel`. Gap between children in pixels. |

### Example

```csharp
using Loam.Controls;

var column = new Stack
{
    Children =
    {
        new TextBlock { Text = "Item 1" },
        new TextBlock { Text = "Item 2" },
    },
};

var row = new Stack
{
    Row = true,
    Spacing = 16,
    Children =
    {
        new Button { Content = "Cancel" },
        new Button { Content = "Save" },
    },
};
```

---

## Spacer

Equivalent of the reference API's `Spacer`. An empty `Control` with `HorizontalAlignment = Stretch` and `VerticalAlignment = Stretch`. Place it as the fill child of a `DockPanel` or a star-sized `Grid` cell to push surrounding siblings to the edges.

**Use it when** you want a flexible gap that pushes a left group and a right group apart — toolbars, app
bars, dialog footers.

### Properties

No configurable properties. Uses `HorizontalAlignment.Stretch` and `VerticalAlignment.Stretch` by default.

### Example

```csharp
using Avalonia.Controls;
using Loam.Controls;

// Typical app-bar toolbar: menu icon | title | [spacer] | action buttons
var toolbar = new DockPanel
{
    Children =
    {
        new Button { Content = "☰" },
        new TextBlock { Text = "My App", VerticalAlignment = VerticalAlignment.Center },
        new Spacer(),                             // pushes actions to the right
        new Button { Content = "Search" },
        new Button { Content = "Account" },
    },
};
```

---

## Hidden

Equivalent of the reference API's `Hidden`. A `Decorator` that monitors the host `TopLevel` (window) width and toggles `IsVisible` on its `Child` based on a breakpoint rule. The rule compares the current `Breakpoint` bucket to the configured `Breakpoint` value using the `HiddenMode` strategy.

**Use it when** a region should appear only on certain screen sizes — a desktop-only sidebar, a
mobile-only menu button.

::: warning Hidden watches the window, not the parent
Unlike [`ResponsiveGrid`](#responsivegrid), which measures its own available width, `Hidden` evaluates
its rule against the host **window** width (`TopLevel`). That makes it the right tool for global
"mobile vs desktop" decisions, but it will *not* respond to a narrow parent on a wide screen.
:::

### HiddenMode enum

Defined in the `Loam` namespace.

| Value | Description |
|---|---|
| `Down` | Hidden at the target breakpoint and every smaller one (e.g. "hide on mobile"). |
| `Up` | Hidden at the target breakpoint and every larger one. |
| `Only` | Hidden only at exactly the target breakpoint. |

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Breakpoint` | `Breakpoint` | `Breakpoint.Sm` | The reference breakpoint for the rule. |
| `Mode` | `HiddenMode` | `HiddenMode.Down` | How the current breakpoint is compared to `Breakpoint`. |

### Example

```csharp
using Loam;
using Loam.Controls;

// Visible only on md and above (hidden on xs and sm)
var desktopOnly = new Hidden
{
    Breakpoint = Breakpoint.Sm,
    Mode = HiddenMode.Down,
    Child = new TextBlock { Text = "Desktop sidebar" },
};

// Visible only on small screens
var mobileOnly = new Hidden
{
    Breakpoint = Breakpoint.Md,
    Mode = HiddenMode.Up,
    Child = new Button { Content = "Mobile menu" },
};
```

---

## ScrollToTop

Equivalent of the reference API's `ScrollToTop`. A `Decorator` that watches a `ScrollViewer` and shows its `Child` once the scroll position passes `VisibleOffset`. Clicking the control scrolls the target back to the top. The default `Child` is an up-arrow FAB.

**Use it when** a scroll region is long enough that getting back to the top is a chore — a feed, a long
document, a chat log.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Target` | `ScrollViewer?` | `null` | The scroll container to monitor. |
| `VisibleOffset` | `double` | `300` | Scroll distance in pixels after which the control becomes visible. |

### Example

```csharp
using Avalonia.Controls;
using Loam.Controls;

ScrollViewer scroll;
// Avalonia's Grid is the right tool for fixed 2D placement (rows/columns).
var page = new Avalonia.Controls.Grid
{
    RowDefinitions = new RowDefinitions("*,Auto"),
    Children =
    {
        (scroll = new ScrollViewer
        {
            Content = new ItemsControl { /* long list */ },
            [Avalonia.Controls.Grid.RowProperty] = 0,
        }),
        new ScrollToTop
        {
            Target = scroll,
            VisibleOffset = 400,
            [Avalonia.Controls.Grid.RowProperty] = 0,         // overlay inside the same cell
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 16, 16),
        },
    },
};
```

---

## Layout

Equivalent of the reference API's `Layout`. A `ContentControl` that forms the application shell. It docks an `AppBar` at the top (full width), a `Drawer` on the left below the bar, and fills the remaining space with its `Content` (typically a `MainContent`).

**Use it when** building the top-level window frame. It wires the bar, drawer, and content together so
docked drawers reserve space and temporary drawers overlay it.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `AppBar` | `object?` | `null` | The top app bar slot (typically an `AppBar`). |
| `Drawer` | `object?` | `null` | The side drawer slot (typically a `Drawer`). Docked drawers reserve space; temporary drawers overlay content. |
| `Content` | `object?` | `null` | The main content slot (inherited from `ContentControl`). |

::: tip Esc closes a temporary drawer
When the `Drawer` slot holds a `Drawer` in `Temporary` mode that is currently `Open`, pressing
<kbd>Esc</kbd> while the shell has focus closes it. `Docked` drawers ignore <kbd>Esc</kbd> — they're
part of the layout, not a transient overlay.
:::

---

## AppBar

Equivalent of the reference API's `AppBar`. A full-width, elevated, colored toolbar surface hosted in the `Layout.AppBar` slot. The default color follows the theme's app-bar palette; set `Color` to any `LoamColor` to override. Height is `64 px` normally and `48 px` when `Dense` is `true`.

**Use it when** the window needs a persistent top toolbar with a title, a navigation icon, and trailing
actions.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Color` | `LoamColor` | `LoamColor.Default` | App-bar background color. `Default` uses the theme's `AppbarBackground` palette token. |
| `Elevation` | `int` | `4` | Shadow depth. |
| `Dense` | `bool` | `false` | Reduces the bar height to 48 px. |
| `Title` / `Subtitle` | `string?` | `null` | Built-in title text shown in the default toolbar. |
| `NavigationIcon` | `string?` | `null` | Leading icon path data (raises `NavigationClick` / runs `NavigationAction`). |
| `Actions` | `IList<AppBarAction>` | empty | Trailing **icon-only** actions, each rendered as an `IconButton`. |
| `CustomActions` | `IList<Control>` | empty | Trailing slot for **arbitrary live controls** (toggles, search fields, stateful actions). Rendered before `Actions`. |

> Use `Actions` for simple icon buttons and `CustomActions` for anything else (a search `TextField`, a
> `ToggleIconButton` you flip, a menu). For a fully custom bar, set `Content` instead — it replaces the
> generated toolbar entirely.

```csharp
var bar = new AppBar
{
    Title = "Inbox",
    NavigationIcon = Icons.Material.Filled.Menu,
    CustomActions = { searchField, new ToggleIconButton { Icon = Icons.Material.Filled.DarkMode } },
    Actions = { new AppBarAction { Icon = Icons.Material.Filled.MoreHoriz, Label = "More" } },
};
```

An `AppBarAction` is an immutable record-like object — set `Icon`, `Label` (its accessible name),
`OnClick`, and optionally `Variant`/`Color`/`Size`/`IsEnabled`. Because it's not a live control, mutate
state through `CustomActions` instead when an action needs to change after render.

---

## Drawer

Equivalent of the reference API's `Drawer`. A left-anchored `ContentControl` that slides open or closed by animating its `Width`. Toggling `Open` switches between `DrawerWidth` and `0`; enabling `Mini` collapses to `MiniWidth` instead of hiding entirely.

**Use it when** the app needs side navigation. Use `Docked` for a persistent desktop rail and
`Temporary` for an overlay that slides in on smaller screens.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Open` | `bool` | `true` | Whether the drawer is expanded. |
| `Mini` | `bool` | `false` | While open, shows the drawer collapsed at `MiniWidth` instead of full `Width`. (Closing always hides it to zero width.) |
| `DrawerWidth` | `double` | `240` | Expanded width in pixels. |
| `MiniWidth` | `double` | `56` | Collapsed (mini) width in pixels. |
| `Mode` | `DrawerMode` | `DrawerMode.Docked` | `Docked` reserves layout space; `Temporary` overlays the main content. |
| `ShowScrim` | `bool` | `true` | Shows a scrim behind a temporary drawer while open. |
| `CloseOnScrimClick` | `bool` | `true` | Closes a temporary drawer when its scrim is clicked. |

The width animates over a short motion-token duration, so toggling `Open` or `Mini` glides rather than
snaps. For imperative control, `OpenDrawer()`, `CloseDrawer()`, and `Toggle()` set `Open` for you.

### DrawerMode

| Value | Description |
|---|---|
| `Docked` | Drawer participates in layout and shifts `MainContent` to the right. |
| `Temporary` | Drawer overlays `MainContent`; `Layout` shows the scrim between content and drawer. |

::: details Generated navigation vs custom content
Like the surfaces, `Drawer` has a generated path: add `DrawerItem` entries to the `Items` collection and
set `Title`/`Subtitle`/`FooterText` (or `Header`/`Footer`) to get a `NavMenu` of `NavLink`s with active
tracking via `SelectedIndex`. A `Temporary` drawer auto-closes after a generated item is chosen unless
`AutoCloseTemporary` is `false`. Setting `Content` directly opts out of all of that — custom content
wins, same precedence rule as `Paper`/`Card`.
:::

---

## MainContent

Equivalent of the reference API's `MainContent`. A `ContentControl` that provides the scrollable, padded main content region of a `Layout`. Place page content inside this control rather than directly in `Layout.Content`.

**Use it when** filling the `Layout.Content` slot — it gives the page region consistent padding and a
scroll viewer, and optionally a generated page header.

### Properties

No additional properties beyond the `ContentControl` base (`Content`, `ContentTemplate`, etc.).

> Beyond hosting `Content`, `MainContent` can render a generated page header: set `Title`/`Subtitle`,
> add header `Actions`, or supply `PrimaryActionText`/`SecondaryActionText` (with `ActionColor` and the
> `PrimaryActionClick`/`SecondaryActionClick` events). Supply a custom `Header` to replace that anatomy.

---

## App-shell example

A minimal shell using `Layout`, `AppBar`, `Drawer`, and `MainContent`:

```csharp
using Avalonia.Controls;
using Loam;
using Loam.Controls;

// Assume this is composed in a Window or root UserControl.
// drawerOpen is a bool field/property wired to your view model.
bool drawerOpen = true;

var drawer = new Drawer
{
    DrawerWidth = 240,
    [!Drawer.OpenProperty] = /* bind to vm */ null!, // replace with real binding
    Content = new StackPanel
    {
        Margin = new Thickness(8),
        Children =
        {
            new Button { Content = "Dashboard" },
            new Button { Content = "Settings" },
        },
    },
};

var temporaryDrawer = new Drawer
{
    Mode = DrawerMode.Temporary,
    DrawerWidth = 280,
    Content = new StackPanel
    {
        Children =
        {
            new Button { Content = "Inbox" },
            new Button { Content = "Archive" },
        },
    },
};

var appBar = new AppBar
{
    Color = LoamColor.Primary,
    Elevation = 4,
    Content = new DockPanel
    {
        Children =
        {
            new Button
            {
                Content = "☰",
                Command = ReactiveCommand.Create(() => drawer.Open = !drawer.Open),
            },
            new TextBlock
            {
                Text = "My Application",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0),
            },
            new Spacer(),
            new Button { Content = "Account" },
        },
    },
};

var shell = new Layout
{
    AppBar = appBar,
    Drawer = drawer,
    Content = new MainContent
    {
        Content = new TextBlock { Text = "Page content goes here." },
    },
};
```

> Replace the `drawer.Open = !drawer.Open` lambda with a `[RelayCommand]` on your view model when wiring via binding.

---

## Recipe: a responsive dashboard page

The layers working together: an `AppBar` with a navigation icon and live trailing actions, a docked
`Drawer` for navigation, and a `MainContent` whose body is a `Container`-capped `ResponsiveGrid` of
`Card` widgets that reflow from three-up to one-up as the window narrows. Everything is plain C#; the
glyphs are verified entries from `Icons.Material.Filled`.

```csharp
using Avalonia;
using Avalonia.Controls;
using Loam;
using Loam.Controls;

Card Widget(string title, string body) => new()
{
    Elevation = 1,
    Title = title,
    BodyText = body,
};

var drawer = new Drawer
{
    Mode = DrawerMode.Docked,
    Title = "Acme",
    Items =
    {
        new DrawerItem { Icon = Icons.Material.Filled.Dashboard, Text = "Overview", IsActive = true },
        new DrawerItem { Icon = Icons.Material.Filled.Notifications, Text = "Alerts" },
        new DrawerItem { Icon = Icons.Material.Filled.Settings, Text = "Settings" },
    },
};

var appBar = new AppBar
{
    Title = "Dashboard",
    NavigationIcon = Icons.Material.Filled.Menu,
    NavigationAction = () => drawer.Toggle(),
    CustomActions =
    {
        new ToggleIconButton { Icon = Icons.Material.Filled.DarkMode },
    },
    Actions =
    {
        new AppBarAction { Icon = Icons.Material.Filled.Search, Label = "Search" },
    },
};

// Cards reflow: three-up on md+, one-up on phones.
var grid = new ResponsiveGrid
{
    Spacing = 16,
    Children =
    {
        new Col { Xs = 12, Md = 4, Child = Widget("Revenue", "Up 12% week over week.") },
        new Col { Xs = 12, Md = 4, Child = Widget("Active users", "8,204 in the last 24h.") },
        new Col { Xs = 12, Md = 4, Child = Widget("Open tickets", "31 awaiting triage.") },
    },
};

var shell = new Layout
{
    AppBar = appBar,
    Drawer = drawer,
    Content = new MainContent
    {
        Content = new Container
        {
            MaxWidthBreakpoint = Breakpoint.Lg,
            Child = grid,
        },
    },
};
```

---

## Accessibility & keyboard

Loam's surface and layout controls are structural, so accessibility is mostly about *naming* and the
small amount of keyboard behavior the shell adds:

- **Automation names** — `Container`, `ResponsiveGrid`, `Col`, `Spacer`, `Hidden`, `ScrollToTop`,
  `Drawer`, `AppBar`, `MainContent`, and `Layout` each set a default automation name, so they surface
  sensibly to screen readers without extra work. Generated surface content derives its name from `Title`/
  `Subtitle`/`Body`.
- **`Layout` — Esc closes a temporary drawer** — when the `Drawer` slot holds a `Temporary`, `Open`
  drawer and the shell has focus, <kbd>Esc</kbd> closes it. A focused `Drawer` itself also closes on
  <kbd>Esc</kbd> under the same conditions. `Docked` drawers ignore the key.
- **`Drawer` — scrim dismissal** — a `Temporary` drawer shows a scrim while open (`ShowScrim`); clicking
  it closes the drawer when `CloseOnScrimClick` is `true`. Disabling a drawer dims it and blocks the
  <kbd>Esc</kbd> shortcut.
- **`AppBar` — name your icon actions** — the built-in navigation button takes its accessible name from
  `NavigationLabel` (default `"Navigation"`), and each `AppBarAction` from its `Label`. Always set
  `Label` on actions so the generated `IconButton`s announce their purpose. Anything in `CustomActions`
  is your own control — name it the way you would any [icon-only button](./buttons#accessibility-keyboard).
- **`ScrollToTop`** — the default child is a labelled up-arrow `Fab`; activating it (click or keyboard,
  since it's a button) scrolls the target home. It stays hidden until the target scrolls past
  `VisibleOffset`, so it isn't in the tab order while irrelevant.

::: tip Name custom surfaces
A bare `Paper` or `Card` holding a custom `Content` has no inherent label. If the surface represents a
distinct region (a settings panel, a stat tile), give it an accessible name so assistive tech can
announce it:

```csharp
using Avalonia.Automation;
using Loam.Controls;

var panel = new Paper { Elevation = 1, Content = settingsForm };
AutomationProperties.SetName(panel, "Display settings");
```
:::

## See also

- [Buttons & menus](./buttons) — the actions you place inside `AppBar`, `CardActions`, and toolbars.
- [Components overview → common parameters](./overview#common-parameters) — how `Color` and `Size` map across controls.
- [Navigation](./navigation) — `NavMenu` / `NavLink`, the controls a generated `Drawer` builds internally.
- [Theming](/guide/theming) — how `Elevation`, surface tokens, and `LoamColor` resolve.
- [v2 → v3 migration guide](../migration/v2-to-v3) — the `Grid → ResponsiveGrid`, `Item → Col`, and `Stack` deprecations.
