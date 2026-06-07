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
using AC = Avalonia.Controls;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Alert"/> theme: token-colored alert anatomy with icon, text stack, action and close regions.</summary>
internal static class AlertTheme
{
    public static ControlTheme Create() =>
        new(typeof(Alert))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(16, 12)),
                ButtonStyles.Dyn(TemplatedControl.CornerRadiusProperty, LoamTokens.ShapeMedium),
            },
        };

    private static FuncControlTemplate<Alert> BuildTemplate() =>
        new((alert, scope) =>
        {
            var icon = new Icon
            {
                Color = LoamColor.Inherit,
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 12, 0),
            }.Named("PART_Icon", scope);

            var title = new Text
            {
                Color = LoamColor.Inherit,
                Typo = Typo.Subtitle2,
                TextWrapping = TextWrapping.Wrap,
            }.Named("PART_Title", scope);
            title.Bind(TextBlock.TextProperty, alert.GetObservable(Alert.TitleProperty));
            title.Bind(Visual.IsVisibleProperty, alert.GetObservable(Alert.TitleProperty, value => !string.IsNullOrWhiteSpace(value)));

            var message = new Text
            {
                Color = LoamColor.Inherit,
                Typo = Typo.Body2,
                TextWrapping = TextWrapping.Wrap,
            }.Named("PART_Message", scope);
            message.Bind(TextBlock.TextProperty, alert.GetObservable(Alert.MessageProperty));
            message.Bind(Visual.IsVisibleProperty, alert.GetObservable(Alert.MessageProperty, value => !string.IsNullOrWhiteSpace(value)));

            var presenter = new ContentPresenter { VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, alert.GetObservable(ContentControl.ContentProperty));
            presenter.Bind(ContentPresenter.ContentTemplateProperty, alert.GetObservable(ContentControl.ContentTemplateProperty));
            presenter.Bind(Visual.IsVisibleProperty, alert.GetObservable(ContentControl.ContentProperty, value => value is not null));

            var textStack = new StackPanel
            {
                Spacing = 2,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { title, message, presenter },
            };

            var action = new ContentPresenter
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 0, 0),
            }.Named("PART_Action", scope);
            action.Bind(ContentPresenter.ContentProperty, alert.GetObservable(Alert.ActionProperty));
            action.Bind(Visual.IsVisibleProperty, alert.GetObservable(Alert.ActionProperty, value => value is not null));

            var close = new IconButton
            {
                Variant = Variant.Text,
                Color = LoamColor.Inherit,
                Size = LoamSize.Medium,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(8, 0, 0, 0),
                IsVisible = false,
            }.Named("PART_Close", scope);

            var row = new AC.Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
                VerticalAlignment = VerticalAlignment.Center,
                Children = { icon, textStack, action, close },
            };
            AC.Grid.SetColumn(textStack, 1);
            AC.Grid.SetColumn(action, 2);
            AC.Grid.SetColumn(close, 3);

            var border = new Border { Child = row }.Named("PART_Root", scope);
            border.Bind(Border.PaddingProperty, alert.GetObservable(TemplatedControl.PaddingProperty));
            border.Bind(Border.CornerRadiusProperty, alert.GetObservable(TemplatedControl.CornerRadiusProperty));
            border.Bind(Layoutable.MinHeightProperty, alert.GetResourceObservable(LoamTokens.DensityInteractiveLarge));
            return border;
        });
}
