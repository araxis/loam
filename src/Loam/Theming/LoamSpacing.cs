namespace Loam.Theming;

/// <summary>Spacing scale used by component metrics and gallery layouts.</summary>
public sealed record LoamSpacing
{
    /// <summary>No spacing.</summary>
    public double None { get; init; }
    /// <summary>2px spacing.</summary>
    public double ExtraExtraSmall { get; init; } = 2;
    /// <summary>4px spacing.</summary>
    public double ExtraSmall { get; init; } = 4;
    /// <summary>8px spacing.</summary>
    public double Small { get; init; } = 8;
    /// <summary>12px spacing.</summary>
    public double Medium { get; init; } = 12;
    /// <summary>16px spacing.</summary>
    public double Large { get; init; } = 16;
    /// <summary>24px spacing.</summary>
    public double ExtraLarge { get; init; } = 24;
    /// <summary>32px spacing.</summary>
    public double ExtraExtraLarge { get; init; } = 32;
    /// <summary>40px section spacing.</summary>
    public double Section { get; init; } = 40;

    /// <summary>The Loam defaults.</summary>
    public static LoamSpacing Default { get; } = new();
}
