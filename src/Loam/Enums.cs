namespace Loam;

/// <summary>Visual style of a control, mirroring the reference API's <c>Variant</c>.</summary>
public enum Variant
{
    /// <summary>No fill or border; text/icon only.</summary>
    Text,

    /// <summary>Solid filled background.</summary>
    Filled,

    /// <summary>Transparent background with a border.</summary>
    Outlined,
}

/// <summary>
/// Semantic color role, mirroring the reference API's <c>Color</c>. Named <c>LoamColor</c> to avoid
/// clashing with <see cref="Avalonia.Media.Color"/>; controls still expose it via a <c>Color</c>
/// property (ADR-0007).
/// </summary>
public enum LoamColor
{
    /// <summary>Neutral default (theme text/surface).</summary>
    Default,
    /// <summary>Primary accent.</summary>
    Primary,
    /// <summary>Secondary accent.</summary>
    Secondary,
    /// <summary>Tertiary accent.</summary>
    Tertiary,
    /// <summary>Informational.</summary>
    Info,
    /// <summary>Success.</summary>
    Success,
    /// <summary>Warning.</summary>
    Warning,
    /// <summary>Error.</summary>
    Error,
    /// <summary>Neutral dark.</summary>
    Dark,
    /// <summary>Transparent.</summary>
    Transparent,
    /// <summary>Inherit the ambient color.</summary>
    Inherit,
}

/// <summary>
/// Control size, mirroring the reference API's <c>Size</c>. Named <c>LoamSize</c> to avoid clashing with
/// <see cref="Avalonia.Size"/>; controls expose it via a <c>Size</c> property (ADR-0007).
/// </summary>
public enum LoamSize
{
    /// <summary>Small.</summary>
    Small,
    /// <summary>Medium (default).</summary>
    Medium,
    /// <summary>Large.</summary>
    Large,
}

/// <summary>
/// Typographic role, mirroring the reference API's <c>Typo</c> (PascalCase here, vs the reference API's lowercase,
/// to match C# conventions and the <see cref="Theming.LoamTypography"/> scale names).
/// </summary>
public enum Typo
{
    /// <summary>Inherit the ambient typography.</summary>
    Inherit,
    /// <summary>Heading 1.</summary>
    H1,
    /// <summary>Heading 2.</summary>
    H2,
    /// <summary>Heading 3.</summary>
    H3,
    /// <summary>Heading 4.</summary>
    H4,
    /// <summary>Heading 5.</summary>
    H5,
    /// <summary>Heading 6.</summary>
    H6,
    /// <summary>Subtitle 1.</summary>
    Subtitle1,
    /// <summary>Subtitle 2.</summary>
    Subtitle2,
    /// <summary>Body 1.</summary>
    Body1,
    /// <summary>Body 2.</summary>
    Body2,
    /// <summary>Button label.</summary>
    Button,
    /// <summary>Caption.</summary>
    Caption,
    /// <summary>Overline.</summary>
    Overline,
}

/// <summary>Text alignment, mirroring the reference API's <c>Align</c>.</summary>
public enum Align
{
    /// <summary>Inherit.</summary>
    Inherit,
    /// <summary>Left.</summary>
    Left,
    /// <summary>Center.</summary>
    Center,
    /// <summary>Right.</summary>
    Right,
    /// <summary>Justify.</summary>
    Justify,
    /// <summary>Start (LTR: left).</summary>
    Start,
    /// <summary>End (LTR: right).</summary>
    End,
}

/// <summary>Divider style, mirroring the reference API's <c>DividerType</c>.</summary>
public enum DividerType
{
    /// <summary>Spans the full length.</summary>
    FullWidth,
    /// <summary>Inset from the start.</summary>
    Inset,
    /// <summary>Inset from both ends.</summary>
    Middle,
}

/// <summary>Responsive breakpoint, mirroring the reference API's <c>Breakpoint</c> (core values).</summary>
public enum Breakpoint
{
    /// <summary>No breakpoint.</summary>
    None,
    /// <summary>Extra small (&lt; 600).</summary>
    Xs,
    /// <summary>Small (≥ 600).</summary>
    Sm,
    /// <summary>Medium (≥ 960).</summary>
    Md,
    /// <summary>Large (≥ 1280).</summary>
    Lg,
    /// <summary>Extra large (≥ 1920).</summary>
    Xl,
    /// <summary>Extra extra large (≥ 2560).</summary>
    Xxl,
    /// <summary>Always.</summary>
    Always,
}

/// <summary>Corner placement for a <see cref="Controls.Badge"/> (subset of the reference API's <c>Origin</c>).</summary>
public enum BadgeOrigin
{
    /// <summary>Top-left corner.</summary>
    TopLeft,
    /// <summary>Top-right corner (default).</summary>
    TopRight,
    /// <summary>Bottom-left corner.</summary>
    BottomLeft,
    /// <summary>Bottom-right corner.</summary>
    BottomRight,
}
