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
            }.Named("PART_Label", scope);

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

            // Hosted, chrome-stripped text box shown only in Editable mode (see ColorPicker.UpdateEditMode).
            var input = new TextBox
            {
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            }.Named("PART_Input", scope);

            DockPanel.SetDock(swatch, Dock.Left);
            // hex and input share the same cell; UpdateEditMode toggles which one is visible.
            var textLayer = new Avalonia.Controls.Grid
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children = { hex, input },
            };

            var box = new Border
            {
                Child = new DockPanel { LastChildFill = true, Children = { swatch, textLayer } },
                Cursor = new Cursor(StandardCursorType.Hand),
                Focusable = true,
            }.Named("PART_Box", scope);

            var labelHost = FieldChrome.BuildLabelHost(label, picker, scope);
            var fieldSurface = new Panel
            {
                ClipToBounds = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children = { box, labelHost },
            };

            var helper = new Text
            {
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                IsVisible = false,
                Margin = new Thickness(0, 3, 0, 0),
            }.Named("PART_HelperText", scope);

            return new StackPanel { Children = { fieldSurface, helper } };
        });
}
