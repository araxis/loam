---
title: Getting Started
---

# Getting Started

## Requirements

- **.NET 8** SDK
- An **Avalonia 12** application

## 1. Reference Loam

Install the package after a release is published, or reference the project while developing from this
repository:

```bash
dotnet add package Loam
```

For source-based development, add a project reference:

```xml
<ItemGroup>
  <ProjectReference Include="..\Loam\src\Loam\Loam.csproj" />
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
        Styles.Add(new LoamTheme());     // Loam's pure-C# theming + control themes
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

That single `new LoamTheme()` registers every Loam control theme and projects the palette/typography/
shadows into Avalonia resources.

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

## 4. Dialogs & snackbars (no provider needed)

Loam's overlay services render into the window's overlay layer, so there is no provider component to
register — just target any visual in the window:

```csharp
var confirmed = await DialogService.For(this)
    .ConfirmAsync("Delete item?", "This action cannot be undone.", "Delete", "Cancel");

SnackbarService.For(this).Add(confirmed ? "Deleted" : "Cancelled", LoamColor.Info);
```

## Run the gallery

The repository ships a live gallery that demonstrates every component group:

```bash
dotnet run --project samples/Loam.Gallery
```

Next: see **[Theming](./theming)** to customize colors and dark mode, then browse the
**[Components](/components/overview)**.
