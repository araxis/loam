using Avalonia.Controls;
using Avalonia.Headless.XUnit;
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
}
