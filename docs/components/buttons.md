---
title: Buttons & menus
---

# Buttons & menus

Loam's button family maps the MudBlazor button suite to Avalonia, using the shared `Variant`, `LoamColor`, and `LoamSize` enums from the `Loam` namespace to keep the API intentionally familiar. All controls are in `Loam.Controls` and driven entirely by their control themes.

---

## Button

Mirrors MudBlazor's `MudButton`. Subclasses Avalonia's `Button` and adds `Variant`, `Color`, `Size`, `FullWidth`, and optional leading/trailing icons via `StartIcon` and `EndIcon`.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Variant` | `Variant` | `Variant.Text` | Visual style: `Text`, `Filled`, or `Outlined`. |
| `Color` | `LoamColor` | `LoamColor.Default` | Semantic color role applied to the button surface. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Button size: `Small`, `Medium`, or `Large`. |
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
    StartIcon  = Icons.Material.Filled.Save,
    FullWidth  = false,
    Command    = ViewModel.SaveCommand,
};
```

---

## IconButton

Mirrors MudBlazor's `MudIconButton`. Inherits `Button` and renders a single centered glyph via the `Icon` property; the `Variant`, `Color`, and `Size` properties are inherited.

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

Mirrors MudBlazor's `MudToggleIconButton`. Inherits `IconButton`; clicking flips the two-way `Toggled` state and swaps the displayed glyph between `Icon` (off) and `ToggledIcon` (on).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Toggled` | `bool` | `false` | Whether the button is in the on state. Two-way bindable. |
| `ToggledIcon` | `string?` | `null` | Glyph shown when `Toggled` is `true`. Falls back to `Icon` if `null`. |
| `Icon` | `string?` | `null` | Inherited; glyph shown when not toggled. |
| `Color` | `LoamColor` | `LoamColor.Default` | Inherited semantic color. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Inherited size. |
| `Click` *(event)* | `EventHandler<RoutedEventArgs>` | — | Fires after the toggle flip, inherited from Avalonia's `Button`. |

```csharp
using Loam;
using Loam.Controls;

var bookmark = new ToggleIconButton
{
    Icon        = Icons.Material.Outlined.BookmarkBorder,
    ToggledIcon = Icons.Material.Filled.Bookmark,
    Color       = LoamColor.Primary,
};
bookmark.Bind(ToggleIconButton.ToggledProperty,
    new Avalonia.Data.Binding(nameof(ViewModel.IsBookmarked)) { Source = ViewModel });
```

---

## ButtonGroup

Mirrors MudBlazor's `MudButtonGroup`. Lays a collection of `Button` instances adjacently with merged borders and shared outer corners. When `OverrideChildStyles` is `true` (the default), the group's `Variant`, `Color`, and `Size` are pushed onto every child.

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

---

## Fab

Mirrors MudBlazor's `MudFab`. Inherits `Button` and renders as a pill-shaped, elevated, filled floating action button. An optional `Label` sets the button's text; `StartIcon` and `EndIcon` are inherited.

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

Mirrors MudBlazor's `MudMenu`. Inherits `Button` for its trigger appearance; clicking opens an Avalonia `Flyout` containing the `Items` list. Each row is represented by a `MenuItem` plain-object.

### Menu properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Items` | `ObservableCollection<MenuItem>` | `[]` | The dropdown rows. |
| `Variant` | `Variant` | `Variant.Text` | Inherited trigger button style. |
| `Color` | `LoamColor` | `LoamColor.Default` | Inherited trigger button color. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Inherited trigger button size. |

### MenuItem properties

`MenuItem` is a plain CLR class (not a `Control`); add instances directly to `Menu.Items`.

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string?` | `null` | Label displayed in the dropdown row. |
| `Icon` | `string?` | `null` | Optional leading icon path data for the row. |
| `OnClick` | `Action?` | `null` | Callback invoked when the row is selected. |

```csharp
using Loam;
using Loam.Controls;

var menu = new Menu
{
    Content = "Actions",
    Variant = Variant.Outlined,
    Color   = LoamColor.Primary,
};
menu.Items.Add(new MenuItem
{
    Text    = "Edit",
    Icon    = Icons.Material.Filled.Edit,
    OnClick = () => ViewModel.EditCommand.Execute(null),
});
menu.Items.Add(new MenuItem
{
    Text    = "Delete",
    Icon    = Icons.Material.Filled.Delete,
    OnClick = () => ViewModel.DeleteCommand.Execute(null),
});
```
