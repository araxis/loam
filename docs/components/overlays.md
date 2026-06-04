---
title: Overlays & feedback
---

# Overlays & feedback

This page covers all overlay and feedback controls in Loam: modal dialogs, snackbar notifications, the full-area overlay scrim, popovers, tooltips, and the inline feedback controls (alert banners, progress indicators, skeleton placeholders, and collapse containers). All controls live in the `Loam.Controls` namespace; enums (`LoamColor`, `LoamSize`, `Variant`) are in the `Loam` namespace.

---

## DialogService / IDialogService

Mirrors the reference API's `IDialogService`. Renders a scrim and a centered `Paper` dialog directly into the window's `OverlayLayer` — no provider component is required. Create an instance with the `DialogService.For(visual)` factory from any attached control.

### Factory

```csharp
using Loam.Controls;

// From inside a view or control already attached to a window:
IDialogService dialogs = DialogService.For(this);
```

### Methods

| Member | Signature | Description |
|--------|-----------|-------------|
| `DialogService.For` | `static DialogService For(Visual visual)` | Creates a service targeting the window that hosts `visual`. Throws if the visual is not attached. |
| `ShowAsync` | `Task<DialogResult> ShowAsync(string? title, Func<DialogInstance, Control> content, DialogOptions? options = null)` | Shows a fully custom dialog. The factory receives a `DialogInstance` so the content can close itself. Resolves when the dialog is closed. |
| `ConfirmAsync` | `Task<bool> ConfirmAsync(string title, string message, string okText = "OK", string cancelText = "Cancel")` | Shows a yes/no confirmation dialog. Resolves `true` when confirmed, `false` when canceled. |
| `MessageBoxAsync` | `Task<bool?> MessageBoxAsync(string title, string message, string yesText = "OK", string? noText = null, string? cancelText = null)` | Shows a message box with up to three buttons. Resolves `true` (yes), `false` (no), or `null` (cancel/dismissed). Omit `noText`/`cancelText` to hide those buttons. |

### DialogOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Width` | `double?` | `null` | Fixed dialog width. When `null` the dialog sizes to content (capped at 560 px). |
| `DismissOnScrimClick` | `bool` | `true` | Whether clicking the backdrop scrim cancels the dialog. |

### DialogInstance

Passed to the `ShowAsync` content factory so the hosted control can close the dialog programmatically.

| Member | Description |
|--------|-------------|
| `Ok(object? data = null)` | Closes with a successful `DialogResult` carrying optional data. |
| `Cancel()` | Closes with a canceled `DialogResult`. |
| `Close(DialogResult result)` | Closes with an explicit result. |

### DialogResult

| Member | Type | Description |
|--------|------|-------------|
| `Canceled` | `bool` | `true` when the dialog was dismissed or canceled. |
| `Data` | `object?` | The data returned by `Ok(data)`, or `null`. |
| `DataAs<T>()` | `T?` | Casts `Data` to `T`, or returns `default`. |
| `DialogResult.Ok(object? data)` | static | Creates a successful result. |
| `DialogResult.Cancel()` | static | Creates a canceled result. |

### Examples

```csharp
using Loam.Controls;

// Confirmation dialog
bool confirmed = await DialogService.For(this).ConfirmAsync(
    "Delete item",
    "This action cannot be undone.",
    okText: "Delete",
    cancelText: "Cancel");

// Message box with three buttons
bool? answer = await DialogService.For(this).MessageBoxAsync(
    "Unsaved changes",
    "Do you want to save before closing?",
    yesText: "Save",
    noText: "Don't save",
    cancelText: "Cancel");

// Custom dialog — content receives a DialogInstance to close itself
DialogResult result = await DialogService.For(this).ShowAsync(
    "Edit name",
    instance =>
    {
        var box = new TextBox { Text = "Current name" };
        var ok = new Button { Content = "OK" };
        ok.Click += (_, _) => instance.Ok(box.Text);
        return new StackPanel { Spacing = 12, Children = { box, ok } };
    },
    new DialogOptions { Width = 400 });

if (!result.Canceled)
{
    var name = result.DataAs<string>();
}
```

