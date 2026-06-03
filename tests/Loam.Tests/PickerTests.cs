using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam.Controls;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class PickerTests
{
    private static void Show(Control content)
    {
        new Window { Width = 400, Height = 400, Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void DatePicker_display_shows_placeholder_then_formatted_date()
    {
        var picker = new Loam.Controls.DatePicker { DateFormat = "yyyy-MM-dd", Placeholder = "Pick a date" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var display = picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Display");
        display.Text.ShouldBe("Pick a date");

        picker.Date = new DateTime(2026, 6, 3);
        Dispatcher.UIThread.RunJobs();
        display.Text.ShouldBe("2026-06-03");
    }

    [AvaloniaFact]
    public void MonthCalendar_renders_one_cell_per_day_in_the_month()
    {
        var calendar = new MonthCalendar { DisplayMonth = new DateTime(2026, 2, 1) }; // February 2026 → 28 days
        Show(calendar);
        Dispatcher.UIThread.RunJobs();

        var dayCells = calendar.GetVisualDescendants().OfType<Border>().Where(b => b.Child is Text).ToList();
        dayCells.Count.ShouldBe(28);

        calendar.DisplayMonth = new DateTime(2026, 1, 1); // January → 31 days
        Dispatcher.UIThread.RunJobs();
        calendar.GetVisualDescendants().OfType<Border>().Count(b => b.Child is Text).ShouldBe(31);
    }

    [AvaloniaFact]
    public void TimePicker_display_shows_placeholder_then_formatted_time()
    {
        var picker = new Loam.Controls.TimePicker { TimeFormat = "HH:mm", Placeholder = "Pick a time" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var display = picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Display");
        display.Text.ShouldBe("Pick a time");

        picker.Time = new TimeSpan(14, 30, 0);
        Dispatcher.UIThread.RunJobs();
        display.Text.ShouldBe("14:30");
    }

    [Fact]
    public void ColorPicker_tohex_formats_uppercase_rgb()
    {
        ColorPicker.ToHex(Color.Parse("#0a0b0c")).ShouldBe("#0A0B0C");
        ColorPicker.ToHex(Colors.White).ShouldBe("#FFFFFF");
    }

    [Fact]
    public void DateRangePicker_format_handles_partial_and_full_ranges()
    {
        DateRangePicker.Format(null, null, "yyyy-MM-dd").ShouldBeNull();
        DateRangePicker.Format(new DateTime(2026, 6, 1), null, "yyyy-MM-dd").ShouldBe("2026-06-01");
        DateRangePicker.Format(new DateTime(2026, 6, 1), new DateTime(2026, 6, 10), "yyyy-MM-dd")
            .ShouldBe("2026-06-01 – 2026-06-10");
    }

    [AvaloniaFact]
    public void DateRangePicker_display_reflects_the_range()
    {
        var picker = new DateRangePicker
        {
            DateFormat = "yyyy-MM-dd",
            Start = new DateTime(2026, 6, 1),
            End = new DateTime(2026, 6, 10),
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Display").Text
            .ShouldBe("2026-06-01 – 2026-06-10");
    }

    [AvaloniaFact]
    public void ColorPicker_shows_value_swatch_and_hex()
    {
        var picker = new ColorPicker { Value = Color.Parse("#FF5722") };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Hex").Text.ShouldBe("#FF5722");
        var swatch = picker.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Swatch");
        ((ISolidColorBrush)swatch.Background!).Color.ShouldBe(Color.Parse("#FF5722"));

        picker.Value = Colors.Black;
        Dispatcher.UIThread.RunJobs();
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Hex").Text.ShouldBe("#000000");
    }
}
