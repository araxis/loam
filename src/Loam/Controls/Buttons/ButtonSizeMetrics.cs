using Avalonia;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Internal button-family size metrics projected from component reference tokens.</summary>
internal static class ButtonSizeMetrics
{
    public static readonly LoamSize[] All =
    [
        LoamSize.ExtraSmall,
        LoamSize.Small,
        LoamSize.Medium,
        LoamSize.Large,
        LoamSize.ExtraLarge,
    ];

    public static string ContainerHeightToken(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => LoamTokens.DensityButtonContainerHeightExtraSmall,
        LoamSize.Small => LoamTokens.DensityButtonContainerHeightSmall,
        LoamSize.Large => LoamTokens.DensityButtonContainerHeightLarge,
        LoamSize.ExtraLarge => LoamTokens.DensityButtonContainerHeightExtraLarge,
        _ => LoamTokens.DensityButtonContainerHeightMedium,
    };

    public static string ButtonPaddingToken(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => LoamTokens.DensityButtonPaddingExtraSmall,
        LoamSize.Small => LoamTokens.DensityButtonPaddingSmall,
        LoamSize.Large => LoamTokens.DensityButtonPaddingLarge,
        LoamSize.ExtraLarge => LoamTokens.DensityButtonPaddingExtraLarge,
        _ => LoamTokens.DensityButtonPaddingMedium,
    };

    public static string IconButtonPaddingToken(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => LoamTokens.DensityIconButtonPaddingExtraSmall,
        LoamSize.Small => LoamTokens.DensityIconButtonPaddingSmall,
        LoamSize.Large => LoamTokens.DensityIconButtonPaddingLarge,
        LoamSize.ExtraLarge => LoamTokens.DensityIconButtonPaddingExtraLarge,
        _ => LoamTokens.DensityIconButtonPaddingMedium,
    };

    public static string IconButtonContainerToken(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => LoamTokens.DensityInteractiveExtraSmall,
        LoamSize.Small => LoamTokens.DensityInteractiveSmall,
        LoamSize.Large => LoamTokens.DensityInteractiveLarge,
        LoamSize.ExtraLarge => LoamTokens.DensityInteractiveExtraLarge,
        _ => LoamTokens.DensityInteractiveMedium,
    };

    public static double IconSpacing(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => 4,
        LoamSize.ExtraLarge => 10,
        _ => 8,
    };

    public static string TextStyleName(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => nameof(Typo.LabelSmall),
        LoamSize.Small => nameof(Typo.LabelMedium),
        LoamSize.ExtraLarge => nameof(Typo.TitleMedium),
        _ => nameof(Typo.LabelLarge),
    };

    public static Thickness FabPadding(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => new Thickness(12, 0),
        LoamSize.Small => new Thickness(16, 0),
        LoamSize.Large => new Thickness(32, 0),
        LoamSize.ExtraLarge => new Thickness(48, 0),
        _ => new Thickness(24, 0),
    };

    public static double FabHeight(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => 40,
        LoamSize.Small => 48,
        LoamSize.Large => 96,
        LoamSize.ExtraLarge => 136,
        _ => 56,
    };

    public static string FabShapeToken(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => LoamTokens.ShapeMedium,
        LoamSize.Small => LoamTokens.ShapeLarge,
        LoamSize.Large => LoamTokens.ShapeExtraLarge,
        LoamSize.ExtraLarge => LoamTokens.ShapeExtraExtraLarge,
        _ => LoamTokens.ShapeLarge,
    };
}
