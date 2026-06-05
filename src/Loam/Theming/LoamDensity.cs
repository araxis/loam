using Avalonia;

namespace Loam.Theming;

/// <summary>Shared component density metrics for hit targets, button padding, and tabular data.</summary>
public sealed record LoamDensity
{
    /// <summary>Small interactive target size.</summary>
    public double InteractiveSmall { get; init; } = 32;

    /// <summary>Default interactive target size.</summary>
    public double InteractiveMedium { get; init; } = 40;

    /// <summary>Large interactive target size.</summary>
    public double InteractiveLarge { get; init; } = 48;

    /// <summary>Small button padding.</summary>
    public Thickness ButtonPaddingSmall { get; init; } = new(16, 6);

    /// <summary>Default button padding.</summary>
    public Thickness ButtonPaddingMedium { get; init; } = new(24, 10);

    /// <summary>Large button padding.</summary>
    public Thickness ButtonPaddingLarge { get; init; } = new(24, 12);

    /// <summary>Small icon button padding.</summary>
    public Thickness IconButtonPaddingSmall { get; init; } = new(6);

    /// <summary>Default icon button padding.</summary>
    public Thickness IconButtonPaddingMedium { get; init; } = new(8);

    /// <summary>Large icon button padding.</summary>
    public Thickness IconButtonPaddingLarge { get; init; } = new(12);

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
