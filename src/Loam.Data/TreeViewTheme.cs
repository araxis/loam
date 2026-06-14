using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="TreeView"/> theme: a vertical stack (<c>PART_Root</c>) the control fills with root nodes.</summary>
internal static class TreeViewTheme
{
    public static ControlTheme Create() =>
        new(typeof(TreeView))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<TreeView> BuildTemplate() =>
        new((tree, scope) => new StackPanel().Named("PART_Root", scope));
}
