using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam;
using Loam.Internal.Templating;
using Loam.Theming;
using AvaPath = Avalonia.Controls.Shapes.Path;

namespace Loam.Controls;

/// <summary>Builds the <see cref="CheckBox"/> theme: a box (<c>PART_Box</c>) holding a checkmark (<c>PART_Check</c>) plus a label; colors set by the control.</summary>
internal static class CheckBoxTheme
{
    public static ControlTheme Create() =>
        new(typeof(CheckBox))
        {
            Setters =
            {
                new Setter(TemplatedControl.TemplateProperty, BuildTemplate()),
                new Setter(Layoutable.VerticalAlignmentProperty, VerticalAlignment.Top),
            },
        };

    private static FuncControlTemplate<CheckBox> BuildTemplate() =>
        new((checkBox, scope) =>
        {
            var check = new AvaPath
            {
                Data = Geometry.Parse(Icons.Material.Filled.Check),
                Stretch = Stretch.Uniform,
                IsVisible = false,
                Margin = new Thickness(3),
            }.Named("PART_Check", scope);

            var box = new Border
            {
                Child = check,
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(4),
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Box", scope);

            var presenter = new ContentPresenter
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
            }.Named("PART_ContentPresenter", scope);
            presenter.Bind(ContentPresenter.ContentProperty, checkBox.GetObservable(ContentControl.ContentProperty));

            var root = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { box, presenter },
            };
            root.Bind(Layoutable.MinHeightProperty, checkBox.GetResourceObservable(LoamTokens.DensityInteractiveMedium));
            return root;
        });
}
