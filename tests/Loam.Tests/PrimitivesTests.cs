using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
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

        ((ISolidColorBrush)Root(paper).Background!).Color.ShouldBe(Colors.White);
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
    public void Text_typo_drives_font_size_and_weight()
    {
        var text = new Text { Text = "hi", Typo = Typo.H6 };
        Show(text);

        text.FontSize.ShouldBe(20d);
        text.FontWeight.ShouldBe(FontWeight.Medium);
    }

    [AvaloniaFact]
    public void Text_color_drives_foreground()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var text = new Text { Text = "hi", Color = LoamColor.Primary };
        Show(text);

        ((ISolidColorBrush)text.Foreground!).Color.ShouldBe(Color.Parse("#594AE2"));
    }

    [AvaloniaFact]
    public void Divider_orientation_sets_thickness()
    {
        new Divider().Height.ShouldBe(1d);
        new Divider { Vertical = true }.Width.ShouldBe(1d);
    }

    [AvaloniaFact]
    public void Button_filled_primary_uses_primary_fill_and_contrast_text()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var button = new Loam.Controls.Button { Content = "Go", Variant = Variant.Filled, Color = LoamColor.Primary };
        Show(button);

        ((ISolidColorBrush)Root(button).Background!).Color.ShouldBe(Color.Parse("#594AE2"));
        ((ISolidColorBrush)button.Foreground!).Color.ShouldBe(Colors.White);
    }

    [AvaloniaFact]
    public void Button_text_variant_has_transparent_background()
    {
        var button = new Loam.Controls.Button { Content = "Go", Variant = Variant.Text, Color = LoamColor.Primary };
        Show(button);

        ((ISolidColorBrush)Root(button).Background!).Color.A.ShouldBe((byte)0);
    }

    [Fact]
    public void Icon_pixel_size_maps_per_size()
    {
        Icon.PixelSize(LoamSize.Small).ShouldBe(20d);
        Icon.PixelSize(LoamSize.Medium).ShouldBe(24d);
        Icon.PixelSize(LoamSize.Large).ShouldBe(32d);
    }

    [AvaloniaFact]
    public void Icon_color_resolves_to_token()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var icon = new Icon { Data = Icons.Material.Filled.Home, Color = LoamColor.Primary };
        Show(icon);

        ((ISolidColorBrush)icon.Foreground!).Color.ShouldBe(Color.Parse("#594AE2"));
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
    public void IconButton_colors_icon_via_inherited_foreground()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var iconButton = new IconButton { Icon = Icons.Material.Filled.Settings, Color = LoamColor.Primary };
        Show(iconButton);
        iconButton.ApplyTemplate();

        ((ISolidColorBrush)iconButton.Foreground!).Color.ShouldBe(Color.Parse("#594AE2"));
        iconButton.GetVisualDescendants().OfType<Icon>().First().Data.ShouldBe(Icons.Material.Filled.Settings);
    }

    [AvaloniaFact]
    public void Fab_is_filled_and_label_sets_content()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var fab = new Fab { Label = "Add", StartIcon = Icons.Material.Filled.Add, Color = LoamColor.Primary };
        Show(fab);
        fab.ApplyTemplate();

        ((ISolidColorBrush)Root(fab).Background!).Color.ShouldBe(Color.Parse("#594AE2"));
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
}
