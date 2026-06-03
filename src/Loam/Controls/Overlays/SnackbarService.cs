using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Loam;

namespace Loam.Controls;

/// <summary>Default <see cref="ISnackbar"/>: stacks auto-dismissing <see cref="Alert"/> toasts at the bottom-right of the window's overlay layer.</summary>
public sealed class SnackbarService : ISnackbar
{
    private readonly TopLevel _topLevel;
    private StackPanel? _host;

    /// <summary>Creates a service targeting the given window/top level.</summary>
    public SnackbarService(TopLevel topLevel) => _topLevel = topLevel ?? throw new ArgumentNullException(nameof(topLevel));

    /// <summary>Creates a service for the window hosting <paramref name="visual"/>.</summary>
    public static SnackbarService For(Visual visual) =>
        new(TopLevel.GetTopLevel(visual) ?? throw new InvalidOperationException("The visual is not attached to a window."));

    /// <inheritdoc />
    public void Add(string message, LoamColor severity = LoamColor.Info, TimeSpan? duration = null)
    {
        var host = EnsureHost();
        if (host is null)
        {
            return;
        }

        var toast = new Alert
        {
            Color = severity,
            Variant = Variant.Filled,
            Content = new Text { Text = message, Color = LoamColor.Inherit },
            MinWidth = 280,
            Margin = new Thickness(0, 8, 0, 0),
        };
        host.Children.Add(toast);

        var timer = new DispatcherTimer { Interval = duration ?? TimeSpan.FromSeconds(4) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            host.Children.Remove(toast);
        };
        timer.Start();
    }

    private StackPanel? EnsureHost()
    {
        if (_host is { Parent: not null })
        {
            return _host;
        }

        var layer = OverlayLayer.GetOverlayLayer(_topLevel);
        if (layer is null)
        {
            return null;
        }

        _host = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 24, 24),
        };

        var root = new Panel { IsHitTestVisible = false, Children = { _host } };
        root.Bind(Layoutable.WidthProperty, layer.GetObservable(Visual.BoundsProperty, b => b.Width));
        root.Bind(Layoutable.HeightProperty, layer.GetObservable(Visual.BoundsProperty, b => b.Height));
        layer.Children.Add(root);
        return _host;
    }
}
