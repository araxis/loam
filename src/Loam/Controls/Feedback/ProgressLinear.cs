using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A horizontal progress bar, mirroring MudBlazor's <c>MudProgressLinear</c>. Determinate fill from
/// <see cref="Value"/> within <see cref="Minimum"/>/<see cref="Maximum"/>, tinted by <see cref="Color"/>.
/// </summary>
public class ProgressLinear : TemplatedControl
{
    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<ProgressLinear, double>(nameof(Value));

    /// <summary>Identifies the <see cref="Minimum"/> property.</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<ProgressLinear, double>(nameof(Minimum), 0);

    /// <summary>Identifies the <see cref="Maximum"/> property.</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<ProgressLinear, double>(nameof(Maximum), 100);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<ProgressLinear, LoamColor>(nameof(Color), LoamColor.Primary);

    private Panel? _area;
    private Border? _fill;
    private IDisposable? _fillBackground;

    /// <summary>Current value. Mirrors MudBlazor's <c>Value</c>.</summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Minimum value. Mirrors MudBlazor's <c>Min</c>.</summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Maximum value. Mirrors MudBlazor's <c>Max</c>.</summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Accent color. Mirrors MudBlazor's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ProgressLinear);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _area = e.NameScope.Find("PART_Area") as Panel;
        _fill = e.NameScope.Find("PART_Fill") as Border;
        ApplyColor();
        UpdateFill();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || change.Property == MinimumProperty ||
            change.Property == MaximumProperty || change.Property == BoundsProperty)
        {
            UpdateFill();
        }
        else if (change.Property == ColorProperty)
        {
            ApplyColor();
        }
    }

    private void ApplyColor()
    {
        if (_fill is null)
        {
            return;
        }

        _fillBackground?.Dispose();
        _fillBackground = _fill.Bind(Border.BackgroundProperty, this.GetResourceObservable(SemanticColor.Resolve(Color).Fill));
    }

    private void UpdateFill()
    {
        if (_area is null || _fill is null)
        {
            return;
        }

        var fraction = Maximum <= Minimum ? 0 : Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0, 1);
        _fill.Width = fraction * _area.Bounds.Width;
    }
}
