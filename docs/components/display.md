---
title: Display primitives
---

# Display primitives

This page covers the read-only and decorative display controls in Loam: `Text`, `Icon`, `Divider`,
`Chip` / `ChipSet`, `Badge`, and `Avatar` / `AvatarGroup`. These are the small building blocks that
carry information rather than collect it — a heading, a glyph, a separator, a status pill, a person's
initials. They have no commands and (mostly) no editable state; you compose them inside the layout and
data-display controls to give a screen its texture. All controls live in `Loam.Controls`; enums
(`LoamColor`, `Variant`, `LoamSize`, `Typo`, `Align`, `DividerType`, `BadgeOrigin`) live in `Loam`.

```csharp
using Loam;          // LoamColor, Variant, LoamSize, Typo, DividerType, BadgeOrigin, Icons
using Loam.Controls; // Text, Icon, Divider, Chip, ChipSet, Badge, Avatar, AvatarGroup
```

::: tip Mental model
These primitives share the same three styling knobs as the rest of Loam — **`Variant`**, **`Color`**
(`LoamColor`), and **`Size`** (`LoamSize`) — but each surfaces only the ones that make sense for it.
`Text` and `Icon` are pure presentation; `Chip`, `Badge`, and `Avatar` are presentation with a thin
shape of state (selection, a count, initials). When you need words, reach for `Text`; when a glyph is
enough, reach for `Icon`; everything else decorates content you already have.
:::

## Choosing a primitive

Several of these controls can look interchangeable at a glance. Pick by the role the element plays, not
by how it looks:

