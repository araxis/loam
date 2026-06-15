using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Loam.Controls;

/// <summary>A single (X, Y) datapoint for an XY chart, with an optional bubble <see cref="Size"/> and <see cref="Label"/>.</summary>
public sealed record ScatterPoint(double X, double Y, double Size = 0, string? Label = null);

/// <summary>One named, optionally colored series of <see cref="ScatterPoint"/>s for a multi-series XY chart.</summary>
public sealed record ScatterSeries(IReadOnlyList<ScatterPoint> Points, string? Name = null, Color? Color = null);

/// <summary>
/// Shared base for charts plotted on two numeric axes (scatter, bubble). Holds the (X, Y) data — a single
/// <see cref="Points"/> list or multiple <see cref="Series"/> — resolves nice-number X and Y domains, draws the
/// axes, and maps each point into the plot. Subclasses size the markers. Subclasses <see cref="ChartBase"/>, so
/// they share the snapshot, theme palette, tooltip, hover/click, and empty state.
/// </summary>
public abstract class XYChartBase : ChartBase
{
    private IReadOnlyList<ScatterPoint>? _points;
    private IReadOnlyList<ScatterSeries>? _series;
    private bool _showAxes = true;
    private int _axisTickCount = 5;
    private double? _xMin;
    private double? _xMax;
    private double? _yMin;
    private double? _yMax;
    private Func<double, string>? _xAxisFormat;
    private Func<double, string>? _yAxisFormat;
    private (Point Center, double Radius)[] _markers = Array.Empty<(Point, double)>();

    /// <summary>The single-series (X, Y) data. When <see cref="Series"/> is set it takes precedence.</summary>
    public IReadOnlyList<ScatterPoint>? Points
    {
        get => _points;
        set { _points = value; RefreshData(); }
    }

    /// <summary>Optional multiple (X, Y) series. When set (non-empty), it drives rendering and the snapshot instead of <see cref="Points"/>.</summary>
    public IReadOnlyList<ScatterSeries>? Series
    {
        get => _series;
        set { _series = value; RefreshData(); }
    }

    /// <summary>When true (the default), draws numeric X and Y axes with nice-number gridlines and tick labels.</summary>
    public bool ShowAxes
    {
        get => _showAxes;
        set { _showAxes = value; InvalidateVisual(); }
    }

    /// <summary>Approximate number of tick intervals per axis used for nice-number scaling.</summary>
    public int AxisTickCount
    {
        get => _axisTickCount;
        set { _axisTickCount = Math.Max(1, value); InvalidateVisual(); }
    }

    /// <summary>Explicit X-axis minimum; when null, derived from the data.</summary>
    public double? XMin
    {
        get => _xMin;
        set { _xMin = value; InvalidateVisual(); }
    }

    /// <summary>Explicit X-axis maximum; when null, derived from the data.</summary>
    public double? XMax
    {
        get => _xMax;
        set { _xMax = value; InvalidateVisual(); }
    }

    /// <summary>Explicit Y-axis minimum; when null, derived from the data.</summary>
    public double? YMin
    {
        get => _yMin;
        set { _yMin = value; InvalidateVisual(); }
    }

    /// <summary>Explicit Y-axis maximum; when null, derived from the data.</summary>
    public double? YMax
    {
        get => _yMax;
        set { _yMax = value; InvalidateVisual(); }
    }

    /// <summary>Formats X-axis tick labels; when null, a compact numeric format is used.</summary>
    public Func<double, string>? XAxisFormat
    {
        get => _xAxisFormat;
        set { _xAxisFormat = value; InvalidateVisual(); }
    }

    /// <summary>Formats Y-axis tick labels; when null, a compact numeric format is used.</summary>
    public Func<double, string>? YAxisFormat
    {
        get => _yAxisFormat;
        set { _yAxisFormat = value; InvalidateVisual(); }
    }

    /// <summary>True when a non-empty multi-series collection is set.</summary>
    protected bool HasSeries => _series is { Count: > 0 };

    /// <summary>Returns the pixel radius for a marker of the given magnitude (the largest magnitude is <paramref name="maxSize"/>).</summary>
    protected abstract double MarkerRadius(double size, double maxSize);

