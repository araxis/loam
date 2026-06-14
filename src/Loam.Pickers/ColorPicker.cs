using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
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
/// A palette color picker, mirroring the reference API's <c>ColorPicker</c> (palette mode). A variant field
/// shows the current <see cref="Value"/> as a swatch + hex string; clicking opens a flyout of preset
/// swatches from the built-in palette that set <see cref="Value"/>.
/// </summary>
public class ColorPicker : TemplatedControl
{
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    /// <summary>Horizontal space the leading swatch occupies (20px wide + 10px right margin), used to inset the label.</summary>
    private const double SwatchLeadingInset = 30;

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

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<ColorPicker, Variant>(nameof(Variant), Loam.Variant.Outlined);

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

    /// <summary>Identifies the <see cref="Editable"/> property.</summary>
    public static readonly StyledProperty<bool> EditableProperty =
        AvaloniaProperty.Register<ColorPicker, bool>(nameof(Editable));

    /// <summary>Identifies the <see cref="InvalidHexText"/> property.</summary>
    public static readonly StyledProperty<string> InvalidHexTextProperty =
        AvaloniaProperty.Register<ColorPicker, string>(nameof(InvalidHexText), "Invalid color");

    private Border? _box;
    private Border? _labelHost;
    private Border? _swatch;
    private Text? _hex;
    private TextBox? _input;
    private bool _flyoutOpening;
    private Text? _label;
    private Text? _helper;
    private IDisposable? _boxBorderBrush;
    private IDisposable? _boxBackground;
    private IDisposable? _labelForeground;
    private IDisposable? _helperForeground;
    private Flyout? _flyout;
    private bool _flyoutOpen;

    /// <summary>Colors shown in the generated swatch palette.</summary>
    public AvaloniaList<AvaColor> Palette { get; } = [];

    /// <summary>Raised when <see cref="Value"/> changes.</summary>
    public event EventHandler<AvaColor>? ValueChanged;

    /// <summary>Creates the picker.</summary>
    public ColorPicker()
    {
        Palette.CollectionChanged += OnPaletteChanged;
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

    /// <summary>Visual field style: outlined, filled, or text/underline.</summary>
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

    /// <summary>When true, the user can type a hex color into the field; the swatch (or Alt+Down) opens the palette.</summary>
    public bool Editable
    {
        get => GetValue(EditableProperty);
        set => SetValue(EditableProperty, value);
    }

    /// <summary>Error message shown when typed text cannot be parsed as a color (<see cref="Editable"/> mode).</summary>
    public string InvalidHexText
    {
        get => GetValue(InvalidHexTextProperty);
        set => SetValue(InvalidHexTextProperty, value);
    }

    /// <summary>Opens the color palette flyout.</summary>
    public void OpenPicker() => Open();

    /// <summary>Closes the color palette flyout.</summary>
    public void ClosePicker()
    {
        _flyout?.Hide();
        ApplyBoxChrome();
    }

    /// <summary>A hue/saturation/value color triple using degrees and unit fractions.</summary>
    public readonly record struct HsvColor(double Hue, double Saturation, double Value);

    /// <summary>Parses a hex (e.g. <c>#RRGGBB</c>/<c>#AARRGGBB</c>) or named color; returns <c>false</c> for empty or unparseable text.</summary>
    public static bool TryParseColor(string? text, out AvaColor color)
    {
        if (!string.IsNullOrWhiteSpace(text) && AvaColor.TryParse(text.Trim(), out color))
        {
            return true;
        }

        color = default;
        return false;
    }

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
        _input = e.NameScope.Find("PART_Input") as TextBox;
        _label = e.NameScope.Find("PART_Label") as Text;
        _helper = e.NameScope.Find("PART_HelperText") as Text;

        if (_swatch is not null)
        {
            // In editable mode the swatch is the palette opener (clicking elsewhere focuses the text box).
            // It is not focusable, so it does not blur the input; the flyout-open guard still covers popup focus theft.
            AutomationProperties.SetName(_swatch, "Open color palette");
            _swatch.PointerPressed += (_, args) =>
            {
                if (Editable && IsEnabled)
                {
                    _flyoutOpening = true;
                    Open();
                    _flyoutOpening = false;
                    args.Handled = true;
                }
            };
        }

        if (_input is not null)
        {
            FieldChrome.ResetInnerTextBox(_input);
            _input.GotFocus += (_, _) =>
            {
                FieldChrome.ResetInnerTextBox(_input);
                ApplyBoxChrome();
            };
            _input.LostFocus += (_, _) =>
            {
                CommitText();
                ApplyBoxChrome();
            };
            _input.KeyDown += (_, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    CommitText();
                    args.Handled = true;
                }
                else if (args.Key is Key.Down && args.KeyModifiers.HasFlag(KeyModifiers.Alt))
                {
                    Open();
                    args.Handled = true;
                }
            };
        }

