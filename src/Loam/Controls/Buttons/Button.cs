using Avalonia;
using Avalonia.Controls.Primitives;
using Loam;

namespace Loam.Controls;

/// <summary>
/// A Material button, mirroring the reference API's <c>Button</c>. Subclasses Avalonia's
/// <see cref="Avalonia.Controls.Button"/> (for click/command/keyboard/pseudo-class states) and adds
/// the reference-style <see cref="Variant"/>, <see cref="Color"/>, <see cref="Size"/> and
/// <see cref="FullWidth"/> parameters. All visuals come from theme tokens via its control theme.
/// </summary>
public class Button : global::Avalonia.Controls.Button
{
    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<Button, Variant>(nameof(Variant), Variant.Text);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Button, LoamColor>(nameof(Color), LoamColor.Default);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<Button, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="FullWidth"/> property.</summary>
    public static readonly StyledProperty<bool> FullWidthProperty =
        AvaloniaProperty.Register<Button, bool>(nameof(FullWidth));

    /// <summary>Visual style. Mirrors the reference API's <c>Variant</c>.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Semantic color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Button size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Stretches to fill the available width. Mirrors the reference API's <c>FullWidth</c>.</summary>
    public bool FullWidth
    {
        get => GetValue(FullWidthProperty);
        set => SetValue(FullWidthProperty, value);
    }

    /// <summary>Identifies the <see cref="StartIcon"/> property.</summary>
    public static readonly StyledProperty<string?> StartIconProperty =
        AvaloniaProperty.Register<Button, string?>(nameof(StartIcon));

    /// <summary>Identifies the <see cref="EndIcon"/> property.</summary>
    public static readonly StyledProperty<string?> EndIconProperty =
        AvaloniaProperty.Register<Button, string?>(nameof(EndIcon));

    private Icon? _startIcon;
    private Icon? _endIcon;

    /// <summary>Leading icon path data. Mirrors the reference API's <c>StartIcon</c>.</summary>
    public string? StartIcon
    {
        get => GetValue(StartIconProperty);
        set => SetValue(StartIconProperty, value);
    }

    /// <summary>Trailing icon path data. Mirrors the reference API's <c>EndIcon</c>.</summary>
    public string? EndIcon
    {
        get => GetValue(EndIconProperty);
        set => SetValue(EndIconProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Button);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _startIcon = e.NameScope.Find("PART_StartIcon") as Icon;
        _endIcon = e.NameScope.Find("PART_EndIcon") as Icon;
        UpdateIcons();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StartIconProperty || change.Property == EndIconProperty)
        {
            UpdateIcons();
        }
    }

    private void UpdateIcons()
    {
        if (_startIcon is not null)
        {
            _startIcon.Data = StartIcon;
            _startIcon.IsVisible = !string.IsNullOrEmpty(StartIcon);
        }

        if (_endIcon is not null)
        {
            _endIcon.Data = EndIcon;
            _endIcon.IsVisible = !string.IsNullOrEmpty(EndIcon);
        }
    }
}
