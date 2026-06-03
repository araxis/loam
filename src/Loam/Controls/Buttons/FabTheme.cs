using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Styling;
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
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(999)),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(20, 12)),
                ButtonStyles.Dyn(TemplatedControl.FontSizeProperty, LoamTokens.TypographyFontSize("Button")),
                ButtonStyles.Dyn(TemplatedControl.FontWeightProperty, LoamTokens.TypographyFontWeight("Button")),
                new Setter(AC.ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
                new Setter(AC.ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

        ButtonStyles.AddFilledColorMatrix(theme);
        ButtonStyles.AddDisabled(theme);
        return theme;
    }
}
