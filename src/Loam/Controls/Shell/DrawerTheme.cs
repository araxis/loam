using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Drawer"/> theme: a drawer-colored panel with a right divider edge; width is managed by the control.</summary>
internal static class DrawerTheme
{
    public static ControlTheme Create() =>
        new(typeof(Drawer))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Drawer> BuildTemplate() =>
        new((drawer, scope) =>
        {
            var presenter = new ContentPresenter().Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, drawer.GetObservable(ContentControl.ContentProperty));
            presenter.Bind(ContentPresenter.ContentTemplateProperty, drawer.GetObservable(ContentControl.ContentTemplateProperty));

            var border = new Border { Child = presenter, BorderThickness = new Thickness(0, 0, 1, 0) }
                .Named("PART_Root", scope);
            border.Bind(Border.BackgroundProperty,
                drawer.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.DrawerBackground))));
            border.Bind(Border.BorderBrushProperty, drawer.GetResourceObservable(LoamTokens.LinesDefault));
            return border;
        });
}
