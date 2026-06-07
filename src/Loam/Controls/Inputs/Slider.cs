using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A Material slider, mirroring the reference API's <c>Slider</c>. A custom track + fill + draggable thumb
/// bound to <see cref="Value"/> within <see cref="Minimum"/>/<see cref="Maximum"/>, tinted by <see cref="Color"/>.
/// </summary>
public class Slider : TemplatedControl
{
    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<Slider, double>(nameof(Value), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Minimum"/> property.</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<Slider, double>(nameof(Minimum), 0);

    /// <summary>Identifies the <see cref="Maximum"/> property.</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<Slider, double>(nameof(Maximum), 100);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Slider, LoamColor>(nameof(Color), LoamColor.Primary);

    private const double ThumbSize = 16;
    private const double StateLayerSize = 40;

    private Panel? _area;
    private Border? _fill;
    private Border? _thumb;
    private Border? _stateLayer;
    private bool _dragging;
    private bool _pressed;
    private IDisposable? _fillBackground;
    private IDisposable? _thumbBackground;
    private IDisposable? _stateLayerBackground;

    /// <summary>Creates the slider.</summary>
    public Slider()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
        InteractionAssist.SetAutomationName(this, "Slider");
        ApplyAutomation();

        GotFocus += (_, _) => ApplyStateLayer(CurrentState());
        LostFocus += (_, _) => ApplyStateLayer(CurrentState());
    }

