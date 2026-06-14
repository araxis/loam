using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="SimpleTable"/> theme: an elevated <see cref="Paper"/> surface (<c>PART_Paper</c>) whose content grid is populated by the control.</summary>
internal static class SimpleTableTheme
{
    public static ControlTheme Create() =>
        new(typeof(SimpleTable))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<SimpleTable> BuildTemplate() =>
        new((table, scope) => new Paper().Named("PART_Paper", scope));
}
