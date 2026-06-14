# Loam vNext (v3) — Plan

Companion to [`REVIEW.md`](./REVIEW.md). Scope: the **Loam** Material Design 3 control library for
Avalonia (`github.com/araxis/loam`). This is a product/engineering roadmap for the next major version,
grounded in reading Loam 2.0.1's source and integrating it into the Nodely sample apps (Avalonia 12.0.4).

> **Status of these docs:** standalone deliverables written inside the Nodely repo; intended to be moved
> into the Loam repo. They are git-excluded here so they don't mix into the Nodely sample-redesign branch.

---

## Key finding that drives the plan: theming only covers Loam's own controls

Verified in `src/Loam/Theming/LoamTheme.cs`:

- `RegisterControlThemes()` registers `ControlTheme`s **only for Loam's own types**
  (`Resources[typeof(Loam.Controls.X)] = …Theme.Create()`).
- `BuildTokens()` writes **only `Loam.*`-prefixed** resource keys; it does **not** override Fluent keys
  (`SystemAccentColor*`, `ThemeBackgroundBrush`, scrollbar/selection brushes) and does **not** theme base
  Avalonia controls (`ScrollBar`, `ToolTip`, `ContextMenu`/`MenuFlyout`/`Menu`, `Window`, `Expander`,
  Avalonia `DataGrid`, native `ComboBox` popup).