    /// <summary>The current value (two-way). Mirrors the reference API's <c>Value</c>.</summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Minimum value. Mirrors the reference API's <c>Min</c>.</summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Maximum value. Mirrors the reference API's <c>Max</c>.</summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Accent color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>The 0–1 position of <paramref name="value"/> within the range.</summary>
    public static double Fraction(double value, double min, double max) =>
        max <= min ? 0 : Math.Clamp((value - min) / (max - min), 0, 1);

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Slider);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _area = e.NameScope.Find("PART_Area") as Panel;
        _fill = e.NameScope.Find("PART_Fill") as Border;
        _thumb = e.NameScope.Find("PART_Thumb") as Border;
        _stateLayer = e.NameScope.Find("PART_StateLayer") as Border;

        if (_area is not null)
        {
            _area.Focusable = true;
            _area.Cursor = Cursor;
            _area.PointerEntered += (_, _) => ApplyStateLayer(CurrentState());
            _area.PointerExited += (_, _) =>
            {
                _pressed = false;
                ApplyStateLayer(CurrentState());
            };
            _area.GotFocus += (_, _) => ApplyStateLayer(CurrentState());
            _area.LostFocus += (_, _) => ApplyStateLayer(CurrentState());
            _area.PointerPressed += OnPointerPressed;
            _area.PointerMoved += OnPointerMoved;
            _area.PointerReleased += OnPointerReleased;
        }

        ApplyColors();
        ApplyEnabledState();
        UpdatePositions();
        ApplyStateLayer(CurrentState());
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || change.Property == MinimumProperty ||
            change.Property == MaximumProperty || change.Property == BoundsProperty)
        {
            UpdatePositions();
            ApplyAutomation();
        }
        else if (change.Property == ColorProperty)
        {
            ApplyColors();
            ApplyStateLayer(CurrentState());
        }
        else if (change.Property == IsEnabledProperty)
        {
            ApplyEnabledState();
            ApplyStateLayer(CurrentState());
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

        if (InteractionAssist.IsIncrementKey(e.Key))
        {
            Value = ClampValue(Value + KeyboardStep());
            e.Handled = true;
            ApplyStateLayer(CurrentState());
        }
        else if (InteractionAssist.IsDecrementKey(e.Key))
        {
            Value = ClampValue(Value - KeyboardStep());
            e.Handled = true;
            ApplyStateLayer(CurrentState());
        }
        else if (e.Key == Key.Home)
        {
            Value = Minimum;
            e.Handled = true;
            ApplyStateLayer(CurrentState());
        }
        else if (e.Key == Key.End)
        {
            Value = Maximum;
            e.Handled = true;
            ApplyStateLayer(CurrentState());
        }
    }

    private void ApplyColors()
    {
        var tokens = SemanticColor.Resolve(Color);
        if (_fill is not null)
        {
            _fillBackground?.Dispose();
            _fillBackground = _fill.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));
        }

        if (_thumb is not null)
        {
            _thumbBackground?.Dispose();
            _thumbBackground = _thumb.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));
        }
    }

    private void UpdatePositions()
    {
        if (_area is null)
        {
            return;
        }

        var width = _area.Bounds.Width;
        var fraction = Fraction(Value, Minimum, Maximum);
        var x = fraction * width;

        if (_fill is not null)
        {
            _fill.Width = x;
        }

        if (_thumb is not null)
        {
            _thumb.Margin = new Thickness(Math.Clamp(x - (ThumbSize / 2), 0, Math.Max(0, width - ThumbSize)), 0, 0, 0);
        }

        if (_stateLayer is not null)
        {
            _stateLayer.Margin = new Thickness(
                Math.Clamp(x - (StateLayerSize / 2), 0, Math.Max(0, width - StateLayerSize)),
                0,
                0,
                0);
        }
    }

    private void ApplyEnabledState() => Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);

    private string? CurrentState()
    {
        if (!IsEnabled)
        {
            return null;
        }

        if (_pressed || _dragging)
        {
            return "Pressed";
        }

        if (IsFocused || _area?.IsFocused == true)
        {
            return "Focus";
        }

        return IsPointerOver || _area?.IsPointerOver == true ? "Hover" : null;
    }

    private void ApplyStateLayer(string? state)
    {
        if (_stateLayer is null)
        {
            return;
        }

        _stateLayerBackground?.Dispose();
        _stateLayerBackground = null;
        if (state is null)
        {
            _stateLayer.Background = Brushes.Transparent;
            return;
        }

        _stateLayerBackground = _stateLayer.Bind(Border.BackgroundProperty,
            this.GetResourceObservable(StateLayerToken(state)));
    }

    private string StateLayerToken(string state)
    {
        if (Color.ToPaletteName() is { } paletteName)
        {
            return state switch
            {
                "Focus" => LoamTokens.PaletteFocus(paletteName),
                "Pressed" => LoamTokens.PalettePressed(paletteName),
                _ => LoamTokens.PaletteHover(paletteName),
            };
        }

        return LoamTokens.ColorSchemeStateLayer(nameof(LoamColorScheme.OnSurface), state);
    }

    private double KeyboardStep()
    {
        var range = Maximum - Minimum;
        return range <= 0 ? 0 : range / 10;
    }

    private double ClampValue(double value) => Maximum <= Minimum ? Minimum : Math.Clamp(value, Minimum, Maximum);

    private void SetValueFromPointer(PointerEventArgs e)
    {
        if (_area is null || _area.Bounds.Width <= 0)
        {
            return;
        }

        var x = e.GetPosition(_area).X;
        var fraction = Math.Clamp(x / _area.Bounds.Width, 0, 1);
        Value = Minimum + (fraction * (Maximum - Minimum));
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        _dragging = true;
        _pressed = true;
        Focus();
        e.Pointer.Capture(_area);
        SetValueFromPointer(e);
        ApplyStateLayer(CurrentState());
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragging)
        {
            SetValueFromPointer(e);
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragging = false;
        _pressed = false;
        e.Pointer.Capture(null);
        ApplyStateLayer(CurrentState());
    }

    private void ApplyAutomation()
    {
        AutomationProperties.SetName(this, "Slider");
        AutomationProperties.SetHelpText(this, $"Value {Value:0.##} from {Minimum:0.##} to {Maximum:0.##}");
    }
}
