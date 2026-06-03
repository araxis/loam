using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Breadcrumbs"/> theme: a horizontal strip (<c>PART_Items</c>) the control fills with links and separators.</summary>
internal static class BreadcrumbsTheme
{
    public static ControlTheme Create() =>
        new(typeof(Breadcrumbs))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Breadcrumbs> BuildTemplate() =>
        new((breadcrumbs, scope) =>
            new StackPanel { Orientation = Orientation.Horizontal }.Named("PART_Items", scope));
}
