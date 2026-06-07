---
title: Pickers
---

# Pickers

Loam provides five picker controls. All controls are self-contained `TemplatedControl` or `Decorator` subclasses that open `Flyout` popups, with no dependency on Avalonia's built-in FluentTheme `Calendar`. Controls live in `Loam.Controls`; when both `Avalonia.Controls` and `Loam.Controls` are imported, qualify the Loam types as `Loam.Controls.DatePicker`, `Loam.Controls.TimePicker`, and `Loam.Controls.ColorPicker` to avoid ambiguity with Avalonia's own types of the same name.

Field-style pickers are focusable. Enter or Space opens the flyout, Escape closes it, and the automation name is derived from the label or displayed value. Labels rest inside empty fields, float when focused or filled, and can be forced to float with `ShrinkLabel`.

---

## DatePicker

Date field with outlined, filled, and text/underline variants. The field displays the committed date
formatted by `DateFormat`; pointer or keyboard activation opens a calendar flyout with pending
selection. Cancel closes without changing `Date`, while OK commits the pending date and raises
`DateSelected`. Disabled pickers suppress pointer, keyboard, and `OpenPicker()` flyout opening while
still allowing programmatic date updates.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Date` | `DateTime?` | `null` | The selected date. Two-way binding. |
| `Label` | `string?` | `null` | Field label rendered above the box. |
| `Placeholder` | `string?` | `"Select a date"` | Text shown when `Date` is `null`. |
| `DateFormat` | `string` | `"d"` | .NET date format string used to render `Date`. |
| `Variant` | `Variant` | `Variant.Outlined` | Field chrome: outlined, filled, or text/underline. |
| `MinDate` | `DateTime?` | `null` | First selectable date. |
| `MaxDate` | `DateTime?` | `null` | Last selectable date. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `Error` | `bool` | `false` | Puts the field in an error state. |
| `HelperText` | `string?` | `null` | Hint shown below the field. |
| `ErrorText` | `string?` | `null` | Error message shown in place of `HelperText` when `Error` is `true`. |
| `ShrinkLabel` | `bool` | `false` | Keeps the label floated even when empty and unfocused. |
| `PickerTitle` | `string` | `"Select date"` | Title shown inside the flyout. |
| `CancelText` | `string` | `"Cancel"` | Text for the generated cancel action. |
| `OkText` | `string` | `"OK"` | Text for the generated commit action. |
| `OpenPicker()` | `void` | — | Opens the calendar flyout when enabled. |
| `ClosePicker()` | `void` | — | Closes the calendar flyout without committing pending changes. |
| `Clear()` | `void` | — | Clears `Date`. |
| `DateSelected` | `event Action<DateTime?>` | — | Raised when OK commits the pending date. |

### Example

```csharp
using Avalonia.Controls;
using Loam;
using Loam.Controls;

var picker = new Loam.Controls.DatePicker
{
    Label = "Start date",
    Variant = Variant.Outlined,
    PickerTitle = "Select start date",
    CancelText = "Dismiss",
    OkText = "Apply",
    DateFormat = "MMM d, yyyy",
    MinDate = DateTime.Today,
    MaxDate = DateTime.Today.AddMonths(6),
};

picker.Bind(Loam.Controls.DatePicker.DateProperty, viewModel.GetObservable(vm => vm.StartDate));
picker.DateSelected += date => viewModel.SaveDraftStartDate(date);
```

---

## TimePicker

Time field with outlined, filled, and text/underline variants. The field displays the committed
`TimeSpan` formatted by `TimeFormat`; pointer or keyboard activation opens a picker flyout with
scrollable hour and minute columns. Row selection updates the pending time, Cancel closes without
changing `Time`, and OK commits the pending value and raises `TimeSelected`. Disabled pickers suppress
pointer, keyboard, and `OpenPicker()` flyout opening while still allowing programmatic time updates.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Time` | `TimeSpan?` | `null` | The selected time. Two-way binding. |
| `Label` | `string?` | `null` | Field label rendered above the box. |
| `Placeholder` | `string?` | `"Select a time"` | Text shown when `Time` is `null`. |
| `TimeFormat` | `string` | `"t"` | .NET time format string used to render `Time`. |
| `Variant` | `Variant` | `Variant.Outlined` | Field chrome: outlined, filled, or text/underline. |
| `MinuteStep` | `int` | `5` | Granularity of the minute column (1–30). Mirrors the reference API's `MinuteSelectionStep`. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `Error` | `bool` | `false` | Puts the field in an error state. |
| `HelperText` | `string?` | `null` | Hint shown below the field. |
| `ErrorText` | `string?` | `null` | Error message shown in place of `HelperText` when `Error` is `true`. |
| `ShrinkLabel` | `bool` | `false` | Keeps the label floated even when empty and unfocused. |
| `PickerTitle` | `string` | `"Select time"` | Title shown inside the flyout. |
| `CancelText` | `string` | `"Cancel"` | Text for the generated cancel action. |
| `OkText` | `string` | `"OK"` | Text for the generated commit action. |
| `OpenPicker()` | `void` | — | Opens the time flyout when enabled. |
| `ClosePicker()` | `void` | — | Closes the time flyout without committing pending changes. |
| `Clear()` | `void` | — | Clears `Time`. |
| `TimeSelected` | `event Action<TimeSpan?>` | — | Raised when OK commits the pending time. |

