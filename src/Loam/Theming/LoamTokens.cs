namespace Loam.Theming;

/// <summary>
/// Stable resource keys for Loam design tokens. Controls bind to these (never to literal
/// colors/sizes) so a single <see cref="LoamTheme"/> drives the whole app and runtime/variant
/// swaps re-style everything (ADR-0005). Keys are namespaced <c>Loam.&lt;group&gt;.&lt;name&gt;</c>.
/// </summary>
/// <remarks>
/// The full palette is projected by reflection over <see cref="LoamPalette"/> property names, so
/// <c>Palette(nameof(LoamPalette.X))</c> always resolves even without a dedicated constant here.
/// The constants below cover the most frequently bound tokens for ergonomics.
/// </remarks>
public static class LoamTokens
{
    /// <summary>Resource key for a role-based color-scheme brush.</summary>
    public static string ColorScheme(string role) => $"Loam.Color.{role}";

    /// <summary>Resource key for a color-scheme role's state-layer brush.</summary>
    public static string ColorSchemeStateLayer(string role, string state) => $"Loam.Color.{role}.{state}";

    /// <summary>Resource key for a palette color brush, e.g. <c>Palette(nameof(LoamPalette.Primary))</c>.</summary>
    public static string Palette(string name) => $"Loam.Palette.{name}";

    /// <summary>Resource key for a semantic color's contrast (on-color) text brush.</summary>
    public static string PaletteContrast(string name) => $"Loam.Palette.{name}ContrastText";

    /// <summary>Resource key for a semantic color's hover overlay brush (color at hover opacity).</summary>
    public static string PaletteHover(string name) => $"Loam.Palette.{name}.Hover";

    /// <summary>Resource key for a semantic color's focus overlay brush.</summary>
    public static string PaletteFocus(string name) => $"Loam.Palette.{name}.Focus";

    /// <summary>Resource key for a semantic color's pressed overlay brush.</summary>
    public static string PalettePressed(string name) => $"Loam.Palette.{name}.Pressed";

    /// <summary>Resource key for a semantic color's selected overlay brush.</summary>
    public static string PaletteSelected(string name) => $"Loam.Palette.{name}.Selected";

    /// <summary>Resource key for a semantic color's darkened brush (pressed/active fill).</summary>
    public static string PaletteDarken(string name) => $"Loam.Palette.{name}.Darken";

    // Common palette brushes
    /// <summary>Primary role brush.</summary>
    public const string ColorPrimary = "Loam.Color.Primary";
    /// <summary>Content on primary role brush.</summary>
    public const string ColorOnPrimary = "Loam.Color.OnPrimary";
    /// <summary>Primary container role brush.</summary>
    public const string ColorPrimaryContainer = "Loam.Color.PrimaryContainer";
    /// <summary>Content on primary container role brush.</summary>
    public const string ColorOnPrimaryContainer = "Loam.Color.OnPrimaryContainer";
    /// <summary>Secondary role brush.</summary>
    public const string ColorSecondary = "Loam.Color.Secondary";
    /// <summary>Content on secondary role brush.</summary>
    public const string ColorOnSecondary = "Loam.Color.OnSecondary";
    /// <summary>Secondary container role brush.</summary>
    public const string ColorSecondaryContainer = "Loam.Color.SecondaryContainer";
    /// <summary>Content on secondary container role brush.</summary>
    public const string ColorOnSecondaryContainer = "Loam.Color.OnSecondaryContainer";
    /// <summary>Tertiary role brush.</summary>
    public const string ColorTertiary = "Loam.Color.Tertiary";
    /// <summary>Content on tertiary role brush.</summary>
    public const string ColorOnTertiary = "Loam.Color.OnTertiary";
    /// <summary>Error role brush.</summary>
    public const string ColorError = "Loam.Color.Error";
    /// <summary>Content on error role brush.</summary>
    public const string ColorOnError = "Loam.Color.OnError";
    /// <summary>Default surface role brush.</summary>
    public const string ColorSurface = "Loam.Color.Surface";
    /// <summary>Content on surface role brush.</summary>
    public const string ColorOnSurface = "Loam.Color.OnSurface";
    /// <summary>Surface variant role brush.</summary>
    public const string ColorSurfaceVariant = "Loam.Color.SurfaceVariant";
    /// <summary>Content on surface variant role brush.</summary>
    public const string ColorOnSurfaceVariant = "Loam.Color.OnSurfaceVariant";
    /// <summary>Default surface container role brush.</summary>
    public const string ColorSurfaceContainer = "Loam.Color.SurfaceContainer";
    /// <summary>High surface container role brush.</summary>
    public const string ColorSurfaceContainerHigh = "Loam.Color.SurfaceContainerHigh";
    /// <summary>Highest surface container role brush.</summary>
    public const string ColorSurfaceContainerHighest = "Loam.Color.SurfaceContainerHighest";
    /// <summary>Outline role brush.</summary>
    public const string ColorOutline = "Loam.Color.Outline";
    /// <summary>Outline variant role brush.</summary>
    public const string ColorOutlineVariant = "Loam.Color.OutlineVariant";
    /// <summary>Scrim role brush.</summary>
    public const string ColorScrim = "Loam.Color.Scrim";

