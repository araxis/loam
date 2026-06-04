using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A Material text input, mirroring the reference API's <c>TextField</c>. Wraps an Avalonia
/// <see cref="TextBox"/> with an optional <see cref="Label"/>, <see cref="HelperText"/>/error text,
/// and Text/Filled/Outlined variant chrome that highlights in <see cref="Color"/> on focus and in
/// the error color when <see cref="Error"/> is set.
/// </summary>
public class TextField : TemplatedControl
{
    /// <summary>Identifies the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TextField, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<TextField, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<TextField, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="Placeholder"/> property.</summary>
    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<TextField, string?>(nameof(Placeholder));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<TextField, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<TextField, Variant>(nameof(Variant), Loam.Variant.Outlined);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<TextField, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Error"/> property.</summary>
    public static readonly StyledProperty<bool> ErrorProperty =
        AvaloniaProperty.Register<TextField, bool>(nameof(Error));

    /// <summary>Identifies the <see cref="ReadOnly"/> property.</summary>
    public static readonly StyledProperty<bool> ReadOnlyProperty =
        AvaloniaProperty.Register<TextField, bool>(nameof(ReadOnly));

    /// <summary>Identifies the <see cref="Required"/> property.</summary>
    public static readonly StyledProperty<bool> RequiredProperty =
        AvaloniaProperty.Register<TextField, bool>(nameof(Required));

    /// <summary>Identifies the <see cref="Validation"/> property.</summary>
    public static readonly StyledProperty<Func<string?, string?>?> ValidationProperty =
        AvaloniaProperty.Register<TextField, Func<string?, string?>?>(nameof(Validation));

    private TextBox? _textBox;
    private Border? _inputBorder;
    private Text? _label;
    private Text? _helper;
    private bool _focused;
    private IDisposable? _borderBrush;
    private IDisposable? _background;
    private IDisposable? _labelForeground;
    private IDisposable? _helperForeground;

    /// <summary>The text value (two-way). Mirrors the reference API's <c>Text</c>.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    /// <summary>Placeholder shown when empty. Mirrors the reference API's <c>Placeholder</c>.</summary>
    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
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

    /// <summary>Whether the field is read-only. Mirrors the reference API's <c>ReadOnly</c>.</summary>
    public bool ReadOnly
    {
        get => GetValue(ReadOnlyProperty);
        set => SetValue(ReadOnlyProperty, value);
    }

    /// <summary>Marks the field required (non-empty). Mirrors the reference API's <c>Required</c>.</summary>
    public bool Required
    {
        get => GetValue(RequiredProperty);
        set => SetValue(RequiredProperty, value);
    }

    /// <summary>A validator returning an error message (or null when valid) for the current text.</summary>
    public Func<string?, string?>? Validation
    {
        get => GetValue(ValidationProperty);
        set => SetValue(ValidationProperty, value);
    }

    /// <summary>Runs validation, updates <see cref="Error"/>/<see cref="ErrorText"/>, and returns the error (or null).</summary>
    public string? Validate()
    {
        string? error = null;
        if (Required && string.IsNullOrWhiteSpace(Text))
        {
            error = "Required";
        }
        else if (Validation is { } validate)
        {
            error = validate(Text);
        }

        Error = error is not null;
        ErrorText = error;
        return error;
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TextField);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _textBox = e.NameScope.Find("PART_TextBox") as TextBox;
        _inputBorder = e.NameScope.Find("PART_InputBorder") as Border;
        _label = e.NameScope.Find("PART_Label") as Text;
        _helper = e.NameScope.Find("PART_HelperText") as Text;

        if (_textBox is not null)
        {
            _textBox.Bind(TextBox.TextProperty, new Binding(nameof(Text)) { Source = this, Mode = BindingMode.TwoWay });
            _textBox.Bind(TextBox.PlaceholderTextProperty, new Binding(nameof(Placeholder)) { Source = this });
            _textBox.Bind(TextBox.IsReadOnlyProperty, new Binding(nameof(ReadOnly)) { Source = this });
            _textBox.GotFocus += OnFocusChanged;
            _textBox.LostFocus += OnFocusChanged;
        }

        ApplyLabels();
        ApplyChrome();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == VariantProperty || change.Property == ColorProperty ||
            change.Property == ErrorProperty || change.Property == IsEnabledProperty)
        {
            ApplyChrome();
        }

        if (change.Property == LabelProperty || change.Property == HelperTextProperty ||
            change.Property == ErrorTextProperty || change.Property == ErrorProperty)
        {
            ApplyLabels();
        }
    }

    private void OnFocusChanged(object? sender, RoutedEventArgs e)
    {
        _focused = _textBox?.IsFocused == true;
        if (!_focused && (Required || Validation is not null))
        {
            Validate();
        }

        ApplyChrome();
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

        var accent = Color is LoamColor.Default or LoamColor.Inherit
            ? LoamTokens.Primary
            : LoamTokens.Palette(Color.ToPaletteName()!);
        var brushKey = Error ? LoamTokens.Error
            : _focused ? accent
            : LoamTokens.Palette(nameof(LoamPalette.LinesInputs));
        var emphasized = _focused || Error;

        _borderBrush?.Dispose();
        _borderBrush = _inputBorder.Bind(Border.BorderBrushProperty, this.GetResourceObservable(brushKey));

        _background?.Dispose();
        _background = null;

        switch (Variant)
        {
            case Loam.Variant.Filled:
                _inputBorder.CornerRadius = new CornerRadius(4, 4, 0, 0);
                _inputBorder.BorderThickness = new Thickness(0, 0, 0, emphasized ? 2 : 1);
                _inputBorder.Padding = new Thickness(12, 8);
                _background = _inputBorder.Bind(Border.BackgroundProperty,
                    this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.ActionDisabledBackground))));
                break;
            case Loam.Variant.Text:
                _inputBorder.CornerRadius = default;
                _inputBorder.BorderThickness = new Thickness(0, 0, 0, emphasized ? 2 : 1);
                _inputBorder.Padding = new Thickness(0, 6);
                _inputBorder.Background = Brushes.Transparent;
                break;
            default: // Outlined
                _inputBorder.CornerRadius = new CornerRadius(4);
                _inputBorder.BorderThickness = new Thickness(emphasized ? 2 : 1);
                _inputBorder.Padding = new Thickness(12, 8);
                _inputBorder.Background = Brushes.Transparent;
                break;
        }
    }
}
