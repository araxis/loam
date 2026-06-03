using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;
using Loam.Theming;
using LoamButton = Loam.Controls.Button;
using LoamGrid = Loam.Controls.Grid;

namespace Loam.Gallery;

/// <summary>Phase 4 showcase: responsive <see cref="LoamGrid"/>/<see cref="Item"/>, <see cref="Stack"/> and <see cref="Container"/>.</summary>
public sealed class LayoutView : UserControl
{
    public LayoutView()
    {
        var stack = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 28,
            Children =
            {
                Section("Responsive grid — resize the window to reflow", BuildGrid()),
                Section("Stack", BuildStacks()),
                Section("Container (max-width = Sm, centered)", BuildContainer()),
            },
        };

        var background = new Border { Child = new ScrollViewer { Content = stack } };
        background.Bind(Border.BackgroundProperty, background.GetResourceObservable(LoamTokens.Background));
        Content = background;
    }

    private static StackPanel Section(string title, Control body) =>
        new()
        {
            Children =
            {
                new Text { Text = title, Typo = Typo.H6, Margin = new Thickness(0, 0, 0, 12) },
                body,
            },
        };

    private static LoamGrid BuildGrid()
    {
        var grid = new LoamGrid { Spacing = 12 };
        for (var i = 1; i <= 6; i++)
        {
            grid.Children.Add(new Item
            {
                Xs = 12,
                Sm = 6,
                Md = 4,
                Child = new Paper
                {
                    Height = 76,
                    Elevation = 2,
                    Content = new Text { Text = $"Item {i}\nxs12 · sm6 · md4", Margin = new Thickness(12) },
                },
            });
        }

        return grid;
    }

    private static StackPanel BuildStacks()
    {
        var row = new Stack
        {
            Row = true,
            Children =
            {
                new LoamButton { Content = "One", Variant = Variant.Filled, Color = LoamColor.Primary },
                new LoamButton { Content = "Two", Variant = Variant.Outlined, Color = LoamColor.Primary },
                new LoamButton { Content = "Three", Variant = Variant.Text, Color = LoamColor.Primary },
            },
        };

        var column = new Stack
        {
            Children =
            {
                new Chip { Text = "Alpha", Color = LoamColor.Info },
                new Chip { Text = "Beta", Color = LoamColor.Success },
                new Chip { Text = "Gamma", Color = LoamColor.Warning },
            },
        };

        return new StackPanel
        {
            Spacing = 16,
            Children =
            {
                new Text { Text = "Row", Typo = Typo.Subtitle2 },
                row,
                new Text { Text = "Column", Typo = Typo.Subtitle2 },
                column,
            },
        };
    }

    private static Container BuildContainer() =>
        new()
        {
            MaxWidthBreakpoint = Breakpoint.Sm,
            Child = new Paper
            {
                Height = 64,
                Elevation = 1,
                Content = new Text { Text = "Centered · capped at 600px", Margin = new Thickness(12) },
            },
        };
}
