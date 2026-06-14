using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Loam.Theming;

/// <summary>
/// Phase 1 — theme consistency. Bridges Loam design tokens onto the resource keys Avalonia's base
/// Fluent theme uses, so base controls that have no dedicated Loam <c>ControlTheme</c> still read as
/// Material in both light and dark: accent, scrollbars, tooltips, menus/flyouts, the window
/// background, text selection, and expanders.
/// </summary>
/// <remarks>
/// Entries are written into <see cref="LoamTheme"/>'s per-variant theme dictionaries, so they swap
/// with the variant and at runtime (<c>SetPrimary</c>/<c>SetPalette</c>/<c>SetData</c>). We override
/// the brush keys controls bind — not just the underlying colors: Fluent's own brushes resolve their
/// nested <c>DynamicResource</c> colors inside FluentTheme's scope, so a color-only override does not
/// cascade (see <c>memory/findings/2026-06-07-fluent-accent-bridge.md</c> and ADR-0011). Keys are
/// version-coupled and verified against Avalonia 12.0.4 source.
/// </remarks>
internal static class FluentBridge
{
    /// <summary>Applies every Fluent bridge into one per-variant resource dictionary.</summary>
    public static void Apply(ResourceDictionary dict, LoamColorScheme scheme, LoamStateLayer stateLayer)
    {
        Accent(dict, scheme.Primary);
        ScrollBars(dict, scheme);
        ToolTips(dict, scheme);
        Menus(dict, scheme, stateLayer);
        WindowChrome(dict, scheme);
        Expanders(dict, scheme, stateLayer);
    }

    // Accent: stray Fluent controls adopt Loam's primary instead of Fluent blue. Override both the
    // SystemAccentColor* color keys (base + six HSL-derived shades mirroring Avalonia 12.0.4's
    // SystemAccentColors) AND the SystemControl*AccentBrush keys — the brushes are what actually
    // retint stray controls. List-accent selection states use Fluent's 0.7/0.6/0.4 opacities.
    private static void Accent(ResourceDictionary dict, Color accent)
    {
        var (dark1, dark2, dark3, light1, light2, light3) = CalculateAccentShades(accent);
        dict["SystemAccentColor"] = accent;
        dict["SystemAccentColorDark1"] = dark1;
        dict["SystemAccentColorDark2"] = dark2;
        dict["SystemAccentColorDark3"] = dark3;
        dict["SystemAccentColorLight1"] = light1;
        dict["SystemAccentColorLight2"] = light2;
        dict["SystemAccentColorLight3"] = light3;

        var accentBrush = new ImmutableSolidColorBrush(accent);
        dict["SystemControlBackgroundAccentBrush"] = accentBrush;
        dict["SystemControlForegroundAccentBrush"] = accentBrush;
        dict["SystemControlDisabledAccentBrush"] = accentBrush;
        dict["SystemControlHighlightAccentBrush"] = accentBrush;
        dict["SystemControlHighlightAltAccentBrush"] = accentBrush;
        dict["SystemControlHyperlinkTextBrush"] = accentBrush;
        dict["SystemControlHighlightListAccentHighBrush"] = new ImmutableSolidColorBrush(accent, 0.7);
        dict["SystemControlHighlightListAccentMediumBrush"] = new ImmutableSolidColorBrush(accent, 0.6);
        dict["SystemControlHighlightListAccentLowBrush"] = new ImmutableSolidColorBrush(accent, 0.4);
        dict["SystemControlHighlightAltListAccentHighBrush"] = new ImmutableSolidColorBrush(accent, 0.7);
        dict["SystemControlHighlightAltListAccentMediumBrush"] = new ImmutableSolidColorBrush(accent, 0.6);
        dict["SystemControlHighlightAltListAccentLowBrush"] = new ImmutableSolidColorBrush(accent, 0.4);
    }

