using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
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
    private IDisposable? _trackBackground;
    private IDisposable? _thumbBackground;

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
        _track = e.NameScope.Find("PART_Track") as Border;
        _thumb = e.NameScope.Find("PART_Thumb") as Border;
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
        var isChecked = IsChecked == true;
        var tokens = SemanticColor.Resolve(Color);

        if (_thumb is not null)
        {
            _thumb.HorizontalAlignment = isChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            _thumbBackground?.Dispose();
            _thumbBackground = _thumb.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(isChecked ? tokens.Fill : LoamTokens.Surface));
        }

        if (_track is not null)
        {
            _track.Opacity = isChecked ? 0.5 : 1;
            _trackBackground?.Dispose();
            _trackBackground = _track.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(isChecked ? tokens.Fill : LoamTokens.Palette(nameof(LoamPalette.GrayLight))));
        }

        Opacity = IsEnabled ? 1 : 0.5;
    }
}
