namespace Loam.Theming;

/// <summary>
/// Contrast level for seed-generated schemes (Material You). Higher levels push role tones toward the
/// extremes for stronger separation, improving readability for low-vision users.
/// </summary>
public enum LoamContrast
{
    /// <summary>Standard Material 3 tones (the default).</summary>
    Standard,

    /// <summary>Increased contrast.</summary>
    Medium,

    /// <summary>Maximum contrast (targets WCAG AAA on the main text pairs).</summary>
    High,
}
