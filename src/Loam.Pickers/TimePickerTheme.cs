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

/// <summary>Builds the <see cref="TimePicker"/> theme: an optional label over a variant field (<c>PART_Box</c>) showing the selected time (<c>PART_Display</c>) and a clock icon.</summary>
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
            }.Named("PART_Label", scope);

            var restingLabel = new Text
            {
                Typo = Typo.Body1,
                Color = LoamColor.Inherit,
                IsHitTestVisible = false,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_RestingLabel", scope);

            var display = new Text
            {
                Color = LoamColor.Inherit,
                Typo = Typo.Body1,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis,
            }
                .Named("PART_Display", scope);

            var icon = new Icon
            {
                Data = Icons.Material.Filled.Schedule,
                Color = LoamColor.Default,
                Size = LoamSize.Small,
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };
            DockPanel.SetDock(icon, Dock.Right);

            var clear = new IconButton
            {
                Icon = Icons.Material.Filled.Close,
                Size = LoamSize.Small,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0),
            }.Named("PART_Clear", scope);
            DockPanel.SetDock(clear, Dock.Right);

            var adornment = new Icon
            {
                Color = LoamColor.Default,
                Size = LoamSize.Small,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0),
            }.Named("PART_Adornment", scope);
            DockPanel.SetDock(adornment, Dock.Left);

            var textLayer = new Avalonia.Controls.Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Children = { display, restingLabel },
            };
            var box = new Border
            {
                MinWidth = 240,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new DockPanel { LastChildFill = true, Children = { icon, clear, adornment, textLayer } },
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
