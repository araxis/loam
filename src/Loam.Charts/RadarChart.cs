using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Loam.Controls;

/// <summary>
/// A radar (spider) chart: one axis per category arranged radially, with each series drawn as a polygon whose
/// vertices sit at <c>value / max</c> along their axes. Subclasses <see cref="MultiSeriesChartBase"/>, so it
/// shares the theme palette, multi-<see cref="ChartSeries"/> support, legend, tooltip, hover/click, and empty
/// state. Single-series data comes from <see cref="ChartBase.Values"/>; category names come from
/// <see cref="ChartBase.Labels"/>. Needs at least three axes (categories) to draw.
/// </summary>
public sealed class RadarChart : MultiSeriesChartBase
{
    private double? _max;
    private int _levels = 4;
    private bool _filled = true;
    private Point[] _vertices = Array.Empty<Point>();

    /// <summary>Explicit outer value for every axis; when null, the largest value across the data is used.</summary>
    public double? Max
    {
        get => _max;
        set { _max = value; InvalidateVisual(); }
    }

    /// <summary>Number of concentric grid rings (default 4).</summary>
    public int Levels
    {
        get => _levels;
        set { _levels = Math.Max(1, value); InvalidateVisual(); }
    }

    /// <summary>Whether each series polygon is filled with a translucent area (default true).</summary>
    public bool Filled
    {
        get => _filled;
        set { _filled = value; InvalidateVisual(); }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(240, 240);

    /// <inheritdoc />
    protected override string ChartAutomationName => "Radar chart";

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        _vertices = Array.Empty<Point>();
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        // A transparent fill makes the whole control hit-testable (vertices and gaps both deliver pointer events).
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

        var categories = HasSeries ? SeriesCategoryCount : Values.Count;
        var dataMax = Max ?? ComputeDataMax();
        var hasPositive = (HasSeries ? SeriesList.SelectMany(s => s.Values) : Values).Any(v => v > 0);
        if (categories < 3 || dataMax <= 0 || !hasPositive)
        {
            // A radar needs at least three axes and some positive data (even when an explicit Max is supplied).
            DrawNoData(context);
            return;
        }

        var labelled = Labels is { Count: > 0 };
        var margin = labelled ? 24d : 6d;
        var maxRadius = Math.Min(Bounds.Width, Bounds.Height) / 2 - margin;
        if (maxRadius <= 0)
        {
            return;
        }

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var axisValues = new double[categories];
        for (var c = 0; c < categories; c++)
        {
            axisValues[c] = dataMax;
        }

        var outer = Charts.RadarPoints(axisValues, dataMax, center, maxRadius);

        DrawRadarGrid(context, center, outer);
        DrawAxisLabels(context, center, outer);

        var seriesValues = HasSeries
            ? SeriesList.Select(s => s.Values).ToArray()
            : new[] { Values };

        var vertices = new List<Point>(seriesValues.Length * categories);
        for (var s = 0; s < seriesValues.Length; s++)
        {
            // Always exactly `categories` (>= 3) vertices here, so _vertices stays index-aligned with the snapshot.
            var verts = Charts.RadarPoints(AxisValues(seriesValues[s], categories), dataMax, center, maxRadius);
            var color = HasSeries ? SeriesColor(s) : SeriesColorAt(0);
            var stroke = new ImmutableSolidColorBrush(color);
            var geometry = Polygon(verts);
            if (Filled)
            {
                context.DrawGeometry(new ImmutableSolidColorBrush(color, Visuals.AreaOpacity), null, geometry);
            }

            context.DrawGeometry(null, new Pen(stroke, 2) { LineJoin = PenLineJoin.Round }, geometry);

            for (var c = 0; c < verts.Count; c++)
            {
                var flat = s * categories + c;
                if (flat == HoveredIndex)
                {
                    context.DrawEllipse(stroke, new Pen(Visuals.Surface, 2), verts[c], 5, 5);
                }
                else
                {
                    context.DrawEllipse(stroke, null, verts[c], 3, 3);
                }
            }

            vertices.AddRange(verts);
        }

        _vertices = vertices.ToArray();
        DrawTooltip(context);
    }

    /// <inheritdoc />
    protected override int HitTest(Point local)
    {
        var best = -1;
        var bestDistance = 14d * 14d;
        for (var i = 0; i < _vertices.Length; i++)
        {
            var dx = local.X - _vertices[i].X;
            var dy = local.Y - _vertices[i].Y;
            var distance = dx * dx + dy * dy;
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }

        return best;
    }

    private double ComputeDataMax() =>
        HasSeries
            ? SeriesList.SelectMany(s => s.Values).DefaultIfEmpty(0).Max()
            : Values.DefaultIfEmpty(0).Max();

    private static double[] AxisValues(IReadOnlyList<double> values, int categories)
    {
        var result = new double[categories];
        for (var c = 0; c < categories; c++)
        {
            result[c] = c < values.Count ? values[c] : 0;
        }

        return result;
    }

    private void DrawRadarGrid(DrawingContext context, Point center, IReadOnlyList<Point> outer)
    {
        var pen = new Pen(Visuals.Grid, 1);

        for (var level = 1; level <= Levels; level++)
        {
            var fraction = (double)level / Levels;
            var ring = new Point[outer.Count];
            for (var i = 0; i < outer.Count; i++)
            {
                ring[i] = new Point(
                    center.X + (outer[i].X - center.X) * fraction,
                    center.Y + (outer[i].Y - center.Y) * fraction);
            }

            context.DrawGeometry(null, pen, Polygon(ring));
        }

        foreach (var spoke in outer)
        {
            context.DrawLine(pen, center, spoke);
        }
    }

    private void DrawAxisLabels(DrawingContext context, Point center, IReadOnlyList<Point> outer)
    {
        if (Labels is not { Count: > 0 } labels)
        {
            return;
        }

        for (var i = 0; i < outer.Count && i < labels.Count; i++)
        {
            var label = labels[i];
            if (string.IsNullOrEmpty(label))
            {
                continue;
            }

            var text = DataLabelText(label);
            var dx = outer[i].X - center.X;
            var dy = outer[i].Y - center.Y;
            var length = Math.Sqrt(dx * dx + dy * dy);
            var ux = length > 0 ? dx / length : 0;
            var uy = length > 0 ? dy / length : 0;

            // Center the label on the vertex, then nudge it outward along the axis so it clears the polygon.
            var x = outer[i].X - text.Width / 2 + ux * (text.Width / 2 + 4);
            var y = outer[i].Y - text.Height / 2 + uy * (text.Height / 2 + 4);
            x = Math.Clamp(x, 0, Math.Max(0, Bounds.Width - text.Width));
            y = Math.Clamp(y, 0, Math.Max(0, Bounds.Height - text.Height));
            context.DrawText(text, new Point(x, y));
        }
    }

    private static StreamGeometry Polygon(IReadOnlyList<Point> points)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(points[0], isFilled: true);
            for (var i = 1; i < points.Count; i++)
            {
                ctx.LineTo(points[i]);
            }

            ctx.EndFigure(true);
        }

        return geometry;
    }
}
