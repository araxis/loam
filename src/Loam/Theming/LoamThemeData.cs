namespace Loam.Theming;

/// <summary>
/// The complete Loam design specification — the Avalonia analogue of the reference API's <c>Theme</c>.
/// Pure data (no Avalonia <c>Styles</c>), so it is trivially constructed, customized with
/// <c>with</c>, and unit-tested. <see cref="LoamTheme"/> projects it into Avalonia resources.
/// </summary>
public sealed record LoamThemeData
{
    /// <summary>Palette used under <see cref="Avalonia.Styling.ThemeVariant.Light"/>.</summary>
    public LoamPalette PaletteLight { get; init; } = LoamPalette.DefaultLight;

    /// <summary>Palette used under <see cref="Avalonia.Styling.ThemeVariant.Dark"/>.</summary>
    public LoamPalette PaletteDark { get; init; } = LoamPalette.DefaultDark;

    /// <summary>Type scale (shared across variants).</summary>
    public LoamTypography Typography { get; init; } = new();

    /// <summary>Elevation shadows (shared across variants).</summary>
    public LoamShadows Shadows { get; init; } = LoamShadows.Default;

    /// <summary>Layout metrics.</summary>
    public LoamLayout Layout { get; init; } = LoamLayout.Default;

    /// <summary>Overlay stacking order.</summary>
    public LoamZIndex ZIndex { get; init; } = LoamZIndex.Default;

    /// <summary>The Loam defaults.</summary>
    public static LoamThemeData Default { get; } = new();
}
