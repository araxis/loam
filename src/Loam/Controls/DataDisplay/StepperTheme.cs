using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Stepper"/> theme: a step header (<c>PART_Header</c>), the active step's body (<c>PART_Content</c>), and Back/Next actions (<c>PART_Back</c>/<c>PART_Next</c>).</summary>
internal static class StepperTheme
{
    public static ControlTheme Create() =>
        new(typeof(Stepper))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Stepper> BuildTemplate() =>
        new((stepper, scope) =>
        {
            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
            }.Named("PART_Header", scope);

            var content = new ContentControl { Margin = new Thickness(0, 20) }.Named("PART_Content", scope);

            var back = new Button { Content = "Back", Variant = Variant.Text }.Named("PART_Back", scope);
            var next = new Button { Content = "Next", Variant = Variant.Filled, Color = LoamColor.Primary }.Named("PART_Next", scope);
            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                HorizontalAlignment = HorizontalAlignment.Right,
                Children = { back, next },
            };

            return new StackPanel { Children = { header, content, actions } };
        });
}
