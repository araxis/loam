namespace Loam.Controls;

/// <summary>
/// A named quick-select shortcut shown in the <see cref="DateRangePicker"/> preset rail. <see cref="Resolve"/>
/// computes the <c>(start, end)</c> pair from an anchor date — the picker passes <see cref="DateTime.Today"/>
/// at click time, then auto-orders the pair and clamps it to <see cref="DateRangePicker.MinDate"/>/
/// <see cref="DateRangePicker.MaxDate"/>.
/// </summary>
public sealed class DateRangePreset
{
    /// <summary>Creates a preset with a caption and a resolver that computes its range from an anchor date.</summary>
    /// <param name="label">The button caption shown in the rail.</param>
    /// <param name="resolve">Computes the range for an anchor date (typically today).</param>
    public DateRangePreset(string label, Func<DateTime, (DateTime Start, DateTime End)> resolve)
    {
        Label = label;
        Resolve = resolve;
    }

    /// <summary>The button caption shown in the preset rail.</summary>
    public string Label { get; }

    /// <summary>Computes the range for an anchor date. The picker auto-orders the pair and clamps it to bounds.</summary>
    public Func<DateTime, (DateTime Start, DateTime End)> Resolve { get; }
}
