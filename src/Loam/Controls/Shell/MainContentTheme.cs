using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="MainContent"/> theme: a padded scroll viewer around the content.</summary>
internal static class MainContentTheme
{
    public static ControlTheme Create() =>
        new(typeof(MainContent))
        {
            Setters =
            {
                new Setter(TemplatedControl.PaddingProperty, new Thickness(24)),
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
            },
        };

    private static FuncControlTemplate<MainContent> BuildTemplate() =>
        new((main, scope) =>
        {
            var header = new ContentControl
            {
                IsVisible = false,
            }.Named("PART_HeaderPresenter", scope);

            var presenter = new ContentPresenter().Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, main.GetObservable(ContentControl.ContentProperty));
            presenter.Bind(ContentPresenter.ContentTemplateProperty, main.GetObservable(ContentControl.ContentTemplateProperty));

            var stack = new StackPanel
            {
                Children =
                {
                    header,
                    presenter,
                },
            };

            var inner = new Border { Child = stack };
            inner.Bind(Border.PaddingProperty, main.GetObservable(TemplatedControl.PaddingProperty));

            return new ScrollViewer { Content = inner }.Named("PART_Root", scope);
        });
}
