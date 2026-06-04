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

            var inputBorder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new DockPanel { LastChildFill = true, Children = { startAdornment, endAdornment, textBox } },
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
