using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ListItem"/> theme: a hover-highlighting <c>PART_Root</c> border with an optional icon + content.</summary>
internal static class ListItemTheme
{
    public static ControlTheme Create()
    {
        var theme = new ControlTheme(typeof(ListItem))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
            },
        };

        theme.Add(new Style(x => x.Nesting().Class(":pointerover"))
        {
            Setters = { ButtonStyles.Dyn(TemplatedControl.BackgroundProperty, LoamTokens.LinesDefault) },
        });

        return theme;
    }

    private static FuncControlTemplate<ListItem> BuildTemplate() =>
        new((item, scope) =>
        {
            var icon = new Icon
            {
                Color = LoamColor.Default,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 16, 0),
            }.Named("PART_Icon", scope);

            var presenter = new ContentPresenter { VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, item.GetObservable(ContentControl.ContentProperty));

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 10),
                Children = { icon, presenter },
            };

            var border = new Border
            {
                Child = row,
                Cursor = new Cursor(StandardCursorType.Hand),
            }.Named("PART_Root", scope);
            border.Bind(Border.BackgroundProperty, item.GetObservable(TemplatedControl.BackgroundProperty));
            return border;
        });
}
