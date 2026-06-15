---
title: Authoring UI in C#
---

# Authoring UI in C#

Loam is **pure C#** — there are no `.axaml` files in the library, and you don't need any in your app
either. This page covers the code-only patterns Loam uses internally and that you'll use when building
screens: how to compose a visual tree with object initializers, fill `Children`/`Content`, wire events,
bind to values and theme tokens, and structure the whole thing for MVVM.

There is nothing exotic here. A "view" is just a method (or a class) that returns a control. Object
initializers give you the top-to-bottom readability of markup; ordinary C# gives you loops, conditionals,
extraction into small builder methods, and refactoring tools — all of which markup makes awkward.

```csharp
using Loam;          // Variant, LoamColor, LoamSize, Typo, Icons
using Loam.Controls; // Button, TextField, Text, … (StackPanel and other panels come from Avalonia.Controls)
```

::: tip Mental model
A screen is an **expression**, not a document. You build it from the inside out: leaf controls become
the `Children` of a panel, panels become the `Content` of a window. Anything that needs to *change* at
runtime is a property you hold a reference to (or a binding); anything static you set once in the
initializer. When a block of tree gets too big to read, lift it into a small method that returns a
`Control`.
:::

## Composition patterns at a glance

The four mechanics below cover nearly every screen. Pick by what the parent expects.

