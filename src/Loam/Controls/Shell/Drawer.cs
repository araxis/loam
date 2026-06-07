using System.Collections.Specialized;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>The layout behavior for a <see cref="Drawer"/>.</summary>
public enum DrawerMode
{
    /// <summary>The drawer reserves horizontal layout space.</summary>
    Docked,

    /// <summary>The drawer floats over the main content and may show a scrim.</summary>
    Temporary,
}

/// <summary>A standard navigation item rendered by <see cref="Drawer"/>.</summary>
public sealed class DrawerItem
{
    /// <summary>Leading icon path data.</summary>
    public string? Icon { get; set; }

    /// <summary>Visible item text.</summary>
    public string? Text { get; set; }

    /// <summary>Whether the item is currently active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Whether the generated navigation item is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Accessible label used when the visible label is hidden, such as mini drawers.</summary>
    public string? Label { get; set; }

    /// <summary>Active accent color for the generated navigation item.</summary>
    public LoamColor Color { get; set; } = LoamColor.Primary;

    /// <summary>Optional URL launched by the generated navigation link.</summary>
    public string? Href { get; set; }

    /// <summary>Optional callback invoked by the generated navigation link.</summary>
    public Action? OnClick { get; set; }
}

/// <summary>
/// A side navigation panel, mirroring the reference API's <c>Drawer</c>. Left-anchored; toggling
/// <see cref="Open"/> slides it between <see cref="DrawerWidth"/> and 0, and <see cref="Mini"/>
/// collapses it to <see cref="MiniWidth"/>. Place it in a <see cref="Layout"/>'s drawer slot.
/// </summary>
public class Drawer : ContentControl
{
    /// <summary>Identifies the <see cref="Open"/> property.</summary>
    public static readonly StyledProperty<bool> OpenProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(Open), defaultValue: true);

    /// <summary>Identifies the <see cref="Mini"/> property.</summary>
    public static readonly StyledProperty<bool> MiniProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(Mini));

    /// <summary>Identifies the <see cref="DrawerWidth"/> property.</summary>
    public static readonly StyledProperty<double> DrawerWidthProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(DrawerWidth), 240);

    /// <summary>Identifies the <see cref="MiniWidth"/> property.</summary>
    public static readonly StyledProperty<double> MiniWidthProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(MiniWidth), 56);

    /// <summary>Identifies the <see cref="Mode"/> property.</summary>
    public static readonly StyledProperty<DrawerMode> ModeProperty =
        AvaloniaProperty.Register<Drawer, DrawerMode>(nameof(Mode), DrawerMode.Docked);

    /// <summary>Identifies the <see cref="ShowScrim"/> property.</summary>
    public static readonly StyledProperty<bool> ShowScrimProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(ShowScrim), true);

    /// <summary>Identifies the <see cref="CloseOnScrimClick"/> property.</summary>
    public static readonly StyledProperty<bool> CloseOnScrimClickProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(CloseOnScrimClick), true);

    /// <summary>Identifies the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<Drawer, int>(nameof(SelectedIndex), -1);

    /// <summary>Identifies the <see cref="ContentPadding"/> property.</summary>
    public static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<Drawer, Thickness>(nameof(ContentPadding), new Thickness(8));

    /// <summary>Identifies the <see cref="AutoCloseTemporary"/> property.</summary>
    public static readonly StyledProperty<bool> AutoCloseTemporaryProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(AutoCloseTemporary), true);

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Drawer, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Subtitle"/> property.</summary>
    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<Drawer, string?>(nameof(Subtitle));

    /// <summary>Identifies the <see cref="FooterText"/> property.</summary>
    public static readonly StyledProperty<string?> FooterTextProperty =
        AvaloniaProperty.Register<Drawer, string?>(nameof(FooterText));

    private bool _usingGeneratedContent;
    private bool _updatingContent;
    private bool _hasCustomContent;
    private object? _header;
    private object? _footer;
    private Border? _generatedRoot;
    private NavMenu? _generatedNav;
    private ContentControl? _generatedHeader;
    private ContentControl? _generatedFooter;

    /// <summary>Creates the drawer.</summary>
    public Drawer()
    {
        ClipToBounds = true;
        Focusable = true;
        Width = ResolveWidth(Open, Mini, DrawerWidth, MiniWidth);
        UpdateTransitions();
        InteractionAssist.SetAutomationName(this, "Navigation drawer");
        Items.CollectionChanged += OnGeneratedContentChanged;
    }

    /// <summary>Standard navigation items rendered when no custom <see cref="ContentControl.Content"/> is supplied.</summary>
    public AvaloniaList<DrawerItem> Items { get; } = [];

    /// <summary>Optional content rendered above generated navigation items.</summary>
    public object? Header
    {
        get => _header;
        set
        {
            _header = value;
            SyncGeneratedContent();
        }
    }

    /// <summary>Optional content rendered below generated navigation items.</summary>
    public object? Footer
    {
        get => _footer;
        set
        {
            _footer = value;
            SyncGeneratedContent();
        }
    }

    /// <summary>Whether the drawer is open. Mirrors the reference API's <c>Open</c>.</summary>
    public bool Open
    {
        get => GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    /// <summary>Collapse to <see cref="MiniWidth"/> instead of hiding. Mirrors the reference API's mini variant.</summary>
    public bool Mini
    {
        get => GetValue(MiniProperty);
        set => SetValue(MiniProperty, value);
    }

    /// <summary>Expanded width. Mirrors the reference API's <c>Width</c>.</summary>
    public double DrawerWidth
    {
        get => GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    /// <summary>Collapsed (mini) width. Mirrors the reference API's <c>MiniWidth</c>.</summary>
    public double MiniWidth
    {
        get => GetValue(MiniWidthProperty);
        set => SetValue(MiniWidthProperty, value);
    }

    /// <summary>Whether the drawer is docked in layout or temporary over content.</summary>
    public DrawerMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>Whether a temporary drawer displays a scrim over content when open.</summary>
    public bool ShowScrim
    {
        get => GetValue(ShowScrimProperty);
        set => SetValue(ShowScrimProperty, value);
    }

    /// <summary>Whether clicking a temporary drawer scrim closes the drawer.</summary>
    public bool CloseOnScrimClick
    {
        get => GetValue(CloseOnScrimClickProperty);
        set => SetValue(CloseOnScrimClickProperty, value);
    }

    /// <summary>Selected generated navigation item index.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Padding around generated navigation content.</summary>
    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    /// <summary>Whether selecting a generated item closes a temporary drawer.</summary>
    public bool AutoCloseTemporary
    {
        get => GetValue(AutoCloseTemporaryProperty);
        set => SetValue(AutoCloseTemporaryProperty, value);
    }

    /// <summary>Generated drawer header title. Leave unset to use a custom <see cref="Header"/>.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Generated drawer header supporting text.</summary>
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Generated footer note shown below navigation items.</summary>
    public string? FooterText
    {
        get => GetValue(FooterTextProperty);
        set => SetValue(FooterTextProperty, value);
    }

    /// <summary>Opens the drawer.</summary>
    public void OpenDrawer() => Open = true;

    /// <summary>Closes the drawer.</summary>
    public void CloseDrawer() => Open = false;

    /// <summary>Toggles the drawer open state.</summary>
    public void Toggle() => Open = !Open;

    /// <summary>The target width for the given open/mini state.</summary>
    public static double ResolveWidth(bool open, bool mini, double width, double miniWidth) =>
        !open ? 0 : mini ? miniWidth : width;

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Drawer);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateTransitions();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OpenProperty || change.Property == MiniProperty ||
            change.Property == DrawerWidthProperty || change.Property == MiniWidthProperty ||
            change.Property == ModeProperty)
        {
            Width = ResolveWidth(Open, Mini, DrawerWidth, MiniWidth);
            InvalidateMeasure();
            if (change.Property == MiniProperty ||
                change.Property == DrawerWidthProperty ||
                change.Property == MiniWidthProperty)
            {
                SyncGeneratedContent();
            }
        }
        else if (change.Property == IsEnabledProperty)
        {
            Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
        }
        else if (change.Property == SelectedIndexProperty ||
                 change.Property == ContentPaddingProperty ||
                 change.Property == AutoCloseTemporaryProperty ||
                 change.Property == TitleProperty ||
                 change.Property == SubtitleProperty ||
                 change.Property == FooterTextProperty)
        {
            SyncGeneratedContent();
            InteractionAssist.SetAutomationName(this, Title, Header, "Navigation drawer");
        }
        else if (change.Property == ContentProperty && !_updatingContent)
        {
            _usingGeneratedContent = false;
            _hasCustomContent = Content is not null;
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsEnabled && e.Key == Key.Escape && Mode == DrawerMode.Temporary && Open)
        {
            Open = false;
            e.Handled = true;
        }
    }

    private void UpdateTransitions()
    {
        Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = WidthProperty,
                Duration = InteractionAssist.DurationToken(this, LoamTokens.MotionDurationShort3, TimeSpan.FromMilliseconds(180)),
                Easing = new CubicEaseOut(),
            },
        };
    }

    private void OnGeneratedContentChanged(object? sender, NotifyCollectionChangedEventArgs e) => SyncGeneratedContent();

    private void SyncGeneratedContent()
    {
        if (!HasGeneratedContent)
        {
            return;
        }

        if (_hasCustomContent && !_usingGeneratedContent)
        {
            return;
        }

        _usingGeneratedContent = true;
        _hasCustomContent = false;
        _updatingContent = true;
        try
        {
            if (_generatedRoot is null || !ReferenceEquals(Content, _generatedRoot))
            {
                Content = BuildGeneratedContent();
            }
            else
            {
                UpdateGeneratedContent();
            }
        }
        finally
        {
            _updatingContent = false;
        }
    }

    private bool HasGeneratedContent =>
        _usingGeneratedContent ||
        Items.Count > 0 ||
        Header is not null ||
        Footer is not null ||
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Subtitle) ||
        !string.IsNullOrWhiteSpace(FooterText);

    private Border BuildGeneratedContent()
    {
        _generatedNav = new NavMenu();
        _generatedHeader = new ContentControl { Margin = new Thickness(0, 0, 0, 8) };
        _generatedFooter = new ContentControl { Margin = new Thickness(0, 8, 0, 0) };
        var content = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(_generatedHeader, Dock.Top);
        content.Children.Add(_generatedHeader);

        DockPanel.SetDock(_generatedFooter, Dock.Bottom);
        content.Children.Add(_generatedFooter);

        content.Children.Add(new ScrollViewer
        {
            Content = _generatedNav,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        });
        var root = new Border { Child = content };
        AutomationProperties.SetName(root, "Drawer navigation");
        _generatedRoot = root;
        UpdateGeneratedContent();
        return root;
    }

    private void UpdateGeneratedContent()
    {
        if (_generatedRoot is not null)
        {
            _generatedRoot.Padding = ContentPadding;
        }

        if (_generatedHeader is not null)
        {
            var header = Header ?? BuildGeneratedHeader();
            _generatedHeader.Content = Mini ? null : header;
            _generatedHeader.IsVisible = !Mini && header is not null;
        }

        if (_generatedFooter is not null)
        {
            var footer = Footer ?? BuildGeneratedFooter();
            _generatedFooter.Content = Mini ? null : footer;
            _generatedFooter.IsVisible = !Mini && footer is not null;
        }

        if (_generatedNav is null)
        {
            return;
        }

        _generatedNav.Width = Mini ? MiniWidth : DrawerWidth;
        _generatedNav.Children.Clear();
        for (var i = 0; i < Items.Count; i++)
        {
            var index = i;
            var item = Items[i];
            var link = new NavLink
            {
                Icon = item.Icon,
                Content = Mini ? string.Empty : item.Text,
                Label = item.Label ?? item.Text,
                Href = item.Href,
                IsActive = item.IsActive || index == SelectedIndex,
                IsEnabled = item.IsEnabled,
                Color = item.Color,
                OnClick = () =>
                {
                    SelectedIndex = index;
                    item.OnClick?.Invoke();
                    if (AutoCloseTemporary && Mode == DrawerMode.Temporary)
                    {
                        Open = false;
                    }
                },
            };
            InteractionAssist.SetAutomationName(link, item.Label, item.Text, item.Href, "Drawer item");
            _generatedNav.Children.Add(link);
        }
    }

    private StackPanel? BuildGeneratedHeader()
    {
        if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Subtitle))
        {
            return null;
        }

        var stack = new StackPanel
        {
            Spacing = 2,
            Margin = new Thickness(8, 8, 8, 12),
        };

        if (!string.IsNullOrWhiteSpace(Title))
        {
            stack.Children.Add(new Text { Text = Title, Typo = Typo.Subtitle2 });
        }

        if (!string.IsNullOrWhiteSpace(Subtitle))
        {
            stack.Children.Add(new Text
            {
                Text = Subtitle,
                Typo = Typo.Caption,
                Color = LoamColor.Secondary,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            });
        }

        return stack;
    }

    private Text? BuildGeneratedFooter()
    {
        if (string.IsNullOrWhiteSpace(FooterText))
        {
            return null;
        }

        return new Text
        {
            Text = FooterText,
            Typo = Typo.Caption,
            Color = LoamColor.Secondary,
            Margin = new Thickness(8, 12, 8, 8),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        };
    }
}
