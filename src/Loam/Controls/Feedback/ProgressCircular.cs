using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A circular progress indicator, mirroring the reference API's <c>ProgressCircular</c>. Draws an arc
/// tinted by <see cref="Color"/>: a determinate sweep from <see cref="Value"/> within
/// <see cref="Minimum"/>/<see cref="Maximum"/>, or a continuously spinning arc when
/// <see cref="Indeterminate"/> (the default).
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable",
    Justification = "The spin CancellationTokenSource is owned by the visual-tree lifecycle and disposed in OnDetachedFromVisualTree.")]
public class ProgressCircular : Control
{
    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<ProgressCircular, double>(nameof(Value));

    /// <summary>Identifies the <see cref="Minimum"/> property.</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<ProgressCircular, double>(nameof(Minimum), 0);

    /// <summary>Identifies the <see cref="Maximum"/> property.</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<ProgressCircular, double>(nameof(Maximum), 100);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<ProgressCircular, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<ProgressCircular, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="StrokeWidth"/> property.</summary>
    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<ProgressCircular, double>(nameof(StrokeWidth));

    /// <summary>Identifies the <see cref="Indeterminate"/> property.</summary>
    public static readonly StyledProperty<bool> IndeterminateProperty =
        AvaloniaProperty.Register<ProgressCircular, bool>(nameof(Indeterminate), true);

    /// <summary>Drives the indeterminate spin (animated 0→360); read by <see cref="Render"/>.</summary>
    public static readonly StyledProperty<double> SpinAngleProperty =
        AvaloniaProperty.Register<ProgressCircular, double>(nameof(SpinAngle));

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<ProgressCircular, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="ShowValue"/> property.</summary>
    public static readonly StyledProperty<bool> ShowValueProperty =
        AvaloniaProperty.Register<ProgressCircular, bool>(nameof(ShowValue));

    /// <summary>Identifies the <see cref="ValueText"/> property.</summary>
    public static readonly StyledProperty<string?> ValueTextProperty =
        AvaloniaProperty.Register<ProgressCircular, string?>(nameof(ValueText));

    /// <summary>Identifies the <see cref="ValueTextFormat"/> property.</summary>
    public static readonly StyledProperty<string> ValueTextFormatProperty =
        AvaloniaProperty.Register<ProgressCircular, string>(nameof(ValueTextFormat), "{0:0}%");

    private IBrush? _accent;
    private IBrush? _track;
    private IBrush? _text;
    private IBrush? _disabled;
    private IDisposable? _accentSubscription;
    private IDisposable? _trackSubscription;
    private IDisposable? _textSubscription;
    private IDisposable? _disabledSubscription;
    private CancellationTokenSource? _spinCancellation;

    static ProgressCircular()
    {
        AffectsRender<ProgressCircular>(ValueProperty, MinimumProperty, MaximumProperty,
            StrokeWidthProperty, IndeterminateProperty, SpinAngleProperty, ShowValueProperty,
            ValueTextProperty, ValueTextFormatProperty, IsEnabledProperty);
        AffectsMeasure<ProgressCircular>(SizeProperty, StrokeWidthProperty);
    }

    /// <summary>Creates the progress indicator.</summary>
    public ProgressCircular()
    {
        InteractionAssist.SetAutomationName(this, "Progress");
    }

    /// <summary>Current value. Mirrors the reference API's <c>Value</c>.</summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Minimum value. Mirrors the reference API's <c>Min</c>.</summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Maximum value. Mirrors the reference API's <c>Max</c>.</summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Accent color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Indicator size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Stroke thickness in pixels. Set to zero to use the size-resolved default.</summary>
    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    /// <summary>Whether the indicator spins indefinitely. Mirrors the reference API's <c>Indeterminate</c>.</summary>
    public bool Indeterminate
    {
        get => GetValue(IndeterminateProperty);
        set => SetValue(IndeterminateProperty, value);
    }