---

## SnackbarService / ISnackbar

Mirrors the reference API's `ISnackbar`. Stacks auto-dismissing `Alert` toasts at the bottom-right of the window's overlay layer. Create an instance with `SnackbarService.For(visual)`.

### Factory

```csharp
using Loam.Controls;

ISnackbar snackbar = SnackbarService.For(this);
```

### Methods

| Member | Signature | Description |
|--------|-----------|-------------|
| `SnackbarService.For` | `static SnackbarService For(Visual visual)` | Creates a service for the window hosting `visual`. |
| `Add` | `void Add(string message, LoamColor severity = LoamColor.Info, TimeSpan? duration = null)` | Queues a toast. The default duration is 4 seconds. |

### Example

```csharp
using Loam;
using Loam.Controls;

ISnackbar snackbar = SnackbarService.For(this);

snackbar.Add("Record saved.");
snackbar.Add("Validation failed.", LoamColor.Error);
snackbar.Add("Processing…", LoamColor.Warning, TimeSpan.FromSeconds(8));
```

---

## Overlay

Mirrors the reference API's `Overlay`. A `ContentControl` that fills its parent with a translucent scrim and centers its content over it. Toggled by the two-way `Visible` property.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Visible` | `bool` | `false` | Shows or hides the scrim (two-way). |
| `DarkBackground` | `bool` | `false` | Uses a darker scrim (`#99000000`) instead of the light default (`#22000000`). |
| `AutoClose` | `bool` | `false` | Sets `Visible = false` when the scrim is clicked. |
| `OnClick` | `Action?` | `null` | Invoked when the scrim is clicked (fires before `AutoClose` hides it). |

### Example

```csharp
using Loam.Controls;

var overlay = new Overlay
{
    DarkBackground = true,
    AutoClose = true,
    OnClick = () => Console.WriteLine("Scrim clicked"),
    Content = new ProgressCircular(),
};
overlay.Bind(Overlay.VisibleProperty, viewModel.GetObservable(vm => vm.IsLoading));
```

---

## Popover

Mirrors the reference API's `Popover`. A `Decorator` wrapping an Avalonia `Popup`. Set `Content`, optionally `Target` and `Placement`, then toggle the two-way `Open` property. Light-dismiss automatically sets `Open = false`.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Content` | `object?` | `null` | The popover body, wrapped in an elevated `Paper`. |
| `Open` | `bool` | `false` | Whether the popover is shown (two-way). |
| `Placement` | `PlacementMode` | `Bottom` | Where the popover sits relative to the target. |
| `Target` | `Control?` | `null` | The anchor control. Defaults to the popover's logical parent. |

### Example

```csharp
using Avalonia.Controls.Primitives;
using Loam.Controls;

var button = new Button { Content = "Open" };
var popover = new Popover
{
    Target = button,
    Placement = PlacementMode.BottomEdgeAlignedLeft,
    Content = new TextBlock { Text = "Popover content" },
};
button.Click += (_, _) => popover.Open = !popover.Open;
```

---

## Tooltip

Mirrors the reference API's `Tooltip`. A static helper that attaches a Loam-styled tooltip (small elevated `Paper` with `Caption` typography) to any `Control`, wrapping Avalonia's built-in `ToolTip`.

### Methods

| Member | Signature | Description |
|--------|-----------|-------------|
| `Tooltip.Set` | `static void Set(Control control, string text)` | Attaches a text tooltip to `control`. |

### Example

```csharp
using Loam.Controls;

var icon = new Icon { Data = Icons.Info };
Tooltip.Set(icon, "More information");
```

---

## Alert

