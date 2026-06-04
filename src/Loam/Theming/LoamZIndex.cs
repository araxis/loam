namespace Loam.Theming;

/// <summary>
/// Stacking order for overlay surfaces, mirroring the reference API's <c>ZIndex</c>. Consumed when
/// Loam builds drawers, popovers, dialogs, etc.
/// </summary>
public sealed record LoamZIndex
{
    /// <summary>Drawer layer (1100).</summary>
    public int Drawer { get; init; } = 1100;

    /// <summary>Popover layer (1200).</summary>
    public int Popover { get; init; } = 1200;

    /// <summary>App bar layer (1300).</summary>
    public int AppBar { get; init; } = 1300;

    /// <summary>Dialog layer (1400).</summary>
    public int Dialog { get; init; } = 1400;

    /// <summary>Snackbar layer (1500).</summary>
    public int Snackbar { get; init; } = 1500;

    /// <summary>Tooltip layer (1600).</summary>
    public int Tooltip { get; init; } = 1600;

    /// <summary>The Loam defaults.</summary>
    public static LoamZIndex Default { get; } = new();
}
