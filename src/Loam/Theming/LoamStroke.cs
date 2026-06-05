namespace Loam.Theming;

/// <summary>Stroke widths used for outlines, dividers, and focus rings.</summary>
public sealed record LoamStroke
{
    /// <summary>No stroke.</summary>
    public double None { get; init; }
    /// <summary>Thin resting stroke.</summary>
    public double Thin { get; init; } = 1;
    /// <summary>Active/focus stroke.</summary>
    public double Focus { get; init; } = 2;
    /// <summary>Strong stroke for emphasized outlines.</summary>
    public double Strong { get; init; } = 3;

    /// <summary>The Loam defaults.</summary>
    public static LoamStroke Default { get; } = new();
}
