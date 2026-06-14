using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Legend rows for Loam chart series, using the same default token order as chart rendering.</summary>
public class ChartLegend : StackPanel
{
    private static readonly string[] DefaultColorTokens =
    [
        LoamTokens.ColorPrimary,
        LoamTokens.ColorSecondary,
        LoamTokens.ColorTertiary,
        LoamTokens.ColorError,
        LoamTokens.ColorPrimaryContainer,
        LoamTokens.ColorSecondaryContainer,
        LoamTokens.ColorScheme(nameof(LoamColorScheme.TertiaryContainer)),
        LoamTokens.ColorOnSurfaceVariant,
    ];

    private IReadOnlyList<Color>? _colors;
    private ChartBase? _source;
    private IReadOnlyList<(string Label, Color Color)>? _sourceEntries;

    /// <summary>Identifies the <see cref="ShowSwatches"/> property.</summary>
    public static readonly StyledProperty<bool> ShowSwatchesProperty =
        AvaloniaProperty.Register<ChartLegend, bool>(nameof(ShowSwatches), defaultValue: true);

    /// <summary>Labels rendered as legend rows.</summary>
    public AvaloniaList<string> Labels { get; } = [];

    /// <summary>Optional explicit swatch colors. When omitted, legend swatches bind to theme role tokens.</summary>
    public IReadOnlyList<Color>? Colors
    {
        get => _colors;
        set
        {
            _colors = value;
            RebuildRows();
        }
    }

    /// <summary>Whether each row includes a color swatch.</summary>
    public bool ShowSwatches
    {
        get => GetValue(ShowSwatchesProperty);
        set => SetValue(ShowSwatchesProperty, value);
    }

    /// <summary>
    /// Optional chart to bind to. When set, the legend derives its rows (labels and colors) from the
    /// chart — one row per series for multi-series charts, otherwise one per category — and refreshes
    /// automatically when the chart's data changes. Overrides manual <see cref="Labels"/>/<see cref="Colors"/>.
    /// </summary>
    public ChartBase? Source
    {
        get => _source;
        set
        {
            if (ReferenceEquals(_source, value))
            {
                return;
            }

            if (_source is not null)
            {
                _source.SnapshotChanged -= OnSourceChanged;
            }

            _source = value;
            if (_source is not null)
            {
                _source.SnapshotChanged += OnSourceChanged;
            }

            RefreshFromSource();
        }
    }

    /// <summary>Initializes a new instance of the <see cref="ChartLegend"/> class.</summary>
    public ChartLegend()
    {
        Orientation = Orientation.Vertical;
        Spacing = 6;
        Labels.CollectionChanged += (_, _) => RebuildRows();
        AutomationProperties.SetName(this, "Chart legend");
    }

    private void OnSourceChanged(object? sender, EventArgs e) => RefreshFromSource();

    private void RefreshFromSource()
    {
        _sourceEntries = _source?.GetLegendEntries();
        RebuildRows();
    }

    private void RebuildRows()
    {
        Children.Clear();

        if (_sourceEntries is { } entries)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                Children.Add(BuildRow(i, entries[i].Label, entries[i].Color));
            }

            AutomationProperties.SetHelpText(this, $"{entries.Count} item{(entries.Count == 1 ? string.Empty : "s")}");
            return;
        }

        for (var i = 0; i < Labels.Count; i++)
        {
            Children.Add(BuildRow(i, Labels[i], color: null));
        }

        AutomationProperties.SetHelpText(this, $"{Labels.Count} item{(Labels.Count == 1 ? string.Empty : "s")}");
    }

    private StackPanel BuildRow(int index, string label, Color? color)
    {
        var swatch = new Border
        {
            Width = 12,
            Height = 12,
            CornerRadius = new CornerRadius(6),
            VerticalAlignment = VerticalAlignment.Center,
        };

        if (color is { } explicitColor)
        {
            swatch.Background = new SolidColorBrush(explicitColor);
        }
        else if (_colors is { Count: > 0 })
        {
            swatch.Background = new SolidColorBrush(_colors[index % _colors.Count]);
        }
        else
        {
            swatch.Bind(Border.BackgroundProperty,
                swatch.GetResourceObservable(DefaultColorTokens[index % DefaultColorTokens.Length]));
        }

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
        };

        if (ShowSwatches)
        {
            row.Children.Add(swatch);
        }

        row.Children.Add(new Text
        {
            Text = label,
            Typo = Typo.Caption,
            VerticalAlignment = VerticalAlignment.Center,
        });

        return row;
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_source is not null)
        {
            _source.SnapshotChanged -= OnSourceChanged;
            _source.SnapshotChanged += OnSourceChanged;
            RefreshFromSource();
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_source is not null)
        {
            _source.SnapshotChanged -= OnSourceChanged;
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ShowSwatchesProperty)
        {
            RebuildRows();
        }
    }
}
