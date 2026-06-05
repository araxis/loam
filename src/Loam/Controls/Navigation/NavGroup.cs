using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A collapsible group of navigation entries, mirroring the reference API's <c>NavGroup</c>. A clickable
/// header (<see cref="Title"/> + optional <see cref="Icon"/> + chevron) reveals the nested
/// <see cref="Items"/> when <see cref="Expanded"/>.
/// </summary>
public class NavGroup : TemplatedControl
{
    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<NavGroup, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<NavGroup, string?>(nameof(Icon));

    /// <summary>Identifies the <see cref="Expanded"/> property.</summary>
    public static readonly StyledProperty<bool> ExpandedProperty =
        AvaloniaProperty.Register<NavGroup, bool>(nameof(Expanded),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private Border? _header;
    private Icon? _iconPart;
    private Text? _titlePart;
    private Icon? _chevron;
    private StackPanel? _items;
    private Collapse? _itemsCollapse;

    /// <summary>Creates the group.</summary>
    public NavGroup()
    {
        Focusable = true;
        Items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>The nested navigation entries.</summary>
    public ObservableCollection<Control> Items { get; } = new();

    /// <summary>The group header text. Mirrors the reference API's <c>Title</c>.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Leading icon path. Mirrors the reference API's <c>Icon</c>.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Whether the group is open. Mirrors the reference API's <c>Expanded</c>.</summary>
    public bool Expanded
    {
        get => GetValue(ExpandedProperty);
        set => SetValue(ExpandedProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NavGroup);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _header = e.NameScope.Find("PART_Header") as Border;
        _iconPart = e.NameScope.Find("PART_Icon") as Icon;
        _titlePart = e.NameScope.Find("PART_Title") as Text;
        _chevron = e.NameScope.Find("PART_Chevron") as Icon;
        _items = e.NameScope.Find("PART_Items") as StackPanel;
        _itemsCollapse = e.NameScope.Find("PART_ItemsCollapse") as Collapse;
        if (_header is not null)
        {
            _header.PointerPressed += (_, _) =>
            {
                Focus();
                Toggle();
            };
            _header.GotFocus += (_, _) => UpdateState();
            _header.LostFocus += (_, _) => UpdateState();
        }

        UpdateHeader();
        RebuildItems();
        UpdateState();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ExpandedProperty)
        {
            UpdateState();
        }
        else if (change.Property == IsEnabledProperty)
        {
            UpdateState();
        }
        else if (change.Property == TitleProperty || change.Property == IconProperty)
        {
            UpdateHeader();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (InteractionAssist.IsActivationKey(e.Key))
        {
            Toggle();
            e.Handled = true;
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildItems();

    private void Toggle() => Expanded = !Expanded;

    private void UpdateHeader()
    {
        if (_titlePart is not null)
        {
            _titlePart.Text = Title;
        }

        if (_iconPart is not null)
        {
            _iconPart.Data = Icon;
            _iconPart.IsVisible = !string.IsNullOrEmpty(Icon);
        }

        InteractionAssist.SetAutomationName(this, Title);
    }

    private void RebuildItems()
    {
        if (_items is null)
        {
            return;
        }

        _items.Children.Clear();
        foreach (var item in Items)
        {
            item.Margin = new Thickness(16, 0, 0, 0);
            _items.Children.Add(item);
        }
    }

    private void UpdateState()
    {
        if (_items is not null)
        {
            _items.IsVisible = Expanded;
        }

        if (_itemsCollapse is not null)
        {
            _itemsCollapse.Expanded = Expanded;
        }

        if (_chevron is not null)
        {
            _chevron.RenderTransform = new RotateTransform(Expanded ? 180 : 0);
        }

        if (_header is not null)
        {
            if (_header.IsFocused || IsFocused)
            {
                _header.Bind(Border.BackgroundProperty,
                    this.GetResourceObservable(LoamTokens.PaletteFocus(nameof(LoamPalette.Primary))));
            }
            else
            {
                _header.Background = Brushes.Transparent;
            }
        }

        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
    }
}
