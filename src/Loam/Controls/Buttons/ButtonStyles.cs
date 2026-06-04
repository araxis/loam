using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Loam;
using Loam.Internal.Templating;
using Loam.Theming;
using AC = Avalonia.Controls;

namespace Loam.Controls;

/// <summary>
/// Shared theme building blocks for the button family (<see cref="Button"/>, <see cref="IconButton"/>,
/// <see cref="Fab"/>): the variant × color × state matrix (generated in loops over
/// <see cref="DynamicResourceExtension"/> setters and <c>PropertyEquals</c>/pseudo-class selectors)
/// and the start-icon/content/end-icon template. Selectors key off <see cref="Button"/>'s styled
/// properties, which the subclasses inherit, so one matrix themes them all.
/// </summary>
internal static class ButtonStyles
{
    internal static readonly LoamColor[] Colors =
    [
        LoamColor.Default, LoamColor.Primary, LoamColor.Secondary, LoamColor.Tertiary,
        LoamColor.Info, LoamColor.Success, LoamColor.Warning, LoamColor.Error, LoamColor.Dark,
    ];

    /// <summary>Adds the full Variant × Color base + hover styles.</summary>
    public static void AddColorMatrix(ControlTheme theme)
    {
        foreach (var color in Colors)
        {
            var c = SemanticColor.Resolve(color);

            theme.Add(VariantStyle(Variant.Filled, color,
                Dyn(TemplatedControl.BackgroundProperty, c.Fill),
                Dyn(TemplatedControl.ForegroundProperty, c.FillText)));
            theme.Add(VariantStateStyle(Variant.Filled, color, ":pointerover",
                Dyn(TemplatedControl.BackgroundProperty, c.FillHover)));

            theme.Add(VariantStyle(Variant.Outlined, color,
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                Dyn(TemplatedControl.ForegroundProperty, c.Accent),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                Dyn(TemplatedControl.BorderBrushProperty, c.Border)));
            theme.Add(VariantStateStyle(Variant.Outlined, color, ":pointerover",
                Dyn(TemplatedControl.BackgroundProperty, c.Overlay)));

            theme.Add(VariantStyle(Variant.Text, color,
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                Dyn(TemplatedControl.ForegroundProperty, c.Accent)));
            theme.Add(VariantStateStyle(Variant.Text, color, ":pointerover",
                Dyn(TemplatedControl.BackgroundProperty, c.Overlay)));
        }
    }

    /// <summary>Adds a filled-only Color matrix (for <see cref="Fab"/>, which has no Variant).</summary>
    public static void AddFilledColorMatrix(ControlTheme theme)
    {
        foreach (var color in Colors)
        {
            var c = SemanticColor.Resolve(color);
            theme.Add(new Style(x => x.Nesting().PropertyEquals(Button.ColorProperty, color))
            {
                Setters = { Dyn(TemplatedControl.BackgroundProperty, c.Fill), Dyn(TemplatedControl.ForegroundProperty, c.FillText) },
            });
            theme.Add(new Style(x => x.Nesting().PropertyEquals(Button.ColorProperty, color).Class(":pointerover"))
            {
                Setters = { Dyn(TemplatedControl.BackgroundProperty, c.FillHover) },
            });
        }
    }

    /// <summary>Adds disabled-state overrides (added last so they win).</summary>
    public static void AddDisabled(ControlTheme theme)
    {
        var disabledFg = LoamTokens.Palette(nameof(LoamPalette.ActionDisabled));
        var disabledBg = LoamTokens.Palette(nameof(LoamPalette.ActionDisabledBackground));

        theme.Add(new Style(x => x.Nesting().Class(":disabled"))
        {
            Setters =
            {
                Dyn(TemplatedControl.ForegroundProperty, disabledFg),
                Dyn(Visual.OpacityProperty, LoamTokens.StateDisabledOpacity),
            },
        });
        theme.Add(new Style(x => x.Nesting().PropertyEquals(Button.VariantProperty, Variant.Filled).Class(":disabled"))
        {
            Setters = { Dyn(TemplatedControl.BackgroundProperty, disabledBg) },
        });
        theme.Add(new Style(x => x.Nesting().PropertyEquals(Button.VariantProperty, Variant.Outlined).Class(":disabled"))
        {
            Setters = { Dyn(TemplatedControl.BorderBrushProperty, disabledBg) },
        });
    }

    /// <summary>The start-icon / content / end-icon template shared by Button and Fab.</summary>
    public static FuncControlTemplate<Button> IconContentTemplate() =>
        new((button, scope) =>
        {
            var start = new Icon { Color = LoamColor.Inherit, IsVisible = false, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_StartIcon", scope);
            start.Bind(Icon.SizeProperty, button.GetObservable(Button.SizeProperty));

            var presenter = new ContentPresenter().Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, button.GetObservable(AC.ContentControl.ContentProperty));
            presenter.Bind(ContentPresenter.ContentTemplateProperty, button.GetObservable(AC.ContentControl.ContentTemplateProperty));
            presenter.Bind(ContentPresenter.HorizontalContentAlignmentProperty, button.GetObservable(AC.ContentControl.HorizontalContentAlignmentProperty));
            presenter.Bind(ContentPresenter.VerticalContentAlignmentProperty, button.GetObservable(AC.ContentControl.VerticalContentAlignmentProperty));

            var end = new Icon { Color = LoamColor.Inherit, IsVisible = false, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_EndIcon", scope);
            end.Bind(Icon.SizeProperty, button.GetObservable(Button.SizeProperty));

            var panel = new AC.StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children = { start, presenter, end },
            };

            var ripple = new Ripple { Child = panel }.Named("PART_Ripple", scope);

            var border = new AC.Border { Child = ripple }.Named("PART_Root", scope);
            border.Bind(AC.Border.BackgroundProperty, button.GetObservable(TemplatedControl.BackgroundProperty));
            border.Bind(AC.Border.BorderBrushProperty, button.GetObservable(TemplatedControl.BorderBrushProperty));
            border.Bind(AC.Border.BorderThicknessProperty, button.GetObservable(TemplatedControl.BorderThicknessProperty));
            border.Bind(AC.Border.CornerRadiusProperty, button.GetObservable(TemplatedControl.CornerRadiusProperty));
            border.Bind(AC.Border.PaddingProperty, button.GetObservable(TemplatedControl.PaddingProperty));
            return border;
        });

    /// <summary>A setter whose value is a dynamic resource (token), resolved live.</summary>
    public static Setter Dyn(AvaloniaProperty property, object resourceKey) =>
        new(property, new DynamicResourceExtension { ResourceKey = resourceKey });

    private static Style VariantStyle(Variant variant, LoamColor color, params SetterBase[] setters)
    {
        var style = new Style(x => x.Nesting()
            .PropertyEquals(Button.VariantProperty, variant)
            .PropertyEquals(Button.ColorProperty, color));
        foreach (var setter in setters)
        {
            style.Setters.Add(setter);
        }

        return style;
    }

    private static Style VariantStateStyle(Variant variant, LoamColor color, string pseudo, params SetterBase[] setters)
    {
        var style = new Style(x => x.Nesting()
            .PropertyEquals(Button.VariantProperty, variant)
            .PropertyEquals(Button.ColorProperty, color)
            .Class(pseudo));
        foreach (var setter in setters)
        {
            style.Setters.Add(setter);
        }

        return style;
    }

}
