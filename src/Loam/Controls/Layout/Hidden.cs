using Avalonia;
using Avalonia.Controls;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>How a <see cref="Hidden"/> rule compares the current breakpoint to its <see cref="Hidden.Breakpoint"/>.</summary>
public enum HiddenMode
{
    /// <summary>Hidden at the breakpoint and every smaller one (e.g. "hide on mobile").</summary>
    Down,

    /// <summary>Hidden at the breakpoint and every larger one.</summary>
    Up,

    /// <summary>Hidden only at exactly the breakpoint.</summary>
    Only,
}

/// <summary>
/// Conditionally hides its content by viewport breakpoint, mirroring MudBlazor's <c>MudHidden</c>.
/// Tracks the host window width and hides the <see cref="Decorator.Child"/> when the current
/// <see cref="Breakpoints"/> bucket satisfies the <see cref="Mode"/> rule against <see cref="Breakpoint"/>.
/// </summary>
public class Hidden : Decorator
{
    /// <summary>Identifies the <see cref="Breakpoint"/> property.</summary>
    public static readonly StyledProperty<Breakpoint> BreakpointProperty =
        AvaloniaProperty.Register<Hidden, Breakpoint>(nameof(Breakpoint), Loam.Breakpoint.Sm);

    /// <summary>Identifies the <see cref="Mode"/> property.</summary>
    public static readonly StyledProperty<HiddenMode> ModeProperty =
        AvaloniaProperty.Register<Hidden, HiddenMode>(nameof(Mode), HiddenMode.Down);

    private IDisposable? _widthSubscription;

    /// <summary>The breakpoint the rule is relative to. Mirrors MudBlazor's <c>Breakpoint</c>.</summary>
    public Breakpoint Breakpoint
    {
        get => GetValue(BreakpointProperty);
        set => SetValue(BreakpointProperty, value);
    }

    /// <summary>How the current breakpoint is compared to <see cref="Breakpoint"/>.</summary>
    public HiddenMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>Returns whether content is hidden for <paramref name="current"/> under the rule.</summary>
    public static bool IsHiddenAt(Breakpoint current, Breakpoint breakpoint, HiddenMode mode) => mode switch
    {
        HiddenMode.Down => current <= breakpoint,
        HiddenMode.Up => current >= breakpoint,
        _ => current == breakpoint,
    };

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var top = TopLevel.GetTopLevel(this);
        if (top is not null)
        {
            _widthSubscription = top.GetObservable(BoundsProperty).Subscribe(new WidthObserver(b => Evaluate(b.Width)));
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _widthSubscription?.Dispose();
        _widthSubscription = null;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BreakpointProperty || change.Property == ModeProperty)
        {
            var top = TopLevel.GetTopLevel(this);
            if (top is not null)
            {
                Evaluate(top.Bounds.Width);
            }
        }
    }

    private void Evaluate(double width)
    {
        if (width <= 0)
        {
            return;
        }

        IsVisible = !IsHiddenAt(Breakpoints.FromWidth(width), Breakpoint, Mode);
    }

    private sealed class WidthObserver : IObserver<Rect>
    {
        private readonly Action<Rect> _onNext;

        public WidthObserver(Action<Rect> onNext) => _onNext = onNext;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Rect value) => _onNext(value);
    }
}
