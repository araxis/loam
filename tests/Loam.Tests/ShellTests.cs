using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam;
using Loam.Controls;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

public class ShellTests
{
    private static void Show(Control content)
    {
        new Window { Width = 800, Height = 600, Content = content }.Show();
        Dispatcher.UIThread.RunJobs();
    }

    private static KeyEventArgs KeyArgs(Key key) => new()
    {
        RoutedEvent = InputElement.KeyDownEvent,
        Key = key,
    };

    [Fact]
    public void Drawer_resolve_width_for_states()
    {
        Drawer.ResolveWidth(open: true, mini: false, width: 240, miniWidth: 56).ShouldBe(240d);
        Drawer.ResolveWidth(open: true, mini: true, width: 240, miniWidth: 56).ShouldBe(56d);
        Drawer.ResolveWidth(open: false, mini: false, width: 240, miniWidth: 56).ShouldBe(0d);
    }

    [AvaloniaFact]
    public void Drawer_mode_defaults_to_docked_with_scrim_close_enabled()
    {
        var drawer = new Drawer();

        drawer.Mode.ShouldBe(DrawerMode.Docked);
        drawer.ShowScrim.ShouldBeTrue();
        drawer.CloseOnScrimClick.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Drawer_is_open_at_full_width_by_default()
    {
        new Drawer().Width.ShouldBe(240d);
    }

    [AvaloniaFact]
    public void AppBar_default_color_uses_surface_role()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var bar = new AppBar { Content = new TextBlock { Text = "Title" } };
        Show(bar);
        bar.ApplyTemplate();

        bar.Height.ShouldBe(64d);
        bar.ZIndex.ShouldBe(LoamZIndex.Default.AppBar);
        var border = bar.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_Root");
        ((ISolidColorBrush)border.Background!).Color.ShouldBe(Color.Parse("#FFFBFE"));
    }

    [AvaloniaFact]
    public void AppBar_dense_is_shorter()
    {
        var bar = new AppBar { Dense = true };
        bar.Height.ShouldBe(48d);
    }

    [AvaloniaFact]
    public void AppBar_toolbar_content_stretches_and_inherits_foreground()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;

