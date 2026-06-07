# ADR-0011 — Bridging base Fluent chrome to Loam tokens (Phase 1)

- **Status:** Accepted
- **Date:** 2026-06-07
- **Deciders:** engineering

## Context

A Loam app layers `FluentTheme` then `LoamTheme` (see `samples/.../App.cs`). `LoamTheme` only
registers `ControlTheme`s for Loam's own types, so every base Avalonia control with no Loam theme
(`ScrollBar`, `ToolTip`, `ContextMenu`/`MenuFlyout`, `Window`, text selection, `Expander`, and any
stray Fluent control) falls back to **Fluent** visuals — most visibly Fluent blue accent and grey
scrollbars. This is the headline gap `REVIEW.md`/`PLAN.md` Phase 1 targets: an app should read as
Material *everywhere*, not just on Loam controls.

We need a way to recolor base chrome from `LoamTheme` without (yet) reimplementing every base control
theme, and without owning the app's `FluentTheme` instance.

## Decision

Bridge Loam tokens onto the **resource keys** Fluent's base controls already bind, from inside
`LoamTheme`'s per-variant theme dictionaries. Implemented as a dedicated
`Loam.Theming.FluentBridge` static helper, applied once per variant from
`LoamTheme.BuildVariantDictionary` (so it swaps with the variant and at runtime via
`SetPrimary`/`SetPalette`/`SetData`).

Key rules:

1. **Override brush keys, not just colors.** Fluent's accent brushes are
   `Color="{DynamicResource SystemAccentColor}"`, and that nested lookup resolves inside FluentTheme's
   own scope — so overriding the *color* alone does **not** cascade. We override the **brush** keys the
   controls bind (e.g. `SystemControlHighlightAccentBrush`, `ScrollBarThumbFill*`,
   `MenuFlyoutItemForeground`), which controls resolve from their own scope where `LoamTheme` (layered
   after Fluent) wins. (We *also* set the `SystemAccentColor*` colors for
   `ColorPaletteResources`/direct consumers.) See
   `findings/2026-06-02-…` and `findings/2026-06-07-fluent-accent-bridge.md`.
2. **Colors only — leave geometry to Fluent.** We bridge brushes/colors; size/thickness/padding/corner
   keys stay Fluent's, so base control metrics and behavior are untouched.
3. **Material role mapping:** accent → primary; scrollbars → neutral on-surface (not accent); tooltips
   → inverse-surface; menus/flyouts → surface-container + on-surface; window → background; text
   selection → primary; expander → tonal container ramp + outline-variant.
4. **Source-verified & version-coupled.** Every key set is read from Avalonia **12.0.4** source
   (`gh api` at tag `12.0.4`) and pinned by ADR-0004; each bridge notes the file it was verified
   against. A version bump requires re-verifying the keys.
5. **Tested** via `FluentBridgeTests`: per-variant projection plus end-to-end resolution through the
   live `TestApp` (FluentTheme under LoamTheme), which also guards the override ordering.

## Consequences

- ✅ Stray base controls read as Material in light & dark with no per-control theme work.
- ✅ Self-contained in `LoamTheme`; no need to own/configure the app's `FluentTheme`.
- ✅ Runtime accent/palette swaps recolor base chrome too.
- ⚠️ **Version-coupled**: the keys are Avalonia-Fluent internals; an Avalonia upgrade can rename/retire
   them. Mitigated by source-verification + tests, and ultimately retired when Loam themes the base
   chrome itself and drops FluentTheme (Phase 3+, per the `App.cs` TODO).
- ⚠️ Colors only: controls whose *metrics* are un-Material (e.g. scrollbar width) still look Fluent in
   shape. Acceptable for Phase 1.
- A few opacities (scrollbar thumb 0.45/0.70/0.72, text selection 0.4) are visual defaults, confirmed
   acceptable in the gallery (light & dark) on 2026-06-07.

## Alternatives considered

- **`ColorPaletteResources.Accent` on the FluentTheme instance** — the idiomatic accent override, and
  it cascades cleanly, but it requires owning/configuring the app's `FluentTheme` (changes the
  one-line setup contract) and only covers accent, not scrollbars/menus/etc. Rejected for now; may
  revisit for the accent piece.
- **Full Loam `ControlTheme`s for every base control** — the eventual Phase 3+ goal (drop FluentTheme),
  but far too large for Phase 1 and duplicates Fluent's templates/behavior.
