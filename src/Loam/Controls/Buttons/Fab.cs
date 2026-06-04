using Avalonia;
using Avalonia.Controls.Primitives;
using Loam.Theming;
using AC = Avalonia.Controls;

namespace Loam.Controls;

/// <summary>
/// A floating action button, mirroring the reference API's <c>Fab</c>. A pill-shaped, elevated, filled
/// button with an optional <see cref="Label"/> and the inherited <c>StartIcon</c>/<c>EndIcon</c>.
/// </summary>
public class Fab : Button
{
    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<Fab, string?>(nameof(Label));

    private AC.Border? _root;

    /// <summary>The text label. Mirrors the reference API's <c>Label</c>.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Fab);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find("PART_Root") as AC.Border;
        _root?.Bind(
            AC.Border.BoxShadowProperty,
            AC.ResourceNodeExtensions.GetResourceObservable(this, LoamTokens.Elevation(6)));
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LabelProperty)
        {
            Content = Label;
        }
    }
}
