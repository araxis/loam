using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Loam.Controls;

/// <summary>
/// A "scroll to top" affordance, mirroring MudBlazor's <c>MudScrollToTop</c>. Watches a
/// <see cref="Target"/> <see cref="ScrollViewer"/> and shows its <see cref="Decorator.Child"/> (a FAB by
/// default) once scrolled past <see cref="VisibleOffset"/>; clicking it scrolls the target home.
/// </summary>
public class ScrollToTop : Decorator
{
    /// <summary>Identifies the <see cref="Target"/> property.</summary>
    public static readonly StyledProperty<ScrollViewer?> TargetProperty =
        AvaloniaProperty.Register<ScrollToTop, ScrollViewer?>(nameof(Target));

    /// <summary>Identifies the <see cref="VisibleOffset"/> property.</summary>
    public static readonly StyledProperty<double> VisibleOffsetProperty =
        AvaloniaProperty.Register<ScrollToTop, double>(nameof(VisibleOffset), 300);

    private ScrollViewer? _subscribed;

    /// <summary>Creates the control with a default up-arrow FAB, hidden until scrolled.</summary>
    public ScrollToTop()
    {
        Child = new Fab { StartIcon = Icons.Material.Filled.ExpandLess, Color = LoamColor.Primary };
        IsVisible = false;
    }

    /// <summary>The scroll container to watch. Mirrors MudBlazor's <c>Selector</c> target.</summary>
    public ScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>The scroll distance (px) after which the control appears. Mirrors MudBlazor's <c>VisibleOffset</c>.</summary>
    public double VisibleOffset
    {
        get => GetValue(VisibleOffsetProperty);
        set => SetValue(VisibleOffsetProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TargetProperty)
        {
            if (_subscribed is not null)
            {
                _subscribed.ScrollChanged -= OnScrollChanged;
            }

            _subscribed = Target;
            if (_subscribed is not null)
            {
                _subscribed.ScrollChanged += OnScrollChanged;
            }

            Evaluate();
        }
        else if (change.Property == VisibleOffsetProperty)
        {
            Evaluate();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        Target?.ScrollToHome();
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e) => Evaluate();

    private void Evaluate() => IsVisible = Target is not null && Target.Offset.Y > VisibleOffset;
}
