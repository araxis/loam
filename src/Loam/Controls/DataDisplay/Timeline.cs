using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Controls.Internal;
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

    /// <summary>Creates a generated entry with title, supporting text, optional time text, and color.</summary>
    public TimelineItem(string title, string? subtitle, string? timeText = null, LoamColor color = LoamColor.Primary)
    {
        Title = title;
        Subtitle = subtitle;
        TimeText = timeText;
        Color = color;
    }

    /// <summary>The entry's content (string or any <see cref="Control"/>).</summary>
    public object? Content { get; set; }

    /// <summary>Title used by the generated timeline item layout when <see cref="Content"/> is empty.</summary>
    public string? Title { get; set; }

    /// <summary>Supporting text used by the generated timeline item layout when <see cref="Content"/> is empty.</summary>
    public string? Subtitle { get; set; }

    /// <summary>Optional time or metadata line used by the generated timeline item layout.</summary>
    public string? TimeText { get; set; }

    /// <summary>The dot color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color { get; set; } = LoamColor.Primary;
}

/// <summary>
/// A timeline, mirroring the reference API's <c>Timeline</c>. Renders <see cref="Items"/> along a
/// connector line, each with a colored dot beside its content card.
/// </summary>
public class Timeline : Decorator
{
    private const double MarkerColumn = 28;
    private const double MarkerRow = 28;
    private const double DotSize = 14;

    /// <summary>Identifies the <see cref="Orientation"/> property.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Timeline, Orientation>(nameof(Orientation), Avalonia.Layout.Orientation.Vertical);

    /// <summary>Creates the timeline.</summary>
    public Timeline()
    {
        Focusable = true;
        Items.CollectionChanged += OnItemsChanged;
        InteractionAssist.SetAutomationName(this, "Timeline");
        Rebuild();
    }

    /// <summary>The timeline entries, top to bottom.</summary>
    public ObservableCollection<TimelineItem> Items { get; } = new();

    /// <summary>Whether entries are laid out vertically or horizontally.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsEnabledProperty)
        {
            Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
        }
        else if (change.Property == OrientationProperty)
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        if (Items.Count == 0)
        {
            Child = new Text
            {
                Text = "No timeline items",
                Color = LoamColor.Secondary,
                Typo = Typo.Body2,
                Margin = new Thickness(0, 4),
            };
            UpdateAutomation();
            return;
        }

        Child = Orientation == Avalonia.Layout.Orientation.Horizontal
            ? BuildHorizontal()
            : BuildVertical();
        UpdateAutomation();
    }

    private AvaGrid BuildVertical()
    {
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

            var presenter = new ContentPresenter { Content = BuildItemContent(item) };
            var card = new Paper
            {
                Elevation = 1,
                Focusable = true,
                Padding = new Thickness(16, 12),
                Margin = new Thickness(8, 0, 0, i == Items.Count - 1 ? 0 : 16),
                Content = presenter,
            };
            InteractionAssist.SetAutomationName(card, $"Timeline item {i + 1}: {ItemLabel(item)}");
            AutomationProperties.SetHelpText(card, $"Item {i + 1} of {Items.Count}");
            AvaGrid.SetColumn(card, 1);
            AvaGrid.SetRow(card, i);
            grid.Children.Add(card);
        }

        return grid;
    }

    private AvaGrid BuildHorizontal()
    {
        var grid = new AvaGrid
        {
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        grid.RowDefinitions.Add(new RowDefinition(new GridLength(MarkerRow)));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (var i = 0; i < Items.Count; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        }

        var line = new Border
        {
            Height = 2,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0),
        };
        line.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.Divider));
        AvaGrid.SetRow(line, 0);
        AvaGrid.SetColumn(line, 0);
        AvaGrid.SetColumnSpan(line, Items.Count);
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
                VerticalAlignment = VerticalAlignment.Center,
            };
            dot.Bind(Border.BackgroundProperty, this.GetResourceObservable(SemanticColor.Resolve(item.Color).Fill));
            AvaGrid.SetColumn(dot, i);
            AvaGrid.SetRow(dot, 0);
            grid.Children.Add(dot);

            var presenter = new ContentPresenter { Content = BuildItemContent(item) };
            var card = new Paper
            {
                Elevation = 1,
                Focusable = true,
                MinWidth = 152,
                Padding = new Thickness(16, 12),
                Margin = new Thickness(0, 8, i == Items.Count - 1 ? 0 : 16, 0),
                Content = presenter,
            };
            InteractionAssist.SetAutomationName(card, $"Timeline item {i + 1}: {ItemLabel(item)}");
            AutomationProperties.SetHelpText(card, $"Item {i + 1} of {Items.Count}");
            AvaGrid.SetColumn(card, i);
            AvaGrid.SetRow(card, 1);
            grid.Children.Add(card);
        }

        return grid;
    }

    private void UpdateAutomation()
    {
        AutomationProperties.SetHelpText(this, Items.Count == 0 ? "No items" : $"{Items.Count} items");
    }

    private static object? BuildItemContent(TimelineItem item)
    {
        if (item.Content is not null)
        {
            return item.Content;
        }

        var stack = new StackPanel { Spacing = 3 };
        if (!string.IsNullOrWhiteSpace(item.TimeText))
        {
            stack.Children.Add(new Text
            {
                Text = item.TimeText,
                Typo = Typo.LabelMedium,
                Color = LoamColor.Secondary,
            });
        }

        stack.Children.Add(new Text
        {
            Text = string.IsNullOrWhiteSpace(item.Title) ? "Timeline item" : item.Title,
            Typo = Typo.Subtitle2,
            TextWrapping = TextWrapping.Wrap,
        });

        if (!string.IsNullOrWhiteSpace(item.Subtitle))
        {
            stack.Children.Add(new Text
            {
                Text = item.Subtitle,
                Typo = Typo.Body2,
                Color = LoamColor.Secondary,
                TextWrapping = TextWrapping.Wrap,
            });
        }

        return stack;
    }

    private static string ItemLabel(TimelineItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Title))
        {
            return item.Title!;
        }

        return item.Content switch
        {
            null => "Item",
            string text when !string.IsNullOrWhiteSpace(text) => text,
            TextBlock textBlock when !string.IsNullOrWhiteSpace(textBlock.Text) => textBlock.Text!,
            Text text when !string.IsNullOrWhiteSpace(text.Text) => text.Text!,
            ContentControl { Content: string text } when !string.IsNullOrWhiteSpace(text) => text,
            _ => "Item",
        };
    }
}
