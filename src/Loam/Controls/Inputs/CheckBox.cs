using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;
using AvaPath = Avalonia.Controls.Shapes.Path;

namespace Loam.Controls;

/// <summary>
/// A Material checkbox, mirroring the reference API's <c>CheckBox</c>. Subclasses Avalonia's
/// <see cref="Avalonia.Controls.CheckBox"/> (for tri-state, keyboard, toggle behavior) and renders a
/// token-colored box + checkmark sized by <see cref="Size"/>, filled with <see cref="Color"/> when checked.
/// </summary>
public class CheckBox : global::Avalonia.Controls.CheckBox
{
    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<CheckBox, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<CheckBox, LoamSize>(nameof(Size), LoamSize.Medium);

    private Border? _box;
    private Border? _stateLayer;
    private Panel? _visualHost;
    private AvaPath? _check;
    private IDisposable? _boxBackground;
    private IDisposable? _boxBorder;
    private IDisposable? _checkFill;
    private IDisposable? _stateLayerBackground;
    private bool _pressed;

    /// <summary>Creates a checkbox with token-driven state feedback.</summary>
    public CheckBox()
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

    /// <summary>Checked color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Checkbox size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CheckBox);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _box = e.NameScope.Find("PART_Box") as Border;
        _stateLayer = e.NameScope.Find("PART_StateLayer") as Border;
        _visualHost = e.NameScope.Find("PART_VisualHost") as Panel;
        _check = e.NameScope.Find("PART_Check") as AvaPath;
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
        }

        if (change.Property == ContentProperty)
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
        var isIndeterminate = IsChecked is null;
        var isSelected = isChecked || isIndeterminate;
        var tokens = SemanticColor.Resolve(Color);

        if (_visualHost is not null)
        {
            _visualHost.Width = size;
            _visualHost.Height = size;
        }

        if (_box is not null)
        {
            _box.Width = size;
            _box.Height = size;
            _boxBackground?.Dispose();
            _boxBorder?.Dispose();
            _boxBorder = null;

            if (isSelected)
            {
                _boxBackground = _box.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));
                _box.BorderThickness = default;
            }
            else
            {
                _box.Background = Brushes.Transparent;
                _boxBackground = null;
                _box.BorderThickness = new Thickness(2);
                _boxBorder = _box.Bind(Border.BorderBrushProperty,
                    this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));
            }
        }

        if (_check is not null)
        {
            _check.IsVisible = isSelected;
            _check.Data = Geometry.Parse(isIndeterminate ? Icons.Material.Filled.HorizontalRule : Icons.Material.Filled.Check);
            _check.Margin = isIndeterminate ? new Thickness(4, 0) : new Thickness(3);
            _checkFill?.Dispose();
            _checkFill = _check.Bind(Shape.FillProperty, this.GetResourceObservable(tokens.FillText));
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
        var selected = IsChecked == true || IsChecked is null;
        if (!selected)
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
        InteractionAssist.SetAutomationName(this, Content, "CheckBox");
        AutomationProperties.SetHelpText(this, IsThreeState ? "Three-state checkbox" : "Checkbox");
    }
}
