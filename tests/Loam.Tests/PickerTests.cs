using System.Globalization;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Loam.Controls.Internal;
using Loam.Theming;
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

    private static KeyEventArgs KeyArgs(Key key, KeyModifiers modifiers) => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = key,
        KeyModifiers = modifiers,
    };

    private static Border Box(Control control) =>
        control.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Box");

    private static Flyout OpenedFlyout(Control picker)
    {
        var field = picker.GetType().GetField("_flyout",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        var flyout = field!.GetValue(picker) as Flyout;
        flyout.ShouldNotBeNull();
        return flyout!;
    }

    private static Flyout? MaybeOpenedFlyout(Control picker)
    {
        var field = picker.GetType().GetField("_flyout",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return field!.GetValue(picker) as Flyout;
    }

    private static Border CalendarDay(MonthCalendar calendar, DateTime date) =>
        calendar.GetVisualDescendants().OfType<Border>()
            .First(b => AutomationProperties.GetName(b) == date.ToString("D", CultureInfo.CurrentCulture));

    private static Border StateLayer(Border row) =>
        row.Child.ShouldBeOfType<Avalonia.Controls.Grid>().Children.OfType<Border>().First();

    private static Border PopupRow(Paper paper, string name) =>
        paper.GetVisualDescendants().OfType<Border>()
            .First(b => AutomationProperties.GetName(b) == name);

    private static IconButton ClearButton(Control picker, string name) =>
        picker.GetVisualDescendants().OfType<IconButton>().First(b => AutomationProperties.GetName(b) == name);

    [AvaloniaFact]
    public void DatePicker_clear_button_resets_value_and_does_not_open()
    {
        var raised = false;
        DateTime? captured = DateTime.MaxValue;
        var picker = new Loam.Controls.DatePicker { Clearable = true, Date = new DateTime(2026, 6, 14) };
        picker.DateSelected += d => { raised = true; captured = d; };
        Show(picker);
        Dispatcher.UIThread.RunJobs();

        var clear = ClearButton(picker, "Clear date");
        clear.IsVisible.ShouldBeTrue();

        clear.RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Date.ShouldBeNull();
        raised.ShouldBeTrue();
        captured.ShouldBeNull();
        MaybeOpenedFlyout(picker).ShouldBeNull(); // clearing did not open the calendar
        clear.IsVisible.ShouldBeFalse();           // hidden once the value is gone
    }

    [AvaloniaFact]
    public void Picker_clear_button_hidden_without_clearable_or_value()
    {
        var noFlag = new Loam.Controls.DatePicker { Date = new DateTime(2026, 6, 14) };
        Show(noFlag);
        Dispatcher.UIThread.RunJobs();
        ClearButton(noFlag, "Clear date").IsVisible.ShouldBeFalse();

        var noValue = new Loam.Controls.DatePicker { Clearable = true };
        Show(noValue);
        Dispatcher.UIThread.RunJobs();
        ClearButton(noValue, "Clear date").IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void TimePicker_and_DateRangePicker_clear_buttons_reset_values()
    {
        var time = new Loam.Controls.TimePicker { Clearable = true, Time = new TimeSpan(9, 30, 0) };
        Show(time);
        Dispatcher.UIThread.RunJobs();
        var timeClear = ClearButton(time, "Clear time");
        timeClear.IsVisible.ShouldBeTrue();
        timeClear.RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        time.Time.ShouldBeNull();

        var range = new DateRangePicker { Clearable = true, Start = new DateTime(2026, 6, 1), End = new DateTime(2026, 6, 14) };
        Show(range);
        Dispatcher.UIThread.RunJobs();
        var rangeClear = ClearButton(range, "Clear dates");
        rangeClear.IsVisible.ShouldBeTrue();
        rangeClear.RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        range.Start.ShouldBeNull();
        range.End.ShouldBeNull();
    }

    private static Loam.Controls.Button PopupButton(Paper paper, string content) =>
        paper.GetVisualDescendants().OfType<Loam.Controls.Button>().First(b => (string?)b.Content == content);

    private static List<string?> PopupButtonLabels(Paper paper) =>
        paper.GetVisualDescendants().OfType<Loam.Controls.Button>().Select(b => (string?)b.Content).ToList();

    [Fact]
    public void DateRangePicker_default_presets_resolve_expected_ranges()
    {
        var anchor = new DateTime(2026, 6, 15); // mid-month, mid-year
        var byLabel = DateRangePicker.DefaultPresets.ToDictionary(p => p.Label, p => p.Resolve(anchor));

        byLabel["Today"].ShouldBe((anchor, anchor));
        byLabel["Yesterday"].ShouldBe((anchor.AddDays(-1), anchor.AddDays(-1)));
        byLabel["Last 7 days"].ShouldBe((anchor.AddDays(-6), anchor));
        byLabel["Last 30 days"].ShouldBe((anchor.AddDays(-29), anchor));
        byLabel["This month"].ShouldBe((new DateTime(2026, 6, 1), new DateTime(2026, 6, 30)));
        byLabel["Last month"].ShouldBe((new DateTime(2026, 5, 1), new DateTime(2026, 5, 31)));
        byLabel["This year"].ShouldBe((new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)));
    }

    [AvaloniaFact]
    public void DateRangePicker_preset_rail_sets_pending_then_ok_commits()
    {
        var committed = false;
        var picker = new DateRangePicker { ShowPresets = true };
        picker.RangeSelected += (_, _) => committed = true;
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        PopupButton(paper, "Last 7 days").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Start.ShouldBeNull(); // preset only stages a pending range
        committed.ShouldBeFalse();

        PopupButton(paper, "OK").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        committed.ShouldBeTrue();
        picker.Start.ShouldNotBeNull();
        picker.End.ShouldNotBeNull();
        (picker.Start!.Value <= picker.End!.Value).ShouldBeTrue();
        (picker.End!.Value - picker.Start!.Value).Days.ShouldBe(6); // last 7 days, inclusive
    }

    [AvaloniaFact]
    public void DateRangePicker_preset_rail_absent_without_ShowPresets()
    {
        var picker = new DateRangePicker();
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var labels = PopupButtonLabels(paper);
        labels.ShouldContain("OK");            // flyout built
        labels.ShouldNotContain("Last 7 days"); // but no preset rail
    }

    [AvaloniaFact]
    public void DateRangePicker_custom_presets_replace_defaults()
    {
        var picker = new DateRangePicker { ShowPresets = true };
        picker.Presets.Add(new DateRangePreset("Just today", a => (a, a)));
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var labels = PopupButtonLabels(paper);
        labels.ShouldContain("Just today");
        labels.ShouldNotContain("Last 7 days"); // defaults no longer used
    }

    [AvaloniaFact]
    public void DateRangePicker_preset_clamps_to_min_bound()
    {
        var picker = new DateRangePicker { ShowPresets = true, MinDate = DateTime.Today };
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        // "Last 7 days" would start before today, but MinDate clamps the start up to today.
        PopupButton(paper, "Last 7 days").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        PopupButton(paper, "OK").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Start.ShouldBe(DateTime.Today);
    }

    [AvaloniaFact]
    public void DateRangePicker_preset_clamps_to_max_bound()
    {
        var committed = false;
        var picker = new DateRangePicker { ShowPresets = true, MaxDate = new DateTime(2026, 6, 30) };
        picker.RangeSelected += (_, _) => committed = true;
        picker.Presets.Add(new DateRangePreset("Wide year", _ => (new DateTime(2026, 1, 1), new DateTime(2026, 12, 31))));
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        PopupButton(paper, "Wide year").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        PopupButton(paper, "OK").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        committed.ShouldBeTrue();
        picker.Start.ShouldBe(new DateTime(2026, 1, 1));
        picker.End.ShouldBe(new DateTime(2026, 6, 30)); // end clamped down to MaxDate
    }

    [AvaloniaFact]
    public void DateRangePicker_preset_outside_bounds_stages_nothing()
    {
        var picker = new DateRangePicker
        {
            ShowPresets = true,
            MinDate = new DateTime(2026, 6, 1),
            MaxDate = new DateTime(2026, 6, 30),
        };
        picker.Presets.Add(new DateRangePreset("Far past", _ => (new DateTime(2020, 1, 1), new DateTime(2020, 1, 31))));
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var calendar = paper.GetVisualDescendants().OfType<MonthCalendar>().Single();
        PopupButton(paper, "Far past").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        // The preset is entirely outside [Min, Max]; clamping leaves start > end, so it must stage nothing.
        calendar.RangeStart.ShouldBeNull();
        calendar.RangeEnd.ShouldBeNull();
    }

    [AvaloniaFact]
    public void DateRangePicker_preset_buttons_are_keyboard_focusable()
    {
        var picker = new DateRangePicker { ShowPresets = true };
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var button = PopupButton(paper, "Last 7 days");
        button.Focusable.ShouldBeTrue();

        button.Focus();
        Dispatcher.UIThread.RunJobs();
        button.IsFocused.ShouldBeTrue();
    }

    [Fact]
    public void DateRangePicker_TryParseRange_handles_empty_single_pair_ordering_and_invalid()
    {
        DateRangePicker.TryParseRange("  ", "yyyy-MM-dd", out var es, out var ee).ShouldBeTrue();
        es.ShouldBeNull();
        ee.ShouldBeNull();

        DateRangePicker.TryParseRange("2026-06-01", "yyyy-MM-dd", out var ss, out var se).ShouldBeTrue();
        ss.ShouldBe(new DateTime(2026, 6, 1)); // a single date is the start, end stays null
        se.ShouldBeNull();

        DateRangePicker.TryParseRange("2026-06-01 – 2026-06-14", "yyyy-MM-dd", out var ps, out var pe).ShouldBeTrue();
        ps.ShouldBe(new DateTime(2026, 6, 1));
        pe.ShouldBe(new DateTime(2026, 6, 14));

        DateRangePicker.TryParseRange("2026-06-01 to 2026-06-14", "yyyy-MM-dd", out var ts, out var te).ShouldBeTrue();
        ts.ShouldBe(new DateTime(2026, 6, 1));
        te.ShouldBe(new DateTime(2026, 6, 14));

        DateRangePicker.TryParseRange("2026-06-01 - 2026-06-14", "yyyy-MM-dd", out var hs, out var he).ShouldBeTrue();
        hs.ShouldBe(new DateTime(2026, 6, 1)); // space-hyphen-space separator
        he.ShouldBe(new DateTime(2026, 6, 14));

        // Reversed input is auto-ordered.
        DateRangePicker.TryParseRange("2026-06-14 – 2026-06-01", "yyyy-MM-dd", out var rs, out var re).ShouldBeTrue();
        rs.ShouldBe(new DateTime(2026, 6, 1));
        re.ShouldBe(new DateTime(2026, 6, 14));

        DateRangePicker.TryParseRange("nonsense", "yyyy-MM-dd", out var bs, out var be).ShouldBeFalse();
        bs.ShouldBeNull();
        be.ShouldBeNull();
    }

    [AvaloniaFact]
    public void DateRangePicker_non_editable_hides_text_input()
    {
        var picker = new DateRangePicker();
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        EditableInput(picker).IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void DateRangePicker_editable_parses_typed_range_on_enter()
    {
        DateTime? cs = null, ce = null;
        var raised = false;
        var picker = new DateRangePicker { Editable = true, DateFormat = "yyyy-MM-dd" };
        picker.RangeSelected += (s, e) => { raised = true; cs = s; ce = e; };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.IsVisible.ShouldBeTrue();
        input.Text = "2026-06-01 – 2026-06-14";
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Start.ShouldBe(new DateTime(2026, 6, 1));
        picker.End.ShouldBe(new DateTime(2026, 6, 14));
        picker.Error.ShouldBeFalse();
        raised.ShouldBeTrue();
        cs.ShouldBe(new DateTime(2026, 6, 1));
        ce.ShouldBe(new DateTime(2026, 6, 14));
    }

    [AvaloniaFact]
    public void DateRangePicker_editable_single_date_sets_start_only()
    {
        var picker = new DateRangePicker { Editable = true, DateFormat = "yyyy-MM-dd" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "2026-06-01";
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Start.ShouldBe(new DateTime(2026, 6, 1));
        picker.End.ShouldBeNull();
        picker.Error.ShouldBeFalse();
        input.Text.ShouldBe("2026-06-01"); // a single date round-trips unchanged (no spurious end)
    }

    [AvaloniaFact]
    public void DateRangePicker_editable_marks_error_on_invalid_text()
    {
        var picker = new DateRangePicker { Editable = true, DateFormat = "yyyy-MM-dd", Start = new DateTime(2026, 1, 1), End = new DateTime(2026, 1, 5) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "garbage";
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Error.ShouldBeTrue();
        picker.ErrorText.ShouldBe(picker.InvalidRangeText);
        picker.Start.ShouldBe(new DateTime(2026, 1, 1)); // unchanged
        picker.End.ShouldBe(new DateTime(2026, 1, 5));
    }

    [AvaloniaFact]
    public void DateRangePicker_editable_rejects_out_of_range()
    {
        var picker = new DateRangePicker
        {
            Editable = true,
            DateFormat = "yyyy-MM-dd",
            MinDate = new DateTime(2026, 6, 1),
            MaxDate = new DateTime(2026, 6, 30),
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "2026-06-10 – 2026-12-25"; // end outside MaxDate
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Error.ShouldBeTrue();
        picker.Start.ShouldBeNull(); // not committed
        picker.End.ShouldBeNull();
    }

    [AvaloniaFact]
    public void DateRangePicker_editable_empty_clears_and_syncs_from_value()
    {
        var picker = new DateRangePicker { Editable = true, DateFormat = "yyyy-MM-dd", Start = new DateTime(2026, 6, 1), End = new DateTime(2026, 6, 14) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text.ShouldBe("2026-06-01 – 2026-06-14"); // committed range flows into the text box

        input.Text = string.Empty;
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Start.ShouldBeNull();
        picker.End.ShouldBeNull();
        picker.Error.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void DateRangePicker_editable_reformats_to_ordered_endash()
    {
        var picker = new DateRangePicker { Editable = true, DateFormat = "yyyy-MM-dd" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "2026-06-14 to 2026-06-01"; // reversed + " to " separator
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        input.Text.ShouldBe("2026-06-01 – 2026-06-14"); // ordered + canonical en-dash separator
    }

    [AvaloniaFact]
    public void DateRangePicker_editable_calendar_button_opens_and_ok_syncs()
    {
        var picker = new DateRangePicker { Editable = true, DateFormat = "yyyy-MM-dd", Start = new DateTime(2026, 6, 1), End = new DateTime(2026, 6, 14) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldBeNull();
        picker.GetVisualDescendants().OfType<IconButton>().First(b => AutomationProperties.GetName(b) == "Open calendar")
            .RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        PopupButton(paper, "OK").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Start.ShouldBe(new DateTime(2026, 6, 1));
        EditableInput(picker).Text.ShouldBe("2026-06-01 – 2026-06-14");
    }

    [AvaloniaFact]
    public void DateRangePicker_editable_alt_down_opens_flyout()
    {
        var picker = new DateRangePicker { Editable = true };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Focus();
        Dispatcher.UIThread.RunJobs();
        MaybeOpenedFlyout(picker).ShouldBeNull();

        input.RaiseEvent(KeyArgs(Key.Down, KeyModifiers.Alt));
        Dispatcher.UIThread.RunJobs();
        MaybeOpenedFlyout(picker).ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void DateRangePicker_non_editable_activation_still_opens_flyout()
    {
        var picker = new DateRangePicker();
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldBeNull();
        picker.RaiseEvent(KeyArgs(Key.Space)); // non-editable activation is unchanged
        Dispatcher.UIThread.RunJobs();
        MaybeOpenedFlyout(picker).ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void DateRangePicker_editable_with_presets_commits_typed_range_and_shows_rail()
    {
        var picker = new DateRangePicker { Editable = true, ShowPresets = true, DateFormat = "yyyy-MM-dd" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        // Editable input and the preset rail coexist.
        var input = EditableInput(picker);
        input.IsVisible.ShouldBeTrue();
        input.Text = "2026-06-01 – 2026-06-14";
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Start.ShouldBe(new DateTime(2026, 6, 1)); // typed range committed, not overridden by presets
        picker.End.ShouldBe(new DateTime(2026, 6, 14));

        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();
        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        PopupButtonLabels(paper).ShouldContain("Last 7 days"); // preset rail still renders
    }

    // Find a time column by its heading ("Hour"/"Minute") rather than child order, so the test does
    // not silently bind to the wrong column if the layout is reordered or another scroller is added.
    private static ScrollViewer TimeColumn(Paper paper, string heading) =>
        paper.GetVisualDescendants().OfType<ScrollViewer>()
            .First(sv => sv.Parent is StackPanel column && column.Children.OfType<Text>().Any(t => t.Text == heading));

    private static void ShouldBeWithinViewport(ScrollViewer column, Border row)
    {
        var top = row.Bounds.Y - column.Offset.Y;
        top.ShouldBeGreaterThanOrEqualTo(-0.5);
        (top + row.Bounds.Height).ShouldBeLessThanOrEqualTo(column.Viewport.Height + 0.5);
    }

    private static void ShouldBeCenteredIn(ScrollViewer column, Border row)
    {
        var centerInViewport = (row.Bounds.Y - column.Offset.Y) + (row.Bounds.Height / 2);
        centerInViewport.ShouldBe(column.Viewport.Height / 2, 3.0);
    }

    // BringIntoView only guarantees the row is visible (it may align to an edge), so assert overlap, not containment.
    private static void ShouldOverlapViewport(ScrollViewer column, Border row)
    {
        var top = row.Bounds.Y - column.Offset.Y;
        (top + row.Bounds.Height).ShouldBeGreaterThan(0);  // row bottom is below the viewport top
        top.ShouldBeLessThan(column.Viewport.Height);      // row top is above the viewport bottom
    }

    [AvaloniaFact]
    public void TimePicker_scrolls_columns_to_selected_time_on_open()
    {
        var picker = new Loam.Controls.TimePicker { Time = new TimeSpan(22, 55, 0) };
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        paper.GetVisualDescendants().OfType<ScrollViewer>().Count().ShouldBe(2);

        var hourColumn = TimeColumn(paper, "Hour");
        var minuteColumn = TimeColumn(paper, "Minute");
        hourColumn.Offset.Y.ShouldBeGreaterThan(0);   // hour 22 scrolled down
        minuteColumn.Offset.Y.ShouldBeGreaterThan(0); // minute 55 scrolled down

        // ...and the selected rows are actually visible in the viewport, not just "some offset applied".
        ShouldBeWithinViewport(hourColumn, PopupRow(paper, "Hour 22"));
        ShouldBeWithinViewport(minuteColumn, PopupRow(paper, "Minute 55"));
    }

    [AvaloniaFact]
    public void TimePicker_centers_mid_list_selection_in_viewport()
    {
        var picker = new Loam.Controls.TimePicker { Time = new TimeSpan(12, 30, 0) };
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        // Mid-list values are not clamped, so the centering math lands them at the viewport center.
        ShouldBeCenteredIn(TimeColumn(paper, "Hour"), PopupRow(paper, "Hour 12"));
        ShouldBeCenteredIn(TimeColumn(paper, "Minute"), PopupRow(paper, "Minute 30"));
    }

    [AvaloniaFact]
    public void TimePicker_does_not_scroll_when_selection_at_top()
    {
        var picker = new Loam.Controls.TimePicker { Time = new TimeSpan(0, 0, 0) };
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        TimeColumn(paper, "Hour").Offset.Y.ShouldBe(0);   // hour 0 already at the top
        TimeColumn(paper, "Minute").Offset.Y.ShouldBe(0); // minute 0 already at the top
    }

    [AvaloniaFact]
    public void TimePicker_defaults_to_top_when_time_is_null()
    {
        var picker = new Loam.Controls.TimePicker(); // Time is null -> Open() falls back to 00:00
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        TimeColumn(paper, "Hour").Offset.Y.ShouldBe(0);
        TimeColumn(paper, "Minute").Offset.Y.ShouldBe(0);
    }

    [AvaloniaFact]
    public void TimePicker_scrolls_to_closest_stepped_minute()
    {
        // 18:50 with a 15-minute step rounds down to minute 45; that row must be the one brought into view.
        var picker = new Loam.Controls.TimePicker { Time = new TimeSpan(18, 50, 0), MinuteStep = 15 };
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var minuteColumn = TimeColumn(paper, "Minute");
        minuteColumn.Offset.Y.ShouldBeGreaterThan(0);
        ShouldBeWithinViewport(minuteColumn, PopupRow(paper, "Minute 45"));
    }

    [AvaloniaFact]
    public void TimePicker_keyboard_focus_brings_row_into_view()
    {
        var picker = new Loam.Controls.TimePicker { Time = new TimeSpan(0, 0, 0) };
        Show(picker);
        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var hourColumn = TimeColumn(paper, "Hour");
        hourColumn.Offset.Y.ShouldBe(0); // starts at the top (hour 0)

        PopupRow(paper, "Hour 20").Focus();
        Dispatcher.UIThread.RunJobs();
        hourColumn.Offset.Y.ShouldBeGreaterThan(0);                     // focusing a far row scrolls
        ShouldOverlapViewport(hourColumn, PopupRow(paper, "Hour 20")); // ...and it is now in view
    }

    [Fact]
    public void TimePicker_TryParseTime_handles_empty_exact_loose_and_invalid()
    {
        Loam.Controls.TimePicker.TryParseTime("  ", "HH:mm", out var empty).ShouldBeTrue();
        empty.ShouldBeNull();

        Loam.Controls.TimePicker.TryParseTime("21:45", "HH:mm", out var exact).ShouldBeTrue();
        exact.ShouldBe(new TimeSpan(21, 45, 0));

        // Not the exact HH:mm format, but parseable loosely by the (invariant) culture.
        Loam.Controls.TimePicker.TryParseTime("9:05 AM", "HH:mm", out var loose).ShouldBeTrue();
        loose.ShouldBe(new TimeSpan(9, 5, 0));

        Loam.Controls.TimePicker.TryParseTime("not a time", "HH:mm", out var bad).ShouldBeFalse();
        bad.ShouldBeNull();

        // A bare number parses as DAYS via TimeSpan and a 25h span exceeds a day — both must be rejected,
        // not silently committed as a corrupt time-of-day.
        Loam.Controls.TimePicker.TryParseTime("5", "HH:mm", out var bareNumber).ShouldBeFalse();
        bareNumber.ShouldBeNull();
        Loam.Controls.TimePicker.TryParseTime("25:00", "HH:mm", out var overflow).ShouldBeFalse();
        overflow.ShouldBeNull();
    }

    [AvaloniaFact]
    public void TimePicker_editable_rejects_bare_number_as_invalid()
    {
        var picker = new Loam.Controls.TimePicker { Editable = true, TimeFormat = "HH:mm", Time = new TimeSpan(8, 0, 0) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "5"; // would be 5 days via TimeSpan.Parse — must not be accepted
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Error.ShouldBeTrue();
        picker.Time.ShouldBe(new TimeSpan(8, 0, 0)); // unchanged, not corrupted to 5 days
    }

    [AvaloniaFact]
    public void TimePicker_non_editable_hides_text_input()
    {
        var picker = new Loam.Controls.TimePicker();
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        EditableInput(picker).IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void TimePicker_editable_parses_typed_time_on_enter()
    {
        TimeSpan? captured = null;
        var raised = false;
        var picker = new Loam.Controls.TimePicker { Editable = true, TimeFormat = "HH:mm" };
        picker.TimeSelected += t => { raised = true; captured = t; };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.IsVisible.ShouldBeTrue();
        input.Text = "21:45";
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Time.ShouldBe(new TimeSpan(21, 45, 0));
        picker.Error.ShouldBeFalse();
        raised.ShouldBeTrue();
        captured.ShouldBe(new TimeSpan(21, 45, 0));
    }

    [AvaloniaFact]
    public void TimePicker_editable_marks_error_on_invalid_text()
    {
        var picker = new Loam.Controls.TimePicker { Editable = true, TimeFormat = "HH:mm", Time = new TimeSpan(8, 0, 0) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "not a time";
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Error.ShouldBeTrue();
        picker.ErrorText.ShouldBe(picker.InvalidTimeText);
        picker.Time.ShouldBe(new TimeSpan(8, 0, 0)); // unchanged
    }

    [AvaloniaFact]
    public void TimePicker_editable_empty_text_clears_and_syncs_from_time()
    {
        var picker = new Loam.Controls.TimePicker { Editable = true, TimeFormat = "HH:mm", Time = new TimeSpan(9, 30, 0) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text.ShouldBe("09:30"); // committed value flows into the text box

        input.Text = string.Empty;
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Time.ShouldBeNull();
        picker.Error.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void TimePicker_editable_loose_parse_reformats_to_time_format()
    {
        var picker = new Loam.Controls.TimePicker { Editable = true, TimeFormat = "HH:mm" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "9:05 AM"; // not exact HH:mm -> loose parse
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Time.ShouldBe(new TimeSpan(9, 5, 0));
        input.Text.ShouldBe("09:05"); // committed text normalized to TimeFormat
    }

    [AvaloniaFact]
    public void TimePicker_editable_clock_button_opens_flyout_and_ok_syncs_input()
    {
        var picker = new Loam.Controls.TimePicker { Editable = true, TimeFormat = "HH:mm", Time = new TimeSpan(14, 0, 0) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldBeNull();
        picker.GetVisualDescendants().OfType<IconButton>().First(b => AutomationProperties.GetName(b) == "Open clock")
            .RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        PopupButton(paper, "OK").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Time.ShouldBe(new TimeSpan(14, 0, 0));
        EditableInput(picker).Text.ShouldBe("14:00"); // flyout commit flows back into the text box
    }

    [AvaloniaFact]
    public void TimePicker_editable_alt_down_opens_flyout()
    {
        var picker = new Loam.Controls.TimePicker { Editable = true };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Focus();
        Dispatcher.UIThread.RunJobs();
        MaybeOpenedFlyout(picker).ShouldBeNull();

        input.RaiseEvent(KeyArgs(Key.Down, KeyModifiers.Alt));
        Dispatcher.UIThread.RunJobs();
        MaybeOpenedFlyout(picker).ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void TimePicker_editable_floats_label_on_focus()
    {
        var picker = new Loam.Controls.TimePicker { Editable = true, Label = "When" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        LabelHost(picker).IsVisible.ShouldBeFalse(); // empty + unfocused -> resting
        EditableInput(picker).Focus();
        Dispatcher.UIThread.RunJobs();
        LabelHost(picker).IsVisible.ShouldBeTrue();  // focusing the text box floats the label
    }

    private static Icon LeadingAdornment(Control picker) =>
        picker.GetVisualDescendants().OfType<Icon>().First(i => i.Name == "PART_Adornment");

    private static Border LabelHost(Control picker) =>
        picker.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_LabelHost");

    private static Text DisplayText(Control picker) =>
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Display");

    // Left offset of the value-text region (PART_Display's parent layer) inside the field box;
    // it shifts right by the icon footprint when a leading adornment is present.
    private static double TextLayerLeft(Control picker)
    {
        var layer = DisplayText(picker).GetVisualParent();
        layer.ShouldNotBeNull();
        return layer!.Bounds.X;
    }

    [AvaloniaFact]
    public void DatePicker_adornment_icon_shows_only_when_set()
    {
        var without = new Loam.Controls.DatePicker();
        Show(without);
        LeadingAdornment(without).IsVisible.ShouldBeFalse();

        var with = new Loam.Controls.DatePicker { AdornmentIcon = Icons.Material.Filled.Schedule };
        Show(with);
        var icon = LeadingAdornment(with);
        icon.IsVisible.ShouldBeTrue();
        icon.Data.ShouldBe(Icons.Material.Filled.Schedule);
    }

    [AvaloniaFact]
    public void Adornment_icon_indents_value_and_label_across_variants()
    {
        var variants = new[] { Variant.Outlined, Variant.Filled, Variant.Text };
        foreach (var variant in variants)
        {
            var plain = new Loam.Controls.DatePicker { Width = 320, Variant = variant, Label = "Date", ShrinkLabel = true };
            Show(plain);
            Dispatcher.UIThread.RunJobs();

            var adorned = new Loam.Controls.DatePicker { Width = 320, Variant = variant, Label = "Date", ShrinkLabel = true, AdornmentIcon = Icons.Material.Filled.Schedule };
            Show(adorned);
            Dispatcher.UIThread.RunJobs();

            // Leading Small icon (20px) + IconSpacing (8px) = 28px: the floating label AND the value text both indent.
            (LabelHost(adorned).Margin.Left - LabelHost(plain).Margin.Left).ShouldBe(28, 0.01);
            (TextLayerLeft(adorned) - TextLayerLeft(plain)).ShouldBe(28, 0.5);
        }
    }

    [AvaloniaFact]
    public void Adornment_icon_set_and_unset_at_runtime()
    {
        var picker = new Loam.Controls.DatePicker { Label = "Date", ShrinkLabel = true };
        Show(picker);
        Dispatcher.UIThread.RunJobs();
        var icon = LeadingAdornment(picker);
        icon.IsVisible.ShouldBeFalse();
        var baseLeft = LabelHost(picker).Margin.Left;

        picker.AdornmentIcon = Icons.Material.Filled.Person; // a glyph the gallery also uses
        Dispatcher.UIThread.RunJobs();
        icon.IsVisible.ShouldBeTrue();
        icon.Data.ShouldBe(Icons.Material.Filled.Person);
        (LabelHost(picker).Margin.Left - baseLeft).ShouldBe(28, 0.01);

        picker.AdornmentIcon = null; // unsetting reverts to the zero-space layout
        Dispatcher.UIThread.RunJobs();
        icon.IsVisible.ShouldBeFalse();
        LabelHost(picker).Margin.Left.ShouldBe(baseLeft, 0.01);
    }

    [AvaloniaFact]
    public void Adornment_icon_and_clear_button_coexist()
    {
        var picker = new Loam.Controls.DatePicker
        {
            Clearable = true,
            AdornmentIcon = Icons.Material.Filled.Person,
            Date = new DateTime(2026, 6, 14),
        };
        Show(picker);
        Dispatcher.UIThread.RunJobs();

        LeadingAdornment(picker).IsVisible.ShouldBeTrue();
        var clear = ClearButton(picker, "Clear date");
        clear.IsVisible.ShouldBeTrue();

        clear.RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Date.ShouldBeNull();
        clear.IsVisible.ShouldBeFalse();                   // trailing clear hides once the value is gone
        LeadingAdornment(picker).IsVisible.ShouldBeTrue(); // leading adornment persists
    }

    [AvaloniaFact]
    public void TimePicker_and_DateRangePicker_show_adornment_icon_when_set()
    {
        var time = new Loam.Controls.TimePicker { AdornmentIcon = Icons.Material.Filled.CalendarToday };
        Show(time);
        var timeIcon = LeadingAdornment(time);
        timeIcon.IsVisible.ShouldBeTrue();
        timeIcon.Data.ShouldBe(Icons.Material.Filled.CalendarToday);

        var range = new DateRangePicker { AdornmentIcon = Icons.Material.Filled.Schedule };
        Show(range);
        LeadingAdornment(range).IsVisible.ShouldBeTrue();
    }

    private static TextBox EditableInput(Control picker) =>
        picker.GetVisualDescendants().OfType<TextBox>().First(t => t.Name == "PART_Input");

    [Fact]
    public void DatePicker_TryParseDate_handles_empty_exact_loose_and_invalid()
    {
        Loam.Controls.DatePicker.TryParseDate("   ", "yyyy-MM-dd", out var empty).ShouldBeTrue();
        empty.ShouldBeNull();

        Loam.Controls.DatePicker.TryParseDate("2026-07-04", "yyyy-MM-dd", out var exact).ShouldBeTrue();
        exact.ShouldBe(new DateTime(2026, 7, 4));

        // Exact format does not match, but the current culture parses it loosely.
        Loam.Controls.DatePicker.TryParseDate("2026-07-04", "MMM d, yyyy", out var loose).ShouldBeTrue();
        loose.ShouldBe(new DateTime(2026, 7, 4));

        Loam.Controls.DatePicker.TryParseDate("not a date", "yyyy-MM-dd", out var bad).ShouldBeFalse();
        bad.ShouldBeNull();
    }

    [AvaloniaFact]
    public void DatePicker_non_editable_hides_text_input()
    {
        var picker = new Loam.Controls.DatePicker();
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        EditableInput(picker).IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void DatePicker_editable_parses_typed_date_on_enter()
    {
        DateTime? captured = null;
        var raised = false;
        var picker = new Loam.Controls.DatePicker { Editable = true, DateFormat = "yyyy-MM-dd" };
        picker.DateSelected += d => { raised = true; captured = d; };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.IsVisible.ShouldBeTrue();
        input.Text = "2026-07-04";
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Date.ShouldBe(new DateTime(2026, 7, 4));
        picker.Error.ShouldBeFalse();
        raised.ShouldBeTrue();
        captured.ShouldBe(new DateTime(2026, 7, 4));
    }

    [AvaloniaFact]
    public void DatePicker_editable_marks_error_on_invalid_text()
    {
        var picker = new Loam.Controls.DatePicker { Editable = true, DateFormat = "yyyy-MM-dd", Date = new DateTime(2026, 1, 1) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "garbage";
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Error.ShouldBeTrue();
        picker.ErrorText.ShouldBe(picker.InvalidDateText);
        picker.Date.ShouldBe(new DateTime(2026, 1, 1)); // unchanged
    }

    [AvaloniaFact]
    public void DatePicker_editable_rejects_out_of_range_text()
    {
        var picker = new Loam.Controls.DatePicker
        {
            Editable = true,
            DateFormat = "yyyy-MM-dd",
            MinDate = new DateTime(2026, 6, 1),
            MaxDate = new DateTime(2026, 6, 30),
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "2026-12-25";
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Error.ShouldBeTrue();
        picker.ErrorText.ShouldBe(picker.InvalidDateText);
        picker.Date.ShouldBeNull(); // out of range, not committed
    }

    [AvaloniaFact]
    public void DatePicker_editable_empty_text_clears_and_syncs_from_date()
    {
        var picker = new Loam.Controls.DatePicker { Editable = true, DateFormat = "yyyy-MM-dd", Date = new DateTime(2026, 6, 14) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text.ShouldBe("2026-06-14"); // committed value flows into the text box

        input.Text = string.Empty;
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Date.ShouldBeNull();
        picker.Error.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void DatePicker_editable_calendar_button_opens_flyout()
    {
        var picker = new Loam.Controls.DatePicker { Editable = true };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldBeNull();
        var calendar = picker.GetVisualDescendants().OfType<IconButton>()
            .First(b => AutomationProperties.GetName(b) == "Open calendar");
        calendar.RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void DatePicker_editable_loose_parse_reformats_to_date_format()
    {
        var picker = new Loam.Controls.DatePicker { Editable = true, DateFormat = "MMMM d, yyyy" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Text = "2026-07-04"; // ISO form, not the exact DateFormat -> loose parse
        input.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Date.ShouldBe(new DateTime(2026, 7, 4));
        input.Text.ShouldBe("July 4, 2026"); // committed text is normalized to DateFormat
    }

    [AvaloniaFact]
    public void DatePicker_editable_calendar_selection_updates_input()
    {
        var picker = new Loam.Controls.DatePicker { Editable = true, DateFormat = "yyyy-MM-dd", Date = new DateTime(2026, 6, 1) };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        picker.GetVisualDescendants().OfType<IconButton>().First(b => AutomationProperties.GetName(b) == "Open calendar")
            .RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var calendar = paper.GetVisualDescendants().OfType<MonthCalendar>().Single();
        CalendarDay(calendar, new DateTime(2026, 6, 18)).RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();
        PopupButton(paper, "OK").RaiseEvent(new RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Date.ShouldBe(new DateTime(2026, 6, 18));
        EditableInput(picker).Text.ShouldBe("2026-06-18"); // flyout selection flows back into the text box
    }

    [AvaloniaFact]
    public void DatePicker_editable_alt_down_opens_flyout()
    {
        var picker = new Loam.Controls.DatePicker { Editable = true };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var input = EditableInput(picker);
        input.Focus();
        Dispatcher.UIThread.RunJobs();
        MaybeOpenedFlyout(picker).ShouldBeNull();

        input.RaiseEvent(KeyArgs(Key.Down, KeyModifiers.Alt));
        Dispatcher.UIThread.RunJobs();
        MaybeOpenedFlyout(picker).ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void DatePicker_editable_floats_label_on_focus()
    {
        var picker = new Loam.Controls.DatePicker { Editable = true, Label = "When" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        LabelHost(picker).IsVisible.ShouldBeFalse(); // empty + unfocused -> resting
        EditableInput(picker).Focus();
        Dispatcher.UIThread.RunJobs();
        LabelHost(picker).IsVisible.ShouldBeTrue();  // focusing the text box floats the label
    }

    [AvaloniaFact]
    public void DatePicker_non_editable_activation_still_opens_flyout()
    {
        var picker = new Loam.Controls.DatePicker();
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldBeNull();
        picker.RaiseEvent(KeyArgs(Key.Space)); // non-editable keyboard activation is unchanged
        Dispatcher.UIThread.RunJobs();
        MaybeOpenedFlyout(picker).ShouldNotBeNull();
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

        var dayCells = calendar.GetVisualDescendants().OfType<Border>()
            .Where(b => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(b)))
            .ToList();
        dayCells.Count.ShouldBe(28);

        calendar.DisplayMonth = new DateTime(2026, 1, 1); // January → 31 days
        Dispatcher.UIThread.RunJobs();
        calendar.GetVisualDescendants().OfType<Border>()
            .Count(b => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(b)))
            .ShouldBe(31);
    }

    [AvaloniaFact]
    public void MonthCalendar_uses_stable_picker_grid_metrics()
    {
        var calendar = new MonthCalendar { DisplayMonth = new DateTime(2026, 6, 1) };
        Show(calendar);
        Dispatcher.UIThread.RunJobs();

        var root = calendar.Child.ShouldBeOfType<StackPanel>();
        root.Width.ShouldBe(336);
        root.Spacing.ShouldBe(0);
        root.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Center);

        var header = root.Children[0].ShouldBeOfType<Avalonia.Controls.Grid>();
        header.Width.ShouldBe(336);
        header.Height.ShouldBe(56);

        var weekdays = root.Children[1].ShouldBeOfType<UniformGrid>();
        weekdays.Columns.ShouldBe(7);
        weekdays.Width.ShouldBe(336);
        weekdays.Height.ShouldBe(48);

        var days = root.Children[2].ShouldBeOfType<UniformGrid>();
        days.Columns.ShouldBe(7);
        days.Rows.ShouldBe(6);
        days.Width.ShouldBe(336);
        days.Height.ShouldBe(288);
        days.Children.Count.ShouldBe(42);

        var day = CalendarDay(calendar, new DateTime(2026, 6, 4));
        day.Width.ShouldBe(40);
        day.Height.ShouldBe(40);
        day.Margin.ShouldBe(new Avalonia.Thickness(4));
        day.CornerRadius.ShouldBe(new Avalonia.CornerRadius(20));
        day.ClipToBounds.ShouldBeTrue();
        StateLayer(day).CornerRadius.ShouldBe(new Avalonia.CornerRadius(20));
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
    public void MonthCalendar_supports_first_day_of_week_and_public_navigation()
    {
        var calendar = new MonthCalendar
        {
            DisplayMonth = new DateTime(2026, 6, 1),
            FirstDayOfWeek = DayOfWeek.Monday,
        };
        Show(calendar);
        Dispatcher.UIThread.RunJobs();

        var headings = calendar.GetVisualDescendants().OfType<Text>()
            .Where(text => text.Typo == Typo.BodyMedium && text.Opacity == 0.6)
            .Select(text => text.Text)
            .Take(7)
            .ToArray();
        headings[0].ShouldBe("Mo");
        headings[6].ShouldBe("Su");
        var headerActions = calendar.GetVisualDescendants().OfType<IconButton>()
            .Select(AutomationProperties.GetName)
            .ToArray();
        headerActions.ShouldContain("Previous month");
        headerActions.ShouldContain("Next month");

        calendar.NextMonth();
        Dispatcher.UIThread.RunJobs();
        calendar.DisplayMonth.ShouldBe(new DateTime(2026, 7, 1));

        calendar.PreviousMonth();
        Dispatcher.UIThread.RunJobs();
        calendar.DisplayMonth.ShouldBe(new DateTime(2026, 6, 1));
    }

    [AvaloniaFact]
    public void MonthCalendar_day_cells_are_focusable_named_and_keyboard_selectable()
    {
        var picked = default(DateTime);
        var calendar = new MonthCalendar { DisplayMonth = new DateTime(2026, 6, 1) };
        calendar.DateSelected += date => picked = date;
        Show(calendar);
        Dispatcher.UIThread.RunJobs();

        var day = CalendarDay(calendar, new DateTime(2026, 6, 4));
        day.Focusable.ShouldBeTrue();
        day.Width.ShouldBe(40);
        day.Height.ShouldBe(40);
        StateLayer(day).Background.ShouldBe(Brushes.Transparent);

        day.Focus();
        Dispatcher.UIThread.RunJobs();
        StateLayer(day).Background.ShouldNotBe(Brushes.Transparent);

        var enter = KeyArgs(Key.Enter);
        day.RaiseEvent(enter);

        enter.Handled.ShouldBeTrue();
        picked.ShouldBe(new DateTime(2026, 6, 4));
    }

    [AvaloniaFact]
    public void MonthCalendar_day_cells_support_arrow_navigation()
    {
        var calendar = new MonthCalendar { DisplayMonth = new DateTime(2026, 6, 1) };
        Show(calendar);
        Dispatcher.UIThread.RunJobs();

        var day = CalendarDay(calendar, new DateTime(2026, 6, 4));
        day.Focus();
        Dispatcher.UIThread.RunJobs();

        var right = KeyArgs(Key.Right);
        day.RaiseEvent(right);
        Dispatcher.UIThread.RunJobs();

        right.Handled.ShouldBeTrue();
        var nextDay = CalendarDay(calendar, new DateTime(2026, 6, 5));
        nextDay.IsFocused.ShouldBeTrue();

        var down = KeyArgs(Key.Down);
        nextDay.RaiseEvent(down);
        Dispatcher.UIThread.RunJobs();

        down.Handled.ShouldBeTrue();
        CalendarDay(calendar, new DateTime(2026, 6, 12)).IsFocused.ShouldBeTrue();

        var pageDown = KeyArgs(Key.PageDown);
        CalendarDay(calendar, new DateTime(2026, 6, 12)).RaiseEvent(pageDown);
        Dispatcher.UIThread.RunJobs();

        pageDown.Handled.ShouldBeTrue();
        calendar.DisplayMonth.ShouldBe(new DateTime(2026, 7, 1));
        CalendarDay(calendar, new DateTime(2026, 7, 12)).IsFocused.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void MonthCalendar_disabled_day_cells_are_not_focusable_or_selectable()
    {
        var picked = default(DateTime?);
        var calendar = new MonthCalendar
        {
            DisplayMonth = new DateTime(2026, 6, 1),
            MinDate = new DateTime(2026, 6, 3),
        };
        calendar.DateSelected += date => picked = date;
        Show(calendar);
        Dispatcher.UIThread.RunJobs();

        var disabled = CalendarDay(calendar, new DateTime(2026, 6, 2));
        disabled.Focusable.ShouldBeFalse();
        disabled.Opacity.ShouldBe(0.35);
        AutomationProperties.GetHelpText(disabled).ShouldBe("Unavailable date");

        var enter = KeyArgs(Key.Enter);
        disabled.RaiseEvent(enter);

        enter.Handled.ShouldBeFalse();
        picked.ShouldBeNull();
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
    public void DatePicker_renders_label_helper_error_and_automation()
    {
        var picker = new Loam.Controls.DatePicker
        {
            Label = "Start date",
            HelperText = "Choose within six months",
            ErrorText = "Choose a later date",
            Date = new DateTime(2026, 6, 1),
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(picker).ShouldNotBeNullOrWhiteSpace();
        AutomationProperties.GetName(picker)!.ShouldContain("Start date");
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Label").Text.ShouldBe("Start date");
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_HelperText").Text.ShouldBe("Choose within six months");

        picker.Error = true;
        Dispatcher.UIThread.RunJobs();

        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_HelperText").Text.ShouldBe("Choose a later date");
    }

    [AvaloniaFact]
    public void DateRangePicker_renders_label_helper_error_and_automation()
    {
        var picker = new DateRangePicker
        {
            Label = "Range",
            HelperText = "Pick start and end",
            ErrorText = "Choose an end date",
            Start = new DateTime(2026, 6, 1),
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(picker).ShouldNotBeNullOrWhiteSpace();
        AutomationProperties.GetName(picker)!.ShouldContain("Range");
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Label").Text.ShouldBe("Range");
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_HelperText").Text.ShouldBe("Pick start and end");

        picker.Error = true;
        Dispatcher.UIThread.RunJobs();

        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_HelperText").Text.ShouldBe("Choose an end date");
    }

    [AvaloniaFact]
    public void TimePicker_renders_label_helper_error_and_automation()
    {
        var picker = new Loam.Controls.TimePicker
        {
            Label = "Start time",
            HelperText = "Choose a reminder time",
            ErrorText = "Choose a valid time",
            Time = new TimeSpan(9, 30, 0),
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(picker).ShouldNotBeNullOrWhiteSpace();
        AutomationProperties.GetName(picker)!.ShouldContain("Start time");
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Label").Text.ShouldBe("Start time");
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_HelperText").Text.ShouldBe("Choose a reminder time");

        picker.Error = true;
        Dispatcher.UIThread.RunJobs();

        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_HelperText").Text.ShouldBe("Choose a valid time");
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
    public void ColorPicker_renders_label_helper_error_and_automation()
    {
        var picker = new ColorPicker
        {
            Label = "Accent",
            HelperText = "Default palette",
            ErrorText = "Choose a visible color",
            Value = Color.Parse("#6750A4"),
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(picker).ShouldBe("Accent");
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Label").Text.ShouldBe("Accent");
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_HelperText").Text.ShouldBe("Default palette");

        picker.Error = true;
        Dispatcher.UIThread.RunJobs();

        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_HelperText").Text.ShouldBe("Choose a visible color");
    }

    [AvaloniaFact]
    public void ColorPicker_uses_custom_palette_when_provided()
    {
        var picker = new ColorPicker
        {
            Palette =
            {
                Color.Parse("#6750A4"),
                Color.Parse("#2E7D32"),
                Color.Parse("#B3261E"),
            },
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var swatches = paper.GetVisualDescendants().OfType<Border>()
            .Where(border => border.Width == 28 && border.Height == 28)
            .ToArray();
        swatches.Length.ShouldBe(3);
        ((ISolidColorBrush)swatches[0].Background!).Color.ShouldBe(Color.Parse("#6750A4"));
    }

    [AvaloniaFact]
    public void ColorPicker_palette_swatches_are_focusable_named_and_keyboard_selectable()
    {
        var picker = new ColorPicker
        {
            Palette =
            {
                Color.Parse("#6750A4"),
                Color.Parse("#2E7D32"),
                Color.Parse("#B3261E"),
            },
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var swatches = paper.GetVisualDescendants().OfType<Border>()
            .Where(border => border.Name == "PART_PaletteSwatch")
            .ToArray();
        swatches.Length.ShouldBe(3);
        swatches.All(swatch => swatch.Focusable).ShouldBeTrue();
        AutomationProperties.GetName(swatches[1]).ShouldBe("Color #2E7D32");
        AutomationProperties.GetHelpText(swatches[1]).ShouldBe("Select color");

        swatches[1].Focus();
        Dispatcher.UIThread.RunJobs();
        swatches[1].GetVisualDescendants().OfType<Border>()
            .First(border => border.Name == "PART_PaletteStateLayer")
            .Background.ShouldNotBe(Brushes.Transparent);

        var activate = KeyArgs(Key.Enter);
        swatches[1].RaiseEvent(activate);
        Dispatcher.UIThread.RunJobs();

        activate.Handled.ShouldBeTrue();
        picker.Value.ShouldBe(Color.Parse("#2E7D32"));
    }

    [AvaloniaFact]
    public void ColorPicker_disabled_suppresses_opening()
    {
        var picker = new ColorPicker { IsEnabled = false };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        Box(picker).IsEnabled.ShouldBeFalse();

        var open = KeyArgs(Key.Space);
        picker.RaiseEvent(open);
        Dispatcher.UIThread.RunJobs();

        open.Handled.ShouldBeFalse();
        MaybeOpenedFlyout(picker).ShouldBeNull();

        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldBeNull();
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

    [AvaloniaFact]
    public void Picker_labels_rest_or_float_with_shared_field_chrome()
    {
        var picker = new Loam.Controls.DatePicker { Label = "Start date", Placeholder = "Pick a date" };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        picker.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_LabelHost").IsVisible.ShouldBeFalse();
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_RestingLabel").IsVisible.ShouldBeTrue();
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Display").IsVisible.ShouldBeFalse();

        picker.Date = new DateTime(2026, 6, 4);
        Dispatcher.UIThread.RunJobs();

        var labelHost = picker.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_LabelHost");
        labelHost.IsVisible.ShouldBeTrue();
        labelHost.Margin.Top.ShouldBe(0);
        Box(picker).Margin.Top.ShouldBe(LoamFieldMetrics.Default.FloatingLabelTopMargin);
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_RestingLabel").IsVisible.ShouldBeFalse();
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Display").IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Date_and_time_pickers_use_stable_field_metrics()
    {
        Control[] pickers =
        [
            new Loam.Controls.DatePicker { Label = "Start date" },
            new DateRangePicker { Label = "Date range" },
            new Loam.Controls.TimePicker { Label = "Start time" },
        ];

        foreach (var picker in pickers)
        {
            Show(picker);
            picker.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var box = Box(picker);
            box.MinHeight.ShouldBeGreaterThanOrEqualTo(56);
            box.MinWidth.ShouldBeGreaterThanOrEqualTo(
                picker is Loam.Controls.TimePicker ? 240 : PopupSurface.PickerWidth);
            box.BorderThickness.ShouldBe(new Avalonia.Thickness(1));
            box.CornerRadius.ShouldNotBe(default);
            box.Padding.Left.ShouldBeGreaterThanOrEqualTo(16);
            box.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Stretch);

            // All three field pickers expose their calendar/clock affordance as an IconButton
            // (clickable to open the flyout, required for Editable mode).
            var button = box.GetVisualDescendants().OfType<IconButton>()
                .First(b => AutomationProperties.GetName(b) is "Open calendar" or "Open clock");
            button.Size.ShouldBe(LoamSize.Small);
        }
    }

    [AvaloniaFact]
    public void DatePicker_variant_applies_field_chrome()
    {
        var outlined = new Loam.Controls.DatePicker { Label = "Outlined", Variant = Variant.Outlined };
        var filled = new Loam.Controls.DatePicker { Label = "Filled", Variant = Variant.Filled };
        var text = new Loam.Controls.DatePicker { Label = "Text", Variant = Variant.Text };

        Show(new StackPanel { Children = { outlined, filled, text } });
        outlined.ApplyTemplate();
        filled.ApplyTemplate();
        text.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var outlinedBox = Box(outlined);
        outlinedBox.BorderThickness.ShouldBe(new Avalonia.Thickness(1));
        outlinedBox.CornerRadius.ShouldNotBe(default);

        var filledBox = Box(filled);
        filledBox.BorderThickness.ShouldBe(new Avalonia.Thickness(0, 0, 0, 1));
        filledBox.CornerRadius.BottomLeft.ShouldBe(0);
        filledBox.CornerRadius.BottomRight.ShouldBe(0);
        filledBox.Background.ShouldNotBeNull();

        var textBox = Box(text);
        textBox.BorderThickness.ShouldBe(new Avalonia.Thickness(0, 0, 0, 1));
        textBox.CornerRadius.ShouldBe(default);
        textBox.Background.ShouldBe(Brushes.Transparent);
    }

    [AvaloniaFact]
    public void DateRangePicker_variant_applies_field_chrome()
    {
        var outlined = new DateRangePicker { Label = "Outlined", Variant = Variant.Outlined };
        var filled = new DateRangePicker { Label = "Filled", Variant = Variant.Filled };
        var text = new DateRangePicker { Label = "Text", Variant = Variant.Text };

        Show(new StackPanel { Children = { outlined, filled, text } });
        outlined.ApplyTemplate();
        filled.ApplyTemplate();
        text.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var outlinedBox = Box(outlined);
        outlinedBox.BorderThickness.ShouldBe(new Avalonia.Thickness(1));
        outlinedBox.CornerRadius.ShouldNotBe(default);

        var filledBox = Box(filled);
        filledBox.BorderThickness.ShouldBe(new Avalonia.Thickness(0, 0, 0, 1));
        filledBox.CornerRadius.BottomLeft.ShouldBe(0);
        filledBox.CornerRadius.BottomRight.ShouldBe(0);
        filledBox.Background.ShouldNotBeNull();

        var textBox = Box(text);
        textBox.BorderThickness.ShouldBe(new Avalonia.Thickness(0, 0, 0, 1));
        textBox.CornerRadius.ShouldBe(default);
        textBox.Background.ShouldBe(Brushes.Transparent);
    }

    [AvaloniaFact]
    public void TimePicker_variant_applies_field_chrome()
    {
        var outlined = new Loam.Controls.TimePicker { Label = "Outlined", Variant = Variant.Outlined };
        var filled = new Loam.Controls.TimePicker { Label = "Filled", Variant = Variant.Filled };
        var text = new Loam.Controls.TimePicker { Label = "Text", Variant = Variant.Text };

        Show(new StackPanel { Children = { outlined, filled, text } });
        outlined.ApplyTemplate();
        filled.ApplyTemplate();
        text.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var outlinedBox = Box(outlined);
        outlinedBox.BorderThickness.ShouldBe(new Avalonia.Thickness(1));
        outlinedBox.CornerRadius.ShouldNotBe(default);

        var filledBox = Box(filled);
        filledBox.BorderThickness.ShouldBe(new Avalonia.Thickness(0, 0, 0, 1));
        filledBox.CornerRadius.BottomLeft.ShouldBe(0);
        filledBox.CornerRadius.BottomRight.ShouldBe(0);
        filledBox.Background.ShouldNotBeNull();

        var textBox = Box(text);
        textBox.BorderThickness.ShouldBe(new Avalonia.Thickness(0, 0, 0, 1));
        textBox.CornerRadius.ShouldBe(default);
        textBox.Background.ShouldBe(Brushes.Transparent);
    }

    [AvaloniaFact]
    public void ColorPicker_variant_applies_field_chrome()
    {
        var outlined = new ColorPicker { Label = "Outlined", Variant = Variant.Outlined };
        var filled = new ColorPicker { Label = "Filled", Variant = Variant.Filled };
        var text = new ColorPicker { Label = "Text", Variant = Variant.Text };

        Show(new StackPanel { Children = { outlined, filled, text } });
        outlined.ApplyTemplate();
        filled.ApplyTemplate();
        text.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var outlinedBox = Box(outlined);
        outlinedBox.BorderThickness.ShouldBe(new Avalonia.Thickness(1));
        outlinedBox.CornerRadius.ShouldNotBe(default);

        var filledBox = Box(filled);
        filledBox.BorderThickness.ShouldBe(new Avalonia.Thickness(0, 0, 0, 1));
        filledBox.CornerRadius.BottomLeft.ShouldBe(0);
        filledBox.CornerRadius.BottomRight.ShouldBe(0);
        filledBox.Background.ShouldNotBeNull();

        var textBox = Box(text);
        textBox.BorderThickness.ShouldBe(new Avalonia.Thickness(0, 0, 0, 1));
        textBox.CornerRadius.ShouldBe(default);
        textBox.Background.ShouldBe(Brushes.Transparent);
    }

    [AvaloniaFact]
    public void Date_and_time_picker_flyouts_use_shared_picker_surface()
    {
        Control[] pickers =
        [
            new Loam.Controls.DatePicker { Label = "Start date" },
            new DateRangePicker { Label = "Date range" },
            new Loam.Controls.TimePicker { Label = "Start time" },
        ];

        foreach (var picker in pickers)
        {
            Show(picker);
            picker.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var open = KeyArgs(Key.Space);
            picker.RaiseEvent(open);
            Dispatcher.UIThread.RunJobs();

            open.Handled.ShouldBeTrue();
            var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
            paper.Width.ShouldBe(PopupSurface.PickerWidth);
            paper.MinWidth.ShouldBe(PopupSurface.PickerWidth);
            paper.MaxWidth.ShouldBe(PopupSurface.PickerWidth);
            paper.Padding.ShouldBe(PopupSurface.PickerPadding);
            paper.ClipToBounds.ShouldBeTrue();

            var content = paper.Content.ShouldBeOfType<StackPanel>();
            if (picker is Loam.Controls.DatePicker)
            {
                content.Children.Count.ShouldBe(5);
                content.Children[0].ShouldBeOfType<Text>().Typo.ShouldBe(Typo.TitleSmall);
                content.Children[1].ShouldBeOfType<Border>();
                content.Children[2].ShouldBeOfType<Border>().Height.ShouldBe(1);
                content.Children[3].ShouldBeOfType<ContentControl>().Content.ShouldBeOfType<MonthCalendar>();
                content.Children[4].ShouldBeOfType<StackPanel>();
                paper.GetVisualDescendants().OfType<TextField>().ShouldBeEmpty();
            }
            else
            {
                content.Children[0].ShouldBeOfType<Text>().Typo.ShouldBe(Typo.TitleMedium);
            }
            content.Children.Count.ShouldBeGreaterThanOrEqualTo(2);
        }
    }

    [AvaloniaFact]
    public void DatePicker_uses_pending_selection_with_cancel_and_ok_actions()
    {
        var committed = default(DateTime?);
        var picker = new Loam.Controls.DatePicker
        {
            Label = "Start date",
            Date = new DateTime(2026, 6, 3),
        };
        picker.DateSelected += date => committed = date;
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        picker.RaiseEvent(KeyArgs(Key.Space));
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var calendar = paper.GetVisualDescendants().OfType<MonthCalendar>().Single();
        var day = CalendarDay(calendar, new DateTime(2026, 6, 4));

        day.RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Date.ShouldBe(new DateTime(2026, 6, 3));
        paper.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ShouldContain("Thu, Jun 4");

        var ok = paper.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "OK");
        ok.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Date.ShouldBe(new DateTime(2026, 6, 4));
        committed.ShouldBe(new DateTime(2026, 6, 4));

        picker.RaiseEvent(KeyArgs(Key.Space));
        Dispatcher.UIThread.RunJobs();
        paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        calendar = paper.GetVisualDescendants().OfType<MonthCalendar>().Single();
        CalendarDay(calendar, new DateTime(2026, 6, 5)).RaiseEvent(KeyArgs(Key.Enter));

        var cancel = paper.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Cancel");
        cancel.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Date.ShouldBe(new DateTime(2026, 6, 4));
    }

    [AvaloniaFact]
    public void DatePicker_disabled_suppresses_opening()
    {
        var picker = new Loam.Controls.DatePicker
        {
            Label = "Start date",
            Date = new DateTime(2026, 6, 4),
            IsEnabled = false,
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        Box(picker).IsEnabled.ShouldBeFalse();

        var open = KeyArgs(Key.Space);
        picker.RaiseEvent(open);
        Dispatcher.UIThread.RunJobs();

        open.Handled.ShouldBeFalse();
        MaybeOpenedFlyout(picker).ShouldBeNull();

        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldBeNull();
    }

    [AvaloniaFact]
    public void DateRangePicker_uses_pending_selection_with_cancel_and_ok_actions()
    {
        var committed = false;
        var picker = new DateRangePicker
        {
            Label = "Range",
            Start = new DateTime(2026, 6, 3),
            End = new DateTime(2026, 6, 10),
            OkText = "Apply",
            CancelText = "Dismiss",
        };
        picker.RangeSelected += (_, _) => committed = true;
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        picker.RaiseEvent(KeyArgs(Key.Space));
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var calendar = paper.GetVisualDescendants().OfType<MonthCalendar>().Single();
        CalendarDay(calendar, new DateTime(2026, 6, 4)).RaiseEvent(KeyArgs(Key.Enter));
        CalendarDay(calendar, new DateTime(2026, 6, 8)).RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Start.ShouldBe(new DateTime(2026, 6, 3));
        picker.End.ShouldBe(new DateTime(2026, 6, 10));
        paper.GetVisualDescendants().OfType<Text>().Select(text => text.Text)
            .ShouldContain(DateRangePicker.Format(new DateTime(2026, 6, 4), new DateTime(2026, 6, 8), "d"));

        var ok = paper.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Apply");
        ok.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        committed.ShouldBeTrue();
        picker.Start.ShouldBe(new DateTime(2026, 6, 4));
        picker.End.ShouldBe(new DateTime(2026, 6, 8));

        picker.RaiseEvent(KeyArgs(Key.Space));
        Dispatcher.UIThread.RunJobs();
        paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        calendar = paper.GetVisualDescendants().OfType<MonthCalendar>().Single();
        CalendarDay(calendar, new DateTime(2026, 6, 11)).RaiseEvent(KeyArgs(Key.Enter));

        var cancel = paper.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Dismiss");
        cancel.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Start.ShouldBe(new DateTime(2026, 6, 4));
        picker.End.ShouldBe(new DateTime(2026, 6, 8));
    }

    [AvaloniaFact]
    public void DateRangePicker_disabled_suppresses_opening()
    {
        var picker = new DateRangePicker
        {
            Label = "Range",
            Start = new DateTime(2026, 6, 1),
            End = new DateTime(2026, 6, 10),
            IsEnabled = false,
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        Box(picker).IsEnabled.ShouldBeFalse();

        var open = KeyArgs(Key.Space);
        picker.RaiseEvent(open);
        Dispatcher.UIThread.RunJobs();

        open.Handled.ShouldBeFalse();
        MaybeOpenedFlyout(picker).ShouldBeNull();

        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldBeNull();
    }

    [AvaloniaFact]
    public void TimePicker_rows_are_focusable_state_layered_and_keyboard_selectable()
    {
        var committed = default(TimeSpan?);
        var picker = new Loam.Controls.TimePicker
        {
            Label = "Start time",
            Time = TimeSpan.Zero,
            MinuteStep = 15,
        };
        picker.TimeSelected += time => committed = time;
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var open = KeyArgs(Key.Space);
        picker.RaiseEvent(open);
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        var scrollers = paper.GetVisualDescendants().OfType<ScrollViewer>().ToArray();
        scrollers.Length.ShouldBe(2);
        foreach (var scroller in scrollers)
        {
            scroller.HorizontalScrollBarVisibility.ShouldBe(ScrollBarVisibility.Disabled);
            scroller.VerticalScrollBarVisibility.ShouldBe(ScrollBarVisibility.Hidden); // scrollable, no visible scrollbar
        }

        var hour = PopupRow(paper, "Hour 09");
        var hourStateLayer = StateLayer(hour);
        hour.Focusable.ShouldBeTrue();
        hour.MinHeight.ShouldBe(48);
        hour.ClipToBounds.ShouldBeTrue();
        hourStateLayer.CornerRadius.ShouldBe(new Avalonia.CornerRadius(24));
        hourStateLayer.Background.ShouldBe(Brushes.Transparent);

        hour.Focus();
        Dispatcher.UIThread.RunJobs();
        hourStateLayer.Background.ShouldNotBe(Brushes.Transparent);

        var hourEnter = KeyArgs(Key.Enter);
        hour.RaiseEvent(hourEnter);
        hourEnter.Handled.ShouldBeTrue();
        picker.Time.ShouldBe(TimeSpan.Zero);

        var minute = PopupRow(paper, "Minute 30");
        var minuteEnter = KeyArgs(Key.Enter);
        minute.RaiseEvent(minuteEnter);

        minuteEnter.Handled.ShouldBeTrue();
        picker.Time.ShouldBe(TimeSpan.Zero);

        var ok = paper.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "OK");
        ok.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Time.ShouldBe(new TimeSpan(9, 30, 0));
        committed.ShouldBe(new TimeSpan(9, 30, 0));
    }

    [AvaloniaFact]
    public void TimePicker_uses_pending_selection_with_cancel_and_ok_actions()
    {
        var picker = new Loam.Controls.TimePicker
        {
            Label = "Start time",
            Time = new TimeSpan(8, 15, 0),
            MinuteStep = 15,
            CancelText = "Dismiss",
            OkText = "Apply",
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        picker.RaiseEvent(KeyArgs(Key.Space));
        Dispatcher.UIThread.RunJobs();

        var paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        PopupRow(paper, "Hour 10").RaiseEvent(KeyArgs(Key.Enter));
        PopupRow(paper, "Minute 45").RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        picker.Time.ShouldBe(new TimeSpan(8, 15, 0));

        var cancel = paper.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Dismiss");
        cancel.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Time.ShouldBe(new TimeSpan(8, 15, 0));

        picker.RaiseEvent(KeyArgs(Key.Space));
        Dispatcher.UIThread.RunJobs();
        paper = OpenedFlyout(picker).Content.ShouldBeOfType<Paper>();
        PopupRow(paper, "Hour 10").RaiseEvent(KeyArgs(Key.Enter));
        PopupRow(paper, "Minute 45").RaiseEvent(KeyArgs(Key.Enter));

        var ok = paper.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Apply");
        ok.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        picker.Time.ShouldBe(new TimeSpan(10, 45, 0));
    }

    [AvaloniaFact]
    public void TimePicker_disabled_suppresses_opening()
    {
        var picker = new Loam.Controls.TimePicker
        {
            Label = "Start time",
            Time = new TimeSpan(8, 15, 0),
            IsEnabled = false,
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        Box(picker).IsEnabled.ShouldBeFalse();

        var open = KeyArgs(Key.Space);
        picker.RaiseEvent(open);
        Dispatcher.UIThread.RunJobs();

        open.Handled.ShouldBeFalse();
        MaybeOpenedFlyout(picker).ShouldBeNull();

        picker.OpenPicker();
        Dispatcher.UIThread.RunJobs();

        MaybeOpenedFlyout(picker).ShouldBeNull();
    }

    [AvaloniaFact]
    public void Picker_error_state_uses_shared_field_chrome()
    {
        var picker = new DateRangePicker
        {
            Label = "Range",
            Error = true,
            ErrorText = "Required",
            HelperText = "Optional",
        };
        Show(picker);
        picker.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        Box(picker).BorderThickness.ShouldBe(new Avalonia.Thickness(2));
        picker.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_HelperText").Text
            .ShouldBe("Required");
    }
}
