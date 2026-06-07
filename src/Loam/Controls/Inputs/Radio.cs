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
/// A Material radio button, mirroring the reference API's <c>Radio</c>. Subclasses Avalonia's
/// <see cref="RadioButton"/> (mutual exclusion, keyboard) and renders a token-colored ring + dot.
/// Carries a <see cref="Value"/> for use with <see cref="RadioGroup"/>.
/// </summary>
public class Radio : RadioButton
{
    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Radio, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<Radio, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<Radio, object?>(nameof(Value));

    private Border? _ring;
    private Border? _dot;
    private Border? _stateLayer;
    private Panel? _visualHost;
    private IDisposable? _ringBorder;
    private IDisposable? _dotBackground;
    private IDisposable? _stateLayerBackground;
    private bool _pressed;

    /// <summary>Creates a radio button with token-driven state feedback.</summary>
    public Radio()
    {
        GotFocus += (_, _) => ApplyStateLayer(CurrentState());
        LostFocus += (_, _) => ApplyStateLayer(CurrentState());
        PointerEntered += (_, _) => ApplyStateLayer(CurrentState());
        PointerExited += (_, _) =>
        {
            _pressed = false;
            ApplyStateLayer(CurrentState());
        };
        PointerPressed += (_, _) =>
        {
            _pressed = true;
            ApplyStateLayer(CurrentState());
        };
        PointerReleased += (_, _) =>
        {
            _pressed = false;
            ApplyStateLayer(CurrentState());
        };
        KeyDown += (_, args) =>
        {
            if (InteractionAssist.IsActivationKey(args.Key))
            {
                _pressed = true;
                ApplyStateLayer(CurrentState());
            }
        };
        KeyUp += (_, args) =>
        {
            if (InteractionAssist.IsActivationKey(args.Key))
            {
                _pressed = false;
                ApplyStateLayer(CurrentState());
            }
        };
    }

    /// <summary>Selected color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Radio size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The value this option represents in a <see cref="RadioGroup"/>.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Radio);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _ring = e.NameScope.Find("PART_Ring") as Border;
        _dot = e.NameScope.Find("PART_Dot") as Border;
        _stateLayer = e.NameScope.Find("PART_StateLayer") as Border;
        _visualHost = e.NameScope.Find("PART_VisualHost") as Panel;
        ApplyVisual();
        ApplyAutomation();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsCheckedProperty || change.Property == ColorProperty ||
            change.Property == SizeProperty || change.Property == IsEnabledProperty)
        {
            ApplyVisual();
            ApplyStateLayer(CurrentState());
            ApplyAutomation();
        }

        if (change.Property == ContentProperty || change.Property == ValueProperty)
        {
            ApplyAutomation();
        }
    }

    private void ApplyVisual()
    {
        var size = Size switch
        {
            LoamSize.ExtraSmall => 16d,
            LoamSize.Small => 18d,
            LoamSize.Large => 24d,
            LoamSize.ExtraLarge => 28d,
            _ => 20d,
        };
        var isChecked = IsChecked == true;
        var tokens = SemanticColor.Resolve(Color);

        if (_visualHost is not null)
        {
            _visualHost.Width = size;
            _visualHost.Height = size;
        }

        if (_ring is not null)
        {
            _ring.Width = size;
            _ring.Height = size;
            _ring.CornerRadius = new CornerRadius(size / 2);
            _ring.BorderThickness = new Thickness(2);
            _ringBorder?.Dispose();
            _ringBorder = _ring.Bind(Border.BorderBrushProperty,
                this.GetResourceObservable(isChecked ? tokens.Fill : LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));
        }

        if (_dot is not null)
        {
            var dotSize = size * 0.5;
            _dot.Width = dotSize;
            _dot.Height = dotSize;
            _dot.CornerRadius = new CornerRadius(dotSize / 2);
            _dot.IsVisible = isChecked;
            _dotBackground?.Dispose();
            _dotBackground = _dot.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));
        }

        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
    }

    private string? CurrentState()
    {
        if (!IsEnabled)
        {
            return null;
        }

        if (_pressed)
        {
            return "Pressed";
        }

        if (IsFocused)
        {
            return "Focus";
        }

        return IsPointerOver ? "Hover" : null;
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
        if (IsChecked != true)
        {
            return LoamTokens.ColorSchemeStateLayer(nameof(LoamColorScheme.OnSurface), state);
        }

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

    private void ApplyAutomation()
    {
        InteractionAssist.SetAutomationName(this, Content, Value, "Radio");
        AutomationProperties.SetHelpText(this, IsChecked == true ? "Radio selected" : "Radio unselected");
    }
}
