using Avalonia;
using Avalonia.Controls;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A placeholder shimmer block shown while content loads, mirroring MudBlazor's <c>MudSkeleton</c>.
/// A skeleton-colored box; set <see cref="Circle"/> for a round avatar placeholder.
/// </summary>
public class Skeleton : Border
{
    /// <summary>Identifies the <see cref="Circle"/> property.</summary>
    public static readonly StyledProperty<bool> CircleProperty =
        AvaloniaProperty.Register<Skeleton, bool>(nameof(Circle));

    /// <summary>Creates the skeleton.</summary>
    public Skeleton()
    {
        Height = 16;
        this.Bind(BackgroundProperty, this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.Skeleton))));
        Apply();
    }

    /// <summary>Renders as a circle (for avatar placeholders). Mirrors MudBlazor's circle skeleton type.</summary>
    public bool Circle
    {
        get => GetValue(CircleProperty);
        set => SetValue(CircleProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Skeleton);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CircleProperty || change.Property == HeightProperty)
        {
            Apply();
        }
    }

    private void Apply()
    {
        var height = double.IsNaN(Height) ? 16 : Height;
        CornerRadius = Circle ? new CornerRadius(height / 2) : new CornerRadius(4);
    }
}
