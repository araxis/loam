using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>One option in a <see cref="ToggleGroup"/>, mirroring the reference API's <c>ToggleItem</c>.</summary>
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
/// A segmented single-select control, mirroring the reference API's <c>ToggleGroup</c>. Renders <see cref="Items"/>
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
    public ToggleGroup()
    {
        Focusable = true;
        Items.CollectionChanged += OnItemsChanged;
        InteractionAssist.SetAutomationName(this, "Toggle group");
    }

    /// <summary>The selectable options.</summary>
    public ObservableCollection<ToggleItem> Items { get; } = new();

    /// <summary>The selected value (two-way). Mirrors the reference API's <c>Value</c>.</summary>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>Selected-segment color. Mirrors the reference API's <c>Color</c>.</summary>
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
        else if (change.Property == ColorProperty || change.Property == IsEnabledProperty)
        {
            ApplyEnabledState();
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
                Padding = InteractionAssist.ThicknessToken(this, LoamTokens.DensityButtonPaddingMedium, new Thickness(16, 8)),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
                BorderThickness = new Thickness(i == 0 ? 0 : 1, 0, 0, 0),
                Focusable = true,
            };
            segment.Bind(Layoutable.MinHeightProperty, this.GetResourceObservable(LoamTokens.DensityInteractiveMedium));
            segment.Bind(Border.BorderBrushProperty,
                this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));

            var captured = item;
            var index = i;
            InteractionAssist.SetAutomationName(segment, captured.Text, captured.Value);
            segment.PointerPressed += (_, _) =>
            {
                segment.Focus();
                SelectedValue = captured.Value;
            };
            segment.KeyDown += (_, args) =>
            {
                if (InteractionAssist.IsActivationKey(args.Key))
                {
                    SelectedValue = captured.Value;
                    args.Handled = true;
                }
                else if (InteractionAssist.IsIncrementKey(args.Key))
                {
                    MoveSelection(index, 1);
                    args.Handled = true;
                }
                else if (InteractionAssist.IsDecrementKey(args.Key))
                {
                    MoveSelection(index, -1);
                    args.Handled = true;
                }
            };
            segment.GotFocus += (_, _) => UpdateSelection();
            segment.LostFocus += (_, _) => UpdateSelection();

            _items.Children.Add(segment);
            _segments.Add((segment, label, item));
        }

        ApplyEnabledState();
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
            else if (segment.IsFocused)
            {
                segment.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.PaletteFocus(accentName)));
                label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.TextPrimary));
            }
            else
            {
                segment.Background = Brushes.Transparent;
                label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.TextPrimary));
            }
        }
    }

    private void MoveSelection(int currentIndex, int direction)
    {
        if (_segments.Count == 0)
        {
            return;
        }

        var selectedIndex = _segments.FindIndex(segment => Equals(segment.Item.Value, SelectedValue));
        var origin = selectedIndex >= 0 ? selectedIndex : currentIndex;
        var next = Math.Clamp(origin + direction, 0, _segments.Count - 1);
        SelectedValue = _segments[next].Item.Value;
        _segments[next].Segment.Focus();
    }

    private void ApplyEnabledState() => Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
}
