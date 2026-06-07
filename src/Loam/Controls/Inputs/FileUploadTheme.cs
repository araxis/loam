using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
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
            var label = new Text
            {
                Typo = Typo.LabelLarge,
                Color = LoamColor.Secondary,
                IsVisible = false,
            }.Named("PART_Label", scope);

            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Left,
            }.Named("PART_Button", scope);
            button.Bind(ContentControl.ContentProperty, upload.GetObservable(FileUpload.ButtonTextProperty));
            button.Bind(Button.VariantProperty, upload.GetObservable(FileUpload.VariantProperty));
            button.Bind(Button.ColorProperty, upload.GetObservable(FileUpload.ColorProperty));
            button.Bind(Button.SizeProperty, upload.GetObservable(FileUpload.SizeProperty));
            button.Bind(Button.StartIconProperty, upload.GetObservable(FileUpload.ButtonIconProperty));

            var files = new WrapPanel { IsVisible = false }.Named("PART_Files", scope);

            var status = new Text
            {
                Typo = Typo.BodySmall,
                Color = LoamColor.Secondary,
                TextWrapping = TextWrapping.Wrap,
                IsVisible = false,
            }.Named("PART_Status", scope);

            var helper = new Text
            {
                Typo = Typo.BodySmall,
                Color = LoamColor.Secondary,
                TextWrapping = TextWrapping.Wrap,
                IsVisible = false,
            }.Named("PART_Helper", scope);

            return new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    label,
                    button,
                    files,
                    status,
                    helper,
                },
            };
        });
}
