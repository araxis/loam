using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A time input with a popup, mirroring the reference API's <c>TimePicker</c>. An outlined box shows
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

    /// <summary>Identifies the <see cref="MinuteStep"/> property.</summary>
    public static readonly StyledProperty<int> MinuteStepProperty =
        AvaloniaProperty.Register<TimePicker, int>(nameof(MinuteStep), 5);

    private readonly List<(Border Row, int Value)> _hourRows = new();
    private readonly List<(Border Row, int Value)> _minuteRows = new();
    private Border? _box;
    private Text? _display;
    private Text? _label;
    private IDisposable? _displayForeground;
    private Flyout? _flyout;

    /// <summary>Creates the picker.</summary>
    public TimePicker() => Focusable = true;

    /// <summary>The selected time (two-way). Mirrors the reference API's <c>Time</c>.</summary>
    public TimeSpan? Time
    {
        get => GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
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

    /// <summary>Granularity of the minute column. Mirrors the reference API's <c>MinuteSelectionStep</c>.</summary>
    public int MinuteStep
    {
        get => GetValue(MinuteStepProperty);
        set => SetValue(MinuteStepProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TimePicker);

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
        if (change.Property == TimeProperty || change.Property == TimeFormatProperty ||
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
        var current = Time ?? TimeSpan.Zero;
        var hours = BuildColumn("Hour", _hourRows, Enumerable.Range(0, 24), current.Hours, h => SetTime(h, null));

        var step = Math.Clamp(MinuteStep, 1, 30);
        var minuteValues = new List<int>();
        for (var m = 0; m < 60; m += step)
        {
            minuteValues.Add(m);
        }

        var minutes = BuildColumn("Min", _minuteRows, minuteValues, current.Minutes - current.Minutes % step, m => SetTime(null, m));

        _flyout = new Flyout
        {
            Content = new Paper
            {
                Elevation = 8,
                Padding = new Thickness(8),
                Content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { hours, minutes } },
            },
            Placement = PlacementMode.BottomEdgeAlignedLeft,
        };
        _flyout.ShowAt(_box ?? (Control)this);
    }

    private StackPanel BuildColumn(string heading, List<(Border Row, int Value)> rows, IEnumerable<int> values, int selected, Action<int> onPick)
    {
        rows.Clear();
        var list = new StackPanel();
        foreach (var value in values)
        {
            var label = new Text
            {
                Text = value.ToString("00", CultureInfo.CurrentCulture),
                Typo = Typo.Body2,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            var row = new Border
            {
                Child = label,
                Padding = new Thickness(16, 6),
                CornerRadius = new CornerRadius(4),
                Cursor = HandCursor,
                Background = Brushes.Transparent,
            };
            var captured = value;
            row.PointerPressed += (_, _) => onPick(captured);
            rows.Add((row, value));
            list.Children.Add(row);
        }

        Highlight(rows, selected);

        return new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new Text { Text = heading, Typo = Typo.Caption, Color = LoamColor.Default, Opacity = 0.6, HorizontalAlignment = HorizontalAlignment.Center },
                new ScrollViewer { Height = 200, Content = list },
            },
        };
    }

    private void Highlight(List<(Border Row, int Value)> rows, int selected)
    {
        foreach (var (row, value) in rows)
        {
            var label = (Text)row.Child!;
            if (value == selected)
            {
                row.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Primary));
                label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.PrimaryContrastText));
            }
            else
            {
                row.Background = Brushes.Transparent;
                label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.TextPrimary));
            }
        }
    }

    private void SetTime(int? hour, int? minute)
    {
        var current = Time ?? TimeSpan.Zero;
        var h = hour ?? current.Hours;
        var m = minute ?? current.Minutes;
        Time = new TimeSpan(h, m, 0);

        if (hour is not null)
        {
            Highlight(_hourRows, h);
        }

        if (minute is not null)
        {
            Highlight(_minuteRows, m);
        }
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

        var hasTime = Time is not null;
        _display.Text = hasTime
            ? DateTime.Today.Add(Time!.Value).ToString(TimeFormat, CultureInfo.CurrentCulture)
            : Placeholder;
        _displayForeground?.Dispose();
        _displayForeground = _display.Bind(TextBlock.ForegroundProperty,
            this.GetResourceObservable(hasTime ? LoamTokens.TextPrimary : LoamTokens.TextSecondary));
        InteractionAssist.SetAutomationName(this, Label, _display.Text, Placeholder);
    }
}
