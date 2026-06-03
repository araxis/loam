# ADR-0007 — API naming & MudBlazor mapping convention

- **Status:** Accepted
- **Date:** 2026-06-02
- **Deciders:** engineering

## Context

MudBlazor prefixes every component with `Mud` (`MudButton`, `MudTextField`). Loam is an
independent brand (ADR-0001) but targets balanced fidelity (ADR-0003) so the API should feel
familiar without copying the brand.

## Decision

- **Control type names:** drop the `Mud` prefix; use the clean Material/control noun in the
  `Loam.Controls` namespace. `MudButton → Loam.Controls.Button`, `MudTextField → TextField`,
  `MudDataGrid → DataGrid`, `MudDialog → Dialog`. (Where a name clashes with a built-in Avalonia
  control the namespace disambiguates; consumers alias if needed.)
- **Parameter names:** **keep MudBlazor's** where they translate (`Variant`, `Color`, `Size`,
  `Dense`, `Elevation`, `Disabled`, `Outlined`, `Square`, `Ripple`, `Class`, `Style`, …) so muscle
  memory transfers. Implement these as Avalonia `StyledProperty<T>` for binding/styling.
- **Enums:** mirror MudBlazor enums (`Variant.Filled/Outlined/Text`, `Color.Primary/Secondary/…`,
  `Size.Small/Medium/Large`) under `Loam` namespaces.
- **Services:** mirror MudBlazor service surfaces — `IDialogService.ShowAsync<TDialog>()`,
  `ISnackbar.Add(...)` — adapted to Avalonia's overlay/window model.
- **Divergences are documented**, never silent: every component row in `component-inventory.md`
  notes any param it omits, renames, or adds, plus the reason.

## Consequences

- ✅ Familiar params + clean, brand-safe type names.
- ✅ `StyledProperty` gives binding, styling, and theming for free.
- ⚠️ `Color`/`Size`/`Variant` names may collide with `System.*`/Avalonia types in some files —
  resolve with namespaces/usings, and prefer explicit `Loam.Color` in public signatures where
  ambiguity is likely.

## Mapping table

The authoritative per-component mapping lives in
[`../component-inventory.md`](../component-inventory.md).
