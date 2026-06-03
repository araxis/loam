using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="TreeViewItem"/> theme: a clickable row (<c>PART_Row</c>: expander <c>PART_Chevron</c> + optional <c>PART_Icon</c> + <c>PART_Text</c>) over the indented child container (<c>PART_Children</c>).</summary>
internal static class TreeViewItemTheme
{
    public static ControlTheme Create() =>
        new(typeof(TreeViewItem))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<TreeViewItem> BuildTemplate() =>
        new((item, scope) =>
        {
            var chevron = new Icon
            {
                Data = Icons.Material.Filled.ExpandMore,
                Color = LoamColor.Default,
                Size = LoamSize.Small,
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransformOrigin = RelativePoint.Center,
                Cursor = new Cursor(StandardCursorType.Hand),
            }.Named("PART_Chevron", scope);

            var icon = new Icon
            {
                Color = LoamColor.Default,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 6, 0),
            }.Named("PART_Icon", scope);

            var text = new Text { Color = LoamColor.Inherit, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Text", scope);

            var row = new Border
            {
                Child = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children = { chevron, icon, text },
                },
                Padding = new Thickness(4, 6),
                CornerRadius = new CornerRadius(4),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
            }.Named("PART_Row", scope);

            var children = new StackPanel { Margin = new Thickness(20, 0, 0, 0), IsVisible = false }
                .Named("PART_Children", scope);

            return new StackPanel { Children = { row, children } };
        });
}
