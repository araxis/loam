using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Loam.Controls;

/// <summary>
/// The app-shell root, mirroring the reference API's <c>Layout</c>. Docks an <see cref="AppBar"/> at the
/// top (full width), a <see cref="Drawer"/> on the left below it, and fills the rest with its
/// <see cref="ContentControl.Content"/> (typically a <see cref="MainContent"/>).
/// </summary>
public class Layout : ContentControl
{
    /// <summary>Identifies the <see cref="AppBar"/> property.</summary>
    public static readonly StyledProperty<object?> AppBarProperty =
        AvaloniaProperty.Register<Layout, object?>(nameof(AppBar));

    /// <summary>Identifies the <see cref="Drawer"/> property.</summary>
    public static readonly StyledProperty<object?> DrawerProperty =
        AvaloniaProperty.Register<Layout, object?>(nameof(Drawer));

    /// <summary>Creates the shell layout.</summary>
    public Layout()
    {
        Focusable = true;
    }

    /// <summary>The top app bar (typically an <see cref="Controls.AppBar"/>).</summary>
    public object? AppBar
    {
        get => GetValue(AppBarProperty);
        set => SetValue(AppBarProperty, value);
    }

    /// <summary>The side drawer (typically a <see cref="Controls.Drawer"/>).</summary>
    public object? Drawer
    {
        get => GetValue(DrawerProperty);
        set => SetValue(DrawerProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Layout);

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape && Drawer is Drawer { Mode: DrawerMode.Temporary, Open: true } drawer)
        {
            drawer.Open = false;
            e.Handled = true;
        }
    }
}
