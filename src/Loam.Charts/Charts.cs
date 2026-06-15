using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
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

    /// <summary>
    /// The clamped 0..1 position of <paramref name="value"/> within the range [<paramref name="min"/>,
    /// <paramref name="max"/>] — the fill fraction for a gauge. Returns 0 when the range is non-positive.
    /// </summary>
    public static double GaugeFraction(double value, double min, double max)
    {
        var span = max - min;
        if (span <= 0)
        {
            return 0;
        }

        return Math.Clamp((value - min) / span, 0, 1);
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

    /// <summary>
    /// Returns the value-axis domain for signed data, always including zero (<c>Min</c> ≤ 0 ≤ <c>Max</c>)
    /// so positive and negative values share one scale around a zero baseline.
    /// </summary>
    public static (double Min, double Max) SignedDomain(IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var min = Math.Min(0, values.DefaultIfEmpty(0).Min());
        var max = Math.Max(0, values.DefaultIfEmpty(0).Max());
        return (min, max);
    }

    /// <summary>
    /// The pixel offset of the zero baseline from the top of a plot of <paramref name="plotHeight"/>,
    /// for a signed domain spanning <paramref name="min"/>..<paramref name="max"/>.
    /// </summary>
    public static double ZeroBaselineOffset(double min, double max, double plotHeight)
    {
        var span = max - min;
        var height = Math.Max(0, plotHeight);
        return span <= 0 ? height : max / span * height;
    }

    /// <summary>
    /// Lays out signed bars over the domain: each <c>(Y, Height)</c> is the bar's top offset from the plot
    /// top and its pixel height, growing up from the zero baseline for positive values and down for negative.
    /// </summary>
    public static IReadOnlyList<(double Y, double Height)> SignedBarLayout(IReadOnlyList<double> values, double min, double max, double plotHeight)
    {
        ArgumentNullException.ThrowIfNull(values);
        var span = max - min;
        var height = Math.Max(0, plotHeight);
        if (span <= 0)
        {
            return values.Select(_ => (0d, 0d)).ToList();
        }

        var zeroY = max / span * height;
        return values.Select(value =>
        {
            var valueY = (max - value) / span * height;
            return value >= 0 ? (valueY, zeroY - valueY) : (zeroY, valueY - zeroY);
        }).ToList();
    }

    /// <summary>
    /// For one category, returns each series segment's <c>(Y, Height)</c> — its top offset from the plot
    /// top and pixel height — stacking positive values upward over the domain [0, <paramref name="max"/>].
    /// Negative values are treated as zero (stacked bars are positive).
    /// </summary>
    public static IReadOnlyList<(double Y, double Height)> StackedBarHeights(IReadOnlyList<double> categoryValues, double max, double plotHeight)
    {
        ArgumentNullException.ThrowIfNull(categoryValues);
        var height = Math.Max(0, plotHeight);
        if (max <= 0)
        {
            return categoryValues.Select(_ => (height, 0d)).ToList();
        }

        var result = new List<(double Y, double Height)>(categoryValues.Count);
        var cumulative = 0d;
        foreach (var raw in categoryValues)
        {
            var value = Math.Max(0, raw);
            var top = (1 - (cumulative + value) / max) * height;
            var bottom = (1 - cumulative / max) * height;
            result.Add((top, bottom - top));
            cumulative += value;
        }

        return result;
    }

    /// <summary>Maps values to plot points over an explicit signed domain (the signed generalization of <see cref="LinePoints"/>).</summary>
    internal static IReadOnlyList<Point> ScaledLinePoints(IReadOnlyList<double> values, double width, double height, double min, double max, double pad = 4)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0 || width <= 0 || height <= 0)
        {
            return Array.Empty<Point>();
        }

        var span = max - min;
        if (span <= 0)
        {
            return Array.Empty<Point>();
        }

        var plotHeight = Math.Max(0, height - pad * 2);
        return values.Select((value, index) =>
        {
            var x = values.Count == 1 ? width / 2 : index / (double)(values.Count - 1) * width;
            var y = pad + (max - value) / span * plotHeight;
            return new Point(x, y);
        }).ToList();
    }

    /// <summary>Maps values to points inside an explicit plot rectangle over a signed domain.</summary>
    internal static IReadOnlyList<Point> ScaledLinePoints(IReadOnlyList<double> values, Rect plot, double min, double max)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0 || plot.Width <= 0 || plot.Height <= 0)
        {
            return Array.Empty<Point>();
        }

        var span = max - min;
        if (span <= 0)
        {
            return Array.Empty<Point>();
        }

        return values.Select((value, index) =>
        {
            var x = values.Count == 1
                ? plot.Left + plot.Width / 2
                : plot.Left + index / (double)(values.Count - 1) * plot.Width;
            var y = plot.Top + (max - value) / span * plot.Height;
            return new Point(x, y);
        }).ToList();
    }

    /// <summary>
    /// Produces a "nice" axis scale (rounded min, max, and step) covering <paramref name="dataMin"/>..
    /// <paramref name="dataMax"/> with roughly <paramref name="targetTicks"/> intervals, using the
    /// classic 1/2/5×10ⁿ rounding so tick labels read cleanly.
    /// </summary>
    public static (double Min, double Max, double Step) NiceScale(double dataMin, double dataMax, int targetTicks)
    {
        var ticks = Math.Max(1, targetTicks);
        if (dataMax <= dataMin)
        {
            dataMax = dataMin + 1;
        }

        var range = NiceNum(dataMax - dataMin, round: false);
        var step = NiceNum(range / ticks, round: true);
        var min = Math.Floor(dataMin / step) * step;
        var max = Math.Ceiling(dataMax / step) * step;
        return (min, max, step);
    }

    /// <summary>Convenience overload producing a nice scale from zero to <paramref name="dataMax"/>.</summary>
    public static (double Min, double Max, double Step) NiceScale(double dataMax, int targetTicks) =>
        NiceScale(0, dataMax, targetTicks);

    private static double NiceNum(double value, bool round)
    {
        if (value <= 0)
        {
            return 1;
        }

        var exponent = Math.Floor(Math.Log10(value));
        var fraction = value / Math.Pow(10, exponent);
        var niceFraction = round
            ? fraction < 1.5 ? 1 : fraction < 3 ? 2 : fraction < 7 ? 5 : 10
            : fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10;
        return niceFraction * Math.Pow(10, exponent);
    }

    internal static IImmutableBrush ColorAt(IReadOnlyList<Color> colors, int index)
    {
        var source = colors.Count > 0 ? colors : Palette;
        return new ImmutableSolidColorBrush(source[index % source.Count]);
    }
}

