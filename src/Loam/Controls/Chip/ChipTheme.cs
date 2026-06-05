using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Chip"/> theme: the icon / label / close template; colors and shape are applied by the control.</summary>
internal static class ChipTheme
{
    public static ControlTheme Create() =>
        new(typeof(Chip))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                ButtonStyles.Dyn(TemplatedControl.FontSizeProperty, LoamTokens.TypographyFontSize("Body2")),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

    private static FuncControlTemplate<Chip> BuildTemplate() =>
        new((chip, scope) =>
        {
            var icon = new Icon { Color = LoamColor.Inherit, Size = LoamSize.Small, IsVisible = false, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Icon", scope);

            var text = new Text { Color = LoamColor.Inherit, Typo = Typo.Body2, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Text", scope);

            var close = new Icon
            {
                Color = LoamColor.Inherit,
                Size = LoamSize.Small,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = new Cursor(StandardCursorType.Hand),
            }.Named("PART_Close", scope);

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { icon, text, close },
            };

            var root = new Border { Child = panel }.Named("PART_Root", scope);
            root.Bind(Layoutable.MinHeightProperty, chip.GetResourceObservable(LoamTokens.DensityInteractiveSmall));
            return root;
        });
}
