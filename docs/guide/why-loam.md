---
title: Why Loam vs plain Avalonia
---

# Why Loam vs plain Avalonia

[Avalonia](https://avaloniaui.net) already ships a capable control set and the `FluentTheme`. So what
does Loam add, and when should you *not* reach for it? This page is the honest version.

## The gap Loam fills

Avalonia gives you primitives (`Button`, `TextBox`, `Border`, panels) and a theme. Building an
*application* on top of that means re-deriving, in every project, the same things: a color system with
light/dark roles, elevation and state layers, a typography scale, form-field chrome, dialogs and
toasts, date/time/color pickers, and a data grid. Loam is that layer — built once, in pure C#, on top
of Avalonia.

### Side by side

A small "card with a primary action", first in plain Avalonia + Fluent:

```csharp
// Plain Avalonia: you assemble chrome, brushes, spacing, and typography yourself.
var card = new Border
{
    Background = new SolidColorBrush(Color.Parse("#1E1E1E")), // pick a surface color by hand
    CornerRadius = new CornerRadius(12),
    BoxShadow = BoxShadows.Parse("0 2 8 #40000000"),          // hand-tuned elevation
    Padding = new Thickness(16),
    Child = new StackPanel
    {
        Spacing = 8,
        Children =
        {
            new TextBlock { Text = "Invite teammate", FontSize = 20, FontWeight = FontWeight.SemiBold },
            new TextBox { Watermark = "Email" },
            new Button { Content = "Send invite" }, // re-style for "primary" yourself
        },
    },
};
```

The same thing in Loam — the vocabulary carries the design decisions:

```csharp
using Loam;
using Loam.Controls;

var card = new Card
{
    Elevation = 2,
    Content = new StackPanel
    {
        Spacing = 8,
        Children =
        {
            new Text { Text = "Invite teammate", Typo = Typo.H6 },
            new TextField { Label = "Email", Variant = Variant.Outlined },
            new Button { Content = "Send invite", Variant = Variant.Filled, Color = LoamColor.Primary },
        },
    },
};
```

Colors, radius, elevation, and type all resolve from theme tokens, so the card restyles itself when the
theme variant flips or the seed color changes — no hand-tuned hex values to chase.

## What you get over raw Avalonia

| Concern | Plain Avalonia + Fluent | Loam |
| --- | --- | --- |
| **Color system** | A Fluent accent; you build light/dark *roles* yourself | Role-based light/dark schemes + **Material You** from one seed (`SetSeed`), plus a high-contrast variant |
| **Theme consistency** | Stray scrollbars/tooltips/menus/window chrome read as Fluent | A base-chrome bridge maps those to the Loam palette in both variants |
| **Component vocabulary** | `FontWeight`, brushes, manual styles per control | `Variant` / `Color` / `Size` / `Dense` / `Elevation` knobs, consistent across controls |
| **Forms** | `TextBox` + your own label/helper/error chrome | `Field`/`TextField`/`Select`/`Form` with label, helper, validation states |
| **Overlays** | Roll your own dialog/toast hosting | `DialogService` / `SnackbarService` on the window overlay layer — no provider component |
| **Pickers** | Bring a package or build them | Self-contained date/time/color pickers + month calendar |
| **Data** | `DataGrid` (separate package) | `DataGrid<T>` with sort/filter/paging/virtualize/inline-edit, **grouping, collapsible groups, frozen columns, group aggregates** |
| **Authoring** | XAML or code | Code-only by design — one language, one toolchain |

## When to use plain Avalonia instead

Loam is additive, not a lock-in — so skip it when it doesn't earn its place:

- You only need a handful of stock controls and the Fluent look is fine.
- You have an established XAML codebase and design system you're happy with.
- You need a control Loam doesn't provide and don't want the extra dependency for it.

You can also mix: Loam composes Avalonia primitives, so dropping a raw Avalonia control into a Loam
screen (or vice versa) works. Loam shrinks the *mental* gap of building application UI; it does not
replace Avalonia.

## Next

- [Getting Started](./getting-started) — install, register the theme, first screen.
- [Theming](./theming) — palettes, light/dark, Material You, runtime recoloring.
- [Authoring UI in C#](./csharp-ui) — the code-only patterns Loam uses.
