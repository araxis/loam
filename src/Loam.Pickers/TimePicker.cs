using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A time input with a popup, mirroring the reference API's <c>TimePicker</c>. A variant field shows
/// the two-way <see cref="Time"/> formatted by <see cref="TimeFormat"/>; clicking it opens a flyout with
/// scrollable hour and minute columns (no FluentTheme dependency).
/// </summary>
public class TimePicker : TemplatedControl
{
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    /// <summary>Identifies the <see cref="Time"/> property.</summary>
    public static readonly StyledProperty<TimeSpan?> TimeProperty =
        AvaloniaProperty.Register<TimePicker, TimeSpan?>(nameof(Time), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<TimePicker, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="Placeholder"/> property.</summary>
    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<TimePicker, string?>(nameof(Placeholder), "Select a time");

    /// <summary>Identifies the <see cref="TimeFormat"/> property.</summary>
    public static readonly StyledProperty<string> TimeFormatProperty =
        AvaloniaProperty.Register<TimePicker, string>(nameof(TimeFormat), "t");

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<TimePicker, Variant>(nameof(Variant), Loam.Variant.Outlined);

    /// <summary>Identifies the <see cref="MinuteStep"/> property.</summary>
    public static readonly StyledProperty<int> MinuteStepProperty =
        AvaloniaProperty.Register<TimePicker, int>(nameof(MinuteStep), 5);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<TimePicker, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Error"/> property.</summary>
    public static readonly StyledProperty<bool> ErrorProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(Error));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<TimePicker, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<TimePicker, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="ShrinkLabel"/> property.</summary>
    public static readonly StyledProperty<bool> ShrinkLabelProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(ShrinkLabel));

    /// <summary>Identifies the <see cref="PickerTitle"/> property.</summary>
    public static readonly StyledProperty<string> PickerTitleProperty =
        AvaloniaProperty.Register<TimePicker, string>(nameof(PickerTitle), "Select time");

    /// <summary>Identifies the <see cref="CancelText"/> property.</summary>
    public static readonly StyledProperty<string> CancelTextProperty =
        AvaloniaProperty.Register<TimePicker, string>(nameof(CancelText), "Cancel");

    /// <summary>Identifies the <see cref="OkText"/> property.</summary>
    public static readonly StyledProperty<string> OkTextProperty =
        AvaloniaProperty.Register<TimePicker, string>(nameof(OkText), "OK");

    private readonly List<TimePickerRow> _hourRows = new();
    private readonly List<TimePickerRow> _minuteRows = new();
    /// <summary>Identifies the <see cref="Clearable"/> property.</summary>
    public static readonly StyledProperty<bool> ClearableProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(Clearable));

    /// <summary>Identifies the <see cref="AdornmentIcon"/> property.</summary>
    public static readonly StyledProperty<string?> AdornmentIconProperty =
        AvaloniaProperty.Register<TimePicker, string?>(nameof(AdornmentIcon));

    /// <summary>Identifies the <see cref="Editable"/> property.</summary>
    public static readonly StyledProperty<bool> EditableProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(Editable));

    /// <summary>Identifies the <see cref="InvalidTimeText"/> property.</summary>
    public static readonly StyledProperty<string> InvalidTimeTextProperty =
        AvaloniaProperty.Register<TimePicker, string>(nameof(InvalidTimeText), "Invalid time");

    /// <summary>Identifies the <see cref="Required"/> property.</summary>
    public static readonly StyledProperty<bool> RequiredProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(Required));

    /// <summary>Identifies the <see cref="RequiredText"/> property.</summary>
    public static readonly StyledProperty<string> RequiredTextProperty =
        AvaloniaProperty.Register<TimePicker, string>(nameof(RequiredText), "Required");

    /// <summary>Identifies the <see cref="Validation"/> property.</summary>
    public static readonly StyledProperty<Func<TimeSpan?, string?>?> ValidationProperty =
        AvaloniaProperty.Register<TimePicker, Func<TimeSpan?, string?>?>(nameof(Validation));

    private Border? _box;
    private IconButton? _clear;
    private IconButton? _clockButton;
    private TextBox? _input;
    private bool _flyoutOpening;
    private Icon? _adornment;
    private Border? _labelHost;
    private Text? _display;
    private Text? _label;
    private Text? _restingLabel;
    private Text? _helper;
    private IDisposable? _displayForeground;
    private IDisposable? _boxBorderBrush;
    private IDisposable? _boxBackground;
    private IDisposable? _labelForeground;
    private IDisposable? _restingLabelForeground;
    private IDisposable? _helperForeground;
    private Flyout? _flyout;
    private bool _flyoutOpen;
    private Text? _popupHourDisplay;
    private Text? _popupMinuteDisplay;
    private ScrollViewer? _hourScroll;
    private ScrollViewer? _minuteScroll;

    /// <summary>Raised when the picker commits a time through the generated OK action.</summary>
    public event Action<TimeSpan?>? TimeSelected;

    /// <summary>Creates the picker.</summary>
    public TimePicker()
    {
        Focusable = true;
        GotFocus += (_, _) => ApplyBoxChrome();
        LostFocus += (_, _) => ApplyBoxChrome();
    }

    /// <summary>The selected time (two-way). Mirrors the reference API's <c>Time</c>.</summary>
    public TimeSpan? Time
    {
        get => GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    /// <summary>When true and a time is set, shows an inline clear (x) button that resets <see cref="Time"/> to null.</summary>
    public bool Clearable
    {
        get => GetValue(ClearableProperty);
        set => SetValue(ClearableProperty, value);
    }

    /// <summary>Optional glyph (e.g. from <see cref="Icons"/>) shown as a leading icon at the start of the field.</summary>
    public string? AdornmentIcon
    {
        get => GetValue(AdornmentIconProperty);
        set => SetValue(AdornmentIconProperty, value);
    }

    /// <summary>
    /// When true, the user can type a time into the field; the trailing clock icon opens the flyout. Typed
    /// entry accepts any minute (it is not snapped to <see cref="MinuteStep"/>, which only constrains the
    /// flyout columns); a time of day outside 0–24h cannot be entered.
    /// </summary>
    public bool Editable
    {
        get => GetValue(EditableProperty);
        set => SetValue(EditableProperty, value);
    }

    /// <summary>Error message shown when typed text cannot be parsed as a time (<see cref="Editable"/> mode).</summary>
    public string InvalidTimeText
    {
        get => GetValue(InvalidTimeTextProperty);
        set => SetValue(InvalidTimeTextProperty, value);
    }

    /// <summary>When true, a null <see cref="Time"/> fails <see cref="Validate"/> with <see cref="RequiredText"/>.</summary>
    public bool Required
    {
        get => GetValue(RequiredProperty);
        set => SetValue(RequiredProperty, value);
    }

    /// <summary>Error message used when <see cref="Required"/> fails.</summary>
    public string RequiredText
    {
        get => GetValue(RequiredTextProperty);
        set => SetValue(RequiredTextProperty, value);
    }

    /// <summary>A validator returning an error message (or null when valid) for the current <see cref="Time"/>.</summary>
    public Func<TimeSpan?, string?>? Validation
    {
        get => GetValue(ValidationProperty);
        set => SetValue(ValidationProperty, value);
    }

    /// <summary>The field label. Mirrors the reference API's <c>Label</c>.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Text shown when no time is set. Mirrors the reference API's <c>Placeholder</c>.</summary>
    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>A .NET time format string for the display. Mirrors the reference API's <c>TimeFormat</c>.</summary>
    public string TimeFormat
    {
        get => GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }

    /// <summary>Visual field style: outlined, filled, or text/underline.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Granularity of the minute column. Mirrors the reference API's <c>MinuteSelectionStep</c>.</summary>
    public int MinuteStep
    {
        get => GetValue(MinuteStepProperty);
        set => SetValue(MinuteStepProperty, value);
    }

    /// <summary>Focus accent color.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Whether the field is in an error state.</summary>
    public bool Error
    {
        get => GetValue(ErrorProperty);
        set => SetValue(ErrorProperty, value);
    }

    /// <summary>Helper text shown below the field.</summary>
    public string? HelperText
    {
        get => GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Error message shown instead of helper text when <see cref="Error"/>.</summary>
    public string? ErrorText
    {
        get => GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>When true, the label stays floated above the field even when empty and unfocused.</summary>
    public bool ShrinkLabel
    {
        get => GetValue(ShrinkLabelProperty);
        set => SetValue(ShrinkLabelProperty, value);
    }

    /// <summary>Title shown at the top of the time picker flyout.</summary>
    public string PickerTitle
    {
        get => GetValue(PickerTitleProperty);
        set => SetValue(PickerTitleProperty, value);
    }

    /// <summary>Text for the generated cancel action.</summary>
    public string CancelText
    {
        get => GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    /// <summary>Text for the generated confirmation action.</summary>
    public string OkText
    {
        get => GetValue(OkTextProperty);
        set => SetValue(OkTextProperty, value);
    }

    /// <summary>Opens the time picker flyout when enabled.</summary>
    public void OpenPicker() => Open();

    /// <summary>Closes the time picker flyout without committing pending changes.</summary>
    public void ClosePicker()
    {
        _flyout?.Hide();
        ApplyBoxChrome();
    }

    /// <summary>Clears the selected time.</summary>
    public void Clear() => Time = null;

    /// <summary>
    /// Runs <see cref="Required"/>/<see cref="Validation"/>, updates <see cref="Error"/>/<see cref="ErrorText"/>,
    /// and returns the error (or null). A no-op that preserves any manually-set error when neither is configured.
    /// </summary>
    public string? Validate()
    {
        if (!Required && Validation is null)
        {
            return ErrorText;
        }

        string? error = null;
        if (Required && Time is null)
        {
            error = RequiredText;
        }
        else if (Validation is { } validate)
        {
            error = validate(Time);
        }

        Error = error is not null;
        ErrorText = error;
        return error;
    }

    /// <summary>
    /// Parses typed time text. Returns <c>true</c> when the text is empty (yielding <paramref name="value"/> =
    /// <c>null</c>) or parses via <paramref name="format"/> (exact), the current culture, or <see cref="TimeSpan"/>;
    /// returns <c>false</c> for non-empty unparseable text.
    /// </summary>
    public static bool TryParseTime(string? text, string format, out TimeSpan? value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = null;
            return true;
        }

        if (DateTime.TryParseExact(text, format, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var exact))
        {
            value = exact.TimeOfDay;
            return true;
        }

        if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var loose))
        {
            value = loose.TimeOfDay;
            return true;
        }

        // Constrain the TimeSpan fallback to a valid time-of-day: TimeSpan.Parse reads bare numbers like
        // "5" as 5 DAYS and accepts spans >= 24h, which would silently corrupt the time-of-day value.
        if (TimeSpan.TryParse(text, CultureInfo.CurrentCulture, out var span)
            && span >= TimeSpan.Zero && span < TimeSpan.FromDays(1))
        {
            value = span;
            return true;
        }

        value = null;
        return false;
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TimePicker);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _box = e.NameScope.Find("PART_Box") as Border;
        _labelHost = e.NameScope.Find("PART_LabelHost") as Border;
        _display = e.NameScope.Find("PART_Display") as Text;
        _label = e.NameScope.Find("PART_Label") as Text;
        _restingLabel = e.NameScope.Find("PART_RestingLabel") as Text;
        _helper = e.NameScope.Find("PART_HelperText") as Text;
        _clear = e.NameScope.Find("PART_Clear") as IconButton;
        _clockButton = e.NameScope.Find("PART_ClockButton") as IconButton;
        _input = e.NameScope.Find("PART_Input") as TextBox;
        _adornment = e.NameScope.Find("PART_Adornment") as Icon;
        if (_clear is not null)
        {
            Avalonia.Automation.AutomationProperties.SetName(_clear, "Clear time");
            _clear.Click += (_, _) =>
            {
                Clear();
                TimeSelected?.Invoke(null);
            };
        }

        if (_clockButton is not null)
        {
            Avalonia.Automation.AutomationProperties.SetName(_clockButton, "Open clock");
            // Pressing the button blurs the input before Click fires; flag it so the resulting commit is
            // suppressed (the flyout selection sets the value instead of in-progress typed text).
            _clockButton.AddHandler(
                InputElement.PointerPressedEvent,
                (_, _) => _flyoutOpening = true,
                Avalonia.Interactivity.RoutingStrategies.Tunnel,
                handledEventsToo: true);
            _clockButton.Click += (_, _) =>
            {
                Open();
                _flyoutOpening = false;
            };
        }

        if (_input is not null)
        {
            FieldChrome.ResetInnerTextBox(_input);
            _input.GotFocus += (_, _) =>
            {
                FieldChrome.ResetInnerTextBox(_input);
                ApplyBoxChrome();
            };
            _input.LostFocus += (_, _) =>
            {
                CommitText();
                ApplyBoxChrome();
            };
            _input.KeyDown += (_, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    CommitText();
                    args.Handled = true;
                }
                else if (args.Key is Key.Down && args.KeyModifiers.HasFlag(KeyModifiers.Alt))
                {
                    Open();
                    args.Handled = true;
                }
            };
        }

        if (_box is not null)
        {
            _box.GotFocus += (_, _) => ApplyBoxChrome();
            _box.LostFocus += (_, _) => ApplyBoxChrome();
            _box.PointerPressed += (_, args) =>
            {
                if (!IsEnabled)
                {
                    return;
                }

                if (Editable)
                {
                    _input?.Focus();
                }
                else
                {
                    Focus();
                    Open();
                }

                args.Handled = true;
            };
        }

        UpdateAdornment();
        UpdateEditMode();
        UpdateLabel();
        UpdateDisplay();
        ApplyBoxChrome();
        UpdateClearButton();
    }

    private void UpdateClearButton()
    {
        if (_clear is not null)
        {
            _clear.IsVisible = Clearable && Time is not null;
        }
    }

    private void UpdateEditMode()
    {
        if (_input is not null)
        {
            _input.IsVisible = Editable;
            _input.IsReadOnly = !Editable;
        }
    }

    private void CommitText()
    {
        // Skip while the flyout is open/opening: the popup steals focus and the flyout selection,
        // not the in-progress typed text, is what should set the value.
        if (_input is null || !Editable || _flyoutOpen || _flyoutOpening)
        {
            return;
        }

        if (!TryParseTime(_input.Text, TimeFormat, out var parsed))
        {
            Error = true;
            ErrorText = InvalidTimeText;
            return; // keep the user's text so they can correct it
        }

        Error = false;
        Time = parsed;
        UpdateDisplay();        // reformat the text box even when the parsed value is unchanged
        Validate();             // business validation runs even on a same-value commit
        TimeSelected?.Invoke(parsed);
    }

    private void UpdateAdornment()
    {
        if (_adornment is not null)
        {
            _adornment.Data = AdornmentIcon;
            _adornment.IsVisible = !string.IsNullOrEmpty(AdornmentIcon);
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TimeProperty || change.Property == TimeFormatProperty ||
            change.Property == PlaceholderProperty)
        {
            UpdateDisplay();
        }

        if (change.Property == TimeProperty || change.Property == ClearableProperty)
        {
            UpdateClearButton();
        }

        if (change.Property == TimeProperty || change.Property == RequiredProperty ||
            change.Property == ValidationProperty || change.Property == RequiredTextProperty)
        {
            Validate();
        }
        else if (change.Property == LabelProperty || change.Property == ShrinkLabelProperty ||
                 change.Property == HelperTextProperty || change.Property == ErrorTextProperty)
        {
            UpdateLabel();
        }

        if (change.Property == AdornmentIconProperty)
        {
            UpdateAdornment();
            UpdateLabel();
        }

        if (change.Property == EditableProperty)
        {
            UpdateEditMode();
            UpdateDisplay();
            ApplyBoxChrome();
        }

        if (change.Property == VariantProperty || change.Property == ColorProperty || change.Property == ErrorProperty ||
            change.Property == IsEnabledProperty)
        {
            ApplyBoxChrome();
            UpdateLabel();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled)
        {
            return;
        }

        // In editable mode, Space/Enter belong to the text box; the flyout opens via the icon or Alt+Down.
        if (!Editable && InteractionAssist.IsActivationKey(e.Key))
        {
            Open();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            _flyout?.Hide();
            ApplyBoxChrome();
            e.Handled = true;
        }
    }

    private void Open()
    {
        if (!IsEnabled)
        {
            return;
        }

        _flyout?.Hide();
        _popupHourDisplay = null;
        _popupMinuteDisplay = null;
        _hourScroll = null;
        _minuteScroll = null;

        var current = Time ?? TimeSpan.Zero;
        var step = Math.Clamp(MinuteStep, 1, 30);
        var pendingHour = current.Hours;
        var pendingMinute = current.Minutes - current.Minutes % step;

        void SetPending(int? hour, int? minute)
        {
            pendingHour = hour ?? pendingHour;
            pendingMinute = minute ?? pendingMinute;
            if (_popupHourDisplay is not null)
            {
                _popupHourDisplay.Text = pendingHour.ToString("00", CultureInfo.CurrentCulture);
            }

            if (_popupMinuteDisplay is not null)
            {
                _popupMinuteDisplay.Text = pendingMinute.ToString("00", CultureInfo.CurrentCulture);
            }

            if (hour is not null)
            {
                Highlight(_hourRows, pendingHour);
            }

            if (minute is not null)
            {
                Highlight(_minuteRows, pendingMinute);
            }
        }

        var hours = BuildColumn("Hour", _hourRows, Enumerable.Range(0, 24), pendingHour, h => SetPending(h, null), s => _hourScroll = s);

        var minuteValues = new List<int>();
        for (var m = 0; m < 60; m += step)
        {
            minuteValues.Add(m);
        }

        var minutes = BuildColumn("Minute", _minuteRows, minuteValues, pendingMinute, m => SetPending(null, m), s => _minuteScroll = s);

        var cancel = new Button
        {
            Content = CancelText,
            Variant = Variant.Text,
            Color = LoamColor.Primary,
        };
        cancel.Click += (_, _) =>
        {
            _flyout?.Hide();
            ApplyBoxChrome();
        };

        var ok = new Button
        {
            Content = OkText,
            Variant = Variant.Text,
            Color = LoamColor.Primary,
        };
        ok.Click += (_, _) =>
        {
            Error = false; // picking a valid time clears any prior typed-input error
            Time = new TimeSpan(pendingHour, pendingMinute, 0);
            UpdateDisplay();
            Validate();    // re-run business validation on the committed value (also covers same-value picks)
            TimeSelected?.Invoke(Time);
            _flyout?.Hide();
            ApplyBoxChrome();
        };

        var body = new StackPanel
        {
            Spacing = 18,
            Children =
            {
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 8,
                    Children =
                    {
                        BuildTimePart("Hour", pendingHour, text => _popupHourDisplay = text),
                        new Text
                        {
                            Text = ":",
                            Typo = Typo.DisplaySmall,
                            Color = LoamColor.Default,
                            Margin = new Thickness(0, 24, 0, 0),
                        },
                        BuildTimePart("Minute", pendingMinute, text => _popupMinuteDisplay = text),
                    },
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 12,
                    Children = { hours, minutes },
                },
            },
        };

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = { cancel, ok },
        };

        _flyout = new Flyout
        {
            Content = PopupSurface.PickerPaper(PopupSurface.PickerContent(PickerTitle, body, actions)),
            Placement = PlacementMode.BottomEdgeAlignedLeft,
            FlyoutPresenterTheme = PopupSurface.FlyoutPresenterTheme,
        };
        _flyout.Closed += (_, _) =>
        {
            _flyoutOpen = false;
            _popupHourDisplay = null;
            _popupMinuteDisplay = null;
            _hourScroll = null;
            _minuteScroll = null;
            ApplyBoxChrome();
        };
        _flyoutOpen = true;
        _flyout.ShowAt(_box ?? (Control)this);
        ApplyBoxChrome();

        // Bring the selected hour/minute into view once the flyout columns have laid out.
        ScheduleScrollToSelection(_hourScroll, _hourRows, pendingHour);
        ScheduleScrollToSelection(_minuteScroll, _minuteRows, pendingMinute);
    }

    private static void ScheduleScrollToSelection(ScrollViewer? scroll, List<TimePickerRow> rows, int value)
    {
        if (scroll is null)
        {
            return;
        }

        var target = rows.Find(r => r.Value == value)?.Row;
        if (target is null)
        {
            return;
        }

        void Center()
        {
            var viewport = scroll.Viewport.Height;
            var extent = scroll.Extent.Height;
            if (viewport <= 0 || extent <= 0 || target.Bounds.Height <= 0)
            {
                return; // not laid out yet
            }

            var rowCenter = target.Bounds.Y + (target.Bounds.Height / 2);
            var max = Math.Max(0, extent - viewport);
            var offsetY = Math.Clamp(rowCenter - (viewport / 2), 0, max);
            scroll.Offset = new Vector(scroll.Offset.X, offsetY);
        }

        if (scroll.Viewport.Height > 0 && target.Bounds.Height > 0)
        {
            Center(); // already laid out (e.g., reopen) — center immediately
            return;
        }

        void OnViewport(object? sender, EffectiveViewportChangedEventArgs e)
        {
            if (scroll.Viewport.Height <= 0 || target.Bounds.Height <= 0)
            {
                return; // wait for a valid layout pass
            }

            scroll.EffectiveViewportChanged -= OnViewport;
            Center();
        }

        scroll.EffectiveViewportChanged += OnViewport;
    }

    private StackPanel BuildTimePart(string labelText, int value, Action<Text> capture)
    {
        var valueText = new Text
        {
            Text = value.ToString("00", CultureInfo.CurrentCulture),
            Typo = Typo.DisplaySmall,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        valueText.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.ColorOnPrimaryContainer));
        capture(valueText);

        var field = new Border
        {
            Width = 104,
            Height = 72,
            CornerRadius = new CornerRadius(16),
            Child = valueText,
        };
        field.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.ColorPrimaryContainer));

        return new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new Text
                {
                    Text = labelText,
                    Typo = Typo.LabelMedium,
                    Color = LoamColor.Default,
                    Opacity = 0.72,
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                field,
            },
        };
    }

    private StackPanel BuildColumn(string heading, List<TimePickerRow> rows, IEnumerable<int> values, int selected, Action<int> onPick, Action<ScrollViewer> captureScroll)
    {
        rows.Clear();
        var list = new StackPanel { Spacing = 4 };
        foreach (var value in values)
        {
            var label = new Text
            {
                Text = value.ToString("00", CultureInfo.CurrentCulture),
                Typo = Typo.Body1,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var stateLayer = new Border
            {
                Background = Brushes.Transparent,
                CornerRadius = new CornerRadius(24),
                IsHitTestVisible = false,
            };
            var row = new Border
            {
                Child = new Avalonia.Controls.Grid { Children = { stateLayer, label } },
                Width = 96,
                MinHeight = 48,
                Padding = new Thickness(12, 8),
                CornerRadius = new CornerRadius(24),
                Cursor = HandCursor,
                Focusable = true,
                Background = Brushes.Transparent,
                ClipToBounds = true,
            };
            AutomationProperties.SetName(row, $"{heading} {value:00}");
            var item = new TimePickerRow(row, label, stateLayer, value);
            var captured = value;
            row.PointerPressed += (_, _) => onPick(captured);
            row.KeyDown += (_, e) =>
            {
                if (InteractionAssist.IsActivationKey(e.Key))
                {
                    onPick(captured);
                    e.Handled = true;
                }
            };
            row.PointerEntered += (_, _) => ApplyTimeRowState(item, "Hover");
            row.PointerExited += (_, _) => ApplyTimeRowState(item, row.IsFocused ? "Focus" : null);
            row.GotFocus += (_, _) =>
            {
                ApplyTimeRowState(item, "Focus");
                row.BringIntoView(); // keep the active row visible during keyboard navigation
            };
            row.LostFocus += (_, _) => ApplyTimeRowState(item, null);
            rows.Add(item);
            list.Children.Add(row);
        }

        Highlight(rows, selected);

        var scroll = new ScrollViewer
        {
            Height = 176,
            Content = list,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            // Hidden (not Disabled) so the column is scrollable — Disabled pins content to the viewport.
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
        };
        captureScroll(scroll);

        return new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new Text { Text = heading, Typo = Typo.LabelMedium, Color = LoamColor.Default, Opacity = 0.72, HorizontalAlignment = HorizontalAlignment.Center },
                scroll,
            },
        };
    }

    private void Highlight(List<TimePickerRow> rows, int selected)
    {
        foreach (var item in rows)
        {
            ApplyTimeRowSelection(item, item.Value == selected);
        }
    }

    private void ApplyTimeRowSelection(TimePickerRow item, bool selected)
    {
        item.Selected = selected;
        item.BackgroundBinding?.Dispose();
        item.ForegroundBinding?.Dispose();
        item.BackgroundBinding = null;
        item.ForegroundBinding = null;

        if (selected)
        {
            item.BackgroundBinding = item.Row.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.ColorPrimaryContainer));
            item.ForegroundBinding = item.Label.Bind(TextBlock.ForegroundProperty,
                this.GetResourceObservable(LoamTokens.ColorOnPrimaryContainer));
        }
        else
        {
            item.Row.Background = Brushes.Transparent;
            item.ForegroundBinding = item.Label.Bind(TextBlock.ForegroundProperty,
                this.GetResourceObservable(LoamTokens.TextPrimary));
        }

        ApplyTimeRowState(item, item.Row.IsFocused ? "Focus" : null);
    }

    private void ApplyTimeRowState(TimePickerRow item, string? state)
    {
        item.StateBinding?.Dispose();
        item.StateBinding = null;
        if (state is null)
        {
            item.StateLayer.Background = Brushes.Transparent;
            return;
        }

        var role = item.Selected ? nameof(LoamColorScheme.OnPrimaryContainer) : nameof(LoamColorScheme.OnSurface);
        item.StateBinding = item.StateLayer.Bind(Border.BackgroundProperty,
            this.GetResourceObservable(LoamTokens.ColorSchemeStateLayer(role, state)));
    }

    private void UpdateLabel()
    {
        var labelForeground = LabelForegroundKey();
        var helperForeground = Error ? LoamTokens.Error : LoamTokens.TextSecondary;
        var hasLabel = !string.IsNullOrEmpty(Label);
        var hasTypedText = Editable && _input is not null && !string.IsNullOrEmpty(_input.Text);
        var floating = hasLabel && (ShrinkLabel || IsActive() || Time is not null || hasTypedText);
        var resting = hasLabel && !floating;

        if (_label is not null)
        {
            _label.Text = Label;
            _label.IsVisible = floating;
            _labelForeground?.Dispose();
            _labelForeground = _label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(labelForeground));
        }

        if (_restingLabel is not null)
        {
            _restingLabel.Text = Label;
            _restingLabel.IsVisible = resting;
            _restingLabelForeground?.Dispose();
            _restingLabelForeground = _restingLabel.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(labelForeground));
        }

        if (_display is not null)
        {
            _display.IsVisible = !resting && !Editable;
        }

        if (_input is not null)
        {
            _input.PlaceholderText = resting ? null : Placeholder;
            Avalonia.Automation.AutomationProperties.SetName(_input, Label ?? Placeholder ?? "Time");
        }

        var leadingInset = string.IsNullOrEmpty(AdornmentIcon) ? 0 : FieldChrome.LeadingAdornmentInset(this);
        FieldChrome.ApplyLabelLayout(this, _box, _labelHost, floating, Variant, leadingInset);

        if (_helper is not null)
        {
            var text = Error && !string.IsNullOrEmpty(ErrorText) ? ErrorText : HelperText;
            _helper.Text = text;
            _helper.IsVisible = !string.IsNullOrEmpty(text);
            _helperForeground?.Dispose();
            _helperForeground = _helper.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(helperForeground));
        }

        InteractionAssist.SetAutomationName(this, Label, _display?.Text, Placeholder);
    }

    private void UpdateDisplay()
    {
        if (_display is null)
        {
            return;
        }

        var hasTime = Time is not null;
        var formatted = hasTime ? DateTime.Today.Add(Time!.Value).ToString(TimeFormat, CultureInfo.CurrentCulture) : null;
        _display.Text = formatted ?? Placeholder;
        _displayForeground?.Dispose();
        _displayForeground = _display.Bind(TextBlock.ForegroundProperty,
            this.GetResourceObservable(hasTime ? LoamTokens.TextPrimary : LoamTokens.TextSecondary));

        // Keep the editable text box in sync with the committed value (empty when cleared).
        if (_input is not null && Editable)
        {
            _input.Text = formatted ?? string.Empty;
        }

        InteractionAssist.SetAutomationName(this, Label, _display.Text, Placeholder);
        UpdateLabel();
    }

    private void ApplyBoxChrome()
    {
        if (_box is null)
        {
            return;
        }

        FieldChrome.Apply(this, _box, Variant, Color, Error, IsActive(), IsEnabled,
            ref _boxBorderBrush, ref _boxBackground);
        _box.IsEnabled = IsEnabled;
        _box.Cursor = !IsEnabled ? Cursor.Default
            : Editable ? new Cursor(StandardCursorType.Ibeam)
            : HandCursor;
        UpdateLabel();
    }

    private bool IsActive() => _flyoutOpen || IsFocused || _box?.IsFocused == true || _input?.IsFocused == true;

    private string LabelForegroundKey()
    {
        if (Error)
        {
            return LoamTokens.Error;
        }

        if (IsActive())
        {
            var paletteName = Color.ToPaletteName();
            return paletteName is null ? LoamTokens.Primary : LoamTokens.Palette(paletteName);
        }

        return LoamTokens.TextSecondary;
    }

    private sealed class TimePickerRow(Border row, Text label, Border stateLayer, int value)
    {
        public Border Row { get; } = row;
        public Text Label { get; } = label;
        public Border StateLayer { get; } = stateLayer;
        public int Value { get; } = value;
        public bool Selected { get; set; }
        public IDisposable? BackgroundBinding { get; set; }
        public IDisposable? ForegroundBinding { get; set; }
        public IDisposable? StateBinding { get; set; }
    }
}