    /// <summary>The animated spin angle (degrees) used while <see cref="Indeterminate"/>.</summary>
    public double SpinAngle
    {
        get => GetValue(SpinAngleProperty);
        set => SetValue(SpinAngleProperty, value);
    }

    /// <summary>Optional accessible label used for the progress indicator.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Shows generated value text in the center for determinate progress.</summary>
    public bool ShowValue
    {
        get => GetValue(ShowValueProperty);
        set => SetValue(ShowValueProperty, value);
    }

    /// <summary>Explicit value text. When unset, <see cref="ValueTextFormat"/> formats the percentage.</summary>
    public string? ValueText
    {
        get => GetValue(ValueTextProperty);
        set => SetValue(ValueTextProperty, value);
    }

    /// <summary>Format string for generated percent text. Receives the percentage as argument 0.</summary>
    public string ValueTextFormat
    {
        get => GetValue(ValueTextFormatProperty);
        set => SetValue(ValueTextFormatProperty, value);
    }

    /// <summary>The pixel diameter for a <see cref="LoamSize"/>.</summary>
    public static double Diameter(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => 24,
        LoamSize.Small => 32,
        LoamSize.Large => 64,
        LoamSize.ExtraLarge => 80,
        _ => 48,
    };

    /// <summary>The default stroke thickness for a <see cref="LoamSize"/>.</summary>
    public static double DefaultStrokeWidth(LoamSize size) => Math.Round(Diameter(size) * 0.083333, 2);

    /// <summary>The stroke thickness used for rendering.</summary>
    public static double EffectiveStrokeWidth(LoamSize size, double strokeWidth) =>
        strokeWidth > 0 ? strokeWidth : DefaultStrokeWidth(size);

