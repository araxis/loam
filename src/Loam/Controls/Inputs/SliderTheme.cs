using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Slider"/> theme: an interactive area (<c>PART_Area</c>) with track, fill (<c>PART_Fill</c>) and thumb (<c>PART_Thumb</c>); colors/geometry set by the control.</summary>
internal static class SliderTheme
{
    public static ControlTheme Create() =>
        new(typeof(Slider))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Slider> BuildTemplate() =>
        new((slider, scope) =>
        {
            var track = new Border
            {
                Height = 4,
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
            };
            track.Bind(Border.BackgroundProperty, slider.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));

            var fill = new Border
            {
                Height = 4,
                Width = 0,
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Fill", scope);

            var thumb = new Border
            {
                Width = 16,
                Height = 16,
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Thumb", scope);
            thumb.Bind(Border.BoxShadowProperty, slider.GetResourceObservable(LoamTokens.Elevation1));

            var area = new Panel
            {
                Children = { track, fill, thumb },
            }.Named("PART_Area", scope);
            area.Bind(Layoutable.MinHeightProperty, slider.GetResourceObservable(LoamTokens.DensityInteractiveMedium));
            return area;
        });
}
