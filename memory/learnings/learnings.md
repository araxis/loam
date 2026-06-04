# Learnings — gotchas, patterns, experiences

Append as we discover things. Keep each entry short: **what**, **why it matters**, **how to apply**.
Link related ADRs/findings. Newest on top.

---

## 2026-06-03 — Never construct platform objects at ControlTheme-build time

**What:** `ListItemTheme`/`NavLinkTheme` created `new Cursor(StandardCursorType.Hand)` as `ControlTheme`
Setter values. The `Cursor` ctor resolves `ICursorFactory` immediately, so `new LoamTheme()` (which
builds every theme) needed the Avalonia platform. Plain `[Fact]` tests (e.g. `ThemingTests`) construct
`new LoamTheme()` with **no** headless app, so they threw `Unable to locate 'ICursorFactory'` — but only
when no prior `[AvaloniaFact]` had initialized the platform process-wide, making it an **order-dependent,
flaky** failure.

**Why it matters:** Theme `Create()` runs eagerly at app/theme construction, often before (or without) a
platform. Anything platform-backed (`Cursor`, native brushes, bitmaps) constructed there couples theme
creation to platform availability and breaks headless/data tests unpredictably.

**How to apply:** Build platform objects **inside the template factory lambda** (`FuncControlTemplate`),
which only runs when a control is realized under a live app — e.g. set `border.Cursor = new Cursor(...)`
in the template, not as a `Setter`. Reuse a single static cursor when many cells need one
(`MonthCalendar.HandCursor`). General rule: `*Theme.Create()` should produce only inert
data/templates/bindings, never live platform resources.

## 2026-06-03 — Input gotchas: ToggleSwitch knob parts, Path vs System.IO.Path

**What:** (1) Subclassing Avalonia `ToggleSwitch` with a custom template throws during measure — it
expects built-in knob parts. Subclass **`ToggleButton`** (what CheckBox/RadioButton derive from) for
a clean toggle with `:checked` and no part expectations. (2) `Avalonia.Controls.Shapes.Path` is
ambiguous with `System.IO.Path` (ImplicitUsings) → alias `using AvaPath = ...Shapes.Path;`. (3)
`Loam.Controls.CheckBox` collides with `Avalonia.Controls.CheckBox` — qualify in mixed files.

**How to apply:** Base custom toggles on `ToggleButton`; alias `Path`; qualify `CheckBox`.

**Also:** Avalonia 12 renamed `TextBox.Watermark` → **`PlaceholderText`** (`WatermarkProperty` is
`[Obsolete]`, which is a build error under warnings-as-errors).

---

## 2026-06-03 — CA1716: `Select` is a reserved keyword (VB)

**What:** Naming a public type `Select` (mirroring `Select`) trips **CA1716** — `Select` is a
reserved keyword in VB.NET (LINQ-ish), so analyzers flag it for cross-language consumers. Same class
of issue as CA1711 (`Stack` suffix). Other reference names to watch: any that map to VB keywords.

**How to apply:** Keep the reference-parity name and suppress on the type with
`[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "...")]`.
Document the parity intent in the justification.

## 2026-06-02 — Layout panel gotchas: Grid name clash, no AffectsParentMeasure, CA1711 Stack

**What:** (1) `Loam.Controls.Grid` collides with `Avalonia.Controls.Grid` in files importing both
(gallery/tests) — qualify `Avalonia.Controls.Grid`/`Loam.Controls.Grid`. Inside `Loam.Controls`
files the local Grid wins, no clash. (2) There is **no `AffectsParentMeasure`** — use
`AffectsMeasure<Item>(spanProps)`; invalidating a child's measure bubbles up and re-runs the parent
panel's `MeasureOverride`. (3) `class Stack` trips **CA1711** (reserved suffix) → suppress with
`[SuppressMessage("Naming","CA1711",...)]` (intentional reference parity).

**How to apply:** Qualify `Grid` in non-`Loam.Controls` files; use `AffectsMeasure` on child layout
props; suppress CA1711 on `Stack`. **Update (2026-06-03):** *inside* `Loam.Controls` (e.g.
`SimpleTable`), bare `Grid` resolves to `Loam.Controls.Grid`, so to use the layout `Grid` —
including the static `Grid.SetRow/SetColumn/SetColumnSpan` attached-property setters — add
`using AvaGrid = Avalonia.Controls.Grid;` and call `AvaGrid.SetRow(...)`.