**Consequence:** an app is Material on Loam controls but falls back to **Fluent** for residual chrome —
most visibly **scrollbars** (in every `ScrollViewer`, including Loam's own), **tooltips**, **context
menus**, the **Window** background, **text selection/caret/focus adorner**, and any base Avalonia control
you mix in (which shows Fluent blue, not Loam primary). Closing this gap is Phase 1.

---

## Guiding principles

1. **Own the design-system layer; lean on Avalonia** for raw primitives and 2D layout. Don't reinvent
   `Grid`, `DockPanel`, or `DataGrid` internals.
2. **New concept → new name.** Only shadow an `Avalonia.Controls` name when genuinely *restyling the same
   concept* (Button/Text/Card). Net-new concepts (responsive grid) get distinct names.
3. **Theme consistency end-to-end** — an app should look Material *everywhere*, including base chrome.
4. **Modular packaging** — a lean core plus optional satellite packages.
5. **v3 may break** — but ship a migration guide, `[Obsolete]` shims, and analyzers for renames.

---

## Remove / deprecate / extract

- **Rename, don't delete** the responsive `Grid`/`Item` (see *Refactor*). Keep the capability.
- **Drop thin wrappers** with little value over Avalonia: `Stack` (≈ `StackPanel`). Review
  `Container`/`Spacer`/`Hidden`/`ScrollToTop`; keep only the genuinely useful responsive helpers.
- **Consolidate the table story** — `SimpleTable` vs `DataGrid<T>`: one path forward.
- **Extract heavy/optional areas into satellite packages** to keep the core lean:
  - `Loam.Charts` (Pie/Bar/Line + donut)
  - `Loam.Pickers` (date/time/color/range + calendar)
  - `Loam.Data` (DataGrid/TreeView/SimpleTable/Pagination)
  - Core `Loam` = theming + primitives + shell + inputs + overlays + navigation.

## Add

- **Base-primitive theming (top priority — see Key Finding).** Loam control themes / Fluent-resource
  bridges for `ScrollBar`/`ScrollViewer`, `ToolTip`, `ContextMenu`/`MenuFlyout`/`Menu`, `Window`
  (+ optional custom title bar), text selection/caret, focus adorner, `Expander`, and Avalonia
  `DataGrid`. Map `SystemAccentColor*` → Loam primary so stray Fluent controls adopt the accent.
- **Material You tonal generator** — full `LoamColorScheme` (light + dark) from a single seed color
  (HCT/tonal palettes), building on the existing `SetPrimary`. The headline "customizable" feature.
- **Shell breadth:** `NavigationRail`, `BottomNavigation`, `CommandPalette`; optional `BottomSheet`,
  `Banner`.
- **Forms:** a declarative validation surface bound to `ObservableValidator` / DataAnnotations.
- **Icons:** larger set + Outlined/Round variants + pluggable icon provider; visible tooltips on icon
  buttons (`IconButton`/`AppBarAction`).
- **Theme playground** page in the gallery; **high-contrast** theme variant; **RTL/localization** audit.

## Refactor / improve

- **Rename responsive grid** → `ResponsiveGrid` (and `Item` → `Col`); add an `AlignItems`/equal-height
  option for card rows. Document: *Avalonia `Grid` for fixed 2D, Loam `ResponsiveGrid` for reflow.*
- **Collision strategy:** distinct names for new concepts; for intentional restyles
  (`Button`/`Text`/`Card`/`Menu`) ship a `GlobalUsings`/alias snippet or analyzer so consumers don't
  hand-alias in every file.
- **`AppBar`/`MainContent`:** add a `CustomActions` slot accepting arbitrary `Control`s (and stateful
  actions), not just immutable `AppBarAction`.
- **Generated-vs-custom content:** make precedence explicit (a `Slots` API or distinct types) and
  document it; emit a debug warning when both `Content` and the generated props are set.
- **DataGrid maturity** (in `Loam.Data`): sorting/filtering UI, grouping, inline edit, virtualization,
  frozen columns — or a formal "use Avalonia DataGrid + Loam style" stance.
- **Accessibility / density:** focus visuals on surfaces/cards, keyboard-nav audit, one-call compact
  mode (the `LoamDensity` model already exists).

---

## Phased sequence

### Phase 0 — Decide & scaffold
- Lock the naming strategy, the package split, and the breaking-change budget.
- Add `[Obsolete]` to v2 names being renamed; scaffold the migration guide.
- **Done when:** ADRs merged for naming + packaging; v2→v3 rename map drafted.

### Phase 1 — Theme consistency *(biggest immediate win)*
- Restyle the residual base primitives + bridge Loam tokens to Fluent resource keys.
- **Done when:** an all-default app (scrollbars, tooltips, menus, window, selection) reads as Material
  in light & dark, and a stray Fluent control inherits Loam's primary.

### Phase 2 — Theming power
- Material You seed→scheme generator; theme playground; high-contrast variant; density switch.
- **Done when:** one seed color produces a complete, accessible light+dark scheme at runtime.

### Phase 3 — Naming & ergonomics refactor
- `ResponsiveGrid` rename (+ `Col`, align option); collision tooling; `AppBar` custom-actions slot;
  generated-content precedence redesign.
- **Done when:** a new app builds with **no** manual `using LoamX = …` aliases for core controls.

### Phase 4 — Component churn
- Drop thin wrappers; consolidate the table; extract `Loam.Charts`/`Loam.Pickers`/`Loam.Data`;
  add `NavigationRail`/`BottomNavigation`/`CommandPalette`/Forms validation; ship the bigger icon set.
- **Done when:** core package surface shrinks; satellites publish independently.

### Phase 5 — Data maturity
- `DataGrid` grouping/inline-edit/virtualization/frozen columns (or the formal defer stance).
- **Done when:** a 10k-row grid scrolls smoothly with sort/filter/group.

### Phase 6 — Docs, migration, samples, release
- Docs: theming precedence; "Avalonia Grid vs Loam ResponsiveGrid"; "what Loam themes vs leaves to
  Fluent"; "Why Loam vs plain Avalonia" positioning.
- Visual-regression snapshot tests on top of the existing headless suite; keep tests green.
- Use the **Nodely gallery** as a real-world integration sample.
- **Done when:** migration guide + upgraded gallery ship with the v3 release.

---

## Quick reference: rename map (draft)

| v2 | v3 | Reason |
| --- | --- | --- |
| `Grid` / `Item` | `ResponsiveGrid` / `Col` | New concept; stop shadowing `Avalonia.Controls.Grid`. |
| `Stack` | *(removed)* | Use `Avalonia.Controls.StackPanel`. |
| `SimpleTable` + `DataGrid<T>` | one table API | Consolidate. |
| Charts / Pickers / Data controls | moved to satellite packages | Lean core. |

(See `REVIEW.md` for the full strengths/weaknesses analysis and scorecard.)
