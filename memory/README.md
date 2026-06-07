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

## Current status (update me)

- **Phase:** **v3 (vNext) — Phase 0: Decide & scaffold** (branch `work/vnext`, version
  `3.0.0-preview.1`). The v3 roadmap and its driving review live at the repo root in
  [`PLAN.md`](../PLAN.md) and [`REVIEW.md`](../REVIEW.md).
- **Latest:** locked the v3 naming/collision strategy, package split, and breaking-change policy
  (ADR-0008/0009/0010); renamed the responsive grid `Grid`→`ResponsiveGrid` and `Item`→`Col` with
  `[Obsolete]` aliases (`LOAM0001`/`LOAM0002`); scaffolded the `docs/migration/v2-to-v3.md` guide and
  rename map. Full solution builds clean; **377 headless/unit tests pass** (added a back-compat test
  for the deprecated aliases).
- **Next:** Phase 1 — theme consistency (bridge Loam tokens to base Avalonia chrome).
- **Last updated:** 2026-06-07
