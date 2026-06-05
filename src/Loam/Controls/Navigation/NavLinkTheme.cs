using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="NavLink"/> theme: a clickable <c>PART_Root</c> (rounded) with an optional icon + content label; active/hover tinting is applied by the control.</summary>
internal static class NavLinkTheme
{
    public static ControlTheme Create() =>
        new(typeof(NavLink))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
            },
        };

    private static FuncControlTemplate<NavLink> BuildTemplate() =>
        new((link, scope) =>
        {
            var icon = new Icon
            {
                Color = LoamColor.Default,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
            }.Named("PART_Icon", scope);

            var presenter = new ContentPresenter { VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, link.GetObservable(ContentControl.ContentProperty));

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 10),
                Children = { icon, presenter },
            };

            var root = new Border
            {
                Child = row,
                CornerRadius = new CornerRadius(4),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
            }.Named("PART_Root", scope);
            root.Bind(Layoutable.MinHeightProperty, link.GetResourceObservable(LoamTokens.DensityInteractiveMedium));
            return root;
        });
}
