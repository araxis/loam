using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A top application bar, mirroring MudBlazor's <c>MudAppBar</c>. A full-width, elevated, colored
/// toolbar surface. Default color uses the theme's app-bar palette; a semantic <see cref="Color"/>
/// overrides it. Host a horizontal toolbar (menu button, title, actions) as its content.
/// </summary>
public class AppBar : ContentControl
{
    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<AppBar, LoamColor>(nameof(Color), LoamColor.Default);

    /// <summary>Identifies the <see cref="Elevation"/> property.</summary>
    public static readonly StyledProperty<int> ElevationProperty =
        AvaloniaProperty.Register<AppBar, int>(nameof(Elevation), 4);

    /// <summary>Identifies the <see cref="Dense"/> property.</summary>
    public static readonly StyledProperty<bool> DenseProperty =
        AvaloniaProperty.Register<AppBar, bool>(nameof(Dense));

    private Border? _root;
    private IDisposable? _backgroundBinding;
    private IDisposable? _foregroundBinding;
    private IDisposable? _shadowBinding;

    /// <summary>App-bar color. Mirrors MudBlazor's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Shadow depth. Mirrors MudBlazor's <c>Elevation</c>.</summary>
    public int Elevation
    {
        get => GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>Reduced height. Mirrors MudBlazor's <c>Dense</c>.</summary>
    public bool Dense
    {
        get => GetValue(DenseProperty);
        set => SetValue(DenseProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(AppBar);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find("PART_Root") as Border;
        ApplyVisual();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColorProperty || change.Property == ElevationProperty || change.Property == DenseProperty)
        {
            ApplyVisual();
        }
    }

    private void ApplyVisual()
    {
        Height = Dense ? 48 : 64;

        string backgroundKey;
        string foregroundKey;
        if (Color is LoamColor.Default or LoamColor.Inherit)
        {
            backgroundKey = LoamTokens.Palette(nameof(LoamPalette.AppbarBackground));
            foregroundKey = LoamTokens.Palette(nameof(LoamPalette.AppbarText));
        }
        else
        {
            var tokens = SemanticColor.Resolve(Color);
            backgroundKey = tokens.Fill;
            foregroundKey = tokens.FillText;
        }

        _foregroundBinding?.Dispose();
        _foregroundBinding = this.Bind(ForegroundProperty, this.GetResourceObservable(foregroundKey));

        if (_root is null)
        {
            return;
        }

        _backgroundBinding?.Dispose();
        _backgroundBinding = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(backgroundKey));
        _shadowBinding?.Dispose();
        _shadowBinding = _root.Bind(Border.BoxShadowProperty, this.GetResourceObservable(LoamTokens.Elevation(Elevation)));
    }
}
