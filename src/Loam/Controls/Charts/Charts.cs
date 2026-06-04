using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Loam.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Shared chart math + the default series palette, mirroring the reference API's <c>Chart</c> helpers.</summary>
public static class Charts
{
    /// <summary>The default categorical series colors (Material 500-ish).</summary>
    public static readonly IReadOnlyList<Color> Palette = new[]
    {
        Color.Parse("#2196F3"), Color.Parse("#4CAF50"), Color.Parse("#FF9800"), Color.Parse("#E91E63"),
        Color.Parse("#9C27B0"), Color.Parse("#00BCD4"), Color.Parse("#FFC107"), Color.Parse("#795548"),
    };

    /// <summary>Returns each value's slice sweep in degrees (summing to 360, or empty when the total is ≤ 0).</summary>
    public static IReadOnlyList<double> SliceSweeps(IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var total = values.Where(v => v > 0).Sum();
        if (total <= 0)
        {
            return Array.Empty<double>();
        }

        return values.Select(v => Math.Max(0, v) / total * 360).ToList();
    }

    /// <summary>Scales values to pixel heights against the largest value (0 when all values are ≤ 0).</summary>
    public static IReadOnlyList<double> BarHeights(IReadOnlyList<double> values, double maxPixels)
    {
        ArgumentNullException.ThrowIfNull(values);
        var max = values.DefaultIfEmpty(0).Max();
        if (max <= 0)
        {
            return values.Select(_ => 0d).ToList();
        }

        return values.Select(v => Math.Max(0, v) / max * maxPixels).ToList();
    }

    internal static IImmutableBrush ColorAt(IReadOnlyList<Color>? colors, int index)
    {
        var source = colors is { Count: > 0 } ? colors : Palette;
        return new ImmutableSolidColorBrush(source[index % source.Count]);
    }
}

/// <summary>Base for custom-drawn charts: holds <see cref="Values"/>/<see cref="Colors"/> and invalidates render on change.</summary>
public abstract class ChartBase : Control
{
    private IReadOnlyList<double> _values = Array.Empty<double>();
    private IReadOnlyList<Color>? _colors;

    /// <summary>The data values.</summary>
    public IReadOnlyList<double> Values
    {
        get => _values;
        set { _values = value ?? Array.Empty<double>(); InvalidateVisual(); }
    }

    /// <summary>Optional explicit series colors (falls back to <see cref="Charts.Palette"/>).</summary>
    public IReadOnlyList<Color>? Colors
    {
        get => _colors;
        set { _colors = value; InvalidateVisual(); }
    }
}

/// <summary>
/// A pie (or donut) chart, mirroring the reference API's <c>Chart</c> Pie/Donut. Draws one slice per value,
/// sized by its share of the total; set <see cref="Donut"/> for a ring.
/// </summary>
public sealed class PieChart : ChartBase
{
    private IDisposable? _surfaceSubscription;
    private IImmutableBrush _surface = new ImmutableSolidColorBrush(global::Avalonia.Media.Colors.White);
    private bool _donut;
    private double _holeRatio = 0.6;

    /// <summary>Whether to render a center hole (donut). Mirrors the reference API's Donut chart.</summary>
    public bool Donut
    {
        get => _donut;
        set { _donut = value; InvalidateVisual(); }
    }

    /// <summary>The hole radius as a fraction of the chart radius (0–1).</summary>
    public double HoleRatio
    {
        get => _holeRatio;
        set { _holeRatio = Math.Clamp(value, 0, 0.95); InvalidateVisual(); }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(220, 220);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _surfaceSubscription = this.GetResourceObservable(LoamTokens.Surface).Subscribe(new AnonObserver<object?>(v =>
        {
            if (v is IImmutableBrush brush)
            {
                _surface = brush;
                InvalidateVisual();
            }
        }));
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _surfaceSubscription?.Dispose();
        _surfaceSubscription = null;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var sweeps = Charts.SliceSweeps(Values);
        if (sweeps.Count == 0)
        {
            return;
        }

        var diameter = Math.Min(Bounds.Width, Bounds.Height);
        var radius = diameter / 2 - 1;
        if (radius <= 0)
        {
            return;
        }

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var start = -90d;
        for (var i = 0; i < sweeps.Count; i++)
        {
            var sweep = sweeps[i];
            if (sweep > 0)
            {
                context.DrawGeometry(Charts.ColorAt(Colors, i), null, Slice(center, radius, start, sweep));
                start += sweep;
            }
        }

        if (Donut)
        {
            context.DrawEllipse(_surface, null, center, radius * HoleRatio, radius * HoleRatio);
        }
    }

