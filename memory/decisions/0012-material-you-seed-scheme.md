# ADR-0012 — Material You seed → scheme generation (Phase 2)

- **Status:** Accepted
- **Date:** 2026-06-07
- **Deciders:** engineering

## Context

Phase 2's headline feature (PLAN/REVIEW) is "customizable": turn a single seed color into a complete,
accessible light + dark `LoamColorScheme` at runtime — Material You. v2 only had `SetPrimary`, which
crudely lightened/darkened the primary and left the rest of the scheme fixed.

Material 3's reference generator (material-color-utilities) builds tonal palettes in **HCT**
(hue/chroma/tone) on top of **CAM16**, with an `HctSolver` that inverts CAM16+tone to sRGB. Porting it
faithfully requires CAM16 forward+inverse and a solver with large lookup tables
(`CRITICAL_PLANES`, 255 entries) — impractical to transcribe accurately and hard to verify here.

## Decision

Generate tonal palettes in **CIELAB**, where **tone = CIE L\***, as a tractable, accessible
approximation of HCT:

- A `LoamTonalPalette` is a fixed **hue + chroma**; `Tone(t)` returns the sRGB color at L\* = t with
  **chroma reduced by bisection** until it fits the sRGB gamut (L\* preserved exactly).
- `LoamColorScheme.FromSeed(seed, dark)` derives six palettes from the seed
  (primary = max(seedChroma, 48); secondary 24; tertiary at hue+60, 36; neutral 6; neutral-variant 12;
  error from the fixed M3 red) and maps **every** role to the standard Material 3 tone for the variant
  (e.g. light primary = P40 / dark = P80; surfaces from the neutral ramp; etc.).
- Runtime entry points: `LoamThemeData.FromSeed(seed)` and `LoamTheme.SetSeed(seed)` (keeps
  typography/shape/spacing; regenerates both schemes + compatibility palettes). The Phase-1
  `FluentBridge` accent follows automatically.

**Why this is accessible by construction:** WCAG relative luminance equals the XYZ Y channel, and
L\* is a function of Y alone — so a color at a given tone has a fixed luminance regardless of
hue/chroma. Tone-gap contrast is therefore deterministic and identical to Material 3's, independent
of seed. Verified: 6 seeds × light/dark × 11 text pairs all clear WCAG AA (≥ 4.5).

## Consequences

- ✅ One seed → a complete, accessible light + dark scheme, swappable at runtime.
- ✅ No external dependency, no giant tables, fully unit-testable; lightness/contrast match M3 exactly.
- ✅ Builds on the existing role-based `LoamColorScheme`; the Fluent bridge and all token-bound
   controls re-theme for free.
- ⚠️ **Not bit-identical to Material You.** CIELAB hue/chroma ≠ CAM16 hue/chroma, so the exact
   saturation/hue feel differs slightly from material-color-utilities. Lightness, tone structure, and
   contrast match. A CAM16/HCT upgrade (behind the same `LoamTonalPalette`/`FromSeed` API) is a tracked
   follow-up if exact M3 fidelity is needed.
- ⚠️ The per-palette chroma constants (48/24/36/6/12) are CIELAB magnitudes, hand-tuned for vivid but
   balanced palettes; they are the knobs to revisit, not the tone mapping.

## Alternatives considered

- **Full CAM16/HCT port** — exact Material You, but the `HctSolver` lookup tables make a faithful
  hand-port impractical and unverifiable here. Deferred as a possible upgrade.
- **Keep `SetPrimary` only** — leaves the scheme mostly fixed; not "generate a scheme from a seed".
  Rejected. (`SetPrimary` stays for simple accent-only tweaks.)
