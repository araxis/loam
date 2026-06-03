using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Loam.Controls;
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
}
