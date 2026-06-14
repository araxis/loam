---
title: Migrating from v3.0 to v3.1
---

# Migrating from Loam v3.0 to v3.1

> **One breaking change, scoped to three control groups.** In 3.1 the chart, picker, and heavy data
> controls move out of the core `Loam` package into opt-in satellite packages (ADR-0009). If your app
> uses any of them you add a package reference and one registrar line; **namespaces are unchanged**, so
> no `using` directives or call sites move. Everything else in 3.0 keeps working untouched.

## Why

The core package carried controls many apps never use — charts, the picker suite, the data grid and
tree. Splitting them out lets a button-and-form app ship a leaner core, while chart/picker/data apps
opt in to exactly what they need. The split is purely about *packaging and assembly boundaries*: the
public API, type names, and namespaces are identical to 3.0.

## What moved

| Satellite package | Controls |
| --- | --- |
| **`Loam.Charts`** | `PieChart`, `BarChart`, `LineChart` |
| **`Loam.Pickers`** | `DatePicker`, `TimePicker`, `ColorPicker`, `DateRangePicker`, `MonthCalendar` |
| **`Loam.Data`** | `DataGrid<T>`, `SimpleTable`, `TreeView`, `TreeViewItem`, `Pagination` |

Each satellite depends on the core `Loam` package, so you never reference `Loam` twice and you still get
its transitive dependency automatically. The packages version in lockstep — `Loam.Charts` 3.1.0 pairs
with `Loam` 3.1.0.

Controls that **stay in core** (no change needed): Tabs, Stepper, Carousel, Timeline, ExpansionPanel,
List/ListItem, and all display, button, layout, input, navigation, and overlay controls.

## Upgrade steps

### 1. Add the satellite packages you use

```bash
dotnet add package Loam.Charts
dotnet add package Loam.Pickers
dotnet add package Loam.Data
```

Add only the ones whose controls you reference. An app that uses none of these groups upgrades by
bumping `Loam` to 3.1.0 and is done.

### 2. Register each satellite's registrar

`new LoamTheme()` no longer registers the moved controls' themes. Add the matching registrar — from the
`Loam.Theming` namespace — **after** `LoamTheme`, so it reuses the tokens `LoamTheme` already projected:

```csharp
using Loam.Theming;

public override void Initialize()
{
    Styles.Add(new FluentTheme());
    Styles.Add(new LoamTheme());

    Styles.Add(new LoamCharts());    // only if you reference Loam.Charts
    Styles.Add(new LoamPickers());   // only if you reference Loam.Pickers
    Styles.Add(new LoamData());      // only if you reference Loam.Data
}
```

If a satellite control renders unthemed (default Avalonia chrome instead of Material), the registrar for
its package is missing — that's the one symptom to look for.

### 3. Nothing else changes

Type names and namespaces are identical. You do **not** edit any `using Loam.Controls;` directives, and
existing code that constructs `new PieChart { … }`, `new DatePicker { … }`, or `new DataGrid<T> { … }`
compiles unchanged once the package and registrar are in place.

## Verifying the upgrade

- Build: an unresolved `PieChart`/`DatePicker`/`DataGrid<T>` type means the satellite package reference
  is missing.
- Runtime: an unthemed satellite control means its registrar isn't in `Application.Styles`.
