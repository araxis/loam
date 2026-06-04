using Avalonia;
using Avalonia.Controls;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A responsive 12-column layout, mirroring the reference API's <c>Grid</c>. Arranges <see cref="Item"/>
/// children (or other controls, treated as full-width) into rows, sizing each by its column span at
/// the current breakpoint — derived from the grid's own available width (container-query style).
/// </summary>
public class Grid : Panel
{
    /// <summary>Identifies the <see cref="Spacing"/> property.</summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<Grid, double>(nameof(Spacing), 8);

    static Grid()
    {
        AffectsMeasure<Grid>(SpacingProperty);
        AffectsArrange<Grid>(SpacingProperty);
    }

    /// <summary>The gutter (px) between columns and rows. Mirrors the reference API's <c>Spacing</c>.</summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var width = availableSize.Width;
        if (double.IsInfinity(width) || width <= 0)
        {
            // No bounded width to compute column percentages — measure children naturally and stack.
            var stacked = 0d;
            foreach (var child in Children)
            {
                child.Measure(availableSize);
                stacked += child.DesiredSize.Height;
            }

            return new Size(0, stacked);
        }

        var gutter = Spacing;
        var columnUnit = width / 12d;
        var breakpoint = Breakpoints.FromWidth(width);

        var y = 0d;
        var rowHeight = 0d;
        var rowSpan = 0;

        foreach (var child in Children)
        {
            var span = SpanOf(child, breakpoint);
            if (rowSpan > 0 && rowSpan + span > 12)
            {
                y += rowHeight + gutter;
                rowHeight = 0;
                rowSpan = 0;
            }

            var itemWidth = Math.Max(0, (span * columnUnit) - gutter);
            child.Measure(new Size(itemWidth, double.PositiveInfinity));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
            rowSpan += span;
        }

        return new Size(width, y + rowHeight);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var width = finalSize.Width;
        var gutter = Spacing;
        var columnUnit = width / 12d;
        var breakpoint = Breakpoints.FromWidth(width);

        var x = 0d;
        var y = 0d;
        var rowHeight = 0d;
        var rowSpan = 0;

        foreach (var child in Children)
        {
            var span = SpanOf(child, breakpoint);
            if (rowSpan > 0 && rowSpan + span > 12)
            {
                y += rowHeight + gutter;
                x = 0;
                rowHeight = 0;
                rowSpan = 0;
            }

            var itemWidth = Math.Max(0, (span * columnUnit) - gutter);
            child.Arrange(new Rect(x, y, itemWidth, child.DesiredSize.Height));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
            x += span * columnUnit;
            rowSpan += span;
        }

        return finalSize;
    }

    private static int SpanOf(Control child, Breakpoint breakpoint) =>
        child is Item item ? item.ResolveSpan(breakpoint) : 12;
}
