---
title: Pickers
---

# Pickers

Pickers turn a constrained value — a date, a time, a color, a span of days — into a single tappable field
that opens a popup to choose from. Loam provides five of them. The four field-style pickers
(`DatePicker`, `TimePicker`, `ColorPicker`, `DateRangePicker`) read like form fields: they share the
familiar `Label` / `Placeholder` / `Variant` / `Color` / `HelperText` / `ErrorText` chrome with
[form inputs](./inputs), so a date field sits next to a `TextField` without looking out of place. The
fifth, `MonthCalendar`, is the bare month grid the date pickers embed — drop it straight into a layout
when you want an always-open calendar instead of a popup.

All controls are self-contained `TemplatedControl` or `Decorator` subclasses that open `Flyout` popups,
with no dependency on Avalonia's built-in FluentTheme `Calendar`. Controls live in `Loam.Controls`; when
both `Avalonia.Controls` and `Loam.Controls` are imported, qualify the Loam types as
`Loam.Controls.DatePicker`, `Loam.Controls.TimePicker`, and `Loam.Controls.ColorPicker` to avoid
ambiguity with Avalonia's own types of the same name.

```csharp
using Loam;          // Variant, LoamColor, Icons
using Loam.Controls; // DatePicker, TimePicker, ColorPicker, DateRangePicker, MonthCalendar
```

::: tip Mental model
A field picker is a **two-stage commit**: clicking the field opens a flyout with a *pending* selection,
and nothing changes on your value until **OK** fires the change event (`DateSelected`, `TimeSelected`,
`RangeSelected`). **Cancel** or **Escape** throws the pending pick away. `ColorPicker` is the exception —
choosing a swatch commits immediately and raises `ValueChanged`. `MonthCalendar` is a lower-level
building block with no flyout and no OK/Cancel: it raises `DateSelected` the moment a day is picked.
:::

> **Package (since 3.1).** The picker controls (and `MonthCalendar`) ship in the **`Loam.Pickers`**
> satellite package. Add the package reference and register its themes with
> `Styles.Add(new LoamPickers())` after `LoamTheme`. Namespaces are unchanged (`Loam.Controls`). See the
> [v3 → v3.1 migration guide](/migration/v3-to-v3.1).

## Choosing a picker

