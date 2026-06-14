namespace Loam.Theming;

/// <summary>
/// The complete Loam design specification — the Avalonia analogue of the reference API's <c>Theme</c>.
/// Pure data (no Avalonia <c>Styles</c>), so it is trivially constructed, customized with
/// <c>with</c>, and unit-tested. <see cref="LoamTheme"/> projects it into Avalonia resources.
/// </summary>
public sealed record LoamThemeData
{
    /// <summary>Role-based color scheme used under <see cref="Avalonia.Styling.ThemeVariant.Light"/>.</summary>
    public LoamColorScheme ColorSchemeLight { get; init; } = LoamColorScheme.DefaultLight;

    /// <summary>Role-based color scheme used under <see cref="Avalonia.Styling.ThemeVariant.Dark"/>.</summary>
    public LoamColorScheme ColorSchemeDark { get; init; } = LoamColorScheme.DefaultDark;

    /// <summary>Palette used under <see cref="Avalonia.Styling.ThemeVariant.Light"/>.</summary>
    public LoamPalette PaletteLight { get; init; } = LoamPalette.DefaultLight;

    /// <summary>Palette used under <see cref="Avalonia.Styling.ThemeVariant.Dark"/>.</summary>
    public LoamPalette PaletteDark { get; init; } = LoamPalette.DefaultDark;

    /// <summary>Type scale (shared across variants).</summary>
    public LoamTypography Typography { get; init; } = new();

    /// <summary>Elevation shadows (shared across variants).</summary>
    public LoamShadows Shadows { get; init; } = LoamShadows.Default;

    /// <summary>Tonal and shadow elevation mapping.</summary>
    public LoamElevation Elevation { get; init; } = LoamElevation.Default;

    /// <summary>Layout metrics.</summary>
    public LoamLayout Layout { get; init; } = LoamLayout.Default;

    /// <summary>Spacing scale.</summary>
    public LoamSpacing Spacing { get; init; } = LoamSpacing.Default;

    /// <summary>Stroke widths.</summary>
    public LoamStroke Stroke { get; init; } = LoamStroke.Default;

    /// <summary>Shared component density metrics.</summary>
    public LoamDensity Density { get; init; } = LoamDensity.Default;

    /// <summary>Overlay stacking order.</summary>
    public LoamZIndex ZIndex { get; init; } = LoamZIndex.Default;

    /// <summary>Corner radii used by component families.</summary>
    public LoamShape Shape { get; init; } = LoamShape.Default;

    /// <summary>Interaction state opacity values.</summary>
    public LoamStateLayer StateLayer { get; init; } = LoamStateLayer.Default;

    /// <summary>Motion durations and easing names.</summary>
    public LoamMotion Motion { get; init; } = LoamMotion.Default;

    /// <summary>Shared field-style input metrics.</summary>
    public LoamFieldMetrics FieldMetrics { get; init; } = LoamFieldMetrics.Default;

    /// <summary>The Loam defaults.</summary>
    public static LoamThemeData Default { get; } = new();

    /// <summary>
    /// Builds a theme from a single seed color (Material You): generates light + dark
    /// <see cref="LoamColorScheme"/>s and matching compatibility palettes, keeping the default
    /// typography/shape/spacing/etc. See ADR-0012.
    /// </summary>
    public static LoamThemeData FromSeed(Avalonia.Media.Color seed, LoamContrast contrast = LoamContrast.Standard)
    {
        var light = LoamColorScheme.FromSeed(seed, dark: false, contrast);
        var dark = LoamColorScheme.FromSeed(seed, dark: true, contrast);
        return Default with
        {
            ColorSchemeLight = light,
            ColorSchemeDark = dark,
            PaletteLight = light.ToPalette(),
            PaletteDark = dark.ToPalette(),
        };
    }

    /// <summary>Legacy visual preset retained for migration.</summary>
    public static LoamThemeData Legacy { get; } = new()
    {
        ColorSchemeLight = LoamColorScheme.FromPalette(LoamPalette.LegacyLight),
        ColorSchemeDark = LoamColorScheme.FromPalette(LoamPalette.LegacyDark),
        PaletteLight = LoamPalette.LegacyLight,
        PaletteDark = LoamPalette.LegacyDark,
        Shape = new LoamShape
        {
            ExtraSmall = new Avalonia.CornerRadius(4),
            Small = new Avalonia.CornerRadius(4),
            Medium = new Avalonia.CornerRadius(8),
            Large = new Avalonia.CornerRadius(12),
        },
        Motion = new LoamMotion
        {
            DurationShort = TimeSpan.FromMilliseconds(120),
            DurationMedium = TimeSpan.FromMilliseconds(180),
            DurationLong = TimeSpan.FromMilliseconds(280),
        },
    };
}
