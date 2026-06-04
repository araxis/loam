using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;

namespace Loam.Controls;

/// <summary>
/// A text input with a filtered suggestion list, mirroring the reference API's <c>Autocomplete</c>. Composes
/// a <see cref="TextField"/> for the Material chrome and opens a <see cref="Flyout"/> of <see cref="Items"/>
/// that match the typed <see cref="Value"/> (case-insensitive contains); choosing one fills the field.
/// </summary>
public class Autocomplete : TemplatedControl
{
    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<Autocomplete, string?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Identifies the <see cref="Label"/> property.</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<Autocomplete, string?>(nameof(Label));

    /// <summary>Identifies the <see cref="Placeholder"/> property.</summary>
    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<Autocomplete, string?>(nameof(Placeholder));

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly StyledProperty<Variant> VariantProperty =
        AvaloniaProperty.Register<Autocomplete, Variant>(nameof(Variant), Loam.Variant.Outlined);

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Autocomplete, LoamColor>(nameof(Color), LoamColor.Primary);

    /// <summary>Identifies the <see cref="MaxItems"/> property.</summary>
    public static readonly StyledProperty<int> MaxItemsProperty =
        AvaloniaProperty.Register<Autocomplete, int>(nameof(MaxItems), 10);

    private TextField? _field;
    private Flyout? _flyout;
    private bool _selecting;
    private int _searchVersion;

    /// <summary>The candidate suggestions.</summary>
    public ObservableCollection<string> Items { get; } = new();

    /// <summary>Optional synchronous search function. When set, it replaces <see cref="Items"/> filtering.</summary>
    public Func<string?, IEnumerable<string>>? SearchFunc { get; set; }

    /// <summary>Optional asynchronous search function. When set, it replaces <see cref="Items"/> filtering.</summary>
    public Func<string?, CancellationToken, Task<IEnumerable<string>>>? SearchAsync { get; set; }

    /// <summary>Optional row content factory for each suggestion.</summary>
    public Func<string, Control>? ItemTemplate { get; set; }

    /// <summary>The current text value (two-way). Mirrors the reference API's <c>Value</c>.</summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The field label. Mirrors the reference API's <c>Label</c>.</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Placeholder shown when empty. Mirrors the reference API's <c>Placeholder</c>.</summary>
    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Visual style. Mirrors the reference API's <c>Variant</c>.</summary>
    public Variant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Focus accent color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Maximum suggestions shown. Mirrors the reference API's <c>MaxItems</c>.</summary>
    public int MaxItems
    {
        get => GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Autocomplete);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _field = e.NameScope.Find("PART_Field") as TextField;
        if (_field is not null)
        {
            _field.Bind(TextField.TextProperty, new Binding(nameof(Value)) { Source = this, Mode = BindingMode.TwoWay });
            _field.GotFocus += (_, _) => _ = ShowSuggestionsAsync(Value);
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty && !_selecting)
        {
            _ = ShowSuggestionsAsync(Value);
        }
    }

    /// <summary>Returns the suggestions matching <paramref name="text"/> (case-insensitive contains), capped at <paramref name="max"/>.</summary>
    public static IReadOnlyList<string> Filter(IEnumerable<string> items, string? text, int max)
    {
        var query = items.Where(i => string.IsNullOrEmpty(text) ||
            i.Contains(text, StringComparison.OrdinalIgnoreCase));
        return query.Take(Math.Max(0, max)).ToList();
    }

    private async Task ShowSuggestionsAsync(string? text)
    {
        if (_field is null)
        {
            return;
        }

        var version = ++_searchVersion;
        var matches = await ResolveMatchesAsync(text).ConfigureAwait(false);
        if (version != _searchVersion)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => ShowMatches(text, matches));
    }

    private async Task<IReadOnlyList<string>> ResolveMatchesAsync(string? text)
    {
        if (SearchAsync is not null)
        {
            var result = await SearchAsync(text, CancellationToken.None).ConfigureAwait(false);
            return result.Take(Math.Max(0, MaxItems)).ToList();
        }

        if (SearchFunc is not null)
        {
            return SearchFunc(text).Take(Math.Max(0, MaxItems)).ToList();
        }

        return Filter(Items, text, MaxItems);
    }

    private void ShowMatches(string? text, IReadOnlyList<string> matches)
    {
        if (_field is null)
        {
            return;
        }

        if (matches.Count == 0 || (matches.Count == 1 && string.Equals(matches[0], text, StringComparison.OrdinalIgnoreCase)))
        {
            _flyout?.Hide();
            return;
        }

        var list = new StackPanel();
        foreach (var match in matches)
        {
            var content = ItemTemplate is null ? match : (object)ItemTemplate(match);
            var row = new ListItem { Content = content, MinWidth = 200 };
            var captured = match;
            row.PointerPressed += (_, _) => Select(captured);
            list.Children.Add(row);
        }

        _flyout ??= new Flyout { Placement = PlacementMode.BottomEdgeAlignedLeft };
        _flyout.Content = new Paper { Elevation = 8, Padding = new Thickness(0, 8), Content = list };
        _flyout.ShowAt(_field);
    }

    private void Select(string value)
    {
        _selecting = true;
        Value = value;
        _selecting = false;
        _flyout?.Hide();
    }
}
