using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Loam.Controls;

namespace Loam.Theming;

/// <summary>
/// The Loam theming backbone — the Avalonia analogue of the reference API's <c>ThemeProvider</c>
/// (ADR-0005). Add a single instance to <see cref="Application.Styles"/>:
/// <code>Styles.Add(new LoamTheme());</code>
/// It projects a <see cref="LoamThemeData"/> into Avalonia resources: the palette into Light/Dark
/// <see cref="IResourceDictionary.ThemeDictionaries"/>, and typography/shadows/layout/z-index as
/// shared tokens. Controls bind tokens via dynamic resources, so switching
/// <see cref="Application.RequestedThemeVariant"/> or calling <see cref="SetData"/>/<see cref="SetPalette"/>
/// re-styles the whole app at runtime.
/// </summary>
public sealed class LoamTheme : Styles
{
    private static readonly PropertyInfo[] PaletteColorProps =
        typeof(LoamPalette).GetProperties().Where(p => p.PropertyType == typeof(Color)).ToArray();

    private static readonly PropertyInfo[] PaletteDoubleProps =
        typeof(LoamPalette).GetProperties().Where(p => p.PropertyType == typeof(double)).ToArray();

    private static readonly PropertyInfo[] ColorSchemeColorProps =
        typeof(LoamColorScheme).GetProperties().Where(p => p.PropertyType == typeof(Color)).ToArray();

    private static readonly PropertyInfo[] SpacingProps =
        typeof(LoamSpacing).GetProperties().Where(p => p.PropertyType == typeof(double)).ToArray();

    private static readonly PropertyInfo[] StrokeProps =
        typeof(LoamStroke).GetProperties().Where(p => p.PropertyType == typeof(double)).ToArray();

    private static readonly PropertyInfo[] DensityDoubleProps =
        typeof(LoamDensity).GetProperties().Where(p => p.PropertyType == typeof(double)).ToArray();

    private static readonly PropertyInfo[] DensityThicknessProps =
        typeof(LoamDensity).GetProperties().Where(p => p.PropertyType == typeof(Thickness)).ToArray();

    private static readonly PropertyInfo[] ElevationProps =
        typeof(LoamElevation).GetProperties().Where(p => p.PropertyType == typeof(int)).ToArray();

    private static readonly PropertyInfo[] ZIndexProps =
        typeof(LoamZIndex).GetProperties().Where(p => p.PropertyType == typeof(int)).ToArray();

    private static readonly string[] SemanticColorNames =
        ["Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"];

    private LoamThemeData _data;

    /// <summary>Creates the theme from the Loam defaults.</summary>
    /// <param name="serviceProvider">Unused; present so the type is XAML-/DI-construction friendly.</param>
    public LoamTheme(IServiceProvider? serviceProvider = null)
        : this(LoamThemeData.Default)
    {
    }

    /// <summary>Creates the theme from an explicit design specification.</summary>
    public LoamTheme(LoamThemeData data)
    {
        _data = data;
        BuildTokens();
        RegisterControlThemes();
    }

    /// <summary>The current design specification.</summary>
    public LoamThemeData Data => _data;

    /// <summary>Replaces the entire design specification and re-projects all tokens at runtime.</summary>
    public void SetData(LoamThemeData data)
    {
        _data = data;
        BuildTokens();
    }

    /// <summary>Replaces both palettes at runtime, keeping typography/shadows/layout/z-index.</summary>
    public void SetPalette(LoamPalette light, LoamPalette dark) =>
        SetData(_data with
        {
            PaletteLight = light,
            PaletteDark = dark,
            ColorSchemeLight = LoamColorScheme.FromPalette(light),
            ColorSchemeDark = LoamColorScheme.FromPalette(dark),
        });

