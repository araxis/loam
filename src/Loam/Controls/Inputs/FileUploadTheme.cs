using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="FileUpload"/> theme: an upload <see cref="Button"/> (<c>PART_Button</c>) above a chip strip of selected file names (<c>PART_Files</c>).</summary>
internal static class FileUploadTheme
{
    public static ControlTheme Create() =>
        new(typeof(FileUpload))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<FileUpload> BuildTemplate() =>
        new((upload, scope) =>
        {
            var button = new Button
            {
                Variant = Variant.Outlined,
                Color = LoamColor.Primary,
                StartIcon = Icons.Material.Filled.CloudUpload,
                HorizontalAlignment = HorizontalAlignment.Left,
            }.Named("PART_Button", scope);
            button.Bind(ContentControl.ContentProperty, upload.GetObservable(FileUpload.ButtonTextProperty));

            var files = new WrapPanel { IsVisible = false }.Named("PART_Files", scope);

            return new StackPanel { Children = { button, files } };
        });
}
