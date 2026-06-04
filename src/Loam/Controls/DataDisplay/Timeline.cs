using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Theming;
using AvaGrid = Avalonia.Controls.Grid;

namespace Loam.Controls;

/// <summary>One entry in a <see cref="Timeline"/>, mirroring the reference API's <c>TimelineItem</c>.</summary>
public sealed class TimelineItem
{
    /// <summary>Creates an empty entry.</summary>
    public TimelineItem()
    {
    }

    /// <summary>Creates an entry with content and an optional dot color.</summary>
    public TimelineItem(object? content, LoamColor color = LoamColor.Primary)
    {
        Content = content;
        Color = color;
    }

    /// <summary>The entry's content (string or any <see cref="Control"/>).</summary>
    public object? Content { get; set; }

    /// <summary>The dot color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color { get; set; } = LoamColor.Primary;
}

/// <summary>
/// A vertical timeline, mirroring the reference API's <c>Timeline</c>. Renders <see cref="Items"/> down a
/// connector line, each with a colored dot beside its content card.
/// </summary>
public class Timeline : Decorator
{
    private const double MarkerColumn = 28;
    private const double DotSize = 14;

    /// <summary>Creates the timeline.</summary>
    public Timeline() => Items.CollectionChanged += OnItemsChanged;

    /// <summary>The timeline entries, top to bottom.</summary>
    public ObservableCollection<TimelineItem> Items { get; } = new();

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (Items.Count == 0)
        {
            Child = null;
            return;
        }

        var grid = new AvaGrid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(MarkerColumn)));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (var i = 0; i < Items.Count; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        // Continuous connector line behind the dots.
        var line = new Border { Width = 2, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Stretch };
        line.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Divider));
        AvaGrid.SetColumn(line, 0);
        AvaGrid.SetRow(line, 0);
        AvaGrid.SetRowSpan(line, Items.Count);
        grid.Children.Add(line);

        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];

            var dot = new Border
            {
                Width = DotSize,
                Height = DotSize,
                CornerRadius = new CornerRadius(DotSize / 2),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 6, 0, 0),
            };
            dot.Bind(Border.BackgroundProperty, this.GetResourceObservable(SemanticColor.Resolve(item.Color).Fill));
            AvaGrid.SetColumn(dot, 0);
            AvaGrid.SetRow(dot, i);
            grid.Children.Add(dot);

            var presenter = new ContentPresenter { Content = item.Content };
            var card = new Paper
            {
                Elevation = 1,
                Padding = new Thickness(16, 12),
                Margin = new Thickness(8, 0, 0, i == Items.Count - 1 ? 0 : 16),
                Content = presenter,
            };
            AvaGrid.SetColumn(card, 1);
            AvaGrid.SetRow(card, i);
            grid.Children.Add(card);
        }

        Child = grid;
    }
}
