namespace Loam.Theming;

/// <summary>Opacity values used for interaction state layers.</summary>
public sealed record LoamStateLayer
{
    /// <summary>Overlay opacity for hover state.</summary>
    public double HoverOpacity { get; init; } = 0.08;

    /// <summary>Overlay opacity for focus state.</summary>
    public double FocusOpacity { get; init; } = 0.12;

    /// <summary>Overlay opacity for pressed state.</summary>
    public double PressedOpacity { get; init; } = 0.12;

    /// <summary>Overlay opacity for selected state.</summary>
    public double SelectedOpacity { get; init; } = 0.12;

    /// <summary>Overlay opacity for dragged state.</summary>
    public double DraggedOpacity { get; init; } = 0.16;

    /// <summary>Opacity applied to disabled controls.</summary>
    public double DisabledOpacity { get; init; } = 0.38;

    /// <summary>The Loam defaults.</summary>
    public static LoamStateLayer Default { get; } = new();
}
