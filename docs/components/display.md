---
title: Display primitives
---

# Display primitives

This page covers the read-only and decorative display controls in Loam: `Text`, `Icon`, `Divider`, `Chip` / `ChipSet`, `Badge`, and `Avatar` / `AvatarGroup`. All controls live in `Loam.Controls`; enums (`LoamColor`, `Variant`, `LoamSize`, `Typo`, `Align`, `DividerType`, `BadgeOrigin`) live in `Loam`.

---

## Text

Typography-aware text label that mirrors the reference API's `Text`. Extends `TextBlock`; font size, weight, and family are driven by theme tokens via `Typo` and update automatically on theme changes. Set content with the inherited `Text` or `Inlines` properties.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Typo` | `Typo` | `Typo.Body1` | Typographic role (`H1`–`H6`, `Subtitle1/2`, `Body1/2`, `Button`, `Caption`, `Overline`, `Inherit`). |
| `Color` | `LoamColor` | `LoamColor.Default` | Semantic text color; `Default` and `Inherit` both resolve to the theme primary text token. |
| `GutterBottom` | `bool` | `false` | Adds 8 px bottom margin, matching the reference API's gutter spacing. |
| `Align` | `TextAlignment` | `TextAlignment.Left` | Horizontal alignment, forwarded to the underlying `TextBlock.TextAlignment`. |

### Example

```csharp
using Loam;
using Loam.Controls;

var heading = new Text
{
    Typo = Typo.H4,
    Color = LoamColor.Primary,
    GutterBottom = true,
    Text = "Order summary",
};

var body = new Text
{
    Typo = Typo.Body2,
    Align = TextAlignment.Center,
    Text = "Review the items below before confirming.",
};
```

---

## Icon

SVG vector icon that mirrors the reference API's `Icon`. Renders an SVG path string (e.g. from `Icons.Material.Filled.*`) scaled from its `ViewBox` coordinate space to a fixed pixel box determined by `Size`. Fill color is driven by theme tokens when `Color` is set; `LoamColor.Inherit` (the default) inherits the ambient `Foreground`, so icons inside a `Button` automatically adopt the button's text color.

### Icon path catalog

`Icons.Material.Filled` (namespace `Loam`) is a static class containing `const string` SVG path fields for the built-in icon catalog (24 × 24 view box). Use them directly as `Data`:

```csharp
Data = Icons.Material.Filled.Search
```

Available paths include `Home`, `Menu`, `Search`, `Close`, `Check`, `Add`, `Delete`, `Favorite`,
`FavoriteBorder`, `Star`, `Settings`, `DarkMode`, `LightMode`, `ArrowBack`, `ArrowForward`, `Edit`,
`Person`, `ExpandMore`, `ExpandLess`, `CalendarToday`, `Schedule`, `CloudUpload`, and other curated
catalog icons used by the component gallery.

The `ViewBox` property accepts any `"x y w h"` string, defaulting to `"0 0 24 24"`, so third-party or custom paths with a different coordinate space render correctly without manual scaling.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Data` | `string?` | `null` | SVG path data. Mirrors the reference API's `Icon` property (renamed to avoid the type/member name clash). |
| `Color` | `LoamColor` | `LoamColor.Inherit` | Semantic fill color. `Inherit` uses the ambient `Foreground`; `Default` resolves to the theme `ActionDefault` palette token. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Rendered pixel size: `ExtraSmall` = 18 px, `Small` = 20 px, `Medium` = 24 px, `Large` = 32 px, `ExtraLarge` = 40 px. |
| `ViewBox` | `string` | `"0 0 24 24"` | SVG coordinate space. Space- or comma-separated `"x y w h"`. |
| `Foreground` | `IBrush?` | inherited | Fill brush; inherited from the visual tree so icons adopt a parent control's text color automatically. |

### Example

```csharp
using Loam;
using Loam.Controls;

// Standard catalog icon
var searchIcon = new Icon
{
    Data = Icons.Material.Filled.Search,
    Size = LoamSize.Large,
    Color = LoamColor.Primary,
};

