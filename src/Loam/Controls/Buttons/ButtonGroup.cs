using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Loam;

namespace Loam.Controls;

/// <summary>
/// A connected row (or column) of <see cref="Button"/>s, mirroring MudBlazor's <c>MudButtonGroup</c>.
/// Lays the <see cref="Items"/> out adjacently with merged borders and shared outer corners, and
/// (when <see cref="OverrideChildStyles"/>) pushes the group's <see cref="Variant"/>/<see cref="Color"/>/
/// <see cref="Size"/> onto every child so they read as one control.
/// </summary>
public class ButtonGroup : TemplatedControl
{
    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<ButtonGroup, Variant>(nameof(Variant), Loam.Variant.Outlined);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<ButtonGroup, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<ButtonGroup, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="Vertical"/> property.</summary>
    public static readonly StyledProperty<bool> VerticalProperty =
        AvaloniaProperty.Register<ButtonGroup, bool>(nameof(Vertical));

    /// <summary>Identifies the <see cref="OverrideChildStyles"/> property.</summary>
    public static readonly StyledProperty<bool> OverrideChildStylesProperty =
        AvaloniaProperty.Register<ButtonGroup, bool>(nameof(OverrideChildStyles), true);

    private const double Radius = 4;

    private StackPanel? _items;

    /// <summary>Creates the group.</summary>
    public ButtonGroup() => Items.CollectionChanged += OnItemsChanged;

    /// <summary>The grouped buttons.</summary>
    public ObservableCollection<Button> Items { get; } = new();

    /// <summary>Shared button style. Mirrors MudBlazor's <c>Variant</c>.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Shared button color. Mirrors MudBlazor's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Shared button size. Mirrors MudBlazor's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Stacks the buttons vertically. Mirrors MudBlazor's <c>VerticalAlign</c>.</summary>
    public bool Vertical
    {
        get => GetValue(VerticalProperty);
        set => SetValue(VerticalProperty, value);
    }

    /// <summary>Whether the group pushes its Variant/Color/Size onto the children.</summary>
    public bool OverrideChildStyles
    {
        get => GetValue(OverrideChildStylesProperty);
        set => SetValue(OverrideChildStylesProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ButtonGroup);

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
        if (change.Property == VariantProperty || change.Property == ColorProperty ||
            change.Property == SizeProperty || change.Property == VerticalProperty ||
            change.Property == OverrideChildStylesProperty)
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

        _items.Orientation = Vertical ? Orientation.Vertical : Orientation.Horizontal;
        _items.Children.Clear();

        for (var i = 0; i < Items.Count; i++)
        {
            var button = Items[i];
            if (OverrideChildStyles)
            {
                button.Variant = Variant;
                button.Color = Color;
                button.Size = Size;
            }

            button.CornerRadius = CornerFor(i, Items.Count);
            button.Margin = OverlapFor(i);
            _items.Children.Add(button);
        }
    }

    private CornerRadius CornerFor(int index, int count)
    {
        var first = index == 0;
        var last = index == count - 1;
        if (count == 1)
        {
            return new CornerRadius(Radius);
        }

        if (Vertical)
        {
            return new CornerRadius(first ? Radius : 0, first ? Radius : 0, last ? Radius : 0, last ? Radius : 0);
        }

        return new CornerRadius(first ? Radius : 0, last ? Radius : 0, last ? Radius : 0, first ? Radius : 0);
    }

    private Thickness OverlapFor(int index)
    {
        if (index == 0)
        {
            return default;
        }

        // Pull each button onto its predecessor by 1px so adjacent borders merge into one.
        return Vertical ? new Thickness(0, -1, 0, 0) : new Thickness(-1, 0, 0, 0);
    }
}
