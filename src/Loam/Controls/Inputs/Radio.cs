using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A Material radio button, mirroring MudBlazor's <c>MudRadio</c>. Subclasses Avalonia's
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
    private IDisposable? _ringBorder;
    private IDisposable? _dotBackground;

    /// <summary>Selected color. Mirrors MudBlazor's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Radio size. Mirrors MudBlazor's <c>Size</c>.</summary>
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
        ApplyVisual();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsCheckedProperty || change.Property == ColorProperty ||
            change.Property == SizeProperty || change.Property == IsEnabledProperty)
        {
            ApplyVisual();
        }
    }

    private void ApplyVisual()
    {
        var size = Size switch { LoamSize.Small => 16d, LoamSize.Large => 24d, _ => 20d };
        var isChecked = IsChecked == true;
        var tokens = SemanticColor.Resolve(Color);

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

        Opacity = IsEnabled ? 1 : 0.5;
    }
}
