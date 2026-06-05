using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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
    public void Drawer_mode_defaults_to_docked_with_scrim_close_enabled()
    {
        var drawer = new Drawer();

        drawer.Mode.ShouldBe(DrawerMode.Docked);
        drawer.ShowScrim.ShouldBeTrue();
        drawer.CloseOnScrimClick.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Drawer_is_open_at_full_width_by_default()
    {
        new Drawer().Width.ShouldBe(240d);
    }

    [AvaloniaFact]
    public void AppBar_default_color_uses_surface_role()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var bar = new AppBar { Content = new TextBlock { Text = "Title" } };
        Show(bar);
        bar.ApplyTemplate();

        bar.Height.ShouldBe(64d);
        var border = bar.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root");
        ((ISolidColorBrush)border.Background!).Color.ShouldBe(Color.Parse("#FFFBFE"));
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

    [AvaloniaFact]
    public void Layout_docked_drawer_reserves_content_space()
    {
        var drawer = new Drawer { Content = new TextBlock { Text = "Nav" } };
        var layout = new Layout
        {
            Drawer = drawer,
            Content = new MainContent { Content = new TextBlock { Text = "Body" } },
        };
        Show(layout);
        layout.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var content = layout.GetVisualDescendants().OfType<ContentPresenter>()
            .First(p => p.Name == "PART_ContentPresenter");
        content.Bounds.X.ShouldBe(240d, 1d);
    }

    [AvaloniaFact]
    public void Layout_temporary_drawer_overlays_content_and_shows_scrim()
    {
        var drawer = new Drawer
        {
            Mode = DrawerMode.Temporary,
            Open = true,
            Content = new TextBlock { Text = "Nav" },
        };
        var layout = new Layout
        {
            Drawer = drawer,
            Content = new MainContent { Content = new TextBlock { Text = "Body" } },
        };
        Show(layout);
        layout.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var content = layout.GetVisualDescendants().OfType<ContentPresenter>()
            .First(p => p.Name == "PART_ContentPresenter");
        content.Bounds.X.ShouldBe(0d, 1d);

        var scrim = layout.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_DrawerScrim");
        scrim.IsVisible.ShouldBeTrue();

        drawer.Open = false;
        Dispatcher.UIThread.RunJobs();
        scrim.IsVisible.ShouldBeFalse();
    }
}
