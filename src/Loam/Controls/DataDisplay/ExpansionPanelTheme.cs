using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ExpansionPanel"/> theme: a clickable header (<c>PART_Header</c>) with a rotating chevron (<c>PART_Chevron</c>) over a collapsible content area (<c>PART_Content</c>).</summary>
internal static class ExpansionPanelTheme
{
    public static ControlTheme Create() =>
        new(typeof(ExpansionPanel))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<ExpansionPanel> BuildTemplate() =>
        new((panel, scope) =>
        {
            var headerPresenter = new ContentPresenter { VerticalAlignment = VerticalAlignment.Center };
            headerPresenter.Bind(ContentPresenter.ContentProperty,
                panel.GetObservable(HeaderedContentControl.HeaderProperty));

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
                Child = new DockPanel { LastChildFill = true, Children = { chevron, headerPresenter } },
                Padding = new Thickness(16, 12),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand),
                Focusable = true,
            }.Named("PART_Header", scope);

            var content = new ContentPresenter { Padding = new Thickness(16, 0, 16, 16), IsVisible = false }
                .Named("PART_Content", scope);
            content.Bind(ContentPresenter.ContentProperty, panel.GetObservable(ContentControl.ContentProperty));

            var collapse = new Collapse { Child = content }.Named("PART_Collapse", scope);

            var root = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = new StackPanel { Children = { header, collapse } },
            }.Named("PART_Root", scope);
            root.Bind(Border.BorderBrushProperty, panel.GetResourceObservable(LoamTokens.Divider));

            return root;
        });
}
