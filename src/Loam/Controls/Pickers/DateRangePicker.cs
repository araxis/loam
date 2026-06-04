using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A date-range input, mirroring the reference API's <c>DateRangePicker</c>. An outlined box shows the
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

    private Border? _box;
    private Text? _display;
    private Text? _label;
    private IDisposable? _displayForeground;
    private Flyout? _flyout;

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

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(DateRangePicker);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _box = e.NameScope.Find("PART_Box") as Border;
        _display = e.NameScope.Find("PART_Display") as Text;
        _label = e.NameScope.Find("PART_Label") as Text;
        if (_box is not null)
        {
            _box.PointerPressed += (_, _) => Open();
        }

        UpdateLabel();
        UpdateDisplay();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StartProperty || change.Property == EndProperty || change.Property == DateFormatProperty)
        {
            UpdateDisplay();
        }
        else if (change.Property == LabelProperty)
        {
            UpdateLabel();
        }
    }

    private void Open()
    {
        var calendar = new MonthCalendar { SelectedDate = End ?? Start };
        if ((Start ?? End) is { } anchor)
        {
            calendar.DisplayMonth = new DateTime(anchor.Year, anchor.Month, 1);
        }

        calendar.DateSelected += picked =>
        {
            if (Start is null || End is not null)
            {
                Start = picked;
                End = null;
                calendar.SelectedDate = picked;
            }
            else if (picked < Start)
            {
                End = Start;
                Start = picked;
                _flyout?.Hide();
            }
            else
            {
                End = picked;
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
    }
}
