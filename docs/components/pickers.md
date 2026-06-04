---
title: Pickers
---

# Pickers

Loam provides five picker controls. All controls are self-contained `TemplatedControl` or `Decorator` subclasses that open `Flyout` popups, with no dependency on Avalonia's built-in FluentTheme `Calendar`. Controls live in `Loam.Controls`; when both `Avalonia.Controls` and `Loam.Controls` are imported, qualify the Loam types as `Loam.Controls.DatePicker`, `Loam.Controls.TimePicker`, and `Loam.Controls.ColorPicker` to avoid ambiguity with Avalonia's own types of the same name.

Field-style pickers are focusable. Enter or Space opens the flyout, Escape closes it, and the automation name is derived from the label or displayed value.

---

## DatePicker

Mirrors the reference API's `DatePicker`. An outlined box displays the selected date formatted by `DateFormat`; clicking opens a `MonthCalendar` flyout. `Date` is two-way by default.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Date` | `DateTime?` | `null` | The selected date. Two-way binding. |
| `Label` | `string?` | `null` | Field label rendered above the box. |
| `Placeholder` | `string?` | `"Select a date"` | Text shown when `Date` is `null`. |
| `DateFormat` | `string` | `"d"` | .NET date format string used to render `Date`. |
| `MinDate` | `DateTime?` | `null` | First selectable date. |
| `MaxDate` | `DateTime?` | `null` | Last selectable date. |

### Example

```csharp
using Avalonia.Controls;
using Loam.Controls;

var picker = new Loam.Controls.DatePicker
{
    Label = "Start date",
    DateFormat = "MMM d, yyyy",
    MinDate = DateTime.Today,
    MaxDate = DateTime.Today.AddMonths(6),
};

picker.Bind(Loam.Controls.DatePicker.DateProperty, viewModel.GetObservable(vm => vm.StartDate));
```

---

## TimePicker

Mirrors the reference API's `TimePicker`. An outlined box displays the selected `TimeSpan` formatted by `TimeFormat`; clicking opens a flyout with scrollable hour and minute columns. `Time` is two-way by default.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Time` | `TimeSpan?` | `null` | The selected time. Two-way binding. |
| `Label` | `string?` | `null` | Field label rendered above the box. |
| `Placeholder` | `string?` | `"Select a time"` | Text shown when `Time` is `null`. |
| `TimeFormat` | `string` | `"t"` | .NET time format string used to render `Time`. |
| `MinuteStep` | `int` | `5` | Granularity of the minute column (1–30). Mirrors the reference API's `MinuteSelectionStep`. |

### Example

```csharp
using Avalonia.Controls;
using Loam.Controls;

var picker = new Loam.Controls.TimePicker
{
    Label = "Meeting time",
    MinuteStep = 15,
    TimeFormat = "HH:mm",
};

picker.Bind(Loam.Controls.TimePicker.TimeProperty, viewModel.GetObservable(vm => vm.MeetingTime));
```

---

## ColorPicker

Mirrors the reference API's `ColorPicker` (palette mode). An outlined box displays a color swatch and the current hex value; clicking opens a flyout of preset swatches. `Value` is two-way by default.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `Color` | `#594AE2` | The selected color. Two-way binding. |
| `Label` | `string?` | `null` | Field label rendered above the box. |
| `ShowAlpha` | `bool` | `false` | Shows an alpha slider in the flyout and displays `#AARRGGBB`. |
| `DefaultPalette` _(static)_ | `IReadOnlyList<Color>` | 20 hues + neutrals | The curated palette shown in the flyout. |
| `ToHex(Color)` _(static)_ | `string` | — | Formats a `Color` as an upper-case `#RRGGBB` string. |
| `ToHexWithAlpha(Color)` _(static)_ | `string` | — | Formats a `Color` as an upper-case `#AARRGGBB` string. |
| `FromHsv(h, s, v, alpha)` _(static)_ | `Color` | — | Converts hue degrees and unit saturation/value to `Color`. |
| `ToHsv(Color)` _(static)_ | `HsvColor` | — | Converts a `Color` to hue/saturation/value. |

### Example

