using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Loam.Controls;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class CommandPaletteTests
{
    private static void Show(Control content)
    {
        new Window { Width = 520, Height = 480, Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static KeyEventArgs KeyDown(Key key) => new() { RoutedEvent = InputElement.KeyDownEvent, Key = key };

    [Fact]
    public void Filter_matches_title_and_keywords_case_insensitively()
    {
        var items = new[]
        {
            new CommandPaletteItem { Title = "Open Settings", Keywords = ["prefs"] },
            new CommandPaletteItem { Title = "New File" },
            new CommandPaletteItem { Title = "Toggle Theme", Keywords = ["dark", "light"] },
        };

        CommandPalette.Filter(items, "").Count.ShouldBe(3);
        CommandPalette.Filter(items, "set").Select(c => c.Title).ShouldBe(["Open Settings"]);
        CommandPalette.Filter(items, "DARK").Select(c => c.Title).ShouldBe(["Toggle Theme"]);
        CommandPalette.Filter(items, "prefs").Single().Title.ShouldBe("Open Settings");
        CommandPalette.Filter(items, "zzz").ShouldBeEmpty();
    }

    [AvaloniaFact]
    public void Setting_filter_text_updates_results_and_resets_selection()
    {
        var palette = new CommandPalette
        {
            Commands = { new() { Title = "Alpha" }, new() { Title = "Beta" }, new() { Title = "Brave" } },
        };
        Show(palette);

        palette.FilterText = "br";
        Dispatcher.UIThread.RunJobs();

        palette.FilteredCommands.Select(c => c.Title).ShouldBe(["Brave"]);
        palette.SelectedIndex.ShouldBe(0);
    }

    [AvaloniaFact]
    public void Keyboard_navigation_invokes_and_closes()
    {
        string? invoked = null;
        var closed = false;
        var palette = new CommandPalette
        {
            Commands =
            {
                new() { Title = "Alpha", OnInvoke = () => invoked = "Alpha" },
                new() { Title = "Beta", OnInvoke = () => invoked = "Beta" },
                new() { Title = "Gamma", OnInvoke = () => invoked = "Gamma" },
            },
        };
        palette.Closed += (_, _) => closed = true;
        Show(palette);

        palette.SelectedIndex.ShouldBe(0);

        palette.RaiseEvent(KeyDown(Key.Down));
        palette.SelectedIndex.ShouldBe(1);

        palette.RaiseEvent(KeyDown(Key.Enter));
        invoked.ShouldBe("Beta");

        palette.RaiseEvent(KeyDown(Key.Escape));
        closed.ShouldBeTrue();
        palette.IsOpen.ShouldBeFalse();
    }
}
