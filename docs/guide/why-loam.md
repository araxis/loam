---
title: Why Loam vs plain Avalonia
---

# Why Loam vs plain Avalonia

[Avalonia](https://avaloniaui.net) already ships a capable control set and the `FluentTheme`. So what
does Loam add, and when should you *not* reach for it? This page is the honest version: the gap Loam
fills, the same screen written both ways, a feature-by-feature comparison, and the cases where plain
Avalonia is the better call.

Loam is not a replacement for Avalonia and not a framework you adopt wholesale. It is a *design layer*
written in pure C# that sits on top of Avalonia's primitives — a token-driven theme plus a vocabulary
of controls (`Card`, `TextField`, `DataGrid<T>`, the pickers, the overlay services) that already
encode the decisions you would otherwise re-make in every project.

::: tip Mental model
Avalonia gives you the *materials* — `Button`, `TextBox`, `Border`, panels, brushes. Loam gives you the
*finished parts* — a `Card` that already knows its elevation, a `TextField` that already has a label,
helper text, and error state, a `DataGrid<T>` that already sorts, groups, and paginates. You still write
C#; you just stop hand-tuning hex values and re-deriving form chrome. Everything resolves from theme
tokens, so a finished screen restyles itself when the variant flips or the seed color changes.
:::

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

### What disappears

It is worth naming what the Loam version *doesn't* have to say. The plain version hard-codes a surface
color, a corner radius, an elevation shadow, a font size, and a font weight — five decisions that have
to stay consistent across every screen and stay correct in both light and dark. The Loam version
expresses the same screen as intent: *this is a card at elevation 2, this is H6 text, this is the
primary filled action*. The "how" lives in the theme, once.

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

::: details Why "no provider component" matters
Many UI libraries make you mount a host element (a dialog provider, a snackbar provider) high in your
visual tree before overlays work. Loam's `DialogService` and `SnackbarService` instead render onto
Avalonia's built-in window overlay layer, so you create one from any attached control with
`DialogService.For(this)` / `SnackbarService.For(this)` and call it — there is nothing to register and
nothing to forget to mount. See [Overlays](../components/overlays) for the full surface.
:::

## Choosing: Loam, plain Avalonia, or both

Loam is additive, so the choice is rarely all-or-nothing. Use this to decide per screen:

| Situation | Reach for |
| --- | --- |
| Building application UI — forms, cards, tables, dialogs, theming that flips light/dark | Loam controls end to end |
| You need exactly one stock control and the Fluent look is fine | Plain Avalonia |
| A Loam screen needs a control Loam doesn't provide | Drop the raw Avalonia control into the Loam layout |
| An existing XAML app you're happy with, adding one new themed screen | Mix — Loam composes Avalonia, so they nest both ways |
| A library/control you ship to others who shouldn't inherit your theme | Plain Avalonia |

## When to use plain Avalonia instead

Loam is additive, not a lock-in — so skip it when it doesn't earn its place:

- You only need a handful of stock controls and the Fluent look is fine.
- You have an established XAML codebase and design system you're happy with.
- You need a control Loam doesn't provide and don't want the extra dependency for it.

You can also mix: Loam composes Avalonia primitives, so dropping a raw Avalonia control into a Loam
screen (or vice versa) works. Loam shrinks the *mental* gap of building application UI; it does not
replace Avalonia.

::: warning Loam owns the theme
The thing that makes Loam coherent — a base-chrome bridge that retints stray scrollbars, tooltips,
menus, and window chrome to the Loam palette — is global to the app. That is exactly what you want for
an application, but it means Loam is a poor fit for a *reusable control* you ship to consumers who have
their own theme. For that case, build on plain Avalonia.
:::

## Recipe: the "invite teammate" card, finished

The side-by-side card above is the skeleton. Here it is wired up the way you'd actually ship it —
validating the email, firing a primary action, and confirming with a toast. Everything below is plain
C# using only Loam controls and the window overlay service; no provider component is mounted anywhere.

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;

// `owner` is any Control already attached to the window (e.g. your page root).
StackPanel BuildInviteCard(Control owner)
{
    var email = new TextField
    {
        Label = "Email",
        Variant = Variant.Outlined,
        Required = true,
    };

    var send = new Button
    {
        Content = "Send invite",
        Variant = Variant.Filled,
        Color = LoamColor.Primary,
        StartIcon = Icons.Material.Filled.Add,
    };

    send.Click += (_, _) =>
    {
        // TextField.Validate() runs Required/Validation and flips Error + ErrorText for you.
        if (email.Validate() is not null)
        {
            return;
        }

        SnackbarService.For(owner).Add($"Invite sent to {email.Text}", LoamColor.Success);
        email.Text = null;
    };

    return new StackPanel
    {
        Spacing = 8,
        Children =
        {
            new Card
            {
                Elevation = 2,
                Content = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        new Text { Text = "Invite teammate", Typo = Typo.H6 },
                        email,
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Children = { send },
                        },
                    },
                },
            },
        },
    };
}
```

The same screen in plain Avalonia would also need: a hand-rolled label that floats on focus, an error
label that appears below the field, the brush math to color the border red on error, and a toast host
mounted somewhere in the tree. Loam's `TextField.Validate()` and `SnackbarService.For(...)` collapse all
of that.

## Accessibility & keyboard

Loam composes Avalonia's input primitives, so the keyboard and assistive-technology behavior you'd
expect from plain Avalonia is preserved — Loam mostly adds *names* on top of it:

- **Focus & activation** — Loam buttons subclass Avalonia's `Button`, so they sit in the tab order and
  respond to <kbd>Space</kbd>/<kbd>Enter</kbd>; `TextField` wraps a real `TextBox`, so caret movement,
  selection, and editing keys all work unchanged.
- **Generated content is named** — surfaces like `Card`/`Paper` and the snackbar set automation names
  from their title/message text, so screen readers announce them without extra work. `Text` mirrors its
  content to its automation name.
- **High contrast is a theme flip, not a rewrite** — `SetSeed(seed, LoamContrast.High)` regenerates the
  whole scheme toward stronger separation, so an entire Loam screen gets a low-vision variant for free.
  Reproducing that on raw Avalonia means a second hand-built palette.
- **Overlays are dismissible** — snackbars close on <kbd>Esc</kbd>; dialogs resolve through a
  `DialogInstance` handle. See [Overlays](../components/overlays) for the keyboard contract.

::: tip Name your icon-only controls
The one accessibility gap Loam can't close for you is an icon with no words. As in plain Avalonia, set
`AutomationProperties.SetName(control, "…")` on an `IconButton` or any glyph-only affordance so it isn't
announced as an empty button.
:::

## Next

- [Getting Started](./getting-started) — install, register the theme, first screen.
- [Theming](./theming) — palettes, light/dark, Material You, runtime recoloring.
- [Authoring UI in C#](./csharp-ui) — the code-only patterns Loam uses.

## See also

- [Components overview](../components/overview) — the full control catalog and the shared `Variant`/`Color`/`Size` knobs.
- [Buttons & menus](../components/buttons) — the button family used in the recipe above.
- [Form inputs](../components/inputs) — `Field`/`TextField`/`Select`/`Form` and validation states.
- [Overlays](../components/overlays) — `DialogService` / `SnackbarService` on the window overlay layer.
- [Data display](../components/data-display) — `DataGrid<T>` with grouping, frozen columns, and aggregates.
