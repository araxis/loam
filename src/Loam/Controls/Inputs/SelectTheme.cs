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
            label.Bind(TextBlock.ForegroundProperty, select.GetResourceObservable(LoamTokens.TextSecondary));

            var display = new Text { Color = LoamColor.Inherit, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Display", scope);

            var chevron = new Icon { Data = Icons.Material.Filled.ExpandMore, Color = LoamColor.Default, VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(chevron, Dock.Right);

            var box = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new DockPanel { LastChildFill = true, Children = { chevron, display } },
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                MinHeight = 52,
                Padding = new Thickness(12, 14),
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

            return new StackPanel { Children = { fieldSurface, popup } };
        });
}
