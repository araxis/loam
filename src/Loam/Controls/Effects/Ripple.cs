using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace Loam.Controls;

/// <summary>
/// A Material click-ripple effect, mirroring MudBlazor's ripple. Wraps a <see cref="Decorator.Child"/>
/// and, on pointer-press, animates a translucent circle expanding from the press point and fading out.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable",
    Justification = "The ripple CancellationTokenSource is owned by the visual-tree lifecycle and disposed in OnDetachedFromVisualTree.")]
public class Ripple : Decorator
{
    /// <summary>Drives the active ripple (0→1); read by <see cref="Render"/>.</summary>
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<Ripple, double>(nameof(Progress));

    private Point _origin;
    private double _maxReach;
    private CancellationTokenSource? _animation;

    static Ripple() => AffectsRender<Ripple>(ProgressProperty);

    /// <summary>Creates the ripple host.</summary>
    public Ripple() => ClipToBounds = true;

    /// <summary>The current ripple animation progress (0–1).</summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>The distance from <paramref name="origin"/> to the farthest corner of <paramref name="size"/> (the ripple's full reach).</summary>
    public static double MaxReach(Point origin, Size size)
    {
        var dx = Math.Max(origin.X, size.Width - origin.X);
        var dy = Math.Max(origin.Y, size.Height - origin.Y);
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _origin = e.GetPosition(this);
        _maxReach = MaxReach(_origin, Bounds.Size);
        StartRipple();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _animation?.Cancel();
        _animation?.Dispose();
        _animation = null;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var progress = Progress;
        if (progress is <= 0 or >= 1 || _maxReach <= 0)
        {
            return;
        }

        var radius = _maxReach * progress;
        var alpha = (byte)(60 * (1 - progress));
        var brush = new ImmutableSolidColorBrush(Color.FromArgb(alpha, 0, 0, 0));
        context.DrawEllipse(brush, null, _origin, radius, radius);
    }

    private void StartRipple()
    {
        _animation?.Cancel();
        _animation?.Dispose();
        _animation = new CancellationTokenSource();

        SetCurrentValue(ProgressProperty, 0d);
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(450),
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(ProgressProperty, 0d) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(ProgressProperty, 1d) } },
            },
        };
        _ = animation.RunAsync(this, _animation.Token);
    }
}