### Example

```csharp
using Avalonia.Controls;
using Loam;
using Loam.Controls;

var picker = new Loam.Controls.TimePicker
{
    Label = "Meeting time",
    Variant = Variant.Outlined,
    PickerTitle = "Select meeting time",
    CancelText = "Dismiss",
    OkText = "Apply",
    MinuteStep = 15,
    TimeFormat = "HH:mm",
};

picker.Bind(Loam.Controls.TimePicker.TimeProperty, viewModel.GetObservable(vm => vm.MeetingTime));
picker.TimeSelected += time => viewModel.SaveDraftTime(time);
```

---

## ColorPicker

Palette color field with outlined, filled, and text/underline variants. The field displays a color swatch
and the current hex value; pointer or keyboard activation opens a tokenized flyout of focusable swatches.
`Value` is two-way by default, and disabled pickers suppress pointer, keyboard, and `OpenPicker()` flyout
opening while still allowing programmatic value updates.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `Color` | `#594AE2` | The selected color. Two-way binding. |
| `Label` | `string?` | `null` | Field label rendered above the box. |
| `Variant` | `Variant` | `Variant.Outlined` | Field chrome: outlined, filled, or text/underline. |
| `ShowAlpha` | `bool` | `false` | Displays the selected value as `#AARRGGBB`. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `Error` | `bool` | `false` | Puts the field in an error state. |
| `HelperText` | `string?` | `null` | Hint shown below the field. |
| `ErrorText` | `string?` | `null` | Error message shown in place of `HelperText` when `Error` is `true`. |
| `ShrinkLabel` | `bool` | `false` | Keeps the label floated. |
| `Palette` | `AvaloniaList<Color>` | empty | Custom swatches shown before falling back to `DefaultPalette`. |
| `OpenPicker()` | `void` | — | Opens the swatch flyout when enabled. |
| `ClosePicker()` | `void` | — | Closes the swatch flyout. |
| `ValueChanged` | `event EventHandler<Color>` | — | Raised after `Value` changes. |
| `DefaultPalette` _(static)_ | `IReadOnlyList<Color>` | 20 hues + neutrals | The curated palette shown in the flyout. |
| `ToHex(Color)` _(static)_ | `string` | — | Formats a `Color` as an upper-case `#RRGGBB` string. |
| `ToHexWithAlpha(Color)` _(static)_ | `string` | — | Formats a `Color` as an upper-case `#AARRGGBB` string. |
| `FromHsv(h, s, v, alpha)` _(static)_ | `Color` | — | Converts hue degrees and unit saturation/value to `Color`. |
| `ToHsv(Color)` _(static)_ | `HsvColor` | — | Converts a `Color` to hue/saturation/value. |

### Example

```csharp
using Avalonia.Media;
using Loam;
using Loam.Controls;

var picker = new Loam.Controls.ColorPicker
{
    Label = "Accent color",
    Variant = Variant.Outlined,
    Value = Color.Parse("#2196F3"),
    Palette =
    {
        Color.Parse("#6750A4"),
        Color.Parse("#2E7D32"),
        Color.Parse("#B3261E"),
    },
    ShowAlpha = true,
};

picker.Bind(Loam.Controls.ColorPicker.ValueProperty, viewModel.GetObservable(vm => vm.AccentColor));
picker.ValueChanged += (_, color) => viewModel.AccentHex = Loam.Controls.ColorPicker.ToHex(color);

// Convert programmatically
string hex = Loam.Controls.ColorPicker.ToHex(picker.Value); // e.g. "#2196F3"
string argb = Loam.Controls.ColorPicker.ToHexWithAlpha(picker.Value); // e.g. "#FF2196F3"
Color vividGreen = Loam.Controls.ColorPicker.FromHsv(120, 1, 1);
```

---

## DateRangePicker

