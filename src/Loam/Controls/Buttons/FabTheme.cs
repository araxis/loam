using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam;
using Loam.Theming;
using AC = Avalonia.Controls;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Fab"/> theme: pill-shaped, filled, elevated (shadow applied by <see cref="Fab"/>).</summary>
internal static class FabTheme
{
    public static ControlTheme Create()
    {
        var theme = new ControlTheme(typeof(Fab))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, ButtonStyles.IconContentTemplate()),
                ButtonStyles.Dyn(TemplatedControl.CornerRadiusProperty, ButtonSizeMetrics.FabShapeToken(LoamSize.Medium)),
                new Setter(TemplatedControl.PaddingProperty, ButtonSizeMetrics.FabPadding(LoamSize.Medium)),
                new Setter(Layoutable.MinHeightProperty, ButtonSizeMetrics.FabHeight(LoamSize.Medium)),
                ButtonStyles.Dyn(TemplatedControl.FontSizeProperty, LoamTokens.TypographyFontSize("Button")),
                ButtonStyles.Dyn(TemplatedControl.FontWeightProperty, LoamTokens.TypographyFontWeight("Button")),
                new Setter(AC.ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
                new Setter(AC.ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

        foreach (var size in ButtonSizeMetrics.All)
        {
            theme.Add(SizeStyle(size));
        }

        ButtonStyles.AddFilledColorMatrix(theme);
        ButtonStyles.AddDisabled(theme);
        return theme;
    }

    private static Style SizeStyle(LoamSize size) =>
        new(x => x.Nesting().PropertyEquals(Button.SizeProperty, size))
        {
            Setters =
            {
                new Setter(TemplatedControl.PaddingProperty, ButtonSizeMetrics.FabPadding(size)),
                new Setter(Layoutable.MinHeightProperty, ButtonSizeMetrics.FabHeight(size)),
                ButtonStyles.Dyn(TemplatedControl.CornerRadiusProperty, ButtonSizeMetrics.FabShapeToken(size)),
                ButtonStyles.Dyn(TemplatedControl.FontSizeProperty, LoamTokens.TypographyFontSize(ButtonSizeMetrics.TextStyleName(size))),
                ButtonStyles.Dyn(TemplatedControl.FontWeightProperty, LoamTokens.TypographyFontWeight(ButtonSizeMetrics.TextStyleName(size))),
            },
        };
}
