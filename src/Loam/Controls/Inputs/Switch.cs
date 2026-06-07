using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A Material toggle switch, mirroring the reference API's <c>Switch</c>. Subclasses Avalonia's
/// <see cref="ToggleButton"/> (toggle behavior, <c>:checked</c> state) and renders a token-colored
/// track + thumb that slides on <see cref="ToggleButton.IsChecked"/>, tinted by <see cref="Color"/>.
/// </summary>
public class Switch : ToggleButton
{
    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Switch, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<Switch, LoamSize>(nameof(Size), LoamSize.Medium);

    private Border? _track;
    private Border? _thumb;
    private Border? _stateLayer;
    private Panel? _switchArea;
    private IDisposable? _trackBackground;
    private IDisposable? _thumbBackground;
    private IDisposable? _stateLayerBackground;
    private bool _pressed;

    /// <summary>Creates a switch with token-driven state feedback.</summary>
    public Switch()
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

    /// <summary>Switch size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Switch);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _switchArea = e.NameScope.Find("PART_SwitchArea") as Panel;
        _track = e.NameScope.Find("PART_Track") as Border;
        _thumb = e.NameScope.Find("PART_Thumb") as Border;
        _stateLayer = e.NameScope.Find("PART_StateLayer") as Border;
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

        if (change.Property == ContentProperty)
        {
            ApplyAutomation();
        }
    }

    private void ApplyVisual()
    {
        var isChecked = IsChecked == true;
        var tokens = SemanticColor.Resolve(Color);
        var metrics = Size switch
        {
            LoamSize.ExtraSmall => (AreaWidth: 30d, AreaHeight: 16d, TrackWidth: 26d, TrackHeight: 10d, ThumbSize: 16d),
            LoamSize.Small => (AreaWidth: 34d, AreaHeight: 18d, TrackWidth: 30d, TrackHeight: 12d, ThumbSize: 18d),
            LoamSize.Large => (AreaWidth: 54d, AreaHeight: 28d, TrackWidth: 48d, TrackHeight: 18d, ThumbSize: 28d),
            LoamSize.ExtraLarge => (AreaWidth: 64d, AreaHeight: 32d, TrackWidth: 58d, TrackHeight: 22d, ThumbSize: 32d),
            _ => (AreaWidth: 40d, AreaHeight: 20d, TrackWidth: 34d, TrackHeight: 14d, ThumbSize: 20d),
        };

        if (_switchArea is not null)
        {
            _switchArea.Width = metrics.AreaWidth;
            _switchArea.Height = metrics.AreaHeight;
        }

        if (_thumb is not null)
        {
            _thumb.Width = metrics.ThumbSize;
            _thumb.Height = metrics.ThumbSize;
            _thumb.CornerRadius = new CornerRadius(metrics.ThumbSize / 2);
            _thumb.HorizontalAlignment = isChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            _thumbBackground?.Dispose();
            _thumbBackground = _thumb.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(isChecked ? tokens.Fill : LoamTokens.Surface));
        }

        if (_stateLayer is not null)
        {
            _stateLayer.HorizontalAlignment = isChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        if (_track is not null)
        {
            _track.Width = metrics.TrackWidth;
            _track.Height = metrics.TrackHeight;
            _track.CornerRadius = new CornerRadius(metrics.TrackHeight / 2);
            _track.Opacity = isChecked ? 0.5 : 1;
            _trackBackground?.Dispose();
            _trackBackground = _track.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(isChecked ? tokens.Fill : LoamTokens.Palette(nameof(LoamPalette.GrayLight))));
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
        InteractionAssist.SetAutomationName(this, Content, "Switch");
        AutomationProperties.SetHelpText(this, IsChecked == true ? "Switch on" : "Switch off");
    }
}
