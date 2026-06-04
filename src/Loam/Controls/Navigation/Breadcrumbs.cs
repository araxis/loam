using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Loam.Controls;

/// <summary>One entry in a <see cref="Breadcrumbs"/> trail, mirroring the reference API's <c>BreadcrumbItem</c>.</summary>
public sealed class BreadcrumbItem
{
    /// <summary>Creates an empty entry.</summary>
    public BreadcrumbItem()
    {
    }

    /// <summary>Creates an entry with text and an optional click handler.</summary>
    public BreadcrumbItem(string text, Action? onClick = null)
    {
        Text = text;
        OnClick = onClick;
    }

    /// <summary>The entry's display text.</summary>
    public string? Text { get; set; }

    /// <summary>Invoked when a non-current entry is clicked.</summary>
    public Action? OnClick { get; set; }

    /// <summary>An optional URL launched on click.</summary>
    public string? Href { get; set; }

    /// <summary>Whether the entry is non-interactive.</summary>
    public bool Disabled { get; set; }
}

/// <summary>
/// A breadcrumb navigation trail, mirroring the reference API's <c>Breadcrumbs</c>. Renders <see cref="Items"/>
/// separated by <see cref="Separator"/>; every entry but the last is a clickable <see cref="Link"/>,
/// and the last is shown as the current (muted) page.
/// </summary>
public class Breadcrumbs : TemplatedControl
{
    /// <summary>Identifies the <see cref="Separator"/> property.</summary>
    public static readonly StyledProperty<string> SeparatorProperty =
        AvaloniaProperty.Register<Breadcrumbs, string>(nameof(Separator), "/");

    private StackPanel? _items;

    /// <summary>Creates the trail.</summary>
    public Breadcrumbs() => Items.CollectionChanged += OnItemsChanged;

    /// <summary>The breadcrumb entries, root first.</summary>
    public ObservableCollection<BreadcrumbItem> Items { get; } = new();

    /// <summary>The text drawn between entries. Mirrors the reference API's <c>Separator</c>.</summary>
    public string Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Breadcrumbs);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _items = e.NameScope.Find("PART_Items") as StackPanel;
        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SeparatorProperty)
        {
            Rebuild();
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (_items is null)
        {
            return;
        }

        _items.Children.Clear();
        for (var i = 0; i < Items.Count; i++)
        {
            if (i > 0)
            {
                _items.Children.Add(new Text
                {
                    Text = Separator,
                    Color = LoamColor.Default,
                    Opacity = 0.5,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0),
                });
            }

            var item = Items[i];
            var isCurrent = i == Items.Count - 1;
            if (isCurrent || item.Disabled || (item.OnClick is null && string.IsNullOrEmpty(item.Href)))
            {
                _items.Children.Add(new Text
                {
                    Text = item.Text,
                    Color = LoamColor.Default,
                    Opacity = isCurrent ? 1 : 0.7,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
            else
            {
                _items.Children.Add(new Link
                {
                    Text = item.Text,
                    OnClick = item.OnClick,
                    Href = item.Href,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
        }
    }
}
