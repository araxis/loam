using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>An option in a <see cref="Select"/>, mirroring MudBlazor's <c>MudSelectItem</c>.</summary>
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
/// A dropdown selector, mirroring MudBlazor's <c>MudSelect</c>. An outlined field showing the chosen
/// option that opens a flyout list of <see cref="Items"/>; the chosen value is <see cref="Value"/> (two-way).
/// </summary>
[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
    Justification = "Mirrors MudBlazor's MudSelect with the Loam convention of dropping the Mud prefix.")]
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

    private Border? _box;
    private Text? _display;
    private Text? _label;
    private IDisposable? _displayForeground;
    private Flyout? _flyout;

    /// <summary>Creates the select.</summary>
    public Select() => Items.CollectionChanged += (_, _) => UpdateDisplay();

    /// <summary>The selectable options.</summary>
    public ObservableCollection<SelectItem> Items { get; } = new();

    /// <summary>The selected value (two-way). Mirrors MudBlazor's <c>Value</c>.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The field label. Mirrors MudBlazor's <c>Label</c>.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Text shown when nothing is selected. Mirrors MudBlazor's <c>Placeholder</c>.</summary>
    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
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
            _box.PointerPressed += (_, _) => Open();
        }

        UpdateLabel();
        UpdateDisplay();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            UpdateDisplay();
        }
        else if (change.Property == LabelProperty)
        {
            UpdateLabel();
        }
    }

    private void Open()
    {
        var list = new StackPanel();
        foreach (var item in Items)
        {
            var row = new ListItem { Content = item.Text, MinWidth = 180 };
            var captured = item;
            row.PointerPressed += (_, _) =>
            {
                Value = captured.Value;
                _flyout?.Hide();
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
    }

    private void UpdateDisplay()
    {
        if (_display is null)
        {
            return;
        }

        var selected = Items.FirstOrDefault(i => Equals(i.Value, Value));
        _display.Text = selected?.Text ?? Placeholder;
        _displayForeground?.Dispose();
        _displayForeground = _display.Bind(TextBlock.ForegroundProperty,
            this.GetResourceObservable(selected is not null ? LoamTokens.TextPrimary : LoamTokens.TextSecondary));
    }
}