/// <summary>
/// An immutable projection of a single chart datapoint, shared by rendering, automation, tooltips, and
/// legends so visible and spoken output never drift. <see cref="Percent"/> is the value's share of the
/// positive total (0 for non-positive values).
/// </summary>
public readonly record struct ChartPoint(int Index, double Value, double Percent, string? Label, Color Color)
{
    /// <summary>The series this point belongs to (0 for single-series charts).</summary>
    public int SeriesIndex { get; init; }
}

/// <summary>One named, optionally colored data series for a multi-series chart.</summary>
public sealed record ChartSeries(IReadOnlyList<double> Values, string? Name = null, Color? Color = null);

/// <summary>How a multi-series <see cref="BarChart"/> arranges its bars.</summary>
public enum BarStackMode
{
    /// <summary>Series render side-by-side within each category.</summary>
    Grouped,

    /// <summary>Series stack on top of each other within each category.</summary>
    Stacked,

    /// <summary>Series stack and each category is normalized to 100%.</summary>
    StackedPercent,
}

/// <summary>Event data for chart pointer interactions. <see cref="Index"/> is -1 (and <see cref="Point"/> null) when the pointer is over no datapoint.</summary>
public sealed class ChartPointEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public ChartPointEventArgs(int index, ChartPoint? point)
    {
        Index = index;
        Point = point;
    }

    /// <summary>Index of the datapoint, or -1 when none.</summary>
    public int Index { get; }

    /// <summary>The datapoint snapshot, or null when none.</summary>
    public ChartPoint? Point { get; }
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
    private IReadOnlyList<string>? _labels;
    private ChartPoint[] _points = Array.Empty<ChartPoint>();
    private bool _showDataLabels;
    private Func<ChartPoint, string>? _dataLabelFormat;
    private bool _showTooltip = true;
    private Func<ChartPoint, string>? _tooltipFormat;
    private int _hoveredIndex = -1;
    private Point _pointer;
    private IEnumerable? _itemsSource;
    private Func<object, double>? _valueSelector;
    private Func<object, string>? _labelSelector;
    private Func<object, Color?>? _colorSelector;
    private INotifyCollectionChanged? _incc;
    private ChartVisuals _visuals = ChartVisuals.Fallback;

    /// <summary>The data values.</summary>
    public IReadOnlyList<double> Values
    {
        get => _values;
        set
        {
            _values = value ?? Array.Empty<double>();
            RebuildPoints();
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
            RebuildPoints();
            InvalidateVisual();
        }
    }

    /// <summary>Optional per-point labels, aligned by index to <see cref="Values"/>. Surfaced via <see cref="ResolvedPoints"/> and accessibility help text.</summary>
    public IReadOnlyList<string>? Labels
    {
        get => _labels;
        set
        {
            _labels = value;
            RebuildPoints();
            UpdateAutomation();
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Optional bound data source. When non-null, each item is projected via <see cref="ValueSelector"/>/
    /// <see cref="LabelSelector"/>/<see cref="ColorSelector"/> into <see cref="Values"/>/<see cref="Labels"/>/
    /// <see cref="Colors"/>, and an <see cref="INotifyCollectionChanged"/> source refreshes the chart live.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        set
        {
            if (ReferenceEquals(_itemsSource, value))
            {
                return;
            }

            _itemsSource = value;
            SubscribeItems();
            ProjectItemsSource();
        }
    }

    /// <summary>Projects an <see cref="ItemsSource"/> item to its numeric value.</summary>
    public Func<object, double>? ValueSelector
    {
        get => _valueSelector;
        set { _valueSelector = value; ProjectItemsSource(); }
    }

    /// <summary>Projects an <see cref="ItemsSource"/> item to its label (optional).</summary>
    public Func<object, string>? LabelSelector
    {
        get => _labelSelector;
        set { _labelSelector = value; ProjectItemsSource(); }
    }

    /// <summary>Projects an <see cref="ItemsSource"/> item to a color; return null to use the theme series color for that point.</summary>
    public Func<object, Color?>? ColorSelector
    {
        get => _colorSelector;
        set { _colorSelector = value; ProjectItemsSource(); }
    }

    /// <summary>When true, draws per-point value annotations on the chart (with responsive thinning to avoid overlap).</summary>
    public bool ShowDataLabels
    {
        get => _showDataLabels;
        set { _showDataLabels = value; InvalidateVisual(); }
    }

    /// <summary>Formats each data label from its <see cref="ChartPoint"/>; when null, a per-chart default is used.</summary>
    public Func<ChartPoint, string>? DataLabelFormat
    {
        get => _dataLabelFormat;
        set { _dataLabelFormat = value; InvalidateVisual(); }
    }

    /// <summary>When true, draws a tokenized tooltip near the pointer for the hovered datapoint.</summary>
    public bool ShowTooltip
    {
        get => _showTooltip;
        set { _showTooltip = value; InvalidateVisual(); }
    }

    /// <summary>Formats the tooltip text from a <see cref="ChartPoint"/>; when null, a per-chart default is used.</summary>
    public Func<ChartPoint, string>? TooltipFormat
    {
        get => _tooltipFormat;
        set { _tooltipFormat = value; InvalidateVisual(); }
    }

    /// <summary>Index of the datapoint currently under the pointer, or -1.</summary>
    public int HoveredIndex => _hoveredIndex;

    /// <summary>Raised when the hovered datapoint changes (index -1 when the pointer leaves all datapoints).</summary>
    public event EventHandler<ChartPointEventArgs>? HoverChanged;

    /// <summary>Raised when a datapoint is clicked.</summary>
    public event EventHandler<ChartPointEventArgs>? PointClicked;

    internal ChartVisuals Visuals => _visuals;

    internal IReadOnlyList<Color> ResolvedSeriesColors => _colors is { Count: > 0 } ? _colors : _visuals.SeriesColors;

    internal bool HasPositiveData => Values.Any(value => value > 0);

    internal bool HasSignedData => Values.Any(value => value != 0);

    /// <summary>The current per-point snapshot, rebuilt whenever values, colors, or labels change.</summary>
    protected internal IReadOnlyList<ChartPoint> ResolvedPoints => _points;

    internal double LabelFontSize =>
        TryChartResource(LoamTokens.TypographyFontSize("LabelMedium"), out var value) &&
        value is double size
            ? size
            : 13d;

    /// <summary>Default automation name for the concrete chart type.</summary>
    protected abstract string ChartAutomationName { get; }

    /// <summary>Returns the index of the datapoint at the given local point, or -1 when none. Charts record their drawn geometry during <c>Render</c>.</summary>
    protected abstract int HitTest(Point local);

    /// <summary>Whether the current values contain data that can be drawn.</summary>
    protected bool HasRenderableData => HasPositiveData;

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeVisualTokens();
        SubscribeItems();
        RefreshVisuals();
        UpdateAutomation();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DisposeVisualSubscriptions();
        UnsubscribeItems();
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _pointer = e.GetPosition(this);
        var index = HitTest(_pointer);
        if (index != _hoveredIndex)
        {
            SetHovered(index);
        }
        else if (ShowTooltip && index >= 0)
        {
            InvalidateVisual();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        SetHovered(-1);
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var index = HitTest(e.GetPosition(this));
        if (index >= 0 && index < _points.Length)
        {
            PointClicked?.Invoke(this, new ChartPointEventArgs(index, _points[index]));
        }
    }

    private void SetHovered(int index)
    {
        if (index == _hoveredIndex)
        {
            return;
        }

        _hoveredIndex = index;
        var point = index >= 0 && index < _points.Length ? _points[index] : (ChartPoint?)null;
        HoverChanged?.Invoke(this, new ChartPointEventArgs(index, point));
        InvalidateVisual();
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

    /// <summary>Resolves the display text for a datapoint label using <see cref="DataLabelFormat"/> or the chart default.</summary>
    protected string ResolveDataLabel(ChartPoint point) =>
        DataLabelFormat?.Invoke(point) ?? DefaultDataLabel(point);

    /// <summary>The default per-point label text when no <see cref="DataLabelFormat"/> is supplied.</summary>
    protected virtual string DefaultDataLabel(ChartPoint point) =>
        point.Value.ToString("0.##", CultureInfo.CurrentCulture);

    /// <summary>Builds a tokenized data-label <see cref="FormattedText"/> (uses the chart text brush unless one is supplied).</summary>
    protected FormattedText DataLabelText(string text, IImmutableBrush? brush = null) =>
        new(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Medium),
            LabelFontSize,
            brush ?? _visuals.Text);

    /// <summary>Picks a readable on-fill text brush (near-black or white) for a given background fill color.</summary>
    protected static IImmutableBrush ContrastBrush(Color background)
    {
        var luminance = (0.299 * background.R + 0.587 * background.G + 0.114 * background.B) / 255.0;
        return luminance > 0.6
            ? new ImmutableSolidColorBrush(Color.FromRgb(0x1C, 0x1B, 0x1F))
            : new ImmutableSolidColorBrush(global::Avalonia.Media.Colors.White);
    }

    /// <summary>Resolves the tooltip text for a datapoint using <see cref="TooltipFormat"/> or the chart default.</summary>
    protected string ResolveTooltip(ChartPoint point) =>
        TooltipFormat?.Invoke(point) ?? DefaultTooltip(point);

    /// <summary>The default tooltip text when no <see cref="TooltipFormat"/> is supplied.</summary>
    protected virtual string DefaultTooltip(ChartPoint point)
    {
        var value = point.Value.ToString("0.##", CultureInfo.CurrentCulture);
        return string.IsNullOrEmpty(point.Label) ? value : $"{point.Label}: {value}";
    }

    /// <summary>Draws the hover tooltip for the currently hovered datapoint near the pointer (when enabled).</summary>
    protected void DrawTooltip(DrawingContext context)
    {
        if (!ShowTooltip || _hoveredIndex < 0 || _hoveredIndex >= _points.Length)
        {
            return;
        }

        var text = new FormattedText(
            ResolveTooltip(_points[_hoveredIndex]),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Medium),
            LabelFontSize,
            _visuals.Text);

        const double padX = 8;
        const double padY = 5;
        const double offset = 14;
        var w = text.Width + padX * 2;
        var h = text.Height + padY * 2;
        var x = _pointer.X + offset;
        var y = _pointer.Y + offset;
        if (x + w > Bounds.Width)
        {
            x = _pointer.X - offset - w;
        }

        if (y + h > Bounds.Height)
        {
            y = _pointer.Y - offset - h;
        }

        x = Math.Clamp(x, 0, Math.Max(0, Bounds.Width - w));
        y = Math.Clamp(y, 0, Math.Max(0, Bounds.Height - h));

        var rect = new Rect(x, y, w, h);
        context.DrawRectangle(_visuals.EmptySurface, new Pen(_visuals.Outline, 1), rect, 6, 6);
        context.DrawText(text, new Point(x + padX, y + padY));
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
        if (_itemsSource is not null)
        {
            ProjectItemsSource();
        }
        else
        {
            RebuildPoints();
        }

        InvalidateVisual();
    }

    private void SubscribeItems()
    {
        UnsubscribeItems();
        if (_itemsSource is INotifyCollectionChanged incc)
        {
            _incc = incc;
            incc.CollectionChanged += OnItemsCollectionChanged;
        }
    }

    private void UnsubscribeItems()
    {
        if (_incc is not null)
        {
            _incc.CollectionChanged -= OnItemsCollectionChanged;
            _incc = null;
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => ProjectItemsSource();

    private void ProjectItemsSource()
    {
        if (_itemsSource is null)
        {
            return;
        }

        var values = new List<double>();
        var labels = _labelSelector is not null ? new List<string>() : null;
        List<Color>? colors = _colorSelector is not null ? new List<Color>() : null;
        var palette = _visuals.SeriesColors.Count > 0 ? _visuals.SeriesColors : Charts.Palette;
        var index = 0;
        foreach (var item in _itemsSource)
        {
            values.Add(_valueSelector?.Invoke(item) ?? 0);
            labels?.Add(_labelSelector!.Invoke(item) ?? string.Empty);
            colors?.Add(_colorSelector!.Invoke(item) ?? palette[index % palette.Count]);
            index++;
        }

        _values = values.ToArray();
        _labels = labels?.ToArray();
        _colors = colors?.ToArray();
        RebuildPoints();
        UpdateAutomation();
        InvalidateVisual();
    }

    /// <summary>Raised after the per-point snapshot is rebuilt (data/colors/labels changed). Used by a bound legend.</summary>
    internal event EventHandler? SnapshotChanged;

    private void RebuildPoints()
    {
        _points = BuildPoints();
        SnapshotChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Legend entries (label + color) derived from the current data. Multi-series charts override this.</summary>
    internal virtual IReadOnlyList<(string Label, Color Color)> GetLegendEntries()
    {
        var result = new List<(string Label, Color Color)>(_points.Length);
        foreach (var point in _points)
        {
            result.Add((point.Label ?? string.Empty, point.Color));
        }

        return result;
    }

    /// <summary>Builds the per-point snapshot from the current data. Overridden by multi-series charts.</summary>
    protected virtual ChartPoint[] BuildPoints()
    {
        var values = _values;
        if (values.Count == 0)
        {
            return Array.Empty<ChartPoint>();
        }

        var colors = ResolvedSeriesColors;
        var positiveTotal = values.Where(value => value > 0).Sum();
        var points = new ChartPoint[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            var value = values[i];
            var percent = positiveTotal > 0 && value > 0 ? value / positiveTotal : 0d;
            var label = _labels is { } labels && i < labels.Count ? labels[i] : null;
            var color = colors.Count > 0
                ? colors[i % colors.Count]
                : Charts.Palette[i % Charts.Palette.Count];
            points[i] = new ChartPoint(i, value, percent, label, color);
        }

        return points;
    }

    /// <summary>The resolved theme/explicit series color for the given series index.</summary>
    private protected Color SeriesColorAt(int index)
    {
        var colors = ResolvedSeriesColors;
        return colors.Count > 0 ? colors[index % colors.Count] : Charts.Palette[index % Charts.Palette.Count];
    }

    /// <summary>Rebuilds the snapshot and accessible state, then invalidates render. Call after data-shaping changes.</summary>
    private protected void RefreshData()
    {
        RebuildPoints();
        UpdateAutomation();
        InvalidateVisual();
    }

    private void UpdateAutomation()
    {
        if (string.IsNullOrWhiteSpace(AutomationProperties.GetName(this)))
        {
            AutomationProperties.SetName(this, ChartAutomationName);
        }

        var positiveCount = Values.Count(value => value > 0);
        string helpText;
        if (positiveCount == 0)
        {
            helpText = "No data";
        }
        else
        {
            helpText = $"{positiveCount} value{(positiveCount == 1 ? string.Empty : "s")}";
            if (_labels is { Count: > 0 })
            {
                var labelled = string.Join(
                    ", ",
                    _points.Where(point => point.Value > 0 && !string.IsNullOrEmpty(point.Label))
                        .Select(point => point.Label));
                if (!string.IsNullOrEmpty(labelled))
                {
                    helpText = $"{helpText}: {labelled}";
                }
            }
        }

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
    private string? _centerText;
    private string? _centerSubText;
    private double? _centerValue;
    private string? _centerValueFormat;
    private Point _center;
    private double _radius;
    private double _holeRadius;
    private (double Start, double End, int Index)[] _sliceRanges = Array.Empty<(double, double, int)>();

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

    /// <summary>Primary text drawn in the donut hole (e.g. a total or KPI). Ignored unless <see cref="Donut"/> is true.</summary>
    public string? CenterText
    {
        get => _centerText;
        set { _centerText = value; InvalidateVisual(); }
    }

    /// <summary>Secondary caption drawn under <see cref="CenterText"/> in the donut hole.</summary>
    public string? CenterSubText
    {
        get => _centerSubText;
        set { _centerSubText = value; InvalidateVisual(); }
    }

    /// <summary>Explicit value formatted by <see cref="CenterValueFormat"/>; when null, the positive-value total is used.</summary>
    public double? CenterValue
    {
        get => _centerValue;
        set { _centerValue = value; InvalidateVisual(); }
    }

    /// <summary>A .NET numeric format string (e.g. <c>"C0"</c>) used to render the center value when <see cref="CenterText"/> is not set.</summary>
    public string? CenterValueFormat
    {
        get => _centerValueFormat;
        set { _centerValueFormat = value; InvalidateVisual(); }
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
        var holeRadius = Donut ? radius * HoleRatio : 0;
        _center = center;
        _radius = radius;
        _holeRadius = holeRadius;

        var ranges = new List<(double, double, int)>();
        var start = -90d;
        for (var i = 0; i < sweeps.Count; i++)
        {
            var sweep = sweeps[i];
            if (sweep > 0)
            {
                var pen = i == HoveredIndex ? new Pen(Visuals.Surface, 2) : null;
                context.DrawGeometry(SeriesBrush(i), pen, Slice(center, radius, start, sweep));
                ranges.Add((start, start + sweep, i));
                start += sweep;
            }
        }

        _sliceRanges = ranges.ToArray();

        if (Donut)
        {
            context.DrawEllipse(Visuals.Surface, null, center, holeRadius, holeRadius);
            DrawCenterText(context, center, holeRadius);
        }

        if (ShowDataLabels)
        {
            DrawSliceLabels(context, center, radius, sweeps);
        }

        DrawTooltip(context);
    }

    /// <inheritdoc />
    protected override int HitTest(Point local)
    {
        var dx = local.X - _center.X;
        var dy = local.Y - _center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance > _radius || distance < _holeRadius)
        {
            return -1;
        }

        var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
        foreach (var (rangeStart, rangeEnd, index) in _sliceRanges)
        {
            var a = angle;
            while (a < rangeStart)
            {
                a += 360;
            }

            if (a < rangeEnd)
            {
                return index;
            }
        }

        return -1;
    }

    /// <inheritdoc />
    protected override string DefaultDataLabel(ChartPoint point) =>
        point.Percent.ToString("P0", CultureInfo.CurrentCulture);

    /// <inheritdoc />
    protected override string DefaultTooltip(ChartPoint point)
    {
        var value = point.Value.ToString("0.##", CultureInfo.CurrentCulture);
        var percent = point.Percent.ToString("P0", CultureInfo.CurrentCulture);
        return string.IsNullOrEmpty(point.Label)
            ? $"{value} ({percent})"
            : $"{point.Label}: {value} ({percent})";
    }

    private void DrawSliceLabels(DrawingContext context, Point center, double radius, IReadOnlyList<double> sweeps)
    {
        var labelRadius = Donut ? radius * (1 + HoleRatio) / 2 : radius * 0.62;
        var angle = -90d;
        for (var i = 0; i < sweeps.Count; i++)
        {
            var sweep = sweeps[i];
            if (sweep >= 16 && i < ResolvedPoints.Count)
            {
                var pos = PointOnCircle(center, labelRadius, angle + sweep / 2);
                var point = ResolvedPoints[i];
                var text = DataLabelText(ResolveDataLabel(point), ContrastBrush(point.Color));
                context.DrawText(text, new Point(pos.X - text.Width / 2, pos.Y - text.Height / 2));
            }

            angle += sweep;
        }
    }

    private void DrawCenterText(DrawingContext context, Point center, double holeRadius)
    {
        var primary = CenterText;
        if (string.IsNullOrEmpty(primary) && CenterValueFormat is { } format)
        {
            var value = CenterValue ?? Values.Where(v => v > 0).Sum();
            primary = value.ToString(format, CultureInfo.CurrentCulture);
        }

        var hasPrimary = !string.IsNullOrEmpty(primary);
        var hasSub = !string.IsNullOrEmpty(CenterSubText);
        if (!hasPrimary && !hasSub)
        {
            return;
        }

        var maxWidth = holeRadius * 2 - 8;
        if (maxWidth <= 0)
        {
            return;
        }

        var baseSize = LabelFontSize;
        var primaryText = hasPrimary ? BuildCenterText(primary!, baseSize * 1.5, FontWeight.SemiBold, maxWidth) : null;
        var subText = hasSub ? BuildCenterText(CenterSubText!, baseSize * 0.95, FontWeight.Normal, maxWidth) : null;

        var gap = hasPrimary && hasSub ? 2d : 0d;
        var totalHeight = (primaryText?.Height ?? 0) + (subText?.Height ?? 0) + gap;
        var y = center.Y - totalHeight / 2;

        if (primaryText is not null)
        {
            context.DrawText(primaryText, new Point(center.X - primaryText.Width / 2, y));
            y += primaryText.Height + gap;
        }

        if (subText is not null)
        {
            context.DrawText(subText, new Point(center.X - subText.Width / 2, y));
        }
    }

    private FormattedText BuildCenterText(string text, double size, FontWeight weight, double maxWidth) =>
        new(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, weight),
            size,
            Visuals.Text)
        {
            MaxTextWidth = maxWidth,
            Trimming = TextTrimming.CharacterEllipsis,
        };

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

/// <summary>Shared base for Cartesian charts (bar, line): value-axis domain, optional axes, and gutters.</summary>
public abstract class CartesianChartBase : ChartBase
{
    private bool _showAxes;
    private double? _min;
    private double? _max;
    private int _yAxisTickCount = 4;
    private Func<double, string>? _yAxisFormat;

    /// <summary>When true, draws a numeric Y-axis and category X-axis (nice-number scaled) in reserved gutters.</summary>
    public bool ShowAxes
    {
        get => _showAxes;
        set { _showAxes = value; InvalidateVisual(); }
    }

    /// <summary>Explicit value-axis minimum; when null, derived from the data (and zero).</summary>
    public double? Min
    {
        get => _min;
        set { _min = value; InvalidateVisual(); }
    }

    /// <summary>Explicit value-axis maximum; when null, derived from the data.</summary>
    public double? Max
    {
        get => _max;
        set { _max = value; InvalidateVisual(); }
    }

    /// <summary>Approximate number of Y-axis tick intervals used for nice-number scaling.</summary>
    public int YAxisTickCount
    {
        get => _yAxisTickCount;
        set { _yAxisTickCount = Math.Max(1, value); InvalidateVisual(); }
    }

    /// <summary>Formats Y-axis tick labels; when null, a compact numeric format is used.</summary>
    public Func<double, string>? YAxisFormat
    {
        get => _yAxisFormat;
        set { _yAxisFormat = value; InvalidateVisual(); }
    }

    private IReadOnlyList<ChartSeries>? _series;

    /// <summary>Optional multiple data series. When set (non-empty), it drives rendering and the snapshot instead of <see cref="ChartBase.Values"/>.</summary>
    public IReadOnlyList<ChartSeries>? Series
    {
        get => _series;
        set { _series = value; RefreshData(); }
    }

    /// <summary>True when a non-empty multi-series collection is set.</summary>
    protected bool HasSeries => _series is { Count: > 0 };

    /// <summary>The active series collection (non-null only when <see cref="HasSeries"/>).</summary>
    private protected IReadOnlyList<ChartSeries> SeriesList => _series!;

    /// <summary>The number of categories across all series.</summary>
    private protected int SeriesCategoryCount => HasSeries ? _series!.Max(s => s.Values.Count) : 0;

    /// <summary>The resolved color for a series (its explicit color, or the theme color for its index).</summary>
    private protected Color SeriesColor(int seriesIndex) => _series![seriesIndex].Color ?? SeriesColorAt(seriesIndex);

    /// <inheritdoc />
    internal override IReadOnlyList<(string Label, Color Color)> GetLegendEntries()
    {
        if (_series is not { Count: > 0 } series)
        {
            return base.GetLegendEntries();
        }

        var result = new List<(string Label, Color Color)>(series.Count);
        for (var s = 0; s < series.Count; s++)
        {
            result.Add((series[s].Name ?? $"Series {s + 1}", series[s].Color ?? SeriesColorAt(s)));
        }

        return result;
    }

    /// <inheritdoc />
    protected override ChartPoint[] BuildPoints()
    {
        if (_series is not { Count: > 0 } series)
        {
            return base.BuildPoints();
        }

        var categories = series.Max(s => s.Values.Count);
        var positiveTotal = series.SelectMany(s => s.Values).Where(v => v > 0).Sum();
        var points = new List<ChartPoint>(series.Count * categories);
        for (var s = 0; s < series.Count; s++)
        {
            var color = series[s].Color ?? SeriesColorAt(s);
            for (var c = 0; c < categories; c++)
            {
                var value = c < series[s].Values.Count ? series[s].Values[c] : 0;
                var percent = positiveTotal > 0 && value > 0 ? value / positiveTotal : 0d;
                var label = Labels is { } labels && c < labels.Count ? labels[c] : null;
                points.Add(new ChartPoint(c, value, percent, label, color) { SeriesIndex = s });
            }
        }

        return points.ToArray();
    }

    /// <summary>True when per-point <see cref="ChartBase.Labels"/> are available for the X axis.</summary>
    protected bool HasCategoryLabels => Labels is { Count: > 0 };

    /// <summary>Resolves the value-axis domain (min, max, step). Applies Min/Max overrides and nice-number scaling when axes are shown.</summary>
    protected (double Min, double Max, double Step) ResolveDomain(double dataMin, double dataMax)
    {
        var lo = Min ?? dataMin;
        var hi = Max ?? dataMax;
        if (hi <= lo)
        {
            hi = lo + 1;
        }

        return ShowAxes ? Charts.NiceScale(lo, hi, YAxisTickCount) : (lo, hi, hi - lo);
    }

    /// <summary>Formats a Y-axis value using <see cref="YAxisFormat"/> or a compact default.</summary>
    protected string FormatAxisValue(double value) =>
        YAxisFormat?.Invoke(value) ?? value.ToString("0.##", CultureInfo.CurrentCulture);

    /// <summary>The left gutter width needed for Y-axis labels (0 when axes are hidden).</summary>
    protected double MeasureYGutter(double min, double max, double step)
    {
        if (!ShowAxes || step <= 0)
        {
            return 0;
        }

        var widest = 0d;
        for (var value = min; value <= max + step / 2; value += step)
        {
            widest = Math.Max(widest, DataLabelText(FormatAxisValue(value)).Width);
        }

        return widest + 6;
    }

    /// <summary>The bottom gutter height needed for X-axis category labels (0 when hidden or unlabeled).</summary>
    protected double MeasureXGutter() =>
        ShowAxes && HasCategoryLabels ? DataLabelText("0").Height + 4 : 0;

    /// <summary>Draws Y-axis gridlines and tick labels in the left gutter (no-op when axes are hidden).</summary>
    protected void DrawYAxis(DrawingContext context, Rect plot, double min, double max, double step)
    {
        if (!ShowAxes || step <= 0)
        {
            return;
        }

        var span = max - min;
        if (span <= 0)
        {
            return;
        }

        var pen = new Pen(Visuals.Grid, 1);
        for (var value = min; value <= max + step / 2; value += step)
        {
            var y = plot.Bottom - (value - min) / span * plot.Height;
            context.DrawLine(pen, new Point(plot.Left, y), new Point(plot.Right, y));
            var text = DataLabelText(FormatAxisValue(value));
            context.DrawText(text, new Point(Math.Max(0, plot.Left - text.Width - 4), y - text.Height / 2));
        }
    }

    /// <summary>Draws X-axis category labels (from <see cref="ChartBase.Labels"/>) beneath the given category centers, with thinning.</summary>
    protected void DrawXAxisLabels(DrawingContext context, Rect plot, double bottomGutter, IReadOnlyList<double> centers)
    {
        if (!ShowAxes || bottomGutter <= 0)
        {
            return;
        }

        var lastRight = double.NegativeInfinity;
        for (var i = 0; i < centers.Count && i < ResolvedPoints.Count; i++)
        {
            var label = ResolvedPoints[i].Label;
            if (string.IsNullOrEmpty(label))
            {
                continue;
            }

            var text = DataLabelText(label);
            var lx = Math.Clamp(centers[i] - text.Width / 2, 0, Math.Max(0, Bounds.Width - text.Width));
            if (lx < lastRight + 4)
            {
                continue;
            }

            context.DrawText(text, new Point(lx, plot.Bottom + 3));
            lastRight = lx + text.Width;
        }
    }
}

/// <summary>A vertical bar chart. Bars scale to the value-axis domain; set <see cref="CartesianChartBase.ShowAxes"/> for labeled axes.</summary>
public sealed class BarChart : CartesianChartBase
{
    private bool _allowNegative;
    private BarStackMode _stackMode = BarStackMode.Grouped;
    private (Rect Rect, int Index)[] _barRects = Array.Empty<(Rect, int)>();

    /// <summary>When true, negative values render as bars below a zero baseline instead of being clamped to zero.</summary>
    public bool AllowNegative
    {
        get => _allowNegative;
        set { _allowNegative = value; InvalidateVisual(); }
    }

    /// <summary>For multi-series charts: arrange series side-by-side (Grouped), stacked, or stacked to 100%.</summary>
    public BarStackMode StackMode
    {
        get => _stackMode;
        set { _stackMode = value; InvalidateVisual(); }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(320, 180);

    /// <inheritdoc />
    protected override string ChartAutomationName => "Bar chart";

    /// <inheritdoc />
    protected override int HitTest(Point local)
    {
        foreach (var (rect, index) in _barRects)
        {
            if (rect.Contains(local))
            {
                return index;
            }
        }

        return -1;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        if (HasSeries)
        {
            RenderSeries(context);
            return;
        }

        var signed = AllowNegative && Values.Any(value => value < 0);
        if (!(signed ? HasSignedData : HasRenderableData))
        {
            DrawNoData(context);
            return;
        }

        double dataMin;
        double dataMax;
        if (signed)
        {
            (dataMin, dataMax) = Charts.SignedDomain(Values);
        }
        else
        {
            dataMin = 0;
            dataMax = Values.DefaultIfEmpty(0).Max();
        }

        var (min, max, step) = ResolveDomain(dataMin, dataMax);

        const double gap = 8;
        const double pad = 8;
        var leftGutter = MeasureYGutter(min, max, step);
        var bottomGutter = MeasureXGutter();
        var plot = new Rect(
            pad + leftGutter,
            pad,
            Math.Max(0, Bounds.Width - pad * 2 - leftGutter),
            Math.Max(0, Bounds.Height - pad * 2 - bottomGutter));

        if (ShowAxes)
        {
            DrawYAxis(context, plot, min, max, step);
        }
        else
        {
            DrawGrid(context, plot);
        }

        var slot = Values.Count > 0 ? plot.Width / Values.Count : plot.Width;
        var barWidth = Math.Max(1, slot - gap);
        var layout = Charts.SignedBarLayout(Values, min, max, plot.Height);

        if (min < 0)
        {
            var zeroY = plot.Top + Charts.ZeroBaselineOffset(min, max, plot.Height);
            context.DrawLine(new Pen(Visuals.Outline, 1), new Point(plot.Left, zeroY), new Point(plot.Right, zeroY));
        }

        var centers = new double[Values.Count];
        var rects = new List<(Rect, int)>();
        for (var i = 0; i < Values.Count; i++)
        {
            centers[i] = plot.Left + i * slot + slot / 2;
            var (y, height) = layout[i];
            if (height <= 0)
            {
                continue;
            }

            var x = plot.Left + i * slot + (slot - barWidth) / 2;
            var rect = new Rect(x, plot.Top + y, barWidth, height);
            var pen = i == HoveredIndex ? new Pen(Visuals.Surface, 2) : null;
            context.DrawRectangle(SeriesBrush(i), pen, rect, 4, 4);
            rects.Add((rect, i));
        }

        _barRects = rects.ToArray();

        if (ShowDataLabels)
        {
            var lastRight = double.NegativeInfinity;
            for (var i = 0; i < Values.Count && i < ResolvedPoints.Count; i++)
            {
                var text = DataLabelText(ResolveDataLabel(ResolvedPoints[i]));
                var lx = Math.Clamp(centers[i] - text.Width / 2, 0, Math.Max(0, Bounds.Width - text.Width));
                if (lx < lastRight + 4)
                {
                    continue;
                }

                var (y, height) = layout[i];
                var ly = Values[i] >= 0 ? plot.Top + y - text.Height - 2 : plot.Top + y + height + 2;
                ly = Math.Clamp(ly, plot.Top, plot.Bottom - text.Height);
                context.DrawText(text, new Point(lx, ly));
                lastRight = lx + text.Width;
            }
        }

        DrawXAxisLabels(context, plot, bottomGutter, centers);
        DrawTooltip(context);
    }

    private void RenderSeries(DrawingContext context)
    {
        var series = SeriesList;
        var categories = SeriesCategoryCount;
        var stacked = StackMode != BarStackMode.Grouped;
        var percent = StackMode == BarStackMode.StackedPercent;

        double dataMax = 0;
        if (stacked)
        {
            for (var c = 0; c < categories; c++)
            {
                var sum = 0d;
                foreach (var s in series)
                {
                    var v = c < s.Values.Count ? s.Values[c] : 0;
                    if (v > 0)
                    {
                        sum += v;
                    }
                }

                dataMax = Math.Max(dataMax, sum);
            }

            if (percent)
            {
                dataMax = 100;
            }
        }
        else
        {
            foreach (var s in series)
            {
                foreach (var v in s.Values)
                {
                    dataMax = Math.Max(dataMax, v);
                }
            }
        }

        if (categories == 0 || dataMax <= 0)
        {
            DrawNoData(context);
            return;
        }

        var (min, max, step) = ResolveDomain(0, dataMax);

        const double gap = 8;
        const double pad = 8;
        var leftGutter = MeasureYGutter(min, max, step);
        var bottomGutter = MeasureXGutter();
        var plot = new Rect(
            pad + leftGutter,
            pad,
            Math.Max(0, Bounds.Width - pad * 2 - leftGutter),
            Math.Max(0, Bounds.Height - pad * 2 - bottomGutter));

        if (ShowAxes)
        {
            DrawYAxis(context, plot, min, max, step);
        }
        else
        {
            DrawGrid(context, plot);
        }

        var slot = plot.Width / categories;
        var centers = new double[categories];
        var rects = new List<(Rect, int)>();

        for (var c = 0; c < categories; c++)
        {
            centers[c] = plot.Left + c * slot + slot / 2;

            if (stacked)
            {
                var values = new double[series.Count];
                for (var s = 0; s < series.Count; s++)
                {
                    values[s] = c < series[s].Values.Count ? series[s].Values[c] : 0;
                }

                IReadOnlyList<double> stackValues = values;
                if (percent)
                {
                    var sum = values.Where(v => v > 0).Sum();
                    stackValues = sum > 0 ? values.Select(v => Math.Max(0, v) / sum * 100).ToArray() : values;
                }

                var segments = Charts.StackedBarHeights(stackValues, max, plot.Height);
                var barWidth = Math.Max(1, slot - gap);
                var x = plot.Left + c * slot + (slot - barWidth) / 2;
                for (var s = 0; s < series.Count; s++)
                {
                    var (y, height) = segments[s];
                    if (height <= 0)
                    {
                        continue;
                    }

                    var flat = s * categories + c;
                    var rect = new Rect(x, plot.Top + y, barWidth, height);
                    var pen = flat == HoveredIndex ? new Pen(Visuals.Surface, 2) : null;
                    context.DrawRectangle(new ImmutableSolidColorBrush(ResolvedPoints[flat].Color), pen, rect, 4, 4);
                    rects.Add((rect, flat));
                }
            }
            else
            {
                var sub = Math.Max(1, (slot - gap) / series.Count);
                for (var s = 0; s < series.Count; s++)
                {
                    var v = c < series[s].Values.Count ? series[s].Values[c] : 0;
                    if (v <= 0)
                    {
                        continue;
                    }

                    var height = v / max * plot.Height;
                    var x = plot.Left + c * slot + gap / 2 + s * sub;
                    var flat = s * categories + c;
                    var rect = new Rect(x, plot.Bottom - height, Math.Max(1, sub - 1), height);
                    var pen = flat == HoveredIndex ? new Pen(Visuals.Surface, 2) : null;
                    context.DrawRectangle(new ImmutableSolidColorBrush(ResolvedPoints[flat].Color), pen, rect, 3, 3);
                    rects.Add((rect, flat));
                }
            }
        }

        _barRects = rects.ToArray();
        DrawXAxisLabels(context, plot, bottomGutter, centers);
        DrawTooltip(context);
    }
}

/// <summary>A line chart. Plots values left-to-right with dots; set <see cref="Area"/> to fill, or <see cref="CartesianChartBase.ShowAxes"/> for labeled axes.</summary>
public sealed class LineChart : CartesianChartBase
{
    private bool _area;
    private bool _allowNegative;
    private Point[] _pointPositions = Array.Empty<Point>();

    /// <summary>Whether to fill the area beneath the line.</summary>
    public bool Area
    {
        get => _area;
        set { _area = value; InvalidateVisual(); }
    }

    /// <summary>When true, negative values are plotted below a zero baseline instead of being clamped to zero.</summary>
    public bool AllowNegative
    {
        get => _allowNegative;
        set { _allowNegative = value; InvalidateVisual(); }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize) => new(320, 180);

    /// <inheritdoc />
    protected override string ChartAutomationName => "Line chart";

    /// <inheritdoc />
    protected override int HitTest(Point local)
    {
        var best = -1;
        var bestDistance = 16d * 16d;
        for (var i = 0; i < _pointPositions.Length; i++)
        {
            var dx = local.X - _pointPositions[i].X;
            var dy = local.Y - _pointPositions[i].Y;
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
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        if (HasSeries)
        {
            RenderSeries(context);
            return;
        }

        var signed = AllowNegative && Values.Any(value => value < 0);
        if (!(signed ? HasSignedData : HasRenderableData))
        {
            DrawNoData(context);
            return;
        }

        double dataMin;
        double dataMax;
        if (signed)
        {
            (dataMin, dataMax) = Charts.SignedDomain(Values);
        }
        else
        {
            dataMin = 0;
            dataMax = Values.DefaultIfEmpty(0).Max();
        }

        var (min, max, step) = ResolveDomain(dataMin, dataMax);

        const double pad = 4;
        var leftGutter = MeasureYGutter(min, max, step);
        var bottomGutter = MeasureXGutter();
        var plot = new Rect(
            leftGutter,
            pad,
            Math.Max(0, Bounds.Width - leftGutter),
            Math.Max(0, Bounds.Height - pad * 2 - bottomGutter));

        if (ShowAxes)
        {
            DrawYAxis(context, plot, min, max, step);
        }
        else
        {
            DrawGrid(context, plot);
        }

        var points = Charts.ScaledLinePoints(Values, plot, min, max);
        if (points.Count == 0)
        {
            DrawNoData(context);
            return;
        }

        _pointPositions = points.ToArray();
        var color = SeriesBrush(0);
        var zeroY = plot.Top + Charts.ZeroBaselineOffset(min, max, plot.Height);

        if (Area && points.Count > 1)
        {
            var area = new StreamGeometry();
            using (var ctx = area.Open())
            {
                ctx.BeginFigure(new Point(points[0].X, zeroY), isFilled: true);
                foreach (var p in points)
                {
                    ctx.LineTo(p);
                }

                ctx.LineTo(new Point(points[^1].X, zeroY));
                ctx.EndFigure(true);
            }

            context.DrawGeometry(AreaBrush(0), null, area);
        }

        if (min < 0)
        {
            context.DrawLine(new Pen(Visuals.Outline, 1), new Point(plot.Left, zeroY), new Point(plot.Right, zeroY));
        }

        var pen = new Pen(color, 2) { LineJoin = PenLineJoin.Round };
        for (var i = 1; i < points.Count; i++)
        {
            context.DrawLine(pen, points[i - 1], points[i]);
        }

        for (var i = 0; i < points.Count; i++)
        {
            if (i == HoveredIndex)
            {
                context.DrawEllipse(color, new Pen(Visuals.Surface, 2), points[i], 5, 5);
            }
            else
            {
                context.DrawEllipse(color, null, points[i], 3, 3);
            }
        }

        if (ShowDataLabels)
        {
            var lastRight = double.NegativeInfinity;
            for (var i = 0; i < points.Count && i < ResolvedPoints.Count; i++)
            {
                var text = DataLabelText(ResolveDataLabel(ResolvedPoints[i]));
                var lx = Math.Clamp(points[i].X - text.Width / 2, 0, Math.Max(0, Bounds.Width - text.Width));
                if (lx < lastRight + 4)
                {
                    continue;
                }

                var ly = points[i].Y - text.Height - 6;
                if (ly < 0)
                {
                    ly = points[i].Y + 6;
                }

                context.DrawText(text, new Point(lx, ly));
                lastRight = lx + text.Width;
            }
        }

        DrawXAxisLabels(context, plot, bottomGutter, _pointPositions.Select(p => p.X).ToArray());
        DrawTooltip(context);
    }

    private void RenderSeries(DrawingContext context)
    {
        var series = SeriesList;
        var categories = SeriesCategoryCount;
        if (categories == 0 || !series.Any(s => s.Values.Any(v => v != 0)))
        {
            DrawNoData(context);
            return;
        }

        var allValues = series.SelectMany(s => s.Values).ToList();
        var dataMax = Math.Max(0, allValues.DefaultIfEmpty(0).Max());
        var dataMin = AllowNegative ? Math.Min(0, allValues.DefaultIfEmpty(0).Min()) : 0;
        var (min, max, step) = ResolveDomain(dataMin, dataMax);
        var span = max - min;

        const double pad = 4;
        var leftGutter = MeasureYGutter(min, max, step);
        var bottomGutter = MeasureXGutter();
        var plot = new Rect(
            leftGutter,
            pad,
            Math.Max(0, Bounds.Width - leftGutter),
            Math.Max(0, Bounds.Height - pad * 2 - bottomGutter));

        if (ShowAxes)
        {
            DrawYAxis(context, plot, min, max, step);
        }
        else
        {
            DrawGrid(context, plot);
        }

        if (min < 0)
        {
            var zeroY = plot.Top + Charts.ZeroBaselineOffset(min, max, plot.Height);
            context.DrawLine(new Pen(Visuals.Outline, 1), new Point(plot.Left, zeroY), new Point(plot.Right, zeroY));
        }

        var positions = new List<Point>(series.Count * categories);
        var categoryX = new double[categories];
        for (var s = 0; s < series.Count; s++)
        {
            var values = series[s].Values;
            var pts = new Point[categories];
            for (var c = 0; c < categories; c++)
            {
                var raw = c < values.Count ? values[c] : 0;
                var v = AllowNegative ? raw : Math.Max(0, raw);
                var x = categories == 1
                    ? plot.Left + plot.Width / 2
                    : plot.Left + c / (double)(categories - 1) * plot.Width;
                var y = plot.Top + (max - v) / span * plot.Height;
                pts[c] = new Point(x, y);
                if (s == 0)
                {
                    categoryX[c] = x;
                }
            }

            var brush = new ImmutableSolidColorBrush(SeriesColor(s));
            var pen = new Pen(brush, 2) { LineJoin = PenLineJoin.Round };
            for (var c = 1; c < categories; c++)
            {
                context.DrawLine(pen, pts[c - 1], pts[c]);
            }

            for (var c = 0; c < categories; c++)
            {
                var flat = s * categories + c;
                if (flat == HoveredIndex)
                {
                    context.DrawEllipse(brush, new Pen(Visuals.Surface, 2), pts[c], 5, 5);
                }
                else
                {
                    context.DrawEllipse(brush, null, pts[c], 3, 3);
                }
            }

            positions.AddRange(pts);
        }

        _pointPositions = positions.ToArray();
        DrawXAxisLabels(context, plot, bottomGutter, categoryX);
        DrawTooltip(context);
    }
}
