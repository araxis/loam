using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Loam;
using Loam.Controls.Internal;

namespace Loam.Controls;

/// <summary>
/// A container of <see cref="Chip"/>s, mirroring the reference API's <c>ChipSet</c>. Lays the <see cref="Items"/>
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

    /// <summary>Identifies the <see cref="MultiSelect"/> property.</summary>
    public static readonly StyledProperty<bool> MultiSelectProperty =
        AvaloniaProperty.Register<ChipSet, bool>(nameof(MultiSelect));

    private readonly HashSet<Chip> _hooked = new();
    private WrapPanel? _items;
    private bool _syncingSelection;

    /// <summary>Creates the chip set.</summary>
    public ChipSet()
    {
        Items.CollectionChanged += OnItemsChanged;
        SelectedIndexes.CollectionChanged += OnSelectedIndexesChanged;
    }

    /// <summary>The chips.</summary>
    public ObservableCollection<Chip> Items { get; } = new();

    /// <summary>The selected chip indexes when <see cref="MultiSelect"/> is enabled.</summary>
    public ObservableCollection<int> SelectedIndexes { get; } = new();

    /// <summary>Whether chips can be selected by clicking. Mirrors the reference API's selection.</summary>
    public bool Selectable
    {
        get => GetValue(SelectableProperty);
        set => SetValue(SelectableProperty, value);
    }

    /// <summary>Whether a selection is required (clicking the selected chip won't clear it). Mirrors the reference API's <c>Mandatory</c>.</summary>
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

    /// <summary>Allows more than one chip to be selected. Single-selection behavior is preserved when false.</summary>
    public bool MultiSelect
    {
        get => GetValue(MultiSelectProperty);
        set => SetValue(MultiSelectProperty, value);
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
        if (change.Property == SelectedIndexProperty || change.Property == SelectableProperty ||
            change.Property == MandatoryProperty || change.Property == MultiSelectProperty)
        {
            if (change.Property == SelectedIndexProperty && !_syncingSelection)
            {
                MirrorSingleSelectionToCollection();
            }
            else if (change.Property == MultiSelectProperty && !MultiSelect)
            {
                MirrorSingleSelectionToCollection();
            }

            UpdateSelection();
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RemoveInvalidSelections();
        Rebuild();
    }

    private void OnSelectedIndexesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_syncingSelection)
        {
            return;
        }

        NormalizeSelectedIndexes();
        MirrorCollectionToSelectedIndex();
        UpdateSelection();
    }

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
            chip.Focusable = true;
            _items.Children.Add(chip);
            if (_hooked.Add(chip))
            {
                chip.PointerPressed += OnChipPressed;
                chip.KeyDown += OnChipKeyDown;
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

        chip.Focus();
        SelectChip(chip);
    }

    private void OnChipKeyDown(object? sender, KeyEventArgs e)
    {
        if (!Selectable || e.Handled || sender is not Chip chip)
        {
            return;
        }

        if (InteractionAssist.IsActivationKey(e.Key))
        {
            SelectChip(chip);
            e.Handled = true;
        }
    }

    private void SelectChip(Chip chip)
    {
        var index = Items.IndexOf(chip);
        if (MultiSelect)
        {
            ToggleMultiSelection(index);
        }
        else
        {
            SelectedIndex = index == SelectedIndex && !Mandatory ? -1 : index;
        }
    }

    private void UpdateSelection()
    {
        if (!Selectable)
        {
            return;
        }

        for (var i = 0; i < Items.Count; i++)
        {
            var selected = MultiSelect ? SelectedIndexes.Contains(i) : i == SelectedIndex;
            Items[i].Variant = selected ? Variant.Filled : Variant.Outlined;
        }
    }

    private void ToggleMultiSelection(int index)
    {
        if (index < 0)
        {
            return;
        }

        if (SelectedIndexes.Contains(index))
        {
            if (!Mandatory || SelectedIndexes.Count > 1)
            {
                SelectedIndexes.Remove(index);
            }
        }
        else
        {
            SelectedIndexes.Add(index);
        }
    }

    private void NormalizeSelectedIndexes()
    {
        _syncingSelection = true;
        try
        {
            for (var i = SelectedIndexes.Count - 1; i >= 0; i--)
            {
                var value = SelectedIndexes[i];
                if (value < 0 || value >= Items.Count || SelectedIndexes.IndexOf(value) != i)
                {
                    SelectedIndexes.RemoveAt(i);
                }
            }
        }
        finally
        {
            _syncingSelection = false;
        }
    }

    private void RemoveInvalidSelections()
    {
        NormalizeSelectedIndexes();
        if (SelectedIndex >= Items.Count)
        {
            SelectedIndex = -1;
        }
    }

    private void MirrorSingleSelectionToCollection()
    {
        _syncingSelection = true;
        try
        {
            SelectedIndexes.Clear();
            if (SelectedIndex >= 0)
            {
                SelectedIndexes.Add(SelectedIndex);
            }
        }
        finally
        {
            _syncingSelection = false;
        }
    }

    private void MirrorCollectionToSelectedIndex()
    {
        _syncingSelection = true;
        try
        {
            SelectedIndex = SelectedIndexes.Count > 0 ? SelectedIndexes[0] : -1;
        }
        finally
        {
            _syncingSelection = false;
        }
    }
}
