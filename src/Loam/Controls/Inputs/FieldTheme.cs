using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Field"/> theme: label, variant chrome, custom content, adornments, and helper/error text.</summary>
internal static class FieldTheme
{
    public static ControlTheme Create() =>
        new(typeof(Field))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Field> BuildTemplate() =>
        new((field, scope) =>
        {
            var label = new Text
            {
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                IsVisible = false,
            }.Named("PART_Label", scope);

            var startAdornment = new ContentPresenter
            {
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0),
            }.Named("PART_StartAdornment", scope);
            DockPanel.SetDock(startAdornment, Dock.Left);

            var endAdornment = new ContentPresenter
            {
                IsVisible = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            }.Named("PART_EndAdornment", scope);
            DockPanel.SetDock(endAdornment, Dock.Right);

            var content = new ContentPresenter
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            content.Bind(ContentPresenter.ContentProperty, field.GetObservable(ContentControl.ContentProperty));

            var inputBorder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new DockPanel { LastChildFill = true, Children = { startAdornment, endAdornment, content } },
            }.Named("PART_InputBorder", scope);
            var labelHost = FieldChrome.BuildLabelHost(label, field, scope);
            var fieldSurface = new Panel
            {
                ClipToBounds = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Children = { inputBorder, labelHost },
            };

            var helper = new Text
            {
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                IsVisible = false,
                Margin = new Thickness(0, 3, 0, 0),
            }.Named("PART_HelperText", scope);

            return new StackPanel { Children = { fieldSurface, helper } };
        });
}
