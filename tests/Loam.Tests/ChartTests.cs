using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam.Controls;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class ChartTests
{
    [Fact]
    public void Charts_slice_sweeps_sum_to_360_and_bar_heights_scale_to_max()
    {
        var values = new[] { 1d, 1d, 2d };
        var expectedSweeps = new[] { 90d, 90d, 180d };
        Charts.SliceSweeps(values).ShouldBe(expectedSweeps);

        var zeros = new[] { 0d, 0d };
        Charts.SliceSweeps(zeros).ShouldBeEmpty();

        var bars = new[] { 5d, 10d };
        var expectedHeights = new[] { 50d, 100d };
        Charts.BarHeights(bars, 100).ShouldBe(expectedHeights);
    }

    [Fact]
    public void Charts_math_clamps_negative_values()
    {
        Charts.SliceSweeps([-5d, 5d]).ShouldBe([0d, 360d]);
        Charts.BarHeights([-5d, 5d], 100).ShouldBe([0d, 100d]);

        var points = Charts.LinePoints([-5d, 5d], 100, 100);

        points.Count.ShouldBe(2);
        points[0].ShouldBe(new Avalonia.Point(0, 96));
        points[1].ShouldBe(new Avalonia.Point(100, 4));
    }

    [AvaloniaFact]
    public void Charts_render_without_throwing()
    {
        var data = new[] { 3d, 2d, 1d, 4d };
        var pie = new PieChart { Width = 200, Height = 200, Values = data, Donut = true };
        var bar = new BarChart { Width = 300, Height = 160, Values = data };
        var line = new LineChart { Width = 300, Height = 160, Values = data, Area = true };

        new Window { Width = 700, Height = 500, Content = new StackPanel { Children = { pie, bar, line } } }.Show();
        Dispatcher.UIThread.RunJobs();

        pie.Bounds.Width.ShouldBeGreaterThan(0);
        bar.Bounds.Height.ShouldBeGreaterThan(0);
        line.Bounds.Width.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void Chart_default_series_colors_follow_theme_roles()
    {
        var light = new PieChart { Values = [1d, 2d, 3d] };
        var lightWindow = Show(light, ThemeVariant.Light);
        try
        {
            light.ResolvedSeriesColors[0].ShouldBe(LoamColorScheme.DefaultLight.Primary);
            light.ResolvedSeriesColors[1].ShouldBe(LoamColorScheme.DefaultLight.Secondary);
            light.ResolvedSeriesColors[2].ShouldBe(LoamColorScheme.DefaultLight.Tertiary);
        }
        finally
        {
            lightWindow.Close();
        }

        var dark = new BarChart { Values = [1d, 2d, 3d] };
        var darkWindow = Show(dark, ThemeVariant.Dark);
        try
        {
            dark.ResolvedSeriesColors[0].ShouldBe(LoamColorScheme.DefaultDark.Primary);
            dark.ResolvedSeriesColors[1].ShouldBe(LoamColorScheme.DefaultDark.Secondary);
            dark.ResolvedSeriesColors[2].ShouldBe(LoamColorScheme.DefaultDark.Tertiary);
        }
        finally
        {
            darkWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Explicit_chart_colors_override_theme_series()
    {
        var colors = new[] { Colors.Black, Colors.White, Colors.Red };
        var chart = new LineChart { Values = [1d, 2d, 3d], Colors = colors, Area = true };
        var window = Show(chart, ThemeVariant.Light);
        try
        {
            chart.ResolvedSeriesColors.ShouldBe(colors);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ChartLegend_renders_theme_and_explicit_color_rows()
    {
        var legend = new ChartLegend { Labels = { "Planning", "Build", "Review" } };
        var window = Show(legend, ThemeVariant.Light);
        try
        {
            var swatches = legend.GetVisualDescendants().OfType<Border>()
                .Where(border => border.Width == 12 && border.Height == 12)
                .ToList();

            swatches.Count.ShouldBe(3);
            ((ISolidColorBrush)swatches[0].Background!).Color.ShouldBe(LoamColorScheme.DefaultLight.Primary);
            ((ISolidColorBrush)swatches[1].Background!).Color.ShouldBe(LoamColorScheme.DefaultLight.Secondary);
            AutomationProperties.GetName(legend).ShouldBe("Chart legend");
            AutomationProperties.GetHelpText(legend).ShouldBe("3 items");
        }
        finally
        {
            window.Close();
        }

        var colors = new[] { Colors.Red, Colors.Blue };
        var explicitLegend = new ChartLegend { Colors = colors, Labels = { "A", "B", "C" } };
        var explicitWindow = Show(explicitLegend, ThemeVariant.Dark);
        try
        {
            var explicitSwatches = explicitLegend.GetVisualDescendants().OfType<Border>()
                .Where(border => border.Width == 12 && border.Height == 12)
                .ToList();

            explicitSwatches.Count.ShouldBe(3);
            ((ISolidColorBrush)explicitSwatches[0].Background!).Color.ShouldBe(Colors.Red);
            ((ISolidColorBrush)explicitSwatches[2].Background!).Color.ShouldBe(Colors.Red);
        }
        finally
        {
            explicitWindow.Close();
        }

        var emptyLegend = new ChartLegend { ShowSwatches = false, Labels = { "No data" } };
        var emptyWindow = Show(emptyLegend, ThemeVariant.Light);
        try
        {
            emptyLegend.GetVisualDescendants().OfType<Border>()
                .Where(border => border.Width == 12 && border.Height == 12)
                .ShouldBeEmpty();
            AutomationProperties.GetHelpText(emptyLegend).ShouldBe("1 item");
        }
        finally
        {
            emptyWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Empty_and_zero_only_charts_render_named_no_data_state()
    {
        var pie = new PieChart { Width = 180, Height = 180 };
        var bar = new BarChart { Width = 220, Height = 140, Values = [0d, -2d] };
        var line = new LineChart { Width = 220, Height = 140, Values = [-4d, 0d], Area = true };

        var window = Show(new StackPanel { Children = { pie, bar, line } }, ThemeVariant.Light);
        try
        {
            pie.HasPositiveData.ShouldBeFalse();
            bar.HasPositiveData.ShouldBeFalse();
            line.HasPositiveData.ShouldBeFalse();
            AutomationProperties.GetName(pie).ShouldBe("Pie chart");
            AutomationProperties.GetName(bar).ShouldBe("Bar chart");
            AutomationProperties.GetName(line).ShouldBe("Line chart");
            AutomationProperties.GetHelpText(pie).ShouldBe("No data");
            AutomationProperties.GetHelpText(bar).ShouldBe("No data");
            AutomationProperties.GetHelpText(line).ShouldBe("No data");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Chart_value_and_mode_changes_refresh_accessible_state()
    {
        var pie = new PieChart { Values = [1d] };
        var line = new LineChart { Values = [1d, 2d] };
        var window = Show(new StackPanel { Children = { pie, line } }, ThemeVariant.Light);
        try
        {
            AutomationProperties.GetHelpText(pie).ShouldBe("1 value");
            pie.Donut = true;
            pie.HoleRatio = 2;
            pie.HoleRatio.ShouldBe(0.95);

            line.Area = true;
            line.Values = [1d, 2d, 0d, -4d, 3d];
            Dispatcher.UIThread.RunJobs();

            AutomationProperties.GetHelpText(line).ShouldBe("3 values");
            line.HasPositiveData.ShouldBeTrue();

            line.Values = [];
            Dispatcher.UIThread.RunJobs();
            AutomationProperties.GetHelpText(line).ShouldBe("No data");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Charts_signed_domain_and_bar_layout_handle_negatives()
    {
        var values = new[] { 10d, -5d, 0d };
        Charts.SignedDomain(values).ShouldBe((-5d, 10d));

        // span = 15, plotHeight = 150 -> zero baseline sits 100px below the top.
        Charts.ZeroBaselineOffset(-5, 10, 150).ShouldBe(100d);

        var layout = Charts.SignedBarLayout(values, -5, 10, 150);
        layout.Count.ShouldBe(3);
        layout[0].ShouldBe((0d, 100d));    // +10: full bar up from the baseline
        layout[1].ShouldBe((100d, 50d));   // -5: bar drops below the baseline
        layout[2].ShouldBe((100d, 0d));    // 0: no bar, sits on the baseline
    }

    [Fact]
    public void Charts_signed_domain_with_all_positive_data_keeps_a_zero_floor()
    {
        Charts.SignedDomain([3d, 8d, 5d]).ShouldBe((0d, 8d));
        Charts.ZeroBaselineOffset(0, 8, 120).ShouldBe(120d); // baseline at the bottom
    }

    [Fact]
    public void Charts_scaled_line_points_map_a_signed_domain()
    {
        var points = Charts.ScaledLinePoints([-10d, 0d, 10d], 100, 100, -10, 10, 0);

        points.Count.ShouldBe(3);
        points[0].ShouldBe(new Avalonia.Point(0, 100));  // -10 at the bottom
        points[1].ShouldBe(new Avalonia.Point(50, 50));  // 0 in the middle
        points[2].ShouldBe(new Avalonia.Point(100, 0));  // +10 at the top
    }

    [AvaloniaFact]
    public void Chart_snapshot_projects_value_label_percent_and_color()
    {
        var labels = new[] { "Alpha", "Beta", "Gamma" };
        var pie = new PieChart { Values = [30d, 10d, 0d], Labels = labels };
        var window = Show(pie, ThemeVariant.Light);
        try
        {
            var points = pie.ResolvedPoints;
            points.Count.ShouldBe(3);

            points[0].Index.ShouldBe(0);
            points[0].Value.ShouldBe(30d);
            points[0].Label.ShouldBe("Alpha");
            points[0].Percent.ShouldBe(0.75d); // 30 of the 40 positive total
            points[1].Percent.ShouldBe(0.25d);
            points[2].Percent.ShouldBe(0d);    // non-positive contributes no share

            points[0].Color.ShouldBe(LoamColorScheme.DefaultLight.Primary);
            points[1].Color.ShouldBe(LoamColorScheme.DefaultLight.Secondary);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Chart_help_text_lists_labels_of_positive_points_when_provided()
    {
        var labels = new[] { "Web", "Idle", "Mobile" };
        var bar = new BarChart { Values = [5d, 0d, 3d], Labels = labels };
        var window = Show(bar, ThemeVariant.Light);
        try
        {
            AutomationProperties.GetHelpText(bar).ShouldBe("2 values: Web, Mobile");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Donut_center_text_and_signed_charts_render_without_throwing()
    {
        var donut = new PieChart
        {
            Width = 180, Height = 180, Values = [540d, 320d, 380d], Donut = true,
            CenterText = "1,240", CenterSubText = "total",
        };
        var donutValue = new PieChart
        {
            Width = 160, Height = 160, Values = [10d, 20d], Donut = true, CenterValueFormat = "C0",
        };
        var bar = new BarChart { Width = 300, Height = 160, Values = [12d, -5d, 8d, -3d], AllowNegative = true };
        var line = new LineChart { Width = 300, Height = 160, Values = [4d, -2d, 6d, -1d], AllowNegative = true, Area = true };

        new Window
        {
            Width = 800,
            Height = 640,
            Content = new StackPanel { Children = { donut, donutValue, bar, line } },
        }.Show();
        Dispatcher.UIThread.RunJobs();

        donut.Bounds.Width.ShouldBeGreaterThan(0);
        bar.HasSignedData.ShouldBeTrue();
        line.HasSignedData.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Allow_negative_treats_negative_only_data_as_renderable()
    {
        var withFlag = new BarChart { Width = 200, Height = 140, Values = [-4d, -2d], AllowNegative = true };
        var withoutFlag = new BarChart { Width = 200, Height = 140, Values = [-4d, -2d] };
        var window = Show(new StackPanel { Children = { withFlag, withoutFlag } }, ThemeVariant.Light);
        try
        {
            // Without the opt-in, negatives are clamped and the chart stays in the empty state.
            withoutFlag.HasPositiveData.ShouldBeFalse();
            AutomationProperties.GetHelpText(withoutFlag).ShouldBe("No data");

            // With the opt-in, the same data is signed/renderable.
            withFlag.HasSignedData.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Data_labels_render_without_throwing()
    {
        var labels = new[] { "A", "B", "C" };
        var bars = new BarChart { Width = 300, Height = 160, Values = [5d, 8d, 3d], Labels = labels, ShowDataLabels = true };
        var signed = new BarChart { Width = 300, Height = 160, Values = [6d, -4d, 9d], AllowNegative = true, ShowDataLabels = true };
        var pie = new PieChart { Width = 200, Height = 200, Donut = true, Values = [40d, 35d, 25d], ShowDataLabels = true, DataLabelFormat = p => $"{p.Percent:P0}" };
        var line = new LineChart { Width = 300, Height = 160, Values = [2d, 9d, 4d], ShowDataLabels = true };

        new Window
        {
            Width = 800,
            Height = 760,
            Content = new StackPanel { Children = { bars, signed, pie, line } },
        }.Show();
        Dispatcher.UIThread.RunJobs();

        bars.ShowDataLabels.ShouldBeTrue();
        signed.HasSignedData.ShouldBeTrue();
        pie.Bounds.Width.ShouldBeGreaterThan(0);
        line.Bounds.Height.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Charts_nice_scale_rounds_to_clean_ticks()
    {
        var (min, max, step) = Charts.NiceScale(0, 65, 4);
        min.ShouldBe(0);
        max.ShouldBe(80);
        step.ShouldBe(20);

        Charts.NiceScale(65, 4).ShouldBe((0d, 80d, 20d)); // zero-based overload
    }

    [Fact]
    public void Charts_scaled_line_points_in_rect_map_over_domain()
    {
        var plot = new Avalonia.Rect(10, 10, 100, 100);
        var points = Charts.ScaledLinePoints(new[] { -10d, 0d, 10d }, plot, -10, 10);

        points.Count.ShouldBe(3);
        points[0].ShouldBe(new Avalonia.Point(10, 110));  // -10 at the bottom edge
        points[1].ShouldBe(new Avalonia.Point(60, 60));   // 0 at the middle
        points[2].ShouldBe(new Avalonia.Point(110, 10));  // +10 at the top edge
    }

    [AvaloniaFact]
    public void Axes_render_without_throwing()
    {
        var labels = new[] { "Q1", "Q2", "Q3", "Q4" };
        var bar = new BarChart { Width = 320, Height = 200, Values = [30d, 45, 28, 60], Labels = labels, ShowAxes = true, YAxisFormat = v => $"${v:N0}" };
        var line = new LineChart { Width = 320, Height = 200, Values = [30d, 45, 28, 60], Labels = labels, ShowAxes = true };

        var window = Show(new StackPanel { Children = { bar, line } }, ThemeVariant.Light);
        try
        {
            bar.ShowAxes.ShouldBeTrue();
            bar.Bounds.Width.ShouldBeGreaterThan(0);
            line.Bounds.Height.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Bar_chart_hover_and_click_hit_test_the_correct_bar()
    {
        var bar = new BarChart { Values = [10d, 20d, 30d] };
        var window = new Window { Width = 320, Height = 200, Content = bar };
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        ChartPointEventArgs? hovered = null;
        bar.HoverChanged += (_, e) => hovered = e;
        ChartPointEventArgs? clicked = null;
        bar.PointClicked += (_, e) => clicked = e;

        // The chart fills the window from the client origin; the middle bar sits under its center.
        var center = new Avalonia.Point(bar.Bounds.Width / 2, bar.Bounds.Height / 2);

        window.MouseMove(center);
        Dispatcher.UIThread.RunJobs();

        bar.HoveredIndex.ShouldBe(1);
        hovered.ShouldNotBeNull();
        hovered!.Index.ShouldBe(1);
        hovered.Point!.Value.Value.ShouldBe(20d);

        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        clicked.ShouldNotBeNull();
        clicked!.Index.ShouldBe(1);

        window.Close();
    }

    [AvaloniaFact]
    public void Moving_off_all_bars_clears_the_hovered_index()
    {
        var bar = new BarChart { Values = [10d, 20d, 30d] };
        var window = new Window { Width = 320, Height = 200, Content = bar };
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        window.MouseMove(new Avalonia.Point(bar.Bounds.Width / 2, bar.Bounds.Height / 2));
        Dispatcher.UIThread.RunJobs();
        bar.HoveredIndex.ShouldBe(1);

        // The top-left padding area holds no bar.
        window.MouseMove(new Avalonia.Point(2, 2));
        Dispatcher.UIThread.RunJobs();
        bar.HoveredIndex.ShouldBe(-1);

        window.Close();
    }

    private static Window Show(Control content, ThemeVariant theme)
    {
        Avalonia.Application.Current!.RequestedThemeVariant = theme;
        var window = new Window
        {
            Width = 700,
            Height = 500,
            Content = content,
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        content.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }
}
