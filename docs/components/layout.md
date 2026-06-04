---
title: Surfaces & layout
---

# Surfaces & layout

This page documents Loam's surface and layout controls. Surface controls (`Paper`, `Card` family) host content on token-driven backgrounds. Layout controls (`Container`, `Grid`, `Stack`, `Spacer`, `Hidden`, `ScrollToTop`) handle spacing, responsive columns, and visibility. Shell controls (`Layout`, `AppBar`, `Drawer`, `MainContent`) compose the application frame.

All controls live in `Loam.Controls`. Enums such as `Breakpoint`, `LoamColor`, and `HiddenMode` live in the `Loam` namespace. Because `Loam.Controls.Grid` shares a name with `Avalonia.Controls.Grid`, always qualify it with its full name in files that also reference Avalonia layout.

---

## Paper

Equivalent of the reference API's `Paper`. A `ContentControl` that renders on a token-driven surface background with an elevation shadow. Optionally removes corner rounding (`Square`) or replaces the shadow with a 1 px outline (`Outlined`).

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

---

## Container

Equivalent of the reference API's `Container`. A `Decorator` that centers its child and caps its width at a responsive breakpoint. Optional `Gutters` add `16 px` horizontal padding on each side.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `MaxWidthBreakpoint` | `Breakpoint` | `Breakpoint.Lg` | The breakpoint whose max-width caps the content. |
| `Gutters` | `bool` | `true` | Adds 16 px horizontal padding inside the width cap. |

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

## Grid

Equivalent of the reference API's `Grid`. A `Panel` that arranges `Item` children (or arbitrary controls, treated as full-width) in a responsive 12-column grid. Column spans are resolved from the grid's own available width (container-query style), not the window width.

> **Note:** Qualify as `Loam.Controls.Grid` when the file also uses `Avalonia.Controls.Grid`.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Spacing` | `double` | `8` | Gutter in pixels between columns and rows. |

### Item

Equivalent of the reference API's `Item`. A `Decorator` child of `Grid` that declares how many of the 12 columns it occupies at each breakpoint. Span resolution cascades down to the nearest smaller breakpoint that has a value set; defaults to `12` (full row) when nothing is set.

| Property | Type | Default | Description |
|---|---|---|---|
| `Xs` | `int` | `0` | Columns at the extra-small breakpoint. |
| `Sm` | `int` | `0` | Columns at the small breakpoint. |
| `Md` | `int` | `0` | Columns at the medium breakpoint. |
| `Lg` | `int` | `0` | Columns at the large breakpoint. |
| `Xl` | `int` | `0` | Columns at the extra-large breakpoint. |
| `Xxl` | `int` | `0` | Columns at the extra-extra-large breakpoint. |

A value of `0` means "not set" — span cascades to the next smaller breakpoint that is set.

### Example

```csharp
using Loam.Controls;

// Qualify to avoid ambiguity with Avalonia.Controls.Grid
var grid = new Loam.Controls.Grid
{
    Spacing = 16,
    Children =
    {
        new Item { Xs = 12, Md = 6, Child = new TextBlock { Text = "Left half on md+" } },
        new Item { Xs = 12, Md = 6, Child = new TextBlock { Text = "Right half on md+" } },
        new Item { Xs = 12, Md = 4, Child = new TextBlock { Text = "Third A" } },
        new Item { Xs = 12, Md = 4, Child = new TextBlock { Text = "Third B" } },
        new Item { Xs = 12, Md = 4, Child = new TextBlock { Text = "Third C" } },
    },
};
```

---

## Stack

Equivalent of the reference API's `Stack`. Extends `StackPanel` with a `Row` toggle and a sensible default `Spacing` of `8 px`. Vertical by default; set `Row = true` for horizontal layout.

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
var page = new Grid
{
    RowDefinitions = new RowDefinitions("*,Auto"),
    Children =
    {
        (scroll = new ScrollViewer
        {
            Content = new ItemsControl { /* long list */ },
            [Grid.RowProperty] = 0,
        }),
        new ScrollToTop
        {
            Target = scroll,
            VisibleOffset = 400,
            [Grid.RowProperty] = 0,         // overlay inside the same cell
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

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `AppBar` | `object?` | `null` | The top app bar slot (typically an `AppBar`). |
| `Drawer` | `object?` | `null` | The side drawer slot (typically a `Drawer`). Docked drawers reserve space; temporary drawers overlay content. |
| `Content` | `object?` | `null` | The main content slot (inherited from `ContentControl`). |

---

## AppBar

Equivalent of the reference API's `AppBar`. A full-width, elevated, colored toolbar surface hosted in the `Layout.AppBar` slot. The default color follows the theme's app-bar palette; set `Color` to any `LoamColor` to override. Height is `64 px` normally and `48 px` when `Dense` is `true`.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Color` | `LoamColor` | `LoamColor.Default` | App-bar background color. `Default` uses the theme's `AppbarBackground` palette token. |
| `Elevation` | `int` | `4` | Shadow depth. |
| `Dense` | `bool` | `false` | Reduces the bar height to 48 px. |

---

## Drawer

Equivalent of the reference API's `Drawer`. A left-anchored `ContentControl` that slides open or closed by animating its `Width`. Toggling `Open` switches between `DrawerWidth` and `0`; enabling `Mini` collapses to `MiniWidth` instead of hiding entirely.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Open` | `bool` | `true` | Whether the drawer is expanded. |
| `Mini` | `bool` | `false` | Collapses to `MiniWidth` when closed instead of hiding. |
| `DrawerWidth` | `double` | `240` | Expanded width in pixels. |
| `MiniWidth` | `double` | `56` | Collapsed (mini) width in pixels. |
| `Mode` | `DrawerMode` | `DrawerMode.Docked` | `Docked` reserves layout space; `Temporary` overlays the main content. |
| `ShowScrim` | `bool` | `true` | Shows a scrim behind a temporary drawer while open. |
| `CloseOnScrimClick` | `bool` | `true` | Closes a temporary drawer when its scrim is clicked. |

### DrawerMode

| Value | Description |
|---|---|
| `Docked` | Drawer participates in layout and shifts `MainContent` to the right. |
| `Temporary` | Drawer overlays `MainContent`; `Layout` shows the scrim between content and drawer. |

---

## MainContent

Equivalent of the reference API's `MainContent`. A `ContentControl` that provides the scrollable, padded main content region of a `Layout`. Place page content inside this control rather than directly in `Layout.Content`.

### Properties

No additional properties beyond the `ContentControl` base (`Content`, `ContentTemplate`, etc.).

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
