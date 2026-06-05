using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam;
using Loam.Internal.Templating;
using Loam.Theming;
using AC = Avalonia.Controls;

namespace Loam.Controls;

/// <summary>Builds the <see cref="IconButton"/> theme: a circular, icon-only button reusing the shared button color matrix.</summary>
internal static class IconButtonTheme
{
    public static ControlTheme Create()
    {
        var theme = new ControlTheme(typeof(IconButton))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                ButtonStyles.Dyn(TemplatedControl.CornerRadiusProperty, LoamTokens.ShapeFull),
                ButtonStyles.Dyn(TemplatedControl.PaddingProperty, LoamTokens.DensityIconButtonPaddingMedium),
                ButtonStyles.Dyn(Layoutable.MinWidthProperty, LoamTokens.DensityInteractiveMedium),
                ButtonStyles.Dyn(Layoutable.MinHeightProperty, LoamTokens.DensityInteractiveMedium),
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                ButtonStyles.Dyn(TemplatedControl.ForegroundProperty, LoamTokens.TextPrimary),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

        theme.Add(SizeStyle(LoamSize.Small, LoamTokens.DensityIconButtonPaddingSmall, LoamTokens.DensityInteractiveSmall));
        theme.Add(SizeStyle(LoamSize.Large, LoamTokens.DensityIconButtonPaddingLarge, LoamTokens.DensityInteractiveLarge));

        ButtonStyles.AddColorMatrix(theme);
        ButtonStyles.AddDisabled(theme);
        return theme;
    }

    private static Style SizeStyle(LoamSize size, string paddingToken, string minSizeToken) =>
        new(x => x.Nesting().PropertyEquals(Button.SizeProperty, size))
        {
            Setters =
            {
                ButtonStyles.Dyn(TemplatedControl.PaddingProperty, paddingToken),
                ButtonStyles.Dyn(Layoutable.MinWidthProperty, minSizeToken),
                ButtonStyles.Dyn(Layoutable.MinHeightProperty, minSizeToken),
            },
        };

    private static FuncControlTemplate<IconButton> BuildTemplate() =>
        new((button, scope) =>
        {
            var icon = new Icon
            {
                Color = LoamColor.Inherit,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Icon", scope);
            icon.Bind(Icon.SizeProperty, button.GetObservable(Button.SizeProperty));

            var ripple = new Ripple { Child = icon }.Named("PART_Ripple", scope);
            ripple.Bind(Ripple.RippleOpacityProperty,
                AC.ResourceNodeExtensions.GetResourceObservable(button, LoamTokens.StatePressedOpacity));
            ripple.Bind(Ripple.DurationProperty,
                AC.ResourceNodeExtensions.GetResourceObservable(button, LoamTokens.MotionDurationShort3));
            ripple.Bind(Ripple.RippleBrushProperty,
                AC.ResourceNodeExtensions.GetResourceObservable(button, LoamTokens.ColorOnSurface));

            var border = new AC.Border { Child = ripple }.Named("PART_Root", scope);
            border.Bind(AC.Border.BackgroundProperty, button.GetObservable(TemplatedControl.BackgroundProperty));
            border.Bind(AC.Border.BorderBrushProperty, button.GetObservable(TemplatedControl.BorderBrushProperty));
            border.Bind(AC.Border.BorderThicknessProperty, button.GetObservable(TemplatedControl.BorderThicknessProperty));
            border.Bind(AC.Border.CornerRadiusProperty, button.GetObservable(TemplatedControl.CornerRadiusProperty));
            border.Bind(AC.Border.PaddingProperty, button.GetObservable(TemplatedControl.PaddingProperty));
            return border;
        });
}
