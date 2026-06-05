using Avalonia.Media;

namespace Loam.Theming;

/// <summary>
/// A single typographic style. Sizes are in device-independent pixels;
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
/// The built-in type scale, mirroring the reference API's <c>Typography</c>.
/// </summary>
public sealed record LoamTypography
{
    /// <summary>Default font family with web-style fallbacks.</summary>
    public FontFamily FontFamily { get; init; } = new("Roboto, Helvetica, Arial, sans-serif");

    /// <summary>Display large.</summary>
    public LoamTypeStyle DisplayLarge { get; init; } = new() { FontSize = 57, LineHeight = 64d / 57d };
    /// <summary>Display medium.</summary>
    public LoamTypeStyle DisplayMedium { get; init; } = new() { FontSize = 45, LineHeight = 52d / 45d };
    /// <summary>Display small.</summary>
    public LoamTypeStyle DisplaySmall { get; init; } = new() { FontSize = 36, LineHeight = 44d / 36d };
    /// <summary>Headline large.</summary>
    public LoamTypeStyle HeadlineLarge { get; init; } = new() { FontSize = 32, LineHeight = 40d / 32d };
    /// <summary>Headline medium.</summary>
    public LoamTypeStyle HeadlineMedium { get; init; } = new() { FontSize = 28, LineHeight = 36d / 28d };
    /// <summary>Headline small.</summary>
    public LoamTypeStyle HeadlineSmall { get; init; } = new() { FontSize = 24, LineHeight = 32d / 24d };
    /// <summary>Title large.</summary>
    public LoamTypeStyle TitleLarge { get; init; } = new() { FontSize = 22, FontWeight = FontWeight.Medium, LineHeight = 28d / 22d };
    /// <summary>Title medium.</summary>
    public LoamTypeStyle TitleMedium { get; init; } = new() { FontSize = 16, FontWeight = FontWeight.Medium, LineHeight = 24d / 16d, LetterSpacing = 0.00938 };
    /// <summary>Title small.</summary>
    public LoamTypeStyle TitleSmall { get; init; } = new() { FontSize = 14, FontWeight = FontWeight.Medium, LineHeight = 20d / 14d, LetterSpacing = 0.00714 };
    /// <summary>Body large.</summary>
    public LoamTypeStyle BodyLarge { get; init; } = new() { FontSize = 16, LineHeight = 24d / 16d, LetterSpacing = 0.03125 };
    /// <summary>Body medium.</summary>
    public LoamTypeStyle BodyMedium { get; init; } = new() { FontSize = 14, LineHeight = 20d / 14d, LetterSpacing = 0.01786 };
    /// <summary>Body small.</summary>
    public LoamTypeStyle BodySmall { get; init; } = new() { FontSize = 12, LineHeight = 16d / 12d, LetterSpacing = 0.03333 };
    /// <summary>Label large.</summary>
    public LoamTypeStyle LabelLarge { get; init; } = new() { FontSize = 14, FontWeight = FontWeight.Medium, LineHeight = 20d / 14d, LetterSpacing = 0.00714 };
    /// <summary>Label medium.</summary>
    public LoamTypeStyle LabelMedium { get; init; } = new() { FontSize = 12, FontWeight = FontWeight.Medium, LineHeight = 16d / 12d, LetterSpacing = 0.04167 };
    /// <summary>Label small.</summary>
    public LoamTypeStyle LabelSmall { get; init; } = new() { FontSize = 11, FontWeight = FontWeight.Medium, LineHeight = 16d / 11d, LetterSpacing = 0.04545 };

