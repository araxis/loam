using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
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

    private static KeyEventArgs KeyArgs(Key key) => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = key,
    };

    [AvaloniaFact]
    public void Alert_filled_uses_severity_color()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var alert = new Alert { Color = LoamColor.Error, Variant = Variant.Filled, Content = "Failed" };
        Show(alert);
        alert.ApplyTemplate();

        var root = alert.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root");
        ((ISolidColorBrush)root.Background!).Color.ShouldBe(Color.Parse("#B3261E"));
        AutomationProperties.GetName(alert).ShouldBe("Failed");
    }

    [AvaloniaFact]
    public void Alert_disabled_uses_state_opacity()
    {
        var alert = new Alert { Content = "Paused", IsEnabled = false };
        Show(alert);

        alert.Opacity.ShouldBeLessThan(1);
    }

    [AvaloniaFact]
    public void ProgressLinear_fill_reflects_value()
    {
        var progress = new ProgressLinear { Value = 50, Width = 200 };
        Show(progress);
        progress.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        progress.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Fill").Width.ShouldBeGreaterThan(0);
        AutomationProperties.GetName(progress).ShouldBe("Progress");
    }

    [AvaloniaFact]
    public void ProgressLinear_indeterminate_uses_moving_fill()
    {
        var progress = new ProgressLinear { Indeterminate = true, Width = 200 };
        Show(progress);
        progress.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var fill = progress.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Fill");
        fill.Width.ShouldBeGreaterThanOrEqualTo(24);

        progress.IndeterminateOffset = 0.5;
        Dispatcher.UIThread.RunJobs();

        var transform = fill.RenderTransform.ShouldBeOfType<TranslateTransform>();
        transform.X.ShouldNotBe(0);
    }

    [AvaloniaFact]
    public void Skeleton_static_and_animated_modes_are_configurable()
    {
        var skeleton = new Skeleton { Animate = false };
        Show(skeleton);

        AutomationProperties.GetName(skeleton).ShouldBe("Loading");
        skeleton.Animate.ShouldBeFalse();
        skeleton.Opacity.ShouldBe(1);

        skeleton.Animate = true;
        Dispatcher.UIThread.RunJobs();
        skeleton.Animate.ShouldBeTrue();
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

    [AvaloniaFact]
    public void ListItem_is_named_selected_disabled_and_keyboard_activates()
    {
        var activated = false;
        var item = new ListItem
        {
            Icon = Icons.Material.Filled.Home,
            Content = "Inbox",
            IsSelected = true,
        };
        item.Activated += (_, _) => activated = true;
        Show(item);
        item.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        item.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(item).ShouldBe("Inbox");
        var root = item.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_Root");
        root.Focusable.ShouldBeTrue();

        var key = KeyArgs(Key.Space);
        item.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        activated.ShouldBeTrue();

        item.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        item.Opacity.ShouldBeLessThan(1);

        var disabledKey = KeyArgs(Key.Enter);
        item.RaiseEvent(disabledKey);
        disabledKey.Handled.ShouldBeFalse();
    }
}
