using Avalonia;
using Avalonia.Controls;

namespace Loam.Controls;

/// <summary>
/// A height-clipping container, mirroring MudBlazor's <c>MudCollapse</c>. Hosts a single
/// <see cref="Decorator.Child"/> and reveals it when <see cref="Expanded"/>, clipping it away
/// (height 0) when collapsed. (Slide animation is a future enhancement.)
/// </summary>
public class Collapse : Decorator
{
    /// <summary>Identifies the <see cref="Expanded"/> property.</summary>
    public static readonly StyledProperty<bool> ExpandedProperty =
        AvaloniaProperty.Register<Collapse, bool>(nameof(Expanded),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Creates the collapse.</summary>
    public Collapse()
    {
        ClipToBounds = true;
        UpdateState();
    }

    /// <summary>Whether the content is shown. Mirrors MudBlazor's <c>Expanded</c>.</summary>
    public bool Expanded
    {
        get => GetValue(ExpandedProperty);
        set => SetValue(ExpandedProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ExpandedProperty)
        {
            UpdateState();
        }
    }

    private void UpdateState() => MaxHeight = Expanded ? double.PositiveInfinity : 0;
}
