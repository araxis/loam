using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ToggleGroup"/> theme: a rounded, clipped outline (<c>PART_Root</c>) wrapping the connected segment strip (<c>PART_Items</c>) the control fills.</summary>
internal static class ToggleGroupTheme
{
    public static ControlTheme Create() =>
        new(typeof(ToggleGroup))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<ToggleGroup> BuildTemplate() =>
        new((group, scope) =>
        {
            var items = new StackPanel { Orientation = Orientation.Horizontal }.Named("PART_Items", scope);

            var root = new Border
            {
                Child = items,
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                ClipToBounds = true,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            root.Bind(Border.BorderBrushProperty,
                group.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));
            return root;
        });
}
