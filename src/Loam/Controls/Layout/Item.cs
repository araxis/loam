namespace Loam.Controls;

/// <summary>
/// Deprecated alias for <see cref="Col"/>. Renamed in Loam v3 alongside
/// <see cref="ResponsiveGrid"/>. Switch to <see cref="Col"/>; this alias will be removed in a future
/// release.
/// </summary>
[Obsolete(
    "Loam.Controls.Item was renamed to Loam.Controls.Col in v3 and will be removed in a future " +
    "release. See https://araxis.github.io/loam/migration/v2-to-v3.",
    error: false,
    DiagnosticId = "LOAM0002",
    UrlFormat = "https://araxis.github.io/loam/migration/v2-to-v3")]
public class Item : Col
{
}
