using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A numeric input with spinner buttons, mirroring the reference API's <c>NumericField</c>. Shares the
/// <see cref="TextField"/> Material chrome (label, variant border, helper/error) and adds
/// <see cref="Minimum"/>/<see cref="Maximum"/> clamping, <see cref="Step"/> increment/decrement, and
/// optional <see cref="Format"/>ting of the two-way <see cref="Value"/>.
/// </summary>
public class NumericField : TemplatedControl
{
    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<NumericField, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Minimum"/> property.</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<NumericField, double>(nameof(Minimum), double.MinValue);

    /// <summary>Identifies the <see cref="Maximum"/> property.</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<NumericField, double>(nameof(Maximum), double.MaxValue);

    /// <summary>Identifies the <see cref="Step"/> property.</summary>
    public static readonly StyledProperty<double> StepProperty =
        AvaloniaProperty.Register<NumericField, double>(nameof(Step), 1);

    /// <summary>Identifies the <see cref="Format"/> property.</summary>
    public static readonly StyledProperty<string?> FormatProperty =
        AvaloniaProperty.Register<NumericField, string?>(nameof(Format));

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<NumericField, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<NumericField, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<NumericField, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<NumericField, Variant>(nameof(Variant), Loam.Variant.Outlined);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<NumericField, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Error"/> property.</summary>
    public static readonly StyledProperty<bool> ErrorProperty =
        AvaloniaProperty.Register<NumericField, bool>(nameof(Error));

    private TextBox? _textBox;
    private Border? _inputBorder;
    private Border? _labelHost;
    private Text? _label;
    private Text? _helper;
    private Control? _up;
    private Control? _down;
    private bool _focused;
    private bool _updatingText;
    private IDisposable? _borderBrush;
    private IDisposable? _background;
    private IDisposable? _labelForeground;
    private IDisposable? _helperForeground;

    /// <summary>The numeric value (two-way). Mirrors the reference API's <c>Value</c>.</summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Smallest allowed value. Mirrors the reference API's <c>Min</c>.</summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Largest allowed value. Mirrors the reference API's <c>Max</c>.</summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Increment/decrement step. Mirrors the reference API's <c>Step</c>.</summary>
    public double Step
    {
        get => GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>A .NET numeric format string for display. Mirrors the reference API's <c>Format</c>.</summary>
    public string? Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    /// <summary>The field label. Mirrors the reference API's <c>Label</c>.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Helper text shown below the field. Mirrors the reference API's <c>HelperText</c>.</summary>
    public string? HelperText
    {
        get => GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Error message shown instead of helper text when <see cref="Error"/>. Mirrors the reference API's <c>ErrorText</c>.</summary>
    public string? ErrorText
    {
        get => GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>Visual style. Mirrors the reference API's <c>Variant</c>.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Focus accent color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Whether the field is in an error state. Mirrors the reference API's <c>Error</c>.</summary>
    public bool Error
    {
        get => GetValue(ErrorProperty);
        set => SetValue(ErrorProperty, value);
    }

    /// <summary>Clamps <paramref name="value"/> to the <paramref name="min"/>/<paramref name="max"/> range.</summary>
    public static double Clamp(double value, double min, double max) =>
        max < min ? value : Math.Clamp(value, min, max);

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NumericField);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _textBox = e.NameScope.Find("PART_TextBox") as TextBox;
        _inputBorder = e.NameScope.Find("PART_InputBorder") as Border;
        _labelHost = e.NameScope.Find("PART_LabelHost") as Border;
        _label = e.NameScope.Find("PART_Label") as Text;
        _helper = e.NameScope.Find("PART_HelperText") as Text;
        _up = e.NameScope.Find("PART_Up") as Control;
        _down = e.NameScope.Find("PART_Down") as Control;

        if (_textBox is not null)
        {
            FieldChrome.ResetInnerTextBox(_textBox);
            _textBox.GotFocus += OnFocusChanged;
            _textBox.LostFocus += OnFocusChanged;
            _textBox.TextChanged += OnTextChanged;
        }

        if (_up is not null)
        {
            _up.PointerPressed += (_, _) => Bump(Step);
        }

        if (_down is not null)
        {
            _down.PointerPressed += (_, _) => Bump(-Step);
        }

        UpdateText();
        ApplyLabels();
        ApplyChrome();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            var clamped = Clamp(Value, Minimum, Maximum);
            if (!_updatingText && clamped != Value)
            {
                Value = clamped;
                return;
            }

            UpdateText();
        }
        else if (change.Property == FormatProperty)
        {
            UpdateText();
        }
        else if (change.Property == VariantProperty || change.Property == ColorProperty ||
                 change.Property == ErrorProperty || change.Property == IsEnabledProperty)
        {
            ApplyChrome();
        }
        else if (change.Property == LabelProperty || change.Property == HelperTextProperty ||
                 change.Property == ErrorTextProperty)
        {
            ApplyLabels();
        }
    }

    private void Bump(double delta)
    {
        if (!IsEnabled)
        {
            return;
        }

        Value = Clamp(Value + delta, Minimum, Maximum);
        _textBox?.Focus();
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_updatingText || _textBox is null)
        {
            return;
        }

        if (double.TryParse(_textBox.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var parsed))
        {
            _updatingText = true;
            Value = Clamp(parsed, Minimum, Maximum);
            _updatingText = false;
        }
    }

    private void OnFocusChanged(object? sender, RoutedEventArgs e)
    {
        _focused = _textBox?.IsFocused == true;
        if (_textBox is not null)
        {
            FieldChrome.ResetInnerTextBox(_textBox);
        }

        if (!_focused)
        {
            UpdateText();
        }

        ApplyChrome();
    }

    private void UpdateText()
    {
        if (_textBox is null || _updatingText)
        {
            return;
        }

        _updatingText = true;
        _textBox.Text = Value.ToString(Format, CultureInfo.CurrentCulture);
        _updatingText = false;
    }

    private void ApplyLabels()
    {
        var muted = Error ? LoamTokens.Error : LoamTokens.TextSecondary;

        if (_label is not null)
        {
            _label.Text = Label;
            _label.IsVisible = !string.IsNullOrEmpty(Label);
            _labelForeground?.Dispose();
            _labelForeground = _label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(muted));
        }
        FieldChrome.ApplyLabelLayout(_inputBorder, _labelHost, _label?.IsVisible == true);

        if (_helper is not null)
        {
            var text = Error && !string.IsNullOrEmpty(ErrorText) ? ErrorText : HelperText;
            _helper.Text = text;
            _helper.IsVisible = !string.IsNullOrEmpty(text);
            _helperForeground?.Dispose();
            _helperForeground = _helper.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(muted));
        }
    }

    private void ApplyChrome()
    {
        Opacity = IsEnabled ? 1 : 0.5;
        if (_inputBorder is null)
        {
            return;
        }

        FieldChrome.Apply(this, _inputBorder, Variant, Color, Error, _focused, IsEnabled,
            ref _borderBrush, ref _background,
            outlinedPadding: new Thickness(12, 10, 4, 10));
    }
}
