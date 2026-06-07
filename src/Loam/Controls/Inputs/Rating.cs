using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A star rating, mirroring the reference API's <c>Rating</c>. Shows <see cref="MaxValue"/> stars filled up
/// to the two-way <see cref="SelectedValue"/> (or the hovered star as a live preview), tinted by
/// <see cref="Color"/>. Set <see cref="ReadOnly"/> to display a fixed score.
/// </summary>
public class Rating : TemplatedControl
{
    private const double StateLayerSize = 40;

    /// <summary>Identifies the <see cref="SelectedValue"/> property.</summary>
    public static readonly StyledProperty<int> SelectedValueProperty =
        AvaloniaProperty.Register<Rating, int>(nameof(SelectedValue),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="MaxValue"/> property.</summary>
    public static readonly StyledProperty<int> MaxValueProperty =
        AvaloniaProperty.Register<Rating, int>(nameof(MaxValue), 5);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Rating, LoamColor>(nameof(Color), LoamColor.Warning);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<Rating, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="ReadOnly"/> property.</summary>
    public static readonly StyledProperty<bool> ReadOnlyProperty =
        AvaloniaProperty.Register<Rating, bool>(nameof(ReadOnly));

    private readonly List<Icon> _stars = new();
    private readonly List<Border> _stateLayers = new();
    private readonly List<Panel> _starTargets = new();
    private readonly List<IDisposable?> _stateLayerBackgrounds = new();
    private StackPanel? _panel;
    private int _hover;
    private int _pressed;
    private bool _keyPressed;

    /// <summary>Creates the rating.</summary>
    public Rating()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
        InteractionAssist.SetAutomationName(this, "Rating");
        ApplyAutomation();

        GotFocus += (_, _) => ApplyStateLayers();
        LostFocus += (_, _) =>
        {
            _keyPressed = false;
            ApplyStateLayers();
        };
    }

    /// <summary>The selected score (two-way). Mirrors the reference API's <c>SelectedValue</c>.</summary>
    public int SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>The number of stars. Mirrors the reference API's <c>MaxValue</c>.</summary>
    public int MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    /// <summary>Filled-star color. Mirrors the reference API's <c>Color</c> (defaults to a star-gold Warning).</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Star size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Whether the rating is display-only. Mirrors the reference API's <c>ReadOnly</c>.</summary>
    public bool ReadOnly
    {
        get => GetValue(ReadOnlyProperty);
        set => SetValue(ReadOnlyProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Rating);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _panel = e.NameScope.Find("PART_Stars") as StackPanel;
        if (_panel is not null)
        {
            _panel.Focusable = true;
            _panel.Cursor = Cursor;
            _panel.PointerExited += (_, _) =>
            {
                _hover = 0;
                _pressed = 0;
                UpdateStars();
            };
        }

        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaxValueProperty || change.Property == SizeProperty)
        {
            Rebuild();
        }
        else if (change.Property == SelectedValueProperty || change.Property == ColorProperty ||
                 change.Property == ReadOnlyProperty || change.Property == IsEnabledProperty)
        {
            ApplyEnabledState();
            ApplyAutomation();
            UpdateStars();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled || ReadOnly)
        {
            return;
        }

        if (InteractionAssist.IsIncrementKey(e.Key))
        {
            _keyPressed = false;
            SelectedValue = ClampValue(SelectedValue + 1);
            e.Handled = true;
            ApplyStateLayers();
        }
        else if (InteractionAssist.IsDecrementKey(e.Key))
        {
            _keyPressed = false;
            SelectedValue = ClampValue(SelectedValue - 1);
            e.Handled = true;
            ApplyStateLayers();
        }
        else if (e.Key == Key.Home)
        {
            _keyPressed = false;
            SelectedValue = 0;
            e.Handled = true;
            ApplyStateLayers();
        }
        else if (e.Key == Key.End)
        {
            _keyPressed = false;
            SelectedValue = Math.Max(0, MaxValue);
            e.Handled = true;
            ApplyStateLayers();
        }
        else if (InteractionAssist.IsActivationKey(e.Key))
        {
            _keyPressed = true;
            if (SelectedValue == 0 && MaxValue > 0)
            {
                SelectedValue = 1;
            }

            e.Handled = true;
            ApplyStateLayers();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (InteractionAssist.IsActivationKey(e.Key))
        {
            _keyPressed = false;
            e.Handled = true;
            ApplyStateLayers();
        }
    }

