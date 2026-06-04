# Findings — Foundations research (2026-06-02)

Source-verified facts gathered during Phase 0 planning. Re-verify against the pinned package
versions in Phase 1.

## Tooling / environment

- Installed .NET SDKs: `10.0.100-rc.2`, `10.0.300`, `11.0.100-preview.4`. Default `dotnet` →
  11.0.100-preview.4. (`dotnet --list-sdks`, 2026-06-02.)
- Repo is **not** a git repository yet; only local workspace notes are present in
  `D:\Projects\Avalonia`.

## Avalonia

- Latest stable **Avalonia 12.0.4**; **12.1** released 2026-05-06. Target the 12.x line (ADR-0004).
  - Source: https://www.nuget.org/packages/avalonia , https://github.com/AvaloniaUI/Avalonia/releases
- v12 highlights: large layout/FPS perf gains, rebuilt Android backend, OSS WebView in-box,
  improved accessibility (keyboard nav, screen-reader/semantic peers across Win/macOS/Linux/iOS/
  Android), 125% more docs vs 11.x. Source: https://avaloniaui.net/avalonia
- ⚠️ Some older local guidance references v11 docs/paths — prefer v12 docs/source.

## Pure-C# control + ControlTheme feasibility (validates ADR-0002)

Confirmed C# APIs (Avalonia docs):

- Templated control: `class X : TemplatedControl`; properties via
  `AvaloniaProperty.Register<X, T>(nameof(P), default)` → `StyledProperty<T>`.
- Template parts: `OnApplyTemplate(TemplateAppliedEventArgs e)` → `e.NameScope.Find<T>("PART_…")`.
- Pseudo-classes: `PseudoClasses.Set(":active", bool)`.
- Templates in code: `FuncControlTemplate<T>((control, scope) => root)` — build visual tree in C#.
  - Source: https://docs.avaloniaui.net/docs/custom-controls/templated-controls
  - API: https://api-docs.avaloniaui.net/docs/T_Avalonia_Controls_Templates_FuncControlTemplate_1
- ControlTheme in code: `new ControlTheme { TargetType = typeof(T) }`, `.Setters.Add(new Setter(
  Prop, value))`, nested `Style` objects (with selectors) in `.Children`/`.Add(...)` for
  pseudo-class states; `BasedOn` for inheritance; assign via control `Theme` property or a global
  `Style`/resource keyed by type.
  - Source: https://docs.avaloniaui.net/docs/basics/user-interface/styling/control-themes
- Theme variants: `ThemeVariant.Light/Dark`, `ResourceDictionary.ThemeDictionaries`.
  - Source: https://docs.avaloniaui.net/docs/guides/styles-and-resources/how-to-use-theme-variants

### Open items to pin in Phase 1 (spikes)

- Exact **C# `TemplateBinding`** usage (docs show XAML `TemplateBinding`, OneWay). Candidates:
  `new TemplateBinding(Prop)` assigned via `child[!Prop]`, or `child.Bind(prop, …)` with a source.
- Exact **C# population of `ThemeDictionaries`** and dynamic-resource binding in setters
  (`new DynamicResourceExtension("key")` as a `Setter.Value`?).
- Selector construction in code for nested pseudo-class styles
  (`new Style { Selector = … }` builder vs. `Selectors.*` fluent API).

## reference (reference being mapped)

- Latest stable reference **9.5.0**; **v8.0.0** was the prior major with a migration guide. Use
  v8/v9 docs as the API reference. Sources: https://www.nuget.org/packages/reference ,
  https://github.com/reference/reference/issues/9953 , https://reference.com/docs/overview
- ⚠️ `reference.com/docs/overview` is a Blazor SPA — not scrapable via simple fetch. Use the GitHub
  source (`src/reference/Components`) and per-component doc pages for source-first verification.
- Component categories (DeepWiki + knowledge): Layout & Navigation, Form/Input (built on
  `BaseInput<T>` / `FormComponent<T,U>`), Data display (`Table`, `DataGrid`),
  Feedback/overlay, Pickers, Utilities/Providers, Charts. Full catalog → `component-inventory.md`.

## Prior art (study, don't necessarily depend on)

- **Material.Avalonia** (AvaloniaCommunity) — Material Design styles for ~all Avalonia controls +
  extra controls (Snackbar, side sheets, FAB, cards, dialogs). Theme via `MaterialTheme`
  (`BaseTheme`, `PrimaryColor`, `SecondaryColor`), design+runtime palette, separate Material icons
  pack. Validates our extra-control list and runtime-palette approach.
  - Source: https://github.com/AvaloniaCommunity/Material.Avalonia (NuGet 3.15.1)
- **Semi.Avalonia** — ControlThemes + style `Classes` for built-in controls; good reference for
  the ControlTheme + class-based variant pattern. Source: https://docs.irihi.tech/semi/
- Useful difference: those map *Material/Semi* design onto Avalonia's native control APIs. **Loam's
  differentiator** is mapping the *reference API surface* (param names, services), not just a look.
