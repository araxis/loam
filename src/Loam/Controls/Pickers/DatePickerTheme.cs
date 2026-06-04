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

/// <summary>Builds the <see cref="DatePicker"/> theme: an optional label over an outlined box (<c>PART_Box</c>) showing the selected date (<c>PART_Display</c>) and a calendar icon.</summary>
internal static class DatePickerTheme
{
    public static ControlTheme Create() =>
        new(typeof(DatePicker))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<DatePicker> BuildTemplate() =>
        new((picker, scope) =>
        {
            var label = new Text
            {
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                IsVisible = false,
            }.Named("PART_Label", scope);

            var restingLabel = new Text
            {
                Typo = Typo.Body1,
                Color = LoamColor.Inherit,
                IsHitTestVisible = false,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_RestingLabel", scope);

            var display = new Text { Color = LoamColor.Inherit, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Display", scope);

            var icon = new Icon { Data = Icons.Material.Filled.CalendarToday, Color = LoamColor.Default, VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(icon, Dock.Right);

            var textLayer = new Avalonia.Controls.Grid { Children = { display, restingLabel } };
            var box = new Border
            {
                Child = new DockPanel { LastChildFill = true, Children = { icon, textLayer } },
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
