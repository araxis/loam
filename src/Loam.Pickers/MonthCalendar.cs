using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A self-contained Material month grid used by <see cref="DatePicker"/> (so Loam needs no FluentTheme
/// <c>Calendar</c>). Shows <see cref="DisplayMonth"/> with prev/next navigation; clicking a day raises
/// <see cref="DateSelected"/> and the chosen <see cref="SelectedDate"/> is highlighted.
/// </summary>
public class MonthCalendar : Decorator
{
    private const double CalendarWidth = 336;
    private const double MonthHeaderHeight = 56;
    private const double DaySlotSize = 48;
    private const double DayContainerSize = 40;
    private const int DaysInWeek = 7;
    private const int MaxCalendarRows = 6;
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

    /// <summary>Identifies the <see cref="FirstDayOfWeek"/> property.</summary>
    public static readonly StyledProperty<DayOfWeek> FirstDayOfWeekProperty =
        AvaloniaProperty.Register<MonthCalendar, DayOfWeek>(
            nameof(FirstDayOfWeek),
            CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

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

    /// <summary>The weekday shown in the first calendar column.</summary>
    public DayOfWeek FirstDayOfWeek
    {
        get => GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    /// <summary>Moves the visible month back by one month.</summary>
    public void PreviousMonth() => DisplayMonth = DisplayMonth.AddMonths(-1);

    /// <summary>Moves the visible month forward by one month.</summary>
    public void NextMonth() => DisplayMonth = DisplayMonth.AddMonths(1);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedDateProperty || change.Property == DisplayMonthProperty ||
            change.Property == MinDateProperty || change.Property == MaxDateProperty ||
            change.Property == RangeStartProperty || change.Property == RangeEndProperty ||
            change.Property == FirstDayOfWeekProperty)
        {
            Build();
        }
    }

    private void Build()
    {
        var monthLabel = new Text
        {
            Text = DisplayMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture),
            Typo = Typo.TitleSmall,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var prev = new IconButton
        {
            Icon = Icons.Material.Filled.ArrowBack,
            Size = LoamSize.Small,
            Width = DaySlotSize,
            Height = DaySlotSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AutomationProperties.SetName(prev, "Previous month");
        AutomationProperties.SetHelpText(prev, "Show previous month");
        prev.Click += (_, _) => PreviousMonth();

        var next = new IconButton
        {
            Icon = Icons.Material.Filled.ArrowForward,
            Size = LoamSize.Small,
            Width = DaySlotSize,
            Height = DaySlotSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AutomationProperties.SetName(next, "Next month");
        AutomationProperties.SetHelpText(next, "Show next month");
        next.Click += (_, _) => NextMonth();

        var header = new Avalonia.Controls.Grid
        {
            Width = CalendarWidth,
            Height = MonthHeaderHeight,
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
            Children = { monthLabel, prev, next },
        };
        Avalonia.Controls.Grid.SetColumn(prev, 1);
        Avalonia.Controls.Grid.SetColumn(next, 2);

        var weekdays = new UniformGrid
        {
            Columns = DaysInWeek,
            Width = CalendarWidth,
            Height = DaySlotSize,
        };
        var names = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
        for (var i = 0; i < DaysInWeek; i++)
        {
            var name = names[((int)FirstDayOfWeek + i) % DaysInWeek];
            weekdays.Children.Add(new Text
            {
                Text = name.Length > 2 ? name[..2] : name,
                Typo = Typo.BodyMedium,
                Color = LoamColor.Default,
                Opacity = 0.6,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        var days = new UniformGrid
        {
            Columns = DaysInWeek,
            Rows = MaxCalendarRows,
            Width = CalendarWidth,
            Height = DaySlotSize * MaxCalendarRows,
        };
        var leading = ((int)DisplayMonth.DayOfWeek - (int)FirstDayOfWeek + DaysInWeek) % DaysInWeek;
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

        while (days.Children.Count < DaysInWeek * MaxCalendarRows)
        {
            days.Children.Add(new Control());
        }

        Child = new StackPanel
        {
            Width = CalendarWidth,
            Spacing = 0,
            HorizontalAlignment = HorizontalAlignment.Center,
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
            Typo = Typo.BodyLarge,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var stateLayer = new Border
        {
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(DayContainerSize / 2),
            IsHitTestVisible = false,
        };

        var cell = new Border
        {
            Width = DayContainerSize,
            Height = DayContainerSize,
            Margin = new Thickness((DaySlotSize - DayContainerSize) / 2),
            CornerRadius = new CornerRadius(DayContainerSize / 2),
            Child = new Avalonia.Controls.Grid { Children = { stateLayer, label } },
            Cursor = disabled ? null : HandCursor,
            Background = Brushes.Transparent,
            Opacity = disabled ? 0.35 : 1,
            Focusable = !disabled,
            ClipToBounds = true,
        };
        AutomationProperties.SetName(cell, date.ToString("D", CultureInfo.CurrentCulture));
        if (disabled)
        {
            AutomationProperties.SetHelpText(cell, "Unavailable date");
        }

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
            cell.KeyDown += (_, args) =>
            {
                if (InteractionAssist.IsActivationKey(args.Key))
                {
                    DateSelected?.Invoke(date);
                    args.Handled = true;
                }
                else if (TryNavigateFrom(date, args.Key))
                {
                    args.Handled = true;
                }
            };

            IDisposable? stateBinding = null;
            void SetStateLayer(string? state)
            {
                stateBinding?.Dispose();
                stateBinding = null;
                if (state is null)
                {
                    stateLayer.Background = Brushes.Transparent;
                    return;
                }

                var role = isSelected ? nameof(LoamColorScheme.OnPrimary) : nameof(LoamColorScheme.OnSurface);
                stateBinding = stateLayer.Bind(Border.BackgroundProperty,
                    this.GetResourceObservable(LoamTokens.ColorSchemeStateLayer(role, state)));
            }

            cell.PointerEntered += (_, _) => SetStateLayer("Hover");
            cell.PointerExited += (_, _) => SetStateLayer(cell.IsFocused ? "Focus" : null);
            cell.GotFocus += (_, _) => SetStateLayer("Focus");
            cell.LostFocus += (_, _) => SetStateLayer(null);
        }

        return cell;
    }

    private bool TryNavigateFrom(DateTime date, Key key)
    {
        var target = key switch
        {
            Key.Left => date.AddDays(-1),
            Key.Right => date.AddDays(1),
            Key.Up => date.AddDays(-DaysInWeek),
            Key.Down => date.AddDays(DaysInWeek),
            Key.PageUp => AddMonthsPreservingDay(date, -1),
            Key.PageDown => AddMonthsPreservingDay(date, 1),
            _ => (DateTime?)null,
        };

        if (target is null || IsDisabled(target.Value, MinDate, MaxDate))
        {
            return false;
        }

        FocusDate(target.Value);
        return true;
    }

    private void FocusDate(DateTime date)
    {
        var month = new DateTime(date.Year, date.Month, 1);
        if (DisplayMonth != month)
        {
            DisplayMonth = month;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var name = date.ToString("D", CultureInfo.CurrentCulture);
            this.GetVisualDescendants().OfType<Border>()
                .FirstOrDefault(border => AutomationProperties.GetName(border) == name)
                ?.Focus();
        });
    }

    private static DateTime AddMonthsPreservingDay(DateTime date, int months)
    {
        var month = new DateTime(date.Year, date.Month, 1).AddMonths(months);
        var day = Math.Min(date.Day, DateTime.DaysInMonth(month.Year, month.Month));
        return new DateTime(month.Year, month.Month, day);
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
