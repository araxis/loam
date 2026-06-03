using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A section header inside a <see cref="List"/>, mirroring MudBlazor's <c>MudListSubheader</c>. A muted,
/// caption-weight <see cref="Text"/> with list-aligned padding.
/// </summary>
public class ListSubheader : Text
{
    /// <summary>Creates the subheader.</summary>
    public ListSubheader()
    {
        Typo = Typo.Caption;
        FontWeight = FontWeight.SemiBold;
        Padding = new Thickness(16, 12, 16, 4);
        this.Bind(ForegroundProperty, this.GetResourceObservable(LoamTokens.TextSecondary));
    }
}
