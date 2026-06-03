using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Tabs"/> theme: a header strip (<c>PART_Headers</c>) over a divider, above the content (<c>PART_Content</c>).</summary>
internal static class TabsTheme
{
    public static ControlTheme Create() =>
        new(typeof(Tabs))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Tabs> BuildTemplate() =>
        new((tabs, scope) =>
        {
            var headers = new StackPanel { Orientation = Orientation.Horizontal }.Named("PART_Headers", scope);

            var headerBar = new Border
            {
                Child = headers,
                BorderThickness = new Thickness(0, 0, 0, 1),
            };
            headerBar.Bind(Border.BorderBrushProperty, tabs.GetResourceObservable(LoamTokens.Divider));
            DockPanel.SetDock(headerBar, Dock.Top);

            var content = new ContentControl { Margin = new Thickness(0, 16, 0, 0) }.Named("PART_Content", scope);

            return new DockPanel { LastChildFill = true, Children = { headerBar, content } };
        });
}
