---
title: Form inputs
---

# Form inputs

Loam's input controls map familiar form surfaces to Avalonia-native `TemplatedControl` and control
subclasses. Shared enums (`LoamColor`, `Variant`, `LoamSize`) live in the `Loam` namespace; all controls
are in `Loam.Controls`. Most of the text-style inputs — `Field`, `TextField`, `NumericField`,
`MaskedTextField`, `Select`, `Autocomplete` — share one piece of chrome: a label that floats on focus,
optional helper/error text below, Text/Filled/Outlined variants, and a focus accent driven by `Color`.
Learn that chrome once and it reads the same across every field on the form.

```csharp
using Loam;          // Variant, LoamColor, LoamSize, Icons
using Loam.Controls; // TextField, Select, CheckBox, Form, …
```

::: tip Mental model
Inputs fall into three buckets. **Field-chrome inputs** (`Field`, `TextField`, `NumericField`,
`MaskedTextField`, `Select`, `Autocomplete`) capture a typed or chosen value and share the floating-label
look. **Toggle inputs** (`CheckBox`, `Switch`, `Radio`/`RadioGroup`, `ToggleGroup`) capture a discrete
choice. **Specialized inputs** (`Slider`, `Rating`, `FileUpload`) capture a value through direct
manipulation. `Form` is the container that lays fields out and runs validation across them.
:::

## Choosing an input

