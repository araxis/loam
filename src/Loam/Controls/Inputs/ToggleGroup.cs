using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Loam;
using Loam.Controls.Internal;

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
/// A connected single-select button strip. Each option is rendered as a real <see cref="Button"/> segment so
/// sizing, typography, focus, disabled, and press feedback stay consistent with the button family.
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

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<ToggleGroup, LoamSize>(nameof(Size), LoamSize.Medium);

    private const double Radius = 999;

    private readonly List<SegmentState> _segments = new();
    private StackPanel? _items;
    private double _intrinsicMinWidth;
    private double _intrinsicMinHeight;

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

    /// <summary>Segment size. Uses the same five-size density scale as the button family.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
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
        else if (change.Property == SizeProperty)
        {
            Rebuild();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!CanInteract || _segments.Count == 0)
        {
            return;
        }

        var index = ActiveIndex();
        if (InteractionAssist.IsActivationKey(e.Key))
        {
            SelectIndex(index, focus: true);
            e.Handled = true;
        }
        else if (InteractionAssist.IsIncrementKey(e.Key))
        {
            MoveSelection(index, 1);
            e.Handled = true;
        }
        else if (InteractionAssist.IsDecrementKey(e.Key))
        {
            MoveSelection(index, -1);
            e.Handled = true;
        }
        else if (e.Key == Key.Home)
        {
            SelectIndex(0, focus: true);
            e.Handled = true;
        }
        else if (e.Key == Key.End)
        {
            SelectIndex(_segments.Count - 1, focus: true);
            e.Handled = true;
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
            var index = i;
            var segment = new Button
            {
                Name = "PART_Segment",
                Content = item.Text,
                Color = ResolvedColor(),
                Size = Size,
                CornerRadius = CornerFor(i, Items.Count),
                Margin = OverlapFor(i),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Focusable = true,
            };

            var captured = item;
            InteractionAssist.SetAutomationName(segment, captured.Text, captured.Value);
            segment.Click += (_, _) =>
            {
                if (!CanInteract)
                {
                    return;
                }

                SelectIndex(index, focus: true);
            };
            segment.KeyDown += (_, args) =>
            {
                if (!CanInteract)
                {
                    return;
                }

                if (InteractionAssist.IsActivationKey(args.Key))
                {
                    SelectIndex(index, focus: true);
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
                else if (args.Key == Key.Home)
                {
                    SelectIndex(0, focus: true);
                    args.Handled = true;
                }
                else if (args.Key == Key.End)
                {
                    SelectIndex(_segments.Count - 1, focus: true);
                    args.Handled = true;
                }
            };

            _segments.Add(new SegmentState(segment, item));
            _items.Children.Add(segment);
        }

        ApplyEnabledState();
        UpdateSelection();
        UpdateIntrinsicSize();
        InvalidateMeasure();
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var desired = base.MeasureOverride(availableSize);
        return new Size(Math.Max(desired.Width, _intrinsicMinWidth), Math.Max(desired.Height, _intrinsicMinHeight));
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arrangedSize = new Size(Math.Max(finalSize.Width, _intrinsicMinWidth), Math.Max(finalSize.Height, _intrinsicMinHeight));
        return base.ArrangeOverride(arrangedSize);
    }

    private void UpdateSelection()
    {
        var color = ResolvedColor();
        for (var i = 0; i < _segments.Count; i++)
        {
            var state = _segments[i];
            var selected = Equals(state.Item.Value, SelectedValue);
            state.Button.Color = color;
            state.Button.Size = Size;
            state.Button.CornerRadius = CornerFor(i, _segments.Count);
            state.Button.Margin = OverlapFor(i);
            state.Button.Variant = selected ? Variant.Filled : Variant.Outlined;
            AutomationProperties.SetHelpText(state.Button, selected ? "Toggle option selected" : "Toggle option");
        }

        ApplyAutomation();
        UpdateIntrinsicSize();
        InvalidateMeasure();
    }

    private void MoveSelection(int currentIndex, int direction)
    {
        if (!CanInteract || _segments.Count == 0)
        {
            return;
        }

        var selectedIndex = _segments.FindIndex(segment => Equals(segment.Item.Value, SelectedValue));
        var origin = selectedIndex >= 0 ? selectedIndex : currentIndex;
        var next = Math.Clamp(origin + direction, 0, _segments.Count - 1);
        SelectIndex(next, focus: true);
    }

    private void SelectIndex(int index, bool focus)
    {
        if (index < 0 || index >= _segments.Count)
        {
            return;
        }

        SelectedValue = _segments[index].Item.Value;
        if (focus)
        {
            _segments[index].Button.Focus();
        }
    }

    private int ActiveIndex()
    {
        var focused = _segments.FindIndex(segment => segment.Button.IsFocused);
        if (focused >= 0)
        {
            return focused;
        }

        var selected = _segments.FindIndex(segment => Equals(segment.Item.Value, SelectedValue));
        return selected >= 0 ? selected : 0;
    }

    private bool CanInteract => IsEnabled;

    private void ApplyEnabledState()
    {
        Opacity = 1;
        Cursor = CanInteract ? new Cursor(StandardCursorType.Hand) : Cursor.Default;

        foreach (var segment in _segments)
        {
            segment.Button.IsEnabled = IsEnabled;
            segment.Button.Cursor = CanInteract ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
        }
    }

    private void ApplyAutomation()
    {
        InteractionAssist.SetAutomationName(this, "Toggle group");
        var selected = _segments.FirstOrDefault(segment => Equals(segment.Item.Value, SelectedValue));
        AutomationProperties.SetHelpText(this,
            selected is null ? "No toggle option selected" : $"Selected {selected.Item.Text ?? selected.Item.Value}");
    }

    private void UpdateIntrinsicSize()
    {
        if (_items is null || _segments.Count == 0)
        {
            _intrinsicMinWidth = 0;
            _intrinsicMinHeight = 0;
            return;
        }

        var unconstrained = new Size(double.PositiveInfinity, double.PositiveInfinity);
        var width = 0d;
        var height = 0d;
        foreach (var state in _segments)
        {
            state.Button.Measure(unconstrained);
            var desired = state.Button.DesiredSize;
            width += desired.Width;
            height = Math.Max(height, desired.Height);
        }

        width -= Math.Max(0, _segments.Count - 1);
        _intrinsicMinWidth = Math.Ceiling(Math.Max(0, width));
        _intrinsicMinHeight = Math.Ceiling(Math.Max(0, height));
        _items.MinWidth = _intrinsicMinWidth;
        _items.MinHeight = _intrinsicMinHeight;
    }

    private LoamColor ResolvedColor() => Color is LoamColor.Default or LoamColor.Inherit ? LoamColor.Primary : Color;

    private static CornerRadius CornerFor(int index, int count)
    {
        if (count <= 1)
        {
            return new CornerRadius(Radius);
        }

        var first = index == 0;
        var last = index == count - 1;
        return new CornerRadius(first ? Radius : 0, last ? Radius : 0, last ? Radius : 0, first ? Radius : 0);
    }

    private static Thickness OverlapFor(int index) => index == 0 ? default : new Thickness(-1, 0, 0, 0);

    private sealed record SegmentState(Button Button, ToggleItem Item);
}
