using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ChipSet"/> theme: a wrap strip (<c>PART_Items</c>) the control fills with chips.</summary>
internal static class ChipSetTheme
{
    public static ControlTheme Create() =>
        new(typeof(ChipSet))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<ChipSet> BuildTemplate() =>
        new((set, scope) => new WrapPanel().Named("PART_Items", scope));
}
