using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Loam.Controls.Internal;
using Loam.Theming;
using AvaColor = Avalonia.Media.Color;

namespace Loam.Controls;

/// <summary>
/// A palette color picker, mirroring the reference API's <c>ColorPicker</c> (palette mode). An outlined box
/// shows the current <see cref="Value"/> as a swatch + hex string; clicking opens a flyout of preset
/// swatches from the built-in palette that set <see cref="Value"/>.
/// </summary>
public class ColorPicker : TemplatedControl
{
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    /// <summary>The default palette shown in the flyout.</summary>
    public static readonly IReadOnlyList<AvaColor> DefaultPalette = new[]
    {
        AvaColor.Parse("#F44336"), AvaColor.Parse("#E91E63"), AvaColor.Parse("#9C27B0"), AvaColor.Parse("#673AB7"),
        AvaColor.Parse("#3F51B5"), AvaColor.Parse("#2196F3"), AvaColor.Parse("#03A9F4"), AvaColor.Parse("#00BCD4"),
        AvaColor.Parse("#009688"), AvaColor.Parse("#4CAF50"), AvaColor.Parse("#8BC34A"), AvaColor.Parse("#CDDC39"),
        AvaColor.Parse("#FFEB3B"), AvaColor.Parse("#FFC107"), AvaColor.Parse("#FF9800"), AvaColor.Parse("#FF5722"),
        AvaColor.Parse("#795548"), AvaColor.Parse("#9E9E9E"), AvaColor.Parse("#607D8B"), AvaColor.Parse("#000000"),
    };

    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<AvaColor> ValueProperty =
        AvaloniaProperty.Register<ColorPicker, AvaColor>(nameof(Value), AvaColor.Parse("#594AE2"),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<ColorPicker, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="ShowAlpha"/> property.</summary>
    public static readonly StyledProperty<bool> ShowAlphaProperty =
        AvaloniaProperty.Register<ColorPicker, bool>(nameof(ShowAlpha));

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<ColorPicker, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Error"/> property.</summary>
    public static readonly StyledProperty<bool> ErrorProperty =
        AvaloniaProperty.Register<ColorPicker, bool>(nameof(Error));

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<ColorPicker, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<ColorPicker, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="ShrinkLabel"/> property.</summary>
    public static readonly StyledProperty<bool> ShrinkLabelProperty =
        AvaloniaProperty.Register<ColorPicker, bool>(nameof(ShrinkLabel));

    private Border? _box;
    private Border? _labelHost;
    private Border? _swatch;
    private Text? _hex;
    private Text? _label;
    private Text? _helper;
    private IDisposable? _boxBorderBrush;
    private IDisposable? _boxBackground;
    private IDisposable? _labelForeground;
    private IDisposable? _helperForeground;
    private Flyout? _flyout;

    /// <summary>Creates the picker.</summary>
    public ColorPicker()
    {
        Focusable = true;
        GotFocus += (_, _) => ApplyBoxChrome();
        LostFocus += (_, _) => ApplyBoxChrome();
    }

    /// <summary>The selected color (two-way). Mirrors the reference API's <c>Value</c>.</summary>
    public AvaColor Value
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

    /// <summary>Whether the flyout exposes alpha and the display includes it in the hex value.</summary>
    public bool ShowAlpha
    {
        get => GetValue(ShowAlphaProperty);
        set => SetValue(ShowAlphaProperty, value);
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

    /// <summary>A hue/saturation/value color triple using degrees and unit fractions.</summary>
    public readonly record struct HsvColor(double Hue, double Saturation, double Value);

    /// <summary>Formats a color as an upper-case <c>#RRGGBB</c> string.</summary>
    public static string ToHex(AvaColor color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>Formats a color as an upper-case <c>#AARRGGBB</c> string.</summary>
    public static string ToHexWithAlpha(AvaColor color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>Converts an HSV color to an Avalonia color.</summary>
    public static AvaColor FromHsv(double hue, double saturation, double value, byte alpha = 255)
    {
        var h = NormalizeHue(hue);
        var s = Math.Clamp(saturation, 0d, 1d);
        var v = Math.Clamp(value, 0d, 1d);
        var c = v * s;
        var x = c * (1d - Math.Abs((h / 60d % 2d) - 1d));
        var m = v - c;

        var (r, g, b) = h switch
        {
            < 60d => (c, x, 0d),
            < 120d => (x, c, 0d),
            < 180d => (0d, c, x),
            < 240d => (0d, x, c),
            < 300d => (x, 0d, c),
            _ => (c, 0d, x),
        };

        return AvaColor.FromArgb(alpha, ToByte(r + m), ToByte(g + m), ToByte(b + m));
    }

    /// <summary>Converts an Avalonia color to HSV.</summary>
    public static HsvColor ToHsv(AvaColor color)
    {
        var r = color.R / 255d;
        var g = color.G / 255d;
        var b = color.B / 255d;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        var hue = delta == 0d ? 0d :
            max == r ? 60d * (((g - b) / delta) % 6d) :
            max == g ? 60d * (((b - r) / delta) + 2d) :
            60d * (((r - g) / delta) + 4d);

        return new HsvColor(NormalizeHue(hue), max == 0d ? 0d : delta / max, max);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ColorPicker);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _box = e.NameScope.Find("PART_Box") as Border;
        _labelHost = e.NameScope.Find("PART_LabelHost") as Border;
        _swatch = e.NameScope.Find("PART_Swatch") as Border;
        _hex = e.NameScope.Find("PART_Hex") as Text;
        _label = e.NameScope.Find("PART_Label") as Text;
        _helper = e.NameScope.Find("PART_HelperText") as Text;
        if (_box is not null)
        {
            _box.GotFocus += (_, _) => ApplyBoxChrome();
            _box.LostFocus += (_, _) => ApplyBoxChrome();
            _box.PointerPressed += (_, _) =>
            {
                Focus();
                Open();
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
        if (change.Property == ValueProperty || change.Property == ShowAlphaProperty)
        {
            UpdateDisplay();
        }
        else if (change.Property == LabelProperty || change.Property == ShrinkLabelProperty ||
                 change.Property == HelperTextProperty || change.Property == ErrorTextProperty)
        {
            UpdateLabel();
        }

        if (change.Property == ColorProperty || change.Property == ErrorProperty ||
            change.Property == IsEnabledProperty)
        {
            ApplyBoxChrome();
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
            ApplyBoxChrome();
            e.Handled = true;
        }
    }

    private void Open()
    {
        var grid = new UniformGrid { Columns = 5, Width = 5 * 36 };
        foreach (var color in DefaultPalette)
        {
            var swatch = new Border
            {
                Width = 28,
                Height = 28,
                Margin = new Thickness(3),
                CornerRadius = new CornerRadius(4),
                Background = new ImmutableSolidColorBrush(color),
                Cursor = HandCursor,
            };
            var captured = color;
            swatch.PointerPressed += (_, _) =>
            {
                Value = ShowAlpha
                    ? AvaColor.FromArgb(Value.A, captured.R, captured.G, captured.B)
                    : captured;
                _flyout?.Hide();
                ApplyBoxChrome();
            };
            grid.Children.Add(swatch);
        }

        Control content = grid;
        if (ShowAlpha)
        {
            var alpha = new global::Avalonia.Controls.Slider
            {
                Minimum = 0,
                Maximum = 255,
                Value = Value.A,
                Width = 180,
                Margin = new Thickness(0, 8, 0, 0),
            };
            alpha.PropertyChanged += (_, change) =>
            {
                if (change.Property == global::Avalonia.Controls.Slider.ValueProperty)
                {
                    var next = (byte)Math.Clamp(Math.Round(alpha.Value), 0d, 255d);
                    Value = AvaColor.FromArgb(next, Value.R, Value.G, Value.B);
                }
            };

            content = new StackPanel { Children = { grid, alpha } };
        }

        _flyout = new Flyout
        {
            Content = new Paper { Elevation = 8, Padding = new Thickness(8), Content = content },
            Placement = PlacementMode.BottomEdgeAlignedLeft,
        };
        _flyout.ShowAt(_box ?? (Control)this);
        ApplyBoxChrome();
    }

    private void UpdateLabel()
    {
        var labelForeground = LabelForegroundKey();
        var helperForeground = Error ? LoamTokens.Error : LoamTokens.TextSecondary;
        var hasLabel = !string.IsNullOrEmpty(Label);

        if (_label is not null)
        {
            _label.Text = Label;
            _label.IsVisible = hasLabel;
            _labelForeground?.Dispose();
            _labelForeground = _label.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(labelForeground));
        }

        FieldChrome.ApplyLabelLayout(this, _box, _labelHost, hasLabel);

        if (_helper is not null)
        {
            var text = Error && !string.IsNullOrEmpty(ErrorText) ? ErrorText : HelperText;
            _helper.Text = text;
            _helper.IsVisible = !string.IsNullOrEmpty(text);
            _helperForeground?.Dispose();
            _helperForeground = _helper.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(helperForeground));
        }

        InteractionAssist.SetAutomationName(this, Label, _hex?.Text);
    }

    private void UpdateDisplay()
    {
        if (_swatch is not null)
        {
            _swatch.Background = new ImmutableSolidColorBrush(Value);
        }

        if (_hex is not null)
        {
            _hex.Text = ShowAlpha ? ToHexWithAlpha(Value) : ToHex(Value);
        }

        InteractionAssist.SetAutomationName(this, Label, _hex?.Text);
        UpdateLabel();
    }

    private void ApplyBoxChrome()
    {
        if (_box is null)
        {
            return;
        }

        FieldChrome.Apply(this, _box, Variant.Outlined, Color, Error, IsActive(), IsEnabled,
            ref _boxBorderBrush, ref _boxBackground);
        UpdateLabel();
    }

    private bool IsActive() => IsFocused || _box?.IsFocused == true;

    private string LabelForegroundKey()
    {
        if (Error)
        {
            return LoamTokens.Error;
        }

        if (IsActive())
        {
            var paletteName = Color.ToPaletteName();
            return paletteName is null ? LoamTokens.Primary : LoamTokens.Palette(paletteName);
        }

        return LoamTokens.TextSecondary;
    }

    private static double NormalizeHue(double hue)
    {
        var normalized = hue % 360d;
        return normalized < 0d ? normalized + 360d : normalized;
    }

    private static byte ToByte(double unit) => (byte)Math.Clamp(Math.Round(unit * 255d), 0d, 255d);
}
