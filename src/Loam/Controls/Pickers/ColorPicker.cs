using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A palette color picker, mirroring MudBlazor's <c>MudColorPicker</c> (palette mode). An outlined box
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

    private Border? _box;
    private Border? _swatch;
    private Text? _hex;
    private Text? _label;
    private Flyout? _flyout;

    /// <summary>The selected color (two-way). Mirrors MudBlazor's <c>Value</c>.</summary>
    public Color Value
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

    /// <summary>Formats a color as an upper-case <c>#RRGGBB</c> string.</summary>
    public static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

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
                Value = captured;
                _flyout?.Hide();
            };
            grid.Children.Add(swatch);
        }

        _flyout = new Flyout
        {
            Content = new Paper { Elevation = 8, Padding = new Thickness(8), Content = grid },
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
            _hex.Text = ToHex(Value);
        }
    }
}
