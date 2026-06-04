using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace Loam.Controls;

/// <summary>
/// A side navigation panel, mirroring the reference API's <c>Drawer</c>. Left-anchored; toggling
/// <see cref="Open"/> slides it between <see cref="DrawerWidth"/> and 0, and <see cref="Mini"/>
/// collapses it to <see cref="MiniWidth"/>. Place it in a <see cref="Layout"/>'s drawer slot.
/// </summary>
public class Drawer : ContentControl
{
    /// <summary>Identifies the <see cref="Open"/> property.</summary>
    public static readonly StyledProperty<bool> OpenProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(Open), defaultValue: true);

    /// <summary>Identifies the <see cref="Mini"/> property.</summary>
    public static readonly StyledProperty<bool> MiniProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(Mini));

    /// <summary>Identifies the <see cref="DrawerWidth"/> property.</summary>
    public static readonly StyledProperty<double> DrawerWidthProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(DrawerWidth), 240);

    /// <summary>Identifies the <see cref="MiniWidth"/> property.</summary>
    public static readonly StyledProperty<double> MiniWidthProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(MiniWidth), 56);

    /// <summary>Creates the drawer.</summary>
    public Drawer()
    {
        ClipToBounds = true;
        Width = ResolveWidth(Open, Mini, DrawerWidth, MiniWidth);
        Transitions = new Transitions
        {
            new DoubleTransition { Property = WidthProperty, Duration = TimeSpan.FromMilliseconds(180), Easing = new CubicEaseOut() },
        };
    }

    /// <summary>Whether the drawer is open. Mirrors the reference API's <c>Open</c>.</summary>
    public bool Open
    {
        get => GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    /// <summary>Collapse to <see cref="MiniWidth"/> instead of hiding. Mirrors the reference API's mini variant.</summary>
    public bool Mini
    {
        get => GetValue(MiniProperty);
        set => SetValue(MiniProperty, value);
    }

    /// <summary>Expanded width. Mirrors the reference API's <c>Width</c>.</summary>
    public double DrawerWidth
    {
        get => GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    /// <summary>Collapsed (mini) width. Mirrors the reference API's <c>MiniWidth</c>.</summary>
    public double MiniWidth
    {
        get => GetValue(MiniWidthProperty);
        set => SetValue(MiniWidthProperty, value);
    }

    /// <summary>The target width for the given open/mini state.</summary>
    public static double ResolveWidth(bool open, bool mini, double width, double miniWidth) =>
        !open ? 0 : mini ? miniWidth : width;

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Drawer);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OpenProperty || change.Property == MiniProperty ||
            change.Property == DrawerWidthProperty || change.Property == MiniWidthProperty)
        {
            Width = ResolveWidth(Open, Mini, DrawerWidth, MiniWidth);
        }
    }
}
