using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;
using AvaGrid = Avalonia.Controls.Grid;

namespace Loam.Controls;

/// <summary>Non-generic sort/paging helpers for <see cref="DataGrid{T}"/> (statics can't live on the generic type).</summary>
public static class DataGrids
{
    /// <summary>Sorts <paramref name="items"/> by <paramref name="column"/> (null = original order).</summary>
    public static IReadOnlyList<T> Sort<T>(IReadOnlyList<T> items, DataGridColumn<T>? column, bool descending)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (column is null)
        {
            return items;
        }

        var sorted = items.OrderBy(i => column.Value(i), CellComparer.Instance).ToList();
        if (descending)
        {
            sorted.Reverse();
        }

        return sorted;
    }

    /// <summary>Filters rows using the supplied predicate and search text.</summary>
    public static IReadOnlyList<T> Filter<T>(IReadOnlyList<T> items, string? text, Func<T, string, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicate);

        if (string.IsNullOrWhiteSpace(text))
        {
            return items;
        }

        return items.Where(item => predicate(item, text)).ToList();
    }

    /// <summary>The total page count for <paramref name="count"/> rows at <paramref name="pageSize"/> (0 = single page).</summary>
    public static int PageCount(int count, int pageSize) =>
        pageSize <= 0 ? 1 : Math.Max(1, (count + pageSize - 1) / pageSize);

    private static readonly object NullGroupKey = new();

    /// <summary>
    /// Groups <paramref name="items"/> by <paramref name="selector"/> in first-appearance order (so
    /// groups follow the current sort). A <c>null</c> key is its own group with a <c>null</c>
    /// <see cref="DataGridGroup{T}.Key"/>.
    /// </summary>
    public static IReadOnlyList<DataGridGroup<T>> Group<T>(IReadOnlyList<T> items, Func<T, object?> selector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selector);

        var order = new List<object>();
        var map = new Dictionary<object, List<T>>();
        foreach (var item in items)
        {
            var key = selector(item) ?? NullGroupKey;
            if (!map.TryGetValue(key, out var list))
            {
                list = new List<T>();
                map[key] = list;
                order.Add(key);
            }

            list.Add(item);
        }

        return order
            .Select(k => new DataGridGroup<T>(ReferenceEquals(k, NullGroupKey) ? null : k, map[k]))
            .ToList();
    }

    internal sealed class CellComparer : IComparer<object?>
    {
        public static readonly CellComparer Instance = new();

        public int Compare(object? x, object? y)
        {
            if (x is null && y is null)
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            if (x is IComparable comparable && x.GetType() == y.GetType())
            {
                return comparable.CompareTo(y);
            }

            return string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}

/// <summary>A group of rows produced by <see cref="DataGrids.Group{T}"/>.</summary>
/// <typeparam name="T">The row item type.</typeparam>
public sealed record DataGridGroup<T>(object? Key, IReadOnlyList<T> Items);

/// <summary>
/// A typed data grid, mirroring the reference API's <c>DataGrid</c>. Renders <see cref="Items"/> across the
/// typed <see cref="Columns"/> with clickable sort headers, optional paging (<see cref="PageSize"/>),
/// row striping/hover, and single-row selection (<see cref="SelectedItem"/>). Built as a self-rendering
/// <see cref="Decorator"/> so the generic type needs no per-closed-type <c>ControlTheme</c> or
/// <c>AvaloniaProperty</c> registrations.
/// </summary>
/// <typeparam name="T">The row item type.</typeparam>
public class DataGrid<T> : Decorator
{
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);
    private static readonly object NullKeyToken = new();

    private IEnumerable<T>? _items;
    private int _pageSize;
    private int _page = 1;
    private T? _selectedItem;
    private bool _striped = true;
    private bool _hover = true;
    private bool _dense;
    private int _elevation = 1;
    private string? _filterText;
    private Func<T, string, bool>? _filter;
    private bool _virtualize;
    private int _maxRenderedRows = 200;
    private DataGridColumn<T>? _sortColumn;
    private bool _sortDescending;
    private Func<T, object?>? _groupBy;
    private readonly HashSet<object> _collapsedGroups = new();
    private bool _collapsibleGroups = true;
    private string _emptyText = "No data";
    private Control? _emptyContent;
    private int _frozenColumns;
    private double _rowHeight;
    private Func<IReadOnlyList<T>, string>? _groupAggregate;

    /// <summary>Creates the grid.</summary>
    public DataGrid()
    {
        Columns.CollectionChanged += OnColumnsChanged;
        InteractionAssist.SetAutomationName(this, "Data grid");
    }

    /// <summary>Raised when <see cref="SelectedItem"/> changes via a row click.</summary>
    public event Action<T?>? SelectionChanged;

    /// <summary>The column definitions.</summary>
    public ObservableCollection<DataGridColumn<T>> Columns { get; } = new();

    /// <summary>The source rows. Mirrors the reference API's <c>Items</c>.</summary>
    public IEnumerable<T>? Items
    {
        get => _items;
        set { _items = value; Rebuild(); }
    }

    /// <summary>Rows per page (0 = no paging). Mirrors the reference API's <c>RowsPerPage</c>.</summary>
    public int PageSize
    {
        get => _pageSize;
        set { _pageSize = value; Rebuild(); }
    }

    /// <summary>The current 1-based page.</summary>
    public int Page
    {
        get => _page;
        set { _page = value; Rebuild(); }
    }

    /// <summary>The selected row, or default. Mirrors the reference API's <c>SelectedItem</c>.</summary>
    public T? SelectedItem
    {
        get => _selectedItem;
        set { _selectedItem = value; Rebuild(); }
    }

    /// <summary>Whether alternating rows are shaded. Mirrors the reference API's <c>Striped</c>.</summary>
    public bool Striped
    {
        get => _striped;
        set { _striped = value; Rebuild(); }
    }

    /// <summary>Whether rows highlight on pointer-over. Mirrors the reference API's <c>Hover</c>.</summary>
    public bool Hover
    {
        get => _hover;
        set { _hover = value; Rebuild(); }
    }

    /// <summary>Whether rows use compact padding. Mirrors the reference API's <c>Dense</c>.</summary>
    public bool Dense
    {
        get => _dense;
        set { _dense = value; Rebuild(); }
    }

    /// <summary>Surface elevation of the host paper. Mirrors the reference API's <c>Elevation</c>.</summary>
    public int Elevation
    {
        get => _elevation;
        set { _elevation = value; Rebuild(); }
    }

    /// <summary>Optional text filter. By default it searches the text of all columns.</summary>
    public string? FilterText
    {
        get => _filterText;
        set { _filterText = value; _page = 1; Rebuild(); }
    }

    /// <summary>Custom row filter used with <see cref="FilterText"/>.</summary>
    public Func<T, string, bool>? Filter
    {
        get => _filter;
        set { _filter = value; _page = 1; Rebuild(); }
    }

    /// <summary>Limits rendered rows for large unpaged data sets.</summary>
    public bool Virtualize
    {
        get => _virtualize;
        set { _virtualize = value; Rebuild(); }
    }

    /// <summary>Maximum rows rendered when <see cref="Virtualize"/> is enabled and paging is off.</summary>
    public int MaxRenderedRows
    {
        get => _maxRenderedRows;
        set { _maxRenderedRows = Math.Max(1, value); Rebuild(); }
    }

    /// <summary>
    /// Optional grouping selector. When set, rows are grouped by key with a group-header row (key +
    /// count) above each group, in first-appearance order (i.e. following the current sort). Grouping
    /// applies within the rendered page.
    /// </summary>
    public Func<T, object?>? GroupBy
    {
        get => _groupBy;
        set { _groupBy = value; _collapsedGroups.Clear(); Rebuild(); }
    }

    /// <summary>
    /// When <see cref="GroupBy"/> is set, whether group headers can be clicked (or activated by
    /// keyboard) to collapse/expand their rows. Collapsed state is keyed by group key and survives
    /// re-renders. Defaults to <c>true</c>.
    /// </summary>
    public bool CollapsibleGroups
    {
        get => _collapsibleGroups;
        set { _collapsibleGroups = value; Rebuild(); }
    }

    /// <summary>Text shown (below the header) when there are no rows to display after filtering. Defaults to "No data".</summary>
    public string EmptyText
    {
        get => _emptyText;
        set { _emptyText = value; Rebuild(); }
    }

    /// <summary>Optional custom empty-state content; overrides <see cref="EmptyText"/> when set.</summary>
    public Control? EmptyContent
    {
        get => _emptyContent;
        set { _emptyContent = value; Rebuild(); }
    }

    /// <summary>
    /// Number of leading columns to freeze (pin) while the remaining columns scroll horizontally;
    /// <c>0</c> disables. Ignored while grouped, or when it is not less than the column count. Frozen
    /// layouts size every column by pixel width (see <see cref="DataGridColumn{T}.Width"/>); columns
    /// without an explicit width get a default so horizontal scrolling works.
    /// </summary>
    public int FrozenColumns
    {
        get => _frozenColumns;
        set { _frozenColumns = Math.Max(0, value); Rebuild(); }
    }

    /// <summary>
    /// Optional fixed body-row height in pixels (<c>0</c> = auto). Useful to guarantee row alignment
    /// across the frozen and scrollable panes when cells use custom templates of differing heights.
    /// </summary>
    public double RowHeight
    {
        get => _rowHeight;
        set { _rowHeight = Math.Max(0, value); Rebuild(); }
    }

    /// <summary>Optional aggregate text appended to each group header, computed from the group's items.</summary>
    public Func<IReadOnlyList<T>, string>? GroupAggregate
    {
        get => _groupAggregate;
        set { _groupAggregate = value; Rebuild(); }
    }

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (Columns.Count == 0)
        {
            Child = null;
            return;
        }

        var all = _items?.ToList() ?? new List<T>();
        var filtered = DataGrids.Filter(all, _filterText, MatchesFilter);
        var sorted = DataGrids.Sort(filtered, _sortColumn, _sortDescending);
        if (_selectedItem is T selected && !sorted.Contains(selected, EqualityComparer<T>.Default))
        {
            _selectedItem = default;
            SelectionChanged?.Invoke(default);
        }

        var pageCount = DataGrids.PageCount(sorted.Count, _pageSize);
        var page = Math.Clamp(_page, 1, pageCount);
        if (page != _page)
        {
            _page = page;
        }

        var rows = _pageSize <= 0
            ? _virtualize ? sorted.Take(_maxRenderedRows).ToList() : sorted
            : sorted.Skip((page - 1) * _pageSize).Take(_pageSize).ToList();

        var grid = BuildGrid(rows);

        Control content = grid;
        if (_pageSize > 0 && pageCount > 1)
        {
            var pagination = new Pagination { Count = pageCount, Selected = page, Margin = new Thickness(8, 4) };
            pagination.GetObservable(Pagination.SelectedProperty).Subscribe(new PageObserver(p =>
            {
                if (p != _page)
                {
                    _page = p;
                    Rebuild();
                }
            }));
            content = new StackPanel { Children = { grid, pagination } };
        }

        Child = new Paper { Elevation = _elevation, Content = content };
    }

    private AvaGrid BuildGrid(IReadOnlyList<T> rows)
    {
        var useFrozen = _frozenColumns > 0 && _groupBy is null && _frozenColumns < Columns.Count;
        return useFrozen ? BuildFrozenGrid(rows) : BuildSingleGrid(rows);
    }

    private static ColumnDefinition StarOrWidth(DataGridColumn<T> column) =>
        new(column.Width is { } w ? new GridLength(w, GridUnitType.Pixel) : GridLength.Star);

    private static ColumnDefinition PixelWidth(DataGridColumn<T> column) =>
        new(new GridLength(column.Width ?? 140d, GridUnitType.Pixel));

    private RowDefinition BodyRowDef() =>
        new(_rowHeight > 0 ? new GridLength(_rowHeight, GridUnitType.Pixel) : GridLength.Auto);

    private AvaGrid BuildSingleGrid(IReadOnlyList<T> rows)
    {
        var grid = new AvaGrid();
        for (var c = 0; c < Columns.Count; c++)
        {
            grid.ColumnDefinitions.Add(StarOrWidth(Columns[c]));
        }

        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (var c = 0; c < Columns.Count; c++)
        {
            var header = BuildHeaderCell(Columns[c]);
            AvaGrid.SetRow(header, 0);
            AvaGrid.SetColumn(header, c);
            grid.Children.Add(header);
        }

        var rowIndex = 1;
        var dataIndex = 0;

        void AddDataRow(T item)
        {
            grid.RowDefinitions.Add(BodyRowDef());
            AddRowBackground(grid, rowIndex, item, dataIndex);
            for (var c = 0; c < Columns.Count; c++)
            {
                var cell = BuildBodyCell(Columns[c], item);
                AvaGrid.SetRow(cell, rowIndex);
                AvaGrid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }

            rowIndex++;
            dataIndex++;
        }

        if (_groupBy is null)
        {
            foreach (var item in rows)
            {
                AddDataRow(item);
            }
        }
        else
        {
            foreach (var group in DataGrids.Group(rows, _groupBy))
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                var collapsed = _collapsibleGroups && _collapsedGroups.Contains(group.Key ?? NullKeyToken);
                var header = BuildGroupHeader(group.Key, group.Items.Count, collapsed, _groupAggregate?.Invoke(group.Items));
                AvaGrid.SetRow(header, rowIndex);
                AvaGrid.SetColumn(header, 0);
                AvaGrid.SetColumnSpan(header, Columns.Count);
                grid.Children.Add(header);
                rowIndex++;

                if (!collapsed)
                {
                    foreach (var item in group.Items)
                    {
                        AddDataRow(item);
                    }
                }
            }
        }

        if (rowIndex == 1)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var empty = BuildEmptyRow();
            AvaGrid.SetRow(empty, rowIndex);
            AvaGrid.SetColumn(empty, 0);
            AvaGrid.SetColumnSpan(empty, Columns.Count);
            grid.Children.Add(empty);
        }

        return grid;
    }

    private AvaGrid BuildFrozenGrid(IReadOnlyList<T> rows)
    {
        var frozen = _frozenColumns;
        var scrollCount = Columns.Count - frozen;

        var leftGrid = new AvaGrid { VerticalAlignment = VerticalAlignment.Top };
        for (var c = 0; c < frozen; c++)
        {
            leftGrid.ColumnDefinitions.Add(PixelWidth(Columns[c]));
        }

        var rightGrid = new AvaGrid { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
        for (var c = frozen; c < Columns.Count; c++)
        {
            rightGrid.ColumnDefinitions.Add(PixelWidth(Columns[c]));
        }

        leftGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        rightGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (var c = 0; c < frozen; c++)
        {
            var header = BuildHeaderCell(Columns[c]);
            AvaGrid.SetRow(header, 0);
            AvaGrid.SetColumn(header, c);
            leftGrid.Children.Add(header);
        }

        for (var c = frozen; c < Columns.Count; c++)
        {
            var header = BuildHeaderCell(Columns[c]);
            AvaGrid.SetRow(header, 0);
            AvaGrid.SetColumn(header, c - frozen);
            rightGrid.Children.Add(header);
        }

        var rowIndex = 1;
        var dataIndex = 0;
        foreach (var item in rows)
        {
            leftGrid.RowDefinitions.Add(BodyRowDef());
            rightGrid.RowDefinitions.Add(BodyRowDef());

            var visual = new RowVisual { Selected = EqualityComparer<T>.Default.Equals(item, _selectedItem) };
            var striped = _striped && dataIndex % 2 == 1;
            AddRowBackgroundTo(leftGrid, rowIndex, 0, frozen, item, dataIndex, visual, striped);
            AddRowBackgroundTo(rightGrid, rowIndex, 0, scrollCount, item, dataIndex, visual, striped);
            ApplyRowBackgrounds(visual, striped);

            for (var c = 0; c < frozen; c++)
            {
                var cell = BuildBodyCell(Columns[c], item);
                AvaGrid.SetRow(cell, rowIndex);
                AvaGrid.SetColumn(cell, c);
                leftGrid.Children.Add(cell);
            }

            for (var c = frozen; c < Columns.Count; c++)
            {
                var cell = BuildBodyCell(Columns[c], item);
                AvaGrid.SetRow(cell, rowIndex);
                AvaGrid.SetColumn(cell, c - frozen);
                rightGrid.Children.Add(cell);
            }

            rowIndex++;
            dataIndex++;
        }

        if (rowIndex == 1)
        {
            rightGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var empty = BuildEmptyRow();
            AvaGrid.SetRow(empty, rowIndex);
            AvaGrid.SetColumn(empty, 0);
            AvaGrid.SetColumnSpan(empty, scrollCount);
            rightGrid.Children.Add(empty);
        }

        var scroller = new ScrollViewer
        {
            Content = rightGrid,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };

        var outer = new AvaGrid();
        outer.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        outer.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        AvaGrid.SetColumn(leftGrid, 0);
        AvaGrid.SetColumn(scroller, 1);
        outer.Children.Add(leftGrid);
        outer.Children.Add(scroller);
        InteractionAssist.SetAutomationName(outer, "Data grid");
        return outer;
    }

    private Border BuildEmptyRow()
    {
        Control content;
        if (_emptyContent is not null)
        {
            if (_emptyContent.Parent is Border previous)
            {
                previous.Child = null;
            }

            content = _emptyContent;
        }
        else
        {
            content = new Text
            {
                Text = _emptyText,
                Typo = Typo.Body2,
                Color = LoamColor.Default,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        var cell = new Border
        {
            Child = content,
            Padding = new Thickness(16, 24),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        InteractionAssist.SetAutomationName(cell, _emptyContent is null ? _emptyText : "No data");
        return cell;
    }

    private Border BuildHeaderCell(DataGridColumn<T> column)
    {
        var pad = InteractionAssist.ThicknessToken(this,
            _dense ? LoamTokens.DensityDataHeaderPaddingDense : LoamTokens.DensityDataHeaderPadding,
            _dense ? new Thickness(8, 6) : new Thickness(16, 12));
        var label = new Text { Text = column.Header, Typo = Typo.Subtitle2, Color = LoamColor.Default, VerticalAlignment = VerticalAlignment.Center };
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, HorizontalAlignment = column.Align, Children = { label } };

        if (column.Sortable && ReferenceEquals(column, _sortColumn))
        {
            row.Children.Add(new Icon
            {
                Data = _sortDescending ? Icons.Material.Filled.ExpandMore : Icons.Material.Filled.ExpandLess,
                Color = LoamColor.Default,
                Size = LoamSize.Small,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        var cell = new Border
        {
            Child = row,
            Padding = pad,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Focusable = column.Sortable,
            Cursor = column.Sortable ? HandCursor : null,
        };
        InteractionAssist.SetAutomationName(cell, column.Sortable
            ? $"Sort by {column.Header}"
            : $"{column.Header} column");
        cell.Bind(Border.BorderBrushProperty, this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableLines))));

        if (column.Sortable)
        {
            var focused = false;
            var hovered = false;
            IDisposable? backgroundBinding = null;
            void ApplyHeaderState()
            {
                backgroundBinding?.Dispose();
                backgroundBinding = null;
                if (focused)
                {
                    backgroundBinding = cell.Bind(Border.BackgroundProperty,
                        this.GetResourceObservable(LoamTokens.PaletteFocus(nameof(LoamPalette.Primary))));
                }
                else if (hovered)
                {
                    backgroundBinding = cell.Bind(Border.BackgroundProperty,
                        this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableHover))));
                }
                else
                {
                    cell.Background = Brushes.Transparent;
                }
            }

            cell.GotFocus += (_, _) =>
            {
                focused = true;
                ApplyHeaderState();
            };
            cell.LostFocus += (_, _) =>
            {
                focused = false;
                ApplyHeaderState();
            };
            cell.PointerEntered += (_, _) =>
            {
                hovered = true;
                ApplyHeaderState();
            };
            cell.PointerExited += (_, _) =>
            {
                hovered = false;
                ApplyHeaderState();
            };
            cell.PointerPressed += (_, _) => ToggleSort(column);
            cell.KeyDown += (_, args) =>
            {
                if (InteractionAssist.IsActivationKey(args.Key))
                {
                    ToggleSort(column);
                    args.Handled = true;
                }
            };
        }

        return cell;
    }

    private Border BuildBodyCell(DataGridColumn<T> column, T item)
    {
        var pad = InteractionAssist.ThicknessToken(this,
            _dense ? LoamTokens.DensityDataCellPaddingDense : LoamTokens.DensityDataCellPadding,
            _dense ? new Thickness(8, 6) : new Thickness(16, 10));
        if (column.CellTemplate is not null)
        {
            return new Border { Child = column.CellTemplate(item), Padding = pad };
        }

        if (column.Editable && column.SetText is not null)
        {
            var editor = new TextBox
            {
                Text = column.Display(item),
                BorderThickness = default,
                Background = Brushes.Transparent,
                Padding = default,
                MinHeight = 0,
                HorizontalAlignment = column.Align,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            editor.TextChanged += (_, _) => column.SetText(item, editor.Text);
            return new Border { Child = editor, Padding = pad };
        }

        var text = new Text { Text = column.Display(item), Typo = Typo.Body2, Color = LoamColor.Inherit, HorizontalAlignment = column.Align, VerticalAlignment = VerticalAlignment.Center };
        return new Border { Child = text, Padding = pad };
    }

    private Border BuildGroupHeader(object? key, int count, bool collapsed, string? aggregate)
    {
        var pad = InteractionAssist.ThicknessToken(this,
            _dense ? LoamTokens.DensityDataHeaderPaddingDense : LoamTokens.DensityDataHeaderPadding,
            _dense ? new Thickness(8, 6) : new Thickness(16, 8));
        var label = new Text
        {
            Text = $"{key} ({count})",
            Typo = Typo.Subtitle2,
            Color = LoamColor.Default,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { label },
        };
        if (_collapsibleGroups)
        {
            row.Children.Insert(0, new Icon
            {
                Data = collapsed ? Icons.Material.Filled.ExpandMore : Icons.Material.Filled.ExpandLess,
                Color = LoamColor.Default,
                Size = LoamSize.Small,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        if (!string.IsNullOrEmpty(aggregate))
        {
            row.Children.Add(new Text
            {
                Text = aggregate,
                Typo = Typo.Body2,
                Color = LoamColor.Default,
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        var cell = new Border
        {
            Child = row,
            Padding = pad,
            BorderThickness = new Thickness(0, 0, 0, 1),
        };
        cell.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.ColorSurfaceContainerHigh));
        cell.Bind(Border.BorderBrushProperty, this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableLines))));

        if (_collapsibleGroups)
        {
            cell.Focusable = true;
            cell.Cursor = HandCursor;
            cell.PointerPressed += (_, _) => ToggleGroup(key);
            cell.KeyDown += (_, args) =>
            {
                if (InteractionAssist.IsActivationKey(args.Key))
                {
                    ToggleGroup(key);
                    args.Handled = true;
                }
            };
            InteractionAssist.SetAutomationName(cell, $"{(collapsed ? "Expand" : "Collapse")} group {key}, {count} items");
        }
        else
        {
            InteractionAssist.SetAutomationName(cell, $"Group {key}, {count} items");
        }

        return cell;
    }

    private sealed class RowVisual
    {
        public bool Selected;
        public bool Focused;
        public bool Hovered;
        public readonly List<Border> Backgrounds = new();
        public readonly List<IDisposable> Bindings = new();
    }

    private void AddRowBackground(AvaGrid grid, int rowIndex, T item, int dataIndex)
    {
        var visual = new RowVisual { Selected = EqualityComparer<T>.Default.Equals(item, _selectedItem) };
        var striped = _striped && dataIndex % 2 == 1;
        AddRowBackgroundTo(grid, rowIndex, 0, Columns.Count, item, dataIndex, visual, striped);
        ApplyRowBackgrounds(visual, striped);
    }

    private void AddRowBackgroundTo(AvaGrid grid, int rowIndex, int columnStart, int columnSpan, T item, int dataIndex, RowVisual visual, bool striped)
    {
        var background = new Border
        {
            Background = Brushes.Transparent,
            Cursor = HandCursor,
            Focusable = IsEnabled,
        };
        InteractionAssist.SetAutomationName(background, $"Row {dataIndex + 1}: {RowLabel(item)}");
        AvaGrid.SetRow(background, rowIndex);
        AvaGrid.SetColumn(background, columnStart);
        AvaGrid.SetColumnSpan(background, columnSpan);
        grid.Children.Add(background);
        visual.Backgrounds.Add(background);

        background.GotFocus += (_, _) =>
        {
            visual.Focused = true;
            ApplyRowBackgrounds(visual, striped);
        };
        background.LostFocus += (_, _) =>
        {
            visual.Focused = false;
            ApplyRowBackgrounds(visual, striped);
        };
        background.KeyDown += (_, args) =>
        {
            if (InteractionAssist.IsActivationKey(args.Key))
            {
                SelectRow(item);
                args.Handled = true;
            }
        };

        if (_hover)
        {
            background.PointerEntered += (_, _) =>
            {
                visual.Hovered = true;
                ApplyRowBackgrounds(visual, striped);
            };
            background.PointerExited += (_, _) =>
            {
                visual.Hovered = false;
                ApplyRowBackgrounds(visual, striped);
            };
        }

        background.PointerPressed += (_, _) => SelectRow(item);
    }

    private void ApplyRowBackgrounds(RowVisual visual, bool striped)
    {
        foreach (var binding in visual.Bindings)
        {
            binding.Dispose();
        }

        visual.Bindings.Clear();

        object? key = null;
        if (visual.Selected)
        {
            key = LoamTokens.PaletteSelected(nameof(LoamPalette.Primary));
        }
        else if (visual.Focused)
        {
            key = LoamTokens.PaletteFocus(nameof(LoamPalette.Primary));
        }
        else if (visual.Hovered && _hover)
        {
            key = LoamTokens.Palette(nameof(LoamPalette.TableHover));
        }
        else if (striped)
        {
            key = LoamTokens.Palette(nameof(LoamPalette.TableStriped));
        }

        foreach (var border in visual.Backgrounds)
        {
            if (key is null)
            {
                border.Background = Brushes.Transparent;
            }
            else
            {
                visual.Bindings.Add(border.Bind(Border.BackgroundProperty, this.GetResourceObservable(key)));
            }
        }
    }

    private void SelectRow(T item)
    {
        if (EqualityComparer<T>.Default.Equals(item, _selectedItem))
        {
            return;
        }

        _selectedItem = item;
        SelectionChanged?.Invoke(item);
        Rebuild();
    }

    private void ToggleSort(DataGridColumn<T> column)
    {
        if (ReferenceEquals(column, _sortColumn))
        {
            _sortDescending = !_sortDescending;
        }
        else
        {
            _sortColumn = column;
            _sortDescending = false;
        }

        Rebuild();
    }

    private void ToggleGroup(object? key)
    {
        var token = key ?? NullKeyToken;
        if (!_collapsedGroups.Remove(token))
        {
            _collapsedGroups.Add(token);
        }

        Rebuild();
    }

    private string RowLabel(T item)
    {
        var label = Columns.Count > 0 ? Columns[0].Display(item) : item?.ToString();
        return string.IsNullOrWhiteSpace(label) ? "item" : label;
    }

    private bool MatchesFilter(T item, string text)
    {
        if (_filter is not null)
        {
            return _filter(item, text);
        }

        return Columns.Any(column => column.Display(item).Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class PageObserver : IObserver<int>
    {
        private readonly Action<int> _onNext;

        public PageObserver(Action<int> onNext) => _onNext = onNext;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(int value) => _onNext(value);
    }
}
