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

/// <summary>Builds the <see cref="Select"/> theme: an optional label over an outlined box (<c>PART_Box</c>) showing the selected text (<c>PART_Display</c>) and a dropdown chevron.</summary>
internal static class SelectTheme
{
    public static ControlTheme Create() =>
        new(typeof(Select))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Select> BuildTemplate() =>
        new((select, scope) =>
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

            var chevron = new Icon { Data = Icons.Material.Filled.ExpandMore, Color = LoamColor.Default, VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(chevron, Dock.Right);

            var textLayer = new Avalonia.Controls.Grid { Children = { display, restingLabel } };
            var box = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new DockPanel { LastChildFill = true, Children = { chevron, textLayer } },
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                Cursor = new Cursor(StandardCursorType.Hand),
                Focusable = true,
            }.Named("PART_Box", scope);

            var labelHost = FieldChrome.BuildLabelHost(label, select, scope);
            var fieldSurface = new Panel
            {
                ClipToBounds = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children = { box, labelHost },
            };

            var popup = new Popup
            {
                IsLightDismissEnabled = true,
                OverlayDismissEventPassThrough = true,
                Placement = PlacementMode.BottomEdgeAlignedLeft,
                PlacementTarget = box,
            }.Named("PART_Popup", scope);

            var helper = new Text
            {
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                IsVisible = false,
                Margin = new Thickness(0, 3, 0, 0),
            }.Named("PART_HelperText", scope);

            return new StackPanel { Children = { fieldSurface, helper, popup } };
        });
}
