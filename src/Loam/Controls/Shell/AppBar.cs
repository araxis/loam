using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A top application bar. A full-width, elevated, colored toolbar surface with built-in title,
/// navigation icon, and action slots. Set the inherited content only for custom toolbar layouts.
/// </summary>
public class AppBar : ContentControl
{
    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<AppBar, LoamColor>(nameof(Color), LoamColor.Default);

    /// <summary>Identifies the <see cref="Elevation"/> property.</summary>
    public static readonly StyledProperty<int> ElevationProperty =
        AvaloniaProperty.Register<AppBar, int>(nameof(Elevation), 4);

    /// <summary>Identifies the <see cref="Dense"/> property.</summary>
    public static readonly StyledProperty<bool> DenseProperty =
        AvaloniaProperty.Register<AppBar, bool>(nameof(Dense));

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AppBar, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Subtitle"/> property.</summary>
    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<AppBar, string?>(nameof(Subtitle));

    /// <summary>Identifies the <see cref="NavigationIcon"/> property.</summary>
    public static readonly StyledProperty<string?> NavigationIconProperty =
        AvaloniaProperty.Register<AppBar, string?>(nameof(NavigationIcon));

    /// <summary>Identifies the <see cref="NavigationLabel"/> property.</summary>
    public static readonly StyledProperty<string?> NavigationLabelProperty =
        AvaloniaProperty.Register<AppBar, string?>(nameof(NavigationLabel), "Navigation");

    private Action? _navigationAction;
    private ContentPresenter? _contentPresenter;
    private Border? _root;
    private IDisposable? _backgroundBinding;
    private IDisposable? _foregroundBinding;
    private IDisposable? _shadowBinding;

    /// <summary>Creates the app bar.</summary>
    public AppBar()
    {
        Actions.CollectionChanged += (_, _) => UpdateContent();
        InteractionAssist.SetAutomationName(this, "App bar");
    }

    /// <summary>Raised when the built-in navigation icon is clicked.</summary>
    public event EventHandler<RoutedEventArgs>? NavigationClick;

