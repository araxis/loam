namespace Loam.Controls;

/// <summary>
/// Deprecated alias for <see cref="ResponsiveGrid"/>. Renamed in Loam v3 so the responsive layout no
/// longer shadows <see cref="Avalonia.Controls.Grid"/>. Switch to <see cref="ResponsiveGrid"/>; this
/// alias will be removed in a future release.
/// </summary>
[Obsolete(
    "Loam.Controls.Grid was renamed to Loam.Controls.ResponsiveGrid in v3 and will be removed in a " +
    "future release. See https://araxis.github.io/loam/migration/v2-to-v3.",
    error: false,
    DiagnosticId = "LOAM0001",
    UrlFormat = "https://araxis.github.io/loam/migration/v2-to-v3")]
public class Grid : ResponsiveGrid
{
}
