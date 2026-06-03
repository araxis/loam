# ADR-0003 — Balanced fidelity (API + look)

- **Status:** Accepted
- **Date:** 2026-06-02
- **Deciders:** Project owner

## Context

"Mapping" MudBlazor can prioritize (a) API familiarity, (b) pixel-perfect Material visuals, or
(c) a balance. MudBlazor is web (HTML/CSS); Avalonia renders via Skia with a different layout and
styling model, so 1:1 pixel parity is costly and sometimes impossible.

## Decision

**Balanced fidelity.** Loam:

1. **Mirrors MudBlazor's public API shape** — component names, and the key parameters Blazor devs
   know: `Variant`, `Color`, `Size`, `Dense`, `Elevation`, `Disabled`, `Square`, `Outlined`,
   `Class`, `Style`, plus per-component params — so the mental model transfers.
2. **Approximates the Material Design look** MudBlazor produces (elevation/shadows, ripple,
   typography scale, palette-driven colors, rounded corners), *good enough* to feel like the same
   design language, without committing to pixel parity.
3. **Diverges deliberately** where Avalonia's model is better served otherwise; every divergence is
   recorded in the MudBlazor→Loam mapping (`component-inventory.md`) so it is discoverable.

## Consequences

- ✅ Low friction for MudBlazor developers; visually clearly "Material".
- ✅ Freedom to use idiomatic Avalonia constructs (e.g., `ItemsSource`, `ControlTemplate`) under a
  MudBlazor-flavored surface.
- ⚠️ Not a drop-in port — apps still need rewriting from Razor to Avalonia views; Loam reduces the
  *cognitive* gap, not the porting work.
- Per-component, we document: "MudBlazor param → Loam property" and any intentional gaps.

## Alternatives considered

- **API-parity first** — risks awkward, web-shaped APIs in a desktop framework.
- **Visual-fidelity first** — high cost chasing CSS pixels; lower payoff than API familiarity.
