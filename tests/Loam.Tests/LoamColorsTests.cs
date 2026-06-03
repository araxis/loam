using Avalonia.Media;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class LoamColorsTests
{
    [Fact]
    public void BlackAlpha_sets_expected_alpha()
    {
        LoamColors.BlackAlpha(0.5).ShouldBe(Color.FromArgb(128, 0, 0, 0));
    }

    [Fact]
    public void WithAlpha_replaces_only_alpha()
    {
        var faded = Colors.Red.WithAlpha(0.5);
        faded.A.ShouldBe((byte)128);
        faded.R.ShouldBe((byte)255);
    }

    [Fact]
    public void ContrastText_picks_readable_foreground()
    {
        Colors.White.ContrastText().ShouldBe(Color.FromArgb(0xDE, 0, 0, 0));
        Colors.Black.ContrastText().ShouldBe(Colors.White);
    }

    [Fact]
    public void Lighten_and_darken_move_toward_white_and_black()
    {
        Color.Parse("#000000").Lighten(0.5).R.ShouldBe((byte)128);
        Color.Parse("#FFFFFF").Darken(0.5).R.ShouldBe((byte)128);
    }
}
