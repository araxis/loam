using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Loam;
using Loam.Controls;
using Loam.Theming;
using LoamButton = Loam.Controls.Button;

namespace Loam.Gallery;

/// <summary>
/// Phase 3 showcase: the core primitives — <see cref="Text"/>, <see cref="LoamButton"/> (full
/// variant × color × size × state matrix), <see cref="Paper"/> and <see cref="Divider"/>.
/// </summary>
public sealed class ComponentsView : UserControl
{
    private static readonly LoamColor[] Colors =
    [
        LoamColor.Default, LoamColor.Primary, LoamColor.Secondary, LoamColor.Tertiary,
        LoamColor.Info, LoamColor.Success, LoamColor.Warning, LoamColor.Error, LoamColor.Dark,
    ];

    public ComponentsView()
    {
        var stack = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 28,
            Children =
            {
                Section("Text", BuildText()),
                Section("Buttons", BuildButtons()),
                Section("Button group", BuildButtonGroup()),
                Section(
                    "Icons & icon buttons",
                    new StackPanel
                    {
                        Spacing = 10,
                        Children = { Labeled("Icons", BuildIcons()), Labeled("Icon buttons", BuildIconButtons()) },
                    }),
                Section("Floating action button", BuildFabs()),
                Section("Avatars", BuildAvatars()),
                Section("Chips", BuildChips()),
                Section("Chip set (selectable)", BuildChipSet()),
                Section("Badges", BuildBadges()),
                Section("Selection (checkbox / switch)", BuildInputs()),
                Section("Text fields", BuildTextFields()),
                Section("Select", BuildSelect()),
                Section("File upload", BuildFileUpload()),
                Section("Date & time pickers", BuildDateTimePickers()),
                Section("Date range picker", BuildDateRangePicker()),
                Section("Color picker", BuildColorPicker()),
                Section("Radio / Slider", BuildRadioSlider()),
                Section("Progress", BuildProgress()),
                Section("Rating", BuildRating()),
                Section("Toggle group", BuildToggleGroup()),
                Section("Overlays (dialog / snackbar)", BuildOverlays()),
                Section("Overlay (scrim)", BuildOverlayScrim()),
                Section("Popover", BuildPopover()),
                Section("Tabs / Menu / Tooltip", BuildTabsMenu()),
                Section("Table", BuildTable()),
                Section("Data grid", BuildDataGrid()),
                Section("Tree view", BuildTreeView()),
                Section("Carousel", BuildCarousel()),
                Section("Expansion panels", BuildExpansionPanels()),
                Section("Collapse", BuildCollapse()),
                Section("Timeline", BuildTimeline()),
                Section("Stepper", BuildStepper()),
                Section("Pagination", BuildPagination()),
                Section("Navigation (breadcrumbs / link)", BuildNavigation()),
                Section("List & spacer", BuildList()),
                Section("Card", BuildCard()),
                Section("Ripple", BuildRipple()),
                Section("Charts", BuildCharts()),
                Section("Paper", BuildPaper()),
                Section("Divider", BuildDivider()),
                Section("Responsive (Hidden)", BuildHidden()),
            },
        };

