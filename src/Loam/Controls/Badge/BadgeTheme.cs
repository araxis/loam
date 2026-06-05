using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Badge"/> theme: the wrapped content plus an overlaid badge whose position, shape and color the control sets.</summary>
internal static class BadgeTheme
{
    public static ControlTheme Create() =>
        new(typeof(Badge))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Left),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

    private static FuncControlTemplate<Badge> BuildTemplate() =>
        new((badge, scope) =>
        {
            var presenter = new ContentPresenter { Name = "PART_ContentPresenter" };
            scope.Register(presenter.Name, presenter);
            presenter.Bind(ContentPresenter.ContentProperty, badge.GetObservable(ContentControl.ContentProperty));
            presenter.Bind(ContentPresenter.ContentTemplateProperty, badge.GetObservable(ContentControl.ContentTemplateProperty));

            var badgeText = new TextBlock
            {
                Name = "PART_BadgeText",
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            scope.Register(badgeText.Name, badgeText);

            var badgeBorder = new Border
            {
                Name = "PART_Badge",
                Child = badgeText,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                IsVisible = false,
            };
            scope.Register(badgeBorder.Name, badgeBorder);

            return new Panel { ClipToBounds = false, Children = { presenter, badgeBorder } };
        });
}
