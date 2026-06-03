using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;

namespace Loam.Controls;

/// <summary>An item in a <see cref="Menu"/>, mirroring MudBlazor's <c>MudMenuItem</c>.</summary>
public sealed class MenuItem
{
    /// <summary>The item label.</summary>
    public string? Text { get; set; }

    /// <summary>An optional leading icon path.</summary>
    public string? Icon { get; set; }

    /// <summary>Invoked when the item is chosen.</summary>
    public Action? OnClick { get; set; }
}

/// <summary>
/// A button that opens a dropdown of <see cref="Items"/>, mirroring MudBlazor's <c>MudMenu</c>.
/// Inherits the <see cref="Button"/> look for the trigger; opens an Avalonia flyout of menu rows.
/// </summary>
public class Menu : Button
{
    private Flyout? _flyout;

    /// <summary>Creates the menu trigger.</summary>
    public Menu() => Click += (_, _) => Open();

    /// <summary>The menu items.</summary>
    public ObservableCollection<MenuItem> Items { get; } = new();

    private void Open()
    {
        var list = new StackPanel();
        foreach (var entry in Items)
        {
            var row = new ListItem { Icon = entry.Icon, Content = entry.Text, MinWidth = 160 };
            var captured = entry;
            row.PointerPressed += (_, _) =>
            {
                _flyout?.Hide();
                captured.OnClick?.Invoke();
            };
            list.Children.Add(row);
        }

        _flyout = new Flyout
        {
            Content = new Paper { Elevation = 8, Padding = new Thickness(0, 8), Content = list },
            Placement = PlacementMode.BottomEdgeAlignedLeft,
        };
        _flyout.ShowAt(this);
    }
}
