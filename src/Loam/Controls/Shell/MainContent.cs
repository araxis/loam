using Avalonia.Controls;

namespace Loam.Controls;

/// <summary>
/// The scrollable main content region of a <see cref="Layout"/>, mirroring the reference API's
/// <c>MainContent</c>. Wraps its content in a padded scroll viewer.
/// </summary>
public class MainContent : ContentControl
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(MainContent);
}
