using Avalonia.Media;
using static Loam.Theming.LoamColors;

namespace Loam.Theming;

/// <summary>
/// Compatibility palette mirroring the reference-shaped API. The rebaselined design system uses
/// <see cref="LoamColorScheme"/> internally; this record remains for existing Loam apps and maps
/// into role-based color resources.
/// </summary>
public sealed record LoamPalette
{
    // Brand / semantic
    /// <summary>Primary accent.</summary>
    public Color Primary { get; init; } = Color.Parse("#6750A4");
    /// <summary>Text/icon color on top of <see cref="Primary"/>.</summary>
    public Color PrimaryContrastText { get; init; } = Colors.White;
    /// <summary>Secondary accent.</summary>
    public Color Secondary { get; init; } = Color.Parse("#625B71");
    /// <summary>Text/icon color on top of <see cref="Secondary"/>.</summary>
    public Color SecondaryContrastText { get; init; } = Colors.White;
    /// <summary>Tertiary accent.</summary>
    public Color Tertiary { get; init; } = Color.Parse("#7D5260");
    /// <summary>Text/icon color on top of <see cref="Tertiary"/>.</summary>
    public Color TertiaryContrastText { get; init; } = Colors.White;
    /// <summary>Informational color.</summary>
    public Color Info { get; init; } = Color.Parse("#7D5260");
    /// <summary>Text/icon color on top of <see cref="Info"/>.</summary>
    public Color InfoContrastText { get; init; } = Colors.White;
    /// <summary>Success color.</summary>
    public Color Success { get; init; } = Color.Parse("#386A20");
    /// <summary>Text/icon color on top of <see cref="Success"/>.</summary>
    public Color SuccessContrastText { get; init; } = Colors.White;
    /// <summary>Warning color.</summary>
    public Color Warning { get; init; } = Color.Parse("#765A00");
    /// <summary>Text/icon color on top of <see cref="Warning"/>.</summary>
    public Color WarningContrastText { get; init; } = Colors.White;
    /// <summary>Error color.</summary>
    public Color Error { get; init; } = Color.Parse("#B3261E");
    /// <summary>Text/icon color on top of <see cref="Error"/>.</summary>
    public Color ErrorContrastText { get; init; } = Colors.White;
    /// <summary>Neutral dark color.</summary>
    public Color Dark { get; init; } = Color.Parse("#313033");
    /// <summary>Text/icon color on top of <see cref="Dark"/>.</summary>
    public Color DarkContrastText { get; init; } = Color.Parse("#F4EFF4");

    // Text
    /// <summary>Primary text color.</summary>
    public Color TextPrimary { get; init; } = Color.Parse("#1C1B1F");
    /// <summary>Secondary text color.</summary>
    public Color TextSecondary { get; init; } = Color.Parse("#49454F");
    /// <summary>Disabled text color.</summary>
    public Color TextDisabled { get; init; } = Color.Parse("#1C1B1F").WithAlpha(0.38);

    // Action
    /// <summary>Default icon/action color.</summary>
    public Color ActionDefault { get; init; } = Color.Parse("#49454F");
    /// <summary>Disabled action color.</summary>
    public Color ActionDisabled { get; init; } = Color.Parse("#1C1B1F").WithAlpha(0.38);
    /// <summary>Disabled action background.</summary>
    public Color ActionDisabledBackground { get; init; } = Color.Parse("#1C1B1F").WithAlpha(0.12);

    // Surfaces
    /// <summary>Page/app background.</summary>
    public Color Background { get; init; } = Color.Parse("#FFFBFE");
    /// <summary>Secondary gray background.</summary>
    public Color BackgroundGray { get; init; } = Color.Parse("#F7F2FA");
    /// <summary>Elevated surface background (cards/papers/menus).</summary>
    public Color Surface { get; init; } = Color.Parse("#FFFBFE");
    /// <summary>Drawer background.</summary>
    public Color DrawerBackground { get; init; } = Color.Parse("#F3EDF7");
    /// <summary>Drawer text.</summary>
    public Color DrawerText { get; init; } = Color.Parse("#1C1B1F");
    /// <summary>Drawer icon.</summary>
    public Color DrawerIcon { get; init; } = Color.Parse("#49454F");
    /// <summary>App bar background.</summary>
    public Color AppbarBackground { get; init; } = Color.Parse("#FFFBFE");
    /// <summary>App bar text.</summary>
    public Color AppbarText { get; init; } = Color.Parse("#1C1B1F");

