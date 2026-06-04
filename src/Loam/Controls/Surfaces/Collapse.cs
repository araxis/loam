using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace Loam.Controls;

/// <summary>
/// A height-clipping container, mirroring the reference API's <c>Collapse</c>. Hosts a single
/// <see cref="Decorator.Child"/> and reveals it when <see cref="Expanded"/>, clipping it away
/// (height 0) when collapsed.
/// </summary>
public class Collapse : Decorator
{
    /// <summary>Identifies the <see cref="Expanded"/> property.</summary>
    public static readonly StyledProperty<bool> ExpandedProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(Expanded),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Animated"/> property.</summary>
    public static readonly StyledProperty<bool> AnimatedProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(Animated), true);

    /// <summary>Identifies the <see cref="Duration"/> property.</summary>
    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<Collapse, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(180));

    /// <summary>Creates the collapse.</summary>
    public Collapse()
    {
        ClipToBounds = true;
        UpdateTransitions();
        UpdateState();
    }

    /// <summary>Whether the content is shown. Mirrors the reference API's <c>Expanded</c>.</summary>
    public bool Expanded
    {
        get => GetValue(ExpandedProperty);
        set => SetValue(ExpandedProperty, value);
    }

    /// <summary>Whether height changes animate.</summary>
    public bool Animated
    {
        get => GetValue(AnimatedProperty);
        set => SetValue(AnimatedProperty, value);
    }

    /// <summary>The reveal/collapse animation duration.</summary>
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ExpandedProperty || change.Property == ChildProperty)
        {
            UpdateState();
        }
        else if (change.Property == AnimatedProperty || change.Property == DurationProperty)
        {
            UpdateTransitions();
            UpdateState();
        }
    }

    private void UpdateTransitions()
    {
        Transitions = Animated
            ? new Transitions
            {
                new DoubleTransition
                {
                    Property = MaxHeightProperty,
                    Duration = Duration,
                    Easing = new CubicEaseOut(),
                },
            }
            : null;
    }

    private void UpdateState()
    {
        MaxHeight = Expanded ? ResolveExpandedHeight() : 0;
        InvalidateMeasure();
    }

    private double ResolveExpandedHeight()
    {
        if (!Animated || Child is null)
        {
            return double.PositiveInfinity;
        }

        if (Child.Bounds.Height > 0)
        {
            return Child.Bounds.Height;
        }

        if (!double.IsNaN(Child.Height) && Child.Height > 0)
        {
            return Child.Height;
        }

        Child.Measure(Size.Infinity);
        if (Child.DesiredSize.Height > 0)
        {
            return Child.DesiredSize.Height;
        }

        return double.PositiveInfinity;
    }
}
