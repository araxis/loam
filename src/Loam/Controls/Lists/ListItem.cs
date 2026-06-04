using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Loam.Controls;

/// <summary>A row in a <see cref="List"/>, mirroring the reference API's <c>ListItem</c>. Shows an optional leading <see cref="Icon"/> + content, highlights on hover.</summary>
public class ListItem : ContentControl
{
    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<ListItem, string?>(nameof(Icon));

    private Loam.Controls.Icon? _iconPart;

    /// <summary>Leading icon path. Mirrors the reference API's <c>Icon</c>.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ListItem);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _iconPart = e.NameScope.Find("PART_Icon") as Loam.Controls.Icon;
        UpdateIcon();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            UpdateIcon();
        }
    }

    private void UpdateIcon()
    {
        if (_iconPart is not null)
        {
            _iconPart.Data = Icon;
            _iconPart.IsVisible = !string.IsNullOrEmpty(Icon);
        }
    }
}

/// <summary>A vertical container of <see cref="ListItem"/>s, mirroring the reference API's <c>List</c>.</summary>
public class List : StackPanel
{
}
