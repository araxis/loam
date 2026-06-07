using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class NavigationRailTests
{
    private static void Show(Control content)
    {
        new Window { Width = 400, Height = 600, Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static NavigationRail BuildRail() => new()
    {
        Items =
        {
            new NavigationRailItem { Icon = Icons.Material.Filled.Home, Label = "Home" },
            new NavigationRailItem { Icon = Icons.Material.Filled.Dashboard, Label = "Dashboard" },
            new NavigationRailItem { Icon = Icons.Material.Filled.Settings, Label = "Settings" },
        },
    };

    [AvaloniaFact]
    public void Rail_selects_first_destination_by_default()
    {
        var rail = BuildRail();
        Show(rail);

        rail.SelectedIndex.ShouldBe(0);
        rail.SelectedItem.ShouldBe(rail.Items[0]);
        rail.Items[0].IsActive.ShouldBeTrue();
        rail.Items[1].IsActive.ShouldBeFalse();
        rail.Items[2].IsActive.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Setting_selected_index_updates_active_states()
    {
        var rail = BuildRail();
        Show(rail);

        rail.SelectedIndex = 2;
        Dispatcher.UIThread.RunJobs();

        rail.SelectedItem.ShouldBe(rail.Items[2]);
        rail.Items[2].IsActive.ShouldBeTrue();
        rail.Items[0].IsActive.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Activating_item_by_keyboard_selects_it()
    {
        var rail = BuildRail();
        Show(rail);

        rail.Items[1].RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Enter });

        rail.SelectedIndex.ShouldBe(1);
        rail.Items[1].IsActive.ShouldBeTrue();
        rail.Items[0].IsActive.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Active_item_shows_secondary_container_indicator()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var rail = BuildRail();
        Show(rail);

        var activePill = rail.Items[0].GetVisualDescendants().OfType<Border>().First(b => b.Width == 56);
        ((ISolidColorBrush)activePill.Background!).Color.ShouldBe(LoamColorScheme.DefaultLight.SecondaryContainer);

        var inactivePill = rail.Items[1].GetVisualDescendants().OfType<Border>().First(b => b.Width == 56);
        ((ISolidColorBrush)inactivePill.Background!).Color.ShouldBe(Colors.Transparent);
    }
}
