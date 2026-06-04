using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Loam;

namespace Loam.Controls;

/// <summary>
/// A page navigator, mirroring the reference API's <c>Pagination</c>. Renders first/last boundary pages,
/// a window of <see cref="MiddleCount"/> pages around the two-way <see cref="Selected"/> page, ellipses
/// for gaps, and previous/next arrows. The selected page button is filled with <see cref="Color"/>.
/// </summary>
public class Pagination : TemplatedControl
{
    /// <summary>Identifies the <see cref="Count"/> property.</summary>
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(Count), 1);

    /// <summary>Identifies the <see cref="Selected"/> property.</summary>
    public static readonly StyledProperty<int> SelectedProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(Selected), 1,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Pagination, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="BoundaryCount"/> property.</summary>
    public static readonly StyledProperty<int> BoundaryCountProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(BoundaryCount), 1);

    /// <summary>Identifies the <see cref="MiddleCount"/> property.</summary>
    public static readonly StyledProperty<int> MiddleCountProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(MiddleCount), 3);

    private StackPanel? _items;

    /// <summary>The total number of pages. Mirrors the reference API's <c>Count</c>.</summary>
    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>The selected page (1-based, two-way). Mirrors the reference API's <c>Selected</c>.</summary>
    public int Selected
    {
        get => GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }

    /// <summary>Selected-page color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Pages shown at each end. Mirrors the reference API's <c>BoundaryCount</c>.</summary>
    public int BoundaryCount
    {
        get => GetValue(BoundaryCountProperty);
        set => SetValue(BoundaryCountProperty, value);
    }

    /// <summary>Pages shown around the selection. Mirrors the reference API's <c>MiddleCount</c>.</summary>
    public int MiddleCount
    {
        get => GetValue(MiddleCountProperty);
        set => SetValue(MiddleCountProperty, value);
    }

    /// <summary>
    /// Computes the page layout: a list of 1-based page numbers with <c>0</c> marking an ellipsis gap.
    /// </summary>
    public static IReadOnlyList<int> BuildPages(int count, int selected, int boundary, int middle)
    {
        if (count <= 0)
        {
            return Array.Empty<int>();
        }

        selected = Math.Clamp(selected, 1, count);
        boundary = Math.Max(0, boundary);
        var window = Math.Max(1, middle);
        var half = (window - 1) / 2;

        var set = new SortedSet<int>();
        void AddRange(int from, int to)
        {
            for (var i = Math.Max(1, from); i <= Math.Min(count, to); i++)
            {
                set.Add(i);
            }
        }

        AddRange(1, boundary);
        AddRange(count - boundary + 1, count);

        // Centered window of `window` pages, shifted to stay fully in range near the edges.
        var start = selected - half;
        var end = start + window - 1;
        if (start < 1)
        {
            end += 1 - start;
            start = 1;
        }

        if (end > count)
        {
            start -= end - count;
            end = count;
        }

        AddRange(start, end);

        var result = new List<int>();
        var previous = 0;
        foreach (var page in set)
        {
            if (previous != 0)
            {
                var gap = page - previous;
                if (gap == 2)
                {
                    result.Add(previous + 1); // a single hidden page reads better than an ellipsis
                }
                else if (gap > 2)
                {
                    result.Add(0);
                }
            }

            result.Add(page);
            previous = page;
        }

        return result;
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Pagination);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _items = e.NameScope.Find("PART_Items") as StackPanel;
        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CountProperty || change.Property == SelectedProperty ||
            change.Property == ColorProperty || change.Property == BoundaryCountProperty ||
            change.Property == MiddleCountProperty)
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        if (_items is null)
        {
            return;
        }

        _items.Children.Clear();

        _items.Children.Add(Arrow(Icons.Material.Filled.ArrowBack, Selected > 1, () => Selected--));

        foreach (var page in BuildPages(Count, Selected, BoundaryCount, MiddleCount))
        {
            _items.Children.Add(page == 0 ? Ellipsis() : PageButton(page));
        }

        _items.Children.Add(Arrow(Icons.Material.Filled.ArrowForward, Selected < Count, () => Selected++));
    }

    private static IconButton Arrow(string icon, bool enabled, Action onClick)
    {
        var button = new IconButton { Icon = icon, Size = LoamSize.Small, IsEnabled = enabled };
        button.Click += (_, _) => onClick();
        return button;
    }

    private Button PageButton(int page)
    {
        var selected = page == Selected;
        var button = new Button
        {
            Content = page.ToString(),
            Variant = selected ? Variant.Filled : Variant.Text,
            Color = Color,
            Size = LoamSize.Small,
            MinWidth = 36,
        };
        var captured = page;
        button.Click += (_, _) => Selected = captured;
        return button;
    }

    private static Text Ellipsis() => new()
    {
        Text = "…",
        Color = LoamColor.Default,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(6, 0),
    };
}
