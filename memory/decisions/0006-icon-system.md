# ADR-0006 — Icon system

- **Status:** Accepted (2026-06-02, Phase 3 part 2)
- **Date:** 2026-06-02
- **Deciders:** engineering

## Context

reference exposes Material Symbols as SVG path strings via `Icons.Material.Filled.X` etc., used by
`Icon` and icon-bearing controls. Avalonia renders vector icons via `PathIcon { Data=Geometry }`
or `Image`/`DrawingImage`. We need an icon story that (a) matches the reference API's `Icon`/`@Icons…`
ergonomics, (b) stays pure-C#, (c) doesn't bloat the core package.

## Options

1. **`Loam.Icons` companion package** with Material Symbols as `StreamGeometry`/path-data
   constants, organized like reference (`Icons.Material.Filled.Home`). Pure C#, no external dep,
   but large and needs a generator.
2. **Depend on `Material.Icons.Avalonia`** (existing community pack) and wrap it behind Loam's
   `Icon` control. Less code to own, but a third-party dependency and different naming.
3. **Bring-your-own-geometry** core (`Icon` takes `Geometry`/path string), with the curated set in
   the optional `Loam.Icons` package.

## Decision (final, Phase 3 part 2)

- **`Icon` control in core** (`Loam.Controls.Icon : Control`) — custom-drawn: parses an SVG path to
  a `Geometry`, scales it from a `ViewBox` (default `0 0 24 24`) to a `Size`-driven box, fills with
  a token color. `Color.Inherit` uses the ambient `Foreground` so icons inside buttons inherit the
  button's text color.
- **The path property is `Data`, not `Icon`** — a property named `Icon` on a type named `Icon` is a
  CS0542 error. the reference API's `Icon.Icon` → Loam `Icon.Data`. (Recorded in `learnings`.) The
  button-family path props keep reference names: `Button.StartIcon/EndIcon`, `IconButton.Icon`,
  `Fab.StartIcon` (no clash there — property name ≠ type name).
- **Curated set in core**: `Loam.Icons` (static `Icons.Material.Filled.*`, ~13 common glyphs) mirrors
  the reference API's structure for ergonomics: `new Icon { Data = Icons.Material.Filled.Home }`.
- **Full pack deferred** to a separate generated `Loam.Icons` NuGet package (option 1) to keep the
  core small. Evaluate reusing `Material.Icons.Avalonia` path data then (confirm licensing).

## Consequences

- ✅ reference-style `Icons.Material.Filled.Home` ergonomics; icons inherit button color for free.
- ✅ Pure C#, no external dependency for the core set.
- ⚠️ Full glyph volume kept out of core (separate package later).
- 🔎 Confirm licensing of any reused icon path data before shipping the full pack.
