using System.Globalization;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Loam.Controls;

/// <summary>Built-in aggregate for a <see cref="DataGridColumn{T}"/> footer cell.</summary>
public enum DataGridSummary
{
    /// <summary>Sum of the column's numeric values.</summary>
    Sum,

    /// <summary>Average of the column's numeric values.</summary>
    Average,

    /// <summary>Smallest numeric value.</summary>
    Min,

    /// <summary>Largest numeric value.</summary>
    Max,

    /// <summary>Row count.</summary>
    Count,
}

/// <summary>
/// A column definition for <see cref="DataGrid{T}"/>, mirroring the reference API's <c>Column</c>/template column.
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

    /// <summary>Whether clicking the header sorts by this column. Mirrors the reference API's <c>Sortable</c>.</summary>
    public bool Sortable { get; init; } = true;

    /// <summary>Horizontal alignment of the cell content. Mirrors the reference API's <c>CellStyle</c> alignment.</summary>
    public HorizontalAlignment Align { get; init; } = HorizontalAlignment.Left;

    /// <summary>
    /// Optional fixed pixel width. When <c>null</c> the column sizes with star (shares the remaining
    /// space). In a frozen-column layout, columns without an explicit width fall back to a default
    /// pixel width so horizontal scrolling works.
    /// </summary>
    public double? Width { get; init; }

    /// <summary>Optional custom cell content factory.</summary>
    public Func<T, Control>? CellTemplate { get; init; }

    /// <summary>Whether the cell renders as an editable text box.</summary>
    public bool Editable { get; init; }

    /// <summary>Receives edited text for editable cells.</summary>
    public Action<T, string?>? SetText { get; init; }

    /// <summary>Custom footer summary text for this column, computed from the current (filtered) rows.</summary>
    public Func<IReadOnlyList<T>, string>? Summary { get; init; }

    /// <summary>Built-in footer aggregate; used when <see cref="Summary"/> is null. Sum/Average/Min/Max use the column's numeric values; Count uses the row count.</summary>
    public DataGridSummary? SummaryKind { get; init; }

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

    /// <summary>The footer text for this column over the given rows, or null when the column has no summary.</summary>
    public string? SummaryText(IReadOnlyList<T> rows)
    {
        if (Summary is not null)
        {
            return Summary(rows);
        }

        if (SummaryKind is not { } kind)
        {
            return null;
        }

        if (kind == DataGridSummary.Count)
        {
            return rows.Count.ToString(CultureInfo.CurrentCulture);
        }

        var numbers = rows.Select(Value).Select(AsNumber).Where(n => n is not null).Select(n => n!.Value).ToList();
        if (numbers.Count == 0)
        {
            return null;
        }

        var result = kind switch
        {
            DataGridSummary.Sum => numbers.Sum(),
            DataGridSummary.Average => numbers.Average(),
            DataGridSummary.Min => numbers.Min(),
            DataGridSummary.Max => numbers.Max(),
            _ => 0d,
        };

        return Format is null ? result.ToString(CultureInfo.CurrentCulture) : string.Format($"{{0:{Format}}}", result);
    }

    private static double? AsNumber(object? value) => value switch
    {
        null => null,
        double d => d,
        float f => f,
        decimal m => (double)m,
        int i => i,
        long l => l,
        short s => s,
        byte b => b,
        _ => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out var parsed) ? parsed : null,
    };
}
