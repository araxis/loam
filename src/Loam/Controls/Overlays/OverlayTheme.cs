using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Controls.Internal;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Overlay"/> theme: a parent-filling scrim (<c>PART_Scrim</c>) — darker when <c>DarkBackground</c> — wrapping centered content (<c>PART_Content</c>).</summary>
internal static class OverlayTheme
{
    public static ControlTheme Create() =>
        new(typeof(Overlay))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Overlay> BuildTemplate() =>
        new((overlay, scope) =>
        {
            var presenter = new ContentPresenter
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Content", scope);
            presenter.Bind(ContentPresenter.ContentProperty, overlay.GetObservable(ContentControl.ContentProperty));

            var scrim = new Border
            {
                Child = presenter,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            }.Named("PART_Scrim", scope);
            InteractionAssist.ApplyZIndex(scrim, LoamTokens.ZIndex(nameof(LoamZIndex.Dialog)), LoamZIndex.Default.Dialog);

            return scrim;
        });
}
