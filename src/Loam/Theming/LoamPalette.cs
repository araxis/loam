using Avalonia.Media;
using static Loam.Theming.LoamColors;

namespace Loam.Theming;

/// <summary>
/// A semantic color palette mirroring the reference API's <c>Palette</c> (all default values verified
/// against Material Design v9.5.0). The record's property defaults are the <b>light</b> palette;
/// <see cref="Dark"/> is derived with <c>with</c> overriding only what differs — exactly how
/// the reference API's <c>PaletteDark</c> extends the base.
/// </summary>
public sealed record LoamPalette
{
    // Brand / semantic
    /// <summary>Primary accent.</summary>
    public Color Primary { get; init; } = Color.Parse("#594AE2");
    /// <summary>Text/icon color on top of <see cref="Primary"/>.</summary>
    public Color PrimaryContrastText { get; init; } = Colors.White;
    /// <summary>Secondary accent.</summary>
    public Color Secondary { get; init; } = Color.Parse("#FF4081");
    /// <summary>Text/icon color on top of <see cref="Secondary"/>.</summary>
    public Color SecondaryContrastText { get; init; } = Colors.White;
    /// <summary>Tertiary accent.</summary>
    public Color Tertiary { get; init; } = Color.Parse("#1EC8A5");
    /// <summary>Text/icon color on top of <see cref="Tertiary"/>.</summary>
    public Color TertiaryContrastText { get; init; } = Colors.White;
    /// <summary>Informational color.</summary>
    public Color Info { get; init; } = Color.Parse("#2196F3");
    /// <summary>Text/icon color on top of <see cref="Info"/>.</summary>
    public Color InfoContrastText { get; init; } = Colors.White;
    /// <summary>Success color.</summary>
    public Color Success { get; init; } = Color.Parse("#00C853");
    /// <summary>Text/icon color on top of <see cref="Success"/>.</summary>
    public Color SuccessContrastText { get; init; } = Colors.White;
    /// <summary>Warning color.</summary>
    public Color Warning { get; init; } = Color.Parse("#FF9800");
    /// <summary>Text/icon color on top of <see cref="Warning"/>.</summary>
    public Color WarningContrastText { get; init; } = Colors.White;
    /// <summary>Error color.</summary>
    public Color Error { get; init; } = Color.Parse("#F44336");
    /// <summary>Text/icon color on top of <see cref="Error"/>.</summary>
    public Color ErrorContrastText { get; init; } = Colors.White;
    /// <summary>Neutral dark color.</summary>
    public Color Dark { get; init; } = Color.Parse("#424242");
    /// <summary>Text/icon color on top of <see cref="Dark"/>.</summary>
    public Color DarkContrastText { get; init; } = Colors.White;

    // Text
    /// <summary>Primary text color.</summary>
    public Color TextPrimary { get; init; } = Color.Parse("#424242");
    /// <summary>Secondary text color.</summary>
    public Color TextSecondary { get; init; } = BlackAlpha(0.54);
    /// <summary>Disabled text color.</summary>
    public Color TextDisabled { get; init; } = BlackAlpha(0.38);

    // Action
    /// <summary>Default icon/action color.</summary>
    public Color ActionDefault { get; init; } = BlackAlpha(0.54);
    /// <summary>Disabled action color.</summary>
    public Color ActionDisabled { get; init; } = BlackAlpha(0.26);
    /// <summary>Disabled action background.</summary>
    public Color ActionDisabledBackground { get; init; } = BlackAlpha(0.12);

    // Surfaces
    /// <summary>Page/app background.</summary>
    public Color Background { get; init; } = Colors.White;
    /// <summary>Secondary gray background.</summary>
    public Color BackgroundGray { get; init; } = Color.Parse("#F5F5F5");
    /// <summary>Elevated surface background (cards/papers/menus).</summary>
    public Color Surface { get; init; } = Colors.White;
    /// <summary>Drawer background.</summary>
    public Color DrawerBackground { get; init; } = Colors.White;
    /// <summary>Drawer text.</summary>
    public Color DrawerText { get; init; } = Color.Parse("#424242");
    /// <summary>Drawer icon.</summary>
    public Color DrawerIcon { get; init; } = Color.Parse("#616161");
    /// <summary>App bar background.</summary>
    public Color AppbarBackground { get; init; } = Color.Parse("#594AE2");
    /// <summary>App bar text.</summary>
    public Color AppbarText { get; init; } = Colors.White;

