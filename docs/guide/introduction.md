---
title: Introduction
---

# Introduction

**Loam** gives [Avalonia](https://avaloniaui.net) applications a complete Material Design control
set with a familiar, compact API — written entirely in C#, with **no XAML**.

## Why Loam?

Avalonia is a superb cross-platform UI framework. Loam adds a Material Design component layer with
the compact vocabulary teams expect in application UI: `Variant`, `Color`, `Dense`, `Elevation`, and
similar knobs.

- **Familiar API.** Component parameters use predictable names and consistent semantics.
- **Material look.** Colors, elevation/shadows, ripple, and typography all resolve from a central
  theme, matching Material Design.
- **Pure C# authoring.** Controls, `ControlTheme`s, templates and bindings are built with Avalonia's
  code-only APIs — no `.axaml`. This keeps the whole UI in one language and one toolchain.
- **Self-contained.** The pickers (date / time / color) and the month calendar are custom-built, so a
  LoamTheme-only application doesn't need to pull in additional control packages.

## What Loam is *not*

- **Not a drop-in Razor port.** You still build Avalonia views and use Avalonia layout. Loam shrinks
  the *mental* gap, not the framework gap.
- **Not a framework wrapper.** Loam is an independent control library built directly on Avalonia.

## Status

Loam is **v1 component-complete**: every component on the v1 component catalog is mapped — built,
themed, registered, tested, and demonstrated in the gallery. A handful of per-component enhancements
(e.g. DataGrid filtering/grouping, picker clock-face/HSV editing, stacked/time-series charts, and some
animations) are documented as deliberate post-v1 follow-ups.

The library targets **Avalonia 12** on **.NET 8**, with **xUnit + Avalonia.Headless** behavior tests.

## How the docs are organized

- **[Getting Started](./getting-started)** — install, register the theme, build your first screen.
- **[Theming](./theming)** — palettes, light/dark, runtime recoloring.
- **[Authoring UI in C#](./csharp-ui)** — the code-only patterns Loam uses and that you'll use too.
- **[Components](/components/overview)** — every control, grouped, with properties and C# examples.
