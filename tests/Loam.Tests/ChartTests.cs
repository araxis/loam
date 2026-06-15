using System.Collections.ObjectModel;
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

    [AvaloniaFact]
    public void ItemsSource_projects_items_and_updates_live()
    {
        var data = new ObservableCollection<Order>
        {
            new("A", 10),
            new("B", 20),
        };
        var bar = new BarChart
        {
            ItemsSource = data,
            ValueSelector = o => ((Order)o).Total,
            LabelSelector = o => ((Order)o).Name,
        };
        var window = Show(bar, ThemeVariant.Light);
        try
        {
            bar.Values.Count.ShouldBe(2);
            bar.Values[0].ShouldBe(10);
            bar.ResolvedPoints[1].Label.ShouldBe("B");

            data.Add(new Order("C", 30));
            Dispatcher.UIThread.RunJobs();

            bar.Values.Count.ShouldBe(3);
            bar.ResolvedPoints[2].Value.ShouldBe(30);
            bar.ResolvedPoints[2].Label.ShouldBe("C");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Charts_stacked_bar_heights_stack_positive_values()
    {
        // values [3, 1], domain max 4, 100px tall -> series 0 sits on top of series 1.
        var values = new[] { 3d, 1d };
        var segments = Charts.StackedBarHeights(values, 4, 100);

        segments.Count.ShouldBe(2);
        segments[0].ShouldBe((25d, 75d)); // 3/4 of the height, from y=25
        segments[1].ShouldBe((0d, 25d));  // 1/4, stacked above
    }

    [AvaloniaFact]
    public void Multi_series_snapshot_carries_series_index_and_per_series_color()
    {
        var labels = new[] { "Q1", "Q2" };
        var web = new[] { 10d, 20d };
        var mobile = new[] { 5d, 8d };
        var bar = new BarChart
        {
            Labels = labels,
            Series = new[]
            {
                new ChartSeries(web, "Web"),
                new ChartSeries(mobile, "Mobile"),
            },
        };
        var window = Show(bar, ThemeVariant.Light);
        try
        {
            bar.ResolvedPoints.Count.ShouldBe(4); // 2 series x 2 categories, series-major

            bar.ResolvedPoints[0].SeriesIndex.ShouldBe(0);
            bar.ResolvedPoints[0].Value.ShouldBe(10);
            bar.ResolvedPoints[0].Label.ShouldBe("Q1");
            bar.ResolvedPoints[2].SeriesIndex.ShouldBe(1); // series 1, category 0
            bar.ResolvedPoints[2].Value.ShouldBe(5);

            bar.ResolvedPoints[0].Color.ShouldBe(LoamColorScheme.DefaultLight.Primary);
            bar.ResolvedPoints[2].Color.ShouldBe(LoamColorScheme.DefaultLight.Secondary);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Multi_series_charts_render_without_throwing()
    {
        var labels = new[] { "Q1", "Q2", "Q3" };
        var web = new[] { 10d, 20d, 15d };
        var mobile = new[] { 6d, 8d, 12d };
        var series = new[]
        {
            new ChartSeries(web, "Web"),
            new ChartSeries(mobile, "Mobile"),
        };
        var grouped = new BarChart { Width = 320, Height = 200, Labels = labels, Series = series, ShowAxes = true };
        var stacked = new BarChart { Width = 320, Height = 200, Labels = labels, Series = series, StackMode = BarStackMode.Stacked };
        var percent = new BarChart { Width = 320, Height = 200, Labels = labels, Series = series, StackMode = BarStackMode.StackedPercent };
        var lines = new LineChart { Width = 320, Height = 200, Labels = labels, Series = series, ShowAxes = true };

        new Window
        {
            Width = 820,
            Height = 940,
            Content = new StackPanel { Children = { grouped, stacked, percent, lines } },
        }.Show();
        Dispatcher.UIThread.RunJobs();

        grouped.ResolvedPoints.Count.ShouldBe(6);
        lines.ResolvedPoints.Count.ShouldBe(6);
    }

    [AvaloniaFact]
    public void Multi_series_grouped_bar_hover_resolves_the_datapoint()
    {
        var labels = new[] { "Q1", "Q2" };
        var web = new[] { 10d, 20d };
        var mobile = new[] { 30d, 8d }; // Mobile Q1 = 30 is the tallest bar
        var bar = new BarChart
        {
            Labels = labels,
            Series = new[]
            {
                new ChartSeries(web, "Web"),
                new ChartSeries(mobile, "Mobile"),
            },
        };
        var window = new Window { Width = 320, Height = 200, Content = bar };
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        ChartPointEventArgs? hovered = null;
        bar.HoverChanged += (_, e) => hovered = e;

        // The tall Mobile bar of category Q1 occupies the right half of the first category slot.
        window.MouseMove(new Avalonia.Point(120, 100));
        Dispatcher.UIThread.RunJobs();

        bar.HoveredIndex.ShouldBeGreaterThanOrEqualTo(0);
        hovered.ShouldNotBeNull();
        hovered!.Point!.Value.Value.ShouldBe(30);

        window.Close();
    }

    [AvaloniaFact]
    public void Bound_legend_derives_rows_from_chart_series()
    {
        var labels = new[] { "Q1", "Q2" };
        var web = new[] { 10d, 20d };
        var mobile = new[] { 5d, 8d };
        var bar = new BarChart
        {
            Labels = labels,
            Series = new[] { new ChartSeries(web, "Web"), new ChartSeries(mobile, "Mobile") },
        };
        var legend = new ChartLegend { Source = bar };

        var window = Show(new StackPanel { Children = { bar, legend } }, ThemeVariant.Light);
        try
        {
            var swatches = legend.GetVisualDescendants().OfType<Border>()
                .Where(b => b.Width == 12 && b.Height == 12)
                .ToList();

            swatches.Count.ShouldBe(2); // one row per series
            ((ISolidColorBrush)swatches[0].Background!).Color.ShouldBe(LoamColorScheme.DefaultLight.Primary);
            ((ISolidColorBrush)swatches[1].Background!).Color.ShouldBe(LoamColorScheme.DefaultLight.Secondary);
            AutomationProperties.GetHelpText(legend).ShouldBe("2 items");

            var rowTexts = legend.GetVisualDescendants().OfType<Text>().Select(t => t.Text).ToList();
            rowTexts.ShouldContain("Web");
            rowTexts.ShouldContain("Mobile");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Charts_gauge_fraction_clamps_to_range()
    {
        Charts.GaugeFraction(50, 0, 100).ShouldBe(0.5);
        Charts.GaugeFraction(-10, 0, 100).ShouldBe(0);  // below min clamps to 0
        Charts.GaugeFraction(150, 0, 100).ShouldBe(1);  // above max clamps to 1
        Charts.GaugeFraction(15, 10, 20).ShouldBe(0.5); // offset range
        Charts.GaugeFraction(5, 0, 0).ShouldBe(0);      // non-positive span
    }

    [AvaloniaFact]
    public void Radial_gauge_and_sparkline_render_without_throwing()
    {
        var gauge = new RadialGauge { Width = 160, Height = 160, Value = 64, Maximum = 100, Caption = "CPU" };
        var emptyGauge = new RadialGauge { Width = 160, Height = 160, Minimum = 5, Maximum = 5, Value = 5 }; // empty range
        var line = new Sparkline { Width = 120, Height = 28, Values = [3d, 7d, 4d, 9d, 6d] };
        var bars = new Sparkline { Width = 120, Height = 28, Mode = SparklineMode.Bar, Values = [3d, 7d, 4d, 9d, 6d] };
        var emptySpark = new Sparkline { Width = 120, Height = 28, Values = [] };

        new Window
        {
            Width = 400,
            Height = 500,
            Content = new StackPanel { Children = { gauge, emptyGauge, line, bars, emptySpark } },
        }.Show();
        Dispatcher.UIThread.RunJobs();

        gauge.Bounds.Width.ShouldBeGreaterThan(0);
        line.Bounds.Width.ShouldBeGreaterThan(0);
        bars.Bounds.Width.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void Radial_gauge_exposes_automation_name_and_help_text()
    {
        var gauge = new RadialGauge { Width = 160, Height = 160, Value = 42, Maximum = 50, Caption = "Load" };
        new Window { Width = 200, Height = 200, Content = gauge }.Show();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(gauge).ShouldBe("Gauge");
        var help = AutomationProperties.GetHelpText(gauge);
        help.ShouldNotBeNull();
        help!.ShouldContain("Load");
        help.ShouldContain("42");

        // An empty range announces the same "No data" the visual empty state shows.
        var empty = new RadialGauge { Width = 160, Height = 160, Minimum = 0, Maximum = 0 };
        new Window { Width = 200, Height = 200, Content = empty }.Show();
        Dispatcher.UIThread.RunJobs();
        AutomationProperties.GetHelpText(empty).ShouldBe("No data");
    }

    [AvaloniaFact]
    public void Radial_gauge_hover_and_click_hit_test_the_arc()
    {
        var gauge = new RadialGauge { Value = 70, Caption = "Speed" };
        var window = new Window { Width = 200, Height = 200, Content = gauge };
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        ChartPointEventArgs? hovered = null;
        gauge.HoverChanged += (_, e) => hovered = e;
        ChartPointEventArgs? clicked = null;
        gauge.PointClicked += (_, e) => clicked = e;

        var cx = gauge.Bounds.Width / 2;
        var cy = gauge.Bounds.Height / 2;
        var radius = Math.Min(gauge.Bounds.Width, gauge.Bounds.Height) / 2 - gauge.Thickness / 2 - 1;
        var arc = new Avalonia.Point(cx - radius, cy); // left-middle — inside the 135°..405° sweep band

        window.MouseMove(arc);
        Dispatcher.UIThread.RunJobs();
        gauge.HoveredIndex.ShouldBe(0); // the arc band hit-tests to the single datapoint
        hovered.ShouldNotBeNull();
        hovered!.Index.ShouldBe(0);
        hovered.Point.ShouldNotBeNull();
        hovered.Point!.Value.Value.ShouldBe(70d);     // BuildPoints carries Value...
        hovered.Point.Value.Label.ShouldBe("Speed");  // ...and Caption as the point label

        window.MouseDown(arc, MouseButton.Left);
        window.MouseUp(arc, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        clicked.ShouldNotBeNull();
        clicked!.Index.ShouldBe(0);

        window.MouseMove(new Avalonia.Point(cx, cy)); // center hole — off the arc band
        Dispatcher.UIThread.RunJobs();
        gauge.HoveredIndex.ShouldBe(-1);

        window.Close();
    }

    [AvaloniaFact]
    public void Sparkline_hover_hit_tests_a_point()
    {
        var spark = new Sparkline { Values = [10d, 20d, 30d] };
        var window = new Window { Width = 200, Height = 60, Content = spark };
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        ChartPointEventArgs? hovered = null;
        spark.HoverChanged += (_, e) => hovered = e;

        // Middle point (value 20 of max 30): x at the midpoint, y mapped by magnitude (matches Charts.LinePoints, pad = 2).
        var x = spark.Bounds.Width / 2;
        var y = 2 + (1 - 20d / 30d) * (spark.Bounds.Height - 4);
        window.MouseMove(new Avalonia.Point(x, y));
        Dispatcher.UIThread.RunJobs();

        spark.HoveredIndex.ShouldBe(1);
        hovered.ShouldNotBeNull();
        hovered!.Point!.Value.Value.ShouldBe(20d);

        window.Close();
    }

    [AvaloniaFact]
    public void Sparkline_disables_tooltip_by_default_unlike_other_charts()
    {
        new Sparkline().ShowTooltip.ShouldBeFalse();
        new RadialGauge().ShowTooltip.ShouldBeTrue();
    }

    [Fact]
    public void Charts_radar_points_map_axes_and_radius()
    {
        var center = new Avalonia.Point(100, 100);

        // 4 axes all at max -> on the circle at top / right / bottom / left.
        var allMax = new[] { 10d, 10d, 10d, 10d };
        var pts = Charts.RadarPoints(allMax, 10, center, 50);
        pts.Count.ShouldBe(4);
        pts[0].X.ShouldBe(100, 0.01); pts[0].Y.ShouldBe(50, 0.01);   // top   (-90°)
        pts[1].X.ShouldBe(150, 0.01); pts[1].Y.ShouldBe(100, 0.01);  // right (0°)
        pts[2].X.ShouldBe(100, 0.01); pts[2].Y.ShouldBe(150, 0.01);  // bottom (90°)
        pts[3].X.ShouldBe(50, 0.01); pts[3].Y.ShouldBe(100, 0.01);   // left  (180°)

        // Zero value sits at the center; values above max clamp to the outer ring.
        var mixedValues = new[] { 0d, 20d };
        var mixed = Charts.RadarPoints(mixedValues, 10, center, 50);
        mixed[0].ShouldBe(center);
        mixed[1].X.ShouldBe(100, 0.01); mixed[1].Y.ShouldBe(150, 0.01);

        var three = new[] { 1d, 2d, 3d };
        Charts.RadarPoints(System.Array.Empty<double>(), 10, center, 50).ShouldBeEmpty();
        Charts.RadarPoints(three, 0, center, 50).ShouldBeEmpty(); // non-positive max
    }

    [AvaloniaFact]
    public void Radar_chart_renders_without_throwing()
    {
        var single = new RadarChart
        {
            Width = 240, Height = 240,
            Labels = ["Speed", "Power", "Range", "Cost", "Comfort"],
            Values = [8d, 6d, 9d, 4d, 7d],
        };
        var multi = new RadarChart
        {
            Width = 240, Height = 240,
            Labels = ["A", "B", "C", "D"],
            Series = new[] { new ChartSeries([8d, 6d, 9d, 4d], "X"), new ChartSeries([5d, 7d, 3d, 8d], "Y") },
        };
        var tooFew = new RadarChart { Width = 240, Height = 240, Values = [1d, 2d] };   // < 3 axes -> no data
        var empty = new RadarChart { Width = 240, Height = 240, Values = [0d, 0d, 0d] }; // zero -> no data

        new Window
        {
            Width = 600,
            Height = 700,
            Content = new StackPanel { Children = { single, multi, tooFew, empty } },
        }.Show();
        Dispatcher.UIThread.RunJobs();

        single.Bounds.Width.ShouldBeGreaterThan(0);
        multi.Bounds.Width.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void Radar_chart_multi_series_snapshot_carries_series_index()
    {
        var radar = new RadarChart
        {
            Labels = ["A", "B", "C"],
            Series = new[] { new ChartSeries([8d, 6d, 9d], "X"), new ChartSeries([5d, 7d, 3d], "Y") },
        };
        var window = Show(radar, ThemeVariant.Light);
        try
        {
            radar.ResolvedPoints.Count.ShouldBe(6); // 2 series x 3 categories, series-major
            radar.ResolvedPoints[0].SeriesIndex.ShouldBe(0);
            radar.ResolvedPoints[0].Value.ShouldBe(8);
            radar.ResolvedPoints[0].Label.ShouldBe("A");
            radar.ResolvedPoints[3].SeriesIndex.ShouldBe(1); // series 1, category 0
            radar.ResolvedPoints[3].Value.ShouldBe(5);

            // Multi-series automation reports a real count from the snapshot, not "No data" (Values is empty here).
            AutomationProperties.GetHelpText(radar).ShouldNotBe("No data");

            // The legend derives one row per series, in order, with the series names.
            var legend = radar.GetLegendEntries();
            legend.Count.ShouldBe(2);
            legend[0].Label.ShouldBe("X");
            legend[1].Label.ShouldBe("Y");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Radar_chart_multi_series_hover_resolves_the_series_vertex()
    {
        // Series 0 peaks on axis 0 (top); series 1 peaks on axis 1 — so the outer vertex on axis 1 is series 1's.
        var radar = new RadarChart
        {
            Width = 260,
            Height = 260,
            Labels = ["A", "B", "C"],
            Series = new[] { new ChartSeries([10d, 1d, 1d], "X"), new ChartSeries([1d, 10d, 1d], "Y") },
        };
        var window = new Window { Width = 260, Height = 260, Content = radar };
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        ChartPointEventArgs? hovered = null;
        radar.HoverChanged += (_, e) => hovered = e;

        var cx = radar.Bounds.Width / 2;
        var cy = radar.Bounds.Height / 2;
        var maxRadius = Math.Min(radar.Bounds.Width, radar.Bounds.Height) / 2 - 24;
        var angle = (-90 + 1 * 360.0 / 3) * Math.PI / 180; // axis 1
        var vertex = new Avalonia.Point(cx + maxRadius * Math.Cos(angle), cy + maxRadius * Math.Sin(angle));

        window.MouseMove(vertex);
        Dispatcher.UIThread.RunJobs();

        radar.HoveredIndex.ShouldBe(4); // series-major flat index: series 1 (s=1) * 3 categories + category 1
        hovered.ShouldNotBeNull();
        hovered!.Point!.Value.SeriesIndex.ShouldBe(1);
        hovered.Point.Value.Value.ShouldBe(10d);

        window.Close();
    }

    [AvaloniaFact]
    public void Radar_chart_hover_and_click_hit_test_a_vertex()
    {
        var radar = new RadarChart { Width = 240, Height = 240, Labels = ["A", "B", "C"], Values = [10d, 5d, 5d] };
        var window = new Window { Width = 240, Height = 240, Content = radar };
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        ChartPointEventArgs? hovered = null;
        radar.HoverChanged += (_, e) => hovered = e;
        ChartPointEventArgs? clicked = null;
        radar.PointClicked += (_, e) => clicked = e;

        var cx = radar.Bounds.Width / 2;
        var cy = radar.Bounds.Height / 2;
        var maxRadius = Math.Min(radar.Bounds.Width, radar.Bounds.Height) / 2 - 24; // labels present -> 24px margin
        var top = new Avalonia.Point(cx, cy - maxRadius); // axis 0 (top) with value == max sits on the outer ring

        window.MouseMove(top);
        Dispatcher.UIThread.RunJobs();
        radar.HoveredIndex.ShouldBe(0);
        hovered.ShouldNotBeNull();
        hovered!.Point!.Value.Value.ShouldBe(10d);
        hovered.Point.Value.Label.ShouldBe("A");

        window.MouseDown(top, MouseButton.Left);
        window.MouseUp(top, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        clicked.ShouldNotBeNull();
        clicked!.Index.ShouldBe(0);

        window.Close();
    }

    [Fact]
    public void Charts_map_linear_extent_and_bubble_radius()
    {
        Charts.MapLinear(5, 0, 10, 0, 100).ShouldBe(50);
        Charts.MapLinear(0, 0, 10, 100, 0).ShouldBe(100); // inverted target range (axis flip)
        Charts.MapLinear(10, 0, 10, 100, 0).ShouldBe(0);
        Charts.MapLinear(7, 5, 5, 0, 100).ShouldBe(50);   // empty source span -> target midpoint

        var values = new[] { 3d, -2d, 8d, 1d };
        var ext = Charts.Extent(values);
        ext.Min.ShouldBe(-2);
        ext.Max.ShouldBe(8);
        Charts.Extent(System.Array.Empty<double>()).ShouldBe((0d, 0d));

        Charts.BubbleRadius(100, 100, 4, 24).ShouldBe(24);          // max magnitude -> max radius
        Charts.BubbleRadius(25, 100, 4, 24).ShouldBe(4 + 0.5 * 20); // sqrt(0.25) = 0.5 -> area-proportional
        Charts.BubbleRadius(0, 100, 4, 24).ShouldBe(4);             // zero -> min radius
        Charts.BubbleRadius(50, 0, 4, 24).ShouldBe(4);             // non-positive maxSize -> min radius
    }

    [AvaloniaFact]
    public void Scatter_and_bubble_render_without_throwing()
    {
        var pa = new[] { new ScatterPoint(1, 2), new ScatterPoint(3, 5), new ScatterPoint(6, 4) };
        var pb = new[] { new ScatterPoint(2, 8), new ScatterPoint(5, 3) };
        var bubbles = new[] { new ScatterPoint(1, 2, 10), new ScatterPoint(4, 6, 40), new ScatterPoint(7, 3, 5) };

        var scatter = new ScatterChart { Width = 320, Height = 220, Points = pa };
        var multi = new ScatterChart { Width = 320, Height = 220, Series = new[] { new ScatterSeries(pa, "A"), new ScatterSeries(pb, "B") } };
        var bubble = new BubbleChart { Width = 320, Height = 240, Points = bubbles };
        var empty = new ScatterChart { Width = 320, Height = 220 };

        new Window
        {
            Width = 700,
            Height = 860,
            Content = new StackPanel { Children = { scatter, multi, bubble, empty } },
        }.Show();
        Dispatcher.UIThread.RunJobs();

        scatter.Bounds.Width.ShouldBeGreaterThan(0);
        multi.Bounds.Width.ShouldBeGreaterThan(0);
        bubble.Bounds.Width.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void Scatter_snapshot_carries_xy_size_and_series()
    {
        var s1 = new[] { new ScatterPoint(1, 2, 10, "p0"), new ScatterPoint(3, 5, 20) };
        var s2 = new[] { new ScatterPoint(7, 4, 30) };
        var chart = new BubbleChart { Series = new[] { new ScatterSeries(s1, "A"), new ScatterSeries(s2, "B") } };
        var window = Show(chart, ThemeVariant.Light);
        try
        {
            chart.ResolvedPoints.Count.ShouldBe(3); // 2 + 1, series-major
            chart.ResolvedPoints[0].SeriesIndex.ShouldBe(0);
            chart.ResolvedPoints[0].X.ShouldBe(1);
            chart.ResolvedPoints[0].Value.ShouldBe(2); // Y maps to Value
            chart.ResolvedPoints[0].Size.ShouldBe(10);
            chart.ResolvedPoints[0].Label.ShouldBe("p0");
            chart.ResolvedPoints[2].SeriesIndex.ShouldBe(1);
            chart.ResolvedPoints[2].X.ShouldBe(7);

            var legend = chart.GetLegendEntries();
            legend.Count.ShouldBe(2);
            legend[0].Label.ShouldBe("A");
            legend[1].Label.ShouldBe("B");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Scatter_hover_and_click_hit_test_a_marker()
    {
        // Axes off + explicit 0..10 domains make the middle point (5,5) map to the exact plot center.
        var pts = new[] { new ScatterPoint(0, 0), new ScatterPoint(5, 5, 0, "mid"), new ScatterPoint(10, 10) };
        var chart = new ScatterChart { Width = 200, Height = 200, ShowAxes = false, XMin = 0, XMax = 10, YMin = 0, YMax = 10, Points = pts };
        var window = new Window { Width = 200, Height = 200, Content = chart };
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        ChartPointEventArgs? hovered = null;
        chart.HoverChanged += (_, e) => hovered = e;
        ChartPointEventArgs? clicked = null;
        chart.PointClicked += (_, e) => clicked = e;

        var center = new Avalonia.Point(chart.Bounds.Width / 2, chart.Bounds.Height / 2);

        window.MouseMove(center);
        Dispatcher.UIThread.RunJobs();
        chart.HoveredIndex.ShouldBe(1);
        hovered.ShouldNotBeNull();
        hovered!.Point!.Value.X.ShouldBe(5);
        hovered.Point.Value.Value.ShouldBe(5); // Y
        hovered.Point.Value.Label.ShouldBe("mid");

        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        clicked.ShouldNotBeNull();
        clicked!.Index.ShouldBe(1);

        window.Close();
    }

    [AvaloniaFact]
    public void Scatter_with_negative_y_reports_point_count_not_no_data()
    {
        // Y can be negative for XY charts; every marker still renders, so the summary must not say "No data".
        var pts = new[] { new ScatterPoint(1, -2), new ScatterPoint(2, -5, 0, "low") };
        var chart = new ScatterChart { Width = 200, Height = 200, Points = pts };
        new Window { Width = 220, Height = 220, Content = chart }.Show();
        Dispatcher.UIThread.RunJobs();

        var help = AutomationProperties.GetHelpText(chart);
        help.ShouldNotBeNull();
        help.ShouldNotBe("No data");
        help!.ShouldContain("2 points");
        help.ShouldContain("low");
    }

    [AvaloniaFact]
    public void Bubble_multi_series_hover_resolves_size_and_radius_scales()
    {
        var seriesA = new[] { new ScatterPoint(5, 5, 100, "big") };  // global-max size -> ~MaxRadius
        var seriesB = new[] { new ScatterPoint(2, 2, 25, "small") };
        var chart = new BubbleChart
        {
            Width = 200,
            Height = 200,
            ShowAxes = false,
            XMin = 0, XMax = 10, YMin = 0, YMax = 10,
            MinRadius = 4, MaxRadius = 22,
            Series = new[] { new ScatterSeries(seriesA, "A"), new ScatterSeries(seriesB, "B") },
        };
        var window = new Window { Width = 200, Height = 200, Content = chart };
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        ChartPointEventArgs? hovered = null;
        chart.HoverChanged += (_, e) => hovered = e;

        // The big bubble (5,5) maps to the symmetric plot center regardless of padding.
        var center = new Avalonia.Point(chart.Bounds.Width / 2, chart.Bounds.Height / 2);
        window.MouseMove(center);
        Dispatcher.UIThread.RunJobs();
        chart.HoveredIndex.ShouldBe(0); // series-major flat index: series A, point 0
        hovered.ShouldNotBeNull();
        hovered!.Point!.Value.Size.ShouldBe(100);
        hovered.Point.Value.SeriesIndex.ShouldBe(0);

        // 20px off-center still hits the big bubble (radius ~22), confirming Size scales the marker radius.
        window.MouseMove(new Avalonia.Point(center.X, center.Y - 20));
        Dispatcher.UIThread.RunJobs();
        chart.HoveredIndex.ShouldBe(0);

        window.Close();
    }

    private sealed record Order(string Name, double Total);

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
