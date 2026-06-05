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

        Brush(res, LoamTokens.ColorPrimary, ThemeVariant.Light).ShouldBe(Color.Parse("#6750A4"));
        Brush(res, LoamTokens.ColorPrimary, ThemeVariant.Dark).ShouldBe(Color.Parse("#D0BCFF"));
        Brush(res, LoamTokens.ColorSurface, ThemeVariant.Light).ShouldBe(Color.Parse("#FFFBFE"));
        Brush(res, LoamTokens.ColorSurface, ThemeVariant.Dark).ShouldBe(Color.Parse("#1C1B1F"));
        Brush(res, LoamTokens.ColorSurfaceContainer, ThemeVariant.Light).ShouldBe(Color.Parse("#F3EDF7"));
        Brush(res, LoamTokens.Primary, ThemeVariant.Light).ShouldBe(Color.Parse("#6750A4"));
    }

    [Fact]
    public void Typography_layout_and_zindex_tokens_resolve()
    {
        var res = new LoamTheme().Resources;

        res.TryGetResource(LoamTokens.TypographyFontSize("H6"), ThemeVariant.Light, out var size).ShouldBeTrue();
        size.ShouldBe(24d);
        res.TryGetResource(LoamTokens.TypographyFontSize("TitleMedium"), ThemeVariant.Light, out var titleSize).ShouldBeTrue();
        titleSize.ShouldBe(16d);
        res.TryGetResource(LoamTokens.TypographyFontWeight("TitleMedium"), ThemeVariant.Light, out var weight).ShouldBeTrue();
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
        shape.ShouldBe(new Avalonia.CornerRadius(8));
        res.TryGetResource(LoamTokens.ShapeExtraSmall, ThemeVariant.Light, out var extraSmallShape).ShouldBeTrue();
        extraSmallShape.ShouldBe(new Avalonia.CornerRadius(4));
        res.TryGetResource(LoamTokens.StateHoverOpacity, ThemeVariant.Light, out var hover).ShouldBeTrue();
        hover.ShouldBe(0.08);
        res.TryGetResource(LoamTokens.StateDisabledOpacity, ThemeVariant.Light, out var disabled).ShouldBeTrue();
        disabled.ShouldBe(0.38);
        res.TryGetResource(LoamTokens.MotionDurationMedium, ThemeVariant.Light, out var duration).ShouldBeTrue();
        duration.ShouldBe(TimeSpan.FromMilliseconds(250));
        res.TryGetResource(LoamTokens.MotionDuration(nameof(LoamMotion.Long4)), ThemeVariant.Light, out var longDuration).ShouldBeTrue();
        longDuration.ShouldBe(TimeSpan.FromMilliseconds(600));
        res.TryGetResource(LoamTokens.Spacing(nameof(LoamSpacing.Large)), ThemeVariant.Light, out var spacing).ShouldBeTrue();
        spacing.ShouldBe(16d);
        res.TryGetResource(LoamTokens.Stroke(nameof(LoamStroke.Focus)), ThemeVariant.Light, out var stroke).ShouldBeTrue();
        stroke.ShouldBe(2d);
        res.TryGetResource(LoamTokens.DensityButtonPaddingMedium, ThemeVariant.Light, out var densityPadding).ShouldBeTrue();
        densityPadding.ShouldBe(new Avalonia.Thickness(24, 10));
        res.TryGetResource(LoamTokens.DensityInteractiveMedium, ThemeVariant.Light, out var densityTarget).ShouldBeTrue();
        densityTarget.ShouldBe(40d);
        res.TryGetResource(LoamTokens.ElevationShadow(nameof(LoamElevation.Level3Shadow)), ThemeVariant.Light, out var elevation).ShouldBeTrue();
        elevation.ShouldBe(6);
        res.TryGetResource(LoamTokens.FieldOutlinedHeight, ThemeVariant.Light, out var height).ShouldBeTrue();
        height.ShouldBe(56d);
        res.TryGetResource(LoamTokens.FieldOutlinedPadding, ThemeVariant.Light, out var padding).ShouldBeTrue();
        padding.ShouldBe(new Avalonia.Thickness(16, 16));
    }

    [Fact]
    public void Role_pairs_meet_text_contrast_baseline()
    {
        var pairs = new (Color Foreground, Color Background)[]
        {
            (LoamColorScheme.DefaultLight.OnPrimary, LoamColorScheme.DefaultLight.Primary),
            (LoamColorScheme.DefaultLight.OnPrimaryContainer, LoamColorScheme.DefaultLight.PrimaryContainer),
            (LoamColorScheme.DefaultLight.OnSecondary, LoamColorScheme.DefaultLight.Secondary),
            (LoamColorScheme.DefaultLight.OnTertiary, LoamColorScheme.DefaultLight.Tertiary),
            (LoamColorScheme.DefaultLight.OnError, LoamColorScheme.DefaultLight.Error),
            (LoamColorScheme.DefaultLight.OnSurface, LoamColorScheme.DefaultLight.Surface),
            (LoamColorScheme.DefaultDark.OnPrimary, LoamColorScheme.DefaultDark.Primary),
            (LoamColorScheme.DefaultDark.OnPrimaryContainer, LoamColorScheme.DefaultDark.PrimaryContainer),
            (LoamColorScheme.DefaultDark.OnSecondary, LoamColorScheme.DefaultDark.Secondary),
            (LoamColorScheme.DefaultDark.OnTertiary, LoamColorScheme.DefaultDark.Tertiary),
            (LoamColorScheme.DefaultDark.OnError, LoamColorScheme.DefaultDark.Error),
            (LoamColorScheme.DefaultDark.OnSurface, LoamColorScheme.DefaultDark.Surface),
        };

        foreach (var (foreground, background) in pairs)
        {
            LoamColors.ContrastRatio(foreground, background).ShouldBeGreaterThan(4.5);
        }
    }

    [Fact]
    public void Custom_state_and_field_tokens_flow_into_resources()
    {
        var theme = new LoamTheme(LoamThemeData.Default with
        {
            StateLayer = LoamStateLayer.Default with { HoverOpacity = 0.2, DisabledOpacity = 0.6 },
            FieldMetrics = LoamFieldMetrics.Default with { OutlinedHeight = 60 },
            Spacing = LoamSpacing.Default with { Large = 18 },
            Density = LoamDensity.Default with { InteractiveMedium = 44 },
        });

        theme.Resources.TryGetResource(LoamTokens.StateHoverOpacity, ThemeVariant.Light, out var hover).ShouldBeTrue();
        hover.ShouldBe(0.2);
        theme.Resources.TryGetResource(LoamTokens.StateDisabledOpacity, ThemeVariant.Light, out var disabled).ShouldBeTrue();
        disabled.ShouldBe(0.6);
        theme.Resources.TryGetResource(LoamTokens.FieldOutlinedHeight, ThemeVariant.Light, out var height).ShouldBeTrue();
        height.ShouldBe(60d);
        theme.Resources.TryGetResource(LoamTokens.Spacing(nameof(LoamSpacing.Large)), ThemeVariant.Light, out var spacing).ShouldBeTrue();
        spacing.ShouldBe(18d);
        theme.Resources.TryGetResource(LoamTokens.DensityInteractiveMedium, ThemeVariant.Light, out var density).ShouldBeTrue();
        density.ShouldBe(44d);
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
        theme.Resources.TryGetResource(LoamTokens.ColorPrimary, ThemeVariant.Dark, out var dark).ShouldBeTrue();
        ((ISolidColorBrush)dark!).Color.ShouldBe(Colors.Red.Lighten(0.35));
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

    [Fact]
    public void Legacy_theme_preset_retains_migration_palette()
    {
        var theme = new LoamTheme(LoamThemeData.Legacy);

        Brush(theme.Resources, LoamTokens.Primary, ThemeVariant.Light).ShouldBe(Color.Parse("#594AE2"));
        Brush(theme.Resources, LoamTokens.Surface, ThemeVariant.Dark).ShouldBe(Color.Parse("#373740"));
    }
}
