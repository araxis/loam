# Loam — Development Plan

> **Loam** is a Material-Design **Avalonia** control library that mirrors the **reference**
> component API and look-and-feel, authored **entirely in C#** (no XAML). It helps Blazor and
> Avalonia developers build cross-platform apps (desktop, mobile, browser) with a familiar API.

This is the master roadmap. It is **phased**, **vertical-slice**, and each phase has an explicit
**Definition of Done (DoD)** and **exit gate**. Decisions live in [`memory/decisions`](memory/decisions);
the live component tracker is [`memory/component-inventory.md`](memory/component-inventory.md);
progress is logged in [`memory/progress/progress-log.md`](memory/progress/progress-log.md).

---

## 1. Locked decisions (summary)

| # | Decision | ADR |
| --- | --- | --- |
| Name | **Loam** (original, brand-safe) | [0001](memory/decisions/0001-project-name-loam.md) |
| Authoring | **Pure C#**, no XAML; thin fluent template helpers allowed | [0002](memory/decisions/0002-pure-csharp-no-xaml.md) |
| Fidelity | **Balanced**: reference API params + approximate Material look | [0003](memory/decisions/0003-balanced-fidelity.md) |
| Targets | Avalonia **12.x**; lib `net8.0`; cross-platform | [0004](memory/decisions/0004-target-frameworks.md) |
| Theming | **`LoamTheme`** mirroring `Theme`, projected to Avalonia resources | [0005](memory/decisions/0005-theming-engine-loamtheme.md) |
| Icons | `Icon` core + generated `Loam.Icons` package (decide Phase 3) | [0006](memory/decisions/0006-icon-system.md) |
| Naming | `Loam.Controls.*` (drop legacy prefixes), keep reference param names | [0007](memory/decisions/0007-api-naming-mapping.md) |

## 2. Engineering principles (apply to every slice)

- **Vertical slices.** Each component is delivered end-to-end (control + properties + C#
  ControlTheme for all variants/states + light/dark + a11y + tests + gallery demo + mapping note)
  before moving on. No half-built components spanning phases.
- **SOLID / KISS / SRP.** Small, composable, single-purpose types. Behavior in the control;
  visuals in the theme; tokens in the theme model. No god-classes, no service-locator, no
  reflection magic.
- **Source-first.** Verify reference params (GitHub source, v8/v9 tag) and Avalonia APIs (v12
  docs/source) before coding. Record non-obvious findings in `memory/`.
- **Pure C#, readable.** Official Avalonia code-only APIs first; thin fluent helpers only to remove
  real repetition, never to hide concepts. No third-party fluent-markup dependency.
- **Performance by default.** Minimal visual trees, virtualization-aware data controls, no
  per-frame allocations in templates/render, compiled/observable bindings, lazy overlays.
- **Customizable by design.** Everything themeable via `LoamTheme` tokens + per-instance
  `StyledProperty` params + `Class`/`Style` escape hatches. No hard-coded colors/sizes in controls.
- **Accessible.** Keyboard operable, focus-visible, automation peers/names, color is never the only
  signal, respects reduced-motion, RTL-aware.
- **Tested.** xUnit + `Avalonia.Headless` for control behavior/binding/state; unit tests for theme
  resolution, validation, mappers. Build is warnings-as-errors and green before a slice is "done".

## 3. Repository / solution structure (target)

```
Avalonia/                      # repo root (folder name legacy; product = Loam)
  Loam.sln
  Directory.Build.props           # shared MSBuild: nullable, langversion, analyzers, warnings-as-errors
  Directory.Packages.props        # central package versions (Avalonia 12.0.x pinned)
  .editorconfig  .gitignore  LICENSE  README.md  DEVELOPMENT_PLAN.md
  src/
    Loam/                         # the control library (net8.0, no platform code, no XAML)
      Theming/                    # LoamTheme, Palette, Typography, Shadows, tokens, LoamColor
      Controls/                   # one folder per component family (vertical slices)
        Buttons/  Inputs/  Layout/  Overlays/  DataDisplay/  Navigation/ ...
      Internal/                   # fluent template helpers, ripple, overlay layer, shared bits
    Loam.Icons/                   # generated Material symbol geometries (optional package)
  samples/
    Loam.Gallery/                 # Avalonia app = the "ControlCatalog": one page per component
  tests/
    Loam.Tests/                   # xUnit + Avalonia.Headless.XUnit
  memory/                         # project memory (decisions, findings, progress, learnings, tracker)
```

