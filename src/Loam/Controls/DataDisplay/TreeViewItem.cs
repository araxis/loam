using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A node in a <see cref="TreeView"/>, mirroring the reference API's <c>TreeViewItem</c>. Shows an optional
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
    private bool _focused;
    private IDisposable? _rowBackground;

    /// <summary>Creates the node.</summary>
    public TreeViewItem()
    {
        Focusable = true;
        GotFocus += (_, _) =>
        {
            _focused = true;
            UpdateRow();
        };
        LostFocus += (_, _) =>
        {
            _focused = false;
            UpdateRow();
        };
        Items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>Raised when the node's row is clicked.</summary>
    public event EventHandler<RoutedEventArgs> ItemSelected
    {
        add => AddHandler(ItemSelectedEvent, value);
        remove => RemoveHandler(ItemSelectedEvent, value);
    }

    /// <summary>The child nodes.</summary>
    public ObservableCollection<TreeViewItem> Items { get; } = new();

    /// <summary>The node label. Mirrors the reference API's <c>Text</c>.</summary>
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

    /// <summary>Whether children are shown. Mirrors the reference API's <c>Expanded</c>.</summary>
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
                Focus();
                Expanded = !Expanded;
                e.Handled = true;
            };
        }

        if (_row is not null)
        {
            _row.Focusable = true;
            _row.PointerPressed += (_, _) =>
            {
                Focus();
                SelectItem();
            };
            _row.KeyDown += (_, e) => HandleKeyDown(e);
            _row.GotFocus += (_, _) =>
            {
                _focused = true;
                UpdateRow();
            };
            _row.LostFocus += (_, _) =>
            {
                _focused = false;
                UpdateRow();
            };
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
        else if (change.Property == IsSelectedProperty || change.Property == IsEnabledProperty)
        {
            UpdateRow();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        HandleKeyDown(e);
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
        this.GetVisualAncestors().OfType<TreeView>().FirstOrDefault()?.ReconcileSelection();
    }

    private void UpdateText()
    {
        if (_text is not null)
        {
            _text.Text = Text;
        }

        InteractionAssist.SetAutomationName(this, Text, "Tree item");
        if (_row is not null)
        {
            InteractionAssist.SetAutomationName(_row, Text, "Tree item");
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

    private void HandleKeyDown(KeyEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (!IsEnabled)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            SelectItem();
            e.Handled = true;
        }
        else if (e.Key == Key.Space)
        {
            if (Items.Count > 0)
            {
                Expanded = !Expanded;
            }
            else
            {
                SelectItem();
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            if (Items.Count > 0 && !Expanded)
            {
                Expanded = true;
            }
            else
            {
                FocusFirstVisibleChild();
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Left)
        {
            if (Items.Count > 0 && Expanded)
            {
                Expanded = false;
            }
            else
            {
                FocusParentItem();
            }

            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            MoveFocus(1);
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            MoveFocus(-1);
            e.Handled = true;
        }
    }

    private void SelectItem() => RaiseEvent(new RoutedEventArgs(ItemSelectedEvent));

    private void UpdateRow()
    {
        if (_row is null)
        {
            return;
        }

        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
        _rowBackground?.Dispose();
        _rowBackground = null;

        if (!IsEnabled)
        {
            _row.Background = Brushes.Transparent;
        }
        else if (IsSelected)
        {
            _rowBackground = _row.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.PaletteSelected(nameof(LoamPalette.Primary))));
        }
        else if (_focused)
        {
            _rowBackground = _row.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.PaletteFocus(nameof(LoamPalette.Primary))));
        }
        else if (_hover)
        {
            _rowBackground = _row.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableHover))));
        }
        else
        {
            _row.Background = Brushes.Transparent;
        }
    }

    private void FocusFirstVisibleChild()
    {
        var child = Items.FirstOrDefault(x => x.IsEnabled);
        child?.Focus();
    }

    private void FocusParentItem()
    {
        var parent = this.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault(x => x.IsEnabled);
        parent?.Focus();
    }

    private void MoveFocus(int delta)
    {
        var tree = this.GetVisualAncestors().OfType<TreeView>().FirstOrDefault();
        if (tree is null)
        {
            return;
        }

        var visible = VisibleItems(tree).Where(x => x.IsEnabled).ToList();
        var current = visible.IndexOf(this);
        if (current < 0)
        {
            return;
        }

        var next = current + delta;
        if (next >= 0 && next < visible.Count)
        {
            visible[next].Focus();
        }
    }

    private static IEnumerable<TreeViewItem> VisibleItems(TreeView tree)
    {
        foreach (var root in tree.Items)
        {
            foreach (var item in VisibleItems(root))
            {
                yield return item;
            }
        }
    }

    private static IEnumerable<TreeViewItem> VisibleItems(TreeViewItem item)
    {
        yield return item;
        if (!item.Expanded)
        {
            yield break;
        }

        foreach (var child in item.Items)
        {
            foreach (var visible in VisibleItems(child))
            {
                yield return visible;
            }
        }
    }
}