    // Material-neutral scrollbars: a subtle on-surface thumb on a transparent track; geometry/size
    // keys left to Fluent. Scrollbars are intentionally neutral, not accent-colored.
    private static void ScrollBars(ResourceDictionary dict, LoamColorScheme scheme)
    {
        var transparent = new ImmutableSolidColorBrush(Colors.Transparent);
        var arrow = new ImmutableSolidColorBrush(scheme.OnSurfaceVariant);
        var arrowActive = new ImmutableSolidColorBrush(scheme.OnSurface);

        dict["ScrollBarBackground"] = transparent;
        dict["ScrollBarBorderBrush"] = transparent;
        dict["ScrollBarForeground"] = arrow;
        dict["ScrollBarTrackFill"] = transparent;
        dict["ScrollBarTrackStroke"] = transparent;

        dict["ScrollBarPanningThumbBackground"] = new ImmutableSolidColorBrush(scheme.OnSurfaceVariant, 0.45);
        dict["ScrollBarThumbFillPointerOver"] = new ImmutableSolidColorBrush(scheme.OnSurfaceVariant, 0.70);
        dict["ScrollBarThumbFillPressed"] = new ImmutableSolidColorBrush(scheme.OnSurface, 0.72);
        dict["ScrollBarThumbFillDisabled"] = new ImmutableSolidColorBrush(scheme.OnSurface, 0.12);

        dict["ScrollBarButtonBackground"] = transparent;
        dict["ScrollBarButtonBackgroundPointerOver"] = new ImmutableSolidColorBrush(scheme.OnSurface, 0.08);
        dict["ScrollBarButtonBackgroundPressed"] = new ImmutableSolidColorBrush(scheme.OnSurface, 0.12);
        dict["ScrollBarButtonBackgroundDisabled"] = transparent;
        dict["ScrollBarButtonBorderBrush"] = transparent;
        dict["ScrollBarButtonBorderBrushPointerOver"] = transparent;
        dict["ScrollBarButtonBorderBrushPressed"] = transparent;
        dict["ScrollBarButtonBorderBrushDisabled"] = transparent;
        dict["ScrollBarButtonArrowForeground"] = arrow;
        dict["ScrollBarButtonArrowForegroundPointerOver"] = arrowActive;
        dict["ScrollBarButtonArrowForegroundPressed"] = arrowActive;
        dict["ScrollBarButtonArrowForegroundDisabled"] = new ImmutableSolidColorBrush(scheme.OnSurface, 0.38);
    }

    // Material tooltips: inverse-surface container, inverse-on-surface text, no border.
    private static void ToolTips(ResourceDictionary dict, LoamColorScheme scheme)
    {
        dict["ToolTipBackground"] = new ImmutableSolidColorBrush(scheme.InverseSurface);
        dict["ToolTipForeground"] = new ImmutableSolidColorBrush(scheme.InverseOnSurface);
        dict["ToolTipBorderBrush"] = new ImmutableSolidColorBrush(Colors.Transparent);
    }

    // Material menus/flyouts: SurfaceContainer surface (no border), OnSurface item text, OnSurface
    // hover/press state layers, muted OnSurfaceVariant shortcut text and submenu chevrons.
    private static void Menus(ResourceDictionary dict, LoamColorScheme scheme, LoamStateLayer stateLayer)
    {
        var transparent = new ImmutableSolidColorBrush(Colors.Transparent);
        var surface = new ImmutableSolidColorBrush(scheme.SurfaceContainer);
        var onSurface = new ImmutableSolidColorBrush(scheme.OnSurface);
        var onSurfaceVariant = new ImmutableSolidColorBrush(scheme.OnSurfaceVariant);
        var disabled = new ImmutableSolidColorBrush(scheme.OnSurface, stateLayer.DisabledOpacity);

        dict["MenuFlyoutPresenterBackground"] = surface;
        dict["FlyoutPresenterBackground"] = surface;
        dict["MenuFlyoutPresenterBorderBrush"] = transparent;

        dict["MenuFlyoutItemBackground"] = transparent;
        dict["MenuFlyoutItemBackgroundPointerOver"] = new ImmutableSolidColorBrush(scheme.OnSurface, stateLayer.HoverOpacity);
        dict["MenuFlyoutItemBackgroundPressed"] = new ImmutableSolidColorBrush(scheme.OnSurface, stateLayer.PressedOpacity);
        dict["MenuFlyoutItemBackgroundDisabled"] = transparent;

        dict["MenuFlyoutItemForeground"] = onSurface;
        dict["MenuFlyoutItemForegroundPointerOver"] = onSurface;
        dict["MenuFlyoutItemForegroundPressed"] = onSurface;
        dict["MenuFlyoutItemForegroundDisabled"] = disabled;

        dict["MenuFlyoutItemKeyboardAcceleratorTextForeground"] = onSurfaceVariant;
        dict["MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver"] = onSurfaceVariant;
        dict["MenuFlyoutItemKeyboardAcceleratorTextForegroundPressed"] = onSurfaceVariant;
        dict["MenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled"] = disabled;

        dict["MenuFlyoutSubItemChevron"] = onSurfaceVariant;
        dict["MenuFlyoutSubItemChevronPointerOver"] = onSurface;
        dict["MenuFlyoutSubItemChevronPressed"] = onSurface;
        dict["MenuFlyoutSubItemChevronDisabled"] = disabled;
        dict["MenuFlyoutSubItemChevronSubMenuOpened"] = onSurface;
    }

