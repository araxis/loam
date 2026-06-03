using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using Loam;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="TextField"/> theme: label + a chrome border (<c>PART_InputBorder</c>) wrapping a borderless <see cref="TextBox"/> + helper/error text; chrome applied by the control.</summary>
internal static class TextFieldTheme
{
    public static ControlTheme Create() =>
        new(typeof(TextField))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<TextField> BuildTemplate() =>
        new((field, scope) =>
        {
            var label = new Text
            {
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                IsVisible = false,
                Margin = new Thickness(0, 0, 0, 3),
            }.Named("PART_Label", scope);

            var textBox = new TextBox
            {
                BorderThickness = default,
                Background = Brushes.Transparent,
                Padding = default,
                MinHeight = 24,
            }.Named("PART_TextBox", scope);

            var inputBorder = new Border { Child = textBox }.Named("PART_InputBorder", scope);

            var helper = new Text
            {
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                IsVisible = false,
                Margin = new Thickness(0, 3, 0, 0),
            }.Named("PART_HelperText", scope);

            return new StackPanel { Children = { label, inputBorder, helper } };
        });
}
