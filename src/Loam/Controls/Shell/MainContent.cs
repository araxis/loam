using Avalonia.Automation;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Loam.Controls.Internal;

namespace Loam.Controls;

/// <summary>
/// The scrollable main content region of a <see cref="Layout"/>, mirroring the reference API's
/// <c>MainContent</c>. Wraps its content in a padded scroll viewer.
/// </summary>
public class MainContent : ContentControl
{
    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MainContent, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Subtitle"/> property.</summary>
    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<MainContent, string?>(nameof(Subtitle));

    /// <summary>Identifies the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MainContent, object?>(nameof(Header));

    /// <summary>Identifies the <see cref="PrimaryActionText"/> property.</summary>
    public static readonly StyledProperty<string?> PrimaryActionTextProperty =
        AvaloniaProperty.Register<MainContent, string?>(nameof(PrimaryActionText));

    /// <summary>Identifies the <see cref="SecondaryActionText"/> property.</summary>
    public static readonly StyledProperty<string?> SecondaryActionTextProperty =
        AvaloniaProperty.Register<MainContent, string?>(nameof(SecondaryActionText));

    /// <summary>Identifies the <see cref="ActionColor"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ActionColorProperty =
        AvaloniaProperty.Register<MainContent, LoamColor>(nameof(ActionColor), LoamColor.Primary);

    private readonly global::Avalonia.Controls.Grid _generatedHeader = new()
    {
        ColumnDefinitions = new ColumnDefinitions("*,Auto"),
        Margin = new Thickness(0, 0, 0, 20),
    };
    private readonly StackPanel _generatedTextStack = new() { Spacing = 4 };
    private readonly StackPanel _generatedActionRow = new()
    {
        Orientation = Orientation.Horizontal,
        Spacing = 8,
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Center,
    };
    private readonly Text _generatedTitle = new() { Typo = Typo.HeadlineSmall };
    private readonly Text _generatedSubtitle = new()
    {
        Typo = Typo.Body2,
        Color = LoamColor.Secondary,
        TextWrapping = TextWrapping.Wrap,
        MaxWidth = 560,
    };
    private readonly Button _generatedSecondaryAction = new() { Variant = Variant.Text, Size = LoamSize.Small };
    private readonly Button _generatedPrimaryAction = new() { Variant = Variant.Filled, Size = LoamSize.Small };
    private ContentControl? _headerPresenter;

    /// <summary>Creates the scrollable main content region.</summary>
    public MainContent()
    {
        AutomationProperties.SetName(this, "Main content");
        Actions.CollectionChanged += (_, _) => UpdateHeader();
        _generatedSecondaryAction.Click += (_, _) => SecondaryActionClick?.Invoke(this, EventArgs.Empty);
        _generatedPrimaryAction.Click += (_, _) => PrimaryActionClick?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raised when the generated primary header action is clicked.</summary>
    public event EventHandler? PrimaryActionClick;

    /// <summary>Raised when the generated secondary header action is clicked.</summary>
    public event EventHandler? SecondaryActionClick;

    /// <summary>Generated page title shown above the scrollable content body.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Generated supporting text shown below <see cref="Title"/>.</summary>
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Custom header content. Leave unset to use generated title/action anatomy.</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Generated primary action text shown in the page header.</summary>
    public string? PrimaryActionText
    {
        get => GetValue(PrimaryActionTextProperty);
        set => SetValue(PrimaryActionTextProperty, value);
    }

    /// <summary>Generated secondary action text shown in the page header.</summary>
    public string? SecondaryActionText
    {
        get => GetValue(SecondaryActionTextProperty);
        set => SetValue(SecondaryActionTextProperty, value);
    }

    /// <summary>Semantic color for generated header actions.</summary>
    public LoamColor ActionColor
    {
        get => GetValue(ActionColorProperty);
        set => SetValue(ActionColorProperty, value);
    }

    /// <summary>Custom header actions rendered before generated text actions.</summary>
    public AvaloniaList<Control> Actions { get; } = [];

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(MainContent);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _headerPresenter = e.NameScope.Find<ContentControl>("PART_HeaderPresenter");
        UpdateHeader();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TitleProperty ||
            change.Property == SubtitleProperty ||
            change.Property == HeaderProperty ||
            change.Property == PrimaryActionTextProperty ||
            change.Property == SecondaryActionTextProperty ||
            change.Property == ActionColorProperty)
        {
            UpdateHeader();
        }
    }

    private void UpdateHeader()
    {
        InteractionAssist.SetAutomationName(this, Title, Header, "Main content");
        AutomationProperties.SetHelpText(this, Subtitle);

        if (_headerPresenter is null)
        {
            return;
        }

        var header = Header ?? BuildGeneratedHeader();
        _headerPresenter.Content = header;
        _headerPresenter.IsVisible = header is not null;
    }

    private global::Avalonia.Controls.Grid? BuildGeneratedHeader()
    {
        var hasText = !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Subtitle);
        var hasActions = Actions.Count > 0 ||
                         !string.IsNullOrWhiteSpace(PrimaryActionText) ||
                         !string.IsNullOrWhiteSpace(SecondaryActionText);
        if (!hasText && !hasActions)
        {
            return null;
        }

        _generatedTextStack.Children.Clear();
        _generatedActionRow.Children.Clear();

        if (!string.IsNullOrWhiteSpace(Title))
        {
            _generatedTitle.Text = Title;
            _generatedTextStack.Children.Add(_generatedTitle);
        }

        if (!string.IsNullOrWhiteSpace(Subtitle))
        {
            _generatedSubtitle.Text = Subtitle;
            _generatedTextStack.Children.Add(_generatedSubtitle);
        }

        foreach (var action in Actions)
        {
            _generatedActionRow.Children.Add(action);
        }

        if (!string.IsNullOrWhiteSpace(SecondaryActionText))
        {
            _generatedSecondaryAction.Content = SecondaryActionText;
            _generatedSecondaryAction.Color = ActionColor;
            InteractionAssist.SetAutomationName(_generatedSecondaryAction, SecondaryActionText);
            _generatedActionRow.Children.Add(_generatedSecondaryAction);
        }

        if (!string.IsNullOrWhiteSpace(PrimaryActionText))
        {
            _generatedPrimaryAction.Content = PrimaryActionText;
            _generatedPrimaryAction.Color = ActionColor;
            InteractionAssist.SetAutomationName(_generatedPrimaryAction, PrimaryActionText);
            _generatedActionRow.Children.Add(_generatedPrimaryAction);
        }

        _generatedHeader.Children.Clear();
        if (_generatedTextStack.Children.Count > 0)
        {
            global::Avalonia.Controls.Grid.SetColumn(_generatedTextStack, 0);
            _generatedHeader.Children.Add(_generatedTextStack);
        }

        if (_generatedActionRow.Children.Count > 0)
        {
            global::Avalonia.Controls.Grid.SetColumn(_generatedActionRow, 1);
            _generatedHeader.Children.Add(_generatedActionRow);
        }

        return _generatedHeader;
    }
}
