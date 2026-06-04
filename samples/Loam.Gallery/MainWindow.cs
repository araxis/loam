using Avalonia.Controls;

namespace Loam.Gallery;

/// <summary>Gallery shell.</summary>
public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "Loam Gallery";
        Width = 1020;
        Height = 840;
        MinWidth = 900;
        MinHeight = 680;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Content = new ComponentsView();
    }
}
