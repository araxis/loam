---
title: Getting Started
---

# Getting Started

Loam is a control library, not a framework — you keep your Avalonia `Application`, your window shell, and
your own architecture, and Loam supplies a complete themed control set on top. Getting from an empty
Avalonia app to a Loam screen is three steps: **reference the package(s)**, **register the theme**, and
**compose controls in C#**. There is no XAML, no markup compiler, and no provider tree to wire up — every
control is a plain Avalonia control you `new` up with object initializers.

::: tip Mental model
Think in three layers, added in order. **`FluentTheme`** supplies the base templates for the window shell
and the built-in controls Loam composes (like `TextBox` and `ScrollViewer`). **`LoamTheme`** projects the
palette, typography, shadows, and z-index tokens into Avalonia resources and registers the *core* control
themes. **Satellite registrars** (`LoamCharts`, `LoamPickers`, `LoamData`) add the themes for their
package and reuse the tokens `LoamTheme` already projected. Get the order right once in `App` and every
control resolves its theme for the life of the app.
:::

## Requirements

- **.NET 8** SDK
- An **Avalonia 12** application

## 1. Reference Loam

Install the core package after a release is published, or reference the project while developing from
this repository:

```bash
dotnet add package Loam
```

The chart, picker, and heavy data controls ship as **opt-in satellite packages** (since 3.1) — add only
the ones you use. Each depends on the core package, so you don't reference `Loam` twice:

```bash
dotnet add package Loam.Charts    # PieChart, BarChart, LineChart
dotnet add package Loam.Pickers   # DatePicker, TimePicker, ColorPicker, DateRangePicker, MonthCalendar
dotnet add package Loam.Data      # DataGrid<T>, SimpleTable, TreeView, Pagination
```

Namespaces are unchanged across the split — everything stays under `Loam.Controls`, so your
using-directives don't change when you add a satellite. If you're upgrading from 3.0, see the
[v3 → v3.1 migration guide](/migration/v3-to-v3.1).

### Which packages do I need?

Start with `Loam` alone — it covers buttons, inputs, text, surfaces, layout, navigation, and the overlay
services. Add a satellite only when you reach for a control it owns:

| Add this package | When you need | Don't forget |
| --- | --- | --- |
| `Loam` *(always)* | Buttons, `TextField`, `Text`, `Paper`, layout, navigation, dialogs, snackbars | `new LoamTheme()` |
| `Loam.Charts` | `PieChart`, `BarChart`, `LineChart` | `new LoamCharts()` |
| `Loam.Pickers` | `DatePicker`, `TimePicker`, `ColorPicker`, `DateRangePicker`, `MonthCalendar` | `new LoamPickers()` |
| `Loam.Data` | `DataGrid<T>`, `SimpleTable`, `TreeView`, `Pagination` | `new LoamData()` |

Each satellite package pairs with a registrar of the same family name — referencing the package is half
the job, registering its themes (step 2) is the other half.

For source-based development, add project references instead:

```xml
<ItemGroup>
  <ProjectReference Include="..\Loam\src\Loam\Loam.csproj" />
  <ProjectReference Include="..\Loam\src\Loam.Charts\Loam.Charts.csproj" />
  <ProjectReference Include="..\Loam\src\Loam.Pickers\Loam.Pickers.csproj" />
  <ProjectReference Include="..\Loam\src\Loam.Data\Loam.Data.csproj" />
</ItemGroup>
```

## 2. Register the theme

Add Avalonia's `FluentTheme` (it supplies base templates for the window shell and built-in controls
Loam composes, such as `TextBox` and `ScrollViewer`), then layer `LoamTheme` on top:

```csharp
using Avalonia;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Loam.Theming;

public sealed class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());   // base templates for the shell + built-in controls
        Styles.Add(new LoamTheme());     // Loam's pure-C# theming + core control themes

        // Add a registrar for each satellite package you reference (omit the ones you don't use):
        Styles.Add(new LoamCharts());    // from Loam.Charts
        Styles.Add(new LoamPickers());   // from Loam.Pickers
        Styles.Add(new LoamData());      // from Loam.Data

        RequestedThemeVariant = ThemeVariant.Light;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}
```

`new LoamTheme()` projects the palette/typography/shadows into Avalonia resources and registers every
**core** control theme. Each satellite registrar (`LoamCharts`, `LoamPickers`, `LoamData`) — all in the
`Loam.Theming` namespace — adds the control themes for its package and reuses the tokens `LoamTheme`
already projected, so order them after `LoamTheme`. A satellite control rendered without its registrar
falls back to unthemed defaults.

::: warning Order matters
The three layers are added in dependency order: `FluentTheme` → `LoamTheme` → satellites. `LoamTheme`
relies on the base templates `FluentTheme` provides, and each satellite reuses the tokens `LoamTheme`
projects. If a satellite control looks unthemed, the usual cause is a missing or out-of-order registrar —
confirm its `new Loam…()` line sits *after* `new LoamTheme()`.
:::

Tokens resolve through dynamic resources, so switching `RequestedThemeVariant` between
`ThemeVariant.Light` and `ThemeVariant.Dark` — or re-projecting the palette — re-styles the whole app at
runtime. See [Theming](./theming) for the data model and runtime swapping.

## 3. Build a screen in C#

Loam controls are plain Avalonia controls — compose them with object initializers:

