using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam.Controls;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class ShellTests
{
    private static void Show(Control content)
    {
        new Window { Width = 800, Height = 600, Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    [Fact]
    public void Drawer_resolve_width_for_states()
    {
        Drawer.ResolveWidth(open: true, mini: false, width: 240, miniWidth: 56).ShouldBe(240d);
        Drawer.ResolveWidth(open: true, mini: true, width: 240, miniWidth: 56).ShouldBe(56d);
        Drawer.ResolveWidth(open: false, mini: false, width: 240, miniWidth: 56).ShouldBe(0d);
    }

    [AvaloniaFact]
    public void Drawer_is_open_at_full_width_by_default()
    {
        new Drawer().Width.ShouldBe(240d);
    }

    [AvaloniaFact]
    public void AppBar_default_color_uses_appbar_palette()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var bar = new AppBar { Content = new TextBlock { Text = "Title" } };
        Show(bar);
        bar.ApplyTemplate();

        bar.Height.ShouldBe(64d);
        var border = bar.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root");
        ((ISolidColorBrush)border.Background!).Color.ShouldBe(Color.Parse("#594AE2")); // AppbarBackground (light)
    }

    [AvaloniaFact]
    public void AppBar_dense_is_shorter()
    {
        var bar = new AppBar { Dense = true };
        bar.Height.ShouldBe(48d);
    }

    [AvaloniaFact]
    public void Layout_hosts_appbar_drawer_and_main_content()
    {
        var layout = new Layout
        {
            AppBar = new AppBar { Content = new TextBlock { Text = "Bar" } },
            Drawer = new Drawer { Content = new TextBlock { Text = "Nav" } },
            Content = new MainContent { Content = new TextBlock { Text = "Body" } },
        };
        Show(layout);
        layout.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        layout.GetVisualDescendants().OfType<AppBar>().ShouldNotBeEmpty();
        layout.GetVisualDescendants().OfType<Drawer>().ShouldNotBeEmpty();
        layout.GetVisualDescendants().OfType<MainContent>().ShouldNotBeEmpty();
    }
}
