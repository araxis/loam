namespace Loam.Theming;

/// <summary>Elevation model: tonal surface levels plus matching shadow levels.</summary>
public sealed record LoamElevation
{
    /// <summary>No elevation.</summary>
    public int Level0Shadow { get; init; }
    /// <summary>Low elevation shadow level.</summary>
    public int Level1Shadow { get; init; } = 1;
    /// <summary>Raised elevation shadow level.</summary>
    public int Level2Shadow { get; init; } = 3;
    /// <summary>High elevation shadow level.</summary>
    public int Level3Shadow { get; init; } = 6;
    /// <summary>Higher elevation shadow level.</summary>
    public int Level4Shadow { get; init; } = 8;
    /// <summary>Highest common elevation shadow level.</summary>
    public int Level5Shadow { get; init; } = 12;

    /// <summary>The Loam defaults.</summary>
    public static LoamElevation Default { get; } = new();
}
