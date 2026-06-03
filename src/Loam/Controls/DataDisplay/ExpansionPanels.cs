using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Loam.Controls;

/// <summary>
/// A container for <see cref="ExpansionPanel"/>s, mirroring MudBlazor's <c>MudExpansionPanels</c>.
/// Stacks the <see cref="Panels"/> on an elevated surface. Unless <see cref="MultiExpansion"/> is set,
/// it behaves as an accordion: expanding one panel collapses the others.
/// </summary>
public class ExpansionPanels : TemplatedControl
{
    /// <summary>Identifies the <see cref="MultiExpansion"/> property.</summary>
    public static readonly StyledProperty<bool> MultiExpansionProperty =
        AvaloniaProperty.Register<ExpansionPanels, bool>(nameof(MultiExpansion));

    private readonly List<ExpansionPanel> _subscribed = new();
    private StackPanel? _stack;

    /// <summary>Creates the container.</summary>
    public ExpansionPanels() => Panels.CollectionChanged += OnPanelsChanged;

    /// <summary>The contained panels.</summary>
    public ObservableCollection<ExpansionPanel> Panels { get; } = new();

    /// <summary>Whether multiple panels may be open at once. Mirrors MudBlazor's <c>MultiExpansion</c>.</summary>
    public bool MultiExpansion
    {
        get => GetValue(MultiExpansionProperty);
        set => SetValue(MultiExpansionProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ExpansionPanels);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _stack = e.NameScope.Find("PART_Stack") as StackPanel;
        Rebuild();
    }

    private void OnPanelsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        foreach (var panel in _subscribed)
        {
            panel.PropertyChanged -= OnPanelPropertyChanged;
        }

        _subscribed.Clear();

        if (_stack is null)
        {
            return;
        }

        _stack.Children.Clear();
        foreach (var panel in Panels)
        {
            _stack.Children.Add(panel);
            panel.PropertyChanged += OnPanelPropertyChanged;
            _subscribed.Add(panel);
        }
    }

    private void OnPanelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ExpansionPanel.IsExpandedProperty || MultiExpansion ||
            !e.GetNewValue<bool>() || sender is not ExpansionPanel expanded)
        {
            return;
        }

        foreach (var panel in Panels)
        {
            if (!ReferenceEquals(panel, expanded))
            {
                panel.IsExpanded = false;
            }
        }
    }
}
