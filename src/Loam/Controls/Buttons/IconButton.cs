using Avalonia;
using Avalonia.Controls.Primitives;

namespace Loam.Controls;

/// <summary>
/// An icon-only button, mirroring the reference API's <c>IconButton</c>. Inherits the button family's
/// <c>Variant</c>/<c>Color</c>/<c>Size</c> and renders a single centered <see cref="Icon"/>.
/// </summary>
public class IconButton : Button
{
    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<IconButton, string?>(nameof(Icon));

    private Loam.Controls.Icon? _iconPart;

    /// <summary>Icon path data. Mirrors the reference API's <c>Icon</c>.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(IconButton);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _iconPart = e.NameScope.Find("PART_Icon") as Loam.Controls.Icon;
        if (_iconPart is not null)
        {
            _iconPart.Data = Icon;
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty && _iconPart is not null)
        {
            _iconPart.Data = Icon;
        }
    }
}
