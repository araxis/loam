using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
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

public class FeedbackTests
{
    private static void Show(Control content)
    {
        new Window { Width = 400, Height = 300, Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static KeyEventArgs KeyArgs(Key key) => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = key,
    };

    [AvaloniaFact]
    public void Alert_filled_uses_severity_color()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var alert = new Alert { Color = LoamColor.Error, Variant = Variant.Filled, Content = "Failed" };
        Show(alert);
        alert.ApplyTemplate();

        var root = alert.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root");
        ((ISolidColorBrush)root.Background!).Color.ShouldBe(Color.Parse("#B3261E"));
        AutomationProperties.GetName(alert).ShouldBe("Failed");
    }

    [AvaloniaFact]
    public void Alert_disabled_uses_state_opacity()
    {
        var closed = false;
        var alert = new Alert { Content = "Paused", Closeable = true, IsEnabled = false };
        alert.Closed += (_, _) => closed = true;
        Show(alert);
        alert.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        alert.Opacity.ShouldBeLessThan(1);
        var close = alert.GetVisualDescendants().OfType<IconButton>().First(button => button.Name == "PART_Close");
        close.IsEnabled.ShouldBeFalse();

        alert.Close();

        alert.IsVisible.ShouldBeTrue();
        closed.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Alert_generates_title_message_action_and_close_regions()
    {
        var closed = false;
        var action = new Loam.Controls.Button { Content = "View", Variant = Variant.Text, Color = LoamColor.Info };
        var alert = new Alert
        {
            Color = LoamColor.Info,
            Icon = Icons.Material.Filled.Settings,
            Title = "Configuration saved",
            Message = "Generated alert anatomy is built by the control.",
            Action = action,
            Closeable = true,
        };
        alert.Closed += (_, _) => closed = true;
        Show(alert);
        alert.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        alert.GetVisualDescendants().OfType<Icon>().First(icon => icon.Name == "PART_Icon").IsVisible.ShouldBeTrue();
        alert.GetVisualDescendants().OfType<Text>().First(text => text.Name == "PART_Title").Text.ShouldBe("Configuration saved");
        alert.GetVisualDescendants().OfType<Text>().First(text => text.Name == "PART_Message").Text.ShouldBe("Generated alert anatomy is built by the control.");
        alert.GetVisualDescendants().OfType<ContentPresenter>().First(presenter => presenter.Name == "PART_Action").Content.ShouldBeSameAs(action);

        var close = alert.GetVisualDescendants().OfType<IconButton>().First(button => button.Name == "PART_Close");
        close.IsVisible.ShouldBeTrue();
        close.Focusable.ShouldBeTrue();
        close.Icon.ShouldBe(Icons.Material.Filled.Close);
        close.MinWidth.ShouldBeGreaterThanOrEqualTo(40);
        AutomationProperties.GetName(close).ShouldBe("Close alert");
        AutomationProperties.GetHelpText(close).ShouldBe("Dismiss alert");
        AutomationProperties.GetName(alert).ShouldBe("Configuration saved Generated alert anatomy is built by the control.");

        close.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        closed.ShouldBeTrue();
        alert.IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ProgressLinear_fill_reflects_value()
    {
        var progress = new ProgressLinear { Value = 50, Width = 200 };
        Show(progress);
        progress.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        progress.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Fill").Width.ShouldBeGreaterThan(0);
        AutomationProperties.GetName(progress).ShouldBe("Progress");
    }

    [AvaloniaFact]
    public void ProgressLinear_indeterminate_uses_moving_fill()
    {
        var progress = new ProgressLinear { Indeterminate = true, Width = 200 };
        Show(progress);
        progress.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var fill = progress.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Fill");
        fill.Width.ShouldBeGreaterThanOrEqualTo(24);

        progress.IndeterminateOffset = 0.5;
        Dispatcher.UIThread.RunJobs();

        var transform = fill.RenderTransform.ShouldBeOfType<TranslateTransform>();
        transform.X.ShouldNotBe(0);
    }

    [AvaloniaFact]
    public void ProgressLinear_generates_label_value_text_and_automation()
    {
        var progress = new ProgressLinear
        {
            Label = "Upload",
            ShowValue = true,
            Value = 42,
            Width = 240,
        };
        Show(progress);
        progress.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        progress.GetVisualDescendants().OfType<Text>().First(text => text.Name == "PART_Label").Text.ShouldBe("Upload");
        progress.GetVisualDescendants().OfType<Text>().First(text => text.Name == "PART_ValueText").Text.ShouldBe("42%");
        progress.GetVisualDescendants().OfType<Control>().First(control => control.Name == "PART_Header").IsVisible.ShouldBeTrue();
        AutomationProperties.GetName(progress).ShouldBe("Upload");
        AutomationProperties.GetHelpText(progress).ShouldBe("42%");

        progress.ValueText = "Almost done";
        Dispatcher.UIThread.RunJobs();

        progress.GetVisualDescendants().OfType<Text>().First(text => text.Name == "PART_ValueText").Text.ShouldBe("Almost done");
        AutomationProperties.GetHelpText(progress).ShouldBe("Almost done");
    }

    [Fact]
    public void ProgressLinear_fraction_clamps_and_sizes_by_size()
    {
        ProgressLinear.Fraction(25, 0, 100).ShouldBe(0.25);
        ProgressLinear.Fraction(-5, 0, 100).ShouldBe(0);
        ProgressLinear.Fraction(150, 0, 100).ShouldBe(1);
        ProgressLinear.TrackHeight(LoamSize.ExtraSmall).ShouldBe(2);
        ProgressLinear.TrackHeight(LoamSize.Small).ShouldBe(3);
        ProgressLinear.TrackHeight(LoamSize.Medium).ShouldBe(4);
        ProgressLinear.TrackHeight(LoamSize.Large).ShouldBe(6);
        ProgressLinear.TrackHeight(LoamSize.ExtraLarge).ShouldBe(8);
    }

    [AvaloniaFact]
    public void ProgressLinear_size_updates_track_metrics()
    {
        var progress = new ProgressLinear { Size = LoamSize.ExtraLarge, Value = 50, Width = 240 };
        Show(progress);
        progress.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var area = progress.GetVisualDescendants().OfType<Panel>().First(panel => panel.Name == "PART_Area");
        var track = progress.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_Track");
        var fill = progress.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_Fill");

        area.Height.ShouldBe(8);
        track.Height.ShouldBe(8);
        fill.Height.ShouldBe(8);
        track.CornerRadius.ShouldBe(new CornerRadius(4));
        fill.CornerRadius.ShouldBe(new CornerRadius(4));
    }

    [AvaloniaFact]
    public void ProgressLinear_disabled_indeterminate_is_static_and_named()
    {
        var progress = new ProgressLinear
        {
            Label = "Queued",
            Indeterminate = true,
            IsEnabled = false,
            Width = 240,
        };
        Show(progress);
        progress.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var fill = progress.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_Fill");
        fill.Width.ShouldBeGreaterThanOrEqualTo(24);
        fill.RenderTransform.ShouldBeOfType<TranslateTransform>().X.ShouldBe(0);
        progress.IndeterminateOffset.ShouldBe(0);
        AutomationProperties.GetName(progress).ShouldBe("Queued");
        AutomationProperties.GetHelpText(progress).ShouldBe("Indeterminate");
    }

    [AvaloniaFact]
    public void Skeleton_static_and_animated_modes_are_configurable()
    {
        var skeleton = new Skeleton { Animate = false };
        Show(skeleton);

        AutomationProperties.GetName(skeleton).ShouldBe("Loading");
        skeleton.Animate.ShouldBeFalse();
        skeleton.Opacity.ShouldBe(1);

        skeleton.Animate = true;
        Dispatcher.UIThread.RunJobs();
        skeleton.Animate.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Skeleton_presets_and_factories_apply_generated_metrics()
    {
        var line = Skeleton.TextLine(180, LoamSize.Large, animate: false, label: "Headline loading");
        Show(line);
        line.Preset.ShouldBe(SkeletonPreset.Text);
        line.Size.ShouldBe(LoamSize.Large);
        line.Width.ShouldBe(180);
        line.Height.ShouldBe(20);
        line.Animate.ShouldBeFalse();
        AutomationProperties.GetName(line).ShouldBe("Headline loading");

        var avatar = Skeleton.Avatar(LoamSize.Small, label: "User loading");
        Show(avatar);
        avatar.Preset.ShouldBe(SkeletonPreset.Avatar);
        avatar.Circle.ShouldBeTrue();
        avatar.Width.ShouldBe(32);
        avatar.Height.ShouldBe(32);
        avatar.CornerRadius.ShouldBe(new CornerRadius(16));
        AutomationProperties.GetName(avatar).ShouldBe("User loading");

        var card = Skeleton.Card(260, 96, animate: false, label: "Card loading");
        Show(card);
        card.Preset.ShouldBe(SkeletonPreset.Card);
        card.Width.ShouldBe(260);
        card.Height.ShouldBe(96);
        card.CornerRadius.ShouldBe(new CornerRadius(12));
        card.Animate.ShouldBeFalse();
    }

    [Fact]
    public void Skeleton_size_metrics_cover_all_sizes()
    {
        Skeleton.TextLineHeight(LoamSize.ExtraSmall).ShouldBe(10);
        Skeleton.TextLineHeight(LoamSize.Small).ShouldBe(12);
        Skeleton.TextLineHeight(LoamSize.Medium).ShouldBe(16);
        Skeleton.TextLineHeight(LoamSize.Large).ShouldBe(20);
        Skeleton.TextLineHeight(LoamSize.ExtraLarge).ShouldBe(24);
        Skeleton.AvatarSize(LoamSize.ExtraSmall).ShouldBe(24);
        Skeleton.AvatarSize(LoamSize.Small).ShouldBe(32);
        Skeleton.AvatarSize(LoamSize.Medium).ShouldBe(40);
        Skeleton.AvatarSize(LoamSize.Large).ShouldBe(56);
        Skeleton.AvatarSize(LoamSize.ExtraLarge).ShouldBe(72);
        Skeleton.ButtonHeight(LoamSize.ExtraSmall).ShouldBe(32);
        Skeleton.ButtonHeight(LoamSize.Small).ShouldBe(40);
        Skeleton.ButtonHeight(LoamSize.Medium).ShouldBe(48);
        Skeleton.ButtonHeight(LoamSize.Large).ShouldBe(56);
        Skeleton.ButtonHeight(LoamSize.ExtraLarge).ShouldBe(64);
    }

    [AvaloniaFact]
    public void Skeleton_disabled_state_is_static_and_accessible()
    {
        var skeleton = new Skeleton
        {
            Preset = SkeletonPreset.Text,
            Size = LoamSize.Medium,
            Width = 180,
            Label = "Disabled loading",
            IsEnabled = false,
        };
        Show(skeleton);
        Dispatcher.UIThread.RunJobs();

        skeleton.Height.ShouldBe(16);
        skeleton.Opacity.ShouldBeLessThan(1);
        AutomationProperties.GetName(skeleton).ShouldBe("Disabled loading");
        AutomationProperties.GetHelpText(skeleton).ShouldBe("Static loading placeholder");

        skeleton.IsEnabled = true;
        skeleton.Animate = false;
        Dispatcher.UIThread.RunJobs();

        skeleton.Opacity.ShouldBe(1);
        AutomationProperties.GetHelpText(skeleton).ShouldBe("Static loading placeholder");
    }

    [Fact]
    public void ProgressCircular_fraction_clamps_and_sizes_by_size()
    {
        ProgressCircular.Fraction(25, 0, 100).ShouldBe(0.25);
        ProgressCircular.Fraction(-5, 0, 100).ShouldBe(0);
        ProgressCircular.Fraction(150, 0, 100).ShouldBe(1);
        ProgressCircular.Diameter(LoamSize.ExtraSmall).ShouldBe(24);
        ProgressCircular.Diameter(LoamSize.Small).ShouldBe(32);
        ProgressCircular.Diameter(LoamSize.Medium).ShouldBe(48);
        ProgressCircular.Diameter(LoamSize.Large).ShouldBe(64);
        ProgressCircular.Diameter(LoamSize.ExtraLarge).ShouldBe(80);
        ProgressCircular.DefaultStrokeWidth(LoamSize.Medium).ShouldBe(4);
        ProgressCircular.EffectiveStrokeWidth(LoamSize.Medium, 6).ShouldBe(6);
    }

    [AvaloniaFact]
    public void ProgressCircular_measures_to_its_diameter()
    {
        var progress = new ProgressCircular { Size = LoamSize.Medium, Indeterminate = false, Value = 40 };
        Show(progress);
        progress.Measure(Size.Infinity);

        progress.DesiredSize.Width.ShouldBe(48);
        progress.DesiredSize.Height.ShouldBe(48);
    }

    [AvaloniaFact]
    public void ProgressCircular_generates_value_text_and_automation()
    {
        var progress = new ProgressCircular
        {
            Label = "Import",
            Indeterminate = false,
            Value = 33,
            ShowValue = true,
        };
        Show(progress);
        progress.Measure(Size.Infinity);

        progress.DesiredSize.Width.ShouldBe(48);
        AutomationProperties.GetName(progress).ShouldBe("Import");
        AutomationProperties.GetHelpText(progress).ShouldBe("33%");

        progress.ValueText = "Step 2";
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetHelpText(progress).ShouldBe("Step 2");
    }

    [AvaloniaFact]
    public void ProgressCircular_disabled_uses_static_disabled_state()
    {
        var progress = new ProgressCircular
        {
            Label = "Queued",
            Indeterminate = true,
            IsEnabled = false,
        };
        Show(progress);
        progress.Measure(Size.Infinity);

        progress.DesiredSize.Width.ShouldBe(48);
        progress.SpinAngle.ShouldBe(0);
        AutomationProperties.GetName(progress).ShouldBe("Queued");
        AutomationProperties.GetHelpText(progress).ShouldBe("Indeterminate");
    }

    [AvaloniaFact]
    public void ListItem_shows_icon_and_content()
    {
        var item = new ListItem { Icon = Icons.Material.Filled.Home, Content = "Home" };
        Show(item);
        item.ApplyTemplate();

        item.GetVisualDescendants().OfType<Icon>().First(i => i.Name == "PART_Icon").IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void ListItem_shows_secondary_text_and_trailing_action()
    {
        var action = new IconButton { Icon = Icons.Material.Filled.Settings };
        var item = new ListItem
        {
            Icon = Icons.Material.Filled.Home,
            Content = "Inbox",
            SecondaryText = "24 unread",
            Action = action,
        };
        Show(item);
        item.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var secondary = item.GetVisualDescendants().OfType<Text>().First(text => text.Name == "PART_SecondaryText");
        secondary.Text.ShouldBe("24 unread");
        secondary.IsVisible.ShouldBeTrue();

        var actionPresenter = item.GetVisualDescendants().OfType<ContentPresenter>()
            .First(presenter => presenter.Name == "PART_Action");
        actionPresenter.Content.ShouldBeSameAs(action);
        actionPresenter.IsVisible.ShouldBeTrue();

        AutomationProperties.GetName(item).ShouldBe("Inbox 24 unread");
    }

    [AvaloniaFact]
    public void ListItem_is_named_selected_disabled_and_keyboard_activates()
    {
        var activated = false;
        var item = new ListItem
        {
            Icon = Icons.Material.Filled.Home,
            Content = "Inbox",
            IsSelected = true,
        };
        item.Activated += (_, _) => activated = true;
        Show(item);
        item.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        item.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(item).ShouldBe("Inbox");
        var root = item.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_Root");
        root.Focusable.ShouldBeTrue();

        var key = KeyArgs(Key.Space);
        item.RaiseEvent(key);
        key.Handled.ShouldBeTrue();
        activated.ShouldBeTrue();

        item.IsEnabled = false;
        Dispatcher.UIThread.RunJobs();
        item.Opacity.ShouldBeLessThan(1);

        var disabledKey = KeyArgs(Key.Enter);
        item.RaiseEvent(disabledKey);
        disabledKey.Handled.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ListItem_hover_uses_neutral_state_layer_not_table_hover()
    {
        var item = new ListItem { Content = "Archive" };
        var window = new Window { Width = 240, Height = 120, Content = item };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        item.ApplyTemplate();

        var root = item.GetVisualDescendants().OfType<Border>().First(border => border.Name == "PART_Root");
        var point = root.TranslatePoint(new Point(root.Bounds.Width / 2, root.Bounds.Height / 2), window);
        point.ShouldNotBeNull();

        window.MouseMove(point.Value);
        Dispatcher.UIThread.RunJobs();

        var hoverColor = ((ISolidColorBrush)root.Background!).Color;
        hoverColor.ShouldBe(LoamColorScheme.DefaultLight.OnSurface.WithAlpha(0.08));
        hoverColor.ShouldNotBe(LoamPalette.DefaultLight.TableHover);
    }
}