Date-range field with outlined, filled, and text/underline variants. The field displays the committed
`Start` and `End` dates; pointer or keyboard activation opens a picker flyout with pending selection.
Cancel closes without changing the committed range, while OK commits the pending start/end dates and
raises `RangeSelected`. Disabled pickers suppress pointer, keyboard, and `OpenPicker()` flyout opening
while still allowing programmatic range updates.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Start` | `DateTime?` | `null` | Range start. Two-way binding. Mirrors the reference API's `DateRange.Start`. |
| `End` | `DateTime?` | `null` | Range end. Two-way binding. Mirrors the reference API's `DateRange.End`. |
| `Label` | `string?` | `null` | Field label rendered above the box. |
| `Placeholder` | `string?` | `"Select a range"` | Text shown when no range is set. |
| `DateFormat` | `string` | `"d"` | .NET date format string used to render start and end dates. |
| `Variant` | `Variant` | `Variant.Outlined` | Field chrome: outlined, filled, or text/underline. |
| `MinDate` | `DateTime?` | `null` | First selectable date. |
| `MaxDate` | `DateTime?` | `null` | Last selectable date. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `Error` | `bool` | `false` | Puts the field in an error state. |
| `HelperText` | `string?` | `null` | Hint shown below the field. |
| `ErrorText` | `string?` | `null` | Error message shown in place of `HelperText` when `Error` is `true`. |
| `ShrinkLabel` | `bool` | `false` | Keeps the label floated even when empty and unfocused. |
| `PickerTitle` | `string` | `"Select range"` | Title shown inside the flyout. |
| `CancelText` | `string` | `"Cancel"` | Text for the generated cancel action. |
| `OkText` | `string` | `"OK"` | Text for the generated commit action. |
| `OpenPicker()` | `void` | — | Opens the range flyout when enabled. |
| `ClosePicker()` | `void` | — | Closes the range flyout without committing pending changes. |
| `Clear()` | `void` | — | Clears both `Start` and `End`. |
| `RangeSelected` | `event Action<DateTime?, DateTime?>` | — | Raised when OK commits the pending range. |
| `Format(start, end, fmt)` _(static)_ | `string?` | — | Returns a formatted `"start – end"` string, or just the start if `end` is `null`, or `null` when `start` is `null`. |

### Example

```csharp
using Loam;
using Loam.Controls;

var picker = new DateRangePicker
{
    Label = "Booking period",
    Variant = Variant.Outlined,
    PickerTitle = "Select booking period",
    CancelText = "Dismiss",
    OkText = "Apply",
    DateFormat = "MMM d",
    MinDate = DateTime.Today,
    MaxDate = DateTime.Today.AddMonths(3),
};

picker.Bind(DateRangePicker.StartProperty, viewModel.GetObservable(vm => vm.BookingStart));
picker.Bind(DateRangePicker.EndProperty,   viewModel.GetObservable(vm => vm.BookingEnd));
picker.RangeSelected += (start, end) => viewModel.SaveDraftRange(start, end);

// Format programmatically
string? display = DateRangePicker.Format(picker.Start, picker.End, "d");
```

---

## MonthCalendar

A reusable month-grid control used internally by `DatePicker` and `DateRangePicker`. It renders a
self-contained calendar grid, previous/next month actions, selected and range states, constrained
disabled days, configurable first weekday, and keyboard navigation across days and months. Pointer,
Enter, or Space selection raises `DateSelected`.

### Properties

| Property / Member | Type | Default | Description |
|---|---|---|---|
| `SelectedDate` | `DateTime?` | `null` | The highlighted day. |
| `DisplayMonth` | `DateTime` | First day of the current month | The month currently rendered. |
| `MinDate` | `DateTime?` | `null` | First selectable date. |
| `MaxDate` | `DateTime?` | `null` | Last selectable date. |
| `RangeStart` | `DateTime?` | `null` | Start of the highlighted range. |
| `RangeEnd` | `DateTime?` | `null` | End of the highlighted range. |
| `FirstDayOfWeek` | `DayOfWeek` | Current culture first day | Weekday shown in the first calendar column. |
| `PreviousMonth()` | `void` | — | Moves `DisplayMonth` backward by one month. |
| `NextMonth()` | `void` | — | Moves `DisplayMonth` forward by one month. |
| `DateSelected` | `event Action<DateTime>?` | — | Raised when the user selects a day by pointer, Enter, or Space. |
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
    FirstDayOfWeek = DayOfWeek.Monday,
};

calendar.DateSelected += date =>
{
    Console.WriteLine($"User picked {date:d}");
};

calendar.NextMonth();

// Embed directly in any layout panel
var panel = new StackPanel { Children = { calendar } };
```
