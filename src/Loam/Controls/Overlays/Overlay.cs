using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Loam.Controls;

/// <summary>
/// A full-area scrim, mirroring the reference API's <c>Overlay</c>. Fills its parent with a translucent
/// backdrop (darker when <see cref="DarkBackground"/>) behind centered content; toggled by
/// <see cref="Visible"/>. When <see cref="AutoClose"/>, clicking the scrim hides it and invokes
/// <see cref="OnClick"/>.
/// </summary>
public class Overlay : ContentControl
{
    /// <summary>Identifies the <see cref="Visible"/> property.</summary>
    public static readonly StyledProperty<bool> VisibleProperty =
        AvaloniaProperty.Register<Overlay, bool>(nameof(Visible),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="DarkBackground"/> property.</summary>
    public static readonly StyledProperty<bool> DarkBackgroundProperty =
        AvaloniaProperty.Register<Overlay, bool>(nameof(DarkBackground));

    /// <summary>Identifies the <see cref="AutoClose"/> property.</summary>
    public static readonly StyledProperty<bool> AutoCloseProperty =
        AvaloniaProperty.Register<Overlay, bool>(nameof(AutoClose));

    private Border? _scrim;

    /// <summary>Creates the overlay (hidden until <see cref="Visible"/>).</summary>
    public Overlay() => IsVisible = Visible;

    /// <summary>Whether the scrim is shown (two-way). Mirrors the reference API's <c>Visible</c>.</summary>
    public bool Visible
    {
        get => GetValue(VisibleProperty);
        set => SetValue(VisibleProperty, value);
    }

    /// <summary>Whether the scrim is darker. Mirrors the reference API's <c>DarkBackground</c>.</summary>
    public bool DarkBackground
    {
        get => GetValue(DarkBackgroundProperty);
        set => SetValue(DarkBackgroundProperty, value);
    }

    /// <summary>Whether clicking the scrim hides it. Mirrors the reference API's <c>AutoClose</c>.</summary>
    public bool AutoClose
    {
        get => GetValue(AutoCloseProperty);
        set => SetValue(AutoCloseProperty, value);
    }

    /// <summary>Invoked when the scrim is clicked. Mirrors the reference API's <c>OnClick</c>.</summary>
    public Action? OnClick { get; set; }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Overlay);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _scrim = e.NameScope.Find("PART_Scrim") as Border;
        if (_scrim is not null)
        {
            _scrim.PointerPressed += OnScrimPressed;
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == VisibleProperty)
        {
            IsVisible = Visible;
        }
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, _scrim))
        {
            return; // ignore clicks bubbling up from the centered content
        }

        OnClick?.Invoke();
        if (AutoClose)
        {
            Visible = false;
        }
    }
}
