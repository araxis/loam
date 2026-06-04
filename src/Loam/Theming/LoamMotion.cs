namespace Loam.Theming;

/// <summary>Shared durations and easing names for component feedback.</summary>
public sealed record LoamMotion
{
    /// <summary>Short feedback duration used for small state changes.</summary>
    public TimeSpan DurationShort { get; init; } = TimeSpan.FromMilliseconds(120);

    /// <summary>Medium feedback duration used for reveals and surface movement.</summary>
    public TimeSpan DurationMedium { get; init; } = TimeSpan.FromMilliseconds(180);

    /// <summary>Long feedback duration used for larger layout movement.</summary>
    public TimeSpan DurationLong { get; init; } = TimeSpan.FromMilliseconds(280);

    /// <summary>Standard easing string for simple transitions.</summary>
    public string EasingStandard { get; init; } = "0.2,0,0,1";

    /// <summary>Emphasized easing string for reveal-style transitions.</summary>
    public string EasingEmphasized { get; init; } = "0.2,0,0,1";

    /// <summary>The Loam defaults.</summary>
    public static LoamMotion Default { get; } = new();
}