        if (_box is not null)
        {
            _box.GotFocus += (_, _) => ApplyBoxChrome();
            _box.LostFocus += (_, _) => ApplyBoxChrome();
            _box.PointerPressed += (_, _) =>
            {
                if (!IsEnabled)
                {
                    return;
                }

                if (Editable)
                {
                    _input?.Focus();
                }
                else
                {
                    Focus();
                    Open();
                }
            };
        }

        UpdateEditMode();
        UpdateLabel();
        UpdateDisplay();
        ApplyBoxChrome();
    }

    private void UpdateEditMode()
    {
        if (_input is not null)
        {
            _input.IsVisible = Editable;
            _input.IsReadOnly = !Editable;
        }

        if (_hex is not null)
        {
            _hex.IsVisible = !Editable;
        }
    }

    private void CommitText()
    {
        // Skip while the flyout is open/opening: the popup steals focus and the palette selection,
        // not the in-progress typed text, is what should set the value.
        if (_input is null || !Editable || _flyoutOpen || _flyoutOpening)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_input.Text))
        {
            Error = false;
            UpdateDisplay(); // a color has no empty state — revert the text to the current value
            return;
        }

        if (!TryParseColor(_input.Text, out var parsed))
        {
            Error = true;
            ErrorText = InvalidHexText;
            return; // keep the user's text so they can correct it
        }

        Error = false;
        // Honor ShowAlpha: when alpha is not exposed, force opaque even if the user typed #AARRGGBB.
        Value = ShowAlpha ? parsed : AvaColor.FromArgb(255, parsed.R, parsed.G, parsed.B);
        UpdateDisplay();        // reformat the text box even when the parsed value is unchanged
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || change.Property == ShowAlphaProperty)
        {
            UpdateDisplay();
            if (change.Property == ValueProperty)
            {
                ValueChanged?.Invoke(this, Value);
            }
        }
        else if (change.Property == LabelProperty || change.Property == ShrinkLabelProperty ||
                 change.Property == HelperTextProperty || change.Property == ErrorTextProperty)
        {
            UpdateLabel();
        }

        if (change.Property == EditableProperty)
        {
            UpdateEditMode();
            UpdateDisplay();
            ApplyBoxChrome();
        }

        if (change.Property == VariantProperty || change.Property == ColorProperty || change.Property == ErrorProperty ||
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
        if (!IsEnabled)
        {
            return;
        }

        // In editable mode, Space/Enter belong to the text box; the palette opens via the swatch or Alt+Down.
        if (!Editable && InteractionAssist.IsActivationKey(e.Key))
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
        if (!IsEnabled)
        {
            return;
        }

        _flyout?.Hide();
        var colors = Palette.Count > 0 ? Palette : DefaultPalette;
        var columns = Math.Clamp(colors.Count, 1, 5);
        var grid = new UniformGrid { Columns = columns, Width = columns * 36 };
        foreach (var color in colors)
        {
            var fill = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(4),
                Background = new ImmutableSolidColorBrush(color),
            };

            var stateLayer = new Border
            {
                Name = "PART_PaletteStateLayer",
                Background = Brushes.Transparent,
                CornerRadius = new CornerRadius(4),
                IsHitTestVisible = false,
            };

            var swatch = new Border
            {
                Name = "PART_PaletteSwatch",
                Width = 36,
                Height = 36,
                Padding = new Thickness(3),
                Child = new ResponsiveGrid { Children = { stateLayer, fill } },
                Cursor = HandCursor,
                Focusable = true,
            };
            var captured = color;
            IDisposable? stateLayerBinding = null;
            void ApplyState(string? state)
            {
                stateLayerBinding?.Dispose();
                stateLayerBinding = null;
                if (state is null)
                {
                    stateLayer.Background = Brushes.Transparent;
                    return;
                }

                stateLayerBinding = stateLayer.Bind(Border.BackgroundProperty,
                    this.GetResourceObservable(LoamTokens.ColorSchemeStateLayer(nameof(LoamColorScheme.OnSurface), state)));
            }

            void SelectColor()
            {
                Value = ShowAlpha
                    ? AvaColor.FromArgb(Value.A, captured.R, captured.G, captured.B)
                    : captured;
                _flyout?.Hide();
                ApplyBoxChrome();
            }

            AutomationProperties.SetName(swatch, $"Color {ToHex(captured)}");
            AutomationProperties.SetHelpText(swatch, "Select color");
            swatch.PointerEntered += (_, _) => ApplyState("Hover");
            swatch.PointerExited += (_, _) => ApplyState(swatch.IsFocused ? "Focus" : null);
            swatch.PointerPressed += (_, args) =>
            {
                ApplyState("Pressed");
                swatch.Focus();
                SelectColor();
                args.Handled = true;
            };
            swatch.GotFocus += (_, _) => ApplyState("Focus");
            swatch.LostFocus += (_, _) => ApplyState(null);
            swatch.KeyDown += (_, args) =>
            {
                if (InteractionAssist.IsActivationKey(args.Key))
                {
                    SelectColor();
                    args.Handled = true;
                }
                else if (args.Key == Key.Escape)
                {
                    _flyout?.Hide();
                    args.Handled = true;
                }
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
            Content = PopupSurface.PickerPaper(content, 0, new Thickness(8)),
            Placement = PlacementMode.BottomEdgeAlignedLeft,
            FlyoutPresenterTheme = PopupSurface.FlyoutPresenterTheme,
        };
        _flyout.Closed += (_, _) =>
        {
            _flyoutOpen = false;
            ApplyBoxChrome();
        };
        _flyoutOpen = true;
        _flyout.ShowAt(_box ?? (Control)this);
        ApplyBoxChrome();
    }

    private void OnPaletteChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_flyoutOpen)
        {
            Open();
        }
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

        // Inset the floating label past the leading swatch (20px wide + 10px margin) so it aligns above the value.
        FieldChrome.ApplyLabelLayout(this, _box, _labelHost, hasLabel, Variant, SwatchLeadingInset);

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

        var hexText = ShowAlpha ? ToHexWithAlpha(Value) : ToHex(Value);
        if (_hex is not null)
        {
            _hex.Text = hexText;
        }

        // Keep the editable text box in sync with the committed value.
        if (_input is not null && Editable)
        {
            _input.Text = hexText;
            AutomationProperties.SetName(_input, Label ?? "Color hex");
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

        FieldChrome.Apply(this, _box, Variant, Color, Error, IsActive(), IsEnabled,
            ref _boxBorderBrush, ref _boxBackground);
        _box.IsEnabled = IsEnabled;
        _box.Cursor = !IsEnabled ? Cursor.Default
            : Editable ? new Cursor(StandardCursorType.Ibeam)
            : HandCursor;
        UpdateLabel();
    }

    private bool IsActive() => _flyoutOpen || IsFocused || _box?.IsFocused == true || _input?.IsFocused == true;

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
