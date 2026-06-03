using Avalonia;

namespace Loam.Theming;

/// <summary>
/// Layout metrics, mirroring MudBlazor's <c>LayoutProperties</c> (values verified against
/// MudBlazor v9.5.0). Stored in device-independent pixels.
/// </summary>
public sealed record LoamLayout
{
    /// <summary>Default control corner radius (MudBlazor: 4px).</summary>
    public CornerRadius DefaultBorderRadius { get; init; } = new(4);

    /// <summary>Expanded drawer width (MudBlazor: 240px).</summary>
    public double DrawerWidth { get; init; } = 240;

    /// <summary>Collapsed (mini) drawer width (MudBlazor: 56px).</summary>
    public double DrawerMiniWidth { get; init; } = 56;

    /// <summary>App bar height (MudBlazor: 64px).</summary>
    public double AppBarHeight { get; init; } = 64;

    /// <summary>The Loam defaults.</summary>
    public static LoamLayout Default { get; } = new();
}
