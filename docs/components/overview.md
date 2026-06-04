---
title: Components overview
---

# Components

Every component in the v1 catalog is grouped below. Each page
documents the real public API (properties, events, static helpers) with copy-paste C# examples.

> Types live in `Loam.Controls`; shared enums (`LoamColor`, `Variant`, `LoamSize`, `Typo`) live in
> `Loam`; theming (`LoamTheme`, `LoamTokens`) in `Loam.Theming`.

## By area

| Area | What's inside |
| --- | --- |
| [Display primitives](./display) | `Text`, `Icon`, `Divider`, `Chip` / `ChipSet`, `Badge`, `Avatar` / `AvatarGroup` |
| [Buttons & menus](./buttons) | `Button`, `IconButton`, `ToggleIconButton`, `ButtonGroup`, `Fab`, `Menu` |
| [Surfaces & layout](./layout) | `Paper`, `Card` family, `Container`, `Grid`/`Item`, `Stack`, `Spacer`, `Hidden`, `ScrollToTop`, app shell (`Layout`/`AppBar`/`Drawer`/`MainContent`) |
| [Form inputs](./inputs) | `TextField`, `NumericField`, `MaskedTextField`, `Select`, `Autocomplete`, `CheckBox`, `Switch`, `Radio`, `Slider`, `Rating`, `ToggleGroup`, `FileUpload`, `Form` |
| [Pickers](./pickers) | `DatePicker`, `TimePicker`, `ColorPicker`, `DateRangePicker`, `MonthCalendar` |
| [Data display](./data-display) | `List`, `SimpleTable`, `DataGrid<T>`, `TreeView`, `Tabs`, `ExpansionPanels`, `Timeline`, `Carousel`, `Pagination`, `Stepper` |
| [Navigation](./navigation) | `Link`, `Breadcrumbs`, `NavMenu`/`NavLink`/`NavGroup` |
| [Overlays & feedback](./overlays) | `DialogService`, `SnackbarService`, `Overlay`, `Popover`, `Tooltip`, `Alert`, `ProgressLinear`/`ProgressCircular`, `Skeleton`, `Collapse` |
| [Charts & effects](./charts) | `PieChart`, `BarChart`, `LineChart`, `Ripple` |

## Common parameters

Most controls accept the same familiar knobs:

- **`Variant`** — `Filled` · `Outlined` · `Text`
- **`Color` (`LoamColor`)** — `Primary` · `Secondary` · `Tertiary` · `Info` · `Success` · `Warning` ·
  `Error` · `Dark` · `Default` · `Inherit` · `Transparent`
- **`Size` (`LoamSize`)** — `Small` · `Medium` · `Large`
- **`Elevation`**, **`Dense`**, **`Square`**, **`Outlined`**, **`FullWidth`** where applicable

See [Theming](/guide/theming) for how these map to tokens and how to customize them.
