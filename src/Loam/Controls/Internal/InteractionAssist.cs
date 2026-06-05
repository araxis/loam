using Avalonia.Automation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

    public static string TonalSurfaceToken(int elevation, bool outlined = false) =>
        LoamTokens.TonalElevation(outlined ? 0 : Math.Clamp(elevation, 0, 5));

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
