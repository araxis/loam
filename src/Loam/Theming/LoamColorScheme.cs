using Avalonia.Media;

namespace Loam.Theming;

/// <summary>
/// Role-based color scheme used by the rebaselined Loam design system. Roles are paired by
/// intent: content placed on a role should use the matching <c>On*</c> role.
/// </summary>
public sealed record LoamColorScheme
{
    /// <summary>Main high-emphasis accent.</summary>
    public Color Primary { get; init; } = Color.Parse("#6750A4");
    /// <summary>Content on <see cref="Primary"/>.</summary>
    public Color OnPrimary { get; init; } = Colors.White;
    /// <summary>Lower-emphasis primary container.</summary>
    public Color PrimaryContainer { get; init; } = Color.Parse("#EADDFF");
    /// <summary>Content on <see cref="PrimaryContainer"/>.</summary>
    public Color OnPrimaryContainer { get; init; } = Color.Parse("#21005D");

    /// <summary>Secondary accent for less prominent expression.</summary>
    public Color Secondary { get; init; } = Color.Parse("#625B71");
    /// <summary>Content on <see cref="Secondary"/>.</summary>
    public Color OnSecondary { get; init; } = Colors.White;
    /// <summary>Lower-emphasis secondary container.</summary>
    public Color SecondaryContainer { get; init; } = Color.Parse("#E8DEF8");
    /// <summary>Content on <see cref="SecondaryContainer"/>.</summary>
    public Color OnSecondaryContainer { get; init; } = Color.Parse("#1D192B");

    /// <summary>Tertiary accent for complementary expression.</summary>
    public Color Tertiary { get; init; } = Color.Parse("#7D5260");
    /// <summary>Content on <see cref="Tertiary"/>.</summary>
    public Color OnTertiary { get; init; } = Colors.White;
    /// <summary>Lower-emphasis tertiary container.</summary>
    public Color TertiaryContainer { get; init; } = Color.Parse("#FFD8E4");
    /// <summary>Content on <see cref="TertiaryContainer"/>.</summary>
    public Color OnTertiaryContainer { get; init; } = Color.Parse("#31111D");

    /// <summary>Error semantic role.</summary>
    public Color Error { get; init; } = Color.Parse("#B3261E");
    /// <summary>Content on <see cref="Error"/>.</summary>
    public Color OnError { get; init; } = Colors.White;
    /// <summary>Error container role.</summary>
    public Color ErrorContainer { get; init; } = Color.Parse("#F9DEDC");
    /// <summary>Content on <see cref="ErrorContainer"/>.</summary>
    public Color OnErrorContainer { get; init; } = Color.Parse("#410E0B");

    /// <summary>App background.</summary>
    public Color Background { get; init; } = Color.Parse("#FFFBFE");
    /// <summary>Content on <see cref="Background"/>.</summary>
    public Color OnBackground { get; init; } = Color.Parse("#1C1B1F");
    /// <summary>Default surface.</summary>
    public Color Surface { get; init; } = Color.Parse("#FFFBFE");
    /// <summary>Content on <see cref="Surface"/>.</summary>
    public Color OnSurface { get; init; } = Color.Parse("#1C1B1F");
    /// <summary>Dim surface tone.</summary>
    public Color SurfaceDim { get; init; } = Color.Parse("#DED8E1");
    /// <summary>Bright surface tone.</summary>
    public Color SurfaceBright { get; init; } = Color.Parse("#FFFBFE");
    /// <summary>Lowest surface container tone.</summary>
    public Color SurfaceContainerLowest { get; init; } = Colors.White;
    /// <summary>Low surface container tone.</summary>
    public Color SurfaceContainerLow { get; init; } = Color.Parse("#F7F2FA");
    /// <summary>Default surface container tone.</summary>
    public Color SurfaceContainer { get; init; } = Color.Parse("#F3EDF7");
    /// <summary>High surface container tone.</summary>
    public Color SurfaceContainerHigh { get; init; } = Color.Parse("#ECE6F0");
    /// <summary>Highest surface container tone.</summary>
    public Color SurfaceContainerHighest { get; init; } = Color.Parse("#E6E0E9");
    /// <summary>Variant surface tone for lower-emphasis containers.</summary>
    public Color SurfaceVariant { get; init; } = Color.Parse("#E7E0EC");
    /// <summary>Content on <see cref="SurfaceVariant"/>.</summary>
    public Color OnSurfaceVariant { get; init; } = Color.Parse("#49454F");

