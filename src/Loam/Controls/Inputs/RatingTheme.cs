using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Rating"/> theme: a horizontal star strip (<c>PART_Stars</c>) the control fills with interactive star icons.</summary>
internal static class RatingTheme
{
    public static ControlTheme Create() =>
        new(typeof(Rating))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Rating> BuildTemplate() =>
        new((rating, scope) =>
            new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2 }.Named("PART_Stars", scope));
}
