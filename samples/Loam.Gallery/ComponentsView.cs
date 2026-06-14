using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Loam;
using Loam.Controls;
using Loam.Theming;
using ThemeVariant = Avalonia.Styling.ThemeVariant;
using LoamButton = Loam.Controls.Button;

namespace Loam.Gallery;

/// <summary>Documentation-style component lab for the Loam catalog.</summary>
public sealed class ComponentsView : UserControl
{
    private static readonly LoamColor[] Colors =
    [
        LoamColor.Default, LoamColor.Primary, LoamColor.Secondary, LoamColor.Tertiary,
        LoamColor.Info, LoamColor.Success, LoamColor.Warning, LoamColor.Error, LoamColor.Dark,
    ];

    private static readonly LoamSize[] Sizes =
    [
        LoamSize.ExtraSmall,
        LoamSize.Small,
        LoamSize.Medium,
        LoamSize.Large,
        LoamSize.ExtraLarge,
    ];

    private readonly IReadOnlyList<GalleryPage> _pages;
    private readonly Dictionary<string, NavLink> _links = [];
    private readonly ContentControl _pageHost = new() { HorizontalAlignment = HorizontalAlignment.Stretch };

    public ComponentsView()
    {
        _pages = PageCatalog;

        var nav = BuildSideMenu();
        var navSurface = new Border
        {
            Width = 268,
            Padding = new Thickness(8, 14),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = new ScrollViewer { Content = nav },
        };
        navSurface.Bind(Border.BackgroundProperty, navSurface.GetResourceObservable(LoamTokens.Surface));
        navSurface.Bind(Border.BorderBrushProperty, navSurface.GetResourceObservable(LoamTokens.Divider));

        var scroller = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _pageHost,
        };
        scroller.SizeChanged += (_, args) => _pageHost.Width = Math.Max(0, args.NewSize.Width);

