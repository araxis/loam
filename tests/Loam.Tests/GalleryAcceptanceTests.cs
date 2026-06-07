using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Loam.Gallery;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class GalleryAcceptanceTests
{
    private static readonly LoamSize[] ExpectedSizes =
    [
        LoamSize.ExtraSmall,
        LoamSize.Small,
        LoamSize.Medium,
        LoamSize.Large,
        LoamSize.ExtraLarge,
    ];

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
            page.AcceptanceCriteria.ShouldNotBeEmpty();
            page.AcceptanceCriteria.Distinct().Count().ShouldBe(page.AcceptanceCriteria.Count, page.Route);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.Anatomy);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.ColorRoles);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.Shape);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.LightDark);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.SourceReference);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.SourceLinkedCode);
            page.SourceReference.ShouldNotBeNullOrWhiteSpace(page.Route);
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
    public void Gallery_design_acceptance_matrix_covers_component_state_families()
    {
        var pages = ComponentsView.PageCatalog;

        foreach (var page in pages.Where(page => page.Group is "Buttons" or "Inputs" or "Pickers" or "Data" or "Navigation" or "Shell"))
        {
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.FocusVisible, page.Route);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.Keyboard, page.Route);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.Automation, page.Route);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.StateLayer, page.Route);
        }

        foreach (var page in pages.Where(page => page.Group is "Buttons" or "Inputs" or "Pickers"))
        {
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.Disabled, page.Route);
            page.AcceptanceCriteria.ShouldContain(ComponentsView.GalleryAcceptanceCriterion.Density, page.Route);
        }

        foreach (var route in new[] { "Inputs/TextField", "Inputs/NumericField", "Inputs/Select", "Inputs/Form", "Pickers/DatePicker", "Pickers/TimePicker", "Pickers/DateRangePicker" })
        {
            pages.Single(page => page.Route == route).AcceptanceCriteria
                .ShouldContain(ComponentsView.GalleryAcceptanceCriterion.Error, route);
        }

        foreach (var route in new[] { "Inputs/Autocomplete", "Inputs/Select", "Pickers/DatePicker", "Pickers/TimePicker", "Pickers/DateRangePicker", "Feedback/DialogService", "Feedback/SnackbarService", "Shell/Drawer" })
        {
            pages.Single(page => page.Route == route).AcceptanceCriteria
                .ShouldContain(ComponentsView.GalleryAcceptanceCriterion.OpenOrDismiss, route);
        }

        foreach (var route in new[] { "Feedback/ProgressCircular", "Feedback/ProgressLinear", "Feedback/Skeleton" })
        {
            pages.Single(page => page.Route == route).AcceptanceCriteria
                .ShouldContain(ComponentsView.GalleryAcceptanceCriterion.Loading, route);
        }

        foreach (var route in new[] { "Data/SimpleTable", "Data/DataGrid", "Data/TreeView", "Charts/PieChart", "Charts/BarChart", "Charts/LineChart" })
        {
            pages.Single(page => page.Route == route).AcceptanceCriteria
                .ShouldContain(ComponentsView.GalleryAcceptanceCriterion.Empty, route);
        }
    }

    [Fact]
    public void Gallery_catalog_pages_use_distinct_builder_methods()
    {
        var duplicateBuilders = ComponentsView.PageCatalog
            .GroupBy(page => page.BuilderMethod)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key}: {string.Join(", ", group.Select(page => page.Route))}")
            .ToArray();

        duplicateBuilders.ShouldBeEmpty();
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

    [AvaloniaFact]
    public void Components_view_header_uses_right_aligned_icon_theme_toggle()
    {
        var view = new ComponentsView();
        var window = Show(view, ThemeVariant.Light);
        try
        {
            var statusPills = view.GetVisualDescendants().OfType<Border>()
                .Where(pill => pill.Name is "PART_HeaderStatusComponentLab" or "PART_HeaderStatusLiveControls")
                .ToArray();
            statusPills.Length.ShouldBe(2);
            statusPills.Select(pill => ((Text)pill.Child!).Text).ShouldBe(["component lab", "live controls"]);
            foreach (var pill in statusPills)
            {
                pill.BorderThickness.ShouldBe(new Thickness(1));
                pill.MinHeight.ShouldBe(32);
                pill.ClipToBounds.ShouldBeFalse();
            }

            var actions = view.GetVisualDescendants().OfType<StackPanel>()
                .Single(panel => panel.Name == "PART_HeaderActions");
            actions.HorizontalAlignment.ShouldBe(HorizontalAlignment.Right);
            Avalonia.Controls.Grid.GetColumn(actions).ShouldBe(2);

            var theme = view.GetVisualDescendants().OfType<ToggleIconButton>()
                .Single(button => AutomationProperties.GetName(button) == "Toggle theme");
            theme.Icon.ShouldBe(Icons.Material.Filled.DarkMode);
            theme.ToggledIcon.ShouldBe(Icons.Material.Filled.LightMode);
            theme.StartIcon.ShouldBeNull();
            theme.Content.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Code_sample_view_exposes_copy_button()
    {
        var view = new CodeSampleView("Copy", "new LoamButton { Content = \"Save\" };");
        var window = Show(view, ThemeVariant.Light);
        try
        {
            var copy = view.GetVisualDescendants().OfType<IconButton>()
                .Single(button => AutomationProperties.GetName(button) == "Copy code");
            copy.Icon.ShouldBe(Icons.Material.Filled.ContentCopy);
            copy.Size.ShouldBe(LoamSize.Small);

            view.GetVisualDescendants().OfType<TextBlock>()
                .Any(text => text.Text == "C#")
                .ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Alert_gallery_page_uses_generated_component_api()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Feedback/Alert");
        foreach (var expected in new[] { "Title =", "Message =", "Action =", "Closeable = true", "IsEnabled = false", "Compatibility path" })
        {
            page.Code.ShouldContain(expected);
        }

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var alerts = preview.GetVisualDescendants().OfType<Alert>().ToArray();
            alerts.Length.ShouldBeGreaterThanOrEqualTo(6);
            alerts.Any(alert => !string.IsNullOrWhiteSpace(alert.Title) && !string.IsNullOrWhiteSpace(alert.Message)).ShouldBeTrue();
            alerts.Any(alert => alert.Action is Loam.Controls.Button).ShouldBeTrue();
            alerts.Any(alert => alert.Closeable).ShouldBeTrue();
            alerts.Any(alert => !alert.IsEnabled).ShouldBeTrue();
            alerts.Any(alert => string.Equals(alert.Content as string, "Compatibility path: raw Content still renders.", StringComparison.Ordinal)).ShouldBeTrue();

            var closeable = alerts.First(alert => alert.Closeable);
            closeable.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            var close = closeable.GetVisualDescendants().OfType<IconButton>().First(button => button.Name == "PART_Close");
            close.IsVisible.ShouldBeTrue();
            close.Focusable.ShouldBeTrue();
            AutomationProperties.GetName(close).ShouldBe("Close alert");
            AutomationProperties.GetHelpText(close).ShouldBe("Dismiss alert");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Loading_gallery_pages_use_generated_component_apis()
    {
        var circularPage = ComponentsView.PageCatalog.Single(page => page.Route == "Feedback/ProgressCircular");
        foreach (var expected in new[]
                 {
                     "Label =",
                     "ShowValue = true",
                     "Indeterminate = false",
                     "LoamSize.ExtraLarge",
                     "ValueText =",
                     "IsEnabled = false",
                 })
        {
            circularPage.Code.ShouldContain(expected);
        }
        circularPage.Code.ShouldNotContain("CircularCase");

        var circularPreview = circularPage.Build();
        var circularWindow = Show(circularPreview, ThemeVariant.Light);
        try
        {
            var circular = circularPreview.GetVisualDescendants().OfType<ProgressCircular>().ToArray();
            circular.Length.ShouldBeGreaterThanOrEqualTo(10);
            circular.Any(progress => progress.ShowValue && !progress.Indeterminate).ShouldBeTrue();
            circular.Any(progress => progress.ValueText == "Step 2").ShouldBeTrue();
            circular.Any(progress => !progress.IsEnabled).ShouldBeTrue();
            foreach (var size in new[] { LoamSize.ExtraSmall, LoamSize.Small, LoamSize.Medium, LoamSize.Large, LoamSize.ExtraLarge })
            {
                circular.Any(progress => progress.Size == size).ShouldBeTrue(size.ToString());
            }

            circular.Where(progress => !string.IsNullOrWhiteSpace(progress.Label))
                .All(progress => AutomationProperties.GetName(progress) == progress.Label)
                .ShouldBeTrue();
        }
        finally
        {
            circularWindow.Close();
        }

        var linearPage = ComponentsView.PageCatalog.Single(page => page.Route == "Feedback/ProgressLinear");
        foreach (var expected in new[]
                 {
                     "Label =",
                     "ShowValue = true",
                     "Indeterminate = true",
                     "LoamSize.ExtraLarge",
                     "ValueText =",
                     "IsEnabled = false",
                 })
        {
            linearPage.Code.ShouldContain(expected);
        }
        linearPage.Code.ShouldNotContain("LinearCase");

        var linearPreview = linearPage.Build();
        var linearWindow = Show(linearPreview, ThemeVariant.Light);
        try
        {
            var linear = linearPreview.GetVisualDescendants().OfType<ProgressLinear>().ToArray();
            linear.Length.ShouldBeGreaterThanOrEqualTo(10);
            linear.All(progress => progress.ShowValue && !string.IsNullOrWhiteSpace(progress.Label)).ShouldBeTrue();
            linear.Any(progress => progress.ValueText == "Step 2 of 4").ShouldBeTrue();
            linear.Any(progress => !progress.IsEnabled).ShouldBeTrue();
            foreach (var size in ExpectedSizes)
            {
                linear.Any(progress => progress.Size == size).ShouldBeTrue(size.ToString());
            }

            linearPreview.GetVisualDescendants().OfType<Text>()
                .Any(text => text.Name == "PART_ValueText" && text.IsVisible)
                .ShouldBeTrue();
        }
        finally
        {
            linearWindow.Close();
        }

        var skeletonPage = ComponentsView.PageCatalog.Single(page => page.Route == "Feedback/Skeleton");
        foreach (var expected in new[]
                 {
                     "Skeleton.TextLine",
                     "Skeleton.Avatar",
                     "Skeleton.Button",
                     "Skeleton.Thumbnail",
                     "Skeleton.Card",
                     "LoamSize.ExtraLarge",
                     "IsEnabled = false",
                 })
        {
            skeletonPage.Code.ShouldContain(expected);
        }

        var skeletonPreview = skeletonPage.Build();
        var skeletonWindow = Show(skeletonPreview, ThemeVariant.Light);
        try
        {
            var skeletons = skeletonPreview.GetVisualDescendants().OfType<Skeleton>().ToArray();
            skeletons.Length.ShouldBeGreaterThanOrEqualTo(20);
            skeletons.Any(skeleton => skeleton.Preset == SkeletonPreset.Text).ShouldBeTrue();
            skeletons.Any(skeleton => skeleton.Preset == SkeletonPreset.Avatar && skeleton.Circle).ShouldBeTrue();
            skeletons.Any(skeleton => skeleton.Preset == SkeletonPreset.Button).ShouldBeTrue();
            skeletons.Any(skeleton => skeleton.Preset == SkeletonPreset.Thumbnail).ShouldBeTrue();
            skeletons.Any(skeleton => skeleton.Preset == SkeletonPreset.Card && !skeleton.Animate).ShouldBeTrue();
            skeletons.Any(skeleton => !skeleton.IsEnabled).ShouldBeTrue();
            skeletons.Any(skeleton => skeleton.Preset == SkeletonPreset.Custom && skeleton.Circle && !skeleton.Animate).ShouldBeTrue();
            foreach (var size in ExpectedSizes)
            {
                skeletons.Any(skeleton => skeleton.Size == size).ShouldBeTrue(size.ToString());
            }
        }
        finally
        {
            skeletonWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Sizes_gallery_page_covers_every_size_aware_surface()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Start/Sizes");
        page.Code.ShouldContain("LoamSize.ExtraSmall");
        page.Code.ShouldContain("LoamSize.Small");
        page.Code.ShouldContain("LoamSize.Medium");
        page.Code.ShouldContain("LoamSize.Large");
        page.Code.ShouldContain("LoamSize.ExtraLarge");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var renderedTypeNames = ControlTypeNames(preview);
            foreach (var componentName in page.ExpectedComponentNames)
            {
                renderedTypeNames.Contains(componentName).ShouldBeTrue($"Sizes page is missing {componentName}");
            }

            var sizeLabels = preview.GetVisualDescendants().OfType<Text>()
                .Select(text => text.Text)
                .ToArray();

            var matrix = preview.GetVisualDescendants().OfType<StackPanel>()
                .Single(panel => panel.Name == "PART_SizeMatrix");
            matrix.HorizontalAlignment.ShouldBe(HorizontalAlignment.Left);
            var header = preview.GetVisualDescendants().OfType<Avalonia.Controls.Grid>()
                .Single(grid => grid.Name == "PART_SizeMatrixHeader");
            header.ColumnDefinitions.Count.ShouldBe(6);
            header.ColumnDefinitions.Skip(1).All(column => column.Width.IsAuto).ShouldBeTrue();
            foreach (var size in ExpectedSizes)
            {
                header.Children.OfType<Text>().Any(text => string.Equals(text.Text, size.ToString(), StringComparison.Ordinal))
                    .ShouldBeTrue(size.ToString());
            }

            var buttonGroupRow = preview.GetVisualDescendants().OfType<Avalonia.Controls.Grid>()
                .Single(grid => grid.Name == "PART_SizeRow_ButtonGroup");
            buttonGroupRow.ColumnDefinitions.Count.ShouldBe(6);
            buttonGroupRow.Children.OfType<StackPanel>()
                .Count(panel => panel.Name?.StartsWith("PART_SizeCell_ButtonGroup_", StringComparison.Ordinal) == true)
                .ShouldBe(ExpectedSizes.Length);

            var toggleGroupRow = preview.GetVisualDescendants().OfType<Avalonia.Controls.Grid>()
                .Single(grid => grid.Name == "PART_SizeRow_ToggleGroup");
            toggleGroupRow.ColumnDefinitions.Count.ShouldBe(6);
            toggleGroupRow.Children.OfType<StackPanel>()
                .Count(panel => panel.Name?.StartsWith("PART_SizeCell_ToggleGroup_", StringComparison.Ordinal) == true)
                .ShouldBe(ExpectedSizes.Length);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Field_gallery_custom_text_box_stays_chromeless_when_focused()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/Field");
        page.Code.ShouldContain("Label = \"Phone\"");
        page.Code.ShouldContain("Label = \"Accent\"");
        page.Code.ShouldContain("Label = \"Notification channels\"");
        page.Code.ShouldContain("Label = \"Quick filter\"");
        page.Code.ShouldContain("Label = \"Custom amount\"");
        page.Code.ShouldContain("Label = \"Read-only token\"");
        page.Code.ShouldContain("Variant = Variant.Filled");
        page.Code.ShouldContain("Variant = Variant.Text");
        page.Code.ShouldContain("InnerPadding = false");
        page.Code.ShouldContain("IsEnabled = false");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var fields = preview.GetVisualDescendants().OfType<Field>().ToArray();
            fields.Length.ShouldBeGreaterThanOrEqualTo(6);
            fields.Select(field => field.Label).ShouldContain("Phone");
            fields.Select(field => field.Label).ShouldContain("Accent");
            fields.Select(field => field.Label).ShouldContain("Notification channels");
            fields.Select(field => field.Label).ShouldContain("Quick filter");
            fields.Select(field => field.Label).ShouldContain("Custom amount");
            fields.Select(field => field.Label).ShouldContain("Read-only token");
            fields.Any(field => field.Variant == Variant.Filled).ShouldBeTrue();
            fields.Any(field => field.Variant == Variant.Text).ShouldBeTrue();
            fields.Any(field => field.Error && !string.IsNullOrWhiteSpace(field.ErrorText)).ShouldBeTrue();
            fields.Any(field => !field.InnerPadding).ShouldBeTrue();
            fields.Any(field => !field.IsEnabled).ShouldBeTrue();

            foreach (var field in fields.Where(field => field.Variant == Variant.Outlined && !string.IsNullOrWhiteSpace(field.Label)))
            {
                var inputBorder = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_InputBorder");
                var labelHost = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_LabelHost");
                inputBorder.Margin.Top.ShouldBe(LoamFieldMetrics.Default.FloatingLabelTopMargin);
                labelHost.Margin.Top.ShouldBe(0);
                labelHost.IsVisible.ShouldBeTrue();
            }

            var textBox = preview.GetVisualDescendants().OfType<TextBox>().First();
            textBox.ApplyTemplate();
            textBox.Focus();
            Dispatcher.UIThread.RunJobs();

            textBox.BorderThickness.ShouldBe(default);
            textBox.FocusAdorner.ShouldBeNull();
            ((ISolidColorBrush)textBox.Resources["TextBoxBorderBrushFocused"]!).Color.A.ShouldBe((byte)0);

            foreach (var border in textBox.GetVisualDescendants().OfType<Border>().Where(border => border.Name == "PART_BorderElement"))
            {
                border.BorderThickness.ShouldBe(default);
                if (border.BorderBrush is ISolidColorBrush brush)
                {
                    brush.Color.A.ShouldBe((byte)0);
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void TextField_gallery_shows_real_state_variants()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/TextField");
        foreach (var expected in new[] { "Outlined", "Filled", "Text / underline", "Budget", "Always floated", "Read-only", "Email", "Disabled" })
        {
            page.Code.ShouldContain($"Label = \"{expected}\"");
        }

        page.Code.ShouldContain("Variant = Variant.Outlined");
        page.Code.ShouldContain("Variant = Variant.Filled");
        page.Code.ShouldContain("Variant = Variant.Text");
        page.Code.ShouldContain("StartAdornment = new TextBlock");
        page.Code.ShouldContain("EndAdornment = new TextBlock");
        page.Code.ShouldContain("ShrinkLabel = true");
        page.Code.ShouldContain("ReadOnly = true");
        page.Code.ShouldContain("Required = true");
        page.Code.ShouldContain("Error = true");
        page.Code.ShouldContain("IsEnabled = false");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var fields = preview.GetVisualDescendants().OfType<TextField>().ToArray();
            fields.Length.ShouldBe(8);
            fields.All(field => field.Width == 320).ShouldBeTrue();

            var labels = fields.Select(field => field.Label).ToArray();
            labels.ShouldContain("Outlined");
            labels.ShouldContain("Filled");
            labels.ShouldContain("Text / underline");
            labels.ShouldContain("Budget");
            labels.ShouldContain("Always floated");
            labels.ShouldContain("Read-only");
            labels.ShouldContain("Email");
            labels.ShouldContain("Disabled");

            fields.Any(field => field.Variant == Variant.Outlined).ShouldBeTrue();
            fields.Any(field => field.Variant == Variant.Filled).ShouldBeTrue();
            fields.Any(field => field.Variant == Variant.Text).ShouldBeTrue();
            fields.Any(field => field.StartAdornment is TextBlock && field.EndAdornment is TextBlock).ShouldBeTrue();
            fields.Any(field => field.ShrinkLabel).ShouldBeTrue();
            fields.Any(field => field.ReadOnly).ShouldBeTrue();
            fields.Any(field => field.Required && field.Error && !string.IsNullOrWhiteSpace(field.ErrorText)).ShouldBeTrue();
            fields.Any(field => !field.IsEnabled).ShouldBeTrue();

            foreach (var field in fields.Where(field =>
                         field.Variant == Variant.Outlined &&
                         (field.ShrinkLabel || !string.IsNullOrWhiteSpace(field.Text))))
            {
                var inputBorder = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_InputBorder");
                var labelHost = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_LabelHost");
                inputBorder.Margin.Top.ShouldBe(LoamFieldMetrics.Default.FloatingLabelTopMargin);
                labelHost.Margin.Top.ShouldBe(0);
                labelHost.IsVisible.ShouldBeTrue();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NumericField_gallery_shows_real_state_variants()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/NumericField");
        foreach (var expected in new[] { "Quantity", "Price", "Text / underline", "Step 0.25", "Maximum", "Temperature", "Amount", "Disabled" })
        {
            page.Code.ShouldContain($"Label = \"{expected}\"");
        }

        page.Code.ShouldContain("Variant = Variant.Filled");
        page.Code.ShouldContain("Variant = Variant.Text");
        page.Code.ShouldContain("Minimum = -40");
        page.Code.ShouldContain("Maximum = 120");
        page.Code.ShouldContain("Step = 0.25");
        page.Code.ShouldContain("Format = \"0.00\"");
        page.Code.ShouldContain("Error = true");
        page.Code.ShouldContain("IsEnabled = false");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var fields = preview.GetVisualDescendants().OfType<NumericField>().ToArray();
            fields.Length.ShouldBe(8);
            fields.All(field => field.Width == 320).ShouldBeTrue();

            var labels = fields.Select(field => field.Label).ToArray();
            labels.ShouldContain("Quantity");
            labels.ShouldContain("Price");
            labels.ShouldContain("Text / underline");
            labels.ShouldContain("Step 0.25");
            labels.ShouldContain("Maximum");
            labels.ShouldContain("Temperature");
            labels.ShouldContain("Amount");
            labels.ShouldContain("Disabled");

            fields.Any(field => field.Variant == Variant.Filled).ShouldBeTrue();
            fields.Any(field => field.Variant == Variant.Text).ShouldBeTrue();
            fields.Any(field => field.Step == 0.25 && field.Format == "0.00").ShouldBeTrue();
            fields.Any(field => field.Minimum == -40 && field.Maximum == 120 && field.Value == -3.5).ShouldBeTrue();
            fields.Any(field => field.Error && !string.IsNullOrWhiteSpace(field.ErrorText)).ShouldBeTrue();
            fields.Any(field => !field.IsEnabled).ShouldBeTrue();

            foreach (var field in fields.Where(field => field.Variant == Variant.Outlined && !string.IsNullOrWhiteSpace(field.Label)))
            {
                var inputBorder = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_InputBorder");
                var labelHost = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_LabelHost");
                var up = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_Up");
                var down = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_Down");

                inputBorder.Margin.Top.ShouldBe(LoamFieldMetrics.Default.FloatingLabelTopMargin);
                labelHost.Margin.Top.ShouldBe(0);
                labelHost.IsVisible.ShouldBeTrue();
                AutomationProperties.GetName(up).ShouldBe("Increase value");
                AutomationProperties.GetName(down).ShouldBe("Decrease value");
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MaskedTextField_gallery_shows_real_state_variants()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/MaskedTextField");
        foreach (var expected in new[] { "Phone", "Postal code", "Date", "Access code", "Product key", "Partial phone", "Invalid", "Disabled" })
        {
            page.Code.ShouldContain($"Label = \"{expected}\"");
        }

        page.Code.ShouldContain("Pattern = \"(###) ###-####\"");
        page.Code.ShouldContain("Pattern = \"AAA-###\"");
        page.Code.ShouldContain("Pattern = \"***-***\"");
        page.Code.ShouldContain("Variant = Variant.Filled");
        page.Code.ShouldContain("Variant = Variant.Text");
        page.Code.ShouldContain("Color = LoamColor.Secondary");
        page.Code.ShouldContain("Error = true");
        page.Code.ShouldContain("IsEnabled = false");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var fields = preview.GetVisualDescendants().OfType<MaskedTextField>().ToArray();
            fields.Length.ShouldBe(8);
            fields.All(field => field.Width == 320).ShouldBeTrue();

            var labels = fields.Select(field => field.Label).ToArray();
            labels.ShouldContain("Phone");
            labels.ShouldContain("Postal code");
            labels.ShouldContain("Date");
            labels.ShouldContain("Access code");
            labels.ShouldContain("Product key");
            labels.ShouldContain("Partial phone");
            labels.ShouldContain("Invalid");
            labels.ShouldContain("Disabled");

            fields.Any(field => field.Variant == Variant.Filled).ShouldBeTrue();
            fields.Any(field => field.Variant == Variant.Text).ShouldBeTrue();
            fields.Any(field => field.Pattern == "AAA-###" && field.Text == "abc-123").ShouldBeTrue();
            fields.Any(field => field.Pattern == "***-***" && field.Text == "A7Z-9Q1").ShouldBeTrue();
            fields.Any(field => field.Error && !string.IsNullOrWhiteSpace(field.ErrorText)).ShouldBeTrue();
            fields.Any(field => !field.IsEnabled).ShouldBeTrue();

            foreach (var field in fields.Where(field =>
                         field.Variant == Variant.Outlined &&
                         !string.IsNullOrWhiteSpace(field.Label) &&
                         !string.IsNullOrWhiteSpace(field.Text)))
            {
                var inputBorder = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_InputBorder");
                var labelHost = field.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_LabelHost");

                inputBorder.Margin.Top.ShouldBe(LoamFieldMetrics.Default.FloatingLabelTopMargin);
                labelHost.Margin.Top.ShouldBe(0);
                labelHost.IsVisible.ShouldBeTrue();
                AutomationProperties.GetName(field).ShouldBe(field.Label);
                var helpText = AutomationProperties.GetHelpText(field);
                helpText.ShouldNotBeNull();
                helpText.ShouldContain("Pattern");
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Form_gallery_uses_realistic_field_layout()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/Form");
        page.Code.ShouldContain("Project access");
        page.Code.ShouldContain("Title = title");
        page.Code.ShouldContain("Validation error");
        page.Code.ShouldContain("Ready state");
        page.Code.ShouldContain("Disabled");
        page.Code.ShouldContain("Action sizes");
        page.Code.ShouldContain("HelperText = \"Fill the required fields and validate.\"");
        page.Code.ShouldContain("SuccessText = \"Ready to submit.\"");
        page.Code.ShouldContain("ErrorText = \"Review the highlighted fields.\"");
        page.Code.ShouldContain("SubmitIcon = Icons.Material.Filled.Check");
        foreach (var size in ExpectedSizes)
        {
            page.Code.ShouldContain($"LoamSize.{size}");
        }

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var fields = preview.GetVisualDescendants().OfType<TextField>().ToArray();
            fields.Length.ShouldBeGreaterThanOrEqualTo(12);
            fields.All(field => field.Width >= 320).ShouldBeTrue();

            preview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Any(button => string.Equals(button.Content as string, "Validate", StringComparison.Ordinal))
                .ShouldBeTrue();

            var forms = preview.GetVisualDescendants().OfType<Form>().ToArray();
            forms.Length.ShouldBeGreaterThanOrEqualTo(8);
            var form = forms.Single(form => form.Title == "Project access");
            form.Title.ShouldBe("Project access");
            form.HelperText.ShouldBe("Fill the required fields and validate.");
            form.SuccessText.ShouldBe("Ready to submit.");
            form.ErrorText.ShouldBe("Review the highlighted fields.");
            form.ActionSize.ShouldBe(LoamSize.Small);
            form.ResetVariant.ShouldBe(Variant.Outlined);
            form.ActionsHorizontalAlignment.ShouldBe(HorizontalAlignment.Right);
            form.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Project access").ShouldBeTrue();
            form.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Fill the required fields and validate.").ShouldBeTrue();
            forms.Any(form => form.Title == "Validation error" && !form.IsValid).ShouldBeTrue();
            forms.Any(form => form.Title == "Ready state" && form.IsValid).ShouldBeTrue();
            forms.Any(form => form.Title == "Disabled" && !form.IsEnabled).ShouldBeTrue();
            AssertAllSizes(forms, form => form.ActionSize);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DatePicker_gallery_shows_state_variants()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Pickers/DatePicker");
        foreach (var expected in new[] { "Outlined", "Filled", "Text / underline", "Empty", "Selected", "Custom format", "Constrained", "Floating label", "Error", "Disabled" })
        {
            page.Code.ShouldContain(expected);
        }
        page.Code.ShouldContain("Variant = Variant.Outlined");
        page.Code.ShouldContain("Variant = Variant.Filled");
        page.Code.ShouldContain("Variant = Variant.Text");
        page.Code.ShouldContain("PickerTitle = \"Select invoice date\"");
        page.Code.ShouldContain("CancelText = \"Dismiss\"");
        page.Code.ShouldContain("OkText = \"Apply\"");
        page.Code.ShouldNotContain("DateCase");
        page.Code.ShouldNotContain("new Text { Text = \"Outlined\"");
        page.Code.ShouldNotContain("new Text { Text = \"Filled\"");
        page.Code.ShouldNotContain("new Text { Text = \"Text / underline\"");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var pickers = preview.GetVisualDescendants().OfType<Loam.Controls.DatePicker>().ToArray();
            pickers.Length.ShouldBeGreaterThanOrEqualTo(10);
            pickers.All(picker => picker.Width == 360).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Outlined).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Filled).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Text).ShouldBeTrue();
            pickers.Any(picker => picker.Date is null).ShouldBeTrue();
            pickers.Any(picker => picker.Date is not null).ShouldBeTrue();
            pickers.Any(picker => picker.DateFormat != "d").ShouldBeTrue();
            pickers.Any(picker => picker.MinDate is not null && picker.MaxDate is not null).ShouldBeTrue();
            pickers.Any(picker => picker.ShrinkLabel).ShouldBeTrue();
            pickers.Any(picker => picker.Error && !string.IsNullOrWhiteSpace(picker.ErrorText)).ShouldBeTrue();
            pickers.Any(picker => !picker.IsEnabled).ShouldBeTrue();
            pickers.Any(picker => picker.PickerTitle == "Select invoice date").ShouldBeTrue();
            pickers.Any(picker => picker.CancelText == "Dismiss" && picker.OkText == "Apply").ShouldBeTrue();
            pickers.Select(picker => picker.Label).ShouldContain("Outlined");
            pickers.Select(picker => picker.Label).ShouldContain("Filled");
            pickers.Select(picker => picker.Label).ShouldContain("Text / underline");
            pickers.Select(picker => picker.Label).ShouldContain("Custom format");
            pickers.Select(picker => picker.Label).ShouldContain("Constrained");
            pickers.Select(picker => picker.Label).ShouldContain("Disabled");

            var labels = preview.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ToArray();
            labels.ShouldContain("Outlined");
            labels.ShouldContain("Filled");
            labels.ShouldContain("Text / underline");
            labels.ShouldContain("Custom format");
            labels.ShouldContain("Constrained");
            labels.ShouldContain("Disabled");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DateRangePicker_gallery_shows_state_variants()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Pickers/DateRangePicker");
        foreach (var expected in new[] { "Outlined", "Filled", "Text / underline", "Empty", "Selected", "Custom format", "Constrained", "Floating label", "Error", "Disabled" })
        {
            page.Code.ShouldContain(expected);
        }
        page.Code.ShouldContain("Variant = Variant.Outlined");
        page.Code.ShouldContain("Variant = Variant.Filled");
        page.Code.ShouldContain("Variant = Variant.Text");
        page.Code.ShouldContain("PickerTitle = \"Select trip range\"");
        page.Code.ShouldContain("CancelText = \"Dismiss\"");
        page.Code.ShouldContain("OkText = \"Apply\"");
        page.Code.ShouldNotContain("RangeCase");
        page.Code.ShouldNotContain("DateCase");
        page.Code.ShouldNotContain("new Text { Text = \"Outlined\"");
        page.Code.ShouldNotContain("new Text { Text = \"Filled\"");
        page.Code.ShouldNotContain("new Text { Text = \"Text / underline\"");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var pickers = preview.GetVisualDescendants().OfType<Loam.Controls.DateRangePicker>().ToArray();
            pickers.Length.ShouldBeGreaterThanOrEqualTo(10);
            pickers.All(picker => picker.Width == 360).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Outlined).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Filled).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Text).ShouldBeTrue();
            pickers.Any(picker => picker.Start is null && picker.End is null).ShouldBeTrue();
            pickers.Any(picker => picker.Start is not null && picker.End is not null).ShouldBeTrue();
            pickers.Any(picker => picker.DateFormat != "d").ShouldBeTrue();
            pickers.Any(picker => picker.MinDate is not null && picker.MaxDate is not null).ShouldBeTrue();
            pickers.Any(picker => picker.ShrinkLabel).ShouldBeTrue();
            pickers.Any(picker => picker.Error && !string.IsNullOrWhiteSpace(picker.ErrorText)).ShouldBeTrue();
            pickers.Any(picker => !picker.IsEnabled).ShouldBeTrue();
            pickers.Any(picker => picker.PickerTitle == "Select trip range").ShouldBeTrue();
            pickers.Any(picker => picker.CancelText == "Dismiss" && picker.OkText == "Apply").ShouldBeTrue();
            pickers.Select(picker => picker.Label).ShouldContain("Outlined");
            pickers.Select(picker => picker.Label).ShouldContain("Filled");
            pickers.Select(picker => picker.Label).ShouldContain("Text / underline");
            pickers.Select(picker => picker.Label).ShouldContain("Custom format");
            pickers.Select(picker => picker.Label).ShouldContain("Constrained");
            pickers.Select(picker => picker.Label).ShouldContain("Disabled");

            var labels = preview.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ToArray();
            labels.ShouldContain("Outlined");
            labels.ShouldContain("Filled");
            labels.ShouldContain("Text / underline");
            labels.ShouldContain("Custom format");
            labels.ShouldContain("Constrained");
            labels.ShouldContain("Disabled");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void TimePicker_gallery_shows_state_variants()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Pickers/TimePicker");
        foreach (var expected in new[] { "Outlined", "Filled", "Text / underline", "Empty", "Selected", "24-hour format", "Minute step", "Floating label", "Error", "Disabled" })
        {
            page.Code.ShouldContain(expected);
        }
        page.Code.ShouldContain("Variant = Variant.Outlined");
        page.Code.ShouldContain("Variant = Variant.Filled");
        page.Code.ShouldContain("Variant = Variant.Text");
        page.Code.ShouldContain("PickerTitle = \"Select reminder time\"");
        page.Code.ShouldContain("CancelText = \"Dismiss\"");
        page.Code.ShouldContain("OkText = \"Apply\"");
        page.Code.ShouldNotContain("TimeCase");
        page.Code.ShouldNotContain("DateCase");
        page.Code.ShouldNotContain("new Text { Text = \"Outlined\"");
        page.Code.ShouldNotContain("new Text { Text = \"Filled\"");
        page.Code.ShouldNotContain("new Text { Text = \"Text / underline\"");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var pickers = preview.GetVisualDescendants().OfType<Loam.Controls.TimePicker>().ToArray();
            pickers.Length.ShouldBeGreaterThanOrEqualTo(10);
            pickers.All(picker => picker.Width == 360).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Outlined).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Filled).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Text).ShouldBeTrue();
            pickers.Any(picker => picker.Time is null).ShouldBeTrue();
            pickers.Any(picker => picker.Time is not null).ShouldBeTrue();
            pickers.Any(picker => picker.TimeFormat != "t").ShouldBeTrue();
            pickers.Any(picker => picker.MinuteStep != 5).ShouldBeTrue();
            pickers.Any(picker => picker.ShrinkLabel).ShouldBeTrue();
            pickers.Any(picker => picker.Error && !string.IsNullOrWhiteSpace(picker.ErrorText)).ShouldBeTrue();
            pickers.Any(picker => !picker.IsEnabled).ShouldBeTrue();
            pickers.Any(picker => picker.PickerTitle == "Select reminder time").ShouldBeTrue();
            pickers.Any(picker => picker.CancelText == "Dismiss" && picker.OkText == "Apply").ShouldBeTrue();
            pickers.Select(picker => picker.Label).ShouldContain("Outlined");
            pickers.Select(picker => picker.Label).ShouldContain("Filled");
            pickers.Select(picker => picker.Label).ShouldContain("Text / underline");
            pickers.Select(picker => picker.Label).ShouldContain("24-hour format");
            pickers.Select(picker => picker.Label).ShouldContain("Minute step");
            pickers.Select(picker => picker.Label).ShouldContain("Disabled");

            var labels = preview.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ToArray();
            labels.ShouldContain("Outlined");
            labels.ShouldContain("Filled");
            labels.ShouldContain("Text / underline");
            labels.ShouldContain("24-hour format");
            labels.ShouldContain("Minute step");
            labels.ShouldContain("Disabled");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void MonthCalendar_gallery_shows_state_variants()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Pickers/MonthCalendar");
        foreach (var expected in new[] { "Selected", "Range", "Constrained", "RangeStart", "RangeEnd", "MinDate", "MaxDate", "FirstDayOfWeek = DayOfWeek.Monday", "DateSelected +=" })
        {
            page.Code.ShouldContain(expected);
        }
        page.Code.ShouldNotContain("CalendarCase");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var calendars = preview.GetVisualDescendants().OfType<MonthCalendar>().ToArray();
            calendars.Length.ShouldBeGreaterThanOrEqualTo(3);
            calendars.Any(calendar => calendar.SelectedDate == new DateTime(2026, 6, 4)).ShouldBeTrue();
            calendars.Any(calendar => calendar.RangeStart == new DateTime(2026, 6, 10) && calendar.RangeEnd == new DateTime(2026, 6, 16)).ShouldBeTrue();
            calendars.Any(calendar => calendar.MinDate == new DateTime(2026, 6, 10) && calendar.MaxDate == new DateTime(2026, 6, 20)).ShouldBeTrue();
            calendars.All(calendar => calendar.FirstDayOfWeek == DayOfWeek.Monday).ShouldBeTrue();

            var labels = preview.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ToArray();
            labels.ShouldContain("Selected");
            labels.ShouldContain("Range");
            labels.ShouldContain("Constrained");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ColorPicker_gallery_shows_state_variants()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Pickers/ColorPicker");
        foreach (var expected in new[] { "Outlined", "Filled", "Text / underline", "Default value", "Alpha", "Custom palette", "Error", "Disabled" })
        {
            page.Code.ShouldContain(expected);
        }
        page.Code.ShouldContain("Variant = Variant.Outlined");
        page.Code.ShouldContain("Variant = Variant.Filled");
        page.Code.ShouldContain("Variant = Variant.Text");
        page.Code.ShouldContain("ShowAlpha = true");
        page.Code.ShouldContain("Palette =");
        page.Code.ShouldNotContain("ColorCase");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var pickers = preview.GetVisualDescendants().OfType<Loam.Controls.ColorPicker>().ToArray();
            pickers.Length.ShouldBeGreaterThanOrEqualTo(8);
            pickers.All(picker => picker.Width == 360).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Outlined).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Filled).ShouldBeTrue();
            pickers.Any(picker => picker.Variant == Variant.Text).ShouldBeTrue();
            pickers.Any(picker => picker.ShowAlpha).ShouldBeTrue();
            pickers.Any(picker => picker.Palette.Count >= 5).ShouldBeTrue();
            pickers.Any(picker => picker.Error && !string.IsNullOrWhiteSpace(picker.ErrorText)).ShouldBeTrue();
            pickers.Any(picker => !picker.IsEnabled).ShouldBeTrue();
            pickers.Select(picker => picker.Label).ShouldContain("Outlined");
            pickers.Select(picker => picker.Label).ShouldContain("Filled");
            pickers.Select(picker => picker.Label).ShouldContain("Text / underline");
            pickers.Select(picker => picker.Label).ShouldContain("Alpha");
            pickers.All(picker => !string.IsNullOrWhiteSpace(picker.HelperText) || picker.Error).ShouldBeTrue();

            var labels = preview.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ToArray();
            labels.ShouldContain("Outlined");
            labels.ShouldContain("Filled");
            labels.ShouldContain("Text / underline");
            labels.ShouldContain("Alpha");
            labels.ShouldContain("Disabled");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void FileUpload_gallery_shows_state_variants()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/FileUpload");
        foreach (var expected in new[] { "Outlined", "Filled", "Text", "Multiple files", "Sizes", "ExtraSmall", "Small", "Medium", "Large", "ExtraLarge", "Disabled" })
        {
            page.Code.ShouldContain(expected);
        }
        page.Code.ShouldContain("ShowSelection");
        page.Code.ShouldContain("Variant = Variant.Outlined");
        page.Code.ShouldContain("Variant = Variant.Filled");
        page.Code.ShouldContain("Variant = Variant.Text");
        page.Code.ShouldContain("Size = LoamSize.Small");
        page.Code.ShouldContain("LoamSize.Medium");
        page.Code.ShouldContain("Size = LoamSize.Large");
        page.Code.ShouldContain("LoamSize.ExtraSmall");
        page.Code.ShouldContain("LoamSize.ExtraLarge");
        page.Code.ShouldContain("AcceptedFileTypes");
        page.Code.ShouldContain("ShowRemoveButtons = true");
        page.Code.ShouldContain("ShowClearButton = true");
        page.Code.ShouldContain("Label = \"Outlined\"");
        page.Code.ShouldContain("HelperText =");
        page.Code.ShouldContain("EmptyText =");
        page.Code.ShouldContain("SelectedTextFormat =");
        page.Code.ShouldNotContain("UploadCase");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var uploads = preview.GetVisualDescendants().OfType<FileUpload>().ToArray();
            uploads.Length.ShouldBeGreaterThanOrEqualTo(7);
            uploads.All(upload => upload.Width == 360).ShouldBeTrue();
            uploads.Any(upload => upload.Variant == Variant.Outlined).ShouldBeTrue();
            uploads.Any(upload => upload.Variant == Variant.Filled).ShouldBeTrue();
            uploads.Any(upload => upload.Variant == Variant.Text).ShouldBeTrue();
            uploads.Any(upload => upload.Size == LoamSize.Small).ShouldBeTrue();
            uploads.Any(upload => upload.Size == LoamSize.Large).ShouldBeTrue();
            AssertAllSizes(uploads, upload => upload.Size);
            uploads.Any(upload => upload.AllowMultiple).ShouldBeTrue();
            uploads.Any(upload => !upload.AllowMultiple).ShouldBeTrue();
            uploads.Any(upload => upload.FileNames.Count > 1).ShouldBeTrue();
            uploads.Any(upload => upload.AcceptedFileTypes is not null).ShouldBeTrue();
            uploads.Any(upload => upload.ShowRemoveButtons).ShouldBeTrue();
            uploads.Any(upload => upload.ShowClearButton).ShouldBeTrue();
            uploads.Any(upload => !upload.IsEnabled).ShouldBeTrue();
            uploads.All(upload => !string.IsNullOrWhiteSpace(upload.Label)).ShouldBeTrue();
            uploads.All(upload => !string.IsNullOrWhiteSpace(upload.HelperText)).ShouldBeTrue();
            uploads.All(upload => !string.IsNullOrWhiteSpace(upload.EmptyText)).ShouldBeTrue();

            var buttons = preview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Where(button => button.Name == "PART_Button")
                .ToArray();
            buttons.Length.ShouldBe(uploads.Length);
            buttons.Any(button => button.Variant == Variant.Filled).ShouldBeTrue();
            buttons.Any(button => button.Size == LoamSize.Large).ShouldBeTrue();

            var chips = preview.GetVisualDescendants().OfType<Chip>().ToArray();
            AssertAllSizes(chips, chip => chip.Size);
            var chipTexts = chips.Select(chip => chip.Text).ToArray();
            chipTexts.ShouldContain("invoice-0626.pdf");
            chipTexts.ShouldContain("metrics.csv");

            var labels = preview.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ToArray();
            labels.ShouldContain("Outlined");
            labels.ShouldContain("No images attached");
            labels.ShouldContain("3 evidence files selected");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Selection_input_gallery_pages_show_state_matrices()
    {
        var checkBoxPage = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/CheckBox");
        AssertSourceCoverage(checkBoxPage, ["Checked", "Unchecked", "Indeterminate", "Sizes", "Disabled"], "CheckCase");
        var checkBoxPreview = checkBoxPage.Build();
        var checkBoxWindow = Show(checkBoxPreview, ThemeVariant.Light);
        try
        {
            var checkBoxes = checkBoxPreview.GetVisualDescendants().OfType<Loam.Controls.CheckBox>().ToArray();
            checkBoxes.Length.ShouldBeGreaterThanOrEqualTo(13);
            checkBoxes.Any(checkBox => checkBox.IsChecked == true).ShouldBeTrue();
            checkBoxes.Any(checkBox => checkBox.IsChecked == false).ShouldBeTrue();
            checkBoxes.Any(checkBox => checkBox.IsThreeState && checkBox.IsChecked is null).ShouldBeTrue();
            checkBoxes.Any(checkBox => !checkBox.IsEnabled).ShouldBeTrue();
            AssertAllSizes(checkBoxes, checkBox => checkBox.Size);
        }
        finally
        {
            checkBoxWindow.Close();
        }

        var switchPage = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/Switch");
        AssertSourceCoverage(switchPage, ["On", "Off", "Success", "Warning", "Sizes", "Disabled"], "SwitchCase");
        var switchPreview = switchPage.Build();
        var switchWindow = Show(switchPreview, ThemeVariant.Light);
        try
        {
            var switches = switchPreview.GetVisualDescendants().OfType<Loam.Controls.Switch>().ToArray();
            switches.Length.ShouldBeGreaterThanOrEqualTo(11);
            switches.Any(toggle => toggle.IsChecked == true).ShouldBeTrue();
            switches.Any(toggle => toggle.IsChecked == false).ShouldBeTrue();
            switches.Any(toggle => !toggle.IsEnabled).ShouldBeTrue();
            AssertAllSizes(switches, toggle => toggle.Size);
        }
        finally
        {
            switchWindow.Close();
        }

        var radioPage = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/Radio");
        AssertSourceCoverage(radioPage, ["Selected", "Unselected", "Error", "Sizes", "Disabled"], "RadioCase");
        var radioPreview = radioPage.Build();
        var radioWindow = Show(radioPreview, ThemeVariant.Light);
        try
        {
            var radios = radioPreview.GetVisualDescendants().OfType<Radio>().ToArray();
            radios.Length.ShouldBeGreaterThanOrEqualTo(11);
            radios.Any(radio => radio.IsChecked == true).ShouldBeTrue();
            radios.Any(radio => radio.IsChecked == false).ShouldBeTrue();
            radios.Any(radio => !radio.IsEnabled).ShouldBeTrue();
            AssertAllSizes(radios, radio => radio.Size);
        }
        finally
        {
            radioWindow.Close();
        }

        var radioGroupPage = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/RadioGroup");
        AssertSourceCoverage(radioGroupPage, ["Vertical group", "Horizontal group", "Disabled group"], "RadioGroupCase");
        var radioGroupPreview = radioGroupPage.Build();
        var radioGroupWindow = Show(radioGroupPreview, ThemeVariant.Light);
        try
        {
            var groups = radioGroupPreview.GetVisualDescendants().OfType<RadioGroup>().ToArray();
            groups.Length.ShouldBeGreaterThanOrEqualTo(3);
            groups.Any(group => Equals(group.Value, "express")).ShouldBeTrue();
            groups.Any(group => Equals(group.Value, "push")).ShouldBeTrue();
            groups.Any(group => !group.IsEnabled).ShouldBeTrue();
            radioGroupPreview.GetVisualDescendants().OfType<Radio>().Count().ShouldBeGreaterThanOrEqualTo(8);
        }
        finally
        {
            radioGroupWindow.Close();
        }

        var sliderPage = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/Slider");
        AssertSourceCoverage(sliderPage, ["Default range", "Custom min and max", "Color states", "Zero value", "Disabled"], "SliderCase");
        var sliderPreview = sliderPage.Build();
        var sliderWindow = Show(sliderPreview, ThemeVariant.Light);
        try
        {
            var sliders = sliderPreview.GetVisualDescendants().OfType<Loam.Controls.Slider>().ToArray();
            sliders.Length.ShouldBeGreaterThanOrEqualTo(5);
            sliders.All(slider => slider.Width == 360).ShouldBeTrue();
            sliders.Any(slider => slider.Minimum == 20 && slider.Maximum == 120).ShouldBeTrue();
            sliders.Any(slider => slider.Value == 0).ShouldBeTrue();
            sliders.Any(slider => !slider.IsEnabled).ShouldBeTrue();
        }
        finally
        {
            sliderWindow.Close();
        }

        var toggleGroupPage = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/ToggleGroup");
        AssertSourceCoverage(toggleGroupPage, ["Selected", "Color", "No selection", "Sizes", "Disabled"], "ToggleCase");
        var toggleGroupPreview = toggleGroupPage.Build();
        var toggleGroupWindow = Show(toggleGroupPreview, ThemeVariant.Light);
        try
        {
            var groups = toggleGroupPreview.GetVisualDescendants().OfType<ToggleGroup>().ToArray();
            groups.Length.ShouldBeGreaterThanOrEqualTo(4);
            groups.Any(group => Equals(group.SelectedValue, "week")).ShouldBeTrue();
            groups.Any(group => Equals(group.SelectedValue, "high") && group.Color == LoamColor.Secondary).ShouldBeTrue();
            groups.Any(group => group.SelectedValue is null).ShouldBeTrue();
            groups.Any(group => !group.IsEnabled).ShouldBeTrue();
            AssertAllSizes(groups, group => group.Size);
        }
        finally
        {
            toggleGroupWindow.Close();
        }

        var ratingPage = ComponentsView.PageCatalog.Single(page => page.Route == "Inputs/Rating");
        AssertSourceCoverage(ratingPage, ["States", "Sizes", "MaxValue = 6", "ReadOnly = true"], "RatingCase");
        var ratingPreview = ratingPage.Build();
        var ratingWindow = Show(ratingPreview, ThemeVariant.Light);
        try
        {
            var ratings = ratingPreview.GetVisualDescendants().OfType<Rating>().ToArray();
            ratings.Length.ShouldBeGreaterThanOrEqualTo(9);
            ratings.Any(rating => rating.MaxValue == 6).ShouldBeTrue();
            ratings.Any(rating => rating.ReadOnly).ShouldBeTrue();
            ratings.Any(rating => !rating.IsEnabled).ShouldBeTrue();
            AssertAllSizes(ratings, rating => rating.Size);
        }
        finally
        {
            ratingWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Chip_gallery_pages_show_component_state_matrices()
    {
        var chipPage = ComponentsView.PageCatalog.Single(page => page.Route == "Display/Chip");
        foreach (var expected in new[] { "Variants", "Colors", "Sizes", "Disabled", "Label shape", "Closeable" })
        {
            chipPage.Code.ShouldContain(expected);
        }

        foreach (var size in ExpectedSizes)
        {
            chipPage.Code.ShouldContain($"LoamSize.{size}");
        }

        chipPage.Code.ShouldContain("Variant = Variant.Outlined");
        chipPage.Code.ShouldContain("Variant = Variant.Text");
        chipPage.Code.ShouldContain("Closeable = true");
        chipPage.Code.ShouldContain("Label = true");
        chipPage.Code.ShouldNotContain("ChipCase");

        var chipPreview = chipPage.Build();
        var chipWindow = Show(chipPreview, ThemeVariant.Light);
        try
        {
            var chips = chipPreview.GetVisualDescendants().OfType<Chip>().ToArray();
            chips.Length.ShouldBeGreaterThanOrEqualTo(20);
            chips.Any(chip => chip.Variant == Variant.Filled).ShouldBeTrue();
            chips.Any(chip => chip.Variant == Variant.Outlined).ShouldBeTrue();
            chips.Any(chip => chip.Variant == Variant.Text).ShouldBeTrue();
            chips.Any(chip => chip.Closeable).ShouldBeTrue();
            chips.Any(chip => chip.Label).ShouldBeTrue();
            chips.Any(chip => !chip.IsEnabled).ShouldBeTrue();
            chips.Any(chip => chip.Icon == Icons.Material.Filled.Star).ShouldBeTrue();
            AssertAllSizes(chips, chip => chip.Size);

            foreach (var chip in chips.Take(4))
            {
                chip.ApplyTemplate();
                chip.GetVisualDescendants().OfType<Border>()
                    .Any(border => border.Name == "PART_StateLayer")
                    .ShouldBeTrue(chip.Text);
            }
        }
        finally
        {
            chipWindow.Close();
        }

        var chipSetPage = ComponentsView.PageCatalog.Single(page => page.Route == "Display/ChipSet");
        foreach (var expected in new[] { "Single mandatory", "Multi-select", "Optional selection", "Disabled set" })
        {
            chipSetPage.Code.ShouldContain(expected);
        }

        chipSetPage.Code.ShouldContain("Selectable = true");
        chipSetPage.Code.ShouldContain("Mandatory = true");
        chipSetPage.Code.ShouldContain("MultiSelect = true");
        chipSetPage.Code.ShouldContain("SelectedIndexes.Add(0)");
        chipSetPage.Code.ShouldNotContain("ChipSetCase");

        var chipSetPreview = chipSetPage.Build();
        var chipSetWindow = Show(chipSetPreview, ThemeVariant.Light);
        try
        {
            var sets = chipSetPreview.GetVisualDescendants().OfType<ChipSet>().ToArray();
            sets.Length.ShouldBeGreaterThanOrEqualTo(4);
            sets.Any(set => set.Selectable && set.Mandatory && set.SelectedIndex == 0).ShouldBeTrue();
            sets.Any(set => set.Selectable && set.MultiSelect && set.SelectedIndexes.SequenceEqual([0, 2])).ShouldBeTrue();
            sets.Any(set => set.Selectable && !set.Mandatory && set.SelectedIndex == -1).ShouldBeTrue();
            sets.Any(set => !set.IsEnabled).ShouldBeTrue();

            var chips = chipSetPreview.GetVisualDescendants().OfType<Chip>().ToArray();
            chips.Length.ShouldBeGreaterThanOrEqualTo(13);
            chips.Any(chip => chip.Closeable).ShouldBeTrue();
            chips.Any(chip => chip.Icon == Icons.Material.Filled.Check).ShouldBeTrue();
        }
        finally
        {
            chipSetWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Badge_avatar_gallery_pages_show_component_state_matrices()
    {
        var badgePage = ComponentsView.PageCatalog.Single(page => page.Route == "Display/Badge");
        foreach (var expected in new[] { "Values", "Origins", "Surface behavior", "BadgeOrigin.TopLeft", "BadgeOrigin.BottomRight", "Bordered = true", "Visible = false" })
        {
            badgePage.Code.ShouldContain(expected);
        }

        badgePage.Code.ShouldContain("Dot = true");
        badgePage.Code.ShouldContain("Overlap = true");
        badgePage.Code.ShouldContain("Max = 99");
        badgePage.Code.ShouldNotContain("BadgeCase");

        var badgePreview = badgePage.Build();
        var badgeWindow = Show(badgePreview, ThemeVariant.Light);
        try
        {
            var badges = badgePreview.GetVisualDescendants().OfType<Badge>().ToArray();
            badges.Length.ShouldBeGreaterThanOrEqualTo(12);
            badges.Any(badge => badge.Value is 150 && badge.Max == 99).ShouldBeTrue();
            badges.Any(badge => string.Equals(badge.Value as string, "NEW", StringComparison.Ordinal)).ShouldBeTrue();
            badges.Any(badge => badge.Dot).ShouldBeTrue();
            badges.Any(badge => badge.Overlap).ShouldBeTrue();
            badges.Any(badge => badge.Bordered).ShouldBeTrue();
            badges.Any(badge => !badge.Visible).ShouldBeTrue();
            badges.Select(badge => badge.Origin).Distinct().Count().ShouldBe(4);
        }
        finally
        {
            badgeWindow.Close();
        }

        var avatarPage = ComponentsView.PageCatalog.Single(page => page.Route == "Display/Avatar");
        foreach (var expected in new[] { "Variants", "Colors", "Shapes", "Sizes", "Rounded = true", "Square = true" })
        {
            avatarPage.Code.ShouldContain(expected);
        }

        foreach (var size in ExpectedSizes)
        {
            avatarPage.Code.ShouldContain($"LoamSize.{size}");
        }

        avatarPage.Code.ShouldContain("Variant = Variant.Outlined");
        avatarPage.Code.ShouldContain("Variant = Variant.Text");
        avatarPage.Code.ShouldNotContain("AvatarCase");

        var avatarPreview = avatarPage.Build();
        var avatarWindow = Show(avatarPreview, ThemeVariant.Light);
        try
        {
            var avatars = avatarPreview.GetVisualDescendants().OfType<Avatar>().ToArray();
            avatars.Length.ShouldBeGreaterThanOrEqualTo(19);
            avatars.Any(avatar => avatar.Variant == Variant.Filled).ShouldBeTrue();
            avatars.Any(avatar => avatar.Variant == Variant.Outlined).ShouldBeTrue();
            avatars.Any(avatar => avatar.Variant == Variant.Text).ShouldBeTrue();
            avatars.Any(avatar => avatar.Rounded).ShouldBeTrue();
            avatars.Any(avatar => avatar.Square).ShouldBeTrue();
            avatars.Any(avatar => avatar.Content is Icon).ShouldBeTrue();
            AssertAllSizes(avatars, avatar => avatar.Size);
            avatars.Any(avatar => AutomationProperties.GetName(avatar) == "AB").ShouldBeTrue();
        }
        finally
        {
            avatarWindow.Close();
        }

        var avatarGroupPage = ComponentsView.PageCatalog.Single(page => page.Route == "Display/AvatarGroup");
        foreach (var expected in new[] { "Overflow", "Compact", "Relaxed spacing", "Rounded", "Square", "Sizes", "Max = 3", "Spacing = -10", "Spacing = 6" })
        {
            avatarGroupPage.Code.ShouldContain(expected);
        }

        avatarGroupPage.Code.ShouldContain("Rounded = true");
        avatarGroupPage.Code.ShouldContain("Square = true");
        avatarGroupPage.Code.ShouldNotContain("AvatarGroupCase");

        var avatarGroupPreview = avatarGroupPage.Build();
        var avatarGroupWindow = Show(avatarGroupPreview, ThemeVariant.Light);
        try
        {
            var groups = avatarGroupPreview.GetVisualDescendants().OfType<AvatarGroup>().ToArray();
            groups.Length.ShouldBeGreaterThanOrEqualTo(10);
            groups.Any(group => group.Items.Count > group.Max).ShouldBeTrue();
            groups.Any(group => group.Spacing == -10).ShouldBeTrue();
            groups.Any(group => group.Spacing == 6).ShouldBeTrue();
            groups.Any(group => group.Items.Any(avatar => avatar.Rounded)).ShouldBeTrue();
            groups.Any(group => group.Items.Any(avatar => avatar.Square)).ShouldBeTrue();
            AssertAllSizes(groups.SelectMany(group => group.Items), avatar => avatar.Size);
            groups.Any(group => AutomationProperties.GetName(group)?.Contains("items", StringComparison.Ordinal) == true).ShouldBeTrue();
        }
        finally
        {
            avatarGroupWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Text_icon_divider_gallery_pages_show_foundation_matrices()
    {
        var textPage = ComponentsView.PageCatalog.Single(page => page.Route == "Display/Text");
        foreach (var expected in new[] { "Display roles", "Content roles", "Legacy aliases", "Colors", "Alignment and wrapping", "Typo.DisplayLarge", "Typo.HeadlineLarge", "Typo.LabelSmall", "GutterBottom = true" })
        {
            textPage.Code.ShouldContain(expected);
        }

        textPage.Code.ShouldContain("Align = TextAlignment.Center");
        textPage.Code.ShouldContain("TextWrapping = TextWrapping.Wrap");
        textPage.Code.ShouldNotContain("TextCase");

        var textPreview = textPage.Build();
        var textWindow = Show(textPreview, ThemeVariant.Light);
        try
        {
            var texts = textPreview.GetVisualDescendants().OfType<Text>().ToArray();
            texts.Length.ShouldBeGreaterThanOrEqualTo(35);
            texts.Any(text => text.Typo == Typo.DisplayLarge).ShouldBeTrue();
            texts.Any(text => text.Typo == Typo.HeadlineLarge).ShouldBeTrue();
            texts.Any(text => text.Typo == Typo.TitleSmall).ShouldBeTrue();
            texts.Any(text => text.Typo == Typo.BodySmall).ShouldBeTrue();
            texts.Any(text => text.Typo == Typo.LabelSmall).ShouldBeTrue();
            texts.Any(text => text.Typo == Typo.H4).ShouldBeTrue();
            texts.Any(text => text.Color == LoamColor.Primary).ShouldBeTrue();
            texts.Any(text => text.Color == LoamColor.Error).ShouldBeTrue();
            texts.Any(text => text.Align == TextAlignment.Center).ShouldBeTrue();
            texts.Any(text => text.TextWrapping == TextWrapping.Wrap && text.GutterBottom).ShouldBeTrue();
            texts.Any(text => AutomationProperties.GetName(text) == "Display roles").ShouldBeTrue();
        }
        finally
        {
            textWindow.Close();
        }

        var iconPage = ComponentsView.PageCatalog.Single(page => page.Route == "Display/Icon");
        foreach (var expected in new[] { "Colors", "Sizes", "Common glyphs", "LoamSize.ExtraSmall", "LoamSize.ExtraLarge", "Icons.Material.Filled.ShowChart" })
        {
            iconPage.Code.ShouldContain(expected);
        }

        iconPage.Code.ShouldNotContain("IconCase");

        var iconPreview = iconPage.Build();
        var iconWindow = Show(iconPreview, ThemeVariant.Light);
        try
        {
            var icons = iconPreview.GetVisualDescendants().OfType<Icon>().ToArray();
            icons.Length.ShouldBeGreaterThanOrEqualTo(17);
            icons.Any(icon => icon.Color == LoamColor.Primary).ShouldBeTrue();
            icons.Any(icon => icon.Color == LoamColor.Error).ShouldBeTrue();
            icons.Any(icon => icon.Data == Icons.Material.Filled.ShowChart).ShouldBeTrue();
            AssertAllSizes(icons, icon => icon.Size);
            icons.All(icon => AutomationProperties.GetName(icon) == "Icon").ShouldBeTrue();
        }
        finally
        {
            iconWindow.Close();
        }

        var dividerPage = ComponentsView.PageCatalog.Single(page => page.Route == "Display/Divider");
        foreach (var expected in new[] { "Horizontal", "Vertical", "DividerType.Inset", "DividerType.Middle", "Light = true", "Vertical = true" })
        {
            dividerPage.Code.ShouldContain(expected);
        }

        dividerPage.Code.ShouldNotContain("DividerCase");

        var dividerPreview = dividerPage.Build();
        var dividerWindow = Show(dividerPreview, ThemeVariant.Light);
        try
        {
            var dividers = dividerPreview.GetVisualDescendants().OfType<Divider>().ToArray();
            dividers.Length.ShouldBeGreaterThanOrEqualTo(8);
            dividers.Count(divider => divider.Vertical).ShouldBeGreaterThanOrEqualTo(4);
            dividers.Any(divider => divider.DividerType == DividerType.Inset).ShouldBeTrue();
            dividers.Any(divider => divider.DividerType == DividerType.Middle).ShouldBeTrue();
            dividers.Any(divider => divider.Light).ShouldBeTrue();
            dividers.All(divider => AutomationProperties.GetName(divider) == "Divider").ShouldBeTrue();
        }
        finally
        {
            dividerWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Layout_gallery_pages_show_component_specific_panels()
    {
        var containerPage = ComponentsView.PageCatalog.Single(page => page.Route == "Layout/Container");
        foreach (var expected in new[] { "Breakpoint caps", "MaxWidthBreakpoint = Breakpoint.Sm", "MaxWidthBreakpoint = Breakpoint.Md", "MaxWidthBreakpoint = Breakpoint.Lg", "Gutters = false" })
        {
            containerPage.Code.ShouldContain(expected);
        }

        var containerPreview = containerPage.Build();
        var containerWindow = Show(containerPreview, ThemeVariant.Light);
        try
        {
            var containers = containerPreview.GetVisualDescendants().OfType<Loam.Controls.Container>().ToArray();
            containers.Length.ShouldBeGreaterThanOrEqualTo(4);
            containers.Any(container => container.MaxWidthBreakpoint == Breakpoint.Sm).ShouldBeTrue();
            containers.Any(container => container.MaxWidthBreakpoint == Breakpoint.Md).ShouldBeTrue();
            containers.Any(container => container.MaxWidthBreakpoint == Breakpoint.Lg).ShouldBeTrue();
            containers.Any(container => !container.Gutters).ShouldBeTrue();
            containers.All(container => AutomationProperties.GetName(container) == "Container").ShouldBeTrue();
        }
        finally
        {
            containerWindow.Close();
        }

        var gridPage = ComponentsView.PageCatalog.Single(page => page.Route == "Layout/ResponsiveGrid");
        foreach (var expected in new[] { "Fixed spans", "Responsive spans", "Xs = 12", "Sm = 6", "Md = 4", "Non-Item child spans 12 columns" })
        {
            gridPage.Code.ShouldContain(expected);
        }

        var gridPreview = gridPage.Build();
        var gridWindow = Show(gridPreview, ThemeVariant.Light);
        try
        {
            var grids = gridPreview.GetVisualDescendants().OfType<Loam.Controls.ResponsiveGrid>().ToArray();
            grids.Length.ShouldBeGreaterThanOrEqualTo(2);
            grids.Any(grid => grid.Spacing == 12).ShouldBeTrue();
            gridPreview.GetVisualDescendants().OfType<Col>().Count().ShouldBeGreaterThanOrEqualTo(10);
            grids.All(grid => AutomationProperties.GetName(grid) == "Grid layout").ShouldBeTrue();
        }
        finally
        {
            gridWindow.Close();
        }

        var itemPage = ComponentsView.PageCatalog.Single(page => page.Route == "Layout/Col");
        foreach (var expected in new[] { "Item breakpoint props", "Xs = 12", "Sm = 12", "Md = 8", "Lg = 8", "Xl = 2", "Xxl = 1" })
        {
            itemPage.Code.ShouldContain(expected);
        }

        var itemPreview = itemPage.Build();
        var itemWindow = Show(itemPreview, ThemeVariant.Light);
        try
        {
            var items = itemPreview.GetVisualDescendants().OfType<Col>().ToArray();
            items.Length.ShouldBeGreaterThanOrEqualTo(5);
            items.Any(item => item.Xs == 12 && item.Md == 8).ShouldBeTrue();
            items.Any(item => item.Xxl == 1).ShouldBeTrue();
            items.All(item => AutomationProperties.GetName(item) == "Grid item").ShouldBeTrue();
        }
        finally
        {
            itemWindow.Close();
        }

        var stackPage = ComponentsView.PageCatalog.Single(page => page.Route == "Layout/Stack");
        foreach (var expected in new[] { "Vertical stack", "Row stack", "Custom spacing", "Row = true", "Spacing = 16" })
        {
            stackPage.Code.ShouldContain(expected);
        }

        var stackPreview = stackPage.Build();
        var stackWindow = Show(stackPreview, ThemeVariant.Light);
        try
        {
            var stacks = stackPreview.GetVisualDescendants().OfType<Loam.Controls.Stack>().ToArray();
            stacks.Length.ShouldBeGreaterThanOrEqualTo(3);
            stacks.Any(stack => stack.Row).ShouldBeTrue();
            stacks.Any(stack => stack.Spacing == 16).ShouldBeTrue();
            stacks.All(stack => AutomationProperties.GetName(stack) == "Stack").ShouldBeTrue();
        }
        finally
        {
            stackWindow.Close();
        }

        var spacerPage = ComponentsView.PageCatalog.Single(page => page.Route == "Layout/Spacer");
        foreach (var expected in new[] { "Star column spacer", "Dock fill spacer", "new Spacer()" })
        {
            spacerPage.Code.ShouldContain(expected);
        }

        var spacerPreview = spacerPage.Build();
        var spacerWindow = Show(spacerPreview, ThemeVariant.Light);
        try
        {
            var spacers = spacerPreview.GetVisualDescendants().OfType<Spacer>().ToArray();
            spacers.Length.ShouldBeGreaterThanOrEqualTo(2);
            spacers.All(spacer => AutomationProperties.GetName(spacer) == "Spacer").ShouldBeTrue();
        }
        finally
        {
            spacerWindow.Close();
        }

        var hiddenPage = ComponentsView.PageCatalog.Single(page => page.Route == "Layout/Hidden");
        foreach (var expected in new[] { "HiddenMode.Down", "HiddenMode.Up", "HiddenMode.Only", "Breakpoint.Sm", "Breakpoint.Md", "Breakpoint.Lg" })
        {
            hiddenPage.Code.ShouldContain(expected);
        }

        var hiddenPreview = hiddenPage.Build();
        var hiddenWindow = Show(hiddenPreview, ThemeVariant.Light);
        try
        {
            var hidden = hiddenPreview.GetVisualDescendants().OfType<Hidden>().ToArray();
            hidden.Length.ShouldBeGreaterThanOrEqualTo(3);
            hidden.Select(rule => rule.Mode).ShouldContain(HiddenMode.Down);
            hidden.Select(rule => rule.Mode).ShouldContain(HiddenMode.Up);
            hidden.Select(rule => rule.Mode).ShouldContain(HiddenMode.Only);
            hidden.All(rule => AutomationProperties.GetName(rule) == "Hidden").ShouldBeTrue();
        }
        finally
        {
            hiddenWindow.Close();
        }

        var scrollPage = ComponentsView.PageCatalog.Single(page => page.Route == "Layout/ScrollToTop");
        foreach (var expected in new[] { "new ScrollToTop", "Target = scroller", "VisibleOffset = 32", "scroller.Offset" })
        {
            scrollPage.Code.ShouldContain(expected);
        }

        var scrollPreview = scrollPage.Build();
        var scrollWindow = Show(scrollPreview, ThemeVariant.Light);
        try
        {
            var scrollToTop = scrollPreview.GetVisualDescendants().OfType<ScrollToTop>().Single();
            scrollToTop.Target.ShouldNotBeNull();
            scrollToTop.VisibleOffset.ShouldBe(32);
            AutomationProperties.GetName(scrollToTop).ShouldBe("Scroll to top");
            AutomationProperties.GetName(scrollToTop.Child.ShouldBeOfType<Fab>()).ShouldBe("Scroll to top");
        }
        finally
        {
            scrollWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Navigation_gallery_pages_show_component_specific_panels()
    {
        var breadcrumbsPage = ComponentsView.PageCatalog.Single(page => page.Route == "Navigation/Breadcrumbs");
        foreach (var expected in new[] { "Default trail", "Custom separator", "Href and disabled item", "Deep trail", "Separator = \">\"", "Disabled = true", "Href =" })
        {
            breadcrumbsPage.Code.ShouldContain(expected);
        }

        var breadcrumbsPreview = breadcrumbsPage.Build();
        var breadcrumbsWindow = Show(breadcrumbsPreview, ThemeVariant.Light);
        try
        {
            var breadcrumbs = breadcrumbsPreview.GetVisualDescendants().OfType<Breadcrumbs>().ToArray();
            breadcrumbs.Length.ShouldBeGreaterThanOrEqualTo(4);
            breadcrumbs.Any(trail => trail.Separator == ">").ShouldBeTrue();
            breadcrumbs.Any(trail => trail.Items.Any(item => item.Disabled)).ShouldBeTrue();
            breadcrumbs.Any(trail => trail.Items.Any(item => !string.IsNullOrWhiteSpace(item.Href))).ShouldBeTrue();
            breadcrumbs.All(trail => AutomationProperties.GetName(trail) == "Breadcrumbs").ShouldBeTrue();
        }
        finally
        {
            breadcrumbsWindow.Close();
        }

        var linkPage = ComponentsView.PageCatalog.Single(page => page.Route == "Navigation/Link");
        foreach (var expected in new[] { "Hover underline", "Always underline", "Success link", "External href", "Disabled link", "Href =" })
        {
            linkPage.Code.ShouldContain(expected);
        }

        var linkPreview = linkPage.Build();
        var linkWindow = Show(linkPreview, ThemeVariant.Light);
        try
        {
            var links = linkPreview.GetVisualDescendants().OfType<Link>().ToArray();
            links.Length.ShouldBeGreaterThanOrEqualTo(5);
            links.Any(link => link.Underline).ShouldBeTrue();
            links.Any(link => link.Color == LoamColor.Success).ShouldBeTrue();
            links.Any(link => !string.IsNullOrWhiteSpace(link.Href)).ShouldBeTrue();
            links.Any(link => !link.IsEnabled).ShouldBeTrue();
            links.Where(link => link.IsEnabled).All(link => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(link))).ShouldBeTrue();
        }
        finally
        {
            linkWindow.Close();
        }

        var navMenuPage = ComponentsView.PageCatalog.Single(page => page.Route == "Navigation/NavMenu");
        foreach (var expected in new[] { "Simple menu", "Grouped menu", "new NavMenu", "new NavGroup", "Spacing = 4", "IsEnabled = false" })
        {
            navMenuPage.Code.ShouldContain(expected);
        }

        var navMenuPreview = navMenuPage.Build();
        var navMenuWindow = Show(navMenuPreview, ThemeVariant.Light);
        try
        {
            var menus = navMenuPreview.GetVisualDescendants().OfType<NavMenu>().ToArray();
            menus.Length.ShouldBeGreaterThanOrEqualTo(2);
            menus.All(menu => AutomationProperties.GetName(menu) == "Navigation menu").ShouldBeTrue();
            menus.Any(menu => menu.Spacing == 4).ShouldBeTrue();
            navMenuPreview.GetVisualDescendants().OfType<NavGroup>().Any().ShouldBeTrue();
            navMenuPreview.GetVisualDescendants().OfType<NavLink>().Any(link => !link.IsEnabled).ShouldBeTrue();
        }
        finally
        {
            navMenuWindow.Close();
        }

        var navLinkPage = ComponentsView.PageCatalog.Single(page => page.Route == "Navigation/NavLink");
        foreach (var expected in new[] { "Active", "Idle", "Secondary active", "Text only", "Href target", "Disabled", "Href =" })
        {
            navLinkPage.Code.ShouldContain(expected);
        }

        var navLinkPreview = navLinkPage.Build();
        var navLinkWindow = Show(navLinkPreview, ThemeVariant.Light);
        try
        {
            var links = navLinkPreview.GetVisualDescendants().OfType<NavLink>().ToArray();
            links.Length.ShouldBeGreaterThanOrEqualTo(6);
            links.Any(link => link.IsActive && link.Color == LoamColor.Primary).ShouldBeTrue();
            links.Any(link => link.IsActive && link.Color == LoamColor.Secondary).ShouldBeTrue();
            links.Any(link => string.Equals(link.Content as string, "Text only", StringComparison.Ordinal) && string.IsNullOrEmpty(link.Icon)).ShouldBeTrue();
            links.Any(link => !string.IsNullOrWhiteSpace(link.Href)).ShouldBeTrue();
            links.Any(link => !link.IsEnabled).ShouldBeTrue();
            links.Where(link => link.IsEnabled).All(link => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(link))).ShouldBeTrue();
        }
        finally
        {
            navLinkWindow.Close();
        }

        var navGroupPage = ComponentsView.PageCatalog.Single(page => page.Route == "Navigation/NavGroup");
        foreach (var expected in new[] { "Expanded group", "Collapsed group", "Disabled group", "Expanded = true", "IsEnabled = false" })
        {
            navGroupPage.Code.ShouldContain(expected);
        }

        var navGroupPreview = navGroupPage.Build();
        var navGroupWindow = Show(navGroupPreview, ThemeVariant.Light);
        try
        {
            var groups = navGroupPreview.GetVisualDescendants().OfType<NavGroup>().ToArray();
            groups.Length.ShouldBeGreaterThanOrEqualTo(3);
            groups.Any(group => group.Expanded).ShouldBeTrue();
            groups.Any(group => !group.Expanded).ShouldBeTrue();
            groups.Any(group => !group.IsEnabled).ShouldBeTrue();
            groups.Where(group => group.IsEnabled).All(group => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(group))).ShouldBeTrue();
            navGroupPreview.GetVisualDescendants().OfType<NavLink>().Count().ShouldBeGreaterThanOrEqualTo(5);
        }
        finally
        {
            navGroupWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Stepper_pagination_gallery_pages_show_workflow_navigation_states()
    {
        var paginationPage = ComponentsView.PageCatalog.Single(page => page.Route == "Data/Pagination");
        foreach (var expected in new[] { "Boundary pages", "Windowed pages", "Secondary color", "Clamped selected page", "Empty and disabled", "BoundaryCount = 2", "MiddleCount = 5", "Selected = 99", "IsEnabled = false" })
        {
            paginationPage.Code.ShouldContain(expected);
        }

        var paginationPreview = paginationPage.Build();
        var paginationWindow = Show(paginationPreview, ThemeVariant.Light);
        try
        {
            var paginations = paginationPreview.GetVisualDescendants().OfType<Pagination>().ToArray();
            paginations.Length.ShouldBeGreaterThanOrEqualTo(6);
            paginations.Any(pagination => pagination.Count == 24 && pagination.BoundaryCount == 2 && pagination.MiddleCount == 5).ShouldBeTrue();
            paginations.Any(pagination => pagination.Color == LoamColor.Secondary).ShouldBeTrue();
            paginations.Any(pagination => pagination.Count == 7 && pagination.Selected == 7).ShouldBeTrue();
            paginations.Any(pagination => pagination.Count == 0 && pagination.Selected == 0).ShouldBeTrue();
            paginations.Any(pagination => !pagination.IsEnabled).ShouldBeTrue();
            paginations.All(pagination => AutomationProperties.GetName(pagination) == "Pagination").ShouldBeTrue();
        }
        finally
        {
            paginationWindow.Close();
        }

        var stepperPage = ComponentsView.PageCatalog.Single(page => page.Route == "Data/Stepper");
        foreach (var expected in new[] { "Active step", "Completed steps", "Clamped ActiveIndex", "Disabled", "Empty", "Completed = true", "ActiveIndex = 99", "IsEnabled = false" })
        {
            stepperPage.Code.ShouldContain(expected);
        }

        var stepperPreview = stepperPage.Build();
        var stepperWindow = Show(stepperPreview, ThemeVariant.Light);
        try
        {
            var steppers = stepperPreview.GetVisualDescendants().OfType<Stepper>().ToArray();
            steppers.Length.ShouldBeGreaterThanOrEqualTo(5);
            steppers.Any(stepper => stepper.ActiveIndex == 1 && stepper.Steps.Count == 3).ShouldBeTrue();
            steppers.Any(stepper => stepper.ActiveIndex == 2 && stepper.Steps.Count(step => step.Completed) >= 2).ShouldBeTrue();
            steppers.Any(stepper => stepper.Steps.Count == 2 && stepper.ActiveIndex == 1).ShouldBeTrue();
            steppers.Any(stepper => !stepper.IsEnabled).ShouldBeTrue();
            steppers.Any(stepper => stepper.Steps.Count == 0).ShouldBeTrue();
            steppers.All(stepper => AutomationProperties.GetName(stepper) == "Stepper").ShouldBeTrue();
        }
        finally
        {
            stepperWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Tabs_expansion_collapse_gallery_pages_show_reveal_states()
    {
        var tabsPage = ComponentsView.PageCatalog.Single(page => page.Route == "Data/Tabs");
        foreach (var expected in new[] { "Default tabs", "Secondary selected", "Clamped SelectedIndex", "Disabled", "Empty", "SelectedIndex = 99", "IsEnabled = false", "LoamColor.Secondary", "LoamColor.Tertiary" })
        {
            tabsPage.Code.ShouldContain(expected);
        }

        var tabsPreview = tabsPage.Build();
        var tabsWindow = Show(tabsPreview, ThemeVariant.Light);
        try
        {
            var tabs = tabsPreview.GetVisualDescendants().OfType<Tabs>().ToArray();
            tabs.Length.ShouldBeGreaterThanOrEqualTo(5);
            tabs.Any(tab => tab.Color == LoamColor.Secondary && tab.SelectedIndex == 1).ShouldBeTrue();
            tabs.Any(tab => tab.Color == LoamColor.Tertiary && tab.Items.Count == 2 && tab.SelectedIndex == 1).ShouldBeTrue();
            tabs.Any(tab => !tab.IsEnabled).ShouldBeTrue();
            tabs.Any(tab => tab.Items.Count == 0).ShouldBeTrue();
            tabs.All(tab => AutomationProperties.GetName(tab) == "Tabs").ShouldBeTrue();
        }
        finally
        {
            tabsWindow.Close();
        }

        var expansionPage = ComponentsView.PageCatalog.Single(page => page.Route == "Data/ExpansionPanels");
        foreach (var expected in new[] { "Accordion", "MultiExpansion", "Disabled panel", "MultiExpansion = true", "AddPanel", "ExpandAll", "isExpanded: true", "isEnabled: false" })
        {
            expansionPage.Code.ShouldContain(expected);
        }

        var expansionPreview = expansionPage.Build();
        var expansionWindow = Show(expansionPreview, ThemeVariant.Light);
        try
        {
            var groups = expansionPreview.GetVisualDescendants().OfType<ExpansionPanels>().ToArray();
            groups.Length.ShouldBeGreaterThanOrEqualTo(3);
            groups.Any(group => group.MultiExpansion && group.Panels.Count(panel => panel.IsExpanded) >= 2).ShouldBeTrue();
            groups.Any(group => group.Panels.Any(panel => !panel.IsEnabled)).ShouldBeTrue();
            groups.All(group => AutomationProperties.GetName(group) == "Expansion panels").ShouldBeTrue();
        }
        finally
        {
            expansionWindow.Close();
        }

        var collapsePage = ComponentsView.PageCatalog.Single(page => page.Route == "Data/Collapse");
        foreach (var expected in new[] { "Animated reveal", "Static reveal", "Custom duration", "Disabled static", "Zero duration", "Show animated details", "Show static details", "Show custom-duration details", "Show instant details", "Animated = false", "Duration = TimeSpan.FromMilliseconds(320)", "Duration = TimeSpan.Zero", "IsEnabled = false", "Expanded = true" })
        {
            collapsePage.Code.ShouldContain(expected);
        }

        var collapsePreview = collapsePage.Build();
        var collapseWindow = Show(collapsePreview, ThemeVariant.Light);
        try
        {
            var collapses = collapsePreview.GetVisualDescendants().OfType<Collapse>().ToArray();
            collapses.Length.ShouldBeGreaterThanOrEqualTo(5);
            collapses.Any(collapse => !collapse.Animated).ShouldBeTrue();
            collapses.Any(collapse => collapse.Duration == TimeSpan.FromMilliseconds(320)).ShouldBeTrue();
            collapses.Any(collapse => collapse.Duration == TimeSpan.Zero).ShouldBeTrue();
            collapses.Any(collapse => !collapse.IsEnabled).ShouldBeTrue();
            collapses.Any(collapse => collapse.Expanded).ShouldBeTrue();
            collapses.Any(collapse => !collapse.Expanded).ShouldBeTrue();
            collapses.All(collapse => AutomationProperties.GetName(collapse) == "Collapse").ShouldBeTrue();
            collapses.Any(collapse => AutomationProperties.GetHelpText(collapse) == "Expanded, static").ShouldBeTrue();

            foreach (var label in new[] { "Show animated details", "Show static details", "Show custom-duration details", "Show instant details" })
            {
                var button = collapsePreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                    .Single(control => string.Equals(control.Content as string, label, StringComparison.Ordinal));
                button.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                Dispatcher.UIThread.RunJobs();
            }

            collapses.Count(collapse => collapse.Expanded).ShouldBeGreaterThanOrEqualTo(5);
        }
        finally
        {
            collapseWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Timeline_carousel_gallery_pages_show_data_motion_states()
    {
        var timelinePage = ComponentsView.PageCatalog.Single(page => page.Route == "Data/Timeline");
        foreach (var expected in new[] { "Default sequence", "Rich content", "Horizontal", "Empty", "Disabled", "Orientation = Orientation.Horizontal", "LoamColor.Success", "IsEnabled = false", "\"Order placed\", \"Customer submitted checkout.\"", "\"9:24 AM\"" })
        {
            timelinePage.Code.ShouldContain(expected);
        }

        var timelinePreview = timelinePage.Build();
        var timelineWindow = Show(timelinePreview, ThemeVariant.Light);
        try
        {
            var timelines = timelinePreview.GetVisualDescendants().OfType<Timeline>().ToArray();
            timelines.Length.ShouldBeGreaterThanOrEqualTo(4);
            timelines.Any(timeline => timeline.Items.Count >= 4).ShouldBeTrue();
            timelines.Any(timeline => timeline.Orientation == Orientation.Horizontal).ShouldBeTrue();
            timelines.Any(timeline => timeline.Items.Count == 0 && AutomationProperties.GetHelpText(timeline) == "No items").ShouldBeTrue();
            timelines.Any(timeline => !timeline.IsEnabled).ShouldBeTrue();
            timelines.All(timeline => AutomationProperties.GetName(timeline) == "Timeline").ShouldBeTrue();
            timelinePreview.GetVisualDescendants().OfType<Text>()
                .Any(text => text.Text == "No timeline items")
                .ShouldBeTrue();
            timelinePreview.GetVisualDescendants().OfType<Paper>()
                .Any(paper => AutomationProperties.GetName(paper)?.StartsWith("Timeline item ", StringComparison.Ordinal) == true)
                .ShouldBeTrue();
        }
        finally
        {
            timelineWindow.Close();
        }

        var carouselPage = ComponentsView.PageCatalog.Single(page => page.Route == "Data/Carousel");
        foreach (var expected in new[] { "Default carousel", "Chrome hidden", "Auto play", "GoTo clamped", "Empty", "Disabled", "ShowArrows = false", "ShowBullets = false", "AutoPlay = true", "AutoPlayInterval", "GoTo(99)", "new CarouselItem(\"Intake\"", "IsEnabled = false" })
        {
            carouselPage.Code.ShouldContain(expected);
        }

        var carouselPreview = carouselPage.Build();
        var carouselWindow = Show(carouselPreview, ThemeVariant.Light);
        try
        {
            var carousels = carouselPreview.GetVisualDescendants().OfType<Loam.Controls.Carousel>().ToArray();
            carousels.Length.ShouldBeGreaterThanOrEqualTo(5);
            carousels.Any(carousel => !carousel.ShowArrows && !carousel.ShowBullets).ShouldBeTrue();
            carousels.Any(carousel => carousel.AutoPlay && carousel.AutoPlayInterval == TimeSpan.FromSeconds(2)).ShouldBeTrue();
            carousels.Any(carousel => carousel.Items.Count == 2 && carousel.SelectedIndex == 1).ShouldBeTrue();
            carousels.Any(carousel => carousel.Items.Count == 0 && AutomationProperties.GetHelpText(carousel) == "No slides").ShouldBeTrue();
            carousels.Any(carousel => !carousel.IsEnabled).ShouldBeTrue();
            carousels.All(carousel => AutomationProperties.GetName(carousel) == "Carousel").ShouldBeTrue();
        }
        finally
        {
            carouselWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Overlay_gallery_pages_use_component_options()
    {
        var overlayPage = ComponentsView.PageCatalog.Single(page => page.Route == "Feedback/Overlay");
        foreach (var expected in new[] { "Show light overlay", "Show dark overlay", "Show manual overlay", "Close manually", "AutoClose = true", "AutoClose = false", "DarkBackground = true", "IsEnabled = false" })
        {
            overlayPage.Code.ShouldContain(expected);
        }

        var overlayPreview = overlayPage.Build();
        var overlayWindow = Show(overlayPreview, ThemeVariant.Light);
        try
        {
            var overlays = overlayPreview.GetVisualDescendants().OfType<Overlay>().ToArray();
            overlays.Length.ShouldBeGreaterThanOrEqualTo(4);
            overlays.Any(overlay => overlay.Visible && !overlay.IsEnabled).ShouldBeTrue();
            overlays.Any(overlay => !overlay.AutoClose).ShouldBeTrue();
            overlays.Any(overlay => overlay.DarkBackground).ShouldBeTrue();
            overlays.All(overlay => AutomationProperties.GetName(overlay) == "Overlay").ShouldBeTrue();

            foreach (var label in new[] { "Show light overlay", "Show dark overlay", "Show manual overlay" })
            {
                var trigger = overlayPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                    .Single(button => string.Equals(button.Content as string, label, StringComparison.Ordinal));
                trigger.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                Dispatcher.UIThread.RunJobs();
            }

            overlays.Count(overlay => overlay.Visible).ShouldBeGreaterThanOrEqualTo(4);
            overlays.Any(overlay => overlay.Visible && overlay.AutoClose && !overlay.DarkBackground).ShouldBeTrue();
            overlays.Any(overlay => overlay.Visible && overlay.AutoClose && overlay.DarkBackground).ShouldBeTrue();
            overlays.Any(overlay => overlay.Visible && !overlay.AutoClose).ShouldBeTrue();

            var manualClose = overlayPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Close manually", StringComparison.Ordinal));
            manualClose.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            overlays.Any(overlay => !overlay.AutoClose && !overlay.Visible).ShouldBeTrue();
        }
        finally
        {
            overlayWindow.Close();
        }

        var menuPage = ComponentsView.PageCatalog.Single(page => page.Route == "Buttons/Menu");
        foreach (var expected in new[] { "MenuWidth = 220", "ShortcutText", "IsDivider = true", "IsEnabled = false", "CloseOnItemClick = false" })
        {
            menuPage.Code.ShouldContain(expected);
        }

        var menuPreview = menuPage.Build();
        var menuWindow = Show(menuPreview, ThemeVariant.Light);
        try
        {
            var menus = menuPreview.GetVisualDescendants().OfType<Loam.Controls.Menu>().ToArray();
            menus.Length.ShouldBeGreaterThanOrEqualTo(4);
            menus.Any(menu => menu.MenuWidth == 220).ShouldBeTrue();
            menus.Any(menu => !menu.CloseOnItemClick).ShouldBeTrue();
            menus.Any(menu => !menu.IsEnabled).ShouldBeTrue();
            menus.SelectMany(menu => menu.Items).Any(item => item.IsDivider).ShouldBeTrue();
            menus.SelectMany(menu => menu.Items).Any(item => !item.IsEnabled).ShouldBeTrue();
        }
        finally
        {
            menuWindow.Close();
        }

        var popoverPage = ComponentsView.PageCatalog.Single(page => page.Route == "Feedback/Popover");
        foreach (var expected in new[]
        {
            "Trigger = detailsTrigger",
            "Trigger = actionTrigger",
            "Trigger = controlledTrigger",
            "Placement = Avalonia.Controls.PlacementMode.BottomEdgeAlignedLeft",
            "Placement = Avalonia.Controls.PlacementMode.RightEdgeAlignedTop",
            "Open = true",
            "IsEnabled = false",
            "Close controlled",
        })
        {
            popoverPage.Code.ShouldContain(expected);
        }

        var popoverPreview = popoverPage.Build();
        var popoverWindow = Show(popoverPreview, ThemeVariant.Light);
        try
        {
            var popovers = popoverPreview.GetVisualDescendants().OfType<Popover>().ToArray();
            popovers.Length.ShouldBe(4);
            popovers.Count(popover => popover.Open).ShouldBe(2);
            popovers.Count(popover => popover.Trigger is not null).ShouldBe(4);
            popovers.Count(popover => popover.Target is not null).ShouldBe(0);
            popovers.Any(popover => !popover.Open && popover.Trigger is Loam.Controls.Button { IsEnabled: false }).ShouldBeTrue();

            var detailsTrigger = popoverPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Open details", StringComparison.Ordinal));
            detailsTrigger.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            popovers.Single(popover => popover.Trigger == detailsTrigger).Open.ShouldBeTrue();

            var close = popoverWindow.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Close", StringComparison.Ordinal));
            close.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            popovers.Single(popover => popover.Trigger is Loam.Controls.Button button &&
                string.Equals(button.Content as string, "Open actions", StringComparison.Ordinal)).Open.ShouldBeFalse();

            var controlledClose = popoverWindow.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Close controlled", StringComparison.Ordinal));
            controlledClose.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            popovers.Single(popover => popover.Trigger is Loam.Controls.Button button &&
                string.Equals(button.Content as string, "Controlled open", StringComparison.Ordinal)).Open.ShouldBeFalse();
        }
        finally
        {
            popoverWindow.Close();
        }

        var tooltipPage = ComponentsView.PageCatalog.Single(page => page.Route == "Feedback/Tooltip");
        foreach (var expected in new[]
        {
            "Tooltip.Set",
            "new TooltipOptions",
            "Placement = PlacementMode.Bottom",
            "ShowOnDisabled = true",
            "ServiceEnabled = false",
            "Tooltip.Clear",
        })
        {
            tooltipPage.Code.ShouldContain(expected);
        }

        var tooltipPreview = tooltipPage.Build();
        var tooltipWindow = Show(tooltipPreview, ThemeVariant.Light);
        try
        {
            var standard = tooltipPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Standard", StringComparison.Ordinal));
            ToolTip.GetTip(standard).ShouldBeOfType<Paper>();
            AutomationProperties.GetHelpText(standard).ShouldBe("Quick context tooltip");

            var rich = tooltipPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Rich tooltip", StringComparison.Ordinal));
            var richTip = ToolTip.GetTip(rich).ShouldBeOfType<Paper>();
            richTip.Elevation.ShouldBe(5);
            ToolTip.GetPlacement(rich).ShouldBe(PlacementMode.BottomEdgeAlignedLeft);
            ToolTip.GetVerticalOffset(rich).ShouldBe(8);

            var delayed = tooltipPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Delayed bottom", StringComparison.Ordinal));
            ToolTip.GetPlacement(delayed).ShouldBe(PlacementMode.Bottom);
            ToolTip.GetShowDelay(delayed).ShouldBe(250);
            ToolTip.GetBetweenShowDelay(delayed).ShouldBe(100);

            var disabled = tooltipPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Disabled target", StringComparison.Ordinal));
            disabled.IsEnabled.ShouldBeFalse();
            ToolTip.GetShowOnDisabled(disabled).ShouldBeTrue();

            var serviceDisabled = tooltipPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Service disabled", StringComparison.Ordinal));
            ToolTip.GetServiceEnabled(serviceDisabled).ShouldBeFalse();

            var cleared = tooltipPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Cleared", StringComparison.Ordinal));
            ToolTip.GetTip(cleared).ShouldBeNull();
            AutomationProperties.GetHelpText(cleared).ShouldBeNull();
        }
        finally
        {
            tooltipWindow.Close();
        }

        var dialogPage = ComponentsView.PageCatalog.Single(page => page.Route == "Feedback/DialogService");
        foreach (var expected in new[]
        {
            "ConfirmAsync",
            "MessageBoxAsync",
            "new DialogOptions",
            "Width = 360",
            "MaxWidth = 420",
            "MinWidth = 320",
            "DismissOnEscape = true",
            "DismissOnScrimClick = false",
        })
        {
            dialogPage.Code.ShouldContain(expected);
        }

        var dialogPreview = dialogPage.Build();
        var dialogWindow = Show(dialogPreview, ThemeVariant.Light);
        try
        {
            var custom = dialogPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Custom dialog", StringComparison.Ordinal));
            custom.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            var paper = dialogWindow.GetVisualDescendants().OfType<Paper>().Single();
            paper.Width.ShouldBe(360);
            paper.MaxWidth.ShouldBe(420);
            paper.MinWidth.ShouldBe(320);

            var cancel = dialogWindow.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Cancel", StringComparison.Ordinal));
            cancel.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
        }
        finally
        {
            dialogWindow.Close();
        }

        var snackbarPage = ComponentsView.PageCatalog.Single(page => page.Route == "Feedback/SnackbarService");
        foreach (var expected in new[]
        {
            "SnackbarPosition.BottomCenter",
            "SnackbarPosition.TopCenter",
            "SnackbarPosition.BottomLeft",
            "DismissText = \"Dismiss\"",
            "DismissText = \"Close\"",
            "ActionText = \"Undo\"",
            "Timeout.InfiniteTimeSpan",
            "MaxVisible = 2",
        })
        {
            snackbarPage.Code.ShouldContain(expected);
        }

        var snackbarPreview = snackbarPage.Build();
        var snackbarWindow = Show(snackbarPreview, ThemeVariant.Light);
        try
        {
            var action = snackbarPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Action snackbar", StringComparison.Ordinal));
            action.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            var host = snackbarWindow.GetVisualDescendants().OfType<StackPanel>()
                .Single(panel => panel.Name == "PART_LoamSnackbarHost");
            host.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
            host.VerticalAlignment.ShouldBe(VerticalAlignment.Bottom);
            snackbarWindow.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Any(button => string.Equals(button.Content as string, "Dismiss", StringComparison.Ordinal))
                .ShouldBeTrue();

            var queue = snackbarPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Queue limit", StringComparison.Ordinal));
            queue.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            host.HorizontalAlignment.ShouldBe(HorizontalAlignment.Left);
            host.VerticalAlignment.ShouldBe(VerticalAlignment.Bottom);
            var queueMessages = snackbarWindow.GetVisualDescendants().OfType<Text>()
                .Select(text => text.Text)
                .Where(text => text?.StartsWith("Queue item ", StringComparison.Ordinal) == true)
                .ToArray();
            queueMessages.Length.ShouldBe(2);
            queueMessages.ShouldContain("Queue item 3");
            queueMessages.ShouldContain("Queue item 4");
        }
        finally
        {
            snackbarWindow.Close();
        }
    }

    [AvaloniaFact]
    public void Paper_gallery_square_sample_is_visible_and_square()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Surfaces/Paper");
        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var square = preview.GetVisualDescendants().OfType<Paper>()
                .Single(paper => string.Equals(paper.Title, "Square", StringComparison.Ordinal));
            square.Square.ShouldBeTrue();
            square.Elevation.ShouldBe(4);
            square.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var root = square.GetVisualDescendants().OfType<Border>().First();
            root.CornerRadius.ShouldBe(new CornerRadius(0));
            ((ISolidColorBrush)root.Background!).Color.ShouldBe(Color.Parse("#E6E0E9"));
            root.BoxShadow.Count.ShouldBe(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Card_gallery_sample_uses_compact_real_card_anatomy()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Surfaces/Card");
        page.Code.ShouldContain("HeaderAvatar = new Avatar");
        page.Code.ShouldContain("ShowMedia = true");
        page.Code.ShouldContain("MediaHeight = 96");
        page.Code.ShouldContain("BodyText =");
        page.Code.ShouldContain("SecondaryActionText = \"Details\"");
        page.Code.ShouldContain("PrimaryActionText = \"Open\"");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var card = preview is Card rootCard
                ? rootCard
                : preview.GetVisualDescendants().OfType<Card>().Single();
            card.Width.ShouldBe(360);
            card.Elevation.ShouldBe(1);
            card.Outlined.ShouldBeTrue();

            var media = card.GetVisualDescendants().OfType<CardMedia>().Single();
            media.MediaHeight.ShouldBe(96);

            card.GetVisualDescendants().OfType<Text>()
                .Any(text => string.Equals(text.Text, "Release board", StringComparison.Ordinal))
                .ShouldBeTrue();
            card.GetVisualDescendants().OfType<Text>()
                .Any(text => string.Equals(text.Text, "Inputs, pickers, and surfaces are ready for review.", StringComparison.Ordinal))
                .ShouldBeTrue();
            card.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Details", StringComparison.Ordinal))
                .Variant.ShouldBe(Variant.Text);
            card.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Open", StringComparison.Ordinal))
                .Variant.ShouldBe(Variant.Filled);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Shell_gallery_pages_use_generated_component_apis()
    {
        var layoutPage = ComponentsView.PageCatalog.Single(page => page.Route == "Shell/Layout");
        foreach (var expected in new[] { "new Layout", "new AppBar", "new Drawer", "NavigationAction = () => drawer.Toggle()", "Subtitle =" })
        {
            layoutPage.Code.ShouldContain(expected);
        }

        var layoutPreview = layoutPage.Build();
        var layoutWindow = Show(layoutPreview, ThemeVariant.Light);
        try
        {
            layoutPreview.GetVisualDescendants().OfType<Layout>().ShouldHaveSingleItem();
            var appBar = layoutPreview.GetVisualDescendants().OfType<AppBar>().Single();
            appBar.Title.ShouldBe("Layout shell");
            appBar.Subtitle.ShouldNotBeNullOrWhiteSpace();
            appBar.NavigationIcon.ShouldBe(Icons.Material.Filled.Menu);
            layoutPreview.GetVisualDescendants().OfType<Drawer>().ShouldHaveSingleItem();
            AutomationProperties.GetName(layoutPreview.GetVisualDescendants().OfType<Layout>().Single()).ShouldBe("Application layout");
        }
        finally
        {
            layoutWindow.Close();
        }

        var appBarPage = ComponentsView.PageCatalog.Single(page => page.Route == "Shell/AppBar");
        foreach (var expected in new[] { "Subtitle = \"Configured from properties\"", "new AppBarAction", "IsEnabled = false", "Size = LoamSize.Small", "Dense = true" })
        {
            appBarPage.Code.ShouldContain(expected);
        }

        var appBarPreview = appBarPage.Build();
        var appBarWindow = Show(appBarPreview, ThemeVariant.Light);
        try
        {
            var bars = appBarPreview.GetVisualDescendants().OfType<AppBar>().ToArray();
            bars.Length.ShouldBeGreaterThanOrEqualTo(2);
            bars.Any(bar => bar.Subtitle == "Configured from properties").ShouldBeTrue();
            bars.Any(bar => bar.Dense && bar.Elevation == 0).ShouldBeTrue();
            appBarPreview.GetVisualDescendants().OfType<IconButton>()
                .Any(button => AutomationProperties.GetName(button) == "Delete disabled" && !button.IsEnabled)
                .ShouldBeTrue();
        }
        finally
        {
            appBarWindow.Close();
        }

        var drawerPage = ComponentsView.PageCatalog.Single(page => page.Route == "Shell/Drawer");
        foreach (var expected in new[] { "new DrawerItem", "Label = \"Disabled item\"", "IsEnabled = false", "drawer.Title = title", "drawer.FooterText = \"Generated Drawer.Items\"", "Toggle temporary" })
        {
            drawerPage.Code.ShouldContain(expected);
        }

        var drawerPreview = drawerPage.Build();
        var drawerWindow = Show(drawerPreview, ThemeVariant.Light);
        try
        {
            var drawers = drawerPreview.GetVisualDescendants().OfType<Drawer>().ToArray();
            drawers.Length.ShouldBeGreaterThanOrEqualTo(3);
            drawers.Any(drawer => drawer.Mini).ShouldBeTrue();
            var temporary = drawers.Single(drawer => drawer.Mode == DrawerMode.Temporary);
            temporary.Open.ShouldBeTrue();
            drawerPreview.GetVisualDescendants().OfType<NavLink>()
                .Any(link => AutomationProperties.GetName(link) == "Disabled item" && !link.IsEnabled)
                .ShouldBeTrue();

            var toggle = drawerPreview.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Toggle temporary", StringComparison.Ordinal));
            toggle.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            temporary.Open.ShouldBeFalse();

            toggle.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            temporary.Open.ShouldBeTrue();
        }
        finally
        {
            drawerWindow.Close();
        }
    }

    [AvaloniaFact]
    public void MainContent_gallery_sample_shows_shell_context_and_wrapped_content()
    {
        var page = ComponentsView.PageCatalog.Single(page => page.Route == "Shell/MainContent");
        page.Code.ShouldContain("new Layout");
        page.Code.ShouldContain("new MainContent");
        page.Code.ShouldContain("Title = \"Main content region\"");
        page.Code.ShouldContain("Subtitle = \"MainContent owns the scrollable work area");
        page.Code.ShouldContain("PrimaryActionText = \"Review\"");

        var preview = page.Build();
        var window = Show(preview, ThemeVariant.Light);
        try
        {
            var frame = preview.ShouldBeOfType<Border>();
            frame.Width.ShouldBe(640);
            frame.Height.ShouldBe(460);
            preview.GetVisualDescendants().OfType<Layout>().ShouldHaveSingleItem();
            preview.GetVisualDescendants().OfType<AppBar>().ShouldHaveSingleItem();
            preview.GetVisualDescendants().OfType<Drawer>().ShouldHaveSingleItem();

            var main = preview.GetVisualDescendants().OfType<MainContent>().Single();
            main.Padding.ShouldBe(new Thickness(20));

            var body = main.GetVisualDescendants().OfType<Text>()
                .Single(text => string.Equals(text.Text, "MainContent owns the scrollable work area below the app bar and beside the drawer, keeping page padding and body content independent from shell chrome.", StringComparison.Ordinal));
            body.TextWrapping.ShouldBe(TextWrapping.Wrap);
            body.MaxWidth.ShouldBe(560);

            main.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Export", StringComparison.Ordinal))
                .Variant.ShouldBe(Variant.Text);
            main.GetVisualDescendants().OfType<Loam.Controls.Button>()
                .Single(button => string.Equals(button.Content as string, "Review", StringComparison.Ordinal))
                .Variant.ShouldBe(Variant.Filled);

            main.GetVisualDescendants().OfType<ListItem>()
                .Any(item => string.Equals(item.Content as string, "Acceptance notes", StringComparison.Ordinal))
                .ShouldBeTrue();
            main.GetVisualDescendants().OfType<ListItem>()
                .Any(item => string.Equals(item.Content as string, "Release checklist", StringComparison.Ordinal))
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

    private static void AssertSourceCoverage(ComponentsView.GalleryPage page, string[] expectedLabels, string forbiddenHelper)
    {
        foreach (var expected in expectedLabels)
        {
            page.Code.ShouldContain(expected);
        }

        foreach (var size in ExpectedSizes)
        {
            if (page.Route is "Inputs/CheckBox" or "Inputs/Switch" or "Inputs/Radio" or "Inputs/Rating" or "Inputs/ToggleGroup")
            {
                page.Code.ShouldContain($"LoamSize.{size}");
            }
        }

        page.Code.ShouldNotContain(forbiddenHelper);
    }

    private static void AssertAllSizes<T>(IEnumerable<T> controls, Func<T, LoamSize> readSize)
    {
        var renderedSizes = controls.Select(readSize).ToArray();
        foreach (var size in ExpectedSizes)
        {
            renderedSizes.ShouldContain(size);
        }
    }

    private static string FriendlyTypeName(Type type)
    {
        var name = type.Name;
        var tick = name.IndexOf('`', StringComparison.Ordinal);
        return tick >= 0 ? name[..tick] : name;
    }
}
