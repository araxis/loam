using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Automation;
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

    /// <summary>Whether the row can be activated.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Whether this entry renders as a divider row.</summary>
    public bool IsDivider { get; set; }

    /// <summary>Optional secondary text, commonly a shortcut hint.</summary>
    public string? ShortcutText { get; set; }
}

/// <summary>
/// A button that opens a dropdown of <see cref="Items"/>, mirroring the reference API's <c>Menu</c>.
/// Inherits the <see cref="Button"/> look for the trigger; opens an Avalonia flyout of menu rows.
/// </summary>
public class Menu : Button
{
    /// <summary>Identifies the <see cref="CloseOnItemClick"/> property.</summary>
    public static readonly StyledProperty<bool> CloseOnItemClickProperty =
        AvaloniaProperty.Register<Menu, bool>(nameof(CloseOnItemClick), true);

    /// <summary>Identifies the <see cref="MenuWidth"/> property.</summary>
    public static readonly StyledProperty<double> MenuWidthProperty =
        AvaloniaProperty.Register<Menu, double>(nameof(MenuWidth), 180);

    private Flyout? _flyout;
    private IInputElement? _restoreFocus;

    /// <summary>Creates the menu trigger.</summary>
    public Menu()
    {
        Click += (_, _) => Open();
        InteractionAssist.SetAutomationName(this, Content, "Menu");
        ApplyAutomationState(false);
    }

    /// <summary>The menu items.</summary>
    public ObservableCollection<MenuItem> Items { get; } = new();

    /// <summary>Whether choosing an enabled item closes the menu.</summary>
    public bool CloseOnItemClick
    {
        get => GetValue(CloseOnItemClickProperty);
        set => SetValue(CloseOnItemClickProperty, value);
    }

    /// <summary>Minimum width of the opened menu surface.</summary>
    public double MenuWidth
    {
        get => GetValue(MenuWidthProperty);
        set => SetValue(MenuWidthProperty, value);
    }

    /// <summary>Opens the menu.</summary>
    public void OpenMenu() => Open();

    /// <summary>Closes the menu.</summary>
    public void CloseMenu() => Close();

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
        if (!IsEnabled)
        {
            ApplyAutomationState(false);
            return;
        }

        Close();

        var list = new StackPanel();
        var rows = new System.Collections.Generic.List<ListItem>();
        ListItem? firstRow = null;
        foreach (var entry in Items)
        {
            if (entry.IsDivider)
            {
                list.Children.Add(new Divider { Margin = new Thickness(0, 4) });
                continue;
            }

            var row = new ListItem
            {
                Icon = entry.Icon,
                Content = entry.Text,
                MinWidth = Math.Max(120, MenuWidth - 16),
                Focusable = entry.IsEnabled,
                IsEnabled = entry.IsEnabled,
            };
            var captured = entry;
            if (!string.IsNullOrWhiteSpace(captured.ShortcutText))
            {
                row.Action = new Text
                {
                    Text = captured.ShortcutText,
                    Typo = Typo.LabelMedium,
                    Color = LoamColor.Default,
                    Margin = new Thickness(24, 0, 0, 0),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                };
            }

            InteractionAssist.SetAutomationName(row, captured.Text, captured.ShortcutText, "Menu item");
            AutomationProperties.SetHelpText(row, captured.IsEnabled ? "Menu item" : "Disabled menu item");
            void Choose()
            {
                if (!row.IsEnabled)
                {
                    return;
                }

                captured.OnClick?.Invoke();
                if (CloseOnItemClick)
                {
                    Close();
                }
            }

            row.Activated += (_, _) => Choose();
            row.KeyDown += (_, args) =>
            {
                HandlePopupKey(args, rows, row);
            };
            if (row.IsEnabled)
            {
                rows.Add(row);
                firstRow ??= row;
            }

            list.Children.Add(row);
        }

        list.KeyDown += (_, args) => HandlePopupKey(args, rows, null);

        _restoreFocus = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        var paper = PopupSurface.MenuPaper(list, MenuWidth);
        AutomationProperties.SetName(paper, $"{AutomationProperties.GetName(this)} menu");
        paper.KeyDown += (_, args) => HandlePopupKey(args, rows, null);
        InteractionAssist.ApplyZIndex(paper, LoamTokens.ZIndex(nameof(LoamZIndex.Popover)), LoamZIndex.Default.Popover);

        var flyout = PopupSurface.Flyout(paper);
        _flyout = flyout;
        flyout.Closed += (_, _) =>
        {
            if (ReferenceEquals(_flyout, flyout))
            {
                _flyout = null;
                ApplyAutomationState(false);
                RestoreFocus();
            }
        };
        flyout.ShowAt(this);
        ApplyAutomationState(true);
        firstRow?.Focus();
    }

    private void HandlePopupKey(KeyEventArgs args, IReadOnlyList<ListItem> rows, ListItem? current)
    {
        if (args.Handled)
        {
            return;
        }

        switch (args.Key)
        {
            case Key.Escape:
                Close();
                args.Handled = true;
                break;
            case Key.Down:
            case Key.Up:
                FocusRelativeRow(rows, current, args.Key == Key.Down ? 1 : -1);
                args.Handled = true;
                break;
        }
    }

    private static void FocusRelativeRow(IReadOnlyList<ListItem> rows, ListItem? current, int direction)
    {
        if (rows.Count == 0)
        {
            return;
        }

        var index = -1;
        if (current is not null)
        {
            for (var i = 0; i < rows.Count; i++)
            {
                if (ReferenceEquals(rows[i], current))
                {
                    index = i;
                    break;
                }
            }
        }

        var next = index < 0
            ? 0
            : (index + direction + rows.Count) % rows.Count;
        rows[next].Focus();
    }

    private void Close()
    {
        if (_flyout is null)
        {
            ApplyAutomationState(false);
            return;
        }

        _flyout.Hide();
    }

    private void ApplyAutomationState(bool open) =>
        AutomationProperties.SetHelpText(this, open ? "Open menu" : "Closed menu");

    private void RestoreFocus()
    {
        if (TopLevel.GetTopLevel(this) is { } topLevel)
        {
            InteractionAssist.RestoreFocus(topLevel, _restoreFocus);
        }

        _restoreFocus = null;
    }
}
