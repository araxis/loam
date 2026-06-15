---
title: Buttons & menus
---

# Buttons & menus

Buttons are how users commit to an action. Loam's button family maps a familiar button suite onto
Avalonia and drives every variant through control themes, so a button looks and behaves consistently
wherever you place it. The whole family shares three knobs — **`Variant`**, **`Color`** (`LoamColor`),
and **`Size`** (`LoamSize`) — from the `Loam` namespace, so once you've learned one button you've
learned them all. All controls live in `Loam.Controls`.

```csharp
using Loam;          // Variant, LoamColor, LoamSize, Icons
using Loam.Controls; // Button, IconButton, Fab, Menu, …
```

::: tip Mental model
Reach for a **`Button`** for anything with a text label, an **`IconButton`** when an icon alone is
unambiguous, and a **`Fab`** for the one screen-level primary action. Group related buttons with
**`ButtonGroup`**; collapse a list of actions behind a **`Menu`**. Everything else is styling via the
three shared knobs.
:::

## Choosing a button

| Use | When | Reach for |
| --- | --- | --- |
| Labelled action | The action needs words to be clear ("Save", "Cancel") | [`Button`](#button) |
| Icon-only action | The glyph is unambiguous and space is tight (toolbars, list rows) | [`IconButton`](#iconbutton) |
| On/off action | A single control flips between two states (favorite, mute) | [`ToggleIconButton`](#toggleiconbutton) |
| Segmented choice | A small set of mutually related actions sit together | [`ButtonGroup`](#buttongroup) |
| Screen primary action | One prominent, floating "create/add" affordance per view | [`Fab`](#fab) |
| Overflow / list of actions | More actions than fit, or contextual commands | [`Menu`](#menu) |

`Variant`, `Color`, and `Size` mean the same thing across all of them — see
[Components overview → common parameters](./overview#common-parameters) and [Theming](/guide/theming)
for how they map to tokens.

---

## Button

Mirrors the reference API's `Button`. Subclasses Avalonia's `Button` and adds `Variant`, `Color`, `Size`, `FullWidth`, and optional leading/trailing icons via `StartIcon` and `EndIcon`.
Button templates include press ripple feedback automatically.

**Use it when** an action needs a text label. Pick the variant by emphasis: `Filled` for the primary
action in a group, `Outlined` for secondary actions, `Text` for low-emphasis or inline actions.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Variant` | `Variant` | `Variant.Text` | Visual style: `Text`, `Filled`, or `Outlined`. |
| `Color` | `LoamColor` | `LoamColor.Default` | Semantic color role applied to the button surface. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Button size: `ExtraSmall`, `Small`, `Medium`, `Large`, or `ExtraLarge`. |
| `FullWidth` | `bool` | `false` | Stretches the button to fill the available width. |
| `StartIcon` | `string?` | `null` | SVG path data for a leading icon. |
| `EndIcon` | `string?` | `null` | SVG path data for a trailing icon. |
| `Click` *(event)* | `EventHandler<RoutedEventArgs>` | — | Inherited from Avalonia's `Button`; fires on activation. |
| `Command` *(property)* | `ICommand?` | `null` | Inherited; executed on click. |

```csharp
using Loam;
using Loam.Controls;

var saveButton = new Button
{
    Content    = "Save",
    Variant    = Variant.Filled,
    Color      = LoamColor.Primary,
    Size       = LoamSize.Medium,
    StartIcon  = Icons.Material.Filled.Check,
    FullWidth  = false,
    Command    = ViewModel.SaveCommand,
};
```

::: tip Emphasis, not decoration
On any given surface, aim for a single `Filled` button (the primary action). Reaching for two or three
filled buttons side by side flattens the hierarchy — make the rest `Outlined` or `Text`.
:::

---

## IconButton

Mirrors the reference API's `IconButton`. Inherits `Button` and renders a single centered glyph via the `Icon` property; the `Variant`, `Color`, and `Size` properties are inherited.
Icon button templates include the same press ripple host as regular buttons.

**Use it when** the glyph alone is unmistakable (close, delete, edit) and horizontal space is scarce —
toolbars, list-row trailing actions, app bars. Always set an accessible name (see
[Accessibility](#accessibility-keyboard)).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Icon` | `string?` | `null` | SVG path data for the button glyph. |
| `Variant` | `Variant` | `Variant.Text` | Inherited visual style. |
| `Color` | `LoamColor` | `LoamColor.Default` | Inherited semantic color. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Inherited size. |
| `Click` *(event)* | `EventHandler<RoutedEventArgs>` | — | Inherited from Avalonia's `Button`. |

```csharp
using Loam;
using Loam.Controls;

var deleteButton = new IconButton
{
    Icon    = Icons.Material.Filled.Delete,
    Color   = LoamColor.Error,
    Size    = LoamSize.Small,
    Variant = Variant.Outlined,
    Command = ViewModel.DeleteCommand,
};
```

---

## ToggleIconButton

Mirrors the reference API's `ToggleIconButton`. Inherits `IconButton`; clicking flips the two-way `Toggled` state and swaps the displayed glyph between `Icon` (off) and `ToggledIcon` (on).

**Use it when** one control represents a binary state the user flips in place — favorite/unfavorite,
mute/unmute, pin/unpin. Bind `Toggled` to your view model; the glyph and color follow the state.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Toggled` | `bool` | `false` | Whether the button is in the on state. Two-way bindable. |
| `ToggledIcon` | `string?` | `null` | Glyph shown when `Toggled` is `true`. Falls back to `Icon` if `null`. |
| `ToggledColor` | `LoamColor?` | `null` | Optional glyph color while `Toggled` is `true`; unset keeps the inherited icon color. |
| `Icon` | `string?` | `null` | Inherited; glyph shown when not toggled. |
| `Color` | `LoamColor` | `LoamColor.Default` | Inherited semantic color. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Inherited size. |
| `Click` *(event)* | `EventHandler<RoutedEventArgs>` | — | Fires after the toggle flip, inherited from Avalonia's `Button`. |

```csharp
using Loam;
using Loam.Controls;

var bookmark = new ToggleIconButton
{
    Icon        = Icons.Material.Filled.FavoriteBorder,
    ToggledIcon = Icons.Material.Filled.Favorite,
    Color       = LoamColor.Primary,
    ToggledColor = LoamColor.Success,
};
bookmark.Bind(ToggleIconButton.ToggledProperty,
    new Avalonia.Data.Binding(nameof(ViewModel.IsBookmarked)) { Source = ViewModel });
```

---

## ButtonGroup

Mirrors the reference API's `ButtonGroup`. Lays a collection of `Button` instances adjacently with merged borders and shared outer corners. When `OverrideChildStyles` is `true` (the default), the group's `Variant`, `Color`, and `Size` are pushed onto every child.

**Use it when** a few related actions belong together as one unit — a segmented "Day / Week / Month"
switch, or a split of related commands. For *mutually exclusive selection* (only one active at a time),
prefer [`ToggleGroup`](./inputs#togglegroup), which tracks a selected value.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `ObservableCollection<Button>` | `[]` | The grouped buttons. |
| `Variant` | `Variant` | `Variant.Outlined` | Shared visual style forwarded to children when `OverrideChildStyles` is `true`. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Shared semantic color forwarded to children. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Shared size forwarded to children. |
| `Vertical` | `bool` | `false` | Stacks buttons vertically instead of horizontally. |
| `OverrideChildStyles` | `bool` | `true` | Pushes `Variant`/`Color`/`Size` onto each child and manages corner radii. |

```csharp
using Loam;
using Loam.Controls;

var group = new ButtonGroup
{
    Variant = Variant.Outlined,
    Color   = LoamColor.Primary,
    Size    = LoamSize.Medium,
};
group.Items.Add(new Button { Content = "Day" });
group.Items.Add(new Button { Content = "Week" });
group.Items.Add(new Button { Content = "Month" });
```

::: warning Per-child styling
While `OverrideChildStyles` is `true` (the default) the group owns each child's `Variant`/`Color`/`Size`
and corner radii — setting those on an individual child has no effect. Set `OverrideChildStyles = false`
when you need a child to differ.
:::

---

## Fab

Mirrors the reference API's `Fab`. Inherits `Button` and renders as a pill-shaped, elevated, filled floating action button. An optional `Label` sets the button's text; `StartIcon` and `EndIcon` are inherited.

**Use it when** a view has one dominant action ("Add", "Compose"). Keep it to a single FAB per screen —
its elevation and fill are meant to stand out, and a second one cancels that out.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Label` | `string?` | `null` | Text label displayed inside the FAB. Setting this updates `Content`. |
| `StartIcon` | `string?` | `null` | Inherited leading icon path data. |
| `EndIcon` | `string?` | `null` | Inherited trailing icon path data. |
| `Color` | `LoamColor` | `LoamColor.Default` | Inherited semantic color. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Inherited size. |
| `Click` *(event)* | `EventHandler<RoutedEventArgs>` | — | Inherited from Avalonia's `Button`. |

```csharp
using Loam;
using Loam.Controls;

var fab = new Fab
{
    Label     = "Add item",
    StartIcon = Icons.Material.Filled.Add,
    Color     = LoamColor.Primary,
    Command   = ViewModel.AddCommand,
};
```

---

## Menu

Mirrors the reference API's `Menu`. Inherits `Button` for its trigger appearance; clicking opens an Avalonia `Flyout` containing the `Items` list. Each row is represented by a `MenuItem` plain-object. Disabled menu triggers do not open, disabled rows are skipped by keyboard navigation, Escape closes the popup, and Up/Down move through enabled rows.

**Use it when** there are more actions than fit comfortably, or the actions are contextual/secondary.
For navigation between views, use [`NavMenu`](./navigation#navmenu) instead.

### Menu properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `ObservableCollection<MenuItem>` | `[]` | The dropdown rows. |
| `Variant` | `Variant` | `Variant.Text` | Inherited trigger button style. |
| `Color` | `LoamColor` | `LoamColor.Default` | Inherited trigger button color. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Inherited trigger button size. |
| `MenuWidth` | `double` | `180` | Minimum popup surface width. |
| `CloseOnItemClick` | `bool` | `true` | Whether choosing an enabled row closes the popup. |
| `OpenMenu()` / `CloseMenu()` | methods | — | Public imperative open/close hooks. |

### MenuItem properties

`MenuItem` is a plain CLR class (not a `Control`); add instances directly to `Menu.Items`.

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string?` | `null` | Label displayed in the dropdown row. |
| `Icon` | `string?` | `null` | Optional leading icon path data for the row. |
| `OnClick` | `Action?` | `null` | Callback invoked when the row is selected. |
| `ShortcutText` | `string?` | `null` | Optional trailing shortcut hint. |
| `IsDivider` | `bool` | `false` | Renders this entry as a divider row. |
| `IsEnabled` | `bool` | `true` | Disabled rows are visible but not focusable or activatable. |

```csharp
using Loam;
using Loam.Controls;

var menu = new Menu
{
    Content = "Actions",
    Variant = Variant.Outlined,
    Color   = LoamColor.Primary,
    MenuWidth = 220,
};
menu.Items.Add(new MenuItem
{
    Text    = "Edit",
    Icon    = Icons.Material.Filled.Edit,
    ShortcutText = "E",
    OnClick = () => ViewModel.EditCommand.Execute(null),
});
menu.Items.Add(new MenuItem { IsDivider = true });
menu.Items.Add(new MenuItem
{
    Text    = "Delete",
    Icon    = Icons.Material.Filled.Delete,
    IsEnabled = ViewModel.CanDelete,
    OnClick = () => ViewModel.DeleteCommand.Execute(null),
});
```

---

## Recipe: a row toolbar

A common layout — a labelled primary action, a grouped set, and an overflow menu — composed from the
family above. Everything is plain C#; lay the pieces out with a `StackPanel` (see
[Surfaces & layout](./layout)).

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

var toolbar = new StackPanel
{
    Orientation = Orientation.Horizontal,
    Spacing = 8,
    Children =
    {
        new Button
        {
            Content   = "New",
            Variant   = Variant.Filled,
            Color     = LoamColor.Primary,
            StartIcon = Icons.Material.Filled.Add,
            Command   = ViewModel.NewCommand,
        },
        new ButtonGroup
        {
            Variant = Variant.Outlined,
            Color   = LoamColor.Default,
            Items =
            {
                new Button { Content = "Day" },
                new Button { Content = "Week" },
                new Button { Content = "Month" },
            },
        },
        new IconButton
        {
            Icon    = Icons.Material.Filled.ContentCopy,
            Variant = Variant.Text,
            Command = ViewModel.CopyCommand,
        },
    },
};
```

## Accessibility & keyboard

Every button in this family subclasses Avalonia's `Button`, so it is keyboard-operable out of the box:

- **Focus** — buttons are in the tab order and show a focus adorner; <kbd>Tab</kbd> / <kbd>Shift</kbd>+<kbd>Tab</kbd> move between them.
- **Activation** — <kbd>Space</kbd> and <kbd>Enter</kbd> invoke `Click` / `Command`. `ToggleIconButton` flips `Toggled` on activation.
- **Disabled** — setting `IsEnabled = false` removes the control from the tab order and blocks activation.
- **`Menu`** — the trigger opens on activation; inside the popup <kbd>↑</kbd>/<kbd>↓</kbd> move through enabled rows, disabled rows are skipped, and <kbd>Esc</kbd> closes it.

::: tip Name your icon-only buttons
An `IconButton` or `ToggleIconButton` has no text for assistive technology to read. Give it an accessible
name so screen readers announce its purpose:

```csharp
using Avalonia.Automation;

var delete = new IconButton { Icon = Icons.Material.Filled.Delete, Color = LoamColor.Error };
AutomationProperties.SetName(delete, "Delete");
```
:::

## See also

- [Display primitives](./display) — `Icon`, `Chip`, and the glyph set behind `StartIcon`/`Icon`.
- [Form inputs → ToggleGroup](./inputs#togglegroup) — for single-select segmented choices.
- [Navigation → NavMenu](./navigation#navmenu) — for navigating between views rather than firing actions.
- [Theming](/guide/theming) — how `Variant`, `Color`, and `Size` resolve to tokens.
