using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ProgressLinear"/> theme: a track area (<c>PART_Area</c>) with a fill (<c>PART_Fill</c>).</summary>
internal static class ProgressLinearTheme
{
    public static ControlTheme Create() =>
        new(typeof(ProgressLinear))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<ProgressLinear> BuildTemplate() =>
        new((progress, scope) =>
        {
            var track = new Border
            {
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            track.Bind(Border.BackgroundProperty, progress.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));

            var fill = new Border
            {
                Width = 0,
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Left,
            }.Named("PART_Fill", scope);

            return new Panel
            {
                Height = 4,
                Children = { track, fill },
            }.Named("PART_Area", scope);
        });
}
