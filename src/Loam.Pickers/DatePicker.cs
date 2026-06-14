using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A date input with a calendar popup, mirroring the reference API's <c>DatePicker</c>. A variant
/// field shows the two-way <see cref="Date"/> formatted by <see cref="DateFormat"/>; clicking it opens
/// a self-contained <see cref="MonthCalendar"/> flyout (no FluentTheme dependency).
/// </summary>
public class DatePicker : TemplatedControl
{
    /// <summary>Identifies the <see cref="Date"/> property.</summary>
    public static readonly StyledProperty<DateTime?> DateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(Date), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<DatePicker, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="Placeholder"/> property.</summary>
    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<DatePicker, string?>(nameof(Placeholder), "Select a date");

    /// <summary>Identifies the <see cref="DateFormat"/> property.</summary>
    public static readonly StyledProperty<string> DateFormatProperty =
        AvaloniaProperty.Register<DatePicker, string>(nameof(DateFormat), "d");

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<DatePicker, Variant>(nameof(Variant), Loam.Variant.Outlined);

    /// <summary>Identifies the <see cref="MinDate"/> property.</summary>
    public static readonly StyledProperty<DateTime?> MinDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MinDate));

    /// <summary>Identifies the <see cref="MaxDate"/> property.</summary>
    public static readonly StyledProperty<DateTime?> MaxDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MaxDate));

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<DatePicker, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Error"/> property.</summary>
    public static readonly StyledProperty<bool> ErrorProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(Error));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<DatePicker, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<DatePicker, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="ShrinkLabel"/> property.</summary>
    public static readonly StyledProperty<bool> ShrinkLabelProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(ShrinkLabel));

    /// <summary>Identifies the <see cref="PickerTitle"/> property.</summary>
    public static readonly StyledProperty<string> PickerTitleProperty =
        AvaloniaProperty.Register<DatePicker, string>(nameof(PickerTitle), "Select date");

    /// <summary>Identifies the <see cref="CancelText"/> property.</summary>
    public static readonly StyledProperty<string> CancelTextProperty =
        AvaloniaProperty.Register<DatePicker, string>(nameof(CancelText), "Cancel");

    /// <summary>Identifies the <see cref="OkText"/> property.</summary>
    public static readonly StyledProperty<string> OkTextProperty =
        AvaloniaProperty.Register<DatePicker, string>(nameof(OkText), "OK");

    /// <summary>Identifies the <see cref="Clearable"/> property.</summary>
    public static readonly StyledProperty<bool> ClearableProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(Clearable));

    /// <summary>Identifies the <see cref="AdornmentIcon"/> property.</summary>
    public static readonly StyledProperty<string?> AdornmentIconProperty =
        AvaloniaProperty.Register<DatePicker, string?>(nameof(AdornmentIcon));

    /// <summary>Identifies the <see cref="Editable"/> property.</summary>
    public static readonly StyledProperty<bool> EditableProperty =
        AvaloniaProperty.Register<DatePicker, bool>(nameof(Editable));

    /// <summary>Identifies the <see cref="InvalidDateText"/> property.</summary>
    public static readonly StyledProperty<string> InvalidDateTextProperty =
        AvaloniaProperty.Register<DatePicker, string>(nameof(InvalidDateText), "Invalid date");

    private Border? _box;
    private IconButton? _clear;
    private IconButton? _calendarButton;
    private TextBox? _input;
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
    private bool _flyoutOpening;

    /// <summary>Raised when the picker commits a date through the generated OK action.</summary>
    public event Action<DateTime?>? DateSelected;

    /// <summary>Creates the picker.</summary>
    public DatePicker()
    {
        Focusable = true;
        GotFocus += (_, _) => ApplyBoxChrome();
        LostFocus += (_, _) => ApplyBoxChrome();
    }

    /// <summary>When true and a date is set, shows an inline clear (x) button that resets <see cref="Date"/> to null.</summary>
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

    /// <summary>When true, the user can type a date into the field; the trailing calendar icon opens the flyout.</summary>
    public bool Editable
    {
        get => GetValue(EditableProperty);
        set => SetValue(EditableProperty, value);
    }

    /// <summary>Error message shown when typed text cannot be parsed or is out of range (<see cref="Editable"/> mode).</summary>
    public string InvalidDateText
    {
        get => GetValue(InvalidDateTextProperty);
        set => SetValue(InvalidDateTextProperty, value);
    }

    /// <summary>The selected date (two-way). Mirrors the reference API's <c>Date</c>.</summary>
    public DateTime? Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    /// <summary>The field label. Mirrors the reference API's <c>Label</c>.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Text shown when no date is set. Mirrors the reference API's <c>Placeholder</c>.</summary>
    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>A .NET date format string for the display. Mirrors the reference API's <c>DateFormat</c>.</summary>
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

    /// <summary>Title shown at the top of the date picker flyout.</summary>
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

    /// <summary>Opens the date picker flyout when enabled.</summary>
    public void OpenPicker() => Open();

    /// <summary>Closes the date picker flyout without committing pending changes.</summary>
    public void ClosePicker()
    {
        _flyout?.Hide();
        ApplyBoxChrome();
    }

    /// <summary>Clears the selected date.</summary>
    public void Clear() => Date = null;

    /// <summary>
    /// Parses typed date text. Returns <c>true</c> when the text is empty (yielding <paramref name="value"/> =
    /// <c>null</c>) or parses via <paramref name="format"/> (exact) or the current culture (loose); returns
    /// <c>false</c> for non-empty unparseable text.
    /// </summary>
    public static bool TryParseDate(string? text, string format, out DateTime? value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = null;
            return true;
        }

        if (DateTime.TryParseExact(text, format, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var exact))
        {
            value = exact;
            return true;
        }

        if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var loose))
        {
            value = loose;
            return true;
        }

        value = null;
        return false;
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(DatePicker);

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
            Avalonia.Automation.AutomationProperties.SetName(_clear, "Clear date");
            _clear.Click += (_, _) =>
            {
                Clear();
                DateSelected?.Invoke(null);
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
                    Open(); // Alt+Down opens the calendar for keyboard users
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
        // Skip while the flyout is open/opening: the popup steals focus and the calendar selection,
        // not the in-progress typed text, is what should set the value.
        if (_input is null || !Editable || _flyoutOpen || _flyoutOpening)
        {
            return;
        }

        if (!TryParseDate(_input.Text, DateFormat, out var parsed))
        {
            Error = true;
            ErrorText = InvalidDateText;
            return; // keep the user's text so they can correct it
        }

        if (parsed is { } picked && IsOutOfRange(picked))
        {
            Error = true;
            ErrorText = InvalidDateText;
            return;
        }

        Error = false;
        Date = parsed;
        UpdateDisplay();        // reformat the text box even when the parsed value is unchanged
        DateSelected?.Invoke(parsed);
    }

    private bool IsOutOfRange(DateTime value) =>
        (MinDate is { } min && value.Date < min.Date) || (MaxDate is { } max && value.Date > max.Date);

    private void UpdateClearButton()
    {
        if (_clear is not null)
        {
            _clear.IsVisible = Clearable && Date is not null;
        }
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
        if (change.Property == DateProperty || change.Property == DateFormatProperty ||
            change.Property == PlaceholderProperty)
        {
            UpdateDisplay();
        }

        if (change.Property == DateProperty || change.Property == ClearableProperty)
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

        // In editable mode, Space/Enter belong to the text box; the calendar opens via the icon or Alt+Down.
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

        var pending = Date;
        var headline = new Text
        {
            Text = FormatPickerHeadline(pending, PickerTitle),
            Typo = Typo.DisplaySmall,
            Color = LoamColor.Default,
            TextWrapping = TextWrapping.NoWrap,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        var bodyHost = new ContentControl
        {
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };

        void SyncHeadline() => headline.Text = FormatPickerHeadline(pending, PickerTitle);

        var calendar = new MonthCalendar
        {
            SelectedDate = pending,
            MinDate = MinDate,
            MaxDate = MaxDate,
        };
        if (pending is { } date)
        {
            calendar.DisplayMonth = new DateTime(date.Year, date.Month, 1);
        }

        calendar.DateSelected += picked =>
        {
            if (MonthCalendar.IsDisabled(picked, MinDate, MaxDate))
            {
                return;
            }

            pending = picked;
            calendar.SelectedDate = picked;
            SyncHeadline();
        };
        bodyHost.Content = calendar;

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
            Error = false; // picking a valid date clears any prior typed-input error
            Date = pending;
            UpdateDisplay();
            DateSelected?.Invoke(Date);
            _flyout?.Hide();
            ApplyBoxChrome();
        };

        var content = new StackPanel
        {
            Spacing = 0,
            Children =
            {
                new Text
                {
                    Text = PickerTitle,
                    Typo = Typo.TitleSmall,
                    Color = LoamColor.Default,
                    Opacity = 0.72,
                    Margin = new Thickness(24, 20, 24, 0),
                },
                BuildDatePickerHeadline(headline),
                BuildDatePickerDivider(),
                bodyHost,
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Margin = PopupSurface.PickerActionsMargin,
                    Children = { cancel, ok },
                },
            },
        };

        _flyout = new Flyout
        {
            Content = PopupSurface.PickerPaper(content),
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

    private static Border BuildDatePickerHeadline(Text headline)
    {
        return new Border
        {
            MinHeight = 64,
            Margin = new Thickness(24, 8, 12, 20),
            Child = headline,
        };
    }

    private Border BuildDatePickerDivider()
    {
        var divider = new Border { Height = 1 };
        divider.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Divider));
        return divider;
    }

    private static string FormatPickerHeadline(DateTime? date, string fallback) =>
        date is { } value
            ? value.ToString("ddd, MMM d", CultureInfo.CurrentCulture)
            : fallback;

    private void UpdateLabel()
    {
        var labelForeground = LabelForegroundKey();
        var helperForeground = Error ? LoamTokens.Error : LoamTokens.TextSecondary;
        var hasLabel = !string.IsNullOrEmpty(Label);
        var hasTypedText = Editable && _input is not null && !string.IsNullOrEmpty(_input.Text);
        var floating = hasLabel && (ShrinkLabel || IsActive() || Date is not null || hasTypedText);
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
            // Show the placeholder in the text box only when no resting label covers it (mirrors TextField).
            _input.PlaceholderText = resting ? null : Placeholder;
            Avalonia.Automation.AutomationProperties.SetName(_input, Label ?? Placeholder ?? "Date");
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

        var hasDate = Date is not null;
        var formatted = hasDate ? Date!.Value.ToString(DateFormat, CultureInfo.CurrentCulture) : null;
        _display.Text = formatted ?? Placeholder;
        _displayForeground?.Dispose();
        _displayForeground = _display.Bind(TextBlock.ForegroundProperty,
            this.GetResourceObservable(hasDate ? LoamTokens.TextPrimary : LoamTokens.TextSecondary));

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
