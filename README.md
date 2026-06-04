# Loam

**Pure C# themed controls for Avalonia applications.**

Loam gives Avalonia apps a complete themed control set with a familiar, compact API, runtime
theming, and no XAML.

> **Status:** ✅ **v1.2 polish release.** Every component on the v1 component catalog is
> mapped — built, themed, registered, tested, and demonstrated in the gallery. Full solution builds
> clean with **139 headless/unit tests passing**.
> See the **[Development Plan](DEVELOPMENT_PLAN.md)** and the project **[memory](memory/README.md)**
> (decisions, progress log, learnings, and the per-component status tracker).

## 📖 Documentation

A full documentation site (built with [VitePress](https://vitepress.dev)) lives in **[`docs/`](docs/)**
— guides (getting started, theming, C# authoring) plus a documented, example-rich page for every
component group. It is published to **GitHub Pages** automatically via
[`.github/workflows/docs.yml`](.github/workflows/docs.yml) (set repository *Settings → Pages → Source*
to *GitHub Actions*; the site publishes to **https://araxis.github.io/loam/**).

Run the docs locally:

```bash
cd docs
npm install
npm run docs:dev      # local preview at http://localhost:5173
npm run docs:build    # static build → docs/.vitepress/dist
```

## What Loam is

- **Familiar API.** Component parameters use predictable names (`Variant`, `Color`, `Size`, `Dense`,
  `Elevation`, …); types live in `Loam.Controls` (e.g. `Button`, `TextField`, `DataGrid<T>`, the
  `DialogService`).
- **Polished look.** Palette-driven colors, elevation/shadows, a click ripple, and a full typography
  scale, all token-driven.
- **Pure C#.** Controls, `ControlTheme`s, and templates are authored in C# — no `.axaml`.
- **Self-contained.** Pickers (date/time/color) and the calendar are custom-built, so a LoamTheme-only
  app needs no extra control packages.
- **Highly themeable.** A `LoamTheme` data model with light/dark variants and runtime
  palette swapping.
- **Cross-platform.** One library targeting Avalonia 12 everywhere it runs.

## What Loam is not

- Not a drop-in Razor port — you still build Avalonia views; Loam shrinks the *mental* gap.
- **Not a framework wrapper.** Loam is an independent control library built directly on Avalonia.

## Quick start

Install the package after a release is published, or reference `src/Loam/Loam.csproj` while developing
from this repository.

```bash
dotnet add package Loam
```

**1. Register the theme** in your `Application` (add Avalonia's `FluentTheme` for the base controls
Loam composes, then `LoamTheme` on top):

```csharp
using Avalonia;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Loam.Theming;

public sealed class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());   // base templates for the window shell + built-in controls
        Styles.Add(new LoamTheme());     // Loam's pure-C# theming + control themes on top
        RequestedThemeVariant = ThemeVariant.Light;
    }
}
```

**2. Build UI with Loam controls** (object initializers, no XAML):

```csharp
using Loam;
using Loam.Controls;

var form = new StackPanel
{
    Spacing = 12,
    Children =
    {
        new Text { Text = "Sign in", Typo = Typo.H5 },
        new TextField { Label = "Email", Variant = Variant.Outlined },
        new TextField { Label = "Password", Variant = Variant.Outlined },
        new Button { Content = "Continue", Variant = Variant.Filled, Color = LoamColor.Primary },
    },
};
```

**3. Show a dialog / snackbar** (no provider component — uses the window's overlay layer):

```csharp
var ok = await DialogService.For(this).ConfirmAsync("Delete?", "This cannot be undone.", "Delete", "Cancel");
SnackbarService.For(this).Add(ok ? "Deleted" : "Cancelled", LoamColor.Info);
```

## Theming

`LoamTheme` projects a `LoamThemeData` (palette, typography, shadows, layout, z-index) into Avalonia
resources, with light/dark theme dictionaries.

```csharp
var theme = new LoamTheme();
theme.SetPrimary(Colors.Indigo);                 // recolor the primary at runtime
theme.SetPalette(customLight, customDark);        // swap whole palettes
Application.Current!.RequestedThemeVariant = ThemeVariant.Dark; // flip light/dark
```

All control colors resolve from tokens (`LoamTokens`), so theme/variant changes restyle the tree
automatically. Per-control overrides use the same Loam knobs: `Variant`, `Color`, `Size`,
`Dense`, `Elevation`, `Outlined`, `Square`, etc.

## Component catalog

| Area | Controls |
| --- | --- |
| **Primitives** | `Text`, `Icon`, `Button`, `IconButton`, `ToggleIconButton`, `ButtonGroup`, `Fab`, `Paper`, `Card` (+`CardHeader`/`CardMedia`/`CardContent`/`CardActions`), `Divider`, `Chip`/`ChipSet`, `Badge`, `Avatar`/`AvatarGroup` |
| **Layout & shell** | `Container`, `Grid`/`Item`, `Stack`, `Spacer`, `Hidden`, `ScrollToTop`, `Layout`, `AppBar`, `Drawer`, `MainContent` |
| **Inputs** | `Field`, `TextField`, `NumericField`, `MaskedTextField` (+`Mask`), `Select`, `Autocomplete`, `CheckBox`, `Switch`, `Radio`/`RadioGroup`, `Slider`, `Rating`, `ToggleGroup`, `FileUpload`, `Form` |
| **Pickers** | `DatePicker`, `TimePicker`, `ColorPicker`, `DateRangePicker` (+ self-contained `MonthCalendar`) |
| **Overlays & feedback** | `DialogService`/`MessageBoxAsync`, `SnackbarService`, `Overlay`, `Popover`, `Tooltip`, `Menu`, `Alert`, `ProgressLinear`, `ProgressCircular`, `Skeleton`, `Collapse` |
| **Data display** | `List`/`ListItem`/`ListSubheader`, `SimpleTable`, `DataGrid<T>`, `TreeView`, `Tabs`, `ExpansionPanels`, `Timeline`, `Carousel`, `Pagination`, `Stepper` |
| **Navigation** | `Link`, `Breadcrumbs`, `NavMenu`/`NavLink`/`NavGroup` |
| **Charts** | `PieChart`, `BarChart`, `LineChart` (+ donut mode) |
| **Effects** | `Ripple` |

See **[`memory/component-inventory.md`](memory/component-inventory.md)** for the full component catalog
and the live status of each control, including the documented v1 scope cuts (e.g.
DataGrid filter/group/edit, picker clock-face/HSV, stacked/time-series charts) earmarked for follow-up.

## Run the gallery

```bash
dotnet run --project samples/Loam.Gallery
```

The gallery has a side menu and focused pages for the component catalog.

## Repository layout

| Path | Purpose |
| --- | --- |
| `src/Loam/` | The control library (net8.0, packable as `Loam`). |
| `samples/Loam.Gallery/` | Live component gallery. |
| `tests/Loam.Tests/` | Headless + unit tests (xUnit + `Avalonia.Headless`). |
| `DEVELOPMENT_PLAN.md` | Phased roadmap with per-phase & per-component Definition of Done. |
| `memory/` | Decisions (ADRs), research findings, progress log, learnings, component tracker. |

## Tech

- Avalonia **12.x**, .NET **8** library target, C# pure code-only UI.
- xUnit + `Avalonia.Headless.XUnit` for behavior tests; `.slnx` solution; SDK pinned via `global.json`.
- Build: `dotnet build`  ·  Test: `dotnet test`  ·  Pack: `dotnet pack src/Loam`.

## License

[MIT](LICENSE).
