---
title: Overlays & feedback
---

# Overlays & feedback

This page covers all overlay and feedback controls in Loam: modal dialogs, snackbar notifications, the full-area overlay scrim, popovers, tooltips, and the inline feedback controls (alert banners, progress indicators, skeleton placeholders, and collapse containers). All controls live in the `Loam.Controls` namespace; enums (`LoamColor`, `LoamSize`, `Variant`, `Typo`) are in the `Loam` namespace.

```csharp
using Loam;          // LoamColor, LoamSize, Variant, Typo, Icons
using Loam.Controls; // DialogService, SnackbarService, Overlay, Popover, Alert, …
```

::: tip Mental model
There are two families here. **Services** (`DialogService`, `SnackbarService`) render *into the
window's overlay layer* — you call them imperatively and they manage their own surface, scrim, and
lifetime. **Controls** (`Overlay`, `Popover`, `Tooltip`, `Alert`, the progress/skeleton/collapse set)
are things you place in the visual tree yourself and toggle with a property. Reach for a service when
the interaction is transient and not part of your layout; reach for a control when the feedback lives
inside a specific view.
:::

## Choosing a surface

Several of these controls overlap. The decision usually comes down to *how much the user is interrupted*
and *who owns the surface in the tree*.

| Use | When | Reach for |
| --- | --- | --- |
| Blocking decision | The user must answer before continuing (confirm a delete, save changes) | [`DialogService`](#dialogservice-idialogservice) |
| Transient confirmation | "Saved", "Copied", an undoable action — no interruption | [`SnackbarService`](#snackbarservice-isnackbar) |
| Manual scrim over a view | Dim a region while something runs (a spinner, a custom panel) | [`Overlay`](#overlay) |
| Anchored rich content | A details card, mini-form, or picker hung off a button | [`Popover`](#popover) |
| One-line hint on hover | Explain an icon-only control | [`Tooltip`](#tooltip) |
| Persistent inline message | A banner that stays in the layout (warning, info, error) | [`Alert`](#alert) |
| Determinate / busy progress | Show how far along, or that work is happening | [`ProgressLinear`](#progresslinear) / [`ProgressCircular`](#progresscircular) |
| Loading placeholder | Reserve layout while content streams in | [`Skeleton`](#skeleton) |
| Show / hide a region | Reveal secondary content in place | [`Collapse`](#collapse) |
| Searchable action launcher | Ctrl+K-style "jump to command" | [`CommandPalette`](#commandpalette) |

::: tip Dialog vs Snackbar
If the user *must* act, use a `DialogService` — it blocks and returns a result you `await`. If you're
just acknowledging something that already happened (even with an "Undo"), use a `SnackbarService`. A
snackbar that demands a decision will be missed when it auto-dismisses.
:::

---

## DialogService / IDialogService

Mirrors the reference API's `IDialogService`. Renders a scrim and a centered `Paper` dialog directly into the window's `OverlayLayer` — no provider component is required. Create an instance with the `DialogService.For(visual)` factory from any attached control.

**Use it when** the user must make a decision or complete a focused task before continuing — confirming a destructive action, resolving unsaved changes, or filling a small modal form.

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
| `MaxWidth` | `double` | `560` | Maximum dialog width. |
| `MinWidth` | `double` | `280` | Minimum dialog width. |
| `MaxHeight` | `double` | `double.PositiveInfinity` | Maximum dialog height. |
| `Margin` | `Thickness` | `new(24)` | Outer spacing from the window edge. |
| `Padding` | `Thickness` | `new(24)` | Dialog surface padding. |
| `DismissOnScrimClick` | `bool` | `true` | Whether clicking the backdrop scrim cancels the dialog. |
| `DismissOnEscape` | `bool` | `true` | Whether pressing Escape cancels the dialog. |
| `AutoFocus` | `bool` | `true` | Whether the first enabled focusable child receives focus when the dialog opens. |

::: warning Don't let a destructive flow dismiss itself
For an irreversible action, set both `DismissOnScrimClick = false` and `DismissOnEscape = false` so a
stray click or keystroke can't silently cancel — force the user to choose a real button.
:::

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
        var field = new TextField { Label = "Name", Text = "Current name" };
        var ok = new Button { Content = "OK", Variant = Variant.Text };
        ok.Click += (_, _) => instance.Ok(field.Text);
        return new StackPanel { Spacing = 12, Children = { field, ok } };
    },
    new DialogOptions
    {
        Width = 400,
        MaxWidth = 520,
        DismissOnEscape = true,
    });

if (!result.Canceled)
{
    var name = result.DataAs<string>();
}
```

---

## SnackbarService / ISnackbar

Mirrors the reference API's `ISnackbar`. Stacks auto-dismissing snackbar surfaces in the window's overlay layer. Create an instance with `SnackbarService.For(visual)`.

**Use it when** you want to acknowledge an action without interrupting — "Record saved", a failed
validation, or an undoable change with an inline "Undo" action.

The service keeps at most `MaxVisible` toasts on screen (default **3**) and stacks them at
`Position` (default `SnackbarPosition.BottomRight`); newer toasts trim the oldest. Both can be set per
toast through `SnackbarOptions`.

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
| `Add` | `void Add(SnackbarOptions options)` | Queues a toast with action text, callback, duration, severity, and visible-count options. |

### SnackbarOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Message` | `string` | required | Toast message text. |
| `Severity` | `LoamColor` | `LoamColor.Info` | Toast color. |
| `Duration` | `TimeSpan?` | `null` | Visible duration. `null` uses 4 seconds; `Timeout.InfiniteTimeSpan` keeps the toast until dismissed. |
| `ActionText` | `string?` | `null` | Optional action button text. |
| `Action` | `Action?` | `null` | Invoked when the action button is clicked; the toast then dismisses. |
| `MaxVisible` | `int?` | `null` | Maximum visible toast count after this toast is added. Uses the service default when null. |
| `DismissText` | `string?` | `null` | Optional dismiss button text. Escape still dismisses the snackbar. |
| `Position` | `SnackbarPosition?` | `null` | Optional stack placement for this snackbar. Uses the service default when null. |

`SnackbarPosition` values: `BottomRight` (default), `BottomLeft`, `TopRight`, `TopLeft`, `BottomCenter`, `TopCenter`.

::: tip Pair "Undo" with a longer duration
An undoable toast is only useful if the user can reach the button before it disappears. When you set
`ActionText`/`Action`, bump `Duration` (e.g. 8 seconds) — or use `Timeout.InfiniteTimeSpan` with a
`DismissText` so it waits for the user.
:::

### Example

```csharp
using Loam;
using Loam.Controls;

ISnackbar snackbar = SnackbarService.For(this);

snackbar.Add("Record saved.");
snackbar.Add("Validation failed.", LoamColor.Error);
snackbar.Add("Processing…", LoamColor.Warning, TimeSpan.FromSeconds(8));

snackbar.Add(new SnackbarOptions("Item archived")
{
    Severity = LoamColor.Info,
    ActionText = "Undo",
    Action = () => RestoreItem(),
    DismissText = "Dismiss",
    Position = SnackbarPosition.BottomCenter,
    Duration = TimeSpan.FromSeconds(8),
    MaxVisible = 3,
});

snackbar.Add(new SnackbarOptions("Waiting for approval")
{
    Duration = Timeout.InfiniteTimeSpan,
    DismissText = "Close",
    Position = SnackbarPosition.TopCenter,
});
```

---

## Overlay

Mirrors the reference API's `Overlay`. A `ContentControl` that fills its parent with a translucent scrim and centers its content over it. Toggled by the two-way `Visible` property.

**Use it when** you need to dim a region of your own layout and float something over it — a loading
spinner, a custom blocking panel — without going through the dialog service. For a *managed* modal with
a result, prefer [`DialogService`](#dialogservice-idialogservice).

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Visible` | `bool` | `false` | Shows or hides the scrim (two-way). |
| `DarkBackground` | `bool` | `false` | Uses the darker `OverlayDark` scrim token instead of the default translucent `OverlayLight` one. |
| `AutoClose` | `bool` | `false` | Sets `Visible = false` when the scrim is clicked or Escape is pressed while enabled. |
| `OnClick` | `Action?` | `null` | Invoked when the enabled scrim is clicked or Escape closes an auto-close overlay. |

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

var manualOverlay = new Overlay
{
    AutoClose = false,
    DarkBackground = true,
};
var close = new Button { Content = "Close" };
close.Click += (_, _) => manualOverlay.Visible = false;
manualOverlay.Content = close;
```

::: warning A busy overlay should not auto-close
When the overlay is masking work in progress (an indeterminate spinner), leave `AutoClose = false` so a
scrim click or Escape can't dismiss the scrim while the operation is still running.
:::

---

## Popover

Mirrors the reference API's `Popover`. A `Decorator` wrapping an Avalonia `Popup`. Set `Content`, optionally `Target` and `Placement`, then toggle the two-way `Open` property. Assign `Trigger` when the popover should open from a button or other control without custom event wiring. Light-dismiss automatically sets `Open = false`; Escape closes the open surface while the popover is enabled.

**Use it when** you want richer, interactive content anchored to a control — a details card, a small
form, a color picker. For a single line of explanatory text on hover, use [`Tooltip`](#tooltip)
instead.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Content` | `object?` | `null` | The popover body, wrapped in an elevated `Paper`. |
| `Open` | `bool` | `false` | Whether the popover is shown (two-way). |
| `Placement` | `PlacementMode` | `Bottom` | Where the popover sits relative to the target. |
| `Target` | `Control?` | `null` | The anchor control. Defaults to the popover's logical parent. |
| `Trigger` | `Control?` | `null` | Optional control that toggles `Open` on pointer click or keyboard activation. |

### Example

```csharp
using Avalonia.Controls;
using Loam;
using Loam.Controls;

var button = new Button { Content = "Open details" };
var popover = new Popover
{
    Trigger = button,
    Placement = PlacementMode.BottomEdgeAlignedLeft,
    Content = new StackPanel
    {
        Spacing = 8,
        Children =
        {
            new Text { Text = "Project details", Typo = Typo.Subtitle1 },
            new Text { Text = "Escape or light-dismiss closes this surface." },
        },
    },
};
```

---

## Tooltip

Mirrors the reference API's `Tooltip`. A static helper that attaches a Loam-styled tooltip (small elevated `Paper` with `Caption` typography) to any `Control`, wrapping Avalonia's built-in `ToolTip`.

**Use it when** a control needs a short, non-interactive hint — most often to name an icon-only
button. A tooltip is supplementary; never hide essential information behind one, since it only appears
on hover/focus.

### Methods

| Member | Signature | Description |
|--------|-----------|-------------|
| `Tooltip.Set` | `static void Set(Control control, string text)` | Attaches a text tooltip to `control`. |

### Example

```csharp
using Loam.Controls;

var icon = new Icon { Data = Icons.Material.Filled.Info };
Tooltip.Set(icon, "More information");
```

---

## Alert

Contextual message banner colored by severity (`Color`) and styled by `Variant`. Use the generated
`Title`, `Message`, `Action`, and `Closeable` regions for standard alert anatomy, or keep using raw
`Content` for compatibility. Closeable alerts use a generated icon button with keyboard access and
raise `Closed` after `Close()` hides the alert.

**Use it when** a message should stay in the layout until the situation changes or the user dismisses
it — a validation summary, a degraded-state warning, an informational banner. For transient
acknowledgements, use a [`SnackbarService`](#snackbarservice-isnackbar) instead.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Color` | `LoamColor` | `LoamColor.Info` | Severity color (mirrors the reference API's `Severity`). |
| `Variant` | `Variant` | `Variant.Text` | Visual style: `Filled`, `Outlined`, or `Text` (tinted background). |
| `Icon` | `string?` | `null` | Optional leading icon path. |
| `Title` | `string?` | `null` | Generated alert title. |
| `Message` | `string?` | `null` | Generated alert body text. |
| `Action` | `object?` | `null` | Trailing action content, usually a text button. |
| `Closeable` | `bool` | `false` | Shows a trailing close button. |
| `CloseIcon` | `string?` | close icon | Icon path used by the generated close button. |
| `Close()` | `void` | — | Hides a closeable enabled alert and raises `Closed`. |
| `Closed` | `event EventHandler?` | — | Raised after the generated close action or `Close()` hides the alert. |
| `Content` | `object?` | — | The message content (inherited from `ContentControl`). |

### Example

```csharp
using Loam;
using Loam.Controls;

var alert = new Alert
{
    Color = LoamColor.Warning,
    Variant = Variant.Outlined,
    Icon = Icons.Material.Filled.Warning,
    Title = "Low disk space",
    Message = "Archive old build artifacts before continuing.",
    Action = new Button { Content = "Review", Variant = Variant.Text, Color = LoamColor.Warning },
    Closeable = true,
};
alert.Closed += (_, _) => viewModel.DismissWarning();
```

---

## ProgressLinear

Mirrors the reference API's `ProgressLinear`. A horizontal progress bar tinted by `Color`; use `Indeterminate = true` for a moving fill when no value is available. The bar resolves its track, fill, disabled state, motion, and size metrics from theme tokens.

**Use it when** progress is tied to a width — a page-top load bar, an upload row, a stepper. Set a
known `Value` for determinate work; switch to `Indeterminate` when you can't estimate completion.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `double` | `0` | Current progress value (mirrors the reference API's `Value`). |
| `Minimum` | `double` | `0` | Lower bound (mirrors the reference API's `Min`). |
| `Maximum` | `double` | `100` | Upper bound (mirrors the reference API's `Max`). |
| `Color` | `LoamColor` | `LoamColor.Primary` | Accent color of the fill bar. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Track thickness: `ExtraSmall` = 2 px, `Small` = 3 px, `Medium` = 4 px, `Large` = 6 px, `ExtraLarge` = 8 px. |
| `Indeterminate` | `bool` | `false` | Shows an animated moving fill instead of the fixed value. |
| `Label` | `string?` | `null` | Optional generated label shown above the track and used as the automation name. |
| `ShowValue` | `bool` | `false` | Shows generated value text beside `Label`. |
| `ValueText` | `string?` | `null` | Explicit value text. When unset, `ValueTextFormat` formats the percentage. |
| `ValueTextFormat` | `string` | `"{0:0}%"` | Format string for generated percentage text. |

### Example

```csharp
using Loam;
using Loam.Controls;

var progress = new ProgressLinear
{
    Label = "Upload",
    ShowValue = true,
    Color = LoamColor.Success,
    Size = LoamSize.Medium,
};
progress.Bind(ProgressLinear.ValueProperty, viewModel.GetObservable(vm => vm.UploadPercent));

var loading = new ProgressLinear
{
    Label = "Loading records",
    ShowValue = true,
    Indeterminate = true,
    Width = 240,
};

var compact = new ProgressLinear
{
    Label = "Compact sync",
    ShowValue = true,
    Size = LoamSize.ExtraSmall,
    Value = 48,
};
```

---

## ProgressCircular

Mirrors the reference API's `ProgressCircular`. Draws an arc tinted by `Color`: a determinate sweep from `Value`, or a continuously spinning arc when `Indeterminate` (the default).

**Use it when** progress sits in a compact spot — inside a button, an [`Overlay`](#overlay), or a card
— where a circular indicator reads better than a bar. Note it defaults to `Indeterminate = true`; set
it `false` to show a `Value`.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `double` | `0` | Current progress value (determinate mode). |
| `Minimum` | `double` | `0` | Lower bound. |
| `Maximum` | `double` | `100` | Upper bound. |
| `Color` | `LoamColor` | `LoamColor.Primary` | Accent color of the arc. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Indicator diameter: `ExtraSmall` = 24 px, `Small` = 32 px, `Medium` = 48 px, `Large` = 64 px, `ExtraLarge` = 80 px. |
| `StrokeWidth` | `double` | `0` | Arc stroke thickness in pixels. `0` uses the size-resolved default (`8.3333%` of diameter). |
| `Indeterminate` | `bool` | `true` | Spins indefinitely when `true`; shows a fraction of `Value` when `false`. |
| `Label` | `string?` | `null` | Accessible name for the indicator. |
| `ShowValue` | `bool` | `false` | Draws generated value text in the center for determinate indicators. |
| `ValueText` | `string?` | `null` | Explicit value text. When unset, `ValueTextFormat` formats the percentage. |
| `ValueTextFormat` | `string` | `"{0:0}%"` | Format string for generated value text. |

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
    Label = "Upload progress",
    ShowValue = true,
};

var compact = new ProgressCircular
{
    Size = LoamSize.Small,
    StrokeWidth = ProgressCircular.DefaultStrokeWidth(LoamSize.Small),
    Label = "Sync progress",
};
```

---

## Skeleton

Mirrors the reference API's `Skeleton`. A themed placeholder block shown while content is loading. Extends `Border` with a skeleton palette color and rounded corners. Use the public factories for common loading anatomy, or set `Circle = true` for a custom round placeholder.

**Use it when** content is on its way and you want to reserve its layout — a list row, an avatar, a
card — so the page doesn't jump when data arrives. For "something is happening but I don't know the
shape", a [progress indicator](#progresscircular) is the better fit.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Circle` | `bool` | `false` | Renders as a fully rounded circle (avatar placeholder). |
| `Animate` | `bool` | `true` | Enables the subtle loading shimmer. Set `false` for a static placeholder. |
| `Preset` | `SkeletonPreset` | `Custom` | Generated anatomy: `Text`, `Avatar`, `Button`, `Thumbnail`, or `Card`. |
| `Size` | `LoamSize` | `LoamSize.Medium` | Size token used by text, avatar, and button presets. |
| `Label` | `string?` | `null` | Accessible loading label. |
| `Height` | `double` | `16` | Block height (inherited from `Border`). |
| `Width` | `double` | — | Block width (inherited from `Border`). |

### Example

```csharp
using Loam.Controls;

// Text line placeholder
var line = Skeleton.TextLine(200, LoamSize.Medium, label: "Title loading");

// Avatar placeholder
var avatar = Skeleton.Avatar(LoamSize.Medium, label: "Avatar loading");

// Static placeholder
var staticLine = Skeleton.TextLine(160, LoamSize.Small, animate: false, label: "Subtitle loading");

// Media and card placeholders
var thumbnail = Skeleton.Thumbnail(128, 84, label: "Thumbnail loading");
var card = Skeleton.Card(260, 96, animate: false, label: "Card loading");
```

---

## Collapse

Mirrors the reference API's `Collapse`. A `Decorator` that reveals its single `Child` when `Expanded`, clipping it to zero height when collapsed.

**Use it when** you want to hide secondary content in place and reveal it on demand — an expander
section, "show more" details, an inline edit panel. The reveal is animated by default; set
`Animated = false` (or `Duration = TimeSpan.Zero`) for reduced-motion scenarios.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Expanded` | `bool` | `false` | Whether the child is visible (two-way). `false` clips content to zero height. |
| `Animated` | `bool` | `true` | Enables a short height reveal/collapse animation when the control is enabled and duration is greater than zero. |
| `Duration` | `TimeSpan` | `180 ms` | Reveal/collapse duration. `TimeSpan.Zero` resolves immediately for reduced-motion scenarios. |
| `Child` | `Control?` | — | The content to show/hide (inherited from `Decorator`). |

### Example

```csharp
using Loam.Controls;

var toggle = new Button { Content = "Toggle details" };
var collapse = new Collapse
{
    Duration = TimeSpan.FromMilliseconds(220),
    Child = new TextBlock { Text = "Hidden details shown when expanded." },
};
toggle.Click += (_, _) => collapse.Expanded = !collapse.Expanded;

var staticCollapse = new Collapse
{
    Animated = false,
    Expanded = true,
    Child = new TextBlock { Text = "Shown immediately without motion." },
};
```

## CommandPalette

A searchable command palette: a search field over a live-filtered list of commands, with keyboard navigation (Down/Up to move, Enter to run, Escape to close). Host it inside an `Overlay`, a dialog, or place it inline. Matching is exposed as the pure static `CommandPalette.Filter(commands, query)` (case-insensitive contains on `Title` or any `Keywords`).

**Use it when** there are many actions a power user might want to reach quickly — a Ctrl+K launcher.
Hosting it in an [`Overlay`](#overlay) gives you the familiar dimmed, click-away-to-close behavior.

### CommandPalette properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Commands` | `AvaloniaList<CommandPaletteItem>` | `(empty)` | The commands to search. |
| `FilterText` | `string?` | `null` | The current query (two-way; bound to the search field). |
| `IsOpen` | `bool` | `true` | Whether the palette is shown (two-way). Escape sets it `false`. |
| `SelectedIndex` | `int` (get) | `-1` | Highlighted index within the filtered results. |
| `FilteredCommands` | `IReadOnlyList<CommandPaletteItem>` (get) | — | The current filtered results. |
| `Invoked` | event | — | Raised when a command is chosen (Enter or click). |
| `Closed` | event | — | Raised when dismissed (Escape). |

`CommandPaletteItem` carries `Title`, optional `Icon` and `Keywords`, and an `OnInvoke` callback.

```csharp
using Loam.Controls;

var palette = new CommandPalette
{
    Commands =
    {
        new CommandPaletteItem { Title = "New item", Icon = Icons.Material.Filled.Add, Keywords = ["create"] },
        new CommandPaletteItem { Title = "Toggle dark mode", Icon = Icons.Material.Filled.DarkMode, Keywords = ["theme"] },
        new CommandPaletteItem { Title = "Settings", Icon = Icons.Material.Filled.Settings },
    },
};
palette.Invoked += (_, command) => Run(command);
```

---

## Recipe: a long-running task with progress and feedback

A typical end-to-end flow: confirm the action with a dialog, dim the view with an `Overlay` carrying a
`ProgressCircular` while the work runs, then acknowledge the result with a snackbar. Everything is
plain C# — wire the pieces from a button click handler.

```csharp
using Avalonia.Controls;
using Loam;
using Loam.Controls;

// 'this' is a control already attached to the window.
var busy = new Overlay
{
    DarkBackground = true,
    AutoClose = false, // don't let a click dismiss the scrim mid-run
    Content = new ProgressCircular
    {
        Color = LoamColor.Primary,
        Size = LoamSize.Large,
        Label = "Publishing",
    },
};
rootPanel.Children.Add(busy); // overlay fills its parent

var publish = new Button
{
    Content = "Publish",
    Variant = Variant.Filled,
    Color = LoamColor.Primary,
    StartIcon = Icons.Material.Filled.CloudUpload,
};

publish.Click += async (_, _) =>
{
    bool ok = await DialogService.For(this).ConfirmAsync(
        "Publish changes",
        "This will make your edits live.",
        okText: "Publish",
        cancelText: "Not yet");
    if (!ok)
    {
        return;
    }

    busy.Visible = true;
    try
    {
        await viewModel.PublishAsync();
        SnackbarService.For(this).Add("Published.", LoamColor.Success);
    }
    catch
    {
        SnackbarService.For(this).Add(new SnackbarOptions("Publish failed")
        {
            Severity = LoamColor.Error,
            ActionText = "Retry",
            Action = () => publish.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)),
            Duration = TimeSpan.FromSeconds(8),
        });
    }
    finally
    {
        busy.Visible = false;
    }
};
```

## Accessibility & keyboard

These controls cover the focus and keyboard behavior you'd expect from overlay UI:

- **Dialogs** — when `AutoFocus` is `true` (the default) the first enabled focusable child receives focus on open. <kbd>Esc</kbd> cancels while `DismissOnEscape` is `true`; clicking the scrim cancels while `DismissOnScrimClick` is `true`. Turn both off for irreversible actions.
- **Snackbars** — each toast is focusable and announced to assistive technology (its message is the automation name). <kbd>Esc</kbd> dismisses the focused toast even when no `DismissText` button is shown.
- **Overlay** — when `AutoClose` is enabled, <kbd>Esc</kbd> and a scrim click both set `Visible = false` and invoke `OnClick`. Leave `AutoClose` off to make the scrim non-dismissible.
- **Popover** — assigning a `Trigger` makes it open via <kbd>Space</kbd>/<kbd>Enter</kbd> as well as click. <kbd>Esc</kbd> closes the open surface, and clicking outside light-dismisses it; placement is reflected to assistive tech as help text.
- **CommandPalette** — <kbd>↓</kbd>/<kbd>↑</kbd> move the highlight, <kbd>Enter</kbd> runs the highlighted command, <kbd>Esc</kbd> closes the palette.
- **Alert** — a `Closeable` alert's close affordance is a keyboard-accessible icon button; `Close()` and the button both raise `Closed`.

::: tip Name your indicators and icon triggers
A bare [`ProgressCircular`](#progresscircular)/[`ProgressLinear`](#progresslinear) or
[`Skeleton`](#skeleton) has no text for a screen reader. Set `Label` so the busy/loading state is
announced. Likewise, give an icon-only [`Popover`](#popover) trigger an accessible name:

```csharp
using Avalonia.Automation;

var spinner = new ProgressCircular { Label = "Loading results" };

var info = new IconButton { Icon = Icons.Material.Filled.Info };
AutomationProperties.SetName(info, "Show details");
```
:::

## See also

- [Buttons & menus](./buttons) — `Button`, `IconButton`, and `Menu` triggers used throughout these examples.
- [Form inputs](./inputs) — `TextField` and friends for dialog form content.
- [Display primitives](./display) — `Text`, `Icon`, `Paper`, and the glyph set behind `Icon`/`StartIcon`.
- [Components overview → common parameters](./overview#common-parameters) — how `Color`, `Size`, and `Variant` behave across controls.
- [Theming](/guide/theming) — how severity colors and motion resolve to tokens.