```csharp
using Avalonia.Media;
using Loam.Controls;

var picker = new Loam.Controls.ColorPicker
{
    Label = "Accent color",
    Value = Color.Parse("#2196F3"),
    ShowAlpha = true,
};

picker.Bind(Loam.Controls.ColorPicker.ValueProperty, viewModel.GetObservable(vm => vm.AccentColor));

// Convert programmatically
string hex = Loam.Controls.ColorPicker.ToHex(picker.Value); // e.g. "#2196F3"
string argb = Loam.Controls.ColorPicker.ToHexWithAlpha(picker.Value); // e.g. "#FF2196F3"
Color vividGreen = Loam.Controls.ColorPicker.FromHsv(120, 1, 1);
```

---

## DateRangePicker

Mirrors the reference API's `DateRangePicker`. An outlined box displays the selected range; clicking opens a `MonthCalendar` flyout where the first click sets `Start` and the second click sets `End` (dates are auto-ordered). Both `Start` and `End` are two-way by default.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Start` | `DateTime?` | `null` | Range start. Two-way binding. Mirrors the reference API's `DateRange.Start`. |
| `End` | `DateTime?` | `null` | Range end. Two-way binding. Mirrors the reference API's `DateRange.End`. |
| `Label` | `string?` | `null` | Field label rendered above the box. |
| `Placeholder` | `string?` | `"Select a range"` | Text shown when no range is set. |
| `DateFormat` | `string` | `"d"` | .NET date format string used to render start and end dates. |
| `MinDate` | `DateTime?` | `null` | First selectable date. |
| `MaxDate` | `DateTime?` | `null` | Last selectable date. |
| `Format(start, end, fmt)` _(static)_ | `string?` | — | Returns a formatted `"start – end"` string, or just the start if `end` is `null`, or `null` when `start` is `null`. |

### Example

```csharp
using Loam.Controls;

var picker = new DateRangePicker
{
    Label = "Booking period",
    DateFormat = "MMM d",
    MinDate = DateTime.Today,
    MaxDate = DateTime.Today.AddMonths(3),
};

picker.Bind(DateRangePicker.StartProperty, viewModel.GetObservable(vm => vm.BookingStart));
picker.Bind(DateRangePicker.EndProperty,   viewModel.GetObservable(vm => vm.BookingEnd));

// Format programmatically
string? display = DateRangePicker.Format(picker.Start, picker.End, "d");
```

---

## MonthCalendar

A reusable month-grid control used internally by `DatePicker` and `DateRangePicker` (eliminating any dependency on Avalonia's FluentTheme `Calendar`). Shows `DisplayMonth` with previous/next month navigation; clicking a day raises `DateSelected` and highlights `SelectedDate`.

### Properties

| Property / Member | Type | Default | Description |
|---|---|---|---|
| `SelectedDate` | `DateTime?` | `null` | The highlighted day. |
| `DisplayMonth` | `DateTime` | First day of the current month | The month currently rendered. |
| `MinDate` | `DateTime?` | `null` | First selectable date. |
| `MaxDate` | `DateTime?` | `null` | Last selectable date. |
| `RangeStart` | `DateTime?` | `null` | Start of the highlighted range. |
| `RangeEnd` | `DateTime?` | `null` | End of the highlighted range. |
| `DateSelected` | `event Action<DateTime>?` | — | Raised when the user clicks a day cell. |
| `IsDisabled(date, min, max)` _(static)_ | `bool` | — | Returns whether a date is outside the selectable bounds. |
| `IsInRange(date, start, end)` _(static)_ | `bool` | — | Returns whether a date falls within a highlighted range. |

### Example

```csharp
using Loam.Controls;

var calendar = new MonthCalendar
{
    SelectedDate = DateTime.Today,
    DisplayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
    MinDate = DateTime.Today.AddDays(-7),
    MaxDate = DateTime.Today.AddDays(30),
    RangeStart = DateTime.Today,
    RangeEnd = DateTime.Today.AddDays(5),
};

calendar.DateSelected += date =>
{
    Console.WriteLine($"User picked {date:d}");
};

// Embed directly in any layout panel
var panel = new StackPanel { Children = { calendar } };
```
