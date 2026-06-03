using Avalonia.Controls;

namespace Loam.Gallery;

/// <summary>Gallery shell. Tabs across the Loam design system and the component showcase.</summary>
public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "Loam Gallery";
        Width = 1000;
        Height = 800;
        Content = new TabControl
        {
            Items =
            {
                new TabItem { Header = "Design System", Content = new DesignSystemView() },
                new TabItem { Header = "Components", Content = new ComponentsView() },
                new TabItem { Header = "Layout", Content = new LayoutView() },
                new TabItem { Header = "App Shell", Content = new ShellView() },
            },
        };
    }
}