    /// <summary>
    /// Changes the primary accent at runtime for both variants, recomputing a readable contrast
    /// text color. Demonstrates live palette editing.
    /// </summary>
    public void SetPrimary(Color color) =>
        SetData(_data with
        {
            ColorSchemeLight = _data.ColorSchemeLight with
            {
                Primary = color,
                OnPrimary = color.ContrastText(),
                PrimaryContainer = color.Lighten(0.78),
                OnPrimaryContainer = color.Darken(0.72).ContrastText(),
            },
            ColorSchemeDark = _data.ColorSchemeDark with
            {
                Primary = color.Lighten(0.35),
                OnPrimary = color.Darken(0.65).ContrastText(),
                PrimaryContainer = color.Darken(0.25),
                OnPrimaryContainer = color.Lighten(0.78).ContrastText(),
            },
            PaletteLight = _data.PaletteLight with { Primary = color, PrimaryContrastText = color.ContrastText() },
            PaletteDark = _data.PaletteDark with { Primary = color.Lighten(0.35), PrimaryContrastText = color.Darken(0.65).ContrastText() },
        });

    private void BuildTokens()
    {
        // Color roles → per-variant dictionaries. Assigning fresh dictionaries guarantees a resource
        // change notification so bound controls re-resolve immediately on a runtime swap.
        Resources.ThemeDictionaries[ThemeVariant.Light] = BuildVariantDictionary(
            ResolveColorScheme(light: true),
            _data.StateLayer);
        Resources.ThemeDictionaries[ThemeVariant.Dark] = BuildVariantDictionary(
            ResolveColorScheme(light: false),
            _data.StateLayer);

        ProjectSharedTokens();
    }

    private LoamColorScheme ResolveColorScheme(bool light)
    {
        var scheme = light ? _data.ColorSchemeLight : _data.ColorSchemeDark;
        var defaultScheme = light ? LoamColorScheme.DefaultLight : LoamColorScheme.DefaultDark;
        var palette = light ? _data.PaletteLight : _data.PaletteDark;
        var defaultPalette = light ? LoamPalette.DefaultLight : LoamPalette.DefaultDark;

        return scheme == defaultScheme && palette != defaultPalette
            ? LoamColorScheme.FromPalette(palette)
            : scheme;
    }

    private static ResourceDictionary BuildVariantDictionary(LoamColorScheme scheme, LoamStateLayer stateLayer)
    {
        var dict = new ResourceDictionary();

        foreach (var prop in ColorSchemeColorProps)
        {
            var color = (Color)prop.GetValue(scheme)!;
            dict[$"Loam.Color.{prop.Name}"] = new ImmutableSolidColorBrush(color);
        }

        dict[LoamTokens.TonalElevation(0)] = new ImmutableSolidColorBrush(scheme.Surface);
        dict[LoamTokens.TonalElevation(1)] = new ImmutableSolidColorBrush(scheme.SurfaceContainerLow);
        dict[LoamTokens.TonalElevation(2)] = new ImmutableSolidColorBrush(scheme.SurfaceContainer);
        dict[LoamTokens.TonalElevation(3)] = new ImmutableSolidColorBrush(scheme.SurfaceContainerHigh);
        dict[LoamTokens.TonalElevation(4)] = new ImmutableSolidColorBrush(scheme.SurfaceContainerHighest);
        dict[LoamTokens.TonalElevation(5)] = new ImmutableSolidColorBrush(scheme.SurfaceContainerHighest);

        var palette = scheme.ToPalette();
        foreach (var prop in PaletteColorProps)
        {
            var color = (Color)prop.GetValue(palette)!;
            dict[$"Loam.Palette.{prop.Name}"] = new ImmutableSolidColorBrush(color);
        }

        foreach (var prop in PaletteDoubleProps)
        {
            dict[$"Loam.Palette.{prop.Name}"] = (double)prop.GetValue(palette)!;
        }

        foreach (var name in SemanticColorNames)
        {
            var baseColor = (Color)typeof(LoamPalette).GetProperty(name)!.GetValue(palette)!;
            dict[$"Loam.Palette.{name}.Hover"] = new ImmutableSolidColorBrush(baseColor.WithAlpha(stateLayer.HoverOpacity));
            dict[$"Loam.Palette.{name}.Focus"] = new ImmutableSolidColorBrush(baseColor.WithAlpha(stateLayer.FocusOpacity));
            dict[$"Loam.Palette.{name}.Pressed"] = new ImmutableSolidColorBrush(baseColor.WithAlpha(stateLayer.PressedOpacity));
            dict[$"Loam.Palette.{name}.Selected"] = new ImmutableSolidColorBrush(baseColor.WithAlpha(stateLayer.SelectedOpacity));
            dict[$"Loam.Palette.{name}.Darken"] = new ImmutableSolidColorBrush(baseColor.Darken(0.075));
        }

        return dict;
    }

