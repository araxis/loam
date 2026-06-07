using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
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

    /// <summary>Identifies the <see cref="StartAdornment"/> property.</summary>
    public static readonly StyledProperty<object?> StartAdornmentProperty =
        AvaloniaProperty.Register<TextField, object?>(nameof(StartAdornment));

    /// <summary>Identifies the <see cref="EndAdornment"/> property.</summary>
    public static readonly StyledProperty<object?> EndAdornmentProperty =
        AvaloniaProperty.Register<TextField, object?>(nameof(EndAdornment));

    /// <summary>Identifies the <see cref="FloatingLabel"/> property.</summary>
    public static readonly StyledProperty<bool> FloatingLabelProperty =
        AvaloniaProperty.Register<TextField, bool>(nameof(FloatingLabel));

    /// <summary>Identifies the <see cref="ShrinkLabel"/> property.</summary>
    public static readonly StyledProperty<bool> ShrinkLabelProperty =
        AvaloniaProperty.Register<TextField, bool>(nameof(ShrinkLabel));

    private TextBox? _textBox;
    private Border? _inputBorder;
    private Border? _labelHost;
    private Text? _label;
    private Text? _restingLabel;
    private Text? _helper;
    private ContentPresenter? _startAdornment;
    private ContentPresenter? _endAdornment;
    private bool _focused;
    private IDisposable? _borderBrush;
    private IDisposable? _background;
    private IDisposable? _labelForeground;
    private IDisposable? _restingLabelForeground;
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

    /// <summary>Optional content shown before the editable text.</summary>
    public object? StartAdornment
    {
        get => GetValue(StartAdornmentProperty);
        set => SetValue(StartAdornmentProperty, value);
    }

    /// <summary>Optional content shown after the editable text.</summary>
    public object? EndAdornment
    {
        get => GetValue(EndAdornmentProperty);
        set => SetValue(EndAdornmentProperty, value);
    }

    /// <summary>When true, the label is only shown once the field is focused or has a value.</summary>
    public bool FloatingLabel
    {
        get => GetValue(FloatingLabelProperty);
        set => SetValue(FloatingLabelProperty, value);
    }

    /// <summary>When true, the label stays floated above the field even when empty and unfocused.</summary>
    public bool ShrinkLabel
    {
        get => GetValue(ShrinkLabelProperty);
        set => SetValue(ShrinkLabelProperty, value);
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
        _labelHost = e.NameScope.Find("PART_LabelHost") as Border;
        _label = e.NameScope.Find("PART_Label") as Text;
        _restingLabel = e.NameScope.Find("PART_RestingLabel") as Text;
        _helper = e.NameScope.Find("PART_HelperText") as Text;
        _startAdornment = e.NameScope.Find("PART_StartAdornment") as ContentPresenter;
        _endAdornment = e.NameScope.Find("PART_EndAdornment") as ContentPresenter;

        if (_textBox is not null)
        {
            FieldChrome.ResetInnerTextBox(_textBox);
            _textBox.Bind(TextBox.TextProperty, new Binding(nameof(Text)) { Source = this, Mode = BindingMode.TwoWay });
            _textBox.Bind(TextBox.IsReadOnlyProperty, new Binding(nameof(ReadOnly)) { Source = this });
            _textBox.GotFocus += OnFocusChanged;
            _textBox.LostFocus += OnFocusChanged;
        }

        ApplyLabels();
        ApplyAdornments();
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
            change.Property == ErrorTextProperty || change.Property == ErrorProperty ||
            change.Property == FloatingLabelProperty || change.Property == ShrinkLabelProperty ||
            change.Property == PlaceholderProperty || change.Property == TextProperty ||
            change.Property == ColorProperty)
        {
            ApplyLabels();
        }

        if (change.Property == StartAdornmentProperty || change.Property == EndAdornmentProperty)
        {
            ApplyAdornments();
        }
    }

    private void OnFocusChanged(object? sender, RoutedEventArgs e)
    {
        _focused = _textBox?.IsFocused == true;
        if (_textBox is not null)
        {
            FieldChrome.ResetInnerTextBox(_textBox);
        }

        if (!_focused && (Required || Validation is not null))
        {
            Validate();
        }

        ApplyChrome();
        ApplyLabels();
    }

    private void ApplyLabels()
    {
        var labelForeground = LabelForegroundKey();
        var helperForeground = Error ? LoamTokens.Error : LoamTokens.TextSecondary;
        var hasLabel = !string.IsNullOrEmpty(Label);
        var hasText = !string.IsNullOrEmpty(Text);
        var floating = hasLabel && (ShrinkLabel || _focused || hasText);
        var resting = hasLabel && !floating && !FloatingLabel;

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

        if (_textBox is not null)
        {
            _textBox.PlaceholderText = resting ? null : Placeholder;
        }

        FieldChrome.ApplyLabelLayout(this, _inputBorder, _labelHost, floating, Variant);

        if (_helper is not null)
        {
            var text = Error && !string.IsNullOrEmpty(ErrorText) ? ErrorText : HelperText;
            _helper.Text = text;
            _helper.IsVisible = !string.IsNullOrEmpty(text);
            _helperForeground?.Dispose();
            _helperForeground = _helper.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(helperForeground));
        }
    }

    private string LabelForegroundKey()
    {
        if (Error)
        {
            return LoamTokens.Error;
        }

        if (_focused)
        {
            var paletteName = Color.ToPaletteName();
            return paletteName is null ? LoamTokens.Primary : LoamTokens.Palette(paletteName);
        }

        return LoamTokens.TextSecondary;
    }

    private void ApplyAdornments()
    {
        if (_startAdornment is not null)
        {
            _startAdornment.Content = StartAdornment;
            _startAdornment.IsVisible = StartAdornment is not null;
        }

        if (_endAdornment is not null)
        {
            _endAdornment.Content = EndAdornment;
            _endAdornment.IsVisible = EndAdornment is not null;
        }
    }

    private void ApplyChrome()
    {
        if (_inputBorder is null)
        {
            return;
        }

        FieldChrome.Apply(this, _inputBorder, Variant, Color, Error, _focused, IsEnabled,
            ref _borderBrush, ref _background);
    }
}
