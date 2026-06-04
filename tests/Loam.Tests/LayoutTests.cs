using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Loam;
using Loam.Controls;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class LayoutTests
{
    private static void Show(Control content)
    {
        new Window { Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Grid_places_two_half_items_on_one_row_at_md()
    {
        var a = new Item { Md = 6, Child = new Border { Height = 20 } };
        var b = new Item { Md = 6, Child = new Border { Height = 20 } };
        var grid = new Loam.Controls.Grid { Width = 1000, Children = { a, b } };
        Show(grid);

        // colUnit = 1000/12; itemWidth = 6*colUnit - 8 = 492; b.X = 6*colUnit = 500.
        a.Bounds.Width.ShouldBe(492d, 1d);
        b.Bounds.Width.ShouldBe(492d, 1d);
        a.Bounds.Y.ShouldBe(0d, 0.5);
        b.Bounds.Y.ShouldBe(0d, 0.5);
        b.Bounds.X.ShouldBe(500d, 1d);
    }

    [AvaloniaFact]
    public void Grid_stacks_items_full_width_at_xs()
    {
        var a = new Item { Xs = 12, Md = 6, Child = new Border { Height = 20 } };
        var b = new Item { Xs = 12, Md = 6, Child = new Border { Height = 20 } };
        var grid = new Loam.Controls.Grid { Width = 400, Children = { a, b } };
        Show(grid);

        a.Bounds.Width.ShouldBe(392d, 1d); // 12/12*400 - 8
        a.Bounds.Y.ShouldBe(0d, 0.5);
        b.Bounds.Y.ShouldBeGreaterThan(0d); // wrapped to second row
    }

    [AvaloniaFact]
    public void Container_caps_width_and_centers()
    {
        var child = new Border { Height = 50 };
        var container = new Container { Width = 2000, MaxWidthBreakpoint = Breakpoint.Md, Child = child };
        Show(container);

        child.Bounds.Width.ShouldBe(928d, 1d); // min(2000, 960) - 2*16 gutters
        child.Bounds.X.ShouldBe(536d, 1d); // (2000-960)/2 + 16
    }

    [Fact]
    public void Stack_row_toggles_orientation_and_defaults()
    {
        new Stack().Orientation.ShouldBe(Orientation.Vertical);
        new Stack().Spacing.ShouldBe(8d);
        new Stack { Row = true }.Orientation.ShouldBe(Orientation.Horizontal);
    }

    [Fact]
    public void Breakpoint_from_width_thresholds()
    {
        Breakpoints.FromWidth(500).ShouldBe(Breakpoint.Xs);
        Breakpoints.FromWidth(700).ShouldBe(Breakpoint.Sm);
        Breakpoints.FromWidth(1000).ShouldBe(Breakpoint.Md);
        Breakpoints.FromWidth(1400).ShouldBe(Breakpoint.Lg);
        Breakpoints.FromWidth(2000).ShouldBe(Breakpoint.Xl);
    }

    [AvaloniaFact]
    public void Collapse_clips_height_when_collapsed()
    {
        var collapse = new Collapse { Child = new Border { Height = 40, Width = 100 } };
        Show(collapse);

        collapse.Animated.ShouldBeTrue();
        collapse.Duration.ShouldBe(TimeSpan.FromMilliseconds(180));
        collapse.MaxHeight.ShouldBe(0);

        collapse.Expanded = true;
        Dispatcher.UIThread.RunJobs();
        collapse.Expanded.ShouldBeTrue();

        collapse.Expanded = false;
        Dispatcher.UIThread.RunJobs();
        collapse.Expanded.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Collapse_static_mode_uses_immediate_final_states()
    {
        var collapse = new Collapse { Animated = false, Child = new Border { Height = 40, Width = 100 } };
        Show(collapse);

        collapse.MaxHeight.ShouldBe(0);

        collapse.Expanded = true;
        Dispatcher.UIThread.RunJobs();
        collapse.MaxHeight.ShouldBe(double.PositiveInfinity);
    }

    [AvaloniaFact]
    public void Spacer_fills_remaining_space_in_a_dockpanel()
    {
        var left = new Border { Width = 60, Height = 20 };
        var spacer = new Spacer();
        DockPanel.SetDock(left, Dock.Left);
        var dock = new DockPanel { Width = 400, Height = 40, LastChildFill = true, Children = { left, spacer } };
        Show(dock);
        Dispatcher.UIThread.RunJobs();

        spacer.Bounds.Width.ShouldBe(340d, 1d);
    }

    [AvaloniaFact]
    public void ListSubheader_is_muted_secondary_text()
    {
        Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
        var subheader = new ListSubheader { Text = "Section" };
        Show(subheader);

        subheader.Typo.ShouldBe(Typo.Caption);
        ((Avalonia.Media.ISolidColorBrush)subheader.Foreground!).Color.A.ShouldBe((byte)0x8A);
    }

    [Fact]
    public void Ripple_maxreach_is_distance_to_farthest_corner()
    {
        Ripple.MaxReach(new Avalonia.Point(0, 0), new Avalonia.Size(3, 4)).ShouldBe(5.0, 0.001);
        Ripple.MaxReach(new Avalonia.Point(50, 50), new Avalonia.Size(100, 100)).ShouldBe(Math.Sqrt(5000), 0.001);
    }

    [Fact]
    public void Hidden_rule_evaluates_per_mode()
    {
        Hidden.IsHiddenAt(Breakpoint.Xs, Breakpoint.Sm, HiddenMode.Down).ShouldBeTrue();
        Hidden.IsHiddenAt(Breakpoint.Lg, Breakpoint.Sm, HiddenMode.Down).ShouldBeFalse();
        Hidden.IsHiddenAt(Breakpoint.Lg, Breakpoint.Md, HiddenMode.Up).ShouldBeTrue();
        Hidden.IsHiddenAt(Breakpoint.Md, Breakpoint.Md, HiddenMode.Only).ShouldBeTrue();
        Hidden.IsHiddenAt(Breakpoint.Lg, Breakpoint.Md, HiddenMode.Only).ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ScrollToTop_appears_after_scrolling_past_offset()
    {
        var scroll = new ScrollViewer { Width = 200, Height = 200, Content = new Border { Height = 2000 } };
        var scrollToTop = new ScrollToTop { Target = scroll, VisibleOffset = 100 };
        Show(new Panel { Children = { scroll, scrollToTop } });
        Dispatcher.UIThread.RunJobs();

        scrollToTop.IsVisible.ShouldBeFalse();

        scroll.Offset = new Avalonia.Vector(0, 300);
        Dispatcher.UIThread.RunJobs();
        scrollToTop.IsVisible.ShouldBeTrue();
    }
}
