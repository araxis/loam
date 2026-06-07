# ADR-0010 — v3 versioning & deprecation policy (breaking-change budget)

- **Status:** Accepted
- **Date:** 2026-06-07
- **Deciders:** engineering

## Context

v3 ("vNext") is the first intentionally **breaking** major version (renamed responsive grid, planned
wrapper removals, planned package split). We need a predictable, non-surprising contract for how
breaks are introduced so consumers can upgrade incrementally. The repo already enforces
`TreatWarningsAsErrors=true`, so any deprecation we emit becomes a hard error for *us* and for
strict consumers — the policy has to account for that.

## Decision

**Versioning.**

- v3 development carries a **`3.0.0-preview.N`** version on the `work/vnext` branch (`Loam.csproj`
  `<Version>`), bumping `N` per preview milestone, until a stable `3.0.0`.
- v3 **may break source compatibility**; it follows SemVer at the major boundary.

**Deprecation (the breaking-change budget).** Nothing is removed without a deprecation window:

1. A renamed/removed public type or member ships first as an **`[Obsolete(…, error: false)]`** alias —
   a **warning**, not a removal.
2. Each carries a **stable diagnostic id** `LOAMxxxx` and a `UrlFormat` pointing at the migration
   guide, so the warning is greppable, suppressable, and self-documenting.
   - Registry (append-only): `LOAM0001` = `Grid`→`ResponsiveGrid`; `LOAM0002` = `Item`→`Col`.
3. The alias survives **at least one full preview** before removal.
4. Every deprecation is recorded in `docs/migration/v2-to-v3.md` (the canonical rename map) **before**
   the old name is removed.

**Build hygiene under `TreatWarningsAsErrors`.**

- Library and sample code must contain **zero** uses of its own obsolete aliases (migrate internal
  call sites when the alias is introduced) so the solution stays warning-clean.
- The **only** sanctioned internal use of an obsolete alias is a dedicated back-compat **test**, which
  scopes `#pragma warning disable LOAMxxxx` to the test body.

## Consequences

- ✅ Upgrading is incremental: build, read the `LOAMxxxx` warnings, fix at your own pace, then the
  alias eventually disappears.
- ✅ Diagnostic ids give consumers a precise `<NoWarn>`/`#pragma` handle instead of the blunt CS0618.
- ✅ The migration guide can never lag the code — it's part of the deprecation checklist.
- ⚠️ Discipline cost: introducing a rename means *also* migrating all internal call sites in the same
  change to keep the build green.
- ⚠️ Strict (`TreatWarningsAsErrors`) consumers must `<NoWarn>` the relevant `LOAMxxxx` ids while
  migrating.

## Alternatives considered

- **Hard-break renames (no aliases):** smaller library, but every rename is an immediate consumer
  break with no migration runway. Rejected.
- **Plain `[Obsolete]` without diagnostic ids:** simpler, but consumers can only suppress the generic
  `CS0618`, losing per-rename control. Rejected.
