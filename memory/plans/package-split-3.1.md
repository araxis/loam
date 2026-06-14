# Loam 3.1 — Package split implementation plan

Implements **ADR-0009** (`memory/decisions/0009-v3-package-split.md`). Extract three satellite NuGet
packages — **`Loam.Charts`**, **`Loam.Pickers`**, **`Loam.Data`** — from the single `Loam` package so
apps pull only what they use. Namespaces stay `Loam.Controls` everywhere (no consumer `using` churn).
**Core depends on no satellite; each satellite depends only on core.**

Investigation that grounds this plan: workflow `wf_60d02fea-843` (4 read-only probes — theme coupling,
control inventory, packaging/CI, consumer impact). File/line references below are from that sweep.

## Decisions (resolved by the investigation)

1. **Registration = per-satellite `Styles` subclass.** Each satellite ships one
   `public sealed class Loam{Charts,Pickers,Data} : Styles` whose ctor registers its own `ControlTheme`s
   (`Resources[typeof(X)] = XTheme.Create()`). Consumers add them next to `LoamTheme`:
   ```csharp
   Styles.Add(new LoamTheme());
   Styles.Add(new LoamPickers());   // only if you use pickers
   Styles.Add(new LoamData());      // only if you use the data controls
   // LoamCharts: ship for symmetry; it registers no ControlThemes today (charts self-render).
   ```
   Pure C#, no reflection (trim/AOT-safe), tokens shared automatically via Avalonia resource resolution
   up `Application.Styles`. **Rejected:** reflection auto-discovery (defeats decoupling, AOT-hostile) and
   a `LoamTheme.AddPickers()` in core (core must not name satellite types — ADR-0009).

2. **Tokens stay 100% in core `LoamTheme`.** Satellites contribute ControlThemes only and resolve
   `Loam.*` tokens by key. Runtime `SetData`/`SetSeed`/`SetDensity` on core re-styles satellite controls
   for free (DynamicResource). Do **not** split tokens.

3. **Internal helpers → `InternalsVisibleTo`.** Satellites use core internals (`PopupSurface`,
   `InteractionAssist`, `FieldChrome`, `Loam.Internal.Templating.TemplateScope.Named`, `AnonObserver`,
   `DualContent`/`SemanticColor`). Add `[InternalsVisibleTo("Loam.Charts/Pickers/Data")]` in core (same
   pattern already used for `Loam.Tests`) rather than widening core's public API permanently.

4. **Single version source.** Move version to `Directory.Build.props` (`<VersionPrefix>3.1.0</VersionPrefix>`)
   and delete the per-csproj `<Version>` (currently `Loam.csproj:12`). The tag-derived `/p:PackageVersion`
   in `package.yml` already flows to every packable project, keeping all four in lockstep. Hoist shared
   packaging metadata (license/icon/readme/repo URLs + the `<None>` README/icon includes) into
   `Directory.Build.props` under `Condition="'$(IsPackable)' == 'true'"`; keep `PackageId`/`Description`/
   `PackageTags`/`PackageReleaseNotes` per project.

## Control → package mapping (inventory confirms clean boundaries)

- **Loam.Charts** — `Controls/Charts/Charts.cs` (Charts helpers, `ChartBase`, `PieChart`/`BarChart`/`LineChart`), `ChartLegend.cs`. No ControlThemes (self-render). Easiest extraction.
- **Loam.Pickers** — `DatePicker`, `TimePicker`, `ColorPicker`, `DateRangePicker` (+ their `*Theme.cs`), `MonthCalendar` (no theme).
- **Loam.Data** — `DataGrid<T>`/`DataGridColumn<T>`/`DataGrids` (self-render `Decorator`, no theme), `SimpleTable` (+theme), `TreeView`/`TreeViewItem` (+themes), `Pagination` (+theme).
- **Core (Loam)** — everything else, **including the `DataDisplay` controls that STAY**: `Tabs`, `Stepper`, `Timeline`, `Carousel`, `ExpansionPanel(s)`. ⚠️ Split `DataDisplay` **per-type, not per-folder** (the folder mixes data-package and core controls).

No control in one satellite references another satellite. Only intra-package edges exist
(`DataGrid → Pagination`; `DatePicker`/`DateRangePicker → MonthCalendar`). The only forbidden edge today
is the theme registration below.

