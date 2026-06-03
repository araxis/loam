# ADR-0005 — Theming engine: LoamTheme

- **Status:** Accepted
- **Date:** 2026-06-02
- **Deciders:** engineering

## Context

MudBlazor's theming centers on a `MudTheme` object: `PaletteLight`, `PaletteDark`, `Typography`,
`Shadows`, `LayoutProperties`, `ZIndex`, consumed app-wide via `MudThemeProvider`. Avalonia themes
its controls through `Styles`/`ControlTheme`, `ThemeVariant` (Light/Dark), resource dictionaries,
and `ThemeDictionaries`. We must bridge these so MudBlazor devs feel at home while staying
idiomatic and runtime-customizable in Avalonia.

## Decision

Ship a **`LoamTheme`** as the theming backbone, mirroring MudBlazor's model:

- `LoamTheme` is a `Styles`-derived object the consumer adds to `Application.Styles` (one line of
  setup, the Loam analogue of `MudThemeProvider`). It carries the control themes for all Loam
  controls plus the design-token resources.
- It exposes a strongly-typed **theme model**: `Palette` (Light + Dark), `Typography`, `Shadows`
  (elevation 0–24), `LayoutProperties` (default border radius, drawer width, app-bar height, …),
  `ZIndex`. Property names track MudBlazor's where reasonable.
- The model is **projected into Avalonia resources** under a stable key namespace
  (e.g., `Loam.Palette.Primary`, `Loam.Typography.H6`, `Loam.Elevation.4`) and split across
  `ThemeDictionaries[ThemeVariant.Light/Dark]` so light/dark switch natively.
- Control themes consume tokens via **dynamic resources** so a **runtime** theme/palette swap
  re-styles the whole app without rebuilding the visual tree.
- A `LoamColor`/palette helper provides Material-correct derivations (hover/ripple/disabled
  opacities, text-on-color contrast) so a consumer can set one primary color and get a full ramp.

## Consequences

- ✅ Familiar `MudTheme`-shaped customization + native Avalonia light/dark + runtime swap.
- ✅ Controls reference tokens, not literals — consistent, themeable, single source of truth.
- ⚠️ Must confirm the exact C# API for `ThemeDictionaries` population and dynamic-resource binding
  in code (Phase 2 spike; record in `findings/`).
- This is the **first real slice after scaffolding** because every component depends on it.

## Alternatives considered

- **Avalonia-native only (`ThemeVariant` + raw resources, no Mud-shaped model)** — idiomatic but
  unfamiliar to MudBlazor devs; loses the `MudTheme` mental model. Rejected for the balanced goal.
