using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
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

    /// <summary>The total page count for <paramref name="count"/> rows at <paramref name="pageSize"/> (0 = single page).</summary>
    public static int PageCount(int count, int pageSize) =>
        pageSize <= 0 ? 1 : Math.Max(1, (count + pageSize - 1) / pageSize);

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

    private IEnumerable<T>? _items;
    private int _pageSize;
    private int _page = 1;
    private T? _selectedItem;
    private bool _striped = true;
    private bool _hover = true;
    private bool _dense;
    private int _elevation = 1;
    private DataGridColumn<T>? _sortColumn;
    private bool _sortDescending;

    /// <summary>Creates the grid.</summary>
    public DataGrid() => Columns.CollectionChanged += OnColumnsChanged;

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

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (Columns.Count == 0)
        {
            Child = null;
            return;
        }

        var all = _items?.ToList() ?? new List<T>();
        var sorted = DataGrids.Sort(all, _sortColumn, _sortDescending);

        var pageCount = DataGrids.PageCount(sorted.Count, _pageSize);
        var page = Math.Clamp(_page, 1, pageCount);
        var rows = _pageSize <= 0 ? sorted : sorted.Skip((page - 1) * _pageSize).Take(_pageSize).ToList();

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
        var grid = new AvaGrid();
        for (var c = 0; c < Columns.Count; c++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (var c = 0; c < Columns.Count; c++)
        {
            var header = BuildHeaderCell(Columns[c]);
            AvaGrid.SetRow(header, 0);
            AvaGrid.SetColumn(header, c);
            grid.Children.Add(header);
        }

        for (var r = 0; r < rows.Count; r++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var rowIndex = r + 1;
            var item = rows[r];

            AddRowBackground(grid, rowIndex, item, r);

            for (var c = 0; c < Columns.Count; c++)
            {
                var cell = BuildBodyCell(Columns[c], item);
                AvaGrid.SetRow(cell, rowIndex);
                AvaGrid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }
        }

        return grid;
    }

    private Border BuildHeaderCell(DataGridColumn<T> column)
    {
        var pad = _dense ? new Thickness(8, 6) : new Thickness(16, 12);
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

        var cell = new Border { Child = row, Padding = pad, BorderThickness = new Thickness(0, 0, 0, 1) };
        cell.Bind(Border.BorderBrushProperty, this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableLines))));

        if (column.Sortable)
        {
            cell.Cursor = HandCursor;
            cell.PointerPressed += (_, _) => ToggleSort(column);
        }

        return cell;
    }

    private Border BuildBodyCell(DataGridColumn<T> column, T item)
    {
        var pad = _dense ? new Thickness(8, 6) : new Thickness(16, 10);
        var text = new Text { Text = column.Display(item), Typo = Typo.Body2, Color = LoamColor.Inherit, HorizontalAlignment = column.Align, VerticalAlignment = VerticalAlignment.Center };
        return new Border { Child = text, Padding = pad };
    }

    private void AddRowBackground(AvaGrid grid, int rowIndex, T item, int dataIndex)
    {
        var selected = EqualityComparer<T>.Default.Equals(item, _selectedItem);
        var striped = _striped && dataIndex % 2 == 1;

        var background = new Border { Background = Brushes.Transparent, Cursor = HandCursor };
        AvaGrid.SetRow(background, rowIndex);
        AvaGrid.SetColumn(background, 0);
        AvaGrid.SetColumnSpan(background, Columns.Count);
        grid.Children.Add(background);

        IDisposable? baseBinding = null;
        void ApplyBase()
        {
            baseBinding?.Dispose();
            baseBinding = null;
            if (selected)
            {
                baseBinding = background.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.PaletteHover(nameof(LoamPalette.Primary))));
            }
            else if (striped)
            {
                baseBinding = background.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableStriped))));
            }
            else
            {
                background.Background = Brushes.Transparent;
            }
        }

        ApplyBase();

        if (_hover && !selected)
        {
            background.PointerEntered += (_, _) =>
            {
                baseBinding?.Dispose();
                baseBinding = background.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableHover))));
            };
            background.PointerExited += (_, _) => ApplyBase();
        }

        background.PointerPressed += (_, _) =>
        {
            _selectedItem = item;
            SelectionChanged?.Invoke(item);
            Rebuild();
        };
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
