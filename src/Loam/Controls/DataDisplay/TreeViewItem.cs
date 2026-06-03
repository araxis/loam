using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A node in a <see cref="TreeView"/>, mirroring MudBlazor's <c>MudTreeViewItem</c>. Shows an optional
/// <see cref="Icon"/> + <see cref="Text"/>; nodes with <see cref="Items"/> get an expander chevron that
/// toggles <see cref="Expanded"/>. Clicking the row selects it (<see cref="IsSelected"/>).
/// </summary>
public class TreeViewItem : TemplatedControl
{
    /// <summary>Raised (bubbling) when this node's row is clicked.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> ItemSelectedEvent =
        RoutedEvent.Register<TreeViewItem, RoutedEventArgs>(nameof(ItemSelected), RoutingStrategies.Bubble);

    /// <summary>Identifies the <see cref="Text"/> property.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TreeViewItem, string?>(nameof(Text));

    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<TreeViewItem, string?>(nameof(Icon));

    /// <summary>Identifies the <see cref="Expanded"/> property.</summary>
    public static readonly StyledProperty<bool> ExpandedProperty =
        AvaloniaProperty.Register<TreeViewItem, bool>(nameof(Expanded),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="IsSelected"/> property.</summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<TreeViewItem, bool>(nameof(IsSelected));

    private Border? _row;
    private Icon? _chevron;
    private Icon? _iconPart;
    private Text? _text;
    private StackPanel? _children;
    private bool _hover;
    private IDisposable? _rowBackground;

    /// <summary>Creates the node.</summary>
    public TreeViewItem() => Items.CollectionChanged += OnItemsChanged;

    /// <summary>Raised when the node's row is clicked.</summary>
    public event EventHandler<RoutedEventArgs> ItemSelected
    {
        add => AddHandler(ItemSelectedEvent, value);
        remove => RemoveHandler(ItemSelectedEvent, value);
    }

    /// <summary>The child nodes.</summary>
    public ObservableCollection<TreeViewItem> Items { get; } = new();

    /// <summary>The node label. Mirrors MudBlazor's <c>Text</c>.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Leading icon path. Mirrors MudBlazor's <c>Icon</c>.</summary>
    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Whether children are shown. Mirrors MudBlazor's <c>Expanded</c>.</summary>
    public bool Expanded
    {
        get => GetValue(ExpandedProperty);
        set => SetValue(ExpandedProperty, value);
    }

    /// <summary>Whether this node is selected.</summary>
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TreeViewItem);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _row = e.NameScope.Find("PART_Row") as Border;
        _chevron = e.NameScope.Find("PART_Chevron") as Icon;
        _iconPart = e.NameScope.Find("PART_Icon") as Icon;
        _text = e.NameScope.Find("PART_Text") as Text;
        _children = e.NameScope.Find("PART_Children") as StackPanel;

        if (_chevron is not null)
        {
            _chevron.PointerPressed += (_, e) =>
            {
                Expanded = !Expanded;
                e.Handled = true;
            };
        }

        if (_row is not null)
        {
            _row.PointerPressed += (_, _) => RaiseEvent(new RoutedEventArgs(ItemSelectedEvent));
        }

        UpdateText();
        UpdateIcon();
        UpdateChevron();
        RebuildChildren();
        UpdateRow();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ExpandedProperty)
        {
            UpdateChevron();
            if (_children is not null)
            {
                _children.IsVisible = Expanded;
            }
        }
        else if (change.Property == TextProperty)
        {
            UpdateText();
        }
        else if (change.Property == IconProperty)
        {
            UpdateIcon();
        }
        else if (change.Property == IsSelectedProperty)
        {
            UpdateRow();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _hover = true;
        UpdateRow();
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _hover = false;
        UpdateRow();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateChevron();
        RebuildChildren();
    }

    private void UpdateText()
    {
        if (_text is not null)
        {
            _text.Text = Text;
        }
    }

    private void UpdateIcon()
    {
        if (_iconPart is not null)
        {
            _iconPart.Data = Icon;
            _iconPart.IsVisible = !string.IsNullOrEmpty(Icon);
        }
    }

    private void UpdateChevron()
    {
        if (_chevron is not null)
        {
            _chevron.Opacity = Items.Count > 0 ? 1 : 0; // keep layout space so leaf labels stay aligned
            _chevron.RenderTransform = new RotateTransform(Expanded ? 0 : -90);
        }
    }

    private void RebuildChildren()
    {
        if (_children is null)
        {
            return;
        }

        _children.Children.Clear();
        foreach (var child in Items)
        {
            _children.Children.Add(child);
        }

        _children.IsVisible = Expanded;
    }

    private void UpdateRow()
    {
        if (_row is null)
        {
            return;
        }

        _rowBackground?.Dispose();
        _rowBackground = null;

        if (IsSelected)
        {
            _rowBackground = _row.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.PaletteHover(nameof(LoamPalette.Primary))));
        }
        else if (_hover)
        {
            _rowBackground = _row.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.LinesDefault));
        }
        else
        {
            _row.Background = Brushes.Transparent;
        }
    }
}
