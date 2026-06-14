using Avalonia.Styling;
using Loam.Controls;

namespace Loam.Theming;

/// <summary>
/// Control themes for Loam's picker controls (<see cref="DatePicker"/>, <see cref="TimePicker"/>,
/// <see cref="ColorPicker"/>, <see cref="DateRangePicker"/>). Add it to <c>Application.Styles</c>
/// alongside <see cref="LoamTheme"/> so the pickers are themed:
/// <code>Styles.Add(new LoamTheme()); Styles.Add(new LoamPickers());</code>
/// Tokens (palette, typography, spacing) are resolved from the active <see cref="LoamTheme"/> via
/// dynamic resources, so runtime recoloring/density on <see cref="LoamTheme"/> restyle these too.
/// </summary>
public sealed class LoamPickers : Styles
{
    /// <summary>Registers the picker control themes.</summary>
    public LoamPickers()
    {
        Resources[typeof(Loam.Controls.DatePicker)] = DatePickerTheme.Create();
        Resources[typeof(Loam.Controls.TimePicker)] = TimePickerTheme.Create();
        Resources[typeof(Loam.Controls.ColorPicker)] = ColorPickerTheme.Create();
        Resources[typeof(DateRangePicker)] = DateRangePickerTheme.Create();
    }
}
