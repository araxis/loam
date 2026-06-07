using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Styling;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A placeholder shimmer block shown while content loads, mirroring the reference API's <c>Skeleton</c>.
/// A skeleton-colored box; use <see cref="Preset"/> or the static factories for common loading anatomy.
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

    /// <summary>Identifies the <see cref="Preset"/> property.</summary>
    public static readonly StyledProperty<SkeletonPreset> PresetProperty =
        AvaloniaProperty.Register<Skeleton, SkeletonPreset>(nameof(Preset), SkeletonPreset.Custom);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<Skeleton, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<Skeleton, string?>(nameof(Label));

    private bool _applyingPreset;
    private CancellationTokenSource? _shimmerAnimation;

    /// <summary>Creates the skeleton.</summary>
    public Skeleton()
    {
        Height = 16;
        this.Bind(BackgroundProperty, this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.Skeleton))));
        Apply();
    }

    /// <summary>Creates a text-line placeholder with useful default metrics.</summary>
    public static Skeleton TextLine(double width = 160, LoamSize size = LoamSize.Medium, bool animate = true, string? label = null)
    {
        var skeleton = new Skeleton { Preset = SkeletonPreset.Text, Size = size, Animate = animate, Label = label };
        skeleton.Width = width;
        return skeleton;
    }

    /// <summary>Creates a circular avatar placeholder.</summary>
    public static Skeleton Avatar(LoamSize size = LoamSize.Medium, bool animate = true, string? label = null) =>
        new() { Preset = SkeletonPreset.Avatar, Size = size, Animate = animate, Label = label };

    /// <summary>Creates a pill-shaped action placeholder.</summary>
    public static Skeleton Button(double width = 120, LoamSize size = LoamSize.Medium, bool animate = true, string? label = null)
    {
        var skeleton = new Skeleton { Preset = SkeletonPreset.Button, Size = size, Animate = animate, Label = label };
        skeleton.Width = width;
        return skeleton;
    }

    /// <summary>Creates a media thumbnail placeholder.</summary>
    public static Skeleton Thumbnail(double width = 96, double height = 72, bool animate = true, string? label = null) =>
        new() { Preset = SkeletonPreset.Thumbnail, Width = width, Height = height, Animate = animate, Label = label };

    /// <summary>Creates a content-card placeholder.</summary>
    public static Skeleton Card(double width = 280, double height = 140, bool animate = true, string? label = null) =>
        new() { Preset = SkeletonPreset.Card, Width = width, Height = height, Animate = animate, Label = label };

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

    /// <summary>Generated placeholder anatomy preset.</summary>
    public SkeletonPreset Preset
    {
        get => GetValue(PresetProperty);
        set => SetValue(PresetProperty, value);
    }

    /// <summary>Preset size used by generated loading anatomy.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Accessible loading label.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Skeleton);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        StartShimmer();
        ApplyVisualState();
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
        if (change.Property == CircleProperty || change.Property == HeightProperty ||
            change.Property == WidthProperty || change.Property == PresetProperty ||
            change.Property == SizeProperty || change.Property == LabelProperty)
        {
            Apply();
        }
        else if (change.Property == AnimateProperty || change.Property == IsEnabledProperty)
        {
            StopShimmer();
            StartShimmer();
            ApplyVisualState();
        }
    }

    private void Apply()
    {
        if (_applyingPreset)
        {
            return;
        }

        _applyingPreset = true;
        try
        {
            ApplyPresetMetrics();
        }
        finally
        {
            _applyingPreset = false;
        }

        var height = double.IsNaN(Height) ? 16 : Height;
        CornerRadius = Preset switch
        {
            SkeletonPreset.Text or SkeletonPreset.Button => new CornerRadius(height / 2),
            SkeletonPreset.Avatar => new CornerRadius(height / 2),
            SkeletonPreset.Thumbnail or SkeletonPreset.Card => new CornerRadius(12),
            _ => Circle ? new CornerRadius(height / 2) : new CornerRadius(4),
        };
        InteractionAssist.SetAutomationName(this, Label, Preset == SkeletonPreset.Custom ? "Loading" : $"Loading {Preset}");
        AutomationProperties.SetHelpText(this, Animate && IsEnabled ? "Animated loading placeholder" : "Static loading placeholder");
        ApplyVisualState();
    }

    private void ApplyPresetMetrics()
    {
        switch (Preset)
        {
            case SkeletonPreset.Text:
                if (double.IsNaN(Width))
                {
                    Width = 160;
                }

                Height = TextLineHeight(Size);
                Circle = false;
                break;
            case SkeletonPreset.Avatar:
                var avatar = AvatarSize(Size);
                Width = avatar;
                Height = avatar;
                Circle = true;
                break;
            case SkeletonPreset.Button:
                if (double.IsNaN(Width))
                {
                    Width = 120;
                }

                Height = ButtonHeight(Size);
                Circle = false;
                break;
            case SkeletonPreset.Thumbnail:
                if (double.IsNaN(Width))
                {
                    Width = 96;
                }

                if (double.IsNaN(Height))
                {
                    Height = 72;
                }

                Circle = false;
                break;
            case SkeletonPreset.Card:
                if (double.IsNaN(Width))
                {
                    Width = 280;
                }

                if (double.IsNaN(Height))
                {
                    Height = 140;
                }

                Circle = false;
                break;
        }
    }

    /// <summary>The generated text-line height for a <see cref="LoamSize"/>.</summary>
    public static double TextLineHeight(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => 10,
        LoamSize.Small => 12,
        LoamSize.Large => 20,
        LoamSize.ExtraLarge => 24,
        _ => 16,
    };

    /// <summary>The generated avatar diameter for a <see cref="LoamSize"/>.</summary>
    public static double AvatarSize(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => 24,
        LoamSize.Small => 32,
        LoamSize.Large => 56,
        LoamSize.ExtraLarge => 72,
        _ => 40,
    };

    /// <summary>The generated action-placeholder height for a <see cref="LoamSize"/>.</summary>
    public static double ButtonHeight(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => 32,
        LoamSize.Small => 40,
        LoamSize.Large => 56,
        LoamSize.ExtraLarge => 64,
        _ => 48,
    };

    private void StartShimmer()
    {
        if (!IsEnabled || !Animate || _shimmerAnimation is not null)
        {
            return;
        }

        var duration = InteractionAssist.DurationToken(this,
            LoamTokens.MotionDuration(nameof(LoamMotion.ExtraLong3)),
            TimeSpan.FromSeconds(1.1));
        if (duration <= TimeSpan.Zero)
        {
            Opacity = 1;
            return;
        }

        _shimmerAnimation = new CancellationTokenSource();
        var animation = new Animation
        {
            Duration = duration,
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

    private void ApplyVisualState()
    {
        AutomationProperties.SetHelpText(this, Animate && IsEnabled ? "Animated loading placeholder" : "Static loading placeholder");

        if (!IsEnabled)
        {
            Opacity = InteractionAssist.DisabledOpacity(this);
            return;
        }

        if (!Animate || _shimmerAnimation is null)
        {
            Opacity = 1;
        }
    }
}
