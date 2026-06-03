using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Loam;
using Loam.Controls;
using LoamGrid = Loam.Controls.Grid;

namespace Loam.Gallery;

/// <summary>Phase 4 part 2 showcase: a working app shell — <see cref="Layout"/> + <see cref="AppBar"/> (menu toggles the drawer) + <see cref="Drawer"/> + <see cref="MainContent"/>.</summary>
public sealed class ShellView : UserControl
{
    private readonly Drawer _drawer;

    public ShellView()
    {
        _drawer = new Drawer { Content = BuildNav() };
        Content = new Layout
        {
            AppBar = new AppBar { Color = LoamColor.Primary, Content = BuildToolbar() },
            Drawer = _drawer,
            Content = new MainContent { Content = BuildBody() },
        };
    }

    private StackPanel BuildToolbar()
    {
        var menu = new Icon
        {
            Data = Icons.Material.Filled.Menu,
            Color = LoamColor.Inherit,
            Cursor = new Cursor(StandardCursorType.Hand),
            VerticalAlignment = VerticalAlignment.Center,
        };
        menu.PointerPressed += (_, _) => _drawer.Open = !_drawer.Open;

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                menu,
                new Text { Text = "Loam App", Typo = Typo.H6, Color = LoamColor.Inherit, VerticalAlignment = VerticalAlignment.Center },
            },
        };
    }

    private static Border BuildNav()
    {
        var items = new StackPanel { Spacing = 2 };
        foreach (var (icon, label) in new[]
                 {
                     (Icons.Material.Filled.Home, "Home"),
                     (Icons.Material.Filled.Search, "Search"),
                     (Icons.Material.Filled.Favorite, "Favorites"),
                     (Icons.Material.Filled.Person, "Account"),
                     (Icons.Material.Filled.Settings, "Settings"),
                 })
        {
            items.Children.Add(NavItem(icon, label));
        }

        return new Border { Child = items, Padding = new Thickness(8) };
    }

    private static Border NavItem(string icon, string label)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 16,
            Children =
            {
                new Icon { Data = icon, Color = LoamColor.Default, VerticalAlignment = VerticalAlignment.Center },
                new Text { Text = label, Typo = Typo.Body2, VerticalAlignment = VerticalAlignment.Center },
            },
        };
        return new Border { Child = row, Padding = new Thickness(16, 10), Cursor = new Cursor(StandardCursorType.Hand) };
    }

    private static StackPanel BuildBody()
    {
        var grid = new LoamGrid { Spacing = 16 };
        for (var i = 1; i <= 4; i++)
        {
            grid.Children.Add(new Item
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