    /// <inheritdoc />
    protected override ChartPoint[] BuildPoints()
    {
        var series = ActiveSeries();
        if (series.Count == 0)
        {
            return Array.Empty<ChartPoint>();
        }

        var result = new List<ChartPoint>();
        for (var s = 0; s < series.Count; s++)
        {
            var color = series[s].Color ?? SeriesColorAt(s);
            var points = series[s].Points;
            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i];
                result.Add(new ChartPoint(i, p.Y, 0, p.Label, color) { SeriesIndex = s, X = p.X, Size = p.Size });
            }
        }

        return result.ToArray();
    }

    /// <inheritdoc />
    internal override IReadOnlyList<(string Label, Color Color)> GetLegendEntries()
    {
        if (_series is not { Count: > 0 } series)
        {
            return Array.Empty<(string, Color)>(); // single-series scatter has no meaningful legend
        }

        var result = new List<(string Label, Color Color)>(series.Count);
        for (var s = 0; s < series.Count; s++)
        {
            result.Add((series[s].Name ?? $"Series {s + 1}", series[s].Color ?? SeriesColorAt(s)));
        }

        return result;
    }

    /// <inheritdoc />
    protected override string ComputeAutomationHelpText()
    {
        // For XY charts Value is the Y coordinate (can be zero/negative), so count plotted points, not positive values.
        var points = ResolvedPoints;
        if (points.Count == 0)
        {
            return "No data";
        }

        var helpText = $"{points.Count} point{(points.Count == 1 ? string.Empty : "s")}";
        var labelled = string.Join(
            ", ",
            points.Where(point => !string.IsNullOrEmpty(point.Label)).Select(point => point.Label).Distinct());
        return string.IsNullOrEmpty(labelled) ? helpText : $"{helpText}: {labelled}";
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        _markers = Array.Empty<(Point, double)>();
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        // A transparent fill makes the whole control hit-testable (markers and gaps both deliver pointer events).
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

        var points = ResolvedPoints;
        if (points.Count == 0)
        {
            DrawNoData(context);
            return;
        }

        var (xMin, xMax, xStep) = ResolveAxis(points.Select(p => p.X).ToList(), XMin, XMax);
        var (yMin, yMax, yStep) = ResolveAxis(points.Select(p => p.Value).ToList(), YMin, YMax);

        // Inset by the largest marker radius so points at the domain edges stay fully inside the control.
        var maxSize = points.Max(p => p.Size);
        var markerPad = points.Max(p => Math.Max(1, MarkerRadius(p.Size, maxSize)));
        var pad = Math.Max(8, markerPad + 1);
        var leftGutter = MeasureLeftGutter(yMin, yMax, yStep);
        var bottomGutter = ShowAxes ? DataLabelText("0").Height + 4 : 0;
        var plot = new Rect(
            pad + leftGutter,
            pad,
            Math.Max(0, Bounds.Width - pad * 2 - leftGutter),
            Math.Max(0, Bounds.Height - pad * 2 - bottomGutter));
        if (plot.Width <= 0 || plot.Height <= 0)
        {
            return;
        }

        if (ShowAxes)
        {
            DrawAxes(context, plot, xMin, xMax, xStep, yMin, yMax, yStep);
        }
        else
        {
            DrawGrid(context, plot);
        }

        var markers = new (Point, double)[points.Count];
        for (var i = 0; i < points.Count; i++)
        {
            var p = points[i];
            var center = new Point(
                Charts.MapLinear(p.X, xMin, xMax, plot.Left, plot.Right),
                Charts.MapLinear(p.Value, yMin, yMax, plot.Bottom, plot.Top));
            var radius = Math.Max(1, MarkerRadius(p.Size, maxSize));
            DrawMarker(context, center, radius, p.Color, i == HoveredIndex);
            markers[i] = (center, radius);
        }

        _markers = markers;
        DrawTooltip(context);
    }

    /// <inheritdoc />
    protected override int HitTest(Point local)
    {
        var best = -1;
        var bestSlack = double.PositiveInfinity;
        for (var i = 0; i < _markers.Length; i++)
        {
            var (center, radius) = _markers[i];
            var dx = local.X - center.X;
            var dy = local.Y - center.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            var slack = distance - radius; // <= 0 means the pointer is inside the marker
            if (distance <= radius + 6 && slack < bestSlack)
            {
                bestSlack = slack;
                best = i;
            }
        }

        return best;
    }

    /// <inheritdoc />
    protected override string DefaultTooltip(ChartPoint point)
    {
        var xy = $"({FormatX(point.X)}, {FormatY(point.Value)})";
        return string.IsNullOrEmpty(point.Label) ? xy : $"{point.Label}: {xy}";
    }

    /// <summary>Draws one marker. The default is a solid filled dot; overrides may change the fill/outline.</summary>
    protected virtual void DrawMarker(DrawingContext context, Point center, double radius, Color color, bool hovered)
    {
        var brush = new ImmutableSolidColorBrush(color);
        if (hovered)
        {
            context.DrawEllipse(brush, new Pen(Visuals.Surface, 2), center, radius + 1, radius + 1);
        }
        else
        {
            context.DrawEllipse(brush, null, center, radius, radius);
        }
    }

    /// <summary>Formats an X-axis value (override-aware).</summary>
    protected string FormatX(double value) =>
        XAxisFormat?.Invoke(value) ?? value.ToString("0.##", CultureInfo.CurrentCulture);

    /// <summary>Formats a Y-axis value (override-aware).</summary>
    protected string FormatY(double value) =>
        YAxisFormat?.Invoke(value) ?? value.ToString("0.##", CultureInfo.CurrentCulture);

    private IReadOnlyList<ScatterSeries> ActiveSeries()
    {
        if (_series is { Count: > 0 } series)
        {
            return series;
        }

        return _points is { Count: > 0 } points
            ? new[] { new ScatterSeries(points) }
            : Array.Empty<ScatterSeries>();
    }

    private (double Min, double Max, double Step) ResolveAxis(IReadOnlyList<double> values, double? userMin, double? userMax)
    {
        var (dataMin, dataMax) = Charts.Extent(values);
        var lo = userMin ?? dataMin;
        var hi = userMax ?? dataMax;
        if (hi <= lo)
        {
            hi = lo + 1;
        }

        return ShowAxes ? Charts.NiceScale(lo, hi, AxisTickCount) : (lo, hi, hi - lo);
    }

    private double MeasureLeftGutter(double min, double max, double step)
    {
        if (!ShowAxes || step <= 0)
        {
            return 0;
        }

        var widest = 0d;
        for (var value = min; value <= max + step / 2; value += step)
        {
            widest = Math.Max(widest, DataLabelText(FormatY(value)).Width);
        }

        return widest + 6;
    }

    private void DrawAxes(DrawingContext context, Rect plot, double xMin, double xMax, double xStep, double yMin, double yMax, double yStep)
    {
        var pen = new Pen(Visuals.Grid, 1);

        if (yStep > 0)
        {
            for (var value = yMin; value <= yMax + yStep / 2; value += yStep)
            {
                var y = Charts.MapLinear(value, yMin, yMax, plot.Bottom, plot.Top);
                context.DrawLine(pen, new Point(plot.Left, y), new Point(plot.Right, y));
                var text = DataLabelText(FormatY(value));
                context.DrawText(text, new Point(Math.Max(0, plot.Left - text.Width - 4), y - text.Height / 2));
            }
        }

        if (xStep > 0)
        {
            var lastRight = double.NegativeInfinity;
            for (var value = xMin; value <= xMax + xStep / 2; value += xStep)
            {
                var x = Charts.MapLinear(value, xMin, xMax, plot.Left, plot.Right);
                context.DrawLine(pen, new Point(x, plot.Top), new Point(x, plot.Bottom));
                var text = DataLabelText(FormatX(value));
                var lx = Math.Clamp(x - text.Width / 2, 0, Math.Max(0, Bounds.Width - text.Width));
                if (lx >= lastRight + 4)
                {
                    context.DrawText(text, new Point(lx, plot.Bottom + 3));
                    lastRight = lx + text.Width;
                }
            }
        }
    }
}

