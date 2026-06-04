using Loam;

namespace Loam.Controls;

/// <summary>Options for one snackbar notification.</summary>
public sealed record SnackbarOptions(string Message)
{
    /// <summary>Toast severity color.</summary>
    public LoamColor Severity { get; init; } = LoamColor.Info;

    /// <summary>How long the toast stays visible. Use <see cref="Timeout.InfiniteTimeSpan"/> to keep it until dismissed.</summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>Optional action button text.</summary>
    public string? ActionText { get; init; }

    /// <summary>Action invoked when the action button is clicked.</summary>
    public Action? Action { get; init; }

    /// <summary>Maximum number of visible toasts after this toast is added. Uses the service default when null.</summary>
    public int? MaxVisible { get; init; }
}
