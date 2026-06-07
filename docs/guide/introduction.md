---
title: Introduction
---

# Introduction

**Loam** gives [Avalonia](https://avaloniaui.net) applications a complete themed control
set with a familiar, compact API — written entirely in C#, with **no XAML**.

## Why Loam?

Avalonia is a superb cross-platform UI framework. Loam adds a component layer with
the compact vocabulary teams expect in application UI: `Variant`, `Color`, `Dense`, `Elevation`, and
similar knobs.

- **Familiar API.** Component parameters use predictable names and consistent semantics.
- **Polished look.** Role-based colors, elevation/shadows, ripple, and typography all resolve from
  central theme tokens.
- **Pure C# authoring.** Controls, `ControlTheme`s, templates and bindings are built with Avalonia's
  code-only APIs — no `.axaml`. This keeps the whole UI in one language and one toolchain.
- **Self-contained.** The pickers (date / time / color) and the month calendar are custom-built, so a
  LoamTheme-only application doesn't need to pull in additional control packages.

## What Loam is *not*

- **Not a drop-in Razor port.** You still build Avalonia views and use Avalonia layout. Loam shrinks
  the *mental* gap, not the framework gap.
- **Not a framework wrapper.** Loam is an independent control library built directly on Avalonia.

## Status

Loam is in its **v2.0 rebaseline**: every component in the current catalog is built, themed,
registered, tested, and demonstrated in the gallery. Remaining future enhancements, such as deeper
DataGrid grouping/editing, picker clock-face/HSV editing, and stacked/time-series charts, are tracked
separately from the release baseline.

The library targets **Avalonia 12** on **.NET 8**, with **xUnit + Avalonia.Headless** behavior tests.

## How the docs are organized

- **[Getting Started](./getting-started)** — install, register the theme, build your first screen.
- **[Theming](./theming)** — palettes, light/dark, runtime recoloring.
- **[Authoring UI in C#](./csharp-ui)** — the code-only patterns Loam uses and that you'll use too.
- **[Components](/components/overview)** — every control, grouped, with properties and C# examples.
