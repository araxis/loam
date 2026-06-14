using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;
using LoamGrid = Loam.Controls.ResponsiveGrid;

namespace Loam.Gallery;

/// <summary>Phase 4 part 2 showcase: a working app shell — <see cref="Layout"/> + <see cref="AppBar"/> (menu toggles the drawer) + <see cref="Drawer"/> + <see cref="MainContent"/>.</summary>
public sealed class ShellView : UserControl
{
    private readonly Drawer _drawer;

    public ShellView()
    {
        _drawer = new Drawer { Mode = DrawerMode.Temporary, Content = BuildNav() };
        Content = new Layout
        {
            AppBar = new AppBar
            {
                Color = LoamColor.Primary,
                Title = "Loam App",
                NavigationIcon = Icons.Material.Filled.Menu,
                NavigationAction = () => _drawer.Open = !_drawer.Open,
            },
            Drawer = _drawer,
            Content = new MainContent { Content = BuildBody() },
        };
    }

    private static Border BuildNav()
    {
        var items = new NavMenu();
        foreach (var (icon, label) in new[]
                 {
                     (Icons.Material.Filled.Home, "Home"),
                     (Icons.Material.Filled.Search, "Search"),
                     (Icons.Material.Filled.Favorite, "Favorites"),
                     (Icons.Material.Filled.Person, "Account"),
                     (Icons.Material.Filled.Settings, "Settings"),
                 })
        {
            items.Children.Add(new NavLink { Icon = icon, Content = label, IsActive = label == "Home" });
        }

        return new Border { Child = items, Padding = new Thickness(8) };
    }

    private static StackPanel BuildBody()
    {
        var grid = new LoamGrid { Spacing = 16 };
        for (var i = 1; i <= 4; i++)
        {
            grid.Children.Add(new Col
            {
                Xs = 12,
                Sm = 6,
                Child = new Paper
                {
                    Height = 120,
                    Elevation = 2,
                    Content = new Text { Text = $"Card {i}", Typo = Typo.H6, Margin = new Thickness(16) },
                },
            });
        }

        return new StackPanel
        {
            Spacing = 16,
            Children =
            {
                new Text { Text = "Dashboard", Typo = Typo.H4, GutterBottom = true },
                new Text { Text = "Tap the menu icon to toggle the drawer.", Typo = Typo.Body1, Color = LoamColor.Default },
                grid,
            },
        };
    }
}
