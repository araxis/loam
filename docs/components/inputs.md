---
title: Form inputs
---

# Form inputs

Loam's input controls map the MudBlazor form surface to Avalonia-native `TemplatedControl` and control
subclasses. Shared enums (`LoamColor`, `Variant`, `LoamSize`) live in the `Loam` namespace; all controls
are in `Loam.Controls`.

---

## TextField

Material text input equivalent to MudBlazor's `MudTextField`. Wraps an Avalonia `TextBox` with an
optional label, helper/error text, and Text/Filled/Outlined variant chrome that highlights in `Color`
on focus and switches to the error color when `Error` is set. Validates automatically on blur when
`Required` or `Validation` is set.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Text` | `string?` | `null` | The text value (two-way). |
| `Label` | `string?` | `null` | Floating label above the field. |
| `Placeholder` | `string?` | `null` | Placeholder text shown when the field is empty. |
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
```

---

## NumericField

Numeric text input with spinner buttons, equivalent to MudBlazor's `MudNumericField`. Shares the
`TextField` Material chrome and adds `Minimum`/`Maximum` clamping, a `Step` increment/decrement,
and optional .NET format-string display.

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
to MudBlazor's masked `MudTextField`/`MudMask`. Inherits all `TextField` properties.

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

Dropdown single-select control equivalent to MudBlazor's `MudSelect`/`MudSelectItem`. An outlined field
shows the chosen option's display text; clicking opens a flyout list built from the `Items` collection.

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
| `Items` | `ObservableCollection<SelectItem>` | empty | The available options. |

```csharp
using Loam;
using Loam.Controls;

var select = new Select
{
    Label       = "Country",
    Placeholder = "Choose one…",
    Items =
    {
        new SelectItem("Canada",        "ca"),
        new SelectItem("United States", "us"),
        new SelectItem("Mexico",        "mx"),
    },
};
select.Items.Add(new SelectItem("Brazil", "br"));
```

---

## Autocomplete

Free-text input with a filtered suggestion flyout, equivalent to MudBlazor's `MudAutocomplete`. Wraps
a `TextField` for Material chrome and opens a `Flyout` listing `Items` entries that contain the typed
text (case-insensitive); choosing one fills the field.

### Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Value` | `string?` | `null` | The current text value (two-way). |
| `Label` | `string?` | `null` | Field label. |
| `Placeholder` | `string?` | `null` | Placeholder shown when empty. |
| `Variant` | `Variant` | `Variant.Outlined` | Visual chrome style. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Focus accent color. |
| `MaxItems` | `int` | `10` | Maximum suggestions shown in the flyout. |
| `Items` | `ObservableCollection<string>` | empty | The full candidate list. |
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
```

---

## CheckBox

Material checkbox equivalent to MudBlazor's `MudCheckBox`. Subclasses Avalonia's `CheckBox` (inheriting
tri-state, keyboard toggle, and `IsChecked`) and renders a token-colored box and checkmark scaled by
`Size`.

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

Material toggle switch equivalent to MudBlazor's `MudSwitch`. Subclasses Avalonia's `ToggleButton`
(toggle behavior, `:checked` pseudo-class) and renders a tinted sliding track and thumb.

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

`Radio` is a Material radio button equivalent to MudBlazor's `MudRadio`. It subclasses Avalonia's
`RadioButton` and renders a token-colored ring and dot. `RadioGroup` is a `Decorator` that coordinates
child `Radio` controls and exposes the chosen option's value as a two-way `Value` property, mirroring
MudBlazor's `MudRadioGroup`.

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

---

## Slider

Horizontal drag slider equivalent to MudBlazor's `MudSlider`. Renders a custom track, fill, and
draggable thumb; pointer drag and click both set `Value` within `Minimum`/`Maximum`.

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

Star rating control equivalent to MudBlazor's `MudRating`. Shows `MaxValue` stars filled up to
`SelectedValue` (with hover preview); set `ReadOnly` to display a fixed score without interaction.

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

Segmented single-select control equivalent to MudBlazor's `MudToggleGroup`/`MudToggleItem`. Renders
`Items` as connected border segments; the segment whose value equals `SelectedValue` is filled with
`Color`.

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

File picker control equivalent to MudBlazor's `MudFileUpload`. A button opens the platform file picker
via Avalonia's `IStorageProvider`; chosen files are exposed as `Files` and their names rendered as
chips. The `FilesSelected` event fires after each successful pick.

### Properties and members

| Member | Type | Default | Description |
|---|---|---|---|
| `ButtonText` | `string` | `"Upload files"` | Caption of the picker button. |
| `AllowMultiple` | `bool` | `true` | Whether multiple files may be selected. |
| `Files` | `IReadOnlyList<IStorageFile>` | empty | Last picked files (read-only). |
| `FileNames` | `IReadOnlyList<string>` | empty | Display names of the current selection (read-only). |
| **Event** `FilesSelected` | `Action<IReadOnlyList<IStorageFile>>?` | — | Raised after a successful pick. |
| **Method** `ShowSelection(names)` | `void` | — | Manually sets the displayed chip list without re-opening the picker. |
| **Method** `Clear()` | `void` | — | Clears `Files`, `FileNames`, and the chip display. |

```csharp
using Loam.Controls;

var upload = new FileUpload
{
    ButtonText    = "Choose images",
    AllowMultiple = true,
};
upload.FilesSelected += files =>
{
    foreach (var f in files)
        Console.WriteLine(f.Name);
};
```

---

## Form

Lightweight form container equivalent to MudBlazor's `MudForm`. Wraps any layout panel in a `Decorator`
and provides a single `Validate()` call that walks visual descendants, triggers each `TextField`'s
validation, and sets `IsValid`.

### Properties and methods

| Member | Type | Default | Description |
|---|---|---|---|
| `Child` | `Control?` | `null` | The content containing input controls (inherited from `Decorator`). |
| `IsValid` | `bool` | `true` | `true` after `Validate()` if all `TextField` descendants passed. |
| **Method** `Validate()` | `bool` | — | Runs `TextField.Validate()` on every descendant; returns `true` when all pass and updates `IsValid`. |

> `Form` currently validates `TextField` descendants only (including `MaskedTextField`). `NumericField`,
> `Select`, and other controls are not automatically validated by `Form.Validate()`.

```csharp
using Loam;
using Loam.Controls;
using Avalonia.Controls;

var nameField  = new TextField { Label = "Name",  Required = true };
var emailField = new TextField
{
    Label      = "Email",
    Required   = true,
    Validation = v => v?.Contains('@') == true ? null : "Invalid email",
};

var form = new Form
{
    Child = new StackPanel
    {
        Spacing  = 16,
        Children = { nameField, emailField },
    },
};

// On submit:
if (form.Validate())
{
    // all fields are valid
}
```
