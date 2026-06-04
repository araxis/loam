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
/// A date input with a calendar popup, mirroring the reference API's <c>DatePicker</c>. An outlined
/// box shows the two-way <see cref="Date"/> formatted by <see cref="DateFormat"/>; clicking it opens a
/// self-contained <see cref="MonthCalendar"/> flyout (no FluentTheme dependency).
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

    /// <summary>Identifies the <see cref="MinDate"/> property.</summary>
    public static readonly StyledProperty<DateTime?> MinDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MinDate));

    /// <summary>Identifies the <see cref="MaxDate"/> property.</summary>
    public static readonly StyledProperty<DateTime?> MaxDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MaxDate));

    private Border? _box;
    private Text? _display;
    private Text? _label;
    private IDisposable? _displayForeground;
    private Flyout? _flyout;

    /// <summary>Creates the picker.</summary>
    public DatePicker() => Focusable = true;

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

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(DatePicker);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _box = e.NameScope.Find("PART_Box") as Border;
        _display = e.NameScope.Find("PART_Display") as Text;
        _label = e.NameScope.Find("PART_Label") as Text;
        if (_box is not null)
        {
            _box.PointerPressed += (_, _) =>
            {
                Focus();
                Open();
            };
        }

        UpdateLabel();
        UpdateDisplay();
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
        else if (change.Property == LabelProperty)
        {
            UpdateLabel();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (InteractionAssist.IsActivationKey(e.Key))
        {
            Open();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            _flyout?.Hide();
            e.Handled = true;
        }
    }

    private void Open()
    {
        var calendar = new MonthCalendar { SelectedDate = Date, MinDate = MinDate, MaxDate = MaxDate };
        if (Date is { } date)
        {
            calendar.DisplayMonth = new DateTime(date.Year, date.Month, 1);
        }

        calendar.DateSelected += picked =>
        {
            if (!MonthCalendar.IsDisabled(picked, MinDate, MaxDate))
            {
                Date = picked;
                _flyout?.Hide();
            }
        };

        _flyout = new Flyout
        {
            Content = new Paper { Elevation = 8, Padding = new Thickness(12), Content = calendar },
            Placement = PlacementMode.BottomEdgeAlignedLeft,
        };
        _flyout.ShowAt(_box ?? (Control)this);
    }

    private void UpdateLabel()
    {
        if (_label is not null)
        {
            _label.Text = Label;
            _label.IsVisible = !string.IsNullOrEmpty(Label);
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
        _display.Text = hasDate ? Date!.Value.ToString(DateFormat, CultureInfo.CurrentCulture) : Placeholder;
        _displayForeground?.Dispose();
        _displayForeground = _display.Bind(TextBlock.ForegroundProperty,
            this.GetResourceObservable(hasDate ? LoamTokens.TextPrimary : LoamTokens.TextSecondary));
        InteractionAssist.SetAutomationName(this, Label, _display.Text, Placeholder);
    }
}
