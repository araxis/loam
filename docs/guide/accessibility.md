---
title: Accessibility
---

# Accessibility

Loam controls are keyboard-operable and screen-reader-aware out of the box, because almost every one of
them either subclasses an Avalonia input control (so it inherits focus and key handling) or wires its own
key handlers and `AutomationProperties` in code. This page collects the conventions that hold *across* the
library — how focus moves, which keys do what in each control, how disabled state is expressed, and the one
thing you still have to do yourself: name controls that show only an icon.

There is no XAML and no separate accessibility layer to configure. Everything here is plain C#, set the
same way you set any other property.

```csharp
using Avalonia.Automation; // AutomationProperties.SetName / SetHelpText
using Loam;                 // Icons
using Loam.Controls;        // IconButton, Menu, DataGrid<T>, …
```

::: tip Mental model
Three things make a Loam control accessible, and Loam handles two of them for you. **Focus & keys** come
from the Avalonia base type or a control-specific key handler — you get them for free. **Automation names**
are derived automatically wherever there's text to derive them from (a label, content, or value). The one
gap you own is an **icon-only control with no text** — there is nothing to derive a name from, so you set
one with `AutomationProperties.SetName`. If a control shows a word, it already announces it; if it shows
only a glyph, name it.
:::

## Where names come from

A screen reader announces a control by its *automation name*. Loam fills this in automatically from
whatever text the control already carries, falling back through a small chain. You only need to intervene
when that chain comes up empty — typically an icon-only control.

