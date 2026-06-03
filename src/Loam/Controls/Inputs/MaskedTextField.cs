using Avalonia;

namespace Loam.Controls;

/// <summary>
/// A <see cref="TextField"/> that formats its input through a <see cref="Mask"/> pattern, mirroring
/// MudBlazor's masked <c>MudTextField</c>/<c>MudMask</c>. Reuses the <see cref="TextField"/> chrome
/// (inherited style key) and reformats <see cref="TextField.Text"/> as the user types.
/// </summary>
public class MaskedTextField : TextField
{
    /// <summary>Identifies the <see cref="Pattern"/> property.</summary>
    public static readonly StyledProperty<string?> PatternProperty =
        AvaloniaProperty.Register<MaskedTextField, string?>(nameof(Pattern));

    private bool _masking;

    /// <summary>The mask pattern (see <see cref="Mask"/>). Mirrors MudBlazor's <c>Mask</c>.</summary>
    public string? Pattern
    {
        get => GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (_masking || string.IsNullOrEmpty(Pattern))
        {
            return;
        }

        if (change.Property == TextProperty || change.Property == PatternProperty)
        {
            var masked = Mask.Apply(Text, Pattern);
            if (masked != Text)
            {
                _masking = true;
                Text = masked;
                _masking = false;
            }
        }
    }
}