    // Lines / dividers / tables
    /// <summary>Default border/line color.</summary>
    public Color LinesDefault { get; init; } = BlackAlpha(0.12);
    /// <summary>Input underline/border color.</summary>
    public Color LinesInputs { get; init; } = Color.Parse("#BDBDBD");
    /// <summary>Table row lines.</summary>
    public Color TableLines { get; init; } = Color.Parse("#E0E0E0");
    /// <summary>Striped table row background.</summary>
    public Color TableStriped { get; init; } = BlackAlpha(0.02);
    /// <summary>Hovered table row background.</summary>
    public Color TableHover { get; init; } = BlackAlpha(0.04);
    /// <summary>Divider color.</summary>
    public Color Divider { get; init; } = Color.Parse("#E0E0E0");
    /// <summary>Light divider color.</summary>
    public Color DividerLight { get; init; } = BlackAlpha(0.8);
    /// <summary>Skeleton placeholder color.</summary>
    public Color Skeleton { get; init; } = BlackAlpha(0.11);

    // Neutrals / overlays
    /// <summary>Near-black brand neutral.</summary>
    public Color Black { get; init; } = Color.Parse("#272C34");
    /// <summary>White.</summary>
    public Color White { get; init; } = Colors.White;
    /// <summary>Default gray.</summary>
    public Color GrayDefault { get; init; } = Color.Parse("#9E9E9E");
    /// <summary>Light gray.</summary>
    public Color GrayLight { get; init; } = Color.Parse("#BDBDBD");
    /// <summary>Lighter gray.</summary>
    public Color GrayLighter { get; init; } = Color.Parse("#E0E0E0");
    /// <summary>Dark gray.</summary>
    public Color GrayDark { get; init; } = Color.Parse("#757575");
    /// <summary>Darker gray.</summary>
    public Color GrayDarker { get; init; } = Color.Parse("#616161");
    /// <summary>Dark scrim overlay.</summary>
    public Color OverlayDark { get; init; } = Color.Parse("#212121").WithAlpha(0.5);
    /// <summary>Light scrim overlay.</summary>
    public Color OverlayLight { get; init; } = WhiteAlpha(0.5);

    // Interaction opacities (0–1)
    /// <summary>Hover state overlay opacity.</summary>
    public double HoverOpacity { get; init; } = 0.06;
    /// <summary>Ripple overlay opacity.</summary>
    public double RippleOpacity { get; init; } = 0.1;

    /// <summary>The default light palette.</summary>
    public static LoamPalette DefaultLight { get; } = new();

    /// <summary>The default dark palette (overrides only what differs from light).</summary>
    public static LoamPalette DefaultDark { get; } = DefaultLight with
    {
        Black = Color.Parse("#27272F"),
        Primary = Color.Parse("#776BE7"),
        Info = Color.Parse("#3299FF"),
        Success = Color.Parse("#0BBA83"),
        Warning = Color.Parse("#FFA800"),
        Error = Color.Parse("#F64E62"),
        Dark = Color.Parse("#27272F"),
        TextPrimary = WhiteAlpha(0.70),
        TextSecondary = WhiteAlpha(0.50),
        TextDisabled = WhiteAlpha(0.20),
        ActionDefault = Color.Parse("#ADADB1"),
        ActionDisabled = WhiteAlpha(0.26),
        ActionDisabledBackground = WhiteAlpha(0.12),
        Background = Color.Parse("#32333D"),
        BackgroundGray = Color.Parse("#27272F"),
        Surface = Color.Parse("#373740"),
        DrawerBackground = Color.Parse("#27272F"),
        DrawerText = WhiteAlpha(0.50),
        DrawerIcon = WhiteAlpha(0.50),
        AppbarBackground = Color.Parse("#27272F"),
        AppbarText = WhiteAlpha(0.70),
        LinesDefault = WhiteAlpha(0.12),
        LinesInputs = WhiteAlpha(0.30),
        TableLines = WhiteAlpha(0.12),
        TableStriped = WhiteAlpha(0.20),
        Divider = WhiteAlpha(0.12),
        DividerLight = WhiteAlpha(0.06),
        Skeleton = WhiteAlpha(0.11),
    };
}
