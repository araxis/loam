using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Loam.Controls;

/// <summary>
/// A "scroll to top" affordance, mirroring the reference API's <c>ScrollToTop</c>. Watches a
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
        AddHandler(Avalonia.Controls.Button.ClickEvent, OnChildButtonClick);
        AutomationProperties.SetName(this, "Scroll to top");
        var button = new Fab { StartIcon = Icons.Material.Filled.ExpandLess, Color = LoamColor.Primary };
        AutomationProperties.SetName(button, "Scroll to top");
        Child = button;
        IsVisible = false;
    }

    /// <summary>The scroll container to watch. Mirrors the reference API's <c>Selector</c> target.</summary>
    public ScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>The scroll distance (px) after which the control appears. Mirrors the reference API's <c>VisibleOffset</c>.</summary>
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
        if (e.Handled)
        {
            return;
        }

        ScrollTargetHome();
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_subscribed is not null)
        {
            _subscribed.ScrollChanged -= OnScrollChanged;
            _subscribed = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e) => Evaluate();

    private void OnChildButtonClick(object? sender, RoutedEventArgs e)
    {
        ScrollTargetHome();
        e.Handled = true;
    }

    private void ScrollTargetHome()
    {
        if (Target is null)
        {
            return;
        }

        Target.ScrollToHome();
        Target.Offset = default;
        Evaluate();
    }

    private void Evaluate() => IsVisible = Target is not null && Target.Offset.Y > VisibleOffset;
}
