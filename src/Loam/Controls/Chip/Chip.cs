using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Loam;

namespace Loam.Controls;

/// <summary>
/// A compact element representing an input, attribute or action, mirroring the reference API's
/// <c>Chip</c>. Shows optional leading <see cref="Icon"/>, a <see cref="Text"/> label, and an
/// optional close button (<see cref="Closeable"/>) that raises <see cref="Closed"/>.
/// </summary>
public class Chip : TemplatedControl
{
    /// <summary>Identifies the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Chip, string?>(nameof(Text));

    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<Chip, string?>(nameof(Icon));

    /// <summary>Identifies the <see cref="CloseIcon"/> property.</summary>
    public static readonly StyledProperty<string?> CloseIconProperty =
        AvaloniaProperty.Register<Chip, string?>(nameof(CloseIcon));

    /// <summary>Identifies the <see cref="Closeable"/> property.</summary>
    public static readonly StyledProperty<bool> CloseableProperty =
        AvaloniaProperty.Register<Chip, bool>(nameof(Closeable));

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<bool> LabelProperty =
        AvaloniaProperty.Register<Chip, bool>(nameof(Label));

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<Chip, Variant>(nameof(Variant), Loam.Variant.Filled);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Chip, LoamColor>(nameof(Color), LoamColor.Default);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<Chip, LoamSize>(nameof(Size), LoamSize.Medium);

    private Border? _root;
    private Loam.Controls.Icon? _iconPart;
    private Loam.Controls.Text? _textPart;
    private Loam.Controls.Icon? _closePart;
    private IDisposable? _backgroundBinding;
    private IDisposable? _foregroundBinding;
    private IDisposable? _borderBinding;

    /// <summary>Raised when the close button is clicked.</summary>
    public event EventHandler? Closed;

    /// <summary>The chip label. Mirrors the reference API's <c>Text</c>.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Leading icon path. Mirrors the reference API's <c>Icon</c>.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Close-button icon path. Mirrors the reference API's <c>CloseIcon</c>.</summary>
    public string? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    /// <summary>Shows the close button. (Material Design shows it when <c>OnClose</c> is set.)</summary>
    public bool Closeable
    {
        get => GetValue(CloseableProperty);
        set => SetValue(CloseableProperty, value);
    }

    /// <summary>Rounded-rectangle (label) shape instead of a pill. Mirrors the reference API's <c>Label</c>.</summary>
    public bool Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

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

    /// <summary>Chip size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Chip);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find("PART_Root") as Border;
        _iconPart = e.NameScope.Find("PART_Icon") as Loam.Controls.Icon;
        _textPart = e.NameScope.Find("PART_Text") as Loam.Controls.Text;
        _closePart = e.NameScope.Find("PART_Close") as Loam.Controls.Icon;
        if (_closePart is not null)
        {
            _closePart.PointerPressed += OnClosePressed;
        }

        ApplyVisual();
        ApplyContent();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == VariantProperty || change.Property == ColorProperty ||
            change.Property == SizeProperty || change.Property == LabelProperty)
        {
            ApplyVisual();
        }
        else if (change.Property == TextProperty || change.Property == IconProperty ||
                 change.Property == CloseIconProperty || change.Property == CloseableProperty)
        {
            ApplyContent();
        }
    }

    private void OnClosePressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyVisual()
    {
        var height = Size switch { LoamSize.Small => 24d, LoamSize.Large => 40d, _ => 32d };
        Height = height;

        if (_root is null)
        {
            return;
        }

        _root.Padding = new Thickness(10, 0);
        _root.CornerRadius = Label ? new CornerRadius(4) : new CornerRadius(height / 2);

        var tokens = SemanticColor.Resolve(Color);
        _backgroundBinding?.Dispose();
        _foregroundBinding?.Dispose();
        _borderBinding?.Dispose();
        _borderBinding = null;

        if (Variant == Loam.Variant.Filled)
        {
            _backgroundBinding = _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(tokens.Fill));
            _foregroundBinding = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.FillText));
            _root.BorderThickness = default;
        }
        else
        {
            _root.Background = Brushes.Transparent;
            _backgroundBinding = null;
            _foregroundBinding = this.Bind(ForegroundProperty, this.GetResourceObservable(tokens.Accent));
            if (Variant == Loam.Variant.Outlined)
            {
                _root.BorderThickness = new Thickness(1);
                _borderBinding = _root.Bind(Border.BorderBrushProperty, this.GetResourceObservable(tokens.Border));
            }
            else
            {
                _root.BorderThickness = default;
            }
        }
    }

    private void ApplyContent()
    {
        if (_textPart is not null)
        {
            _textPart.Text = Text;
        }

        if (_iconPart is not null)
        {
            _iconPart.Data = Icon;
            _iconPart.IsVisible = !string.IsNullOrEmpty(Icon);
        }

        if (_closePart is not null)
        {
            _closePart.Data = CloseIcon ?? Icons.Material.Filled.Close;
            _closePart.IsVisible = Closeable;
        }
    }
}
