using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class NavigationTests
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
    public void Link_defaults_to_primary_and_underline_follows_property()
    {
        var link = new Link { Text = "Go" };
        Show(link);

        link.Color.ShouldBe(LoamColor.Primary);
        link.TextDecorations.ShouldBeNull();

        link.Underline = true;
        link.TextDecorations.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void Link_is_focusable_named_and_keyboard_clickable()
    {
        var clicked = false;
        var link = new Link { Text = "Go", OnClick = () => clicked = true };
        Show(link);

        link.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(link).ShouldBe("Go");

        var key = KeyArgs(Key.Enter);
        link.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        clicked.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Breadcrumbs_renders_links_with_a_current_tail()
    {
        var crumbs = new Breadcrumbs();
        crumbs.Items.Add(new BreadcrumbItem("Home", () => { }));
        crumbs.Items.Add(new BreadcrumbItem("Library", () => { }));
        crumbs.Items.Add(new BreadcrumbItem("Data"));
        Show(crumbs);
        crumbs.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var panel = crumbs.GetVisualDescendants().OfType<StackPanel>().First(p => p.Name == "PART_Items");
        panel.Children.Count.ShouldBe(5); // item, sep, item, sep, item

        crumbs.GetVisualDescendants().OfType<Link>().Count().ShouldBe(2);
        var current = panel.Children[4].ShouldBeOfType<Loam.Controls.Text>();
        current.Text.ShouldBe("Data");
        AutomationProperties.GetName(crumbs).ShouldBe("Breadcrumbs");
    }

    [Fact]
    public void NavMenu_defaults_to_vertical_spacing_and_automation_name()
    {
        var menu = new NavMenu();

        menu.Orientation.ShouldBe(Orientation.Vertical);
        menu.Spacing.ShouldBe(2d);
        AutomationProperties.GetName(menu).ShouldBe("Navigation menu");
    }

    [AvaloniaFact]
    public void Breadcrumb_links_are_focusable_and_keyboard_clickable()
    {
        var clicked = false;
        var crumbs = new Breadcrumbs();
        crumbs.Items.Add(new BreadcrumbItem("Home", () => clicked = true));
        crumbs.Items.Add(new BreadcrumbItem("Current"));
        Show(crumbs);
        crumbs.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var link = crumbs.GetVisualDescendants().OfType<Link>().Single();
        link.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(link).ShouldBe("Home");

        var key = KeyArgs(Key.Space);
        link.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        clicked.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void NavLink_active_tints_icon_and_background()
    {
        var link = new NavLink { Icon = Icons.Material.Filled.Home, Content = "Home", IsActive = true };
        Show(link);
        link.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var root = link.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root");
        var brush = root.Background.ShouldBeAssignableTo<ISolidColorBrush>();
        brush!.Color.A.ShouldBeGreaterThan((byte)0);

        var icon = link.GetVisualDescendants().OfType<Icon>().First(i => i.Name == "PART_Icon");
        icon.Color.ShouldBe(LoamColor.Primary);
        icon.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void NavLink_inactive_icon_is_default()
    {
        var link = new NavLink { Icon = Icons.Material.Filled.Home, Content = "Home" };
        Show(link);
        link.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        link.GetVisualDescendants().OfType<Icon>().First(i => i.Name == "PART_Icon").Color.ShouldBe(LoamColor.Default);
    }

    [AvaloniaFact]
    public void NavLink_is_focusable_named_and_keyboard_clickable()
    {
        var clicked = false;
        var link = new NavLink { Icon = Icons.Material.Filled.Home, Content = "Home", OnClick = () => clicked = true };
        Show(link);
        link.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        link.Focusable.ShouldBeTrue();
        link.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root").Focusable.ShouldBeTrue();
        AutomationProperties.GetName(link).ShouldBe("Home");

        var key = KeyArgs(Key.Enter);
        link.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        clicked.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void NavGroup_toggles_nested_items_visibility()
    {
        var group = new NavGroup { Title = "Admin", Icon = Icons.Material.Filled.Settings };
        group.Items.Add(new NavLink { Content = "Users" });
        group.Items.Add(new NavLink { Content = "Roles" });
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var items = group.GetVisualDescendants().OfType<StackPanel>().First(p => p.Name == "PART_Items");
        items.Children.Count.ShouldBe(2);
        items.IsVisible.ShouldBeFalse();

        group.Expanded = true;
        Dispatcher.UIThread.RunJobs();
        items.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void NavGroup_is_focusable_named_and_toggles_from_keyboard()
    {
        var group = new NavGroup { Title = "Admin" };
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        group.Focusable.ShouldBeTrue();
        group.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Header").Focusable.ShouldBeTrue();
        AutomationProperties.GetName(group).ShouldBe("Admin");

        var key = KeyArgs(Key.Enter);
        group.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        group.Expanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void NavGroup_disabled_does_not_toggle_from_keyboard()
    {
        var group = new NavGroup { Title = "Admin", IsEnabled = false };
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var key = KeyArgs(Key.Enter);
        group.RaiseEvent(key);

        key.Handled.ShouldBeFalse();
        group.Expanded.ShouldBeFalse();
    }
}
