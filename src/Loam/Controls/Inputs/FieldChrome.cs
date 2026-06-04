using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Loam.Theming;

namespace Loam.Controls;

internal static class FieldChrome
{
    public static void ResetInnerTextBox(TextBox textBox)
    {
        textBox.Background = Brushes.Transparent;
        textBox.BorderBrush = Brushes.Transparent;
        textBox.BorderThickness = default;
        textBox.Padding = default;
        textBox.MinHeight = 24;
    }

    public static void Apply(
        Control host,
        Border inputBorder,
        Variant variant,
        LoamColor color,
        bool error,
        bool focused,
        bool enabled,
        ref IDisposable? borderBrush,
        ref IDisposable? background,
        Thickness? textPadding = null,
        Thickness? filledPadding = null,
        Thickness? outlinedPadding = null)
    {
        host.Opacity = enabled ? 1 : 0.5;

        var paletteName = color.ToPaletteName();
        var accent = paletteName is null ? LoamTokens.Primary : LoamTokens.Palette(paletteName);
        var brushKey = error ? LoamTokens.Error
            : focused ? accent
            : LoamTokens.Palette(nameof(LoamPalette.LinesInputs));
        var emphasized = focused || error;

        borderBrush?.Dispose();
        borderBrush = inputBorder.Bind(Border.BorderBrushProperty, host.GetResourceObservable(brushKey));

        background?.Dispose();
        background = null;

        switch (variant)
        {
            case Variant.Filled:
                inputBorder.CornerRadius = new CornerRadius(4, 4, 0, 0);
                inputBorder.BorderThickness = new Thickness(0, 0, 0, emphasized ? 2 : 1);
                inputBorder.Padding = filledPadding ?? new Thickness(12, 8);
                background = inputBorder.Bind(Border.BackgroundProperty,
                    host.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.ActionDisabledBackground))));
                break;
            case Variant.Text:
                inputBorder.CornerRadius = default;
                inputBorder.BorderThickness = new Thickness(0, 0, 0, emphasized ? 2 : 1);
                inputBorder.Padding = textPadding ?? new Thickness(0, 6);
                inputBorder.Background = Brushes.Transparent;
                break;
            default:
                inputBorder.CornerRadius = new CornerRadius(4);
                inputBorder.BorderThickness = new Thickness(emphasized ? 2 : 1);
                inputBorder.Padding = outlinedPadding ?? new Thickness(12, 8);
                inputBorder.Background = Brushes.Transparent;
                break;
        }
    }
}
