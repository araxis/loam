using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A contextual message banner, mirroring the reference API's <c>Alert</c>. Colored by <see cref="Color"/>
/// (severity) and styled by <see cref="Variant"/> (Filled / Outlined / Text-tint), with an optional
/// leading <see cref="Icon"/> plus generated title, message, action and close regions.
/// </summary>
public class Alert : ContentControl
{
    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Alert, LoamColor>(nameof(Color), LoamColor.Info);

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<Alert, Variant>(nameof(Variant), Loam.Variant.Text);

    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Icon));

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Message"/> property.</summary>
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Message));

    /// <summary>Identifies the <see cref="Action"/> property.</summary>
    public static readonly StyledProperty<object?> ActionProperty =
        AvaloniaProperty.Register<Alert, object?>(nameof(Action));

    /// <summary>Identifies the <see cref="Closeable"/> property.</summary>
    public static readonly StyledProperty<bool> CloseableProperty =
        AvaloniaProperty.Register<Alert, bool>(nameof(Closeable));

    /// <summary>Identifies the <see cref="CloseIcon"/> property.</summary>
    public static readonly StyledProperty<string?> CloseIconProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(CloseIcon));

    private Border? _root;
    private Icon? _iconPart;
    private IconButton? _closePart;
    private IDisposable? _background;
    private IDisposable? _foreground;
    private IDisposable? _border;

    /// <summary>Raised after the close affordance hides the alert.</summary>
    public event EventHandler? Closed;

    /// <summary>Severity color. Mirrors the reference API's <c>Severity</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Visual style. Mirrors the reference API's <c>Variant</c>.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Leading icon path. Mirrors the reference API's <c>Icon</c>.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Generated alert title. Leave unset to use only <see cref="ContentControl.Content"/> or <see cref="Message"/>.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Generated alert body text.</summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Trailing action content, typically a text button.</summary>
    public object? Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    /// <summary>Shows a trailing close affordance that hides the alert and raises <see cref="Closed"/>.</summary>
    public bool Closeable
    {
        get => GetValue(CloseableProperty);
        set => SetValue(CloseableProperty, value);
    }

    /// <summary>Close affordance icon path. Defaults to <see cref="Icons.Material.Filled.Close"/>.</summary>
    public string? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    /// <summary>Hides the alert and raises <see cref="Closed"/>.</summary>
    public void Close()
    {
        if (!Closeable || !IsEnabled)
        {
            return;
        }

        SetCurrentValue(IsVisibleProperty, false);
        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Alert);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_closePart is not null)
        {
            _closePart.Click -= OnCloseClicked;
        }

        _root = e.NameScope.Find("PART_Root") as Border;
        _iconPart = e.NameScope.Find("PART_Icon") as Icon;
        _closePart = e.NameScope.Find("PART_Close") as IconButton;
        if (_closePart is not null)
        {
            _closePart.Click += OnCloseClicked;
        }

        ApplyVisual();
        ApplyContent();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColorProperty || change.Property == VariantProperty || change.Property == IconProperty ||
            change.Property == CloseIconProperty || change.Property == CloseableProperty || change.Property == IsEnabledProperty)
        {
            ApplyVisual();
            ApplyContent();
        }
        else if (change.Property == ContentProperty || change.Property == TitleProperty ||
                 change.Property == MessageProperty || change.Property == ActionProperty)
        {
            ApplyContent();
        }
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Close();
    }

    private void ApplyVisual()
    {
        var tokens = SemanticColor.Resolve(Color);

        if (_iconPart is not null)
        {
            _iconPart.Data = Icon;
            _iconPart.IsVisible = !string.IsNullOrEmpty(Icon);
        }

        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);

        _foreground?.Dispose();
        _border?.Dispose();
        _border = null;
        _background?.Dispose();
        _background = null;

        if (_root is null)
        {
            return;
        }

        switch (Variant)
        {
            case Loam.Variant.Filled:
                _background = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));
                _foreground = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.FillText));
                _root.BorderThickness = default;
                break;
            case Loam.Variant.Outlined:
                _root.Background = Brushes.Transparent;
                _foreground = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.Accent));
                _root.BorderThickness = new Thickness(1);
                _border = _root.Bind(Border.BorderBrushProperty, this.GetResourceObservable(tokens.Border));
                break;
            default: // Text-tint
                _background = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Overlay));
                _foreground = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.Accent));
                _root.BorderThickness = default;
                break;
        }
    }

    private void ApplyContent()
    {
        if (_closePart is not null)
        {
            _closePart.Icon = CloseIcon ?? Icons.Material.Filled.Close;
            _closePart.IsVisible = Closeable;
            _closePart.IsEnabled = Closeable && IsEnabled;
            AutomationProperties.SetName(_closePart, "Close alert");
            AutomationProperties.SetHelpText(_closePart, "Dismiss alert");
        }

        InteractionAssist.SetAutomationName(this, BuildAutomationName());
    }

    private string? BuildAutomationName()
    {
        var parts = new[] { Title, Message, Content?.ToString() }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        return parts.Length == 0 ? null : string.Join(" ", parts);
    }
}