Each component family folder is flat and self-contained: `Button.cs`, `ButtonThemes.cs`,
`ButtonEnums.cs`, `ButtonTests.cs` (or co-located in `tests/`), demo page in the gallery.

## 4. The "Per-Component Definition of Done" (the slice checklist)

A component is **Done** only when ALL hold:

1. **Control** inherits the right base (`TemplatedControl`/`ContentControl`/`ItemsControl`/…); UI
   state in the control, none in view models.
2. **Properties** mirror reference params (verified against source) as `StyledProperty<T>` /
   `DirectProperty<T>`; sensible defaults; XML-doc each public member.
3. **ControlTheme (C#)** covers every `Variant` × `Color` × `Size` and every interactive state
   (`:pointerover`, `:pressed`, `:disabled`, `:focus-visible`, `:checked`/`:selected`, error) in
   **both** light and dark, consuming `LoamTheme` tokens (no literals).
4. **Behavior** complete: keyboard, focus, ripple where applicable, validation/error where
   applicable, command/selection semantics.
5. **Accessibility**: automation peer/name, keyboard operability, focus-visible, reduced-motion,
   RTL sanity.
6. **Tests** (headless): renders, key params change visuals/behavior, two-way binding/commands,
   states. Green.
7. **Gallery page**: live demo exercising variants/colors/sizes/states + a code sample.
8. **Mapping note**: `component-inventory.md` row → ✅ with any divergence documented.
9. **Build**: warnings-as-errors clean; public API XML-doc'd.

---

## 5. Phased roadmap

Each phase: **Goal → Scope → Key tasks → Definition of Done / exit gate.** Phases are sequential
because later ones depend on the theming + primitives foundation, but components *within* a phase
can be parallelized as independent slices.

### Phase 0 — Discovery & Planning ✅ (complete 2026-06-02)
- **Goal:** Lock decisions, verify foundations, produce this plan + the memory system.
- **DoD:** ADR-0001…0007 accepted; foundations researched & recorded; `DEVELOPMENT_PLAN.md` +
  `memory/` in place; component inventory drafted. ✅

### Phase 1 — Solution Foundation & Tooling
- **Goal:** A building, testing, CI-backed skeleton that proves the **pure-C# theming pipeline**
  end-to-end on one trivial control.
- **Scope:** repo + solution + projects + build hygiene + CI + one smoke control.
- **Key tasks:**
  1. `git init`, `.gitignore`, `LICENSE` (MIT proposed), root `README`.
  2. `Loam.sln`; projects `src/Loam`, `samples/Loam.Gallery`, `tests/Loam.Tests`.
  3. `Directory.Build.props` (nullable, `LangVersion=latest`, implicit usings, analyzers,
     `TreatWarningsAsErrors=true`), `Directory.Packages.props` (pin Avalonia 12.0.x), `.editorconfig`.
  4. Gallery app shell (Avalonia desktop) that boots with `LoamTheme` (even if near-empty).
  5. **Spikes** (record in `findings/`): C# `TemplateBinding` pattern; populating
     `ThemeDictionaries` in code; dynamic-resource `Setter` values; selector construction for
     nested pseudo-class styles.
  6. Author the **fluent template helper** seed (`Internal/Templating`).
  7. **Smoke control** `Surface` (a themable `Paper`-lite) built purely in C#, themed via a C#
     `ControlTheme`, shown in the gallery, with a headless render test.
  8. CI (GitHub Actions): `dotnet build` + `dotnet test` on push/PR.
- **DoD / exit gate:** `dotnet build` and `dotnet test` green locally and in CI; gallery launches
  and renders the smoke control in **both** light and dark via a C#-authored ControlTheme; spikes
  documented. No XAML anywhere.

### Phase 2 — Design System & Theming Engine (backbone)
- **Goal:** Ship `LoamTheme` — the foundation every component consumes.
- **Scope:** `Palette` (light/dark), `Typography`, `Shadows` (elevation 0–24), `LayoutProperties`,
  `ZIndex`, token→resource projection, `ThemeVariant` integration, runtime swap, `LoamColor`
  derivations, spacing scale.
- **Key tasks:** model types mirroring `Theme`; project tokens into
  `ResourceDictionary.ThemeDictionaries[Light/Dark]` under a stable key namespace
  (`Loam.Palette.*`, `Loam.Typography.*`, `Loam.Elevation.*`); dynamic-resource consumption so
  runtime palette/variant swaps re-style live; typography as reusable text styles; default theme
  matching the reference API's default palette.
- **DoD / exit gate:** gallery "Theme" page toggles light/dark and edits the primary color at
  runtime with the whole app re-theming; all tokens resolve; headless tests for palette resolution
  + variant switching + runtime swap; documented token catalog.

### Phase 3 — Core Primitives
- **Goal:** The building blocks most other components reuse.
- **Scope:** `Text`, `Icon` (+ icon decision, ADR-0006), `Button`, `IconButton`, `ButtonGroup`,
  `ToggleIconButton`, `Fab`, `Paper`, `Card` (+ parts), `Divider`, `Chip`/`ChipSet`, `Badge`,
  `Avatar`/`AvatarGroup`, reusable `Ripple`, elevation/shadow application.
- **DoD / exit gate:** every listed control passes the §4 checklist; gallery "Buttons/Surfaces"
  pages demo all variants×colors×sizes×states in light/dark; ripple + elevation working; tests green.

### Phase 4 — Layout & App Shell
- **Goal:** Build real app shells.
- **Scope:** `Container`, responsive `Grid`/`Item` (12-col), `Stack`, `Spacer`, `Hidden`,
  `IBreakpointService` (xs–xxl), `Layout`, `AppBar`, `Drawer` (+ header/container, responsive/mini),
  `MainContent`, `ScrollToTop`.
- **DoD / exit gate:** a responsive shell demo (app bar + collapsible/mini drawer + content) adapts
  correctly across window sizes/breakpoints; breakpoint service + responsive grid have headless
  tests; §4 met for each control.

### Phase 5 — Forms & Inputs
- **Goal:** Complete, validated data entry.
- **Scope:** `Form` + validation engine (`INotifyDataErrorInfo`, sync/async rules mirroring
  `Form`), `Field` base (adornments/label/helper/error), `TextField`, `NumericField`, `Select`/
  `SelectItem`, `Autocomplete`, `CheckBox` (tri-state), `Switch`, `RadioGroup`/`Radio`, `Slider`,
  `Rating`, `ToggleGroup`/`ToggleItem`, `FileUpload` (via Avalonia `StorageProvider`).
- **DoD / exit gate:** a validated form demo (required/regex/async, Text/Filled/Outlined variants,
  all states) works; two-way binding + validation covered by headless/unit tests; §4 met.

### Phase 6 — Overlays & Feedback
- **Goal:** Modal & transient UX with reference-shaped services.
- **Scope:** overlay layer foundation, `Popover`, `Overlay`, `Tooltip`, `Menu`/`MenuItem`,
  `Dialog` + `IDialogService` (`ShowAsync<TDialog>()`, typed `DialogResult`) + provider,
  `MessageBox`, `ISnackbar` (queue/severity/action) + provider, `Alert`, `ProgressCircular`,
  `ProgressLinear`, `Skeleton`, `Collapse`.
- **DoD / exit gate:** dialog returns a typed result and supports cancel/escape/destructive-confirm;
  snackbar queues and dismisses; popovers/menus/tooltips position correctly across edges; service
  flows covered by headless tests; §4 met.

### Phase 7 — Data Display
- **Goal:** Lists, tables, and rich containers at scale.
- **Scope:** `List`/`ListItem`/`ListSubheader`, `SimpleTable`, `Table` (sort/page/select,
  server+client), `DataGrid` (columns, sort/filter/group/edit, **virtualized**), `TreeView`,
  `ExpansionPanels`, `Tabs`/`DynamicTabs`, `Timeline`, `Carousel`, `Pagination`.
- **DoD / exit gate:** `Table` and `DataGrid` demos handle ~10k rows smoothly (sort/filter/page/
  select) with virtualization; sort/filter/paging logic unit-tested; §4 met. Record perf numbers
  in `findings/`.

### Phase 8 — Navigation & Pickers
- **Goal:** Navigation surfaces + value pickers.
- **Scope:** `NavMenu`/`NavLink`/`NavGroup`, `Breadcrumbs`, `Link`, `Stepper` (linear/non-linear),
  `DatePicker`, `TimePicker`, `DateRangePicker`, `ColorPicker` (shared `Picker` base).
- **DoD / exit gate:** pickers produce correct values with full keyboard nav; stepper enforces
  linear/non-linear flow; tests for value logic + keyboard; §4 met.

### Phase 9 — Charts (stretch / optional `Loam.Charts`)
- **Goal:** `Chart` parity (Line/Bar/Pie/Donut/StackedBar/TimeSeries), custom-drawn in C#.
- **Scope/DoD:** separate package; deferred to post-1.0 unless pulled forward. Marked ⏸️ in the
  inventory.

### Phase 10 — Hardening, Docs, Packaging, 1.0
- **Goal:** Ship a credible 1.0.
- **Scope/tasks:** accessibility audit (keyboard/screen-reader/automation peers, RTL, reduced
  motion); theme-variant + token audit; performance pass (allocation/render budgets, profiling);
  **public API review & freeze**; complete XML docs; NuGet packaging (`Loam`, `Loam.Icons`, optional
  `Loam.Charts`) with SourceLink, symbols, README, license, **SemVer**; gallery polished to cover
  every component; **reference→Loam migration guide** + mapping table; release CI pipeline.
- **DoD / exit gate:** `1.0.0` packages built (and published or release-ready); gallery covers all
  shipped components; docs + migration guide complete; CI builds, tests, and packs on tag.

---

## 6. Cross-cutting workstreams (run continuously, not a phase)

- **Theming & tokens** — owned in Phase 2, extended as components need new tokens; never hard-code.
- **Icons** — `Loam.Icons` generation + licensing (ADR-0006).
- **CSS-utility mapping** — reference utility classes (`pa-4`, `d-flex`, `elevation-N`, gutters)
  → small C# layout/utility helper extensions (documented), not literal CSS classes.
- **Accessibility** — part of every slice's DoD; final audit in Phase 10.
- **Performance** — budgets enforced per slice; profiling pass for data controls (Phase 7) and
  overall in Phase 10.
- **Testing & CI** — headless tests grow with each slice; CI gates every PR.
- **Docs** — gallery is living documentation; migration guide + token catalog finalized in Phase 10.
- **Memory upkeep** — update `progress-log.md`, `component-inventory.md`, `learnings.md`,
  `findings/` as work happens.

## 7. Risks & mitigations

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Pure-C# templates verbose | Slows authoring, hurts readability | Fluent template helpers (Phase 1); standard `PART_*`; review for clarity per slice. |
| Scope (huge component set) | Never "done" | Strict vertical slices + phase gates; charts deferred; ship value each phase. |
| Avalonia 12 API drift / unknowns | Rework | Source-first against pinned 12.0.x; spikes in Phase 1; record in `findings/`. |
| reference API edge cases | Surprise divergences | Verify each component vs v8/v9 source; document every divergence in the inventory. |
| Runtime theming complexity | Re-theme bugs | Dynamic resources + Phase 2 runtime-swap tests before building dependent controls. |
| Perf of data controls / custom drawing | Janky tables/charts | Virtualization-aware; profile in Phase 7/10; record numbers. |
| Icon data size / licensing | Bloated/unsafe package | Keep icons in `Loam.Icons`; confirm license before shipping. |

## 8. Definition of "project success" (1.0)

A reference developer can build a polished, accessible, cross-platform Avalonia app using Loam with
a familiar API; the library is **pure C#**, **highly themeable** (`LoamTheme`, light/dark, runtime
swap), **SOLID**, **performant**, **tested**, **documented**, and shipped as versioned NuGet
packages — with every reference→Loam mapping decision traceable in `memory/`.
