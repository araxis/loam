using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Alert"/> theme: a rounded <c>PART_Root</c> border with an optional icon + content; colors set by the control.</summary>
internal static class AlertTheme
{
    public static ControlTheme Create() =>
        new(typeof(Alert))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(16, 10)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(4)),
            },
        };

    private static FuncControlTemplate<Alert> BuildTemplate() =>
        new((alert, scope) =>
        {
            var icon = new Icon
            {
                Color = LoamColor.Inherit,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
            }.Named("PART_Icon", scope);

            var presenter = new ContentPresenter { VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, alert.GetObservable(ContentControl.ContentProperty));

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { icon, presenter },
            };

            var border = new Border { Child = row }.Named("PART_Root", scope);
            border.Bind(Border.PaddingProperty, alert.GetObservable(TemplatedControl.PaddingProperty));
            border.Bind(Border.CornerRadiusProperty, alert.GetObservable(TemplatedControl.CornerRadiusProperty));
            return border;
        });
}
