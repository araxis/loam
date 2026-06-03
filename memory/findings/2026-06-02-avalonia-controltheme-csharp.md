# Findings — Authoring Avalonia 12 control themes in C# (verified 2026-06-02)

Verified against Avalonia **12.0.4** source (gh API) and proven by passing Button tests. This is the
canonical recipe for Loam control themes (ADR-0002).

## Building a ControlTheme with nested styles in C#

- `new ControlTheme(typeof(MyControl)) { Setters = { ... } }` — base setters.
- Nested styles (variants/states): `controlTheme.Add(IStyle)` (StyleBase exposes
  `Children : IList<IStyle>` and `Add(IStyle)` / `Add(SetterBase)`).
- `new Style(Func<Selector?, Selector> selector) { Setters = { ... } }`.
- Selectors (extension methods in `Avalonia.Styling.Selectors`):
  - `x.Nesting()` — the `^` self-reference to the templated control (start every nested selector here).
  - `x.Nesting().PropertyEquals(MyControl.VariantProperty, Variant.Filled)` — **select on a styled
    property value** (reactive). Lets us avoid syncing style classes from enum props.
  - `x.Nesting().Class(":pointerover")` — pseudo-class state. Pseudo-classes are entries in the
    Classes collection prefixed with `:`. Avalonia Button provides `:pointerover`, `:pressed`,
    `:disabled`.
- **Order matters, not specificity.** Avalonia has no CSS specificity; among matching setters at the
  same priority the **last-added** wins. So add: base → color → hover → disabled (disabled last).

## DynamicResource in a Setter (token-driven themes)

- `DynamicResourceExtension : BindingBase`, and `Setter` handles `Value is BindingBase` by binding.
  So **`new Setter(prop, new DynamicResourceExtension { ResourceKey = key })` works in code** and
  updates on theme/variant/runtime swap. (Verified: Filled+Primary button resolves Primary fill.)
- Use this for declarative control themes. (The imperative `control.Bind(prop,
  control.GetResourceObservable(key))` is the alternative — used where a control reacts to its own
  changing property, e.g. `Paper` elevation, `Text` typo.)

## Template parts in a C# FuncControlTemplate

- `new FuncControlTemplate<MyControl>((control, scope) => root)`; the lambda runs per instance, so
  capture `control` and bind children to it: `child.Bind(P, control.GetObservable(SourceP))`.
- Name + register parts: our `TemplateScope.Named(name, scope)` → `INameScope.Register`.
- `TemplatedControl` already defines `Background`, `BorderBrush`, `BorderThickness`, `CornerRadius`,
  `Padding`, `FontSize/Family/Weight`, `Foreground` — template-bind the root `Border` to these so
  theme setters (which target the control) flow through. Only `BoxShadow` is NOT on TemplatedControl
  (handle elevation on the inner Border directly — see `Paper`).

## Gotchas

- `GetObservable`/`GetResourceObservable` are extensions in namespace **`Avalonia`** — needs
  `using Avalonia;` (missing it → CS1061).
- **`Loam.Controls.Button` vs `Avalonia.Controls.Button`** collide. In files importing both
  namespaces, qualify (`Loam.Controls.Button`) or alias (`using LoamButton = Loam.Controls.Button;`).
- A subclass of a built-in (e.g. `Button : Avalonia.Controls.Button`) must
  `protected override Type StyleKeyOverride => typeof(TheSubclass);` to resolve its own theme.
- `Text` subclasses `TextBlock` (not a TemplatedControl) so it renders itself — no ControlTheme/
  template needed; it just binds its own font/foreground to tokens.