    /// <summary>Primary accent brush.</summary>
    public const string Primary = "Loam.Palette.Primary";
    /// <summary>Brush for content on top of <see cref="Primary"/>.</summary>
    public const string PrimaryContrastText = "Loam.Palette.PrimaryContrastText";
    /// <summary>Secondary accent brush.</summary>
    public const string Secondary = "Loam.Palette.Secondary";
    /// <summary>Tertiary accent brush.</summary>
    public const string Tertiary = "Loam.Palette.Tertiary";
    /// <summary>Info brush.</summary>
    public const string Info = "Loam.Palette.Info";
    /// <summary>Success brush.</summary>
    public const string Success = "Loam.Palette.Success";
    /// <summary>Warning brush.</summary>
    public const string Warning = "Loam.Palette.Warning";
    /// <summary>Error brush.</summary>
    public const string Error = "Loam.Palette.Error";
    /// <summary>Elevated surface background brush.</summary>
    public const string Surface = "Loam.Palette.Surface";
    /// <summary>Page/app background brush.</summary>
    public const string Background = "Loam.Palette.Background";
    /// <summary>Primary text brush.</summary>
    public const string TextPrimary = "Loam.Palette.TextPrimary";
    /// <summary>Secondary text brush.</summary>
    public const string TextSecondary = "Loam.Palette.TextSecondary";
    /// <summary>Disabled text brush.</summary>
    public const string TextDisabled = "Loam.Palette.TextDisabled";
    /// <summary>Default border/line brush.</summary>
    public const string LinesDefault = "Loam.Palette.LinesDefault";
    /// <summary>Divider brush.</summary>
    public const string Divider = "Loam.Palette.Divider";
    /// <summary>Default action/icon brush.</summary>
    public const string ActionDefault = "Loam.Palette.ActionDefault";

    // Typography
    /// <summary>Default font family.</summary>
    public const string FontFamily = "Loam.Typography.FontFamily";
    /// <summary>Resource key for a type scale's font size (double, px).</summary>
    public static string TypographyFontSize(string name) => $"Loam.Typography.{name}.FontSize";
    /// <summary>Resource key for a type scale's font weight.</summary>
    public static string TypographyFontWeight(string name) => $"Loam.Typography.{name}.FontWeight";
    /// <summary>Resource key for a type scale's absolute line height (double, px).</summary>
    public static string TypographyLineHeight(string name) => $"Loam.Typography.{name}.LineHeight";

