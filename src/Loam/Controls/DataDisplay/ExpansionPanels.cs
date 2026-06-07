using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Loam.Controls.Internal;

namespace Loam.Controls;

/// <summary>
/// A container for <see cref="ExpansionPanel"/>s, mirroring the reference API's <c>ExpansionPanels</c>.
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
    public ExpansionPanels()
    {
        AutomationProperties.SetName(this, "Expansion panels");
        Panels.CollectionChanged += OnPanelsChanged;
    }

    /// <summary>The contained panels.</summary>
    public ObservableCollection<ExpansionPanel> Panels { get; } = new();

    /// <summary>Whether multiple panels may be open at once. Mirrors the reference API's <c>MultiExpansion</c>.</summary>
    public bool MultiExpansion
    {
        get => GetValue(MultiExpansionProperty);
        set => SetValue(MultiExpansionProperty, value);
    }

    /// <summary>Adds a panel and returns it for further customization.</summary>
    public ExpansionPanel AddPanel(object? header, object? content, bool isExpanded = false, bool isEnabled = true)
    {
        var panel = new ExpansionPanel
        {
            Header = header,
            Content = content,
            IsExpanded = isExpanded,
            IsEnabled = isEnabled,
        };
        Panels.Add(panel);
        return panel;
    }

    /// <summary>Expands the panel at <paramref name="index"/> when it exists.</summary>
    public void ExpandPanel(int index)
    {
        if (index < 0 || index >= Panels.Count)
        {
            return;
        }

        Panels[index].IsExpanded = true;
    }

    /// <summary>Collapses the panel at <paramref name="index"/> when it exists.</summary>
    public void CollapsePanel(int index)
    {
        if (index < 0 || index >= Panels.Count)
        {
            return;
        }

        Panels[index].IsExpanded = false;
        UpdateAutomation();
    }

    /// <summary>Collapses every panel.</summary>
    public void CollapseAll()
    {
        foreach (var panel in Panels)
        {
            panel.IsExpanded = false;
        }

        UpdateAutomation();
    }

    /// <summary>Expands all panels in multi-expansion mode, or the first panel in accordion mode.</summary>
    public void ExpandAll()
    {
        if (Panels.Count == 0)
        {
            return;
        }

        if (!MultiExpansion)
        {
            ExpandPanel(0);
            return;
        }

        foreach (var panel in Panels)
        {
            panel.IsExpanded = true;
        }

        UpdateAutomation();
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

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MultiExpansionProperty)
        {
            EnforceAccordion();
            UpdateAutomation();
        }
        else if (change.Property == IsEnabledProperty)
        {
            Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
        }
    }

    private void OnPanelsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Rebuild();
        EnforceAccordion();
        UpdateAutomation();
    }

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

        EnforceAccordion();
        UpdateAutomation();
    }

    private void OnPanelPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ExpansionPanel.IsExpandedProperty)
        {
            return;
        }

        if (!MultiExpansion && e.GetNewValue<bool>() && sender is ExpansionPanel expanded)
        {
            foreach (var panel in Panels)
            {
                if (!ReferenceEquals(panel, expanded))
                {
                    panel.IsExpanded = false;
                }
            }
        }

        UpdateAutomation();
    }

    private void EnforceAccordion()
    {
        if (MultiExpansion)
        {
            return;
        }

        var seenExpanded = false;
        foreach (var panel in Panels)
        {
            if (!panel.IsExpanded)
            {
                continue;
            }

            if (!seenExpanded)
            {
                seenExpanded = true;
                continue;
            }

            panel.IsExpanded = false;
        }
    }

    private void UpdateAutomation()
    {
        var mode = MultiExpansion ? "multi expansion" : "accordion";
        var expanded = Panels.Count(panel => panel.IsExpanded);
        AutomationProperties.SetHelpText(this, $"{Panels.Count} panels, {expanded} expanded, {mode}");
    }
}
