using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="AppBar"/> theme: a full-width <c>PART_Root</c> border + toolbar content; color/elevation set by the control.</summary>
internal static class AppBarTheme
{
    public static ControlTheme Create() =>
        new(typeof(AppBar))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 0)),
                new Setter(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Center),
                new Setter(Layoutable.HorizontalAlignmentProperty, HorizontalAlignment.Stretch),
            },
        };

    private static FuncControlTemplate<AppBar> BuildTemplate() =>
        new((appBar, scope) =>
        {
            var presenter = new ContentPresenter().Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, appBar.GetObservable(ContentControl.ContentProperty));
            presenter.Bind(ContentPresenter.ContentTemplateProperty, appBar.GetObservable(ContentControl.ContentTemplateProperty));
            presenter.Bind(ContentPresenter.PaddingProperty, appBar.GetObservable(TemplatedControl.PaddingProperty));
            presenter.Bind(ContentPresenter.VerticalContentAlignmentProperty, appBar.GetObservable(ContentControl.VerticalContentAlignmentProperty));

            return new Border { Child = presenter }.Named("PART_Root", scope);
        });
}
