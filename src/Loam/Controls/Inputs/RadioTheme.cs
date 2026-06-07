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

/// <summary>Builds the <see cref="Radio"/> theme: a ring (<c>PART_Ring</c>) with a centered dot (<c>PART_Dot</c>) plus a label.</summary>
internal static class RadioTheme
{
    public static ControlTheme Create() =>
        new(typeof(Radio))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

    private static FuncControlTemplate<Radio> BuildTemplate() =>
        new((radio, scope) =>
        {
            var dot = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsVisible = false,
            }.Named("PART_Dot", scope);

            var ring = new Border
            {
                Width = 20,
                Height = 20,
                Child = dot,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Ring", scope);

            var stateLayer = new Border
            {
                Width = 40,
                Height = 40,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
            }.Named("PART_StateLayer", scope);
            stateLayer.Bind(Border.CornerRadiusProperty, radio.GetResourceObservable(LoamTokens.ShapeFull));

            var visualHost = new Panel
            {
                Width = 20,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { stateLayer, ring },
            }.Named("PART_VisualHost", scope);

            var presenter = new ContentPresenter
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            }.Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, radio.GetObservable(ContentControl.ContentProperty));

            var root = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = new Cursor(StandardCursorType.Hand),
                Children = { visualHost, presenter },
            };
            root.Bind(Layoutable.MinHeightProperty, radio.GetResourceObservable(LoamTokens.DensityInteractiveMedium));
            return root;
        });
}