// Custom path with non-standard view box
var customIcon = new Icon
{
    Data = "M0 0 L48 48",
    ViewBox = "0 0 48 48",
    Size = LoamSize.Medium,
};
```

---

## Divider

A thin 1 px separator line that mirrors the reference API's `Divider`. Extends `Border`; color is bound to theme tokens automatically. Supports horizontal and vertical orientations and three inset styles via `DividerType`.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Vertical` | `bool` | `false` | Renders as a vertical 1 px line (full height, left-aligned) instead of horizontal. |
| `Light` | `bool` | `false` | Uses the lighter `DividerLight` palette token instead of the standard `Divider` token. |
| `DividerType` | `DividerType` | `DividerType.FullWidth` | `FullWidth` spans the full width/height; `Inset` adds a 16 px leading margin; `Middle` adds 16 px on both ends. |

### Example

```csharp
using Loam;
using Loam.Controls;

var separator = new Divider
{
    DividerType = DividerType.Middle,
    Light = true,
};

var verticalDivider = new Divider
{
    Vertical = true,
};
```

---

## Chip

A compact pill element representing an input, attribute, or action, mirroring the reference API's `Chip`. Displays an optional leading icon path, a text label, and an optional close button. Visual style (`Variant`) and semantic color drive the background, foreground, and border via theme tokens.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string?` | `null` | The chip label. |
| `Icon` | `string?` | `null` | Leading icon SVG path. Hidden when `null` or empty. |
| `CloseIcon` | `string?` | `null` | Close-button icon path. Defaults to `Icons.Material.Filled.Close` when `null`. |
| `Closeable` | `bool` | `false` | Shows the close button. Raises `Closed` when clicked. |
| `Label` | `bool` | `false` | Rounded-rectangle shape (corner radius 4) instead of the default pill shape. |
| `Variant` | `Variant` | `Variant.Filled` | `Filled`, `Outlined`, or `Text` visual style. |
| `Color` | `LoamColor` | `LoamColor.Default` | Semantic color for background/foreground/border tokens. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Height: `ExtraSmall` = 24 px, `Small` = 28 px, `Medium` = 32 px, `Large` = 40 px, `ExtraLarge` = 48 px. |

### Events

| Event | Description |
|---|---|
| `Closed` | Raised when the close button is pressed. |

### Example

```csharp
using Loam;
using Loam.Controls;

var chip = new Chip
{
    Text = "C#",
    Icon = Icons.Material.Filled.Star,
    Color = LoamColor.Primary,
    Variant = Variant.Outlined,
    Closeable = true,
};
chip.Closed += (_, _) => panel.Children.Remove(chip);
```

---

## ChipSet

A wrap-layout container of `Chip` items that mirrors the reference API's `ChipSet`. When `Selectable` is `true`, clicking a chip updates the two-way `SelectedIndex` and toggles chip variants automatically (selected = `Filled`, others = `Outlined`). Set `Mandatory` to prevent clearing the selection.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `ObservableCollection<Chip>` | — | The chips in the set. Mutations are observed and re-rendered automatically. |
| `Selectable` | `bool` | `false` | Enables click-to-select behavior. |
| `Mandatory` | `bool` | `false` | When `true`, clicking the already-selected chip does not clear the selection. |
| `MultiSelect` | `bool` | `false` | Allows more than one selected chip. |
| `SelectedIndex` | `int` | `-1` | Index of the selected chip, or -1 for none. Preserved for single-select mode and mirrors the first selected chip in multi-select mode. Two-way bindable. |
| `SelectedIndexes` | `ObservableCollection<int>` | empty | Selected chip indexes when `MultiSelect` is `true`. |

### Example

```csharp
using Loam;
using Loam.Controls;

var chipSet = new ChipSet
{
    Selectable = true,
    Mandatory = true,
    SelectedIndex = 0,
    Items =
    {
        new Chip { Text = "All" },
        new Chip { Text = "Active" },
        new Chip { Text = "Archived" },
    },
};

var filters = new ChipSet { Selectable = true, MultiSelect = true };
filters.Items.Add(new Chip { Text = "Open" });
filters.Items.Add(new Chip { Text = "Assigned" });
filters.Items.Add(new Chip { Text = "Overdue" });
filters.SelectedIndexes.Add(0);
filters.SelectedIndexes.Add(2);
```

