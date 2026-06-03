using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class FeedbackTests
{
    private static void Show(Control content)
    {
        new Window { Width = 400, Height = 300, Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Alert_filled_uses_severity_color()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var alert = new Alert { Color = LoamColor.Error, Variant = Variant.Filled, Content = "Failed" };
        Show(alert);
        alert.ApplyTemplate();

        var root = alert.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root");
        ((ISolidColorBrush)root.Background!).Color.ShouldBe(Color.Parse("#F44336"));
    }

    [AvaloniaFact]
    public void ProgressLinear_fill_reflects_value()
    {
        var progress = new ProgressLinear { Value = 50, Width = 200 };
        Show(progress);
        progress.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        progress.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Fill").Width.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ProgressCircular_fraction_clamps_and_sizes_by_size()
    {
        ProgressCircular.Fraction(25, 0, 100).ShouldBe(0.25);
        ProgressCircular.Fraction(-5, 0, 100).ShouldBe(0);
        ProgressCircular.Fraction(150, 0, 100).ShouldBe(1);
        ProgressCircular.Diameter(LoamSize.Small).ShouldBe(24);
        ProgressCircular.Diameter(LoamSize.Large).ShouldBe(56);
    }

    [AvaloniaFact]
    public void ProgressCircular_measures_to_its_diameter()
    {
        var progress = new ProgressCircular { Size = LoamSize.Medium, Indeterminate = false, Value = 40 };
        Show(progress);
        progress.Measure(Size.Infinity);

        progress.DesiredSize.Width.ShouldBe(40);
        progress.DesiredSize.Height.ShouldBe(40);
    }

    [AvaloniaFact]
    public void ListItem_shows_icon_and_content()
    {
        var item = new ListItem { Icon = Icons.Material.Filled.Home, Content = "Home" };
        Show(item);
        item.ApplyTemplate();

        item.GetVisualDescendants().OfType<Icon>().First(i => i.Name == "PART_Icon").IsVisible.ShouldBeTrue();
    }
}
