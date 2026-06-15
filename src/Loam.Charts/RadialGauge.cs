using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Media;

namespace Loam.Controls;

/// <summary>
/// A radial gauge: a single <see cref="Value"/> drawn as a filled arc over a track, with an optional center
/// readout and caption. Subclasses <see cref="ChartBase"/>, so it shares the theme palette, tooltip,
/// hover/click, and empty state. The fill is the resolved series color (override via <see cref="ChartBase.Colors"/>);
/// the track uses the theme grid color.
/// </summary>
public sealed class RadialGauge : ChartBase
{
    private double _value;
    private double _minimum;
    private double _maximum = 100;
    private double _startAngle = 135;
    private double _sweepAngle = 270;
    private double _thickness = 14;
    private string? _format;
    private string? _centerText;
    private string? _caption;

    private Point _center;
    private double _radius;
    private double _arcStart;
    private double _arcSweep;

    /// <summary>The value to display. Clamped to [<see cref="Minimum"/>, <see cref="Maximum"/>] for the fill.</summary>
    public double Value
    {
        get => _value;
        set { _value = value; RefreshGauge(); }
    }

    /// <summary>The low end of the range (default 0).</summary>
    public double Minimum
    {
        get => _minimum;
        set { _minimum = value; RefreshGauge(); }
    }

    /// <summary>The high end of the range (default 100).</summary>
    public double Maximum
    {
        get => _maximum;
        set { _maximum = value; RefreshGauge(); }
    }

    /// <summary>The arc start angle in degrees, measured clockwise from due-east (default 135 — lower-left).</summary>
    public double StartAngle
    {
        get => _startAngle;
        set { _startAngle = value; InvalidateVisual(); }
    }

    /// <summary>The total arc length in degrees (default 270 — a bottom-open speedometer arc).</summary>
    public double SweepAngle
    {
        get => _sweepAngle;
        set { _sweepAngle = value; InvalidateVisual(); }
    }

    /// <summary>The ring thickness in pixels (default 14).</summary>
    public double Thickness
    {
        get => _thickness;
        set { _thickness = Math.Max(1, value); InvalidateVisual(); }
    }

    /// <summary>A .NET numeric format for the center readout (e.g. <c>"0.#"</c>, <c>"P0"</c>, <c>"C0"</c>); when null, a compact default is used.</summary>
    public string? Format
    {
        get => _format;
        set { _format = value; RefreshGauge(); }
    }

    /// <summary>Explicit center readout text; when null, <see cref="Value"/> formatted by <see cref="Format"/> is shown.</summary>
    public string? CenterText
    {
        get => _centerText;
        set { _centerText = value; RefreshGauge(); }
    }

    /// <summary>An optional caption drawn beneath the readout (e.g. the metric name).</summary>
    public string? Caption
    {
        get => _caption;
        set { _caption = value; RefreshGauge(); }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(160, 160);

    /// <inheritdoc />
    protected override string ChartAutomationName => "Gauge";

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RefreshGauge();
    }

    /// <inheritdoc />
    protected override ChartPoint[] BuildPoints()
    {
        var fraction = Charts.GaugeFraction(Value, Minimum, Maximum);
        return [new ChartPoint(0, Value, fraction, Caption, SeriesColorAt(0))];
    }

    /// <inheritdoc />
    protected override string DefaultTooltip(ChartPoint point)
    {
        var readout = CenterText ?? FormatValue(point.Value);
        return Caption is { Length: > 0 } ? $"{Caption}: {readout}" : readout;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        // A transparent fill makes the whole control hit-testable (so hover/click reach the arc and the hole).
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

        if (Maximum <= Minimum)
        {
            DrawNoData(context);
            return;
        }

        var size = Math.Min(Bounds.Width, Bounds.Height);
        var radius = size / 2 - Thickness / 2 - 1;
        if (radius <= 0)
        {
            return;
        }

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        _center = center;
        _radius = radius;
        _arcStart = StartAngle;
        _arcSweep = Math.Clamp(SweepAngle, 0, 359.999); // match the range ArcGeometry actually draws so hit-testing agrees

        var track = new Pen(Visuals.Grid, Thickness) { LineCap = PenLineCap.Round };
        context.DrawGeometry(null, track, ArcGeometry(center, radius, StartAngle, SweepAngle));

        var fraction = Charts.GaugeFraction(Value, Minimum, Maximum);
        if (fraction > 0)
        {
            var fill = new Pen(SeriesBrush(0), Thickness) { LineCap = PenLineCap.Round };
            context.DrawGeometry(null, fill, ArcGeometry(center, radius, StartAngle, SweepAngle * fraction));
        }

        DrawReadout(context, center, radius);
        DrawTooltip(context);
    }

