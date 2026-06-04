using Avalonia;

namespace Loam.Theming;

/// <summary>Shared metrics for field-style input surfaces.</summary>
public sealed record LoamFieldMetrics
{
    /// <summary>Minimum height for outlined field surfaces.</summary>
    public double OutlinedHeight { get; init; } = 52;

    /// <summary>Minimum height for filled field surfaces.</summary>
    public double FilledHeight { get; init; } = 48;

    /// <summary>Minimum height for text field surfaces.</summary>
    public double TextHeight { get; init; } = 40;

    /// <summary>Resting outline width.</summary>
    public double OutlineWidth { get; init; } = 1;

    /// <summary>Focused or error outline width.</summary>
    public double ActiveOutlineWidth { get; init; } = 2;

    /// <summary>Internal padding for outlined fields.</summary>
    public Thickness OutlinedPadding { get; init; } = new(12, 14);

    /// <summary>Internal padding for filled fields.</summary>
    public Thickness FilledPadding { get; init; } = new(12, 12);

    /// <summary>Internal padding for text fields.</summary>
    public Thickness TextPadding { get; init; } = new(0, 9);

    /// <summary>Horizontal label offset inside the outline notch.</summary>
    public double LabelX { get; init; } = 10;

    /// <summary>Top margin applied when a floating label creates a notch.</summary>
    public double FloatingLabelTopMargin { get; init; } = 7;

    /// <summary>Horizontal padding around the floating label text.</summary>
    public double FloatingLabelHorizontalPadding { get; init; } = 5;

    /// <summary>Horizontal gap between field content and adornment icons.</summary>
    public double IconSpacing { get; init; } = 8;

    /// <summary>Top gap between the field and helper or error text.</summary>
    public double HelperTopSpacing { get; init; } = 3;

    /// <summary>The Loam defaults.</summary>
    public static LoamFieldMetrics Default { get; } = new();
}
