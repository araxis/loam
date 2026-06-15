using Avalonia;
using Avalonia.Media;

namespace Loam.Controls;

/// <summary>How a <see cref="Sparkline"/> draws its values.</summary>
public enum SparklineMode
{
    /// <summary>A connected line through the values.</summary>
    Line,

    /// <summary>One thin bar per value.</summary>
    Bar,
}

/// <summary>
/// A compact, inline chart for tables, lists, and KPI tiles: it draws <see cref="ChartBase.Values"/> as a
/// small line or bar strip with no axes or data labels, and no tooltip by default. Subclasses
/// <see cref="ChartBase"/>, so it still resolves theme colors and supports hover/click when enabled.
/// Values are shown by magnitude (non-positive values read as zero) — for signed data use <see cref="LineChart"/>.
/// </summary>
public sealed class Sparkline : ChartBase
{
    private SparklineMode _mode = SparklineMode.Line;
    private Point[] _points = Array.Empty<Point>();
    private (Rect Rect, int Index)[] _bars = Array.Empty<(Rect, int)>();

    /// <summary>Creates a sparkline. Tooltips are off by default; set <see cref="ChartBase.ShowTooltip"/> to enable them.</summary>
    public Sparkline()
    {
        ShowTooltip = false;
    }

    /// <summary>Whether to draw a connected line or one bar per value (default <see cref="SparklineMode.Line"/>).</summary>
    public SparklineMode Mode
    {
        get => _mode;
        set { _mode = value; InvalidateVisual(); }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(96, 28);

    /// <inheritdoc />
    protected override string ChartAutomationName => "Sparkline";

    /// <inheritdoc />
    protected override int HitTest(Point local)
    {
        if (Mode == SparklineMode.Bar)
        {
            foreach (var (rect, index) in _bars)
            {
                if (rect.Contains(local))
                {
                    return index;
                }
            }

            return -1;
        }

        var best = -1;
        var bestDistance = 12d * 12d;
        for (var i = 0; i < _points.Length; i++)
        {
            var dx = local.X - _points[i].X;
            var dy = local.Y - _points[i].Y;
            var distance = dx * dx + dy * dy;
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }

        return best;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        _points = Array.Empty<Point>();
        _bars = Array.Empty<(Rect, int)>();

        // Intentionally blank when empty — a sparkline is inline chrome and should not show a "No data" card.
        if (Bounds.Width <= 0 || Bounds.Height <= 0 || !HasRenderableData)
        {
            return;
        }

        // A transparent fill makes the whole strip hit-testable for optional hover/click.
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

        if (Mode == SparklineMode.Bar)
        {
            RenderBars(context);
        }
        else
        {
            RenderLine(context);
        }

        DrawTooltip(context);
    }

    private void RenderLine(DrawingContext context)
    {
        const double pad = 2;
        var points = Charts.LinePoints(Values, Bounds.Width, Bounds.Height, pad);
        if (points.Count == 0)
        {
            return;
        }

        _points = points.ToArray();
        var brush = SeriesBrush(0);
        var pen = new Pen(brush, 1.5) { LineJoin = PenLineJoin.Round, LineCap = PenLineCap.Round };
        for (var i = 1; i < points.Count; i++)
        {
            context.DrawLine(pen, points[i - 1], points[i]);
        }

        // Emphasize the latest point, and the hovered point when hovering is enabled.
        context.DrawEllipse(brush, null, points[^1], 2, 2);
        if (HoveredIndex >= 0 && HoveredIndex < points.Count)
        {
            context.DrawEllipse(brush, new Pen(Visuals.Surface, 1.5), points[HoveredIndex], 3, 3);
        }
    }

    private void RenderBars(DrawingContext context)
    {
        const double pad = 1;
        const double gap = 1;
        var available = Math.Max(0, Bounds.Height - pad * 2);
        var heights = Charts.BarHeights(Values, available);
        if (heights.Count == 0)
        {
            return;
        }

        var brush = SeriesBrush(0);
        var slot = Bounds.Width / heights.Count;
        var barWidth = Math.Max(1, slot - gap);
        var bars = new List<(Rect, int)>();
        for (var i = 0; i < heights.Count; i++)
        {
            var height = heights[i];
            if (height <= 0)
            {
                continue;
            }

            var x = i * slot + (slot - barWidth) / 2;
            var rect = new Rect(x, Bounds.Height - pad - height, barWidth, height);
            var pen = i == HoveredIndex ? new Pen(Visuals.Surface, 1) : null;
            context.DrawRectangle(brush, pen, rect, 1, 1);
            bars.Add((rect, i));
        }

        _bars = bars.ToArray();
    }
}