        var scrollToTop = new ScrollToTop
        {
            Target = scroller,
            VisibleOffset = 200,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 24, 24),
        };

        var content = new Panel { Children = { scroller, scrollToTop } };
        var body = new Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            Children = { navSurface, content },
        };
        Avalonia.Controls.Grid.SetColumn(content, 1);

        var shell = new DockPanel();
        var appBar = BuildTopBar();
        DockPanel.SetDock(appBar, Dock.Top);
        shell.Children.Add(appBar);
        shell.Children.Add(body);

        var background = new Border { Child = shell };
        background.Bind(Border.BackgroundProperty, background.GetResourceObservable(LoamTokens.Background));
        Content = background;

        ShowPage(_pages[0]);
    }

    private static Border BuildTopBar()
    {
        static Border StatusPill(string name, string text)
        {
            var label = new Text
            {
                Text = text,
                Typo = Typo.LabelMedium,
                Color = LoamColor.Primary,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var pill = new Border
            {
                Name = name,
                MinHeight = 32,
                Padding = new Thickness(12, 0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                ClipToBounds = false,
                VerticalAlignment = VerticalAlignment.Center,
                Child = label,
            };
            pill.Bind(Border.BorderBrushProperty, pill.GetResourceObservable(LoamTokens.ColorPrimary));
            pill.Bind(Border.CornerRadiusProperty, pill.GetResourceObservable(LoamTokens.ShapeFull));
            return pill;
        }

        var brand = new StackPanel
        {
            Name = "PART_HeaderBrand",
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Icon { Data = Icons.Material.Filled.Widgets, Color = LoamColor.Primary, Size = LoamSize.Medium },
                new Text { Text = "Loam Gallery", Typo = Typo.H6, VerticalAlignment = VerticalAlignment.Center },
                StatusPill("PART_HeaderStatusComponentLab", "component lab"),
                StatusPill("PART_HeaderStatusLiveControls", "live controls"),
            },
        };

        var theme = new ToggleIconButton
        {
            Icon = Icons.Material.Filled.DarkMode,
            ToggledIcon = Icons.Material.Filled.LightMode,
            Toggled = Application.Current?.ActualThemeVariant == ThemeVariant.Dark,
            Variant = Variant.Outlined,
            Color = LoamColor.Primary,
            Size = LoamSize.Small,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AutomationProperties.SetName(theme, "Toggle theme");
        AutomationProperties.SetHelpText(theme, "Switch between light and dark gallery themes.");
        Tooltip.Set(theme, "Toggle theme");
        theme.Click += (_, _) => ToggleTheme();

        var seed = BuildSeedPicker();

        var actions = new StackPanel
        {
            Name = "PART_HeaderActions",
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { seed, theme },
        };

        var layout = new Avalonia.Controls.Grid
        {
            Margin = new Thickness(24, 0),
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            VerticalAlignment = VerticalAlignment.Center,
            Children = { brand, actions },
        };
        Avalonia.Controls.Grid.SetColumn(actions, 2);

        var bar = new Border
        {
            Height = 72,
            Child = layout,
            BorderThickness = new Thickness(0, 0, 0, 1),
        };
        bar.Bind(Border.BackgroundProperty, bar.GetResourceObservable(LoamTokens.Surface));
        bar.Bind(Border.BorderBrushProperty, bar.GetResourceObservable(LoamTokens.Divider));
        return bar;
    }

    private static void ToggleTheme()
    {
        if (Application.Current is not { } app)
        {
            return;
        }

        app.RequestedThemeVariant =
            app.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
    }

    // Material You (Phase 2) playground: pick a seed and the whole gallery re-themes at runtime via
    // LoamTheme.SetSeed (one seed -> complete light + dark scheme). The Fluent bridge follows too.
    private static readonly string[] SeedPresets =
    [
        "#6750A4", "#006A6A", "#386A20", "#B3261E", "#765A00",
        "#1565C0", "#7D5260", "#5B5BD6", "#3F6212", "#9A3412",
    ];

    private static IconButton BuildSeedPicker()
    {
        var button = new IconButton
        {
            Icon = Icons.Material.Filled.Palette,
            Variant = Variant.Outlined,
            Color = LoamColor.Primary,
            Size = LoamSize.Small,
            VerticalAlignment = VerticalAlignment.Center,
            Flyout = new Flyout
            {
                Placement = PlacementMode.BottomEdgeAlignedRight,
                Content = BuildSeedFlyout(),
            },
        };
        AutomationProperties.SetName(button, "Theme seed");
        AutomationProperties.SetHelpText(button, "Generate the whole theme from a seed color (Material You).");
        Tooltip.Set(button, "Material You seed");
        return button;
    }

    private static StackPanel BuildSeedFlyout()
    {
        var seed = Color.Parse(SeedPresets[0]);
        var contrast = LoamContrast.Standard;
        void ApplySeed() => CurrentLoamTheme()?.SetSeed(seed, contrast);

        var caption = new Text
        {
            Text = "Theme playground",
            Typo = Typo.Subtitle2,
            Margin = new Thickness(4, 0, 4, 8),
        };

        var swatches = new WrapPanel { MaxWidth = 220 };
        foreach (var hex in SeedPresets)
        {
            var color = Color.Parse(hex);
            swatches.Children.Add(SeedSwatch(color, () =>
            {
                seed = color;
                ApplySeed();
            }));
        }

        var highContrast = new Switch
        {
            Content = "High contrast",
            Color = LoamColor.Primary,
            Margin = new Thickness(4, 12, 4, 0),
        };
        highContrast.IsCheckedChanged += (_, _) =>
        {
            contrast = highContrast.IsChecked == true ? LoamContrast.High : LoamContrast.Standard;
            ApplySeed();
        };

        var compact = new Switch
        {
            Content = "Compact density",
            Color = LoamColor.Primary,
            Margin = new Thickness(4, 4, 4, 0),
        };
        compact.IsCheckedChanged += (_, _) =>
            CurrentLoamTheme()?.SetDensity(compact.IsChecked == true ? LoamDensity.Compact : LoamDensity.Default);

        var reset = new LoamButton
        {
            Content = "Reset",
            Variant = Variant.Text,
            Color = LoamColor.Primary,
            Size = LoamSize.Small,
            Margin = new Thickness(0, 8, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        reset.Click += (_, _) =>
        {
            seed = Color.Parse(SeedPresets[0]);
            contrast = LoamContrast.Standard;
            highContrast.IsChecked = false;
            compact.IsChecked = false;
            CurrentLoamTheme()?.SetData(LoamThemeData.Default);
        };

        return new StackPanel { Margin = new Thickness(8), Children = { caption, swatches, highContrast, compact, reset } };
    }

    private static Border SeedSwatch(Color color, Action onPick)
    {
        var swatch = new Border
        {
            Width = 32,
            Height = 32,
            Margin = new Thickness(4),
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(color),
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        AutomationProperties.SetName(swatch, $"Seed {color}");
        swatch.PointerPressed += (_, _) => onPick();
        return swatch;
    }

    private static LoamTheme? CurrentLoamTheme() =>
        Application.Current?.Styles.OfType<LoamTheme>().FirstOrDefault();

    internal enum GallerySampleKind
    {
        SingleComponent,
        Family,
    }

    internal enum GalleryAcceptanceCriterion
    {
        Anatomy,
        ColorRoles,
        Typography,
        Shape,
        Stroke,
        Elevation,
        StateLayer,
        FocusVisible,
        RippleOrPress,
        Disabled,
        SelectedOrActive,
        Error,
        OpenOrDismiss,
        Loading,
        Empty,
        Keyboard,
        Automation,
        Responsive,
        Density,
        SizeVariants,
        Motion,
        LightDark,
        SourceReference,
        SourceLinkedCode,
    }

    internal sealed record GalleryPage(
        string Group,
        string Title,
        string Description,
        Func<Control> Build,
        string Code,
        string BuilderMethod,
        GallerySampleKind SampleKind,
        string SourceReference,
        IReadOnlyList<string> ExpectedComponentNames,
        IReadOnlyList<GalleryAcceptanceCriterion> AcceptanceCriteria)
    {
        internal string Route => $"{Group}/{Title}";

        /// <summary>
        /// Optional per-sample breakdown. When non-empty, the page renders each sample's preview
        /// followed by its own code snippet (instead of one preview + one combined code block).
        /// </summary>
        internal IReadOnlyList<GallerySample> Samples { get; init; } = [];
    }

    /// <summary>One labelled example on a gallery page: a live preview plus its own C# snippet.</summary>
    internal sealed record GallerySample(string Caption, Func<Control> Build, string Code);

    internal static IReadOnlyList<GalleryPage> PageCatalog { get; } = BuildPageCatalog();

    private static GalleryPage Page(
        string group,
        string title,
        string description,
        Func<Control> build,
        params string[] expectedComponentNames) =>
        CreatePage(group, title, description, build, GallerySampleKind.SingleComponent, expectedComponentNames);

    private static GalleryPage Family(
        string group,
        string title,
        string description,
        Func<Control> build,
        params string[] expectedComponentNames) =>
        CreatePage(group, title, description, build, GallerySampleKind.Family, expectedComponentNames);

    private static GalleryPage CreatePage(
        string group,
        string title,
        string description,
        Func<Control> build,
        GallerySampleKind sampleKind,
        string[] expectedComponentNames)
    {
        var builderMethod = build.Method.Name;
        var expected = expectedComponentNames.Length > 0 ? expectedComponentNames : [title];

        return new(
            group,
            title,
            description,
            build,
            GallerySourceCode.ForMethod(builderMethod),
            builderMethod,
            sampleKind,
            SourceReferenceFor(group, title),
            expected,
            AcceptanceFor(group, title));
    }

    private static GallerySample Sample(string caption, Func<Control> build) =>
        new(caption, build, GallerySourceCode.ForMethod(build.Method.Name));

    private static GalleryPage PageWithSamples(
        string group,
        string title,
        string description,
        params GallerySample[] samples)
    {
        var builderMethod = samples[0].Build.Method.Name;
        var methods = string.Join($"{Environment.NewLine}{Environment.NewLine}", samples.Select(sample => sample.Code));
        var captions = string.Join(", ", samples.Select(sample => sample.Caption));
        var code = $"{methods}{Environment.NewLine}{Environment.NewLine}// Samples: {captions}";

        Func<Control> buildAll = () =>
        {
            var stack = new StackPanel { Spacing = 24, HorizontalAlignment = HorizontalAlignment.Left };
            foreach (var sample in samples)
            {
                stack.Children.Add(sample.Build());
            }

            return stack;
        };

        return new GalleryPage(
            group,
            title,
            description,
            buildAll,
            code,
            builderMethod,
            GallerySampleKind.SingleComponent,
            SourceReferenceFor(group, title),
            [title],
            AcceptanceFor(group, title))
        {
            Samples = samples,
        };
    }

    private static string SourceReferenceFor(string group, string title) => (group, title) switch
    {
        ("Buttons", "Button" or "IconButton" or "ToggleIconButton" or "ButtonGroup" or "Fab") =>
            "source/API: button defaults, state layers, and component tokens",
        ("Inputs", _) or ("Pickers", _) =>
            "source/API: field anatomy, popup behavior, and component tokens",
        ("Feedback", _) or ("Surfaces", _) or ("Shell", _) =>
            "source/API: surface, overlay, motion, and elevation tokens",
        ("Data", _) or ("Navigation", _) =>
            "source/API: interaction, keyboard, and state tokens",
        ("Charts", _) =>
            "source/API: chart surface and theme role tokens",
        _ => "source/API: component anatomy and tokens",
    };

    private static GalleryAcceptanceCriterion[] AcceptanceFor(string group, string title)
    {
        var criteria = new SortedSet<GalleryAcceptanceCriterion>
        {
            GalleryAcceptanceCriterion.Anatomy,
            GalleryAcceptanceCriterion.ColorRoles,
            GalleryAcceptanceCriterion.Typography,
            GalleryAcceptanceCriterion.Shape,
            GalleryAcceptanceCriterion.Responsive,
            GalleryAcceptanceCriterion.LightDark,
            GalleryAcceptanceCriterion.SourceReference,
            GalleryAcceptanceCriterion.SourceLinkedCode,
        };

        void Add(params GalleryAcceptanceCriterion[] values)
        {
            foreach (var value in values)
            {
                criteria.Add(value);
            }
        }

        switch (group)
        {
            case "Start":
                Add(GalleryAcceptanceCriterion.Density, GalleryAcceptanceCriterion.SizeVariants);
                break;
            case "Display":
                Add(GalleryAcceptanceCriterion.Density);
                if (title is "Chip" or "ChipSet")
                {
                    Add(
                        GalleryAcceptanceCriterion.StateLayer,
                        GalleryAcceptanceCriterion.FocusVisible,
                        GalleryAcceptanceCriterion.RippleOrPress,
                        GalleryAcceptanceCriterion.Disabled,
                        GalleryAcceptanceCriterion.SelectedOrActive,
                        GalleryAcceptanceCriterion.Keyboard,
                        GalleryAcceptanceCriterion.Automation,
                        GalleryAcceptanceCriterion.SizeVariants);
                }

                if (title is "Badge")
                {
                    Add(GalleryAcceptanceCriterion.StateLayer, GalleryAcceptanceCriterion.Automation);
                }

                if (title is "Avatar" or "AvatarGroup" or "Icon")
                {
                    Add(GalleryAcceptanceCriterion.SizeVariants, GalleryAcceptanceCriterion.Automation);
                }

                break;
            case "Buttons":
                Add(
                    GalleryAcceptanceCriterion.StateLayer,
                    GalleryAcceptanceCriterion.FocusVisible,
                    GalleryAcceptanceCriterion.RippleOrPress,
                    GalleryAcceptanceCriterion.Disabled,
                    GalleryAcceptanceCriterion.Keyboard,
                    GalleryAcceptanceCriterion.Automation,
                    GalleryAcceptanceCriterion.Density,
                    GalleryAcceptanceCriterion.SizeVariants);
                if (title is "ToggleIconButton" or "ButtonGroup")
                {
                    Add(GalleryAcceptanceCriterion.SelectedOrActive);
                }

                if (title is "Menu")
                {
                    Add(GalleryAcceptanceCriterion.OpenOrDismiss, GalleryAcceptanceCriterion.Elevation, GalleryAcceptanceCriterion.Motion);
                }

                break;
            case "Inputs":
                Add(
                    GalleryAcceptanceCriterion.StateLayer,
                    GalleryAcceptanceCriterion.FocusVisible,
                    GalleryAcceptanceCriterion.Disabled,
                    GalleryAcceptanceCriterion.Keyboard,
                    GalleryAcceptanceCriterion.Automation,
                    GalleryAcceptanceCriterion.Density,
                    GalleryAcceptanceCriterion.Stroke);
                if (title is "Field" or "TextField" or "NumericField" or "MaskedTextField" or "Autocomplete" or "Select" or "Form")
                {
                    Add(GalleryAcceptanceCriterion.Error);
                }

                if (title is "Autocomplete" or "Select" or "FileUpload")
                {
                    Add(GalleryAcceptanceCriterion.OpenOrDismiss, GalleryAcceptanceCriterion.Elevation, GalleryAcceptanceCriterion.Motion);
                }

                if (title is "CheckBox" or "Switch" or "Radio" or "RadioGroup" or "Slider" or "Rating" or "ToggleGroup")
                {
                    Add(GalleryAcceptanceCriterion.SelectedOrActive, GalleryAcceptanceCriterion.SizeVariants);
                }

                break;
            case "Pickers":
                Add(
                    GalleryAcceptanceCriterion.StateLayer,
                    GalleryAcceptanceCriterion.FocusVisible,
                    GalleryAcceptanceCriterion.Disabled,
                    GalleryAcceptanceCriterion.Keyboard,
                    GalleryAcceptanceCriterion.Automation,
                    GalleryAcceptanceCriterion.Density,
                    GalleryAcceptanceCriterion.Stroke,
                    GalleryAcceptanceCriterion.OpenOrDismiss,
                    GalleryAcceptanceCriterion.Elevation,
                    GalleryAcceptanceCriterion.Motion);
                if (title is "DatePicker" or "DateRangePicker" or "TimePicker")
                {
                    Add(GalleryAcceptanceCriterion.Error);
                }

                if (title is "MonthCalendar")
                {
                    Add(GalleryAcceptanceCriterion.SelectedOrActive);
                }

                break;
            case "Feedback":
                Add(GalleryAcceptanceCriterion.Automation, GalleryAcceptanceCriterion.Motion);
                if (title is "ProgressCircular" or "ProgressLinear" or "Skeleton")
                {
                    Add(GalleryAcceptanceCriterion.Loading);
                }

                if (title is "Overlay" or "Popover" or "DialogService" or "SnackbarService")
                {
                    Add(
                        GalleryAcceptanceCriterion.OpenOrDismiss,
                        GalleryAcceptanceCriterion.FocusVisible,
                        GalleryAcceptanceCriterion.Keyboard,
                        GalleryAcceptanceCriterion.Elevation,
                        GalleryAcceptanceCriterion.StateLayer);
                }

                if (title is "Tooltip")
                {
                    Add(
                        GalleryAcceptanceCriterion.OpenOrDismiss,
                        GalleryAcceptanceCriterion.FocusVisible,
                        GalleryAcceptanceCriterion.Keyboard,
                        GalleryAcceptanceCriterion.Elevation);
                }

                break;
            case "Data":
                Add(
                    GalleryAcceptanceCriterion.StateLayer,
                    GalleryAcceptanceCriterion.FocusVisible,
                    GalleryAcceptanceCriterion.Keyboard,
                    GalleryAcceptanceCriterion.Automation,
                    GalleryAcceptanceCriterion.Density,
                    GalleryAcceptanceCriterion.SelectedOrActive);
                if (title is "SimpleTable" or "DataGrid" or "TreeView")
                {
                    Add(GalleryAcceptanceCriterion.Empty);
                }

                if (title is "Collapse" or "ExpansionPanels" or "Carousel")
                {
                    Add(GalleryAcceptanceCriterion.Motion);
                }

                break;
            case "Navigation":
                Add(
                    GalleryAcceptanceCriterion.StateLayer,
                    GalleryAcceptanceCriterion.FocusVisible,
                    GalleryAcceptanceCriterion.Keyboard,
                    GalleryAcceptanceCriterion.Automation,
                    GalleryAcceptanceCriterion.Density,
                    GalleryAcceptanceCriterion.SelectedOrActive);
                if (title is "NavGroup")
                {
                    Add(GalleryAcceptanceCriterion.OpenOrDismiss, GalleryAcceptanceCriterion.Motion);
                }

                break;
            case "Layout":
                Add(GalleryAcceptanceCriterion.Density);
                if (title is "ScrollToTop")
                {
                    Add(
                        GalleryAcceptanceCriterion.StateLayer,
                        GalleryAcceptanceCriterion.FocusVisible,
                        GalleryAcceptanceCriterion.RippleOrPress,
                        GalleryAcceptanceCriterion.Keyboard,
                        GalleryAcceptanceCriterion.Automation,
                        GalleryAcceptanceCriterion.Motion);
                }

                break;
            case "Shell":
                Add(
                    GalleryAcceptanceCriterion.StateLayer,
                    GalleryAcceptanceCriterion.FocusVisible,
                    GalleryAcceptanceCriterion.Keyboard,
                    GalleryAcceptanceCriterion.Automation,
                    GalleryAcceptanceCriterion.Density,
                    GalleryAcceptanceCriterion.Elevation,
                    GalleryAcceptanceCriterion.OpenOrDismiss,
                    GalleryAcceptanceCriterion.Motion);
                break;
            case "Surfaces":
                Add(GalleryAcceptanceCriterion.Elevation, GalleryAcceptanceCriterion.Density);
                if (title is "List")
                {
                    Add(
                        GalleryAcceptanceCriterion.StateLayer,
                        GalleryAcceptanceCriterion.FocusVisible,
                        GalleryAcceptanceCriterion.Keyboard,
                        GalleryAcceptanceCriterion.Automation,
                        GalleryAcceptanceCriterion.SelectedOrActive);
                }

                if (title is "Ripple")
                {
                    Add(GalleryAcceptanceCriterion.StateLayer, GalleryAcceptanceCriterion.RippleOrPress, GalleryAcceptanceCriterion.Motion);
                }

                break;
            case "Charts":
                Add(GalleryAcceptanceCriterion.Automation, GalleryAcceptanceCriterion.Empty);
                break;
        }

        return criteria.ToArray();
    }

    private static IReadOnlyList<GalleryPage> BuildPageCatalog() =>
    [
        Family("Start", "Overview", "A composed screen built from the same public controls used on the component pages.", BuildOverview, "Alert", "Button", "IconButton", "TextField", "Select", "ProgressLinear", "Timeline", "PieChart", "LineChart"),
        Family("Start", "Sizes", "Five-size rendering for every size-aware control.", BuildSizeMatrix, "Button", "IconButton", "ToggleIconButton", "ButtonGroup", "ToggleGroup", "Fab", "Icon", "Avatar", "AvatarGroup", "Chip", "CheckBox", "Switch", "Radio", "Rating", "ProgressCircular", "ProgressLinear", "Skeleton"),

        PageWithSamples("Display", "Text", "Typography, color, spacing, and alignment.",
            Sample("Display roles", BuildTextDisplayRoles),
            Sample("Content roles", BuildTextContentRoles),
            Sample("Legacy aliases", BuildTextLegacyAliases),
            Sample("Colors", BuildTextColors),
            Sample("Alignment and wrapping", BuildTextAlignment)),
        PageWithSamples("Display", "Icon", "Vector icon rendering with semantic colors and sizes.",
            Sample("Colors", BuildIconsColors),
            Sample("Sizes", BuildIconsSizes),
            Sample("Common glyphs", BuildIconsCommonGlyphs)),
        PageWithSamples("Display", "Divider", "Horizontal and vertical dividers with token colors.",
            Sample("Horizontal", BuildDividerHorizontal),
            Sample("Vertical", BuildDividerVertical)),
        PageWithSamples("Display", "Chip", "Compact labels, icons, close affordances, and variants.",
            Sample("Variants", BuildChipsVariants),
            Sample("Colors", BuildChipsColors),
            Sample("Sizes", BuildChipsSizes),
            Sample("Disabled", BuildChipsDisabled)),
        PageWithSamples("Display", "ChipSet", "Selectable single and multi-select chip groups.",
            Sample("Single mandatory", BuildChipSetSingleMandatory),
            Sample("Multi-select", BuildChipSetMultiSelect),
            Sample("Optional selection", BuildChipSetOptional),
            Sample("Disabled set", BuildChipSetDisabled)),
        PageWithSamples("Display", "Badge", "Numeric and dot badges positioned around child content.",
            Sample("Values", BuildBadgesValues),
            Sample("Origins", BuildBadgesOrigins),
            Sample("Surface behavior", BuildBadgesSurfaceBehavior)),
        PageWithSamples("Display", "Avatar", "Initials, icon avatars, sizes, colors, and shapes.",
            Sample("Variants", BuildAvatarVariants),
            Sample("Colors", BuildAvatarColors),
            Sample("Shapes", BuildAvatarShapes),
            Sample("Sizes", BuildAvatarSizes)),
        PageWithSamples("Display", "AvatarGroup", "Grouped avatars with overflow count behavior.",
            Sample("Overflow", BuildAvatarGroupOverflow),
            Sample("Compact", BuildAvatarGroupCompact),
            Sample("Relaxed spacing", BuildAvatarGroupRelaxed),
            Sample("Rounded", BuildAvatarGroupRounded),
            Sample("Square", BuildAvatarGroupSquare),
            Sample("Sizes", BuildAvatarGroupSizes)),

        PageWithSamples("Buttons", "Button", "Filled, outlined, text, color, size, disabled, and icon buttons.",
            Sample("Filled", BuildButtonsFilled),
            Sample("Outlined", BuildButtonsOutlined),
            Sample("Text", BuildButtonsText),
            Sample("Configurations", BuildButtonConfigurationRail),
            Sample("Icon sizes", BuildButtonsIconSizes),
            Sample("Disabled", BuildButtonsDisabled),
            Sample("With icons", BuildButtonsWithIcons)),
        PageWithSamples("Buttons", "IconButton", "Icon-only actions in default, filled, and outlined variants.",
            Sample("Variants", BuildIconButtonsVariants),
            Sample("Sizes", BuildIconButtonsSizes)),
        PageWithSamples("Buttons", "ToggleIconButton", "Two-state icon action with a separate toggled color.",
            Sample("Favorite", BuildToggleIconButtonFavorite),
            Sample("Sizes", BuildToggleIconButtonSizes)),
        PageWithSamples("Buttons", "ButtonGroup", "Connected button segments with shared variant and color.",
            Sample("Outlined", BuildButtonGroupOutlined),
            Sample("Filled", BuildButtonGroupFilled),
            Sample("Sizes", BuildButtonGroupSizes),
            Sample("Paired with a toggle", BuildButtonGroupToggle)),
        PageWithSamples("Buttons", "Fab", "Floating action buttons with icon-only and label modes.",
            Sample("Sizes", BuildFabSizes),
            Sample("Icon-only and extended", BuildFabShapes)),
        Family("Buttons", "Menu", "Button-anchored menu rows.", BuildMenu, "Menu", "MenuItem"),

        PageWithSamples("Inputs", "Field", "A reusable field shell for custom input-like content.",
            Sample("Custom editors", BuildFieldVariants),
            Sample("Underline & validation", BuildFieldUnderlineAndValidation),
            Sample("Disabled", BuildFieldDisabled)),
        PageWithSamples("Inputs", "TextField", "Text field variants, adornments, helper text, and error state.",
            Sample("Variants", BuildTextFieldVariants),
            Sample("Adornments & floating label", BuildTextFieldAdornments),
            Sample("States", BuildTextFieldStates)),
        PageWithSamples("Inputs", "NumericField", "Numeric parsing, formatting, bounds, and spinner controls.",
            Sample("Variants", BuildNumericFieldVariants),
            Sample("Steps & bounds", BuildNumericFieldStepsAndBounds),
            Sample("States", BuildNumericFieldStates)),
        PageWithSamples("Inputs", "MaskedTextField", "Pattern-based text formatting for phone-style entry.",
            Sample("Variants", BuildMaskedTextFieldVariants),
            Sample("Mask types", BuildMaskedTextFieldMaskTypes),
            Sample("States", BuildMaskedTextFieldStates)),
        PageWithSamples("Inputs", "Autocomplete", "Text entry with filtered suggestions.",
            Sample("Filtered suggestions", BuildAutocompleteFiltered),
            Sample("Prefilled value", BuildAutocompletePrefilled)),
        PageWithSamples("Inputs", "Select", "Single and multi-select dropdowns.",
            Sample("Single select", BuildSelectSingle),
            Sample("Multi-select", BuildSelectMulti),
            Sample("States", BuildSelectStates)),
        PageWithSamples("Inputs", "CheckBox", "Checkbox states, colors, and disabled rendering.",
            Sample("States", BuildCheckBoxStates),
            Sample("Sizes", BuildCheckBoxSizes),
            Sample("Disabled", BuildCheckBoxDisabled)),
        PageWithSamples("Inputs", "Switch", "On/off switch states and colors.",
            Sample("States", BuildSwitchStates),
            Sample("Sizes", BuildSwitchSizes),
            Sample("Disabled", BuildSwitchDisabled)),
        PageWithSamples("Inputs", "Radio", "Radio choices coordinated by a radio group.",
            Sample("States", BuildRadioStates),
            Sample("Sizes", BuildRadioSizes),
            Sample("Disabled", BuildRadioDisabled)),
        PageWithSamples("Inputs", "RadioGroup", "Grouped single-choice selection.",
            Sample("Vertical group", BuildRadioGroupVertical),
            Sample("Horizontal group", BuildRadioGroupHorizontal),
            Sample("Disabled group", BuildRadioGroupDisabled)),
        PageWithSamples("Inputs", "Slider", "Pointer-driven range selection.",
            Sample("Default range", BuildSliderDefaultRange),
            Sample("Custom min and max", BuildSliderCustomRange),
            Sample("Color states", BuildSliderColorStates),
            Sample("Zero value", BuildSliderZeroValue),
            Sample("Disabled", BuildSliderDisabled)),
        PageWithSamples("Inputs", "Rating", "Interactive and read-only star ratings.",
            Sample("States", BuildRatingStates),
            Sample("Sizes", BuildRatingSizes)),
        PageWithSamples("Inputs", "ToggleGroup", "Segmented single selection.",
            Sample("Selected", BuildToggleGroupSelected),
            Sample("Color", BuildToggleGroupColor),
            Sample("No selection", BuildToggleGroupNoSelection),
            Sample("Sizes", BuildToggleGroupSizes),
            Sample("Disabled", BuildToggleGroupDisabled)),
        PageWithSamples("Inputs", "FileUpload", "Platform file picking and selected-name chips.",
            Sample("Variants", BuildFileUploadVariants),
            Sample("Sizes", BuildFileUploadSizes)),
        PageWithSamples("Inputs", "Form", "Lightweight validation over text-field descendants.",
            Sample("States", BuildFormStates),
            Sample("Action sizes", BuildFormActionSizes)),

        PageWithSamples("Pickers", "DatePicker", "Date input with a calendar flyout.",
            Sample("Variants", BuildDatePickerVariants),
            Sample("Selected & custom format", BuildDatePickerSelected),
            Sample("Clearable", BuildDatePickerClearable),
            Sample("Constrained & floating label", BuildDatePickerConstrained),
            Sample("States", BuildDatePickerStates)),
        PageWithSamples("Pickers", "TimePicker", "Time input with hour and minute columns.",
            Sample("Variants", BuildTimePickerVariants),
            Sample("Selected & custom format", BuildTimePickerSelected),
            Sample("Clearable", BuildTimePickerClearable),
            Sample("Floating label", BuildTimePickerConstrained),
            Sample("States", BuildTimePickerStates)),
        PageWithSamples("Pickers", "DateRangePicker", "Two-click date range selection.",
            Sample("Variants", BuildDateRangePickerVariants),
            Sample("Selected & custom format", BuildDateRangePickerSelected),
            Sample("Clearable", BuildDateRangePickerClearable),
            Sample("Quick-select presets", BuildDateRangePickerPresets),
            Sample("Constrained & floating label", BuildDateRangePickerConstrained),
            Sample("States", BuildDateRangePickerStates)),
        PageWithSamples("Pickers", "ColorPicker", "Swatch picker with hex display.",
            Sample("Variants", BuildColorPickerVariants),
            Sample("Selected & custom format", BuildColorPickerValues),
            Sample("States", BuildColorPickerStates)),
        PageWithSamples("Pickers", "MonthCalendar", "Standalone month grid used by date pickers.",
            Sample("Selected", BuildMonthCalendarSelected),
            Sample("Range", BuildMonthCalendarRange),
            Sample("Constrained", BuildMonthCalendarConstrained)),

        PageWithSamples("Feedback", "Alert", "Contextual message banners across variants and severities.",
            Sample("Severities and variants", BuildAlertSeveritiesAndVariants),
            Sample("Disabled", BuildAlertDisabled),
            Sample("Content fallback", BuildAlertContentFallback)),
        PageWithSamples("Feedback", "ProgressCircular", "Determinate and indeterminate circular progress.",
            Sample("States", BuildProgressCircularStates),
            Sample("Sizes", BuildProgressCircularSizes),
            Sample("Disabled", BuildProgressCircularDisabled)),
        PageWithSamples("Feedback", "ProgressLinear", "Determinate and indeterminate linear progress.",
            Sample("States", BuildProgressLinearStates),
            Sample("Sizes", BuildProgressLinearSizes)),
        PageWithSamples("Feedback", "Skeleton", "Animated and static loading placeholders.",
            Sample("Presets", BuildSkeletonPresets),
            Sample("Composition", BuildSkeletonComposition),
            Sample("Sizes", BuildSkeletonSizes),
            Sample("States", BuildSkeletonStates)),
        Page("Feedback", "Overlay", "Auto-closing scrim over local content.", BuildOverlayScrim),
        PageWithSamples("Feedback", "Popover", "Anchored floating content.",
            Sample("Trigger", BuildPopoverTrigger),
            Sample("Open and close", BuildPopoverOpenAndClose),
            Sample("Disabled", BuildPopoverDisabled),
            Sample("Controlled", BuildPopoverControlled)),
        PageWithSamples("Feedback", "Tooltip", "Attached contextual help on focusable targets.",
            Sample("Standard", BuildTooltipStandard),
            Sample("Rich surface", BuildTooltipRichSurface),
            Sample("Placement and delay", BuildTooltipPlacementAndDelay),
            Sample("Disabled target", BuildTooltipDisabledTarget),
            Sample("Suppressed", BuildTooltipSuppressed),
            Sample("Cleared", BuildTooltipCleared)),
        Page("Feedback", "DialogService", "Confirm, action, and message dialogs.", BuildDialogService),
        Page("Feedback", "SnackbarService", "Toast messages with colors and actions.", BuildSnackbarService),
        Page("Feedback", "CommandPalette", "Searchable command list with keyboard navigation.", BuildCommandPalette),

        PageWithSamples("Data", "SimpleTable", "Small tabular datasets with hover and stripe options.",
            Sample("Dense", BuildTableDense),
            Sample("Empty", BuildTableEmpty)),
        PageWithSamples("Data", "DataGrid", "Typed sortable, pageable, filterable data grid.",
            Sample("Sortable · filtered · paged", BuildDataGridPaged),
            Sample("Live data — bound to an ObservableCollection", BuildDataGridLive),
            Sample("Async states — loading / error / ready", BuildDataGridAsyncStates),
            Sample("Footer totals", BuildDataGridFooter),
            Sample("Grouped with aggregate — click a header to collapse", BuildDataGridGrouped),
            Sample("Frozen first column — scroll the rest horizontally", BuildDataGridFrozen),
            Sample("Editable cells", BuildDataGridEditable),
            Sample("Virtualized — capped render", BuildDataGridVirtualized),
            Sample("Empty state", BuildDataGridEmpty)),
        Page("Data", "TreeView", "Nested rows with selection and expansion.", BuildTreeView),
        PageWithSamples("Data", "Tabs", "Header strip and selected content region.",
            Sample("Default tabs", BuildTabsDefault),
            Sample("Secondary selected", BuildTabsSecondarySelected),
            Sample("Clamped SelectedIndex", BuildTabsClampedSelectedIndex),
            Sample("Disabled", BuildTabsDisabled),
            Sample("Empty", BuildTabsEmpty)),
        PageWithSamples("Data", "ExpansionPanels", "Accordion-style expandable content.",
            Sample("Accordion", BuildExpansionPanelsAccordion),
            Sample("MultiExpansion", BuildExpansionPanelsMulti),
            Sample("Disabled panel", BuildExpansionPanelsDisabled)),
        PageWithSamples("Data", "Collapse", "Animated and static content reveal.",
            Sample("Animated reveal", BuildCollapseAnimated),
            Sample("Static reveal", BuildCollapseStatic),
            Sample("Custom duration", BuildCollapseCustomDuration),
            Sample("Disabled static", BuildCollapseDisabledStatic),
            Sample("Zero duration", BuildCollapseZeroDuration)),
        PageWithSamples("Data", "Timeline", "Vertical event sequence.",
            Sample("Default sequence", BuildTimelineDefault),
            Sample("Rich content", BuildTimelineRich),
            Sample("Horizontal", BuildTimelineHorizontal),
            Sample("Empty", BuildTimelineEmpty),
            Sample("Disabled", BuildTimelineDisabled)),
        PageWithSamples("Data", "Carousel", "Slide navigation with arrows and bullets.",
            Sample("Default carousel", BuildCarouselDefault),
            Sample("Chrome hidden", BuildCarouselChromeHidden),
            Sample("Auto play", BuildCarouselAutoPlay),
            Sample("GoTo clamped", BuildCarouselGoToClamped),
            Sample("Empty", BuildCarouselEmpty),
            Sample("Disabled", BuildCarouselDisabled)),
        PageWithSamples("Data", "Stepper", "Multi-step workflow navigation.",
            Sample("Active step", BuildStepperActive),
            Sample("Completed steps", BuildStepperCompleted),
            Sample("Clamped ActiveIndex", BuildStepperClamped),
            Sample("Disabled", BuildStepperDisabled),
            Sample("Empty", BuildStepperEmpty)),
        PageWithSamples("Data", "Pagination", "Page buttons with boundary and ellipsis behavior.",
            Sample("Boundary pages", BuildPaginationBoundary),
            Sample("Windowed pages", BuildPaginationWindowed),
            Sample("Secondary color", BuildPaginationSecondaryColor),
            Sample("Clamped selected page", BuildPaginationClamped),
            Sample("Empty and disabled", BuildPaginationEmptyAndDisabled)),

        PageWithSamples("Navigation", "Breadcrumbs", "Path navigation with current item text.",
            Sample("Default trail", BuildBreadcrumbsDefaultTrail),
            Sample("Custom separator", BuildBreadcrumbsCustomSeparator),
            Sample("Href and disabled item", BuildBreadcrumbsHrefAndDisabled),
            Sample("Deep trail", BuildBreadcrumbsDeepTrail)),
        PageWithSamples("Navigation", "Link", "Clickable text link variants.",
            Sample("States and colors", BuildLinkColors),
            Sample("Href and disabled", BuildLinkHrefAndDisabled)),
        PageWithSamples("Navigation", "NavMenu", "Side-menu container with links and groups.",
            Sample("Simple menu", BuildNavMenuSimple),
            Sample("Grouped menu", BuildNavMenuGrouped)),
        Page("Navigation", "NavLink", "Active and hoverable navigation rows.", BuildNavLink),
        Page("Navigation", "NavGroup", "Collapsible navigation groups.", BuildNavGroup),
        Page("Navigation", "NavigationRail", "Compact vertical destination rail with single selection.", BuildNavigationRail),
        Page("Navigation", "BottomNavigation", "Horizontal bottom destination bar with single selection.", BuildBottomNavigation),

        PageWithSamples("Layout", "Container", "Centered and width-capped content regions.",
            Sample("Breakpoint caps", BuildContainerBreakpointCaps),
            Sample("No gutters", BuildContainerNoGutters)),
        PageWithSamples("Layout", "ResponsiveGrid", "Responsive 12-column layout with column spans.",
            Sample("Fixed spans", BuildGridLayoutFixedSpans),
            Sample("Responsive spans", BuildGridLayoutResponsiveSpans)),
        Page("Layout", "Col", "ResponsiveGrid child span settings across breakpoints.", BuildItemLayout),
        PageWithSamples("Layout", "Spacer", "Flexible space for toolbars and docked rows.",
            Sample("Star column spacer", BuildSpacerStarColumn),
            Sample("Dock fill spacer", BuildSpacerDockFill)),
        PageWithSamples("Layout", "Hidden", "Breakpoint-based visibility.",
            Sample("Down mode", BuildHiddenDownMode),
            Sample("Up mode", BuildHiddenUpMode),
            Sample("Only mode", BuildHiddenOnlyMode)),
        Page("Layout", "ScrollToTop", "Floating scroll affordance used in this app shell.", BuildScrollToTop),

        Page("Shell", "Layout", "App shell composition with bar, drawer, and content.", BuildShellLayout),
        PageWithSamples("Shell", "AppBar", "Elevated top application bar.",
            Sample("Actions", BuildAppBarActions),
            Sample("Custom actions slot", BuildAppBarCustomActions),
            Sample("Dense", BuildAppBarDense)),
        Page("Shell", "Drawer", "Docked or temporary side navigation.", BuildDrawer),
        Page("Shell", "MainContent", "Scrollable main content region.", BuildMainContent),

        PageWithSamples("Surfaces", "Paper", "Elevation, outlined, square, and filled surfaces.",
            Sample("Elevation", BuildPaperElevation),
            Sample("Outlined", BuildPaperOutlined),
            Sample("Square", BuildPaperSquare),
            Sample("Colored", BuildPaperColored)),
        Page("Surfaces", "Card", "Header, media, content, and actions.", BuildCard),
        Family("Surfaces", "List", "List rows, subheaders, secondary text, and trailing actions.", BuildList, "List", "ListSubheader", "ListItem", "Badge", "IconButton"),
        Page("Surfaces", "Ripple", "Pointer feedback effect.", BuildRipple),

        PageWithSamples("Charts", "PieChart", "Pie and donut chart rendering.",
            Sample("Themed pie", BuildPieChartThemedPie),
            Sample("Explicit donut", BuildPieChartExplicitDonut),
            Sample("Donut with center total", BuildPieChartCenterTotal),
            Sample("Slice percentages", BuildPieChartDataLabels)),
        PageWithSamples("Charts", "BarChart", "Bar chart rendering from numeric values.",
            Sample("Themed bars", BuildBarChartThemedBars),
            Sample("Axes", BuildBarChartAxes),
            Sample("Bound data", BuildBarChartBound),
            Sample("Grouped series", BuildBarChartGrouped),
            Sample("Stacked series", BuildBarChartStacked),
            Sample("Signed values", BuildBarChartSigned),
            Sample("Data labels", BuildBarChartDataLabels),
            Sample("Interactive", BuildBarChartInteractive),
            Sample("No data", BuildBarChartNoData)),
        PageWithSamples("Charts", "LineChart", "Line and area chart rendering.",
            Sample("Line", BuildLineChartLine),
            Sample("Axes", BuildLineChartAxes),
            Sample("Multiple series", BuildLineChartSeries),
            Sample("Area", BuildLineChartArea),
            Sample("Signed values", BuildLineChartSigned),
            Sample("Data labels", BuildLineChartDataLabels)),
    ];

    private NavMenu BuildSideMenu()
    {
        var nav = new NavMenu { Width = 244, Spacing = 2 };
        foreach (var group in _pages.GroupBy(page => page.Group))
        {
            var navGroup = new NavGroup
            {
                Title = group.Key,
                Icon = IconForGroup(group.Key),
                Expanded = group.Key is "Start" or "Display" or "Buttons" or "Inputs",
            };

            foreach (var page in group)
            {
                var link = new NavLink { Content = page.Title, Icon = IconForPage(page.Title) };
                link.OnClick = () => ShowPage(page);
                _links[page.Route] = link;
                navGroup.Items.Add(link);
            }

            nav.Children.Add(navGroup);
        }

        return nav;
    }

    private void ShowPage(GalleryPage page)
    {
        _pageHost.Content = BuildArticle(page);

        foreach (var link in _links)
        {
            link.Value.IsActive = string.Equals(link.Key, page.Route, StringComparison.Ordinal);
        }
    }

    internal static StackPanel BuildArticle(GalleryPage page)
    {
        var breadcrumbs = new Breadcrumbs();
        breadcrumbs.Items.Add(new BreadcrumbItem("Components"));
        breadcrumbs.Items.Add(new BreadcrumbItem(page.Group));
        breadcrumbs.Items.Add(new BreadcrumbItem(page.Title));

        var header = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                breadcrumbs,
                new Text { Text = page.Title, Typo = Typo.H4 },
                new Text { Text = page.Description, Typo = Typo.Body1, Color = LoamColor.Default, Opacity = 0.72, TextWrapping = TextWrapping.Wrap, MaxWidth = 620 },
            },
        };

        var article = new StackPanel
        {
            Margin = new Thickness(32, 28, 32, 40),
            Spacing = 24,
            Children = { header },
        };

        if (page.Samples.Count > 0)
        {
            foreach (var sample in page.Samples)
            {
                article.Children.Add(BuildSampleBlock(sample));
            }
        }
        else
        {
            article.Children.Add(BuildPreviewPanel(page));
            article.Children.Add(new CodeSampleView(page.Title, page.Code));
        }

        return article;
    }

    private static StackPanel BuildSampleBlock(GallerySample sample)
    {
        var preview = new Paper
        {
            Elevation = 1,
            Padding = new Thickness(0),
            Content = new StackPanel
            {
                Children =
                {
                    PanelHeader(sample.Caption, "Live control surface"),
                    new Border
                    {
                        Padding = new Thickness(28),
                        Child = sample.Build(),
                    },
                },
            },
        };

        return new StackPanel
        {
            Spacing = 12,
            Children = { preview, new CodeSampleView(sample.Caption, sample.Code) },
        };
    }

    private static Paper BuildPreviewPanel(GalleryPage page)
    {
        var preview = page.Build();
        Control body = page.Route == "Start/Sizes"
            ? new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = preview,
            }
            : preview;

        return new Paper
        {
            Elevation = 1,
            Padding = new Thickness(0),
            Content = new StackPanel
            {
                Children =
                {
                    PanelHeader("Preview", "Live control surface"),
                    new Border
                    {
                        Padding = new Thickness(28),
                        Child = body,
                    },
                },
            },
        };
    }

    private static Border PanelHeader(string title, string meta)
    {
        var header = new Border
        {
            Padding = new Thickness(16, 12),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = new Avalonia.Controls.Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Children =
                {
                    new Text { Text = title, Typo = Typo.Subtitle1, VerticalAlignment = VerticalAlignment.Center },
                    new Text { Text = meta, Typo = Typo.Caption, Color = LoamColor.Secondary, VerticalAlignment = VerticalAlignment.Center },
                },
            },
        };
        Avalonia.Controls.Grid.SetColumn(((Avalonia.Controls.Grid)header.Child!).Children[1], 1);
        header.Bind(Border.BackgroundProperty, header.GetResourceObservable(LoamTokens.ColorSurfaceContainer));
        header.Bind(Border.BorderBrushProperty, header.GetResourceObservable(LoamTokens.ColorOutlineVariant));
        return header;
    }

    private static string IconForGroup(string group) => group switch
    {
        "Start" => Icons.Material.Filled.Dashboard,
        "Display" => Icons.Material.Filled.Widgets,
        "Buttons" => Icons.Material.Filled.TouchApp,
        "Inputs" => Icons.Material.Filled.Edit,
        "Pickers" => Icons.Material.Filled.CalendarToday,
        "Feedback" => Icons.Material.Filled.Info,
        "Data" => Icons.Material.Filled.Table,
        "Navigation" => Icons.Material.Filled.AltRoute,
        "Layout" => Icons.Material.Filled.GridView,
        "Shell" => Icons.Material.Filled.WebAsset,
        "Surfaces" => Icons.Material.Filled.Layers,
        "Charts" => Icons.Material.Filled.ShowChart,
        _ => Icons.Material.Filled.Widgets,
    };

    private static string IconForPage(string title) => title switch
    {
        "Overview" => Icons.Material.Filled.Dashboard,
        "Sizes" => Icons.Material.Filled.FormatSize,

        "Text" => Icons.Material.Filled.FormatSize,
        "Icon" => Icons.Material.Filled.Widgets,
        "Divider" => Icons.Material.Filled.HorizontalRule,
        "Chip" or "ChipSet" => Icons.Material.Filled.Label,
        "Badge" => Icons.Material.Filled.Notifications,
        "Avatar" => Icons.Material.Filled.Person,
        "AvatarGroup" => Icons.Material.Filled.Groups,

        "Button" or "IconButton" => Icons.Material.Filled.TouchApp,
        "ToggleIconButton" => Icons.Material.Filled.ToggleOn,
        "ButtonGroup" or "ToggleGroup" => Icons.Material.Filled.ViewWeek,
        "Fab" => Icons.Material.Filled.Add,
        "Menu" or "NavMenu" => Icons.Material.Filled.Menu,

        "Field" or "TextField" or "MaskedTextField" => Icons.Material.Filled.Edit,
        "NumericField" => Icons.Material.Filled.FormatSize,
        "Autocomplete" or "CommandPalette" => Icons.Material.Filled.Search,
        "Select" or "NavGroup" => Icons.Material.Filled.ExpandMore,
        "CheckBox" => Icons.Material.Filled.CheckBox,
        "Switch" => Icons.Material.Filled.ToggleOn,
        "Radio" or "RadioGroup" => Icons.Material.Filled.RadioButtonChecked,
        "Slider" => Icons.Material.Filled.Tune,
        "Rating" => Icons.Material.Filled.Star,
        "FileUpload" => Icons.Material.Filled.CloudUpload,
        "Form" => Icons.Material.Filled.Article,

        "DatePicker" or "DateRangePicker" or "MonthCalendar" => Icons.Material.Filled.CalendarToday,
        "TimePicker" => Icons.Material.Filled.Schedule,
        "ColorPicker" => Icons.Material.Filled.Palette,

        "Alert" => Icons.Material.Filled.Info,
        "ProgressCircular" or "ProgressLinear" => Icons.Material.Filled.ProgressActivity,
        "Skeleton" => Icons.Material.Filled.ViewHeadline,
        "Overlay" or "Popover" or "DialogService" => Icons.Material.Filled.Layers,
        "SnackbarService" => Icons.Material.Filled.Chat,

        "SimpleTable" => Icons.Material.Filled.Table,
        "DataGrid" => Icons.Material.Filled.GridView,
        "TreeView" => Icons.Material.Filled.AccountTree,
        "Tabs" => Icons.Material.Filled.Tabs,
        "ExpansionPanels" or "Collapse" => Icons.Material.Filled.ExpandMore,
        "Timeline" => Icons.Material.Filled.Timeline,
        "Carousel" => Icons.Material.Filled.ViewCarousel,
        "Stepper" => Icons.Material.Filled.Checklist,
        "Pagination" => Icons.Material.Filled.MoreHoriz,

        "Breadcrumbs" => Icons.Material.Filled.AltRoute,
        "Link" => Icons.Material.Filled.OpenInNew,
        "NavLink" => Icons.Material.Filled.ArrowForward,
        "NavigationRail" => Icons.Material.Filled.ViewWeek,
        "BottomNavigation" => Icons.Material.Filled.ViewHeadline,

        "Container" => Icons.Material.Filled.WebAsset,
        "ResponsiveGrid" or "Col" => Icons.Material.Filled.GridView,
        "Spacer" => Icons.Material.Filled.SwapHoriz,
        "Hidden" => Icons.Material.Filled.VisibilityOff,
        "ScrollToTop" => Icons.Material.Filled.ExpandLess,

        "Layout" or "AppBar" or "Drawer" or "MainContent" => Icons.Material.Filled.WebAsset,
        "Paper" or "Card" => Icons.Material.Filled.Article,
        "List" => Icons.Material.Filled.ViewHeadline,
        "Ripple" => Icons.Material.Filled.ProgressActivity,

        "PieChart" => Icons.Material.Filled.PieChart,
        "BarChart" => Icons.Material.Filled.BarChart,
        "LineChart" => Icons.Material.Filled.ShowChart,
        _ => Icons.Material.Filled.Widgets,
    };

    private static Control BuildOverview()
    {
        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                new LoamButton { Content = "Create", StartIcon = Icons.Material.Filled.Add, Variant = Variant.Filled, Color = LoamColor.Primary },
                new LoamButton { Content = "Review", StartIcon = Icons.Material.Filled.Check, Variant = Variant.Outlined, Color = LoamColor.Primary },
                new IconButton { Icon = Icons.Material.Filled.Settings, Variant = Variant.Text },
            },
        };

        var form = new StackPanel
        {
            Spacing = 14,
            MaxWidth = 360,
            Children =
            {
                new TextField { Label = "Project", Text = "Component audit" },
                new Select
                {
                    Label = "Priority",
                    Value = "high",
                    Items =
                    {
                        new SelectItem("Normal", "normal"),
                        new SelectItem("High", "high"),
                        new SelectItem("Urgent", "urgent"),
                    },
                },
                new ProgressLinear { Value = 68, Color = LoamColor.Success },
            },
        };

        var timeline = new Timeline { MaxWidth = 360 };
        timeline.Items.Add(new TimelineItem("Token pass", LoamColor.Primary));
        timeline.Items.Add(new TimelineItem("Input review", LoamColor.Info));
        timeline.Items.Add(new TimelineItem("Gallery polish", LoamColor.Success));

        var charts = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
            Children =
            {
                new PieChart { Width = 150, Height = 150, Values = [40, 25, 20, 15], Donut = true },
                new LineChart { Width = 260, Height = 150, Values = [12, 18, 15, 24, 20, 29], Area = true },
            },
        };

        var board = new Loam.Controls.ResponsiveGrid { Spacing = 18 };
        board.Children.Add(new Col
        {
            Xs = 12,
            Md = 7,
            Child = new StackPanel
            {
                Spacing = 18,
                Children =
                {
                    new Alert { Color = LoamColor.Success, Icon = Icons.Material.Filled.Check, Content = "All previews use live Loam controls." },
                    actions,
                    form,
                },
            },
        });
        board.Children.Add(new Col
        {
            Xs = 12,
            Md = 5,
            Child = new StackPanel
            {
                Spacing = 18,
                Children =
                {
                    charts,
                    timeline,
                },
            },
        });

        return board;
    }

    private static StackPanel BuildSizeMatrix()
    {
        var allSizes = new[] { LoamSize.ExtraSmall, LoamSize.Small, LoamSize.Medium, LoamSize.Large, LoamSize.ExtraLarge };

        static AvatarGroup AvatarGroupFor(LoamSize size)
        {
            var group = new AvatarGroup { Max = 2, Spacing = size switch { LoamSize.ExtraLarge => -18, LoamSize.Large => -12, LoamSize.ExtraSmall => -6, _ => -8 } };
            group.Items.Add(new Avatar { Content = "A", Size = size, Color = LoamColor.Primary });
            group.Items.Add(new Avatar { Content = "B", Size = size, Color = LoamColor.Secondary });
            group.Items.Add(new Avatar { Content = "C", Size = size, Color = LoamColor.Info });
            return group;
        }

        ColumnDefinitions MatrixColumns() => new("132,Auto,Auto,Auto,Auto,Auto");

        Avalonia.Controls.Grid HeaderRow()
        {
            var row = new Avalonia.Controls.Grid
            {
                Name = "PART_SizeMatrixHeader",
                ColumnDefinitions = MatrixColumns(),
                Margin = new Thickness(0, 0, 0, 4),
            };

            foreach (var size in allSizes)
            {
                var header = new Text
                {
                    Text = size.ToString(),
                    Typo = Typo.Caption,
                    Color = LoamColor.Secondary,
                    Margin = new Thickness(0, 0, 24, 0),
                };
                Avalonia.Controls.Grid.SetColumn(header, Array.IndexOf(allSizes, size) + 1);
                row.Children.Add(header);
            }

            return row;
        }

        Avalonia.Controls.Grid SizeRow(string label, Func<LoamSize, Control> build)
        {
            var row = new Avalonia.Controls.Grid
            {
                Name = $"PART_SizeRow_{label}",
                ColumnDefinitions = MatrixColumns(),
                MinHeight = 78,
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            row.Children.Add(new Text
            {
                Text = label,
                Typo = Typo.Subtitle2,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 24, 0),
            });

            for (var i = 0; i < allSizes.Length; i++)
            {
                var size = allSizes[i];
                var cell = new StackPanel
                {
                    Name = $"PART_SizeCell_{label}_{size}",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 24, 0),
                    Children = { build(size) },
                };
                Avalonia.Controls.Grid.SetColumn(cell, i + 1);
                row.Children.Add(cell);
            }

            return row;
        }

        var matrix = new StackPanel
        {
            Name = "PART_SizeMatrix",
            Spacing = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                HeaderRow(),
                SizeRow("IconButton", size => new IconButton { Icon = Icons.Material.Filled.Settings, Size = size, Variant = Variant.Outlined, Color = LoamColor.Primary }),
                SizeRow("ToggleIconButton", size => new ToggleIconButton { Icon = Icons.Material.Filled.FavoriteBorder, ToggledIcon = Icons.Material.Filled.Favorite, Toggled = size is LoamSize.Large or LoamSize.ExtraLarge, Size = size, Color = LoamColor.Error }),
                SizeRow("ButtonGroup", size =>
                {
                    var group = new ButtonGroup { Size = size, Variant = Variant.Outlined, Color = LoamColor.Primary };
                    group.Items.Add(new LoamButton { Content = "Day" });
                    group.Items.Add(new LoamButton { Content = "Week" });
                    group.Items.Add(new LoamButton { Content = "Month" });
                    return group;
                }),
                SizeRow("ToggleGroup", size =>
                {
                    var group = new ToggleGroup { Size = size, SelectedValue = "week" };
                    group.Items.Add(new ToggleItem("Day", "day"));
                    group.Items.Add(new ToggleItem("Week", "week"));
                    group.Items.Add(new ToggleItem("Month", "month"));
                    return group;
                }),
                SizeRow("Fab", size => new Fab { Label = size.ToString(), StartIcon = Icons.Material.Filled.Add, Size = size, Color = LoamColor.Secondary }),
                SizeRow("Icon", size => new Icon { Data = Icons.Material.Filled.Widgets, Size = size, Color = LoamColor.Primary }),
                SizeRow("Avatar", size => new Avatar { Content = size.ToString()[0].ToString(), Size = size, Color = LoamColor.Primary }),
                SizeRow("AvatarGroup", AvatarGroupFor),
                SizeRow("Chip", size => new Chip { Text = size.ToString(), Icon = Icons.Material.Filled.Label, Size = size, Color = LoamColor.Info }),
                SizeRow("CheckBox", size => new Loam.Controls.CheckBox { Content = size.ToString(), Size = size, IsChecked = true, Color = LoamColor.Primary }),
                SizeRow("Switch", size => new Switch { Content = size.ToString(), Size = size, IsChecked = true, Color = LoamColor.Success }),
                SizeRow("Radio", size => new Radio { Content = size.ToString(), Size = size, IsChecked = true, Color = LoamColor.Primary }),
                SizeRow("Rating", size => new Rating { Size = size, SelectedValue = 3, Color = LoamColor.Warning }),
                SizeRow("ProgressCircular", size => new ProgressCircular { Size = size, Indeterminate = false, Value = 64, Color = LoamColor.Primary }),
                SizeRow("ProgressLinear", size => new ProgressLinear { Size = size, Value = 64, Label = size.ToString(), ShowValue = true, Color = LoamColor.Primary }),
                SizeRow("Skeleton", size => new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        Skeleton.TextLine(180, size, animate: false, label: $"{size} text loading"),
                        Skeleton.Button(132, size, animate: false, label: $"{size} action loading"),
                    },
                }),
            },
        };

        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                matrix,
            },
        };
    }

    private static Border BuildButtonConfigurationRail()
    {
        var textBrush = new SolidColorBrush(Color.Parse("#F4EFF8"));
        var mutedBrush = new SolidColorBrush(Color.Parse("#CAC4D0"));
        var lineBrush = new SolidColorBrush(Color.Parse("#484450"));
        var dotBrush = new SolidColorBrush(Color.Parse("#383440"));

        Text RailText(string value, Typo typo, IBrush brush, double opacity = 1) => new()
        {
            Text = value,
            Typo = typo,
            Color = LoamColor.Inherit,
            Foreground = brush,
            Opacity = opacity,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        Border Dot() => new()
        {
            Width = 1,
            Height = 1,
            Background = dotBrush,
            Opacity = 0.7,
        };

        var dotGrid = new UniformGrid { Columns = 56, Rows = 7, Opacity = 0.62 };
        for (var i = 0; i < 392; i++)
        {
            dotGrid.Children.Add(Dot());
        }

        var samples = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        foreach (var size in Sizes)
        {
            samples.Children.Add(new StackPanel
            {
                MinWidth = size switch
                {
                    LoamSize.ExtraSmall => 74,
                    LoamSize.Small => 78,
                    LoamSize.Large => 118,
                    LoamSize.ExtraLarge => 156,
                    _ => 104,
                },
                Spacing = 10,
                VerticalAlignment = VerticalAlignment.Bottom,
                Children =
                {
                    RailText(SizeLabel(size), Typo.Caption, mutedBrush, 0.86),
                    new LoamButton
                    {
                        Content = SizeLabel(size),
                        Size = size,
                        Variant = Variant.Filled,
                        Color = LoamColor.Primary,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                },
            });
        }

        var axis = new Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            VerticalAlignment = VerticalAlignment.Center,
        };
        var marker = new Border
        {
            Width = 18,
            Height = 18,
            CornerRadius = new CornerRadius(9),
            BorderThickness = new Thickness(1),
            BorderBrush = mutedBrush,
            Child = RailText("1", Typo.LabelSmall, mutedBrush),
        };
        var baseline = new Border
        {
            Height = 1,
            Background = lineBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 0, 0),
        };
        Avalonia.Controls.Grid.SetColumn(baseline, 1);
        axis.Children.Add(marker);
        axis.Children.Add(baseline);

        var rail = new Avalonia.Controls.Grid
        {
            MinHeight = 218,
            Children =
            {
                dotGrid,
                new StackPanel
                {
                    Spacing = 14,
                    Margin = new Thickness(40, 24, 40, 28),
                    Children =
                    {
                        new Border
                        {
                            Height = 134,
                            Child = samples,
                        },
                        axis,
                    },
                },
            },
        };

        return new Border
        {
            Background = new SolidColorBrush(Color.Parse("#141218")),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(24),
            MaxWidth = 980,
            Child = new StackPanel
            {
                Spacing = 18,
                Children =
                {
                    RailText("Configurations", Typo.H5, textBrush),
                    new ScrollViewer
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                        Content = rail,
                    },
                    BuildButtonVariantRail(textBrush, mutedBrush),
                },
            },
        };
    }

    private static Avalonia.Controls.Grid BuildButtonVariantRail(IBrush textBrush, IBrush mutedBrush)
    {
        var grid = new Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("112,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
            Margin = new Thickness(0, 4, 0, 0),
        };

        void AddRow(int rowIndex, string label, params Control[] controls)
        {
            var caption = new Text
            {
                Text = label,
                Typo = Typo.Caption,
                Color = LoamColor.Inherit,
                Foreground = mutedBrush,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Avalonia.Controls.Grid.SetRow(caption, rowIndex);
            grid.Children.Add(caption);

            var strip = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
            foreach (var control in controls)
            {
                control.Margin = new Thickness(0, 0, 10, 10);
                strip.Children.Add(control);
            }

            Avalonia.Controls.Grid.SetColumn(strip, 1);
            Avalonia.Controls.Grid.SetRow(strip, rowIndex);
            grid.Children.Add(strip);
        }

        AddRow(0, "Variant",
            new LoamButton { Content = "Filled", Variant = Variant.Filled, Color = LoamColor.Primary },
            new LoamButton { Content = "Outlined", Variant = Variant.Outlined, Color = LoamColor.Primary },
            new LoamButton { Content = "Text", Variant = Variant.Text, Color = LoamColor.Primary });

        AddRow(1, "Icon",
            new LoamButton { Content = "Create", StartIcon = Icons.Material.Filled.Add, Variant = Variant.Filled, Color = LoamColor.Primary },
            new LoamButton { Content = "Open", EndIcon = Icons.Material.Filled.ArrowForward, Variant = Variant.Text, Color = LoamColor.Primary });

        AddRow(2, "State",
            new LoamButton { Content = "Available", Variant = Variant.Filled, Color = LoamColor.Primary },
            new LoamButton { Content = "Disabled", Variant = Variant.Filled, Color = LoamColor.Primary, IsEnabled = false });

        return grid;
    }

    private static string SizeLabel(LoamSize size) => size switch
    {
        LoamSize.ExtraSmall => "Extra small",
        LoamSize.ExtraLarge => "Extra large",
        _ => size.ToString(),
    };

    private static StackPanel BuildAlertSeveritiesAndVariants()
    {
        var stack = new StackPanel { Spacing = 12, MaxWidth = 620, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Alert
        {
            Color = LoamColor.Info,
            Icon = Icons.Material.Filled.Settings,
            Title = "Configuration saved",
            Message = "Generated title and message regions keep alert anatomy inside the component.",
            Action = new LoamButton { Content = "View", Variant = Variant.Text, Color = LoamColor.Info, Size = LoamSize.Small },
        });
        stack.Children.Add(new Alert
        {
            Color = LoamColor.Success,
            Variant = Variant.Filled,
            Icon = Icons.Material.Filled.Check,
            Title = "Build completed",
            Message = "Filled alerts keep icon, text, action and close affordances aligned.",
            Closeable = true,
        });
        stack.Children.Add(new Alert
        {
            Color = LoamColor.Warning,
            Variant = Variant.Outlined,
            Icon = Icons.Material.Filled.Star,
            Title = "Review required",
            Message = "Outlined alerts use tokenized border and foreground roles.",
            Action = new LoamButton { Content = "Open", Variant = Variant.Text, Color = LoamColor.Warning, Size = LoamSize.Small },
            Closeable = true,
        });
        stack.Children.Add(new Alert { Color = LoamColor.Error, Title = "Validation failed", Message = "Error alert without a leading icon." });
        return stack;
    }

    private static Alert BuildAlertDisabled()
    {
        return new Alert
        {
            Color = LoamColor.Info,
            Title = "Disabled",
            Message = "Disabled alerts dim generated text, icon, action and close regions.",
            Closeable = true,
            IsEnabled = false,
            MaxWidth = 620,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
    }

    private static Alert BuildAlertContentFallback()
    {
        return new Alert
        {
            Color = LoamColor.Default,
            Content = "Compatibility path: raw Content still renders.",
            MaxWidth = 620,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
    }

    private static StackPanel BuildLayoutSamples()
    {
        var grid = new Loam.Controls.ResponsiveGrid { Spacing = 12, MaxWidth = 720 };
        for (var i = 1; i <= 6; i++)
        {
            grid.Children.Add(new Col
            {
                Xs = 12,
                Sm = 6,
                Md = 4,
                Child = new Paper
                {
                    Height = 72,
                    Elevation = 1,
                    Content = new Text { Text = $"Item {i}\nxs12 · sm6 · md4", Margin = new Thickness(12) },
                },
            });
        }

        var stack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new LoamButton { Content = "One", Variant = Variant.Filled, Color = LoamColor.Primary },
                new LoamButton { Content = "Two", Variant = Variant.Outlined, Color = LoamColor.Primary },
                new LoamButton { Content = "Three", Variant = Variant.Text, Color = LoamColor.Primary },
            },
        };

        var container = new Loam.Controls.Container
        {
            MaxWidthBreakpoint = Breakpoint.Sm,
            Child = new Paper
            {
                Height = 58,
                Elevation = 1,
                Content = new Text { Text = "Container capped at Sm", Margin = new Thickness(12) },
            },
        };

        return new StackPanel
        {
            Spacing = 18,
            Children =
            {
                Labeled("ResponsiveGrid", grid),
                Labeled("StackPanel", stack),
                Labeled("Container", container),
            },
        };
    }

    private static Loam.Controls.Container ContainerExample(string label, Breakpoint breakpoint, bool gutters)
    {
        return new Loam.Controls.Container
        {
            Width = 780,
            MaxWidthBreakpoint = breakpoint,
            Gutters = gutters,
            Child = new Paper
            {
                Height = 64,
                Elevation = 0,
                Outlined = true,
                Padding = new Thickness(16),
                Content = new Text { Text = label, Typo = Typo.Body2 },
            },
        };
    }

    private static StackPanel BuildContainerBreakpointCaps()
    {
        return new StackPanel
        {
            Spacing = 16,
            Children =
            {
                ContainerExample("MaxWidthBreakpoint = Breakpoint.Sm", Breakpoint.Sm, gutters: true),
                ContainerExample("MaxWidthBreakpoint = Breakpoint.Md", Breakpoint.Md, gutters: true),
                ContainerExample("MaxWidthBreakpoint = Breakpoint.Lg", Breakpoint.Lg, gutters: true),
            },
        };
    }

    private static StackPanel BuildContainerNoGutters()
    {
        return new StackPanel
        {
            Spacing = 16,
            Children =
            {
                ContainerExample("Gutters = false", Breakpoint.Md, gutters: false),
            },
        };
    }

    private static StackPanel BuildGridLayoutFixedSpans()
    {
        var spanGrid = new Loam.Controls.ResponsiveGrid { Spacing = 12, MaxWidth = 780 };
        foreach (var (label, span) in new[] { ("xs12", 12), ("xs6", 6), ("xs4", 4), ("xs3", 3) })
        {
            spanGrid.Children.Add(new Col
            {
                Xs = span,
                Child = new Paper
                {
                    Height = 56,
                    Elevation = 0,
                    Outlined = true,
                    Padding = new Thickness(12),
                    Content = new Text { Text = label, Typo = Typo.Body2 },
                },
            });
        }

        return new StackPanel
        {
            Spacing = 18,
            Children =
            {
                spanGrid,
            },
        };
    }

    private static StackPanel BuildGridLayoutResponsiveSpans()
    {
        var responsiveGrid = new Loam.Controls.ResponsiveGrid { Spacing = 12, MaxWidth = 780 };
        for (var i = 1; i <= 6; i++)
        {
            responsiveGrid.Children.Add(new Col
            {
                Xs = 12,
                Sm = 6,
                Md = 4,
                Child = new Paper
                {
                    Height = 72,
                    Elevation = 1,
                    Padding = new Thickness(12),
                    Content = new Text { Text = $"Item {i}\nxs12 / sm6 / md4", Typo = Typo.Body2 },
                },
            });
        }

        responsiveGrid.Children.Add(new Paper
        {
            Height = 56,
            Elevation = 0,
            Outlined = true,
            Padding = new Thickness(12),
            Content = new Text { Text = "Non-Item child spans 12 columns", Typo = Typo.Body2 },
        });

        return new StackPanel
        {
            Spacing = 18,
            Children =
            {
                responsiveGrid,
            },
        };
    }

    private static StackPanel BuildItemLayout()
    {
        var grid = new Loam.Controls.ResponsiveGrid { Spacing = 12, MaxWidth = 780 };
        grid.Children.Add(new Col { Xs = 12, Sm = 12, Md = 8, Lg = 8, Child = new Paper { Height = 72, Elevation = 1, Padding = new Thickness(12), Content = new Text { Text = "Main\nxs12 / md8", Typo = Typo.Body2 } } });
        grid.Children.Add(new Col { Xs = 12, Sm = 12, Md = 4, Lg = 4, Child = new Paper { Height = 72, Elevation = 1, Padding = new Thickness(12), Content = new Text { Text = "Side\nxs12 / md4", Typo = Typo.Body2 } } });
        grid.Children.Add(new Col { Xs = 6, Sm = 4, Md = 3, Lg = 2, Xl = 2, Xxl = 1, Child = new Paper { Height = 56, Elevation = 0, Outlined = true, Padding = new Thickness(12), Content = new Text { Text = "xs6 / sm4 / md3 / lg2 / xxl1", Typo = Typo.Body2 } } });
        grid.Children.Add(new Col { Xs = 6, Sm = 4, Md = 3, Lg = 2, Child = new Paper { Height = 56, Elevation = 0, Outlined = true, Padding = new Thickness(12), Content = new Text { Text = "breakpoint fallback", Typo = Typo.Body2 } } });
        grid.Children.Add(new Col { Xs = 12, Sm = 4, Md = 6, Lg = 4, Child = new Paper { Height = 56, Elevation = 0, Outlined = true, Padding = new Thickness(12), Content = new Text { Text = "mixed item span", Typo = Typo.Body2 } } });

        return new StackPanel
        {
            Spacing = 12,
            Children =
            {
                new Text { Text = "Item breakpoint props", Typo = Typo.Subtitle2 },
                grid,
            },
        };
    }

    private static StackPanel BuildSpacerStarColumn()
    {
        var toolbar = new Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Width = 480,
            Children =
            {
                new LoamButton { Content = "Back", Variant = Variant.Text, Color = LoamColor.Primary },
                new Spacer(),
                new LoamButton { Content = "Continue", Variant = Variant.Filled, Color = LoamColor.Primary },
            },
        };
        Avalonia.Controls.Grid.SetColumn(toolbar.Children[1], 1);
        Avalonia.Controls.Grid.SetColumn(toolbar.Children[2], 2);

        return new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Paper { Elevation = 0, Outlined = true, Padding = new Thickness(12), Content = toolbar },
            },
        };
    }

    private static StackPanel BuildSpacerDockFill()
    {
        var dockLeft = new Text { Text = "Leading", VerticalAlignment = VerticalAlignment.Center };
        var dockRight = new Text { Text = "Trailing", VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(dockLeft, Dock.Left);
        DockPanel.SetDock(dockRight, Dock.Right);
        var dock = new DockPanel
        {
            Width = 480,
            Height = 48,
            LastChildFill = true,
            Children =
            {
                dockLeft,
                dockRight,
                new Spacer(),
            },
        };

        return new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Paper { Elevation = 0, Outlined = true, Padding = new Thickness(12), Content = dock },
            },
        };
    }

    private static StackPanel BuildShellPreview()
    {
        static void AddShellItems(Drawer drawer, string first)
        {
            drawer.Items.Add(new DrawerItem { Icon = Icons.Material.Filled.Home, Text = first, IsActive = true });
            drawer.Items.Add(new DrawerItem { Icon = Icons.Material.Filled.Search, Text = "Search" });
            drawer.Items.Add(new DrawerItem { Icon = Icons.Material.Filled.Settings, Text = "Settings" });
        }

        static Border Frame(string title, Drawer drawer)
        {
            drawer.DrawerWidth = 148;
            drawer.MiniWidth = 52;
            AddShellItems(drawer, title);

            var content = new MainContent
            {
                Title = "Content",
                Subtitle = "Generated page header",
                Content = new StackPanel
                {
                    Spacing = 10,
                    Children =
                    {
                        new Alert { Color = LoamColor.Info, Content = "Tokenized shell" },
                        new ProgressLinear { Value = drawer.Mode == DrawerMode.Temporary ? 68 : 46, Width = 120 },
                    },
                },
            };

            var shell = new Layout
            {
                AppBar = new AppBar
                {
                    Dense = true,
                    Color = LoamColor.Primary,
                    Title = title,
                    NavigationIcon = Icons.Material.Filled.Menu,
                },
                Drawer = drawer,
                Content = content,
            };

            var frame = new Border
            {
                Width = 320,
                Height = 260,
                Child = shell,
                ClipToBounds = true,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
            };
            frame.Bind(Border.BorderBrushProperty, frame.GetResourceObservable(LoamTokens.Divider));
            return frame;
        }

        static StackPanel Example(string label, Border frame) => new()
        {
            Width = 320,
            Margin = new Thickness(0, 0, 18, 18),
            Spacing = 8,
            Children =
            {
                new Text { Text = label, Typo = Typo.Subtitle2 },
                frame,
            },
        };

        var frames = new WrapPanel { Orientation = Orientation.Horizontal };
        frames.Children.Add(Example("Docked", Frame("Docked", new Drawer())));
        frames.Children.Add(Example("Mini", Frame("Mini", new Drawer { Mini = true })));
        frames.Children.Add(Example("Temporary", Frame("Temporary", new Drawer { Mode = DrawerMode.Temporary, Open = true })));

        return new StackPanel
        {
            Spacing = 16,
            Children =
            {
                frames,
                new Alert { Color = LoamColor.Info, Content = "Temporary drawers use tokenized scrims and Escape close." },
            },
        };
    }

    private static Border BuildShellLayout()
    {
        var drawer = new Drawer
        {
            DrawerWidth = 148,
            Items =
            {
                new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Dashboard", IsActive = true },
                new DrawerItem { Icon = Icons.Material.Filled.Search, Text = "Search" },
                new DrawerItem { Icon = Icons.Material.Filled.Settings, Text = "Settings" },
            },
        };

        var shell = new Layout
        {
            AppBar = new AppBar
            {
                Dense = true,
                Color = LoamColor.Primary,
                Title = "Layout shell",
                Subtitle = "Generated app bar, drawer, and content",
                NavigationIcon = Icons.Material.Filled.Menu,
                NavigationAction = () => drawer.Toggle(),
                Actions =
                {
                    new AppBarAction
                    {
                        Icon = Icons.Material.Filled.Settings,
                        Label = "Settings",
                    },
                },
            },
            Drawer = drawer,
            Content = new MainContent
            {
                Title = "Main content",
                Subtitle = "Generated page header inside the shell body",
                Content = new StackPanel
                {
                    Spacing = 10,
                    Children =
                    {
                        new Alert { Color = LoamColor.Info, Content = "Layout coordinates generated app-bar, drawer, and content APIs." },
                    },
                },
            },
        };

        return new Border { Width = 360, Height = 260, ClipToBounds = true, Child = shell };
    }

    private static AppBar BuildAppBarActions()
    {
        return new AppBar
        {
            Color = LoamColor.Primary,
            Title = "Primary app bar",
            Subtitle = "Configured from properties",
            NavigationIcon = Icons.Material.Filled.Menu,
            NavigationAction = () => { },
            Actions =
            {
                new AppBarAction
                {
                    Icon = Icons.Material.Filled.Settings,
                    Label = "Settings",
                    OnClick = () => { },
                },
                new AppBarAction
                {
                    Icon = Icons.Material.Filled.Search,
                    Label = "Search",
                    Color = LoamColor.Inherit,
                    Size = LoamSize.Small,
                    OnClick = () => { },
                },
                new AppBarAction
                {
                    Icon = Icons.Material.Filled.Delete,
                    Label = "Delete disabled",
                    IsEnabled = false,
                },
            },
        };
    }

    private static AppBar BuildAppBarCustomActions()
    {
        return new AppBar
        {
            Color = LoamColor.Secondary,
            Title = "Custom actions slot",
            Subtitle = "Arbitrary controls via CustomActions",
            NavigationIcon = Icons.Material.Filled.Menu,
            NavigationAction = () => { },
            CustomActions =
            {
                new LoamButton
                {
                    Content = "Upgrade",
                    Variant = Variant.Filled,
                    Color = LoamColor.Inherit,
                    StartIcon = Icons.Material.Filled.Star,
                },
                new IconButton { Icon = Icons.Material.Filled.Settings, Color = LoamColor.Inherit },
            },
        };
    }

    private static AppBar BuildAppBarDense()
    {
        return new AppBar
        {
            Dense = true,
            Elevation = 0,
            Color = LoamColor.Default,
            Title = "Dense app bar",
            Subtitle = "No custom toolbar required",
        };
    }

    private static StackPanel BuildDrawer()
    {
        static Drawer DrawerSample(string title, Drawer drawer)
        {
            drawer.DrawerWidth = 156;
            drawer.MiniWidth = 56;
            drawer.Title = title;
            drawer.Subtitle = drawer.Mode == DrawerMode.Temporary ? "Overlay navigation" : "Side navigation";
            drawer.FooterText = "Generated Drawer.Items";
            drawer.Items.Add(new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Overview", Label = "Overview", IsActive = true });
            drawer.Items.Add(new DrawerItem { Icon = Icons.Material.Filled.Settings, Text = "Settings", Label = "Settings" });
            drawer.Items.Add(new DrawerItem { Icon = Icons.Material.Filled.Delete, Text = "Disabled", Label = "Disabled item", IsEnabled = false, Color = LoamColor.Error });
            return drawer;
        }

        var temporary = DrawerSample("Temporary", new Drawer { Mode = DrawerMode.Temporary, Open = true });
        var toggle = new LoamButton
        {
            Content = "Toggle temporary",
            Variant = Variant.Outlined,
            Color = LoamColor.Primary,
            StartIcon = Icons.Material.Filled.Menu,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        toggle.Click += (_, _) => temporary.Toggle();

        return new StackPanel
        {
            Spacing = 12,
            Children =
            {
                toggle,
                new WrapPanel
                {
                    Children =
                    {
                        new Border { Width = 180, Height = 260, ClipToBounds = true, Margin = new Thickness(0, 0, 16, 16), Child = DrawerSample("Docked", new Drawer()) },
                        new Border { Width = 88, Height = 260, ClipToBounds = true, Margin = new Thickness(0, 0, 16, 16), Child = DrawerSample("Mini", new Drawer { Mini = true }) },
                        new Border { Width = 220, Height = 260, ClipToBounds = true, Child = temporary },
                    },
                },
            },
        };
    }

    private static Border BuildMainContent()
    {
        var drawer = new Drawer
        {
            DrawerWidth = 156,
            Items =
            {
                new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Overview", Label = "Overview", IsActive = true },
                new DrawerItem { Icon = Icons.Material.Filled.BarChart, Text = "Reports", Label = "Reports" },
                new DrawerItem { Icon = Icons.Material.Filled.Settings, Text = "Settings", Label = "Settings" },
            },
        };

        var content = new MainContent
        {
            Padding = new Thickness(20),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Title = "Main content region",
            Subtitle = "MainContent owns the scrollable work area below the app bar and beside the drawer, keeping page padding and body content independent from shell chrome.",
            SecondaryActionText = "Export",
            PrimaryActionText = "Review",
            Content = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    new WrapPanel
                    {
                        Children =
                        {
                            new Chip { Text = "Pinned", Color = LoamColor.Primary, Size = LoamSize.Small, Margin = new Thickness(0, 0, 8, 0) },
                            new Chip { Text = "3 updates", Variant = Variant.Outlined, Color = LoamColor.Secondary, Size = LoamSize.Small },
                        },
                    },
                    new ProgressLinear { Value = 64, Width = 300 },
                    new Alert { Color = LoamColor.Success, Content = "Scrollable content keeps its padding while the shell stays fixed." },
                    new StackPanel
                    {
                        Spacing = 4,
                        Children =
                        {
                            new ListItem { Icon = Icons.Material.Filled.Article, Content = "Acceptance notes", IsSelected = true },
                            new ListItem { Icon = Icons.Material.Filled.Check, Content = "Verified component states" },
                            new ListItem { Icon = Icons.Material.Filled.Person, Content = "Assigned reviewers" },
                            new ListItem { Icon = Icons.Material.Filled.Article, Content = "Release checklist" },
                        },
                    },
                },
            },
        };

        var shell = new Layout
        {
            AppBar = new AppBar
            {
                Dense = true,
                Color = LoamColor.Primary,
                Elevation = 1,
                Title = "Project workspace",
                Subtitle = "Shell component API",
                NavigationIcon = Icons.Material.Filled.Menu,
                NavigationAction = () => drawer.Toggle(),
                Actions =
                {
                    new AppBarAction { Icon = Icons.Material.Filled.Settings, Label = "Settings" },
                    new AppBarAction { Icon = Icons.Material.Filled.Search, Label = "Search", Size = LoamSize.Small },
                },
            },
            Drawer = drawer,
            Content = content,
        };

        var frame = new Border
        {
            Width = 640,
            Height = 460,
            ClipToBounds = true,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = shell,
        };
        frame.Bind(Border.BorderBrushProperty, frame.GetResourceObservable(LoamTokens.ColorOutlineVariant));
        return frame;
    }

    private static StackPanel BuildTextDisplayRoles()
    {
        var displayRoles = new StackPanel { Spacing = 6 };
        foreach (var (role, label) in new[]
        {
            (Typo.DisplayLarge, "Display large"),
            (Typo.DisplayMedium, "Display medium"),
            (Typo.DisplaySmall, "Display small"),
        })
        {
            displayRoles.Children.Add(new Text { Text = $"{label} - component headline", Typo = role });
        }

        return new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {                displayRoles,
            },
        };
    }

    private static StackPanel BuildTextContentRoles()
    {
        var contentRoles = new StackPanel { Spacing = 5 };
        foreach (var (role, label) in new[]
        {
            (Typo.HeadlineLarge, "Headline large"),
            (Typo.HeadlineMedium, "Headline medium"),
            (Typo.HeadlineSmall, "Headline small"),
            (Typo.TitleLarge, "Title large"),
            (Typo.TitleMedium, "Title medium"),
            (Typo.TitleSmall, "Title small"),
            (Typo.BodyLarge, "Body large"),
            (Typo.BodyMedium, "Body medium"),
            (Typo.BodySmall, "Body small"),
            (Typo.LabelLarge, "Label large"),
            (Typo.LabelMedium, "Label medium"),
            (Typo.LabelSmall, "Label small"),
        })
        {
            contentRoles.Children.Add(new Text { Text = $"{label} - The quick brown fox", Typo = role });
        }

        return new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {                contentRoles,
            },
        };
    }

    private static StackPanel BuildTextLegacyAliases()
    {
        var legacyAliases = new WrapPanel();
        foreach (var typo in new[] { Typo.H4, Typo.H6, Typo.Subtitle1, Typo.Body1, Typo.Body2, Typo.Caption, Typo.Overline })
        {
            legacyAliases.Children.Add(new Text
            {
                Text = typo.ToString(),
                Typo = typo,
                Color = LoamColor.Secondary,
                Margin = new Thickness(0, 0, 18, 10),
            });
        }

        return new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {                legacyAliases,
            },
        };
    }

    private static StackPanel BuildTextColors()
    {
        var colorSamples = new WrapPanel();
        foreach (var color in new[] { LoamColor.Default, LoamColor.Primary, LoamColor.Secondary, LoamColor.Tertiary, LoamColor.Success, LoamColor.Warning, LoamColor.Error })
        {
            colorSamples.Children.Add(new Text
            {
                Text = color.ToString(),
                Typo = Typo.BodyMedium,
                Color = color,
                Margin = new Thickness(0, 0, 18, 10),
            });
        }

        return new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {                colorSamples,
            },
        };
    }

    private static StackPanel BuildTextAlignment()
    {
        var alignment = new StackPanel
        {
            Spacing = 6,
            Width = 420,
            Children =
            {
                new Text { Text = "Left aligned body text", Typo = Typo.BodyMedium, Align = TextAlignment.Left },
                new Text { Text = "Centered label text", Typo = Typo.LabelLarge, Align = TextAlignment.Center },
                new Text { Text = "Right aligned helper text", Typo = Typo.BodySmall, Align = TextAlignment.Right, Color = LoamColor.Secondary },
                new Text { Text = "Wrapped body text keeps readable line length and uses token typography for repeated application content.", Typo = Typo.BodyMedium, TextWrapping = TextWrapping.Wrap, MaxWidth = 360, GutterBottom = true },
            },
        };

        return new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {                alignment,
            },
        };
    }

    private static ButtonGroup BuildButtonGroupOutlined()
    {
        var outlined = new ButtonGroup { Variant = Variant.Outlined, Color = LoamColor.Primary };
        outlined.Items.Add(new LoamButton { Content = "Left" });
        outlined.Items.Add(new LoamButton { Content = "Center" });
        outlined.Items.Add(new LoamButton { Content = "Right" });
        return outlined;
    }

    private static ButtonGroup BuildButtonGroupFilled()
    {
        var filled = new ButtonGroup { Variant = Variant.Filled, Color = LoamColor.Secondary };
        filled.Items.Add(new LoamButton { Content = "Day" });
        filled.Items.Add(new LoamButton { Content = "Week" });
        filled.Items.Add(new LoamButton { Content = "Month" });
        return filled;
    }

    private static StackPanel BuildButtonGroupSizes()
    {
        var sizeRows = new StackPanel { Spacing = 8, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var size in Sizes)
        {
            var group = new ButtonGroup { Variant = Variant.Outlined, Color = LoamColor.Primary, Size = size };
            group.Items.Add(new LoamButton { Content = "Day" });
            group.Items.Add(new LoamButton { Content = "Week" });
            group.Items.Add(new LoamButton { Content = "Month" });
            sizeRows.Children.Add(group);
        }

        return sizeRows;
    }

    private static StackPanel BuildButtonGroupToggle()
    {
        var favorites = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        favorites.Children.Add(new Text { Text = "Toggle icon button:", VerticalAlignment = VerticalAlignment.Center });
        favorites.Children.Add(new ToggleIconButton
        {
            Icon = Icons.Material.Filled.FavoriteBorder,
            ToggledIcon = Icons.Material.Filled.Favorite,
            Color = LoamColor.Default,
            ToggledColor = LoamColor.Error,
        });
        return favorites;
    }

    private static StackPanel BuildToggleIconButtonFavorite()
    {
        var favorites = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        favorites.Children.Add(new Text { Text = "Favorite", VerticalAlignment = VerticalAlignment.Center });
        favorites.Children.Add(new ToggleIconButton
        {
            Icon = Icons.Material.Filled.FavoriteBorder,
            ToggledIcon = Icons.Material.Filled.Favorite,
            Color = LoamColor.Default,
            ToggledColor = LoamColor.Error,
        });

        return new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                favorites,
                new Text { Text = "The toggled state swaps the glyph and can tint it independently.", Typo = Typo.Body2, Color = LoamColor.Secondary },
            },
        };
    }

    private static WrapPanel BuildToggleIconButtonSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel();
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new ToggleIconButton
            {
                Icon = Icons.Material.Filled.FavoriteBorder,
                ToggledIcon = Icons.Material.Filled.Favorite,
                Toggled = size is LoamSize.Large or LoamSize.ExtraLarge,
                Color = LoamColor.Default,
                ToggledColor = LoamColor.Error,
                Size = size,
                Margin = new Thickness(0, 0, 8, 8),
            });
        }

        return sizes;
    }

    private static WrapPanel BuildButtonsVariantRow(Variant variant)
    {
        var row = new WrapPanel();
        foreach (var color in Colors)
        {
            row.Children.Add(new LoamButton
            {
                Content = color.ToString(),
                Variant = variant,
                Color = color,
                Margin = new Thickness(0, 0, 8, 8),
            });
        }

        return row;
    }

    private static WrapPanel BuildButtonsFilled()
    {
        return BuildButtonsVariantRow(Variant.Filled);
    }

    private static WrapPanel BuildButtonsOutlined()
    {
        return BuildButtonsVariantRow(Variant.Outlined);
    }

    private static WrapPanel BuildButtonsText()
    {
        return BuildButtonsVariantRow(Variant.Text);
    }

    private static WrapPanel BuildButtonsIconSizes()
    {
        var iconSizes = new WrapPanel();
        foreach (var size in Sizes)
        {
            iconSizes.Children.Add(new LoamButton
            {
                Content = size.ToString(),
                StartIcon = Icons.Material.Filled.Check,
                EndIcon = Icons.Material.Filled.ArrowForward,
                Variant = Variant.Outlined,
                Color = LoamColor.Primary,
                Size = size,
                Margin = new Thickness(0, 0, 8, 8),
            });
        }

        return iconSizes;
    }

    private static WrapPanel BuildButtonsDisabled()
    {
        var disabled = new WrapPanel();
        foreach (var variant in new[] { Variant.Filled, Variant.Outlined, Variant.Text })
        {
            disabled.Children.Add(new LoamButton
            {
                Content = variant.ToString(),
                Variant = variant,
                Color = LoamColor.Primary,
                IsEnabled = false,
                Margin = new Thickness(0, 0, 8, 8),
            });
        }

        return disabled;
    }

    private static WrapPanel BuildButtonsWithIcons()
    {
        var withIcons = new WrapPanel();
        withIcons.Children.Add(new LoamButton
        {
            Content = "Save", StartIcon = Icons.Material.Filled.Check,
            Variant = Variant.Filled, Color = LoamColor.Primary, Margin = new Thickness(0, 0, 8, 8),
        });
        withIcons.Children.Add(new LoamButton
        {
            Content = "Delete", StartIcon = Icons.Material.Filled.Delete,
            Variant = Variant.Outlined, Color = LoamColor.Error, Margin = new Thickness(0, 0, 8, 8),
        });
        withIcons.Children.Add(new LoamButton
        {
            Content = "Back", StartIcon = Icons.Material.Filled.ArrowBack,
            Variant = Variant.Text, Color = LoamColor.Primary, Margin = new Thickness(0, 0, 8, 8),
        });
        return withIcons;
    }

    private static StackPanel BuildIconsColors()
    {
        var colorIcons = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        (string Data, LoamColor Color)[] icons =
        [
            (Icons.Material.Filled.Home, LoamColor.Default),
            (Icons.Material.Filled.Search, LoamColor.Primary),
            (Icons.Material.Filled.Settings, LoamColor.Info),
            (Icons.Material.Filled.Favorite, LoamColor.Error),
            (Icons.Material.Filled.Star, LoamColor.Warning),
            (Icons.Material.Filled.Person, LoamColor.Success),
        ];
        foreach (var (data, color) in icons)
        {
            colorIcons.Children.Add(new Icon { Data = data, Color = color, Size = LoamSize.Large, Margin = new Thickness(0, 0, 18, 12) });
        }

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                colorIcons,
            },
        };
    }

    private static StackPanel BuildIconsSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel();
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new StackPanel
            {
                Spacing = 4,
                Margin = new Thickness(0, 0, 22, 12),
                Children =
                {
                    new Icon { Data = Icons.Material.Filled.Settings, Color = LoamColor.Primary, Size = size },
                    new Text { Text = size.ToString(), Typo = Typo.Caption },
                },
            });
        }

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                sizes,
            },
        };
    }

    private static StackPanel BuildIconsCommonGlyphs()
    {
        var contentIcons = new WrapPanel();
        foreach (var (label, data) in new[]
        {
            ("Home", Icons.Material.Filled.Home),
            ("Favorite", Icons.Material.Filled.Favorite),
            ("Calendar", Icons.Material.Filled.CalendarToday),
            ("Upload", Icons.Material.Filled.CloudUpload),
            ("Article", Icons.Material.Filled.Article),
            ("Chart", Icons.Material.Filled.ShowChart),
        })
        {
            contentIcons.Children.Add(new StackPanel
            {
                Spacing = 4,
                Margin = new Thickness(0, 0, 24, 12),
                Children =
                {
                    new Icon { Data = data, Color = LoamColor.Default, Size = LoamSize.Large },
                    new Text { Text = label, Typo = Typo.Caption },
                },
            });
        }

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                contentIcons,
            },
        };
    }

    private static WrapPanel BuildIconButtonsVariants()
    {
        var wrap = new WrapPanel();
        foreach (var color in new[] { LoamColor.Default, LoamColor.Primary, LoamColor.Secondary, LoamColor.Error })
        {
            wrap.Children.Add(new IconButton { Icon = Icons.Material.Filled.Favorite, Color = color, Margin = new Thickness(0, 0, 4, 0) });
        }

        wrap.Children.Add(new IconButton { Icon = Icons.Material.Filled.Add, Color = LoamColor.Primary, Variant = Variant.Filled, Margin = new Thickness(8, 0, 4, 0) });
        wrap.Children.Add(new IconButton { Icon = Icons.Material.Filled.Edit, Color = LoamColor.Primary, Variant = Variant.Outlined });
        return wrap;
    }

    private static WrapPanel BuildIconButtonsSizes()
    {
        var sizes = new WrapPanel();
        foreach (var size in Sizes)
        {
            sizes.Children.Add(new IconButton
            {
                Icon = Icons.Material.Filled.Settings,
                Color = LoamColor.Primary,
                Variant = Variant.Outlined,
                Size = size,
                Margin = new Thickness(0, 0, 8, 8),
            });
        }

        return sizes;
    }

    private static WrapPanel BuildFabSizes()
    {
        var wrap = new WrapPanel();
        foreach (var size in Sizes)
        {
            wrap.Children.Add(new Fab
            {
                Label = size.ToString(),
                StartIcon = Icons.Material.Filled.Add,
                Color = LoamColor.Primary,
                Size = size,
                Margin = new Thickness(0, 0, 12, 12),
            });
        }

        return wrap;
    }

    private static WrapPanel BuildFabShapes()
    {
        var wrap = new WrapPanel();
        wrap.Children.Add(new Fab { StartIcon = Icons.Material.Filled.Edit, Color = LoamColor.Secondary, Margin = new Thickness(0, 0, 12, 12) });
        wrap.Children.Add(new Fab { Label = "Save", StartIcon = Icons.Material.Filled.Check, Color = LoamColor.Success, Margin = new Thickness(0, 0, 12, 12) });
        return wrap;
    }

    private static StackPanel BuildAvatarVariants()
    {
        var margin = new Thickness(0, 0, 16, 12);
        var variants = new WrapPanel();
        variants.Children.Add(new Avatar { Content = "AB", Color = LoamColor.Primary, Margin = margin });
        variants.Children.Add(new Avatar { Content = "OL", Variant = Variant.Outlined, Color = LoamColor.Primary, Margin = margin });
        variants.Children.Add(new Avatar { Content = "TX", Variant = Variant.Text, Color = LoamColor.Secondary, Margin = margin });
        variants.Children.Add(new Avatar { Content = new Icon { Data = Icons.Material.Filled.Person, Color = LoamColor.Inherit }, Color = LoamColor.Info, Margin = margin });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                variants,
            },
        };
    }

    private static StackPanel BuildAvatarColors()
    {
        var margin = new Thickness(0, 0, 16, 12);
        var colors = new WrapPanel();
        colors.Children.Add(new Avatar { Content = "PR", Color = LoamColor.Primary, Margin = margin });
        colors.Children.Add(new Avatar { Content = "SE", Color = LoamColor.Secondary, Margin = margin });
        colors.Children.Add(new Avatar { Content = "TE", Color = LoamColor.Tertiary, Margin = margin });
        colors.Children.Add(new Avatar { Content = "SU", Color = LoamColor.Success, Margin = margin });
        colors.Children.Add(new Avatar { Content = "WA", Color = LoamColor.Warning, Margin = margin });
        colors.Children.Add(new Avatar { Content = "ER", Color = LoamColor.Error, Margin = margin });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                colors,
            },
        };
    }

    private static StackPanel BuildAvatarShapes()
    {
        var margin = new Thickness(0, 0, 16, 12);
        var shapes = new WrapPanel();
        shapes.Children.Add(new Avatar { Content = "CI", Color = LoamColor.Primary, Margin = margin });
        shapes.Children.Add(new Avatar { Content = "RO", Rounded = true, Color = LoamColor.Secondary, Margin = margin });
        shapes.Children.Add(new Avatar { Content = "SQ", Square = true, Color = LoamColor.Dark, Margin = margin });
        shapes.Children.Add(new Avatar { Content = "OS", Variant = Variant.Outlined, Square = true, Color = LoamColor.Primary, Margin = margin });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                shapes,
            },
        };
    }

    private static StackPanel BuildAvatarSizes()
    {
        var margin = new Thickness(0, 0, 16, 12);
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel();
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new StackPanel
            {
                Spacing = 4,
                Margin = margin,
                Children =
                {
                    new Text { Text = size.ToString(), Typo = Typo.Caption },
                    new Avatar { Content = size.ToString()[0].ToString(), Size = size, Color = LoamColor.Primary },
                },
            });
        }

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                sizes,
            },
        };
    }

    private static StackPanel BuildAvatarGroupOverflow()
    {
        var overflow = new AvatarGroup { Max = 3, Spacing = -10, HorizontalAlignment = HorizontalAlignment.Left };
        overflow.Items.Add(new Avatar { Content = "AB", Color = LoamColor.Primary });
        overflow.Items.Add(new Avatar { Content = "CD", Color = LoamColor.Secondary });
        overflow.Items.Add(new Avatar { Content = "EF", Color = LoamColor.Info });
        overflow.Items.Add(new Avatar { Content = "GH", Color = LoamColor.Success });
        overflow.Items.Add(new Avatar { Content = "IJ", Color = LoamColor.Warning });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                overflow,
            },
        };
    }

    private static StackPanel BuildAvatarGroupCompact()
    {
        var compact = new AvatarGroup { Max = 2, Spacing = -6, HorizontalAlignment = HorizontalAlignment.Left };
        compact.Items.Add(new Avatar { Content = "A", Size = LoamSize.Small, Color = LoamColor.Primary });
        compact.Items.Add(new Avatar { Content = "B", Size = LoamSize.Small, Color = LoamColor.Secondary });
        compact.Items.Add(new Avatar { Content = "C", Size = LoamSize.Small, Color = LoamColor.Info });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                compact,
            },
        };
    }

    private static StackPanel BuildAvatarGroupRelaxed()
    {
        var relaxed = new AvatarGroup { Max = 4, Spacing = 6, HorizontalAlignment = HorizontalAlignment.Left };
        relaxed.Items.Add(new Avatar { Content = "AL", Color = LoamColor.Primary });
        relaxed.Items.Add(new Avatar { Content = "BE", Color = LoamColor.Secondary });
        relaxed.Items.Add(new Avatar { Content = "CY", Color = LoamColor.Tertiary });
        relaxed.Items.Add(new Avatar { Content = "DI", Color = LoamColor.Success });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                relaxed,
            },
        };
    }

    private static StackPanel BuildAvatarGroupRounded()
    {
        var rounded = new AvatarGroup { Max = 3, Spacing = -8, HorizontalAlignment = HorizontalAlignment.Left };
        rounded.Items.Add(new Avatar { Content = "RO", Rounded = true, Color = LoamColor.Primary });
        rounded.Items.Add(new Avatar { Content = "UN", Rounded = true, Color = LoamColor.Secondary });
        rounded.Items.Add(new Avatar { Content = "DE", Rounded = true, Color = LoamColor.Info });
        rounded.Items.Add(new Avatar { Content = "D", Rounded = true, Color = LoamColor.Success });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                rounded,
            },
        };
    }

    private static StackPanel BuildAvatarGroupSquare()
    {
        var square = new AvatarGroup { Max = 3, Spacing = -8, HorizontalAlignment = HorizontalAlignment.Left };
        square.Items.Add(new Avatar { Content = "SQ", Square = true, Color = LoamColor.Primary });
        square.Items.Add(new Avatar { Content = "UA", Square = true, Color = LoamColor.Secondary });
        square.Items.Add(new Avatar { Content = "RE", Square = true, Color = LoamColor.Info });
        square.Items.Add(new Avatar { Content = "D", Square = true, Color = LoamColor.Success });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                square,
            },
        };
    }

    private static StackPanel BuildAvatarGroupSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel();
        foreach (var size in allSizes)
        {
            var group = new AvatarGroup
            {
                Max = 2,
                Spacing = size switch { LoamSize.ExtraLarge => -18, LoamSize.Large => -12, LoamSize.ExtraSmall => -6, _ => -8 },
                Margin = new Thickness(0, 0, 28, 16),
            };
            group.Items.Add(new Avatar { Content = "A", Size = size, Color = LoamColor.Primary });
            group.Items.Add(new Avatar { Content = "B", Size = size, Color = LoamColor.Secondary });
            group.Items.Add(new Avatar { Content = "C", Size = size, Color = LoamColor.Info });

            sizes.Children.Add(new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new Text { Text = size.ToString(), Typo = Typo.Caption },
                    group,
                },
            });
        }

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                sizes,
            },
        };
    }

    private static WrapPanel BuildAvatars()
    {
        var m = new Thickness(0, 0, 12, 8);
        var wrap = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        wrap.Children.Add(new Avatar { Content = "AB", Color = LoamColor.Primary, Margin = m });
        wrap.Children.Add(new Avatar { Content = "CD", Color = LoamColor.Secondary, Margin = m });
        wrap.Children.Add(new Avatar { Content = new Icon { Data = Icons.Material.Filled.Person, Color = LoamColor.Inherit }, Color = LoamColor.Info, Margin = m });
        wrap.Children.Add(new Avatar { Content = "XL", Color = LoamColor.Success, Size = LoamSize.Large, Margin = m });
        wrap.Children.Add(new Avatar { Content = "OL", Variant = Variant.Outlined, Color = LoamColor.Primary, Margin = m });
        wrap.Children.Add(new Avatar { Content = "SQ", Color = LoamColor.Dark, Square = true, Margin = m });

        var avatarGroup = new AvatarGroup { Max = 3, Margin = m };
        avatarGroup.Items.Add(new Avatar { Content = "AB", Color = LoamColor.Primary });
        avatarGroup.Items.Add(new Avatar { Content = "CD", Color = LoamColor.Secondary });
        avatarGroup.Items.Add(new Avatar { Content = "EF", Color = LoamColor.Info });
        avatarGroup.Items.Add(new Avatar { Content = "GH", Color = LoamColor.Success });
        avatarGroup.Items.Add(new Avatar { Content = "IJ", Color = LoamColor.Warning });
        wrap.Children.Add(avatarGroup);
        return wrap;
    }

    private static StackPanel BuildChipsVariants()
    {
        var margin = new Thickness(0, 0, 12, 12);
        var variants = new WrapPanel();
        variants.Children.Add(new Chip { Text = "Filled", Color = LoamColor.Primary, Margin = margin });
        variants.Children.Add(new Chip { Text = "Outlined", Variant = Variant.Outlined, Color = LoamColor.Primary, Margin = margin });
        variants.Children.Add(new Chip { Text = "Text", Variant = Variant.Text, Color = LoamColor.Secondary, Margin = margin });
        variants.Children.Add(new Chip { Text = "Label shape", Label = true, Variant = Variant.Outlined, Color = LoamColor.Tertiary, Margin = margin });
        variants.Children.Add(new Chip { Text = "With icon", Icon = Icons.Material.Filled.Star, Color = LoamColor.Warning, Margin = margin });
        variants.Children.Add(new Chip { Text = "Closeable", Color = LoamColor.Info, Closeable = true, Margin = margin });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                variants,
            },
        };
    }

    private static StackPanel BuildChipsColors()
    {
        var margin = new Thickness(0, 0, 12, 12);
        var colors = new WrapPanel();
        colors.Children.Add(new Chip { Text = "Primary", Color = LoamColor.Primary, Margin = margin });
        colors.Children.Add(new Chip { Text = "Secondary", Color = LoamColor.Secondary, Margin = margin });
        colors.Children.Add(new Chip { Text = "Tertiary", Color = LoamColor.Tertiary, Margin = margin });
        colors.Children.Add(new Chip { Text = "Success", Color = LoamColor.Success, Margin = margin });
        colors.Children.Add(new Chip { Text = "Warning", Color = LoamColor.Warning, Margin = margin });
        colors.Children.Add(new Chip { Text = "Error", Color = LoamColor.Error, Margin = margin });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                colors,
            },
        };
    }

    private static StackPanel BuildChipsSizes()
    {
        var margin = new Thickness(0, 0, 12, 12);
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel();
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new Chip
            {
                Text = size.ToString(),
                Icon = Icons.Material.Filled.Label,
                Size = size,
                Color = LoamColor.Primary,
                Margin = margin,
            });
        }

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                sizes,
            },
        };
    }

    private static StackPanel BuildChipsDisabled()
    {
        var margin = new Thickness(0, 0, 12, 12);
        var disabled = new WrapPanel();
        disabled.Children.Add(new Chip { Text = "Disabled filled", Color = LoamColor.Primary, IsEnabled = false, Margin = margin });
        disabled.Children.Add(new Chip { Text = "Disabled outlined", Variant = Variant.Outlined, Color = LoamColor.Primary, IsEnabled = false, Margin = margin });
        disabled.Children.Add(new Chip { Text = "Disabled closeable", Color = LoamColor.Secondary, Closeable = true, IsEnabled = false, Margin = margin });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildChipSetSingleMandatory()
    {
        var set = new ChipSet { Selectable = true, Mandatory = true, SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var label in new[] { "All", "Active", "Archived", "Draft" })
        {
            set.Items.Add(new Chip { Text = label, Color = LoamColor.Primary, Icon = label == "All" ? Icons.Material.Filled.Check : null });
        }

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                set,
            },
        };
    }

    private static StackPanel BuildChipSetMultiSelect()
    {
        var multi = new ChipSet { Selectable = true, MultiSelect = true, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var label in new[] { "Open", "Assigned", "Overdue" })
        {
            multi.Items.Add(new Chip { Text = label, Color = LoamColor.Secondary, Closeable = label == "Assigned" });
        }

        multi.SelectedIndexes.Add(0);
        multi.SelectedIndexes.Add(2);

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                multi,
            },
        };
    }

    private static StackPanel BuildChipSetOptional()
    {
        var optional = new ChipSet { Selectable = true, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var label in new[] { "Design", "Build", "Verify" })
        {
            optional.Items.Add(new Chip { Text = label, Color = LoamColor.Tertiary, Variant = Variant.Outlined });
        }

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                optional,
            },
        };
    }

    private static StackPanel BuildChipSetDisabled()
    {
        var disabled = new ChipSet
        {
            Selectable = true,
            SelectedIndex = 1,
            IsEnabled = false,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        foreach (var label in new[] { "Email", "Push", "SMS" })
        {
            disabled.Items.Add(new Chip { Text = label, Color = LoamColor.Primary });
        }

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildBadgesValues()
    {
        var margin = new Thickness(0, 8, 32, 12);
        var values = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        values.Children.Add(new Badge { Value = 4, Color = LoamColor.Error, Margin = margin, Content = new Icon { Data = Icons.Material.Filled.Favorite, Color = LoamColor.Default, Size = LoamSize.Large } });
        values.Children.Add(new Badge { Value = 150, Max = 99, Color = LoamColor.Primary, Margin = margin, Content = new Icon { Data = Icons.Material.Filled.Home, Color = LoamColor.Default, Size = LoamSize.Large } });
        values.Children.Add(new Badge { Value = "NEW", Color = LoamColor.Secondary, Margin = margin, Content = new Icon { Data = Icons.Material.Filled.Chat, Color = LoamColor.Default, Size = LoamSize.Large } });
        values.Children.Add(new Badge { Dot = true, Color = LoamColor.Success, Margin = margin, Content = new Icon { Data = Icons.Material.Filled.Person, Color = LoamColor.Default, Size = LoamSize.Large } });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                values,
            },
        };
    }

    private static StackPanel BuildBadgesOrigins()
    {
        var margin = new Thickness(0, 8, 32, 12);
        var origins = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        origins.Children.Add(new Badge { Value = 1, Origin = BadgeOrigin.TopLeft, Color = LoamColor.Primary, Margin = margin, Content = new Avatar { Content = "TL", Color = LoamColor.Secondary } });
        origins.Children.Add(new Badge { Value = 2, Origin = BadgeOrigin.TopRight, Color = LoamColor.Primary, Margin = margin, Content = new Avatar { Content = "TR", Color = LoamColor.Secondary } });
        origins.Children.Add(new Badge { Value = 3, Origin = BadgeOrigin.BottomLeft, Color = LoamColor.Primary, Margin = margin, Content = new Avatar { Content = "BL", Color = LoamColor.Secondary } });
        origins.Children.Add(new Badge { Value = 4, Origin = BadgeOrigin.BottomRight, Color = LoamColor.Primary, Margin = margin, Content = new Avatar { Content = "BR", Color = LoamColor.Secondary } });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                origins,
            },
        };
    }

    private static StackPanel BuildBadgesSurfaceBehavior()
    {
        var margin = new Thickness(0, 8, 32, 12);
        var surfaces = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        surfaces.Children.Add(new Badge { Value = 7, Overlap = true, Color = LoamColor.Secondary, Margin = margin, Content = new Avatar { Content = "AB", Color = LoamColor.Primary } });
        surfaces.Children.Add(new Badge { Value = 12, Bordered = true, Color = LoamColor.Error, Margin = margin, Content = new Avatar { Content = "PL", Color = LoamColor.Primary } });
        surfaces.Children.Add(new Badge { Dot = true, Bordered = true, Color = LoamColor.Warning, Margin = margin, Content = new Avatar { Content = "QA", Color = LoamColor.Success } });
        surfaces.Children.Add(new Badge { Value = 0, Visible = false, Color = LoamColor.Primary, Margin = margin, Content = new Avatar { Content = "HD", Color = LoamColor.Secondary } });

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                surfaces,
            },
        };
    }

    private static Avalonia.Controls.Grid Labeled(string label, Control content)
    {
        var grid = new Avalonia.Controls.Grid { ColumnDefinitions = new ColumnDefinitions("96,*") };
        var caption = new Text { Text = label, Typo = Typo.Subtitle2, VerticalAlignment = VerticalAlignment.Center };
        Avalonia.Controls.Grid.SetColumn(caption, 0);
        Avalonia.Controls.Grid.SetColumn(content, 1);
        grid.Children.Add(caption);
        grid.Children.Add(content);
        return grid;
    }

    private static WrapPanel BuildCheckBoxStates()
    {
        var margin = new Thickness(0, 0, 24, 12);
        var states = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        states.Children.Add(new Loam.Controls.CheckBox { Content = "Checked", Color = LoamColor.Primary, IsChecked = true, Margin = margin });
        states.Children.Add(new Loam.Controls.CheckBox { Content = "Unchecked", Color = LoamColor.Primary, IsChecked = false, Margin = margin });
        states.Children.Add(new Loam.Controls.CheckBox { Content = "Indeterminate", Color = LoamColor.Secondary, IsThreeState = true, IsChecked = null, Margin = margin });
        states.Children.Add(new Loam.Controls.CheckBox { Content = "Success", Color = LoamColor.Success, IsChecked = true, Margin = margin });
        states.Children.Add(new Loam.Controls.CheckBox { Content = "Error", Color = LoamColor.Error, IsChecked = true, Margin = margin });
        return states;
    }

    private static WrapPanel BuildCheckBoxSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new Loam.Controls.CheckBox
            {
                Content = size.ToString(),
                Color = LoamColor.Primary,
                Size = size,
                IsChecked = true,
                Margin = new Thickness(0, 0, 12, 8),
            });
        }

        return sizes;
    }

    private static WrapPanel BuildCheckBoxDisabled()
    {
        var margin = new Thickness(0, 0, 24, 12);
        var disabled = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        disabled.Children.Add(new Loam.Controls.CheckBox { Content = "Disabled checked", IsChecked = true, IsEnabled = false, Margin = margin });
        disabled.Children.Add(new Loam.Controls.CheckBox { Content = "Disabled unchecked", IsChecked = false, IsEnabled = false, Margin = margin });
        disabled.Children.Add(new Loam.Controls.CheckBox { Content = "Disabled mixed", IsThreeState = true, IsChecked = null, IsEnabled = false, Margin = margin });
        return disabled;
    }

    private static WrapPanel BuildSwitchStates()
    {
        var margin = new Thickness(0, 0, 24, 12);
        var states = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        states.Children.Add(new Switch { Content = "On", Color = LoamColor.Primary, IsChecked = true, Margin = margin });
        states.Children.Add(new Switch { Content = "Off", Color = LoamColor.Primary, IsChecked = false, Margin = margin });
        states.Children.Add(new Switch { Content = "Success", Color = LoamColor.Success, IsChecked = true, Margin = margin });
        states.Children.Add(new Switch { Content = "Warning", Color = LoamColor.Warning, IsChecked = true, Margin = margin });
        return states;
    }

    private static WrapPanel BuildSwitchSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new Switch
            {
                Content = size.ToString(),
                Color = LoamColor.Success,
                Size = size,
                IsChecked = true,
                Margin = new Thickness(0, 0, 12, 8),
            });
        }

        return sizes;
    }

    private static WrapPanel BuildSwitchDisabled()
    {
        var margin = new Thickness(0, 0, 24, 12);
        var disabled = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        disabled.Children.Add(new Switch { Content = "Disabled on", IsChecked = true, IsEnabled = false, Margin = margin });
        disabled.Children.Add(new Switch { Content = "Disabled off", IsChecked = false, IsEnabled = false, Margin = margin });
        return disabled;
    }

    private static StackPanel BuildInputs()
    {
        var m = new Thickness(0, 0, 16, 8);

        var checks = new WrapPanel();
        checks.Children.Add(new Loam.Controls.CheckBox { Content = "Primary", Color = LoamColor.Primary, IsChecked = true, Margin = m });
        checks.Children.Add(new Loam.Controls.CheckBox { Content = "Secondary", Color = LoamColor.Secondary, IsChecked = true, Margin = m });
        checks.Children.Add(new Loam.Controls.CheckBox { Content = "Unchecked", Color = LoamColor.Primary, Margin = m });
        checks.Children.Add(new Loam.Controls.CheckBox { Content = "Disabled", IsChecked = true, IsEnabled = false, Margin = m });

        var switches = new WrapPanel();
        switches.Children.Add(new Switch { Content = "On", Color = LoamColor.Primary, IsChecked = true, Margin = m });
        switches.Children.Add(new Switch { Content = "Success", Color = LoamColor.Success, IsChecked = true, Margin = m });
        switches.Children.Add(new Switch { Content = "Off", Color = LoamColor.Primary, Margin = m });
        switches.Children.Add(new Switch { Content = "Disabled", IsChecked = true, IsEnabled = false, Margin = m });

        return new StackPanel { Spacing = 12, Children = { Labeled("Checkboxes", checks), Labeled("Switches", switches) } };
    }

    private static StackPanel BuildFieldVariants()
    {
        static TextBox InnerTextBox(string text, string? watermark = null) =>
            FieldEditor.MakeChromeless(new TextBox
            {
                Text = text,
                PlaceholderText = watermark,
                VerticalContentAlignment = VerticalAlignment.Center,
            });

        var phone = new Field
        {
            Label = "Phone",
            HelperText = "Custom phone entry inside shared field chrome.",
            Content = InnerTextBox("(555) 123-4567", "(555) 123-4567"),
            StartAdornment = new TextBlock { Text = "+1" },
            EndAdornment = new Icon { Data = Icons.Material.Filled.Check, Color = LoamColor.Success, Size = LoamSize.Small },
        };

        var colorSwatch = new Border
        {
            Width = 24,
            Height = 24,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(Color.Parse("#594AE2")),
        };
        var color = new Field
        {
            Label = "Accent",
            Variant = Variant.Filled,
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    colorSwatch,
                    new Text { Text = "#594AE2", VerticalAlignment = VerticalAlignment.Center },
                },
            },
        };

        var options = new Field
        {
            Label = "Notification channels",
            InnerPadding = false,
            Variant = Variant.Outlined,
            Content = new StackPanel
            {
                Margin = new Thickness(8, 6),
                Spacing = 4,
                Children =
                {
                    new Loam.Controls.CheckBox { Content = "Email", IsChecked = true },
                    new Loam.Controls.CheckBox { Content = "SMS" },
                    new Loam.Controls.CheckBox { Content = "Push", IsChecked = true },
                },
            },
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 380,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { phone, color, options },
        };
    }

    private static StackPanel BuildFieldUnderlineAndValidation()
    {
        static TextBox InnerTextBox(string text, string? watermark = null) =>
            FieldEditor.MakeChromeless(new TextBox
            {
                Text = text,
                PlaceholderText = watermark,
                VerticalContentAlignment = VerticalAlignment.Center,
            });

        var search = new Field
        {
            Label = "Quick filter",
            Variant = Variant.Text,
            HelperText = "Underline variant for custom editors.",
            Content = InnerTextBox("Component audit"),
            EndAdornment = new Icon { Data = Icons.Material.Filled.Search, Size = LoamSize.Small },
        };

        var invalid = new Field
        {
            Label = "Custom amount",
            Error = true,
            ErrorText = "Enter a value greater than zero",
            StartAdornment = new TextBlock { Text = "$" },
            Content = InnerTextBox("0"),
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 380,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { search, invalid },
        };
    }

    private static Field BuildFieldDisabled()
    {
        return new Field
        {
            Label = "Read-only token",
            IsEnabled = false,
            HelperText = "Disabled custom field shell.",
            Content = new Text { Text = "LOAM-2.0" },
            MaxWidth = 380,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
    }

    private static StackPanel BuildMenu()
    {
        var primary = new Loam.Controls.Menu { Content = "Account", Variant = Variant.Filled, Color = LoamColor.Primary, MenuWidth = 220 };
        primary.Items.Add(new Loam.Controls.MenuItem { Text = "Profile", Icon = Icons.Material.Filled.Person });
        primary.Items.Add(new Loam.Controls.MenuItem { Text = "Settings", Icon = Icons.Material.Filled.Settings, ShortcutText = "Ctrl+," });
        primary.Items.Add(new Loam.Controls.MenuItem { IsDivider = true });
        primary.Items.Add(new Loam.Controls.MenuItem { Text = "Billing disabled", Icon = Icons.Material.Filled.VisibilityOff, IsEnabled = false });
        primary.Items.Add(new Loam.Controls.MenuItem { Text = "Sign out", Icon = Icons.Material.Filled.ArrowBack, ShortcutText = "Shift+Q" });

        var danger = new Loam.Controls.Menu { Content = "Danger", Variant = Variant.Outlined, Color = LoamColor.Error };
        danger.Items.Add(new Loam.Controls.MenuItem { Text = "Archive", Icon = Icons.Material.Filled.Check });
        danger.Items.Add(new Loam.Controls.MenuItem { Text = "Delete", Icon = Icons.Material.Filled.Delete });

        var persistent = new Loam.Controls.Menu
        {
            Content = "Keep open",
            Variant = Variant.Outlined,
            Color = LoamColor.Secondary,
            MenuWidth = 200,
            CloseOnItemClick = false,
        };
        persistent.Items.Add(new Loam.Controls.MenuItem { Text = "Mark reviewed", Icon = Icons.Material.Filled.Check, ShortcutText = "R" });
        persistent.Items.Add(new Loam.Controls.MenuItem { Text = "Schedule follow-up", Icon = Icons.Material.Filled.Schedule, ShortcutText = "S" });

        var disabled = new Loam.Controls.Menu
        {
            Content = "Disabled",
            Variant = Variant.Outlined,
            IsEnabled = false,
            MenuWidth = 180,
        };
        disabled.Items.Add(new Loam.Controls.MenuItem { Text = "Unavailable", Icon = Icons.Material.Filled.VisibilityOff });

        return new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Text { Text = "Triggers", Typo = Typo.TitleSmall },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children = { primary, danger, persistent, disabled },
                },
                new Alert
                {
                    Color = LoamColor.Info,
                    Content = "Menus expose tokenized rows, dividers, trailing shortcuts, disabled states, keyboard navigation, and public open/close APIs.",
                },
            },
        };
    }

    private static StackPanel BuildTabsDefault()
    {
        var tabs = new Tabs { Color = LoamColor.Primary, Width = 520, HorizontalAlignment = HorizontalAlignment.Left };
        tabs.Items.Add(new Loam.Controls.TabItem("Overview", new Text { Text = "Overview content.", Typo = Typo.Body1, Margin = new Thickness(0, 8) }));
        tabs.Items.Add(new Loam.Controls.TabItem("Details", new Text { Text = "Details content.", Typo = Typo.Body1, Margin = new Thickness(0, 8) }));
        tabs.Items.Add(new Loam.Controls.TabItem("Settings", new Text { Text = "Settings content.", Typo = Typo.Body1, Margin = new Thickness(0, 8) }));

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                tabs,
            },
        };
    }

    private static StackPanel BuildTabsSecondarySelected()
    {
        var secondary = new Tabs { Color = LoamColor.Secondary, Width = 420, HorizontalAlignment = HorizontalAlignment.Left };
        secondary.Items.Add(new Loam.Controls.TabItem("Open", new Text { Text = "Open items." }));
        secondary.Items.Add(new Loam.Controls.TabItem("Assigned", new Text { Text = "Assigned items." }));
        secondary.Items.Add(new Loam.Controls.TabItem("Done", new Text { Text = "Completed items." }));
        secondary.SelectedIndex = 1;

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                secondary,
            },
        };
    }

    private static StackPanel BuildTabsClampedSelectedIndex()
    {
        var clamped = new Tabs { Color = LoamColor.Tertiary, Width = 420, HorizontalAlignment = HorizontalAlignment.Left };
        clamped.Items.Add(new Loam.Controls.TabItem("First", new Text { Text = "Invalid SelectedIndex clamps into range." }));
        clamped.Items.Add(new Loam.Controls.TabItem("Last", new Text { Text = "The last available tab is selected." }));
        clamped.SelectedIndex = 99;

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                clamped,
            },
        };
    }

    private static StackPanel BuildTabsDisabled()
    {
        var disabled = new Tabs { Color = LoamColor.Primary, Width = 420, HorizontalAlignment = HorizontalAlignment.Left, IsEnabled = false };
        disabled.Items.Add(new Loam.Controls.TabItem("Queued", new Text { Text = "Disabled tab strips keep content stable." }));
        disabled.Items.Add(new Loam.Controls.TabItem("Paused", new Text { Text = "Header activation is suppressed." }));
        disabled.SelectedIndex = 1;

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildTabsEmpty()
    {
        var empty = new Tabs { Width = 420, HorizontalAlignment = HorizontalAlignment.Left };

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                empty,
            },
        };
    }

    private static StackPanel BuildTabsMenu()
    {
        var tabs = new Tabs { Color = LoamColor.Primary };
        tabs.Items.Add(new Loam.Controls.TabItem("Overview", new Text { Text = "Overview content.", Typo = Typo.Body1, Margin = new Thickness(0, 8) }));
        tabs.Items.Add(new Loam.Controls.TabItem("Details", new Text { Text = "Details content.", Typo = Typo.Body1, Margin = new Thickness(0, 8) }));
        tabs.Items.Add(new Loam.Controls.TabItem("Settings", new Text { Text = "Settings content.", Typo = Typo.Body1, Margin = new Thickness(0, 8) }));

        var menu = new Loam.Controls.Menu { Content = "Open menu", Variant = Variant.Filled, Color = LoamColor.Primary };
        menu.Items.Add(new Loam.Controls.MenuItem { Text = "Profile", Icon = Icons.Material.Filled.Person });
        menu.Items.Add(new Loam.Controls.MenuItem { Text = "Settings", Icon = Icons.Material.Filled.Settings });
        menu.Items.Add(new Loam.Controls.MenuItem { Text = "Delete", Icon = Icons.Material.Filled.Delete });

        var tooltipButton = new LoamButton { Content = "Hover me", Variant = Variant.Outlined, Color = LoamColor.Primary };
        Tooltip.Set(tooltipButton, "A helpful Loam tooltip");

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { menu, tooltipButton } };
        return new StackPanel { Spacing = 16, Children = { tabs, row } };
    }

    private static StackPanel BuildDialogService()
    {
        var confirm = new LoamButton { Content = "Confirm delete", Variant = Variant.Filled, Color = LoamColor.Primary };
        confirm.Click += async (sender, _) =>
        {
            if (sender is Control control)
            {
                var confirmed = await DialogService.For(control)
                    .ConfirmAsync("Delete item?", "This action cannot be undone.", "Delete", "Cancel");
                SnackbarService.For(control).Add(confirmed ? "Item deleted" : "Cancelled", confirmed ? LoamColor.Error : LoamColor.Default);
            }
        };

        var messageBox = new LoamButton { Content = "Message box", Variant = Variant.Outlined, Color = LoamColor.Secondary };
        messageBox.Click += async (sender, _) =>
        {
            if (sender is Control control)
            {
                var answer = await DialogService.For(control)
                    .MessageBoxAsync("Save changes?", "Your changes will be lost otherwise.", "Save", "Discard", "Cancel");
                var text = answer switch { true => "Saved", false => "Discarded", _ => "Cancelled" };
                SnackbarService.For(control).Add(text, LoamColor.Info);
            }
        };

        var custom = new LoamButton { Content = "Custom dialog", Variant = Variant.Outlined, Color = LoamColor.Primary };
        custom.Click += async (sender, _) =>
        {
            if (sender is Control control)
            {
                var result = await DialogService.For(control).ShowAsync(
                    "Edit project",
                    instance =>
                    {
                        var name = new TextField
                        {
                            Label = "Project",
                            Text = "Project Loam",
                            HelperText = "Custom content can use regular Loam controls.",
                            Width = 320,
                        };
                        var cancel = new LoamButton { Content = "Cancel", Variant = Variant.Text, Color = LoamColor.Primary };
                        cancel.Click += (_, _) => instance.Cancel();
                        var save = new LoamButton { Content = "Save", Variant = Variant.Text, Color = LoamColor.Primary };
                        save.Click += (_, _) => instance.Ok(name.Text);

                        return new StackPanel
                        {
                            Spacing = 20,
                            Children =
                            {
                                name,
                                new StackPanel
                                {
                                    Orientation = Orientation.Horizontal,
                                    Spacing = 8,
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    Children = { cancel, save },
                                },
                            },
                        };
                    },
                    new DialogOptions
                    {
                        Width = 360,
                        MaxWidth = 420,
                        MinWidth = 320,
                        MaxHeight = 480,
                        Margin = new Thickness(24),
                        Padding = new Thickness(24),
                        DismissOnEscape = true,
                        AutoFocus = true,
                    });
                SnackbarService.For(control).Add(result.Canceled ? "Edit cancelled" : $"Saved {result.DataAs<string>()}", LoamColor.Info);
            }
        };

        var persistent = new LoamButton { Content = "Persistent dialog", Variant = Variant.Text, Color = LoamColor.Secondary };
        persistent.Click += async (sender, _) =>
        {
            if (sender is Control control)
            {
                await DialogService.For(control).ShowAsync(
                    "Review required",
                    instance =>
                    {
                        var close = new LoamButton { Content = "Close", Variant = Variant.Text, Color = LoamColor.Primary };
                        close.Click += (_, _) => instance.Cancel();
                        return new StackPanel
                        {
                            Spacing = 20,
                            Children =
                            {
                                new Text { Text = "Backdrop and Escape dismissal are disabled for this dialog.", Typo = Typo.Body1 },
                                new StackPanel
                                {
                                    Orientation = Orientation.Horizontal,
                                    Spacing = 8,
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    Children = { close },
                                },
                            },
                        };
                    },
                    new DialogOptions
                    {
                        Width = 360,
                        MaxWidth = 420,
                        DismissOnScrimClick = false,
                        DismissOnEscape = false,
                    });
            }
        };

        var actions = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var button in new[] { confirm, messageBox, custom, persistent })
        {
            button.Margin = new Thickness(0, 0, 10, 10);
            actions.Children.Add(button);
        }

        return new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                actions,
                new Alert { Color = LoamColor.Info, Content = "Dialogs restore focus, support Escape/backdrop options, and return typed results." },
            },
        };
    }

    private static StackPanel BuildSnackbarService()
    {
        var success = new LoamButton { Content = "Show snackbar", Variant = Variant.Filled, Color = LoamColor.Success };
        success.Click += (sender, _) =>
        {
            if (sender is Control control)
            {
                SnackbarService.For(control).Add("Saved successfully", LoamColor.Success);
            }
        };

        var action = new LoamButton { Content = "Action snackbar", Variant = Variant.Outlined, Color = LoamColor.Info };
        action.Click += (sender, _) =>
        {
            if (sender is Control control)
            {
                var snackbar = SnackbarService.For(control);
                snackbar.Position = SnackbarPosition.BottomCenter;
                snackbar.Add(new SnackbarOptions("Item archived")
                {
                    Severity = LoamColor.Info,
                    ActionText = "Undo",
                    DismissText = "Dismiss",
                    Position = SnackbarPosition.BottomCenter,
                    Action = () => snackbar.Add("Archive undone", LoamColor.Success),
                    Duration = TimeSpan.FromSeconds(8),
                });
            }
        };

        var persistent = new LoamButton { Content = "Persistent snackbar", Variant = Variant.Outlined, Color = LoamColor.Warning };
        persistent.Click += (sender, _) =>
        {
            if (sender is Control control)
            {
                SnackbarService.For(control).Add(new SnackbarOptions("Waiting for approval")
                {
                    Severity = LoamColor.Warning,
                    DismissText = "Close",
                    Duration = Timeout.InfiniteTimeSpan,
                    Position = SnackbarPosition.TopCenter,
                });
            }
        };

        var queue = new LoamButton { Content = "Queue limit", Variant = Variant.Text, Color = LoamColor.Secondary };
        queue.Click += (sender, _) =>
        {
            if (sender is Control control)
            {
                var snackbar = SnackbarService.For(control);
                snackbar.Position = SnackbarPosition.BottomLeft;
                for (var i = 1; i <= 4; i++)
                {
                    snackbar.Add(new SnackbarOptions($"Queue item {i}")
                    {
                        Duration = Timeout.InfiniteTimeSpan,
                        MaxVisible = 2,
                        Position = SnackbarPosition.BottomLeft,
                    });
                }
            }
        };

        var actions = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var button in new[] { success, action, persistent, queue })
        {
            button.Margin = new Thickness(0, 0, 10, 10);
            actions.Children.Add(button);
        }

        return new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                actions,
                new Alert { Color = LoamColor.Success, Content = "Snackbar actions, dismiss buttons, placement, persistent duration, and queue limits are component options." },
            },
        };
    }

    private static StackPanel BuildOverlays()
    {
        var openDialog = new LoamButton { Content = "Open dialog", Variant = Variant.Filled, Color = LoamColor.Primary };
        openDialog.Click += async (sender, _) =>
        {
            if (sender is Control control)
            {
                var confirmed = await DialogService.For(control)
                    .ConfirmAsync("Delete item?", "This action cannot be undone.", "Delete", "Cancel");
                SnackbarService.For(control).Add(confirmed ? "Item deleted" : "Cancelled", confirmed ? LoamColor.Error : LoamColor.Default);
            }
        };

        var showSnackbar = new LoamButton { Content = "Show snackbar", Variant = Variant.Outlined, Color = LoamColor.Primary };
        showSnackbar.Click += (sender, _) =>
        {
            if (sender is Control control)
            {
                SnackbarService.For(control).Add("Hello from Loam!", LoamColor.Success);
            }
        };

        var actionSnackbar = new LoamButton { Content = "Show action snackbar", Variant = Variant.Outlined, Color = LoamColor.Info };
        actionSnackbar.Click += (sender, _) =>
        {
            if (sender is Control control)
            {
                var snackbar = SnackbarService.For(control);
                snackbar.Add(new SnackbarOptions("Item archived")
                {
                    Severity = LoamColor.Info,
                    ActionText = "Undo",
                    Action = () => snackbar.Add("Archive undone", LoamColor.Success),
                    Duration = TimeSpan.FromSeconds(8),
                });
            }
        };

        var messageBox = new LoamButton { Content = "Message box", Variant = Variant.Outlined, Color = LoamColor.Secondary };
        messageBox.Click += async (sender, _) =>
        {
            if (sender is Control control)
            {
                var answer = await DialogService.For(control)
                    .MessageBoxAsync("Save changes?", "Your changes will be lost otherwise.", "Save", "Discard", "Cancel");
                var text = answer switch { true => "Saved", false => "Discarded", _ => "Cancelled" };
                SnackbarService.For(control).Add(text, LoamColor.Info);
            }
        };

        foreach (var button in new[] { openDialog, showSnackbar, actionSnackbar, messageBox })
        {
            button.Margin = new Thickness(0, 0, 8, 8);
        }

        var actions = new WrapPanel { Orientation = Orientation.Horizontal };
        actions.Children.Add(openDialog);
        actions.Children.Add(showSnackbar);
        actions.Children.Add(actionSnackbar);
        actions.Children.Add(messageBox);

        return new StackPanel
        {
            Spacing = 12,
            Children =
            {
                actions,
                new Alert { Color = LoamColor.Info, Content = "Snackbar actions stay keyboard-accessible and dialogs restore focus after dismissal." },
            },
        };
    }

    private static WrapPanel BuildRadioStates()
    {
        var margin = new Thickness(0, 0, 24, 12);
        var states = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        states.Children.Add(new Radio { GroupName = "radio-states-a", Content = "Selected", Color = LoamColor.Primary, IsChecked = true, Margin = margin });
        states.Children.Add(new Radio { GroupName = "radio-states-b", Content = "Unselected", Color = LoamColor.Primary, IsChecked = false, Margin = margin });
        states.Children.Add(new Radio { GroupName = "radio-states-c", Content = "Secondary", Color = LoamColor.Secondary, IsChecked = true, Margin = margin });
        states.Children.Add(new Radio { GroupName = "radio-states-d", Content = "Error", Color = LoamColor.Error, IsChecked = true, Margin = margin });
        return states;
    }

    private static WrapPanel BuildRadioSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new Radio
            {
                GroupName = $"radio-size-{size}",
                Content = size.ToString(),
                Color = LoamColor.Primary,
                Size = size,
                IsChecked = true,
                Margin = new Thickness(0, 0, 12, 8),
            });
        }

        return sizes;
    }

    private static WrapPanel BuildRadioDisabled()
    {
        var margin = new Thickness(0, 0, 24, 12);
        var disabled = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        disabled.Children.Add(new Radio { GroupName = "radio-disabled-a", Content = "Disabled selected", IsChecked = true, IsEnabled = false, Margin = margin });
        disabled.Children.Add(new Radio { GroupName = "radio-disabled-b", Content = "Disabled unselected", IsChecked = false, IsEnabled = false, Margin = margin });
        return disabled;
    }

    private static StackPanel BuildRadioGroupVertical()
    {
        var shipping = new RadioGroup
        {
            Value = "express",
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new Radio { Value = "standard", Content = "Standard", Color = LoamColor.Primary },
                    new Radio { Value = "express", Content = "Express", Color = LoamColor.Primary },
                    new Radio { Value = "pickup", Content = "Pickup", Color = LoamColor.Primary },
                },
            },
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                shipping,
            },
        };
    }

    private static StackPanel BuildRadioGroupHorizontal()
    {
        var notification = new RadioGroup
        {
            Value = "push",
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 16,
                Children =
                {
                    new Radio { Value = "push", Content = "Push", Color = LoamColor.Secondary },
                    new Radio { Value = "email", Content = "Email", Color = LoamColor.Secondary },
                    new Radio { Value = "sms", Content = "SMS", Color = LoamColor.Secondary },
                },
            },
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                notification,
            },
        };
    }

    private static StackPanel BuildRadioGroupDisabled()
    {
        var disabled = new RadioGroup
        {
            Value = "email",
            IsEnabled = false,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 16,
                Children =
                {
                    new Radio { Value = "email", Content = "Email" },
                    new Radio { Value = "sms", Content = "SMS" },
                },
            },
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildSliderDefaultRange()
    {
        var primary = new Loam.Controls.Slider { Value = 40, Width = 360, Color = LoamColor.Primary };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                primary,
            },
        };
    }

    private static StackPanel BuildSliderCustomRange()
    {
        var customRange = new Loam.Controls.Slider { Value = 72, Minimum = 20, Maximum = 120, Width = 360, Color = LoamColor.Secondary };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                customRange,
            },
        };
    }

    private static StackPanel BuildSliderColorStates()
    {
        var warning = new Loam.Controls.Slider { Value = 30, Width = 360, Color = LoamColor.Warning };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                warning,
            },
        };
    }

    private static StackPanel BuildSliderZeroValue()
    {
        var zero = new Loam.Controls.Slider { Value = 0, Width = 360, Color = LoamColor.Primary };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                zero,
            },
        };
    }

    private static StackPanel BuildSliderDisabled()
    {
        var disabled = new Loam.Controls.Slider { Value = 70, Width = 360, IsEnabled = false };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildRadioSlider()
    {
        var group = new RadioGroup
        {
            Value = "b",
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new Radio { Value = "a", Content = "One", Color = LoamColor.Primary },
                    new Radio { Value = "b", Content = "Two", Color = LoamColor.Primary },
                    new Radio { Value = "c", Content = "Three", Color = LoamColor.Primary },
                },
            },
        };

        var slider = new Loam.Controls.Slider { Value = 40, Width = 280, HorizontalAlignment = HorizontalAlignment.Left };

        var disabledRadio = new RadioGroup
        {
            Value = "email",
            IsEnabled = false,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new Radio { Value = "email", Content = "Email", IsChecked = true },
                    new Radio { Value = "sms", Content = "SMS" },
                },
            },
        };

        var disabledSlider = new Loam.Controls.Slider
        {
            Value = 70,
            Width = 280,
            IsEnabled = false,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        return new StackPanel
        {
            Spacing = 16,
            Children =
            {
                Labeled("Radio", group),
                Labeled("Slider", slider),
                Labeled("Disabled", new StackPanel { Spacing = 10, Children = { disabledRadio, disabledSlider } }),
            },
        };
    }

    private static WrapPanel BuildTextFieldVariants()
    {
        var itemMargin = new Thickness(0, 0, 18, 18);
        return new WrapPanel
        {
            MaxWidth = 700,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new TextField
                {
                    Label = "Outlined",
                    Placeholder = "Type here...",
                    HelperText = "Helper text stays below the field.",
                    Variant = Variant.Outlined,
                    Width = 320,
                    Margin = itemMargin,
                },
                new TextField
                {
                    Label = "Filled",
                    Variant = Variant.Filled,
                    Text = "Prefilled value",
                    HelperText = "Filled container style.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new TextField
                {
                    Label = "Text / underline",
                    Variant = Variant.Text,
                    Text = "Component audit",
                    HelperText = "Underline field style.",
                    Width = 320,
                    Margin = itemMargin,
                },
            },
        };
    }

    private static WrapPanel BuildTextFieldAdornments()
    {
        var itemMargin = new Thickness(0, 0, 18, 18);
        return new WrapPanel
        {
            MaxWidth = 700,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new TextField
                {
                    Label = "Budget",
                    StartAdornment = new TextBlock { Text = "$" },
                    EndAdornment = new TextBlock { Text = "USD" },
                    Text = "2,500",
                    HelperText = "Start and end adornments.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new TextField
                {
                    Label = "Always floated",
                    ShrinkLabel = true,
                    Placeholder = "Optional note",
                    HelperText = "Label remains above an empty field.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new TextField
                {
                    Label = "Read-only",
                    Text = "Generated by workflow",
                    ReadOnly = true,
                    HelperText = "Focusable but not editable.",
                    Width = 320,
                    Margin = itemMargin,
                },
            },
        };
    }

    private static WrapPanel BuildTextFieldStates()
    {
        var itemMargin = new Thickness(0, 0, 18, 18);
        return new WrapPanel
        {
            MaxWidth = 700,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new TextField
                {
                    Label = "Email",
                    Variant = Variant.Outlined,
                    Text = "not-an-email",
                    Required = true,
                    Error = true,
                    ErrorText = "Enter a valid email.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new TextField
                {
                    Label = "Disabled",
                    Text = "Archived project",
                    IsEnabled = false,
                    HelperText = "Unavailable while archived.",
                    Width = 320,
                    Margin = itemMargin,
                },
            },
        };
    }

    private static WrapPanel BuildNumericFieldVariants()
    {
        var itemMargin = new Thickness(0, 0, 18, 18);
        return new WrapPanel
        {
            MaxWidth = 700,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new NumericField
                {
                    Label = "Quantity",
                    Minimum = 0,
                    Maximum = 99,
                    Value = 3,
                    HelperText = "Whole-number bounds.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new NumericField
                {
                    Label = "Price",
                    Variant = Variant.Filled,
                    Minimum = 0,
                    Step = 0.5,
                    Value = 9.5,
                    Format = "0.00",
                    HelperText = "Formatted filled field.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new NumericField
                {
                    Label = "Text / underline",
                    Variant = Variant.Text,
                    Minimum = 0,
                    Maximum = 100,
                    Step = 5,
                    Value = 45,
                    HelperText = "Underline numeric entry.",
                    Width = 320,
                    Margin = itemMargin,
                },
            },
        };
    }

    private static WrapPanel BuildNumericFieldStepsAndBounds()
    {
        var itemMargin = new Thickness(0, 0, 18, 18);
        return new WrapPanel
        {
            MaxWidth = 700,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new NumericField
                {
                    Label = "Step 0.25",
                    Minimum = 0,
                    Maximum = 2,
                    Step = 0.25,
                    Value = 1.25,
                    Format = "0.00",
                    Color = LoamColor.Secondary,
                    HelperText = "Fine-grained spinner step.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new NumericField
                {
                    Label = "Maximum",
                    Minimum = 0,
                    Maximum = 10,
                    Value = 10,
                    HelperText = "At maximum bound.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new NumericField
                {
                    Label = "Temperature",
                    Minimum = -40,
                    Maximum = 120,
                    Step = 0.5,
                    Value = -3.5,
                    Format = "0.0",
                    HelperText = "Negative values with decimal step.",
                    Width = 320,
                    Margin = itemMargin,
                },
            },
        };
    }

    private static WrapPanel BuildNumericFieldStates()
    {
        var itemMargin = new Thickness(0, 0, 18, 18);
        return new WrapPanel
        {
            MaxWidth = 700,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new NumericField
                {
                    Label = "Amount",
                    Value = 0,
                    Error = true,
                    ErrorText = "Enter a value greater than zero.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new NumericField
                {
                    Label = "Disabled",
                    Value = 12,
                    IsEnabled = false,
                    HelperText = "Unavailable while archived.",
                    Width = 320,
                    Margin = itemMargin,
                },
            },
        };
    }

    private static WrapPanel BuildMaskedTextFieldVariants()
    {
        var itemMargin = new Thickness(0, 0, 18, 18);
        return new WrapPanel
        {
            MaxWidth = 700,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new MaskedTextField
                {
                    Label = "Phone",
                    Pattern = "(###) ###-####",
                    Placeholder = "(555) 123-4567",
                    HelperText = "Digit mask with inserted separators.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new MaskedTextField
                {
                    Label = "Postal code",
                    Pattern = "#####",
                    Text = "90210",
                    HelperText = "Five digits.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new MaskedTextField
                {
                    Label = "Date",
                    Pattern = "##/##/####",
                    Text = "06052026",
                    Variant = Variant.Filled,
                    HelperText = "Filled date mask.",
                    Width = 320,
                    Margin = itemMargin,
                },
            },
        };
    }

    private static WrapPanel BuildMaskedTextFieldMaskTypes()
    {
        var itemMargin = new Thickness(0, 0, 18, 18);
        return new WrapPanel
        {
            MaxWidth = 700,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new MaskedTextField
                {
                    Label = "Access code",
                    Pattern = "AAA-###",
                    Text = "abc123",
                    Variant = Variant.Text,
                    HelperText = "Letters and numbers.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new MaskedTextField
                {
                    Label = "Product key",
                    Pattern = "***-***",
                    Text = "A7Z9Q1",
                    Color = LoamColor.Secondary,
                    HelperText = "Alphanumeric mask.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new MaskedTextField
                {
                    Label = "Partial phone",
                    Pattern = "(###) ###-####",
                    Text = "55512",
                    HelperText = "Partial input keeps typed progress.",
                    Width = 320,
                    Margin = itemMargin,
                },
            },
        };
    }

    private static WrapPanel BuildMaskedTextFieldStates()
    {
        var itemMargin = new Thickness(0, 0, 18, 18);
        return new WrapPanel
        {
            MaxWidth = 700,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new MaskedTextField
                {
                    Label = "Invalid",
                    Pattern = "###-###",
                    Text = "12",
                    Error = true,
                    ErrorText = "Complete the pattern.",
                    Width = 320,
                    Margin = itemMargin,
                },
                new MaskedTextField
                {
                    Label = "Disabled",
                    Pattern = "#####",
                    Text = "10101",
                    IsEnabled = false,
                    HelperText = "Unavailable while archived.",
                    Width = 320,
                    Margin = itemMargin,
                },
            },
        };
    }

    private static Autocomplete BuildAutocompleteFiltered()
    {
        var fruit = new Autocomplete { Label = "Fruit", Placeholder = "Start typing...", HelperText = "Suggestions use the same field chrome", MaxWidth = 360, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var name in new[] { "Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grape", "Mango", "Orange", "Peach", "Pineapple" })
        {
            fruit.Items.Add(name);
        }

        fruit.SearchFunc = text => fruit.Items.Where(item => item.Contains(text ?? "", StringComparison.OrdinalIgnoreCase));
        return fruit;
    }

    private static Autocomplete BuildAutocompletePrefilled()
    {
        var country = new Autocomplete { Label = "Country", Placeholder = "Type a country", Value = "Sweden", Variant = Variant.Filled, MaxWidth = 360, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var name in new[] { "Denmark", "Finland", "Norway", "Sweden" })
        {
            country.Items.Add(name);
        }

        country.SearchFunc = text => country.Items.Where(item => item.Contains(text ?? "", StringComparison.OrdinalIgnoreCase));
        return country;
    }

    private static StackPanel BuildTextFields()
    {
        var stack = new StackPanel { Spacing = 12, MaxWidth = 320, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new TextField { Label = "Project", Placeholder = "Type here…", HelperText = "We never share this", Variant = Variant.Outlined });
        stack.Children.Add(new TextField { Label = "Filled", Variant = Variant.Filled, Text = "Prefilled value" });
        stack.Children.Add(new TextField { Label = "Search", Variant = Variant.Text, Text = "Component audit" });
        stack.Children.Add(new TextField { Label = "Budget", StartAdornment = new TextBlock { Text = "$" }, EndAdornment = new TextBlock { Text = "USD" }, Text = "2,500" });
        stack.Children.Add(new TextField { Label = "Email", Variant = Variant.Outlined, Text = "not-an-email", Error = true, ErrorText = "Enter a valid email" });
        stack.Children.Add(new NumericField { Label = "Quantity", Minimum = 0, Maximum = 99, Value = 3, HelperText = "0-99" });
        stack.Children.Add(new NumericField { Label = "Price", Variant = Variant.Filled, Minimum = 0, Step = 0.5, Value = 9.5, Format = "0.00" });
        stack.Children.Add(new MaskedTextField { Label = "Phone", Pattern = "(###) ###-####", Placeholder = "(555) 123-4567" });

        var fruit = new Autocomplete { Label = "Fruit", Placeholder = "Start typing…", HelperText = "Suggestions use the same field chrome" };
        foreach (var name in new[] { "Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grape", "Mango", "Orange", "Peach", "Pineapple" })
        {
            fruit.Items.Add(name);
        }

        fruit.SearchFunc = text => fruit.Items.Where(item => item.Contains(text ?? "", StringComparison.OrdinalIgnoreCase));
        stack.Children.Add(fruit);
        return stack;
    }

    private static StackPanel BuildToggleGroupSelected()
    {
        var group = new ToggleGroup { HorizontalAlignment = HorizontalAlignment.Left, SelectedValue = "week" };
        group.Items.Add(new ToggleItem("Day", "day"));
        group.Items.Add(new ToggleItem("Week", "week"));
        group.Items.Add(new ToggleItem("Month", "month"));

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                group,
            },
        };
    }

    private static StackPanel BuildToggleGroupColor()
    {
        var priority = new ToggleGroup
        {
            Color = LoamColor.Secondary,
            HorizontalAlignment = HorizontalAlignment.Left,
            SelectedValue = "high",
        };
        priority.Items.Add(new ToggleItem("Low", "low"));
        priority.Items.Add(new ToggleItem("High", "high"));
        priority.Items.Add(new ToggleItem("Urgent", "urgent"));

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                priority,
            },
        };
    }

    private static StackPanel BuildToggleGroupNoSelection()
    {
        var noSelection = new ToggleGroup { HorizontalAlignment = HorizontalAlignment.Left };
        noSelection.Items.Add(new ToggleItem("Draft", "draft"));
        noSelection.Items.Add(new ToggleItem("Review", "review"));
        noSelection.Items.Add(new ToggleItem("Done", "done"));

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                noSelection,
            },
        };
    }

    private static StackPanel BuildToggleGroupSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        static ToggleGroup CreateGroup(LoamSize size, object? selectedValue)
        {
            var result = new ToggleGroup { Size = size, HorizontalAlignment = HorizontalAlignment.Left, SelectedValue = selectedValue };
            result.Items.Add(new ToggleItem("Day", "day"));
            result.Items.Add(new ToggleItem("Week", "week"));
            result.Items.Add(new ToggleItem("Month", "month"));
            return result;
        }

        var sizes = new WrapPanel();
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new StackPanel
            {
                Spacing = 4,
                Margin = new Thickness(0, 0, 24, 12),
                Children =
                {
                    new Text { Text = size.ToString(), Typo = Typo.Caption },
                    CreateGroup(size, "week"),
                },
            });
        }

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                sizes,
            },
        };
    }

    private static StackPanel BuildToggleGroupDisabled()
    {
        var disabled = new ToggleGroup { IsEnabled = false, HorizontalAlignment = HorizontalAlignment.Left, SelectedValue = "open" };
        disabled.Items.Add(new ToggleItem("Open", "open"));
        disabled.Items.Add(new ToggleItem("Closed", "closed"));

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildRatingStates()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Rating { SelectedValue = 3 });
        stack.Children.Add(new Rating { SelectedValue = 4, MaxValue = 6, Color = LoamColor.Primary });
        stack.Children.Add(new Rating { SelectedValue = 5, ReadOnly = true, Size = LoamSize.Small });
        stack.Children.Add(new Rating { SelectedValue = 2, IsEnabled = false });
        return stack;
    }

    private static WrapPanel BuildRatingSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel { MaxWidth = 760, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new StackPanel
            {
                Spacing = 4,
                Margin = new Thickness(0, 0, 24, 12),
                Children =
                {
                    new Text { Text = size.ToString(), Typo = Typo.Caption },
                    new Rating { SelectedValue = 3, Size = size },
                },
            });
        }

        return sizes;
    }

    private static StackPanel BuildPopoverTrigger()
    {
        var detailsTrigger = new LoamButton
        {
            Content = "Open details",
            Variant = Variant.Filled,
            Color = LoamColor.Primary,
        };
        var detailsPopover = new Popover
        {
            Trigger = detailsTrigger,
            Placement = Avalonia.Controls.PlacementMode.BottomEdgeAlignedLeft,
            Content = new StackPanel
            {
                Width = 236,
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Project details", Typo = Typo.Subtitle1 },
                    new Text { Text = "Anchored content uses a compact elevated surface.", Typo = Typo.Body2, Color = LoamColor.Secondary },
                    new LoamButton
                    {
                        Content = "Review",
                        Variant = Variant.Text,
                        Color = LoamColor.Primary,
                        Size = LoamSize.Small,
                    },
                },
            },
        };

        return new StackPanel
        {
            Width = 280,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                detailsTrigger,
                detailsPopover,
                new Border { Height = 112 },
            },
        };
    }

    private static StackPanel BuildPopoverOpenAndClose()
    {
        var actionTrigger = new LoamButton
        {
            Content = "Open actions",
            Variant = Variant.Outlined,
            Color = LoamColor.Primary,
        };
        var actionPopover = new Popover
        {
            Trigger = actionTrigger,
            Placement = Avalonia.Controls.PlacementMode.RightEdgeAlignedTop,
            Open = true,
        };
        var dismissAction = new LoamButton
        {
            Content = "Close",
            Variant = Variant.Text,
            Color = LoamColor.Primary,
            Size = LoamSize.Small,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        dismissAction.Click += (_, _) => actionPopover.Open = false;
        actionPopover.Content = new StackPanel
        {
            Width = 220,
            Spacing = 10,
            Children =
            {
                new Text { Text = "Quick actions", Typo = Typo.Subtitle1 },
                new Text { Text = "Escape, light-dismiss, or the action closes this flyout.", Typo = Typo.Body2, Color = LoamColor.Secondary },
                dismissAction,
            },
        };

        return new StackPanel
        {
            Width = 300,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                actionTrigger,
                actionPopover,
                new Border { Height = 132 },
            },
        };
    }

    private static StackPanel BuildPopoverDisabled()
    {
        var disabledTrigger = new LoamButton
        {
            Content = "Disabled trigger",
            Variant = Variant.Filled,
            Color = LoamColor.Primary,
            IsEnabled = false,
        };
        var disabledPopover = new Popover
        {
            Trigger = disabledTrigger,
            Placement = Avalonia.Controls.PlacementMode.Bottom,
            Content = new StackPanel
            {
                Width = 200,
                Spacing = 8,
                Children =
                {
                    new Text { Text = "Disabled", Typo = Typo.Subtitle1 },
                    new Text { Text = "This stays closed while the trigger is disabled.", Typo = Typo.Body2, Color = LoamColor.Secondary },
                },
            },
        };

        return new StackPanel
        {
            Width = 280,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                disabledTrigger,
                disabledPopover,
                new Border { Height = 88 },
            },
        };
    }

    private static StackPanel BuildPopoverControlled()
    {
        var controlledTrigger = new LoamButton
        {
            Content = "Controlled open",
            Variant = Variant.Outlined,
            Color = LoamColor.Primary,
        };
        var controlledPopover = new Popover
        {
            Trigger = controlledTrigger,
            Placement = Avalonia.Controls.PlacementMode.BottomEdgeAlignedLeft,
            Open = true,
        };
        var controlledClose = new LoamButton
        {
            Content = "Close controlled",
            Variant = Variant.Text,
            Color = LoamColor.Primary,
            Size = LoamSize.Small,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        controlledClose.Click += (_, _) => controlledPopover.Open = false;
        controlledPopover.Content = new StackPanel
        {
            Width = 232,
            Spacing = 10,
            Children =
            {
                new Text { Text = "Controlled state", Typo = Typo.Subtitle1 },
                new Text { Text = "Set Open from app state and still keep Escape and light-dismiss.", Typo = Typo.Body2, Color = LoamColor.Secondary },
                controlledClose,
            },
        };

        return new StackPanel
        {
            Width = 280,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                controlledTrigger,
                controlledPopover,
                new Border { Height = 132 },
            },
        };
    }

    private static StackPanel BuildTooltipStandard()
    {
        var standard = new LoamButton
        {
            Content = "Standard",
            Variant = Variant.Filled,
            Color = LoamColor.Primary,
        };
        Tooltip.Set(standard, "Quick context for this action", new TooltipOptions
        {
            HelpText = "Quick context tooltip",
        });

        return new StackPanel
        {
            Width = 240,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                standard,
            },
        };
    }

    private static StackPanel BuildTooltipRichSurface()
    {
        var rich = new LoamButton
        {
            Content = "Rich tooltip",
            Variant = Variant.Outlined,
            Color = LoamColor.Secondary,
        };
        Tooltip.Set(rich, "Shows title, custom padding, color, and elevation.", new TooltipOptions
        {
            Title = "Details",
            Elevation = 5,
            Padding = new Thickness(12, 8),
            Color = LoamColor.Secondary,
            Placement = PlacementMode.BottomEdgeAlignedLeft,
            VerticalOffset = 8,
            HelpText = "Rich tooltip with title",
        });

        return new StackPanel
        {
            Width = 260,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                rich,
            },
        };
    }

    private static StackPanel BuildTooltipPlacementAndDelay()
    {
        var delayed = new LoamButton
        {
            Content = "Delayed bottom",
            Variant = Variant.Outlined,
            Color = LoamColor.Primary,
        };
        Tooltip.Set(delayed, "Uses placement and show delay options.", new TooltipOptions
        {
            Placement = PlacementMode.Bottom,
            HorizontalOffset = 4,
            VerticalOffset = 10,
            ShowDelay = 250,
            BetweenShowDelay = 100,
            HelpText = "Delayed tooltip",
        });

        return new StackPanel
        {
            Width = 260,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                delayed,
            },
        };
    }

    private static StackPanel BuildTooltipDisabledTarget()
    {
        var disabled = new LoamButton
        {
            Content = "Disabled target",
            Variant = Variant.Filled,
            Color = LoamColor.Primary,
            IsEnabled = false,
        };
        Tooltip.Set(disabled, "Disabled controls can still expose help.", new TooltipOptions
        {
            ShowOnDisabled = true,
            HelpText = "Disabled target tooltip",
        });

        return new StackPanel
        {
            Width = 260,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildTooltipSuppressed()
    {
        var serviceDisabled = new LoamButton
        {
            Content = "Service disabled",
            Variant = Variant.Outlined,
            Color = LoamColor.Error,
        };
        Tooltip.Set(serviceDisabled, "This tooltip is registered but the service is disabled.", new TooltipOptions
        {
            ServiceEnabled = false,
            HelpText = "Tooltip service disabled",
        });

        return new StackPanel
        {
            Width = 260,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                serviceDisabled,
            },
        };
    }

    private static StackPanel BuildTooltipCleared()
    {
        var cleared = new LoamButton
        {
            Content = "Cleared",
            Variant = Variant.Text,
            Color = LoamColor.Primary,
        };
        Tooltip.Set(cleared, "This tooltip is removed by Clear.");
        Tooltip.Clear(cleared);

        return new StackPanel
        {
            Width = 220,
            Spacing = 8,
            Margin = new Thickness(0, 0, 24, 16),
            Children =
            {
                cleared,
            },
        };
    }

    private static StackPanel BuildOverlayScrim()
    {
        var lightOverlay = new Overlay { AutoClose = true };
        var lightClose = new Loam.Controls.Button
        {
            Content = "Done",
            Variant = Variant.Text,
            Color = LoamColor.Primary,
        };
        lightClose.Click += (_, _) => lightOverlay.Visible = false;
        lightOverlay.Content = new Paper
        {
            Elevation = 3,
            Compact = true,
            Title = "Light overlay",
            Body = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new ProgressCircular { Label = "Loading", Size = LoamSize.Small },
                    lightClose,
                },
            },
        };

        var lightRegion = new Panel { Width = 320, Height = 180, HorizontalAlignment = HorizontalAlignment.Left };
        lightRegion.Children.Add(new Paper
        {
            Elevation = 1,
            Title = "Light auto-close",
            Body = "Click the scrim or press Escape.",
            Padding = new Thickness(16),
        });
        lightRegion.Children.Add(lightOverlay);

        var showLight = new Loam.Controls.Button { Content = "Show light overlay", Variant = Variant.Filled, Color = LoamColor.Primary };
        showLight.Click += (_, _) => lightOverlay.Visible = true;

        var darkOverlay = new Overlay { AutoClose = true, DarkBackground = true };
        var darkClose = new Loam.Controls.Button
        {
            Content = "Dismiss",
            Variant = Variant.Text,
            Color = LoamColor.Primary,
        };
        darkClose.Click += (_, _) => darkOverlay.Visible = false;
        darkOverlay.Content = new Paper
        {
            Elevation = 3,
            Compact = true,
            Title = "Dark overlay",
            Body = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new ProgressCircular { Label = "Syncing", Size = LoamSize.Small, Color = LoamColor.Secondary },
                    darkClose,
                },
            },
        };

        var darkRegion = new Panel { Width = 320, Height = 180, HorizontalAlignment = HorizontalAlignment.Left };
        darkRegion.Children.Add(new Paper
        {
            Elevation = 1,
            Title = "Dark auto-close",
            Body = "Darker scrim over the same local surface.",
            Padding = new Thickness(16),
        });
        darkRegion.Children.Add(darkOverlay);

        var showDark = new Loam.Controls.Button { Content = "Show dark overlay", Variant = Variant.Filled, Color = LoamColor.Secondary };
        showDark.Click += (_, _) => darkOverlay.Visible = true;

        var manualOverlay = new Overlay { AutoClose = false, DarkBackground = true };
        var manualClose = new Loam.Controls.Button
        {
            Content = "Close manually",
            Variant = Variant.Filled,
            Color = LoamColor.Primary,
        };
        manualClose.Click += (_, _) => manualOverlay.Visible = false;
        manualOverlay.Content = new Paper
        {
            Elevation = 3,
            Compact = true,
            Title = "Manual overlay",
            Body = manualClose,
        };

        var manualRegion = new Panel { Width = 320, Height = 180, HorizontalAlignment = HorizontalAlignment.Left };
        manualRegion.Children.Add(new Paper
        {
            Elevation = 1,
            Title = "Manual close",
            Body = "Scrim clicks stay ignored until the action closes it.",
            Padding = new Thickness(16),
        });
        manualRegion.Children.Add(manualOverlay);

        var showManual = new Loam.Controls.Button { Content = "Show manual overlay", Variant = Variant.Outlined, Color = LoamColor.Primary };
        showManual.Click += (_, _) => manualOverlay.Visible = true;

        var disabledOverlay = new Overlay
        {
            Visible = true,
            AutoClose = true,
            DarkBackground = true,
            IsEnabled = false,
            Content = new Paper
            {
                Elevation = 3,
                Compact = true,
                Title = "Disabled overlay",
                Body = "Auto-close is suppressed.",
            },
        };

        var disabledRegion = new Panel { Width = 320, Height = 180, HorizontalAlignment = HorizontalAlignment.Left };
        disabledRegion.Children.Add(new Paper
        {
            Elevation = 1,
            Title = "Disabled auto-close",
            Body = "The overlay remains visible while disabled.",
            Padding = new Thickness(16),
        });
        disabledRegion.Children.Add(disabledOverlay);

        var rows = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Left };
        rows.Children.Add(new StackPanel
        {
            Width = 340,
            Spacing = 10,
            Margin = new Thickness(0, 0, 20, 20),
            Children = { showLight, lightRegion },
        });
        rows.Children.Add(new StackPanel
        {
            Width = 340,
            Spacing = 10,
            Margin = new Thickness(0, 0, 20, 20),
            Children = { showDark, darkRegion },
        });
        rows.Children.Add(new StackPanel
        {
            Width = 340,
            Spacing = 10,
            Margin = new Thickness(0, 0, 20, 20),
            Children = { showManual, manualRegion },
        });
        rows.Children.Add(new StackPanel
        {
            Width = 340,
            Spacing = 10,
            Margin = new Thickness(0, 0, 20, 20),
            Children = { new Text { Text = "Disabled state", Typo = Typo.Subtitle2 }, disabledRegion },
        });

        return new StackPanel { Spacing = 12, HorizontalAlignment = HorizontalAlignment.Left, Children = { rows } };
    }

    private static WrapPanel BuildProgressCircularStates()
    {
        var activity = new WrapPanel();
        activity.Children.Add(new StackPanel
        {
            Width = 132,
            Spacing = 8,
            Margin = new Thickness(0, 0, 12, 12),
            Children =
            {
                new Text { Text = "Indeterminate", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressCircular
                {
                    Label = "Loading",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        });
        activity.Children.Add(new StackPanel
        {
            Width = 132,
            Spacing = 8,
            Margin = new Thickness(0, 0, 12, 12),
            Children =
            {
                new Text { Text = "Secondary", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressCircular
                {
                    Label = "Sync",
                    Color = LoamColor.Secondary,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        });
        activity.Children.Add(new StackPanel
        {
            Width = 132,
            Spacing = 8,
            Margin = new Thickness(0, 0, 12, 12),
            Children =
            {
                new Text { Text = "Determinate", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressCircular
                {
                    Label = "Upload",
                    Indeterminate = false,
                    Value = 70,
                    ShowValue = true,
                    Color = LoamColor.Success,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        });
        activity.Children.Add(new StackPanel
        {
            Width = 132,
            Spacing = 8,
            Margin = new Thickness(0, 0, 12, 12),
            Children =
            {
                new Text { Text = "Custom text", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressCircular
                {
                    Label = "Import",
                    Indeterminate = false,
                    Value = 33,
                    ValueText = "Step 2",
                    ShowValue = true,
                    Size = LoamSize.Large,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        });

        return activity;
    }

    private static WrapPanel BuildProgressCircularSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel();
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new StackPanel
            {
                Width = 132,
                Spacing = 8,
                Margin = new Thickness(0, 0, 12, 12),
                Children =
                {
                    new Text { Text = size.ToString(), Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new ProgressCircular
                    {
                        Label = $"{size} progress",
                        Size = size,
                        Indeterminate = false,
                        Value = 64,
                        Color = LoamColor.Primary,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                },
            });
        }

        return sizes;
    }

    private static WrapPanel BuildProgressCircularDisabled()
    {
        var disabled = new WrapPanel();
        disabled.Children.Add(new StackPanel
        {
            Width = 132,
            Spacing = 8,
            Margin = new Thickness(0, 0, 12, 12),
            Children =
            {
                new Text { Text = "Disabled", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressCircular
                {
                    Label = "Disabled progress",
                    Indeterminate = false,
                    Value = 45,
                    ShowValue = true,
                    Color = LoamColor.Warning,
                    IsEnabled = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        });
        disabled.Children.Add(new StackPanel
        {
            Width = 132,
            Spacing = 8,
            Margin = new Thickness(0, 0, 12, 12),
            Children =
            {
                new Text { Text = "Static", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressCircular
                {
                    Label = "Static progress",
                    Indeterminate = false,
                    Value = 100,
                    ShowValue = true,
                    Color = LoamColor.Info,
                    StrokeWidth = 6,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        });

        return disabled;
    }

    private static WrapPanel BuildProgressLinearStates()
    {
        var states = new WrapPanel();
        states.Children.Add(new StackPanel
        {
            Width = 360,
            Spacing = 6,
            Margin = new Thickness(0, 0, 16, 14),
            Children =
            {
                new Text { Text = "Determinate", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressLinear
                {
                    Label = "Upload",
                    ShowValue = true,
                    Value = 60,
                    Width = 360,
                    Color = LoamColor.Primary,
                },
            },
        });
        states.Children.Add(new StackPanel
        {
            Width = 360,
            Spacing = 6,
            Margin = new Thickness(0, 0, 16, 14),
            Children =
            {
                new Text { Text = "Indeterminate", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressLinear
                {
                    Label = "Loading records",
                    ShowValue = true,
                    Indeterminate = true,
                    Width = 360,
                    Color = LoamColor.Info,
                },
            },
        });
        states.Children.Add(new StackPanel
        {
            Width = 360,
            Spacing = 6,
            Margin = new Thickness(0, 0, 16, 14),
            Children =
            {
                new Text { Text = "Custom value text", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressLinear
                {
                    Label = "Import",
                    ShowValue = true,
                    Value = 50,
                    ValueText = "Step 2 of 4",
                    Width = 360,
                    Color = LoamColor.Success,
                },
            },
        });
        states.Children.Add(new StackPanel
        {
            Width = 360,
            Spacing = 6,
            Margin = new Thickness(0, 0, 16, 14),
            Children =
            {
                new Text { Text = "Disabled", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressLinear
                {
                    Label = "Disabled sync",
                    ShowValue = true,
                    Value = 75,
                    Width = 360,
                    IsEnabled = false,
                },
            },
        });
        states.Children.Add(new StackPanel
        {
            Width = 360,
            Spacing = 6,
            Margin = new Thickness(0, 0, 16, 14),
            Children =
            {
                new Text { Text = "Disabled indeterminate", Typo = Typo.Caption, Color = LoamColor.Secondary },
                new ProgressLinear
                {
                    Label = "Queued",
                    ShowValue = true,
                    Indeterminate = true,
                    Width = 360,
                    Color = LoamColor.Secondary,
                    IsEnabled = false,
                },
            },
        });

        return states;
    }

    private static WrapPanel BuildProgressLinearSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel();
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new StackPanel
            {
                Width = 300,
                Spacing = 6,
                Margin = new Thickness(0, 0, 16, 14),
                Children =
                {
                    new Text { Text = size.ToString(), Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new ProgressLinear
                    {
                        Label = $"{size} progress",
                        ShowValue = true,
                        Size = size,
                        Value = 64,
                        Width = 300,
                        Color = LoamColor.Primary,
                    },
                },
            });
        }

        return sizes;
    }

    private static WrapPanel BuildSkeletonPresets()
    {
        var presets = new WrapPanel();
        presets.Children.Add(Skeleton.TextLine(220, LoamSize.Medium, label: "Title loading"));
        presets.Children.Add(Skeleton.TextLine(160, LoamSize.Small, animate: false, label: "Subtitle loading"));
        presets.Children.Add(Skeleton.Avatar(LoamSize.Medium, label: "Avatar loading"));
        presets.Children.Add(Skeleton.Button(132, LoamSize.Medium, label: "Action loading"));
        presets.Children.Add(Skeleton.Thumbnail(128, 84, label: "Thumbnail loading"));
        presets.Children.Add(Skeleton.Card(260, 96, animate: false, label: "Card loading"));

        return presets;
    }

    private static StackPanel BuildSkeletonComposition()
    {
        var article = new StackPanel
        {
            Width = 300,
            Spacing = 10,
            Children =
            {
                Skeleton.TextLine(220, LoamSize.Large, label: "Article title loading"),
                Skeleton.TextLine(180, LoamSize.Small, label: "Article metadata loading"),
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 12,
                    Children =
                    {
                        Skeleton.Avatar(LoamSize.Medium, label: "Author loading"),
                        new StackPanel
                        {
                            Spacing = 8,
                            Children =
                            {
                                Skeleton.TextLine(160, LoamSize.Medium, label: "Author name loading"),
                                Skeleton.TextLine(96, LoamSize.Small, animate: false, label: "Author status loading"),
                            },
                        },
                    },
                },
                Skeleton.Thumbnail(300, 140, label: "Article media loading"),
                Skeleton.Button(132, LoamSize.Medium, label: "Primary action loading"),
            },
        };

        return article;
    }

    private static WrapPanel BuildSkeletonSizes()
    {
        var allSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizes = new WrapPanel();
        foreach (var size in allSizes)
        {
            sizes.Children.Add(new StackPanel
            {
                Width = 220,
                Spacing = 8,
                Margin = new Thickness(0, 0, 16, 14),
                Children =
                {
                    new Text { Text = size.ToString(), Typo = Typo.Caption, Color = LoamColor.Secondary },
                    Skeleton.TextLine(180, size, animate: false, label: $"{size} text loading"),
                    Skeleton.Avatar(size, animate: false, label: $"{size} avatar loading"),
                    Skeleton.Button(132, size, animate: false, label: $"{size} button loading"),
                },
            });
        }

        return sizes;
    }

    private static WrapPanel BuildSkeletonStates()
    {
        var states = new WrapPanel();
        states.Children.Add(Skeleton.TextLine(180, LoamSize.Medium, label: "Animated loading"));
        states.Children.Add(Skeleton.TextLine(180, LoamSize.Medium, animate: false, label: "Static loading"));
        states.Children.Add(new Skeleton
        {
            Preset = SkeletonPreset.Text,
            Width = 180,
            Size = LoamSize.Medium,
            IsEnabled = false,
            Label = "Disabled loading",
        });
        states.Children.Add(new Skeleton
        {
            Width = 48,
            Height = 48,
            Circle = true,
            Animate = false,
            Label = "Custom circular loading",
        });

        return states;
    }

    private static StackPanel BuildProgress()
    {
        var stack = new StackPanel { Spacing = 20, HorizontalAlignment = HorizontalAlignment.Left };

        var spinners = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 24 };
        spinners.Children.Add(new ProgressCircular());
        spinners.Children.Add(new ProgressCircular { Color = LoamColor.Secondary, Size = LoamSize.Small });
        spinners.Children.Add(new ProgressCircular { Indeterminate = false, Value = 70, Color = LoamColor.Success });
        spinners.Children.Add(new ProgressCircular { Indeterminate = false, Value = 33, Size = LoamSize.Large, StrokeWidth = 5 });
        stack.Children.Add(spinners);

        stack.Children.Add(new ProgressLinear { Value = 60, Width = 320 });
        stack.Children.Add(new ProgressLinear { Value = 30, Width = 320, Color = LoamColor.Warning });
        stack.Children.Add(new ProgressLinear { Indeterminate = true, Width = 320, Color = LoamColor.Info });
        stack.Children.Add(new ProgressLinear { Value = 75, Width = 320, IsEnabled = false });

        var skeletons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        skeletons.Children.Add(new Skeleton { Width = 140 });
        skeletons.Children.Add(new Skeleton { Width = 80, Animate = false });
        skeletons.Children.Add(new Skeleton { Circle = true, Width = 32, Height = 32 });
        stack.Children.Add(Labeled("Skeletons", skeletons));

        return stack;
    }

    private static StackPanel BuildBreadcrumbsDefaultTrail()
    {
        var defaultCrumbs = new Breadcrumbs();
        defaultCrumbs.Items.Add(new BreadcrumbItem("Home", () => { }));
        defaultCrumbs.Items.Add(new BreadcrumbItem("Components", () => { }));
        defaultCrumbs.Items.Add(new BreadcrumbItem("Navigation"));

        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                defaultCrumbs,
            },
        };
    }

    private static StackPanel BuildBreadcrumbsCustomSeparator()
    {
        var customSeparator = new Breadcrumbs { Separator = ">" };
        customSeparator.Items.Add(new BreadcrumbItem("Workspace", () => { }));
        customSeparator.Items.Add(new BreadcrumbItem("Projects", () => { }));
        customSeparator.Items.Add(new BreadcrumbItem("Gallery"));

        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                customSeparator,
            },
        };
    }

    private static StackPanel BuildBreadcrumbsHrefAndDisabled()
    {
        var mixedCrumbs = new Breadcrumbs();
        mixedCrumbs.Items.Add(new BreadcrumbItem { Text = "Docs", Href = "https://example.com/docs" });
        mixedCrumbs.Items.Add(new BreadcrumbItem { Text = "Archived", Disabled = true });
        mixedCrumbs.Items.Add(new BreadcrumbItem("Release notes"));

        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                mixedCrumbs,
            },
        };
    }

    private static StackPanel BuildBreadcrumbsDeepTrail()
    {
        var deepCrumbs = new Breadcrumbs();
        deepCrumbs.Items.Add(new BreadcrumbItem("Workspace", () => { }));
        deepCrumbs.Items.Add(new BreadcrumbItem("Projects", () => { }));
        deepCrumbs.Items.Add(new BreadcrumbItem("Gallery", () => { }));
        deepCrumbs.Items.Add(new BreadcrumbItem("Breadcrumbs"));

        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                deepCrumbs,
            },
        };
    }

    private static StackPanel BuildLinkColors()
    {
        var clicked = new Text
        {
            Text = "No link clicked",
            Typo = Typo.Caption,
            Color = LoamColor.Secondary,
        };

        return new StackPanel
        {
            Spacing = 14,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Link { Text = "Hover underline", OnClick = () => clicked.Text = "Hover underline clicked" },
                new Link { Text = "Always underline", Underline = true, Color = LoamColor.Secondary, OnClick = () => clicked.Text = "Always underline clicked" },
                new Link { Text = "Success link", Color = LoamColor.Success, OnClick = () => clicked.Text = "Success link clicked" },
                clicked,
            },
        };
    }

    private static StackPanel BuildLinkHrefAndDisabled()
    {
        return new StackPanel
        {
            Spacing = 14,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Link { Text = "External href", Href = "https://example.com", Underline = true },
                new Link { Text = "Disabled link", IsEnabled = false },
            },
        };
    }

    private static StackPanel BuildNavMenuSimple()
    {
        var primaryMenu = new NavMenu { Width = 280, Spacing = 2 };
        primaryMenu.Children.Add(new NavLink { Icon = Icons.Material.Filled.Home, Content = "Dashboard", IsActive = true });
        primaryMenu.Children.Add(new NavLink { Icon = Icons.Material.Filled.Search, Content = "Search" });
        primaryMenu.Children.Add(new NavLink { Icon = Icons.Material.Filled.Settings, Content = "Settings" });
        primaryMenu.Children.Add(new NavLink { Icon = Icons.Material.Filled.VisibilityOff, Content = "Locked", IsEnabled = false });

        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Paper { Elevation = 0, Outlined = true, Padding = new Thickness(8), Width = 296, Content = primaryMenu },
            },
        };
    }

    private static StackPanel BuildNavMenuGrouped()
    {
        var groupedMenu = new NavMenu { Width = 280, Spacing = 4 };
        groupedMenu.Children.Add(new NavLink { Icon = Icons.Material.Filled.Article, Content = "Overview", IsActive = true });
        var reports = new NavGroup { Title = "Reports", Icon = Icons.Material.Filled.ShowChart, Expanded = true };
        reports.Items.Add(new NavLink { Icon = Icons.Material.Filled.PieChart, Content = "Revenue" });
        reports.Items.Add(new NavLink { Icon = Icons.Material.Filled.BarChart, Content = "Usage" });
        groupedMenu.Children.Add(reports);
        groupedMenu.Children.Add(new NavGroup { Title = "Archived", Icon = Icons.Material.Filled.VisibilityOff, IsEnabled = false });

        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Paper { Elevation = 0, Outlined = true, Padding = new Thickness(8), Width = 296, Content = groupedMenu },
            },
        };
    }

    private static StackPanel BuildNavLink()
    {
        var links = new NavMenu { Width = 280, Spacing = 4 };
        links.Children.Add(new NavLink { Icon = Icons.Material.Filled.Home, Content = "Active", IsActive = true });
        links.Children.Add(new NavLink { Icon = Icons.Material.Filled.Search, Content = "Idle" });
        links.Children.Add(new NavLink { Icon = Icons.Material.Filled.Settings, Content = "Secondary active", Color = LoamColor.Secondary, IsActive = true });
        links.Children.Add(new NavLink { Content = "Text only" });
        links.Children.Add(new NavLink { Icon = Icons.Material.Filled.Article, Content = "Href target", Href = "https://example.com" });
        links.Children.Add(new NavLink { Icon = Icons.Material.Filled.VisibilityOff, Content = "Disabled", IsEnabled = false });

        return new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Text { Text = "Rows", Typo = Typo.Subtitle2 },
                new Paper { Elevation = 0, Outlined = true, Padding = new Thickness(8), Width = 296, Content = links },
            },
        };
    }

    private static NavigationRail BuildNavigationRail()
    {
        return new NavigationRail
        {
            SelectedIndex = 0,
            Items =
            {
                new NavigationRailItem { Icon = Icons.Material.Filled.Home, Label = "Home" },
                new NavigationRailItem { Icon = Icons.Material.Filled.Dashboard, Label = "Dashboard" },
                new NavigationRailItem { Icon = Icons.Material.Filled.Notifications, Label = "Alerts" },
                new NavigationRailItem { Icon = Icons.Material.Filled.Settings, Label = "Settings" },
            },
        };
    }

    private static CommandPalette BuildCommandPalette()
    {
        return new CommandPalette
        {
            Commands =
            {
                new CommandPaletteItem { Title = "New item", Icon = Icons.Material.Filled.Add, Keywords = ["create"] },
                new CommandPaletteItem { Title = "Search", Icon = Icons.Material.Filled.Search },
                new CommandPaletteItem { Title = "Open in new window", Icon = Icons.Material.Filled.OpenInNew, Keywords = ["external"] },
                new CommandPaletteItem { Title = "Toggle dark mode", Icon = Icons.Material.Filled.DarkMode, Keywords = ["theme", "light"] },
                new CommandPaletteItem { Title = "Settings", Icon = Icons.Material.Filled.Settings },
            },
        };
    }

    private static BottomNavigation BuildBottomNavigation()
    {
        return new BottomNavigation
        {
            Width = 420,
            SelectedIndex = 0,
            Items =
            {
                new BottomNavigationItem { Icon = Icons.Material.Filled.Home, Label = "Home" },
                new BottomNavigationItem { Icon = Icons.Material.Filled.Search, Label = "Search" },
                new BottomNavigationItem { Icon = Icons.Material.Filled.Notifications, Label = "Alerts" },
                new BottomNavigationItem { Icon = Icons.Material.Filled.Settings, Label = "Settings" },
            },
        };
    }

    private static StackPanel BuildNavGroup()
    {
        var expanded = new NavGroup { Title = "Expanded group", Icon = Icons.Material.Filled.Settings, Expanded = true };
        expanded.Items.Add(new NavLink { Icon = Icons.Material.Filled.Person, Content = "Users", IsActive = true });
        expanded.Items.Add(new NavLink { Icon = Icons.Material.Filled.Favorite, Content = "Roles" });

        var collapsed = new NavGroup { Title = "Collapsed group", Icon = Icons.Material.Filled.ShowChart };
        collapsed.Items.Add(new NavLink { Icon = Icons.Material.Filled.PieChart, Content = "Revenue" });
        collapsed.Items.Add(new NavLink { Icon = Icons.Material.Filled.BarChart, Content = "Usage" });

        var disabled = new NavGroup { Title = "Disabled group", Icon = Icons.Material.Filled.VisibilityOff, IsEnabled = false };
        disabled.Items.Add(new NavLink { Content = "Hidden child" });

        var nav = new NavMenu
        {
            Width = 300,
            Children =
            {
                expanded,
                collapsed,
                disabled,
            },
        };

        return new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Text { Text = "Expanded, collapsed, and disabled", Typo = Typo.Subtitle2 },
                new Paper { Elevation = 0, Outlined = true, Padding = new Thickness(8), Width = 316, Content = nav },
            },
        };
    }

    private static StackPanel BuildNavigation()
    {
        var stack = new StackPanel { Spacing = 16, HorizontalAlignment = HorizontalAlignment.Left };

        var crumbs = new Breadcrumbs();
        crumbs.Items.Add(new BreadcrumbItem("Home", () => { }));
        crumbs.Items.Add(new BreadcrumbItem("Components", () => { }));
        crumbs.Items.Add(new BreadcrumbItem("Navigation"));
        stack.Children.Add(crumbs);

        var links = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 20 };
        links.Children.Add(new Link { Text = "Hover underline", OnClick = () => { } });
        links.Children.Add(new Link { Text = "Always underline", Underline = true, Color = LoamColor.Secondary });
        links.Children.Add(new Link { Text = "Guide link", OnClick = () => { } });
        links.Children.Add(new Link { Text = "Disabled", IsEnabled = false });
        stack.Children.Add(links);

        var nav = new NavMenu { Width = 220 };
        var dashboard = new NavLink { Icon = Icons.Material.Filled.Home, Content = "Dashboard", IsActive = true };
        var people = new NavLink { Icon = Icons.Material.Filled.Person, Content = "People" };
        var settings = new NavLink { Icon = Icons.Material.Filled.Settings, Content = "Settings" };
        var disabled = new NavLink { Icon = Icons.Material.Filled.VisibilityOff, Content = "Locked", IsEnabled = false };
        foreach (var item in new[] { dashboard, people, settings })
        {
            var current = item;
            current.OnClick = () =>
            {
                foreach (var l in new[] { dashboard, people, settings })
                {
                    l.IsActive = ReferenceEquals(l, current);
                }
            };
            nav.Children.Add(current);
        }

        nav.Children.Add(disabled);

        var group = new NavGroup { Title = "Admin", Icon = Icons.Material.Filled.Settings, Expanded = true };
        group.Items.Add(new NavLink { Icon = Icons.Material.Filled.Person, Content = "Users" });
        group.Items.Add(new NavLink { Icon = Icons.Material.Filled.Favorite, Content = "Roles" });
        nav.Children.Add(group);

        stack.Children.Add(new Paper { Elevation = 1, Width = 220, Content = nav });
        return stack;
    }

    private static StackPanel BuildPaginationBoundary()
    {
        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Pagination { Count = 10, Selected = 1 },
            },
        };
    }

    private static StackPanel BuildPaginationWindowed()
    {
        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Pagination { Count = 24, Selected = 12, BoundaryCount = 2, MiddleCount = 5 },
            },
        };
    }

    private static StackPanel BuildPaginationSecondaryColor()
    {
        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Pagination { Count = 12, Selected = 6, Color = LoamColor.Secondary },
            },
        };
    }

    private static StackPanel BuildPaginationClamped()
    {
        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Pagination { Count = 7, Selected = 99, MiddleCount = 3 },
            },
        };
    }

    private static StackPanel BuildPaginationEmptyAndDisabled()
    {
        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 20,
                    Children =
                    {
                        new Pagination { Count = 0, Selected = 0 },
                        new Pagination { Count = 8, Selected = 4, IsEnabled = false },
                    },
                },
            },
        };
    }

    private static StackPanel BuildStepperActive()
    {
        var active = new Stepper { Width = 520 };
        active.Steps.Add(new Step("Account", new Text { Text = "Create your account credentials." }) { Completed = true });
        active.Steps.Add(new Step("Profile", new Text { Text = "Tell us a little about yourself." }));
        active.Steps.Add(new Step("Review", new Text { Text = "Confirm everything looks right." }));
        active.ActiveIndex = 1;

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                active,
            },
        };
    }

    private static StackPanel BuildStepperCompleted()
    {
        var completed = new Stepper { Width = 520 };
        completed.Steps.Add(new Step("Account", new Text { Text = "Account details captured." }) { Completed = true });
        completed.Steps.Add(new Step("Profile", new Text { Text = "Profile details captured." }) { Completed = true });
        completed.Steps.Add(new Step("Review", new Text { Text = "Ready to finish." }));
        completed.ActiveIndex = 2;

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                completed,
            },
        };
    }

    private static StackPanel BuildStepperClamped()
    {
        var clamped = new Stepper { Width = 520 };
        clamped.Steps.Add(new Step("Start", new Text { Text = "Invalid ActiveIndex clamps to an available step." }));
        clamped.Steps.Add(new Step("Finish", new Text { Text = "The final step remains reachable." }));
        clamped.ActiveIndex = 99;

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                clamped,
            },
        };
    }

    private static StackPanel BuildStepperDisabled()
    {
        var disabled = new Stepper { Width = 520, IsEnabled = false };
        disabled.Steps.Add(new Step("Queued", new Text { Text = "Disabled steppers suppress navigation actions." }) { Completed = true });
        disabled.Steps.Add(new Step("Paused", new Text { Text = "Actions remain visible but disabled." }));
        disabled.ActiveIndex = 1;

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildStepperEmpty()
    {
        var empty = new Stepper { Width = 520 };

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                empty,
            },
        };
    }

    private static StackPanel BuildCollapseAnimated()
    {
        var animated = new Collapse
        {
            Duration = TimeSpan.FromMilliseconds(220),
            Child = new Paper
            {
                Elevation = 1,
                Padding = new Thickness(16),
                Content = new Text { Text = "Animated details slide in and out when the button is pressed." },
            },
        };

        var animatedToggle = new Loam.Controls.Button
        {
            Content = "Show animated details",
            Variant = Variant.Outlined,
            Color = LoamColor.Primary,
        };
        animatedToggle.Click += (_, _) =>
        {
            animated.Expanded = !animated.Expanded;
            animatedToggle.Content = animated.Expanded ? "Hide animated details" : "Show animated details";
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 520,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                animatedToggle,
                animated,
            },
        };
    }

    private static StackPanel BuildCollapseStatic()
    {
        var staticReveal = new Collapse
        {
            Animated = false,
            Child = new Paper
            {
                Elevation = 0,
                Outlined = true,
                Padding = new Thickness(16),
                Content = new Text { Text = "Static content jumps directly to the final open or closed height." },
            },
        };

        var staticToggle = new Loam.Controls.Button
        {
            Content = "Show static details",
            Variant = Variant.Outlined,
            Color = LoamColor.Secondary,
        };
        staticToggle.Click += (_, _) =>
        {
            staticReveal.Expanded = !staticReveal.Expanded;
            staticToggle.Content = staticReveal.Expanded ? "Hide static details" : "Show static details";
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 520,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                staticToggle,
                staticReveal,
            },
        };
    }

    private static StackPanel BuildCollapseCustomDuration()
    {
        var customDuration = new Collapse
        {
            Duration = TimeSpan.FromMilliseconds(320),
            Child = new Paper
            {
                Elevation = 0,
                Outlined = true,
                Padding = new Thickness(16),
                Content = new Text { Text = "Custom-duration content uses a longer reveal timing." },
            },
        };

        var customToggle = new Loam.Controls.Button
        {
            Content = "Show custom-duration details",
            Variant = Variant.Outlined,
            Color = LoamColor.Tertiary,
        };
        customToggle.Click += (_, _) =>
        {
            customDuration.Expanded = !customDuration.Expanded;
            customToggle.Content = customDuration.Expanded
                ? "Hide custom-duration details"
                : "Show custom-duration details";
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 520,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                customToggle,
                customDuration,
            },
        };
    }

    private static StackPanel BuildCollapseDisabledStatic()
    {
        var disabled = new Collapse
        {
            IsEnabled = false,
            Expanded = true,
            Child = new Paper
            {
                Elevation = 0,
                Outlined = true,
                Padding = new Thickness(16),
                Content = new Text { Text = "Disabled reveal keeps the content visible without motion." },
            },
        };
        var disabledButton = new Loam.Controls.Button
        {
            Content = "Disabled reveal remains open",
            Variant = Variant.Outlined,
            IsEnabled = false,
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 520,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabledButton,
                disabled,
            },
        };
    }

    private static StackPanel BuildCollapseZeroDuration()
    {
        var zeroDuration = new Collapse
        {
            Duration = TimeSpan.Zero,
            Child = new Paper
            {
                Elevation = 0,
                Outlined = true,
                Padding = new Thickness(16),
                Content = new Text { Text = "Zero-duration content opens and closes immediately." },
            },
        };

        var instantToggle = new Loam.Controls.Button
        {
            Content = "Show instant details",
            Variant = Variant.Outlined,
            Color = LoamColor.Primary,
        };
        instantToggle.Click += (_, _) =>
        {
            zeroDuration.Expanded = !zeroDuration.Expanded;
            instantToggle.Content = zeroDuration.Expanded ? "Hide instant details" : "Show instant details";
        };

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 520,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                instantToggle,
                zeroDuration,
            },
        };
    }

    private static StackPanel BuildTimelineDefault()
    {
        var timeline = new Timeline { MaxWidth = 460, HorizontalAlignment = HorizontalAlignment.Left };
        timeline.Items.Add(new TimelineItem("Order placed", "Customer submitted checkout.", "9:24 AM", LoamColor.Primary));
        timeline.Items.Add(new TimelineItem("Payment confirmed", "Authorization completed.", "9:25 AM", LoamColor.Success));
        timeline.Items.Add(new TimelineItem("Packed", "Warehouse prepared the shipment.", "10:40 AM", LoamColor.Secondary));
        timeline.Items.Add(new TimelineItem("Out for delivery", "Courier is heading to the customer.", "11:15 AM", LoamColor.Warning));

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                timeline,
            },
        };
    }

    private static StackPanel BuildTimelineRich()
    {
        var rich = new Timeline { MaxWidth = 460, HorizontalAlignment = HorizontalAlignment.Left };
        rich.Items.Add(new TimelineItem(new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new Text { Text = "Review finished", Typo = Typo.Subtitle2 },
                new Text { Text = "Keyboard and automation checks passed.", Typo = Typo.Body2, Color = LoamColor.Secondary },
            },
        }, LoamColor.Success));
        rich.Items.Add(new TimelineItem(new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new Text { Text = "Visual QA", Typo = Typo.Subtitle2 },
                new Chip { Text = "In progress", Color = LoamColor.Info, Size = LoamSize.Small },
            },
        }, LoamColor.Info));

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                rich,
            },
        };
    }

    private static StackPanel BuildTimelineHorizontal()
    {
        var horizontal = new Timeline
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        horizontal.Items.Add(new TimelineItem("Queued", "Waiting", color: LoamColor.Secondary));
        horizontal.Items.Add(new TimelineItem("Running", "Active checks", color: LoamColor.Info));
        horizontal.Items.Add(new TimelineItem("Verified", "All gates passed", color: LoamColor.Success));
        horizontal.Items.Add(new TimelineItem("Published", "Ready", color: LoamColor.Primary));

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new ScrollViewer
                {
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = horizontal,
                },
            },
        };
    }

    private static StackPanel BuildTimelineEmpty()
    {
        var empty = new Timeline { MaxWidth = 460, HorizontalAlignment = HorizontalAlignment.Left };

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                empty,
            },
        };
    }

    private static StackPanel BuildTimelineDisabled()
    {
        var disabled = new Timeline { MaxWidth = 460, HorizontalAlignment = HorizontalAlignment.Left, IsEnabled = false };
        disabled.Items.Add(new TimelineItem("Locked event", "Read-only event metadata.", color: LoamColor.Primary));
        disabled.Items.Add(new TimelineItem("Read-only state", "Interaction disabled.", color: LoamColor.Secondary));

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildExpansionPanelsAccordion()
    {
        var accordion = new ExpansionPanels { Width = 520, HorizontalAlignment = HorizontalAlignment.Left };
        accordion.AddPanel("Shipping address", new Text { Text = "Where should we deliver your order?", Margin = new Thickness(0, 4) }, isExpanded: true);
        accordion.AddPanel("Billing details", new Text { Text = "Card and invoice information.", Margin = new Thickness(0, 4) });
        accordion.AddPanel("Delivery options", new Text { Text = "Standard, express, or pickup.", Margin = new Thickness(0, 4) });

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                accordion,
            },
        };
    }

    private static StackPanel BuildExpansionPanelsMulti()
    {
        var multi = new ExpansionPanels { Width = 520, MultiExpansion = true, HorizontalAlignment = HorizontalAlignment.Left };
        multi.AddPanel("Scope", new Text { Text = "Multiple sections may stay open.", Margin = new Thickness(0, 4) });
        multi.AddPanel("Risks", new Text { Text = "Second section remains open in multi mode.", Margin = new Thickness(0, 4) });
        multi.AddPanel("Notes", new Text { Text = "Additional content can be reviewed independently.", Margin = new Thickness(0, 4) });
        multi.ExpandAll();

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                multi,
            },
        };
    }

    private static StackPanel BuildExpansionPanelsDisabled()
    {
        var disabled = new ExpansionPanels { Width = 520, HorizontalAlignment = HorizontalAlignment.Left };
        disabled.AddPanel(
            "Locked review",
            new Text { Text = "Disabled panels keep a stable header surface.", Margin = new Thickness(0, 4) },
            isEnabled: false);

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static StackPanel BuildCarouselDefault()
    {
        var carousel = new Loam.Controls.Carousel
        {
            Width = 380,
            Height = 220,
            SelectedIndex = 1,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        carousel.Items.Add(new CarouselItem("Intake", "Collect requirements and owners.", LoamColor.Primary));
        carousel.Items.Add(new CarouselItem("Review", "Check keyboard, focus, and states.", LoamColor.Secondary));
        carousel.Items.Add(new CarouselItem("Ship", "Move verified changes forward.", LoamColor.Info));

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                carousel,
            },
        };
    }

    private static StackPanel BuildCarouselChromeHidden()
    {
        var chromeHidden = new Loam.Controls.Carousel
        {
            Width = 380,
            Height = 180,
            ShowArrows = false,
            ShowBullets = false,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        chromeHidden.Items.Add(new CarouselItem("Static slide", "Chrome disabled.", LoamColor.Secondary));
        chromeHidden.Items.Add(new CarouselItem("Hidden chrome", "Content remains readable.", LoamColor.Primary));

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                chromeHidden,
            },
        };
    }

    private static StackPanel BuildCarouselAutoPlay()
    {
        var auto = new Loam.Controls.Carousel
        {
            Width = 380,
            Height = 180,
            AutoPlay = true,
            AutoPlayInterval = TimeSpan.FromSeconds(2),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        auto.Items.Add(new CarouselItem("Auto first", "Advances while attached.", LoamColor.Primary));
        auto.Items.Add(new CarouselItem("Auto second", "Uses the public interval.", LoamColor.Info));
        auto.Items.Add(new CarouselItem("Auto third", "Stops when disabled.", LoamColor.Success));

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                auto,
            },
        };
    }

    private static StackPanel BuildCarouselGoToClamped()
    {
        var clamped = new Loam.Controls.Carousel
        {
            Width = 380,
            Height = 180,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        clamped.Items.Add(new CarouselItem("First", "Index clamps to an available slide.", LoamColor.Primary));
        clamped.Items.Add(new CarouselItem("Clamped last", "SelectedIndex stays deterministic.", LoamColor.Tertiary));
        clamped.GoTo(99);

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                clamped,
            },
        };
    }

    private static StackPanel BuildCarouselEmpty()
    {
        var empty = new Loam.Controls.Carousel { Width = 380, Height = 120, HorizontalAlignment = HorizontalAlignment.Left };

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                empty,
            },
        };
    }

    private static StackPanel BuildCarouselDisabled()
    {
        var disabled = new Loam.Controls.Carousel
        {
            Width = 380,
            Height = 180,
            IsEnabled = false,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        disabled.Items.Add(new CarouselItem("Disabled first", "Navigation is suppressed.", LoamColor.Primary));
        disabled.Items.Add(new CarouselItem("Disabled second", "State remains stable.", LoamColor.Secondary));

        return new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                disabled,
            },
        };
    }

    private static Loam.Controls.TreeView BuildTreeView()
    {
        Loam.Controls.TreeViewItem Node(string text, string? icon, params Loam.Controls.TreeViewItem[] children)
        {
            var node = new Loam.Controls.TreeViewItem { Text = text, Icon = icon };
            foreach (var child in children)
            {
                node.Items.Add(child);
            }

            return node;
        }

        var treeFile = Node("TreeView.cs", null);
        var disabledFile = Node("Archived.cs", null);
        disabledFile.IsEnabled = false;

        var components = Node("Components", null,
            Node("Button.cs", null),
            treeFile,
            disabledFile);
        var root = Node("src", Icons.Material.Filled.Home,
            components,
            Node("Theming", null,
                Node("LoamTheme.cs", null)));
        root.Expanded = true;
        components.Expanded = true;

        var tree = new Loam.Controls.TreeView { MaxWidth = 320, HorizontalAlignment = HorizontalAlignment.Left };
        tree.Items.Add(root);
        tree.SelectedItem = treeFile;
        return tree;
    }

    private sealed class Dessert(string name, int calories, double fat)
    {
        public string Name { get; set; } = name;

        public int Calories { get; } = calories;

        public double Fat { get; } = fat;
    }

    private static List<Dessert> SampleDesserts() =>
    [
        new("Frozen yogurt", 159, 6.0),
        new("Ice cream sandwich", 237, 9.0),
        new("Eclair", 262, 16.0),
        new("Cupcake", 305, 3.7),
        new("Gingerbread", 356, 16.0),
        new("Jelly bean", 375, 0.0),
        new("Lollipop", 392, 0.2),
        new("Honeycomb", 408, 3.2),
    ];

    private static void AddDessertColumns(Loam.Controls.DataGrid<Dessert> grid, bool editable = false)
    {
        grid.Columns.Add(new DataGridColumn<Dessert>("Dessert", d => d.Name)
        {
            Editable = editable,
            SetText = editable ? (Action<Dessert, string?>)((dessert, text) => dessert.Name = text ?? "") : null,
        });
        grid.Columns.Add(new DataGridColumn<Dessert>("Calories", d => d.Calories) { Align = HorizontalAlignment.Right });
        grid.Columns.Add(new DataGridColumn<Dessert>("Fat (g)", d => d.Fat) { Format = "0.0", Align = HorizontalAlignment.Right });
    }

    private static Loam.Controls.DataGrid<Dessert> BuildDataGridFooter()
    {
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Dense = true,
            Striped = true,
            ShowFooter = true,
            MaxWidth = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        grid.Columns.Add(new DataGridColumn<Dessert>("Dessert", d => d.Name) { Summary = rows => $"{rows.Count} desserts" });
        grid.Columns.Add(new DataGridColumn<Dessert>("Calories", d => d.Calories) { Align = HorizontalAlignment.Right, SummaryKind = DataGridSummary.Sum });
        grid.Columns.Add(new DataGridColumn<Dessert>("Fat (g)", d => d.Fat) { Format = "0.0", Align = HorizontalAlignment.Right, SummaryKind = DataGridSummary.Average });
        grid.Items = SampleDesserts();
        return grid;
    }

    private static StackPanel BuildDataGridAsyncStates()
    {
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Dense = true,
            Striped = true,
            MaxWidth = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
            SkeletonRowCount = 5,
        };
        AddDessertColumns(grid);
        grid.Items = SampleDesserts();

        var loading = new LoamButton { Content = "Loading", Variant = Variant.Outlined };
        loading.Click += (_, _) => { grid.ErrorText = null; grid.IsLoading = true; };
        var error = new LoamButton { Content = "Error", Variant = Variant.Outlined };
        error.Click += (_, _) =>
        {
            grid.IsLoading = false;
            grid.OnRetry = () => grid.ErrorText = null;
            grid.ErrorText = "Couldn't load desserts.";
        };
        var ready = new LoamButton { Content = "Ready", Variant = Variant.Text };
        ready.Click += (_, _) => { grid.IsLoading = false; grid.ErrorText = null; };

        return new StackPanel
        {
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Text { Text = "Toggle the loading skeleton, an error with Retry, or the loaded data.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                grid,
                new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { loading, error, ready } },
            },
        };
    }

    private static StackPanel BuildDataGridLive()
    {
        var data = new ObservableCollection<Dessert>(SampleDesserts().Take(4));
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Dense = true,
            Striped = true,
            Hover = true,
            MaxWidth = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        AddDessertColumns(grid);
        grid.Items = data;

        var pool = SampleDesserts();
        var add = new LoamButton { Content = "Add row", Variant = Variant.Outlined };
        add.Click += (_, _) =>
        {
            var next = pool[data.Count % pool.Count];
            data.Add(new Dessert(next.Name, next.Calories, next.Fat));
        };
        var remove = new LoamButton { Content = "Remove last", Variant = Variant.Text };
        remove.Click += (_, _) =>
        {
            if (data.Count > 0)
            {
                data.RemoveAt(data.Count - 1);
            }
        };

        return new StackPanel
        {
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Text { Text = "Bound to an ObservableCollection; Add/Remove updates the grid live.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                grid,
                new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { add, remove } },
            },
        };
    }

    private static Loam.Controls.DataGrid<Dessert> BuildDataGridPaged()
    {
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Dense = true,
            Striped = true,
            Hover = true,
            PageSize = 4,
            FilterText = "i",
            Filter = (dessert, text) => dessert.Name.Contains(text, StringComparison.OrdinalIgnoreCase),
            MaxWidth = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        AddDessertColumns(grid);
        var desserts = SampleDesserts();
        grid.Items = desserts;
        grid.SelectedItem = desserts[1];
        return grid;
    }

    private static Loam.Controls.DataGrid<Dessert> BuildDataGridGrouped()
    {
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Dense = true,
            Striped = true,
            Hover = true,
            GroupBy = d => d.Fat >= 5 ? "Indulgent" : "Light",
            GroupAggregate = items => $"avg {items.Average(d => d.Calories):F0} cal",
            MaxWidth = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        AddDessertColumns(grid);
        grid.Items = SampleDesserts();
        return grid;
    }

    private static Loam.Controls.DataGrid<Dessert> BuildDataGridFrozen()
    {
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Dense = true,
            Striped = true,
            Hover = true,
            FrozenColumns = 1,
            MaxWidth = 560,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        grid.Columns.Add(new DataGridColumn<Dessert>("Dessert", d => d.Name) { Width = 180 });
        grid.Columns.Add(new DataGridColumn<Dessert>("Calories", d => d.Calories) { Width = 120, Align = HorizontalAlignment.Right });
        grid.Columns.Add(new DataGridColumn<Dessert>("Fat (g)", d => d.Fat) { Width = 120, Format = "0.0", Align = HorizontalAlignment.Right });
        grid.Columns.Add(new DataGridColumn<Dessert>("Cal / 100g", d => d.Calories / 100.0) { Width = 130, Format = "0.0", Align = HorizontalAlignment.Right });
        grid.Columns.Add(new DataGridColumn<Dessert>("Tier", d => d.Fat >= 5 ? "Indulgent" : "Light") { Width = 130 });
        grid.Items = SampleDesserts();
        return grid;
    }

    private static Loam.Controls.DataGrid<Dessert> BuildDataGridEditable()
    {
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Dense = true,
            Striped = true,
            Hover = true,
            MaxWidth = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        AddDessertColumns(grid, editable: true);
        grid.Items = SampleDesserts().Take(4).ToList();
        return grid;
    }

    private static Loam.Controls.DataGrid<Dessert> BuildDataGridVirtualized()
    {
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Dense = true,
            Striped = true,
            Hover = true,
            Virtualize = true,
            MaxRenderedRows = 3,
            MaxWidth = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        AddDessertColumns(grid);
        grid.Items = SampleDesserts();
        return grid;
    }

    private static Loam.Controls.DataGrid<Dessert> BuildDataGridEmpty()
    {
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Dense = true,
            EmptyText = "No desserts match your filter.",
            FilterText = "zzz",
            Filter = (dessert, text) => dessert.Name.Contains(text, StringComparison.OrdinalIgnoreCase),
            MaxWidth = 720,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        AddDessertColumns(grid);
        grid.Items = SampleDesserts();
        return grid;
    }

    private static SimpleTable BuildTableDense()
    {
        var table = new SimpleTable
        {
            Dense = true,
            Bordered = true,
            Striped = true,
            Hover = true,
            MaxWidth = 480,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        table.Headers.Add("Dessert");
        table.Headers.Add("Calories");
        table.Headers.Add("Fat (g)");
        table.Rows.Add(new TableRow("Frozen yogurt", 159, 6.0));
        table.Rows.Add(new TableRow("Ice cream sandwich", 237, 9.0));
        table.Rows.Add(new TableRow("Eclair", 262, 16.0));
        table.Rows.Add(new TableRow("Cupcake", 305, 3.7));

        return table;
    }

    private static SimpleTable BuildTableEmpty()
    {
        var empty = new SimpleTable
        {
            Dense = true,
            Bordered = true,
            MaxWidth = 480,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        empty.Headers.Add("Dessert");
        empty.Headers.Add("Calories");

        return empty;
    }

    private static WrapPanel BuildFileUploadVariants()
    {
        var single = new FileUpload
        {
            Label = "Filled",
            HelperText = "Single document, remove and clear actions enabled.",
            EmptyText = "No invoice selected",
            SelectedTextFormat = "{0} invoice selected",
            ButtonText = "Choose invoice",
            Width = 360,
            Margin = new Thickness(0, 0, 24, 24),
            AllowMultiple = false,
            Variant = Variant.Filled,
            ShowRemoveButtons = true,
            ShowClearButton = true,
            AcceptedFileTypes =
            [
                new FilePickerFileType("Documents") { Patterns = ["*.pdf", "*.docx"] },
            ],
        };
        single.ShowSelection(new List<string> { "invoice-0626.pdf" });

        var multiple = new FileUpload
        {
            Label = "Multiple files",
            HelperText = "Evidence files with removable chips and a clear action.",
            EmptyText = "No evidence attached",
            SelectedTextFormat = "{0} evidence files selected",
            ButtonText = "Attach evidence",
            Width = 360,
            Margin = new Thickness(0, 0, 24, 24),
            AllowMultiple = true,
            Variant = Variant.Outlined,
            Color = LoamColor.Secondary,
            ShowRemoveButtons = true,
            ShowClearButton = true,
            ClearText = "Remove all",
            AcceptedFileTypes =
            [
                new FilePickerFileType("Evidence") { Patterns = ["*.md", "*.png", "*.csv"] },
            ],
        };
        multiple.ShowSelection(new List<string> { "brief.md", "screenshot.png", "metrics.csv" });

        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new FileUpload
                {
                    Label = "Outlined",
                    HelperText = "Images only, multiple selection allowed.",
                    EmptyText = "No images attached",
                    ButtonText = "Attach files",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Variant = Variant.Outlined,
                    Color = LoamColor.Primary,
                    AllowMultiple = true,
                    AcceptedFileTypes =
                    [
                        new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg"] },
                    ],
                },
                single,
                new FileUpload
                {
                    Label = "Text",
                    HelperText = "Text action with removable selected-file chips.",
                    EmptyText = "Nothing selected yet",
                    ButtonText = "Browse",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Variant = Variant.Text,
                    Color = LoamColor.Primary,
                    AllowMultiple = true,
                    ShowRemoveButtons = true,
                },
                multiple,
                new FileUpload
                {
                    Label = "Small",
                    HelperText = "Compact upload action.",
                    EmptyText = "No avatar selected",
                    ButtonText = "Upload avatar",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Size = LoamSize.Small,
                    Variant = Variant.Outlined,
                },
                new FileUpload
                {
                    Label = "Large",
                    HelperText = "Large filled action for larger upload surfaces.",
                    EmptyText = "No package selected",
                    ButtonText = "Upload package",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Size = LoamSize.Large,
                    Variant = Variant.Filled,
                    Color = LoamColor.Primary,
                },
                new FileUpload
                {
                    Label = "Disabled",
                    HelperText = "Disabled upload action keeps label and status visible.",
                    EmptyText = "Archived uploads are locked",
                    ButtonText = "Archived upload",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    IsEnabled = false,
                },
            },
        };
    }

    private static StackPanel BuildFileUploadSizes()
    {
        FileUpload SizeSample(string label, LoamSize size)
        {
            var upload = new FileUpload
            {
                Label = label,
                HelperText = "Generated chips and clear action follow the upload size.",
                EmptyText = "No file selected",
                ButtonText = $"Upload {label.ToLowerInvariant()}",
                Width = 360,
                Margin = new Thickness(0, 0, 24, 24),
                Size = size,
                Variant = Variant.Outlined,
                ShowRemoveButtons = true,
                ShowClearButton = true,
            };
            upload.ShowSelection([$"{label.ToLowerInvariant()}-file.txt"]);
            return upload;
        }

        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Children =
                    {
                        SizeSample("ExtraSmall", LoamSize.ExtraSmall),
                        SizeSample("Small", LoamSize.Small),
                        SizeSample("Medium", LoamSize.Medium),
                        SizeSample("Large", LoamSize.Large),
                        SizeSample("ExtraLarge", LoamSize.ExtraLarge),
                    },
                },
            },
        };
    }

    private static WrapPanel BuildFormStates()
    {
        Form AccessForm(string title, LoamSize actionSize = LoamSize.Small, bool filled = false, bool disabled = false)
        {
            var name = new TextField
            {
                Label = "Full name",
                Placeholder = "Ada Lovelace",
                HelperText = "Required",
                Required = true,
                Text = filled ? "Ada Lovelace" : null,
            };

            var email = new TextField
            {
                Label = "Email",
                Placeholder = "name@example.com",
                HelperText = "Used for notifications",
                Required = true,
                Text = filled ? "ada@example.com" : null,
                Validation = value => value?.Contains('@', StringComparison.Ordinal) == true ? null : "Enter a valid email",
            };

            var role = new TextField
            {
                Label = "Role",
                Text = "Design systems",
                HelperText = "Optional",
                Variant = Variant.Filled,
            };

            var form = new Form
            {
                Title = title,
                Subtitle = "Validate required fields before inviting a collaborator.",
                HelperText = "Fill the required fields and validate.",
                SuccessText = "Ready to submit.",
                ErrorText = "Review the highlighted fields.",
                FieldWidth = 320,
                SubmitText = "Validate",
                ResetText = "Reset",
                SubmitIcon = Icons.Material.Filled.Check,
                ResetIcon = Icons.Material.Filled.Close,
                ActionSize = actionSize,
                SubmitVariant = Variant.Filled,
                SubmitColor = LoamColor.Primary,
                ResetVariant = Variant.Outlined,
                ResetColor = LoamColor.Secondary,
                ActionsHorizontalAlignment = HorizontalAlignment.Right,
                IsEnabled = !disabled,
                Children = { name, email, role },
            };
            form.ResetAction = _ => role.Text = "Design systems";
            return form;
        }

        Paper Frame(Form form)
        {
            return new Paper
            {
                Outlined = true,
                Elevation = 0,
                Padding = new Thickness(24),
                Width = 420,
                Margin = new Thickness(0, 0, 24, 24),
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = form,
            };
        }

        var standard = AccessForm("Project access");
        var invalid = AccessForm("Validation error");
        invalid.Validate();
        var ready = AccessForm("Ready state", filled: true);
        ready.Validate();
        var disabled = AccessForm("Disabled", disabled: true);

        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                Frame(standard),
                Frame(invalid),
                Frame(ready),
                Frame(disabled),
            },
        };
    }

    private static StackPanel BuildFormActionSizes()
    {
        Form AccessForm(string title, LoamSize actionSize = LoamSize.Small, bool filled = false, bool disabled = false)
        {
            var name = new TextField
            {
                Label = "Full name",
                Placeholder = "Ada Lovelace",
                HelperText = "Required",
                Required = true,
                Text = filled ? "Ada Lovelace" : null,
            };

            var email = new TextField
            {
                Label = "Email",
                Placeholder = "name@example.com",
                HelperText = "Used for notifications",
                Required = true,
                Text = filled ? "ada@example.com" : null,
                Validation = value => value?.Contains('@', StringComparison.Ordinal) == true ? null : "Enter a valid email",
            };

            var role = new TextField
            {
                Label = "Role",
                Text = "Design systems",
                HelperText = "Optional",
                Variant = Variant.Filled,
            };

            var form = new Form
            {
                Title = title,
                Subtitle = "Validate required fields before inviting a collaborator.",
                HelperText = "Fill the required fields and validate.",
                SuccessText = "Ready to submit.",
                ErrorText = "Review the highlighted fields.",
                FieldWidth = 320,
                SubmitText = "Validate",
                ResetText = "Reset",
                SubmitIcon = Icons.Material.Filled.Check,
                ResetIcon = Icons.Material.Filled.Close,
                ActionSize = actionSize,
                SubmitVariant = Variant.Filled,
                SubmitColor = LoamColor.Primary,
                ResetVariant = Variant.Outlined,
                ResetColor = LoamColor.Secondary,
                ActionsHorizontalAlignment = HorizontalAlignment.Right,
                IsEnabled = !disabled,
                Children = { name, email, role },
            };
            form.ResetAction = _ => role.Text = "Design systems";
            return form;
        }

        Paper Frame(Form form)
        {
            return new Paper
            {
                Outlined = true,
                Elevation = 0,
                Padding = new Thickness(24),
                Width = 420,
                Margin = new Thickness(0, 0, 24, 24),
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = form,
            };
        }

        var actionSizes = new[]
        {
            LoamSize.ExtraSmall,
            LoamSize.Small,
            LoamSize.Medium,
            LoamSize.Large,
            LoamSize.ExtraLarge,
        };

        var sizeSamples = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var size in actionSizes)
        {
            sizeSamples.Children.Add(Frame(AccessForm(size.ToString(), actionSize: size, filled: true)));
        }

        return new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                sizeSamples,
            },
        };
    }

    private static WrapPanel BuildColorPickerVariants()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.ColorPicker
                {
                    Label = "Outlined",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Variant = Variant.Outlined,
                    HelperText = "Default palette",
                },
                new Loam.Controls.ColorPicker
                {
                    Label = "Custom palette",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Value = Color.Parse("#2E7D32"),
                    Palette =
                    {
                        Color.Parse("#6750A4"),
                        Color.Parse("#2E7D32"),
                        Color.Parse("#B3261E"),
                        Color.Parse("#006A6A"),
                        Color.Parse("#795900"),
                    },
                    HelperText = "Palette collection overrides defaults",
                },
                new Loam.Controls.ColorPicker
                {
                    Label = "Filled",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Variant = Variant.Filled,
                    Value = Color.Parse("#6750A4"),
                    HelperText = "Filled field style",
                },
                new Loam.Controls.ColorPicker
                {
                    Label = "Text / underline",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Variant = Variant.Text,
                    Value = Color.Parse("#FF9800"),
                    HelperText = "Underline field style",
                },
            },
        };
    }

    private static WrapPanel BuildColorPickerValues()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.ColorPicker
                {
                    Label = "Default value",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    HelperText = "Uses the component default value",
                },
                new Loam.Controls.ColorPicker
                {
                    Label = "Alpha",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Value = Color.FromArgb(0x80, 0x10, 0x20, 0x30),
                    ShowAlpha = true,
                    HelperText = "Shows #AARRGGBB",
                },
            },
        };
    }

    private static WrapPanel BuildColorPickerStates()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.ColorPicker
                {
                    Label = "Error",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Value = Color.Parse("#F7F2FA"),
                    Error = true,
                    ErrorText = "Choose a visible color",
                },
                new Loam.Controls.ColorPicker
                {
                    Label = "Disabled",
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Value = Color.Parse("#607D8B"),
                    IsEnabled = false,
                    HelperText = "Read-only state",
                },
            },
        };
    }

    private static WrapPanel BuildDateRangePickerVariants()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Outlined",
                    Variant = Variant.Outlined,
                    Placeholder = "Pick start and end",
                    PickerTitle = "Select trip range",
                    HelperText = "Pick dates, then confirm with OK",
                },
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Filled",
                    Variant = Variant.Filled,
                    Start = new DateTime(2026, 6, 8),
                    End = new DateTime(2026, 6, 19),
                    HelperText = "Filled field style",
                },
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Text / underline",
                    Variant = Variant.Text,
                    Start = new DateTime(2026, 6, 15),
                    End = new DateTime(2026, 6, 20),
                    HelperText = "Underline field style",
                },
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Empty",
                    Placeholder = "Select range",
                    HelperText = "No start or end selected",
                },
            },
        };
    }

    private static WrapPanel BuildDateRangePickerSelected()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Selected",
                    Start = new DateTime(2026, 6, 1),
                    End = new DateTime(2026, 6, 30),
                    HelperText = "Both dates selected",
                },
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Custom format",
                    Start = new DateTime(2026, 7, 2),
                    End = new DateTime(2026, 7, 16),
                    DateFormat = "MMM d",
                    CancelText = "Dismiss",
                    OkText = "Apply",
                    HelperText = "Short month/day display",
                },
            },
        };
    }

    private static WrapPanel BuildDateRangePickerClearable()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Clearable",
                    Clearable = true,
                    Start = new DateTime(2026, 6, 1),
                    End = new DateTime(2026, 6, 30),
                    HelperText = "Tap the × to reset both dates",
                },
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Clearable (empty)",
                    Clearable = true,
                    HelperText = "Clear button appears once a range is set",
                },
            },
        };
    }

    private static WrapPanel BuildDateRangePickerPresets()
    {
        var custom = new Loam.Controls.DateRangePicker
        {
            Width = 360,
            Margin = new Thickness(0, 0, 24, 24),
            Label = "Custom presets",
            ShowPresets = true,
            DateFormat = "MMM d",
            HelperText = "Tailored quick ranges",
        };
        custom.Presets.Add(new DateRangePreset("This week", a => (a.AddDays(-(int)a.DayOfWeek), a)));
        custom.Presets.Add(new DateRangePreset("Next 14 days", a => (a, a.AddDays(13))));
        custom.Presets.Add(new DateRangePreset(
            "This quarter",
            a => (new DateTime(a.Year, (((a.Month - 1) / 3) * 3) + 1, 1), a)));

        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Quick ranges",
                    ShowPresets = true,
                    HelperText = "Pick a preset, then confirm with OK",
                },
                custom,
            },
        };
    }

    private static WrapPanel BuildDateRangePickerConstrained()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Constrained",
                    Start = new DateTime(2026, 6, 10),
                    End = new DateTime(2026, 6, 14),
                    MinDate = new DateTime(2026, 6, 1),
                    MaxDate = new DateTime(2026, 7, 31),
                    HelperText = "Limited to June and July",
                },
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Floating label",
                    ShrinkLabel = true,
                    Placeholder = "Choose dates",
                    HelperText = "Label remains visible while empty",
                },
            },
        };
    }

    private static WrapPanel BuildDateRangePickerStates()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Error",
                    Start = new DateTime(2026, 6, 1),
                    Error = true,
                    ErrorText = "Choose an end date",
                },
                new Loam.Controls.DateRangePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Disabled",
                    Start = new DateTime(2026, 5, 1),
                    End = new DateTime(2026, 5, 31),
                    IsEnabled = false,
                    HelperText = "Read-only state",
                },
            },
        };
    }

    private static WrapPanel BuildDatePickerVariants()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Outlined",
                    Variant = Variant.Outlined,
                    Placeholder = "Select a date",
                    HelperText = "Bordered field style",
                },
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Filled",
                    Variant = Variant.Filled,
                    Date = new DateTime(2026, 6, 8),
                    HelperText = "Filled container style",
                },
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Text / underline",
                    Variant = Variant.Text,
                    Date = new DateTime(2026, 6, 9),
                    HelperText = "Underline field style",
                },
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Empty",
                    Placeholder = "Select a date",
                    HelperText = "Calendar opens from the field",
                },
            },
        };
    }

    private static WrapPanel BuildDatePickerSelected()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Selected",
                    Date = new DateTime(2026, 6, 30),
                    HelperText = "Default date format",
                },
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Custom format",
                    PickerTitle = "Select invoice date",
                    Date = new DateTime(2026, 7, 14),
                    DateFormat = "ddd, MMM d yyyy",
                    CancelText = "Dismiss",
                    OkText = "Apply",
                    HelperText = "Formatted display text",
                },
            },
        };
    }

    private static WrapPanel BuildDatePickerClearable()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Clearable",
                    Clearable = true,
                    Date = new DateTime(2026, 6, 30),
                    HelperText = "Tap the × to reset the value",
                },
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Clearable (empty)",
                    Clearable = true,
                    Placeholder = "Pick a date",
                    HelperText = "Clear button appears once a date is set",
                },
            },
        };
    }

    private static WrapPanel BuildDatePickerConstrained()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Constrained",
                    Date = new DateTime(2026, 6, 12),
                    MinDate = new DateTime(2026, 6, 1),
                    MaxDate = new DateTime(2026, 8, 31),
                    HelperText = "Only summer dates are enabled",
                },
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Floating label",
                    Placeholder = "Pick later",
                    ShrinkLabel = true,
                    HelperText = "Label remains above the field",
                },
            },
        };
    }

    private static WrapPanel BuildDatePickerStates()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Error",
                    Date = new DateTime(2026, 6, 1),
                    Error = true,
                    ErrorText = "Choose a later date",
                },
                new Loam.Controls.DatePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Disabled",
                    Date = new DateTime(2026, 7, 4),
                    IsEnabled = false,
                    HelperText = "Unavailable while archived",
                },
            },
        };
    }

    private static WrapPanel BuildTimePickerVariants()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Outlined",
                    Variant = Variant.Outlined,
                    Placeholder = "Select a time",
                    HelperText = "Bordered field style",
                },
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Filled",
                    Variant = Variant.Filled,
                    Time = new TimeSpan(8, 30, 0),
                    HelperText = "Filled field style",
                },
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Text / underline",
                    Variant = Variant.Text,
                    Time = new TimeSpan(13, 0, 0),
                    HelperText = "Underline field style",
                },
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Empty",
                    Placeholder = "Select a time",
                    HelperText = "No time selected",
                },
            },
        };
    }

    private static WrapPanel BuildTimePickerSelected()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Selected",
                    Time = new TimeSpan(9, 30, 0),
                    HelperText = "Default local time format",
                },
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "24-hour format",
                    Time = new TimeSpan(21, 45, 0),
                    TimeFormat = "HH:mm",
                    HelperText = "Custom time display",
                },
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Minute step",
                    PickerTitle = "Select reminder time",
                    Time = new TimeSpan(10, 15, 0),
                    TimeFormat = "HH:mm",
                    MinuteStep = 15,
                    CancelText = "Dismiss",
                    OkText = "Apply",
                    HelperText = "Quarter-hour choices",
                },
            },
        };
    }

    private static WrapPanel BuildTimePickerClearable()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Clearable",
                    Clearable = true,
                    Time = new TimeSpan(9, 30, 0),
                    TimeFormat = "HH:mm",
                    HelperText = "Tap the × to reset the value",
                },
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Clearable (empty)",
                    Clearable = true,
                    Placeholder = "Pick a time",
                    HelperText = "Clear button appears once a time is set",
                },
            },
        };
    }

    private static WrapPanel BuildTimePickerConstrained()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Floating label",
                    ShrinkLabel = true,
                    Placeholder = "Select a time",
                    HelperText = "Label remains visible while empty",
                },
            },
        };
    }

    private static WrapPanel BuildTimePickerStates()
    {
        return new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Error",
                    Error = true,
                    ErrorText = "Choose a time",
                },
                new Loam.Controls.TimePicker
                {
                    Width = 360,
                    Margin = new Thickness(0, 0, 24, 24),
                    Label = "Disabled",
                    Time = new TimeSpan(16, 45, 0),
                    IsEnabled = false,
                    HelperText = "Read-only state",
                },
            },
        };
    }

    private static StackPanel BuildDateTimePickers()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 360, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Loam.Controls.DatePicker { Label = "Start date", MinDate = DateTime.Today, MaxDate = DateTime.Today.AddMonths(6), HelperText = "Resting label until active" });
        stack.Children.Add(new Loam.Controls.DatePicker { Label = "Due date", Date = new DateTime(2026, 6, 30), DateFormat = "ddd, MMM d yyyy" });
        stack.Children.Add(new Loam.Controls.TimePicker { Label = "Reminder", TimeFormat = "t", ShrinkLabel = true });
        stack.Children.Add(new Loam.Controls.TimePicker { Label = "Standup", Time = new TimeSpan(9, 30, 0), TimeFormat = "HH:mm", MinuteStep = 15 });
        return stack;
    }

    private static StackPanel BuildMonthCalendarSelected()
    {
        var selected = new Text { Typo = Typo.Caption, Color = LoamColor.Secondary };
        var selectedCalendar = new MonthCalendar
        {
            DisplayMonth = new DateTime(2026, 6, 1),
            SelectedDate = new DateTime(2026, 6, 4),
            FirstDayOfWeek = DayOfWeek.Monday,
        };
        selected.Text = "Selected: Jun 4, 2026";
        selectedCalendar.DateSelected += date =>
        {
            selectedCalendar.SelectedDate = date;
            selected.Text = $"Selected: {date:MMM d, yyyy}";
        };

        return new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {                selectedCalendar,
                selected,
            },
        };
    }

    private static StackPanel BuildMonthCalendarRange()
    {
        return new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {                new MonthCalendar
                {
                    DisplayMonth = new DateTime(2026, 6, 1),
                    SelectedDate = new DateTime(2026, 6, 16),
                    RangeStart = new DateTime(2026, 6, 10),
                    RangeEnd = new DateTime(2026, 6, 16),
                    FirstDayOfWeek = DayOfWeek.Monday,
                },
            },
        };
    }

    private static StackPanel BuildMonthCalendarConstrained()
    {
        return new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {                new MonthCalendar
                {
                    DisplayMonth = new DateTime(2026, 6, 1),
                    SelectedDate = new DateTime(2026, 6, 14),
                    MinDate = new DateTime(2026, 6, 10),
                    MaxDate = new DateTime(2026, 6, 20),
                    FirstDayOfWeek = DayOfWeek.Monday,
                },
            },
        };
    }

    private static StackPanel BuildSelectSingle()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 360, HorizontalAlignment = HorizontalAlignment.Left };

        var country = new Select { Label = "Country", Placeholder = "Choose a country", HelperText = "Click anywhere in the field" };
        country.Items.Add(new SelectItem("United States", "us"));
        country.Items.Add(new SelectItem("Germany", "de"));
        country.Items.Add(new SelectItem("Japan", "jp"));
        country.Items.Add(new SelectItem("Brazil", "br"));
        stack.Children.Add(country);

        var size = new Select { Label = "Size", Value = "m", Variant = Variant.Filled };
        size.Items.Add(new SelectItem("Small", "s"));
        size.Items.Add(new SelectItem("Medium", "m"));
        size.Items.Add(new SelectItem("Large", "l"));
        stack.Children.Add(size);

        return stack;
    }

    private static Select BuildSelectMulti()
    {
        var tags = new Select { Label = "Tags", MultiSelect = true, ShrinkLabel = true, MaxWidth = 360, HorizontalAlignment = HorizontalAlignment.Left };
        tags.Items.Add(new SelectItem("Design", "design"));
        tags.Items.Add(new SelectItem("Build", "build"));
        tags.Items.Add(new SelectItem("Review", "review"));
        tags.SelectedValues.Add("design");
        tags.SelectedValues.Add("review");
        return tags;
    }

    private static Select BuildSelectStates()
    {
        return new Select
        {
            Label = "Required",
            Error = true,
            ErrorText = "Choose at least one option",
            Placeholder = "No value selected",
            MaxWidth = 360,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
    }

    private static Hidden BuildHiddenDownMode()
    {
        return new Hidden
        {
            Breakpoint = Breakpoint.Sm,
            Mode = HiddenMode.Down,
            Child = new Chip { Text = "Visible on Md and wider", Color = LoamColor.Primary },
        };
    }

    private static Hidden BuildHiddenUpMode()
    {
        return new Hidden
        {
            Breakpoint = Breakpoint.Md,
            Mode = HiddenMode.Up,
            Child = new Chip { Text = "Visible below Md", Color = LoamColor.Secondary },
        };
    }

    private static Hidden BuildHiddenOnlyMode()
    {
        return new Hidden
        {
            Breakpoint = Breakpoint.Lg,
            Mode = HiddenMode.Only,
            Child = new Chip { Text = "Hidden only at Lg", Color = LoamColor.Warning },
        };
    }

    private static Panel BuildScrollToTop()
    {
        var rows = new StackPanel { Spacing = 8, Margin = new Thickness(12) };
        for (var i = 1; i <= 12; i++)
        {
            rows.Children.Add(new Paper
            {
                Elevation = 0,
                Padding = new Thickness(12),
                Content = new Text { Text = $"Section {i}", Typo = Typo.Body2 },
            });
        }

        var scroller = new ScrollViewer
        {
            Width = 360,
            Height = 230,
            Content = rows,
        };
        scroller.Offset = new Vector(0, 120);

        var scrollToTop = new ScrollToTop
        {
            Target = scroller,
            VisibleOffset = 32,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 18, 18),
        };

        return new Panel
        {
            Width = 420,
            Height = 250,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { scroller, scrollToTop },
        };
    }

    private static StackPanel BuildList()
    {
        var list = new List();
        var status = new Text { Text = "Activate a row with pointer or keyboard.", Typo = Typo.Caption, Color = LoamColor.Secondary };
        var inbox = new ListItem
        {
            Icon = Icons.Material.Filled.Home,
            Content = "Inbox",
            SecondaryText = "24 unread",
            Action = new Badge { Value = 24 },
            IsSelected = true,
        };
        inbox.Activated += (_, _) => status.Text = "Inbox activated";
        var starred = new ListItem
        {
            Icon = Icons.Material.Filled.Star,
            Content = "Starred",
            SecondaryText = "Saved project notes",
            Action = new IconButton { Icon = Icons.Material.Filled.Settings, Size = LoamSize.Small },
        };
        starred.Activated += (_, _) => status.Text = "Starred activated";

        list.Children.Add(new ListSubheader { Text = "MAILBOXES" });
        list.Children.Add(inbox);
        list.Children.Add(starred);
        list.Children.Add(new ListSubheader { Text = "LABELS" });
        list.Children.Add(new ListItem { Icon = Icons.Material.Filled.Person, Content = "Personal" });
        list.Children.Add(new ListItem { Icon = Icons.Material.Filled.Settings, Content = "Disabled", IsEnabled = false });

        return new StackPanel
        {
            Spacing = 16,
            MaxWidth = 280,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { new Paper { Elevation = 1, Content = list }, status },
        };
    }

    private static Border BuildRipple()
    {
        var content = new Text { Text = "Click anywhere in this surface", Margin = new Thickness(24, 18), HorizontalAlignment = HorizontalAlignment.Center };
        var ripple = new Ripple { Child = content };
        var border = new Border
        {
            Child = ripple,
            CornerRadius = new CornerRadius(8),
            Width = 320,
            HorizontalAlignment = HorizontalAlignment.Left,
            ClipToBounds = true,
        };
        border.Bind(Border.BackgroundProperty, border.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.BackgroundGray))));
        return border;
    }

    private static Paper BuildPieChartThemedPie()
    {
        var split = new[] { 40d, 25d, 20d, 15d };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new PieChart { Width = 180, Height = 180, Values = split },
                    new ChartLegend { Labels = { "Planning", "Build", "Review" } },
                },
            },
        };
    }

    private static Paper BuildPieChartExplicitDonut()
    {
        var split = new[] { 40d, 25d, 20d, 15d };
        var explicitColors = new[]
        {
            Color.Parse("#355C7D"),
            Color.Parse("#6C5B7B"),
            Color.Parse("#C06C84"),
            Color.Parse("#F67280"),
        };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new PieChart { Width = 180, Height = 180, Values = split, Donut = true, Colors = explicitColors },
                    new ChartLegend { Colors = explicitColors, Labels = { "Alpha", "Beta", "Stable" } },
                },
            },
        };
    }

    private static Paper BuildBarChartThemedBars()
    {
        var revenue = new[] { 12d, 19d, 8d, 22d, 17d, 25d };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Grid and baseline colors come from outline roles.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new BarChart { Width = 320, Height = 180, Values = revenue },
                },
            },
        };
    }

    private static Paper BuildBarChartNoData()
    {
        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new BarChart { Width = 320, Height = 180, Values = [0d, -2d, 0d] },
                },
            },
        };
    }

    private static Paper BuildLineChartLine()
    {
        var revenue = new[] { 12d, 19d, 8d, 22d, 17d, 25d };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new LineChart { Width = 320, Height = 180, Values = revenue },
                },
            },
        };
    }

    private static Paper BuildLineChartArea()
    {
        var revenue = new[] { 12d, 19d, 8d, 22d, 17d, 25d };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Area fill follows the first resolved series color.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new LineChart { Width = 320, Height = 180, Values = revenue, Area = true },
                },
            },
        };
    }

    private static Paper BuildPieChartCenterTotal()
    {
        var split = new[] { 540d, 320d, 380d };
        var labels = new[] { "Desktop", "Browser", "Mobile" };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "The donut hole shows a KPI total drawn from the data.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new PieChart
                    {
                        Width = 180,
                        Height = 180,
                        Donut = true,
                        Values = split,
                        Labels = labels,
                        CenterValueFormat = "N0",
                        CenterSubText = "sessions",
                    },
                    new ChartLegend { Labels = { "Desktop", "Browser", "Mobile" } },
                },
            },
        };
    }

    private static Paper BuildBarChartGrouped()
    {
        var labels = new[] { "Q1", "Q2", "Q3", "Q4" };
        var web = new[] { 30d, 45d, 28d, 60d };
        var mobile = new[] { 18d, 22d, 35d, 30d };
        var series = new[] { new ChartSeries(web, "Web"), new ChartSeries(mobile, "Mobile") };
        var chart = new BarChart { Width = 340, Height = 200, Labels = labels, Series = series, ShowAxes = true };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Series render side-by-side per category; the legend binds to the chart.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    chart,
                    new ChartLegend { Source = chart },
                },
            },
        };
    }

    private static Paper BuildBarChartStacked()
    {
        var labels = new[] { "Q1", "Q2", "Q3", "Q4" };
        var web = new[] { 30d, 45d, 28d, 60d };
        var mobile = new[] { 18d, 22d, 35d, 30d };
        var kiosk = new[] { 10d, 14d, 9d, 20d };
        var series = new[] { new ChartSeries(web, "Web"), new ChartSeries(mobile, "Mobile"), new ChartSeries(kiosk, "Kiosk") };
        var chart = new BarChart { Width = 340, Height = 200, Labels = labels, Series = series, StackMode = BarStackMode.Stacked, ShowAxes = true };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "StackMode = Stacked layers series; StackedPercent normalizes each category to 100%.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    chart,
                    new ChartLegend { Source = chart },
                },
            },
        };
    }

    private static Paper BuildLineChartSeries()
    {
        var labels = new[] { "Q1", "Q2", "Q3", "Q4" };
        var web = new[] { 30d, 45d, 28d, 60d };
        var mobile = new[] { 18d, 22d, 35d, 30d };
        var series = new[] { new ChartSeries(web, "Web"), new ChartSeries(mobile, "Mobile") };
        var chart = new LineChart { Width = 340, Height = 200, Labels = labels, Series = series, ShowAxes = true };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Each series is its own line and color; the legend binds to the chart.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    chart,
                    new ChartLegend { Source = chart },
                },
            },
        };
    }

    private static Paper BuildBarChartBound()
    {
        var data = new ObservableCollection<(string Label, double Value)>
        {
            ("Mon", 12d),
            ("Tue", 19d),
            ("Wed", 8d),
            ("Thu", 22d),
        };
        var day = data.Count;

        var chart = new BarChart
        {
            Width = 320,
            Height = 180,
            ShowAxes = true,
            ItemsSource = data,
            ValueSelector = o => ((ValueTuple<string, double>)o).Item2,
            LabelSelector = o => ((ValueTuple<string, double>)o).Item1,
        };

        var add = new LoamButton { Content = "Add point", Variant = Variant.Outlined };
        add.Click += (_, _) =>
        {
            day++;
            data.Add(($"D{day}", 8 + day * 4));
        };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Bound to an ObservableCollection via selectors; adding items updates the chart live.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    chart,
                    add,
                },
            },
        };
    }

    private static Paper BuildBarChartAxes()
    {
        var revenue = new[] { 30d, 45d, 28d, 60d, 42d };
        var labels = new[] { "Q1", "Q2", "Q3", "Q4", "Q5" };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "ShowAxes adds a nice-number Y-axis and a category X-axis.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new BarChart { Width = 340, Height = 200, Values = revenue, Labels = labels, ShowAxes = true, YAxisFormat = v => $"${v:N0}k" },
                },
            },
        };
    }

    private static Paper BuildLineChartAxes()
    {
        var trend = new[] { 30d, 45d, 28d, 60d, 42d, 70d };
        var labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Axes turn the line into a readable, scaled plot.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new LineChart { Width = 340, Height = 200, Values = trend, Labels = labels, ShowAxes = true, Area = true },
                },
            },
        };
    }

    private static Paper BuildBarChartSigned()
    {
        var netFlow = new[] { 12d, -5d, 8d, -3d, 15d };
        var labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "AllowNegative draws bars above and below a zero baseline.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new BarChart
                    {
                        Width = 320,
                        Height = 180,
                        AllowNegative = true,
                        Values = netFlow,
                        Labels = labels,
                    },
                },
            },
        };
    }

    private static Paper BuildLineChartSigned()
    {
        var variance = new[] { 4d, -2d, 6d, -1d, 3d, -4d };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Signed line values plot around a zero baseline; the area fills to it.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new LineChart
                    {
                        Width = 320,
                        Height = 180,
                        AllowNegative = true,
                        Area = true,
                        Values = variance,
                    },
                },
            },
        };
    }

    private static Paper BuildPieChartDataLabels()
    {
        var split = new[] { 45d, 30d, 25d };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "ShowDataLabels writes each slice's share at its centroid.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new PieChart { Width = 200, Height = 200, Donut = true, Values = split, ShowDataLabels = true },
                },
            },
        };
    }

    private static Paper BuildBarChartDataLabels()
    {
        var revenue = new[] { 12d, 19d, 8d, 22d, 17d };
        var labels = new[] { "Q1", "Q2", "Q3", "Q4", "Q5" };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Values are drawn above each bar; colliding labels are thinned out.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new BarChart { Width = 320, Height = 180, Values = revenue, Labels = labels, ShowDataLabels = true },
                },
            },
        };
    }

    private static Paper BuildBarChartInteractive()
    {
        var revenue = new[] { 12d, 19d, 8d, 22d, 17d };
        var labels = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" };
        var status = new Text { Text = "Hover a bar for a tooltip, or click one.", Typo = Typo.Caption, Color = LoamColor.Secondary };

        var chart = new BarChart { Width = 320, Height = 180, Values = revenue, Labels = labels };
        chart.HoverChanged += (_, e) =>
            status.Text = e.Point is { } p ? $"Hovering {p.Label}: {p.Value:N0}" : "Hover a bar for a tooltip, or click one.";
        chart.PointClicked += (_, e) =>
        {
            if (e.Point is { } p)
            {
                status.Text = $"Clicked {p.Label}: {p.Value:N0}";
            }
        };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Tooltips show on hover (on by default); HoverChanged/PointClicked drive the caption below.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    chart,
                    status,
                },
            },
        };
    }

    private static Paper BuildLineChartDataLabels()
    {
        var revenue = new[] { 12d, 19d, 8d, 22d, 17d, 25d };

        return new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = "Point values are drawn above the line, with overlap thinning.", Typo = Typo.Caption, Color = LoamColor.Secondary },
                    new LineChart { Width = 320, Height = 180, Values = revenue, ShowDataLabels = true },
                },
            },
        };
    }

    private static StackPanel BuildCharts()
    {
        var revenue = new[] { 12d, 19d, 8d, 22d, 17d, 25d };
        var split = new[] { 40d, 25d, 20d, 15d };
        var explicitColors = new[]
        {
            Color.Parse("#355C7D"),
            Color.Parse("#6C5B7B"),
            Color.Parse("#C06C84"),
            Color.Parse("#F67280"),
        };

        Control ChartPanel(string title, string caption, Control chart, Control legend) => new Paper
        {
            Outlined = true,
            Elevation = 0,
            Padding = new Thickness(16),
            Width = 360,
            Margin = new Thickness(0, 0, 16, 16),
            Content = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Text { Text = title, Typo = Typo.Subtitle2 },
                    new Text { Text = caption, Typo = Typo.Caption, Color = LoamColor.Secondary, TextWrapping = TextWrapping.Wrap },
                    chart,
                    legend,
                },
            },
        };

        return new StackPanel
        {
            Children =
            {
                new WrapPanel
                {
                    Children =
                    {
                        ChartPanel(
                            "Themed pie",
                            "Default series colors resolve from the active theme.",
                            new PieChart { Width = 180, Height = 180, Values = split },
                            new ChartLegend { Labels = { "Planning", "Build", "Review" } }),
                        ChartPanel(
                            "Themed donut",
                            "The center hole uses the current surface role.",
                            new PieChart { Width = 180, Height = 180, Values = split, Donut = true },
                            new ChartLegend { Labels = { "Desktop", "Browser", "Mobile" } }),
                        ChartPanel(
                            "Themed bars",
                            "Grid and baseline colors come from outline roles.",
                            new BarChart { Width = 280, Height = 160, Values = revenue },
                            new ChartLegend { Labels = { "Q1", "Q2", "Q3" } }),
                        ChartPanel(
                            "Line and area",
                            "Area fill follows the first resolved series color.",
                            new LineChart { Width = 280, Height = 160, Values = revenue, Area = true },
                            new ChartLegend { Labels = { "Release readiness" } }),
                    },
                },
                new WrapPanel
                {
                    Children =
                    {
                        ChartPanel(
                            "Explicit colors",
                            "Custom series colors still override theme roles.",
                            new PieChart { Width = 180, Height = 180, Values = split, Donut = true, Colors = explicitColors },
                            new ChartLegend { Colors = explicitColors, Labels = { "Alpha", "Beta", "Stable" } }),
                        ChartPanel(
                            "No data",
                            "Empty and zero-only charts render a visible empty state.",
                            new BarChart { Width = 280, Height = 160, Values = [0d, -2d, 0d] },
                            new ChartLegend { ShowSwatches = false, Labels = { "No data" } }),
                    },
                },
            },
        };
    }

    private static Card BuildCard()
    {
        return new Card
        {
            Width = 360,
            Elevation = 1,
            Outlined = true,
            HorizontalAlignment = HorizontalAlignment.Left,
            HeaderAvatar = new Avatar { Content = "PL", Color = LoamColor.Primary },
            Title = "Release board",
            Subtitle = "Updated today",
            HeaderAction = new IconButton { Icon = Icons.Material.Filled.Settings },
            ShowMedia = true,
            MediaHeight = 96,
            BodyText = "Inputs, pickers, and surfaces are ready for review.",
            SecondaryActionText = "Details",
            PrimaryActionText = "Open",
        };
    }

    private static WrapPanel BuildPaperElevation()
    {
        var wrap = new WrapPanel();

        wrap.Children.Add(new Paper
        {
            Width = 130,
            Height = 88,
            Margin = new Thickness(0, 0, 16, 16),
            Title = "Elevation 1",
            Subtitle = "Default",
            Compact = true,
        });

        wrap.Children.Add(new Paper
        {
            Width = 130,
            Height = 88,
            Margin = new Thickness(0, 0, 16, 16),
            Title = "Elevation 4",
            Subtitle = "Tonal",
            Compact = true,
            Elevation = 4,
        });

        wrap.Children.Add(new Paper
        {
            Width = 130,
            Height = 88,
            Margin = new Thickness(0, 0, 16, 16),
            Title = "Elevation 8",
            Subtitle = "Clamped",
            Compact = true,
            Elevation = 8,
            Shape = SurfaceShape.Large,
        });

        return wrap;
    }

    private static Paper BuildPaperOutlined()
    {
        return new Paper
        {
            Width = 130,
            Height = 88,
            Margin = new Thickness(0, 0, 16, 16),
            Title = "Outlined",
            Subtitle = "Stroke",
            Compact = true,
            Outlined = true,
        };
    }

    private static Paper BuildPaperSquare()
    {
        return new Paper
        {
            Width = 130,
            Height = 88,
            Margin = new Thickness(0, 0, 16, 16),
            Title = "Square",
            Subtitle = "No radius",
            Compact = true,
            Elevation = 4,
            Square = true,
        };
    }

    private static Paper BuildPaperColored()
    {
        return new Paper
        {
            Width = 130,
            Height = 88,
            Margin = new Thickness(0, 0, 16, 16),
            Title = "Colored",
            Subtitle = "Role tint",
            Compact = true,
            Color = LoamColor.Primary,
            Shape = SurfaceShape.ExtraLarge,
        };
    }

    private static StackPanel BuildDividerHorizontal()
    {
        var horizontal = new StackPanel
        {
            Spacing = 12,
            Width = 520,
            Children =
            {
                new Text { Text = "Full width", Typo = Typo.BodyMedium },
                new Divider(),
                new Text { Text = "Inset", Typo = Typo.BodyMedium },
                new Divider { DividerType = DividerType.Inset },
                new Text { Text = "Middle inset", Typo = Typo.BodyMedium },
                new Divider { DividerType = DividerType.Middle },
                new Text { Text = "Light", Typo = Typo.BodyMedium },
                new Divider { Light = true },
            },
        };

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                horizontal,
            },
        };
    }

    private static StackPanel BuildDividerVertical()
    {
        var vertical = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Height = 72,
            Spacing = 16,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Text { Text = "Full", Typo = Typo.BodyMedium, VerticalAlignment = VerticalAlignment.Center },
                new Divider { Vertical = true },
                new Text { Text = "Inset", Typo = Typo.BodyMedium, VerticalAlignment = VerticalAlignment.Center },
                new Divider { Vertical = true, DividerType = DividerType.Inset },
                new Text { Text = "Middle", Typo = Typo.BodyMedium, VerticalAlignment = VerticalAlignment.Center },
                new Divider { Vertical = true, DividerType = DividerType.Middle },
                new Text { Text = "Light", Typo = Typo.BodyMedium, VerticalAlignment = VerticalAlignment.Center },
                new Divider { Vertical = true, Light = true },
            },
        };

        return new StackPanel
        {
            Spacing = 8,
            MaxWidth = 760,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                vertical,
            },
        };
    }
}
