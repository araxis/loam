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

/// <summary>Builds the <see cref="Switch"/> theme: an overlaid track (<c>PART_Track</c>) + thumb (<c>PART_Thumb</c>) plus a label; colors/position set by the control.</summary>
internal static class SwitchTheme
{
    public static ControlTheme Create() =>
        new(typeof(Switch))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

    private static FuncControlTemplate<Switch> BuildTemplate() =>
        new((toggle, scope) =>
        {
            var track = new Border
            {
                Width = 34,
                Height = 14,
                CornerRadius = new CornerRadius(7),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Track", scope);

            var stateLayer = new Border
            {
                Width = 40,
                Height = 40,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
            }.Named("PART_StateLayer", scope);
            stateLayer.Bind(Border.CornerRadiusProperty, toggle.GetResourceObservable(LoamTokens.ShapeFull));

            var thumb = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Thumb", scope);
            thumb.Bind(Border.BoxShadowProperty, toggle.GetResourceObservable(LoamTokens.Elevation1));

            var switchArea = new Panel
            {
                Width = 40,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { track, stateLayer, thumb },
            }.Named("PART_SwitchArea", scope);

            var presenter = new ContentPresenter
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            }.Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, toggle.GetObservable(ContentControl.ContentProperty));

            var root = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = new Cursor(StandardCursorType.Hand),
                Children = { switchArea, presenter },
            };
            root.Bind(Layoutable.MinHeightProperty, toggle.GetResourceObservable(LoamTokens.DensityInteractiveMedium));
            return root;
        });
}