    private static StreamGeometry Slice(Point center, double radius, double startDeg, double sweepDeg)
    {
        var capped = Math.Min(sweepDeg, 359.999);
        var startPoint = PointOnCircle(center, radius, startDeg);
        var endPoint = PointOnCircle(center, radius, startDeg + capped);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(center, isFilled: true);
            ctx.LineTo(startPoint);
            ctx.ArcTo(endPoint, new Size(radius, radius), 0, capped > 180, SweepDirection.Clockwise);
            ctx.EndFigure(true);
        }

        return geometry;
    }

    private static Point PointOnCircle(Point center, double radius, double degrees)
    {
        var radians = degrees * Math.PI / 180;
        return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
    }
}

/// <summary>A vertical bar chart, mirroring the reference API's <c>Chart</c> Bar. Bars are scaled to the largest value.</summary>
public sealed class BarChart : ChartBase
{
    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(320, 180);

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        if (Values.Count == 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        const double gap = 8;
        var plotHeight = Bounds.Height - 4;
        var heights = Charts.BarHeights(Values, plotHeight);
        var slot = Bounds.Width / Values.Count;
        var barWidth = Math.Max(1, slot - gap);

        for (var i = 0; i < Values.Count; i++)
        {
            var x = i * slot + (slot - barWidth) / 2;
            var rect = new Rect(x, Bounds.Height - heights[i], barWidth, heights[i]);
            context.DrawRectangle(Charts.ColorAt(Colors, i), null, rect, 2, 2);
        }
    }
}

/// <summary>A line chart, mirroring the reference API's <c>Chart</c> Line. Plots values left-to-right with dots; set <see cref="Area"/> to fill beneath.</summary>
public sealed class LineChart : ChartBase
{
    private bool _area;

    /// <summary>Whether to fill the area beneath the line.</summary>
    public bool Area
    {
        get => _area;
        set { _area = value; InvalidateVisual(); }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(320, 180);

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        if (Values.Count < 2 || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var max = Values.DefaultIfEmpty(0).Max();
        if (max <= 0)
        {
            return;
        }

        const double pad = 4;
        var plotHeight = Bounds.Height - pad * 2;
        var points = new Point[Values.Count];
        for (var i = 0; i < Values.Count; i++)
        {
            var x = Values.Count == 1 ? 0 : i / (double)(Values.Count - 1) * Bounds.Width;
            var y = pad + (1 - Math.Max(0, Values[i]) / max) * plotHeight;
            points[i] = new Point(x, y);
        }

        var color = Charts.ColorAt(Colors, 0);

        if (Area)
        {
            var area = new StreamGeometry();
            using (var ctx = area.Open())
            {
                ctx.BeginFigure(new Point(points[0].X, Bounds.Height), isFilled: true);
                foreach (var p in points)
                {
                    ctx.LineTo(p);
                }

                ctx.LineTo(new Point(points[^1].X, Bounds.Height));
                ctx.EndFigure(true);
            }

            context.DrawGeometry(new ImmutableSolidColorBrush(((ISolidColorBrush)color).Color, 0.18), null, area);
        }

        var pen = new Pen(color, 2) { LineJoin = PenLineJoin.Round };
        for (var i = 1; i < points.Length; i++)
        {
            context.DrawLine(pen, points[i - 1], points[i]);
        }

        foreach (var p in points)
        {
            context.DrawEllipse(color, null, p, 3, 3);
        }
    }
}
