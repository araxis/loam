using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A self-contained Material month grid used by <see cref="DatePicker"/> (so Loam needs no FluentTheme
/// <c>Calendar</c>). Shows <see cref="DisplayMonth"/> with prev/next navigation; clicking a day raises
/// <see cref="DateSelected"/> and the chosen <see cref="SelectedDate"/> is highlighted.
/// </summary>
public class MonthCalendar : Decorator
{
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    /// <summary>Identifies the <see cref="SelectedDate"/> property.</summary>
    public static readonly StyledProperty<DateTime?> SelectedDateProperty =
        AvaloniaProperty.Register<MonthCalendar, DateTime?>(nameof(SelectedDate));

    /// <summary>Identifies the <see cref="DisplayMonth"/> property.</summary>
    public static readonly StyledProperty<DateTime> DisplayMonthProperty =
        AvaloniaProperty.Register<MonthCalendar, DateTime>(nameof(DisplayMonth));

    /// <summary>Identifies the <see cref="MinDate"/> property.</summary>
    public static readonly StyledProperty<DateTime?> MinDateProperty =
        AvaloniaProperty.Register<MonthCalendar, DateTime?>(nameof(MinDate));

    /// <summary>Identifies the <see cref="MaxDate"/> property.</summary>
    public static readonly StyledProperty<DateTime?> MaxDateProperty =
        AvaloniaProperty.Register<MonthCalendar, DateTime?>(nameof(MaxDate));

    /// <summary>Identifies the <see cref="RangeStart"/> property.</summary>
    public static readonly StyledProperty<DateTime?> RangeStartProperty =
        AvaloniaProperty.Register<MonthCalendar, DateTime?>(nameof(RangeStart));

    /// <summary>Identifies the <see cref="RangeEnd"/> property.</summary>
    public static readonly StyledProperty<DateTime?> RangeEndProperty =
        AvaloniaProperty.Register<MonthCalendar, DateTime?>(nameof(RangeEnd));

    /// <summary>Creates the calendar showing the current month.</summary>
    public MonthCalendar()
    {
        var today = DateTime.Today;
        DisplayMonth = new DateTime(today.Year, today.Month, 1);
        Build();
    }

    /// <summary>Raised with the day a user clicks.</summary>
    public event Action<DateTime>? DateSelected;

    /// <summary>The selected day (highlighted).</summary>
    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    /// <summary>The first day of the displayed month.</summary>
    public DateTime DisplayMonth
    {
        get => GetValue(DisplayMonthProperty);
        set => SetValue(DisplayMonthProperty, value);
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

    /// <summary>Range highlight start.</summary>
    public DateTime? RangeStart
    {
        get => GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }

    /// <summary>Range highlight end.</summary>
    public DateTime? RangeEnd
    {
        get => GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedDateProperty || change.Property == DisplayMonthProperty ||
            change.Property == MinDateProperty || change.Property == MaxDateProperty ||
            change.Property == RangeStartProperty || change.Property == RangeEndProperty)
        {
            Build();
        }
    }

    private void Build()
    {
        var monthLabel = new Text
        {
            Text = DisplayMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture),
            Typo = Typo.Subtitle1,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var prev = new IconButton { Icon = Icons.Material.Filled.ArrowBack, Size = LoamSize.Small };
        prev.Click += (_, _) => DisplayMonth = DisplayMonth.AddMonths(-1);
        DockPanel.SetDock(prev, Dock.Left);

        var next = new IconButton { Icon = Icons.Material.Filled.ArrowForward, Size = LoamSize.Small };
        next.Click += (_, _) => DisplayMonth = DisplayMonth.AddMonths(1);
        DockPanel.SetDock(next, Dock.Right);

        var header = new DockPanel { LastChildFill = true, Children = { prev, next, monthLabel } };

        var weekdays = new UniformGrid { Columns = 7 };
        var names = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames; // Sunday-first
        foreach (var name in names)
        {
            weekdays.Children.Add(new Text
            {
                Text = name.Length > 2 ? name[..2] : name,
                Typo = Typo.Caption,
                Color = LoamColor.Default,
                Opacity = 0.6,
                HorizontalAlignment = HorizontalAlignment.Center,
            });
        }

        var days = new UniformGrid { Columns = 7 };
        var leading = (int)DisplayMonth.DayOfWeek; // Sunday = 0
        for (var i = 0; i < leading; i++)
        {
            days.Children.Add(new Control());
        }

        var today = DateTime.Today;
        var daysInMonth = DateTime.DaysInMonth(DisplayMonth.Year, DisplayMonth.Month);
        for (var day = 1; day <= daysInMonth; day++)
        {
            days.Children.Add(BuildDayCell(new DateTime(DisplayMonth.Year, DisplayMonth.Month, day), today));
        }

        Child = new StackPanel
        {
            Width = 7 * 36,
            Spacing = 6,
            Children = { header, weekdays, days },
        };
    }

    private Border BuildDayCell(DateTime date, DateTime today)
    {
        var isSelected = SelectedDate?.Date == date;
        var isToday = date == today;
        var inRange = IsInRange(date, RangeStart, RangeEnd);
        var disabled = IsDisabled(date, MinDate, MaxDate);

        var label = new Text
        {
            Text = date.Day.ToString(CultureInfo.CurrentCulture),
            Typo = Typo.Body2,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var cell = new Border
        {
            Width = 32,
            Height = 32,
            Margin = new Thickness(2),
            CornerRadius = new CornerRadius(16),
            Child = label,
            Cursor = disabled ? null : HandCursor,
            Background = Brushes.Transparent,
            Opacity = disabled ? 0.35 : 1,
        };

        if (isSelected)
        {
            cell.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Primary));
            label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.PrimaryContrastText));
        }
        else if (inRange)
        {
            cell.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.PaletteHover(nameof(LoamPalette.Primary))));
            label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.Primary));
        }
        else if (isToday)
        {
            cell.BorderThickness = new Thickness(1);
            cell.Bind(Border.BorderBrushProperty, this.GetResourceObservable(LoamTokens.Primary));
        }

        if (!disabled)
        {
            cell.PointerPressed += (_, _) => DateSelected?.Invoke(date);
        }

        return cell;
    }

    /// <summary>Whether <paramref name="date"/> is outside the selectable bounds.</summary>
    public static bool IsDisabled(DateTime date, DateTime? min, DateTime? max) =>
        (min is not null && date.Date < min.Value.Date) || (max is not null && date.Date > max.Value.Date);

    /// <summary>Whether <paramref name="date"/> is inside the given range.</summary>
    public static bool IsInRange(DateTime date, DateTime? start, DateTime? end)
    {
        if (start is null || end is null)
        {
            return false;
        }

        var min = start.Value.Date <= end.Value.Date ? start.Value.Date : end.Value.Date;
        var max = start.Value.Date <= end.Value.Date ? end.Value.Date : start.Value.Date;
        return date.Date >= min && date.Date <= max;
    }
}