---

## 2026-06-02 — A property can't share its type's name; Avalonia.Controls extensions vs Loam.Button

**What:** (1) `Icon.Icon` is CS0542 (member name == enclosing type) → the `Icon` control's path
property is `Data`. (2) The generic `INameScope.Find<T>` and `GetResourceObservable` live in the
`Avalonia.Controls` namespace; button-family files can't `using Avalonia.Controls;` (it makes
`Button` ambiguous with `Loam.Controls.Button`). So use `Find("PART_x") as T` and the qualified
`Avalonia.Controls.ResourceNodeExtensions.GetResourceObservable(this, key)` (or a `using AC =
Avalonia.Controls;` alias for types, which does NOT import extension methods).

**Why it matters:** Recurs for every Loam control that subclasses an Avalonia control of the same
name (Button) or is named after a common property (Icon).

**How to apply:** Name path props `Data` on `Icon`; keep `StartIcon`/`EndIcon`/`IconButton.Icon`
(no clash). In button-family files prefer the `AC` alias + non-generic `Find ... as T`.

---

## 2026-06-02 — Declarative control themes work in C# (PropertyEquals + DynamicResource)

**What:** Avalonia 12 lets us build the full variant×color×state matrix declaratively in C#:
`ControlTheme.Add(new Style(x => x.Nesting().PropertyEquals(P, value).Class(":pointerover")) {
Setters = { new Setter(prop, new DynamicResourceExtension { ResourceKey = key }) } })`. Proven by
the Button tests. Order (not specificity) decides winners — add disabled last.

**Why it matters:** This is the scalable pattern for every interactive Loam control; loops generate
the matrix. Full recipe in `findings/2026-06-02-avalonia-controltheme-csharp.md`.

**How to apply:** Prefer this for stateful controls (Button, Chip, inputs). Use imperative
`GetResourceObservable` binding only where a control reacts to its *own* changing property
(Paper elevation, Text typo).

---

## 2026-06-02 — Name clashes: Loam.Button vs Avalonia.Button; LoamColor/LoamSize

**What:** `Loam.Controls.Button` collides with `Avalonia.Controls.Button` wherever both namespaces
are imported (CS0104). The enums `LoamColor`/`LoamSize` are deliberately prefixed to dodge
`Avalonia.Media.Color`/`Avalonia.Size`.

**How to apply:** Qualify (`Loam.Controls.Button`) or alias (`using LoamButton = ...`) in gallery/
tests. Keep control property names un-prefixed (`Color`, `Size`) for reference familiarity.

---

## 2026-06-02 — Test token projection via theme.Resources, not control.TryGetResource

**What:** `control.TryGetResource(key, variant, out v)` on a Border in a shown headless window
returned `false` for Loam tokens, even though the same tokens resolve fine through a control's
`GetResourceObservable` (proven by SurfaceThemeTests). Querying the `LoamTheme.Resources`
dictionary directly with `TryGetResource(key, variant, out v)` works reliably.

**Why it matters:** Picking the wrong probe makes projection tests flakily "fail".

**How to apply:** Unit-test projection against `new LoamTheme().Resources` directly; reserve
control-tree resolution for one end-to-end test (SurfaceThemeTests already covers it).

---

## 2026-06-02 — Static field init order bit us (LoamShadows)

**What:** `public static X Default { get; } = new(Css...)` where `Css` is declared *below* it →
`Css` is null when `Default` initializes (CS8604 + a real latent NRE).

**Why it matters:** Field initializers run in textual order.

**How to apply:** Initialize order-dependent statics in a **static constructor** (runs after all
field initializers regardless of textual position), or declare the dependency first.

---

## 2026-06-02 — Record color prop vs static factory name clash

**What:** `LoamPalette` has a `Dark` color property (reference parity) and we tried a static `Dark`
factory → CS0102. Renamed factories to `DefaultLight`/`DefaultDark`.