| Use | When | Reach for |
| --- | --- | --- |
| Styled text | Any label, heading, or paragraph that follows the type scale | [`Text`](#text) |
| A standalone glyph | An icon outside a button — in a list row, a header, next to text | [`Icon`](#icon) |
| A separator | Splitting sections of a list, menu, or layout | [`Divider`](#divider) |
| A removable / selectable tag | Filters, attributes, multi-select facets | [`Chip`](#chip) / [`ChipSet`](#chipset) |
| A count or status dot on something | Unread counts, "new" indicators on an icon or avatar | [`Badge`](#badge) |
| A person or entity stand-in | Initials, a profile glyph, an image holder | [`Avatar`](#avatar) / [`AvatarGroup`](#avatargroup) |

`Variant`, `Color`, and `Size` mean the same thing here as everywhere else — see
[Components overview → common parameters](./overview#common-parameters) and [Theming](/guide/theming)
for how they resolve to tokens.

---

## Text

Typography-aware text label that mirrors the reference API's `Text`. Extends `TextBlock`; font size, weight, and family are driven by theme tokens via `Typo` and update automatically on theme changes. Set content with the inherited `Text` or `Inlines` properties.

**Use it when** you need any on-screen words to follow the type scale — headings, body copy, captions,
labels. Pick the `Typo` role by meaning (`H4` for a section title, `Body2` for supporting copy), not by
the pixel size you happen to want.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Typo` | `Typo` | `Typo.Body1` | Typographic role (`H1`–`H6`, `Subtitle1/2`, `Body1/2`, `Button`, `Caption`, `Overline`, `Inherit`). |
| `Color` | `LoamColor` | `LoamColor.Default` | Semantic text color; `Default` and `Inherit` both resolve to the theme primary text token. |
| `GutterBottom` | `bool` | `false` | Adds 8 px bottom margin, matching the reference API's gutter spacing. |
| `Align` | `TextAlignment` | `TextAlignment.Left` | Horizontal alignment, forwarded to the underlying `TextBlock.TextAlignment`. |

::: tip Inherit when nesting
Set `Typo = Typo.Inherit` to stop driving font size/weight and let the text adopt whatever the
surrounding context provides, and `Color = LoamColor.Inherit` to pick up the ambient `Foreground`. This
is what lets a `Text` placed inside a colored surface or a button blend in without re-specifying tokens.
:::

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

**Use it when** a glyph stands on its own — in a list row, beside a heading, inside a custom layout.
For a *clickable* icon, use an [`IconButton`](./buttons#iconbutton) instead; `Icon` itself has no press
behavior. Inside Loam buttons, chips, and badges the glyph is already an `Icon` under the hood — you
just pass the path string.

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

::: warning Set a ViewBox for non-24 paths
The default catalog uses a `24 × 24` box. A path authored for a different grid (e.g. a `48 × 48`
export) will render at the wrong scale unless you set `ViewBox` to match its coordinate space. If an
icon looks cropped or tiny, check this first.
:::

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

**Use it when** you need to break a list, menu, or stack into visual groups. A horizontal divider
stretches to fill its container; a vertical one stretches to full height and left-aligns, so drop it
between items in a horizontal `StackPanel` to separate inline controls.

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

**Use it when** you have a small, discrete value the user reads, removes, or selects — a tag, a filter,
a selected facet. A single `Chip` handles the *removable* case via `Closeable` + `Closed`; for a *group*
where chips select against each other, put them in a [`ChipSet`](#chipset).

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

::: warning Closing is a request, not the deletion
`Closed` only fires the event — the chip stays on screen until you remove it. Handle `Closed` and remove
the chip (or the underlying item) yourself, as the example below does. `Closed` also fires when a
focused, `Closeable` chip is activated from the keyboard.
:::

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

**Use it when** several chips select against each other — a filter bar, a category picker. Single-select
by default; flip `MultiSelect` for facet-style filtering where several can be on at once. The set owns
each child's `Variant` while `Selectable` is on, so don't set chip variants yourself in that mode.

::: details Single- vs multi-select, and which property to read
In single-select mode read **`SelectedIndex`** (`-1` for none); it is two-way bindable. In multi-select
mode read **`SelectedIndexes`**, the collection of selected indexes — `SelectedIndex` still tracks the
*first* selected chip so a single binding keeps working. `Mandatory` keeps at least one chip selected:
clicking the last selected chip won't clear it.
:::

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

**Use it when** you want to annotate an existing element with a count or status — an unread count on an
inbox icon, a "new" dot on an avatar. The wrapped element goes in `Content`; the badge floats over it.
For a numeric count use `Value`; for a bare presence indicator use `Dot`.

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

::: tip Hide without reflow
Toggle `Visible` to show or hide the indicator while the wrapped content stays put — the badge collapses
but the icon underneath doesn't move. A `null`/empty `Value` with `Dot = false` also hides the pill, so
binding `Value` straight to a count naturally shows nothing at zero only if you clear it; use `Visible`
for explicit control.
:::

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

**Use it when** you need a compact stand-in for a person or entity — initials, a profile glyph, or an
image. Put whatever you like in `Content`: a `Text` of initials, an `Icon`, or an Avalonia `Image`. The
shape is circular by default; `Rounded` softens to a rounded square, `Square` squares it off.

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

**Use it when** you're showing a set of people on one row — collaborators on a document, members of a
team. Set `Max` to cap how many render before the rest fold into the `"+N"` overflow chip; tune `Spacing`
(negative overlaps) to control how tightly they stack.

::: tip Shape follows the first avatar
The `"+N"` surplus avatar copies the `Size`, `Square`, and `Rounded` of the **first** avatar in `Items`,
so style your first entry the way you want the whole group — including the overflow — to look.
:::

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

---

## Recipe: a contact list row

A common composition — an avatar, a stacked name/subtitle, a vertical divider, a selectable status chip,
and a badged notification icon — built entirely from the primitives above. Lay it out with a horizontal
`StackPanel` (see [Surfaces & layout](./layout)).

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

var row = new StackPanel
{
    Orientation = Orientation.Horizontal,
    Spacing = 12,
    VerticalAlignment = VerticalAlignment.Center,
    Children =
    {
        new Avatar
        {
            Color   = LoamColor.Primary,
            Size    = LoamSize.Medium,
            Content = new Text { Text = "JD", Typo = Typo.Body2 },
        },
        new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Text { Text = "Jane Doe", Typo = Typo.Subtitle1 },
                new Text { Text = "Product design", Typo = Typo.Caption, Color = LoamColor.Secondary },
            },
        },
        new Divider { Vertical = true },
        new ChipSet
        {
            Selectable = true,
            SelectedIndex = 0,
            Items =
            {
                new Chip { Text = "Online", Size = LoamSize.Small, Color = LoamColor.Success },
                new Chip { Text = "Away",   Size = LoamSize.Small, Color = LoamColor.Warning },
            },
        },
        new Badge
        {
            Value   = 3,
            Color   = LoamColor.Error,
            Overlap = true,
            Content = new Icon { Data = Icons.Material.Filled.Notifications, Size = LoamSize.Medium },
        },
    },
};
```

## Accessibility & keyboard

Most of these primitives are decorative and carry an automation name derived from their content, so
assistive technology can read them:

- **`Text`** sets its automation name to its `Text`, so labels and headings are announced as written.
- **`Icon`** exposes a generic `"Icon"` automation name. An icon is decorative on its own — when it
  conveys meaning, pair it with a `Text` label or set an explicit name on the parent (see below).
- **`Avatar`** derives its automation name from its `Content`, and **`AvatarGroup`** announces its item
  count, so "Jane Doe" initials or a "+3" overflow read sensibly.
- **`Badge`** announces its value (e.g. *"Badge 12"*) or *"Badge dot"*, layered over the wrapped content.
- **`Chip`** is focusable and takes part in the tab order. When `Closeable`, pressing <kbd>Space</kbd> or
  <kbd>Enter</kbd> on a focused chip raises `Closed`. Inside a **`ChipSet`** with `Selectable`, the same
  activation keys select the focused chip; <kbd>Tab</kbd> moves between chips.

::: tip Name a meaningful icon
A bare `Icon` reads only as *"Icon"*. When the glyph carries meaning on its own (a status indicator, a
standalone affordance), give its host an accessible name so screen readers announce its purpose:

```csharp
using Avalonia.Automation;

var status = new Icon { Data = Icons.Material.Filled.Check, Color = LoamColor.Success };
AutomationProperties.SetName(status, "Completed");
```
:::

## See also

- [Buttons & menus](./buttons) — `IconButton` for a clickable glyph; `StartIcon`/`Icon` use the same catalog.
- [Form inputs → ToggleGroup](./inputs#togglegroup-and-toggleitem) — for single-select segmented choices when chips aren't the right metaphor.
- [Surfaces & layout](./layout) — `StackPanel`, spacing, and the containers these primitives sit inside.
- [Data display](./data-display) — lists, tabs, and timelines that these primitives commonly decorate.
- [Theming](/guide/theming) — how `Variant`, `Color`, `Size`, and `Typo` resolve to tokens.
