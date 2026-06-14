using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class PrimitivesTests
{
    private static Border Root(Control control)
    {
        control.ApplyTemplate();
        return control.GetVisualDescendants().OfType<Border>().First();
    }

    private static Window Show(Control content)
    {
        var window = new Window { Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    [AvaloniaFact]
    public void Paper_background_resolves_to_surface_token()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var paper = new Paper { Content = new TextBlock { Text = "x" } };
        Show(paper);

        ((ISolidColorBrush)Root(paper).Background!).Color.ShouldBe(Color.Parse("#F7F2FA"));
    }

    [AvaloniaFact]
    public void Paper_outlined_draws_border_and_drops_shadow()
    {
        var paper = new Paper { Outlined = true, Elevation = 6 };
        Show(paper);
        var border = Root(paper);

        border.BorderThickness.ShouldBe(new Thickness(1));
        border.BoxShadow.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public void Paper_elevation_uses_tonal_surface_without_cast_shadow()
    {
        var paper = new Paper { Elevation = 8 };
        Show(paper);
        var border = Root(paper);

        border.BoxShadow.Count.ShouldBe(0);
        ((ISolidColorBrush)border.Background!).Color.ShouldBe(Color.Parse("#E6E0E9"));
    }

    [AvaloniaFact]
    public void Paper_generated_anatomy_uses_shape_color_compact_and_automation()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var paper = new Paper
        {
            Title = "Surface title",
            Subtitle = "Supporting text",
            Body = "Body copy",
            Compact = true,
            Shape = SurfaceShape.Large,
            Color = LoamColor.Primary,
        };
        Show(paper);

        var root = Root(paper);
        root.CornerRadius.ShouldBe(LoamShape.Default.Large);
        ((ISolidColorBrush)root.Background!).Color.ShouldNotBe(Color.Parse("#F7F2FA"));

        paper.GetVisualDescendants().OfType<Text>()
            .Any(text => string.Equals(text.Text, "Surface title", StringComparison.Ordinal))
            .ShouldBeTrue();
        paper.GetVisualDescendants().OfType<Text>()
            .Any(text => string.Equals(text.Text, "Supporting text", StringComparison.Ordinal))
            .ShouldBeTrue();
        paper.GetVisualDescendants().OfType<ContentControl>()
            .Any(content => string.Equals(content.Content as string, "Body copy", StringComparison.Ordinal))
            .ShouldBeTrue();

        AutomationProperties.GetName(paper).ShouldBe("Surface title");
        AutomationProperties.GetHelpText(paper).ShouldBe("Compact surface");
    }

    [AvaloniaFact]
    public void Paper_custom_content_wins_when_set_after_generated_props()
    {
        var paper = new Paper { Title = "Generated" };
        Show(paper);
        paper.GetVisualDescendants().OfType<Text>().Any(t => t.Text == "Generated").ShouldBeTrue();

        var custom = new TextBlock { Text = "custom" };
        paper.Content = custom;
        Dispatcher.UIThread.RunJobs();

        // Custom Content takes precedence; the generated anatomy is dropped.
        paper.Content.ShouldBeSameAs(custom);
        paper.GetVisualDescendants().OfType<Text>().Any(t => t.Text == "Generated").ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Paper_generated_props_are_ignored_when_custom_content_is_present()
    {
        var custom = new TextBlock { Text = "custom" };
        var paper = new Paper { Content = custom };
        Show(paper);

        paper.Title = "Generated";
        Dispatcher.UIThread.RunJobs();

        // Custom Content still wins; setting generated props does not replace it.
        paper.Content.ShouldBeSameAs(custom);
        paper.GetVisualDescendants().OfType<Text>().Any(t => t.Text == "Generated").ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Card_generated_anatomy_exposes_body_text_and_action_events()
    {
        var primaryClicked = false;
        var secondaryClicked = false;
        var card = new Card
        {
            Title = "Release board",
            Subtitle = "Updated today",
            HeaderAvatar = new Avatar { Content = "PL" },
            HeaderAction = new IconButton { Icon = Icons.Material.Filled.Settings },
            ShowMedia = true,
            MediaHeight = 96,
            BodyText = "Inputs, pickers, and surfaces are ready for review.",
            SecondaryActionText = "Details",
            PrimaryActionText = "Open",
        };
        card.PrimaryActionClick += (_, _) => primaryClicked = true;
        card.SecondaryActionClick += (_, _) => secondaryClicked = true;

        Show(card);
        card.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        card.GetVisualDescendants().OfType<CardHeader>().ShouldHaveSingleItem();
        card.GetVisualDescendants().OfType<CardMedia>().Single().MediaHeight.ShouldBe(96);
        card.GetVisualDescendants().OfType<Text>()
            .Any(text => string.Equals(text.Text, card.BodyText, StringComparison.Ordinal))
            .ShouldBeTrue();

        var generatedButtons = card.GetVisualDescendants().OfType<Loam.Controls.Button>().ToArray();
        var details = generatedButtons.Single(button => string.Equals(button.Content as string, "Details", StringComparison.Ordinal));
        var open = generatedButtons.Single(button => string.Equals(button.Content as string, "Open", StringComparison.Ordinal));

        details.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        open.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        secondaryClicked.ShouldBeTrue();
        primaryClicked.ShouldBeTrue();
        AutomationProperties.GetName(details).ShouldBe("Details");
        AutomationProperties.GetName(open).ShouldBe("Open");
    }

    [AvaloniaFact]
    public void Text_typo_drives_font_size_and_weight()
    {
        var text = new Text { Text = "hi", Typo = Typo.H6 };
        Show(text);

        text.FontSize.ShouldBe(24d);
        text.FontWeight.ShouldBe(FontWeight.Normal);
    }

    [AvaloniaFact]
    public void Text_color_drives_foreground()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var text = new Text { Text = "hi", Color = LoamColor.Primary };
        Show(text);

        ((ISolidColorBrush)text.Foreground!).Color.ShouldBe(Color.Parse("#6750A4"));
    }

    [Fact]
    public void Text_align_maps_to_text_alignment()
    {
        var text = new Text { Align = TextAlignment.Center };

        text.TextAlignment.ShouldBe(TextAlignment.Center);
    }

    [AvaloniaFact]
    public void Text_exposes_automation_name_and_inherit_clears_foreground()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var text = new Text { Text = "Status", Color = LoamColor.Primary };
        Show(text);

        AutomationProperties.GetName(text).ShouldBe("Status");
        ((ISolidColorBrush)text.Foreground!).Color.ShouldBe(Color.Parse("#6750A4"));

        text.Color = LoamColor.Inherit;
        Dispatcher.UIThread.RunJobs();

        ((ISolidColorBrush)text.Foreground!).Color.ShouldNotBe(Color.Parse("#6750A4"));
    }

    [AvaloniaFact]
    public void Divider_orientation_sets_thickness()
    {
        new Divider().Height.ShouldBe(1d);
        new Divider { Vertical = true }.Width.ShouldBe(1d);
    }

    [Fact]
    public void Divider_type_sets_insets_and_automation_name()
    {
        var horizontal = new Divider { DividerType = DividerType.Middle };
        horizontal.Margin.ShouldBe(new Thickness(16, 0, 16, 0));
        AutomationProperties.GetName(horizontal).ShouldBe("Divider");

        var vertical = new Divider { Vertical = true, DividerType = DividerType.Inset };
        vertical.Margin.ShouldBe(new Thickness(0, 16, 0, 0));
        vertical.Width.ShouldBe(1d);
    }

    [AvaloniaFact]
    public void Button_filled_primary_uses_primary_fill_and_contrast_text()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var button = new Loam.Controls.Button { Content = "Go", Variant = Variant.Filled, Color = LoamColor.Primary };
        Show(button);

        ((ISolidColorBrush)Root(button).Background!).Color.ShouldBe(Color.Parse("#6750A4"));
        ((ISolidColorBrush)button.Foreground!).Color.ShouldBe(Colors.White);
    }

    [AvaloniaFact]
    public void Button_text_variant_has_transparent_background()
    {
        var button = new Loam.Controls.Button { Content = "Go", Variant = Variant.Text, Color = LoamColor.Primary };
        Show(button);

        ((ISolidColorBrush)Root(button).Background!).Color.A.ShouldBe((byte)0);
    }

    [AvaloniaFact]
    public void Fab_applies_five_size_styles()
    {
        var extraSmall = new Fab { Label = "ExtraSmall", Size = LoamSize.ExtraSmall };
        Show(extraSmall);
        extraSmall.MinHeight.ShouldBe(40d);
        Root(extraSmall).Padding.ShouldBe(new Thickness(12, 0));

        var small = new Fab { Label = "Small", Size = LoamSize.Small };
        Show(small);
        small.MinHeight.ShouldBe(48d);
        Root(small).Padding.ShouldBe(new Thickness(16, 0));

        var large = new Fab { Label = "Large", Size = LoamSize.Large };
        Show(large);
        large.MinHeight.ShouldBe(96d);
        Root(large).Padding.ShouldBe(new Thickness(32, 0));

        var extraLarge = new Fab { Label = "ExtraLarge", Size = LoamSize.ExtraLarge };
        Show(extraLarge);
        extraLarge.MinHeight.ShouldBe(136d);
        Root(extraLarge).Padding.ShouldBe(new Thickness(48, 0));
    }

    [Fact]
    public void Icon_pixel_size_maps_per_size()
    {
        Icon.PixelSize(LoamSize.ExtraSmall).ShouldBe(18d);
        Icon.PixelSize(LoamSize.Small).ShouldBe(20d);
        Icon.PixelSize(LoamSize.Medium).ShouldBe(24d);
        Icon.PixelSize(LoamSize.Large).ShouldBe(32d);
        Icon.PixelSize(LoamSize.ExtraLarge).ShouldBe(40d);
    }

    [AvaloniaFact]
    public void Built_in_icon_paths_parse_as_geometry()
    {
        var fields = typeof(Icons.Material.Filled)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        foreach (var field in fields)
        {
            var data = (string)field.GetValue(null)!;
            var exception = Record.Exception(() => Geometry.Parse(data));

            exception.ShouldBeNull(field.Name);
        }
    }

    [AvaloniaFact]
    public void Icon_color_resolves_to_token()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var icon = new Icon { Data = Icons.Material.Filled.Home, Color = LoamColor.Primary };
        Show(icon);

        ((ISolidColorBrush)icon.Foreground!).Color.ShouldBe(Color.Parse("#6750A4"));
    }

    [AvaloniaFact]
    public void Icon_exposes_automation_name_and_inherit_clears_foreground()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var icon = new Icon { Data = Icons.Material.Filled.Home, Color = LoamColor.Primary };
        Show(icon);

        AutomationProperties.GetName(icon).ShouldBe("Icon");
        ((ISolidColorBrush)icon.Foreground!).Color.ShouldBe(Color.Parse("#6750A4"));

        icon.Color = LoamColor.Inherit;
        Dispatcher.UIThread.RunJobs();

        ((ISolidColorBrush)icon.Foreground!).Color.ShouldNotBe(Color.Parse("#6750A4"));
    }

    [AvaloniaFact]
    public void Button_start_icon_becomes_visible()
    {
        var button = new Loam.Controls.Button { Content = "Save", StartIcon = Icons.Material.Filled.Check };
        Show(button);
        button.ApplyTemplate();

        var start = button.GetVisualDescendants().OfType<Icon>().First(i => i.Name == "PART_StartIcon");
        start.IsVisible.ShouldBeTrue();
        start.Data.ShouldBe(Icons.Material.Filled.Check);
    }

    [AvaloniaFact]
    public void Button_templates_include_ripple_hosts()
    {
        var button = new Loam.Controls.Button { Content = "Save" };
        Show(button);
        button.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        button.GetVisualDescendants().OfType<Ripple>().ShouldNotBeEmpty();

        var iconButton = new IconButton { Icon = Icons.Material.Filled.Settings };
        Show(iconButton);
        iconButton.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        iconButton.GetVisualDescendants().OfType<Ripple>().ShouldNotBeEmpty();
    }

    [AvaloniaFact]
    public void Button_family_applies_all_size_metrics()
    {
        foreach (var size in new[] { LoamSize.ExtraSmall, LoamSize.Small, LoamSize.Medium, LoamSize.Large, LoamSize.ExtraLarge })
        {
            var button = new Loam.Controls.Button { Content = size.ToString(), Size = size };
            Show(button);
            button.MinHeight.ShouldBe(size switch
            {
                LoamSize.ExtraSmall => 32d,
                LoamSize.Small => 36d,
                LoamSize.Large => 54d,
                LoamSize.ExtraLarge => 64d,
                _ => 46d,
            }, size.ToString());
            button.FontSize.ShouldBe(size switch
            {
                LoamSize.ExtraSmall => 11d,
                LoamSize.Small => 12d,
                LoamSize.ExtraLarge => 16d,
                _ => 14d,
            }, size.ToString());

            var iconButton = new IconButton { Icon = Icons.Material.Filled.Settings, Size = size };
            Show(iconButton);
            iconButton.MinHeight.ShouldBe(size switch
            {
                LoamSize.ExtraSmall => 32d,
                LoamSize.Small => 36d,
                LoamSize.Large => 56d,
                LoamSize.ExtraLarge => 64d,
                _ => 48d,
            }, size.ToString());
        }
    }

    [AvaloniaFact]
    public void IconButton_colors_icon_via_inherited_foreground()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var iconButton = new IconButton { Icon = Icons.Material.Filled.Settings, Color = LoamColor.Primary };
        Show(iconButton);
        iconButton.ApplyTemplate();

        ((ISolidColorBrush)iconButton.Foreground!).Color.ShouldBe(Color.Parse("#6750A4"));
        iconButton.GetVisualDescendants().OfType<Icon>().First().Data.ShouldBe(Icons.Material.Filled.Settings);
    }

    [AvaloniaFact]
    public void Fab_is_filled_and_label_sets_content()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var fab = new Fab { Label = "Add", StartIcon = Icons.Material.Filled.Add, Color = LoamColor.Primary };
        Show(fab);
        fab.ApplyTemplate();

        ((ISolidColorBrush)Root(fab).Background!).Color.ShouldBe(Color.Parse("#6750A4"));
        fab.Content.ShouldBe("Add");
    }

    [AvaloniaFact]
    public void ButtonGroup_connects_children_with_shared_styles_and_outer_corners()
    {
        var group = new ButtonGroup { Variant = Variant.Outlined, Color = LoamColor.Primary };
        group.Items.Add(new Loam.Controls.Button { Content = "Left" });
        group.Items.Add(new Loam.Controls.Button { Content = "Mid" });
        group.Items.Add(new Loam.Controls.Button { Content = "Right" });
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        group.Items[0].Variant.ShouldBe(Variant.Outlined);
        group.Items[1].Color.ShouldBe(LoamColor.Primary);

        group.Items[0].CornerRadius.ShouldBe(new CornerRadius(4, 0, 0, 4));
        group.Items[1].CornerRadius.ShouldBe(new CornerRadius(0));
        group.Items[2].CornerRadius.ShouldBe(new CornerRadius(0, 4, 4, 0));
    }

    [AvaloniaFact]
    public void ButtonGroup_filled_child_focus_does_not_draw_inner_border()
    {
        var group = new ButtonGroup { Variant = Variant.Filled, Color = LoamColor.Secondary };
        group.Items.Add(new Loam.Controls.Button { Content = "Day" });
        group.Items.Add(new Loam.Controls.Button { Content = "Week" });
        group.Items.Add(new Loam.Controls.Button { Content = "Month" });
        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        group.Items[2].Focus();
        Dispatcher.UIThread.RunJobs();

        Root(group.Items[2]).BorderThickness.ShouldBe(new Thickness(0));
    }

    [AvaloniaFact]
    public void ButtonGroup_keeps_connected_strip_unclipped_in_constrained_parent()
    {
        var group = new ButtonGroup { Variant = Variant.Outlined, Color = LoamColor.Primary, Size = LoamSize.ExtraLarge };
        group.Items.Add(new Loam.Controls.Button { Content = "Day" });
        group.Items.Add(new Loam.Controls.Button { Content = "Week" });
        group.Items.Add(new Loam.Controls.Button { Content = "Month" });
        var host = new Avalonia.Controls.Grid { ColumnDefinitions = new ColumnDefinitions("260") };
        host.Children.Add(group);
        Show(host);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var strip = group.GetVisualDescendants().OfType<StackPanel>()
            .Single(panel => panel.Name == "PART_Items");
        strip.MinWidth.ShouldBeGreaterThan(260);
        strip.Bounds.Width.ShouldBeGreaterThan(260);
        group.Items.Select(item => Root(item).Bounds.Width).Sum().ShouldBeGreaterThan(260);
    }

    [AvaloniaFact]
    public void ButtonGroup_vertical_keeps_connected_strip_unclipped_in_constrained_parent()
    {
        var group = new ButtonGroup { Variant = Variant.Outlined, Color = LoamColor.Primary, Size = LoamSize.Large, Vertical = true };
        group.Items.Add(new Loam.Controls.Button { Content = "Day" });
        group.Items.Add(new Loam.Controls.Button { Content = "Week" });
        group.Items.Add(new Loam.Controls.Button { Content = "Month" });
        var host = new Avalonia.Controls.Grid { RowDefinitions = new RowDefinitions("120") };
        host.Children.Add(group);
        Show(host);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var strip = group.GetVisualDescendants().OfType<StackPanel>()
            .Single(panel => panel.Name == "PART_Items");
        strip.MinHeight.ShouldBeGreaterThan(120);
        strip.Bounds.Height.ShouldBeGreaterThan(120);
        group.Items.Select(item => Root(item).Bounds.Height).Sum().ShouldBeGreaterThan(120);
    }

    [AvaloniaFact]
    public void ToggleIconButton_toggled_state_swaps_the_glyph()
    {
        var button = new ToggleIconButton
        {
            Icon = Icons.Material.Filled.FavoriteBorder,
            ToggledIcon = Icons.Material.Filled.Favorite,
        };
        Show(button);
        button.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var icon = button.GetVisualDescendants().OfType<Icon>().First(i => i.Name == "PART_Icon");
        icon.Data.ShouldBe(Icons.Material.Filled.FavoriteBorder);

        button.Toggled = true;
        Dispatcher.UIThread.RunJobs();
        icon.Data.ShouldBe(Icons.Material.Filled.Favorite);
    }

    [AvaloniaFact]
    public void ToggleIconButton_toggled_color_applies_to_glyph_only_while_on()
    {
        var button = new ToggleIconButton
        {
            Icon = Icons.Material.Filled.FavoriteBorder,
            ToggledIcon = Icons.Material.Filled.Favorite,
            ToggledColor = LoamColor.Primary,
        };
        Show(button);
        button.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var icon = button.GetVisualDescendants().OfType<Icon>().First(i => i.Name == "PART_Icon");
        icon.Color.ShouldBe(LoamColor.Inherit);

        button.Toggled = true;
        Dispatcher.UIThread.RunJobs();
        icon.Color.ShouldBe(LoamColor.Primary);

        button.Toggled = false;
        Dispatcher.UIThread.RunJobs();
        icon.Color.ShouldBe(LoamColor.Inherit);
    }
}