```csharp
using Loam;
using Loam.Controls;

public sealed class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        Width = 420;
        Height = 360;
        Content = new StackPanel
        {
            Margin = new Avalonia.Thickness(24),
            Spacing = 12,
            Children =
            {
                new Text { Text = "Sign in", Typo = Typo.H5 },
                new TextField { Label = "Email", Variant = Variant.Outlined },
                new TextField { Label = "Password", Variant = Variant.Outlined },
                new Button { Content = "Continue", Variant = Variant.Filled, Color = LoamColor.Primary, FullWidth = true },
            },
        };
    }
}
```

Two `using`s cover most code: `Loam` for the shared enums (`Variant`, `LoamColor`, `LoamSize`, `Typo`) and
`Icons`, and `Loam.Controls` for the controls themselves. The shared knobs mean the same thing on every
control — see [Components overview → common parameters](/components/overview#common-parameters) — so once
you've set `Variant`/`Color`/`Size` on one control you've learned them all.

## 4. Dialogs & snackbars (no provider needed)

Loam's overlay services render into the window's overlay layer, so there is no provider component to
register — just target any visual in the window:

```csharp
var confirmed = await DialogService.For(this)
    .ConfirmAsync("Delete item?", "This action cannot be undone.", "Delete", "Cancel");

SnackbarService.For(this).Add(confirmed ? "Deleted" : "Cancelled", LoamColor.Info);
```

`DialogService.For(visual)` and `SnackbarService.For(visual)` resolve the window hosting the visual (so
`this` inside a `Window` or any attached control works). `ConfirmAsync` returns a `bool`; `Add` takes a
message and an optional `LoamColor` severity. Full overlay surface — custom dialog bodies, snackbar
actions, flyouts — is covered in [Overlays](/components/overlays).

## Recipe: a complete first window

The pieces above, assembled into one runnable window — the theme registered in `App`, a composed form,
and a confirm dialog that reports its result through a snackbar. This is the shape of a real Loam screen:
plain C#, object initializers, and the overlay services targeting `this`.

```csharp
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title  = "Loam";
        Width  = 420;
        Height = 380;

        var email    = new TextField { Label = "Email", Variant = Variant.Outlined };
        var password = new TextField { Label = "Password", Variant = Variant.Outlined };

        var submit = new Button
        {
            Content   = "Continue",
            Variant   = Variant.Filled,
            Color     = LoamColor.Primary,
            StartIcon = Icons.Material.Filled.Check,
            FullWidth = true,
        };
        submit.Click += async (_, _) =>
        {
            var ok = await DialogService.For(this)
                .ConfirmAsync("Sign in?", $"Continue as {email.Text}?", "Sign in", "Cancel");
            SnackbarService.For(this).Add(ok ? "Signed in" : "Cancelled", LoamColor.Info);
        };

        Content = new StackPanel
        {
            Margin  = new Thickness(24),
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Text { Text = "Sign in", Typo = Typo.H5 },
                email,
                password,
                submit,
            },
        };
    }
}
```

::: details Where does `email.Text` come from?
`TextField` is a `TemplatedControl` that wraps an internal `TextBox` and exposes its own two-way `Text`
property — that's the typed value. The rest of `TextField` is presentation (`Label`, `HelperText`,
`Variant`, floating/shrink label behavior), so you bind or read `Text` directly without reaching for the
inner `TextBox`.
:::

## Run the gallery

The repository ships a live gallery that demonstrates every component group:

```bash
dotnet run --project samples/Loam.Gallery
```

## Accessibility & keyboard

Loam builds on Avalonia's input and automation stack, so a screen assembled from the steps above is
keyboard-operable from the start:

- **Focus order** — controls join the tab order in layout order; <kbd>Tab</kbd> / <kbd>Shift</kbd>+<kbd>Tab</kbd> move between the `TextField`s and the `Button`.
- **Activation** — <kbd>Space</kbd> / <kbd>Enter</kbd> invoke a focused `Button`'s `Click` / `Command`.
- **Dialogs** — the confirm dialog traps focus while open and (by default) closes on <kbd>Esc</kbd> or a backdrop click, restoring focus to the control that opened it.
- **Snackbars** — toasts auto-dismiss after a few seconds and respond to <kbd>Esc</kbd> while focused; they carry automation names so assistive tech announces them.
- **Theme variant** — switching `RequestedThemeVariant` to `ThemeVariant.Dark` re-projects the role-based palette, which is built for legible contrast in both variants.

::: tip Name your icon-only controls
A label-less control (an icon-only button, for instance) has no text for assistive technology to read.
Give it an accessible name so screen readers announce its purpose:

```csharp
using Avalonia.Automation;
using Loam;
using Loam.Controls;

var close = new IconButton { Icon = Icons.Material.Filled.Close, Color = LoamColor.Default };
AutomationProperties.SetName(close, "Close");
```
:::

## See also

- [Theming](./theming) — the `LoamTheme` data model, tokens, dark mode, and runtime palette swapping.
- [Authoring UI in C#](./csharp-ui) — patterns for composing Avalonia views without XAML.
- [Components overview](/components/overview) — the full control catalog and the shared `Variant`/`Color`/`Size` parameters.
- [Buttons & menus](/components/buttons) and [Form inputs](/components/inputs) — the controls used on this page.
- [Overlays](/components/overlays) — the full `DialogService` / `SnackbarService` surface.

Next: see **[Theming](./theming)** to customize colors and dark mode, then browse the
**[Components](/components/overview)**.
