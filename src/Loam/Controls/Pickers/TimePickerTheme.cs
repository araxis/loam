using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="TimePicker"/> theme: an optional label over an outlined box (<c>PART_Box</c>) showing the selected time (<c>PART_Display</c>) and a clock icon.</summary>
internal static class TimePickerTheme
{
    public static ControlTheme Create() =>
        new(typeof(TimePicker))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<TimePicker> BuildTemplate() =>
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

            var display = new Text { Color = LoamColor.Inherit, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Display", scope);

            var icon = new Icon { Data = Icons.Material.Filled.Schedule, Color = LoamColor.Default, VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(icon, Dock.Right);

            var box = new Border
            {
                Child = new DockPanel { LastChildFill = true, Children = { icon, display } },
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 8),
                Cursor = new Cursor(StandardCursorType.Hand),
            }.Named("PART_Box", scope);
            box.Bind(Border.BorderBrushProperty,
                picker.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));

            return new StackPanel { Children = { label, box } };
        });
}
