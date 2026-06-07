using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A text input with a filtered suggestion list, mirroring the reference API's <c>Autocomplete</c>. Composes
/// a <see cref="TextField"/> for the Material chrome and opens a popup of <see cref="Items"/>
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

    /// <summary>Identifies the <see cref="HelperText"/> property.</summary>
    public static readonly StyledProperty<string?> HelperTextProperty =
        AvaloniaProperty.Register<Autocomplete, string?>(nameof(HelperText));

    /// <summary>Identifies the <see cref="ErrorText"/> property.</summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<Autocomplete, string?>(nameof(ErrorText));

    /// <summary>Identifies the <see cref="Error"/> property.</summary>
    public static readonly StyledProperty<bool> ErrorProperty =
        AvaloniaProperty.Register<Autocomplete, bool>(nameof(Error));

    /// <summary>Identifies the <see cref="ShrinkLabel"/> property.</summary>
    public static readonly StyledProperty<bool> ShrinkLabelProperty =
        AvaloniaProperty.Register<Autocomplete, bool>(nameof(ShrinkLabel));

    private TextField? _field;
    private TextBox? _textBox;
    private Popup? _popup;
    private Paper? _popupPaper;
    private ScrollViewer? _popupScroller;
    private StackPanel? _popupList;
    private bool _selecting;
    private bool _syncingFieldText;
    private bool _syncingValueFromField;
    private bool _raisedZIndex;
    private int _restingZIndex;
    private int _searchVersion;
    private double _openPopupHeight;
    private const double MinPopupWidth = 240;
    private const double MaxPopupHeight = 320;
    private const double SuggestionRowHeight = 48;
    private const double SuggestionFontSize = 16;
    private const double SuggestionLineHeight = 24;

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

    /// <summary>Helper text shown below the field.</summary>
    public string? HelperText
    {
        get => GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Error message shown instead of helper text when <see cref="Error"/>.</summary>
    public string? ErrorText
    {
        get => GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>Whether the field is in an error state.</summary>
    public bool Error
    {
        get => GetValue(ErrorProperty);
        set => SetValue(ErrorProperty, value);
    }

    /// <summary>When true, the label stays floated above the field even when empty and unfocused.</summary>
    public bool ShrinkLabel
    {
        get => GetValue(ShrinkLabelProperty);
        set => SetValue(ShrinkLabelProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Autocomplete);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_field is not null)
        {
            _field.GotFocus -= OnFieldGotFocus;
            _field.LostFocus -= OnFieldLostFocus;
            _field.KeyDown -= OnFieldKeyDown;
            _field.PropertyChanged -= OnFieldPropertyChanged;
        }
        if (_textBox is not null)
        {
            _textBox.GotFocus -= OnFieldGotFocus;
            _textBox.LostFocus -= OnFieldLostFocus;
            _textBox.PropertyChanged -= OnTextBoxPropertyChanged;
            _textBox.TextChanged -= OnTextBoxTextChanged;
        }

        _field = e.NameScope.Find("PART_Field") as TextField;
        _textBox = null;
        _popup = e.NameScope.Find("PART_Popup") as Popup;
        _popupPaper = null;
        _popupScroller = null;
        _popupList = null;
        _openPopupHeight = 0;
        if (_popup is not null && _field is not null)
        {
            _popup.PlacementTarget = _field;
        }

        if (_field is not null)
        {
            SyncFieldText(Value, moveCaretToEnd: false);
            _field.PropertyChanged += OnFieldPropertyChanged;
            _field.GotFocus += OnFieldGotFocus;
            _field.LostFocus += OnFieldLostFocus;
            _field.KeyDown += OnFieldKeyDown;

            _field.ApplyTemplate();
            AttachTextBox(_field.GetVisualDescendants().OfType<TextBox>().FirstOrDefault());
        }

        ApplyAutomation();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            if (!_syncingValueFromField)
            {
                SyncFieldText(Value, moveCaretToEnd: _selecting);
            }

            if (!_selecting && !_syncingValueFromField)
            {
                _ = ShowSuggestionsAsync(Value);
            }
        }

        if (change.Property == LabelProperty || change.Property == HelperTextProperty ||
            change.Property == ErrorTextProperty || change.Property == ErrorProperty)
        {
            ApplyAutomation();
        }
    }

    private void OnFieldGotFocus(object? sender, EventArgs e)
    {
        _ = ShowSuggestionsAsync(CurrentEditorText());
    }

    private void OnFieldLostFocus(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(Close, DispatcherPriority.Background);
    }

    private void OnFieldPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == TextField.TextProperty)
        {
            OnFieldTextChanged(change.GetNewValue<string?>());
        }
    }

    private void OnTextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == TextBox.TextProperty)
        {
            OnFieldTextChanged(change.GetNewValue<string?>());
        }
    }

    private void OnTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        OnFieldTextChanged(CurrentEditorText());
    }

    private void OnFieldTextChanged(string? text)
    {
        if (_syncingFieldText)
        {
            return;
        }

        SyncWrapperText(text);

        if (!string.Equals(Value, text, StringComparison.Ordinal))
        {
            _syncingValueFromField = true;
            try
            {
                SetCurrentValue(ValueProperty, text);
            }
            finally
            {
                _syncingValueFromField = false;
            }
        }

        if (!_selecting)
        {
            _ = ShowSuggestionsAsync(text);
        }
    }

    private void OnFieldKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
        else if (InteractionAssist.IsActivationKey(e.Key))
        {
            _ = ShowSuggestionsAsync(CurrentEditorText());
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
        if (string.IsNullOrWhiteSpace(text))
        {
            Close();
            return;
        }

        var matches = await ResolveMatchesAsync(text).ConfigureAwait(false);
        if (version != _searchVersion)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (version != _searchVersion || !string.Equals(CurrentEditorText(), text, StringComparison.Ordinal))
            {
                return;
            }

            ShowMatches(text, matches);
        });
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
            Close();
            return;
        }

        var width = PopupWidth();
        var desiredHeight = PopupHeight(matches.Count);
        var height = StablePopupHeight(desiredHeight);
        var list = EnsurePopupSurface(width, height);
        list.Children.Clear();
        foreach (var match in matches)
        {
            var captured = match;
            var row = BuildSuggestionRow(match, width, () => Select(captured));
            list.Children.Add(row);
        }

        if (_popup is null)
        {
            return;
        }

        RaisePopupLayer();
        if (!ReferenceEquals(_popup.Child, _popupPaper))
        {
            _popup.Child = _popupPaper;
        }

        _popup.IsOpen = true;
    }

    private void Select(string value)
    {
        _selecting = true;
        try
        {
            SetCurrentValue(ValueProperty, value);
            SyncFieldText(value, moveCaretToEnd: true);
            Close();
            _field?.Focus();
        }
        finally
        {
            _selecting = false;
        }
    }

    private void SyncFieldText(string? text, bool moveCaretToEnd)
    {
        if (_field is null)
        {
            return;
        }

        _syncingFieldText = true;
        try
        {
            if (!string.Equals(_field.Text, text, StringComparison.Ordinal))
            {
                _field.SetCurrentValue(TextField.TextProperty, text);
            }
        }
        finally
        {
            _syncingFieldText = false;
        }

        if (moveCaretToEnd)
        {
            MoveCaretToEnd();
        }
    }

    private void MoveCaretToEnd()
    {
        if (_field is null)
        {
            return;
        }

        _field.ApplyTemplate();
        var textBox = _field.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        if (textBox is null)
        {
            return;
        }

        var length = textBox.Text?.Length ?? 0;
        textBox.CaretIndex = length;
        textBox.SelectionStart = length;
        textBox.SelectionEnd = length;
    }

    private void Close()
    {
        _searchVersion++;
        if (_popup is not null)
        {
            _popup.IsOpen = false;
        }

        _openPopupHeight = 0;
        RestorePopupLayer();
    }

    private string? CurrentEditorText()
    {
        if (_field is not null)
        {
            _field.ApplyTemplate();
            AttachTextBox(_field.GetVisualDescendants().OfType<TextBox>().FirstOrDefault());
        }

        return _textBox?.Text ?? _field?.Text ?? Value;
    }

    private void AttachTextBox(TextBox? textBox)
    {
        if (ReferenceEquals(_textBox, textBox))
        {
            return;
        }

        if (_textBox is not null)
        {
            _textBox.GotFocus -= OnFieldGotFocus;
            _textBox.LostFocus -= OnFieldLostFocus;
            _textBox.PropertyChanged -= OnTextBoxPropertyChanged;
            _textBox.TextChanged -= OnTextBoxTextChanged;
        }

        _textBox = textBox;
        if (_textBox is null)
        {
            return;
        }

        _textBox.GotFocus += OnFieldGotFocus;
        _textBox.LostFocus += OnFieldLostFocus;
        _textBox.PropertyChanged += OnTextBoxPropertyChanged;
        _textBox.TextChanged += OnTextBoxTextChanged;
    }

    private void SyncWrapperText(string? text)
    {
        if (_field is null || string.Equals(_field.Text, text, StringComparison.Ordinal))
        {
            return;
        }

        _syncingFieldText = true;
        try
        {
            _field.SetCurrentValue(TextField.TextProperty, text);
        }
        finally
        {
            _syncingFieldText = false;
        }
    }

    private double PopupWidth()
    {
        if (_field is null)
        {
            return MinPopupWidth;
        }

        var width = _field.Bounds.Width;
        if (double.IsNaN(width) || width <= 0)
        {
            width = _field.Width;
        }

        if (double.IsNaN(width) || width <= 0)
        {
            width = _field.MinWidth;
        }

        return Math.Max(MinPopupWidth, width);
    }

    private static double PopupHeight(int rowCount) =>
        Math.Min(MaxPopupHeight, Math.Max(SuggestionRowHeight, rowCount * SuggestionRowHeight));

    private double StablePopupHeight(double desiredHeight)
    {
        if (_popup?.IsOpen == true)
        {
            _openPopupHeight = Math.Max(_openPopupHeight, desiredHeight);
        }
        else
        {
            _openPopupHeight = desiredHeight;
        }

        return _openPopupHeight;
    }

    private StackPanel EnsurePopupSurface(double width, double height)
    {
        if (_popupList is null || _popupScroller is null || _popupPaper is null)
        {
            _popupList = new StackPanel();
            _popupScroller = new ScrollViewer
            {
                Content = _popupList,
                MaxHeight = MaxPopupHeight,
                ClipToBounds = true,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            _popupPaper = PopupSurface.MenuPaper(_popupScroller, width);
            _popupPaper.MaxHeight = MaxPopupHeight;
            _popupPaper.ClipToBounds = true;
            InteractionAssist.ApplyZIndex(_popupPaper, LoamTokens.ZIndex(nameof(LoamZIndex.Popover)),
                LoamZIndex.Default.Popover);
        }

        _popupList.Width = width;
        _popupScroller.Width = width;
        _popupScroller.Height = height;
        _popupPaper.Width = width;
        _popupPaper.Height = height;
        _popupPaper.MinWidth = width;
        return _popupList;
    }

    private void RaisePopupLayer()
    {
        if (!_raisedZIndex)
        {
            _restingZIndex = ZIndex;
            _raisedZIndex = true;
        }

        InteractionAssist.ApplyZIndex(this, LoamTokens.ZIndex(nameof(LoamZIndex.Popover)), LoamZIndex.Default.Popover);
    }

    private void RestorePopupLayer()
    {
        if (!_raisedZIndex)
        {
            return;
        }

        ZIndex = _restingZIndex;
        _raisedZIndex = false;
    }

    private Border BuildSuggestionRow(string match, double width, Action activate)
    {
        var content = NormalizeSuggestionContent(ItemTemplate is null ? SuggestionText(match) : ItemTemplate(match));

        var stateLayer = new Border
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false,
        };

        var rowContent = new Avalonia.Controls.Grid
        {
            Children = { stateLayer, content },
        };

        var row = new Border
        {
            Width = width,
            Height = SuggestionRowHeight,
            MinHeight = SuggestionRowHeight,
            Padding = new Thickness(16, 0),
            Cursor = new Cursor(StandardCursorType.Hand),
            Focusable = true,
            ClipToBounds = true,
            Child = rowContent,
        };
        AutomationProperties.SetName(row, match);
        AutomationProperties.SetHelpText(row, "Autocomplete suggestion");
        row.Bind(Border.BackgroundProperty, this.GetResourceObservable(InteractionAssist.TonalSurfaceToken(3)));

        IDisposable? backgroundBinding = null;
        void SetStateLayer(string? state)
        {
            backgroundBinding?.Dispose();
            backgroundBinding = null;
            if (state is null)
            {
                stateLayer.Background = Brushes.Transparent;
                return;
            }

            backgroundBinding = stateLayer.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.ColorSchemeStateLayer(nameof(LoamColorScheme.OnSurface), state)));
        }

        row.PointerEntered += (_, _) => SetStateLayer("Hover");
        row.PointerExited += (_, _) => SetStateLayer(row.IsFocused ? "Focus" : null);
        row.GotFocus += (_, _) => SetStateLayer("Focus");
        row.LostFocus += (_, _) => SetStateLayer(null);
        row.PointerPressed += (_, args) =>
        {
            if (!row.IsEnabled)
            {
                return;
            }

            row.Focus();
            activate();
            args.Handled = true;
        };
        row.KeyDown += (_, args) =>
        {
            if (!row.IsEnabled || !InteractionAssist.IsActivationKey(args.Key))
            {
                return;
            }

            activate();
            args.Handled = true;
        };

        return row;
    }

    private void ApplyAutomation()
    {
        AutomationProperties.SetName(this,
            string.IsNullOrWhiteSpace(Label) ? "Autocomplete" : Label);

        var help = Error && !string.IsNullOrWhiteSpace(ErrorText) ? ErrorText : HelperText;
        AutomationProperties.SetHelpText(this, help);
    }

    private static TextBlock SuggestionText(string text) =>
        new()
        {
            Text = text,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

    private static Control NormalizeSuggestionContent(Control content)
    {
        content.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        content.IsHitTestVisible = false;

        if (content is Text text)
        {
            text.Typo = Typo.Inherit;
            ApplySuggestionTextMetrics(text);
        }
        else if (content is TextBlock textBlock)
        {
            ApplySuggestionTextMetrics(textBlock);
        }

        return content;
    }

    private static void ApplySuggestionTextMetrics(TextBlock textBlock)
    {
        textBlock.FontSize = SuggestionFontSize;
        textBlock.FontWeight = FontWeight.Normal;
        textBlock.LineHeight = SuggestionLineHeight;
        textBlock.Margin = default;
        textBlock.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
    }
}
