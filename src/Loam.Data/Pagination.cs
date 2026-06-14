using Avalonia;
using Avalonia.Automation;
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

    /// <summary>Identifies the <see cref="ShowFirstLast"/> property.</summary>
    public static readonly StyledProperty<bool> ShowFirstLastProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(ShowFirstLast));

    /// <summary>Identifies the <see cref="ShowRange"/> property.</summary>
    public static readonly StyledProperty<bool> ShowRangeProperty =
        AvaloniaProperty.Register<Pagination, bool>(nameof(ShowRange));

    /// <summary>Identifies the <see cref="TotalItems"/> property.</summary>
    public static readonly StyledProperty<int> TotalItemsProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(TotalItems));

    /// <summary>Identifies the <see cref="PageSize"/> property.</summary>
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(PageSize));

    private StackPanel? _items;
    private bool _coercing;

    /// <summary>Creates the pagination control.</summary>
    public Pagination()
    {
        AutomationProperties.SetName(this, "Pagination");
    }

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

    /// <summary>When true, adds first-page and last-page boundary buttons flanking the arrows.</summary>
    public bool ShowFirstLast
    {
        get => GetValue(ShowFirstLastProperty);
        set => SetValue(ShowFirstLastProperty, value);
    }

    /// <summary>When true (and <see cref="PageSize"/>/<see cref="TotalItems"/> are set), shows a "Showing X–Y of N" summary.</summary>
    public bool ShowRange
    {
        get => GetValue(ShowRangeProperty);
        set => SetValue(ShowRangeProperty, value);
    }

    /// <summary>Total item count across all pages, for the range summary.</summary>
    public int TotalItems
    {
        get => GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    /// <summary>Rows per page, for the range summary.</summary>
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
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
            change.Property == MiddleCountProperty || change.Property == IsEnabledProperty ||
            change.Property == ShowFirstLastProperty || change.Property == ShowRangeProperty ||
            change.Property == TotalItemsProperty || change.Property == PageSizeProperty)
        {
            if (change.Property == CountProperty || change.Property == SelectedProperty)
            {
                CoerceSelected();
            }

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

        var rangeText = ShowRange && PageSize > 0 && TotalItems > 0 ? RangeLabel() : null;
        AutomationProperties.SetHelpText(this, Count <= 0
            ? "No pages"
            : rangeText is not null ? $"Page {Selected} of {Count}, {rangeText}" : $"Page {Selected} of {Count}");

        if (ShowFirstLast)
        {
            _items.Children.Add(Arrow(Icons.Material.Filled.FirstPage, IsEnabled && Selected > 1, () => Selected = 1, "First page"));
        }

        _items.Children.Add(Arrow(Icons.Material.Filled.ArrowBack, IsEnabled && Selected > 1, () => Selected--, "Previous page"));

        foreach (var page in BuildPages(Count, Selected, BoundaryCount, MiddleCount))
        {
            _items.Children.Add(page == 0 ? Ellipsis() : PageButton(page));
        }

        _items.Children.Add(Arrow(Icons.Material.Filled.ArrowForward, IsEnabled && Selected < Count, () => Selected++, "Next page"));

        if (ShowFirstLast)
        {
            _items.Children.Add(Arrow(Icons.Material.Filled.LastPage, IsEnabled && Selected < Count, () => Selected = Count, "Last page"));
        }

        if (rangeText is not null)
        {
            _items.Children.Add(new Text
            {
                Text = rangeText,
                Color = LoamColor.Secondary,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
            });
        }
    }

    private string RangeLabel()
    {
        var start = ((Selected - 1) * PageSize) + 1;
        var end = Math.Min(Selected * PageSize, TotalItems);
        return $"Showing {start}–{end} of {TotalItems}";
    }

    private static IconButton Arrow(string icon, bool enabled, Action onClick, string automationName)
    {
        var button = new IconButton { Icon = icon, Size = LoamSize.Small, IsEnabled = enabled };
        AutomationProperties.SetName(button, automationName);
        button.Click += (_, _) =>
        {
            if (button.IsEnabled)
            {
                onClick();
            }
        };
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
            IsEnabled = IsEnabled,
        };
        AutomationProperties.SetName(button, selected ? $"Page {page}, selected" : $"Page {page}");
        var captured = page;
        button.Click += (_, _) =>
        {
            if (button.IsEnabled)
            {
                Selected = captured;
            }
        };
        return button;
    }

    private static Text Ellipsis() => new()
    {
        Text = "…",
        Color = LoamColor.Default,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(6, 0),
    };

    private void CoerceSelected()
    {
        if (_coercing)
        {
            return;
        }

        var value = Count <= 0 ? 0 : Math.Clamp(Selected, 1, Count);
        if (value == Selected)
        {
            return;
        }

        _coercing = true;
        try
        {
            Selected = value;
        }
        finally
        {
            _coercing = false;
        }
    }
}
