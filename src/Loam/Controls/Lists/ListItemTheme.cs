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

            var secondary = new Text { Typo = Typo.Body2, IsVisible = false }
                .Named("PART_SecondaryText", scope);
            secondary.Bind(TextBlock.TextProperty, item.GetObservable(ListItem.SecondaryTextProperty));
            secondary.Bind(Visual.IsVisibleProperty, item.GetObservable(ListItem.SecondaryTextProperty, text => !string.IsNullOrWhiteSpace(text)));
            secondary.Bind(TextBlock.ForegroundProperty, item.GetResourceObservable(LoamTokens.TextSecondary));

            var textStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Children = { presenter, secondary },
            };

            var action = new ContentPresenter { VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Action", scope);
            action.Bind(ContentPresenter.ContentProperty, item.GetObservable(ListItem.ActionProperty));
            action.Bind(Visual.IsVisibleProperty, item.GetObservable(ListItem.ActionProperty, value => value is not null));

            var row = new global::Avalonia.Controls.Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 10),
                Children = { icon, textStack, action },
            };
            global::Avalonia.Controls.Grid.SetColumn(textStack, 1);
            global::Avalonia.Controls.Grid.SetColumn(action, 2);

            var border = new Border
            {
                Child = row,
                Cursor = new Cursor(StandardCursorType.Hand),
                Background = Brushes.Transparent,
                MinHeight = 48,
            }.Named("PART_Root", scope);
            return border;
        });
}
