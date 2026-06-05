using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Loam.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Shared chart math and compatibility helpers for the chart controls.</summary>
public static class Charts
{
    /// <summary>Legacy categorical series colors used only when theme resources are unavailable.</summary>
    public static readonly IReadOnlyList<Color> Palette = new[]
    {
        Color.Parse("#2196F3"), Color.Parse("#4CAF50"), Color.Parse("#FF9800"), Color.Parse("#E91E63"),
        Color.Parse("#9C27B0"), Color.Parse("#00BCD4"), Color.Parse("#FFC107"), Color.Parse("#795548"),
    };

    /// <summary>Returns each value's slice sweep in degrees (summing to 360, or empty when the total is less than or equal to 0).</summary>
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

    /// <summary>Scales values to pixel heights against the largest value (0 when all values are less than or equal to 0).</summary>
    public static IReadOnlyList<double> BarHeights(IReadOnlyList<double> values, double maxPixels)
    {
        ArgumentNullException.ThrowIfNull(values);
        var max = values.DefaultIfEmpty(0).Max();
        if (max <= 0)
        {
            return values.Select(_ => 0d).ToList();
        }

        var availablePixels = Math.Max(0, maxPixels);
        return values.Select(v => Math.Max(0, v) / max * availablePixels).ToList();
    }

    internal static IReadOnlyList<Point> LinePoints(IReadOnlyList<double> values, double width, double height, double pad = 4)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0 || width <= 0 || height <= 0)
        {
            return Array.Empty<Point>();
        }

        var max = values.DefaultIfEmpty(0).Max();
        if (max <= 0)
        {
            return Array.Empty<Point>();
        }

        var plotHeight = Math.Max(0, height - pad * 2);
        return values.Select((value, index) =>
        {
            var x = values.Count == 1 ? width / 2 : index / (double)(values.Count - 1) * width;
            var y = pad + (1 - Math.Max(0, value) / max) * plotHeight;
            return new Point(x, y);
        }).ToList();
    }

    internal static IImmutableBrush ColorAt(IReadOnlyList<Color> colors, int index)
    {
        var source = colors.Count > 0 ? colors : Palette;
        return new ImmutableSolidColorBrush(source[index % source.Count]);
    }
}

/// <summary>Base for custom-drawn charts: holds <see cref="Values"/>/<see cref="Colors"/> and invalidates render on change.</summary>
public abstract class ChartBase : Control
{
    private static readonly string[] VisualTokens =
    [
        LoamTokens.ColorPrimary,
        LoamTokens.ColorSecondary,
        LoamTokens.ColorTertiary,
        LoamTokens.ColorError,
        LoamTokens.ColorPrimaryContainer,
        LoamTokens.ColorSecondaryContainer,
        LoamTokens.ColorScheme(nameof(LoamColorScheme.TertiaryContainer)),
        LoamTokens.ColorOnSurfaceVariant,
        LoamTokens.ColorSurface,
        LoamTokens.ColorSurfaceContainer,
        LoamTokens.ColorOutline,
        LoamTokens.ColorOutlineVariant,
        LoamTokens.ColorOnSurfaceVariant,
        LoamTokens.TypographyFontSize("LabelMedium"),
    ];

    private readonly List<IDisposable> _visualSubscriptions = [];
    private IReadOnlyList<double> _values = Array.Empty<double>();
    private IReadOnlyList<Color>? _colors;
    private ChartVisuals _visuals = ChartVisuals.Fallback;

    /// <summary>The data values.</summary>
    public IReadOnlyList<double> Values
    {
        get => _values;
        set
        {
            _values = value ?? Array.Empty<double>();
            UpdateAutomation();
            InvalidateVisual();
        }
    }

    /// <summary>Optional explicit series colors (falls back to <see cref="Charts.Palette"/>).</summary>
    public IReadOnlyList<Color>? Colors
    {
        get => _colors;
        set
        {
            _colors = value;
            InvalidateVisual();
        }
    }

    internal ChartVisuals Visuals => _visuals;

    internal IReadOnlyList<Color> ResolvedSeriesColors => _colors is { Count: > 0 } ? _colors : _visuals.SeriesColors;

    internal bool HasPositiveData => Values.Any(value => value > 0);

    internal double LabelFontSize =>
        TryChartResource(LoamTokens.TypographyFontSize("LabelMedium"), out var value) &&
        value is double size
            ? size
            : 13d;

    /// <summary>Default automation name for the concrete chart type.</summary>
    protected abstract string ChartAutomationName { get; }