    // Elevation
    /// <summary>Resource key for an elevation box-shadow set (0–25).</summary>
    public static string Elevation(int level) => $"Loam.Elevation.{level}";
    /// <summary>Elevation level 1 box-shadow set.</summary>
    public const string Elevation1 = "Loam.Elevation.1";

    // Layout
    /// <summary>Default control corner radius.</summary>
    public const string DefaultCornerRadius = "Loam.Layout.CornerRadius";
    /// <summary>Expanded drawer width (double).</summary>
    public const string DrawerWidth = "Loam.Layout.DrawerWidth";
    /// <summary>Collapsed (mini) drawer width (double).</summary>
    public const string DrawerMiniWidth = "Loam.Layout.DrawerMiniWidth";
    /// <summary>App bar height (double).</summary>
    public const string AppBarHeight = "Loam.Layout.AppBarHeight";

    // Shape
    /// <summary>No component corner radius.</summary>
    public const string ShapeNone = "Loam.Shape.None";
    /// <summary>Extra-small component corner radius.</summary>
    public const string ShapeExtraSmall = "Loam.Shape.ExtraSmall";
    /// <summary>Small component corner radius.</summary>
    public const string ShapeSmall = "Loam.Shape.Small";
    /// <summary>Medium component corner radius.</summary>
    public const string ShapeMedium = "Loam.Shape.Medium";
    /// <summary>Large component corner radius.</summary>
    public const string ShapeLarge = "Loam.Shape.Large";
    /// <summary>Large-increased component corner radius.</summary>
    public const string ShapeLargeIncreased = "Loam.Shape.LargeIncreased";
    /// <summary>Extra-large component corner radius.</summary>
    public const string ShapeExtraLarge = "Loam.Shape.ExtraLarge";
    /// <summary>Extra-large-increased component corner radius.</summary>
    public const string ShapeExtraLargeIncreased = "Loam.Shape.ExtraLargeIncreased";
    /// <summary>Extra-extra-large component corner radius.</summary>
    public const string ShapeExtraExtraLarge = "Loam.Shape.ExtraExtraLarge";
    /// <summary>Full pill/circle component corner radius.</summary>
    public const string ShapeFull = "Loam.Shape.Full";

    // Spacing
    /// <summary>Resource key for a spacing token.</summary>
    public static string Spacing(string name) => $"Loam.Spacing.{name}";
    /// <summary>Small spacing token.</summary>
    public const string SpacingSmall = "Loam.Spacing.Small";
    /// <summary>Medium spacing token.</summary>
    public const string SpacingMedium = "Loam.Spacing.Medium";
    /// <summary>Large spacing token.</summary>
    public const string SpacingLarge = "Loam.Spacing.Large";
    /// <summary>Extra-large spacing token.</summary>
    public const string SpacingExtraLarge = "Loam.Spacing.ExtraLarge";

    // Stroke
    /// <summary>Resource key for a stroke token.</summary>
    public static string Stroke(string name) => $"Loam.Stroke.{name}";
    /// <summary>Thin stroke token.</summary>
    public const string StrokeThin = "Loam.Stroke.Thin";
    /// <summary>Focus stroke token.</summary>
    public const string StrokeFocus = "Loam.Stroke.Focus";

