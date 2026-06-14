using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A Material 3 bottom navigation bar — a horizontal strip of equal-width top-level destinations for
/// the bottom of a compact (mobile-width) layout. Hosts <see cref="BottomNavigationItem"/>s with single
/// selection. Use <see cref="NavigationRail"/> for the side rail and <see cref="NavMenu"/> for the full
/// drawer list.
/// </summary>
public class BottomNavigation : Decorator
{
    /// <summary>Identifies the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<BottomNavigation, int>(
            nameof(SelectedIndex), defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    private readonly UniformGrid _itemsHost = new() { Rows = 1 };
    private readonly Border _surface;
    private IDisposable? _background;

    /// <summary>Creates the bottom navigation bar.</summary>
    public BottomNavigation()
    {
        _surface = new Border { Height = 80, Child = _itemsHost };
        _background = _surface.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.ColorSurfaceContainer));
        Child = _surface;

        Items.CollectionChanged += (_, _) => Rebuild();
        AutomationProperties.SetName(this, "Bottom navigation");
    }

    /// <summary>Raised when <see cref="SelectedIndex"/> changes (by click or programmatically).</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>The bar destinations (rendered in equal-width cells).</summary>
    public AvaloniaList<BottomNavigationItem> Items { get; } = [];

    /// <summary>The selected destination index (two-way). <c>-1</c> selects nothing.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The selected destination, or <c>null</c> when none is selected.</summary>
    public BottomNavigationItem? SelectedItem =>
        SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedIndexProperty)
        {
            UpdateSelection();
        }
    }

    private void Rebuild()
    {
        _itemsHost.Children.Clear();
        foreach (var item in Items)
        {
            item.Selected -= OnItemSelected;
            item.Selected += OnItemSelected;
            _itemsHost.Children.Add(item);
        }

        UpdateSelection();
    }

    private void OnItemSelected(object? sender, EventArgs e)
    {
        if (sender is BottomNavigationItem item)
        {
            var index = Items.IndexOf(item);
            if (index >= 0)
            {
                SelectedIndex = index;
            }
        }
    }

    private void UpdateSelection()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            Items[i].IsActive = i == SelectedIndex;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// A destination in a <see cref="BottomNavigation"/> bar. Shares the icon-over-label,
/// active-indicator-pill anatomy and activation behavior of <see cref="NavigationRailItem"/>.
/// </summary>
public class BottomNavigationItem : NavigationRailItem
{
}
