using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
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

        var ok = layer.GetVisualDescendants().OfType<Loam.Controls.Button>().First(b => (string?)b.Content == "OK");
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
        layer.GetVisualDescendants().OfType<Loam.Controls.Button>().First(b => (string?)b.Content == "Yes")
            .RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
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

        layer.GetVisualDescendants().OfType<Alert>().ShouldNotBeEmpty();
    }

    [AvaloniaFact]
    public void Snackbar_action_invokes_callback_and_dismisses_toast()
    {
        var layer = ShowAnchor(out var anchor);
        var snackbar = SnackbarService.For(anchor);
        var invoked = false;

        snackbar.Add(new SnackbarOptions("Archived")
        {
            Severity = LoamColor.Info,
            ActionText = "Undo",
            Action = () => invoked = true,
            Duration = Timeout.InfiniteTimeSpan,
        });
        Dispatcher.UIThread.RunJobs();

        var action = layer.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .First(button => (string?)button.Content == "Undo");
        action.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        invoked.ShouldBeTrue();
        layer.GetVisualDescendants().OfType<Alert>().ShouldBeEmpty();
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

        layer.GetVisualDescendants().OfType<Alert>().Count().ShouldBe(2);
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
        ((ISolidColorBrush)scrim.Background!).Color.A.ShouldBe((byte)0x99);
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

        popover.Open = true;
        Dispatcher.UIThread.RunJobs();

        window.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "Popover body").ShouldBeTrue();
    }
}
