using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Reflection;
using Loam;
using Loam.Controls;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class DisplayTests
{
    private static Border Root(Control control)
    {
        control.ApplyTemplate();
        return control.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root");
    }

    private static void Show(Control content)
    {
        new Window { Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static KeyEventArgs KeyArgs(Key key) => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = key,
    };

    [AvaloniaFact]
    public void Avatar_filled_primary_sizes_and_colors()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var avatar = new Avatar { Variant = Variant.Filled, Color = LoamColor.Primary, Content = "AB" };
        Show(avatar);

        avatar.Width.ShouldBe(40d);
        ((ISolidColorBrush)Root(avatar).Background!).Color.ShouldBe(Color.Parse("#6750A4"));
        ((ISolidColorBrush)avatar.Foreground!).Color.ShouldBe(Colors.White);
    }

    [AvaloniaFact]
    public void Chip_filled_primary_colors_and_shows_text()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var chip = new Chip { Variant = Variant.Filled, Color = LoamColor.Primary, Text = "Tag", Closeable = true };
        Show(chip);

        ((ISolidColorBrush)Root(chip).Background!).Color.ShouldBe(Color.Parse("#6750A4"));
        chip.GetVisualDescendants().OfType<Text>().First().Text.ShouldBe("Tag");
        var close = chip.GetVisualDescendants().OfType<Icon>().First(i => i.Name == "PART_Close");
        close.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Chip_is_focusable_named_and_closeable_from_keyboard()
    {
        var closed = false;
        var chip = new Chip { Text = "Tag", Closeable = true };
        chip.Closed += (_, _) => closed = true;
        Show(chip);
        chip.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        chip.Focusable.ShouldBeTrue();
        Root(chip).Focusable.ShouldBeTrue();
        AutomationProperties.GetName(chip).ShouldBe("Tag");

        var key = KeyArgs(Key.Enter);
        chip.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        closed.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Badge_formats_value_and_colors()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var badge = new Badge { Value = 5, Color = LoamColor.Error, Content = new TextBlock { Text = "x" } };
        Show(badge);
        badge.ApplyTemplate();

        var text = badge.GetVisualDescendants().OfType<TextBlock>().First(t => t.Name == "PART_BadgeText");
        text.Text.ShouldBe("5");
        var border = badge.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Badge");
        ((ISolidColorBrush)border.Background!).Color.ShouldBe(Color.Parse("#B3261E"));
    }

    [AvaloniaFact]
    public void Badge_caps_value_at_max()
    {
        var badge = new Badge { Value = 150, Max = 99, Content = new TextBlock { Text = "x" } };
        Show(badge);
        badge.ApplyTemplate();

        badge.GetVisualDescendants().OfType<TextBlock>().First(t => t.Name == "PART_BadgeText").Text.ShouldBe("99+");
    }

    [AvaloniaFact]
    public void Badge_reserves_space_for_top_right_indicator()
    {
        var badge = new Badge
        {
            Value = 4,
            Content = new Icon { Data = Icons.Material.Filled.Favorite, Size = LoamSize.Large },
        };
        Show(badge);
        badge.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var presenter = badge.GetVisualDescendants().OfType<ContentPresenter>()
            .First(p => p.Name == "PART_ContentPresenter");
        var indicator = badge.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Badge");

        presenter.Margin.ShouldBe(new Thickness(0, 9, 9, 0));
        indicator.RenderTransform.ShouldBeNull();
        indicator.Bounds.Y.ShouldBeGreaterThanOrEqualTo(0);
    }

    [AvaloniaFact]
    public void AvatarGroup_collapses_overflow_into_surplus()
    {
        var group = new AvatarGroup { Max = 4 };
        for (var i = 0; i < 6; i++)
        {
            group.Items.Add(new Avatar { Content = i.ToString() });
        }

        Show(group);
        group.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var panel = group.GetVisualDescendants().OfType<StackPanel>().First(p => p.Name == "PART_Items");
        panel.Children.Count.ShouldBe(5);
        panel.Children[4].ShouldBeOfType<Avatar>().Content.ShouldBe("+2");
    }

    [AvaloniaFact]
    public void ChipSet_selected_chip_is_filled_others_outlined()
    {
        var set = new ChipSet { Selectable = true };
        set.Items.Add(new Chip { Text = "A" });
        set.Items.Add(new Chip { Text = "B" });
        set.Items.Add(new Chip { Text = "C" });
        Show(set);
        set.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        set.SelectedIndex = 1;
        Dispatcher.UIThread.RunJobs();
        set.Items[1].Variant.ShouldBe(Variant.Filled);
        set.Items[0].Variant.ShouldBe(Variant.Outlined);
        set.Items[2].Variant.ShouldBe(Variant.Outlined);
    }

    [AvaloniaFact]
    public void ChipSet_selects_chips_from_keyboard()
    {
        var set = new ChipSet { Selectable = true };
        set.Items.Add(new Chip { Text = "A" });
        set.Items.Add(new Chip { Text = "B" });
        Show(set);
        set.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        set.Items.All(chip => chip.Focusable).ShouldBeTrue();
        var key = KeyArgs(Key.Space);
        set.Items[1].RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        set.SelectedIndex.ShouldBe(1);
        set.Items[1].Variant.ShouldBe(Variant.Filled);
    }

    [AvaloniaFact]
    public void ChipSet_multiselect_fills_all_selected_indexes_and_tracks_first()
    {
        var set = new ChipSet { Selectable = true, MultiSelect = true };
        set.Items.Add(new Chip { Text = "A" });
        set.Items.Add(new Chip { Text = "B" });
        set.Items.Add(new Chip { Text = "C" });
        Show(set);
        set.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        set.SelectedIndexes.Add(0);
        set.SelectedIndexes.Add(2);
        Dispatcher.UIThread.RunJobs();

        set.SelectedIndex.ShouldBe(0);
        set.Items[0].Variant.ShouldBe(Variant.Filled);
        set.Items[1].Variant.ShouldBe(Variant.Outlined);
        set.Items[2].Variant.ShouldBe(Variant.Filled);

        set.SelectedIndexes.Add(2);
        set.SelectedIndexes.Add(99);
        set.SelectedIndexes.ShouldBe([0, 2]);
    }

    [AvaloniaFact]
    public void ChipSet_mandatory_multiselect_keeps_last_selected_chip_when_clicked()
    {
        var set = new ChipSet { Selectable = true, MultiSelect = true, Mandatory = true };
        set.Items.Add(new Chip { Text = "A" });
        Show(set);
        set.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        set.SelectedIndexes.Add(0);
        var press = typeof(ChipSet).GetMethod("OnChipPressed", BindingFlags.Instance | BindingFlags.NonPublic);
        press.ShouldNotBeNull();
        press!.Invoke(set, [set.Items[0], null]);

        set.SelectedIndexes.ShouldBe([0]);
        set.SelectedIndex.ShouldBe(0);
    }

    [AvaloniaFact]
    public void CardHeader_shows_title_subtitle_and_hides_absent_avatar()
    {
        var header = new CardHeader { Title = "Project Loam", Subtitle = "Updated today" };
        Show(header);
        header.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        header.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Title").Text.ShouldBe("Project Loam");
        header.GetVisualDescendants().OfType<Text>().First(t => t.Name == "PART_Subtitle").Text.ShouldBe("Updated today");
        header.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>()
            .First(p => p.Name == "PART_Avatar").IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void CardMedia_band_uses_media_height()
    {
        var media = new CardMedia { MediaHeight = 120 };
        Show(media);
        media.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        media.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root").Height.ShouldBe(120);
    }
}