**How to apply:** Name palette static factories `Default*` to avoid clashing with color properties
like `Dark`/`Surface`.

---

## 2026-06-02 — Avalonia 12 Headless.XUnit needs xunit v3 + an Exe test project

**What:** `Avalonia.Headless.XUnit` 12.0.4 depends on `xunit.v3.extensibility.core` 3.2.2. Pairing it
with xunit v2 gives "No test is available" (tests not discovered). xunit v3 test projects must also
set `<OutputType>Exe</OutputType>`.

**Why it matters:** Cost an hour-class debugging trap; affects every future test project.

**How to apply:** Use `xunit.v3` + `xunit.runner.visualstudio` 3.x + `Microsoft.NET.Test.Sdk`, and
`<OutputType>Exe</OutputType>`. Keep these in `Directory.Packages.props`.

---

## 2026-06-02 — Dynamic resources in code-only templates = GetResourceObservable

**What:** To make a templated control's visual react to theme/variant changes from C#, bind via
`element.Bind(Prop, control.GetResourceObservable("Token.Key"))`. Confirmed reactive to runtime
`RequestedThemeVariant` swaps by a passing test.

**Why it matters:** This is the backbone of Loam's runtime theming; avoids fragile attempts to use
`DynamicResourceExtension` as a Setter value in code.

**How to apply:** All control themes bind token-driven props with `GetResourceObservable`. For
templated-parent (non-token) props use `control.GetObservable(SourceProperty)`.

---

## 2026-06-02 — SDK 10 `dotnet new sln` emits `.slnx`; strict analyzers are errors

**What:** `dotnet new sln` now produces an XML `.slnx` (use `dotnet sln Loam.slnx add`). With
`TreatWarningsAsErrors` + `AnalysisLevel=latest-recommended`, CA rules fail the build (CA1859
concrete return types, CA1707/CA1822 in tests).

**Why it matters:** Build hygiene is intentionally strict; know which rules to comply with vs scope-off.

**How to apply:** Comply in library/app code (return concrete types from private builders); `NoWarn`
CA1707/CA1822 only in test projects (xunit naming/instance conventions).

---

## 2026-06-02 — Custom ContentControl subclass needs StyleKeyOverride

**What:** A control deriving from `ContentControl` (e.g., `Surface`) must
`protected override Type StyleKeyOverride => typeof(Surface);` or it resolves the base ContentControl
theme instead of Loam's theme keyed by its own type.

**Why it matters:** Every Loam templated control that subclasses a built-in needs this, or theming
silently falls back.

**How to apply:** Add `StyleKeyOverride` to every Loam templated control; register its theme as
`Resources[typeof(TheControl)] = theme`.

---

## 2026-06-02 — Pure-C# ControlTheme is viable but verbose; build fluent template helpers

**What:** Avalonia 12 can express controls *and* ControlThemes/templates entirely in C#
(`ControlTheme`, `Setter`, nested `Style` selectors, `FuncControlTemplate<T>`, `OnApplyTemplate` +
`NameScope.Find`, `PseudoClasses.Set`). See `findings/2026-06-02-foundations-research.md`.

**Why it matters:** Validates ADR-0002. But raw C# templates get noisy fast for Material visuals.

**How to apply:** Introduce thin, project-local fluent helpers (template/builder extensions) early
in Phase 1, wrapping official APIs only — no third-party fluent-markup package, no hidden concepts.
Standardize `PART_*` template-part names.

---

## 2026-06-02 — Verify against Avalonia 12, not the skill's v11 references

**What:** Latest Avalonia is 12.x; the avalonia-csharp-ui-senior skill cites v11 docs/paths.

**Why it matters:** API/source details can differ between majors.

**How to apply:** When checking exact APIs, use v12 docs/source and the pinned package version.

---

## 2026-06-02 — reference docs site is a SPA; use GitHub source for source-first checks

**What:** `reference.com/docs/*` renders client-side and won't yield content to a plain fetch.

**Why it matters:** We must verify reference component params source-first per ADR-0007.

**How to apply:** Read `reference/reference` `src/reference/Components/**` on GitHub (matching the
v8/v9 tag) plus per-component doc pages rendered in a real browser when needed.