| Use | When | Reach for |
| --- | --- | --- |
| Free text | A short string with no fixed format (name, email) | [`TextField`](#textfield) |
| A number | A bounded numeric value with steppers | [`NumericField`](#numericfield) |
| Formatted text | Input that must follow a fixed pattern (phone, SSN) | [`MaskedTextField`](#maskedtextfield) |
| Pick from a known list | One (or several) options from a closed set | [`Select`](#select) |
| Pick with type-ahead | A long or remote list the user filters as they type | [`Autocomplete`](#autocomplete) |
| One on/off choice | A single boolean ("Accept terms", "Dark mode") | [`CheckBox`](#checkbox) or [`Switch`](#switch) |
| One of a few | Mutually exclusive options shown together | [`Radio`](#radio-and-radiogroup) / [`ToggleGroup`](#togglegroup-and-toggleitem) |
| A value on a range | A continuous quantity (volume, opacity) | [`Slider`](#slider) |
| A score | A 1–N star rating | [`Rating`](#rating) |
| File(s) from disk | Attaching documents or images | [`FileUpload`](#fileupload) |
| Custom editor in field chrome | Wrapping your own control so it matches the others | [`Field`](#field) |

`Variant`, `Color`, and `Size` mean the same thing across the family — see
[Components overview → common parameters](./overview#common-parameters) and [Theming](/guide/theming)
for how they resolve to tokens.

::: details CheckBox vs. Switch vs. ToggleGroup — which boolean control?
All three capture a discrete choice, but the affordance differs. Use a **`Switch`** for a setting that
takes effect immediately (it reads as "on/off"). Use a **`CheckBox`** for an opt-in within a form that
is committed on submit, or when you need tri-state (`IsChecked` is `bool?`). Use a **`ToggleGroup`** when
the choice is one of several mutually exclusive values shown side by side. For a single boolean that maps
to a button-like toggle, see [`ToggleIconButton`](./buttons#toggleiconbutton).
:::

---

## Field

Generic field chrome for custom input-like content. It gives arbitrary Avalonia content the same
label, helper/error text, Text/Filled/Outlined variants, focus accent, and start/end adornment slots
as the built-in field-style inputs.

Use it when you are composing a custom editor that should visually line up with `TextField`,
`NumericField`, `Select`, and the pickers.

Use `FieldEditor.MakeChromeless` when hosting a plain Avalonia `TextBox` so the outer `Field`
owns the background, border, and focus outline.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Content` | `object?` | `null` | Custom content hosted inside the field chrome. |
| `Label` | `string?` | `null` | Field label shown above the chrome. |
| `HelperText` | `string?` | `null` | Hint shown below the field. |
| `ErrorText` | `string?` | `null` | Error message shown in place of `HelperText` when `Error` is `true`. |
| `Variant` | `Variant` | `Variant.Outlined` | Visual chrome style (`Text`, `Filled`, `Outlined`). |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `Error` | `bool` | `false` | Puts the field in an error state. |
| `StartAdornment` | `object?` | `null` | Content shown before the custom content. |
| `EndAdornment` | `object?` | `null` | Content shown after the custom content. |
| `InnerPadding` | `bool` | `true` | Applies standard field padding; set `false` when the child owns spacing. |

```csharp
using Avalonia;
using Avalonia.Controls;
using Loam;
using Loam.Controls;

var rawInput = FieldEditor.MakeChromeless(new TextBox
{
    PlaceholderText = "(555) 123-4567",
});

var phone = new Field
{
    Label = "Phone",
    HelperText = "Custom phone editor",
    StartAdornment = new TextBlock { Text = "+1" },
    Content = rawInput,
};

var compactOptions = new Field
{
    Label = "Channels",
    InnerPadding = false,
    Content = new StackPanel
    {
        Margin = new Thickness(8, 6),
        Children =
        {
            new Loam.Controls.CheckBox { Content = "Email", IsChecked = true },
            new Loam.Controls.CheckBox { Content = "SMS" },
        },
    },
};
```

---

## TextField

Text input equivalent to the reference API's `TextField`. Wraps an Avalonia `TextBox` with an
optional label, helper/error text, and Text/Filled/Outlined variant chrome that highlights in `Color`
on focus and switches to the error color when `Error` is set. Validates automatically on blur when
`Required` or `Validation` is set.

**Use it when** you need a short free-text value. Set `Required` and/or `Validation` to get blur-time
validation for free; call `Validate()` yourself to force a check (this is what [`Form`](#form) does).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string?` | `null` | The text value (two-way). |
| `Label` | `string?` | `null` | Label shown inside the empty field, then floated when focused or filled. |
| `Placeholder` | `string?` | `null` | Hint shown when the field is empty and the label is not resting inside the field. |
| `StartAdornment` | `object?` | `null` | Content shown before the text box inside the field chrome. |
| `EndAdornment` | `object?` | `null` | Content shown after the text box inside the field chrome. |
| `FloatingLabel` | `bool` | `false` | Keeps the label hidden until the field has focus or text. |
| `ShrinkLabel` | `bool` | `false` | Keeps the label floated even when the field is empty and unfocused. |
| `HelperText` | `string?` | `null` | Hint shown below the field. |
| `ErrorText` | `string?` | `null` | Error message shown in place of `HelperText` when `Error` is `true`. |
| `Variant` | `Variant` | `Variant.Outlined` | Visual chrome style (`Text`, `Filled`, `Outlined`). |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `Error` | `bool` | `false` | Puts the field in an error state. |
| `ReadOnly` | `bool` | `false` | Makes the inner `TextBox` read-only. |
| `Required` | `bool` | `false` | Treats an empty value as invalid on blur. |
| `Validation` | `Func<string?, string?>?` | `null` | Custom validator; returns an error string or `null` when valid. |
| **Method** `Validate()` | `string?` | — | Runs Required + custom validation, sets `Error`/`ErrorText`, returns the error or `null`. |

```csharp
using Avalonia.Layout;
using Loam;
using Loam.Controls;

var field = new TextField
{
    Label       = "Email",
    Placeholder = "you@example.com",
    Variant     = Variant.Outlined,
    Color       = LoamColor.Primary,
    Required    = true,
    Validation  = v => v?.Contains('@') == true ? null : "Must be a valid email",
};

var amount = new TextField
{
    Label = "Amount",
    StartAdornment = new TextBlock { Text = "$" },
    EndAdornment = new TextBlock { Text = "USD" },
    ShrinkLabel = true,
};
```

::: tip Adornments aren't validation
`StartAdornment`/`EndAdornment` are purely visual prefixes/suffixes ("$", "USD", an icon). They don't
parse or constrain input — for that, use `Validation`, or reach for [`NumericField`](#numericfield) /
[`MaskedTextField`](#maskedtextfield).
:::

---

## NumericField

Numeric text input with spinner buttons, equivalent to the reference API's `NumericField`. Shares the
`TextField` chrome and adds `Minimum`/`Maximum` clamping, a `Step` increment/decrement,
and optional .NET format-string display.

**Use it when** the value is a number with a sensible range. <kbd>↑</kbd>/<kbd>↓</kbd> and the spinner
buttons both step by `Step`, and values are clamped into `[Minimum, Maximum]`.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `double` | `0` | The numeric value (two-way). |
| `Minimum` | `double` | `double.MinValue` | Lower bound; values are clamped on change. |
| `Maximum` | `double` | `double.MaxValue` | Upper bound; values are clamped on change. |
| `Step` | `double` | `1` | Increment/decrement applied by the spinner buttons. |
| `Format` | `string?` | `null` | .NET numeric format string for display (e.g. `"F2"`). |
| `Label` | `string?` | `null` | Field label. |
| `HelperText` | `string?` | `null` | Hint shown below the field. |
| `ErrorText` | `string?` | `null` | Error message shown when `Error` is `true`. |
| `Variant` | `Variant` | `Variant.Outlined` | Visual chrome style. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `Error` | `bool` | `false` | Puts the field in an error state. |
| **Static** `Clamp(value, min, max)` | `double` | — | Clamps a value to the given range. |

```csharp
using Loam;
using Loam.Controls;

var field = new NumericField
{
    Label   = "Quantity",
    Minimum = 1,
    Maximum = 100,
    Step    = 5,
    Format  = "F0",
    Variant = Variant.Filled,
};
```

---

## MaskedTextField

A `TextField` subclass that reformats its `Text` through a `Mask` pattern as the user types, equivalent
to the reference API's masked `TextField`/`Mask`. Inherits all `TextField` properties.

**Use it when** input must follow a fixed shape — phone numbers, postal codes, card numbers. Because it
is a `TextField`, `Required`, `Validation`, and `Validate()` all work the same way (and `Form`
validates it too).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| *(all `TextField` properties)* | | | Inherited. |
| `Pattern` | `string?` | `null` | Mask pattern string (see `Mask.Apply`). |

### Mask (static helper)

`Loam.Controls.Mask` formats raw input onto a pattern. Pattern placeholder characters:

| Character | Accepts |
|---|---|
| `#` | Digit (`0`–`9`) |
| `A` | Letter (`a`–`z`, `A`–`Z`) |
| `*` | Letter or digit |

Any other character is a literal that is inserted automatically (e.g. the dash in `###-##-####`).

| Method | Signature | Description |
|---|---|---|
| `Mask.Apply` | `static string Apply(string? raw, string? pattern)` | Returns the formatted string; trailing literals are omitted once raw input runs out. |

```csharp
using Loam;
using Loam.Controls;

// Phone number: (555) 123-4567
var phone = new MaskedTextField
{
    Label   = "Phone",
    Pattern = "(###) ###-####",
    Variant = Variant.Outlined,
};

// Manual use of the formatter
string formatted = Mask.Apply("5551234567", "(###) ###-####"); // → "(555) 123-4567"
```

---

## Select

Dropdown single-select control equivalent to the reference API's `Select`/`SelectItem`. An outlined field
shows the chosen option's display text; clicking opens a flyout list built from the `Items` collection.
The field is focusable: Enter or Space opens the list, and Escape closes it.

**Use it when** the user picks from a known, reasonably short set of options. For long or remote lists
the user filters by typing, prefer [`Autocomplete`](#autocomplete). Set `MultiSelect = true` to let
several values be toggled without the flyout closing.

### SelectItem

Plain data class representing one option.

| Member | Type | Description |
|---|---|---|
| `Text` | `string?` | Display text shown in the field and flyout. |
| `Value` | `object?` | Value written to `Select.Value` when chosen. |

### Select properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `object?` | `null` | The selected value (two-way). |
| `Label` | `string?` | `null` | Field label. |
| `Placeholder` | `string?` | `null` | Text shown when nothing is selected. |
| `Variant` | `Variant` | `Variant.Outlined` | Visual chrome style. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `Error` | `bool` | `false` | Puts the field in an error state. |
| `HelperText` | `string?` | `null` | Hint shown below the field. |
| `ErrorText` | `string?` | `null` | Error message shown in place of `HelperText` when `Error` is `true`. |
| `ShrinkLabel` | `bool` | `false` | Keeps the label floated even when empty and unfocused. |
| `Items` | `ObservableCollection<SelectItem>` | empty | The available options. |
| `MultiSelect` | `bool` | `false` | Enables toggling several option values without closing the flyout. |
| `SelectedValues` | `ObservableCollection<object?>` | empty | Selected values used when `MultiSelect` is enabled. |
| `DisplayTextFunc` | `Func<SelectItem, string>?` | `null` | Custom text formatter for the field and default rows. |
| `ItemTemplate` | `Func<SelectItem, Control>?` | `null` | Custom row content for the flyout. |

```csharp
using Loam;
using Loam.Controls;

var select = new Select
{
    Label       = "Country",
    Placeholder = "Choose one…",
    HelperText  = "Click anywhere in the field to open",
    Items =
    {
        new SelectItem("Canada",        "ca"),
        new SelectItem("United States", "us"),
        new SelectItem("Mexico",        "mx"),
    },
};
select.Items.Add(new SelectItem("Brazil", "br"));

var tags = new Select
{
    Label = "Tags",
    MultiSelect = true,
    DisplayTextFunc = item => item.Text?.ToUpperInvariant() ?? "",
    Items =
    {
        new SelectItem("Design", "design"),
        new SelectItem("Build", "build"),
        new SelectItem("Review", "review"),
    },
};
tags.SelectedValues.Add("design");
tags.SelectedValues.Add("review");
```

::: tip Single vs. multi
`Value` is the source of truth in single-select mode; `SelectedValues` is the source of truth once
`MultiSelect` is `true`. Bind to whichever matches the mode — mixing them leads to confusing state.
:::

---

## Autocomplete

Free-text input with a filtered suggestion flyout, equivalent to the reference API's `Autocomplete`. Wraps
a `TextField` for field chrome and opens a `Flyout` listing `Items` entries that contain the typed
text (case-insensitive); choosing one fills the field.

**Use it when** the candidate list is long, or comes from a remote source. Provide `Items` for a static
list, `SearchFunc` for custom synchronous filtering, or `SearchAsync` for a remote source (it takes
precedence over the others).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `string?` | `null` | The current text value (two-way). |
| `Label` | `string?` | `null` | Field label. |
| `Placeholder` | `string?` | `null` | Placeholder shown when empty. |
| `Variant` | `Variant` | `Variant.Outlined` | Visual chrome style. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `HelperText` | `string?` | `null` | Hint shown below the field. |
| `ErrorText` | `string?` | `null` | Error message shown in place of `HelperText` when `Error` is `true`. |
| `Error` | `bool` | `false` | Puts the field in an error state. |
| `ShrinkLabel` | `bool` | `false` | Keeps the label floated even when empty and unfocused. |
| `MaxItems` | `int` | `10` | Maximum suggestions shown in the flyout. |
| `Items` | `ObservableCollection<string>` | empty | The full candidate list. |
| `SearchFunc` | `Func<string?, IEnumerable<string>>?` | `null` | Synchronous search source used instead of `Items` filtering. |
| `SearchAsync` | `Func<string?, CancellationToken, Task<IEnumerable<string>>>?` | `null` | Async search source; takes precedence over `SearchFunc`. |
| `ItemTemplate` | `Func<string, Control>?` | `null` | Custom row content for suggestions. |
| **Static** `Filter(items, text, max)` | `IReadOnlyList<string>` | — | Returns up to `max` entries that contain `text` (case-insensitive). |

```csharp
using Loam;
using Loam.Controls;

var ac = new Autocomplete
{
    Label    = "Framework",
    MaxItems = 5,
    Variant  = Variant.Outlined,
    Items    = { "Avalonia", "WPF", "WinUI", "MAUI", "Uno Platform" },
};

var remote = new Autocomplete
{
    Label = "Customer",
    SearchAsync = async (text, cancellationToken) =>
    {
        await Task.Delay(50, cancellationToken);
        return Customers.Where(c => c.Contains(text ?? "", StringComparison.OrdinalIgnoreCase));
    },
};
```

::: warning Cancel your async searches
`SearchAsync` receives a `CancellationToken` that is cancelled when the query changes or the flyout
closes. Pass it through to your I/O (and to `Task.Delay`, as above) so superseded lookups stop instead
of racing to fill stale results.
:::

---

## CheckBox

Checkbox equivalent to the reference API's `CheckBox`. Subclasses Avalonia's `CheckBox` (inheriting
tri-state, keyboard toggle, and `IsChecked`) and renders a token-colored box and checkmark scaled by
`Size`.

**Use it when** a single boolean opt-in is committed with the rest of a form (terms, preferences), or
when you need an indeterminate state — `IsChecked` is `bool?`.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsChecked` | `bool?` | `false` | Checked state (inherited from Avalonia `CheckBox`; supports tri-state). |
| `Content` | `object?` | `null` | Label next to the box (inherited). |
| `Color` | `LoamColor` | `LoamColor.Primary` | Fill color when checked. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Box and checkmark size. |

```csharp
using Loam;
using Loam.Controls;

var cb = new CheckBox
{
    Content   = "Accept terms",
    Color     = LoamColor.Primary,
    Size      = LoamSize.Medium,
    IsChecked = false,
};
```

---

## Switch

Toggle switch equivalent to the reference API's `Switch`. Subclasses Avalonia's `ToggleButton`
(toggle behavior, `:checked` pseudo-class) and renders a tinted sliding track and thumb.

**Use it when** flipping a setting takes effect right away — its sliding track reads as a live "on/off",
where a `CheckBox` reads as "selected for later".

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsChecked` | `bool?` | `false` | On/off state (inherited from `ToggleButton`). |
| `Content` | `object?` | `null` | Label next to the switch (inherited). |
| `Color` | `LoamColor` | `LoamColor.Primary` | Track and thumb color when on. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Track and thumb size. |

```csharp
using Loam;
using Loam.Controls;

var sw = new Switch
{
    Content   = "Dark mode",
    Color     = LoamColor.Secondary,
    Size      = LoamSize.Medium,
    IsChecked = false,
};
```

---

## Radio and RadioGroup

`Radio` is a radio button equivalent to the reference API's `Radio`. It subclasses Avalonia's
`RadioButton` and renders a token-colored ring and dot. `RadioGroup` is a `Decorator` that coordinates
child `Radio` controls and exposes the chosen option's value as a two-way `Value` property, mirroring
the reference API's `RadioGroup`.

**Use it when** the user must pick exactly one of a small set and you want every option visible at once.
For a compact, segmented version of the same idea, use [`ToggleGroup`](#togglegroup-and-toggleitem); for
a space-saving dropdown, use [`Select`](#select).

### Radio properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsChecked` | `bool?` | `false` | Selected state (inherited from Avalonia `RadioButton`). |
| `Content` | `object?` | `null` | Label next to the button (inherited). |
| `Color` | `LoamColor` | `LoamColor.Primary` | Ring and dot color when selected. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Ring and dot size. |
| `Value` | `object?` | `null` | The value this option represents; read by `RadioGroup`. |

### RadioGroup properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Child` | `Control?` | `null` | Content panel containing `Radio` controls (inherited from `Decorator`). |
| `Value` | `object?` | `null` | The `Value` of the currently selected `Radio` (two-way). |

```csharp
using Loam;
using Loam.Controls;
using Avalonia.Controls;

var group = new RadioGroup
{
    Child = new StackPanel
    {
        Children =
        {
            new Radio { Content = "Small",  Value = "sm", Color = LoamColor.Primary },
            new Radio { Content = "Medium", Value = "md", Color = LoamColor.Primary },
            new Radio { Content = "Large",  Value = "lg", Color = LoamColor.Primary },
        },
    },
    Value = "md",
};
```

::: tip Bind the group, not the buttons
Read and write the selection through `RadioGroup.Value` — it tracks whichever child `Radio.Value` is
checked. You normally don't bind individual `Radio.IsChecked`; the group keeps them mutually exclusive.
:::

---

## Slider

Horizontal drag slider equivalent to the reference API's `Slider`. Renders a custom track, fill, and
draggable thumb; pointer drag and click both set `Value` within `Minimum`/`Maximum`.

**Use it when** the value is a continuous quantity the user adjusts by feel (volume, opacity, zoom) and
the exact number matters less than the relative position. When the precise number is what counts, use
[`NumericField`](#numericfield).

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `double` | `0` | Current position (two-way). |
| `Minimum` | `double` | `0` | Left-end value. |
| `Maximum` | `double` | `100` | Right-end value. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Fill and thumb tint color. |
| **Static** `Fraction(value, min, max)` | `double` | — | Returns the 0–1 position of `value` within the range. |

```csharp
using Loam;
using Loam.Controls;

var slider = new Slider
{
    Minimum = 0,
    Maximum = 10,
    Value   = 5,
    Color   = LoamColor.Secondary,
};
```

---

## Rating

Star rating control equivalent to the reference API's `Rating`. Shows `MaxValue` stars filled up to
`SelectedValue` (with hover preview); set `ReadOnly` to display a fixed score without interaction.

**Use it when** you want a 1–N score with at-a-glance stars — collecting a review, or (with `ReadOnly`)
displaying an average.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `SelectedValue` | `int` | `0` | Number of filled stars (two-way). |
| `MaxValue` | `int` | `5` | Total number of stars rendered. |
| `Color` | `LoamColor` | `LoamColor.Warning` | Filled-star tint (defaults to star-gold Warning). |
| `Size` | `LoamSize` | `LoamSize.Medium` | Star icon size. |
| `ReadOnly` | `bool` | `false` | Disables hover and click interaction. |

```csharp
using Loam;
using Loam.Controls;

var rating = new Rating
{
    MaxValue      = 5,
    SelectedValue = 3,
    Color         = LoamColor.Warning,
    Size          = LoamSize.Large,
};
```

---

## ToggleGroup and ToggleItem

Segmented single-select control equivalent to the reference API's `ToggleGroup`/`ToggleItem`. Renders
`Items` as connected border segments; the segment whose value equals `SelectedValue` is filled with
`Color`.

**Use it when** a small set of mutually exclusive options reads best as a compact segmented switch
("Grid / List / Table"). For *firing actions* rather than tracking a selected value, use
[`ButtonGroup`](./buttons#buttongroup) instead.

### ToggleItem

Plain data class representing one segment.

| Member | Type | Description |
|---|---|---|
| `Text` | `string?` | Label rendered inside the segment. |
| `Value` | `object?` | Value written to `ToggleGroup.SelectedValue` when the segment is clicked. |

### ToggleGroup properties

| Property | Type | Default | Description |
|---|---|---|---|
| `SelectedValue` | `object?` | `null` | The value of the active segment (two-way). |
| `Color` | `LoamColor` | `LoamColor.Primary` | Background fill of the selected segment. |
| `Items` | `ObservableCollection<ToggleItem>` | empty | The available segments; changes rebuild the UI. |

```csharp
using Loam;
using Loam.Controls;

var toggle = new ToggleGroup
{
    SelectedValue = "list",
    Items =
    {
        new ToggleItem("Grid", "grid"),
        new ToggleItem("List", "list"),
        new ToggleItem("Table", "table"),
    },
};
```

---

## FileUpload

File picker control equivalent to the reference API's `FileUpload`. A button opens the platform file picker
via Avalonia's `IStorageProvider`; chosen files are exposed as `Files` and their names rendered as
chips. The `FilesSelected` event fires after each successful pick.

**Use it when** the user attaches files from disk. Constrain types with `AcceptedFileTypes`, toggle
single vs. multiple with `AllowMultiple`, and let the user revise the selection with `ShowRemoveButtons`
/ `ShowClearButton`.

### Properties and members

| Member | Type | Default | Description |
|---|---|---|---|
| `Label` | `string?` | `null` | Optional generated label shown above the picker button. |
| `HelperText` | `string?` | `null` | Optional generated helper text below the picker/chips. |
| `EmptyText` | `string?` | `"No files selected"` | Status text shown before files are selected. |
| `SelectedTextFormat` | `string` | `"{0} files selected"` | Status format after selection; receives count and joined file names. |
| `ButtonText` | `string` | `"Upload files"` | Caption of the picker button. |
| `ButtonIcon` | `string?` | upload icon | Leading icon for the picker button. |
| `Variant` | `Variant` | `Outlined` | Visual style for the generated picker button. |
| `Color` | `LoamColor` | `Primary` | Semantic color for the generated picker button. |
| `Size` | `LoamSize` | `Medium` | Size for the picker button, generated chips, and clear action. |
| `AllowMultiple` | `bool` | `true` | Whether multiple files may be selected. |
| `AcceptedFileTypes` | `IReadOnlyList<FilePickerFileType>?` | `null` | Optional platform file picker filters. |
| `ShowRemoveButtons` | `bool` | `false` | Whether generated file chips show a remove affordance. |
| `ShowClearButton` | `bool` | `false` | Whether to render a generated clear action after selected chips. |
| `ClearText` | `string` | `"Clear"` | Text for the generated clear action. |
| `SelectedFileIcon` | `string?` | document icon | Leading icon for generated selected-file chips. |
| `Files` | `IReadOnlyList<IStorageFile>` | empty | Last picked files (read-only). |
| `FileNames` | `IReadOnlyList<string>` | empty | Display names of the current selection (read-only). |
| **Event** `FilesSelected` | `Action<IReadOnlyList<IStorageFile>>?` | — | Raised after a successful pick. |
| **Event** `FileRemoved` | `Action<string>?` | — | Raised when a generated file chip is removed. |
| **Event** `SelectionCleared` | `EventHandler?` | — | Raised when the generated clear action or `Clear()` clears the selection. |
| **Method** `ShowSelection(names)` | `void` | — | Manually sets the displayed chip list without re-opening the picker. |
| **Method** `Clear()` | `void` | — | Clears `Files`, `FileNames`, and the chip display. |

When `FileUpload` is disabled, the generated picker button, selected-file chips, and clear action
are disabled together. Programmatic methods such as `ShowSelection()` and `Clear()` remain usable for
restoring or resetting application state.

```csharp
using Loam;
using Loam.Controls;

var upload = new FileUpload
{
    Label              = "Evidence",
    HelperText         = "Attach documents for review.",
    EmptyText          = "No evidence attached",
    SelectedTextFormat = "{0} files selected",
    ButtonText         = "Choose images",
    Variant            = Variant.Outlined,
    Color              = LoamColor.Primary,
    Size               = LoamSize.Medium,
    AllowMultiple      = true,
    ShowRemoveButtons  = true,
    ShowClearButton    = true,
};
upload.FilesSelected += files =>
{
    foreach (var f in files)
        Console.WriteLine(f.Name);
};
```

---

## Form

Lightweight form container equivalent to the reference API's `Form`. Wraps any layout panel in a
`Decorator`, or generates a standard stacked form from `Children`, title/subtitle text, helper/status
text, and generated actions. `Validate()` walks visual descendants, triggers each `TextField`'s
validation, and sets `IsValid`.

**Use it when** you want a labelled, validating form without hand-wiring layout and submit/reset buttons.
Add fields to `Children`, set `SubmitText`/`ResetText`, and handle `Submitted`.

### Properties and methods

| Member | Type | Default | Description |
|---|---|---|---|
| `Child` | `Control?` | `null` | The content containing input controls (inherited from `Decorator`). |
| `Title` / `Subtitle` | `string?` | `null` | Optional generated form heading and supporting copy. |
| `HelperText` | `string?` | `null` | Neutral status text shown before submit or after reset. |
| `SuccessText` / `ErrorText` | `string?` | `null` | Status text shown after generated submit succeeds or fails. |
| `Children` | `AvaloniaList<Control>` | empty | Fields rendered in the generated vertical layout. |
| `Actions` | `AvaloniaList<Control>` | empty | Extra action controls rendered before generated submit/reset buttons. |
| `Spacing` | `double` | `16` | Vertical spacing between generated form sections and fields. |
| `ActionSpacing` | `double` | `10` | Horizontal spacing between generated actions. |
| `FieldWidth` | `double` | `360` | Default width applied to generated fields that do not already set `Width`. |
| `SubmitText` / `ResetText` | `string?` | `null` | Optional generated submit and reset buttons. |
| `SubmitIcon` / `ResetIcon` | `string?` | `null` | Optional leading icons for generated submit/reset buttons. |
| `ActionSize` | `LoamSize` | `Medium` | Size for generated submit/reset buttons. |
| `SubmitVariant` / `ResetVariant` | `Variant` | `Filled` / `Text` | Visual styles for generated submit/reset buttons. |
| `SubmitColor` / `ResetColor` | `LoamColor` | `Primary` / `Primary` | Semantic colors for generated submit/reset buttons. |
| `ActionsHorizontalAlignment` | `HorizontalAlignment` | `Left` | Alignment for the generated action row. |
| `IsValid` | `bool` | `true` | `true` after `Validate()` if all `TextField` descendants passed. |
| **Event** `Submitted` | `EventHandler?` | — | Raised after the generated submit action validates the form. |
| **Event** `Reset` | `EventHandler?` | — | Raised after the generated reset action clears generated text fields. |
| **Method** `Validate()` | `bool` | — | Runs `TextField.Validate()` on every descendant; returns `true` when all pass and updates `IsValid`. |
| **Method** `ResetFields()` | `void` | — | Clears generated text fields and validation state. |

> `Form` currently validates `TextField` descendants only (including `MaskedTextField`). `NumericField`,
> `Select`, and other controls are not automatically validated by `Form.Validate()`.

When `Form` is disabled, generated submit/reset actions are disabled and their handlers are
suppressed. Programmatic `Validate()` and `ResetFields()` remain available for application workflows.

```csharp
using Loam;
using Loam.Controls;

var nameField  = new TextField { Label = "Name",  Required = true };
var emailField = new TextField
{
    Label      = "Email",
    Required   = true,
    Validation = v => v?.Contains('@') == true ? null : "Invalid email",
};

var form = new Form
{
    Title = "Project access",
    Subtitle = "Validate required fields before inviting a collaborator.",
    HelperText = "Fill the required fields and validate.",
    SuccessText = "Ready to submit.",
    ErrorText = "Review the highlighted fields.",
    FieldWidth = 360,
    SubmitText = "Validate",
    ResetText = "Reset",
    SubmitIcon = Icons.Material.Filled.Check,
    ResetIcon = Icons.Material.Filled.Close,
    ActionSize = LoamSize.Small,
    SubmitVariant = Variant.Filled,
    ResetVariant = Variant.Outlined,
    ActionsHorizontalAlignment = HorizontalAlignment.Right,
    Children = { nameField, emailField },
};

form.Submitted += (_, _) =>
{
    if (form.IsValid)
    {
        // all fields are valid
    }
};
```

::: warning Validate the rest yourself
`Form.Validate()` only runs `TextField.Validate()` on its descendants (so `MaskedTextField` is covered).
A `NumericField`, `Select`, or `CheckBox` won't be checked automatically — validate those in your
`Submitted` handler before acting on the data.
:::

---

## Recipe: a validating sign-up form

A small form composed from the family above — a masked field, a select, a checkbox, and `Form`'s
generated submit/reset row. The `Submitted` handler runs `Validate()` for the text fields and then
checks the non-text inputs by hand (see the warning above). Everything is plain C#.

```csharp
using Avalonia.Layout;
using Loam;
using Loam.Controls;

var name = new TextField { Label = "Full name", Required = true };

var email = new TextField
{
    Label      = "Email",
    Required   = true,
    StartAdornment = new Icon { Data = Icons.Material.Filled.Person },
    Validation = v => v?.Contains('@') == true ? null : "Enter a valid email",
};

var phone = new MaskedTextField
{
    Label   = "Phone",
    Pattern = "(###) ###-####",
    Required = true,
};

var plan = new Select
{
    Label = "Plan",
    Placeholder = "Choose a plan…",
    Items =
    {
        new SelectItem("Free",  "free"),
        new SelectItem("Pro",   "pro"),
        new SelectItem("Team",  "team"),
    },
};

var terms = new CheckBox { Content = "I accept the terms", Color = LoamColor.Primary };

var form = new Form
{
    Title      = "Create your account",
    Subtitle   = "We'll only use this to set things up.",
    HelperText = "All fields are required.",
    ErrorText  = "Please fix the highlighted fields.",
    SubmitText = "Sign up",
    ResetText  = "Clear",
    SubmitIcon = Icons.Material.Filled.Check,
    ActionsHorizontalAlignment = HorizontalAlignment.Right,
    Children   = { name, email, phone, plan, terms },
};

form.Submitted += (_, _) =>
{
    // Form.Validate() (already run) covers name/email/phone (TextField + MaskedTextField).
    // Select and CheckBox are not auto-validated — check them here.
    var ready = form.IsValid && plan.Value is not null && terms.IsChecked == true;
    if (ready)
    {
        // submit the account
    }
};
```

## Accessibility & keyboard

Inputs are keyboard-operable, with behavior grounded in each control's key handling:

- **Tab order & focus** — every input is focusable and shows a focus adorner; <kbd>Tab</kbd> /
  <kbd>Shift</kbd>+<kbd>Tab</kbd> move between fields, and `IsEnabled = false` removes a control from the
  tab order.
- **Text fields** — `TextField`, `NumericField`, `MaskedTextField`, and `Autocomplete` accept typing
  directly. `TextField` validates on blur when `Required`/`Validation` is set.
- **`NumericField`** — <kbd>↑</kbd>/<kbd>↓</kbd> step the value by `Step` (clamped to the range).
- **`Select`** — <kbd>Enter</kbd> or <kbd>Space</kbd> opens the flyout; <kbd>Esc</kbd> closes it.
- **`Autocomplete`** — <kbd>Esc</kbd> closes the suggestion flyout; <kbd>Enter</kbd>/<kbd>Space</kbd>
  re-runs the search to reopen suggestions.
- **`Slider`** — <kbd>←</kbd>/<kbd>↓</kbd> and <kbd>→</kbd>/<kbd>↑</kbd> step `Value`; <kbd>Home</kbd> /
  <kbd>End</kbd> jump to `Minimum` / `Maximum`.
- **`Rating`** — <kbd>←</kbd>/<kbd>→</kbd> (and <kbd>↑</kbd>/<kbd>↓</kbd>) change the score, <kbd>Home</kbd>
  clears to 0, <kbd>End</kbd> sets the max, and <kbd>Space</kbd>/<kbd>Enter</kbd> activate the current star.
  Set `ReadOnly` to suppress all interaction.
- **`ToggleGroup`** — <kbd>←</kbd>/<kbd>→</kbd> (and <kbd>↑</kbd>/<kbd>↓</kbd>) move between segments,
  <kbd>Home</kbd> / <kbd>End</kbd> jump to the first / last, and <kbd>Space</kbd>/<kbd>Enter</kbd> select.
- **`CheckBox` / `Switch` / `Radio`** — inherit their Avalonia base controls, so <kbd>Space</kbd> toggles
  them and tri-state / group behavior comes from the base type.

::: tip Name controls that have no visible label
A `Slider`, `Rating`, or a `Field` wrapping a bare editor may not expose text for assistive technology.
Give them an accessible name so screen readers announce their purpose:

```csharp
using Avalonia.Automation;
using Loam.Controls;

var volume = new Slider { Minimum = 0, Maximum = 100, Value = 40 };
AutomationProperties.SetName(volume, "Volume");
```

`ToggleGroup` already derives each segment's automation name from its `Text`/`Value`.
:::

## See also

- [Buttons & menus → ButtonGroup](./buttons#buttongroup) — grouped action buttons (vs. `ToggleGroup`'s value selection).
- [Buttons & menus → ToggleIconButton](./buttons#toggleiconbutton) — a single icon-based boolean toggle.
- [Display primitives](./display) — `Icon`, `Chip`, and adornment glyphs used inside fields.
- [Theming](/guide/theming) — how `Variant`, `Color`, and `Size` resolve to tokens.
