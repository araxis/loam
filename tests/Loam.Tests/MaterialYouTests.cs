using Avalonia.Media;
using Avalonia.Styling;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

// Phase 2 — Material You. Verifies one seed color produces a complete, accessible light + dark scheme.
public class MaterialYouTests
{
    private static readonly Color[] Seeds =
    [
        Color.Parse("#6750A4"), // default purple
        Color.Parse("#006A6A"), // teal
        Color.Parse("#B3261E"), // red
        Color.Parse("#1565C0"), // blue
        Color.Parse("#386A20"), // green
        Color.Parse("#FF9800"), // orange
    ];

    private static readonly (string On, string Background)[] ContrastPairs =
    [
        ("OnPrimary", "Primary"),
        ("OnSecondary", "Secondary"),
        ("OnTertiary", "Tertiary"),
        ("OnError", "Error"),
        ("OnPrimaryContainer", "PrimaryContainer"),
        ("OnSecondaryContainer", "SecondaryContainer"),
        ("OnTertiaryContainer", "TertiaryContainer"),
        ("OnErrorContainer", "ErrorContainer"),
        ("OnBackground", "Background"),
        ("OnSurface", "Surface"),
        ("OnSurfaceVariant", "SurfaceVariant"),
    ];

    [Fact]
    public void FromSeed_produces_accessible_text_pairs_for_every_seed_in_both_variants()
    {
        foreach (var seed in Seeds)
        {
            foreach (var dark in new[] { false, true })
            {
                var scheme = LoamColorScheme.FromSeed(seed, dark);
                foreach (var (on, background) in ContrastPairs)
                {
                    LoamColors.ContrastRatio(Role(scheme, on), Role(scheme, background))
                        .ShouldBeGreaterThanOrEqualTo(4.5, $"{on} on {background} (seed #{seed.ToString()[3..]}, dark={dark})");
                }
            }
        }
    }

    [Fact]
    public void FromSeed_orders_tones_by_variant()
    {
        var light = LoamColorScheme.FromSeed(Color.Parse("#1565C0"), dark: false);
        var dark = LoamColorScheme.FromSeed(Color.Parse("#1565C0"), dark: true);

        // Light primary is tone 40, dark primary tone 80 -> dark primary is lighter.
        dark.Primary.RelativeLuminance().ShouldBeGreaterThan(light.Primary.RelativeLuminance());
        // Dark surface is darker than light surface.
        dark.Surface.RelativeLuminance().ShouldBeLessThan(light.Surface.RelativeLuminance());
    }

    [Fact]
    public void Tonal_palette_tracks_lightness_and_clamps_extremes_to_gamut()
    {
        var palette = LoamTonalPalette.FromColor(Color.Parse("#1565C0"));

        palette.Tone(20).RelativeLuminance().ShouldBeLessThan(palette.Tone(80).RelativeLuminance());
        // Tone extremes resolve to (near) black/white regardless of the palette's chroma.
        palette.Tone(0).RelativeLuminance().ShouldBeLessThan(0.02);
        palette.Tone(100).RelativeLuminance().ShouldBeGreaterThan(0.9);
    }

    [Fact]
    public void ThemeData_from_seed_sets_both_schemes()
    {
        var seed = Color.Parse("#006A6A");
        var data = LoamThemeData.FromSeed(seed);

        data.ColorSchemeLight.Primary.ShouldBe(LoamColorScheme.FromSeed(seed, dark: false).Primary);
        data.ColorSchemeDark.Primary.ShouldBe(LoamColorScheme.FromSeed(seed, dark: true).Primary);
    }

    [Fact]
    public void SetSeed_updates_scheme_and_accent_tokens_at_runtime()
    {
        var theme = new LoamTheme();
        var teal = Color.Parse("#006A6A");

        theme.SetSeed(teal);

        var expected = LoamColorScheme.FromSeed(teal, dark: false).Primary;

        theme.Resources.TryGetResource(LoamTokens.ColorPrimary, ThemeVariant.Light, out var primary).ShouldBeTrue();
        ((ISolidColorBrush)primary!).Color.ShouldBe(expected);

        // The Fluent accent bridge follows the seed too.
        theme.Resources.TryGetResource("SystemAccentColor", ThemeVariant.Light, out var accent).ShouldBeTrue();
        ((Color)accent!).ShouldBe(expected);
    }

    [Fact]
    public void Standard_contrast_overload_matches_two_arg_default()
    {
        var seed = Color.Parse("#1565C0");
        foreach (var dark in new[] { false, true })
        {
            LoamColorScheme.FromSeed(seed, dark, LoamContrast.Standard)
                .ShouldBe(LoamColorScheme.FromSeed(seed, dark));
        }
    }

    [Fact]
    public void High_contrast_increases_separation_over_standard()
    {
        foreach (var dark in new[] { false, true })
        {
            var std = LoamColorScheme.FromSeed(Color.Parse("#1565C0"), dark, LoamContrast.Standard);
            var high = LoamColorScheme.FromSeed(Color.Parse("#1565C0"), dark, LoamContrast.High);

            LoamColors.ContrastRatio(high.OnSurface, high.Surface)
                .ShouldBeGreaterThan(LoamColors.ContrastRatio(std.OnSurface, std.Surface));
            LoamColors.ContrastRatio(high.OnPrimaryContainer, high.PrimaryContainer)
                .ShouldBeGreaterThan(LoamColors.ContrastRatio(std.OnPrimaryContainer, std.PrimaryContainer));
            LoamColors.ContrastRatio(high.Outline, high.Surface)
                .ShouldBeGreaterThan(LoamColors.ContrastRatio(std.Outline, std.Surface));
        }
    }

    [Fact]
    public void High_contrast_meets_aaa_on_main_text_pairs()
    {
        foreach (var seed in Seeds)
        {
            foreach (var dark in new[] { false, true })
            {
                var s = LoamColorScheme.FromSeed(seed, dark, LoamContrast.High);
                LoamColors.ContrastRatio(s.OnSurface, s.Surface).ShouldBeGreaterThanOrEqualTo(7.0);
                LoamColors.ContrastRatio(s.OnPrimary, s.Primary).ShouldBeGreaterThanOrEqualTo(7.0);
                LoamColors.ContrastRatio(s.OnSurfaceVariant, s.SurfaceVariant).ShouldBeGreaterThanOrEqualTo(7.0);
            }
        }
    }

    private static Color Role(LoamColorScheme scheme, string role) =>
        (Color)typeof(LoamColorScheme).GetProperty(role)!.GetValue(scheme)!;
}
