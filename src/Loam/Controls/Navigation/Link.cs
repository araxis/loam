using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Loam.Controls;

/// <summary>
/// A clickable hyperlink, mirroring MudBlazor's <c>MudLink</c>. A <see cref="Text"/> tinted by
/// <see cref="Text.Color"/> (default <see cref="LoamColor.Primary"/>) that underlines on pointer-over
/// (always when <see cref="Underline"/> is set). Clicking invokes <see cref="OnClick"/> and, if set,
/// launches <see cref="Href"/> in the default browser.
/// </summary>
public class Link : Text
{
    /// <summary>Identifies the <see cref="Underline"/> property.</summary>
    public static readonly StyledProperty<bool> UnderlineProperty =
        AvaloniaProperty.Register<Link, bool>(nameof(Underline));

    /// <summary>Identifies the <see cref="Href"/> property.</summary>
    public static readonly StyledProperty<string?> HrefProperty =
        AvaloniaProperty.Register<Link, string?>(nameof(Href));

    /// <summary>Creates the link.</summary>
    public Link()
    {
        Color = LoamColor.Primary;
        Cursor = new Cursor(StandardCursorType.Hand);
        UpdateDecorations(hovered: false);
    }

    /// <summary>Invoked when the link is clicked. Mirrors MudBlazor's <c>OnClick</c>.</summary>
    public Action? OnClick { get; set; }

    /// <summary>Whether the link is always underlined (otherwise only on hover). Mirrors MudBlazor's <c>Underline</c>.</summary>
    public bool Underline
    {
        get => GetValue(UnderlineProperty);
        set => SetValue(UnderlineProperty, value);
    }

    /// <summary>An optional URL launched on click. Mirrors MudBlazor's <c>Href</c>.</summary>
    public string? Href
    {
        get => GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == UnderlineProperty)
        {
            UpdateDecorations(hovered: IsPointerOver);
        }
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        UpdateDecorations(hovered: true);
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        UpdateDecorations(hovered: false);
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEnabled)
        {
            return;
        }

        OnClick?.Invoke();
        if (!string.IsNullOrWhiteSpace(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
        {
            TopLevel.GetTopLevel(this)?.Launcher.LaunchUriAsync(uri);
        }
    }

    private void UpdateDecorations(bool hovered) =>
        TextDecorations = Underline || hovered ? Avalonia.Media.TextDecorations.Underline : null;
}
