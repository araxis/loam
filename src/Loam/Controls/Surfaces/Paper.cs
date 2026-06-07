using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A Material surface, mirroring the reference API's <c>Paper</c>. Hosts content on a token-driven
/// tonal elevation background, optionally squared corners or an outline. Supersedes the
/// Phase-1 <c>Surface</c> smoke control.
/// </summary>
public class Paper : ContentControl
{
    /// <summary>Identifies the <see cref="Elevation"/> property.</summary>
    public static readonly StyledProperty<int> ElevationProperty =
        AvaloniaProperty.Register<Paper, int>(nameof(Elevation), defaultValue: 1);

    /// <summary>Identifies the <see cref="Square"/> property.</summary>
    public static readonly StyledProperty<bool> SquareProperty =
        AvaloniaProperty.Register<Paper, bool>(nameof(Square));

    /// <summary>Identifies the <see cref="Outlined"/> property.</summary>
    public static readonly StyledProperty<bool> OutlinedProperty =
        AvaloniaProperty.Register<Paper, bool>(nameof(Outlined));

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Paper, LoamColor>(nameof(Color), LoamColor.Default);

    /// <summary>Identifies the <see cref="Shape"/> property.</summary>
    public static readonly StyledProperty<SurfaceShape> ShapeProperty =
        AvaloniaProperty.Register<Paper, SurfaceShape>(nameof(Shape), SurfaceShape.Default);

    /// <summary>Identifies the <see cref="Compact"/> property.</summary>
    public static readonly StyledProperty<bool> CompactProperty =
        AvaloniaProperty.Register<Paper, bool>(nameof(Compact));

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Paper, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Subtitle"/> property.</summary>
    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<Paper, string?>(nameof(Subtitle));

    /// <summary>Identifies the <see cref="Body"/> property.</summary>
    public static readonly StyledProperty<object?> BodyProperty =
        AvaloniaProperty.Register<Paper, object?>(nameof(Body));

    private readonly StackPanel _generatedRoot = new() { Orientation = Orientation.Vertical };
    private readonly Text _generatedTitle = new() { Typo = Typo.Subtitle1 };
    private readonly Text _generatedSubtitle = new() { Typo = Typo.Body2, Color = LoamColor.Secondary };
    private readonly ContentControl _generatedBody = new();
    private bool _usingGeneratedContent;
    private bool _updatingContent;
    private bool _hasCustomContent;
    private Border? _root;
    private IDisposable? _backgroundBinding;
    private IDisposable? _cornerBinding;
    private IDisposable? _shadowBinding;
    private IDisposable? _borderBinding;

    /// <summary>Tonal elevation depth (0–25). Mirrors the reference API's <c>Elevation</c>.</summary>
    public int Elevation
    {
        get => GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>Removes corner rounding. Mirrors the reference API's <c>Square</c>.</summary>
    public bool Square
    {
        get => GetValue(SquareProperty);
        set => SetValue(SquareProperty, value);
    }

    /// <summary>Draws a 1px outline and removes the shadow. Mirrors the reference API's <c>Outlined</c>.</summary>
    public bool Outlined
    {
        get => GetValue(OutlinedProperty);
        set => SetValue(OutlinedProperty, value);
    }

    /// <summary>Optional semantic surface color. Default uses tonal surface roles.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Surface shape token. <see cref="Square"/> still forces <see cref="SurfaceShape.None"/>.</summary>
    public SurfaceShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    /// <summary>Uses denser generated content padding.</summary>
    public bool Compact
    {
        get => GetValue(CompactProperty);
        set => SetValue(CompactProperty, value);
    }

    /// <summary>Generated surface title. Leave unset to use custom <see cref="ContentControl.Content"/>.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Generated surface subtitle.</summary>
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Generated surface body content.</summary>
    public object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Paper);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find<Border>("PART_Root");
        ApplyVisual();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ElevationProperty ||
            change.Property == SquareProperty ||
            change.Property == OutlinedProperty ||
            change.Property == ColorProperty ||
            change.Property == ShapeProperty)
        {
            ApplyVisual();
        }

        if (change.Property == ContentProperty && !_updatingContent)
        {
            _usingGeneratedContent = false;
            _hasCustomContent = Content is not null;
            return;
        }

        if (change.Property == TitleProperty ||
            change.Property == SubtitleProperty ||
            change.Property == BodyProperty ||
            change.Property == CompactProperty)
        {
            SyncGeneratedContent();
        }
    }

    private void ApplyVisual()
    {
        if (_root is null)
        {
            return;
        }

        _backgroundBinding?.Dispose();
        _backgroundBinding = Color == LoamColor.Default
            ? _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(InteractionAssist.TonalSurfaceToken(Elevation, Outlined)))
            : _root.Bind(Border.BackgroundProperty, this.GetResourceObservable(SemanticColor.Resolve(Color).Overlay));

        _cornerBinding?.Dispose();
        if (Square)
        {
            _cornerBinding = null;
            _root.CornerRadius = new CornerRadius(0);
        }
        else
        {
            _cornerBinding = _root.Bind(Border.CornerRadiusProperty, this.GetResourceObservable(ShapeToken(Shape)));
        }

        _shadowBinding?.Dispose();
        _shadowBinding = null;
        _root.BoxShadow = default;

        _borderBinding?.Dispose();
        if (Outlined)
        {
            _root.BorderThickness = new Thickness(1);
            _borderBinding = _root.Bind(Border.BorderBrushProperty, this.GetResourceObservable(LoamTokens.ColorOutlineVariant));
        }
        else
        {
            _borderBinding = null;
            _root.BorderThickness = default;
        }
    }

    private void SyncGeneratedContent()
    {
        if (!HasGeneratedContent)
        {
            return;
        }

        if (_hasCustomContent && !_usingGeneratedContent)
        {
            return;
        }

        _generatedRoot.Children.Clear();
        _generatedRoot.Spacing = Compact ? 4 : 8;
        _generatedRoot.Margin = Compact ? new Thickness(12) : new Thickness(16);

        if (!string.IsNullOrWhiteSpace(Title))
        {
            _generatedTitle.Text = Title;
            _generatedRoot.Children.Add(_generatedTitle);
        }

        if (!string.IsNullOrWhiteSpace(Subtitle))
        {
            _generatedSubtitle.Text = Subtitle;
            _generatedRoot.Children.Add(_generatedSubtitle);
        }

        if (Body is not null)
        {
            _generatedBody.Content = Body;
            _generatedRoot.Children.Add(_generatedBody);
        }

        _usingGeneratedContent = true;
        _hasCustomContent = false;
        _updatingContent = true;
        try
        {
            Content = _generatedRoot;
        }
        finally
        {
            _updatingContent = false;
        }

        InteractionAssist.SetAutomationName(this, Title, Subtitle, Body, "Surface");
        AutomationProperties.SetHelpText(this, Compact ? "Compact surface" : "Surface");
    }

    private bool HasGeneratedContent =>
        _usingGeneratedContent ||
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Subtitle) ||
        Body is not null;

    private static string ShapeToken(SurfaceShape shape) => shape switch
    {
        SurfaceShape.None => LoamTokens.ShapeNone,
        SurfaceShape.ExtraSmall => LoamTokens.ShapeExtraSmall,
        SurfaceShape.Small => LoamTokens.ShapeSmall,
        SurfaceShape.Large => LoamTokens.ShapeLarge,
        SurfaceShape.ExtraLarge => LoamTokens.ShapeExtraLarge,
        SurfaceShape.ExtraExtraLarge => LoamTokens.ShapeExtraExtraLarge,
        _ => LoamTokens.ShapeMedium,
    };
}
