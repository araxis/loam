using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>A single command shown in a <see cref="CommandPalette"/>.</summary>
public sealed class CommandPaletteItem
{
    /// <summary>The command's display title (also matched by the search query).</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Optional leading icon path data.</summary>
    public string? Icon { get; init; }

    /// <summary>Extra search keywords (matched in addition to <see cref="Title"/>).</summary>
    public string[]? Keywords { get; init; }

    /// <summary>Invoked when the command is chosen.</summary>
    public Action? OnInvoke { get; init; }
}

/// <summary>
/// A searchable command palette: a search field over a live-filtered list of commands, with keyboard
/// navigation (Down/Up to move, Enter to run, Escape to close). Host it on an overlay/dialog, or place
/// it inline. The matching is exposed as the pure, testable <see cref="Filter"/>.
/// </summary>
public class CommandPalette : Decorator
{
    /// <summary>Identifies the <see cref="FilterText"/> property.</summary>
    public static readonly StyledProperty<string?> FilterTextProperty =
        AvaloniaProperty.Register<CommandPalette, string?>(nameof(FilterText), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="IsOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<CommandPalette, bool>(nameof(IsOpen), defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    private readonly TextField _search = new() { Placeholder = "Type a command…", Variant = Variant.Outlined };
    private readonly StackPanel _list = new() { Orientation = Orientation.Vertical, Spacing = 0 };
    private readonly List<CommandPaletteItem> _filtered = [];
    private int _selectedIndex = -1;

    /// <summary>Creates the command palette.</summary>
    public CommandPalette()
    {
        Focusable = true;

        var stack = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(12),
            Children =
            {
                _search,
                new ScrollViewer { MaxHeight = 320, Content = _list },
            },
        };

        Child = new Paper { Elevation = 8, Width = 480, Content = stack };

        _search.Bind(TextField.TextProperty, new Binding { Source = this, Path = nameof(FilterText), Mode = BindingMode.TwoWay });
        Commands.CollectionChanged += (_, _) => ReFilter();
        AutomationProperties.SetName(this, "Command palette");
        ReFilter();
    }

    /// <summary>Raised when a command is chosen (by Enter or click).</summary>
    public event EventHandler<CommandPaletteItem>? Invoked;

    /// <summary>Raised when the palette is dismissed (Escape).</summary>
    public event EventHandler? Closed;

    /// <summary>The commands to search.</summary>
    public AvaloniaList<CommandPaletteItem> Commands { get; } = [];

    /// <summary>The current search query (two-way; bound to the search field).</summary>
    public string? FilterText
    {
        get => GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>Whether the palette is shown (two-way). Set to <c>false</c> to dismiss.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The highlighted index within the filtered results, or <c>-1</c> when empty.</summary>
    public int SelectedIndex => _selectedIndex;

    /// <summary>The current filtered results.</summary>
    public IReadOnlyList<CommandPaletteItem> FilteredCommands => _filtered;

    /// <summary>
    /// Case-insensitive filter: matches when the query is empty, or is contained in a command's
    /// <see cref="CommandPaletteItem.Title"/> or any of its <see cref="CommandPaletteItem.Keywords"/>.
    /// </summary>
    public static IReadOnlyList<CommandPaletteItem> Filter(IEnumerable<CommandPaletteItem> commands, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return commands.ToList();
        }

        var q = query.Trim();
        return commands.Where(c =>
                c.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (c.Keywords?.Any(k => k.Contains(q, StringComparison.OrdinalIgnoreCase)) ?? false))
            .ToList();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FilterTextProperty)
        {
            ReFilter();
        }
        else if (change.Property == IsOpenProperty)
        {
            IsVisible = IsOpen;
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.Down:
                Move(1);
                e.Handled = true;
                break;
            case Key.Up:
                Move(-1);
                e.Handled = true;
                break;
            case Key.Enter:
                InvokeSelected();
                e.Handled = true;
                break;
            case Key.Escape:
                IsOpen = false;
                Closed?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
        }
    }

    private void Move(int delta)
    {
        if (_filtered.Count == 0)
        {
            return;
        }

        _selectedIndex = Math.Clamp(_selectedIndex + delta, 0, _filtered.Count - 1);
        UpdateHighlight();
    }

    private void InvokeSelected()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _filtered.Count)
        {
            Invoke(_filtered[_selectedIndex]);
        }
    }

    private void Invoke(CommandPaletteItem command)
    {
        command.OnInvoke?.Invoke();
        Invoked?.Invoke(this, command);
    }

    private void ReFilter()
    {
        _filtered.Clear();
        _filtered.AddRange(Filter(Commands, FilterText));

        _list.Children.Clear();
        foreach (var command in _filtered)
        {
            _list.Children.Add(BuildRow(command));
        }

        _selectedIndex = _filtered.Count > 0 ? 0 : -1;
        UpdateHighlight();
    }

    private ListItem BuildRow(CommandPaletteItem command)
    {
        Control content;
        if (string.IsNullOrEmpty(command.Icon))
        {
            content = new Text { Text = command.Title, VerticalAlignment = VerticalAlignment.Center };
        }
        else
        {
            content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Children =
                {
                    new Icon { Data = command.Icon, Size = LoamSize.Small, VerticalAlignment = VerticalAlignment.Center },
                    new Text { Text = command.Title, VerticalAlignment = VerticalAlignment.Center },
                },
            };
        }

        var row = new ListItem { Content = content };
        AutomationProperties.SetName(row, command.Title);
        row.Activated += OnRowActivated;
        return row;
    }

    private void OnRowActivated(object? sender, RoutedEventArgs e)
    {
        var index = _list.Children.IndexOf((Control)sender!);
        if (index >= 0 && index < _filtered.Count)
        {
            _selectedIndex = index;
            UpdateHighlight();
            Invoke(_filtered[index]);
        }
    }

    private void UpdateHighlight()
    {
        for (var i = 0; i < _list.Children.Count; i++)
        {
            if (_list.Children[i] is ListItem item)
            {
                item.IsSelected = i == _selectedIndex;
            }
        }
    }
}
