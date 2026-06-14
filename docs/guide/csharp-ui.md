---
title: Authoring UI in C#
---

# Authoring UI in C#

Loam is **pure C#** — there are no `.axaml` files in the library, and you don't need any in your app
either. This page covers the code-only patterns Loam uses internally and that you'll use when building
screens.

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

## Events & two-way values

Wire events with normal C# handlers; two-way properties are read/write:

```csharp
var button = new Button { Content = "Click me" };
button.Click += (_, _) => Console.WriteLine("clicked");

var field = new TextField();
field.Text = "hello";              // set
var current = field.Text;          // get
```

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
