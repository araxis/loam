# Loam — Project Memory

This folder is the **single source of truth** for *why* Loam is built the way it is, *what*
has been done, *what was learned*, and *what is still open*. It is written for maintainers
picking the project up cold. Read this file first.

> Loam is a Material-Design **Avalonia** control library that mirrors the **reference** component
> API and look-and-feel, authored **entirely in C#** (no XAML). It lets reference/Blazor
> developers build cross-platform Avalonia apps with a familiar API.

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

## Current status (update me)

- **Phase:** v1 catalog component-complete. Remaining work is polish, deeper feature variants, and release-by-release hardening.
- **Latest:** `Field` is now a standalone custom-content field shell, field-style input chrome is shared, and the gallery uses side-menu component pages.
- **Last updated:** 2026-06-04