---

## Badge

Overlays a small indicator (a count, text, or dot) on its wrapped `Content`, mirroring the reference API's `Badge`. Extends `ContentControl`; the wrapped element is set via `Content` in the usual Avalonia way. The badge pill is positioned at one of four corners and can optionally overlap the content.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `object?` | `null` | Badge value. Integers are capped at `Max` and formatted as `"{Max}+"`. |
| `Dot` | `bool` | `false` | Shows a 10 px dot instead of a value pill. |
| `Overlap` | `bool` | `false` | Pulls the badge inward over the content (25 % overlap) instead of outward (50 %). |
| `Bordered` | `bool` | `false` | Draws a 2 px surface-colored ring around the badge. |
| `Origin` | `BadgeOrigin` | `BadgeOrigin.TopRight` | Corner placement: `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`. |
| `Max` | `int` | `99` | Numeric cap; values above this are shown as `"{Max}+"`. |
| `Visible` | `bool` | `true` | Controls badge visibility without affecting the wrapped content. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Semantic color for the badge background and text. |

### Example

```csharp
using Loam;
using Loam.Controls;
using Avalonia.Controls;

var badge = new Badge
{
    Value = 12,
    Color = LoamColor.Error,
    Origin = BadgeOrigin.TopRight,
    Overlap = true,
    Content = new Icon
    {
        Data = Icons.Material.Filled.Favorite,
        Size = LoamSize.Large,
    },
};
```

---

## Avatar

A circular (or square/rounded) content holder for initials, icons, or images, mirroring the reference API's `Avatar`. Extends `ContentControl`. Background and foreground colors are bound to theme tokens based on `Variant` and `Color`. Size is controlled by `Size`, and corner shape by `Square` / `Rounded`.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Variant` | `Variant` | `Variant.Filled` | `Filled` (solid background), `Outlined` (border, no fill), or `Text` (transparent). |
| `Color` | `LoamColor` | `LoamColor.Default` | Semantic color for background, foreground, and border tokens. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Diameter: `ExtraSmall` = 24 px, `Small` = 32 px, `Medium` = 40 px, `Large` = 56 px, `ExtraLarge` = 72 px. |
| `Square` | `bool` | `false` | Square corners (corner radius 0). |
| `Rounded` | `bool` | `false` | Rounded-rectangle corners (20 % of size). Ignored when `Square` is `true`. |

### Example

```csharp
using Loam;
using Loam.Controls;

// Initials avatar
var avatar = new Avatar
{
    Color = LoamColor.Secondary,
    Size = LoamSize.Large,
    Content = new Text { Text = "JD", Typo = Typo.Body1 },
};

// Icon avatar with outlined style
var iconAvatar = new Avatar
{
    Variant = Variant.Outlined,
    Color = LoamColor.Primary,
    Content = new Icon { Data = Icons.Material.Filled.Person },
};
```

---

## AvatarGroup

A horizontal cluster of overlapping `Avatar` controls that mirrors the reference API's `AvatarGroup`. Shows up to `Max` avatars; any beyond the limit collapse into a trailing `"+N"` surplus avatar that inherits the size and shape of the first avatar in the collection.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `ObservableCollection<Avatar>` | — | The avatars in the cluster. Mutations are observed and re-rendered automatically. |
| `Max` | `int` | `4` | Maximum number of avatars displayed before collapsing to a `"+N"` overflow avatar. |
| `Spacing` | `double` | `-8` | Horizontal margin between avatars. Negative values produce the characteristic overlap effect. |

### Example

```csharp
using Loam;
using Loam.Controls;

var group = new AvatarGroup
{
    Max = 3,
    Spacing = -10,
    Items =
    {
        new Avatar { Color = LoamColor.Primary,   Content = new Text { Text = "A" } },
        new Avatar { Color = LoamColor.Secondary, Content = new Text { Text = "B" } },
        new Avatar { Color = LoamColor.Tertiary,  Content = new Text { Text = "C" } },
        new Avatar { Color = LoamColor.Info,      Content = new Text { Text = "D" } },
    },
};
```
