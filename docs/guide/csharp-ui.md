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

A few Loam types share a name with `Avalonia.Controls` (`Button`, `Grid`, `CheckBox`, `Slider`,
`TabItem`, `Menu`, `TreeView`, `TreeViewItem`, `Carousel`, `DatePicker`, `TimePicker`). When a file
imports **both** `Avalonia.Controls` and `Loam.Controls`, qualify the Loam one:

```csharp
var button = new Loam.Controls.Button { Content = "Go" };
var grid   = new Loam.Controls.Grid();        // Loam's responsive 12-column grid
```

Inside the Loam library itself the local type always wins, so this only matters in your app code that
imports both namespaces.

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
