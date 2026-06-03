namespace Loam.Theming;

/// <summary>
/// Stable resource keys for Loam design tokens. Controls bind to these (never to literal
/// colors/sizes) so a single <see cref="LoamTheme"/> drives the whole app and runtime/variant
/// swaps re-style everything (ADR-0005). Keys are namespaced <c>Loam.&lt;group&gt;.&lt;name&gt;</c>.
/// </summary>
/// <remarks>
/// The full palette is projected by reflection over <see cref="LoamPalette"/> property names, so
/// <c>Palette(nameof(LoamPalette.X))</c> always resolves even without a dedicated constant here.
/// The constants below cover the most frequently bound tokens for ergonomics.
/// </remarks>
public static class LoamTokens
{
    /// <summary>Resource key for a palette color brush, e.g. <c>Palette(nameof(LoamPalette.Primary))</c>.</summary>
    public static string Palette(string name) => $"Loam.Palette.{name}";

    /// <summary>Resource key for a semantic color's contrast (on-color) text brush.</summary>
    public static string PaletteContrast(string name) => $"Loam.Palette.{name}ContrastText";

    /// <summary>Resource key for a semantic color's hover overlay brush (color at hover opacity).</summary>
    public static string PaletteHover(string name) => $"Loam.Palette.{name}.Hover";

    /// <summary>Resource key for a semantic color's darkened brush (pressed/active fill).</summary>
    public static string PaletteDarken(string name) => $"Loam.Palette.{name}.Darken";

    // Common palette brushes
    /// <summary>Primary accent brush.</summary>
    public const string Primary = "Loam.Palette.Primary";
    /// <summary>Brush for content on top of <see cref="Primary"/>.</summary>
    public const string PrimaryContrastText = "Loam.Palette.PrimaryContrastText";
    /// <summary>Secondary accent brush.</summary>
    public const string Secondary = "Loam.Palette.Secondary";
    /// <summary>Tertiary accent brush.</summary>
    public const string Tertiary = "Loam.Palette.Tertiary";
    /// <summary>Info brush.</summary>
    public const string Info = "Loam.Palette.Info";
    /// <summary>Success brush.</summary>
    public const string Success = "Loam.Palette.Success";
    /// <summary>Warning brush.</summary>
    public const string Warning = "Loam.Palette.Warning";
    /// <summary>Error brush.</summary>
    public const string Error = "Loam.Palette.Error";
    /// <summary>Elevated surface background brush.</summary>
    public const string Surface = "Loam.Palette.Surface";
    /// <summary>Page/app background brush.</summary>
    public const string Background = "Loam.Palette.Background";
    /// <summary>Primary text brush.</summary>
    public const string TextPrimary = "Loam.Palette.TextPrimary";
    /// <summary>Secondary text brush.</summary>
    public const string TextSecondary = "Loam.Palette.TextSecondary";
    /// <summary>Disabled text brush.</summary>
    public const string TextDisabled = "Loam.Palette.TextDisabled";
    /// <summary>Default border/line brush.</summary>
    public const string LinesDefault = "Loam.Palette.LinesDefault";
    /// <summary>Divider brush.</summary>
    public const string Divider = "Loam.Palette.Divider";
    /// <summary>Default action/icon brush.</summary>
    public const string ActionDefault = "Loam.Palette.ActionDefault";

    // Typography
    /// <summary>Default font family.</summary>
    public const string FontFamily = "Loam.Typography.FontFamily";
    /// <summary>Resource key for a type scale's font size (double, px).</summary>
    public static string TypographyFontSize(string name) => $"Loam.Typography.{name}.FontSize";
    /// <summary>Resource key for a type scale's font weight.</summary>
    public static string TypographyFontWeight(string name) => $"Loam.Typography.{name}.FontWeight";
    /// <summary>Resource key for a type scale's absolute line height (double, px).</summary>
    public static string TypographyLineHeight(string name) => $"Loam.Typography.{name}.LineHeight";

    // Elevation
    /// <summary>Resource key for an elevation box-shadow set (0–25).</summary>
    public static string Elevation(int level) => $"Loam.Elevation.{level}";
    /// <summary>Elevation level 1 box-shadow set.</summary>
    public const string Elevation1 = "Loam.Elevation.1";

    // Layout
    /// <summary>Default control corner radius.</summary>
    public const string DefaultCornerRadius = "Loam.Layout.CornerRadius";
    /// <summary>Expanded drawer width (double).</summary>
    public const string DrawerWidth = "Loam.Layout.DrawerWidth";
    /// <summary>Collapsed (mini) drawer width (double).</summary>
    public const string DrawerMiniWidth = "Loam.Layout.DrawerMiniWidth";
    /// <summary>App bar height (double).</summary>
    public const string AppBarHeight = "Loam.Layout.AppBarHeight";

    // Z-index
    /// <summary>Resource key for an overlay z-index (int), e.g. <c>ZIndex(nameof(LoamZIndex.Dialog))</c>.</summary>
    public static string ZIndex(string name) => $"Loam.ZIndex.{name}";
}
