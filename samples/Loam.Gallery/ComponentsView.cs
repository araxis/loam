using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
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

    private readonly IReadOnlyList<GalleryPage> _pages;
    private readonly Dictionary<string, NavLink> _links = [];
    private readonly ContentControl _pageHost = new() { HorizontalAlignment = HorizontalAlignment.Stretch };

    public ComponentsView()
    {
        _pages = BuildPageCatalog();

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
        var brand = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Icon { Data = Icons.Material.Filled.Star, Color = LoamColor.Primary, Size = LoamSize.Medium },
                new Text { Text = "Loam Gallery", Typo = Typo.H6, VerticalAlignment = VerticalAlignment.Center },
                new Chip { Text = "component lab", Variant = Variant.Outlined, Color = LoamColor.Primary, Size = LoamSize.Small },
            },
        };

        var theme = new LoamButton
        {
            Content = "Theme",
            StartIcon = Icons.Material.Filled.Settings,
            Variant = Variant.Outlined,
            Color = LoamColor.Primary,
            Size = LoamSize.Small,
        };
        theme.Click += (_, _) => ToggleTheme();

        brand.Margin = new Thickness(24, 0);
        brand.Children.Add(new Chip { Text = "live controls", Color = LoamColor.Success, Size = LoamSize.Small });
        brand.Children.Add(theme);

        var bar = new Border
        {
            Height = 72,
            Child = brand,
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

    private sealed record GalleryPage(string Group, string Title, string Description, Func<Control> Build, string Code);

    private static GalleryPage Page(string group, string title, string description, Func<Control> build) =>
        new(group, title, description, build, SnippetFor(title));

    private static IReadOnlyList<GalleryPage> BuildPageCatalog() =>
    [
        Page("Start", "Overview", "A composed screen built from the same public controls used on the component pages.", BuildOverview),

        Page("Display", "Text", "Typography, color, spacing, and alignment.", BuildText),
        Page("Display", "Icon", "Vector icon rendering with semantic colors and sizes.", BuildIcons),
        Page("Display", "Divider", "Horizontal and vertical dividers with token colors.", BuildDivider),
        Page("Display", "Chip", "Compact labels, icons, close affordances, and variants.", BuildChips),
        Page("Display", "ChipSet", "Selectable single and multi-select chip groups.", BuildChipSet),
        Page("Display", "Badge", "Numeric and dot badges positioned around child content.", BuildBadges),
        Page("Display", "Avatar", "Initials, icon avatars, sizes, colors, and shapes.", BuildAvatars),
        Page("Display", "AvatarGroup", "Grouped avatars with overflow count behavior.", BuildAvatars),

        Page("Buttons", "Button", "Filled, outlined, text, color, size, disabled, and icon buttons.", BuildButtons),
        Page("Buttons", "IconButton", "Icon-only actions in default, filled, and outlined variants.", BuildIconButtons),
        Page("Buttons", "ToggleIconButton", "Two-state icon action with a separate toggled color.", BuildToggleIconButton),
        Page("Buttons", "ButtonGroup", "Connected button segments with shared variant and color.", BuildButtonGroup),
        Page("Buttons", "Fab", "Floating action buttons with icon-only and label modes.", BuildFabs),
        Page("Buttons", "Menu", "Button-anchored menu rows.", BuildTabsMenu),

        Page("Inputs", "Field", "A reusable field shell for custom input-like content.", BuildField),
        Page("Inputs", "TextField", "Text, numeric, masked, and autocomplete field examples.", BuildTextFields),
        Page("Inputs", "NumericField", "Numeric parsing, formatting, bounds, and spinner controls.", BuildTextFields),
        Page("Inputs", "MaskedTextField", "Pattern-based text formatting for phone-style entry.", BuildTextFields),
        Page("Inputs", "Autocomplete", "Text entry with filtered suggestions.", BuildTextFields),
        Page("Inputs", "Select", "Single and multi-select dropdowns.", BuildSelect),
        Page("Inputs", "CheckBox", "Checkbox states, colors, and disabled rendering.", BuildInputs),
        Page("Inputs", "Switch", "On/off switch states and colors.", BuildInputs),
        Page("Inputs", "Radio", "Radio choices coordinated by a radio group.", BuildRadioSlider),
        Page("Inputs", "RadioGroup", "Grouped single-choice selection.", BuildRadioSlider),
        Page("Inputs", "Slider", "Pointer-driven range selection.", BuildRadioSlider),
        Page("Inputs", "Rating", "Interactive and read-only star ratings.", BuildRating),
        Page("Inputs", "ToggleGroup", "Segmented single selection.", BuildToggleGroup),
        Page("Inputs", "FileUpload", "Platform file picking and selected-name chips.", BuildFileUpload),
        Page("Inputs", "Form", "Lightweight validation over text-field descendants.", BuildFormDemo),

        Page("Pickers", "DatePicker", "Date input with a calendar flyout.", BuildDateTimePickers),
        Page("Pickers", "TimePicker", "Time input with hour and minute columns.", BuildDateTimePickers),
        Page("Pickers", "DateRangePicker", "Two-click date range selection.", BuildDateRangePicker),
        Page("Pickers", "ColorPicker", "Swatch picker with hex display.", BuildColorPicker),
        Page("Pickers", "MonthCalendar", "Standalone month grid used by date pickers.", BuildMonthCalendar),

        Page("Feedback", "Alert", "Contextual message banners across variants and severities.", BuildAlert),
        Page("Feedback", "ProgressCircular", "Determinate and indeterminate circular progress.", BuildProgress),
        Page("Feedback", "ProgressLinear", "Determinate and indeterminate linear progress.", BuildProgress),
        Page("Feedback", "Skeleton", "Animated and static loading placeholders.", BuildProgress),
        Page("Feedback", "Overlay", "Auto-closing scrim over local content.", BuildOverlayScrim),
        Page("Feedback", "Popover", "Anchored floating content.", BuildPopover),
        Page("Feedback", "DialogService", "Confirm, action, and message dialogs.", BuildOverlays),
        Page("Feedback", "SnackbarService", "Toast messages with colors and actions.", BuildOverlays),

        Page("Data", "SimpleTable", "Small tabular datasets with hover and stripe options.", BuildTable),
        Page("Data", "DataGrid", "Typed sortable, pageable, filterable data grid.", BuildDataGrid),
        Page("Data", "TreeView", "Nested rows with selection and expansion.", BuildTreeView),
        Page("Data", "Tabs", "Header strip and selected content region.", BuildTabsMenu),
        Page("Data", "ExpansionPanels", "Accordion-style expandable content.", BuildExpansionPanels),
        Page("Data", "Collapse", "Animated and static content reveal.", BuildCollapse),
        Page("Data", "Timeline", "Vertical event sequence.", BuildTimeline),
        Page("Data", "Carousel", "Slide navigation with arrows and bullets.", BuildCarousel),
        Page("Data", "Stepper", "Multi-step workflow navigation.", BuildStepper),
        Page("Data", "Pagination", "Page buttons with boundary and ellipsis behavior.", BuildPagination),

        Page("Navigation", "Breadcrumbs", "Path navigation with current item text.", BuildNavigation),
        Page("Navigation", "Link", "Clickable text link variants.", BuildNavigation),
        Page("Navigation", "NavMenu", "Side-menu container with links and groups.", BuildNavigation),
        Page("Navigation", "NavLink", "Active and hoverable navigation rows.", BuildNavigation),
        Page("Navigation", "NavGroup", "Collapsible navigation groups.", BuildNavigation),

        Page("Layout", "Container", "Centered and width-capped content regions.", BuildLayoutSamples),
        Page("Layout", "Grid", "Responsive 12-column layout with item spans.", BuildLayoutSamples),
        Page("Layout", "Item", "Grid child span settings across breakpoints.", BuildLayoutSamples),
        Page("Layout", "Stack", "Spaced row and column layout.", BuildLayoutSamples),
        Page("Layout", "Spacer", "Flexible space for toolbars and docked rows.", BuildList),
        Page("Layout", "Hidden", "Breakpoint-based visibility.", BuildHidden),
        Page("Layout", "ScrollToTop", "Floating scroll affordance used in this app shell.", BuildHidden),

        Page("Shell", "Layout", "App shell composition with bar, drawer, and content.", BuildShellPreview),
        Page("Shell", "AppBar", "Elevated top application bar.", BuildShellPreview),
        Page("Shell", "Drawer", "Docked or temporary side navigation.", BuildShellPreview),
        Page("Shell", "MainContent", "Scrollable main content region.", BuildShellPreview),

        Page("Surfaces", "Paper", "Elevation, outlined, square, and filled surfaces.", BuildPaper),
        Page("Surfaces", "Card", "Header, media, content, and actions.", BuildCard),
        Page("Surfaces", "List", "List rows, subheaders, and spacer-driven toolbar layout.", BuildList),
        Page("Surfaces", "Ripple", "Pointer feedback effect.", BuildRipple),

        Page("Charts", "PieChart", "Pie and donut chart rendering.", BuildCharts),
        Page("Charts", "BarChart", "Bar chart rendering from numeric values.", BuildCharts),
        Page("Charts", "LineChart", "Line and area chart rendering.", BuildCharts),
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
                _links[page.Title] = link;
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
            link.Value.IsActive = string.Equals(link.Key, page.Title, StringComparison.Ordinal);
        }
    }

    private static StackPanel BuildArticle(GalleryPage page)
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
                new Text { Text = page.Description, Typo = Typo.Body1, Color = LoamColor.Default, Opacity = 0.72, TextWrapping = TextWrapping.Wrap, MaxWidth = 560 },
                new WrapPanel
                {
                    Children =
                    {
                        new Chip { Text = page.Group, Color = LoamColor.Primary, Margin = new Thickness(0, 0, 8, 8) },
                        new Chip { Text = "Live preview", Variant = Variant.Outlined, Color = LoamColor.Success, Margin = new Thickness(0, 0, 8, 8) },
                        new Chip { Text = "C# sample", Variant = Variant.Outlined, Color = LoamColor.Info, Margin = new Thickness(0, 0, 8, 8) },
                    },
                },
            },
        };

        return new StackPanel
        {
            Margin = new Thickness(32, 28, 32, 40),
            Spacing = 22,
            Children = { header, BuildPreviewPanel(page), BuildCodePanel(page.Code) },
        };
    }

    private static Paper BuildPreviewPanel(GalleryPage page) =>
        new()
        {
            Elevation = 2,
            Padding = new Thickness(28),
            Content = new StackPanel
            {
                Spacing = 18,
                Children =
                {
                    new Text { Text = "Preview", Typo = Typo.Subtitle1 },
                    new Divider(),
                    page.Build(),
                },
            },
        };

    private static Paper BuildCodePanel(string code)
    {
        var block = new TextBlock
        {
            Text = code,
            FontFamily = new FontFamily("Cascadia Mono, Consolas, monospace"),
            FontSize = 13,
            LineHeight = 20,
            TextWrapping = TextWrapping.NoWrap,
        };
        block.Bind(TextBlock.ForegroundProperty, block.GetResourceObservable(LoamTokens.TextPrimary));

        return new Paper
        {
            Elevation = 1,
            Outlined = true,
            Padding = new Thickness(0),
            Content = new DockPanel
            {
                LastChildFill = true,
                Children =
                {
                    CodeHeader(),
                    new ScrollViewer
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Padding = new Thickness(18),
                        Content = block,
                    },
                },
            },
        };
    }

    private static Border CodeHeader()
    {
        var header = new Border
        {
            Padding = new Thickness(14, 10),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new Icon { Data = Icons.Material.Filled.Edit, Size = LoamSize.Small, Color = LoamColor.Default },
                    new Text { Text = "C# sample", Typo = Typo.Subtitle2, VerticalAlignment = VerticalAlignment.Center },
                },
            },
        };
        header.Bind(Border.BackgroundProperty, header.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.BackgroundGray))));
        DockPanel.SetDock(header, Dock.Top);
        return header;
    }

    private static string IconForGroup(string group) => group switch
    {
        "Start" => Icons.Material.Filled.Home,
        "Buttons" => Icons.Material.Filled.Check,
        "Inputs" => Icons.Material.Filled.Edit,
        "Pickers" => Icons.Material.Filled.CalendarToday,
        "Feedback" => Icons.Material.Filled.Settings,
        "Data" => Icons.Material.Filled.Menu,
        "Navigation" => Icons.Material.Filled.Menu,
        "Layout" => Icons.Material.Filled.Star,
        "Shell" => Icons.Material.Filled.Home,
        "Surfaces" => Icons.Material.Filled.Home,
        "Charts" => Icons.Material.Filled.Star,
        _ => Icons.Material.Filled.Favorite,
    };

    private static string IconForPage(string title) => title switch
    {
        "Field" or "TextField" or "NumericField" or "MaskedTextField" or "Autocomplete" => Icons.Material.Filled.Edit,
        "Select" or "Menu" or "NavGroup" => Icons.Material.Filled.ExpandMore,
        "DatePicker" or "TimePicker" or "DateRangePicker" => Icons.Material.Filled.CalendarToday,
        "ColorPicker" => Icons.Material.Filled.Favorite,
        "FileUpload" => Icons.Material.Filled.CloudUpload,
        "DialogService" or "SnackbarService" or "Overlay" or "Popover" => Icons.Material.Filled.Settings,
        "DataGrid" or "SimpleTable" or "TreeView" => Icons.Material.Filled.Menu,
        "Button" or "IconButton" or "ToggleIconButton" or "ButtonGroup" or "Fab" => Icons.Material.Filled.Check,
        _ => Icons.Material.Filled.Star,
    };

    private static string SnippetFor(string title) => title switch
    {
        "Overview" => """
            var board = new Loam.Controls.Grid { Spacing = 18 };
            board.Children.Add(new Item
            {
                Xs = 12,
                Md = 7,
                Child = new StackPanel
                {
                    Children =
                    {
                        new Alert { Color = LoamColor.Success, Content = "Live controls" },
                        new TextField { Label = "Project", Text = "Component audit" },
                        new Select { Label = "Priority", Value = "high" },
                    },
                },
            });
            """,
        "Field" => """
            var phone = new Field
            {
                Label = "Phone",
                HelperText = "Custom phone editor",
                StartAdornment = new TextBlock { Text = "+1" },
                Content = new TextBox
                {
                    PlaceholderText = "(555) 123-4567",
                    BorderThickness = default,
                    Background = Brushes.Transparent,
                    Padding = default,
                },
            };
            """,
        "TextField" => """
            var field = new TextField
            {
                Label = "Email",
                Placeholder = "you@example.com",
                Variant = Variant.Outlined,
                Required = true,
            };
            """,
        "NumericField" => """
            var quantity = new NumericField
            {
                Label = "Quantity",
                Minimum = 0,
                Maximum = 99,
                Step = 1,
                Value = 3,
            };
            """,
        "MaskedTextField" => """
            var phone = new MaskedTextField
            {
                Label = "Phone",
                Pattern = "(###) ###-####",
                Placeholder = "(555) 123-4567",
            };
            """,
        "Autocomplete" => """
            var autocomplete = new Autocomplete
            {
                Label = "Fruit",
                Items = { "Apple", "Banana", "Grape" },
            };
            """,
        "Select" => """
            var select = new Select { Label = "Country" };
            select.Items.Add(new SelectItem("United States", "us"));
            select.Items.Add(new SelectItem("Germany", "de"));
            select.Value = "us";
            """,
        "CheckBox" => """new Loam.Controls.CheckBox { Content = "Primary", IsChecked = true, Color = LoamColor.Primary };""",
        "Switch" => """new Switch { Content = "Enabled", IsChecked = true, Color = LoamColor.Success };""",
        "Radio" or "RadioGroup" => """
            var group = new RadioGroup
            {
                Value = "b",
                Child = new Stack
                {
                    Row = true,
                    Children =
                    {
                        new Radio { Content = "One", Value = "a" },
                        new Radio { Content = "Two", Value = "b" },
                    },
                },
            };
            """,
        "Slider" => """new Loam.Controls.Slider { Minimum = 0, Maximum = 100, Value = 40 };""",
        "Rating" => """new Rating { SelectedValue = 4, MaxValue = 5, Color = LoamColor.Warning };""",
        "ToggleGroup" => """
            var group = new ToggleGroup { SelectedValue = "week" };
            group.Items.Add(new ToggleItem("Day", "day"));
            group.Items.Add(new ToggleItem("Week", "week"));
            """,
        "FileUpload" => """new FileUpload { ButtonText = "Attach files", AllowMultiple = true };""",
        "Form" => """
            var form = new Form
            {
                Child = new StackPanel
                {
                    Children = { new TextField { Label = "Name", Required = true } },
                },
            };
            form.Validate();
            """,
        "DatePicker" => """new Loam.Controls.DatePicker { Label = "Start date", MinDate = DateTime.Today };""",
        "TimePicker" => """new Loam.Controls.TimePicker { Label = "Reminder", MinuteStep = 15 };""",
        "DateRangePicker" => """new Loam.Controls.DateRangePicker { Label = "Trip dates" };""",
        "ColorPicker" => """new Loam.Controls.ColorPicker { Label = "Accent", ShowAlpha = true };""",
        "MonthCalendar" => """
            var calendar = new MonthCalendar
            {
                DisplayMonth = new DateTime(2026, 6, 1),
                SelectedDate = new DateTime(2026, 6, 4),
            };
            """,
        "Alert" => """new Alert { Color = LoamColor.Success, Variant = Variant.Filled, Content = "Saved" };""",
        "ProgressCircular" => """new ProgressCircular { Indeterminate = false, Value = 70, Color = LoamColor.Success };""",
        "ProgressLinear" => """new ProgressLinear { Value = 60, Width = 320, Color = LoamColor.Primary };""",
        "Skeleton" => """new Skeleton { Width = 140, Animate = true };""",
        "Overlay" => """new Overlay { DarkBackground = true, AutoClose = true, Content = new ProgressCircular() };""",
        "Popover" => """new Popover { Target = button, Open = true, Content = new Text { Text = "Details" } };""",
        "DialogService" => """var confirmed = await DialogService.For(control).ConfirmAsync("Delete item?", "This cannot be undone.");""",
        "SnackbarService" => """SnackbarService.For(control).Add("Saved", LoamColor.Success);""",
        "Button" => """new Loam.Controls.Button { Content = "Save", StartIcon = Icons.Material.Filled.Check, Variant = Variant.Filled };""",
        "IconButton" => """new IconButton { Icon = Icons.Material.Filled.Edit, Variant = Variant.Outlined };""",
        "ToggleIconButton" => """new ToggleIconButton { Icon = Icons.Material.Filled.FavoriteBorder, ToggledIcon = Icons.Material.Filled.Favorite };""",
        "ButtonGroup" => """
            var group = new ButtonGroup { Variant = Variant.Outlined };
            group.Items.Add(new Loam.Controls.Button { Content = "Left" });
            group.Items.Add(new Loam.Controls.Button { Content = "Right" });
            """,
        "Fab" => """new Fab { Label = "Add", StartIcon = Icons.Material.Filled.Add, Color = LoamColor.Primary };""",
        "Menu" => """
            var menu = new Loam.Controls.Menu { Content = "Open menu" };
            menu.Items.Add(new Loam.Controls.MenuItem { Text = "Settings", Icon = Icons.Material.Filled.Settings });
            """,
        "Text" => """new Text { Text = "Body text", Typo = Typo.Body1, Color = LoamColor.Primary };""",
        "Icon" => """new Icon { Data = Icons.Material.Filled.Star, Color = LoamColor.Warning, Size = LoamSize.Large };""",
        "Divider" => """new Divider();""",
        "Chip" => """new Chip { Text = "Outlined", Variant = Variant.Outlined, Color = LoamColor.Primary };""",
        "ChipSet" => """
            var set = new ChipSet { Selectable = true, MultiSelect = true };
            set.Items.Add(new Chip { Text = "Open" });
            set.SelectedIndexes.Add(0);
            """,
        "Badge" => """new Badge { Value = 4, Color = LoamColor.Error, Content = new Icon { Data = Icons.Material.Filled.Favorite } };""",
        "Avatar" => """new Avatar { Content = "AB", Color = LoamColor.Primary };""",
        "AvatarGroup" => """
            var group = new AvatarGroup { Max = 3 };
            group.Items.Add(new Avatar { Content = "AB" });
            group.Items.Add(new Avatar { Content = "CD" });
            """,
        "SimpleTable" => """
            var table = new SimpleTable { Striped = true, Hover = true };
            table.Headers.Add("Dessert");
            table.Rows.Add(new TableRow("Cupcake", 305));
            """,
        "DataGrid" => """var grid = new Loam.Controls.DataGrid<Dessert> { PageSize = 4, Striped = true, Hover = true };""",
        "TreeView" => """
            var tree = new Loam.Controls.TreeView();
            tree.Items.Add(new Loam.Controls.TreeViewItem { Text = "Components", Expanded = true });
            """,
        "Tabs" => """
            var tabs = new Tabs();
            tabs.Items.Add(new Loam.Controls.TabItem("Overview", new Text { Text = "Content" }));
            """,
        "ExpansionPanels" => """
            var panels = new ExpansionPanels();
            panels.Panels.Add(new ExpansionPanel { Header = "Details", IsExpanded = true, Content = new Text { Text = "More" } });
            """,
        "Collapse" => """new Collapse { Expanded = true, Child = new Text { Text = "Revealed content" } };""",
        "Timeline" => """var timeline = new Timeline { Items = { new TimelineItem("Created", LoamColor.Primary) } };""",
        "Carousel" => """var carousel = new Loam.Controls.Carousel { Items = { new CarouselItem(new Text { Text = "Slide" }) } };""",
        "Stepper" => """var stepper = new Stepper { Steps = { new Step("Account", new Text { Text = "Create account" }) } };""",
        "Pagination" => """new Pagination { Count = 10, Selected = 2 };""",
        "Breadcrumbs" => """
            var crumbs = new Breadcrumbs();
            crumbs.Items.Add(new BreadcrumbItem("Home", () => { }));
            crumbs.Items.Add(new BreadcrumbItem("Components"));
            """,
        "Link" => """new Link { Text = "Open guide", OnClick = () => { } };""",
        "NavMenu" or "NavLink" or "NavGroup" => """
            var nav = new NavMenu();
            nav.Children.Add(new NavLink { Icon = Icons.Material.Filled.Home, Content = "Home", IsActive = true });
            nav.Children.Add(new NavGroup { Title = "Admin", Expanded = true });
            """,
        "Container" or "Grid" or "Item" or "Stack" => """
            var grid = new Loam.Controls.Grid { Spacing = 12 };
            grid.Children.Add(new Item { Xs = 12, Md = 4, Child = new Paper { Content = new Text { Text = "Card" } } });
            """,
        "Spacer" => """new DockPanel { Children = { new Text { Text = "Left" }, new Spacer(), new IconButton { Icon = Icons.Material.Filled.Settings } } };""",
        "Hidden" => """new Hidden { Breakpoint = Breakpoint.Sm, Mode = HiddenMode.Down, Child = new Chip { Text = "Md+" } };""",
        "ScrollToTop" => """new ScrollToTop { Target = scrollViewer, VisibleOffset = 200 };""",
        "Layout" or "AppBar" or "Drawer" or "MainContent" => """
            new Layout
            {
                AppBar = new AppBar { Content = new Text { Text = "Workspace" } },
                Drawer = new Drawer { Content = new NavMenu() },
                Content = new MainContent { Content = new Text { Text = "Content" } },
            };
            """,
        "Paper" => """new Paper { Elevation = 4, Padding = new Thickness(16), Content = new Text { Text = "Surface" } };""",
        "Card" => """
            new Card
            {
                Content = new StackPanel
                {
                    Children = { new CardHeader { Title = "Project" }, new CardContent { Child = new Text { Text = "Body" } } },
                },
            };
            """,
        "List" => """new List { Children = { new ListSubheader { Text = "MAILBOXES" }, new ListItem { Content = "Inbox" } } };""",
        "Ripple" => """new Ripple { Child = new Text { Text = "Click surface" } };""",
        "PieChart" => """new PieChart { Width = 180, Height = 180, Values = [40d, 25d, 20d, 15d], Donut = true };""",
        "BarChart" => """new BarChart { Width = 280, Height = 160, Values = [12d, 19d, 8d, 22d] };""",
        "LineChart" => """new LineChart { Width = 280, Height = 160, Values = [12d, 19d, 8d, 22d], Area = true };""",
        _ => $"new {title}();",
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

        var board = new Loam.Controls.Grid { Spacing = 18 };
        board.Children.Add(new Item
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
        board.Children.Add(new Item
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

    private static StackPanel BuildAlert()
    {
        var stack = new StackPanel { Spacing = 12, MaxWidth = 620, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Alert { Color = LoamColor.Info, Icon = Icons.Material.Filled.Settings, Content = "Text-tint alert keeps the page calm." });
        stack.Children.Add(new Alert { Color = LoamColor.Success, Variant = Variant.Filled, Icon = Icons.Material.Filled.Check, Content = "Filled success alert." });
        stack.Children.Add(new Alert { Color = LoamColor.Warning, Variant = Variant.Outlined, Icon = Icons.Material.Filled.Star, Content = "Outlined warning alert." });
        stack.Children.Add(new Alert { Color = LoamColor.Error, Content = "Error alert without an icon." });
        return stack;
    }

    private static StackPanel BuildLayoutSamples()
    {
        var grid = new Loam.Controls.Grid { Spacing = 12, MaxWidth = 720 };
        for (var i = 1; i <= 6; i++)
        {
            grid.Children.Add(new Item
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

        var stack = new Stack
        {
            Row = true,
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
                Labeled("Grid", grid),
                Labeled("Stack", stack),
                Labeled("Container", container),
            },
        };
    }

    private static Border BuildShellPreview()
    {
        var nav = new NavMenu { Width = 180 };
        nav.Children.Add(new NavLink { Icon = Icons.Material.Filled.Home, Content = "Dashboard", IsActive = true });
        nav.Children.Add(new NavLink { Icon = Icons.Material.Filled.Search, Content = "Search" });
        nav.Children.Add(new NavLink { Icon = Icons.Material.Filled.Settings, Content = "Settings" });

        var drawer = new Drawer
        {
            DrawerWidth = 196,
            Content = new Border { Padding = new Thickness(8), Child = nav },
        };

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Icon { Data = Icons.Material.Filled.Menu, Color = LoamColor.Inherit },
                new Text { Text = "Workspace", Typo = Typo.H6, Color = LoamColor.Inherit },
            },
        };

        var content = new MainContent
        {
            Content = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    new Text { Text = "Main content", Typo = Typo.H5 },
                    new Alert { Color = LoamColor.Info, Content = "Layout composes AppBar, Drawer, and MainContent." },
                    new ProgressLinear { Value = 46, Width = 280 },
                },
            },
        };

        var shell = new Layout
        {
            AppBar = new AppBar { Dense = true, Color = LoamColor.Primary, Content = toolbar },
            Drawer = drawer,
            Content = content,
        };

        var frame = new Border
        {
            Width = 720,
            Height = 420,
            MaxWidth = 720,
            Child = shell,
            ClipToBounds = true,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
        };
        frame.Bind(Border.BorderBrushProperty, frame.GetResourceObservable(LoamTokens.Divider));
        return frame;
    }

    private static StackPanel BuildText()
    {
        var stack = new StackPanel { Spacing = 2 };
        foreach (var typo in new[] { Typo.H4, Typo.H6, Typo.Subtitle1, Typo.Body1, Typo.Body2, Typo.Caption })
        {
            stack.Children.Add(new Text { Text = $"{typo} · The quick brown fox", Typo = typo });
        }

        stack.Children.Add(new Text { Text = "Primary colored body text", Typo = Typo.Body1, Color = LoamColor.Primary });
        stack.Children.Add(new Text { Text = "Error colored body text", Typo = Typo.Body1, Color = LoamColor.Error });
        stack.Children.Add(new Text { Text = "Centered text alignment", Typo = Typo.Body2, Align = TextAlignment.Center, Width = 320 });
        return stack;
    }

    private static StackPanel BuildButtonGroup()
    {
        var stack = new StackPanel { Spacing = 16, HorizontalAlignment = HorizontalAlignment.Left };

        var outlined = new ButtonGroup { Variant = Variant.Outlined, Color = LoamColor.Primary };
        outlined.Items.Add(new LoamButton { Content = "Left" });
        outlined.Items.Add(new LoamButton { Content = "Center" });
        outlined.Items.Add(new LoamButton { Content = "Right" });
        stack.Children.Add(outlined);

        var filled = new ButtonGroup { Variant = Variant.Filled, Color = LoamColor.Secondary };
        filled.Items.Add(new LoamButton { Content = "Day" });
        filled.Items.Add(new LoamButton { Content = "Week" });
        filled.Items.Add(new LoamButton { Content = "Month" });
        stack.Children.Add(filled);

        var favorites = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        favorites.Children.Add(new Text { Text = "Toggle icon button:", VerticalAlignment = VerticalAlignment.Center });
        favorites.Children.Add(new ToggleIconButton
        {
            Icon = Icons.Material.Filled.FavoriteBorder,
            ToggledIcon = Icons.Material.Filled.Favorite,
            Color = LoamColor.Default,
            ToggledColor = LoamColor.Error,
        });
        stack.Children.Add(favorites);

        return stack;
    }

    private static StackPanel BuildToggleIconButton()
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

    private static StackPanel BuildButtons()
    {
        var stack = new StackPanel { Spacing = 10 };

        foreach (var variant in new[] { Variant.Filled, Variant.Outlined, Variant.Text })
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

            stack.Children.Add(Labeled(variant.ToString(), row));
        }

        var sizes = new WrapPanel();
        foreach (var size in new[] { LoamSize.Small, LoamSize.Medium, LoamSize.Large })
        {
            sizes.Children.Add(new LoamButton
            {
                Content = size.ToString(),
                Variant = Variant.Filled,
                Color = LoamColor.Primary,
                Size = size,
                Margin = new Thickness(0, 0, 8, 8),
            });
        }

        stack.Children.Add(Labeled("Sizes", sizes));

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

        stack.Children.Add(Labeled("Disabled", disabled));

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
        stack.Children.Add(Labeled("With icons", withIcons));

        return stack;
    }

    private static WrapPanel BuildIcons()
    {
        var wrap = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
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
            wrap.Children.Add(new Icon { Data = data, Color = color, Size = LoamSize.Large, Margin = new Thickness(0, 0, 14, 0) });
        }

        return wrap;
    }

    private static WrapPanel BuildIconButtons()
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

    private static WrapPanel BuildFabs()
    {
        var wrap = new WrapPanel();
        wrap.Children.Add(new Fab { Label = "Add", StartIcon = Icons.Material.Filled.Add, Color = LoamColor.Primary, Margin = new Thickness(0, 0, 12, 0) });
        wrap.Children.Add(new Fab { StartIcon = Icons.Material.Filled.Edit, Color = LoamColor.Secondary, Margin = new Thickness(0, 0, 12, 0) });
        wrap.Children.Add(new Fab { Label = "Save", StartIcon = Icons.Material.Filled.Check, Color = LoamColor.Success });
        return wrap;
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

    private static WrapPanel BuildChips()
    {
        var m = new Thickness(0, 0, 8, 8);
        var wrap = new WrapPanel();
        wrap.Children.Add(new Chip { Text = "Filled", Color = LoamColor.Primary, Margin = m });
        wrap.Children.Add(new Chip { Text = "Outlined", Variant = Variant.Outlined, Color = LoamColor.Primary, Margin = m });
        wrap.Children.Add(new Chip { Text = "Text", Variant = Variant.Text, Color = LoamColor.Secondary, Margin = m });
        wrap.Children.Add(new Chip { Text = "With icon", Icon = Icons.Material.Filled.Star, Color = LoamColor.Warning, Margin = m });
        wrap.Children.Add(new Chip { Text = "Closeable", Color = LoamColor.Info, Closeable = true, Margin = m });
        wrap.Children.Add(new Chip { Text = "Error", Color = LoamColor.Error, Margin = m });
        return wrap;
    }

    private static StackPanel BuildChipSet()
    {
        var set = new ChipSet { Selectable = true, Mandatory = true, SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var label in new[] { "All", "Active", "Archived", "Draft" })
        {
            set.Items.Add(new Chip { Text = label, Color = LoamColor.Primary });
        }

        var multi = new ChipSet { Selectable = true, MultiSelect = true, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var label in new[] { "Open", "Assigned", "Overdue" })
        {
            multi.Items.Add(new Chip { Text = label, Color = LoamColor.Secondary });
        }

        multi.SelectedIndexes.Add(0);
        multi.SelectedIndexes.Add(2);

        return new StackPanel
        {
            Spacing = 12,
            Children =
            {
                Labeled("Single", set),
                Labeled("Multi", multi),
            },
        };
    }

    private static WrapPanel BuildBadges()
    {
        var m = new Thickness(0, 8, 28, 8);
        var wrap = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        wrap.Children.Add(new Badge { Value = 4, Color = LoamColor.Error, Margin = m, Content = new Icon { Data = Icons.Material.Filled.Favorite, Color = LoamColor.Default, Size = LoamSize.Large } });
        wrap.Children.Add(new Badge { Value = 150, Max = 99, Color = LoamColor.Primary, Margin = m, Content = new Icon { Data = Icons.Material.Filled.Home, Color = LoamColor.Default, Size = LoamSize.Large } });
        wrap.Children.Add(new Badge { Dot = true, Color = LoamColor.Success, Margin = m, Content = new Icon { Data = Icons.Material.Filled.Person, Color = LoamColor.Default, Size = LoamSize.Large } });
        wrap.Children.Add(new Badge { Value = 7, Overlap = true, Color = LoamColor.Secondary, Margin = m, Content = new Avatar { Content = "AB", Color = LoamColor.Primary } });
        return wrap;
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

    private static StackPanel BuildField()
    {
        static TextBox InnerTextBox(string text, string? watermark = null) => new()
        {
            Text = text,
            PlaceholderText = watermark,
            BorderBrush = Brushes.Transparent,
            BorderThickness = default,
            Background = Brushes.Transparent,
            Padding = default,
            MinHeight = 24,
            VerticalContentAlignment = VerticalAlignment.Center,
        };

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
            Children = { phone, color, options, invalid },
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

        return new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { openDialog, showSnackbar, actionSnackbar, messageBox } };
    }

    private static StackPanel BuildRadioSlider()
    {
        var group = new RadioGroup
        {
            Value = "b",
            Child = new Stack
            {
                Row = true,
                Children =
                {
                    new Radio { Value = "a", Content = "One", Color = LoamColor.Primary },
                    new Radio { Value = "b", Content = "Two", Color = LoamColor.Primary },
                    new Radio { Value = "c", Content = "Three", Color = LoamColor.Primary },
                },
            },
        };

        var slider = new Loam.Controls.Slider { Value = 40, Width = 280, HorizontalAlignment = HorizontalAlignment.Left };

        return new StackPanel { Spacing = 16, Children = { Labeled("Radio", group), Labeled("Slider", slider) } };
    }

    private static StackPanel BuildTextFields()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 360, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new TextField { Label = "Outlined", Placeholder = "Type here…", HelperText = "We never share this", Variant = Variant.Outlined });
        stack.Children.Add(new TextField { Label = "Filled", Variant = Variant.Filled, Text = "Prefilled value" });
        stack.Children.Add(new TextField { Label = "Underline", Variant = Variant.Text, Placeholder = "Search" });
        stack.Children.Add(new TextField { Label = "Budget", StartAdornment = new TextBlock { Text = "$" }, EndAdornment = new TextBlock { Text = "USD" }, FloatingLabel = true });
        stack.Children.Add(new TextField { Label = "Email", Variant = Variant.Outlined, Text = "not-an-email", Error = true, ErrorText = "Enter a valid email" });
        stack.Children.Add(new NumericField { Label = "Quantity", Minimum = 0, Maximum = 99, Value = 3, HelperText = "0–99" });
        stack.Children.Add(new NumericField { Label = "Price", Variant = Variant.Filled, Minimum = 0, Step = 0.5, Value = 9.5, Format = "0.00" });
        stack.Children.Add(new MaskedTextField { Label = "Phone", Pattern = "(###) ###-####", Placeholder = "(555) 123-4567" });

        var fruit = new Autocomplete { Label = "Fruit", Placeholder = "Start typing…" };
        foreach (var name in new[] { "Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grape", "Mango", "Orange", "Peach", "Pineapple" })
        {
            fruit.Items.Add(name);
        }

        fruit.SearchFunc = text => fruit.Items.Where(item => item.Contains(text ?? "", StringComparison.OrdinalIgnoreCase));
        fruit.ItemTemplate = name => new Text { Text = name, Typo = Typo.Body2 };

        stack.Children.Add(fruit);
        return stack;
    }

    private static ToggleGroup BuildToggleGroup()
    {
        var group = new ToggleGroup { HorizontalAlignment = HorizontalAlignment.Left, SelectedValue = "week" };
        group.Items.Add(new ToggleItem("Day", "day"));
        group.Items.Add(new ToggleItem("Week", "week"));
        group.Items.Add(new ToggleItem("Month", "month"));
        return group;
    }

    private static StackPanel BuildRating()
    {
        var stack = new StackPanel { Spacing = 12, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Rating { SelectedValue = 3 });
        stack.Children.Add(new Rating { SelectedValue = 4, MaxValue = 6, Color = LoamColor.Primary });
        stack.Children.Add(new Rating { SelectedValue = 5, ReadOnly = true, Size = LoamSize.Small });
        return stack;
    }

    private static StackPanel BuildPopover()
    {
        var toggle = new LoamButton { Content = "Toggle popover", Variant = Variant.Filled, Color = LoamColor.Primary };
        var popover = new Popover
        {
            Target = toggle,
            Placement = Avalonia.Controls.PlacementMode.Bottom,
            Content = new StackPanel
            {
                Width = 220,
                Spacing = 6,
                Children =
                {
                    new Text { Text = "Popover", Typo = Typo.Subtitle1 },
                    new Text { Text = "A floating panel anchored to its target, dismissed by clicking away.", Typo = Typo.Body2, Color = LoamColor.Secondary },
                },
            },
        };
        toggle.Click += (_, _) => popover.Open = !popover.Open;

        return new StackPanel { Spacing = 8, HorizontalAlignment = HorizontalAlignment.Left, Children = { toggle, popover } };
    }

    private static StackPanel BuildOverlayScrim()
    {
        var overlay = new Overlay { DarkBackground = true, AutoClose = true, Content = new ProgressCircular() };

        var region = new Panel { Width = 320, Height = 160, HorizontalAlignment = HorizontalAlignment.Left };
        region.Children.Add(new Paper
        {
            Elevation = 1,
            Content = new Text { Text = "Content area — click the scrim to dismiss." },
            Padding = new Thickness(16),
        });
        region.Children.Add(overlay);

        var show = new Loam.Controls.Button { Content = "Show overlay", Variant = Variant.Filled, Color = LoamColor.Primary };
        show.Click += (_, _) => overlay.Visible = true;

        return new StackPanel { Spacing = 12, HorizontalAlignment = HorizontalAlignment.Left, Children = { show, region } };
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

        var skeletons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        skeletons.Children.Add(new Skeleton { Width = 140 });
        skeletons.Children.Add(new Skeleton { Width = 80, Animate = false });
        skeletons.Children.Add(new Skeleton { Circle = true, Width = 32, Height = 32 });
        stack.Children.Add(Labeled("Skeletons", skeletons));

        return stack;
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
        stack.Children.Add(links);

        var nav = new NavMenu { Width = 220 };
        var dashboard = new NavLink { Icon = Icons.Material.Filled.Home, Content = "Dashboard", IsActive = true };
        var people = new NavLink { Icon = Icons.Material.Filled.Person, Content = "People" };
        var settings = new NavLink { Icon = Icons.Material.Filled.Settings, Content = "Settings" };
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

        var group = new NavGroup { Title = "Admin", Icon = Icons.Material.Filled.Settings, Expanded = true };
        group.Items.Add(new NavLink { Icon = Icons.Material.Filled.Person, Content = "Users" });
        group.Items.Add(new NavLink { Icon = Icons.Material.Filled.Favorite, Content = "Roles" });
        nav.Children.Add(group);

        stack.Children.Add(new Paper { Elevation = 1, Width = 220, Content = nav });
        return stack;
    }

    private static StackPanel BuildPagination()
    {
        var stack = new StackPanel { Spacing = 12, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Pagination { Count = 10, Selected = 1 });
        stack.Children.Add(new Pagination { Count = 12, Selected = 6, Color = LoamColor.Secondary });
        return stack;
    }

    private static Stepper BuildStepper()
    {
        var stepper = new Stepper { MaxWidth = 480, HorizontalAlignment = HorizontalAlignment.Left };
        stepper.Steps.Add(new Step("Account", new Text { Text = "Create your account credentials." }));
        stepper.Steps.Add(new Step("Profile", new Text { Text = "Tell us a little about yourself." }));
        stepper.Steps.Add(new Step("Review", new Text { Text = "Confirm everything looks right." }));
        return stepper;
    }

    private static StackPanel BuildCollapse()
    {
        var collapse = new Collapse
        {
            Duration = TimeSpan.FromMilliseconds(220),
            Child = new Paper
            {
                Elevation = 1,
                Padding = new Thickness(16),
                Content = new Text { Text = "This content slides in and out of view when toggled." },
            },
        };

        var toggle = new Loam.Controls.Button { Content = "Toggle details", Variant = Variant.Outlined, Color = LoamColor.Primary };
        toggle.Click += (_, _) => collapse.Expanded = !collapse.Expanded;

        var staticCollapse = new Collapse
        {
            Animated = false,
            Expanded = true,
            Child = new Text { Text = "Static reveal mode is available for reduced motion needs.", Color = LoamColor.Secondary },
        };

        return new StackPanel
        {
            Spacing = 12,
            MaxWidth = 420,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { toggle, collapse, staticCollapse },
        };
    }

    private static Timeline BuildTimeline()
    {
        var timeline = new Timeline { MaxWidth = 420, HorizontalAlignment = HorizontalAlignment.Left };
        timeline.Items.Add(new TimelineItem("Order placed — 9:24 AM", LoamColor.Primary));
        timeline.Items.Add(new TimelineItem("Payment confirmed — 9:25 AM", LoamColor.Success));
        timeline.Items.Add(new TimelineItem("Shipped — 2:10 PM", LoamColor.Info));
        timeline.Items.Add(new TimelineItem("Out for delivery", LoamColor.Warning));
        return timeline;
    }

    private static ExpansionPanels BuildExpansionPanels()
    {
        var panels = new ExpansionPanels { MaxWidth = 480, HorizontalAlignment = HorizontalAlignment.Left };
        panels.Panels.Add(new ExpansionPanel
        {
            Header = "Shipping address",
            Content = new Text { Text = "Where should we deliver your order?", Margin = new Thickness(0, 4) },
            IsExpanded = true,
        });
        panels.Panels.Add(new ExpansionPanel
        {
            Header = "Billing details",
            Content = new Text { Text = "Card and invoice information.", Margin = new Thickness(0, 4) },
        });
        panels.Panels.Add(new ExpansionPanel
        {
            Header = "Delivery options",
            Content = new Text { Text = "Standard, express, or pickup.", Margin = new Thickness(0, 4) },
        });
        return panels;
    }

    private static Loam.Controls.Carousel BuildCarousel()
    {
        static Border Slide(string label, LoamColor color) => new()
        {
            Child = new Text
            {
                Text = label,
                Typo = Typo.H5,
                Color = color,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        var carousel = new Loam.Controls.Carousel { Width = 380, Height = 200, HorizontalAlignment = HorizontalAlignment.Left };
        carousel.Items.Add(new CarouselItem(Slide("First slide", LoamColor.Primary)));
        carousel.Items.Add(new CarouselItem(Slide("Second slide", LoamColor.Secondary)));
        carousel.Items.Add(new CarouselItem(Slide("Third slide", LoamColor.Info)));
        return carousel;
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

        var tree = new Loam.Controls.TreeView { MaxWidth = 320, HorizontalAlignment = HorizontalAlignment.Left };
        tree.Items.Add(Node("src", Icons.Material.Filled.Home,
            Node("Components", null,
                Node("Button.cs", null),
                Node("TreeView.cs", null)),
            Node("Theming", null,
                Node("LoamTheme.cs", null))));
        tree.Items[0].Expanded = true;
        return tree;
    }

    private sealed class Dessert(string name, int calories, double fat)
    {
        public string Name { get; set; } = name;

        public int Calories { get; } = calories;

        public double Fat { get; } = fat;
    }

    private static Loam.Controls.DataGrid<Dessert> BuildDataGrid()
    {
        var grid = new Loam.Controls.DataGrid<Dessert>
        {
            Striped = true,
            Hover = true,
            PageSize = 4,
            FilterText = "i",
            Filter = (dessert, text) => dessert.Name.Contains(text, StringComparison.OrdinalIgnoreCase),
            MaxWidth = 480,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        grid.Columns.Add(new DataGridColumn<Dessert>("Dessert", d => d.Name)
        {
            Editable = true,
            SetText = (dessert, text) => dessert.Name = text ?? "",
        });
        grid.Columns.Add(new DataGridColumn<Dessert>("Calories", d => d.Calories) { Align = HorizontalAlignment.Right });
        grid.Columns.Add(new DataGridColumn<Dessert>("Fat (g)", d => d.Fat) { Format = "0.0", Align = HorizontalAlignment.Right });
        grid.Items = new List<Dessert>
        {
            new("Frozen yogurt", 159, 6.0),
            new("Ice cream sandwich", 237, 9.0),
            new("Eclair", 262, 16.0),
            new("Cupcake", 305, 3.7),
            new("Gingerbread", 356, 16.0),
            new("Jelly bean", 375, 0.0),
            new("Lollipop", 392, 0.2),
            new("Honeycomb", 408, 3.2),
        };
        return grid;
    }

    private static SimpleTable BuildTable()
    {
        var table = new SimpleTable { Striped = true, Hover = true, MaxWidth = 480, HorizontalAlignment = HorizontalAlignment.Left };
        table.Headers.Add("Dessert");
        table.Headers.Add("Calories");
        table.Headers.Add("Fat (g)");
        table.Rows.Add(new TableRow("Frozen yogurt", 159, 6.0));
        table.Rows.Add(new TableRow("Ice cream sandwich", 237, 9.0));
        table.Rows.Add(new TableRow("Eclair", 262, 16.0));
        table.Rows.Add(new TableRow("Cupcake", 305, 3.7));
        return table;
    }

    private static StackPanel BuildFileUpload()
    {
        var upload = new FileUpload { ButtonText = "Attach files", AllowMultiple = true };
        var status = new Text { Typo = Typo.Caption, Color = LoamColor.Secondary };
        upload.FilesSelected += files => status.Text = $"{files.Count} file(s) selected";
        return new StackPanel { Spacing = 8, HorizontalAlignment = HorizontalAlignment.Left, Children = { upload, status } };
    }

    private static StackPanel BuildFormDemo()
    {
        var name = new TextField { Label = "Name", Required = true };
        var email = new TextField
        {
            Label = "Email",
            Required = true,
            Validation = value => value?.Contains('@', StringComparison.Ordinal) == true ? null : "Enter a valid email",
        };
        var form = new Form
        {
            Child = new StackPanel
            {
                Spacing = 14,
                Children = { name, email },
            },
        };
        var status = new Text { Typo = Typo.Caption, Color = LoamColor.Secondary };
        var validate = new LoamButton { Content = "Validate", Variant = Variant.Filled, Color = LoamColor.Primary };
        validate.Click += (_, _) =>
        {
            status.Text = form.Validate() ? "Ready to submit" : "Review the highlighted fields";
            status.Color = form.IsValid ? LoamColor.Success : LoamColor.Error;
        };

        return new StackPanel
        {
            Spacing = 14,
            MaxWidth = 360,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { form, validate, status },
        };
    }

    private static StackPanel BuildColorPicker()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 280, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Loam.Controls.ColorPicker { Label = "Theme color" });
        stack.Children.Add(new Loam.Controls.ColorPicker { Label = "Accent", Value = Avalonia.Media.Color.Parse("#FF9800"), ShowAlpha = true });
        return stack;
    }

    private static StackPanel BuildDateRangePicker()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 320, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Loam.Controls.DateRangePicker { Label = "Trip dates" });
        stack.Children.Add(new Loam.Controls.DateRangePicker
        {
            Label = "Reporting period",
            Start = new DateTime(2026, 6, 1),
            End = new DateTime(2026, 6, 30),
            MinDate = new DateTime(2026, 6, 1),
            MaxDate = new DateTime(2026, 7, 31),
            DateFormat = "MMM d",
        });
        return stack;
    }

    private static StackPanel BuildDateTimePickers()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 280, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Loam.Controls.DatePicker { Label = "Start date", MinDate = DateTime.Today, MaxDate = DateTime.Today.AddMonths(6) });
        stack.Children.Add(new Loam.Controls.DatePicker { Label = "Due date", Date = new DateTime(2026, 6, 30), DateFormat = "ddd, MMM d yyyy" });
        stack.Children.Add(new Loam.Controls.TimePicker { Label = "Reminder", TimeFormat = "t" });
        stack.Children.Add(new Loam.Controls.TimePicker { Label = "Standup", Time = new TimeSpan(9, 30, 0), TimeFormat = "HH:mm", MinuteStep = 15 });
        return stack;
    }

    private static StackPanel BuildMonthCalendar()
    {
        var selected = new Text { Typo = Typo.Caption, Color = LoamColor.Secondary };
        var calendar = new MonthCalendar
        {
            DisplayMonth = new DateTime(2026, 6, 1),
            SelectedDate = new DateTime(2026, 6, 4),
            RangeStart = new DateTime(2026, 6, 10),
            RangeEnd = new DateTime(2026, 6, 16),
        };
        selected.Text = "Selected: Jun 4, 2026";
        calendar.DateSelected += date =>
        {
            calendar.SelectedDate = date;
            selected.Text = $"Selected: {date:MMM d, yyyy}";
        };

        return new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Paper { Elevation = 1, Padding = new Thickness(14), Content = calendar },
                selected,
            },
        };
    }

    private static StackPanel BuildSelect()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 360, HorizontalAlignment = HorizontalAlignment.Left };

        var country = new Select { Label = "Country", Placeholder = "Choose a country" };
        country.Items.Add(new SelectItem("United States", "us"));
        country.Items.Add(new SelectItem("Germany", "de"));
        country.Items.Add(new SelectItem("Japan", "jp"));
        country.Items.Add(new SelectItem("Brazil", "br"));
        stack.Children.Add(country);

        var size = new Select { Label = "Size", Value = "m" };
        size.Items.Add(new SelectItem("Small", "s"));
        size.Items.Add(new SelectItem("Medium", "m"));
        size.Items.Add(new SelectItem("Large", "l"));
        stack.Children.Add(size);

        var tags = new Select { Label = "Tags", MultiSelect = true };
        tags.Items.Add(new SelectItem("Design", "design"));
        tags.Items.Add(new SelectItem("Build", "build"));
        tags.Items.Add(new SelectItem("Review", "review"));
        tags.SelectedValues.Add("design");
        tags.SelectedValues.Add("review");
        stack.Children.Add(tags);

        return stack;
    }

    private static StackPanel BuildHidden()
    {
        return new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new Text { Text = "Resize the window — the chip below hides at Sm and narrower.", Typo = Typo.Body2, Color = LoamColor.Secondary },
                new Hidden
                {
                    Breakpoint = Breakpoint.Sm,
                    Mode = HiddenMode.Down,
                    Child = new Chip { Text = "Visible on Md and wider", Color = LoamColor.Primary },
                },
            },
        };
    }

    private static StackPanel BuildList()
    {
        var list = new List();
        list.Children.Add(new ListSubheader { Text = "MAILBOXES" });
        list.Children.Add(new ListItem { Icon = Icons.Material.Filled.Home, Content = "Inbox" });
        list.Children.Add(new ListItem { Icon = Icons.Material.Filled.Star, Content = "Starred" });
        list.Children.Add(new ListSubheader { Text = "LABELS" });
        list.Children.Add(new ListItem { Icon = Icons.Material.Filled.Person, Content = "Personal" });

        // Spacer pushes the trailing button to the right edge of a DockPanel row.
        var title = new Text { Text = "Toolbar", VerticalAlignment = VerticalAlignment.Center };
        var action = new IconButton { Icon = Icons.Material.Filled.Settings };
        DockPanel.SetDock(action, Dock.Right);
        var bar = new Paper
        {
            Elevation = 1,
            Padding = new Thickness(8, 4),
            Content = new DockPanel { LastChildFill = true, Children = { title, action, new Spacer() } },
        };

        return new StackPanel
        {
            Spacing = 16,
            MaxWidth = 280,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { new Paper { Elevation = 1, Content = list }, bar },
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

    private static WrapPanel BuildCharts()
    {
        var revenue = new[] { 12d, 19d, 8d, 22d, 17d, 25d };
        var split = new[] { 40d, 25d, 20d, 15d };

        Control Labeledchart(string label, Control chart) => new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(0, 0, 24, 16),
            Children = { new Text { Text = label, Typo = Typo.Subtitle2 }, chart },
        };

        return new WrapPanel
        {
            Children =
            {
                Labeledchart("Pie", new PieChart { Width = 180, Height = 180, Values = split }),
                Labeledchart("Donut", new PieChart { Width = 180, Height = 180, Values = split, Donut = true }),
                Labeledchart("Bar", new BarChart { Width = 280, Height = 160, Values = revenue }),
                Labeledchart("Line", new LineChart { Width = 280, Height = 160, Values = revenue, Area = true }),
            },
        };
    }

    private static Card BuildCard()
    {
        var actions = new CardActions
        {
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new LoamButton { Content = "Share", Variant = Variant.Text, Color = LoamColor.Primary },
                    new LoamButton { Content = "Learn more", Variant = Variant.Text, Color = LoamColor.Primary },
                },
            },
        };

        return new Card
        {
            Width = 340,
            HorizontalAlignment = HorizontalAlignment.Left,
            Content = new StackPanel
            {
                Children =
                {
                    new CardHeader
                    {
                        Avatar = new Avatar { Content = "PL", Color = LoamColor.Primary },
                        Title = "Project Loam",
                        Subtitle = "Updated today",
                        Action = new IconButton { Icon = Icons.Material.Filled.Settings },
                    },
                    new CardMedia { MediaHeight = 160 },
                    new CardContent
                    {
                        Child = new Text { Text = "reference components, mapped to Avalonia in pure C#. This card composes header, media, content, and actions." },
                    },
                    actions,
                },
            },
        };
    }

    private static WrapPanel BuildPaper()
    {
        var wrap = new WrapPanel();

        Paper Make(string label)
        {
            return new Paper
            {
                Width = 130,
                Height = 88,
                Margin = new Thickness(0, 0, 16, 16),
                Content = new Text { Text = label, Margin = new Thickness(12) },
            };
        }

        wrap.Children.Add(Make("Elevation 1"));

        var e4 = Make("Elevation 4");
        e4.Elevation = 4;
        wrap.Children.Add(e4);

        var e8 = Make("Elevation 8");
        e8.Elevation = 8;
        wrap.Children.Add(e8);

        var outlined = Make("Outlined");
        outlined.Outlined = true;
        wrap.Children.Add(outlined);

        var square = Make("Square");
        square.Square = true;
        wrap.Children.Add(square);

        return wrap;
    }

    private static StackPanel BuildDivider()
    {
        var horizontal = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new Text { Text = "Above the divider", Typo = Typo.Body2 },
                new Divider(),
                new Text { Text = "Below the divider", Typo = Typo.Body2 },
            },
        };

        var vertical = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Height = 28,
            Spacing = 12,
            Margin = new Thickness(0, 16, 0, 0),
            Children =
            {
                new Text { Text = "Left", Typo = Typo.Body2 },
                new Divider { Vertical = true },
                new Text { Text = "Right", Typo = Typo.Body2 },
            },
        };

        return new StackPanel { Children = { horizontal, vertical } };
    }
}