    /// <summary>Whether the current values contain data that can be drawn.</summary>
    protected bool HasRenderableData => HasPositiveData;

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeVisualTokens();
        RefreshVisuals();
        UpdateAutomation();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DisposeVisualSubscriptions();
    }

    /// <summary>Returns the resolved series brush for the requested series index.</summary>
    protected IImmutableBrush SeriesBrush(int index) => Charts.ColorAt(ResolvedSeriesColors, index);

    /// <summary>Returns the translucent area brush for an area series.</summary>
    protected IImmutableBrush AreaBrush(int index)
    {
        var color = ResolvedSeriesColors.Count == 0 ? Charts.Palette[0] : ResolvedSeriesColors[index % ResolvedSeriesColors.Count];
        return new ImmutableSolidColorBrush(color, _visuals.AreaOpacity);
    }

    /// <summary>Draws the shared tokenized empty chart state.</summary>
    protected void DrawNoData(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var rect = new Rect(1, 1, Math.Max(0, Bounds.Width - 2), Math.Max(0, Bounds.Height - 2));
        context.DrawRectangle(_visuals.EmptySurface, new Pen(_visuals.Outline, 1), rect, 12, 12);

        var text = new FormattedText(
            "No data",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Medium),
            LabelFontSize,
            _visuals.Text);
        var point = new Point(
            Math.Max(0, (Bounds.Width - text.Width) / 2),
            Math.Max(0, (Bounds.Height - text.Height) / 2));
        context.DrawText(text, point);
    }

    /// <summary>Draws a shared low-emphasis grid inside the provided plot rectangle.</summary>
    protected void DrawGrid(DrawingContext context, Rect plot, int lines = 4)
    {
        if (plot.Width <= 0 || plot.Height <= 0)
        {
            return;
        }

        var pen = new Pen(_visuals.Grid, 1);
        for (var i = 0; i <= lines; i++)
        {
            var y = plot.Top + plot.Height * i / lines;
            context.DrawLine(pen, new Point(plot.Left, y), new Point(plot.Right, y));
        }
    }

    private void SubscribeVisualTokens()
    {
        DisposeVisualSubscriptions();
        foreach (var token in VisualTokens)
        {
            _visualSubscriptions.Add(this.GetResourceObservable(token)
                .Subscribe(new AnonObserver<object?>(_ => RefreshVisuals())));
        }
    }

    private void DisposeVisualSubscriptions()
    {
        foreach (var subscription in _visualSubscriptions)
        {
            subscription.Dispose();
        }

        _visualSubscriptions.Clear();
    }

    private void RefreshVisuals()
    {
        _visuals = ChartVisuals.From(this);
        InvalidateVisual();
    }

    private void UpdateAutomation()
    {
        if (string.IsNullOrWhiteSpace(AutomationProperties.GetName(this)))
        {
            AutomationProperties.SetName(this, ChartAutomationName);
        }

        var positiveCount = Values.Count(value => value > 0);
        var helpText = positiveCount == 0
            ? "No data"
            : $"{positiveCount} value{(positiveCount == 1 ? string.Empty : "s")}";
        AutomationProperties.SetHelpText(this, helpText);
    }

    private Color ColorToken(string token, Color fallback)
    {
        if (TryChartResource(token, out var value))
        {
            return value switch
            {
                Color color => color,
                ISolidColorBrush brush => brush.Color,
                _ => fallback,
            };
        }

        return fallback;
    }

    private bool TryChartResource(string token, out object? value)
    {
        var variant = ActualThemeVariant == ThemeVariant.Default
            ? Application.Current?.RequestedThemeVariant ?? ThemeVariant.Light
            : ActualThemeVariant;

        if (this.TryGetResource(token, variant, out value))
        {
            return true;
        }

        var theme = Application.Current?.Styles.OfType<LoamTheme>().FirstOrDefault();
        return theme?.Resources.TryGetResource(token, variant, out value) == true;
    }

    internal sealed record ChartVisuals(
        IReadOnlyList<Color> SeriesColors,
        IImmutableBrush Surface,
        IImmutableBrush EmptySurface,
        IImmutableBrush Outline,
        IImmutableBrush Grid,
        IImmutableBrush Text,
        double AreaOpacity)
    {
        public static ChartVisuals Fallback { get; } = new(
            Charts.Palette,
            Brush(global::Avalonia.Media.Colors.White),
            Brush(Color.Parse("#F7F2FA")),
            Brush(Color.Parse("#CAC4D0")),
            Brush(Color.Parse("#CAC4D0").WithAlpha(0.64)),
            Brush(Color.Parse("#49454F")),
            0.18);

        public static ChartVisuals From(ChartBase chart)
        {
            var fallback = Charts.Palette;
            var series = new[]
            {
                chart.ColorToken(LoamTokens.ColorPrimary, fallback[0]),
                chart.ColorToken(LoamTokens.ColorSecondary, fallback[1]),
                chart.ColorToken(LoamTokens.ColorTertiary, fallback[2]),
                chart.ColorToken(LoamTokens.ColorError, fallback[3]),
                chart.ColorToken(LoamTokens.ColorPrimaryContainer, fallback[4]),
                chart.ColorToken(LoamTokens.ColorSecondaryContainer, fallback[5]),
                chart.ColorToken(LoamTokens.ColorScheme(nameof(LoamColorScheme.TertiaryContainer)), fallback[6]),
                chart.ColorToken(LoamTokens.ColorOnSurfaceVariant, fallback[7]),
            };
            var outline = chart.ColorToken(LoamTokens.ColorOutlineVariant, Color.Parse("#CAC4D0"));

            return new ChartVisuals(
                series,
                Brush(chart.ColorToken(LoamTokens.ColorSurface, global::Avalonia.Media.Colors.White)),
                Brush(chart.ColorToken(LoamTokens.ColorSurfaceContainer, Color.Parse("#F7F2FA"))),
                Brush(chart.ColorToken(LoamTokens.ColorOutline, Color.Parse("#79747E"))),
                Brush(outline.WithAlpha(0.64)),
                Brush(chart.ColorToken(LoamTokens.ColorOnSurfaceVariant, Color.Parse("#49454F"))),
                0.18);
        }

        private static ImmutableSolidColorBrush Brush(Color color) => new(color);
    }
}

