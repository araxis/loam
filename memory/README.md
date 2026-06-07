# Loam — Project Memory

This folder is the **single source of truth** for *why* Loam is built the way it is, *what*
has been done, *what was learned*, and *what is still open*. It is written for maintainers
picking the project up cold. Read this file first.

> Loam is an **Avalonia** control library with a familiar component API and a role-based visual
> system, authored **entirely in C#** (no XAML). It lets developers build cross-platform Avalonia
> apps with a compact, themeable API.

## How to use this memory

- **Before making a decision**, check `decisions/` for an existing ADR. If you reverse one, add a
  new ADR that supersedes it (don't silently edit history).
- **When you learn something non-obvious** (an Avalonia/reference gotcha, a perf trap, a working
  pattern), append it to `learnings/learnings.md`.
- **When you research and verify a fact** (version, API shape, behavior), record it under
  `findings/` with the date and source URL. We are *source-first*: never guess when exact
  behavior matters.
- **After finishing any unit of work**, append to `progress/progress-log.md` and update the
  status column in `component-inventory.md`.
- Keep entries **small, dated, and trackable**. One concern per entry.

## Folder map

| Path | Purpose |
| --- | --- |
| `decisions/` | Architecture Decision Records (ADRs). Numbered, immutable once `Accepted`. |
| `findings/` | Dated, source-cited research results and verified facts. |
| `learnings/` | Running log of gotchas, patterns, and experiences. |
| `progress/` | Chronological progress log + phase status board. |
| `component-inventory.md` | The full reference→Loam component catalog with target phase + status. The master tracker. |
| `../DEVELOPMENT_PLAN.md` | The phased roadmap with per-phase Definition of Done. |

## Index of decisions

- [ADR-0001 — Project name: Loam](decisions/0001-project-name-loam.md)
- [ADR-0002 — Pure C# authoring, no XAML](decisions/0002-pure-csharp-no-xaml.md)
- [ADR-0003 — Balanced fidelity (API + look)](decisions/0003-balanced-fidelity.md)
- [ADR-0004 — Target frameworks & Avalonia 12](decisions/0004-target-frameworks.md)
- [ADR-0005 — Theming engine: LoamTheme](decisions/0005-theming-engine-loamtheme.md)
- [ADR-0006 — Icon system](decisions/0006-icon-system.md)
- [ADR-0007 — API naming & reference mapping convention](decisions/0007-api-naming-mapping.md)
- [ADR-0008 — v3 naming & Avalonia collision strategy](decisions/0008-v3-naming-collision-strategy.md)
- [ADR-0009 — v3 package split (lean core + satellites)](decisions/0009-v3-package-split.md)
- [ADR-0010 — v3 versioning & deprecation policy](decisions/0010-v3-versioning-deprecation-policy.md)
- [ADR-0011 — Bridging base Fluent chrome to Loam tokens (Phase 1)](decisions/0011-fluent-base-chrome-bridge.md)
- [ADR-0012 — Material You seed → scheme generation (Phase 2)](decisions/0012-material-you-seed-scheme.md)
- [ADR-0013 — Table strategy: one recommended path (Phase 4)](decisions/0013-table-strategy.md)

## Current status (update me)

- **Phase:** **v3 (vNext) — Phase 4 in progress** (branch `work/vnext`, version
  `3.0.0-preview.1`). Phases 0 (scaffold), 1 (theme consistency), and 2 (theming power) are done. The
  v3 roadmap and its driving review live at the repo root in [`PLAN.md`](../PLAN.md) and
  [`REVIEW.md`](../REVIEW.md).
- **Latest:** **Phase 2 (theming power) complete.** Material You seed→scheme generator (ADR-0012;
  CIELAB tonal palettes, accessible by construction), a live gallery seed picker, a one-call density
  switch (`LoamDensity.Compact` + `SetDensity`), and a high-contrast variant (`LoamContrast`). Runtime
  theming API: `SetSeed`/`SetPrimary`/`SetPalette`/`SetDensity`/`SetData`. Builds clean; **401 tests
  pass**; demo visually confirmed light + dark.
- **Phase 3 core done:** `AppBar.CustomActions`, generated-vs-custom content precedence (+ debug-only
  `DualContent` warning), and a global-usings collision aid (docs). Optional remainder: a Roslyn
  rename/collision analyzer.
- **Phase 4 so far:** deprecated `Stack`→`StackPanel` (`LOAM0003`); table strategy decided
  (ADR-0013: `DataGrid<T>` recommended); added `NavigationRail` and `BottomNavigation` (M3). **411 tests.**
- **Next:** Phase 4 remaining — add `CommandPalette` (additive),
  then the package split into `Loam.Charts`/`Loam.Pickers`/`Loam.Data` satellites (ADR-0009).
  (Optional: CAM16/HCT upgrade; wire/remove the orphaned `DesignSystemView`/`LayoutView`.)
- **Last updated:** 2026-06-07
