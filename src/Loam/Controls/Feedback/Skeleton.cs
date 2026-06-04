using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A placeholder shimmer block shown while content loads, mirroring the reference API's <c>Skeleton</c>.
/// A skeleton-colored box; set <see cref="Circle"/> for a round avatar placeholder.
/// </summary>
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable",
    Justification = "The animation token is owned by the visual-tree lifecycle and disposed in OnDetachedFromVisualTree.")]
public class Skeleton : Border
{
    /// <summary>Identifies the <see cref="Circle"/> property.</summary>
    public static readonly StyledProperty<bool> CircleProperty =
        AvaloniaProperty.Register<Skeleton, bool>(nameof(Circle));

    /// <summary>Identifies the <see cref="Animate"/> property.</summary>
    public static readonly StyledProperty<bool> AnimateProperty =
        AvaloniaProperty.Register<Skeleton, bool>(nameof(Animate), true);

    private CancellationTokenSource? _shimmerAnimation;

    /// <summary>Creates the skeleton.</summary>
    public Skeleton()
    {
        Height = 16;
        this.Bind(BackgroundProperty, this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.Skeleton))));
        Apply();
    }

    /// <summary>Renders as a circle (for avatar placeholders). Mirrors the reference API's circle skeleton type.</summary>
    public bool Circle
    {
        get => GetValue(CircleProperty);
        set => SetValue(CircleProperty, value);
    }

    /// <summary>Whether the loading placeholder subtly animates.</summary>
    public bool Animate
    {
        get => GetValue(AnimateProperty);
        set => SetValue(AnimateProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Skeleton);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        StartShimmer();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopShimmer();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CircleProperty || change.Property == HeightProperty)
        {
            Apply();
        }
        else if (change.Property == AnimateProperty)
        {
            if (Animate)
            {
                StartShimmer();
            }
            else
            {
                StopShimmer();
            }
        }
    }

    private void Apply()
    {
        var height = double.IsNaN(Height) ? 16 : Height;
        CornerRadius = Circle ? new CornerRadius(height / 2) : new CornerRadius(4);
    }

    private void StartShimmer()
    {
        if (!Animate || _shimmerAnimation is not null)
        {
            return;
        }

        _shimmerAnimation = new CancellationTokenSource();
        var animation = new Animation
        {
            Duration = TimeSpan.FromSeconds(1.1),
            IterationCount = IterationCount.Infinite,
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(OpacityProperty, 0.72d) } },
                new KeyFrame { Cue = new Cue(0.5d), Setters = { new Setter(OpacityProperty, 1d) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(OpacityProperty, 0.72d) } },
            },
        };
        _ = animation.RunAsync(this, _shimmerAnimation.Token);
    }

    private void StopShimmer()
    {
        _shimmerAnimation?.Cancel();
        _shimmerAnimation?.Dispose();
        _shimmerAnimation = null;
        Opacity = 1;
    }
}
