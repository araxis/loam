using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam;
using Loam.Internal.Templating;
using Loam.Theming;
using AC = Avalonia.Controls;

namespace Loam.Controls;

/// <summary>Builds the <see cref="ProgressLinear"/> theme: optional generated label/value text plus track and fill.</summary>
internal static class ProgressLinearTheme
{
    public static ControlTheme Create() =>
        new(typeof(ProgressLinear))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<ProgressLinear> BuildTemplate() =>
        new((progress, scope) =>
        {
            var label = new Text
            {
                Typo = Typo.Body2,
                Color = LoamColor.Default,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_Label", scope);

            var value = new Text
            {
                Typo = Typo.Caption,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            }.Named("PART_ValueText", scope);
            value.Bind(AC.TextBlock.ForegroundProperty, progress.GetResourceObservable(LoamTokens.TextSecondary));

            var header = new AC.Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                IsVisible = false,
                Children = { label, value },
            }.Named("PART_Header", scope);
            AC.Grid.SetColumn(value, 1);

            var track = new Border
            {
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            }.Named("PART_Track", scope);

            var fill = new Border
            {
                Width = 0,
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Left,
            }.Named("PART_Fill", scope);

            var area = new Panel
            {
                Height = 4,
                ClipToBounds = true,
                Children = { track, fill },
            }.Named("PART_Area", scope);

            return new StackPanel
            {
                Spacing = 6,
                Children = { header, area },
            };
        });
}
