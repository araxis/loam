using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Pagination"/> theme: a horizontal strip (<c>PART_Items</c>) the control fills with arrows, page buttons, and ellipses.</summary>
internal static class PaginationTheme
{
    public static ControlTheme Create() =>
        new(typeof(Pagination))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Pagination> BuildTemplate() =>
        new((pagination, scope) =>
            new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 2,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Items", scope));
}