    private void Rebuild()
    {
        if (_panel is null)
        {
            return;
        }

        _panel.Children.Clear();
        _stars.Clear();
        ClearStateLayerBindings();
        _stateLayers.Clear();
        _starTargets.Clear();

        for (var i = 1; i <= MaxValue; i++)
        {
            var stateLayer = new Border
            {
                Name = "PART_StateLayer",
                Width = StateLayerSize,
                Height = StateLayerSize,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
            };
            stateLayer.Bind(Border.CornerRadiusProperty, this.GetResourceObservable(LoamTokens.ShapeFull));

            var star = new Icon
            {
                Data = Icons.Material.Filled.Star,
                Size = Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
            };

            var index = i;
            var target = new Panel
            {
                Width = StateLayerSize,
                Height = StateLayerSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursor,
                Children = { stateLayer, star },
            };
            InteractionAssist.SetAutomationName(target, $"Set rating {index} of {MaxValue}");
            InteractionAssist.SetAutomationName(star, $"{index} of {MaxValue}");

            target.PointerEntered += (_, _) =>
            {
                if (CanInteract)
                {
                    _hover = index;
                    UpdateStars();
                }
            };
            target.PointerPressed += (_, _) =>
            {
                if (CanInteract)
                {
                    Focus();
                    _pressed = index;
                    SelectedValue = index;
                    UpdateStars();
                }
            };
            target.PointerReleased += (_, _) =>
            {
                _pressed = 0;
                ApplyStateLayers();
            };

            _panel.Children.Add(target);
            _starTargets.Add(target);
            _stateLayers.Add(stateLayer);
            _stateLayerBackgrounds.Add(null);
            _stars.Add(star);
        }

        UpdateStars();
        ApplyEnabledState();
        ApplyAutomation();
    }

    private void UpdateStars()
    {
        var effective = CanInteract && _hover > 0 ? _hover : SelectedValue;
        for (var i = 0; i < _stars.Count; i++)
        {
            var filled = i < effective;
            _stars[i].Color = filled ? Color : LoamColor.Default;
            _stars[i].Opacity = filled ? 1 : 0.3;
            _stars[i].Cursor = CanInteract ? new Cursor(StandardCursorType.Hand) : Cursor.Default;

            if (i < _starTargets.Count)
            {
                _starTargets[i].Cursor = CanInteract ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
                AutomationProperties.SetHelpText(_starTargets[i],
                    filled ? "Selected rating target" : "Unselected rating target");
            }
        }

        ApplyStateLayers();
    }

    private int ClampValue(int value) => Math.Clamp(value, 0, Math.Max(0, MaxValue));

    private bool CanInteract => IsEnabled && !ReadOnly;

    private void ApplyEnabledState()
    {
        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
        Cursor = CanInteract ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
        if (_panel is not null)
        {
            _panel.Cursor = Cursor;
        }
    }

    private void ApplyStateLayers()
    {
        for (var i = 0; i < _stateLayers.Count; i++)
        {
            ApplyStateLayer(i, StateForIndex(i + 1));
        }
    }

    private string? StateForIndex(int index)
    {
        if (!CanInteract)
        {
            return null;
        }

        if (_pressed == index || (_keyPressed && FocusIndex() == index))
        {
            return "Pressed";
        }

        if (_hover == index)
        {
            return "Hover";
        }

        return IsFocused && FocusIndex() == index ? "Focus" : null;
    }

    private int FocusIndex()
    {
        if (MaxValue <= 0)
        {
            return 0;
        }

        return SelectedValue <= 0 ? 1 : ClampValue(SelectedValue);
    }

    private void ApplyStateLayer(int index, string? state)
    {
        if (index >= _stateLayers.Count)
        {
            return;
        }

        _stateLayerBackgrounds[index]?.Dispose();
        _stateLayerBackgrounds[index] = null;
        if (state is null)
        {
            _stateLayers[index].Background = Brushes.Transparent;
            return;
        }

        _stateLayerBackgrounds[index] = _stateLayers[index].Bind(Border.BackgroundProperty,
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

    private void ClearStateLayerBindings()
    {
        foreach (var binding in _stateLayerBackgrounds)
        {
            binding?.Dispose();
        }

        _stateLayerBackgrounds.Clear();
    }

    private void ApplyAutomation()
    {
        AutomationProperties.SetName(this, "Rating");
        AutomationProperties.SetHelpText(this, $"Rating {ClampValue(SelectedValue)} of {Math.Max(0, MaxValue)}");
    }
}