    /// <summary>App-bar color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Shadow depth. Mirrors the reference API's <c>Elevation</c>.</summary>
    public int Elevation
    {
        get => GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>Reduced height. Mirrors the reference API's <c>Dense</c>.</summary>
    public bool Dense
    {
        get => GetValue(DenseProperty);
        set => SetValue(DenseProperty, value);
    }

    /// <summary>Built-in title text.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Built-in subtitle text shown below <see cref="Title"/>.</summary>
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Built-in navigation icon path data, usually a menu or back icon.</summary>
    public string? NavigationIcon
    {
        get => GetValue(NavigationIconProperty);
        set => SetValue(NavigationIconProperty, value);
    }

    /// <summary>Accessible label for the built-in navigation button.</summary>
    public string? NavigationLabel
    {
        get => GetValue(NavigationLabelProperty);
        set => SetValue(NavigationLabelProperty, value);
    }

    /// <summary>Optional callback invoked with <see cref="NavigationClick"/>.</summary>
    public Action? NavigationAction
    {
        get => _navigationAction;
        set
        {
            _navigationAction = value;
            UpdateContent();
        }
    }

    /// <summary>Built-in trailing icon actions.</summary>
    public AvaloniaList<AppBarAction> Actions { get; } = [];

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(AppBar);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find("PART_Root") as Border;
        _contentPresenter = e.NameScope.Find("PART_ContentPresenter") as ContentPresenter;
        ApplyVisual();
        UpdateContent();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColorProperty || change.Property == ElevationProperty || change.Property == DenseProperty)
        {
            ApplyVisual();
            UpdateContent();
        }
        else if (change.Property == TitleProperty ||
                 change.Property == SubtitleProperty ||
                 change.Property == NavigationIconProperty ||
                 change.Property == NavigationLabelProperty ||
                 change.Property == ContentProperty)
        {
            UpdateContent();
        }
    }

    private void ApplyVisual()
    {
        Height = Dense
            ? InteractionAssist.DoubleToken(this, LoamTokens.DensityInteractiveLarge, 48)
            : InteractionAssist.DoubleToken(this, LoamTokens.AppBarHeight, 64);
        InteractionAssist.ApplyZIndex(this, LoamTokens.ZIndex(nameof(LoamZIndex.AppBar)), LoamZIndex.Default.AppBar);

        string backgroundKey;
        string foregroundKey;
        if (Color is LoamColor.Default or LoamColor.Inherit)
        {
            backgroundKey = LoamTokens.Palette(nameof(LoamPalette.AppbarBackground));
            foregroundKey = LoamTokens.Palette(nameof(LoamPalette.AppbarText));
        }
        else
        {
            var tokens = SemanticColor.Resolve(Color);
            backgroundKey = tokens.Fill;
            foregroundKey = tokens.FillText;
        }

        _foregroundBinding?.Dispose();
        _foregroundBinding = this.Bind(ForegroundProperty, this.GetResourceObservable(foregroundKey));

        if (_root is null)
        {
            return;
        }

        _backgroundBinding?.Dispose();
        _backgroundBinding = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(backgroundKey));
        _shadowBinding?.Dispose();
        _shadowBinding = _root.Bind(Border.BoxShadowProperty, this.GetResourceObservable(LoamTokens.Elevation(Elevation)));
    }

    private void UpdateContent()
    {
        if (_contentPresenter is null)
        {
            return;
        }

        _contentPresenter.Content = Content ?? BuildDefaultToolbar();
        InteractionAssist.SetAutomationName(this, Title, Subtitle, Content, "App bar");
    }

    private global::Avalonia.Controls.Grid BuildDefaultToolbar()
    {
        var toolbar = new global::Avalonia.Controls.Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };

        if (!string.IsNullOrWhiteSpace(NavigationIcon))
        {
            toolbar.Children.Add(CreateNavigationButton());
        }

        Control titleContent;
        var title = new Text
        {
            Text = Title,
            Typo = Typo.Subtitle1,
            Color = LoamColor.Inherit,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (string.IsNullOrWhiteSpace(Subtitle))
        {
            titleContent = title;
        }
        else
        {
            titleContent = new StackPanel
            {
                Spacing = 0,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    title,
                    new Text
                    {
                        Text = Subtitle,
                        Typo = Typo.Caption,
                        Color = LoamColor.Inherit,
                    },
                },
            };
        }

        titleContent.Margin = new Thickness(string.IsNullOrWhiteSpace(NavigationIcon) ? 0 : 4, 0, 0, 0);
        global::Avalonia.Controls.Grid.SetColumn(titleContent, 1);
        toolbar.Children.Add(titleContent);

        if (Actions.Count > 0)
        {
            var actionStrip = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };
            foreach (var action in Actions)
            {
                actionStrip.Children.Add(CreateActionButton(action));
            }

            global::Avalonia.Controls.Grid.SetColumn(actionStrip, 2);
            toolbar.Children.Add(actionStrip);
        }

        return toolbar;
    }

    private IconButton CreateNavigationButton()
    {
        var button = CreateToolbarButton(NavigationIcon, NavigationLabel, Variant.Text, LoamColor.Inherit, LoamSize.Medium, true);
        button.Click += (_, args) =>
        {
            if (button.IsEnabled)
            {
                NavigationAction?.Invoke();
                NavigationClick?.Invoke(this, args);
            }
        };
        return button;
    }

    private IconButton CreateActionButton(AppBarAction action)
    {
        var button = CreateToolbarButton(action.Icon, action.Label, action.Variant, action.Color, action.Size, action.IsEnabled);
        button.Click += (_, _) =>
        {
            if (button.IsEnabled)
            {
                action.OnClick?.Invoke();
            }
        };
        return button;
    }

    private IconButton CreateToolbarButton(
        string? icon,
        string? label,
        Variant variant,
        LoamColor color,
        LoamSize size,
        bool isEnabled)
    {
        var button = new IconButton
        {
            Icon = icon,
            Variant = variant,
            Color = color,
            Size = size,
            IsEnabled = isEnabled,
            VerticalAlignment = VerticalAlignment.Center,
        };
        if (color is LoamColor.Inherit)
        {
            button.Bind(TemplatedControl.ForegroundProperty, this.GetObservable(ForegroundProperty));
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            AutomationProperties.SetName(button, label);
        }

        return button;
    }
}