        var scroller = new ScrollViewer { Content = stack };
        var scrollToTop = new ScrollToTop
        {
            Target = scroller,
            VisibleOffset = 200,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 24, 24),
        };

        var background = new Border { Child = new Panel { Children = { scroller, scrollToTop } } };
        background.Bind(Border.BackgroundProperty, background.GetResourceObservable(LoamTokens.Background));
        Content = background;
    }

    private static StackPanel Section(string title, Control body) =>
        new()
        {
            Children =
            {
                new Text { Text = title, Typo = Typo.H6, Margin = new Thickness(0, 0, 0, 12) },
                body,
            },
        };

    private static StackPanel BuildText()
    {
        var stack = new StackPanel { Spacing = 2 };
        foreach (var typo in new[] { Typo.H4, Typo.H6, Typo.Subtitle1, Typo.Body1, Typo.Body2, Typo.Caption })
        {
            stack.Children.Add(new Text { Text = $"{typo} · The quick brown fox", Typo = typo });
        }

        stack.Children.Add(new Text { Text = "Primary colored body text", Typo = Typo.Body1, Color = LoamColor.Primary });
        stack.Children.Add(new Text { Text = "Error colored body text", Typo = Typo.Body1, Color = LoamColor.Error });
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
            Color = LoamColor.Error,
        });
        stack.Children.Add(favorites);

        return stack;
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

    private static ChipSet BuildChipSet()
    {
        var set = new ChipSet { Selectable = true, Mandatory = true, SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var label in new[] { "All", "Active", "Archived", "Draft" })
        {
            set.Items.Add(new Chip { Text = label, Color = LoamColor.Primary });
        }

        return set;
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

        return new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { openDialog, showSnackbar, messageBox } };
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
        stack.Children.Add(new TextField { Label = "Email", Variant = Variant.Outlined, Text = "not-an-email", Error = true, ErrorText = "Enter a valid email" });
        stack.Children.Add(new NumericField { Label = "Quantity", Minimum = 0, Maximum = 99, Value = 3, HelperText = "0–99" });
        stack.Children.Add(new NumericField { Label = "Price", Variant = Variant.Filled, Minimum = 0, Step = 0.5, Value = 9.5, Format = "0.00" });
        stack.Children.Add(new MaskedTextField { Label = "Phone", Pattern = "(###) ###-####", Placeholder = "(555) 123-4567" });

        var fruit = new Autocomplete { Label = "Fruit", Placeholder = "Start typing…" };
        foreach (var name in new[] { "Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grape", "Mango", "Orange", "Peach", "Pineapple" })
        {
            fruit.Items.Add(name);
        }

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
        links.Children.Add(new Link { Text = "Avalonia docs", Href = "https://docs.avaloniaui.net" });
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
            Child = new Paper
            {
                Elevation = 1,
                Padding = new Thickness(16),
                Content = new Text { Text = "This content slides in and out of view when toggled." },
            },
        };

        var toggle = new Loam.Controls.Button { Content = "Toggle details", Variant = Variant.Outlined, Color = LoamColor.Primary };
        toggle.Click += (_, _) => collapse.Expanded = !collapse.Expanded;

        return new StackPanel { Spacing = 12, MaxWidth = 420, HorizontalAlignment = HorizontalAlignment.Left, Children = { toggle, collapse } };
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

    private sealed record Dessert(string Name, int Calories, double Fat);

    private static Loam.Controls.DataGrid<Dessert> BuildDataGrid()
    {
        var grid = new Loam.Controls.DataGrid<Dessert> { Striped = true, Hover = true, PageSize = 4, MaxWidth = 480, HorizontalAlignment = HorizontalAlignment.Left };
        grid.Columns.Add(new DataGridColumn<Dessert>("Dessert", d => d.Name));
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

    private static StackPanel BuildColorPicker()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 280, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Loam.Controls.ColorPicker { Label = "Theme color" });
        stack.Children.Add(new Loam.Controls.ColorPicker { Label = "Accent", Value = Avalonia.Media.Color.Parse("#FF9800") });
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
            DateFormat = "MMM d",
        });
        return stack;
    }

    private static StackPanel BuildDateTimePickers()
    {
        var stack = new StackPanel { Spacing = 18, MaxWidth = 280, HorizontalAlignment = HorizontalAlignment.Left };
        stack.Children.Add(new Loam.Controls.DatePicker { Label = "Start date" });
        stack.Children.Add(new Loam.Controls.DatePicker { Label = "Due date", Date = new DateTime(2026, 6, 30), DateFormat = "ddd, MMM d yyyy" });
        stack.Children.Add(new Loam.Controls.TimePicker { Label = "Reminder", TimeFormat = "t" });
        stack.Children.Add(new Loam.Controls.TimePicker { Label = "Standup", Time = new TimeSpan(9, 30, 0), TimeFormat = "HH:mm", MinuteStep = 15 });
        return stack;
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
                        Child = new Text { Text = "MudBlazor components, mapped to Avalonia in pure C#. This card composes header, media, content, and actions." },
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
