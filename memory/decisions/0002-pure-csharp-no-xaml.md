# ADR-0002 — Pure C# authoring, no XAML

- **Status:** Accepted
- **Date:** 2026-06-02
- **Deciders:** Project owner

## Context

Avalonia control/theme libraries (Fluent, Semi.Avalonia, Material.Avalonia) ship their
ControlThemes as `.axaml` resource dictionaries — the ecosystem norm. The project owner's
standing rule is **C#-only UI, no XAML**.

We verified (2026-06-02, see `findings/2026-06-02-foundations-research.md`) that Avalonia 12
fully supports authoring controls *and* ControlThemes in C#:

- `new ControlTheme { TargetType = typeof(T) }` with `.Setters.Add(new Setter(prop, value))`.
- Nested `Style` objects with selectors for pseudo-classes (`:pointerover`, `:pressed`,
  `:disabled`, `:checked`, …).
- `FuncControlTemplate<T>((control, scope) => …)` to build the visual tree in code.
- `OnApplyTemplate(TemplateAppliedEventArgs e)` + `e.NameScope.Find<T>("PART_…")`.
- `PseudoClasses.Set(":active", bool)` to drive state.
- `ThemeVariant` + `ResourceDictionary.ThemeDictionaries` for Light/Dark.

## Decision

**All** Loam controls, ControlThemes, templates, styles, and theme resources are authored in
**C#**. No `.axaml`/XAML files in the shipped library or the sample gallery.

To keep complex Material templates readable, we add **thin, project-local fluent template
builder helpers** (e.g., `Template<T>(...)`, `.WithChild(...)`, `.TemplateBind(...)`,
`.WithSetter(...)`, pseudo-class style helpers). These wrap official Avalonia APIs only — they
must never hide property/binding/template concepts, and we do **not** take a third-party fluent
markup dependency.

## Consequences

- ✅ Honors the owner's C#-only rule; everything refactors/greps/tests with normal C# tooling.
- ✅ Themes are composable in code (loops, factories) — useful for generating color/size variants.
- ⚠️ Material templates are more verbose than XAML. Mitigation: the fluent template helpers and a
  shared template-part naming convention (`PART_*`).
- ⚠️ Some docs/samples are XAML-only; we translate them to C# (XAML→C# rules in the skill).
- 🔎 `TemplateBinding` in code needs a confirmed pattern — to be pinned in Phase 1 (candidates:
  `new TemplateBinding(Prop)` assigned via `child[!Prop]`, or `child.Bind(prop, …)`).

## Alternatives considered

- **Hybrid (C# logic + XAML themes)** — most conventional/maintainable for Material, but violates
  the C#-only rule. Rejected by owner.
- **XAML-first** — least aligned with owner preference. Rejected.
