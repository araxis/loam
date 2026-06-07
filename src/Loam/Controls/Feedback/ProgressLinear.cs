using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A horizontal progress bar, mirroring the reference API's <c>ProgressLinear</c>. Determinate fill from
/// <see cref="Value"/> within <see cref="Minimum"/>/<see cref="Maximum"/>, tinted by <see cref="Color"/>.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable",
    Justification = "The animation token is owned by the visual-tree lifecycle and disposed in OnDetachedFromVisualTree.")]
public class ProgressLinear : TemplatedControl
{
    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<ProgressLinear, double>(nameof(Value));

    /// <summary>Identifies the <see cref="Minimum"/> property.</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<ProgressLinear, double>(nameof(Minimum), 0);

    /// <summary>Identifies the <see cref="Maximum"/> property.</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<ProgressLinear, double>(nameof(Maximum), 100);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<ProgressLinear, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<ProgressLinear, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="Indeterminate"/> property.</summary>
    public static readonly StyledProperty<bool> IndeterminateProperty =
        AvaloniaProperty.Register<ProgressLinear, bool>(nameof(Indeterminate));

    /// <summary>Drives the moving fill while <see cref="Indeterminate"/>.</summary>
    public static readonly StyledProperty<double> IndeterminateOffsetProperty =
        AvaloniaProperty.Register<ProgressLinear, double>(nameof(IndeterminateOffset));

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<ProgressLinear, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="ShowValue"/> property.</summary>
    public static readonly StyledProperty<bool> ShowValueProperty =
        AvaloniaProperty.Register<ProgressLinear, bool>(nameof(ShowValue));

    /// <summary>Identifies the <see cref="ValueText"/> property.</summary>
    public static readonly StyledProperty<string?> ValueTextProperty =
        AvaloniaProperty.Register<ProgressLinear, string?>(nameof(ValueText));

    /// <summary>Identifies the <see cref="ValueTextFormat"/> property.</summary>
    public static readonly StyledProperty<string> ValueTextFormatProperty =
        AvaloniaProperty.Register<ProgressLinear, string>(nameof(ValueTextFormat), "{0:0}%");

    private readonly TranslateTransform _indeterminateTransform = new();
    private Panel? _area;
    private Border? _track;
    private Border? _fill;
    private Control? _header;
    private Text? _labelPart;
    private Text? _valueTextPart;
    private IBrush? _fillBrush;
    private IBrush? _trackBrush;
    private IBrush? _disabledBrush;
    private IDisposable? _fillBackground;
    private IDisposable? _trackBackground;
    private IDisposable? _disabledBackground;
    private CancellationTokenSource? _indeterminateAnimation;

    static ProgressLinear()
    {
        AffectsMeasure<ProgressLinear>(SizeProperty);
    }

    /// <summary>Creates the progress bar.</summary>
    public ProgressLinear()
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

    /// <summary>Track thickness size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Whether the bar shows a moving fill instead of a fixed value.</summary>
    public bool Indeterminate
    {
        get => GetValue(IndeterminateProperty);
        set => SetValue(IndeterminateProperty, value);
    }

    /// <summary>The animated indeterminate offset (0–1).</summary>
    public double IndeterminateOffset
    {
        get => GetValue(IndeterminateOffsetProperty);
        set => SetValue(IndeterminateOffsetProperty, value);
    }

    /// <summary>Optional generated label shown above the progress track and used for automation.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Shows generated value text beside <see cref="Label"/> for determinate progress.</summary>
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

    /// <summary>The clamped 0–1 progress fraction for the given value range.</summary>
    public static double Fraction(double value, double min, double max) =>
        max <= min ? 0 : Math.Clamp((value - min) / (max - min), 0, 1);

