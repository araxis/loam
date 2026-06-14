# ADR-0008 — v3 naming & Avalonia collision strategy

- **Status:** Accepted
- **Date:** 2026-06-07
- **Deciders:** engineering
- **Supersedes/extends:** refines [ADR-0007](0007-api-naming-mapping.md) for v3.

## Context

ADR-0007 dropped the reference prefix and put clean control nouns in `Loam.Controls`. In real-app
integration (see root `REVIEW.md`) the **single biggest day-to-day friction** was type-name collisions
with `Avalonia.Controls`: `Grid`, `Button`, `Text`, `Menu`, `Card`, `Carousel`, `TreeView`,
`DatePicker`, `TimePicker` all shadow Avalonia types, forcing a `using LoamX = …` alias or full
qualification in every file that also touches Avalonia.

A key insight: not all collisions are equal.

- **Restyle collisions** (`Button`, `Text`, `Card`): Loam's type *is the same concept* as Avalonia's,
  themed. The shared name communicates "drop-in replacement," so the parallel name is at least
  defensible.
- **Concept collisions** (`Grid`): Loam's `Grid` is a **responsive 12-column reflow** — a *different
  layout algorithm* from Avalonia's fixed 2D `Grid`. The shared name provides zero drop-in benefit and
  actively misleads.

## Decision

1. **New concept → new name.** When Loam adds a capability Avalonia lacks, give it a distinct name.
   The responsive grid is renamed **`Grid` → `ResponsiveGrid`** and **`Item` → `Col`**.
   - Documented rule: *use `Avalonia.Controls.Grid` for fixed 2D layout; use Loam `ResponsiveGrid`
     for breakpoint-based reflow.*
2. **Restyle → keep the parallel name** (`Button`/`Text`/`Card`/`Menu`/pickers/etc.), accepting the
   alias trade-off, because the shared noun signals an intentional themed replacement of the same
   concept.
3. **Ease the residual restyle friction** (Phase 3, planned) with an official ergonomics aid — a
   `GlobalUsings` snippet and/or an analyzer/alias set — so consumers don't hand-alias in every file,
   rather than renaming every restyle.
4. **Deprecate, never silently rename.** Renamed types ship first as `[Obsolete]` aliases carrying a
   stable diagnostic id (`LOAMxxxx`) and a migration URL (see [ADR-0010](0010-v3-versioning-deprecation-policy.md)).
   - `LOAM0001` = `Grid` → `ResponsiveGrid`; `LOAM0002` = `Item` → `Col`.
5. **The rename map is canonical** in `docs/migration/v2-to-v3.md` and mirrored in
   `component-inventory.md`.

## Consequences

- ✅ The most confusing collision (`Grid`) is gone; new responsive-layout code needs no alias.
- ✅ Existing v2 code keeps compiling (deprecation warning, not break) for at least one preview.
- ✅ A clear, repeatable rule for future controls: new concept = new name.
- ⚠️ Restyle collisions remain by design; their friction is addressed by tooling, not renames
  (Phase 3). Until then, `Button`/`Text`/`Card`/… still need a using-alias where Avalonia is also
  referenced.
- ⚠️ Consumers with `TreatWarningsAsErrors` must `<NoWarn>` or fix the rename diagnostics.

## Alternatives considered

- **Rename every collision** (e.g. `LoamButton`, `MText`): heavy churn, abandons the "familiar noun"
  value of ADR-0007, and over-corrects for restyles where the shared name is helpful. Rejected.
- **Keep `Grid`/`Item` and rely on aliases forever:** preserves the misleading name and the
  per-file alias tax for the worst offender. Rejected.
