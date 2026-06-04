using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

// Projection is verified directly against the theme's resource dictionary. End-to-end resolution
// through a live control tree is covered by SurfaceThemeTests.
public class ThemingTests
{
    private static Color Brush(IResourceDictionary res, string key, ThemeVariant variant)
    {
        res.TryGetResource(key, variant, out var value).ShouldBeTrue();
        return ((ISolidColorBrush)value!).Color;
    }

    [Fact]
    public void Palette_tokens_resolve_per_variant()
    {
        var res = new LoamTheme().Resources;

        Brush(res, LoamTokens.Primary, ThemeVariant.Light).ShouldBe(Color.Parse("#594AE2"));
        Brush(res, LoamTokens.Primary, ThemeVariant.Dark).ShouldBe(Color.Parse("#776BE7"));
        Brush(res, LoamTokens.Surface, ThemeVariant.Light).ShouldBe(Colors.White);
        Brush(res, LoamTokens.Surface, ThemeVariant.Dark).ShouldBe(Color.Parse("#373740"));
        Brush(res, LoamTokens.Background, ThemeVariant.Dark).ShouldBe(Color.Parse("#32333D"));
    }

    [Fact]
    public void Typography_layout_and_zindex_tokens_resolve()
    {
        var res = new LoamTheme().Resources;

        res.TryGetResource(LoamTokens.TypographyFontSize("H6"), ThemeVariant.Light, out var size).ShouldBeTrue();
        size.ShouldBe(20d);
        res.TryGetResource(LoamTokens.TypographyFontWeight("H6"), ThemeVariant.Light, out var weight).ShouldBeTrue();
        weight.ShouldBe(FontWeight.Medium);
        res.TryGetResource(LoamTokens.DefaultCornerRadius, ThemeVariant.Light, out var radius).ShouldBeTrue();
        radius.ShouldBe(new Avalonia.CornerRadius(4));
        res.TryGetResource(LoamTokens.ZIndex(nameof(LoamZIndex.Dialog)), ThemeVariant.Light, out var z).ShouldBeTrue();
        z.ShouldBe(1400);
    }

    [Fact]
    public void Shape_state_motion_and_field_tokens_resolve()
    {
        var theme = new LoamTheme();
        var res = theme.Resources;

        res.TryGetResource(LoamTokens.ShapeSmall, ThemeVariant.Light, out var shape).ShouldBeTrue();
        shape.ShouldBe(new Avalonia.CornerRadius(4));
        res.TryGetResource(LoamTokens.StateHoverOpacity, ThemeVariant.Light, out var hover).ShouldBeTrue();
        hover.ShouldBe(0.08);
        res.TryGetResource(LoamTokens.StateDisabledOpacity, ThemeVariant.Light, out var disabled).ShouldBeTrue();
        disabled.ShouldBe(0.38);
        res.TryGetResource(LoamTokens.MotionDurationMedium, ThemeVariant.Light, out var duration).ShouldBeTrue();
        duration.ShouldBe(TimeSpan.FromMilliseconds(180));
        res.TryGetResource(LoamTokens.FieldOutlinedHeight, ThemeVariant.Light, out var height).ShouldBeTrue();
        height.ShouldBe(52d);
        res.TryGetResource(LoamTokens.FieldOutlinedPadding, ThemeVariant.Light, out var padding).ShouldBeTrue();
        padding.ShouldBe(new Avalonia.Thickness(12, 14));
    }

    [Fact]
    public void Custom_state_and_field_tokens_flow_into_resources()
    {
        var theme = new LoamTheme(LoamThemeData.Default with
        {
            StateLayer = LoamStateLayer.Default with { HoverOpacity = 0.2, DisabledOpacity = 0.6 },
            FieldMetrics = LoamFieldMetrics.Default with { OutlinedHeight = 60 },
        });

        theme.Resources.TryGetResource(LoamTokens.StateHoverOpacity, ThemeVariant.Light, out var hover).ShouldBeTrue();
        hover.ShouldBe(0.2);
        theme.Resources.TryGetResource(LoamTokens.StateDisabledOpacity, ThemeVariant.Light, out var disabled).ShouldBeTrue();
        disabled.ShouldBe(0.6);
        theme.Resources.TryGetResource(LoamTokens.FieldOutlinedHeight, ThemeVariant.Light, out var height).ShouldBeTrue();
        height.ShouldBe(60d);
        Brush(theme.Resources, LoamTokens.PaletteHover(nameof(LoamPalette.Primary)), ThemeVariant.Light).A.ShouldBe((byte)51);
    }

    [Fact]
    public void Elevation_tokens_resolve()
    {
        var res = new LoamTheme().Resources;

        res.TryGetResource(LoamTokens.Elevation(0), ThemeVariant.Light, out var e0).ShouldBeTrue();
        ((BoxShadows)e0!).Count.ShouldBe(0);
        res.TryGetResource(LoamTokens.Elevation(1), ThemeVariant.Light, out var e1).ShouldBeTrue();
        ((BoxShadows)e1!).Count.ShouldBe(3);
    }

    [Fact]
    public void SetPrimary_updates_primary_token_at_runtime()
    {
        var theme = new LoamTheme();

        theme.SetPrimary(Colors.Red);

        theme.Resources.TryGetResource(LoamTokens.Primary, ThemeVariant.Light, out var light).ShouldBeTrue();
        ((ISolidColorBrush)light!).Color.ShouldBe(Colors.Red);
        theme.Resources.TryGetResource(LoamTokens.Primary, ThemeVariant.Dark, out var dark).ShouldBeTrue();
        ((ISolidColorBrush)dark!).Color.ShouldBe(Colors.Red);
    }

    [Fact]
    public void Custom_theme_data_flows_into_resources()
    {
        var data = LoamThemeData.Default with
        {
            PaletteLight = LoamPalette.DefaultLight with { Primary = Color.Parse("#112233") },
        };
        var theme = new LoamTheme(data);

        theme.Resources.TryGetResource(LoamTokens.Primary, ThemeVariant.Light, out var v).ShouldBeTrue();
        ((ISolidColorBrush)v!).Color.ShouldBe(Color.Parse("#112233"));
    }
}
