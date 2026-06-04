using Loam;

namespace Loam.Controls;

/// <summary>
/// Shows transient toast notifications, mirroring the reference API's <c>ISnackbar</c>. Create one with
/// <see cref="SnackbarService.For"/>; it stacks toasts in the window's overlay layer and auto-dismisses them.
/// </summary>
public interface ISnackbar
{
    /// <summary>Queues a toast with a severity color and optional duration (default 4s).</summary>
    void Add(string message, LoamColor severity = LoamColor.Info, TimeSpan? duration = null);
}