    /// <summary>The clamped 0–1 progress fraction for the given value range.</summary>
    public static double Fraction(double value, double min, double max) =>
        max <= min ? 0 : Math.Clamp((value - min) / (max - min), 0, 1);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeBrushes();
        StartSpin();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _accentSubscription?.Dispose();
        _trackSubscription?.Dispose();
        _textSubscription?.Dispose();
        _disabledSubscription?.Dispose();
        StopSpin();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColorProperty)
        {
            SubscribeBrushes();
        }
        else if (change.Property == IndeterminateProperty)
        {
            StopSpin();
            StartSpin();
            UpdateAutomation();
        }
        else if (change.Property == IsEnabledProperty)
        {
            StopSpin();
            StartSpin();
            InvalidateVisual();
        }
        else if (change.Property == ValueProperty || change.Property == MinimumProperty ||
                 change.Property == MaximumProperty || change.Property == LabelProperty ||
                 change.Property == ValueTextProperty || change.Property == ValueTextFormatProperty)
        {
            UpdateAutomation();
        }
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var d = Diameter(Size);
        return new Size(d, d);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var d = Diameter(Size);
        var sw = EffectiveStrokeWidth(Size, StrokeWidth);
        var radius = (d - sw) / 2;
        if (radius <= 0)
        {
            return;
        }

        var center = new Point(d / 2, d / 2);
        var accent = IsEnabled ? _accent : _disabled ?? _accent;
        var track = IsEnabled ? _track : _disabled ?? _track;
        var text = IsEnabled ? _text : _disabled ?? _text;

        if (!Indeterminate && track is not null)
        {
            context.DrawEllipse(null, new Pen(track, sw), center, radius, radius);
        }

        if (accent is null)
        {
            return;
        }

        var pen = new Pen(accent, sw) { LineCap = PenLineCap.Round };
        if (Indeterminate)
        {
            context.DrawGeometry(null, pen, BuildArc(center, radius, SpinAngle, 90));
        }
        else
        {
            var sweep = Math.Min(Fraction(Value, Minimum, Maximum) * 360, 359.99);
            if (sweep > 0)
            {
                context.DrawGeometry(null, pen, BuildArc(center, radius, -90, sweep));
            }

            if (ShowValue)
            {
                DrawValueText(context, center, d, text);
            }
        }
    }

    private static StreamGeometry BuildArc(Point center, double radius, double startDeg, double sweepDeg)
    {
        var start = PointOnCircle(center, radius, startDeg);
        var end = PointOnCircle(center, radius, startDeg + sweepDeg);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(start, isFilled: false);
            ctx.ArcTo(end, new Size(radius, radius), 0, sweepDeg > 180, SweepDirection.Clockwise);
            ctx.EndFigure(false);
        }

        return geometry;
    }

    private static Point PointOnCircle(Point center, double radius, double degrees)
    {
        var radians = degrees * Math.PI / 180;
        return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
    }

    private void SubscribeBrushes()
    {
        _accentSubscription?.Dispose();
        _accentSubscription = this.GetResourceObservable(SemanticColor.Resolve(Color).Fill)
            .Subscribe(new BrushObserver(brush => { _accent = brush; InvalidateVisual(); }));

        _trackSubscription?.Dispose();
        _trackSubscription = this.GetResourceObservable(LoamTokens.Divider)
            .Subscribe(new BrushObserver(brush => { _track = brush; InvalidateVisual(); }));

        _textSubscription?.Dispose();
        _textSubscription = this.GetResourceObservable(LoamTokens.TextPrimary)
            .Subscribe(new BrushObserver(brush => { _text = brush; InvalidateVisual(); }));

        _disabledSubscription?.Dispose();
        _disabledSubscription = this.GetResourceObservable(LoamTokens.TextDisabled)
            .Subscribe(new BrushObserver(brush => { _disabled = brush; InvalidateVisual(); }));

        UpdateAutomation();
    }

    private void DrawValueText(DrawingContext context, Point center, double diameter, IBrush? textBrush)
    {
        if (textBrush is null)
        {
            return;
        }

        var text = new FormattedText(
            BuildValueText(),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Medium),
            Math.Max(10, Math.Min(18, diameter * 0.28)),
            textBrush);
        context.DrawText(text, new Point(center.X - text.Width / 2, center.Y - text.Height / 2));
    }

    private void UpdateAutomation()
    {
        InteractionAssist.SetAutomationName(this, Label, "Progress");
        AutomationProperties.SetHelpText(this, Indeterminate ? "Indeterminate" : BuildValueText());
    }

    private string BuildValueText()
    {
        if (!string.IsNullOrWhiteSpace(ValueText))
        {
            return ValueText!;
        }

        if (Indeterminate)
        {
            return "Indeterminate";
        }

        var percent = Fraction(Value, Minimum, Maximum) * 100;
        try
        {
            return string.Format(CultureInfo.CurrentCulture, ValueTextFormat, percent);
        }
        catch (FormatException)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:0}%", percent);
        }
    }

    private void StartSpin()
    {
        if (!IsEnabled || !Indeterminate || _spinCancellation is not null)
        {
            return;
        }

        _spinCancellation = new CancellationTokenSource();
        var duration = InteractionAssist.DurationToken(this,
            LoamTokens.MotionDuration(nameof(LoamMotion.ExtraLong4)),
            TimeSpan.FromSeconds(1.2));
        if (duration <= TimeSpan.Zero)
        {
            SpinAngle = -90;
            return;
        }

        var animation = new Animation
        {
            Duration = duration,
            IterationCount = IterationCount.Infinite,
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(SpinAngleProperty, -90d) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(SpinAngleProperty, 270d) } },
            },
        };
        _ = animation.RunAsync(this, _spinCancellation.Token);
    }

    private void StopSpin()
    {
        _spinCancellation?.Cancel();
        _spinCancellation?.Dispose();
        _spinCancellation = null;
    }

    private sealed class BrushObserver : IObserver<object?>
    {
        private readonly Action<IBrush?> _onNext;

        public BrushObserver(Action<IBrush?> onNext) => _onNext = onNext;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(object? value) => _onNext(value as IBrush);
    }
}
