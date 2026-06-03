using Avalonia.Layout;

namespace Loam.Controls;

/// <summary>
/// A column definition for <see cref="DataGrid{T}"/>, mirroring MudBlazor's <c>Column</c>/template column.
/// Projects each row to a cell value via <see cref="Value"/>; sortable by that value unless
/// <see cref="Sortable"/> is cleared.
/// </summary>
/// <typeparam name="T">The row item type.</typeparam>
public sealed class DataGridColumn<T>
{
    /// <summary>Creates a column.</summary>
    /// <param name="header">The column header text.</param>
    /// <param name="value">Projects a row to its cell value.</param>
    public DataGridColumn(string header, Func<T, object?> value)
    {
        Header = header;
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>The header text.</summary>
    public string Header { get; }

    /// <summary>Projects a row to its cell value (used for display and sorting).</summary>
    public Func<T, object?> Value { get; }

    /// <summary>An optional .NET format string applied to the cell value.</summary>
    public string? Format { get; init; }

    /// <summary>Whether clicking the header sorts by this column. Mirrors MudBlazor's <c>Sortable</c>.</summary>
    public bool Sortable { get; init; } = true;

    /// <summary>Horizontal alignment of the cell content. Mirrors MudBlazor's <c>CellStyle</c> alignment.</summary>
    public HorizontalAlignment Align { get; init; } = HorizontalAlignment.Left;

    /// <summary>Formats a row's cell value to display text.</summary>
    public string Display(T item)
    {
        var value = Value(item);
        if (value is null)
        {
            return string.Empty;
        }

        return Format is null ? value.ToString() ?? string.Empty : string.Format($"{{0:{Format}}}", value);
    }
}
