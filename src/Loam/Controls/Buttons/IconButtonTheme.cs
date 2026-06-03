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
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(999)),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8)),
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                ButtonStyles.Dyn(TemplatedControl.ForegroundProperty, LoamTokens.TextPrimary),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

        theme.Add(SizeStyle(LoamSize.Small, new Thickness(6)));
        theme.Add(SizeStyle(LoamSize.Large, new Thickness(12)));

        ButtonStyles.AddColorMatrix(theme);
        ButtonStyles.AddDisabled(theme);
        return theme;
    }

    private static Style SizeStyle(LoamSize size, Thickness padding) =>
        new(x => x.Nesting().PropertyEquals(Button.SizeProperty, size))
        {
            Setters = { new Setter(TemplatedControl.PaddingProperty, padding) },
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

            var border = new AC.Border { Child = icon }.Named("PART_Root", scope);
            border.Bind(AC.Border.BackgroundProperty, button.GetObservable(TemplatedControl.BackgroundProperty));
            border.Bind(AC.Border.BorderBrushProperty, button.GetObservable(TemplatedControl.BorderBrushProperty));
            border.Bind(AC.Border.BorderThicknessProperty, button.GetObservable(TemplatedControl.BorderThicknessProperty));
            border.Bind(AC.Border.CornerRadiusProperty, button.GetObservable(TemplatedControl.CornerRadiusProperty));
            border.Bind(AC.Border.PaddingProperty, button.GetObservable(TemplatedControl.PaddingProperty));
            return border;
        });
}
