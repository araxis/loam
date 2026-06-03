using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>One option in a <see cref="ToggleGroup"/>, mirroring MudBlazor's <c>MudToggleItem</c>.</summary>
public sealed class ToggleItem
{
    /// <summary>Creates an empty option.</summary>
    public ToggleItem()
    {
    }

    /// <summary>Creates an option with display text and value.</summary>
    public ToggleItem(string text, object? value)
    {
        Text = text;
        Value = value;
    }

    /// <summary>The option's display text.</summary>
    public string? Text { get; set; }

    /// <summary>The option's value.</summary>
    public object? Value { get; set; }
}

/// <summary>
/// A segmented single-select control, mirroring MudBlazor's <c>MudToggleGroup</c>. Renders <see cref="Items"/>
/// as connected segments; the one whose value equals the two-way <see cref="SelectedValue"/> is filled
/// with <see cref="Color"/>.
/// </summary>
public class ToggleGroup : TemplatedControl
{
    /// <summary>Identifies the <see cref="SelectedValue"/> property.</summary>
    public static readonly StyledProperty<object?> SelectedValueProperty =
        AvaloniaProperty.Register<ToggleGroup, object?>(nameof(SelectedValue),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<ToggleGroup, LoamColor>(nameof(Color), LoamColor.Primary);

    private readonly List<(Border Segment, Text Label, ToggleItem Item)> _segments = new();
    private StackPanel? _items;

    /// <summary>Creates the group.</summary>
    public ToggleGroup() => Items.CollectionChanged += OnItemsChanged;

    /// <summary>The selectable options.</summary>
    public ObservableCollection<ToggleItem> Items { get; } = new();

    /// <summary>The selected value (two-way). Mirrors MudBlazor's <c>Value</c>.</summary>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>Selected-segment color. Mirrors MudBlazor's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ToggleGroup);

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
        if (change.Property == SelectedValueProperty)
        {
            UpdateSelection();
        }
        else if (change.Property == ColorProperty)
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
        _segments.Clear();

        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var label = new Text
            {
                Text = item.Text,
                Typo = Typo.Button,
                Color = LoamColor.Inherit,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var segment = new Border
            {
                Child = label,
                Padding = new Thickness(16, 8),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
                BorderThickness = new Thickness(i == 0 ? 0 : 1, 0, 0, 0),
            };
            segment.Bind(Border.BorderBrushProperty,
                this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));

            var captured = item;
            segment.PointerPressed += (_, _) => SelectedValue = captured.Value;

            _items.Children.Add(segment);
            _segments.Add((segment, label, item));
        }

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        var accentName = Color is LoamColor.Default or LoamColor.Inherit
            ? nameof(LoamPalette.Primary)
            : Color.ToPaletteName()!;

        foreach (var (segment, label, item) in _segments)
        {
            var selected = Equals(item.Value, SelectedValue);
            if (selected)
            {
                segment.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Palette(accentName)));
                label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.PaletteContrast(accentName)));
            }
            else
            {
                segment.Background = Brushes.Transparent;
                label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.TextPrimary));
            }
        }
    }
}
