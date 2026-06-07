using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
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