/// <summary>
/// A pie (or donut) chart, mirroring the reference API's <c>Chart</c> Pie/Donut. Draws one slice per value,
/// sized by its share of the total; set <see cref="Donut"/> for a ring.
/// </summary>
public sealed class PieChart : ChartBase
{
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
    protected override string ChartAutomationName => "Pie chart";

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var sweeps = Charts.SliceSweeps(Values);
        if (sweeps.Count == 0)
        {
            DrawNoData(context);
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
                context.DrawGeometry(SeriesBrush(i), null, Slice(center, radius, start, sweep));
                start += sweep;
            }
        }

        if (Donut)
        {
            context.DrawEllipse(Visuals.Surface, null, center, radius * HoleRatio, radius * HoleRatio);
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
    protected override string ChartAutomationName => "Bar chart";

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        if (!HasRenderableData)
        {
            DrawNoData(context);
            return;
        }

        const double gap = 8;
        const double pad = 8;
        var plot = new Rect(pad, pad, Math.Max(0, Bounds.Width - pad * 2), Math.Max(0, Bounds.Height - pad * 2));
        DrawGrid(context, plot);
        var plotHeight = plot.Height;
        var heights = Charts.BarHeights(Values, plotHeight);
        var slot = plot.Width / Values.Count;
        var barWidth = Math.Max(1, slot - gap);

        for (var i = 0; i < Values.Count; i++)
        {
            var x = plot.Left + i * slot + (slot - barWidth) / 2;
            var rect = new Rect(x, plot.Bottom - heights[i], barWidth, heights[i]);
            context.DrawRectangle(SeriesBrush(i), null, rect, 4, 4);
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
    protected override string ChartAutomationName => "Line chart";

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        if (!HasRenderableData)
        {
            DrawNoData(context);
            return;
        }

        const double pad = 4;
        var plot = new Rect(0, pad, Bounds.Width, Math.Max(0, Bounds.Height - pad * 2));
        DrawGrid(context, plot);
        var points = Charts.LinePoints(Values, Bounds.Width, Bounds.Height, pad);

        if (points.Count == 0)
        {
            DrawNoData(context);
            return;
        }

        var color = SeriesBrush(0);

        if (Area && points.Count > 1)
        {
            var area = new StreamGeometry();
            using (var ctx = area.Open())
            {
                ctx.BeginFigure(new Point(points[0].X, Bounds.Height - pad), isFilled: true);
                foreach (var p in points)
                {
                    ctx.LineTo(p);
                }

                ctx.LineTo(new Point(points[^1].X, Bounds.Height - pad));
                ctx.EndFigure(true);
            }

            context.DrawGeometry(AreaBrush(0), null, area);
        }

        var pen = new Pen(color, 2) { LineJoin = PenLineJoin.Round };
        for (var i = 1; i < points.Count; i++)
        {
            context.DrawLine(pen, points[i - 1], points[i]);
        }

        foreach (var p in points)
        {
            context.DrawEllipse(color, null, p, 3, 3);
        }
    }
}
