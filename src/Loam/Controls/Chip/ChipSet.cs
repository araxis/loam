using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Loam;

namespace Loam.Controls;

/// <summary>
/// A container of <see cref="Chip"/>s, mirroring MudBlazor's <c>MudChipSet</c>. Lays the <see cref="Items"/>
/// out in a wrap; when <see cref="Selectable"/>, clicking a chip sets the two-way <see cref="SelectedIndex"/>
/// and renders it filled (others outlined). Set <see cref="Mandatory"/> to keep one always selected.
/// </summary>
public class ChipSet : TemplatedControl
{
    /// <summary>Identifies the <see cref="Selectable"/> property.</summary>
    public static readonly StyledProperty<bool> SelectableProperty =
        AvaloniaProperty.Register<ChipSet, bool>(nameof(Selectable));

    /// <summary>Identifies the <see cref="Mandatory"/> property.</summary>
    public static readonly StyledProperty<bool> MandatoryProperty =
        AvaloniaProperty.Register<ChipSet, bool>(nameof(Mandatory));

    /// <summary>Identifies the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<ChipSet, int>(nameof(SelectedIndex), -1,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private readonly HashSet<Chip> _hooked = new();
    private WrapPanel? _items;

    /// <summary>Creates the chip set.</summary>
    public ChipSet() => Items.CollectionChanged += OnItemsChanged;

    /// <summary>The chips.</summary>
    public ObservableCollection<Chip> Items { get; } = new();

    /// <summary>Whether chips can be selected by clicking. Mirrors MudBlazor's selection.</summary>
    public bool Selectable
    {
        get => GetValue(SelectableProperty);
        set => SetValue(SelectableProperty, value);
    }

    /// <summary>Whether a selection is required (clicking the selected chip won't clear it). Mirrors MudBlazor's <c>Mandatory</c>.</summary>
    public bool Mandatory
    {
        get => GetValue(MandatoryProperty);
        set => SetValue(MandatoryProperty, value);
    }

    /// <summary>The selected chip index, or -1 (two-way).</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ChipSet);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _items = e.NameScope.Find("PART_Items") as WrapPanel;
        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedIndexProperty || change.Property == SelectableProperty)
        {
            UpdateSelection();
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (_items is null)
        {
            return;
        }

        _items.Children.Clear();
        foreach (var chip in Items)
        {
            chip.Margin = new Thickness(0, 0, 8, 8);
            _items.Children.Add(chip);
            if (_hooked.Add(chip))
            {
                chip.PointerPressed += OnChipPressed;
            }
        }

        UpdateSelection();
    }

    private void OnChipPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (!Selectable || sender is not Chip chip)
        {
            return;
        }

        var index = Items.IndexOf(chip);
        SelectedIndex = index == SelectedIndex && !Mandatory ? -1 : index;
    }

    private void UpdateSelection()
    {
        if (!Selectable)
        {
            return;
        }

        for (var i = 0; i < Items.Count; i++)
        {
            Items[i].Variant = i == SelectedIndex ? Variant.Filled : Variant.Outlined;
        }
    }
}
