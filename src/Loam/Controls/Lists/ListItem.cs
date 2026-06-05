using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>A row in a <see cref="List"/>, mirroring the reference API's <c>ListItem</c>. Shows an optional leading <see cref="Icon"/> + content, highlights on hover.</summary>
public class ListItem : ContentControl
{
    /// <summary>Raised when the row is activated by pointer or keyboard.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ActivatedEvent =
        RoutedEvent.Register<ListItem, RoutedEventArgs>(nameof(Activated), RoutingStrategies.Bubble);

    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<ListItem, string?>(nameof(Icon));

    /// <summary>Identifies the <see cref="IsSelected"/> property.</summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<ListItem, bool>(nameof(IsSelected));

    private Loam.Controls.Icon? _iconPart;
    private Border? _root;
    private bool _focused;
    private bool _hovered;
    private IDisposable? _rootBackground;

    /// <summary>Creates the row.</summary>
    public ListItem()
    {
        Focusable = true;
        GotFocus += (_, _) =>
        {
            _focused = true;
            UpdateState();
        };
        LostFocus += (_, _) =>
        {
            _focused = false;
            UpdateState();
        };
        InteractionAssist.SetAutomationName(this, "List item");
    }

    /// <summary>Raised when the row is activated by pointer or keyboard.</summary>
    public event EventHandler<RoutedEventArgs> Activated
    {
        add => AddHandler(ActivatedEvent, value);
        remove => RemoveHandler(ActivatedEvent, value);
    }

    /// <summary>Leading icon path. Mirrors the reference API's <c>Icon</c>.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Whether this row is currently selected.</summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ListItem);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _root = e.NameScope.Find("PART_Root") as Border;
        _iconPart = e.NameScope.Find("PART_Icon") as Loam.Controls.Icon;

        if (_root is not null)
        {
            _root.Focusable = true;
            _root.PointerPressed += (_, args) =>
            {
                Focus();
                Activate();
                args.Handled = true;
            };
            _root.PointerEntered += (_, _) =>
            {
                _hovered = true;
                UpdateState();
            };
            _root.PointerExited += (_, _) =>
            {
                _hovered = false;
                UpdateState();
            };
            _root.GotFocus += (_, _) =>
            {
                _focused = true;
                UpdateState();
            };
            _root.LostFocus += (_, _) =>
            {
                _focused = false;
                UpdateState();
            };
            _root.KeyDown += (_, args) => HandleKeyDown(args);
        }

        UpdateIcon();
        UpdateAutomationName();
        UpdateState();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty)
        {
            UpdateIcon();
        }
        else if (change.Property == IsSelectedProperty || change.Property == IsEnabledProperty)
        {
            UpdateState();
        }
        else if (change.Property == ContentProperty)
        {
            UpdateAutomationName();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        HandleKeyDown(e);
    }

    private void UpdateIcon()
    {
        if (_iconPart is not null)
        {
            _iconPart.Data = Icon;
            _iconPart.IsVisible = !string.IsNullOrEmpty(Icon);
        }
    }

    private void HandleKeyDown(KeyEventArgs e)
    {
        if (!IsEnabled || e.Handled || !InteractionAssist.IsActivationKey(e.Key))
        {
            return;
        }

        Activate();
        e.Handled = true;
    }

    private void Activate()
    {
        if (!IsEnabled)
        {
            return;
        }

        RaiseEvent(new RoutedEventArgs(ActivatedEvent));
    }

    private void UpdateState()
    {
        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);

        if (_root is null)
        {
            return;
        }

        _rootBackground?.Dispose();
        _rootBackground = null;

        if (!IsEnabled)
        {
            _root.Background = Brushes.Transparent;
        }
        else if (IsSelected)
        {
            _rootBackground = _root.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.PaletteSelected(nameof(LoamPalette.Primary))));
        }
        else if (_focused)
        {
            _rootBackground = _root.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.PaletteFocus(nameof(LoamPalette.Primary))));
        }
        else if (_hovered)
        {
            _rootBackground = _root.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableHover))));
        }
        else
        {
            _root.Background = Brushes.Transparent;
        }
    }

    private void UpdateAutomationName() => InteractionAssist.SetAutomationName(this, Content, "List item");
}

/// <summary>A vertical container of <see cref="ListItem"/>s, mirroring the reference API's <c>List</c>.</summary>
public class List : StackPanel
{
}
