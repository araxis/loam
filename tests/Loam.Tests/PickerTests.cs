using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
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

    private static KeyEventArgs KeyArgs(Key key) => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = key,
    };

    private static Border Box(Control control) =>
        control.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Box");

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
    public void ColorPicker_formats_alpha_and_converts_hsv()
    {
        ColorPicker.ToHexWithAlpha(Color.FromArgb(0x80, 0x10, 0x20, 0x30)).ShouldBe("#80102030");

        ColorPicker.FromHsv(120, 1, 1, 0x80).ShouldBe(Color.FromArgb(0x80, 0, 255, 0));

        var original = Color.Parse("#336699");
        var hsv = ColorPicker.ToHsv(original);
        hsv.Hue.ShouldBe(210d, 0.001);
        hsv.Saturation.ShouldBe(2d / 3d, 0.001);
        hsv.Value.ShouldBe(0.6d, 0.001);
        ColorPicker.FromHsv(hsv.Hue, hsv.Saturation, hsv.Value).ShouldBe(original);
    }

    [Fact]
    public void DateRangePicker_format_handles_partial_and_full_ranges()
    {
        DateRangePicker.Format(null, null, "yyyy-MM-dd").ShouldBeNull();
        DateRangePicker.Format(new DateTime(2026, 6, 1), null, "yyyy-MM-dd").ShouldBe("2026-06-01");
        DateRangePicker.Format(new DateTime(2026, 6, 1), new DateTime(2026, 6, 10), "yyyy-MM-dd")
            .ShouldBe("2026-06-01 – 2026-06-10");
    }

    [Fact]
    public void MonthCalendar_bounds_and_range_helpers_handle_dates()
    {
        var min = new DateTime(2026, 6, 3);
        var max = new DateTime(2026, 6, 10);

        MonthCalendar.IsDisabled(new DateTime(2026, 6, 2), min, max).ShouldBeTrue();
        MonthCalendar.IsDisabled(new DateTime(2026, 6, 11), min, max).ShouldBeTrue();
        MonthCalendar.IsDisabled(new DateTime(2026, 6, 6), min, max).ShouldBeFalse();

        MonthCalendar.IsInRange(new DateTime(2026, 6, 6), max, min).ShouldBeTrue();
        MonthCalendar.IsInRange(new DateTime(2026, 6, 12), max, min).ShouldBeFalse();
        MonthCalendar.IsInRange(new DateTime(2026, 6, 6), min, null).ShouldBeFalse();
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

    [AvaloniaFact]
    public void ColorPicker_showalpha_displays_alpha_hex()
    {
        var picker = new ColorPicker
        {
            ShowAlpha = true,
            Value = Color.FromArgb(0x80, 0x10, 0x20, 0x30),
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var hex = picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Hex");
        hex.Text.ShouldBe("#80102030");

        picker.ShowAlpha = false;
        Dispatcher.UIThread.RunJobs();
        hex.Text.ShouldBe("#102030");
    }

    [AvaloniaFact]
    public void Pickers_are_focusable_named_and_keyboard_openable()
    {
        Control[] pickers =
        [
            new Loam.Controls.DatePicker { Label = "Start date" },
            new DateRangePicker { Label = "Range" },
            new Loam.Controls.TimePicker { Label = "Start time" },
            new ColorPicker { Label = "Accent" },
        ];

        foreach (var picker in pickers)
        {
            Show(picker);
            picker.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            picker.Focusable.ShouldBeTrue();
            Box(picker).Focusable.ShouldBeTrue();
            AutomationProperties.GetName(picker).ShouldNotBeNullOrWhiteSpace();

            var open = KeyArgs(Key.Space);
            picker.RaiseEvent(open);
            open.Handled.ShouldBeTrue();

            var close = KeyArgs(Key.Escape);
            picker.RaiseEvent(close);
            close.Handled.ShouldBeTrue();
        }
    }
}
