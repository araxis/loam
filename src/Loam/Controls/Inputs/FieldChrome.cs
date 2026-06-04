using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

internal static class FieldChrome
{
    public static void ResetInnerTextBox(TextBox textBox)
    {
        textBox.Background = Brushes.Transparent;
        textBox.BorderBrush = Brushes.Transparent;
        textBox.BorderThickness = default;
        textBox.FocusAdorner = null;
        textBox.Padding = default;
        textBox.MinHeight = 24;
        textBox.ApplyTemplate();

        foreach (var border in textBox.GetVisualDescendants().OfType<Border>().Where(b => b.Name == "PART_BorderElement"))
        {
            border.Background = Brushes.Transparent;
            border.BorderBrush = Brushes.Transparent;
            border.BorderThickness = default;
            border.Padding = default;
        }
    }

    public static Border BuildLabelHost(Text label, Control owner, Avalonia.Controls.INameScope scope)
    {
        label.Margin = default;

        var host = new Border
        {
            Child = label,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false,
            IsVisible = false,
            Margin = new Thickness(10, 0, 0, 0),
            Padding = new Thickness(5, 0),
        }.Named("PART_LabelHost", scope);

        host.Bind(Border.BackgroundProperty, owner.GetResourceObservable(LoamTokens.Surface));
        return host;
    }

    public static void ApplyLabelLayout(Border? inputBorder, Border? labelHost, bool showLabel)
    {
        if (labelHost is not null)
        {
            labelHost.IsVisible = showLabel;
        }

        if (inputBorder is not null)
        {
            inputBorder.Margin = showLabel ? new Thickness(0, 7, 0, 0) : default;
        }
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
                inputBorder.MinHeight = 48;
                inputBorder.CornerRadius = new CornerRadius(4, 4, 0, 0);
                inputBorder.BorderThickness = new Thickness(0, 0, 0, emphasized ? 2 : 1);
                inputBorder.Padding = filledPadding ?? new Thickness(12, 12);
                background = inputBorder.Bind(Border.BackgroundProperty,
                    host.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.ActionDisabledBackground))));
                break;
            case Variant.Text:
                inputBorder.MinHeight = 40;
                inputBorder.CornerRadius = default;
                inputBorder.BorderThickness = new Thickness(0, 0, 0, emphasized ? 2 : 1);
                inputBorder.Padding = textPadding ?? new Thickness(0, 9);
                inputBorder.Background = Brushes.Transparent;
                break;
            default:
                inputBorder.MinHeight = 52;
                inputBorder.CornerRadius = new CornerRadius(4);
                inputBorder.BorderThickness = new Thickness(emphasized ? 2 : 1);
                inputBorder.Padding = outlinedPadding ?? new Thickness(12, 14);
                inputBorder.Background = Brushes.Transparent;
                break;
        }
    }
}
