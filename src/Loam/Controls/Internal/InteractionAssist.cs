using Avalonia.Automation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Loam.Theming;

namespace Loam.Controls.Internal;

internal static class InteractionAssist
{
    public static bool IsActivationKey(Key key) => key is Key.Enter or Key.Space;

    public static bool IsIncrementKey(Key key) => key is Key.Right or Key.Up;

    public static bool IsDecrementKey(Key key) => key is Key.Left or Key.Down;

    public static double DisabledOpacity(Control control) =>
        control.TryGetResource(LoamTokens.StateDisabledOpacity, control.ActualThemeVariant, out var value) &&
        value is double opacity
            ? opacity
            : LoamStateLayer.Default.DisabledOpacity;

    public static Thickness ThicknessToken(Control control, string token, Thickness fallback) =>
        control.TryGetResource(token, control.ActualThemeVariant, out var value) && value is Thickness thickness
            ? thickness
            : fallback;

    public static double DoubleToken(Control control, string token, double fallback) =>
        control.TryGetResource(token, control.ActualThemeVariant, out var value) && value is double number
            ? number
            : fallback;

    public static int IntToken(Control control, string token, int fallback) =>
        control.TryGetResource(token, control.ActualThemeVariant, out var value) && value is int number
            ? number
            : fallback;

    public static void ApplyZIndex(Control control, string token, int fallback) =>
        control.ZIndex = IntToken(control, token, fallback);

    public static TimeSpan DurationToken(Control control, string token, TimeSpan fallback) =>
        control.TryGetResource(token, control.ActualThemeVariant, out var value) && value is TimeSpan duration
            ? duration
            : fallback;

    public static IBrush BrushToken(Control control, string token, IBrush fallback) =>
        control.TryGetResource(token, control.ActualThemeVariant, out var value) && value is IBrush brush
            ? brush
            : fallback;

    public static string TonalSurfaceToken(int elevation, bool outlined = false) =>
        LoamTokens.TonalElevation(outlined ? 0 : Math.Clamp(elevation, 0, 5));

    public static void RestoreFocus(TopLevel topLevel, IInputElement? previous)
    {
        if (previous is not null)
        {
            topLevel.FocusManager?.Focus(previous, NavigationMethod.Unspecified, KeyModifiers.None);
        }
    }

    public static void SetAutomationName(Control control, params object?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            var text = ExtractText(candidate);
            if (!string.IsNullOrWhiteSpace(text))
            {
                AutomationProperties.SetName(control, text);
                return;
            }
        }
    }

    private static string? ExtractText(object? candidate)
    {
        return candidate switch
        {
            null => null,
            string text => text.Trim(),
            TextBlock textBlock => textBlock.Text?.Trim(),
            ContentControl { Content: string text } => text.Trim(),
            Control control => AutomationProperties.GetName(control)?.Trim(),
            _ => candidate.ToString()?.Trim(),
        };
    }
}
