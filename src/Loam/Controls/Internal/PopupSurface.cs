using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Loam.Theming;

namespace Loam.Controls.Internal;

internal static class PopupSurface
{
    public const double PickerWidth = 360;
    public static readonly Thickness PickerPadding = new(0);
    public static readonly Thickness PickerTitleMargin = new(24, 16, 12, 12);
    public static readonly Thickness PickerActionsMargin = new(12, 8, 12, 12);
    private static readonly CornerRadius MenuShape = LoamShape.Default.ExtraSmall;
    public static readonly CornerRadius PickerShape = LoamShape.Default.Large;

    public static ControlTheme FlyoutPresenterTheme { get; } = new(typeof(FlyoutPresenter))
    {
        Setters =
        {
            new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
            new Setter(TemplatedControl.BorderBrushProperty, Brushes.Transparent),
            new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
            new Setter(TemplatedControl.PaddingProperty, new Thickness(0)),
            new Setter(TemplatedControl.TemplateProperty, BuildFlyoutPresenterTemplate()),
        },
    };

    private static FuncControlTemplate<FlyoutPresenter> BuildFlyoutPresenterTemplate() =>
        new((presenter, _) =>
        {
            var content = new ContentPresenter();
            content.Bind(ContentPresenter.ContentProperty, presenter.GetObservable(ContentControl.ContentProperty));
            content.Bind(ContentPresenter.ContentTemplateProperty, presenter.GetObservable(ContentControl.ContentTemplateProperty));
            content.Bind(ContentPresenter.PaddingProperty, presenter.GetObservable(TemplatedControl.PaddingProperty));
            content.Bind(ContentPresenter.HorizontalContentAlignmentProperty,
                presenter.GetObservable(ContentControl.HorizontalContentAlignmentProperty));
            content.Bind(ContentPresenter.VerticalContentAlignmentProperty,
                presenter.GetObservable(ContentControl.VerticalContentAlignmentProperty));
            return content;
        });

    public static Flyout Flyout(Control content, PlacementMode placement = PlacementMode.BottomEdgeAlignedLeft) =>
        new()
        {
            Content = content,
            Placement = placement,
            FlyoutPresenterTheme = FlyoutPresenterTheme,
        };

    public static Paper MenuPaper(Control content, double minWidth = 0)
    {
        var paper = new Paper
        {
            Elevation = 3,
            Padding = new Thickness(0),
            ClipToBounds = true,
            Content = content,
        };
        paper.Resources[LoamTokens.ShapeMedium] = MenuShape;

        if (minWidth > 0)
        {
            paper.MinWidth = minWidth;
        }

        return paper;
    }

    public static StackPanel PickerContent(string title, Control body, Control? actions = null, double width = PickerWidth)
    {
        var content = new StackPanel
        {
            Spacing = 0,
            Children =
            {
                new Text
                {
                    Text = title,
                    Typo = Typo.TitleMedium,
                    Color = LoamColor.Default,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = PickerTitleMargin,
                    MaxWidth = width - PickerTitleMargin.Left - PickerTitleMargin.Right,
                },
                body,
            },
        };

        if (actions is not null)
        {
            actions.Margin = PickerActionsMargin;
            content.Children.Add(actions);
        }

        return content;
    }

    public static Paper PickerPaper(Control content, double width = PickerWidth, Thickness? padding = null)
    {
        var paper = new Paper
        {
            Elevation = 3,
            Padding = padding ?? PickerPadding,
            ClipToBounds = true,
            Content = content,
        };
        if (width > 0)
        {
            paper.Width = width;
            paper.MinWidth = width;
            paper.MaxWidth = width;
        }

        paper.Resources[LoamTokens.ShapeMedium] = PickerShape;
        return paper;
    }

    public static void ActivateListRowOnPointer(ListItem row, Action activate)
    {
        row.AddHandler(
            InputElement.PointerPressedEvent,
            (_, args) =>
            {
                if (!row.IsEnabled)
                {
                    return;
                }

                row.Focus();
                activate();
                args.Handled = true;
            },
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }
}
