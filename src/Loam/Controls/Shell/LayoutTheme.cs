using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Layout"/> theme: a dock panel with the app bar on top, the drawer on the left, and the content filling the rest.</summary>
internal static class LayoutTheme
{
    public static ControlTheme Create() =>
        new(typeof(Layout))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Layout> BuildTemplate() =>
        new((layout, scope) =>
        {
            var appBar = new ContentPresenter().Named("PART_AppBar", scope);
            DockPanel.SetDock(appBar, Dock.Top);
            appBar.Bind(ContentPresenter.ContentProperty, layout.GetObservable(Layout.AppBarProperty));

            var drawer = new ContentPresenter().Named("PART_Drawer", scope);
            DockPanel.SetDock(drawer, Dock.Left);
            drawer.Bind(ContentPresenter.ContentProperty, layout.GetObservable(Layout.DrawerProperty));

            var content = new ContentPresenter().Named("PART_ContentPresenter", scope);
            content.Bind(ContentPresenter.ContentProperty, layout.GetObservable(ContentControl.ContentProperty));
            content.Bind(ContentPresenter.ContentTemplateProperty, layout.GetObservable(ContentControl.ContentTemplateProperty));

            var dock = new DockPanel
            {
                LastChildFill = true,
                Children = { appBar, drawer, content },
            };

            var root = new Border { Child = dock }.Named("PART_Root", scope);
            root.Bind(Border.BackgroundProperty, layout.GetResourceObservable(LoamTokens.Background));
            return root;
        });
}
