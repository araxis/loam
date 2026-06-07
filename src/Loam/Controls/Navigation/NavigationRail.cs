using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A Material 3 navigation rail — a compact vertical strip of top-level destinations for the side of an
/// app shell (3–7 destinations). Hosts <see cref="NavigationRailItem"/>s with single selection and an
/// optional <see cref="Header"/> (e.g. a menu button or FAB). Complements <see cref="NavMenu"/> (the
/// full drawer list) for medium-width layouts.
/// </summary>
public class NavigationRail : Decorator
{
    /// <summary>Identifies the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<NavigationRail, int>(
            nameof(SelectedIndex), defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<NavigationRail, object?>(nameof(Header));

    private readonly StackPanel _itemsHost = new()
    {
        Orientation = Orientation.Vertical,
        Spacing = 12,
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    private readonly ContentControl _headerHost = new()
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new Thickness(0, 0, 0, 12),
        IsVisible = false,
    };

    private readonly Border _surface;
    private IDisposable? _background;

    /// <summary>Creates the navigation rail.</summary>
    public NavigationRail()
    {
        var root = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 12, 0, 12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children = { _headerHost, _itemsHost },
        };

        _surface = new Border { Width = 80, Child = root };
        _background = _surface.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.ColorSurface));
        Child = _surface;

        Items.CollectionChanged += (_, _) => Rebuild();
        AutomationProperties.SetName(this, "Navigation rail");
    }

    /// <summary>Raised when <see cref="SelectedIndex"/> changes (by click or programmatically).</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>The rail destinations.</summary>
    public AvaloniaList<NavigationRailItem> Items { get; } = [];

    /// <summary>Optional content shown above the destinations (e.g. a menu button or FAB).</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>The selected destination index (two-way). <c>-1</c> selects nothing.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The selected destination, or <c>null</c> when none is selected.</summary>
    public NavigationRailItem? SelectedItem =>
        SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null;

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == HeaderProperty)
        {
            _headerHost.Content = Header;
            _headerHost.IsVisible = Header is not null;
        }
        else if (change.Property == SelectedIndexProperty)
        {
            UpdateSelection();
        }
    }

    private void Rebuild()
    {
        _itemsHost.Children.Clear();
        foreach (var item in Items)
        {
            item.Selected -= OnItemSelected;
            item.Selected += OnItemSelected;
            _itemsHost.Children.Add(item);
        }

        UpdateSelection();
    }

    private void OnItemSelected(object? sender, EventArgs e)
    {
        if (sender is NavigationRailItem item)
        {
            var index = Items.IndexOf(item);
            if (index >= 0)
            {
                SelectedIndex = index;
            }
        }
    }

    private void UpdateSelection()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            Items[i].IsActive = i == SelectedIndex;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// A destination in a <see cref="NavigationRail"/>: a centered icon in an active-indicator pill above a
/// label. Selecting it (click or keyboard) raises <see cref="Selected"/> and invokes
/// <see cref="OnClick"/>; the parent rail manages <see cref="IsActive"/>.
/// </summary>
public class NavigationRailItem : Decorator
{
    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<NavigationRailItem, string?>(nameof(Icon));

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<NavigationRailItem, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="IsActive"/> property.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<NavigationRailItem, bool>(nameof(IsActive));

    private readonly Border _pill;
    private readonly Icon _iconPart = new() { Color = LoamColor.Inherit, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    private readonly Text _labelPart = new() { Typo = Typo.Caption, Color = LoamColor.Inherit, HorizontalAlignment = HorizontalAlignment.Center };
    private bool _hover;
    private IDisposable? _pillBackground;
    private IDisposable? _iconForeground;
    private IDisposable? _labelForeground;

    /// <summary>Creates the navigation rail item.</summary>
    public NavigationRailItem()
    {
        Focusable = true;
        Cursor = new Cursor(StandardCursorType.Hand);

        _pill = new Border
        {
            Width = 56,
            Height = 32,
            CornerRadius = new CornerRadius(16),
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = _iconPart,
        };

        Child = new StackPanel
        {
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { _pill, _labelPart },
        };

        GotFocus += (_, _) => ApplyState();
        LostFocus += (_, _) => ApplyState();
        UpdateContent();
        ApplyState();
        UpdateAutomation();
    }

    /// <summary>Raised when the item is activated by click or keyboard.</summary>
    public event EventHandler? Selected;

    /// <summary>Optional callback invoked when the item is activated.</summary>
    public Action? OnClick { get; set; }

    /// <summary>An opaque value carried by the destination (for value-based selection).</summary>
    public object? Value { get; set; }

    /// <summary>Destination icon path data.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Destination label shown under the icon.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Whether this is the selected destination (set by the parent rail).</summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty || change.Property == LabelProperty)
        {
            UpdateContent();
            UpdateAutomation();
        }
        else if (change.Property == IsActiveProperty || change.Property == IsEnabledProperty)
        {
            ApplyState();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _hover = true;
        ApplyState();
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _hover = false;
        ApplyState();
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
        Selected?.Invoke(this, EventArgs.Empty);
        OnClick?.Invoke();
    }

    private void UpdateContent()
    {
        _iconPart.Data = Icon;
        _iconPart.IsVisible = !string.IsNullOrEmpty(Icon);
        _labelPart.Text = Label;
        _labelPart.IsVisible = !string.IsNullOrWhiteSpace(Label);
    }

    private void ApplyState()
    {
        _pillBackground?.Dispose();
        _iconForeground?.Dispose();
        _labelForeground?.Dispose();
        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);

        if (IsActive)
        {
            _pillBackground = _pill.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.ColorSecondaryContainer));
            _iconForeground = _iconPart.Bind(Loam.Controls.Icon.ForegroundProperty, this.GetResourceObservable(LoamTokens.ColorOnSecondaryContainer));
            _labelForeground = _labelPart.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.ColorOnSurface));
            return;
        }

        var focused = IsFocused;
        if (_hover || focused)
        {
            var state = focused
                ? LoamTokens.ColorSchemeStateLayer(nameof(LoamColorScheme.OnSurface), "Focus")
                : LoamTokens.ColorSchemeStateLayer(nameof(LoamColorScheme.OnSurface), "Hover");
            _pillBackground = _pill.Bind(Border.BackgroundProperty, this.GetResourceObservable(state));
        }
        else
        {
            _pill.Background = Brushes.Transparent;
        }

        _iconForeground = _iconPart.Bind(Loam.Controls.Icon.ForegroundProperty, this.GetResourceObservable(LoamTokens.ColorOnSurfaceVariant));
        _labelForeground = _labelPart.Bind(TextBlock.ForegroundProperty, this.GetResourceObservable(LoamTokens.ColorOnSurfaceVariant));
    }

    private void UpdateAutomation() => AutomationProperties.SetName(this, Label ?? string.Empty);
}
