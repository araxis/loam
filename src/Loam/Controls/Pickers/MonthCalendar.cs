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

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedDateProperty || change.Property == DisplayMonthProperty)
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
            Cursor = HandCursor,
            Background = Brushes.Transparent,
        };

        if (isSelected)
        {
            cell.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Primary));
            label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.PrimaryContrastText));
        }
        else if (isToday)
        {
            cell.BorderThickness = new Thickness(1);
            cell.Bind(Border.BorderBrushProperty, this.GetResourceObservable(LoamTokens.Primary));
        }

        cell.PointerPressed += (_, _) => DateSelected?.Invoke(date);
        return cell;
    }
}
