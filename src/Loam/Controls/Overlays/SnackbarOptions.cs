using Loam;

namespace Loam.Controls;

/// <summary>Placement for snackbar stacks.</summary>
public enum SnackbarPosition
{
    /// <summary>Bottom-right corner.</summary>
    BottomRight,
    /// <summary>Bottom-left corner.</summary>
    BottomLeft,
    /// <summary>Top-right corner.</summary>
    TopRight,
    /// <summary>Top-left corner.</summary>
    TopLeft,
    /// <summary>Bottom center.</summary>
    BottomCenter,
    /// <summary>Top center.</summary>
    TopCenter,
}

/// <summary>Options for one snackbar notification.</summary>
public sealed record SnackbarOptions(string Message)
{
    /// <summary>Compatibility severity hint. Snackbar visuals use the theme's snackbar surface roles.</summary>
    public LoamColor Severity { get; init; } = LoamColor.Info;

    /// <summary>How long the toast stays visible. Use <see cref="Timeout.InfiniteTimeSpan"/> to keep it until dismissed.</summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>Optional action button text.</summary>
    public string? ActionText { get; init; }

    /// <summary>Action invoked when the action button is clicked.</summary>
    public Action? Action { get; init; }

    /// <summary>Optional dismiss button text. When omitted, Escape still dismisses the toast.</summary>
    public string? DismissText { get; init; }

    /// <summary>Maximum number of visible toasts after this toast is added. Uses the service default when null.</summary>
    public int? MaxVisible { get; init; }

    /// <summary>Optional stack position for this toast. Uses the service default when null.</summary>
    public SnackbarPosition? Position { get; init; }
}
