# Findings — Phase 1 spikes & pinned versions (2026-06-02)

All verified by building + running headless tests in this repo (not guessed).

## Pinned versions

- **SDK:** pinned to `10.0.300` via `global.json` (`rollForward: latestFeature`) to avoid the
  preview .NET 11 default. Builds net8.0 fine.
- **Avalonia:** `12.0.4` (central, `Directory.Packages.props` → `$(AvaloniaVersion)`).
- **TFMs:** `Loam` = net8.0; `Loam.Gallery` = net8.0; `Loam.Tests` = net8.0. (Gallery simplified to
  net8.0 vs ADR-0004's net9.0 — net8.0 LTS, runtime present, fully runnable. ADR-0004 updated.)
- **Test stack:** `xunit.v3` **3.2.2** + `xunit.runner.visualstudio` **3.1.5** +
  `Microsoft.NET.Test.Sdk` 17.12.0 + `Shouldly` 4.2.1.

## Verified C# patterns (validate ADR-0002 in practice)

- **Pure-C# ControlTheme works.** `new ControlTheme(typeof(Surface)) { Setters = { new Setter(
  TemplatedControl.TemplateProperty, funcTemplate), ... } }`, registered implicitly via
  `Resources[typeof(Surface)] = theme` on a `Styles`-derived `LoamTheme`.
- **Templates in code:** `new FuncControlTemplate<Surface>((surface, scope) => { ... return border; })`.
  The build lambda runs per control instance; `surface` IS that instance, so bind children to it.
- **Template name scope:** `scope.Register(name, element)` (wrapped in our `TemplateScope.Named`
  helper). Required for `ContentControl` to find `PART_ContentPresenter`.
- **Dynamic resource binding in code = `GetResourceObservable`.** Chosen mechanism:
  `border.Bind(Border.BackgroundProperty, surface.GetResourceObservable(LoamTokens.Surface))`.
  Reacts to theme-variant changes at runtime (confirmed by a passing test that swaps Light→Dark and
  re-reads the brush). This is preferred over trying to pass `DynamicResourceExtension` as a Setter
  value in code.
- **Template-parent binding in code:** `presenter.Bind(ContentPresenter.ContentProperty,
  surface.GetObservable(ContentControl.ContentProperty))` (one-way from templated parent). Avoids the
  `[!Property]` indexer operator; explicit and certain.
- **ThemeDictionaries in code:** `Resources.ThemeDictionaries[ThemeVariant.Light] = new
  ResourceDictionary { [key] = brush, ... }` (ResourceDictionary implements IThemeVariantProvider).
- **`StyleKeyOverride` matters:** a `ContentControl` subclass must
  `protected override Type StyleKeyOverride => typeof(Surface);` so it resolves Loam's theme keyed by
  its own type instead of the base ContentControl theme.
- **Runtime variant swap:** `Application.Current.RequestedThemeVariant = ThemeVariant.Dark;`
  re-resolves all token observables (confirmed in test + gallery toggle).

## Tooling gotchas (also in learnings)

- **SDK 10 `dotnet new sln` emits `.slnx`** (XML solution). Use `dotnet sln Loam.slnx add ...`.
- **`Avalonia.Headless.XUnit` 12.x uses xunit v3** (`xunit.v3.extensibility.core` 3.2.2). Mixing
  xunit v2 → "No test is available" (no discovery). Fix: xunit.v3 + runner.visualstudio 3.x.
- **xunit v3 test projects must be `<OutputType>Exe</OutputType>`** (self-executable runner).
- **`TreatWarningsAsErrors=true` + `AnalysisLevel=latest-recommended`** promotes CA rules to errors:
  CA1859 (concrete return types in private builders — complied), CA1707/CA1822 (underscore test
  names / instance test methods — `NoWarn` scoped to the test project only).

## Verification gaps

- Headless tests pass on net8.0. **Live GUI launch of `Loam.Gallery` was not run** (no display in
  this environment). To verify visually: `dotnet run --project samples/Loam.Gallery`.