    /// <summary>Default outline role.</summary>
    public Color Outline { get; init; } = Color.Parse("#79747E");
    /// <summary>Lower-emphasis outline role.</summary>
    public Color OutlineVariant { get; init; } = Color.Parse("#CAC4D0");
    /// <summary>Shadow color role.</summary>
    public Color Shadow { get; init; } = Colors.Black;
    /// <summary>Modal scrim role.</summary>
    public Color Scrim { get; init; } = Colors.Black;

    /// <summary>Inverse surface role.</summary>
    public Color InverseSurface { get; init; } = Color.Parse("#313033");
    /// <summary>Content on <see cref="InverseSurface"/>.</summary>
    public Color InverseOnSurface { get; init; } = Color.Parse("#F4EFF4");
    /// <summary>Inverse primary accent.</summary>
    public Color InversePrimary { get; init; } = Color.Parse("#D0BCFF");

    /// <summary>Fixed primary container role shared across variants.</summary>
    public Color PrimaryFixed { get; init; } = Color.Parse("#EADDFF");
    /// <summary>Dim fixed primary container role shared across variants.</summary>
    public Color PrimaryFixedDim { get; init; } = Color.Parse("#D0BCFF");
    /// <summary>Content on <see cref="PrimaryFixed"/>.</summary>
    public Color OnPrimaryFixed { get; init; } = Color.Parse("#21005D");
    /// <summary>Variant content on fixed primary roles.</summary>
    public Color OnPrimaryFixedVariant { get; init; } = Color.Parse("#4F378B");

    /// <summary>Fixed secondary container role shared across variants.</summary>
    public Color SecondaryFixed { get; init; } = Color.Parse("#E8DEF8");
    /// <summary>Dim fixed secondary container role shared across variants.</summary>
    public Color SecondaryFixedDim { get; init; } = Color.Parse("#CCC2DC");
    /// <summary>Content on <see cref="SecondaryFixed"/>.</summary>
    public Color OnSecondaryFixed { get; init; } = Color.Parse("#1D192B");
    /// <summary>Variant content on fixed secondary roles.</summary>
    public Color OnSecondaryFixedVariant { get; init; } = Color.Parse("#4A4458");

    /// <summary>Fixed tertiary container role shared across variants.</summary>
    public Color TertiaryFixed { get; init; } = Color.Parse("#FFD8E4");
    /// <summary>Dim fixed tertiary container role shared across variants.</summary>
    public Color TertiaryFixedDim { get; init; } = Color.Parse("#EFB8C8");
    /// <summary>Content on <see cref="TertiaryFixed"/>.</summary>
    public Color OnTertiaryFixed { get; init; } = Color.Parse("#31111D");
    /// <summary>Variant content on fixed tertiary roles.</summary>
    public Color OnTertiaryFixedVariant { get; init; } = Color.Parse("#633B48");

    /// <summary>Default light color scheme.</summary>
    public static LoamColorScheme DefaultLight { get; } = new();

    /// <summary>Default dark color scheme.</summary>
    public static LoamColorScheme DefaultDark { get; } = new()
    {
        Primary = Color.Parse("#D0BCFF"),
        OnPrimary = Color.Parse("#381E72"),
        PrimaryContainer = Color.Parse("#4F378B"),
        OnPrimaryContainer = Color.Parse("#EADDFF"),
        Secondary = Color.Parse("#CCC2DC"),
        OnSecondary = Color.Parse("#332D41"),
        SecondaryContainer = Color.Parse("#4A4458"),
        OnSecondaryContainer = Color.Parse("#E8DEF8"),
        Tertiary = Color.Parse("#EFB8C8"),
        OnTertiary = Color.Parse("#492532"),
        TertiaryContainer = Color.Parse("#633B48"),
        OnTertiaryContainer = Color.Parse("#FFD8E4"),
        Error = Color.Parse("#F2B8B5"),
        OnError = Color.Parse("#601410"),
        ErrorContainer = Color.Parse("#8C1D18"),
        OnErrorContainer = Color.Parse("#F9DEDC"),
        Background = Color.Parse("#1C1B1F"),
        OnBackground = Color.Parse("#E6E1E5"),
        Surface = Color.Parse("#1C1B1F"),
        OnSurface = Color.Parse("#E6E1E5"),
        SurfaceDim = Color.Parse("#141218"),
        SurfaceBright = Color.Parse("#3B383E"),
        SurfaceContainerLowest = Color.Parse("#0F0D13"),
        SurfaceContainerLow = Color.Parse("#1D1B20"),
        SurfaceContainer = Color.Parse("#211F26"),
        SurfaceContainerHigh = Color.Parse("#2B2930"),
        SurfaceContainerHighest = Color.Parse("#36343B"),
        SurfaceVariant = Color.Parse("#49454F"),
        OnSurfaceVariant = Color.Parse("#CAC4D0"),
        Outline = Color.Parse("#938F99"),
        OutlineVariant = Color.Parse("#49454F"),
        InverseSurface = Color.Parse("#E6E1E5"),
        InverseOnSurface = Color.Parse("#313033"),
        InversePrimary = Color.Parse("#6750A4"),
    };

