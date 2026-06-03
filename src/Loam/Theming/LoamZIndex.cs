namespace Loam.Theming;

/// <summary>
/// Stacking order for overlay surfaces, mirroring MudBlazor's <c>ZIndex</c> (values verified
/// against MudBlazor v9.5.0). Consumed when Loam builds drawers, popovers, dialogs, etc.
/// </summary>
public sealed record LoamZIndex
{
    /// <summary>Drawer layer (MudBlazor: 1100).</summary>
    public int Drawer { get; init; } = 1100;

    /// <summary>Popover layer (MudBlazor: 1200).</summary>
    public int Popover { get; init; } = 1200;

    /// <summary>App bar layer (MudBlazor: 1300).</summary>
    public int AppBar { get; init; } = 1300;

    /// <summary>Dialog layer (MudBlazor: 1400).</summary>
    public int Dialog { get; init; } = 1400;

    /// <summary>Snackbar layer (MudBlazor: 1500).</summary>
    public int Snackbar { get; init; } = 1500;

    /// <summary>Tooltip layer (MudBlazor: 1600).</summary>
    public int Tooltip { get; init; } = 1600;

    /// <summary>The Loam defaults.</summary>
    public static LoamZIndex Default { get; } = new();
}
