using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ToggleGroup"/> theme: a connected strip (<c>PART_Items</c>) filled with button segments.</summary>
internal static class ToggleGroupTheme
{
    public static ControlTheme Create() =>
        new(typeof(ToggleGroup))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
            },
        };

    private static FuncControlTemplate<ToggleGroup> BuildTemplate() =>
        new((group, scope) => new StackPanel { Orientation = Orientation.Horizontal }.Named("PART_Items", scope));
}
