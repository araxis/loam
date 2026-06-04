using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Theming;
using AvaGrid = Avalonia.Controls.Grid;

namespace Loam.Controls;

/// <summary>One row of cells in a <see cref="SimpleTable"/>. A cell may be a string (rendered as
/// <see cref="Text"/>) or any <see cref="Control"/> (hosted directly).</summary>
public sealed class TableRow
{
    /// <summary>Creates an empty row.</summary>
    public TableRow()
    {
    }

    /// <summary>Creates a row from the given cells.</summary>
    public TableRow(params object?[] cells) => Cells = new List<object?>(cells);

    /// <summary>The row's cells, left to right.</summary>
    public IList<object?> Cells { get; set; } = new List<object?>();
}

/// <summary>
/// A lightweight data table, mirroring the reference API's <c>SimpleTable</c>. Set <see cref="Headers"/>
/// and <see cref="Rows"/>; toggle <see cref="Striped"/>, <see cref="Hover"/>, <see cref="Bordered"/>,
/// and <see cref="Dense"/>. The grid is hosted in an elevated <see cref="Paper"/> surface.
/// </summary>
public class SimpleTable : TemplatedControl
{
    /// <summary>Identifies the <see cref="Striped"/> property.</summary>
    public static readonly StyledProperty<bool> StripedProperty =
        AvaloniaProperty.Register<SimpleTable, bool>(nameof(Striped));

    /// <summary>Identifies the <see cref="Hover"/> property.</summary>
    public static readonly StyledProperty<bool> HoverProperty =
        AvaloniaProperty.Register<SimpleTable, bool>(nameof(Hover));

    /// <summary>Identifies the <see cref="Bordered"/> property.</summary>
    public static readonly StyledProperty<bool> BorderedProperty =
        AvaloniaProperty.Register<SimpleTable, bool>(nameof(Bordered));

    /// <summary>Identifies the <see cref="Dense"/> property.</summary>
    public static readonly StyledProperty<bool> DenseProperty =
        AvaloniaProperty.Register<SimpleTable, bool>(nameof(Dense));

    /// <summary>Identifies the <see cref="Elevation"/> property.</summary>
    public static readonly StyledProperty<int> ElevationProperty =
        AvaloniaProperty.Register<SimpleTable, int>(nameof(Elevation), 1);

    private Paper? _paper;

    /// <summary>Creates the table.</summary>
    public SimpleTable()
    {
        Headers.CollectionChanged += OnDataChanged;
        Rows.CollectionChanged += OnDataChanged;
    }

    /// <summary>The column header labels.</summary>
    public ObservableCollection<string> Headers { get; } = new();

    /// <summary>The data rows.</summary>
    public ObservableCollection<TableRow> Rows { get; } = new();

    /// <summary>Whether alternating rows are shaded. Mirrors the reference API's <c>Striped</c>.</summary>
    public bool Striped
    {
        get => GetValue(StripedProperty);
        set => SetValue(StripedProperty, value);
    }

    /// <summary>Whether rows highlight on pointer-over. Mirrors the reference API's <c>Hover</c>.</summary>
    public bool Hover
    {
        get => GetValue(HoverProperty);
        set => SetValue(HoverProperty, value);
    }

    /// <summary>Whether cells are gridlined. Mirrors the reference API's <c>Bordered</c>.</summary>
    public bool Bordered
    {
        get => GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
    }

    /// <summary>Whether rows use compact padding. Mirrors the reference API's <c>Dense</c>.</summary>
    public bool Dense
    {
        get => GetValue(DenseProperty);
        set => SetValue(DenseProperty, value);
    }

    /// <summary>Surface elevation of the hosting paper. Mirrors the reference API's <c>Elevation</c>.</summary>
    public int Elevation
    {
        get => GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SimpleTable);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _paper = e.NameScope.Find("PART_Paper") as Paper;
        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StripedProperty || change.Property == HoverProperty ||
            change.Property == BorderedProperty || change.Property == DenseProperty ||
            change.Property == ElevationProperty)
        {
            Rebuild();
        }
    }

    private void OnDataChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (_paper is null)
        {
            return;
        }

        _paper.Elevation = Elevation;

        var columns = Math.Max(Headers.Count, Rows.Count == 0 ? 0 : Rows.Max(r => r.Cells.Count));
        if (columns == 0)
        {
            _paper.Content = null;
            return;
        }

        var grid = new AvaGrid();
        for (var c = 0; c < columns; c++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        var rowIndex = 0;
        var hasHeader = Headers.Count > 0;
        if (hasHeader)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (var c = 0; c < columns; c++)
            {
                var cell = BuildCell(c < Headers.Count ? Headers[c] : null, header: true);
                AvaGrid.SetRow(cell, rowIndex);
                AvaGrid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }

            rowIndex++;
        }

        for (var i = 0; i < Rows.Count; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            var striped = Striped && i % 2 == 1;
            var baseBrushKey = striped ? LoamTokens.Palette(nameof(LoamPalette.TableStriped)) : null;
            AddRowBackground(grid, rowIndex, columns, baseBrushKey);

            var row = Rows[i];
            for (var c = 0; c < columns; c++)
            {
                var value = c < row.Cells.Count ? row.Cells[c] : null;
                var cell = BuildCell(value, header: false);
                AvaGrid.SetRow(cell, rowIndex);
                AvaGrid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }

            rowIndex++;
        }

        _paper.Content = grid;
    }

    private void AddRowBackground(AvaGrid grid, int rowIndex, int columns, string? baseBrushKey)
    {
        if (baseBrushKey is null && !Hover)
        {
            return;
        }

        var background = new Border { Background = Brushes.Transparent };
        AvaGrid.SetRow(background, rowIndex);
        AvaGrid.SetColumn(background, 0);
        AvaGrid.SetColumnSpan(background, columns);
        grid.Children.Add(background);

        IDisposable? baseBinding = null;
        if (baseBrushKey is not null)
        {
            baseBinding = background.Bind(Border.BackgroundProperty, this.GetResourceObservable(baseBrushKey));
        }

        if (Hover)
        {
            background.PointerEntered += (_, _) =>
            {
                baseBinding?.Dispose();
                baseBinding = background.Bind(Border.BackgroundProperty,
                    this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableHover))));
            };
            background.PointerExited += (_, _) =>
            {
                baseBinding?.Dispose();
                baseBinding = baseBrushKey is not null
                    ? background.Bind(Border.BackgroundProperty, this.GetResourceObservable(baseBrushKey))
                    : null;
                if (baseBrushKey is null)
                {
                    background.Background = Brushes.Transparent;
                }
            };
        }
    }

    private Border BuildCell(object? value, bool header)
    {
        Control content = value switch
        {
            Control control => control,
            null => new Text(),
            _ => new Text
            {
                Text = value.ToString(),
                Typo = header ? Typo.Subtitle2 : Typo.Body2,
                Color = header ? LoamColor.Default : LoamColor.Inherit,
            },
        };
        content.VerticalAlignment = VerticalAlignment.Center;

        var pad = Dense ? new Thickness(8, 6) : new Thickness(16, 12);
        var cell = new Border { Child = content, Padding = pad };

        var borderThickness = header
            ? new Thickness(Bordered ? 1 : 0, 0, Bordered ? 1 : 0, 1)
            : new Thickness(Bordered ? 1 : 0, 0, Bordered ? 1 : 0, Bordered ? 1 : 0);
        if (borderThickness != default)
        {
            cell.BorderThickness = borderThickness;
            cell.Bind(Border.BorderBrushProperty,
                this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableLines))));
        }

        return cell;
    }
}