/// <summary>
/// A scatter chart: plots (X, Y) datapoints as evenly sized markers on two numeric axes. Use <see cref="Points"/>
/// for one series or <see cref="XYChartBase.Series"/> for several. Mirrors the reference API's scatter plot.
/// </summary>
public sealed class ScatterChart : XYChartBase
{
    private double _markerSize = 8;

    /// <summary>The marker diameter in pixels (default 8).</summary>
    public double MarkerSize
    {
        get => _markerSize;
        set { _markerSize = Math.Max(1, value); InvalidateVisual(); }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(320, 220);

    /// <inheritdoc />
    protected override string ChartAutomationName => "Scatter chart";

    /// <inheritdoc />
    protected override double MarkerRadius(double size, double maxSize) => MarkerSize / 2;
}

/// <summary>
/// A bubble chart: a scatter chart whose markers are sized by each point's <see cref="ScatterPoint.Size"/>
/// (area-proportional, between <see cref="MinRadius"/> and <see cref="MaxRadius"/>) and drawn as translucent
/// outlined circles so overlaps stay readable.
/// </summary>
public sealed class BubbleChart : XYChartBase
{
    private double _minRadius = 4;
    private double _maxRadius = 22;

    /// <summary>The radius (px) of the smallest bubble (default 4).</summary>
    public double MinRadius
    {
        get => _minRadius;
        set { _minRadius = Math.Max(1, value); InvalidateVisual(); }
    }

    /// <summary>The radius (px) of the largest bubble (default 22).</summary>
    public double MaxRadius
    {
        get => _maxRadius;
        set { _maxRadius = Math.Max(1, value); InvalidateVisual(); }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(320, 240);

    /// <inheritdoc />
    protected override string ChartAutomationName => "Bubble chart";

    /// <inheritdoc />
    protected override double MarkerRadius(double size, double maxSize) =>
        Charts.BubbleRadius(size, maxSize, MinRadius, MaxRadius);

    /// <inheritdoc />
    protected override void DrawMarker(DrawingContext context, Point center, double radius, Color color, bool hovered)
    {
        var fill = new ImmutableSolidColorBrush(color, 0.33);
        var pen = new Pen(new ImmutableSolidColorBrush(color), hovered ? 2.5 : 1.5);
        context.DrawEllipse(fill, pen, center, radius, radius);
    }

    /// <inheritdoc />
    protected override string DefaultTooltip(ChartPoint point)
    {
        var xy = $"({FormatX(point.X)}, {FormatY(point.Value)})";
        var sized = $"{xy} · {point.Size.ToString("0.##", CultureInfo.CurrentCulture)}";
        return string.IsNullOrEmpty(point.Label) ? sized : $"{point.Label}: {sized}";
    }
}
