using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>An option in a <see cref="Select"/>, mirroring the reference API's <c>SelectItem</c>.</summary>
public sealed class SelectItem
{
    /// <summary>Creates an empty option.</summary>
    public SelectItem()
    {
    }

    /// <summary>Creates an option with display text and value.</summary>
    public SelectItem(string text, object? value)
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
/// A dropdown selector, mirroring the reference API's <c>Select</c>. An outlined field showing the chosen
/// option that opens a flyout list of <see cref="Items"/>; the chosen value is <see cref="Value"/> (two-way).
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
    Justification = "Mirrors the reference API's Select with the Loam convention of dropping the legacy prefix.")]
public class Select : TemplatedControl
{
    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<Select, object?>(nameof(Value), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<Select, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="Placeholder"/> property.</summary>
    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<Select, string?>(nameof(Placeholder));

    /// <summary>Identifies the <see cref="MultiSelect"/> property.</summary>
    public static readonly StyledProperty<bool> MultiSelectProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(MultiSelect));

    private Border? _box;
    private Text? _display;
    private Text? _label;
    private IDisposable? _displayForeground;
    private Flyout? _flyout;

    /// <summary>Creates the select.</summary>
    public Select()
    {
        Focusable = true;
        Items.CollectionChanged += (_, _) => UpdateDisplay();
        SelectedValues.CollectionChanged += (_, _) => UpdateDisplay();
    }

    /// <summary>The selectable options.</summary>
    public ObservableCollection<SelectItem> Items { get; } = new();

    /// <summary>Selected values when <see cref="MultiSelect"/> is enabled.</summary>
    public ObservableCollection<object?> SelectedValues { get; } = new();

    /// <summary>Optional display text formatter for options.</summary>
    public Func<SelectItem, string>? DisplayTextFunc { get; set; }

    /// <summary>Optional row content factory for flyout items.</summary>
    public Func<SelectItem, Control>? ItemTemplate { get; set; }

    /// <summary>The selected value (two-way). Mirrors the reference API's <c>Value</c>.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The field label. Mirrors the reference API's <c>Label</c>.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Text shown when nothing is selected. Mirrors the reference API's <c>Placeholder</c>.</summary>
    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Whether multiple values can be selected.</summary>
    public bool MultiSelect
    {
        get => GetValue(MultiSelectProperty);
        set => SetValue(MultiSelectProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Select);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _box = e.NameScope.Find("PART_Box") as Border;
        _display = e.NameScope.Find("PART_Display") as Text;
        _label = e.NameScope.Find("PART_Label") as Text;
        if (_box is not null)
        {
            _box.PointerPressed += (_, _) =>
            {
                Focus();
                Open();
            };
        }

        UpdateLabel();
        UpdateDisplay();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || change.Property == MultiSelectProperty ||
            change.Property == PlaceholderProperty)
        {
            UpdateDisplay();
        }
        else if (change.Property == LabelProperty)
        {
            UpdateLabel();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (InteractionAssist.IsActivationKey(e.Key))
        {
            Open();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            _flyout?.Hide();
            e.Handled = true;
        }
    }

    private void Open()
    {
        var list = new StackPanel();
        foreach (var item in Items)
        {
            var row = new ListItem { Content = BuildItemContent(item), MinWidth = 180 };
            var captured = item;
            row.PointerPressed += (_, _) =>
            {
                if (MultiSelect)
                {
                    ToggleSelectedValue(captured.Value);
                    row.Content = BuildItemContent(captured);
                }
                else
                {
                    Value = captured.Value;
                    _flyout?.Hide();
                }
            };
            list.Children.Add(row);
        }

        _flyout = new Flyout
        {
            Content = new Paper { Elevation = 8, Padding = new Thickness(0, 8), Content = list },
            Placement = PlacementMode.BottomEdgeAlignedLeft,
        };
        _flyout.ShowAt(_box ?? (Control)this);
    }

    private void UpdateLabel()
    {
        if (_label is not null)
        {
            _label.Text = Label;
            _label.IsVisible = !string.IsNullOrEmpty(Label);
        }

        InteractionAssist.SetAutomationName(this, Label, _display?.Text, Placeholder);
    }

    private void UpdateDisplay()
    {
        if (_display is null)
        {
            return;
        }

        var selected = Items.FirstOrDefault(i => Equals(i.Value, Value));
        var text = MultiSelect ? MultiSelectText() : selected is not null ? DisplayText(selected) : Value?.ToString();
        _display.Text = string.IsNullOrEmpty(text) ? Placeholder : text;
        _displayForeground?.Dispose();
        _displayForeground = _display.Bind(TextBlock.ForegroundProperty,
            this.GetResourceObservable(!string.IsNullOrEmpty(text) ? LoamTokens.TextPrimary : LoamTokens.TextSecondary));
        InteractionAssist.SetAutomationName(this, Label, _display.Text, Placeholder);
    }

    private Control BuildItemContent(SelectItem item)
    {
        if (ItemTemplate is not null)
        {
            return ItemTemplate(item);
        }

        var selected = MultiSelect && SelectedValues.Any(value => Equals(value, item.Value));
        return new Text
        {
            Text = selected ? $"[x] {DisplayText(item)}" : DisplayText(item),
            Color = selected ? LoamColor.Primary : LoamColor.Inherit,
        };
    }

    private string DisplayText(SelectItem item) => DisplayTextFunc?.Invoke(item) ?? item.Text ?? item.Value?.ToString() ?? string.Empty;

    private string? MultiSelectText()
    {
        if (SelectedValues.Count == 0)
        {
            return null;
        }

        return string.Join(", ", SelectedValues.Select(value =>
            Items.FirstOrDefault(item => Equals(item.Value, value)) is { } item ? DisplayText(item) : value?.ToString()));
    }

    private void ToggleSelectedValue(object? value)
    {
        var existing = SelectedValues.FirstOrDefault(selected => Equals(selected, value));
        var hasExisting = SelectedValues.Any(selected => Equals(selected, value));
        if (hasExisting)
        {
            SelectedValues.Remove(existing);
        }
        else
        {
            SelectedValues.Add(value);
        }
    }
}