    // Density
    /// <summary>Resource key for a component density token.</summary>
    public static string Density(string name) => $"Loam.Density.{name}";
    /// <summary>Extra-small interactive target size.</summary>
    public const string DensityInteractiveExtraSmall = "Loam.Density.InteractiveExtraSmall";
    /// <summary>Small interactive target size.</summary>
    public const string DensityInteractiveSmall = "Loam.Density.InteractiveSmall";
    /// <summary>Default interactive target size.</summary>
    public const string DensityInteractiveMedium = "Loam.Density.InteractiveMedium";
    /// <summary>Large interactive target size.</summary>
    public const string DensityInteractiveLarge = "Loam.Density.InteractiveLarge";
    /// <summary>Extra-large interactive target size.</summary>
    public const string DensityInteractiveExtraLarge = "Loam.Density.InteractiveExtraLarge";
    /// <summary>Extra-small button container height.</summary>
    public const string DensityButtonContainerHeightExtraSmall = "Loam.Density.ButtonContainerHeightExtraSmall";
    /// <summary>Small button container height.</summary>
    public const string DensityButtonContainerHeightSmall = "Loam.Density.ButtonContainerHeightSmall";
    /// <summary>Medium button container height.</summary>
    public const string DensityButtonContainerHeightMedium = "Loam.Density.ButtonContainerHeightMedium";
    /// <summary>Large button container height.</summary>
    public const string DensityButtonContainerHeightLarge = "Loam.Density.ButtonContainerHeightLarge";
    /// <summary>Extra-large button container height.</summary>
    public const string DensityButtonContainerHeightExtraLarge = "Loam.Density.ButtonContainerHeightExtraLarge";
    /// <summary>Extra-small button padding.</summary>
    public const string DensityButtonPaddingExtraSmall = "Loam.Density.ButtonPaddingExtraSmall";
    /// <summary>Small button padding.</summary>
    public const string DensityButtonPaddingSmall = "Loam.Density.ButtonPaddingSmall";
    /// <summary>Default button padding.</summary>
    public const string DensityButtonPaddingMedium = "Loam.Density.ButtonPaddingMedium";
    /// <summary>Large button padding.</summary>
    public const string DensityButtonPaddingLarge = "Loam.Density.ButtonPaddingLarge";
    /// <summary>Extra-large button padding.</summary>
    public const string DensityButtonPaddingExtraLarge = "Loam.Density.ButtonPaddingExtraLarge";
    /// <summary>Extra-small icon button padding.</summary>
    public const string DensityIconButtonPaddingExtraSmall = "Loam.Density.IconButtonPaddingExtraSmall";
    /// <summary>Small icon button padding.</summary>
    public const string DensityIconButtonPaddingSmall = "Loam.Density.IconButtonPaddingSmall";
    /// <summary>Default icon button padding.</summary>
    public const string DensityIconButtonPaddingMedium = "Loam.Density.IconButtonPaddingMedium";
    /// <summary>Large icon button padding.</summary>
    public const string DensityIconButtonPaddingLarge = "Loam.Density.IconButtonPaddingLarge";
    /// <summary>Extra-large icon button padding.</summary>
    public const string DensityIconButtonPaddingExtraLarge = "Loam.Density.IconButtonPaddingExtraLarge";
    /// <summary>Default data header padding.</summary>
    public const string DensityDataHeaderPadding = "Loam.Density.DataHeaderPadding";
    /// <summary>Compact data header padding.</summary>
    public const string DensityDataHeaderPaddingDense = "Loam.Density.DataHeaderPaddingDense";
    /// <summary>Default data cell padding.</summary>
    public const string DensityDataCellPadding = "Loam.Density.DataCellPadding";
    /// <summary>Compact data cell padding.</summary>
    public const string DensityDataCellPaddingDense = "Loam.Density.DataCellPaddingDense";

    // Tonal elevation
    /// <summary>Resource key for a tonal elevation surface brush.</summary>
    public static string TonalElevation(int level) => $"Loam.Elevation.Tonal.{level}";
    /// <summary>Resource key for an elevation shadow mapping token.</summary>
    public static string ElevationShadow(string name) => $"Loam.Elevation.Shadow.{name}";

