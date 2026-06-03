using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Loam.Controls;

/// <summary>
/// A cluster of overlapping <see cref="Avatar"/>s, mirroring MudBlazor's <c>MudAvatarGroup</c>. Shows up
/// to <see cref="Max"/> avatars overlapped by <see cref="Spacing"/>; any beyond <see cref="Max"/> collapse
/// into a trailing "+N" surplus avatar.
/// </summary>
public class AvatarGroup : TemplatedControl
{
    /// <summary>Identifies the <see cref="Max"/> property.</summary>
    public static readonly StyledProperty<int> MaxProperty =
        AvaloniaProperty.Register<AvatarGroup, int>(nameof(Max), 4);

    /// <summary>Identifies the <see cref="Spacing"/> property.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<AvatarGroup, double>(nameof(Spacing), -8);

    private StackPanel? _items;

    /// <summary>Creates the group.</summary>
    public AvatarGroup() => Items.CollectionChanged += OnItemsChanged;

    /// <summary>The avatars in the cluster.</summary>
    public ObservableCollection<Avatar> Items { get; } = new();

    /// <summary>Maximum avatars shown before a "+N" surplus. Mirrors MudBlazor's <c>Max</c>.</summary>
    public int Max
    {
        get => GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>Horizontal overlap (negative overlaps). Mirrors MudBlazor's <c>Spacing</c>.</summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(AvatarGroup);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _items = e.NameScope.Find("PART_Items") as StackPanel;
        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaxProperty || change.Property == SpacingProperty)
        {
            Rebuild();
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

        var shown = Math.Min(Math.Max(0, Max), Items.Count);
        for (var i = 0; i < shown; i++)
        {
            Items[i].Margin = i == 0 ? default : new Thickness(Spacing, 0, 0, 0);
            _items.Children.Add(Items[i]);
        }

        var surplus = Items.Count - shown;
        if (surplus > 0)
        {
            var sample = Items.Count > 0 ? Items[0] : new Avatar();
            _items.Children.Add(new Avatar
            {
                Content = $"+{surplus}",
                Size = sample.Size,
                Square = sample.Square,
                Rounded = sample.Rounded,
                Margin = shown == 0 ? default : new Thickness(Spacing, 0, 0, 0),
            });
        }
    }
}
