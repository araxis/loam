using Avalonia;

namespace Loam.Theming;

/// <summary>Shared component density metrics for hit targets, button padding, and tabular data.</summary>
public sealed record LoamDensity
{
    /// <summary>Extra-small interactive target size.</summary>
    public double InteractiveExtraSmall { get; init; } = 32;

    /// <summary>Small interactive target size.</summary>
    public double InteractiveSmall { get; init; } = 36;

    /// <summary>Default interactive target size.</summary>
    public double InteractiveMedium { get; init; } = 48;

    /// <summary>Large interactive target size.</summary>
    public double InteractiveLarge { get; init; } = 56;

    /// <summary>Extra-large interactive target size.</summary>
    public double InteractiveExtraLarge { get; init; } = 64;

    /// <summary>Extra-small button container height.</summary>
    public double ButtonContainerHeightExtraSmall { get; init; } = 32;

    /// <summary>Small button container height.</summary>
    public double ButtonContainerHeightSmall { get; init; } = 36;

    /// <summary>Medium button container height.</summary>
    public double ButtonContainerHeightMedium { get; init; } = 46;

    /// <summary>Large button container height.</summary>
    public double ButtonContainerHeightLarge { get; init; } = 54;

    /// <summary>Extra-large button container height.</summary>
    public double ButtonContainerHeightExtraLarge { get; init; } = 64;

    /// <summary>Extra-small button padding.</summary>
    public Thickness ButtonPaddingExtraSmall { get; init; } = new(12, 0);

    /// <summary>Small button padding.</summary>
    public Thickness ButtonPaddingSmall { get; init; } = new(12, 0);

    /// <summary>Default button padding.</summary>
    public Thickness ButtonPaddingMedium { get; init; } = new(16, 0);

    /// <summary>Large button padding.</summary>
    public Thickness ButtonPaddingLarge { get; init; } = new(20, 0);

    /// <summary>Extra-large button padding.</summary>
    public Thickness ButtonPaddingExtraLarge { get; init; } = new(24, 0);

    /// <summary>Extra-small icon button padding.</summary>
    public Thickness IconButtonPaddingExtraSmall { get; init; } = new(4);

    /// <summary>Small icon button padding.</summary>
    public Thickness IconButtonPaddingSmall { get; init; } = new(8);

    /// <summary>Default icon button padding.</summary>
    public Thickness IconButtonPaddingMedium { get; init; } = new(12);

    /// <summary>Large icon button padding.</summary>
    public Thickness IconButtonPaddingLarge { get; init; } = new(12);

    /// <summary>Extra-large icon button padding.</summary>
    public Thickness IconButtonPaddingExtraLarge { get; init; } = new(12);

    /// <summary>Default data header padding.</summary>
    public Thickness DataHeaderPadding { get; init; } = new(16, 12);

    /// <summary>Compact data header padding.</summary>
    public Thickness DataHeaderPaddingDense { get; init; } = new(8, 6);

    /// <summary>Default data cell padding.</summary>
    public Thickness DataCellPadding { get; init; } = new(16, 10);

    /// <summary>Compact data cell padding.</summary>
    public Thickness DataCellPaddingDense { get; init; } = new(8, 6);

    /// <summary>The Loam defaults.</summary>
    public static LoamDensity Default { get; } = new();
}