Mirrors the reference API's `Alert`. A `ContentControl` that renders a contextual message banner colored by severity (`Color`) and styled by `Variant`.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Color` | `LoamColor` | `LoamColor.Info` | Severity color (mirrors the reference API's `Severity`). |
| `Variant` | `Variant` | `Variant.Text` | Visual style: `Filled`, `Outlined`, or `Text` (tinted background). |
| `Icon` | `string?` | `null` | Optional leading icon path. |
| `Content` | `object?` | — | The message content (inherited from `ContentControl`). |

### Example

```csharp
using Loam;
using Loam.Controls;

var alert = new Alert
{
    Color = LoamColor.Warning,
    Variant = Variant.Filled,
    Content = new TextBlock { Text = "Low disk space." },
};
```

---

## ProgressLinear

Mirrors the reference API's `ProgressLinear`. A horizontal determinate progress bar tinted by `Color`.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `double` | `0` | Current progress value (mirrors the reference API's `Value`). |
| `Minimum` | `double` | `0` | Lower bound (mirrors the reference API's `Min`). |
| `Maximum` | `double` | `100` | Upper bound (mirrors the reference API's `Max`). |
| `Color` | `LoamColor` | `LoamColor.Primary` | Accent color of the fill bar. |

### Example

```csharp
using Loam;
using Loam.Controls;

var progress = new ProgressLinear { Color = LoamColor.Success };
progress.Bind(ProgressLinear.ValueProperty, viewModel.GetObservable(vm => vm.UploadPercent));
```

---

## ProgressCircular

Mirrors the reference API's `ProgressCircular`. Draws an arc tinted by `Color`: a determinate sweep from `Value`, or a continuously spinning arc when `Indeterminate` (the default).

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `double` | `0` | Current progress value (determinate mode). |
| `Minimum` | `double` | `0` | Lower bound. |
| `Maximum` | `double` | `100` | Upper bound. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Accent color of the arc. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Indicator diameter: `Small` = 24 px, `Medium` = 40 px, `Large` = 56 px. |
| `StrokeWidth` | `double` | `3` | Arc stroke thickness in pixels. |
| `Indeterminate` | `bool` | `true` | Spins indefinitely when `true`; shows a fraction of `Value` when `false`. |

### Example

```csharp
using Loam;
using Loam.Controls;

// Indeterminate spinner (default)
var spinner = new ProgressCircular { Color = LoamColor.Primary, Size = LoamSize.Large };

// Determinate
var bar = new ProgressCircular
{
    Indeterminate = false,
    Color = LoamColor.Success,
    Value = 72,
};
```

---

## Skeleton

Mirrors the reference API's `Skeleton`. A themed placeholder block shown while content is loading. Extends `Border` with a skeleton palette color and rounded corners. Set `Circle = true` for round avatar placeholders.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Circle` | `bool` | `false` | Renders as a fully rounded circle (avatar placeholder). |
| `Height` | `double` | `16` | Block height (inherited from `Border`). |
| `Width` | `double` | — | Block width (inherited from `Border`). |

### Example

```csharp
using Loam.Controls;

// Text line placeholder
var line = new Skeleton { Width = 200 };

// Avatar placeholder
var avatar = new Skeleton { Circle = true, Width = 40, Height = 40 };
```

---

## Collapse

Mirrors the reference API's `Collapse`. A `Decorator` that reveals its single `Child` when `Expanded`, clipping it to zero height when collapsed.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Expanded` | `bool` | `false` | Whether the child is visible (two-way). `false` clips content to zero height. |
| `Child` | `Control?` | — | The content to show/hide (inherited from `Decorator`). |

### Example

```csharp
using Loam.Controls;

var toggle = new Button { Content = "Toggle details" };
var collapse = new Collapse
{
    Child = new TextBlock { Text = "Hidden details shown when expanded." },
};
toggle.Click += (_, _) => collapse.Expanded = !collapse.Expanded;
```
