using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class LoamShadowsTests
{
    [Fact]
    public void Empty_css_yields_no_shadow()
    {
        LoamShadows.ParseCss(string.Empty).Count.ShouldBe(0);
    }

    [Fact]
    public void Parses_multilayer_css_shadow()
    {
        var shadows = LoamShadows.ParseCss(
            "0px 2px 1px -1px rgba(0,0,0,0.2),0px 1px 1px 0px rgba(0,0,0,0.14),0px 1px 3px 0px rgba(0,0,0,0.12)");

        shadows.Count.ShouldBe(3);
        shadows[0].OffsetX.ShouldBe(0d);
        shadows[0].OffsetY.ShouldBe(2d);
        shadows[0].Blur.ShouldBe(1d);
        shadows[0].Spread.ShouldBe(-1d);
        shadows[0].Color.A.ShouldBe((byte)51); // 0.2 * 255
    }

    [Fact]
    public void Default_set_spans_levels_0_to_25()
    {
        LoamShadows.Default.MaxElevation.ShouldBe(25);
        LoamShadows.Default[0].Count.ShouldBe(0);
        LoamShadows.Default[1].Count.ShouldBe(3);
    }

    [Fact]
    public void Default_shadows_are_soft_without_positive_spread()
    {
        var elevated = LoamShadows.Default[8];

        elevated.Count.ShouldBe(3);
        for (var i = 0; i < elevated.Count; i++)
        {
            elevated[i].Spread.ShouldBeLessThanOrEqualTo(0);
            elevated[i].Color.A.ShouldBeLessThanOrEqualTo((byte)18);
            elevated[i].Blur.ShouldBeGreaterThan(elevated[i].OffsetY);
        }
    }
}
