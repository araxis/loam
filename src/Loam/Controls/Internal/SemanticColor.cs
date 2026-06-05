using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>The set of token keys a semantic <see cref="LoamColor"/> resolves to for the variants.</summary>
internal readonly record struct SemanticTokens(
    string Fill, string FillText, string FillHover, string Accent, string Border, string Overlay);

/// <summary>
/// Maps a <see cref="LoamColor"/> to the token keys used by filled/outlined/text appearances, shared
/// by the button family and the display primitives (Avatar, Chip). Non-semantic roles fall back to
/// neutral grays / text colors.
/// </summary>
internal static class SemanticColor
{
    public static SemanticTokens Resolve(LoamColor color) => color switch
    {
        LoamColor.Primary => Scheme(
            nameof(LoamColorScheme.Primary),
            nameof(LoamColorScheme.OnPrimary)),
        LoamColor.Secondary => Scheme(
            nameof(LoamColorScheme.Secondary),
            nameof(LoamColorScheme.OnSecondary)),
        LoamColor.Tertiary => Scheme(
            nameof(LoamColorScheme.Tertiary),
            nameof(LoamColorScheme.OnTertiary)),
        LoamColor.Error => Scheme(
            nameof(LoamColorScheme.Error),
            nameof(LoamColorScheme.OnError)),
        _ => Palette(color),
    };

    private static SemanticTokens Scheme(string role, string onRole) =>
        new(
            Fill: LoamTokens.ColorScheme(role),
            FillText: LoamTokens.ColorScheme(onRole),
            FillHover: LoamTokens.PaletteDarken(role),
            Accent: LoamTokens.ColorScheme(role),
            Border: LoamTokens.ColorOutline,
            Overlay: LoamTokens.PaletteHover(role.Replace("Container", string.Empty)));

    private static SemanticTokens Palette(LoamColor color)
    {
        var name = color.ToPaletteName();
        return new SemanticTokens(
            Fill: name is null ? LoamTokens.ColorSurfaceContainerHigh : LoamTokens.Palette(name),
            FillText: name is null ? LoamTokens.ColorOnSurface : LoamTokens.PaletteContrast(name),
            FillHover: name is null ? LoamTokens.ColorSurfaceContainerHighest : LoamTokens.PaletteDarken(name),
            Accent: name is null ? LoamTokens.ColorOnSurface : LoamTokens.Palette(name),
            Border: name is null ? LoamTokens.ColorOutlineVariant : LoamTokens.ColorOutline,
            Overlay: name is null ? LoamTokens.ColorSurfaceContainerHigh : LoamTokens.PaletteHover(name));
    }
}
