using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// A single collapsible panel, mirroring the reference API's <c>ExpansionPanel</c>. Shows its
/// <see cref="HeaderedContentControl.Header"/> with a chevron; clicking the header toggles
/// <see cref="IsExpanded"/>, which reveals the <see cref="ContentControl.Content"/>.
/// </summary>
public class ExpansionPanel : HeaderedContentControl
{
    /// <summary>Identifies the <see cref="IsExpanded"/> property.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<ExpansionPanel, bool>(nameof(IsExpanded),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private Border? _header;
    private Icon? _chevron;
    private Control? _content;
    private Collapse? _collapse;
    private bool _headerFocused;
    private bool _headerHovered;
    private IDisposable? _headerBackground;

    /// <summary>Creates the panel.</summary>
    public ExpansionPanel()
    {
        Focusable = true;
        GotFocus += (_, _) =>
        {
            _headerFocused = true;
            UpdateHeaderState();
        };
        LostFocus += (_, _) =>
        {
            _headerFocused = false;
            UpdateHeaderState();
        };
    }

    /// <summary>Whether the panel is open. Mirrors the reference API's <c>IsExpanded</c>.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ExpansionPanel);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _header = e.NameScope.Find("PART_Header") as Border;
        _chevron = e.NameScope.Find("PART_Chevron") as Icon;
        _content = e.NameScope.Find("PART_Content") as Control;
        _collapse = e.NameScope.Find("PART_Collapse") as Collapse;
        if (_header is not null)
        {
            _header.Focusable = true;
            _header.PointerPressed += (_, _) =>
            {
                Focus();
                Toggle();
            };
            _header.PointerEntered += (_, _) =>
            {
                _headerHovered = true;
                UpdateHeaderState();
            };
            _header.PointerExited += (_, _) =>
            {
                _headerHovered = false;
                UpdateHeaderState();
            };
            _header.GotFocus += (_, _) =>
            {
                _headerFocused = true;
                UpdateHeaderState();
            };
            _header.LostFocus += (_, _) =>
            {
                _headerFocused = false;
                UpdateHeaderState();
            };
            _header.KeyDown += (_, args) => HandleKeyDown(args);
        }

        UpdateState();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsExpandedProperty)
        {
            UpdateState();
        }
        else if (change.Property == IsEnabledProperty)
        {
            UpdateHeaderState();
        }
        else if (change.Property == HeaderProperty)
        {
            UpdateAutomationName();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        HandleKeyDown(e);
    }

    private void HandleKeyDown(KeyEventArgs e)
    {
        if (!IsEnabled || e.Handled || !InteractionAssist.IsActivationKey(e.Key))
        {
            return;
        }

        Toggle();
        e.Handled = true;
    }

    private void Toggle()
    {
        if (IsEnabled)
        {
            IsExpanded = !IsExpanded;
        }
    }

    private void UpdateState()
    {
        if (_content is not null)
        {
            _content.IsVisible = IsExpanded;
        }

        if (_collapse is not null)
        {
            _collapse.Expanded = IsExpanded;
        }

        if (_chevron is not null)
        {
            _chevron.RenderTransform = new RotateTransform(IsExpanded ? 180 : 0);
        }

        UpdateAutomationName();
        UpdateHeaderState();
    }

    private void UpdateHeaderState()
    {
        Opacity = IsEnabled ? 1 : InteractionAssist.DisabledOpacity(this);
        if (_header is null)
        {
            return;
        }

        _headerBackground?.Dispose();
        _headerBackground = null;

        if (!IsEnabled)
        {
            _header.Background = Brushes.Transparent;
        }
        else if (_headerFocused)
        {
            _headerBackground = _header.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.PaletteFocus(nameof(LoamPalette.Primary))));
        }
        else if (_headerHovered)
        {
            _headerBackground = _header.Bind(Border.BackgroundProperty,
                this.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.TableHover))));
        }
        else
        {
            _header.Background = Brushes.Transparent;
        }
    }

    private void UpdateAutomationName()
    {
        InteractionAssist.SetAutomationName(this, Header, "Expansion panel");
        if (_header is not null)
        {
            InteractionAssist.SetAutomationName(_header, Header, "Expansion panel");
        }
    }
}
