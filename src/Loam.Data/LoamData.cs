using Avalonia.Styling;
using Loam.Controls;

namespace Loam.Theming;

/// <summary>
/// Control themes for Loam's data controls (<see cref="SimpleTable"/>, <see cref="Pagination"/>,
/// <see cref="TreeView"/>/<see cref="TreeViewItem"/>). Add it to <c>Application.Styles</c> alongside
/// <see cref="LoamTheme"/> so these controls are themed:
/// <code>Styles.Add(new LoamTheme()); Styles.Add(new LoamData());</code>
/// (<c>DataGrid&lt;T&gt;</c> self-renders and needs no control theme — it resolves tokens at runtime.)
/// Tokens are resolved from the active <see cref="LoamTheme"/> via dynamic resources.
/// </summary>
public sealed class LoamData : Styles
{
    /// <summary>Registers the data control themes.</summary>
    public LoamData()
    {
        Resources[typeof(SimpleTable)] = SimpleTableTheme.Create();
        Resources[typeof(Pagination)] = PaginationTheme.Create();
        Resources[typeof(Loam.Controls.TreeView)] = TreeViewTheme.Create();
        Resources[typeof(Loam.Controls.TreeViewItem)] = TreeViewItemTheme.Create();
    }
}