    /// <summary>Maps this scheme into the compatibility palette used by older Loam APIs.</summary>
    public LoamPalette ToPalette() => new()
    {
        Primary = Primary,
        PrimaryContrastText = OnPrimary,
        Secondary = Secondary,
        SecondaryContrastText = OnSecondary,
        Tertiary = Tertiary,
        TertiaryContrastText = OnTertiary,
        Info = Tertiary,
        InfoContrastText = OnTertiary,
        Success = Color.Parse("#386A20"),
        SuccessContrastText = Colors.White,
        Warning = Color.Parse("#765A00"),
        WarningContrastText = Colors.White,
        Error = Error,
        ErrorContrastText = OnError,
        Dark = InverseSurface,
        DarkContrastText = InverseOnSurface,
        TextPrimary = OnSurface,
        TextSecondary = OnSurfaceVariant,
        TextDisabled = OnSurface.WithAlpha(0.38),
        ActionDefault = OnSurfaceVariant,
        ActionDisabled = OnSurface.WithAlpha(0.38),
        ActionDisabledBackground = OnSurface.WithAlpha(0.12),
        Background = Background,
        BackgroundGray = SurfaceContainerLow,
        Surface = Surface,
        DrawerBackground = SurfaceContainer,
        DrawerText = OnSurface,
        DrawerIcon = OnSurfaceVariant,
        AppbarBackground = Surface,
        AppbarText = OnSurface,
        LinesDefault = OutlineVariant,
        LinesInputs = Outline,
        TableLines = OutlineVariant,
        TableStriped = SurfaceContainerLow,
        TableHover = SurfaceContainerHigh,
        Divider = OutlineVariant,
        DividerLight = OutlineVariant.WithAlpha(0.6),
        Skeleton = OnSurface.WithAlpha(0.11),
        Black = Shadow,
        White = Colors.White,
        GrayDefault = Outline,
        GrayLight = OutlineVariant,
        GrayLighter = SurfaceContainerHighest,
        GrayDark = OnSurfaceVariant,
        GrayDarker = OnSurface,
        OverlayDark = Scrim.WithAlpha(0.32),
        OverlayLight = Colors.White.WithAlpha(0.6),
        HoverOpacity = LoamStateLayer.Default.HoverOpacity,
        RippleOpacity = LoamStateLayer.Default.PressedOpacity,
    };

    /// <summary>Creates a color scheme from the compatibility palette.</summary>
    public static LoamColorScheme FromPalette(LoamPalette palette) => DefaultLight with
    {
        Primary = palette.Primary,
        OnPrimary = palette.PrimaryContrastText,
        PrimaryContainer = palette.Primary.Lighten(0.78),
        OnPrimaryContainer = palette.Primary.Darken(0.72).ContrastText(),
        Secondary = palette.Secondary,
        OnSecondary = palette.SecondaryContrastText,
        SecondaryContainer = palette.Secondary.Lighten(0.78),
        OnSecondaryContainer = palette.Secondary.Darken(0.72).ContrastText(),
        Tertiary = palette.Tertiary,
        OnTertiary = palette.TertiaryContrastText,
        TertiaryContainer = palette.Tertiary.Lighten(0.78),
        OnTertiaryContainer = palette.Tertiary.Darken(0.72).ContrastText(),
        Error = palette.Error,
        OnError = palette.ErrorContrastText,
        ErrorContainer = palette.Error.Lighten(0.82),
        OnErrorContainer = palette.Error.Darken(0.75).ContrastText(),
        Background = palette.Background,
        OnBackground = palette.TextPrimary,
        Surface = palette.Surface,
        OnSurface = palette.TextPrimary,
        SurfaceDim = palette.BackgroundGray,
        SurfaceBright = palette.Surface,
        SurfaceContainerLowest = palette.Surface,
        SurfaceContainerLow = palette.BackgroundGray,
        SurfaceContainer = palette.BackgroundGray,
        SurfaceContainerHigh = palette.GrayLighter,
        SurfaceContainerHighest = palette.GrayLight,
        SurfaceVariant = palette.BackgroundGray,
        OnSurfaceVariant = palette.TextSecondary,
        Outline = palette.LinesInputs,
        OutlineVariant = palette.LinesDefault,
        Shadow = palette.Black,
        Scrim = palette.OverlayDark,
        InverseSurface = palette.Dark,
        InverseOnSurface = palette.DarkContrastText,
        InversePrimary = palette.Primary.Lighten(0.35),
    };
}
