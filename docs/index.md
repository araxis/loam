---
layout: home

hero:
  name: Loam
  text: MudBlazor, mapped to Avalonia
  tagline: Material-Design controls with a familiar API — authored in pure C#, no XAML.
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
    details: Parameters mirror MudBlazor — Variant, Color, Size, Dense, Elevation. If you know MudBlazor, you already know Loam.
  - icon: 🎨
    title: Material look
    details: Palette-driven colors, elevation shadows, a click ripple and a full typography scale — all token-driven.
  - icon: 💜
    title: Pure C#
    details: Controls, ControlThemes and templates authored entirely in C#. No .axaml files anywhere.
  - icon: 📦
    title: Self-contained
    details: Custom calendar, time and color pickers — a LoamTheme-only app needs no extra control packages.
  - icon: 🌗
    title: Themeable
    details: A LoamTheme (mirroring MudTheme) with light/dark variants and runtime palette swapping.
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
                Child = new Text { Text = "MudBlazor components, mapped to Avalonia in pure C#." },
            },
            new CardActions
            {
                Child = new Button { Content = "Learn more", Variant = Variant.Text, Color = LoamColor.Primary },
            },
        },
    },
};
```

Every MudBlazor component on the master inventory is mapped — built, themed, tested, and demonstrated
in the live gallery. The full solution builds clean (Debug + Release, 0 warnings) with **111 headless
and unit tests passing**.

> Loam is an independent, MudBlazor-*inspired* library and is **not affiliated** with the MudBlazor
> project. "MudBlazor" is a trademark of its respective owners.

</div>
