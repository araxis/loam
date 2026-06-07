using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A thin separating line, mirroring the reference API's <c>Divider</c>. Renders as a token-colored
/// 1px line; supports horizontal/vertical orientation, a lighter shade, and inset/middle insets.
/// </summary>
public class Divider : Border
{
    /// <summary>Identifies the <see cref="Vertical"/> property.</summary>
    public static readonly StyledProperty<bool> VerticalProperty =
        AvaloniaProperty.Register<Divider, bool>(nameof(Vertical));

    /// <summary>Identifies the <see cref="Light"/> property.</summary>
    public static readonly StyledProperty<bool> LightProperty =
        AvaloniaProperty.Register<Divider, bool>(nameof(Light));

    /// <summary>Identifies the <see cref="DividerType"/> property.</summary>
    public static readonly StyledProperty<DividerType> DividerTypeProperty =
        AvaloniaProperty.Register<Divider, DividerType>(nameof(DividerType), DividerType.FullWidth);

    private IDisposable? _brushBinding;

    /// <summary>Creates the divider.</summary>
    public Divider()
    {
        AutomationProperties.SetName(this, "Divider");
        Apply();
    }

    /// <summary>Renders vertically instead of horizontally. Mirrors the reference API's <c>Vertical</c>.</summary>
    public bool Vertical
    {
        get => GetValue(VerticalProperty);
        set => SetValue(VerticalProperty, value);
    }

    /// <summary>Uses the lighter divider color. Mirrors the reference API's <c>Light</c>.</summary>
    public bool Light
    {
        get => GetValue(LightProperty);
        set => SetValue(LightProperty, value);
    }

    /// <summary>Inset style. Mirrors the reference API's <c>DividerType</c>.</summary>
    public DividerType DividerType
    {
        get => GetValue(DividerTypeProperty);
        set => SetValue(DividerTypeProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Divider);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == VerticalProperty ||
            change.Property == LightProperty ||
            change.Property == DividerTypeProperty)
        {
            Apply();
        }
    }

    private void Apply()
    {
        _brushBinding?.Dispose();
        var key = Light ? LoamTokens.Palette(nameof(LoamPalette.DividerLight)) : LoamTokens.Divider;
        _brushBinding = this.Bind(BackgroundProperty, this.GetResourceObservable(key));

        var inset = DividerType switch
        {
            DividerType.Inset => 16d,
            DividerType.Middle => 16d,
            _ => 0d,
        };
        var trailing = DividerType == DividerType.Middle ? 16d : 0d;

        if (Vertical)
        {
            Width = 1;
            Height = double.NaN;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Stretch;
            Margin = new Thickness(0, inset, 0, trailing);
        }
        else
        {
            Height = 1;
            Width = double.NaN;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Center;
            Margin = new Thickness(inset, 0, trailing, 0);
        }
    }
}
