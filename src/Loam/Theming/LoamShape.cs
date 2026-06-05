using Avalonia;

namespace Loam.Theming;

/// <summary>Corner radii used by component families.</summary>
public sealed record LoamShape
{
    /// <summary>No corner radius.</summary>
    public CornerRadius None { get; init; }

    /// <summary>Extra-small corner radius.</summary>
    public CornerRadius ExtraSmall { get; init; } = new(4);

    /// <summary>Small corner radius.</summary>
    public CornerRadius Small { get; init; } = new(8);

    /// <summary>Medium corner radius.</summary>
    public CornerRadius Medium { get; init; } = new(12);

    /// <summary>Large corner radius.</summary>
    public CornerRadius Large { get; init; } = new(16);

    /// <summary>Large-increased corner radius.</summary>
    public CornerRadius LargeIncreased { get; init; } = new(20);

    /// <summary>Extra-large corner radius.</summary>
    public CornerRadius ExtraLarge { get; init; } = new(28);

    /// <summary>Extra-large-increased corner radius.</summary>
    public CornerRadius ExtraLargeIncreased { get; init; } = new(32);

    /// <summary>Extra-extra-large corner radius.</summary>
    public CornerRadius ExtraExtraLarge { get; init; } = new(48);

    /// <summary>Full pill/circle corner radius.</summary>
    public CornerRadius Full { get; init; } = new(999);

    /// <summary>The Loam defaults.</summary>
    public static LoamShape Default { get; } = new();
}