| Control | Automation name derived from | You set a name when… |
| --- | --- | --- |
| [`Button`](../components/buttons#button) / [`Fab`](../components/buttons#fab) | `Content` / `Label` text | never (it has a label) |
| [`IconButton`](../components/buttons#iconbutton) / [`ToggleIconButton`](../components/buttons#toggleiconbutton) | — (glyph only) | always |
| [`NavLink`](../components/navigation#navlink) | `Label`, then `Content`, then `Href` | `Content` is a custom control, not text |
| [`NavigationRailItem`](../components/navigation#navigationrail) / `BottomNavigationItem` | `Label` | always set `Label` (icon-only rows) |
| [`ListItem`](../components/data-display#list-listitem-listsubheader) | `Content` + `SecondaryText` | the row is icon-only |
| [`TreeViewItem`](../components/data-display#loam-controls-treeview-loam-controls-treeviewitem) | `Text` | only an `Icon` is set, no `Text` |
| Field inputs ([`TextField`](../components/inputs#textfield), [`Select`](../components/inputs#select), pickers) | `Label`, then placeholder / value | rarely (set a `Label`) |
| [`Slider`](../components/inputs#slider) / [`Rating`](../components/inputs#rating) | — (no inherent text) | when there's no adjacent label |
| [`ProgressLinear`](../components/overlays#progresslinear) / [`ProgressCircular`](../components/overlays#progresscircular) / [`Skeleton`](../components/overlays#skeleton) | `Label` property | set `Label` so the busy state is announced |

::: tip Name your icon-only controls
This is the single highest-value accessibility habit in a Loam app. An `IconButton`, an icon-only
`NavLink`, or a bare `Slider` has no text for assistive technology to read, so it announces as nothing.
Give it a name:

```csharp
using Avalonia.Automation;
using Loam;
using Loam.Controls;

var delete = new IconButton { Icon = Icons.Material.Filled.Delete, Color = LoamColor.Error };
AutomationProperties.SetName(delete, "Delete");
```

Several controls expose a dedicated property that *is* the name — prefer it when it exists:
`ProgressCircular.Label`, `Skeleton.Label`, `NavigationRailItem.Label`, `NavLink.Label`. Setting the
property keeps the name and the visible text in sync.
:::

## Focus management

Focus behavior is consistent across the library:

- **Tab order** — every interactive control is focusable and joins the tab order;
  <kbd>Tab</kbd> / <kbd>Shift</kbd>+<kbd>Tab</kbd> move forward and back. A focused control shows a focus
  highlight.
- **Arrow keys move *within* a control** — once a composite control (a `DataGrid<T>` row group, a
  `ToggleGroup`, a `Tabs` strip, a `Menu` popup) has focus, the arrow keys navigate *inside* it rather than
  leaving it. <kbd>Tab</kbd> is what moves you *between* controls.
- **Activation keys are <kbd>Enter</kbd> and <kbd>Space</kbd>** — this is uniform: buttons, list rows, tree
  nodes, tabs, expansion-panel headers, nav items, and field pickers all activate on these two keys.

### Focus restore on overlays

Loam's transient surfaces capture the element that had focus when they opened and return focus to it when
they close. This holds for [`Menu`](../components/buttons#menu),
[`DialogService`](../components/overlays#dialogservice-idialogservice),
[`Overlay`](../components/overlays#overlay), and [`Popover`](../components/overlays#popover) — so dismissing
a dialog or closing a menu lands the user back where they were, not at the top of the page.

For dialogs specifically, when `DialogOptions.AutoFocus` is `true` (the default) the first enabled,
visible, focusable child of the dialog receives focus as it opens. The backdrop scrim and the dialog
surface carry their own automation help text describing whether Escape and scrim-click dismissal are
enabled.

```csharp
using Loam.Controls;

// AutoFocus moves focus into the dialog on open; focus is restored to the
// triggering control after ShowAsync resolves.
var result = await DialogService.For(this).ShowAsync(
    "Rename",
    instance =>
    {
        var field = new TextField { Label = "Name" }; // first focusable child → gets focus
        var ok = new Button { Content = "Save" };
        ok.Click += (_, _) => instance.Ok(field.Text);
        return new StackPanel { Spacing = 12, Children = { field, ok } };
    });
```

## Disabled semantics

Setting `IsEnabled = false` on any Loam control does three things consistently:

- **Removes it from the tab order** — it can no longer be focused.
- **Blocks pointer and keyboard activation** — clicks and activation keys are ignored.
- **Dims it to the theme's disabled opacity** — resolved from the `StateDisabledOpacity` theme token, so a
  disabled control reads as muted in both light and dark themes (see
  [Theming → tokens](./theming#tokens)).

Composite controls disable their parts together. A disabled [`FileUpload`](../components/inputs#fileupload)
disables its generated picker button, file chips, and clear action at once; a disabled
[`Form`](../components/inputs#form) disables its generated submit/reset actions. In both cases the
*programmatic* API stays live — `FileUpload.Clear()`, `Form.Validate()`, and `Form.ResetFields()` still run
so you can drive state from your view model while the UI is locked.

The field [pickers](../components/pickers) follow the same rule: a disabled picker suppresses pointer,
keyboard, and `OpenPicker()` from opening the flyout, while still accepting programmatic value updates.

::: warning Disabled is not the same as read-only
`IsEnabled = false` removes a control from the tab order entirely — a keyboard user can't reach it to read
its value. When you want a value to stay *readable and focusable* but not editable, prefer the control's
own read-only flag instead: [`TextField.ReadOnly`](../components/inputs#textfield) or
[`Rating.ReadOnly`](../components/inputs#rating). Reserve `IsEnabled = false` for actions that genuinely
aren't available right now.
:::

## Keyboard reference by control

The tables below list the keys each control handles *once it has focus*. They are grounded in each
control's actual key handling; the per-component pages carry the same detail in context.

### Buttons & menus

| Control | Key | Action |
| --- | --- | --- |
| `Button` / `IconButton` / `Fab` | <kbd>Space</kbd> / <kbd>Enter</kbd> | Invoke `Click` / `Command`. |
| `ToggleIconButton` | <kbd>Space</kbd> / <kbd>Enter</kbd> | Flip `Toggled`, then fire `Click`. |
| `Menu` (trigger) | <kbd>Space</kbd> / <kbd>Enter</kbd> | Open the flyout and focus the first enabled row. |
| `Menu` (popup) | <kbd>↑</kbd> / <kbd>↓</kbd> | Move through enabled rows (wraps); disabled rows are skipped. |
| `Menu` (popup) | <kbd>Esc</kbd> | Close the popup; focus returns to the trigger. |

See [Buttons & menus → Accessibility](../components/buttons#accessibility-keyboard).

### Form inputs

| Control | Key | Action |
| --- | --- | --- |
| `TextField` / `MaskedTextField` | (typing) | Edit text; `TextField` validates on blur when `Required`/`Validation` is set. |
| `NumericField` | <kbd>↑</kbd> / <kbd>↓</kbd> | Step `Value` by `Step`, clamped to `[Minimum, Maximum]`. |
| `Select` | <kbd>Enter</kbd> / <kbd>Space</kbd> | Open the flyout. <kbd>Esc</kbd> closes it. |
| `Autocomplete` | <kbd>Esc</kbd> | Close the suggestion flyout. <kbd>Enter</kbd>/<kbd>Space</kbd> re-runs the search. |
| `Slider` | <kbd>←</kbd>/<kbd>↓</kbd>, <kbd>→</kbd>/<kbd>↑</kbd> | Step `Value`. <kbd>Home</kbd>/<kbd>End</kbd> jump to `Minimum`/`Maximum`. |
| `Rating` | <kbd>←</kbd>/<kbd>→</kbd> (and <kbd>↑</kbd>/<kbd>↓</kbd>) | Change the score. <kbd>Home</kbd> clears to 0, <kbd>End</kbd> sets the max, <kbd>Space</kbd>/<kbd>Enter</kbd> activate. |
| `ToggleGroup` | <kbd>←</kbd>/<kbd>→</kbd> (and <kbd>↑</kbd>/<kbd>↓</kbd>) | Move between segments. <kbd>Home</kbd>/<kbd>End</kbd> jump to first/last; <kbd>Space</kbd>/<kbd>Enter</kbd> select. |
| `CheckBox` / `Switch` / `Radio` | <kbd>Space</kbd> | Toggle (inherited from the Avalonia base control). |

See [Form inputs → Accessibility](../components/inputs#accessibility-keyboard).

### Pickers

| Control | Key | Action |
| --- | --- | --- |
| Field picker (closed) | <kbd>Enter</kbd> / <kbd>Space</kbd> | Open the flyout (non-editable mode). |
| Field picker in `Editable` mode | <kbd>Alt</kbd>+<kbd>↓</kbd> | Open the flyout — <kbd>Enter</kbd>/<kbd>Space</kbd> belong to the text box. |
| Date/Time/Range flyout | <kbd>Esc</kbd> | Close without committing; OK commits, Cancel discards. |
| `Editable` field | <kbd>Enter</kbd> | Commit typed text (also on focus loss); invalid text stays on screen with the `Invalid…` error. |
| `MonthCalendar` | <kbd>Enter</kbd> / <kbd>Space</kbd> | Select the focused day and raise `DateSelected`; arrow keys move across days and months. |

`ColorPicker` commits on swatch selection rather than via OK/Cancel. The inline clear button (when
`Clearable`) is named "Clear date" / "Clear time" / "Clear dates" and clears without opening the flyout.
See [Pickers → Accessibility](../components/pickers#accessibility-keyboard).

### Data display

| Control | Key | Action |
| --- | --- | --- |
| `DataGrid<T>` row | <kbd>↑</kbd>/<kbd>↓</kbd>, <kbd>Home</kbd>/<kbd>End</kbd> | Move focus between rendered rows (no wrap, stays on the current page). |
| `DataGrid<T>` row | <kbd>Space</kbd> / <kbd>Enter</kbd> | Select the focused row (toggles in `Multiple`). |
| `DataGrid<T>` (`Multiple`) | <kbd>Shift</kbd>+<kbd>↑</kbd>/<kbd>↓</kbd>/<kbd>Home</kbd>/<kbd>End</kbd>, <kbd>Ctrl</kbd>+<kbd>A</kbd> | Extend / select-all the rendered rows. <kbd>Esc</kbd> clears. |
| `DataGrid<T>` | <kbd>Ctrl</kbd>/<kbd>Cmd</kbd>+<kbd>C</kbd> | Copy the selection (or whole view) as TSV. |
| `Tabs` header | <kbd>←</kbd>/<kbd>↓</kbd>, <kbd>→</kbd>/<kbd>↑</kbd> | Move to the previous / next tab. <kbd>Enter</kbd>/<kbd>Space</kbd> select. |
| `TreeView` node | <kbd>→</kbd> / <kbd>←</kbd> | Expand / collapse (or step into / out of children). <kbd>↑</kbd>/<kbd>↓</kbd> move through visible nodes. |
| `TreeView` node | <kbd>Enter</kbd> / <kbd>Space</kbd> | Select / toggle the focused node. |
| `ExpansionPanel` header | <kbd>Enter</kbd> / <kbd>Space</kbd> | Toggle the panel (announces "Expanded"/"Collapsed"). |
| `ListItem` row | <kbd>Enter</kbd> / <kbd>Space</kbd> | Raise `Activated`. |
| `Carousel` | <kbd>←</kbd> / <kbd>→</kbd> | Previous / next slide; arrows and bullets are individually focusable. |
| `Pagination` | <kbd>Tab</kbd> + <kbd>Enter</kbd>/<kbd>Space</kbd> | Move to and activate a page or arrow button. |

See [Data display → Accessibility](../components/data-display#accessibility-keyboard) and the dedicated
[DataGrid keyboard table](../components/data-display#keyboard).

### Overlays & navigation

| Control | Key | Action |
| --- | --- | --- |
| `DialogService` | <kbd>Esc</kbd> | Cancel while `DismissOnEscape` is `true`. AutoFocus focuses the first focusable child on open. |
| `SnackbarService` toast | <kbd>Esc</kbd> | Dismiss the focused toast (even with no dismiss button shown). |
| `Overlay` | <kbd>Esc</kbd> | Set `Visible = false` while `AutoClose` is enabled. |
| `Popover` | <kbd>Esc</kbd> | Close the open surface; a `Trigger` also opens on <kbd>Space</kbd>/<kbd>Enter</kbd>. |
| `CommandPalette` | <kbd>↓</kbd>/<kbd>↑</kbd>, <kbd>Enter</kbd>, <kbd>Esc</kbd> | Move the highlight, run the command, close the palette. |
| `Link` / `NavLink` / `NavGroup` | <kbd>Enter</kbd> / <kbd>Space</kbd> | Activate the link, or toggle a `NavGroup` open/closed. |
| `NavigationRail` / `BottomNavigation` item | <kbd>Enter</kbd> / <kbd>Space</kbd> | Select the destination. |

See [Overlays → Accessibility](../components/overlays#accessibility-keyboard) and
[Navigation → Accessibility](../components/navigation#accessibility-keyboard).

## Recipe: an accessible icon toolbar

A row of icon-only actions is the most common place accessibility slips, because nothing in the markup
carries text. The fix is mechanical: give every glyph an `AutomationProperties` name (and, for sighted
users, a [`Tooltip`](../components/overlays#tooltip)). Everything below is plain C#.

```csharp
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

IconButton Named(string glyph, string name, LoamColor color = LoamColor.Default)
{
    var button = new IconButton { Icon = glyph, Color = color, Variant = Variant.Text };
    AutomationProperties.SetName(button, name); // screen-reader name
    Tooltip.Set(button, name);                  // visible hint on hover/focus
    return button;
}

var favorite = new ToggleIconButton
{
    Icon = Icons.Material.Filled.FavoriteBorder,
    ToggledIcon = Icons.Material.Filled.Favorite,
    Color = LoamColor.Primary,
};
AutomationProperties.SetName(favorite, "Favorite");
Tooltip.Set(favorite, "Favorite");

var toolbar = new StackPanel
{
    Orientation = Orientation.Horizontal,
    Spacing = 4,
    Children =
    {
        Named(Icons.Material.Filled.Edit, "Edit"),
        Named(Icons.Material.Filled.ContentCopy, "Copy"),
        Named(Icons.Material.Filled.Delete, "Delete", LoamColor.Error),
        favorite,
    },
};
```

The same pattern covers an icon-only [`NavLink`](../components/navigation#navlink) (set `Label` instead of
`Content`), a [`NavigationRailItem`](../components/navigation#navigationrail) (always set `Label`), and a
busy [`ProgressCircular`](../components/overlays#progresscircular) (set `Label`).

## Reduced motion

A few controls animate by default. Where motion could be a problem, each one exposes a switch to turn it
off without losing the control:

- [`Collapse`](../components/overlays#collapse) — set `Animated = false` (or `Duration = TimeSpan.Zero`)
  for an instant reveal.
- [`Carousel`](../components/data-display#loam-controls-carousel) — `AutoPlay` is off by default; leave it
  off, or if you enable it keep `ShowArrows`/`ShowBullets` on so there's always a manual control.

::: tip Don't auto-advance content the user can't pause
Motion a user can't stop is a real accessibility barrier. `Carousel.AutoPlay` defaults to off for exactly
this reason — turn it on only with manual controls visible and a calm `AutoPlayInterval`.
:::

## See also

- [Buttons & menus → Accessibility](../components/buttons#accessibility-keyboard) — the button family and `Menu` popup keys.
- [Form inputs → Accessibility](../components/inputs#accessibility-keyboard) — field, toggle, and specialized-input keys.
- [Pickers → Accessibility](../components/pickers#accessibility-keyboard) — flyout commit/dismiss and editable-mode keys.
- [Data display → Accessibility](../components/data-display#accessibility-keyboard) — grid, tabs, tree, and list navigation.
- [Overlays → Accessibility](../components/overlays#accessibility-keyboard) — dialog, snackbar, overlay, and palette behavior.
- [Navigation → Accessibility](../components/navigation#accessibility-keyboard) — links, nav rows, rail, and bottom-bar keys.
- [Theming → tokens](./theming#tokens) — where the disabled-opacity and focus tokens come from.
