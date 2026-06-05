using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>One tab: a header label and its content, mirroring the reference API's <c>TabPanel</c>.</summary>
public sealed class TabItem
{
    /// <summary>Creates an empty tab.</summary>
    public TabItem()
    {
    }

    /// <summary>Creates a tab with a header and content.</summary>
    public TabItem(string header, Control content)
    {
        Header = header;
        Content = content;
    }

    /// <summary>The tab header text.</summary>
    public string? Header { get; set; }

    /// <summary>The tab content shown when selected.</summary>
    public Control? Content { get; set; }
}

/// <summary>
/// A tab strip with switchable content, mirroring the reference API's <c>Tabs</c>. Add <see cref="TabItem"/>s
/// to <see cref="Items"/>; the active header is underlined in <see cref="Color"/>.
/// </summary>
public class Tabs : TemplatedControl
{
    /// <summary>Identifies the <see cref="SelectedIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<Tabs, int>(nameof(SelectedIndex));

    /// <summary>Identifies the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ColorProperty =
        AvaloniaProperty.Register<Tabs, LoamColor>(nameof(Color), LoamColor.Primary);

    private readonly List<(Border Root, Text Label, Border Underline)> _headerControls = new();
    private StackPanel? _headers;
    private ContentControl? _content;

    /// <summary>Creates the tabs.</summary>
    public Tabs()
    {
        Focusable = true;
        Items.CollectionChanged += OnItemsChanged;
        InteractionAssist.SetAutomationName(this, "Tabs");
    }

    /// <summary>The tabs.</summary>
    public ObservableCollection<TabItem> Items { get; } = new();

    /// <summary>The selected tab index.</summary>
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Active-tab accent color. Mirrors the reference API's <c>Color</c>.</summary>
    public LoamColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Tabs);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _headers = e.NameScope.Find("PART_Headers") as StackPanel;
        _content = e.NameScope.Find("PART_Content") as ContentControl;
        Rebuild();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedIndexProperty)
        {
            UpdateActive();
            ShowContent();
        }
        else if (change.Property == ColorProperty)
        {
            Rebuild();
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (_headers is null)
        {
            return;
        }

        _headers.Children.Clear();
        _headerControls.Clear();

        var accent = Color is LoamColor.Default or LoamColor.Inherit
            ? LoamTokens.Primary
            : LoamTokens.Palette(Color.ToPaletteName()!);

        for (var i = 0; i < Items.Count; i++)
        {
            var label = new Text { Text = Items[i].Header, Typo = Typo.Button, HorizontalAlignment = HorizontalAlignment.Center };
            var underline = new Border { Height = 2, Margin = new Thickness(0, 8, 0, 0), IsVisible = false };
            underline.Bind(Border.BackgroundProperty, this.GetResourceObservable(accent));

            var root = new Border
            {
                Child = new StackPanel { Children = { label, underline } },
                Padding = InteractionAssist.ThicknessToken(this, LoamTokens.DensityButtonPaddingMedium, new Thickness(16, 12, 16, 0)),
                Cursor = new Cursor(StandardCursorType.Hand),
                Focusable = true,
            };
            root.Bind(Layoutable.MinHeightProperty, this.GetResourceObservable(LoamTokens.DensityInteractiveMedium));
            var index = i;
            InteractionAssist.SetAutomationName(root, Items[i].Header);
            root.PointerPressed += (_, _) =>
            {
                root.Focus();
                SelectedIndex = index;
            };
            root.KeyDown += (_, args) =>
            {
                if (InteractionAssist.IsActivationKey(args.Key))
                {
                    SelectedIndex = index;
                    args.Handled = true;
                }
                else if (InteractionAssist.IsIncrementKey(args.Key))
                {
                    MoveSelection(index, 1);
                    args.Handled = true;
                }
                else if (InteractionAssist.IsDecrementKey(args.Key))
                {
                    MoveSelection(index, -1);
                    args.Handled = true;
                }
            };
            root.GotFocus += (_, _) => UpdateActive();
            root.LostFocus += (_, _) => UpdateActive();

            _headers.Children.Add(root);
            _headerControls.Add((root, label, underline));
        }

        UpdateActive();
        ShowContent();
    }

    private void UpdateActive()
    {
        for (var i = 0; i < _headerControls.Count; i++)
        {
            var active = i == SelectedIndex;
            _headerControls[i].Label.Color = active ? Color : LoamColor.Default;
            _headerControls[i].Underline.IsVisible = active;
            if (!active && _headerControls[i].Root.IsFocused)
            {
                var accentName = Color is LoamColor.Default or LoamColor.Inherit
                    ? nameof(LoamPalette.Primary)
                    : Color.ToPaletteName()!;
                _headerControls[i].Root.Bind(Border.BackgroundProperty, this.GetResourceObservable(LoamTokens.PaletteFocus(accentName)));
            }
            else
            {
                _headerControls[i].Root.Background = null;
            }
        }
    }

    private void ShowContent()
    {
        if (_content is not null && SelectedIndex >= 0 && SelectedIndex < Items.Count)
        {
            _content.Content = Items[SelectedIndex].Content;
        }
    }

    private void MoveSelection(int currentIndex, int direction)
    {
        if (_headerControls.Count == 0)
        {
            return;
        }

        var next = Math.Clamp(currentIndex + direction, 0, _headerControls.Count - 1);
        SelectedIndex = next;
        _headerControls[next].Root.Focus();
    }
}
