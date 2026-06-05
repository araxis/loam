using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam;
using Loam.Theming;
using AC = Avalonia.Controls;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Button"/> control theme from the shared <see cref="ButtonStyles"/> engine.</summary>
internal static class ButtonTheme
{
    public static ControlTheme Create()
    {
        var theme = new ControlTheme(typeof(Button))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, ButtonStyles.IconContentTemplate()),
                ButtonStyles.Dyn(TemplatedControl.CornerRadiusProperty, LoamTokens.ShapeFull),
                ButtonStyles.Dyn(TemplatedControl.PaddingProperty, LoamTokens.DensityButtonPaddingMedium),
                ButtonStyles.Dyn(Layoutable.MinHeightProperty, LoamTokens.DensityInteractiveMedium),
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                ButtonStyles.Dyn(TemplatedControl.ForegroundProperty, LoamTokens.TextPrimary),
                ButtonStyles.Dyn(TemplatedControl.FontSizeProperty, LoamTokens.TypographyFontSize("Button")),
                ButtonStyles.Dyn(TemplatedControl.FontWeightProperty, LoamTokens.TypographyFontWeight("Button")),
                new Setter(AC.ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
                new Setter(AC.ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

        theme.Add(SizeStyle(LoamSize.Small, LoamTokens.DensityButtonPaddingSmall, LoamTokens.DensityInteractiveSmall));
        theme.Add(SizeStyle(LoamSize.Large, LoamTokens.DensityButtonPaddingLarge, LoamTokens.DensityInteractiveLarge));
        theme.Add(new Style(x => x.Nesting().PropertyEquals(Button.FullWidthProperty, true))
        {
            Setters = { new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch) },
        });

        ButtonStyles.AddColorMatrix(theme);
        ButtonStyles.AddDisabled(theme);
        return theme;
    }

    private static Style SizeStyle(LoamSize size, string paddingToken, string minHeightToken) =>
        new(x => x.Nesting().PropertyEquals(Button.SizeProperty, size))
        {
            Setters =
            {
                ButtonStyles.Dyn(TemplatedControl.PaddingProperty, paddingToken),
                ButtonStyles.Dyn(Layoutable.MinHeightProperty, minHeightToken),
            },
        };
}
