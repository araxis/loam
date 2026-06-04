using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ColorPicker"/> theme: an optional label over an outlined box (<c>PART_Box</c>) showing the current color swatch (<c>PART_Swatch</c>) + hex (<c>PART_Hex</c>).</summary>
internal static class ColorPickerTheme
{
    public static ControlTheme Create() =>
        new(typeof(ColorPicker))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<ColorPicker> BuildTemplate() =>
        new((picker, scope) =>
        {
            var label = new Text
            {
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                IsVisible = false,
                Margin = new Thickness(0, 0, 0, 3),
            }.Named("PART_Label", scope);
            label.Bind(TextBlock.ForegroundProperty, picker.GetResourceObservable(LoamTokens.TextSecondary));

            var swatch = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(3),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
            }.Named("PART_Swatch", scope);
            swatch.Bind(Border.BorderBrushProperty, picker.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));
            swatch.BorderThickness = new Thickness(1);

            var hex = new Text { Color = LoamColor.Default, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Hex", scope);

            var box = new Border
            {
                Child = new StackPanel { Orientation = Orientation.Horizontal, Children = { swatch, hex } },
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 8),
                Cursor = new Cursor(StandardCursorType.Hand),
                Focusable = true,
            }.Named("PART_Box", scope);
            box.Bind(Border.BorderBrushProperty,
                picker.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));

            return new StackPanel { Children = { label, box } };
        });
}
