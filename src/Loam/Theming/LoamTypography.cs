using Avalonia.Media;

namespace Loam.Theming;

/// <summary>
/// A single typographic style. Sizes are in device-independent pixels (MudBlazor rem × 16);
/// <see cref="LineHeight"/> is a multiplier and <see cref="LineHeightPx"/> the absolute value
/// Avalonia's <c>TextBlock.LineHeight</c> expects.
/// </summary>
public sealed record LoamTypeStyle
{
    /// <summary>Font size in device-independent pixels.</summary>
    public required double FontSize { get; init; }

    /// <summary>Font weight.</summary>
    public FontWeight FontWeight { get; init; } = FontWeight.Normal;

    /// <summary>Line height as a multiple of <see cref="FontSize"/>.</summary>
    public double LineHeight { get; init; } = 1.5;

    /// <summary>Letter spacing in em (applied by text controls that support it).</summary>
    public double LetterSpacing { get; init; }

    /// <summary>Whether the text is rendered uppercase (buttons, overline).</summary>
    public bool Uppercase { get; init; }

    /// <summary>Absolute line height in pixels for <c>TextBlock.LineHeight</c>.</summary>
    public double LineHeightPx => FontSize * LineHeight;
}

/// <summary>
/// The Material type scale, mirroring MudBlazor's <c>Typography</c> (values verified against
/// MudBlazor v9.5.0; Button/Caption/Overline use the standard MUI defaults).
/// </summary>
public sealed record LoamTypography
{
    /// <summary>Default font family with web-style fallbacks.</summary>
    public FontFamily FontFamily { get; init; } = new("Roboto, Helvetica, Arial, sans-serif");

    /// <summary>Base body style applied when no explicit typo is set.</summary>
    public LoamTypeStyle Default { get; init; } = new() { FontSize = 14, LineHeight = 1.43, LetterSpacing = 0.01071 };
    /// <summary>Heading 1.</summary>
    public LoamTypeStyle H1 { get; init; } = new() { FontSize = 96, FontWeight = FontWeight.Light, LineHeight = 1.167, LetterSpacing = -0.01562 };
    /// <summary>Heading 2.</summary>
    public LoamTypeStyle H2 { get; init; } = new() { FontSize = 60, FontWeight = FontWeight.Light, LineHeight = 1.2, LetterSpacing = -0.00833 };
    /// <summary>Heading 3.</summary>
    public LoamTypeStyle H3 { get; init; } = new() { FontSize = 48, LineHeight = 1.167 };
    /// <summary>Heading 4.</summary>
    public LoamTypeStyle H4 { get; init; } = new() { FontSize = 34, LineHeight = 1.235, LetterSpacing = 0.00735 };
    /// <summary>Heading 5.</summary>
    public LoamTypeStyle H5 { get; init; } = new() { FontSize = 24, LineHeight = 1.334 };
    /// <summary>Heading 6.</summary>
    public LoamTypeStyle H6 { get; init; } = new() { FontSize = 20, FontWeight = FontWeight.Medium, LineHeight = 1.6, LetterSpacing = 0.0075 };
    /// <summary>Subtitle 1.</summary>
    public LoamTypeStyle Subtitle1 { get; init; } = new() { FontSize = 16, LineHeight = 1.75, LetterSpacing = 0.00938 };
    /// <summary>Subtitle 2.</summary>
    public LoamTypeStyle Subtitle2 { get; init; } = new() { FontSize = 14, FontWeight = FontWeight.Medium, LineHeight = 1.57, LetterSpacing = 0.00714 };
    /// <summary>Body 1.</summary>
    public LoamTypeStyle Body1 { get; init; } = new() { FontSize = 16, LineHeight = 1.5, LetterSpacing = 0.00938 };
    /// <summary>Body 2.</summary>
    public LoamTypeStyle Body2 { get; init; } = new() { FontSize = 14, LineHeight = 1.43, LetterSpacing = 0.01071 };
    /// <summary>Button label.</summary>
    public LoamTypeStyle Button { get; init; } = new() { FontSize = 14, FontWeight = FontWeight.Medium, LineHeight = 1.75, LetterSpacing = 0.02857, Uppercase = true };
    /// <summary>Caption.</summary>
    public LoamTypeStyle Caption { get; init; } = new() { FontSize = 12, LineHeight = 1.66, LetterSpacing = 0.03333 };
    /// <summary>Overline.</summary>
    public LoamTypeStyle Overline { get; init; } = new() { FontSize = 12, LineHeight = 2.66, LetterSpacing = 0.08333, Uppercase = true };

    /// <summary>All named styles, for projection and showcases.</summary>
    public IReadOnlyList<(string Name, LoamTypeStyle Style)> Scales =>
    [
        (nameof(Default), Default), (nameof(H1), H1), (nameof(H2), H2), (nameof(H3), H3),
        (nameof(H4), H4), (nameof(H5), H5), (nameof(H6), H6), (nameof(Subtitle1), Subtitle1),
        (nameof(Subtitle2), Subtitle2), (nameof(Body1), Body1), (nameof(Body2), Body2),
        (nameof(Button), Button), (nameof(Caption), Caption), (nameof(Overline), Overline),
    ];
}