    // Field metrics
    /// <summary>Outlined field minimum height.</summary>
    public const string FieldOutlinedHeight = "Loam.Field.OutlinedHeight";
    /// <summary>Filled field minimum height.</summary>
    public const string FieldFilledHeight = "Loam.Field.FilledHeight";
    /// <summary>Text field minimum height.</summary>
    public const string FieldTextHeight = "Loam.Field.TextHeight";
    /// <summary>Resting field outline width.</summary>
    public const string FieldOutlineWidth = "Loam.Field.OutlineWidth";
    /// <summary>Active field outline width.</summary>
    public const string FieldActiveOutlineWidth = "Loam.Field.ActiveOutlineWidth";
    /// <summary>Outlined field padding.</summary>
    public const string FieldOutlinedPadding = "Loam.Field.OutlinedPadding";
    /// <summary>Filled field padding.</summary>
    public const string FieldFilledPadding = "Loam.Field.FilledPadding";
    /// <summary>Text field padding.</summary>
    public const string FieldTextPadding = "Loam.Field.TextPadding";
    /// <summary>Floating label x offset.</summary>
    public const string FieldLabelX = "Loam.Field.LabelX";
    /// <summary>Floating label top margin.</summary>
    public const string FieldFloatingLabelTopMargin = "Loam.Field.FloatingLabelTopMargin";
    /// <summary>Floating label horizontal padding.</summary>
    public const string FieldFloatingLabelHorizontalPadding = "Loam.Field.FloatingLabelHorizontalPadding";
    /// <summary>Field icon spacing.</summary>
    public const string FieldIconSpacing = "Loam.Field.IconSpacing";
    /// <summary>Helper/error text top spacing.</summary>
    public const string FieldHelperTopSpacing = "Loam.Field.HelperTopSpacing";

    // State layers
    /// <summary>Hover state layer opacity.</summary>
    public const string StateHoverOpacity = "Loam.State.HoverOpacity";
    /// <summary>Focus state layer opacity.</summary>
    public const string StateFocusOpacity = "Loam.State.FocusOpacity";
    /// <summary>Pressed state layer opacity.</summary>
    public const string StatePressedOpacity = "Loam.State.PressedOpacity";
    /// <summary>Selected state layer opacity.</summary>
    public const string StateSelectedOpacity = "Loam.State.SelectedOpacity";
    /// <summary>Dragged state layer opacity.</summary>
    public const string StateDraggedOpacity = "Loam.State.DraggedOpacity";
    /// <summary>Disabled state opacity.</summary>
    public const string StateDisabledOpacity = "Loam.State.DisabledOpacity";

    // Motion
    /// <summary>Short motion duration.</summary>
    public const string MotionDurationShort = "Loam.Motion.DurationShort";
    /// <summary>Medium motion duration.</summary>
    public const string MotionDurationMedium = "Loam.Motion.DurationMedium";
    /// <summary>Long motion duration.</summary>
    public const string MotionDurationLong = "Loam.Motion.DurationLong";
    /// <summary>Standard easing string.</summary>
    public const string MotionEasingStandard = "Loam.Motion.EasingStandard";
    /// <summary>Emphasized easing string.</summary>
    public const string MotionEasingEmphasized = "Loam.Motion.EasingEmphasized";
    /// <summary>Resource key for a named motion duration.</summary>
    public static string MotionDuration(string name) => $"Loam.Motion.Duration.{name}";
    /// <summary>Resource key for a named motion easing.</summary>
    public static string MotionEasing(string name) => $"Loam.Motion.Easing.{name}";
    /// <summary>Pressed/ripple motion duration.</summary>
    public const string MotionDurationShort3 = "Loam.Motion.Duration.Short3";
    /// <summary>Reveal motion duration.</summary>
    public const string MotionDurationMedium1 = "Loam.Motion.Duration.Medium1";
    /// <summary>Emphasized decelerate easing string.</summary>
    public const string MotionEasingEmphasizedDecelerate = "Loam.Motion.Easing.EmphasizedDecelerate";
    /// <summary>Linear easing string.</summary>
    public const string MotionEasingLinear = "Loam.Motion.Easing.Linear";

    // Z-index
    /// <summary>Resource key for an overlay z-index (int), e.g. <c>ZIndex(nameof(LoamZIndex.Dialog))</c>.</summary>
    public static string ZIndex(string name) => $"Loam.ZIndex.{name}";
}
