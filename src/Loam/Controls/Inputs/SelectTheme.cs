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
                Margin = new Thickness(0, 0, 0, 3),
            }.Named("PART_Label", scope);
            label.Bind(TextBlock.ForegroundProperty, select.GetResourceObservable(LoamTokens.TextSecondary));

            var display = new Text { Color = LoamColor.Inherit, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Display", scope);

            var chevron = new Icon { Data = Icons.Material.Filled.ExpandMore, Color = LoamColor.Default, VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(chevron, Dock.Right);

            var box = new Border
            {
                Child = new DockPanel { LastChildFill = true, Children = { chevron, display } },
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 8),
                Cursor = new Cursor(StandardCursorType.Hand),
                Focusable = true,
            }.Named("PART_Box", scope);
            box.Bind(Border.BorderBrushProperty,
                select.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.LinesInputs))));

            return new StackPanel { Children = { label, box } };
        });
}