    private void ProjectSharedTokens()
    {
        var typography = _data.Typography;
        Resources[LoamTokens.FontFamily] = typography.FontFamily;
        foreach (var (name, style) in typography.Scales)
        {
            Resources[LoamTokens.TypographyFontSize(name)] = style.FontSize;
            Resources[LoamTokens.TypographyFontWeight(name)] = style.FontWeight;
            Resources[LoamTokens.TypographyLineHeight(name)] = style.LineHeightPx;
        }

        for (var level = 0; level <= _data.Shadows.MaxElevation; level++)
        {
            Resources[LoamTokens.Elevation(level)] = _data.Shadows[level];
        }

        foreach (var prop in ElevationProps)
        {
            Resources[LoamTokens.ElevationShadow(prop.Name)] = (int)prop.GetValue(_data.Elevation)!;
        }

        Resources[LoamTokens.DefaultCornerRadius] = _data.Layout.DefaultBorderRadius;
        Resources[LoamTokens.DrawerWidth] = _data.Layout.DrawerWidth;
        Resources[LoamTokens.DrawerMiniWidth] = _data.Layout.DrawerMiniWidth;
        Resources[LoamTokens.AppBarHeight] = _data.Layout.AppBarHeight;

        var shape = _data.Shape;
        Resources[LoamTokens.ShapeNone] = shape.None;
        Resources[LoamTokens.ShapeExtraSmall] = shape.ExtraSmall;
        Resources[LoamTokens.ShapeSmall] = shape.Small;
        Resources[LoamTokens.ShapeMedium] = shape.Medium;
        Resources[LoamTokens.ShapeLarge] = shape.Large;
        Resources[LoamTokens.ShapeLargeIncreased] = shape.LargeIncreased;
        Resources[LoamTokens.ShapeExtraLarge] = shape.ExtraLarge;
        Resources[LoamTokens.ShapeExtraLargeIncreased] = shape.ExtraLargeIncreased;
        Resources[LoamTokens.ShapeExtraExtraLarge] = shape.ExtraExtraLarge;
        Resources[LoamTokens.ShapeFull] = shape.Full;

        foreach (var prop in SpacingProps)
        {
            Resources[LoamTokens.Spacing(prop.Name)] = (double)prop.GetValue(_data.Spacing)!;
        }

        foreach (var prop in StrokeProps)
        {
            Resources[LoamTokens.Stroke(prop.Name)] = (double)prop.GetValue(_data.Stroke)!;
        }

        foreach (var prop in DensityDoubleProps)
        {
            Resources[LoamTokens.Density(prop.Name)] = (double)prop.GetValue(_data.Density)!;
        }

        foreach (var prop in DensityThicknessProps)
        {
            Resources[LoamTokens.Density(prop.Name)] = (Thickness)prop.GetValue(_data.Density)!;
        }

        var field = _data.FieldMetrics;
        Resources[LoamTokens.FieldOutlinedHeight] = field.OutlinedHeight;
        Resources[LoamTokens.FieldFilledHeight] = field.FilledHeight;
        Resources[LoamTokens.FieldTextHeight] = field.TextHeight;
        Resources[LoamTokens.FieldOutlineWidth] = field.OutlineWidth;
        Resources[LoamTokens.FieldActiveOutlineWidth] = field.ActiveOutlineWidth;
        Resources[LoamTokens.FieldOutlinedPadding] = field.OutlinedPadding;
        Resources[LoamTokens.FieldFilledPadding] = field.FilledPadding;
        Resources[LoamTokens.FieldTextPadding] = field.TextPadding;
        Resources[LoamTokens.FieldLabelX] = field.LabelX;
        Resources[LoamTokens.FieldFloatingLabelTopMargin] = field.FloatingLabelTopMargin;
        Resources[LoamTokens.FieldFloatingLabelHorizontalPadding] = field.FloatingLabelHorizontalPadding;
        Resources[LoamTokens.FieldIconSpacing] = field.IconSpacing;
        Resources[LoamTokens.FieldHelperTopSpacing] = field.HelperTopSpacing;

        var state = _data.StateLayer;
        Resources[LoamTokens.StateHoverOpacity] = state.HoverOpacity;
        Resources[LoamTokens.StateFocusOpacity] = state.FocusOpacity;
        Resources[LoamTokens.StatePressedOpacity] = state.PressedOpacity;
        Resources[LoamTokens.StateSelectedOpacity] = state.SelectedOpacity;
        Resources[LoamTokens.StateDraggedOpacity] = state.DraggedOpacity;
        Resources[LoamTokens.StateDisabledOpacity] = state.DisabledOpacity;

        var motion = _data.Motion;
        Resources[LoamTokens.MotionDurationShort] = motion.DurationShort;
        Resources[LoamTokens.MotionDurationMedium] = motion.DurationMedium;
        Resources[LoamTokens.MotionDurationLong] = motion.DurationLong;
        Resources[LoamTokens.MotionEasingStandard] = motion.EasingStandard;
        Resources[LoamTokens.MotionEasingEmphasized] = motion.EasingEmphasized;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Short1))] = motion.Short1;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Short2))] = motion.Short2;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Short3))] = motion.Short3;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Short4))] = motion.Short4;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Medium1))] = motion.Medium1;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Medium2))] = motion.Medium2;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Medium3))] = motion.Medium3;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Medium4))] = motion.Medium4;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Long1))] = motion.Long1;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Long2))] = motion.Long2;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Long3))] = motion.Long3;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.Long4))] = motion.Long4;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.ExtraLong1))] = motion.ExtraLong1;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.ExtraLong2))] = motion.ExtraLong2;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.ExtraLong3))] = motion.ExtraLong3;
        Resources[LoamTokens.MotionDuration(nameof(LoamMotion.ExtraLong4))] = motion.ExtraLong4;
        Resources[LoamTokens.MotionEasing(nameof(LoamMotion.EasingStandard))] = motion.EasingStandard;
        Resources[LoamTokens.MotionEasing(nameof(LoamMotion.EasingStandardDecelerate))] = motion.EasingStandardDecelerate;
        Resources[LoamTokens.MotionEasing(nameof(LoamMotion.EasingStandardAccelerate))] = motion.EasingStandardAccelerate;
        Resources[LoamTokens.MotionEasing(nameof(LoamMotion.EasingEmphasized))] = motion.EasingEmphasized;
        Resources[LoamTokens.MotionEasing(nameof(LoamMotion.EasingEmphasizedDecelerate))] = motion.EasingEmphasizedDecelerate;
        Resources[LoamTokens.MotionEasing(nameof(LoamMotion.EasingEmphasizedAccelerate))] = motion.EasingEmphasizedAccelerate;
        Resources[LoamTokens.MotionEasing(nameof(LoamMotion.EasingLinear))] = motion.EasingLinear;

        foreach (var prop in ZIndexProps)
        {
            Resources[LoamTokens.ZIndex(prop.Name)] = (int)prop.GetValue(_data.ZIndex)!;
        }
    }

    private void RegisterControlThemes()
    {
        Resources[typeof(Paper)] = PaperTheme.Create();
        Resources[typeof(CardHeader)] = CardHeaderTheme.Create();
        Resources[typeof(CardMedia)] = CardMediaTheme.Create();
        Resources[typeof(Loam.Controls.Button)] = ButtonTheme.Create();
        Resources[typeof(IconButton)] = IconButtonTheme.Create();
        Resources[typeof(ButtonGroup)] = ButtonGroupTheme.Create();
        Resources[typeof(Fab)] = FabTheme.Create();
        Resources[typeof(Avatar)] = AvatarTheme.Create();
        Resources[typeof(AvatarGroup)] = AvatarGroupTheme.Create();
        Resources[typeof(Chip)] = ChipTheme.Create();
        Resources[typeof(ChipSet)] = ChipSetTheme.Create();
        Resources[typeof(Badge)] = BadgeTheme.Create();
        Resources[typeof(AppBar)] = AppBarTheme.Create();
        Resources[typeof(Drawer)] = DrawerTheme.Create();
        Resources[typeof(MainContent)] = MainContentTheme.Create();
        Resources[typeof(Layout)] = LayoutTheme.Create();
        Resources[typeof(Loam.Controls.CheckBox)] = CheckBoxTheme.Create();
        Resources[typeof(Switch)] = SwitchTheme.Create();
        Resources[typeof(Field)] = FieldTheme.Create();
        Resources[typeof(TextField)] = TextFieldTheme.Create();
        Resources[typeof(Select)] = SelectTheme.Create();
        Resources[typeof(NumericField)] = NumericFieldTheme.Create();
        Resources[typeof(Autocomplete)] = AutocompleteTheme.Create();
        Resources[typeof(Loam.Controls.DatePicker)] = DatePickerTheme.Create();
        Resources[typeof(Loam.Controls.TimePicker)] = TimePickerTheme.Create();
        Resources[typeof(Loam.Controls.ColorPicker)] = ColorPickerTheme.Create();
        Resources[typeof(DateRangePicker)] = DateRangePickerTheme.Create();
        Resources[typeof(Radio)] = RadioTheme.Create();
        Resources[typeof(Loam.Controls.Slider)] = SliderTheme.Create();
        Resources[typeof(Rating)] = RatingTheme.Create();
        Resources[typeof(ToggleGroup)] = ToggleGroupTheme.Create();
        Resources[typeof(FileUpload)] = FileUploadTheme.Create();
        Resources[typeof(Alert)] = AlertTheme.Create();
        Resources[typeof(ProgressLinear)] = ProgressLinearTheme.Create();
        Resources[typeof(ListItem)] = ListItemTheme.Create();
        Resources[typeof(Overlay)] = OverlayTheme.Create();
        Resources[typeof(Tabs)] = TabsTheme.Create();
        Resources[typeof(SimpleTable)] = SimpleTableTheme.Create();
        Resources[typeof(Stepper)] = StepperTheme.Create();
        Resources[typeof(Pagination)] = PaginationTheme.Create();
        Resources[typeof(Loam.Controls.TreeView)] = TreeViewTheme.Create();
        Resources[typeof(Loam.Controls.TreeViewItem)] = TreeViewItemTheme.Create();
        Resources[typeof(Loam.Controls.Carousel)] = CarouselTheme.Create();
        Resources[typeof(ExpansionPanel)] = ExpansionPanelTheme.Create();
        Resources[typeof(ExpansionPanels)] = ExpansionPanelsTheme.Create();
        Resources[typeof(Breadcrumbs)] = BreadcrumbsTheme.Create();
        Resources[typeof(NavLink)] = NavLinkTheme.Create();
        Resources[typeof(NavGroup)] = NavGroupTheme.Create();
    }
}
