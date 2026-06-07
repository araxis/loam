using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

// Phase 1 — theme consistency. Verifies LoamTheme bridges its accent into Avalonia Fluent's
// SystemAccentColor system so stray base Fluent controls (no Loam ControlTheme) adopt Loam's primary
// instead of Fluent blue. Projection is checked directly; cascade/ordering is checked end-to-end
// through the live TestApp (FluentTheme layered under LoamTheme).
public class FluentBridgeTests
{
    private static Color Accent(LoamTheme theme, string key, ThemeVariant variant)
    {
        theme.Resources.TryGetResource(key, variant, out var value).ShouldBeTrue($"{key} ({variant})");
        return (Color)value!;
    }

    [Fact]
    public void Accent_color_keys_project_loam_primary_per_variant()
    {
        var theme = new LoamTheme();

        Accent(theme, "SystemAccentColor", ThemeVariant.Light).ShouldBe(LoamColorScheme.DefaultLight.Primary);
        Accent(theme, "SystemAccentColor", ThemeVariant.Dark).ShouldBe(LoamColorScheme.DefaultDark.Primary);

        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            foreach (var key in new[]
            {
                "SystemAccentColorDark1", "SystemAccentColorDark2", "SystemAccentColorDark3",
                "SystemAccentColorLight1", "SystemAccentColorLight2", "SystemAccentColorLight3",
            })
            {
                theme.Resources.TryGetResource(key, variant, out _).ShouldBeTrue($"{key} ({variant})");
            }
        }

        // Derived shades track lightness around the base accent: dark darker, light lighter.
        var baseLightness = LoamColorScheme.DefaultLight.Primary.ToHsl().L;
        Accent(theme, "SystemAccentColorDark3", ThemeVariant.Light).ToHsl().L.ShouldBeLessThan(baseLightness);
        Accent(theme, "SystemAccentColorLight3", ThemeVariant.Light).ToHsl().L.ShouldBeGreaterThan(baseLightness);
    }

    [Fact]
    public void SetPrimary_updates_fluent_accent_at_runtime()
    {
        var theme = new LoamTheme();
        var teal = Color.Parse("#00BFA5");

        theme.SetPrimary(teal);

        Accent(theme, "SystemAccentColor", ThemeVariant.Light).ShouldBe(teal);
        Accent(theme, "SystemAccentColor", ThemeVariant.Dark).ShouldBe(teal.Lighten(0.35));
    }

    [AvaloniaFact]
    public void Stray_fluent_accent_brush_resolves_to_loam_primary()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;

        // LoamTheme's override wins over FluentTheme's accent provider (ordering): the effective
        // accent is no longer Fluent's default blue.
        Application.Current.TryGetResource("SystemAccentColor", ThemeVariant.Light, out var accentObj).ShouldBeTrue();
        var accent = (Color)accentObj!;
        accent.ShouldNotBe(Color.FromRgb(0, 120, 215));

        // A base Fluent accent brush (Color="{DynamicResource SystemAccentColor}") in a live tree
        // cascades to that accent.
        var probe = new Border();
        new Window { Content = probe }.Show();
        Dispatcher.UIThread.RunJobs();
        probe.Bind(Border.BackgroundProperty, probe.GetResourceObservable("SystemControlHighlightAccentBrush"));
        Dispatcher.UIThread.RunJobs();

        ((ISolidColorBrush)probe.Background!).Color.ShouldBe(accent);
    }
}
