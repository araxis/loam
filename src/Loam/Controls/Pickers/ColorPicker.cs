using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A palette color picker, mirroring the reference API's <c>ColorPicker</c> (palette mode). An outlined box
/// shows the current <see cref="Value"/> as a swatch + hex string; clicking opens a flyout of preset
/// swatches (a curated Material palette) that set <see cref="Value"/>.
/// </summary>
public class ColorPicker : TemplatedControl
{
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    /// <summary>The default palette shown in the flyout (Material 500-ish hues + neutrals).</summary>
    public static readonly IReadOnlyList<Color> DefaultPalette = new[]
    {
        Color.Parse("#F44336"), Color.Parse("#E91E63"), Color.Parse("#9C27B0"), Color.Parse("#673AB7"),
        Color.Parse("#3F51B5"), Color.Parse("#2196F3"), Color.Parse("#03A9F4"), Color.Parse("#00BCD4"),
        Color.Parse("#009688"), Color.Parse("#4CAF50"), Color.Parse("#8BC34A"), Color.Parse("#CDDC39"),
        Color.Parse("#FFEB3B"), Color.Parse("#FFC107"), Color.Parse("#FF9800"), Color.Parse("#FF5722"),
        Color.Parse("#795548"), Color.Parse("#9E9E9E"), Color.Parse("#607D8B"), Color.Parse("#000000"),
    };

    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<Color> ValueProperty =
        AvaloniaProperty.Register<ColorPicker, Color>(nameof(Value), Color.Parse("#594AE2"),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<ColorPicker, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="ShowAlpha"/> property.</summary>
    public static readonly StyledProperty<bool> ShowAlphaProperty =
        AvaloniaProperty.Register<ColorPicker, bool>(nameof(ShowAlpha));

    private Border? _box;
    private Border? _swatch;
    private Text? _hex;
    private Text? _label;
    private Flyout? _flyout;

    /// <summary>The selected color (two-way). Mirrors the reference API's <c>Value</c>.</summary>
    public Color Value
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

    /// <summary>A hue/saturation/value color triple using degrees and unit fractions.</summary>
    public readonly record struct HsvColor(double Hue, double Saturation, double Value);

    /// <summary>Formats a color as an upper-case <c>#RRGGBB</c> string.</summary>
    public static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>Formats a color as an upper-case <c>#AARRGGBB</c> string.</summary>
    public static string ToHexWithAlpha(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>Converts an HSV color to an Avalonia color.</summary>
    public static Color FromHsv(double hue, double saturation, double value, byte alpha = 255)
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

        return Color.FromArgb(alpha, ToByte(r + m), ToByte(g + m), ToByte(b + m));
    }

    /// <summary>Converts an Avalonia color to HSV.</summary>
    public static HsvColor ToHsv(Color color)
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
        _swatch = e.NameScope.Find("PART_Swatch") as Border;
        _hex = e.NameScope.Find("PART_Hex") as Text;
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
        if (change.Property == ValueProperty || change.Property == ShowAlphaProperty)
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
                    ? Color.FromArgb(Value.A, captured.R, captured.G, captured.B)
                    : captured;
                _flyout?.Hide();
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
                    Value = Color.FromArgb(next, Value.R, Value.G, Value.B);
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
        if (_swatch is not null)
        {
            _swatch.Background = new ImmutableSolidColorBrush(Value);
        }

        if (_hex is not null)
        {
            _hex.Text = ShowAlpha ? ToHexWithAlpha(Value) : ToHex(Value);
        }
    }

    private static double NormalizeHue(double hue)
    {
        var normalized = hue % 360d;
        return normalized < 0d ? normalized + 360d : normalized;
    }

    private static byte ToByte(double unit) => (byte)Math.Clamp(Math.Round(unit * 255d), 0d, 255d);
}
