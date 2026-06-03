using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Carousel"/> theme: a z-stacked slide (<c>PART_Content</c>) under overlay arrows (<c>PART_Prev</c>/<c>PART_Next</c>) and bottom bullets (<c>PART_Bullets</c>).</summary>
internal static class CarouselTheme
{
    public static ControlTheme Create() =>
        new(typeof(Carousel))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Carousel> BuildTemplate() =>
        new((carousel, scope) =>
        {
            var content = new ContentControl
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
            }.Named("PART_Content", scope);

            var prev = new IconButton
            {
                Icon = Icons.Material.Filled.ArrowBack,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0),
            }.Named("PART_Prev", scope);

            var next = new IconButton
            {
                Icon = Icons.Material.Filled.ArrowForward,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
            }.Named("PART_Next", scope);

            var bullets = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 10),
            }.Named("PART_Bullets", scope);

            return new Panel { Children = { content, prev, next, bullets } };
        });
}
