using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Loam.Controls.Internal;

namespace Loam.Controls;

/// <summary>
/// A clickable hyperlink, mirroring the reference API's <c>Link</c>. A <see cref="Text"/> tinted by
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
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
        UpdateDecorations(hovered: false);
        UpdateAutomation();
    }

    /// <summary>Invoked when the link is clicked. Mirrors the reference API's <c>OnClick</c>.</summary>
    public Action? OnClick { get; set; }

    /// <summary>Whether the link is always underlined (otherwise only on hover). Mirrors the reference API's <c>Underline</c>.</summary>
    public bool Underline
    {
        get => GetValue(UnderlineProperty);
        set => SetValue(UnderlineProperty, value);
    }

    /// <summary>An optional URL launched on click. Mirrors the reference API's <c>Href</c>.</summary>
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
        else if (change.Property == TextProperty || change.Property == HrefProperty)
        {
            UpdateAutomation();
        }
        else if (change.Property == IsEnabledProperty)
        {
            Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
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

        Focus();
        Activate();
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsEnabled && InteractionAssist.IsActivationKey(e.Key))
        {
            Activate();
            e.Handled = true;
        }
    }

    private void Activate()
    {
        OnClick?.Invoke();
        if (!string.IsNullOrWhiteSpace(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
        {
            TopLevel.GetTopLevel(this)?.Launcher.LaunchUriAsync(uri);
        }
    }

    private void UpdateDecorations(bool hovered) =>
        TextDecorations = Underline || hovered ? Avalonia.Media.TextDecorations.Underline : null;

    private void UpdateAutomation() => InteractionAssist.SetAutomationName(this, Text, Href);
}
