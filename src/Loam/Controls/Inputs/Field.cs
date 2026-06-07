using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A field shell for custom input content. It provides the same label, helper/error text,
/// Text/Filled/Outlined chrome, focus accent, and adornment slots as the field-style inputs
/// while leaving the editable content to the caller.
/// </summary>
public class Field : ContentControl
{
    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<Field, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<Field, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<Field, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<Field, Variant>(nameof(Variant), Loam.Variant.Outlined);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Field, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Error"/> property.</summary>
    public static readonly StyledProperty<bool> ErrorProperty =
        AvaloniaProperty.Register<Field, bool>(nameof(Error));

    /// <summary>Identifies the <see cref="StartAdornment"/> property.</summary>
    public static readonly StyledProperty<object?> StartAdornmentProperty =
        AvaloniaProperty.Register<Field, object?>(nameof(StartAdornment));

    /// <summary>Identifies the <see cref="EndAdornment"/> property.</summary>
    public static readonly StyledProperty<object?> EndAdornmentProperty =
        AvaloniaProperty.Register<Field, object?>(nameof(EndAdornment));

    /// <summary>Identifies the <see cref="InnerPadding"/> property.</summary>
    public static readonly StyledProperty<bool> InnerPaddingProperty =
        AvaloniaProperty.Register<Field, bool>(nameof(InnerPadding), true);

    private Border? _inputBorder;
    private Border? _labelHost;
    private Text? _label;
    private Text? _helper;
    private ContentPresenter? _startAdornment;
    private ContentPresenter? _endAdornment;
    private bool _focused;
    private IDisposable? _borderBrush;
    private IDisposable? _background;
    private IDisposable? _labelForeground;
    private IDisposable? _helperForeground;

    /// <summary>Creates the field shell.</summary>
    public Field()
    {
        Focusable = true;
        GotFocus += OnFocusChanged;
        LostFocus += OnFocusChanged;
    }

    /// <summary>The field label.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
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

    /// <summary>Optional content shown before the custom content.</summary>
    public object? StartAdornment
    {
        get => GetValue(StartAdornmentProperty);
        set => SetValue(StartAdornmentProperty, value);
    }

    /// <summary>Optional content shown after the custom content.</summary>
    public object? EndAdornment
    {
        get => GetValue(EndAdornmentProperty);
        set => SetValue(EndAdornmentProperty, value);
    }

    /// <summary>Whether the field applies standard internal padding around its content.</summary>
    public bool InnerPadding
    {
        get => GetValue(InnerPaddingProperty);
        set => SetValue(InnerPaddingProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Field);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _inputBorder = e.NameScope.Find("PART_InputBorder") as Border;
        _labelHost = e.NameScope.Find("PART_LabelHost") as Border;
        _label = e.NameScope.Find("PART_Label") as Text;
        _helper = e.NameScope.Find("PART_HelperText") as Text;
        _startAdornment = e.NameScope.Find("PART_StartAdornment") as ContentPresenter;
        _endAdornment = e.NameScope.Find("PART_EndAdornment") as ContentPresenter;

        ApplyLabels();
        ApplyAdornments();
        ApplyChrome();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == VariantProperty || change.Property == ColorProperty ||
            change.Property == ErrorProperty || change.Property == IsEnabledProperty ||
            change.Property == InnerPaddingProperty)
        {
            ApplyChrome();
        }

        if (change.Property == LabelProperty || change.Property == HelperTextProperty ||
            change.Property == ErrorTextProperty || change.Property == ErrorProperty ||
            change.Property == ContentProperty)
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
        Dispatcher.UIThread.Post(() =>
        {
            _focused = HasFocusWithin();
            ApplyChrome();
        });
    }

    private bool HasFocusWithin() =>
        IsFocused || this.GetVisualDescendants().OfType<InputElement>().Any(element => element.IsFocused);

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
        FieldChrome.ApplyLabelLayout(this, _inputBorder, _labelHost, _label?.IsVisible == true, Variant);

        if (_helper is not null)
        {
            var text = Error && !string.IsNullOrEmpty(ErrorText) ? ErrorText : HelperText;
            _helper.Text = text;
            _helper.IsVisible = !string.IsNullOrEmpty(text);
            _helperForeground?.Dispose();
            _helperForeground = _helper.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(muted));
        }

        InteractionAssist.SetAutomationName(this, Label, Content, HelperText);
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

        var padding = InnerPadding ? (Thickness?)null : new Thickness(0);
        FieldChrome.Apply(this, _inputBorder, Variant, Color, Error, _focused, IsEnabled,
            ref _borderBrush, ref _background,
            textPadding: padding,
            filledPadding: padding,
            outlinedPadding: padding);
    }
}