    /// <summary>The track height in pixels for a <see cref="LoamSize"/>.</summary>
    public static double TrackHeight(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => 2,
        LoamSize.Small => 3,
        LoamSize.Large => 6,
        LoamSize.ExtraLarge => 8,
        _ => 4,
    };

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ProgressLinear);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplyColor();
        StartIndeterminate();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _fillBackground?.Dispose();
        _trackBackground?.Dispose();
        _disabledBackground?.Dispose();
        _fillBackground = null;
        _trackBackground = null;
        _disabledBackground = null;
        StopIndeterminate();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _area = e.NameScope.Find("PART_Area") as Panel;
        _track = e.NameScope.Find("PART_Track") as Border;
        _fill = e.NameScope.Find("PART_Fill") as Border;
        _header = e.NameScope.Find("PART_Header") as Control;
        _labelPart = e.NameScope.Find("PART_Label") as Text;
        _valueTextPart = e.NameScope.Find("PART_ValueText") as Text;
        if (_fill is not null)
        {
            _fill.RenderTransform = _indeterminateTransform;
        }

        ApplyColor();
        UpdateSize();
        UpdateFill();
        UpdateText();
        StartIndeterminate();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || change.Property == MinimumProperty ||
            change.Property == MaximumProperty || change.Property == BoundsProperty ||
            change.Property == IndeterminateOffsetProperty)
        {
            UpdateFill();
            UpdateText();
        }
        else if (change.Property == ColorProperty)
        {
            ApplyColor();
        }
        else if (change.Property == SizeProperty)
        {
            UpdateSize();
            UpdateFill();
        }
        else if (change.Property == IndeterminateProperty)
        {
            if (Indeterminate)
            {
                StartIndeterminate();
            }
            else
            {
                StopIndeterminate();
            }

            UpdateFill();
            UpdateText();
        }
        else if (change.Property == IsEnabledProperty)
        {
            StopIndeterminate();
            StartIndeterminate();
            ApplyBrushes();
            UpdateFill();
            UpdateText();
        }
        else if (change.Property == LabelProperty || change.Property == ShowValueProperty ||
                 change.Property == ValueTextProperty || change.Property == ValueTextFormatProperty)
        {
            UpdateText();
        }
    }

    private void ApplyColor()
    {
        if (_fill is null && _track is null)
        {
            return;
        }

        _fillBackground?.Dispose();
        _fillBackground = this.GetResourceObservable(SemanticColor.Resolve(Color).Fill)
            .Subscribe(new BrushObserver(brush => { _fillBrush = brush; ApplyBrushes(); }));

        _trackBackground?.Dispose();
        _trackBackground = this.GetResourceObservable(LoamTokens.Divider)
            .Subscribe(new BrushObserver(brush => { _trackBrush = brush; ApplyBrushes(); }));

        _disabledBackground?.Dispose();
        _disabledBackground = this.GetResourceObservable(LoamTokens.TextDisabled)
            .Subscribe(new BrushObserver(brush => { _disabledBrush = brush; ApplyBrushes(); }));
    }

    private void ApplyBrushes()
    {
        if (_fill is not null)
        {
            _fill.Background = IsEnabled ? _fillBrush : _disabledBrush ?? _fillBrush;
        }

        if (_track is not null)
        {
            _track.Background = IsEnabled ? _trackBrush : _disabledBrush ?? _trackBrush;
        }
    }

    private void UpdateSize()
    {
        if (_area is null)
        {
            return;
        }

        var height = TrackHeight(Size);
        var radius = new CornerRadius(height / 2);
        _area.Height = height;
        if (_track is not null)
        {
            _track.Height = height;
            _track.CornerRadius = radius;
        }

        if (_fill is not null)
        {
            _fill.Height = height;
            _fill.CornerRadius = radius;
        }
    }

    private void UpdateFill()
    {
        if (_area is null || _fill is null)
        {
            return;
        }

        var width = _area.Bounds.Width > 0 ? _area.Bounds.Width : Bounds.Width;
        if (Indeterminate)
        {
            var fillWidth = Math.Max(24, width * 0.35);
            _fill.Width = fillWidth;
            _indeterminateTransform.X = IsEnabled
                ? IndeterminateOffset * (width + fillWidth) - fillWidth
                : 0;
            return;
        }

        var fraction = Fraction(Value, Minimum, Maximum);
        _fill.Width = fraction * width;
        _indeterminateTransform.X = 0;
    }

    private void UpdateText()
    {
        var hasLabel = !string.IsNullOrWhiteSpace(Label);
        var hasValue = ShowValue;

        if (_header is not null)
        {
            _header.IsVisible = hasLabel || hasValue;
        }

        if (_labelPart is not null)
        {
            _labelPart.Text = Label;
            _labelPart.IsVisible = hasLabel;
        }

        if (_valueTextPart is not null)
        {
            _valueTextPart.Text = BuildValueText();
            _valueTextPart.IsVisible = hasValue;
        }

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

    private void StartIndeterminate()
    {
        if (!IsEnabled || !Indeterminate || _indeterminateAnimation is not null)
        {
            return;
        }

        SetCurrentValue(IndeterminateOffsetProperty, 0d);
        var duration = InteractionAssist.DurationToken(this,
            LoamTokens.MotionDuration(nameof(LoamMotion.ExtraLong4)),
            TimeSpan.FromSeconds(1.2));
        if (duration <= TimeSpan.Zero)
        {
            SetCurrentValue(IndeterminateOffsetProperty, 0.5d);
            return;
        }

        _indeterminateAnimation = new CancellationTokenSource();
        var animation = new Animation
        {
            Duration = duration,
            IterationCount = IterationCount.Infinite,
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(IndeterminateOffsetProperty, 0d) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(IndeterminateOffsetProperty, 1d) } },
            },
        };
        _ = animation.RunAsync(this, _indeterminateAnimation.Token);
    }

    private void StopIndeterminate()
    {
        _indeterminateAnimation?.Cancel();
        _indeterminateAnimation?.Dispose();
        _indeterminateAnimation = null;
        SetCurrentValue(IndeterminateOffsetProperty, 0d);
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
