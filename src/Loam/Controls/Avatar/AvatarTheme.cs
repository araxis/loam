using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Avatar"/> theme: the template (a <c>PART_Root</c> border + centered content); colors are applied by the control.</summary>
internal static class AvatarTheme
{
    public static ControlTheme Create() =>
        new(typeof(Avatar))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
                new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
                ButtonStyles.Dyn(TemplatedControl.FontSizeProperty, LoamTokens.TypographyFontSize("Subtitle1")),
                ButtonStyles.Dyn(TemplatedControl.FontWeightProperty, LoamTokens.TypographyFontWeight("Subtitle1")),
            },
        };

    private static FuncControlTemplate<Avatar> BuildTemplate() =>
        new((avatar, scope) =>
        {
            var presenter = new ContentPresenter().Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, avatar.GetObservable(ContentControl.ContentProperty));
            presenter.Bind(ContentPresenter.ContentTemplateProperty, avatar.GetObservable(ContentControl.ContentTemplateProperty));
            presenter.HorizontalAlignment = HorizontalAlignment.Center;
            presenter.VerticalAlignment = VerticalAlignment.Center;

            return new Border { Child = presenter, ClipToBounds = true }.Named("PART_Root", scope);
        });
}
