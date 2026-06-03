using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>
/// Builds the <see cref="Paper"/> control theme in C#. The theme supplies only the visual template
/// (a <see cref="Border"/> named <c>PART_Root</c> wrapping a <see cref="ContentPresenter"/>); the
/// token-driven styling (background, corner radius, elevation, outline) is applied reactively by
/// <see cref="Paper"/> itself so it can react to <c>Elevation</c>/<c>Square</c>/<c>Outlined</c>.
/// </summary>
internal static class PaperTheme
{
    public static ControlTheme Create() =>
        new(typeof(Paper))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Paper> BuildTemplate() =>
        new((paper, scope) =>
        {
            var presenter = new ContentPresenter()
                .Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty,
                paper.GetObservable(ContentControl.ContentProperty));
            presenter.Bind(ContentPresenter.ContentTemplateProperty,
                paper.GetObservable(ContentControl.ContentTemplateProperty));
            presenter.Bind(ContentPresenter.PaddingProperty,
                paper.GetObservable(TemplatedControl.PaddingProperty));
            presenter.Bind(ContentPresenter.HorizontalContentAlignmentProperty,
                paper.GetObservable(ContentControl.HorizontalContentAlignmentProperty));
            presenter.Bind(ContentPresenter.VerticalContentAlignmentProperty,
                paper.GetObservable(ContentControl.VerticalContentAlignmentProperty));

            return new Border { Child = presenter }.Named("PART_Root", scope);
        });
}
