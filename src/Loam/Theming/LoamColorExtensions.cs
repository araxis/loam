using Loam;

namespace Loam.Theming;

/// <summary>
/// Maps the semantic <see cref="LoamColor"/> roles to their palette token base name. Control themes
/// build token keys from this (e.g. <c>Loam.Palette.{name}</c>, <c>{name}ContrastText</c>,
/// <c>{name}.Hover</c>). <see cref="LoamColor.Default"/>/<see cref="LoamColor.Inherit"/>/
/// <see cref="LoamColor.Transparent"/> return <c>null</c> and are handled per-control.
/// </summary>
public static class LoamColorExtensions
{
    /// <summary>The palette property name for a semantic color, or null for non-semantic roles.</summary>
    public static string? ToPaletteName(this LoamColor color) => color switch
    {
        LoamColor.Primary => nameof(LoamPalette.Primary),
        LoamColor.Secondary => nameof(LoamPalette.Secondary),
        LoamColor.Tertiary => nameof(LoamPalette.Tertiary),
        LoamColor.Info => nameof(LoamPalette.Info),
        LoamColor.Success => nameof(LoamPalette.Success),
        LoamColor.Warning => nameof(LoamPalette.Warning),
        LoamColor.Error => nameof(LoamPalette.Error),
        LoamColor.Dark => nameof(LoamPalette.Dark),
        _ => null,
    };

    /// <summary>The semantic color roles, in declaration order.</summary>
    public static IReadOnlyList<LoamColor> Semantic { get; } =
    [
        LoamColor.Primary, LoamColor.Secondary, LoamColor.Tertiary, LoamColor.Info,
        LoamColor.Success, LoamColor.Warning, LoamColor.Error, LoamColor.Dark,
    ];
}
