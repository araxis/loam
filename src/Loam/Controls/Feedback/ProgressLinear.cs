using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
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

    /// <summary>Identifies the <see cref="Indeterminate"/> property.</summary>
    public static readonly StyledProperty<bool> IndeterminateProperty =
        AvaloniaProperty.Register<ProgressLinear, bool>(nameof(Indeterminate));

    /// <summary>Drives the moving fill while <see cref="Indeterminate"/>.</summary>
    public static readonly StyledProperty<double> IndeterminateOffsetProperty =
        AvaloniaProperty.Register<ProgressLinear, double>(nameof(IndeterminateOffset));

    private readonly TranslateTransform _indeterminateTransform = new();
    private Panel? _area;
    private Border? _fill;
    private IDisposable? _fillBackground;
    private CancellationTokenSource? _indeterminateAnimation;

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

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ProgressLinear);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        StartIndeterminate();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopIndeterminate();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _area = e.NameScope.Find("PART_Area") as Panel;
        _fill = e.NameScope.Find("PART_Fill") as Border;
        if (_fill is not null)
        {
            _fill.RenderTransform = _indeterminateTransform;
        }

        ApplyColor();
        UpdateFill();
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
        }
        else if (change.Property == ColorProperty)
        {
            ApplyColor();
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
        }
    }

    private void ApplyColor()
    {
        if (_fill is null)
        {
            return;
        }

        _fillBackground?.Dispose();
        _fillBackground = _fill.Bind(Border.BackgroundProperty, this.GetResourceObservable(SemanticColor.Resolve(Color).Fill));
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
            _indeterminateTransform.X = IndeterminateOffset * (width + fillWidth) - fillWidth;
            return;
        }

        var fraction = Maximum <= Minimum ? 0 : Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0, 1);
        _fill.Width = fraction * width;
        _indeterminateTransform.X = 0;
    }

    private void StartIndeterminate()
    {
        if (!Indeterminate || _indeterminateAnimation is not null)
        {
            return;
        }

        _indeterminateAnimation = new CancellationTokenSource();
        SetCurrentValue(IndeterminateOffsetProperty, 0d);
        var animation = new Animation
        {
            Duration = InteractionAssist.DurationToken(this,
                LoamTokens.MotionDuration(nameof(LoamMotion.ExtraLong4)),
                TimeSpan.FromSeconds(1.2)),
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
}
