using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="NumericField"/> theme: the <see cref="TextField"/> chrome plus a vertical spinner (<c>PART_Up</c>/<c>PART_Down</c>) docked beside a borderless <see cref="TextBox"/>.</summary>
internal static class NumericFieldTheme
{
    public static ControlTheme Create() =>
        new(typeof(NumericField))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<NumericField> BuildTemplate() =>
        new((field, scope) =>
        {
            var label = new Text
            {
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                IsVisible = false,
            }.Named("PART_Label", scope);

            var textBox = new TextBox
            {
                BorderThickness = default,
                BorderBrush = Brushes.Transparent,
                Background = Brushes.Transparent,
                Padding = default,
                MinHeight = 24,
                VerticalContentAlignment = VerticalAlignment.Center,
            }.Named("PART_TextBox", scope);
            FieldChrome.ResetInnerTextBox(textBox);

            var spinners = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Children = { Spinner(Icons.Material.Filled.ExpandLess, "PART_Up", scope), Spinner(Icons.Material.Filled.ExpandMore, "PART_Down", scope) } };
            DockPanel.SetDock(spinners, Dock.Right);

            var inputBorder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new DockPanel { LastChildFill = true, Children = { spinners, textBox } },
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

    private static Border Spinner(string icon, string name, Avalonia.Controls.INameScope scope) =>
        new Border
        {
            Child = new Icon { Data = icon, Color = LoamColor.Default, Size = LoamSize.Small },
            Cursor = new Cursor(StandardCursorType.Hand),
            Background = Brushes.Transparent,
            Padding = new Thickness(2, 0),
        }.Named(name, scope);
}
