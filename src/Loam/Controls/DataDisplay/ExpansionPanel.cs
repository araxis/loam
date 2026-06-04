using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Loam.Controls;

/// <summary>
/// A single collapsible panel, mirroring the reference API's <c>ExpansionPanel</c>. Shows its
/// <see cref="HeaderedContentControl.Header"/> with a chevron; clicking the header toggles
/// <see cref="IsExpanded"/>, which reveals the <see cref="ContentControl.Content"/>.
/// </summary>
public class ExpansionPanel : HeaderedContentControl
{
    /// <summary>Identifies the <see cref="IsExpanded"/> property.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<ExpansionPanel, bool>(nameof(IsExpanded),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private Border? _header;
    private Icon? _chevron;
    private Control? _content;

    /// <summary>Whether the panel is open. Mirrors the reference API's <c>IsExpanded</c>.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ExpansionPanel);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _header = e.NameScope.Find("PART_Header") as Border;
        _chevron = e.NameScope.Find("PART_Chevron") as Icon;
        _content = e.NameScope.Find("PART_Content") as Control;
        if (_header is not null)
        {
            _header.PointerPressed += (_, _) => IsExpanded = !IsExpanded;
        }

        UpdateState();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsExpandedProperty)
        {
            UpdateState();
        }
    }

    private void UpdateState()
    {
        if (_content is not null)
        {
            _content.IsVisible = IsExpanded;
        }

        if (_chevron is not null)
        {
            _chevron.RenderTransform = new RotateTransform(IsExpanded ? 180 : 0);
        }
    }
}
