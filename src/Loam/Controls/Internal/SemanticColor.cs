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
    public static SemanticTokens Resolve(LoamColor color)
    {
        var name = color.ToPaletteName();
        return new SemanticTokens(
            Fill: name is null ? LoamTokens.Palette(nameof(LoamPalette.GrayLighter)) : LoamTokens.Palette(name),
            FillText: name is null ? LoamTokens.TextPrimary : LoamTokens.PaletteContrast(name),
            FillHover: name is null ? LoamTokens.Palette(nameof(LoamPalette.GrayLight)) : LoamTokens.PaletteDarken(name),
            Accent: name is null ? LoamTokens.TextPrimary : LoamTokens.Palette(name),
            Border: name is null ? LoamTokens.LinesDefault : LoamTokens.Palette(name),
            Overlay: name is null ? LoamTokens.LinesDefault : LoamTokens.PaletteHover(name));
    }
}
