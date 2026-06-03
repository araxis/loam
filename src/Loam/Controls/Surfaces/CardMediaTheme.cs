using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Builds the <see cref="CardMedia"/> theme: a fixed-height band (<c>PART_Root</c>) with a neutral placeholder behind a fill <see cref="Image"/> (<c>PART_Image</c>).</summary>
internal static class CardMediaTheme
{
    public static ControlTheme Create() =>
        new(typeof(CardMedia))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<CardMedia> BuildTemplate() =>
        new((media, scope) =>
        {
            var image = new Image { Stretch = Stretch.UniformToFill }.Named("PART_Image", scope);
            image.Bind(Image.SourceProperty, media.GetObservable(CardMedia.SourceProperty));

            var root = new Border { Child = image, ClipToBounds = true }.Named("PART_Root", scope);
            root.Bind(Layoutable.HeightProperty, media.GetObservable(CardMedia.MediaHeightProperty));
            root.Bind(Border.BackgroundProperty,
                media.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.BackgroundGray))));
            return root;
        });
}
