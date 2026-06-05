using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A contextual message banner, mirroring the reference API's <c>Alert</c>. Colored by <see cref="Color"/>
/// (severity) and styled by <see cref="Variant"/> (Filled / Outlined / Text-tint), with an optional
/// leading <see cref="Icon"/>.
/// </summary>
public class Alert : ContentControl
{
    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Alert, LoamColor>(nameof(Color), LoamColor.Info);

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<Alert, Variant>(nameof(Variant), Loam.Variant.Text);

    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Icon));

    private Border? _root;
    private Icon? _iconPart;
    private IDisposable? _background;
    private IDisposable? _foreground;
    private IDisposable? _border;

    /// <summary>Severity color. Mirrors the reference API's <c>Severity</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Visual style. Mirrors the reference API's <c>Variant</c>.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Leading icon path. Mirrors the reference API's <c>Icon</c>.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Alert);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find("PART_Root") as Border;
        _iconPart = e.NameScope.Find("PART_Icon") as Icon;
        ApplyVisual();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColorProperty || change.Property == VariantProperty || change.Property == IconProperty ||
            change.Property == IsEnabledProperty)
        {
            ApplyVisual();
        }
        else if (change.Property == ContentProperty)
        {
            InteractionAssist.SetAutomationName(this, Content);
        }
    }

    private void ApplyVisual()
    {
        var tokens = SemanticColor.Resolve(Color);

        if (_iconPart is not null)
        {
            _iconPart.Data = Icon;
            _iconPart.IsVisible = !string.IsNullOrEmpty(Icon);
        }

        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
        InteractionAssist.SetAutomationName(this, Content);

        _foreground?.Dispose();
        _border?.Dispose();
        _border = null;
        _background?.Dispose();
        _background = null;

        if (_root is null)
        {
            return;
        }

        switch (Variant)
        {
            case Loam.Variant.Filled:
                _background = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));
                _foreground = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.FillText));
                _root.BorderThickness = default;
                break;
            case Loam.Variant.Outlined:
                _root.Background = Brushes.Transparent;
                _foreground = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.Accent));
                _root.BorderThickness = new Thickness(1);
                _border = _root.Bind(Border.BorderBrushProperty, this.GetResourceObservable(tokens.Border));
                break;
            default: // Text-tint
                _background = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Overlay));
                _foreground = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.Accent));
                _root.BorderThickness = default;
                break;
        }
    }
}
