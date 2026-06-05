using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Loam.Controls.Internal;

namespace Loam.Controls;

/// <summary>
/// A star rating, mirroring the reference API's <c>Rating</c>. Shows <see cref="MaxValue"/> stars filled up
/// to the two-way <see cref="SelectedValue"/> (or the hovered star as a live preview), tinted by
/// <see cref="Color"/>. Set <see cref="ReadOnly"/> to display a fixed score.
/// </summary>
public class Rating : TemplatedControl
{
    /// <summary>Identifies the <see cref="SelectedValue"/> property.</summary>
    public static readonly StyledProperty<int> SelectedValueProperty =
        AvaloniaProperty.Register<Rating, int>(nameof(SelectedValue),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="MaxValue"/> property.</summary>
    public static readonly StyledProperty<int> MaxValueProperty =
        AvaloniaProperty.Register<Rating, int>(nameof(MaxValue), 5);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Rating, LoamColor>(nameof(Color), LoamColor.Warning);

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly StyledProperty<LoamSize> SizeProperty =
        AvaloniaProperty.Register<Rating, LoamSize>(nameof(Size), LoamSize.Medium);

    /// <summary>Identifies the <see cref="ReadOnly"/> property.</summary>
    public static readonly StyledProperty<bool> ReadOnlyProperty =
        AvaloniaProperty.Register<Rating, bool>(nameof(ReadOnly));

    private readonly List<Icon> _stars = new();
    private StackPanel? _panel;
    private int _hover;

    /// <summary>Creates the rating.</summary>
    public Rating()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);
        InteractionAssist.SetAutomationName(this, "Rating");
    }

    /// <summary>The selected score (two-way). Mirrors the reference API's <c>SelectedValue</c>.</summary>
    public int SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>The number of stars. Mirrors the reference API's <c>MaxValue</c>.</summary>
    public int MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    /// <summary>Filled-star color. Mirrors the reference API's <c>Color</c> (defaults to a star-gold Warning).</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Star size. Mirrors the reference API's <c>Size</c>.</summary>
    public LoamSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Whether the rating is display-only. Mirrors the reference API's <c>ReadOnly</c>.</summary>
    public bool ReadOnly
    {
        get => GetValue(ReadOnlyProperty);
        set => SetValue(ReadOnlyProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Rating);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _panel = e.NameScope.Find("PART_Stars") as StackPanel;
        if (_panel is not null)
        {
            _panel.Focusable = true;
            _panel.Cursor = Cursor;
            _panel.PointerExited += (_, _) =>
            {
                _hover = 0;
                UpdateStars();
            };
        }

        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == MaxValueProperty || change.Property == SizeProperty)
        {
            Rebuild();
        }
        else if (change.Property == SelectedValueProperty || change.Property == ColorProperty ||
                 change.Property == ReadOnlyProperty || change.Property == IsEnabledProperty)
        {
            ApplyEnabledState();
            UpdateStars();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled || ReadOnly)
        {
            return;
        }

        if (InteractionAssist.IsIncrementKey(e.Key))
        {
            SelectedValue = ClampValue(SelectedValue + 1);
            e.Handled = true;
        }
        else if (InteractionAssist.IsDecrementKey(e.Key))
        {
            SelectedValue = ClampValue(SelectedValue - 1);
            e.Handled = true;
        }
        else if (e.Key == Key.Home)
        {
            SelectedValue = 0;
            e.Handled = true;
        }
        else if (e.Key == Key.End)
        {
            SelectedValue = Math.Max(0, MaxValue);
            e.Handled = true;
        }
    }

    private void Rebuild()
    {
        if (_panel is null)
        {
            return;
        }

        _panel.Children.Clear();
        _stars.Clear();

        for (var i = 1; i <= MaxValue; i++)
        {
            var star = new Icon
            {
                Data = Icons.Material.Filled.Star,
                Size = Size,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var index = i;
            InteractionAssist.SetAutomationName(star, $"{index} of {MaxValue}");
            star.PointerEntered += (_, _) =>
            {
                if (!ReadOnly)
                {
                    _hover = index;
                    UpdateStars();
                }
            };
            star.PointerPressed += (_, _) =>
            {
                if (!ReadOnly)
                {
                    Focus();
                    SelectedValue = index;
                }
            };

            _panel.Children.Add(star);
            _stars.Add(star);
        }

        UpdateStars();
        ApplyEnabledState();
    }

    private void UpdateStars()
    {
        var effective = _hover > 0 ? _hover : SelectedValue;
        for (var i = 0; i < _stars.Count; i++)
        {
            var filled = i < effective;
            _stars[i].Color = filled ? Color : LoamColor.Default;
            _stars[i].Opacity = filled ? 1 : 0.3;
            _stars[i].Cursor = ReadOnly ? Cursor.Default : new Cursor(StandardCursorType.Hand);
        }
    }

    private int ClampValue(int value) => Math.Clamp(value, 0, Math.Max(0, MaxValue));

    private void ApplyEnabledState() => Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
}
