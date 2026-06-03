using Avalonia;
using Avalonia.Controls.Primitives;

namespace Loam.Controls;

/// <summary>
/// An icon button that switches between two glyphs, mirroring MudBlazor's <c>MudToggleIconButton</c>.
/// The inherited <see cref="IconButton.Icon"/> is the off glyph; <see cref="ToggledIcon"/> is shown
/// when the two-way <see cref="Toggled"/> state is on. Clicking flips <see cref="Toggled"/>.
/// </summary>
public class ToggleIconButton : IconButton
{
    /// <summary>Identifies the <see cref="Toggled"/> property.</summary>
    public static readonly StyledProperty<bool> ToggledProperty =
        AvaloniaProperty.Register<ToggleIconButton, bool>(nameof(Toggled),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="ToggledIcon"/> property.</summary>
    public static readonly StyledProperty<string?> ToggledIconProperty =
        AvaloniaProperty.Register<ToggleIconButton, string?>(nameof(ToggledIcon));

    private Icon? _icon;

    /// <summary>Whether the button is in the toggled (on) state. Mirrors MudBlazor's <c>Toggled</c>.</summary>
    public bool Toggled
    {
        get => GetValue(ToggledProperty);
        set => SetValue(ToggledProperty, value);
    }

    /// <summary>The glyph shown when <see cref="Toggled"/>. Mirrors MudBlazor's <c>ToggledIcon</c>.</summary>
    public string? ToggledIcon
    {
        get => GetValue(ToggledIconProperty);
        set => SetValue(ToggledIconProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _icon = e.NameScope.Find("PART_Icon") as Icon;
        UpdateDisplay();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ToggledProperty || change.Property == ToggledIconProperty ||
            change.Property == IconProperty)
        {
            UpdateDisplay();
        }
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        Toggled = !Toggled;
        base.OnClick();
    }

    private void UpdateDisplay()
    {
        if (_icon is not null)
        {
            _icon.Data = Toggled ? ToggledIcon ?? Icon : Icon;
        }
    }
}
