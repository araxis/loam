using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Loam.Controls.Internal;

namespace Loam.Controls;

/// <summary>
/// A hierarchical tree, mirroring the reference API's <c>TreeView</c>. Hosts root <see cref="Items"/>
/// (<see cref="TreeViewItem"/>s, each with their own children); clicking any node selects it and
/// updates <see cref="SelectedItem"/>, highlighting that single row.
/// </summary>
public class TreeView : TemplatedControl
{
    /// <summary>Identifies the <see cref="SelectedItem"/> property.</summary>
    public static readonly StyledProperty<TreeViewItem?> SelectedItemProperty =
        AvaloniaProperty.Register<TreeView, TreeViewItem?>(nameof(SelectedItem),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private StackPanel? _root;

    /// <summary>Creates the tree.</summary>
    public TreeView()
    {
        Focusable = true;
        Items.CollectionChanged += OnItemsChanged;
        AddHandler(TreeViewItem.ItemSelectedEvent, OnItemSelected);
        InteractionAssist.SetAutomationName(this, "Tree view");
    }

    /// <summary>The root nodes.</summary>
    public ObservableCollection<TreeViewItem> Items { get; } = new();

    /// <summary>The selected node, or null (two-way).</summary>
    public TreeViewItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TreeView);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find("PART_Root") as StackPanel;
        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedItemProperty)
        {
            ApplySelection(SelectedItem);
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ReconcileSelection();
        Rebuild();
    }

    internal void ReconcileSelection()
    {
        if (SelectedItem is not null && !AllItems().Contains(SelectedItem))
        {
            SelectedItem.IsSelected = false;
            SelectedItem = null;
        }

        ApplySelection(SelectedItem);
    }

    private void Rebuild()
    {
        if (_root is null)
        {
            return;
        }

        _root.Children.Clear();
        foreach (var item in Items)
        {
            _root.Children.Add(item);
        }

        ApplySelection(SelectedItem);
    }

    private void OnItemSelected(object? sender, RoutedEventArgs e)
    {
        if (e.Source is TreeViewItem item)
        {
            SelectedItem = item;
            e.Handled = true;
        }
    }

    private void ApplySelection(TreeViewItem? selected)
    {
        foreach (var node in AllItems())
        {
            node.IsSelected = ReferenceEquals(node, selected);
        }
    }

    private IEnumerable<TreeViewItem> AllItems()
    {
        foreach (var item in Items)
        {
            foreach (var node in Flatten(item))
            {
                yield return node;
            }
        }
    }

    private static IEnumerable<TreeViewItem> Flatten(TreeViewItem item)
    {
        yield return item;
        foreach (var child in item.Items)
        {
            foreach (var node in Flatten(child))
            {
                yield return node;
            }
        }
    }
}