## The one real blocker: core → satellite theme coupling

`LoamTheme.RegisterControlThemes()` (`src/Loam/Theming/LoamTheme.cs:331-381`) registers satellite control
themes via `Resources[typeof(DatePicker)] = DatePickerTheme.Create()` — **8 lines** naming satellite types
(pickers `356-359`; data `370,372,373,374`). Moving those types out breaks core compilation.
**Invert this FIRST**, before any file move: remove the 8 lines from core; the satellites' `Styles`
subclasses register them instead. Also drop the redundant `ChartBase` fallback
`Application.Current.Styles.OfType<LoamTheme>()` (`Charts.cs:279`) — the normal `TryGetResource` tree-walk
already resolves tokens, and removing it deletes the only satellite→core *type* reference from Charts.

## Phased steps

**Phase A — decouple in place (still one project; stays green, fully reversible):**
1. Add `[InternalsVisibleTo]` for the 3 satellite assemblies in core.
2. Create `LoamPickers`/`LoamData`/`LoamCharts : Styles` registrars (still in the core project for now);
   move the 8 `typeof` registrations out of `RegisterControlThemes` into `LoamPickers`/`LoamData`.
3. Gallery `samples/Loam.Gallery/App.cs` + `tests/Loam.Tests/TestApp.cs`: add
   `Styles.Add(new LoamPickers()); Styles.Add(new LoamData());`.
4. Drop the `OfType<LoamTheme>()` chart fallback (`Charts.cs:279`).
5. Build 0/0, full suite green. *(Registration decentralized; still one assembly — low risk.)*

**Phase B — physical split:**
6. Create `src/Loam.Charts/`, `src/Loam.Pickers/`, `src/Loam.Data/` csproj (net8.0, `<PackageReference Include="Avalonia"/>` via CPM, `ProjectReference` → `src/Loam`, packaging metadata). Add to `Loam.slnx`.
7. Move the mapped control + `*Theme` + registrar files into each satellite (namespaces unchanged).
8. Gallery + tests add `ProjectReference` to the 3 satellites.
9. Hoist packaging metadata + version into `Directory.Build.props`; delete per-csproj `<Version>`.
10. Build 0/0, full suite green.

**Phase C — packaging/CI + docs + release:**
11. `package.yml` + `ci.yml`: pack the **solution** (or each packable project) instead of only `src/Loam`;
    the publish loop already pushes every `*.nupkg` with `--skip-duplicate`. Ensure the satellite→core
    NuGet dependency version equals the shared version.
12. CI guard test: core assembly no longer contains the moved types; each satellite `Styles` registers its
    expected `typeof()` keys; `LoamTheme.SetData()` does not clear separately-registered `Resources[typeof(...)]`.
13. Docs: promote the migration-guide "package split" row from _Planned_ to a real step (per-control table);
    fix getting-started's "one `LoamTheme` registers everything" claim; tag `charts.md`/`pickers.md`/
    `data-display.md`/`overview.md` with their package; add a `3.1.0` changelog section.
14. Tag `v3.1.0` → CI publishes all four packages.

## Back-compat / messaging (call out loudly)

This is a **breaking packaging change** despite identical namespaces:
- A 3.0 consumer who referenced only `Loam` and used a picker/data/chart control gets **`CS0246`** after
  upgrading until they add the satellite package. There is no `[Obsolete]`/source bridge for a moved
  assembly.
- If they add the package but forget `Styles.Add(new Loam…())`, the control instantiates but renders
  **un-themed** (no compile error). Document the symptom.

Mitigate with a prominent migration section + per-control table ("used `DatePicker` → add `Loam.Pickers`
+ `Styles.Add(new LoamPickers())`") and a `3.1` changelog entry.

## Definition of Done

- All 4 packages build 0/0 (`TreatWarningsAsErrors`), full suite green, **core has no satellite reference**
  (CI-guarded).
- `dotnet pack` emits 4 nupkgs with correct cross-deps + one shared version; `v3.1.0` tag publishes all four.
- Migration guide + getting-started + component docs updated; `3.1` changelog shipped.

## Rough size

~Phase A: small (a few files, no moves). Phase B: mechanical file moves + 3 csproj + slnx/refs.
Phase C: CI/docs. The risky thinking (registration design, internal visibility) is resolved here, so
execution is largely mechanical and well-gated.
