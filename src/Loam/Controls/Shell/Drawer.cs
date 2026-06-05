using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>The layout behavior for a <see cref="Drawer"/>.</summary>
public enum DrawerMode
{
    /// <summary>The drawer reserves horizontal layout space.</summary>
    Docked,

    /// <summary>The drawer floats over the main content and may show a scrim.</summary>
    Temporary,
}

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

    /// <summary>Identifies the <see cref="Mode"/> property.</summary>
    public static readonly StyledProperty<DrawerMode> ModeProperty =
        AvaloniaProperty.Register<Drawer, DrawerMode>(nameof(Mode), DrawerMode.Docked);

    /// <summary>Identifies the <see cref="ShowScrim"/> property.</summary>
    public static readonly StyledProperty<bool> ShowScrimProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(ShowScrim), true);

    /// <summary>Identifies the <see cref="CloseOnScrimClick"/> property.</summary>
    public static readonly StyledProperty<bool> CloseOnScrimClickProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(CloseOnScrimClick), true);

    /// <summary>Creates the drawer.</summary>
    public Drawer()
    {
        ClipToBounds = true;
        Focusable = true;
        Width = ResolveWidth(Open, Mini, DrawerWidth, MiniWidth);
        UpdateTransitions();
        InteractionAssist.SetAutomationName(this, "Navigation drawer");
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

    /// <summary>Whether the drawer is docked in layout or temporary over content.</summary>
    public DrawerMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>Whether a temporary drawer displays a scrim over content when open.</summary>
    public bool ShowScrim
    {
        get => GetValue(ShowScrimProperty);
        set => SetValue(ShowScrimProperty, value);
    }

    /// <summary>Whether clicking a temporary drawer scrim closes the drawer.</summary>
    public bool CloseOnScrimClick
    {
        get => GetValue(CloseOnScrimClickProperty);
        set => SetValue(CloseOnScrimClickProperty, value);
    }

    /// <summary>The target width for the given open/mini state.</summary>
    public static double ResolveWidth(bool open, bool mini, double width, double miniWidth) =>
        !open ? 0 : mini ? miniWidth : width;

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Drawer);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateTransitions();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OpenProperty || change.Property == MiniProperty ||
            change.Property == DrawerWidthProperty || change.Property == MiniWidthProperty ||
            change.Property == ModeProperty)
        {
            Width = ResolveWidth(Open, Mini, DrawerWidth, MiniWidth);
            InvalidateMeasure();
        }
        else if (change.Property == IsEnabledProperty)
        {
            Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsEnabled && e.Key == Key.Escape && Mode == DrawerMode.Temporary && Open)
        {
            Open = false;
            e.Handled = true;
        }
    }

    private void UpdateTransitions()
    {
        Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = WidthProperty,
                Duration = InteractionAssist.DurationToken(this, LoamTokens.MotionDurationShort3, TimeSpan.FromMilliseconds(180)),
                Easing = new CubicEaseOut(),
            },
        };
    }
}