    // Lines / dividers / tables
    /// <summary>Default border/line color.</summary>
    public Color LinesDefault { get; init; } = Color.Parse("#CAC4D0");
    /// <summary>Input underline/border color.</summary>
    public Color LinesInputs { get; init; } = Color.Parse("#79747E");
    /// <summary>Table row lines.</summary>
    public Color TableLines { get; init; } = Color.Parse("#CAC4D0");
    /// <summary>Striped table row background.</summary>
    public Color TableStriped { get; init; } = Color.Parse("#F7F2FA");
    /// <summary>Hovered table row background.</summary>
    public Color TableHover { get; init; } = Color.Parse("#ECE6F0");
    /// <summary>Divider color.</summary>
    public Color Divider { get; init; } = Color.Parse("#CAC4D0");
    /// <summary>Light divider color.</summary>
    public Color DividerLight { get; init; } = Color.Parse("#CAC4D0").WithAlpha(0.6);
    /// <summary>Skeleton placeholder color.</summary>
    public Color Skeleton { get; init; } = Color.Parse("#1C1B1F").WithAlpha(0.11);

    // Neutrals / overlays
    /// <summary>Near-black brand neutral.</summary>
    public Color Black { get; init; } = Colors.Black;
    /// <summary>White.</summary>
    public Color White { get; init; } = Colors.White;
    /// <summary>Default gray.</summary>
    public Color GrayDefault { get; init; } = Color.Parse("#79747E");
    /// <summary>Light gray.</summary>
    public Color GrayLight { get; init; } = Color.Parse("#CAC4D0");
    /// <summary>Lighter gray.</summary>
    public Color GrayLighter { get; init; } = Color.Parse("#E6E0E9");
    /// <summary>Dark gray.</summary>
    public Color GrayDark { get; init; } = Color.Parse("#49454F");
    /// <summary>Darker gray.</summary>
    public Color GrayDarker { get; init; } = Color.Parse("#1C1B1F");
    /// <summary>Dark scrim overlay.</summary>
    public Color OverlayDark { get; init; } = Colors.Black.WithAlpha(0.32);
    /// <summary>Light scrim overlay.</summary>
    public Color OverlayLight { get; init; } = WhiteAlpha(0.6);

    // Interaction opacities (0–1)
    /// <summary>Hover state overlay opacity.</summary>
    public double HoverOpacity { get; init; } = 0.08;
    /// <summary>Ripple overlay opacity.</summary>
    public double RippleOpacity { get; init; } = 0.1;

    /// <summary>The default light compatibility palette.</summary>
    public static LoamPalette DefaultLight { get; } = LoamColorScheme.DefaultLight.ToPalette();

    /// <summary>The default dark compatibility palette.</summary>
    public static LoamPalette DefaultDark { get; } = LoamColorScheme.DefaultDark.ToPalette();

    /// <summary>Legacy light palette retained as a migration preset.</summary>
    public static LoamPalette LegacyLight { get; } = new()
    {
        Primary = Color.Parse("#594AE2"),
        PrimaryContrastText = Colors.White,
        Secondary = Color.Parse("#FF4081"),
        SecondaryContrastText = Colors.White,
        Tertiary = Color.Parse("#1EC8A5"),
        TertiaryContrastText = Colors.White,
        Info = Color.Parse("#2196F3"),
        InfoContrastText = Colors.White,
        Success = Color.Parse("#00C853"),
        SuccessContrastText = Colors.White,
        Warning = Color.Parse("#FF9800"),
        WarningContrastText = Colors.White,
        Error = Color.Parse("#F44336"),
        ErrorContrastText = Colors.White,
        Dark = Color.Parse("#424242"),
        DarkContrastText = Colors.White,
        TextPrimary = Color.Parse("#424242"),
        TextSecondary = BlackAlpha(0.54),
        TextDisabled = BlackAlpha(0.38),
        ActionDefault = BlackAlpha(0.54),
        ActionDisabled = BlackAlpha(0.26),
        ActionDisabledBackground = BlackAlpha(0.12),
        Background = Colors.White,
        BackgroundGray = Color.Parse("#F5F5F5"),
        Surface = Colors.White,
        DrawerBackground = Colors.White,
        DrawerText = Color.Parse("#424242"),
        DrawerIcon = Color.Parse("#616161"),
        AppbarBackground = Color.Parse("#594AE2"),
        AppbarText = Colors.White,
        LinesDefault = BlackAlpha(0.12),
        LinesInputs = Color.Parse("#BDBDBD"),
        TableLines = Color.Parse("#E0E0E0"),
        TableStriped = BlackAlpha(0.02),
        TableHover = BlackAlpha(0.04),
        Divider = Color.Parse("#E0E0E0"),
        DividerLight = BlackAlpha(0.8),
        Skeleton = BlackAlpha(0.11),
        Black = Color.Parse("#272C34"),
        GrayDefault = Color.Parse("#9E9E9E"),
        GrayLight = Color.Parse("#BDBDBD"),
        GrayLighter = Color.Parse("#E0E0E0"),
        GrayDark = Color.Parse("#757575"),
        GrayDarker = Color.Parse("#616161"),
        OverlayDark = Color.Parse("#212121").WithAlpha(0.5),
        OverlayLight = WhiteAlpha(0.5),
        HoverOpacity = 0.06,
        RippleOpacity = 0.1,
    };

    /// <summary>Legacy dark palette retained as a migration preset.</summary>
    public static LoamPalette LegacyDark { get; } = LegacyLight with
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
