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

/// <summary>Builds the <see cref="NavGroup"/> theme: a clickable header (<c>PART_Header</c>: icon + title + rotating <c>PART_Chevron</c>) over the collapsible nested items (<c>PART_Items</c>).</summary>
internal static class NavGroupTheme
{
    public static ControlTheme Create() =>
        new(typeof(NavGroup))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<NavGroup> BuildTemplate() =>
        new((group, scope) =>
        {
            var icon = new Icon
            {
                Color = LoamColor.Default,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0),
            }.Named("PART_Icon", scope);

            var title = new Text { Color = LoamColor.Default, VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Title", scope);

            var chevron = new Icon
            {
                Data = Icons.Material.Filled.ExpandMore,
                Color = LoamColor.Default,
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransformOrigin = RelativePoint.Center,
            }.Named("PART_Chevron", scope);
            DockPanel.SetDock(chevron, Dock.Right);

            var header = new Border
            {
                Child = new DockPanel { LastChildFill = true, Children = { chevron, icon, title } },
                Padding = new Thickness(16, 10),
                CornerRadius = new CornerRadius(4),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
                Focusable = true,
            }.Named("PART_Header", scope);
            header.Bind(Layoutable.MinHeightProperty, group.GetResourceObservable(LoamTokens.DensityInteractiveMedium));

            var items = new StackPanel { IsVisible = false }.Named("PART_Items", scope);
            var itemsCollapse = new Collapse { Child = items }.Named("PART_ItemsCollapse", scope);

            return new StackPanel { Children = { header, itemsCollapse } };
        });
}
