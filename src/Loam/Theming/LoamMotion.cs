namespace Loam.Theming;

/// <summary>Shared durations and easing names for component feedback.</summary>
public sealed record LoamMotion
{
    /// <summary>Shortest duration token.</summary>
    public TimeSpan Short1 { get; init; } = TimeSpan.FromMilliseconds(50);
    /// <summary>Short duration token.</summary>
    public TimeSpan Short2 { get; init; } = TimeSpan.FromMilliseconds(100);
    /// <summary>Medium-short duration token.</summary>
    public TimeSpan Short3 { get; init; } = TimeSpan.FromMilliseconds(150);
    /// <summary>Longest short duration token.</summary>
    public TimeSpan Short4 { get; init; } = TimeSpan.FromMilliseconds(200);

    /// <summary>Shortest medium duration token.</summary>
    public TimeSpan Medium1 { get; init; } = TimeSpan.FromMilliseconds(250);
    /// <summary>Medium duration token.</summary>
    public TimeSpan Medium2 { get; init; } = TimeSpan.FromMilliseconds(300);
    /// <summary>Longer medium duration token.</summary>
    public TimeSpan Medium3 { get; init; } = TimeSpan.FromMilliseconds(350);
    /// <summary>Longest medium duration token.</summary>
    public TimeSpan Medium4 { get; init; } = TimeSpan.FromMilliseconds(400);

    /// <summary>Shortest long duration token.</summary>
    public TimeSpan Long1 { get; init; } = TimeSpan.FromMilliseconds(450);
    /// <summary>Long duration token.</summary>
    public TimeSpan Long2 { get; init; } = TimeSpan.FromMilliseconds(500);
    /// <summary>Longer duration token.</summary>
    public TimeSpan Long3 { get; init; } = TimeSpan.FromMilliseconds(550);
    /// <summary>Longest standard duration token.</summary>
    public TimeSpan Long4 { get; init; } = TimeSpan.FromMilliseconds(600);

    /// <summary>Shortest extra-long duration token.</summary>
    public TimeSpan ExtraLong1 { get; init; } = TimeSpan.FromMilliseconds(700);
    /// <summary>Extra-long duration token.</summary>
    public TimeSpan ExtraLong2 { get; init; } = TimeSpan.FromMilliseconds(800);
    /// <summary>Longer extra-long duration token.</summary>
    public TimeSpan ExtraLong3 { get; init; } = TimeSpan.FromMilliseconds(900);
    /// <summary>Longest extra-long duration token.</summary>
    public TimeSpan ExtraLong4 { get; init; } = TimeSpan.FromMilliseconds(1000);

    /// <summary>Short feedback duration used for small state changes.</summary>
    public TimeSpan DurationShort { get; init; } = TimeSpan.FromMilliseconds(150);

    /// <summary>Medium feedback duration used for reveals and surface movement.</summary>
    public TimeSpan DurationMedium { get; init; } = TimeSpan.FromMilliseconds(250);

    /// <summary>Long feedback duration used for larger layout movement.</summary>
    public TimeSpan DurationLong { get; init; } = TimeSpan.FromMilliseconds(450);

    /// <summary>Standard easing string for simple transitions.</summary>
    public string EasingStandard { get; init; } = "0.2,0,0,1";

    /// <summary>Standard decelerate easing string for entrances.</summary>
    public string EasingStandardDecelerate { get; init; } = "0,0,0,1";

    /// <summary>Standard accelerate easing string for exits.</summary>
    public string EasingStandardAccelerate { get; init; } = "0.3,0,1,1";

    /// <summary>Emphasized easing string for reveal-style transitions.</summary>
    public string EasingEmphasized { get; init; } = "path:M0,0 C0.05,0 0.133333,0.06 0.166666,0.4 C0.208333,0.82 0.25,1 1,1";

    /// <summary>Emphasized decelerate easing string for prominent entrances.</summary>
    public string EasingEmphasizedDecelerate { get; init; } = "0.05,0.7,0.1,1";

    /// <summary>Emphasized accelerate easing string for prominent exits.</summary>
    public string EasingEmphasizedAccelerate { get; init; } = "0.3,0,0.8,0.15";

    /// <summary>Linear easing string for continuous mechanical motion.</summary>
    public string EasingLinear { get; init; } = "0,0,1,1";

    /// <summary>The Loam defaults.</summary>
    public static LoamMotion Default { get; } = new();
}
