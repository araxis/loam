using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ButtonGroup"/> theme: a strip (<c>PART_Items</c>) the control fills with connected buttons.</summary>
internal static class ButtonGroupTheme
{
    public static ControlTheme Create() =>
        new(typeof(ButtonGroup))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
            },
        };

    private static FuncControlTemplate<ButtonGroup> BuildTemplate() =>
        new((group, scope) => new StackPanel { Orientation = Orientation.Horizontal }.Named("PART_Items", scope));
}
