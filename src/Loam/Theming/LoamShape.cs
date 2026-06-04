using Avalonia;

namespace Loam.Theming;

/// <summary>Corner radii used by component families.</summary>
public sealed record LoamShape
{
    /// <summary>Extra-small corner radius.</summary>
    public CornerRadius ExtraSmall { get; init; } = new(4);

    /// <summary>Small corner radius.</summary>
    public CornerRadius Small { get; init; } = new(4);

    /// <summary>Medium corner radius.</summary>
    public CornerRadius Medium { get; init; } = new(8);

    /// <summary>Large corner radius.</summary>
    public CornerRadius Large { get; init; } = new(12);

    /// <summary>Full pill/circle corner radius.</summary>
    public CornerRadius Full { get; init; } = new(999);

    /// <summary>The Loam defaults.</summary>
    public static LoamShape Default { get; } = new();
}
