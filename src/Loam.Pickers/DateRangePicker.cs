using System.Globalization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A date-range input, mirroring the reference API's <c>DateRangePicker</c>. A variant field shows the
/// two-way <see cref="Start"/>/<see cref="End"/> dates; clicking opens a <see cref="MonthCalendar"/>
/// flyout where the first click sets the start and the second the end (auto-ordered).
/// </summary>
public class DateRangePicker : TemplatedControl
{
    /// <summary>Identifies the <see cref="Start"/> property.</summary>
    public static readonly StyledProperty<DateTime?> StartProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(Start), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="End"/> property.</summary>
    public static readonly StyledProperty<DateTime?> EndProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(End), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<DateRangePicker, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="Placeholder"/> property.</summary>
    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<DateRangePicker, string?>(nameof(Placeholder), "Select a range");

    /// <summary>Identifies the <see cref="DateFormat"/> property.</summary>
    public static readonly StyledProperty<string> DateFormatProperty =
        AvaloniaProperty.Register<DateRangePicker, string>(nameof(DateFormat), "d");

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<DateRangePicker, Variant>(nameof(Variant), Loam.Variant.Outlined);

    /// <summary>Identifies the <see cref="MinDate"/> property.</summary>
    public static readonly StyledProperty<DateTime?> MinDateProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(MinDate));

    /// <summary>Identifies the <see cref="MaxDate"/> property.</summary>
    public static readonly StyledProperty<DateTime?> MaxDateProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(MaxDate));

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<DateRangePicker, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Error"/> property.</summary>
    public static readonly StyledProperty<bool> ErrorProperty =
        AvaloniaProperty.Register<DateRangePicker, bool>(nameof(Error));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<DateRangePicker, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<DateRangePicker, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="ShrinkLabel"/> property.</summary>
    public static readonly StyledProperty<bool> ShrinkLabelProperty =
        AvaloniaProperty.Register<DateRangePicker, bool>(nameof(ShrinkLabel));

    /// <summary>Identifies the <see cref="PickerTitle"/> property.</summary>
    public static readonly StyledProperty<string> PickerTitleProperty =
        AvaloniaProperty.Register<DateRangePicker, string>(nameof(PickerTitle), "Select range");

    /// <summary>Identifies the <see cref="CancelText"/> property.</summary>
    public static readonly StyledProperty<string> CancelTextProperty =
        AvaloniaProperty.Register<DateRangePicker, string>(nameof(CancelText), "Cancel");

    /// <summary>Identifies the <see cref="OkText"/> property.</summary>
    public static readonly StyledProperty<string> OkTextProperty =
        AvaloniaProperty.Register<DateRangePicker, string>(nameof(OkText), "OK");

    /// <summary>Identifies the <see cref="Clearable"/> property.</summary>
    public static readonly StyledProperty<bool> ClearableProperty =
        AvaloniaProperty.Register<DateRangePicker, bool>(nameof(Clearable));

    /// <summary>Identifies the <see cref="AdornmentIcon"/> property.</summary>
    public static readonly StyledProperty<string?> AdornmentIconProperty =
        AvaloniaProperty.Register<DateRangePicker, string?>(nameof(AdornmentIcon));

    /// <summary>Identifies the <see cref="ShowPresets"/> property.</summary>
    public static readonly StyledProperty<bool> ShowPresetsProperty =
        AvaloniaProperty.Register<DateRangePicker, bool>(nameof(ShowPresets));

    /// <summary>The built-in quick-select presets used when <see cref="ShowPresets"/> is true and <see cref="Presets"/> is empty.</summary>
    public static readonly IReadOnlyList<DateRangePreset> DefaultPresets = new[]
    {
        new DateRangePreset("Today", static a => (a, a)),
        new DateRangePreset("Yesterday", static a => (a.AddDays(-1), a.AddDays(-1))),
        new DateRangePreset("Last 7 days", static a => (a.AddDays(-6), a)),
        new DateRangePreset("Last 30 days", static a => (a.AddDays(-29), a)),
        new DateRangePreset("This month", static a => (
            new DateTime(a.Year, a.Month, 1),
            new DateTime(a.Year, a.Month, DateTime.DaysInMonth(a.Year, a.Month)))),
        new DateRangePreset("Last month", static a => PreviousMonth(a)),
        new DateRangePreset("This year", static a => (new DateTime(a.Year, 1, 1), new DateTime(a.Year, 12, 31))),
    };

    /// <summary>Identifies the <see cref="Editable"/> property.</summary>
    public static readonly StyledProperty<bool> EditableProperty =
        AvaloniaProperty.Register<DateRangePicker, bool>(nameof(Editable));

    /// <summary>Identifies the <see cref="InvalidRangeText"/> property.</summary>
    public static readonly StyledProperty<string> InvalidRangeTextProperty =
        AvaloniaProperty.Register<DateRangePicker, string>(nameof(InvalidRangeText), "Invalid range");

    /// <summary>Separators accepted between the start and end of a typed range (the first matches the displayed en-dash).</summary>
    private static readonly string[] RangeSeparators = { "–", " to ", " - " };

    /// <summary>Width (excluding margins) of the preset rail shown beside the calendar.</summary>
    private const double PresetRailWidth = 168;

    /// <summary>Maximum rail height; longer custom preset lists scroll rather than stretching the flyout.</summary>
    private const double PresetRailMaxHeight = 360;

    private Border? _box;
    private IconButton? _clear;
    private IconButton? _calendarButton;
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

    /// <summary>Raised when the picker commits a range through the generated OK action.</summary>
    public event Action<DateTime?, DateTime?>? RangeSelected;

    /// <summary>Creates the picker.</summary>
    public DateRangePicker()
    {
        Focusable = true;
        GotFocus += (_, _) => ApplyBoxChrome();
        LostFocus += (_, _) => ApplyBoxChrome();
    }

    /// <summary>The range start (two-way). Mirrors the reference API's <c>DateRange.Start</c>.</summary>
    public DateTime? Start
    {
        get => GetValue(StartProperty);
        set => SetValue(StartProperty, value);
    }

    /// <summary>The range end (two-way). Mirrors the reference API's <c>DateRange.End</c>.</summary>
    public DateTime? End
    {
        get => GetValue(EndProperty);
        set => SetValue(EndProperty, value);
    }

    /// <summary>When true and a range is set, shows an inline clear (x) button that resets the range.</summary>
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

    /// <summary>When true, the user can type a range into the field; the trailing calendar icon opens the flyout.</summary>
    public bool Editable
    {
        get => GetValue(EditableProperty);
        set => SetValue(EditableProperty, value);
    }

    /// <summary>Error message shown when typed text cannot be parsed as a range or is out of range (<see cref="Editable"/> mode).</summary>
    public string InvalidRangeText
    {
        get => GetValue(InvalidRangeTextProperty);
        set => SetValue(InvalidRangeTextProperty, value);
    }

    /// <summary>When true, the flyout shows a quick-select rail of <see cref="Presets"/> (or <see cref="DefaultPresets"/> when none are set).</summary>
    public bool ShowPresets
    {
        get => GetValue(ShowPresetsProperty);
        set => SetValue(ShowPresetsProperty, value);
    }

    /// <summary>Custom quick-select presets shown when <see cref="ShowPresets"/> is true. When empty, <see cref="DefaultPresets"/> is used.</summary>
    public AvaloniaList<DateRangePreset> Presets { get; } = [];

    /// <summary>The field label.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Text shown when no range is set.</summary>
    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>A .NET date format string for the display.</summary>
    public string DateFormat
    {
        get => GetValue(DateFormatProperty);
        set => SetValue(DateFormatProperty, value);
    }

    /// <summary>Visual field style: outlined, filled, or text/underline.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>First selectable date.</summary>
    public DateTime? MinDate
    {
        get => GetValue(MinDateProperty);
        set => SetValue(MinDateProperty, value);
    }

    /// <summary>Last selectable date.</summary>
    public DateTime? MaxDate
    {
        get => GetValue(MaxDateProperty);
        set => SetValue(MaxDateProperty, value);
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

    /// <summary>Title shown at the top of the range picker flyout.</summary>
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

    /// <summary>Opens the range picker flyout.</summary>
    public void OpenPicker() => Open();

    /// <summary>Closes the range picker flyout without changing the selected range.</summary>
    public void ClosePicker()
    {
        _flyout?.Hide();
        ApplyBoxChrome();
    }

    /// <summary>Clears the selected start and end dates.</summary>
    public void Clear()
    {
        Start = null;
        End = null;
    }

    /// <summary>Returns the first and last day of the calendar month before <paramref name="anchor"/>.</summary>
    private static (DateTime Start, DateTime End) PreviousMonth(DateTime anchor)
    {
        var lastDayOfPreviousMonth = new DateTime(anchor.Year, anchor.Month, 1).AddDays(-1);
        var firstDayOfPreviousMonth = new DateTime(lastDayOfPreviousMonth.Year, lastDayOfPreviousMonth.Month, 1);
        return (firstDayOfPreviousMonth, lastDayOfPreviousMonth);
    }

    /// <summary>Formats a range for display (null when empty).</summary>
    public static string? Format(DateTime? start, DateTime? end, string format)
    {
        if (start is null)
        {
            return null;
        }

        var startText = start.Value.ToString(format, CultureInfo.CurrentCulture);
        return end is null ? startText : $"{startText} – {end.Value.ToString(format, CultureInfo.CurrentCulture)}";
    }

    /// <summary>
    /// Parses typed range text. Returns <c>true</c> when the text is empty (yielding both <c>null</c>), is a
    /// single date parseable via <paramref name="format"/> (start only), or is two dates separated by an
    /// en-dash, " to ", or " - " (auto-ordered). Each half is parsed by <see cref="DatePicker.TryParseDate"/>;
    /// returns <c>false</c> for non-empty unparseable text.
    /// </summary>
    public static bool TryParseRange(string? text, string format, out DateTime? start, out DateTime? end)
    {
        start = null;
        end = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        var (left, right, hasSeparator) = SplitRange(text);
        if (!DatePicker.TryParseDate(left, format, out var parsedStart) || parsedStart is null)
        {
            return false;
        }

        if (!hasSeparator)
        {
            start = parsedStart; // a single date is treated as the start with no end
            return true;
        }

        if (!DatePicker.TryParseDate(right, format, out var parsedEnd) || parsedEnd is null)
        {
            return false;
        }

        if (parsedEnd < parsedStart)
        {
            (parsedStart, parsedEnd) = (parsedEnd, parsedStart); // auto-order
        }

        start = parsedStart;
        end = parsedEnd;
        return true;
    }

    private static (string Left, string Right, bool HasSeparator) SplitRange(string text)
    {
        foreach (var separator in RangeSeparators)
        {
            var index = text.IndexOf(separator, StringComparison.Ordinal);
            if (index >= 0)
            {
                return (text[..index].Trim(), text[(index + separator.Length)..].Trim(), true);
            }
        }

        return (text.Trim(), string.Empty, false);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(DateRangePicker);

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
        _calendarButton = e.NameScope.Find("PART_CalendarButton") as IconButton;
        _input = e.NameScope.Find("PART_Input") as TextBox;
        _adornment = e.NameScope.Find("PART_Adornment") as Icon;
        if (_clear is not null)
        {
            Avalonia.Automation.AutomationProperties.SetName(_clear, "Clear dates");
            _clear.Click += (_, _) =>
            {
                Clear();
                RangeSelected?.Invoke(null, null);
            };
        }

        if (_calendarButton is not null)
        {
            Avalonia.Automation.AutomationProperties.SetName(_calendarButton, "Open calendar");
            // Pressing the button blurs the input before Click fires; flag it so the resulting commit is
            // suppressed (the flyout selection sets the value instead of in-progress typed text).
            _calendarButton.AddHandler(
                InputElement.PointerPressedEvent,
                (_, _) => _flyoutOpening = true,
                Avalonia.Interactivity.RoutingStrategies.Tunnel,
                handledEventsToo: true);
            _calendarButton.Click += (_, _) =>
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
            _clear.IsVisible = Clearable && (Start is not null || End is not null);
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

        if (!TryParseRange(_input.Text, DateFormat, out var start, out var end))
        {
            Error = true;
            ErrorText = InvalidRangeText;
            return; // keep the user's text so they can correct it
        }

        if ((start is { } s && IsOutOfRange(s)) || (end is { } en && IsOutOfRange(en)))
        {
            Error = true;
            ErrorText = InvalidRangeText;
            return;
        }

        Error = false;
        Start = start;
        End = end;
        UpdateDisplay();        // reformat the text box even when the parsed value is unchanged
        RangeSelected?.Invoke(Start, End);
    }

    private bool IsOutOfRange(DateTime value) =>
        (MinDate is { } min && value.Date < min.Date) || (MaxDate is { } max && value.Date > max.Date);

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
        if (change.Property == StartProperty || change.Property == EndProperty ||
            change.Property == DateFormatProperty || change.Property == PlaceholderProperty)
        {
            UpdateDisplay();
        }

        if (change.Property == StartProperty || change.Property == EndProperty ||
            change.Property == ClearableProperty)
        {
            UpdateClearButton();
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

        var pendingStart = Start;
        var pendingEnd = End;
        var preview = new Text
        {
            Text = Format(pendingStart, pendingEnd, DateFormat) ?? Placeholder ?? PickerTitle,
            Typo = Typo.TitleSmall,
            Color = LoamColor.Default,
            Margin = new Thickness(24, 0, 24, 8),
        };
        var calendar = new MonthCalendar
        {
            SelectedDate = pendingEnd ?? pendingStart,
            RangeStart = pendingStart,
            RangeEnd = pendingEnd,
            MinDate = MinDate,
            MaxDate = MaxDate,
        };
        if ((pendingStart ?? pendingEnd) is { } anchor)
        {
            calendar.DisplayMonth = new DateTime(anchor.Year, anchor.Month, 1);
        }

        void SyncPendingDisplay()
        {
            preview.Text = Format(pendingStart, pendingEnd, DateFormat) ?? Placeholder ?? PickerTitle;
            calendar.SelectedDate = pendingEnd ?? pendingStart;
            calendar.RangeStart = pendingStart;
            calendar.RangeEnd = pendingEnd;
        }

        calendar.DateSelected += picked =>
        {
            if (MonthCalendar.IsDisabled(picked, MinDate, MaxDate))
            {
                return;
            }

            if (pendingStart is null || pendingEnd is not null)
            {
                pendingStart = picked;
                pendingEnd = null;
            }
            else if (picked < pendingStart)
            {
                pendingEnd = pendingStart;
                pendingStart = picked;
            }
            else
            {
                pendingEnd = picked;
            }

            SyncPendingDisplay();
        };

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
            Error = false; // picking a valid range clears any prior typed-input error
            Start = pendingStart;
            End = pendingEnd;
            UpdateDisplay();
            RangeSelected?.Invoke(Start, End);
            _flyout?.Hide();
            ApplyBoxChrome();
        };

        var calendarColumn = new StackPanel
        {
            Spacing = 0,
            Children = { preview, calendar },
        };

        Control body = calendarColumn;
        var paperWidth = PopupSurface.PickerWidth;
        if (ShowPresets)
        {
            var presets = Presets.Count > 0 ? (IReadOnlyList<DateRangePreset>)Presets : DefaultPresets;
            var rail = BuildPresetRail(presets, preset =>
            {
                var (start, end) = preset.Resolve(DateTime.Today);
                if (end < start)
                {
                    (start, end) = (end, start);
                }

                if (MinDate is { } min && start < min)
                {
                    start = min;
                }

                if (MaxDate is { } max && end > max)
                {
                    end = max;
                }

                if (start > end)
                {
                    return; // preset falls entirely outside the allowed bounds
                }

                pendingStart = start;
                pendingEnd = end;
                calendar.DisplayMonth = new DateTime(start.Year, start.Month, 1);
                SyncPendingDisplay();
            });

            DockPanel.SetDock(rail, Dock.Left);
            body = new DockPanel { LastChildFill = true, Children = { rail, calendarColumn } };
            paperWidth += PresetRailWidth + 16; // rail width plus its horizontal margins
        }

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = { cancel, ok },
        };

        _flyout = new Flyout
        {
            Content = PopupSurface.PickerPaper(
                PopupSurface.PickerContent(PickerTitle, body, actions, paperWidth), paperWidth),
            Placement = PlacementMode.BottomEdgeAlignedLeft,
            FlyoutPresenterTheme = PopupSurface.FlyoutPresenterTheme,
        };
        _flyout.Closed += (_, _) =>
        {
            _flyoutOpen = false;
            ApplyBoxChrome();
        };
        _flyoutOpen = true;
        _flyout.ShowAt(_box ?? (Control)this);
        ApplyBoxChrome();
    }

    private static ScrollViewer BuildPresetRail(IReadOnlyList<DateRangePreset> presets, Action<DateRangePreset> apply)
    {
        var rail = new StackPanel
        {
            Spacing = 2,
        };

        foreach (var preset in presets)
        {
            var captured = preset;
            var button = new Button
            {
                // Text content is the accessible name; no extra AutomationProperties.SetName needed.
                Content = captured.Label,
                Variant = Variant.Text,
                Color = LoamColor.Primary,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
            };
            button.Click += (_, _) => apply(captured);
            rail.Children.Add(button);
        }

        // Cap the rail height so a long custom preset list scrolls instead of stretching the flyout.
        return new ScrollViewer
        {
            Content = rail,
            Width = PresetRailWidth,
            MaxHeight = PresetRailMaxHeight,
            Margin = new Thickness(12, 4, 4, 8),
            VerticalAlignment = VerticalAlignment.Top,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
        };
    }

    private void UpdateLabel()
    {
        var labelForeground = LabelForegroundKey();
        var helperForeground = Error ? LoamTokens.Error : LoamTokens.TextSecondary;
        var hasLabel = !string.IsNullOrEmpty(Label);
        var hasTypedText = Editable && _input is not null && !string.IsNullOrEmpty(_input.Text);
        var floating = hasLabel && (ShrinkLabel || IsActive() || Start is not null || End is not null || hasTypedText);
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
            Avalonia.Automation.AutomationProperties.SetName(_input, Label ?? Placeholder ?? "Date range");
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

        var text = Format(Start, End, DateFormat);
        var hasValue = text is not null;
        _display.Text = hasValue ? text : Placeholder;
        _displayForeground?.Dispose();
        _displayForeground = _display.Bind(TextBlock.ForegroundProperty,
            this.GetResourceObservable(hasValue ? LoamTokens.TextPrimary : LoamTokens.TextSecondary));

        // Keep the editable text box in sync with the committed value (empty when cleared).
        if (_input is not null && Editable)
        {
            _input.Text = text ?? string.Empty;
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
            : new Cursor(StandardCursorType.Hand);
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
}