| Use | When | Reach for |
| --- | --- | --- |
| A single calendar date | Birthday, due date, "starts on" | [`DatePicker`](#datepicker) |
| A clock time | Meeting time, reminder, opening hours | [`TimePicker`](#timepicker) |
| A color | Accent, label color, theme swatch | [`ColorPicker`](#colorpicker) |
| Two dates as one span | Booking period, report range, filter window | [`DateRangePicker`](#daterangepicker) |
| An always-visible month grid | Embed a calendar inline, build your own popup | [`MonthCalendar`](#monthcalendar) |

`Variant`, `Color`, and the field chrome (`Label`, `HelperText`, `ErrorText`) mean the same thing here as
on every form input — see [Components overview → common parameters](./overview#common-parameters) and
[Theming](/guide/theming) for how they map to tokens.

## Shared field behavior

The four field pickers share a common chrome and a set of opt-in behaviors. Learn them once and they
apply across `DatePicker`, `TimePicker`, `ColorPicker`, and `DateRangePicker`.

**Focus & flyout.** Field-style pickers are focusable. Enter or Space opens the flyout, Escape closes it,
and the automation name is derived from the label or displayed value. Labels rest inside empty fields,
float when focused or filled, and can be forced to float with `ShrinkLabel`.

**Clearing.** Set `Clearable` on `DatePicker`, `TimePicker`, or `DateRangePicker` to surface an inline
trailing × button whenever the field holds a value. Clicking it clears the value, raises the change event
with `null`, and intentionally does not open the flyout (the button consumes the pointer before it reaches
the field).

**Leading icon.** Set `AdornmentIcon` (a glyph from `Loam.Icons`, e.g. `Icons.Material.Filled.Person`) on
the same three field pickers to show a leading icon at the start of the field. The value text, resting
label, and floating label all indent to the icon's right so nothing overlaps, across every variant.

**Validation.** `DatePicker`, `TimePicker`, and `DateRangePicker` share validation members: set `Required`
(with optional `RequiredText`, default `"Required"`) and/or `Validation` (a `Func` returning an error
message, or `null` when valid) and call — or let the control call — `Validate()`. Validation runs
automatically whenever the value changes (covering flyout OK, editable commit, `Clear()`, and programmatic
updates) and drives `Error`/`ErrorText`. It self-gates: when neither `Required` nor `Validation` is set,
any manually-assigned `Error`/`ErrorText` is left untouched. In editable mode a parse/format error takes
precedence over business validation. `ColorPicker` also has `Validation` and `Validate()` (but no
`Required`, since its `Value` is non-null).

**Typed entry.** Set `Editable` on `DatePicker`, `TimePicker`, or `DateRangePicker` to let the user type
the value directly into the field. The text is parsed and committed on Enter or focus loss — exact format
first (`DateFormat`/`TimeFormat`), then a loose current-culture parse — and validated against
`MinDate`/`MaxDate` (for the date pickers); unparseable or out-of-range input leaves the value unchanged
and shows `InvalidDateText`/`InvalidTimeText`/`InvalidRangeText` in the error slot. `DateRangePicker`
accepts a `"start – end"` range (also `" to "` or `" - "`, a single date sets only `Start`, and the pair
is auto-ordered). The trailing icon (now a button) and `Alt+Down` still open the flyout, which stays in
sync with the typed value.

::: warning Editable mode keeps invalid text on screen
When a typed value can't be parsed or is out of range, the picker deliberately leaves the user's text in
the box (with `Error`/`InvalidDateText` set) so they can fix it, rather than silently reverting. The
committed value only changes once the text parses — guard on `Validate()` returning `null` before treating
the field as clean.
:::

---

## DatePicker

Date field with outlined, filled, and text/underline variants. The field displays the committed date
formatted by `DateFormat`; pointer or keyboard activation opens a calendar flyout with pending
selection. Cancel closes without changing `Date`, while OK commits the pending date and raises
`DateSelected`. Disabled pickers suppress pointer, keyboard, and `OpenPicker()` flyout opening while
still allowing programmatic date updates.

**Use it when** the user needs to choose one calendar day — a deadline, a birthday, an effective date.

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
| `Clearable` | `bool` | `false` | Shows an inline trailing × button when `Date` is set; clicking it clears the value, raises `DateSelected` with `null`, and does not open the flyout. |
| `AdornmentIcon` | `string?` | `null` | Optional leading glyph shown at the start of the field; the value text and label indent to its right. |
| `Editable` | `bool` | `false` | Lets the user type a date into the field. Text is committed on Enter or focus loss; the trailing calendar icon (or Alt+Down) still opens the flyout. |
| `InvalidDateText` | `string` | `"Invalid date"` | `ErrorText` shown when typed text cannot be parsed or falls outside `MinDate`/`MaxDate` (editable mode). |
| `Required` | `bool` | `false` | When set, a `null` `Date` fails validation with `RequiredText`. Shared by all three date/time field pickers. |
| `RequiredText` | `string` | `"Required"` | Error message used when `Required` fails. |
| `Validation` | `Func<DateTime?, string?>?` | `null` | Returns an error message (or `null` when valid) for the current value; run on every value change. |
| `Validate()` | `string?` | — | Runs `Required`/`Validation`, updates `Error`/`ErrorText`, returns the error. No-op when neither is configured. |
| `TryParseDate(text, format, out value)` _(static)_ | `bool` | — | Parses typed text: `true` for empty (→ `null`) or text parseable via `format` (exact) or the current culture (loose); `false` otherwise. |
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

When the flyout opens, both columns automatically scroll to center the selected (or closest) hour and
minute, so a late time such as `22:55` is in view immediately instead of off-screen at the top. During
keyboard navigation the focused row is kept in view.

**Use it when** the value is a wall-clock time — meeting time, alarm, opening hour. The minute column is
quantized by `MinuteStep`; flip on `Editable` if users need arbitrary minutes.

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
| `Clearable` | `bool` | `false` | Shows an inline trailing × button when `Time` is set; clicking it clears the value, raises `TimeSelected` with `null`, and does not open the flyout. |
| `AdornmentIcon` | `string?` | `null` | Optional leading glyph shown at the start of the field; the value text and label indent to its right. |
| `Editable` | `bool` | `false` | Lets the user type a time into the field. Text is committed on Enter or focus loss; the trailing clock icon (or Alt+Down) still opens the flyout. |
| `InvalidTimeText` | `string` | `"Invalid time"` | `ErrorText` shown when typed text cannot be parsed as a time (editable mode). |
| `Required` | `bool` | `false` | When set, a `null` `Time` fails validation with `RequiredText`. |
| `RequiredText` | `string` | `"Required"` | Error message used when `Required` fails. |
| `Validation` | `Func<TimeSpan?, string?>?` | `null` | Returns an error message (or `null` when valid) for the current value; run on every value change. |
| `Validate()` | `string?` | — | Runs `Required`/`Validation`, updates `Error`/`ErrorText`, returns the error. No-op when neither is configured. |
| `TryParseTime(text, format, out value)` _(static)_ | `bool` | — | Parses typed text: `true` for empty (→ `null`) or text parseable via `format` (exact), the current culture, or `TimeSpan`; `false` otherwise. |
| `PickerTitle` | `string` | `"Select time"` | Title shown inside the flyout. |
| `CancelText` | `string` | `"Cancel"` | Text for the generated cancel action. |
| `OkText` | `string` | `"OK"` | Text for the generated commit action. |
| `OpenPicker()` | `void` | — | Opens the time flyout when enabled. |
| `ClosePicker()` | `void` | — | Closes the time flyout without committing pending changes. |
| `Clear()` | `void` | — | Clears `Time`. |
| `TimeSelected` | `event Action<TimeSpan?>` | — | Raised when OK commits the pending time. |

::: tip MinuteStep is clamped, not snapped on type
`MinuteStep` only controls which rows the minute column offers (it is clamped to 1–30). It does **not**
round a typed-in time: in `Editable` mode a user can commit any minute, so don't rely on `MinuteStep` for
business validation — use `Validation` for that.
:::

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

Set `Editable` to let the user type or paste a hex color directly into the field (the full 24-bit space,
versus the curated palette). The text is parsed and committed on Enter or focus loss — `#RRGGBB`, or
`#AARRGGBB` when `ShowAlpha` — and unparseable input leaves `Value` unchanged and shows `InvalidHexText`. In
editable mode clicking the swatch (or `Alt+Down`) opens the palette flyout, which stays in sync.

**Use it when** the user picks from a small, curated set of colors — label colors, theme accents, tags.
Because choosing a swatch commits immediately, there's no OK/Cancel step here unlike the other field pickers.

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
| `Editable` | `bool` | `false` | Lets the user type/paste a hex color (`#RRGGBB`, or `#AARRGGBB` when `ShowAlpha`) into the field. Committed on Enter or focus loss; the swatch (or Alt+Down) opens the palette. |
| `InvalidHexText` | `string` | `"Invalid color"` | `ErrorText` shown when typed text cannot be parsed as a color (editable mode). |
| `Validation` | `Func<Color, string?>?` | `null` | Returns an error message (or `null` when valid) for the current `Value`; run on every value change. (No `Required` — `Value` is non-null.) |
| `Validate()` | `string?` | — | Runs `Validation`, updates `Error`/`ErrorText`, returns the error. No-op when `Validation` is unset. |
| `Palette` | `AvaloniaList<Color>` | empty | Custom swatches shown before falling back to `DefaultPalette`. |
| `TryParseColor(text, out color)` _(static)_ | `bool` | — | Parses a hex/named color; `false` for empty or unparseable text. |
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

**Use it when** two dates belong together as one value — a booking window, a report period, a "from / to"
filter. For a single date, use [`DatePicker`](#datepicker) instead.

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
| `Clearable` | `bool` | `false` | Shows an inline trailing × button when a range is set; clicking it clears `Start`/`End`, raises `RangeSelected` with `null, null`, and does not open the flyout. |
| `AdornmentIcon` | `string?` | `null` | Optional leading glyph shown at the start of the field; the range text and label indent to its right. |
| `Editable` | `bool` | `false` | Lets the user type a range (`"start – end"`, also `" to "`/`" - "`) into the field; a single date sets only `Start`. Committed on Enter or focus loss; the trailing calendar icon (or Alt+Down) still opens the flyout. |
| `InvalidRangeText` | `string` | `"Invalid range"` | `ErrorText` shown when typed text cannot be parsed or is out of range (editable mode). |
| `Required` | `bool` | `false` | When set, a missing `Start` fails validation with `RequiredText`. |
| `RequiredText` | `string` | `"Required"` | Error message used when `Required` fails. |
| `Validation` | `Func<DateTime?, DateTime?, string?>?` | `null` | Returns an error message (or `null` when valid) for the current `Start`/`End`; run on every value change. |
| `Validate()` | `string?` | — | Runs `Required`/`Validation`, updates `Error`/`ErrorText`, returns the error. No-op when neither is configured. |
| `TryParseRange(text, format, out start, out end)` _(static)_ | `bool` | — | Parses typed range text (empty → both `null`; single date → `start` only; two dates → auto-ordered). Each half uses `DatePicker.TryParseDate`. |
| `ShowPresets` | `bool` | `false` | Shows a quick-select rail in the flyout listing `Presets` (or `DefaultPresets` when none are set). |
| `Presets` | `AvaloniaList<DateRangePreset>` | empty | Custom quick-select shortcuts. When empty and `ShowPresets` is `true`, `DefaultPresets` is used. |
| `DefaultPresets` _(static)_ | `IReadOnlyList<DateRangePreset>` | 7 built-ins | Today, Yesterday, Last 7 days, Last 30 days, This month, Last month, This year. |
| `PickerTitle` | `string` | `"Select range"` | Title shown inside the flyout. |
| `CancelText` | `string` | `"Cancel"` | Text for the generated cancel action. |
| `OkText` | `string` | `"OK"` | Text for the generated commit action. |
| `OpenPicker()` | `void` | — | Opens the range flyout when enabled. |
| `ClosePicker()` | `void` | — | Closes the range flyout without committing pending changes. |
| `Clear()` | `void` | — | Clears both `Start` and `End`. |
| `RangeSelected` | `event Action<DateTime?, DateTime?>` | — | Raised when OK commits the pending range. |
| `Format(start, end, fmt)` _(static)_ | `string?` | — | Returns a formatted `"start – end"` string, or just the start if `end` is `null`, or `null` when `start` is `null`. |

#### Quick-select presets

Set `ShowPresets` to add a rail of one-click shortcuts beside the calendar. Clicking a preset stages a
**pending** range — the calendar highlights it and the user still confirms with OK (or adjusts first), so
presets compose with the two-click commit model instead of bypassing it. A preset's range is auto-ordered
and clamped to `MinDate`/`MaxDate`; a preset that falls entirely outside the bounds does nothing.

`DefaultPresets` supplies Today, Yesterday, Last 7 days, Last 30 days, This month, Last month, and This
year. Add `DateRangePreset` items to `Presets` to replace the defaults with your own. Each preset's
`Resolve` delegate receives an anchor date (the picker passes `DateTime.Today`) and returns a
`(start, end)` pair:

```csharp
var picker = new DateRangePicker { ShowPresets = true };

// Replace the built-ins with custom shortcuts
picker.Presets.Add(new DateRangePreset("This week", a => (a.AddDays(-(int)a.DayOfWeek), a)));
picker.Presets.Add(new DateRangePreset("Next 14 days", a => (a, a.AddDays(13))));
```

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

**Use it when** you want the calendar always visible inline, or you're building your own popup or
multi-month layout and need the grid without a field wrapper.

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

---

## Recipe: a "schedule an event" form

A common layout — a date, a time, and a label color, each a required field with a leading icon, validated
together before submit. Everything is plain C#; lay the pieces out with a `StackPanel` (see
[Surfaces & layout](./layout)). The form gathers a draft and only commits when every `Validate()` returns
`null`.

```csharp
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Controls;

var date = new Loam.Controls.DatePicker
{
    Label = "Event date",
    AdornmentIcon = Icons.Material.Filled.CalendarToday,
    Clearable = true,
    Required = true,
    MinDate = DateTime.Today,
    HelperText = "Today or later",
};

var time = new Loam.Controls.TimePicker
{
    Label = "Start time",
    AdornmentIcon = Icons.Material.Filled.Schedule,
    MinuteStep = 15,
    TimeFormat = "HH:mm",
    Required = true,
    Editable = true,
};

var color = new Loam.Controls.ColorPicker
{
    Label = "Label color",
    Value = Color.Parse("#2196F3"),
    Validation = c => c.A == 0 ? "Pick a visible color" : null,
};

var save = new Button
{
    Content = "Save event",
    Variant = Variant.Filled,
    Color = LoamColor.Primary,
    StartIcon = Icons.Material.Filled.Check,
};
save.Click += (_, _) =>
{
    // Every field validates itself on change, but force a pass before committing.
    var ok = date.Validate() is null & time.Validate() is null & color.Validate() is null;
    if (ok)
    {
        viewModel.SaveEvent(date.Date, time.Time, color.Value);
    }
};

var form = new StackPanel
{
    Spacing = 16,
    Children = { date, time, color, save },
};
```

::: details Why `&` and not `&&` above
Using the non-short-circuiting `&` runs `Validate()` on *every* field even after the first failure, so all
invalid fields light up their `ErrorText` at once — friendlier than revealing errors one submit at a time.
:::

## Accessibility & keyboard

The four field pickers are focusable controls with automation names derived from their `Label` (falling
back to the displayed value or placeholder), so assistive technology announces what each field is for.

- **Focus** — field pickers join the tab order and show a focus accent; <kbd>Tab</kbd> / <kbd>Shift</kbd>+<kbd>Tab</kbd> move between them.
- **Open** — <kbd>Enter</kbd> or <kbd>Space</kbd> opens the flyout on a non-editable field. In `Editable` mode those keys belong to the text box, so use <kbd>Alt</kbd>+<kbd>↓</kbd> (or click the trailing icon) to open instead.
- **Commit / dismiss** — inside a date/time/range flyout, the **OK** button commits the pending value and **Cancel** discards it; <kbd>Esc</kbd> closes without committing. `ColorPicker` commits on swatch selection.
- **Typing** — in `Editable` mode the value commits on <kbd>Enter</kbd> or focus loss; unparseable text is kept on screen with the `Invalid…` error so it can be corrected.
- **Clear** — the inline × button (when `Clearable`) carries the automation name "Clear date" and clears without opening the flyout.
- **MonthCalendar** — supports keyboard navigation across days and months; <kbd>Enter</kbd> or <kbd>Space</kbd> selects the focused day and raises `DateSelected`, and the focused row is kept in view.
- **Disabled** — a disabled picker blocks pointer, keyboard, and `OpenPicker()` while still accepting programmatic value updates.

::: tip Help text doubles as guidance
Set `HelperText` to spell out the expected format ("DD/MM/YYYY") or the bounds ("Within the next 30 days").
On error, `ErrorText` replaces it in the same slot, so users see one consistent line below the field.
:::

## See also

- [Form inputs](./inputs) — `TextField`, `Select`, and the shared `Field` chrome these pickers mirror.
- [Form inputs → Form](./inputs#form) — wiring required/validated fields into a submit flow.
- [Buttons & menus](./buttons) — the `Button` that drives the recipe's commit and the flyout's OK/Cancel actions.
- [Display primitives](./display) — `Icon` and the glyph set behind `AdornmentIcon`.
- [Theming](/guide/theming) — how `Variant`, `Color`, and the field tokens resolve.
