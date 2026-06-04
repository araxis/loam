using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
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
    private AvaPath? _check;
    private IDisposable? _boxBackground;
    private IDisposable? _boxBorder;
    private IDisposable? _checkFill;

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
        _check = e.NameScope.Find("PART_Check") as AvaPath;
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
        var size = Size switch { LoamSize.Small => 18d, LoamSize.Large => 24d, _ => 20d };
        var isChecked = IsChecked == true;
        var tokens = SemanticColor.Resolve(Color);

        if (_box is not null)
        {
            _box.Width = size;
            _box.Height = size;
            _boxBackground?.Dispose();
            _boxBorder?.Dispose();
            _boxBorder = null;

            if (isChecked)
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
            _check.IsVisible = isChecked;
            _checkFill?.Dispose();
            _checkFill = _check.Bind(Shape.FillProperty, this.GetResourceObservable(tokens.FillText));
        }

        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
    }
}
