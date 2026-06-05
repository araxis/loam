using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam.Gallery;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class GalleryAcceptanceTests
{
    [Fact]
    public void Gallery_catalog_metadata_is_complete_and_unique()
    {
        var pages = ComponentsView.PageCatalog;
        pages.ShouldNotBeEmpty();

        var duplicateRoutes = pages
            .GroupBy(page => page.Route)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        duplicateRoutes.ShouldBeEmpty();

        foreach (var page in pages)
        {
            page.Group.ShouldNotBeNullOrWhiteSpace();
            page.Title.ShouldNotBeNullOrWhiteSpace();
            page.Description.ShouldNotBeNullOrWhiteSpace();
            page.BuilderMethod.ShouldStartWith("Build");
            page.ExpectedComponentNames.ShouldNotBeEmpty();
            page.Code.Trim().ShouldNotBe($"{page.BuilderMethod}();", page.Route);
            page.Code.TrimStart().StartsWith("private static", StringComparison.Ordinal).ShouldBeTrue(page.Route);
            page.Code.Contains($"{page.BuilderMethod}(", StringComparison.Ordinal).ShouldBeTrue(page.Route);

            if (page.SampleKind == ComponentsView.GallerySampleKind.SingleComponent)
            {
                page.ExpectedComponentNames.Count.ShouldBe(1, page.Route);
            }
            else
            {
                page.ExpectedComponentNames.Count.ShouldBeGreaterThan(1, page.Route);
            }
        }
    }

    [Fact]
    public void Shared_builder_pages_are_marked_as_family_samples()
    {
        var sharedPages = ComponentsView.PageCatalog
            .GroupBy(page => page.BuilderMethod)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group)
            .ToArray();

        sharedPages.ShouldNotBeEmpty();
        foreach (var page in sharedPages)
        {
            page.SampleKind.ShouldBe(ComponentsView.GallerySampleKind.Family, page.Route);
        }
    }

    [AvaloniaFact]
    public void Every_gallery_page_article_renders_in_light_and_dark()
    {
        foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            foreach (var page in ComponentsView.PageCatalog)
            {
                var article = ComponentsView.BuildArticle(page);
                var window = Show(article, theme);
                try
                {
                    article.GetVisualDescendants().OfType<CodeSampleView>().ShouldHaveSingleItem(page.Route);
                    article.GetVisualDescendants().OfType<ContentControl>().Any().ShouldBeTrue(page.Route);
                }
                finally
                {
                    window.Close();
                }
            }
        }
    }

    [AvaloniaFact]
    public void Every_gallery_page_covers_expected_components()
    {
        foreach (var page in ComponentsView.PageCatalog)
        {
            var preview = page.Build();
            var window = Show(preview, ThemeVariant.Light);
            try
            {
                var renderedTypeNames = ControlTypeNames(preview);

                foreach (var componentName in page.ExpectedComponentNames)
                {
                    var appearsInPreview = renderedTypeNames.Contains(componentName);
                    var appearsInSource = page.Code.Contains(componentName, StringComparison.Ordinal);
                    (appearsInPreview || appearsInSource).ShouldBeTrue($"{page.Route} is missing {componentName}");
                }
            }
            finally
            {
                window.Close();
            }
        }
    }

    [AvaloniaFact]
    public void Components_view_shell_renders_catalog_page()
    {
        var view = new ComponentsView();
        var window = Show(view, ThemeVariant.Light);
        try
        {
            view.GetVisualDescendants().OfType<CodeSampleView>().ShouldHaveSingleItem();
            view.GetVisualDescendants().OfType<TextBlock>()
                .Any(text => string.Equals(text.Text, "Overview", StringComparison.Ordinal))
                .ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    private static Window Show(Control content, ThemeVariant theme)
    {
        Application.Current!.RequestedThemeVariant = theme;

        var window = new Window
        {
            Width = 1280,
            Height = 900,
            Content = content,
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        content.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static HashSet<string> ControlTypeNames(Control root)
    {
        root.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var controls = root.GetVisualDescendants().OfType<Control>().Prepend(root);
        return controls
            .Select(control => FriendlyTypeName(control.GetType()))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string FriendlyTypeName(Type type)
    {
        var name = type.Name;
        var tick = name.IndexOf('`', StringComparison.Ordinal);
        return tick >= 0 ? name[..tick] : name;
    }
}
