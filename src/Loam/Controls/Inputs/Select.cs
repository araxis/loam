using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<Select, Variant>(nameof(Variant), Loam.Variant.Outlined);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Select, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Error"/> property.</summary>
    public static readonly StyledProperty<bool> ErrorProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(Error));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<Select, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<Select, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="ShrinkLabel"/> property.</summary>
    public static readonly StyledProperty<bool> ShrinkLabelProperty =
        AvaloniaProperty.Register<Select, bool>(nameof(ShrinkLabel));

    private Border? _box;
    private Border? _labelHost;
    private Text? _display;
    private Text? _label;
    private Text? _restingLabel;
    private Text? _helper;
    private IDisposable? _displayForeground;
    private IDisposable? _boxBorderBrush;
    private IDisposable? _boxBackground;
    private IDisposable? _labelForeground;
    private IDisposable? _restingLabelForeground;
    private IDisposable? _helperForeground;
    private Popup? _popup;

    /// <summary>Creates the select.</summary>
    public Select()
    {
        Focusable = true;
        GotFocus += (_, _) => ApplyBoxChrome();
        LostFocus += (_, _) => ApplyBoxChrome();
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

    /// <summary>Visual chrome style.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Focus accent color.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Whether the field is in an error state.</summary>
    public bool Error
    {
        get => GetValue(ErrorProperty);
        set => SetValue(ErrorProperty, value);
    }

    /// <summary>Helper text shown below the field.</summary>
    public string? HelperText
    {
        get => GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Error message shown instead of helper text when <see cref="Error"/>.</summary>
    public string? ErrorText
    {
        get => GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>When true, the label stays floated above the field even when empty and unfocused.</summary>
    public bool ShrinkLabel
    {
        get => GetValue(ShrinkLabelProperty);
        set => SetValue(ShrinkLabelProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Select);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _box = e.NameScope.Find("PART_Box") as Border;
        _labelHost = e.NameScope.Find("PART_LabelHost") as Border;
        _popup = e.NameScope.Find("PART_Popup") as Popup;
        _display = e.NameScope.Find("PART_Display") as Text;
        _label = e.NameScope.Find("PART_Label") as Text;
        _restingLabel = e.NameScope.Find("PART_RestingLabel") as Text;
        _helper = e.NameScope.Find("PART_HelperText") as Text;
        if (_popup is not null)
        {
            _popup.Closed += (_, _) => ApplyBoxChrome();
        }

        if (_box is not null)
        {
            _box.GotFocus += (_, _) => ApplyBoxChrome();
            _box.LostFocus += (_, _) => ApplyBoxChrome();
            _box.PointerPressed += (_, args) =>
            {
                Focus();
                Open();
                args.Handled = true;
            };
        }

        UpdateLabel();
        UpdateDisplay();
        ApplyBoxChrome();
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
        else if (change.Property == LabelProperty || change.Property == ShrinkLabelProperty ||
                 change.Property == HelperTextProperty || change.Property == ErrorTextProperty ||
                 change.Property == ErrorProperty)
        {
            UpdateLabel();
        }
        else if (change.Property == VariantProperty || change.Property == ColorProperty ||
                 change.Property == IsEnabledProperty)
        {
            ApplyBoxChrome();
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
            Close();
            e.Handled = true;
        }
    }

    private void Open()
    {
        if (!IsEnabled || _popup is null)
        {
            return;
        }

        var list = new StackPanel();
        foreach (var item in Items)
        {
            var row = BuildItemRow(item);
            var captured = item;
            void Choose()
            {
                if (MultiSelect)
                {
                    ToggleSelectedValue(captured.Value);
                    ApplyItemRowState(row, captured);
                }
                else
                {
                    Value = captured.Value;
                    Close();
                }
            }

            PopupSurface.ActivateListRowOnPointer(row, Choose);
            row.Activated += (_, _) => Choose();
            row.KeyDown += (_, args) =>
            {
                if (args.Key == Key.Escape)
                {
                    Close();
                    args.Handled = true;
                }
            };
            list.Children.Add(row);
        }

        _popup.Child = PopupSurface.MenuPaper(list, Math.Max(180, _box?.Bounds.Width ?? 0));
        _popup.IsOpen = true;
        ApplyBoxChrome();
    }

    private void Close()
    {
        if (_popup is not null)
        {
            _popup.IsOpen = false;
        }
        ApplyBoxChrome();
    }

    private void UpdateLabel()
    {
        var labelForeground = LabelForegroundKey();
        var helperForeground = Error ? LoamTokens.Error : LoamTokens.TextSecondary;
        var hasLabel = !string.IsNullOrEmpty(Label);
        var active = IsFocused || _box?.IsFocused == true || _popup?.IsOpen == true;
        var floating = hasLabel && (ShrinkLabel || active || HasSelection());
        var resting = hasLabel && !floating;

        if (_label is not null)
        {
            _label.Text = Label;
            _label.IsVisible = floating;
            _labelForeground?.Dispose();
            _labelForeground = _label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(labelForeground));
        }

        if (_restingLabel is not null)
        {
            _restingLabel.Text = Label;
            _restingLabel.IsVisible = resting;
            _restingLabelForeground?.Dispose();
            _restingLabelForeground = _restingLabel.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(labelForeground));
        }

        if (_display is not null)
        {
            _display.IsVisible = !resting;
        }

        FieldChrome.ApplyLabelLayout(this, _box, _labelHost, floating, Variant);

        if (_helper is not null)
        {
            var text = Error && !string.IsNullOrEmpty(ErrorText) ? ErrorText : HelperText;
            _helper.Text = text;
            _helper.IsVisible = !string.IsNullOrEmpty(text);
            _helperForeground?.Dispose();
            _helperForeground = _helper.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(helperForeground));
        }

        InteractionAssist.SetAutomationName(this, Label, _display?.Text, Placeholder);
    }

    private void ApplyBoxChrome()
    {
        if (_box is null)
        {
            return;
        }

        var active = IsFocused || _box.IsFocused || _popup?.IsOpen == true;
        FieldChrome.Apply(this, _box, Variant, Color, Error, active, IsEnabled,
            ref _boxBorderBrush, ref _boxBackground);
        UpdateLabel();
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
        UpdateLabel();
    }

    private ListItem BuildItemRow(SelectItem item)
    {
        var row = new ListItem { MinWidth = 180 };
        ApplyItemRowState(row, item);
        return row;
    }

    private void ApplyItemRowState(ListItem row, SelectItem item)
    {
        if (ItemTemplate is not null)
        {
            row.Content = ItemTemplate(item);
        }
        else
        {
            row.Content = DisplayText(item);
        }

        var selected = IsItemSelected(item);
        row.Icon = selected ? Icons.Material.Filled.Check : null;
        row.IsSelected = selected;
        InteractionAssist.SetAutomationName(row, DisplayText(item));
    }

    private bool IsItemSelected(SelectItem item) =>
        MultiSelect
            ? SelectedValues.Any(value => Equals(value, item.Value))
            : Equals(Value, item.Value);

    private string DisplayText(SelectItem item) => DisplayTextFunc?.Invoke(item) ?? item.Text ?? item.Value?.ToString() ?? string.Empty;

    private bool HasSelection()
    {
        if (MultiSelect)
        {
            return SelectedValues.Count > 0;
        }

        return Value is not null || Items.Any(i => Equals(i.Value, Value));
    }

    private string LabelForegroundKey()
    {
        if (Error)
        {
            return LoamTokens.Error;
        }

        if (IsFocused || _box?.IsFocused == true || _popup?.IsOpen == true)
        {
            var paletteName = Color.ToPaletteName();
            return paletteName is null ? LoamTokens.Primary : LoamTokens.Palette(paletteName);
        }

        return LoamTokens.TextSecondary;
    }

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
