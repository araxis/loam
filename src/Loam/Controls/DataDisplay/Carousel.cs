using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>One slide in a <see cref="Carousel"/>, mirroring the reference API's <c>CarouselItem</c>.</summary>
public sealed class CarouselItem
{
    /// <summary>Creates an empty slide.</summary>
    public CarouselItem()
    {
    }

    /// <summary>Creates a slide with content.</summary>
    public CarouselItem(object? content) => Content = content;

    /// <summary>Creates a generated slide with title, subtitle, and semantic color.</summary>
    public CarouselItem(string title, string? subtitle, LoamColor color = LoamColor.Primary)
    {
        Title = title;
        Subtitle = subtitle;
        Color = color;
    }

    /// <summary>The slide content (string or any <see cref="Control"/>).</summary>
    public object? Content { get; set; }

    /// <summary>Title used by the generated slide layout when <see cref="Content"/> is empty.</summary>
    public string? Title { get; set; }

    /// <summary>Supporting text used by the generated slide layout when <see cref="Content"/> is empty.</summary>
    public string? Subtitle { get; set; }

    /// <summary>Semantic color used by the generated slide layout.</summary>
    public LoamColor Color { get; set; } = LoamColor.Primary;
}

/// <summary>
/// A slideshow, mirroring the reference API's <c>Carousel</c>. Shows one of the <see cref="Items"/> at a time
/// (<see cref="SelectedIndex"/>, two-way) with optional prev/next arrows (<see cref="ShowArrows"/>) and
/// clickable bullet indicators (<see cref="ShowBullets"/>); navigation wraps around.
/// </summary>
public class Carousel : TemplatedControl
{
    /// <summary>Identifies the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<Carousel, int>(nameof(SelectedIndex),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="ShowArrows"/> property.</summary>
    public static readonly StyledProperty<bool> ShowArrowsProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(ShowArrows), true);

    /// <summary>Identifies the <see cref="ShowBullets"/> property.</summary>
    public static readonly StyledProperty<bool> ShowBulletsProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(ShowBullets), true);

    /// <summary>Identifies the <see cref="AutoPlay"/> property.</summary>
    public static readonly StyledProperty<bool> AutoPlayProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(AutoPlay));

    /// <summary>Identifies the <see cref="AutoPlayInterval"/> property.</summary>
    public static readonly StyledProperty<TimeSpan> AutoPlayIntervalProperty =
        AvaloniaProperty.Register<Carousel, TimeSpan>(nameof(AutoPlayInterval), TimeSpan.FromSeconds(4));

    private readonly List<Border> _bullets = new();
    private readonly List<IDisposable?> _bulletBindings = new();
    private ContentControl? _content;
    private StackPanel? _bulletPanel;
    private Control? _prev;
    private Control? _next;
    private DispatcherTimer? _autoPlayTimer;
    private bool _attached;

    /// <summary>Creates the carousel.</summary>
    public Carousel()
    {
        Focusable = true;
        Items.CollectionChanged += OnItemsChanged;
        InteractionAssist.SetAutomationName(this, "Carousel");
        UpdateAutomation();
    }

    /// <summary>Raised after the visible slide index changes.</summary>
    public event EventHandler<int>? SelectedIndexChanged;

    /// <summary>The slides.</summary>
    public ObservableCollection<CarouselItem> Items { get; } = new();

    /// <summary>The visible slide index (two-way).</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Whether prev/next arrows are shown. Mirrors the reference API's <c>ShowArrows</c>.</summary>
    public bool ShowArrows
    {
        get => GetValue(ShowArrowsProperty);
        set => SetValue(ShowArrowsProperty, value);
    }

    /// <summary>Whether bullet indicators are shown. Mirrors the reference API's <c>ShowBullets</c>.</summary>
    public bool ShowBullets
    {
        get => GetValue(ShowBulletsProperty);
        set => SetValue(ShowBulletsProperty, value);
    }

    /// <summary>Whether the carousel automatically advances while attached, enabled, and showing at least two slides.</summary>
    public bool AutoPlay
    {
        get => GetValue(AutoPlayProperty);
        set => SetValue(AutoPlayProperty, value);
    }

    /// <summary>How often <see cref="AutoPlay"/> advances to the next slide.</summary>
    public TimeSpan AutoPlayInterval
    {
        get => GetValue(AutoPlayIntervalProperty);
        set => SetValue(AutoPlayIntervalProperty, value);
    }

    /// <summary>Advances to the next slide, wrapping to the first.</summary>
    public void Next()
    {
        if (IsEnabled && Items.Count > 0)
        {
            GoTo((SelectedIndex + 1) % Items.Count);
        }
    }

    /// <summary>Returns to the previous slide, wrapping to the last.</summary>
    public void Previous()
    {
        if (IsEnabled && Items.Count > 0)
        {
            GoTo((SelectedIndex - 1 + Items.Count) % Items.Count);
        }
    }

    /// <summary>Moves to the requested slide index, clamping to the available slides.</summary>
    public void GoTo(int index)
    {
        if (!IsEnabled || Items.Count == 0)
        {
            return;
        }

        SelectedIndex = Math.Clamp(index, 0, Items.Count - 1);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Carousel);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        UpdateAutoPlay();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _attached = false;
        StopAutoPlay();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _content = e.NameScope.Find("PART_Content") as ContentControl;
        _bulletPanel = e.NameScope.Find("PART_Bullets") as StackPanel;
        _prev = e.NameScope.Find("PART_Prev") as Control;
        _next = e.NameScope.Find("PART_Next") as Control;

        if (_prev is not null)
        {
            _prev.Focusable = true;
            InteractionAssist.SetAutomationName(_prev, "Previous slide");
            if (_prev is global::Avalonia.Controls.Button previousButton)
            {
                previousButton.Click += (_, _) => Previous();
            }
            else
            {
                _prev.PointerPressed += (_, _) => Previous();
            }

            _prev.KeyDown += (_, args) =>
            {
                if (IsEnabled && InteractionAssist.IsActivationKey(args.Key))
                {
                    Previous();
                    args.Handled = true;
                }
            };
        }

        if (_next is not null)
        {
            _next.Focusable = true;
            InteractionAssist.SetAutomationName(_next, "Next slide");
            if (_next is global::Avalonia.Controls.Button nextButton)
            {
                nextButton.Click += (_, _) => Next();
            }
            else
            {
                _next.PointerPressed += (_, _) => Next();
            }

            _next.KeyDown += (_, args) =>
            {
                if (IsEnabled && InteractionAssist.IsActivationKey(args.Key))
                {
                    Next();
                    args.Handled = true;
                }
            };
        }

        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedIndexProperty)
        {
            ShowContent();
            UpdateBullets();
            UpdateAutomation();
            SelectedIndexChanged?.Invoke(this, SelectedIndex);
        }
        else if (change.Property == ShowArrowsProperty || change.Property == ShowBulletsProperty ||
                 change.Property == IsEnabledProperty)
        {
            Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
            UpdateChrome();
            UpdateAutoPlay();
        }
        else if (change.Property == AutoPlayProperty || change.Property == AutoPlayIntervalProperty)
        {
            UpdateAutoPlay();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || !IsEnabled || Items.Count == 0)
        {
            return;
        }

        if (e.Key == Key.Left)
        {
            Previous();
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            Next();
            e.Handled = true;
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (_bulletPanel is not null)
        {
            foreach (var binding in _bulletBindings)
            {
                binding?.Dispose();
            }

            _bulletPanel.Children.Clear();
            _bullets.Clear();
            _bulletBindings.Clear();
            for (var i = 0; i < Items.Count; i++)
            {
                var bullet = new Border
                {
                    Width = 8,
                    Height = 8,
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(3, 0),
                    Cursor = new Cursor(StandardCursorType.Hand),
                    Focusable = true,
                };
                var index = i;
                bullet.PointerPressed += (_, _) =>
                {
                    if (IsEnabled)
                    {
                        GoTo(index);
                    }
                };
                bullet.KeyDown += (_, args) =>
                {
                    if (IsEnabled && InteractionAssist.IsActivationKey(args.Key))
                    {
                        GoTo(index);
                        args.Handled = true;
                    }
                };
                InteractionAssist.SetAutomationName(bullet, $"Slide {index + 1}");
                _bulletPanel.Children.Add(bullet);
                _bullets.Add(bullet);
                _bulletBindings.Add(null);
            }
        }

        ShowContent();
        UpdateBullets();
        UpdateChrome();
        UpdateAutomation();
        UpdateAutoPlay();
    }

    private void ShowContent()
    {
        if (_content is null)
        {
            return;
        }

        if (Items.Count == 0)
        {
            _content.Content = new Text
            {
                Text = "No slides",
                Color = LoamColor.Secondary,
                Typo = Typo.Body2,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            if (SelectedIndex != 0)
            {
                SelectedIndex = 0;
            }

            return;
        }

        var clamped = Math.Clamp(SelectedIndex, 0, Items.Count - 1);
        if (clamped != SelectedIndex)
        {
            SelectedIndex = clamped;
            return;
        }

        if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
        {
            _content.Content = BuildSlideContent(Items[SelectedIndex]);
        }
    }

    private Control BuildSlideContent(CarouselItem item)
    {
        if (item.Content is Control control)
        {
            return control;
        }

        if (item.Content is not null)
        {
            return new ContentControl { Content = item.Content };
        }

        var title = string.IsNullOrWhiteSpace(item.Title) ? $"Slide {SelectedIndex + 1}" : item.Title;
        var tokens = SemanticColor.Resolve(item.Color);
        var titleText = new Text
        {
            Text = title,
            Typo = Typo.H6,
            Color = LoamColor.Inherit,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        };
        titleText.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(tokens.FillText));

        var stack = new StackPanel
        {
            Spacing = 6,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Children = { titleText },
        };

        if (!string.IsNullOrWhiteSpace(item.Subtitle))
        {
            var subtitleText = new Text
            {
                Text = item.Subtitle,
                Typo = Typo.Body2,
                Color = LoamColor.Inherit,
                Opacity = 0.82,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            };
            subtitleText.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(tokens.FillText));
            stack.Children.Add(subtitleText);
        }

        var surface = new Border
        {
            Child = stack,
            Padding = new Thickness(56, 24),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };
        surface.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));
        return surface;
    }

    private void UpdateBullets()
    {
        for (var i = 0; i < _bullets.Count; i++)
        {
            _bulletBindings[i]?.Dispose();
            _bullets[i].IsEnabled = IsEnabled;
            AutomationProperties.SetHelpText(_bullets[i], i == SelectedIndex ? "Selected" : "Not selected");
            _bulletBindings[i] = _bullets[i].Bind(Border.BackgroundProperty, this.GetResourceObservable(
                i == SelectedIndex ? LoamTokens.Primary : LoamTokens.Palette(nameof(LoamPalette.GrayLight))));
        }
    }

    private void UpdateChrome()
    {
        if (_prev is not null)
        {
            _prev.IsVisible = ShowArrows;
            _prev.IsEnabled = IsEnabled && Items.Count > 0;
        }

        if (_next is not null)
        {
            _next.IsVisible = ShowArrows;
            _next.IsEnabled = IsEnabled && Items.Count > 0;
        }

        if (_bulletPanel is not null)
        {
            _bulletPanel.IsVisible = ShowBullets;
        }
    }

    private void UpdateAutomation()
    {
        AutomationProperties.SetHelpText(this, Items.Count == 0 ? "No slides" : $"Slide {SelectedIndex + 1} of {Items.Count}");
    }

    private void UpdateAutoPlay()
    {
        StopAutoPlay();
        if (!_attached || !AutoPlay || !IsEnabled || Items.Count < 2 || AutoPlayInterval <= TimeSpan.Zero)
        {
            return;
        }

        _autoPlayTimer = new DispatcherTimer { Interval = AutoPlayInterval };
        _autoPlayTimer.Tick += OnAutoPlayTick;
        _autoPlayTimer.Start();
    }

    private void StopAutoPlay()
    {
        if (_autoPlayTimer is null)
        {
            return;
        }

        _autoPlayTimer.Stop();
        _autoPlayTimer.Tick -= OnAutoPlayTick;
        _autoPlayTimer = null;
    }

    private void OnAutoPlayTick(object? sender, EventArgs e) => Next();
}
