using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ExpansionPanels"/> theme: an elevated <see cref="Paper"/> hosting the panel stack (<c>PART_Stack</c>).</summary>
internal static class ExpansionPanelsTheme
{
    public static ControlTheme Create() =>
        new(typeof(ExpansionPanels))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<ExpansionPanels> BuildTemplate() =>
        new((panels, scope) =>
        {
            var stack = new StackPanel().Named("PART_Stack", scope);
            return new Paper { Elevation = 1, Content = stack };
        });
}
