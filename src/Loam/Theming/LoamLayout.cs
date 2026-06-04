using Avalonia;

namespace Loam.Theming;

/// <summary>
/// Layout metrics, mirroring the reference API's <c>LayoutProperties</c>. Stored in device-independent pixels.
/// </summary>
public sealed record LoamLayout
{
    /// <summary>Default control corner radius (4px).</summary>
    public CornerRadius DefaultBorderRadius { get; init; } = new(4);

    /// <summary>Expanded drawer width (240px).</summary>
    public double DrawerWidth { get; init; } = 240;

    /// <summary>Collapsed (mini) drawer width (56px).</summary>
    public double DrawerMiniWidth { get; init; } = 56;

    /// <summary>App bar height (64px).</summary>
    public double AppBarHeight { get; init; } = 64;

    /// <summary>The Loam defaults.</summary>
    public static LoamLayout Default { get; } = new();
}
