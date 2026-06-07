---
layout: home

hero:
  name: Loam
  text: Themed controls for Avalonia
  tagline: Pure C# controls with runtime theming and no XAML.
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: Components
      link: /components/overview
    - theme: alt
      text: Why Loam?
      link: /guide/introduction

features:
  - icon: 🧩
    title: Familiar API
    details: Predictable names for variant, color, size, density, and elevation.
  - icon: 🎨
    title: Polished look
    details: Role-based colors, elevation shadows, a click ripple and a full typography scale — all token-driven.
  - icon: 💜
    title: Pure C#
    details: Controls, ControlThemes and templates authored entirely in C#. No .axaml files anywhere.
  - icon: 📦
    title: Self-contained
    details: Custom calendar, time and color pickers — a LoamTheme-only app needs no extra control packages.
  - icon: 🌗
    title: Themeable
    details: A LoamTheme data model with light/dark variants and runtime palette swapping.
  - icon: 🖥️
    title: Cross-platform
    details: One library targeting Avalonia 12 — desktop, mobile and browser.
---

<div style="max-width: 760px; margin: 2.5rem auto 0; padding: 0 1.5rem;">

## A familiar component, the Avalonia way

```csharp
using Loam;
using Loam.Controls;

var card = new Card
{
    Content = new StackPanel
    {
        Children =
        {
            new CardHeader { Title = "Project Loam", Subtitle = "Updated today" },
            new CardContent
            {
                Child = new Text { Text = "Reference-style components, mapped to Avalonia in pure C#." },
            },
            new CardActions
            {
                Child = new Button { Content = "Learn more", Variant = Variant.Text, Color = LoamColor.Primary },
            },
        },
    },
};
```

Every component in the current catalog is built, themed, tested, and demonstrated
in the live gallery. The full solution builds clean in Release with **376 headless
and unit tests passing**.

</div>
