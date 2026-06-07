# Findings — Fluent accent bridge (sourced 2026-06-07)

Avalonia **12.0.4**. Verified against Avalonia source at tag `12.0.4`:
- `src/Avalonia.Themes.Fluent/Accents/SystemAccentColors.cs`
- `src/Avalonia.Themes.Fluent/Accents/BaseResources.xaml`

## Accent keys

`SystemAccentColor` + `SystemAccentColor{Dark1,Dark2,Dark3,Light1,Light2,Light3}` are **Color**
resources, supplied by the internal `SystemAccentColors : ResourceProvider` (default `#0078D7`, or the
OS accent). Shades derive from the base via HSL **lightness** offsets: dark1 −28.5/255, dark2 −49/255,
dark3 −74.5/255; light1 +39/255, light2 +70/255, light3 +103/255 (public static
`CalculateAccentShades`).

## Gotcha — overriding the color does NOT cascade to brushes

Fluent's accent brushes (`SystemControl*AccentBrush`) are defined as
`Color="{DynamicResource SystemAccentColor}"`. That inner `DynamicResource` resolves inside
**FluentTheme's own scope**, where Fluent's `SystemAccentColors` provider supplies the color. A sibling
`LoamTheme` (added after `FluentTheme`) that overrides `SystemAccentColor` wins for **direct** color
lookups (`Application.TryGetResource("SystemAccentColor")` returns Loam primary) but **not** for the
brushes' internal lookup — so the brushes stay Fluent blue. Verified empirically: the first version of
the end-to-end test showed `SystemControlHighlightAccentBrush.Color == #0078D7` even with the color
overridden.

## Fix that works

Override the accent **brush** keys themselves in `LoamTheme`. Controls bind the brush key via
`DynamicResource` and resolve it from their own scope → app styles → `LoamTheme` (which wins, as proven
by the app-level `SystemAccentColor` lookup). Keys + opacities (same in light and dark):

- `@1.0`: `SystemControlBackgroundAccentBrush`, `SystemControlForegroundAccentBrush`,
  `SystemControlDisabledAccentBrush`, `SystemControlHighlightAccentBrush`,
  `SystemControlHighlightAltAccentBrush`, `SystemControlHyperlinkTextBrush`.
- list-selection states `@ {0.7, 0.6, 0.4}`: `SystemControl[Alt]HighlightListAccent{High,Medium,Low}Brush`.

WinUI-style `AccentFillColor*Brush` / `TextOnAccent*Brush` keys do **not** exist in Avalonia 12 Fluent
(it uses the UWP `SystemControl*` naming).

Implemented in `LoamTheme.BridgeFluentAccent` (per-variant, runtime-swappable; both the color keys and
the brush keys are set). The clean long-term alternative is `ColorPaletteResources.Accent` on the
`FluentTheme` instance, but that requires owning the `FluentTheme`; brush overrides keep `LoamTheme`
self-contained until Loam themes the base chrome itself and drops FluentTheme (Phase 3+).
