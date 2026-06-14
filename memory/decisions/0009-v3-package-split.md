# ADR-0009 — v3 package split (lean core + satellites)

- **Status:** Accepted (direction); **implementation deferred to Phase 4**
- **Date:** 2026-06-07
- **Deciders:** engineering

## Context

v2 ships everything in a single `Loam` package: theming, primitives, shell, inputs, overlays,
navigation, **plus** heavier/optional areas — charts, pickers + calendar, and data controls
(`DataGrid<T>`, `TreeView`, `SimpleTable`, `Pagination`). Many apps (LOB shells, dashboards, tools)
never touch charts or a typed data grid, yet carry their surface area and maintenance weight in the
core. `REVIEW.md` flags charts and the data layer as the least-mature areas — exactly the parts worth
isolating so the core can stay lean and stable.

## Decision

Split v3 into a **lean core** plus **optional satellite packages**, all under the `Loam.*` namespace
family:

| Package | Contents |
| --- | --- |
| **`Loam`** (core) | theming + design tokens + primitives + shell + inputs + overlays + navigation. |
| **`Loam.Charts`** | `PieChart`/`BarChart`/`LineChart` (+ donut) and the chart base. |
| **`Loam.Pickers`** | date/time/color/range pickers + `MonthCalendar`. |
| **`Loam.Data`** | `DataGrid<T>`, `TreeView`, `SimpleTable`, `Pagination`. |

Guidelines:

- **Namespaces stay `Loam.Controls`** across packages — moving a control between packages is a
  *package* change, not a *namespace* change, so `using` statements don't churn.
- **Core has no dependency on satellites;** satellites depend on core.
- **Versioned together** at first (same version stream) to keep the matrix simple; independent
  cadence can come later if needed.
- **Theming covers satellite controls** the same way it covers core (a satellite registers its own
  control themes against the shared token set).

## Consequences

- ✅ A theming/shell/inputs app pulls a smaller, more stable core.
- ✅ The weakest areas (charts, data) can iterate behind their own package boundary.
- ✅ Stable namespaces mean adding a satellite is a `PackageReference` change, not a code edit.
- ⚠️ More packaging/CI surface (4 nupkgs, pack ordering, project references).
- ⚠️ The repo must reorganize `src/` into per-package projects; the gallery references all of them.
- **Deferred:** the actual project split lands in **Phase 4** to avoid destabilizing the Phase 1
  theming work; this ADR locks the *direction* and the package boundaries now.

## Alternatives considered

- **Single package forever:** simplest to ship, but the core keeps carrying optional heavy areas and
  can't shed the least-mature surface. Rejected for v3's "lean core" goal.
- **Per-control micro-packages:** maximum modularity, unmanageable version matrix and discovery cost.
  Rejected.
