using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>An item in a <see cref="Menu"/>, mirroring the reference API's <c>MenuItem</c>.</summary>
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
/// A button that opens a dropdown of <see cref="Items"/>, mirroring the reference API's <c>Menu</c>.
/// Inherits the <see cref="Button"/> look for the trigger; opens an Avalonia flyout of menu rows.
/// </summary>
public class Menu : Button
{
    private Flyout? _flyout;
    private IInputElement? _restoreFocus;

    /// <summary>Creates the menu trigger.</summary>
    public Menu()
    {
        Click += (_, _) => Open();
        InteractionAssist.SetAutomationName(this, Content, "Menu");
    }

    /// <summary>The menu items.</summary>
    public ObservableCollection<MenuItem> Items { get; } = new();

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            InteractionAssist.SetAutomationName(this, Content, "Menu");
        }
    }

    private void Open()
    {
        var list = new StackPanel();
        ListItem? firstRow = null;
        foreach (var entry in Items)
        {
            var row = new ListItem { Icon = entry.Icon, Content = entry.Text, MinWidth = 160, Focusable = true };
            var captured = entry;
            InteractionAssist.SetAutomationName(row, captured.Text);
            void Choose()
            {
                captured.OnClick?.Invoke();
                Close();
            }

            row.PointerPressed += (_, _) => Choose();
            row.KeyDown += (_, args) =>
            {
                if (InteractionAssist.IsActivationKey(args.Key))
                {
                    Choose();
                    args.Handled = true;
                }
                else if (args.Key == Key.Escape)
                {
                    Close();
                    args.Handled = true;
                }
            };
            firstRow ??= row;
            list.Children.Add(row);
        }

        _restoreFocus = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        var paper = new Paper { Elevation = 8, Padding = new Thickness(0, 8), Content = list };
        InteractionAssist.ApplyZIndex(paper, LoamTokens.ZIndex(nameof(LoamZIndex.Popover)), LoamZIndex.Default.Popover);

        _flyout = new Flyout
        {
            Content = paper,
            Placement = PlacementMode.BottomEdgeAlignedLeft,
        };
        _flyout.Closed += (_, _) => RestoreFocus();
        _flyout.ShowAt(this);
        firstRow?.Focus();
    }

    private void Close() => _flyout?.Hide();

    private void RestoreFocus()
    {
        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            InteractionAssist.RestoreFocus(topLevel, _restoreFocus);
        }

        _restoreFocus = null;
    }
}
