using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="CardHeader"/> theme: optional avatar (<c>PART_Avatar</c>), a title/subtitle stack, and a trailing action (<c>PART_Action</c>).</summary>
internal static class CardHeaderTheme
{
    public static ControlTheme Create() =>
        new(typeof(CardHeader))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<CardHeader> BuildTemplate() =>
        new((header, scope) =>
        {
            var avatar = new ContentPresenter { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) }
                .Named("PART_Avatar", scope);
            avatar.Bind(ContentPresenter.ContentProperty, header.GetObservable(CardHeader.AvatarProperty));
            avatar.Bind(Visual.IsVisibleProperty, header.GetObservable(CardHeader.AvatarProperty, a => a is not null));
            DockPanel.SetDock(avatar, Dock.Left);

            var action = new ContentPresenter { VerticalAlignment = VerticalAlignment.Center }
                .Named("PART_Action", scope);
            action.Bind(ContentPresenter.ContentProperty, header.GetObservable(CardHeader.ActionProperty));
            action.Bind(Visual.IsVisibleProperty, header.GetObservable(CardHeader.ActionProperty, a => a is not null));
            DockPanel.SetDock(action, Dock.Right);

            var title = new Text { Typo = Typo.Subtitle1 }.Named("PART_Title", scope);
            title.Bind(TextBlock.TextProperty, header.GetObservable(CardHeader.TitleProperty));
            title.Bind(Visual.IsVisibleProperty, header.GetObservable(CardHeader.TitleProperty, t => !string.IsNullOrEmpty(t)));

            var subtitle = new Text { Typo = Typo.Body2 }.Named("PART_Subtitle", scope);
            subtitle.Bind(TextBlock.ForegroundProperty, header.GetResourceObservable(LoamTokens.TextSecondary));
            subtitle.Bind(TextBlock.TextProperty, header.GetObservable(CardHeader.SubtitleProperty));
            subtitle.Bind(Visual.IsVisibleProperty, header.GetObservable(CardHeader.SubtitleProperty, s => !string.IsNullOrEmpty(s)));

            var titles = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Children = { title, subtitle } };

            return new DockPanel { LastChildFill = true, Margin = new Thickness(16), Children = { avatar, action, titles } };
        });
}
