using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>One slide in a <see cref="Carousel"/>, mirroring MudBlazor's <c>MudCarouselItem</c>.</summary>
public sealed class CarouselItem
{
    /// <summary>Creates an empty slide.</summary>
    public CarouselItem()
    {
    }

    /// <summary>Creates a slide with content.</summary>
    public CarouselItem(object? content) => Content = content;

    /// <summary>The slide content (string or any <see cref="Control"/>).</summary>
    public object? Content { get; set; }
}

/// <summary>
/// A slideshow, mirroring MudBlazor's <c>MudCarousel</c>. Shows one of the <see cref="Items"/> at a time
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

    private readonly List<Border> _bullets = new();
    private ContentControl? _content;
    private StackPanel? _bulletPanel;
    private Control? _prev;
    private Control? _next;

    /// <summary>Creates the carousel.</summary>
    public Carousel() => Items.CollectionChanged += OnItemsChanged;

    /// <summary>The slides.</summary>
    public ObservableCollection<CarouselItem> Items { get; } = new();

    /// <summary>The visible slide index (two-way).</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Whether prev/next arrows are shown. Mirrors MudBlazor's <c>ShowArrows</c>.</summary>
    public bool ShowArrows
    {
        get => GetValue(ShowArrowsProperty);
        set => SetValue(ShowArrowsProperty, value);
    }

    /// <summary>Whether bullet indicators are shown. Mirrors MudBlazor's <c>ShowBullets</c>.</summary>
    public bool ShowBullets
    {
        get => GetValue(ShowBulletsProperty);
        set => SetValue(ShowBulletsProperty, value);
    }

    /// <summary>Advances to the next slide, wrapping to the first.</summary>
    public void Next()
    {
        if (Items.Count > 0)
        {
            SelectedIndex = (SelectedIndex + 1) % Items.Count;
        }
    }

    /// <summary>Returns to the previous slide, wrapping to the last.</summary>
    public void Previous()
    {
        if (Items.Count > 0)
        {
            SelectedIndex = (SelectedIndex - 1 + Items.Count) % Items.Count;
        }
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Carousel);

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
            _prev.PointerPressed += (_, _) => Previous();
        }

        if (_next is not null)
        {
            _next.PointerPressed += (_, _) => Next();
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
        }
        else if (change.Property == ShowArrowsProperty || change.Property == ShowBulletsProperty)
        {
            UpdateChrome();
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (_bulletPanel is not null)
        {
            _bulletPanel.Children.Clear();
            _bullets.Clear();
            for (var i = 0; i < Items.Count; i++)
            {
                var bullet = new Border
                {
                    Width = 8,
                    Height = 8,
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(3, 0),
                    Cursor = new Cursor(StandardCursorType.Hand),
                };
                var index = i;
                bullet.PointerPressed += (_, _) => SelectedIndex = index;
                _bulletPanel.Children.Add(bullet);
                _bullets.Add(bullet);
            }
        }

        ShowContent();
        UpdateBullets();
        UpdateChrome();
    }

    private void ShowContent()
    {
        if (_content is not null && SelectedIndex >= 0 && SelectedIndex < Items.Count)
        {
            _content.Content = Items[SelectedIndex].Content;
        }
    }

    private void UpdateBullets()
    {
        for (var i = 0; i < _bullets.Count; i++)
        {
            _bullets[i].Bind(Border.BackgroundProperty, this.GetResourceObservable(
                i == SelectedIndex ? LoamTokens.Primary : LoamTokens.Palette(nameof(LoamPalette.GrayLight))));
        }
    }

    private void UpdateChrome()
    {
        if (_prev is not null)
        {
            _prev.IsVisible = ShowArrows;
        }

        if (_next is not null)
        {
            _next.IsVisible = ShowArrows;
        }

        if (_bulletPanel is not null)
        {
            _bulletPanel.IsVisible = ShowBullets;
        }
    }
}
