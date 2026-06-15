---
title: Recipes
---

# Recipes

The component pages document each control on its own. This page does the opposite: it shows the *seams* —
how a handful of controls fit together into the patterns you actually build. Each recipe is a complete,
self-contained C# method or class you can paste into a view and adapt, grounded in the same source the
component pages describe. There is no XAML and no hidden glue; every recipe is plain Avalonia controls
composed with object initializers.

```csharp
using Loam;          // Variant, LoamColor, LoamSize, Typo, Icons
using Loam.Controls; // Form, TextField, Select, DataGrid<T>, DialogService, Layout, …
```

::: tip Mental model
Loam stays out of your architecture. A recipe is just controls you `new` up and wire with their own
events and callbacks — `Form.Submitted`, `DataGrid<T>.SelectionChanged`, `DialogService.ShowAsync`,
`Drawer.Toggle()`. Pick the controls, set their properties, subscribe to their events, and you have a
working screen. Hold a reference to anything you need to read back (the `DataGrid<T>`, a `Field`'s editor,
a detail panel) and mutate it directly.
:::

## Choosing a recipe

| You're building | Reach for | Recipe |
| --- | --- | --- |
| A data-entry screen with validation | [`Form`](../components/inputs#form) + fields | [Validated form](#a-validated-form) |
| A list where picking a row shows details | [`DataGrid<T>`](../components/data-display#datagrid-t) + a detail panel | [Master–detail](#master-detail) |
| A modal step that returns a value | [`DialogService`](../components/overlays#dialogservice-idialogservice) | [A dialog workflow](#a-dialog-workflow) |
| The whole window frame | [`Layout`](../components/layout#layout) + `AppBar` + `Drawer` | [An app shell](#an-app-shell) |

---

## A validated form

`Form` is a lightweight container: drop fields into its `Children`, give it a `SubmitText`, and it
generates the submit/reset action row for you. Its built-in `Validate()` walks every descendant
`TextField` and calls each one's `Validate()` — so `Required` and a `Validation` delegate on a `TextField`
are checked automatically. Controls that aren't `TextField` (like `Select`) aren't validated for you, so
validate those in the `SubmitAction` callback, which runs right after the built-in pass.

```csharp
using Avalonia.Controls;
using Loam;
using Loam.Controls;

public static Form BuildContactForm()
{
    var name = new TextField
    {
        Label    = "Full name",
        Required = true,
        Variant  = Variant.Outlined,
    };

    var email = new TextField
    {
        Label      = "Email",
        Required   = true,
        Validation = value =>
            value is { Length: > 0 } && value.Contains('@')
                ? null
                : "Enter a valid email address",
    };

    var role = new Select
    {
        Label       = "Role",
        Placeholder = "Choose a role",
    };
    role.Items.Add(new SelectItem("Viewer", "viewer"));
    role.Items.Add(new SelectItem("Editor", "editor"));
    role.Items.Add(new SelectItem("Owner", "owner"));

    var form = new Form
    {
        Title       = "Invite a teammate",
        Subtitle    = "They'll receive access with the role you choose.",
        SubmitText  = "Send invite",
        ResetText   = "Clear",
        SubmitIcon  = Icons.Material.Filled.Check,
        SuccessText = "Invite sent.",
        ErrorText   = "Please fix the errors above.",
        Children    = { name, email, role },
    };

    form.SubmitAction = f =>
    {
        // The built-in pass already validated the TextFields and set f.IsValid.
        // Validate the Select ourselves and fold the result back into the form.
        var hasRole = role.Value is not null;
        role.Error     = !hasRole;
        role.ErrorText = hasRole ? null : "Pick a role";

        if (f.IsValid && hasRole)
        {
            // f.IsValid is true and the Select is set — safe to submit.
        }
    };

    return form;
}
```

::: tip Validation timing
A `TextField` with `Required` or a `Validation` delegate also validates itself when it loses focus, so the
user gets feedback before they ever press submit. `Form.Validate()` (called for you by the generated submit
button) re-runs every field and updates `Form.IsValid`, then shows `SuccessText` or `ErrorText` in the
status line.
:::

Need a non-text input to sit inside the field chrome (a slider, a color swatch, a date display)? Wrap it
in a [`Field`](../components/inputs#field) — the shell gives you the same label and helper/error text
without assuming an editable text box.

---

## Master-detail

A `DataGrid<T>` raises `SelectionChanged` with the primary selected row (or `default` when the selection
clears). Hold a reference to a detail panel and rebuild it from that row. With the default selection mode
(`DataGridSelectionMode.Single`), clicking a row replaces the selection and fires the event with that row.

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

public sealed record Customer(string Name, string Plan, int Seats);

public static Control BuildMasterDetail()
{
    var customers = new[]
    {
        new Customer("Acme Corp", "Enterprise", 240),
        new Customer("Globex", "Team", 18),
        new Customer("Initech", "Team", 32),
    };

    var grid = new DataGrid<Customer>
    {
        Items = customers,
        Columns =
        {
            new DataGridColumn<Customer>("Name", c => c.Name),
            new DataGridColumn<Customer>("Plan", c => c.Plan),
            new DataGridColumn<Customer>("Seats", c => c.Seats)
            {
                Align = HorizontalAlignment.Right,
            },
        },
    };

    var detail = new ContentControl
    {
        Width   = 280,
        Content = new Text { Text = "Select a customer", Typo = Typo.Body2, Color = LoamColor.Secondary },
    };

    grid.SelectionChanged += selected =>
    {
        detail.Content = selected is null
            ? new Text { Text = "Select a customer", Typo = Typo.Body2, Color = LoamColor.Secondary }
            : new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new Text { Text = selected.Name, Typo = Typo.TitleMedium },
                    new Text { Text = $"Plan: {selected.Plan}", Typo = Typo.Body2 },
                    new Text { Text = $"Seats: {selected.Seats}", Typo = Typo.Body2 },
                },
            };
    };

    return new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Spacing     = 24,
        Children    = { grid, new Paper { Elevation = 1, Padding = new Avalonia.Thickness(16), Content = detail } },
    };
}
```

::: tip Live data
Assign an `ObservableCollection<T>` to `Items` and the grid refreshes on add/remove/reset on its own. To
also refresh when a *row's own properties* change (an edited cell), set `ObserveItemChanges = true` and use
rows that implement `INotifyPropertyChanged`. For a non-observable source mutated in place, call
`Refresh()`.
:::

---

## A dialog workflow

`DialogService` renders a scrim and a centered dialog into the window's overlay layer — no provider
component to register. Create one with `DialogService.For(this)` from any attached control. The
`ShowAsync` content factory receives a `DialogInstance` so the dialog's own buttons can close it with a
result; `await` resolves to a `DialogResult` carrying any data you passed to `Ok`.

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

public static async Task<string?> PromptForNameAsync(Control owner)
{
    var input = new TextField { Label = "Project name", Required = true };

    var result = await DialogService.For(owner).ShowAsync(
        "New project",
        instance =>
        {
            var create = new Button { Content = "Create", Variant = Variant.Filled, Color = LoamColor.Primary };
            create.Click += (_, _) =>
            {
                if (input.Validate() is null) // null means valid
                {
                    instance.Ok(input.Text); // closes the dialog, returns the text as result data
                }
            };

            var cancel = new Button { Content = "Cancel", Variant = Variant.Text };
            cancel.Click += (_, _) => instance.Cancel();

            return new StackPanel
            {
                Spacing  = 20,
                Children =
                {
                    input,
                    new StackPanel
                    {
                        Orientation         = Orientation.Horizontal,
                        Spacing             = 8,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Children            = { cancel, create },
                    },
                },
            };
        });

    return result.Canceled ? null : result.DataAs<string>();
}
```

::: tip Skip the boilerplate for yes/no
For a plain confirmation you don't need a custom factory — `await DialogService.For(this).ConfirmAsync("Delete?",
"This can't be undone.")` returns a `bool`, and `MessageBoxAsync` gives you up to three buttons resolving to
`true` / `false` / `null`. Use `ShowAsync` (above) only when the dialog must collect or return a value.
:::

---

## An app shell

`Layout` is the shell root: it docks an `AppBar` across the top, a `Drawer` down the left, and fills the
rest with its `Content` (usually a `MainContent`). The app bar's built-in navigation icon and the drawer's
`Toggle()` are all you need to wire a collapsible side nav. `Drawer.Items` takes plain `DrawerItem`
objects with an icon, text, and `OnClick`; the drawer tracks the selected index for you.

```csharp
using Loam;
using Loam.Controls;

public static Layout BuildAppShell()
{
    var main = new MainContent
    {
        Title    = "Dashboard",
        Subtitle = "Everything at a glance.",
    };

    var drawer = new Drawer
    {
        Title = "Loam App",
        Mode  = DrawerMode.Docked,
        Items =
        {
            new DrawerItem
            {
                Icon     = Icons.Material.Filled.Dashboard,
                Text     = "Dashboard",
                IsActive = true,
                OnClick  = () => main.Title = "Dashboard",
            },
            new DrawerItem
            {
                Icon    = Icons.Material.Filled.Groups,
                Text    = "Team",
                OnClick = () => main.Title = "Team",
            },
            new DrawerItem
            {
                Icon    = Icons.Material.Filled.Settings,
                Text    = "Settings",
                OnClick = () => main.Title = "Settings",
            },
        },
    };

    var appBar = new AppBar
    {
        Title          = "Loam App",
        Color          = LoamColor.Primary,
        NavigationIcon = Icons.Material.Filled.Menu,
        NavigationAction = drawer.Toggle, // the hamburger collapses/expands the drawer
        Actions =
        {
            new AppBarAction { Icon = Icons.Material.Filled.Notifications, Label = "Notifications" },
            new AppBarAction { Icon = Icons.Material.Filled.Person, Label = "Account" },
        },
    };

    return new Layout
    {
        AppBar  = appBar,
        Drawer  = drawer,
        Content = main,
    };
}
```

::: tip Docked vs temporary
A `Docked` drawer reserves layout space and `Toggle()` slides it between `DrawerWidth` and `0` (or set
`Mini` to collapse to icons instead). Switch `Mode` to `DrawerMode.Temporary` and the drawer floats over
the content with a scrim; in that mode `Escape`, a scrim click, or selecting an item closes it
automatically.
:::

---

## Accessibility & keyboard

Every recipe inherits the per-control behavior documented on the component pages, so composing them keeps
those guarantees:

- **Form** — submit and reset are real `Button`s in the tab order; <kbd>Space</kbd>/<kbd>Enter</kbd>
  activate them, and a field validates on blur so errors surface before submit.
- **DataGrid&lt;T&gt;** — once a row is focused, <kbd>↑</kbd>/<kbd>↓</kbd>/<kbd>Home</kbd>/<kbd>End</kbd>
  move row focus (selection follows in single-select); <kbd>Esc</kbd> clears the selection and
  <kbd>Ctrl</kbd>+<kbd>C</kbd> copies the current view.
- **DialogService** — the dialog is modal, focuses its first enabled control on open, and (by default)
  dismisses on <kbd>Esc</kbd> or a scrim click. Set `DismissOnEscape`/`DismissOnScrimClick` on
  `DialogOptions` to lock that down for destructive flows.
- **App shell** — the app bar's navigation button carries an accessible name from `NavigationLabel`, each
  `AppBarAction` from its `Label`, and a temporary drawer closes on <kbd>Esc</kbd>.

::: warning Name your icon-only actions
The `AppBarAction`s above pass a `Label`, which becomes the accessible name for the generated icon button.
Always set it — an icon alone has no text for a screen reader to announce.
:::

## See also

- [Form inputs](../components/inputs) — `Form`, `TextField`, `Select`, and `Field` in detail.
- [Data display → DataGrid&lt;T&gt;](../components/data-display#datagrid-t) — columns, sorting, paging, grouping, and selection modes.
- [Overlays & feedback → DialogService](../components/overlays#dialogservice-idialogservice) — confirm/message-box helpers and `DialogOptions`.
- [Surfaces & layout](../components/layout) — `Layout`, `AppBar`, `Drawer`, and `MainContent` anatomy.
- [Buttons & menus](../components/buttons) — the action controls these recipes wire up.
- [Getting Started](./getting-started) — registering the theme before any of this renders.
