# ADR-0013 — Table strategy: one recommended path (Phase 4)

- **Status:** Accepted
- **Date:** 2026-06-07
- **Deciders:** engineering

## Context

v2 shipped two overlapping table controls (REVIEW "consolidate the table story"):

- **`DataGrid<T>`** — a typed grid with sortable headers, paging, filtering, single-row selection,
  striping/hover/dense, basic virtualization, and editable text cells.
- **`SimpleTable`** — a data-driven `Headers`/`Rows` table (string → `Text`, else hosted control)
  rendered into a `Grid` inside an elevated `Paper`, with striped/hover/bordered/dense styling.

Having two "table" answers is the confusion PLAN/REVIEW flag. We want **one recommended path**, without
throwing away the genuinely-simpler control for trivial cases.

## Decision

**`DataGrid<T>` is the recommended table API** for anything data-shaped — bound collections, sorting,
paging, filtering, selection, editing. New table guidance, docs, and samples lead with it.

**`SimpleTable` is retained as the minimal, static option** — a thin "markup-style" table for small,
fixed, non-interactive tabular content where a typed model and columns would be overkill (e.g. a
spec/comparison table built inline). It is **not** deprecated, but it is explicitly positioned as
secondary and gets no new interactive features.

Guidance (documented in `docs/components/data-display.md`):

> Reach for **`DataGrid<T>`** by default. Use **`SimpleTable`** only for a handful of static rows you'd
> otherwise hand-build with a `Grid`.

We do **not** add a third unified `Table` type — the existing `Table` entry in the inventory already
resolves to "covered by `SimpleTable` + `DataGrid<T>`", and a third surface would add, not remove,
confusion.

## Consequences

- ✅ One clear default (`DataGrid<T>`); the "which table?" question has a documented answer.
- ✅ No churn/removal: `SimpleTable` stays for the simple-static niche it serves well.
- ✅ Future data investment (grouping, inline edit, virtualization, frozen columns — Phase 5)
  concentrates on a single control.
- ⚠️ Two controls still exist; the consolidation is by *guidance*, not deletion. If `SimpleTable` use
  stays negligible, a later major version can deprecate it (its own ADR).
- When the data controls move to the **`Loam.Data`** satellite (ADR-0009), both travel together.

## Alternatives considered

- **Deprecate `SimpleTable`, force `DataGrid<T>` everywhere** — cleaner "one type", but `DataGrid<T>`
  (typed model + columns) is heavy for a 3-row static table; pushes friction onto simple cases.
  Rejected for now.
- **Build a new unified `Table`** — a third control to replace two; maximal churn, and it would just be
  one of the existing two under a new name. Rejected.