    /// <inheritdoc />
    protected override int HitTest(Point local)
    {
        if (_radius <= 0)
        {
            return -1;
        }

        var dx = local.X - _center.X;
        var dy = local.Y - _center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        var half = Thickness / 2 + 4;
        if (distance < _radius - half || distance > _radius + half)
        {
            return -1;
        }

        var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
        while (angle < _arcStart)
        {
            angle += 360;
        }

        return angle <= _arcStart + _arcSweep ? 0 : -1;
    }

    private void RefreshGauge()
    {
        RefreshData(); // rebuilds the single-point snapshot + base automation, then invalidates
        AutomationProperties.SetHelpText(this, GaugeHelpText());
    }

    private string GaugeHelpText()
    {
        if (Maximum <= Minimum)
        {
            return "No data"; // match the visual empty state Render draws for an empty range
        }

        var readout = CenterText ?? FormatValue(Value);
        var range = $"{FormatValue(Minimum)} to {FormatValue(Maximum)}";
        return Caption is { Length: > 0 }
            ? $"{Caption}: {readout} ({range})"
            : $"{readout} ({range})";
    }

    private string FormatValue(double value)
    {
        var finite = double.IsFinite(value) ? value : Minimum; // never surface "NaN"/"Infinity" to users or screen readers
        return Format is { Length: > 0 } format
            ? finite.ToString(format, CultureInfo.CurrentCulture)
            : finite.ToString("0.##", CultureInfo.CurrentCulture);
    }

    private void DrawReadout(DrawingContext context, Point center, double radius)
    {
        var readout = CenterText ?? FormatValue(Value);
        var hasReadout = !string.IsNullOrEmpty(readout);
        var hasCaption = !string.IsNullOrEmpty(Caption);
        if (!hasReadout && !hasCaption)
        {
            return;
        }

        var maxWidth = Math.Max(0, (radius - Thickness) * 2);
        if (maxWidth <= 0)
        {
            return;
        }

        var readoutText = hasReadout ? CenterFormatted(readout, LabelFontSize * 1.6, FontWeight.SemiBold, maxWidth) : null;
        var captionText = hasCaption ? CenterFormatted(Caption!, LabelFontSize * 0.95, FontWeight.Normal, maxWidth) : null;
        var gap = hasReadout && hasCaption ? 2d : 0d;
        var total = (readoutText?.Height ?? 0) + (captionText?.Height ?? 0) + gap;
        var y = center.Y - total / 2;

        if (readoutText is not null)
        {
            context.DrawText(readoutText, new Point(center.X - readoutText.Width / 2, y));
            y += readoutText.Height + gap;
        }

        if (captionText is not null)
        {
            context.DrawText(captionText, new Point(center.X - captionText.Width / 2, y));
        }
    }

    private FormattedText CenterFormatted(string text, double fontSize, FontWeight weight, double maxWidth) =>
        new(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, weight),
            fontSize,
            Visuals.Text)
        {
            MaxTextWidth = maxWidth,
            Trimming = TextTrimming.CharacterEllipsis,
        };

    private static StreamGeometry ArcGeometry(Point center, double radius, double startDeg, double sweepDeg)
    {
        var sweep = Math.Clamp(sweepDeg, 0, 359.999);
        var start = PointOnCircle(center, radius, startDeg);
        var end = PointOnCircle(center, radius, startDeg + sweep);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(start, isFilled: false);
            ctx.ArcTo(end, new Size(radius, radius), 0, sweep > 180, SweepDirection.Clockwise);
            ctx.EndFigure(false);
        }

        return geometry;
    }

    private static Point PointOnCircle(Point center, double radius, double degrees)
    {
        var radians = degrees * Math.PI / 180;
        return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
    }
}