    /// <summary>Base body style applied when no explicit typo is set.</summary>
    public LoamTypeStyle Default { get; init; } = new() { FontSize = 14, LineHeight = 20d / 14d, LetterSpacing = 0.01786 };
    /// <summary>Heading 1.</summary>
    public LoamTypeStyle H1 { get; init; } = new() { FontSize = 57, LineHeight = 64d / 57d };
    /// <summary>Heading 2.</summary>
    public LoamTypeStyle H2 { get; init; } = new() { FontSize = 45, LineHeight = 52d / 45d };
    /// <summary>Heading 3.</summary>
    public LoamTypeStyle H3 { get; init; } = new() { FontSize = 36, LineHeight = 44d / 36d };
    /// <summary>Heading 4.</summary>
    public LoamTypeStyle H4 { get; init; } = new() { FontSize = 32, LineHeight = 40d / 32d };
    /// <summary>Heading 5.</summary>
    public LoamTypeStyle H5 { get; init; } = new() { FontSize = 28, LineHeight = 36d / 28d };
    /// <summary>Heading 6.</summary>
    public LoamTypeStyle H6 { get; init; } = new() { FontSize = 24, LineHeight = 32d / 24d };
    /// <summary>Subtitle 1.</summary>
    public LoamTypeStyle Subtitle1 { get; init; } = new() { FontSize = 16, FontWeight = FontWeight.Medium, LineHeight = 24d / 16d, LetterSpacing = 0.00938 };
    /// <summary>Subtitle 2.</summary>
    public LoamTypeStyle Subtitle2 { get; init; } = new() { FontSize = 14, FontWeight = FontWeight.Medium, LineHeight = 20d / 14d, LetterSpacing = 0.00714 };
    /// <summary>Body 1.</summary>
    public LoamTypeStyle Body1 { get; init; } = new() { FontSize = 16, LineHeight = 24d / 16d, LetterSpacing = 0.03125 };
    /// <summary>Body 2.</summary>
    public LoamTypeStyle Body2 { get; init; } = new() { FontSize = 14, LineHeight = 20d / 14d, LetterSpacing = 0.01786 };
    /// <summary>Button label.</summary>
    public LoamTypeStyle Button { get; init; } = new() { FontSize = 14, FontWeight = FontWeight.Medium, LineHeight = 20d / 14d, LetterSpacing = 0.00714 };
    /// <summary>Caption.</summary>
    public LoamTypeStyle Caption { get; init; } = new() { FontSize = 12, LineHeight = 16d / 12d, LetterSpacing = 0.03333 };
    /// <summary>Overline.</summary>
    public LoamTypeStyle Overline { get; init; } = new() { FontSize = 12, LineHeight = 2.66, LetterSpacing = 0.08333, Uppercase = true };

    /// <summary>All named styles, for projection and showcases.</summary>
    public IReadOnlyList<(string Name, LoamTypeStyle Style)> Scales =>
    [
        (nameof(Default), Default), (nameof(H1), H1), (nameof(H2), H2), (nameof(H3), H3),
        (nameof(H4), H4), (nameof(H5), H5), (nameof(H6), H6), (nameof(Subtitle1), Subtitle1),
        (nameof(Subtitle2), Subtitle2), (nameof(Body1), Body1), (nameof(Body2), Body2),
        (nameof(Button), Button), (nameof(Caption), Caption), (nameof(Overline), Overline),
        (nameof(DisplayLarge), DisplayLarge), (nameof(DisplayMedium), DisplayMedium),
        (nameof(DisplaySmall), DisplaySmall), (nameof(HeadlineLarge), HeadlineLarge),
        (nameof(HeadlineMedium), HeadlineMedium), (nameof(HeadlineSmall), HeadlineSmall),
        (nameof(TitleLarge), TitleLarge), (nameof(TitleMedium), TitleMedium),
        (nameof(TitleSmall), TitleSmall), (nameof(BodyLarge), BodyLarge),
        (nameof(BodyMedium), BodyMedium), (nameof(BodySmall), BodySmall),
        (nameof(LabelLarge), LabelLarge), (nameof(LabelMedium), LabelMedium),
        (nameof(LabelSmall), LabelSmall),
    ];
}
