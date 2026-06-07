using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Loam.Controls.Internal;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class OverlayTests
{
    private static OverlayLayer ShowAnchor(out Border anchor)
    {
        anchor = new Border();
        new Window { Width = 400, Height = 300, Content = anchor }.Show();
        Dispatcher.UIThread.RunJobs();
        return OverlayLayer.GetOverlayLayer(anchor)!;
    }

    private static KeyEventArgs KeyArgs(Key key) => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = key,
    };

    [AvaloniaFact]
    public void PopupSurface_flyout_presenter_theme_renders_content_without_extra_chrome()
    {
        var presenter = new FlyoutPresenter
        {
            Theme = PopupSurface.FlyoutPresenterTheme,
            Content = new TextBlock { Text = "Archive" },
        };
        new Window { Content = presenter }.Show();
        presenter.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        presenter.Padding.ShouldBe(new Thickness(0));
        presenter.BorderThickness.ShouldBe(new Thickness(0));
        presenter.GetVisualDescendants().OfType<TextBlock>().Select(text => text.Text).ShouldContain("Archive");
    }

    [AvaloniaFact]
    public void PopupSurface_menu_paper_uses_compact_menu_surface()
    {
        var paper = PopupSurface.MenuPaper(new Border(), 180);

        paper.Elevation.ShouldBe(3);
        paper.Padding.ShouldBe(new Thickness(0));
        paper.MinWidth.ShouldBe(180);
        paper.ClipToBounds.ShouldBeTrue();
        paper.Resources.TryGetResource(LoamTokens.ShapeMedium, ThemeVariant.Light, out var radius).ShouldBeTrue();
        radius.ShouldBe(LoamShape.Default.ExtraSmall);
    }

    [AvaloniaFact]
    public void PopupSurface_picker_paper_uses_shared_picker_surface()
    {
        var paper = PopupSurface.PickerPaper(new Border());

        paper.Elevation.ShouldBe(3);
        paper.Width.ShouldBe(PopupSurface.PickerWidth);
        paper.MinWidth.ShouldBe(PopupSurface.PickerWidth);
        paper.MaxWidth.ShouldBe(PopupSurface.PickerWidth);
        paper.Padding.ShouldBe(PopupSurface.PickerPadding);
        paper.ClipToBounds.ShouldBeTrue();
        paper.Resources.TryGetResource(LoamTokens.ShapeMedium, ThemeVariant.Light, out var radius).ShouldBeTrue();
        radius.ShouldBe(PopupSurface.PickerShape);

        new Window { Width = 420, Height = 420, Content = paper }.Show();
        paper.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        paper.GetVisualDescendants().OfType<Border>()
            .First(border => border.Name == "PART_Root")
            .CornerRadius.ShouldBe(PopupSurface.PickerShape);
    }

    [AvaloniaFact]
    public void PopupSurface_picker_content_uses_title_body_and_optional_actions()
    {
        var body = new Border();
        var actions = new Border();
        var content = PopupSurface.PickerContent("Select value", body, actions);

        content.Spacing.ShouldBe(0);
        content.Children.Count.ShouldBe(3);
        var title = content.Children[0].ShouldBeOfType<Text>();
        title.Typo.ShouldBe(Typo.TitleMedium);
        title.Margin.ShouldBe(PopupSurface.PickerTitleMargin);
        actions.Margin.ShouldBe(PopupSurface.PickerActionsMargin);
        content.Children[1].ShouldBeSameAs(body);
        content.Children[2].ShouldBeSameAs(actions);
    }

    [AvaloniaFact]
    public async Task Dialog_shows_in_overlay_and_closes_with_result()
    {
        var layer = ShowAnchor(out var anchor);
        var service = DialogService.For(anchor);

        DialogInstance? instance = null;
        var task = service.ShowAsync("Title", i => { instance = i; return new Border(); });
        Dispatcher.UIThread.RunJobs();

        task.IsCompleted.ShouldBeFalse();
        layer.GetVisualDescendants().OfType<Paper>().ShouldNotBeEmpty();

        instance!.Ok("data");
        instance.Cancel();
        var result = await task;

        result.Canceled.ShouldBeFalse();
        result.Data.ShouldBe("data");
        Dispatcher.UIThread.RunJobs();
        layer.GetVisualDescendants().OfType<Paper>().ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task Confirm_returns_true_when_ok_clicked()
    {
        var layer = ShowAnchor(out var anchor);
        var service = DialogService.For(anchor);

        var task = service.ConfirmAsync("Title", "Are you sure?");
        Dispatcher.UIThread.RunJobs();

        var cancel = layer.GetVisualDescendants().OfType<Loam.Controls.Button>().First(b => (string?)b.Content == "Cancel");
        cancel.Variant.ShouldBe(Variant.Text);
        cancel.Color.ShouldBe(LoamColor.Primary);
        TopLevel.GetTopLevel(anchor)!.FocusManager!.GetFocusedElement().ShouldBe(cancel);
        var ok = layer.GetVisualDescendants().OfType<Loam.Controls.Button>().First(b => (string?)b.Content == "OK");
        ok.Variant.ShouldBe(Variant.Text);
        ok.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        (await task).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task MessageBox_returns_true_for_yes_and_false_for_no()
    {
        var layer = ShowAnchor(out var anchor);
        var service = DialogService.For(anchor);

        var yesTask = service.MessageBoxAsync("Title", "Save changes?", "Yes", "No", "Cancel");
        Dispatcher.UIThread.RunJobs();
        var yes = layer.GetVisualDescendants().OfType<Loam.Controls.Button>().First(b => (string?)b.Content == "Yes");
        yes.Variant.ShouldBe(Variant.Text);
        yes.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        (await yesTask).ShouldBe(true);

        var noTask = service.MessageBoxAsync("Title", "Save changes?", "Yes", "No", "Cancel");
        Dispatcher.UIThread.RunJobs();
        layer.GetVisualDescendants().OfType<Loam.Controls.Button>().First(b => (string?)b.Content == "No")
            .RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        (await noTask).ShouldBe(false);
    }

    [AvaloniaFact]
    public void Snackbar_adds_a_toast_to_the_overlay()
    {
        var layer = ShowAnchor(out var anchor);
        var snackbar = SnackbarService.For(anchor);

        snackbar.Add("Saved", LoamColor.Success);
        Dispatcher.UIThread.RunJobs();

        var toast = layer.GetVisualDescendants().OfType<Border>()
            .Single(border => AutomationProperties.GetName(border) == "Saved");
        toast.MinWidth.ShouldBe(280);
        toast.MinHeight.ShouldBe(48);
        AutomationProperties.GetHelpText(toast).ShouldBe("Snackbar at BottomRight, Escape dismissible");
        var variant = toast.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Dark : ThemeVariant.Light;
        var theme = new LoamTheme();
        theme.Resources.TryGetResource(LoamTokens.ColorScheme(nameof(LoamColorScheme.InverseSurface)), variant, out var surface).ShouldBeTrue();
        theme.Resources.TryGetResource(LoamTokens.ShapeExtraSmall, variant, out var shape).ShouldBeTrue();
        ((ISolidColorBrush)toast.Background!).Color.ShouldBe(((ISolidColorBrush)surface!).Color);
        toast.CornerRadius.ShouldBe((CornerRadius)shape!);

        var message = toast.GetVisualDescendants().OfType<Text>().Single(text => text.Text == "Saved");
        theme.Resources.TryGetResource(LoamTokens.ColorScheme(nameof(LoamColorScheme.InverseOnSurface)), variant, out var contentColor).ShouldBeTrue();
        ((ISolidColorBrush)message.Foreground!).Color.ShouldBe(((ISolidColorBrush)contentColor!).Color);
    }

    [AvaloniaFact]
    public void Snackbar_action_invokes_callback_and_dismisses_toast()
    {
        var layer = ShowAnchor(out var anchor);
        var snackbar = SnackbarService.For(anchor);
        var invoked = 0;

        snackbar.Add(new SnackbarOptions("Archived")
        {
            Severity = LoamColor.Info,
            ActionText = "Undo",
            Action = () => invoked++,
            Duration = Timeout.InfiniteTimeSpan,
        });
        Dispatcher.UIThread.RunJobs();

        var action = layer.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .First(button => (string?)button.Content == "Undo");
        var toast = layer.GetVisualDescendants().OfType<Border>()
            .Single(border => AutomationProperties.GetName(border) == "Archived");
        AutomationProperties.GetHelpText(toast).ShouldBe("Snackbar at BottomRight, Escape dismissible, Action Undo");
        var variant = action.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Dark : ThemeVariant.Light;
        var theme = new LoamTheme();
        theme.Resources.TryGetResource(LoamTokens.ColorScheme(nameof(LoamColorScheme.InversePrimary)), variant, out var actionColor).ShouldBeTrue();
        ((ISolidColorBrush)action.Foreground!).Color.ShouldBe(((ISolidColorBrush)actionColor!).Color);

        action.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        action.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        invoked.ShouldBe(1);
        layer.GetVisualDescendants().OfType<Border>()
            .Where(border => AutomationProperties.GetName(border) == "Archived")
            .ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void Snackbar_position_and_dismiss_button_are_component_options()
    {
        var layer = ShowAnchor(out var anchor);
        var snackbar = SnackbarService.For(anchor);

        snackbar.Add(new SnackbarOptions("Positioned")
        {
            Duration = Timeout.InfiniteTimeSpan,
            DismissText = "Dismiss",
            Position = SnackbarPosition.TopCenter,
        });
        Dispatcher.UIThread.RunJobs();

        var host = layer.GetVisualDescendants().OfType<StackPanel>()
            .Single(panel => panel.Name == "PART_LoamSnackbarHost");
        host.HorizontalAlignment.ShouldBe(HorizontalAlignment.Center);
        host.VerticalAlignment.ShouldBe(VerticalAlignment.Top);
        var toast = layer.GetVisualDescendants().OfType<Border>()
            .Single(border => AutomationProperties.GetName(border) == "Positioned");
        AutomationProperties.GetHelpText(toast).ShouldBe("Snackbar at TopCenter, Escape dismissible, Dismiss Dismiss");

        var dismiss = layer.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => (string?)button.Content == "Dismiss");
        dismiss.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        layer.GetVisualDescendants().OfType<Border>()
            .Where(border => AutomationProperties.GetName(border) == "Positioned")
            .ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void Snackbar_limits_visible_toasts_across_service_instances()
    {
        var layer = ShowAnchor(out var anchor);

        for (var i = 1; i <= 4; i++)
        {
            SnackbarService.For(anchor).Add(new SnackbarOptions($"Message {i}")
            {
                Duration = Timeout.InfiniteTimeSpan,
                MaxVisible = 2,
            });
        }

        Dispatcher.UIThread.RunJobs();

        var toasts = layer.GetVisualDescendants().OfType<Border>()
            .Where(border => AutomationProperties.GetName(border)?.StartsWith("Message ") == true)
            .ToArray();
        toasts.Length.ShouldBe(2);
        var messages = layer.GetVisualDescendants().OfType<Text>().Select(text => text.Text).ToArray();
        messages.ShouldNotContain("Message 1");
        messages.ShouldNotContain("Message 2");
        messages.ShouldContain("Message 3");
        messages.ShouldContain("Message 4");
    }

    [AvaloniaFact]
    public void Overlay_visible_toggles_and_dark_scrim_applies()
    {
        var overlay = new Overlay { DarkBackground = true, Content = new TextBlock { Text = "Loading" } };
        new Window { Width = 300, Height = 200, Content = overlay }.Show();
        Dispatcher.UIThread.RunJobs();

        overlay.IsVisible.ShouldBeFalse();

        overlay.Visible = true;
        Dispatcher.UIThread.RunJobs();
        overlay.IsVisible.ShouldBeTrue();
        overlay.ApplyTemplate();

        var scrim = overlay.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Scrim");
        scrim.Focusable.ShouldBeTrue();
        scrim.ZIndex.ShouldBe(LoamZIndex.Default.Dialog);
        ((ISolidColorBrush)scrim.Background!).Color.A.ShouldBe((byte)0x52);
        AutomationProperties.GetHelpText(overlay).ShouldBe("Visible, manual close");
    }

    [AvaloniaFact]
    public void Overlay_escape_autoclose_hides_and_invokes_callback()
    {
        var clicked = false;
        var overlay = new Overlay
        {
            Visible = true,
            AutoClose = true,
            OnClick = () => clicked = true,
            Content = new TextBlock { Text = "Working" },
        };
        new Window { Width = 300, Height = 200, Content = overlay }.Show();
        Dispatcher.UIThread.RunJobs();

        var key = KeyArgs(Key.Escape);
        overlay.RaiseEvent(key);
        Dispatcher.UIThread.RunJobs();

        clicked.ShouldBeTrue();
        key.Handled.ShouldBeTrue();
        overlay.Visible.ShouldBeFalse();
        overlay.IsVisible.ShouldBeFalse();
        AutomationProperties.GetHelpText(overlay).ShouldBe("Hidden, auto-close");
    }

    [AvaloniaFact]
    public void Overlay_manual_and_disabled_states_suppress_escape_autoclose()
    {
        var manualClicked = false;
        var manual = new Overlay
        {
            Visible = true,
            AutoClose = false,
            OnClick = () => manualClicked = true,
            Content = new TextBlock { Text = "Manual" },
        };
        new Window { Width = 300, Height = 200, Content = manual }.Show();
        Dispatcher.UIThread.RunJobs();

        var manualKey = KeyArgs(Key.Escape);
        manual.RaiseEvent(manualKey);
        Dispatcher.UIThread.RunJobs();

        manualKey.Handled.ShouldBeFalse();
        manualClicked.ShouldBeFalse();
        manual.Visible.ShouldBeTrue();
        AutomationProperties.GetHelpText(manual).ShouldBe("Visible, manual close");

        var disabledClicked = false;
        var disabled = new Overlay
        {
            Visible = true,
            AutoClose = true,
            IsEnabled = false,
            OnClick = () => disabledClicked = true,
            Content = new TextBlock { Text = "Disabled" },
        };
        new Window { Width = 300, Height = 200, Content = disabled }.Show();
        Dispatcher.UIThread.RunJobs();

        var disabledKey = KeyArgs(Key.Escape);
        disabled.RaiseEvent(disabledKey);
        Dispatcher.UIThread.RunJobs();

        disabledKey.Handled.ShouldBeFalse();
        disabledClicked.ShouldBeFalse();
        disabled.Visible.ShouldBeTrue();
        AutomationProperties.GetHelpText(disabled).ShouldBe("Visible, auto-close");
    }

    [AvaloniaFact]
    public async Task Dialog_escape_cancels_and_removes_overlay()
    {
        var layer = ShowAnchor(out var anchor);
        var service = DialogService.For(anchor);

        var task = service.ShowAsync("Title", _ => new Border());
        Dispatcher.UIThread.RunJobs();

        var root = layer.Children.OfType<Panel>().Single();
        root.Focusable.ShouldBeTrue();
        root.ZIndex.ShouldBe(LoamZIndex.Default.Dialog);
        AutomationProperties.GetName(root).ShouldBe("Title");
        AutomationProperties.GetHelpText(root).ShouldBe("Modal layer, Escape dismissible");
        var dialog = layer.GetVisualDescendants().OfType<Paper>().Single();
        AutomationProperties.GetName(dialog).ShouldBe("Title");
        AutomationProperties.GetHelpText(dialog).ShouldBe("Modal dialog, Escape dismissible");
        var scrim = layer.GetVisualDescendants().OfType<Border>()
            .Single(border => AutomationProperties.GetName(border) == "Dialog backdrop");
        AutomationProperties.GetHelpText(scrim).ShouldBe("Click to dismiss");
        root.RaiseEvent(KeyArgs(Key.Escape));
        Dispatcher.UIThread.RunJobs();

        var result = await task;
        result.Canceled.ShouldBeTrue();
        layer.GetVisualDescendants().OfType<Paper>().ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void Popover_open_shows_content()
    {
        var anchor = new Border { Width = 50, Height = 20 };
        var popover = new Popover { Target = anchor, Content = new TextBlock { Text = "Popover body" } };
        var window = new Window { Width = 300, Height = 200, Content = new StackPanel { Children = { anchor, popover } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        popover.Open.ShouldBeFalse();
        AutomationProperties.GetHelpText(popover).ShouldBe("Closed, Bottom placement");

        popover.Open = true;
        Dispatcher.UIThread.RunJobs();

        window.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "Popover body").ShouldBeTrue();
        AutomationProperties.GetHelpText(popover).ShouldBe("Open, Bottom placement");
    }

    [AvaloniaFact]
    public void Popover_trigger_toggles_open_state()
    {
        var trigger = new Loam.Controls.Button { Content = "Toggle" };
        var popover = new Popover
        {
            Trigger = trigger,
            Content = new TextBlock { Text = "Triggered body" },
        };
        var window = new Window { Width = 300, Height = 200, Content = new StackPanel { Children = { trigger, popover } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        trigger.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        popover.Open.ShouldBeTrue();
        window.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "Triggered body").ShouldBeTrue();
        AutomationProperties.GetHelpText(popover).ShouldBe("Open, Bottom placement");

        trigger.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        popover.Open.ShouldBeFalse();
        AutomationProperties.GetHelpText(popover).ShouldBe("Closed, Bottom placement");
    }

    [AvaloniaFact]
    public void Popover_initial_open_shows_content_after_attach()
    {
        var trigger = new Loam.Controls.Button { Content = "Toggle" };
        var popover = new Popover
        {
            Trigger = trigger,
            Open = true,
            Content = new TextBlock { Text = "Initially open body" },
        };
        var window = new Window { Width = 300, Height = 200, Content = new StackPanel { Children = { trigger, popover } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        popover.Open.ShouldBeTrue();
        window.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "Initially open body").ShouldBeTrue();
        AutomationProperties.GetHelpText(popover).ShouldBe("Open, Bottom placement");
    }

    [AvaloniaFact]
    public void Popover_escape_closes()
    {
        var anchor = new Border { Width = 50, Height = 20 };
        var popover = new Popover { Target = anchor, Content = new TextBlock { Text = "Popover body" } };
        var window = new Window { Width = 300, Height = 200, Content = new StackPanel { Children = { anchor, popover } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        popover.Open = true;
        Dispatcher.UIThread.RunJobs();

        var paper = window.GetVisualDescendants().OfType<Paper>().First();
        paper.ZIndex.ShouldBe(LoamZIndex.Default.Popover);
        paper.RaiseEvent(KeyArgs(Key.Escape));
        Dispatcher.UIThread.RunJobs();

        popover.Open.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Popover_disabled_trigger_and_disabled_control_do_not_open()
    {
        var trigger = new Loam.Controls.Button { Content = "Toggle", IsEnabled = false };
        var popover = new Popover
        {
            Trigger = trigger,
            Content = new TextBlock { Text = "Disabled body" },
        };
        var window = new Window { Width = 300, Height = 200, Content = new StackPanel { Children = { trigger, popover } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        trigger.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        popover.Open.ShouldBeFalse();

        popover.IsEnabled = false;
        popover.Open = true;
        Dispatcher.UIThread.RunJobs();

        popover.Open.ShouldBeFalse();
        AutomationProperties.GetHelpText(popover).ShouldBe("Closed, Bottom placement");
        window.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "Disabled body").ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Popover_trigger_reattaches_after_visual_tree_reentry()
    {
        var trigger = new Loam.Controls.Button { Content = "Toggle" };
        var popover = new Popover
        {
            Trigger = trigger,
            Content = new TextBlock { Text = "Reattached body" },
        };
        var host = new StackPanel { Children = { trigger, popover } };
        var window = new Window { Width = 300, Height = 200, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        window.Content = null;
        Dispatcher.UIThread.RunJobs();
        window.Content = host;
        Dispatcher.UIThread.RunJobs();

        trigger.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        popover.Open.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Snackbar_escape_dismisses_toast()
    {
        var layer = ShowAnchor(out var anchor);
        var snackbar = SnackbarService.For(anchor);

        snackbar.Add(new SnackbarOptions("Saved") { Duration = Timeout.InfiniteTimeSpan });
        Dispatcher.UIThread.RunJobs();

        var toast = layer.GetVisualDescendants().OfType<Border>()
            .Single(border => AutomationProperties.GetName(border) == "Saved");
        toast.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(toast).ShouldBe("Saved");
        toast.RaiseEvent(KeyArgs(Key.Escape));
        Dispatcher.UIThread.RunJobs();

        layer.GetVisualDescendants().OfType<Border>()
            .Where(border => AutomationProperties.GetName(border) == "Saved")
            .ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void Tooltip_sets_help_text_and_loam_surface()
    {
        var button = new Avalonia.Controls.Button { Content = "Info" };

        Tooltip.Set(button, "More detail");

        AutomationProperties.GetHelpText(button).ShouldBe("More detail");
        var tip = ToolTip.GetTip(button).ShouldBeOfType<Paper>();
        tip.ZIndex.ShouldBe(LoamZIndex.Default.Tooltip);
    }

    [AvaloniaFact]
    public void Tooltip_options_set_title_surface_and_help_text()
    {
        var button = new Avalonia.Controls.Button { Content = "Info" };

        Tooltip.Set(button, "More detail", new TooltipOptions
        {
            Title = "Details",
            Elevation = 2,
            Padding = new Thickness(12, 8),
            Color = LoamColor.Secondary,
            HelpText = "Detailed help",
        });

        AutomationProperties.GetHelpText(button).ShouldBe("Detailed help");
        var tip = ToolTip.GetTip(button).ShouldBeOfType<Paper>();
        tip.Elevation.ShouldBe(2);
        tip.Padding.ShouldBe(new Thickness(12, 8));
        var texts = tip.Content.ShouldBeOfType<StackPanel>().Children.OfType<Text>().Select(text => text.Text).ToArray();
        texts.ShouldContain("Details");
        texts.ShouldContain("More detail");
    }

    [AvaloniaFact]
    public void Tooltip_options_wire_attached_behavior_and_clear_removes_tip()
    {
        var button = new Avalonia.Controls.Button { Content = "Info" };

        Tooltip.Set(button, "Placed detail", new TooltipOptions
        {
            Placement = PlacementMode.BottomEdgeAlignedLeft,
            HorizontalOffset = 4,
            VerticalOffset = 8,
            ShowDelay = 125,
            BetweenShowDelay = 60,
            ShowOnDisabled = true,
            ServiceEnabled = false,
            HelpText = "Placed help",
        });

        AutomationProperties.GetHelpText(button).ShouldBe("Placed help");
        ToolTip.GetTip(button).ShouldBeOfType<Paper>();
        ToolTip.GetPlacement(button).ShouldBe(PlacementMode.BottomEdgeAlignedLeft);
        ToolTip.GetHorizontalOffset(button).ShouldBe(4);
        ToolTip.GetVerticalOffset(button).ShouldBe(8);
        ToolTip.GetShowDelay(button).ShouldBe(125);
        ToolTip.GetBetweenShowDelay(button).ShouldBe(60);
        ToolTip.GetShowOnDisabled(button).ShouldBeTrue();
        ToolTip.GetServiceEnabled(button).ShouldBeFalse();
        ToolTip.GetIsOpen(button).ShouldBeFalse();

        Tooltip.Clear(button);

        ToolTip.GetTip(button).ShouldBeNull();
        AutomationProperties.GetHelpText(button).ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task Dialog_options_control_escape_padding_and_max_width()
    {
        var layer = ShowAnchor(out var anchor);
        var service = DialogService.For(anchor);
        DialogInstance? instance = null;

        var task = service.ShowAsync("Options", dialog =>
        {
            instance = dialog;
            return new Border();
        }, new DialogOptions
        {
            DismissOnEscape = false,
            MaxWidth = 320,
            MinWidth = 240,
            MaxHeight = 360,
            Margin = new Thickness(16),
            Padding = new Thickness(12),
            AutoFocus = false,
        });
        Dispatcher.UIThread.RunJobs();

        var root = layer.Children.OfType<Panel>().Single();
        AutomationProperties.GetHelpText(root).ShouldBe("Modal layer, Escape disabled");
        root.RaiseEvent(KeyArgs(Key.Escape));
        Dispatcher.UIThread.RunJobs();
        task.IsCompleted.ShouldBeFalse();

        var paper = layer.GetVisualDescendants().OfType<Paper>().Single();
        AutomationProperties.GetHelpText(paper).ShouldBe("Modal dialog, Escape disabled");
        paper.MaxWidth.ShouldBe(320);
        paper.MinWidth.ShouldBe(240);
        paper.MaxHeight.ShouldBe(360);
        paper.Margin.ShouldBe(new Thickness(16));
        paper.Padding.ShouldBe(new Thickness(12));

        instance!.Cancel();
        (await task).Canceled.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Menu_items_support_disabled_dividers_shortcuts_and_public_open_close()
    {
        var invoked = false;
        var menu = new Loam.Controls.Menu
        {
            Content = "Actions",
            MenuWidth = 220,
            Items =
            {
                new Loam.Controls.MenuItem { Text = "Archive", Icon = Icons.Material.Filled.Check, ShortcutText = "A", OnClick = () => invoked = true },
                new Loam.Controls.MenuItem { IsDivider = true },
                new Loam.Controls.MenuItem { Text = "Disabled", Icon = Icons.Material.Filled.Delete, IsEnabled = false, OnClick = () => invoked = true },
            },
        };
        var window = new Window { Width = 400, Height = 300, Content = menu };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        menu.OpenMenu();
        Dispatcher.UIThread.RunJobs();

        var rows = window.GetVisualDescendants().OfType<ListItem>().ToArray();
        rows.Length.ShouldBe(2);
        rows[0].SecondaryText.ShouldBeNull();
        rows[0].Action.ShouldBeOfType<Text>().Text.ShouldBe("A");
        rows[0].MinWidth.ShouldBe(204);
        rows[1].IsEnabled.ShouldBeFalse();
        rows[1].Focusable.ShouldBeFalse();
        AutomationProperties.GetHelpText(menu).ShouldBe("Open menu");
        AutomationProperties.GetHelpText(rows[1]).ShouldBe("Disabled menu item");
        window.GetVisualDescendants().OfType<Divider>().ShouldNotBeEmpty();

        rows[1].RaiseEvent(KeyArgs(Key.Enter));
        invoked.ShouldBeFalse();

        rows[0].RaiseEvent(KeyArgs(Key.Enter));
        invoked.ShouldBeTrue();
        menu.CloseMenu();
    }

    [AvaloniaFact]
    public void Menu_keyboard_navigation_skips_disabled_rows_and_escape_closes()
    {
        var menu = new Loam.Controls.Menu
        {
            Content = "Actions",
            CloseOnItemClick = false,
            Items =
            {
                new Loam.Controls.MenuItem { Text = "Archive", Icon = Icons.Material.Filled.Check },
                new Loam.Controls.MenuItem { Text = "Disabled", Icon = Icons.Material.Filled.Delete, IsEnabled = false },
                new Loam.Controls.MenuItem { Text = "Delete", Icon = Icons.Material.Filled.Delete },
            },
        };
        var window = new Window { Width = 400, Height = 300, Content = menu };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        menu.OpenMenu();
        Dispatcher.UIThread.RunJobs();

        var rows = window.GetVisualDescendants().OfType<ListItem>().ToArray();
        rows.Length.ShouldBe(3);
        window.FocusManager?.GetFocusedElement().ShouldBe(rows[0]);

        var next = KeyArgs(Key.Down);
        rows[0].RaiseEvent(next);
        Dispatcher.UIThread.RunJobs();
        next.Handled.ShouldBeTrue();
        window.FocusManager?.GetFocusedElement().ShouldBe(rows[2]);

        var previous = KeyArgs(Key.Up);
        rows[2].RaiseEvent(previous);
        Dispatcher.UIThread.RunJobs();
        previous.Handled.ShouldBeTrue();
        window.FocusManager?.GetFocusedElement().ShouldBe(rows[0]);

        var close = KeyArgs(Key.Escape);
        rows[0].RaiseEvent(close);
        Dispatcher.UIThread.RunJobs();
        close.Handled.ShouldBeTrue();
        AutomationProperties.GetHelpText(menu).ShouldBe("Closed menu");
    }

    [AvaloniaFact]
    public void Menu_disabled_trigger_does_not_open()
    {
        var menu = new Loam.Controls.Menu
        {
            Content = "Disabled actions",
            IsEnabled = false,
            Items =
            {
                new Loam.Controls.MenuItem { Text = "Archive", Icon = Icons.Material.Filled.Check },
            },
        };
        var window = new Window { Width = 400, Height = 300, Content = menu };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        menu.OpenMenu();
        Dispatcher.UIThread.RunJobs();

        window.GetVisualDescendants().OfType<ListItem>().ShouldBeEmpty();
        AutomationProperties.GetHelpText(menu).ShouldBe("Closed menu");
    }
}