        var title = new Text
        {
            Text = "Primary app bar",
            Typo = Typo.Subtitle1,
            Color = LoamColor.Inherit,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var action = new IconButton
        {
            Icon = Icons.Material.Filled.Settings,
            Color = LoamColor.Inherit,
            VerticalAlignment = VerticalAlignment.Center,
        };
        action.Bind(TemplatedControl.ForegroundProperty,
            action.GetResourceObservable(LoamTokens.ColorScheme(nameof(LoamColorScheme.OnPrimary))));

        var toolbar = new Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Icon { Data = Icons.Material.Filled.Menu, Color = LoamColor.Inherit, VerticalAlignment = VerticalAlignment.Center },
                title,
                action,
            },
        };
        Avalonia.Controls.Grid.SetColumn(title, 1);
        Avalonia.Controls.Grid.SetColumn(action, 2);

        var bar = new AppBar
        {
            Width = 640,
            Color = LoamColor.Primary,
            Content = toolbar,
        };
        Show(bar);
        bar.ApplyTemplate();
        action.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        toolbar.Bounds.Width.ShouldBeGreaterThan(600);
        action.Bounds.X.ShouldBeGreaterThan(560);

        var expected = ((ISolidColorBrush)bar.Foreground!).Color;
        ((ISolidColorBrush)title.Foreground!).Color.ShouldBe(expected);
        ((ISolidColorBrush)action.Foreground!).Color.ShouldBe(expected);
    }

    [AvaloniaFact]
    public void AppBar_builds_standard_toolbar_from_properties()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var navigationClicked = false;
        var settingsClicked = false;
        var bar = new AppBar
        {
            Width = 640,
            Color = LoamColor.Primary,
            Title = "Primary app bar",
            NavigationIcon = Icons.Material.Filled.Menu,
            NavigationAction = () => navigationClicked = true,
            Actions =
            {
                new AppBarAction
                {
                    Icon = Icons.Material.Filled.Settings,
                    Label = "Settings",
                    OnClick = () => settingsClicked = true,
                },
            },
        };
        Show(bar);
        bar.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var title = bar.GetVisualDescendants().OfType<Text>().Single(text => text.Text == "Primary app bar");
        var buttons = bar.GetVisualDescendants().OfType<IconButton>().ToArray();
        buttons.Length.ShouldBe(2);
        AutomationProperties.GetName(buttons[0]).ShouldBe("Navigation");
        AutomationProperties.GetName(buttons[1]).ShouldBe("Settings");

        var expected = ((ISolidColorBrush)bar.Foreground!).Color;
        ((ISolidColorBrush)title.Foreground!).Color.ShouldBe(expected);
        foreach (var button in buttons)
        {
            ((ISolidColorBrush)button.Foreground!).Color.ShouldBe(expected);
        }

        buttons[0].RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        buttons[1].RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        navigationClicked.ShouldBeTrue();
        settingsClicked.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void AppBar_supports_subtitle_and_action_settings()
    {
        var searchClicked = false;
        var deleteClicked = false;
        var bar = new AppBar
        {
            Width = 640,
            Color = LoamColor.Primary,
            Title = "Workspace",
            Subtitle = "Configured shell",
            NavigationIcon = Icons.Material.Filled.Menu,
            Actions =
            {
                new AppBarAction
                {
                    Icon = Icons.Material.Filled.Search,
                    Label = "Search",
                    Size = LoamSize.Small,
                    Color = LoamColor.Inherit,
                    OnClick = () => searchClicked = true,
                },
                new AppBarAction
                {
                    Icon = Icons.Material.Filled.Delete,
                    Label = "Delete disabled",
                    IsEnabled = false,
                    OnClick = () => deleteClicked = true,
                },
            },
        };
        Show(bar);
        bar.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        bar.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Workspace").ShouldBeTrue();
        bar.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Configured shell").ShouldBeTrue();
        AutomationProperties.GetName(bar).ShouldBe("Workspace");

        var search = bar.GetVisualDescendants().OfType<IconButton>()
            .Single(button => AutomationProperties.GetName(button) == "Search");
        search.Size.ShouldBe(LoamSize.Small);
        search.Color.ShouldBe(LoamColor.Inherit);
        search.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        searchClicked.ShouldBeTrue();

        var disabled = bar.GetVisualDescendants().OfType<IconButton>()
            .Single(button => AutomationProperties.GetName(button) == "Delete disabled");
        disabled.IsEnabled.ShouldBeFalse();
        disabled.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        deleteClicked.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void AppBar_renders_custom_actions_alongside_icon_actions()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var toggle = new Switch { Name = "PART_CustomToggle" };
        var bar = new AppBar
        {
            Width = 640,
            Title = "Inbox",
            CustomActions = { toggle },
            Actions =
            {
                new AppBarAction { Icon = Icons.Material.Filled.Search, Label = "Search" },
            },
        };
        Show(bar);
        bar.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        bar.GetVisualDescendants().OfType<Switch>()
            .Any(s => s.Name == "PART_CustomToggle").ShouldBeTrue();
        bar.GetVisualDescendants().OfType<IconButton>()
            .Any(button => AutomationProperties.GetName(button) == "Search").ShouldBeTrue();
    }

    [AvaloniaFact]
    public void AppBar_custom_actions_survive_rebuilds_without_reparenting_errors()
    {
        Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var search = new TextBox { Name = "PART_Search", Width = 160 };
        var bar = new AppBar { Width = 640, Title = "Files" };
        Show(bar);
        bar.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        bar.CustomActions.Add(search);
        Dispatcher.UIThread.RunJobs();
        bar.GetVisualDescendants().OfType<TextBox>().Any(t => t.Name == "PART_Search").ShouldBeTrue();

        // A further rebuild must re-host the same live control without throwing.
        bar.Actions.Add(new AppBarAction { Icon = Icons.Material.Filled.Add, Label = "Add" });
        Dispatcher.UIThread.RunJobs();
        bar.GetVisualDescendants().OfType<TextBox>().Any(t => t.Name == "PART_Search").ShouldBeTrue();
        bar.GetVisualDescendants().OfType<IconButton>()
            .Any(button => AutomationProperties.GetName(button) == "Add").ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Layout_hosts_appbar_drawer_and_main_content()
    {
        var layout = new Layout
        {
            AppBar = new AppBar { Content = new TextBlock { Text = "Bar" } },
            Drawer = new Drawer { Content = new TextBlock { Text = "Nav" } },
            Content = new MainContent { Content = new TextBlock { Text = "Body" } },
        };
        Show(layout);
        layout.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        layout.GetVisualDescendants().OfType<AppBar>().ShouldNotBeEmpty();
        layout.GetVisualDescendants().OfType<Drawer>().ShouldNotBeEmpty();
        layout.GetVisualDescendants().OfType<MainContent>().ShouldNotBeEmpty();
        AutomationProperties.GetName(layout).ShouldBe("Application layout");
        AutomationProperties.GetName(layout.GetVisualDescendants().OfType<MainContent>().Single()).ShouldBe("Main content");
    }

    [AvaloniaFact]
    public void MainContent_builds_generated_header_and_action_events()
    {
        var primaryClicked = false;
        var secondaryClicked = false;
        var main = new MainContent
        {
            Title = "Main content region",
            Subtitle = "Scrollable shell content",
            SecondaryActionText = "Export",
            PrimaryActionText = "Review",
            Content = new TextBlock { Text = "Body" },
        };
        main.PrimaryActionClick += (_, _) => primaryClicked = true;
        main.SecondaryActionClick += (_, _) => secondaryClicked = true;

        Show(main);
        main.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(main).ShouldBe("Main content region");
        AutomationProperties.GetHelpText(main).ShouldBe("Scrollable shell content");
        main.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Main content region").ShouldBeTrue();
        main.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Scrollable shell content").ShouldBeTrue();

        var export = main.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => string.Equals(button.Content as string, "Export", StringComparison.Ordinal));
        var review = main.GetVisualDescendants().OfType<Loam.Controls.Button>()
            .Single(button => string.Equals(button.Content as string, "Review", StringComparison.Ordinal));

        export.Variant.ShouldBe(Variant.Text);
        review.Variant.ShouldBe(Variant.Filled);
        export.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
        review.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        secondaryClicked.ShouldBeTrue();
        primaryClicked.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Layout_docked_drawer_reserves_content_space()
    {
        var drawer = new Drawer { Content = new TextBlock { Text = "Nav" } };
        var layout = new Layout
        {
            Drawer = drawer,
            Content = new MainContent { Content = new TextBlock { Text = "Body" } },
        };
        Show(layout);
        layout.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var content = layout.GetVisualDescendants().OfType<ContentPresenter>()
            .First(p => p.Name == "PART_ContentPresenter");
        content.Bounds.X.ShouldBe(240d, 1d);
    }

    [AvaloniaFact]
    public void Layout_temporary_drawer_overlays_content_and_shows_scrim()
    {
        var drawer = new Drawer
        {
            Mode = DrawerMode.Temporary,
            Open = true,
            Content = new TextBlock { Text = "Nav" },
        };
        var layout = new Layout
        {
            Drawer = drawer,
            Content = new MainContent { Content = new TextBlock { Text = "Body" } },
        };
        Show(layout);
        layout.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var content = layout.GetVisualDescendants().OfType<ContentPresenter>()
            .First(p => p.Name == "PART_ContentPresenter");
        content.Bounds.X.ShouldBe(0d, 1d);

        var scrim = layout.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_DrawerScrim");
        scrim.IsVisible.ShouldBeTrue();

        drawer.Open = false;
        Dispatcher.UIThread.RunJobs();
        scrim.IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Drawer_escape_closes_temporary_drawer()
    {
        var drawer = new Drawer { Mode = DrawerMode.Temporary, Open = true };
        Show(drawer);

        drawer.Focusable.ShouldBeTrue();
        AutomationProperties.GetName(drawer).ShouldBe("Navigation drawer");

        var key = KeyArgs(Key.Escape);
        drawer.RaiseEvent(key);

        key.Handled.ShouldBeTrue();
        drawer.Open.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Drawer_builds_navigation_items_and_autocloses_temporary()
    {
        var clicked = false;
        var drawer = new Drawer
        {
            Mode = DrawerMode.Temporary,
            Open = true,
            DrawerWidth = 180,
            Items =
            {
                new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Home", IsActive = true },
                new DrawerItem { Icon = Icons.Material.Filled.Settings, Text = "Settings", OnClick = () => clicked = true },
            },
        };
        Show(drawer);
        drawer.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var links = drawer.GetVisualDescendants().OfType<NavLink>().ToList();
        links.Count.ShouldBe(2);
        links[0].Content.ShouldBe("Home");
        links[0].IsActive.ShouldBeTrue();

        links[1].RaiseEvent(KeyArgs(Key.Enter));
        Dispatcher.UIThread.RunJobs();

        clicked.ShouldBeTrue();
        drawer.SelectedIndex.ShouldBe(1);
        drawer.Open.ShouldBeFalse();

        drawer.OpenDrawer();
        drawer.Open.ShouldBeTrue();
        drawer.Toggle();
        drawer.Open.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Drawer_mini_generated_items_keep_icons_and_hide_labels()
    {
        var drawer = new Drawer
        {
            Mini = true,
            Items =
            {
                new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Home", Label = "Home", IsActive = true },
            },
        };
        Show(drawer);
        drawer.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        drawer.Width.ShouldBe(drawer.MiniWidth);
        var link = drawer.GetVisualDescendants().OfType<NavLink>().Single();
        link.Content.ShouldBe(string.Empty);
        link.Icon.ShouldBe(Icons.Material.Filled.Home);
        AutomationProperties.GetName(link).ShouldBe("Home");
    }

    [AvaloniaFact]
    public void Drawer_generated_items_support_disabled_labels_and_color()
    {
        var clicked = false;
        var drawer = new Drawer
        {
            Items =
            {
                new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Home", Label = "Home label", Color = LoamColor.Secondary },
                new DrawerItem
                {
                    Icon = Icons.Material.Filled.Delete,
                    Text = "Disabled",
                    Label = "Disabled item",
                    IsEnabled = false,
                    Color = LoamColor.Error,
                    OnClick = () => clicked = true,
                },
            },
        };
        Show(drawer);
        drawer.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var links = drawer.GetVisualDescendants().OfType<NavLink>().ToArray();
        links.Length.ShouldBe(2);
        links[0].Color.ShouldBe(LoamColor.Secondary);
        AutomationProperties.GetName(links[0]).ShouldBe("Home label");
        links[1].IsEnabled.ShouldBeFalse();
        links[1].Color.ShouldBe(LoamColor.Error);
        AutomationProperties.GetName(links[1]).ShouldBe("Disabled item");

        var key = KeyArgs(Key.Enter);
        links[1].RaiseEvent(key);
        key.Handled.ShouldBeFalse();
        clicked.ShouldBeFalse();
        drawer.SelectedIndex.ShouldBe(-1);
    }

    [AvaloniaFact]
    public void Drawer_builds_generated_title_subtitle_and_footer_text()
    {
        var drawer = new Drawer
        {
            Title = "Temporary",
            Subtitle = "Overlay navigation",
            FooterText = "Generated Drawer.Items",
            Items =
            {
                new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Overview", Label = "Overview" },
            },
        };
        Show(drawer);
        drawer.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        AutomationProperties.GetName(drawer).ShouldBe("Temporary");
        drawer.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Temporary").ShouldBeTrue();
        drawer.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Overlay navigation").ShouldBeTrue();
        drawer.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Generated Drawer.Items").ShouldBeTrue();

        drawer.Mini = true;
        Dispatcher.UIThread.RunJobs();

        drawer.GetVisualDescendants().OfType<Text>().Any(text => text.Text == "Temporary").ShouldBeFalse();
        drawer.GetVisualDescendants().OfType<NavLink>().Single().Content.ShouldBe(string.Empty);
    }

    [AvaloniaFact]
    public void Drawer_generated_nav_scrolls_between_header_and_footer()
    {
        var drawer = new Drawer
        {
            Height = 180,
            Title = "Temporary",
            Subtitle = "Overlay navigation",
            FooterText = "Generated Drawer.Items",
            Items =
            {
                new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Overview", Label = "Overview" },
                new DrawerItem { Icon = Icons.Material.Filled.Settings, Text = "Settings", Label = "Settings" },
                new DrawerItem { Icon = Icons.Material.Filled.Delete, Text = "Disabled", Label = "Disabled item" },
                new DrawerItem { Icon = Icons.Material.Filled.Search, Text = "Search", Label = "Search" },
            },
        };
        Show(drawer);
        drawer.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var navScroll = drawer.GetVisualDescendants().OfType<ScrollViewer>()
            .Single(scroll => scroll.Content is NavMenu);
        navScroll.HorizontalScrollBarVisibility.ShouldBe(ScrollBarVisibility.Disabled);
        navScroll.VerticalScrollBarVisibility.ShouldBe(ScrollBarVisibility.Auto);
        drawer.GetVisualDescendants().OfType<Text>()
            .Any(text => text.Text == "Generated Drawer.Items")
            .ShouldBeTrue();
        drawer.GetVisualDescendants().OfType<NavLink>().Count().ShouldBe(4);
    }

    [AvaloniaFact]
    public void Drawer_toggle_open_keeps_generated_header_footer_attached_safely()
    {
        var drawer = new Drawer
        {
            Mode = DrawerMode.Temporary,
            Open = true,
            Header = new Text { Text = "Temporary", Typo = Typo.Subtitle2 },
            Footer = new Text { Text = "Generated Drawer.Items", Typo = Typo.Caption },
            Items =
            {
                new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Overview", Label = "Overview", IsActive = true },
                new DrawerItem { Icon = Icons.Material.Filled.Settings, Text = "Settings", Label = "Settings" },
            },
        };
        Show(drawer);
        drawer.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        drawer.Toggle();
        Dispatcher.UIThread.RunJobs();
        drawer.Open.ShouldBeFalse();
        drawer.Content.ShouldNotBeNull();

        drawer.Toggle();
        Dispatcher.UIThread.RunJobs();
        drawer.Open.ShouldBeTrue();
        drawer.GetVisualDescendants().OfType<NavLink>().Count().ShouldBe(2);
    }

    [AvaloniaFact]
    public void Drawer_generated_content_rebuild_reparents_header_footer_safely()
    {
        var drawer = new Drawer
        {
            Header = new Text { Text = "Docked", Typo = Typo.Subtitle2 },
            Footer = new Text { Text = "Footer", Typo = Typo.Caption },
            Items =
            {
                new DrawerItem { Icon = Icons.Material.Filled.Home, Text = "Overview", Label = "Overview" },
            },
        };
        Show(drawer);
        drawer.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        drawer.Mini = true;
        Dispatcher.UIThread.RunJobs();
        drawer.Mini.ShouldBeTrue();
        drawer.GetVisualDescendants().OfType<NavLink>().Single().Content.ShouldBe(string.Empty);

        drawer.Mini = false;
        Dispatcher.UIThread.RunJobs();
        drawer.Mini.ShouldBeFalse();
        drawer.GetVisualDescendants().OfType<NavLink>().Single().Content.ShouldBe("Overview");
    }

    [AvaloniaFact]
    public void Layout_scrim_escape_closes_temporary_drawer()
    {
        var drawer = new Drawer
        {
            Mode = DrawerMode.Temporary,
            Open = true,
            Content = new TextBlock { Text = "Nav" },
        };
        var layout = new Layout
        {
            Drawer = drawer,
            Content = new MainContent { Content = new TextBlock { Text = "Body" } },
        };
        Show(layout);
        layout.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var scrim = layout.GetVisualDescendants().OfType<Border>().First(b => b.Name == "PART_DrawerScrim");
        scrim.Focusable.ShouldBeTrue();
        scrim.ZIndex.ShouldBe(LoamZIndex.Default.Drawer);
        ((ISolidColorBrush)scrim.Background!).Color.A.ShouldBe((byte)0x52);

        var key = KeyArgs(Key.Escape);
        scrim.RaiseEvent(key);
        Dispatcher.UIThread.RunJobs();

        key.Handled.ShouldBeTrue();
        drawer.Open.ShouldBeFalse();
        scrim.IsVisible.ShouldBeFalse();
    }
}