    // The bare Window region reads as the Material app background, and base text selection uses Loam
    // primary instead of Fluent blue (flows into base TextBoxes, incl. Loam Field/TextField). Caret /
    // base text foreground are left to Fluent (caret follows TextControlForeground).
    private static void WindowChrome(ResourceDictionary dict, LoamColorScheme scheme)
    {
        dict["SystemRegionBrush"] = new ImmutableSolidColorBrush(scheme.Background);
        dict["TextControlSelectionHighlightColor"] = new ImmutableSolidColorBrush(scheme.Primary, 0.4);
    }

    // Material expanders: header on the tonal container ramp (rest/hover/press), OnSurface header
    // text, a subtle outline-variant edge, surface content, and a neutral chevron. Size/thickness/
    // padding/alignment keys are left to Fluent.
    private static void Expanders(ResourceDictionary dict, LoamColorScheme scheme, LoamStateLayer stateLayer)
    {
        var transparent = new ImmutableSolidColorBrush(Colors.Transparent);
        var onSurface = new ImmutableSolidColorBrush(scheme.OnSurface);
        var onSurfaceVariant = new ImmutableSolidColorBrush(scheme.OnSurfaceVariant);
        var disabledFg = new ImmutableSolidColorBrush(scheme.OnSurface, stateLayer.DisabledOpacity);
        var outline = new ImmutableSolidColorBrush(scheme.OutlineVariant);

        dict["ExpanderHeaderBackground"] = new ImmutableSolidColorBrush(scheme.SurfaceContainer);
        dict["ExpanderHeaderBackgroundPointerOver"] = new ImmutableSolidColorBrush(scheme.SurfaceContainerHigh);
        dict["ExpanderHeaderBackgroundPressed"] = new ImmutableSolidColorBrush(scheme.SurfaceContainerHighest);
        dict["ExpanderHeaderBackgroundDisabled"] = new ImmutableSolidColorBrush(scheme.SurfaceContainer);

        dict["ExpanderHeaderForeground"] = onSurface;
        dict["ExpanderHeaderForegroundPointerOver"] = onSurface;
        dict["ExpanderHeaderForegroundPressed"] = onSurface;
        dict["ExpanderHeaderForegroundDisabled"] = disabledFg;

        dict["ExpanderHeaderBorderBrush"] = outline;
        dict["ExpanderHeaderBorderBrushPointerOver"] = outline;
        dict["ExpanderHeaderBorderBrushPressed"] = outline;
        dict["ExpanderHeaderBorderBrushDisabled"] = outline;

        dict["ExpanderContentBackground"] = new ImmutableSolidColorBrush(scheme.Surface);
        dict["ExpanderContentBorderBrush"] = outline;

        dict["ExpanderChevronBackground"] = transparent;
        dict["ExpanderChevronBackgroundPointerOver"] = transparent;
        dict["ExpanderChevronBackgroundPressed"] = transparent;
        dict["ExpanderChevronBackgroundDisabled"] = transparent;
        dict["ExpanderChevronBorderBrush"] = transparent;
        dict["ExpanderChevronBorderBrushPointerOver"] = transparent;
        dict["ExpanderChevronBorderBrushPressed"] = transparent;
        dict["ExpanderChevronBorderBrushDisabled"] = transparent;
        dict["ExpanderChevronForeground"] = onSurfaceVariant;
        dict["ExpanderChevronForegroundPointerOver"] = onSurface;
        dict["ExpanderChevronForegroundPressed"] = onSurface;
        dict["ExpanderChevronForegroundDisabled"] = disabledFg;
    }

    // Mirrors Avalonia 12.0.4 SystemAccentColors.CalculateAccentShades (HSL lightness offsets) so the
    // derived accent shades match Fluent's own derivation for the overridden accent.
    private static (Color D1, Color D2, Color D3, Color L1, Color L2, Color L3) CalculateAccentShades(Color accent)
    {
        const double dark1Step = 28.5 / 255d;
        const double dark2Step = 49 / 255d;
        const double dark3Step = 74.5 / 255d;
        const double light1Step = 39 / 255d;
        const double light2Step = 70 / 255d;
        const double light3Step = 103 / 255d;

        var hsl = accent.ToHsl();
        return (
            new HslColor(hsl.A, hsl.H, hsl.S, hsl.L - dark1Step).ToRgb(),
            new HslColor(hsl.A, hsl.H, hsl.S, hsl.L - dark2Step).ToRgb(),
            new HslColor(hsl.A, hsl.H, hsl.S, hsl.L - dark3Step).ToRgb(),
            new HslColor(hsl.A, hsl.H, hsl.S, hsl.L + light1Step).ToRgb(),
            new HslColor(hsl.A, hsl.H, hsl.S, hsl.L + light2Step).ToRgb(),
            new HslColor(hsl.A, hsl.H, hsl.S, hsl.L + light3Step).ToRgb());
    }
}