| You want to | Use | Notes |
| --- | --- | --- |
| Lay several controls in a row/column | A panel's [`Children`](#collections) collection | `StackPanel`, `ResponsiveGrid`, etc. |
| Put one control inside another | The parent's `Content` | `Window`, `Card`, `Button`, any `ContentControl` |
| Fill a Loam container that owns its items | Its `ObservableCollection<T>` | `Tabs.Items`, `Select.Items`, `Menu.Items` |
| React to user input | A [C# event handler](#events-two-way-values) or `Command` | `+=` a lambda, or bind `Command` to your VM |
| Keep a value in sync with state | A [binding](#bindings) | `Bind(...Property, ...)` for two-way data |

## Object initializers over markup

Build the visual tree with constructors and initializers. It reads top-to-bottom like markup, but it's
just C#:

```csharp
var panel = new StackPanel
{
    Spacing = 8,
    Children =
    {
        new Text { Text = "Title", Typo = Typo.H6 },
        new TextField { Label = "Name" },
        new Button { Content = "OK", Variant = Variant.Filled, Color = LoamColor.Primary },
    },
};
```

## Children vs Content

Two shapes cover most layout. A **panel** holds many children in a `Children` collection; a
**`ContentControl`** (a `Window`, `Card`, `Button`, …) holds exactly one `Content`. You compose a screen
by nesting the two — children inside a panel, the panel as someone's content:

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

var window = new Window
{
    // Content = one child …
    Content = new StackPanel
    {
        Margin  = new Avalonia.Thickness(24),
        Spacing = 12,
        // … Children = many.
        Children =
        {
            new Text { Text = "Profile", Typo = Typo.H5 },
            new TextField { Label = "Display name" },
            new Button { Content = "Save", Variant = Variant.Filled, Color = LoamColor.Primary },
        },
    },
};
```

`Content` accepts any single object — a control, or a bare `string` that Avalonia wraps in a text
presenter (that's why `new Button { Content = "Save" }` works). When you need *several* things where one
is expected, reach for a panel and use its `Children`.

## Collections

Loam container controls expose `ObservableCollection<T>` you add items to:

```csharp
var tabs = new Tabs();
tabs.Items.Add(new TabItem("Overview", new Text { Text = "…" }));
tabs.Items.Add(new TabItem("Details",  new Text { Text = "…" }));

var select = new Select { Label = "Country" };
select.Items.Add(new SelectItem("United States", "us"));
select.Items.Add(new SelectItem("Germany", "de"));
```

Because the collection is observable, adding or removing later rebuilds the affected part of the tree —
no need to recreate the control. That makes these collections the natural seam for data that grows: load
your model, then `foreach` it into `Items`.

```csharp
var select = new Select { Label = "Country" };
foreach (var c in viewModel.Countries)        // any IEnumerable
{
    select.Items.Add(new SelectItem(c.Name, c.Code));
}
```

::: tip Initializer collection syntax
C#'s collection-initializer syntax calls `Add` for you, so the two styles below are equivalent. Use the
inline form for static items, the `foreach` form for data:

```csharp
var tabs = new Tabs
{
    Items =
    {
        new TabItem("Overview", overviewView),
        new TabItem("Details",  detailsView),
    },
};
```
:::

## Events & two-way values

Wire events with normal C# handlers; two-way properties are read/write:

```csharp
var button = new Button { Content = "Click me" };
button.Click += (_, _) => Console.WriteLine("clicked");

var field = new TextField();
field.Text = "hello";              // set
var current = field.Text;          // get
```

For anything you need to read or update *after* construction, keep a local reference (as `field` above)
rather than burying the control in an initializer. The handler closes over it:

```csharp
var name   = new TextField { Label = "Name" };
var submit = new Button { Content = "Save", Variant = Variant.Filled, Color = LoamColor.Primary };
submit.Click += (_, _) => Save(name.Text);   // reads the live value on click
```

## Bindings

Direct property assignment is fine for one-shot values; reach for a **binding** when a control and a data
source must stay in sync. Loam's two-way control properties (`TextField.Text`, `Select.Value`, an
Avalonia `CheckBox.IsChecked`, …) bind like any Avalonia `StyledProperty`. The code-only way to attach
one is `control.Bind(SomeProperty, new Binding(...))`:

```csharp
using Avalonia.Data;
using Loam;
using Loam.Controls;

var field = new TextField { Label = "Email" };
field.Bind(TextField.TextProperty,
    new Binding(nameof(ViewModel.Email)) { Source = viewModel, Mode = BindingMode.TwoWay });
```

`TextField.TextProperty` already defaults to two-way, so `Mode` is optional here; set it explicitly when
you want to be unambiguous or override a property's default direction. The same shape binds a command:

```csharp
var save = new Button { Content = "Save", Variant = Variant.Filled, Color = LoamColor.Primary };
save.Bind(Button.CommandProperty, new Binding(nameof(ViewModel.SaveCommand)) { Source = viewModel });
```

::: details When to bind vs. when to assign
If the value never changes after you build the tree (a label, a one-time default), just assign it — a
binding is overhead you don't need. Bind when the **source** can change (model updates the UI) or the
**target** can change and the source must follow (UI updates the model). For a handful of fields, setting
`DataContext` once on a parent and binding without an explicit `Source` (`new Binding(nameof(...))`) lets
the binding inherit the context — see [MVVM with Loam](#mvvm-with-loam).
:::

## Small builder methods

The single most useful habit in code-only UI: when a sub-tree grows past a screenful, **extract it into
a method that returns a `Control`**. Methods compose exactly like controls do, so the call site stays as
readable as the initializer it replaces — and you get naming, reuse, and parameters for free.

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

static Control Section(string title, params Control[] body)
{
    var panel = new StackPanel { Spacing = 8 };
    panel.Children.Add(new Text { Text = title, Typo = Typo.TitleMedium });
    foreach (var child in body)
    {
        panel.Children.Add(child);
    }
    return panel;
}

// Call sites read like the controls they build:
var page = new StackPanel
{
    Spacing = 24,
    Children =
    {
        Section("Account",
            new TextField { Label = "Email" },
            new TextField { Label = "Display name" }),
        Section("Preferences",
            new Avalonia.Controls.CheckBox { Content = "Email me product news" }),
    },
};
```

The same idea scales up: a method per *card*, per *toolbar row*, per *list item factory*. For a list, a
`Func<TModel, Control>` builder maps data to controls without a templating language.

## MVVM with Loam

Loam controls are ordinary Avalonia controls, so the standard Avalonia MVVM stack works unchanged: a
plain view-model that implements `INotifyPropertyChanged`, an `ICommand` for actions, and a `DataContext`
on the view that bindings resolve against. Loam ships **no** MVVM framework of its own — bring whatever
you already use (CommunityToolkit.Mvvm, ReactiveUI, or hand-rolled).

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

public sealed class SignInViewModel : INotifyPropertyChanged
{
    private string? _email;

    public string? Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public ICommand SignInCommand { get; }

    public SignInViewModel() => SignInCommand = new RelayCommand(() => { /* … */ });

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

The view sets `DataContext` once, then binds against it without repeating the `Source`. A binding with no
`Source` walks up the visual tree to the inherited `DataContext`:

```csharp
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

public sealed class SignInView : Window
{
    public SignInView()
    {
        DataContext = new SignInViewModel();

        var email = new TextField { Label = "Email" };
        email.Bind(TextField.TextProperty, new Binding(nameof(SignInViewModel.Email)));

        var signIn = new Button { Content = "Sign in", Variant = Variant.Filled, Color = LoamColor.Primary };
        signIn.Bind(Button.CommandProperty, new Binding(nameof(SignInViewModel.SignInCommand)));

        Content = new StackPanel
        {
            Margin  = new Avalonia.Thickness(24),
            Spacing = 12,
            Children = { new Text { Text = "Sign in", Typo = Typo.H5 }, email, signIn },
        };
    }
}
```

::: tip Event handler or `Command`?
For a quick, view-local action (toggle a panel, focus a field), a `Click += …` handler is the least
ceremony. For anything that belongs to the view-model — has business logic, needs `CanExecute`
enable/disable, or you want to unit-test — bind `Command` instead. Mixing both in one app is normal.
:::

`RelayCommand` above stands in for whatever command type your MVVM library provides; Loam does not supply
one. Loam's [`Form`](/components/inputs#form) container offers a lighter, non-binding alternative for
simple data entry — it collects fields, runs per-field `Validation`, and exposes `SubmitAction`/`IsValid`
without a view-model. Use `Form` for self-contained forms; reach for full MVVM bindings when the same
state drives more than one view.

## Name clashes with Avalonia

A few Loam types share a name with `Avalonia.Controls` (`Button`, `CheckBox`, `Slider`,
`TabItem`, `Menu`, `TreeView`, `TreeViewItem`, `Carousel`, `DatePicker`, `TimePicker`). When a file
imports **both** `Avalonia.Controls` and `Loam.Controls`, qualify the Loam one:

```csharp
var button = new Loam.Controls.Button { Content = "Go" };
```

> The responsive grid was renamed `Grid` → `ResponsiveGrid` (and `Item` → `Col`) in v3 precisely to
> remove this clash, so it needs no qualification. See the [migration guide](../migration/v2-to-v3).

Inside the Loam library itself the local type always wins, so this only matters in your app code that
imports both namespaces.

### Avoid per-file aliases with one global-usings file

Rather than adding `using LoamButton = …` to every file, drop a single **`GlobalUsings.cs`** in your
project. A `global using` **alias** makes the bare name resolve to the Loam type across the whole
project, with no per-file aliasing and no ambiguity:

```csharp
// GlobalUsings.cs — alias only the restyled types you actually use.
global using Button = Loam.Controls.Button;
global using Text = Loam.Controls.Text;
global using Card = Loam.Controls.Card;
global using Menu = Loam.Controls.Menu;
global using CheckBox = Loam.Controls.CheckBox;
global using Slider = Loam.Controls.Slider;
```

Now `new Button { … }` means Loam's button everywhere. The trade-off: in the rare file that needs the
Avalonia control, qualify it (`new Avalonia.Controls.Button()`). Alias only what you use both ways —
net-new Loam concepts (e.g. `ResponsiveGrid`, `Col`, `Paper`, `Chip`) never clash and need nothing.

## Binding to theme tokens

For custom controls, resolve Loam tokens through dynamic-resource observables — exactly how Loam's own
controls stay theme-aware:

```csharp
using Loam.Theming;

icon.Bind(Icon.ForegroundProperty, this.GetResourceObservable(LoamTokens.Primary));
```

`GetResourceObservable` returns a live observable, so when the theme switches (light ↔ dark, or a
palette change) every bound property updates automatically — there is no manual re-style step. The token
constants live on `LoamTokens` in `Loam.Theming`; see [Theming → tokens](/guide/theming#tokens) for the
full set and how they resolve.

::: warning Bind tokens, don't read brushes
Assigning a one-off brush from a token (`icon.Foreground = someBrush`) bakes in the *current* theme and
will not follow a later theme change. Prefer the `Bind(..., GetResourceObservable(token))` form for any
themed color so it tracks the active theme.
:::

## Recipe: a small composed screen

The patterns above — initializers, `Children`/`Content`, a builder method, an event handler, and a
binding — assembled into one view. A header, a couple of bound fields lifted into a builder, and a save
button wired to the view-model:

```csharp
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

public sealed class ProfileView : Window
{
    public ProfileView(ProfileViewModel vm)
    {
        DataContext = vm;
        Title = "Profile";
        Width = 420;

        Content = new StackPanel
        {
            Margin  = new Avalonia.Thickness(24),
            Spacing = 16,
            Children =
            {
                new Text { Text = "Edit profile", Typo = Typo.H5 },
                Field("Display name", nameof(ProfileViewModel.DisplayName)),
                Field("Email",        nameof(ProfileViewModel.Email)),
                SaveButton(),
            },
        };
    }

    // Builder method: one bound field, reused per property.
    static Control Field(string label, string path)
    {
        var field = new TextField { Label = label };
        field.Bind(TextField.TextProperty, new Binding(path));   // inherits DataContext
        return field;
    }

    Control SaveButton()
    {
        var save = new Button
        {
            Content   = "Save",
            Variant   = Variant.Filled,
            Color     = LoamColor.Primary,
            StartIcon = Icons.Material.Filled.Check,
            FullWidth = true,
        };
        save.Bind(Button.CommandProperty, new Binding(nameof(ProfileViewModel.SaveCommand)));
        return save;
    }
}
```

Everything is plain C#: the screen is an expression, the repeated field is a method, and the only state
that crosses the view/model boundary does so through bindings against the `DataContext`.

## Testing

Loam is verified with **xUnit + Avalonia.Headless**. The same approach works for your UI: render a
control in a headless window, pump the dispatcher, and assert on the visual tree.

```csharp
[AvaloniaFact]
public void Field_binds_text()
{
    var field = new TextField();
    new Window { Content = field }.Show();
    Dispatcher.UIThread.RunJobs();

    field.Text = "hi";
    Dispatcher.UIThread.RunJobs();
    // assert against field.GetVisualDescendants()…
}
```

Because views are just methods and classes, the same pattern tests a *composed* screen: construct it
with a stub view-model, `Show()` it in a headless window, drive properties or raise events, pump the
dispatcher, and assert. No markup means no XAML loader to stand up — the tree is exactly the objects you
allocated.

## Accessibility & keyboard

Authoring in C# does not change Avalonia's accessibility model — focus order, keyboard activation, and
automation peers all work the same as they would from markup. A few code-only habits keep it correct:

- **Tab order** follows the order you add controls to `Children`/`Content`. Build the tree in the order
  a keyboard user should traverse it, rather than relying on positioning to imply sequence.
- **Name icon-only controls.** A control with no text (an `IconButton`, a bare `Icon`) has nothing for a
  screen reader to read. Set an accessible name explicitly:

  ```csharp
  using Avalonia.Automation;

  var close = new IconButton { Icon = Icons.Material.Filled.Close };
  AutomationProperties.SetName(close, "Close");
  ```

- **Labels.** `Select` and `Field` set their accessible name from `Label` automatically. `TextField`
  renders `Label` visually but does not wire an automation name, so give a labelled `TextField` an explicit
  `AutomationProperties.Name` (or host it in a `Field`) when a screen reader needs to announce it.
- **`DataContext` and commands.** Binding `Command` (rather than only handling `Click`) lets the command's
  `CanExecute` drive the control's enabled state, which in turn removes disabled controls from the tab
  order automatically.

Per-control keyboard behavior (which keys activate what) is documented on each component page — see
[Buttons & menus → accessibility](/components/buttons#accessibility-keyboard) and
[Form inputs → accessibility](/components/inputs#accessibility-keyboard).

## See also

- [Getting started](/guide/getting-started) — install Loam, register the theme, and build a first window.
- [Form inputs](/components/inputs) — `TextField`, `Select`, `Form`, and the controls you'll bind most.
- [Buttons & menus](/components/buttons) — actions, `Command`, and the shared `Variant`/`Color`/`Size` knobs.
- [Theming](/guide/theming) — `LoamTokens`, semantic colors, and how `GetResourceObservable` tracks the theme.
- [Migration v2 → v3](../migration/v2-to-v3) — the `Grid` → `ResponsiveGrid` rename and other clash fixes.
