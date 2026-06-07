using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Loam.Controls.Internal;

namespace Loam.Controls;

/// <summary>
/// A surface for grouping related content and actions. Use the high-level title/media/body/action slots for
/// standard card anatomy, or set inherited <see cref="ContentControl.Content"/> for a custom layout.
/// </summary>
public class Card : Paper
{
    private readonly StackPanel _generatedRoot = new();
    private readonly CardHeader _generatedHeader = new();
    private readonly CardMedia _generatedMedia = new();
    private readonly CardContent _generatedBody = new();
    private readonly CardActions _generatedActions = new();
    private readonly StackPanel _generatedActionRow = new() { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right };
    private readonly Button _generatedSecondaryAction = new() { Variant = Variant.Text, Color = LoamColor.Primary };
    private readonly Button _generatedPrimaryAction = new() { Variant = Variant.Filled, Color = LoamColor.Primary };
    private bool _usingGeneratedContent;
    private bool _updatingContent;
    private bool _hasCustomContent;

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static new readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Card, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Subtitle"/> property.</summary>
    public static new readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<Card, string?>(nameof(Subtitle));

    /// <summary>Identifies the <see cref="HeaderAvatar"/> property.</summary>
    public static readonly StyledProperty<object?> HeaderAvatarProperty =
        AvaloniaProperty.Register<Card, object?>(nameof(HeaderAvatar));

    /// <summary>Identifies the <see cref="HeaderAction"/> property.</summary>
    public static readonly StyledProperty<object?> HeaderActionProperty =
        AvaloniaProperty.Register<Card, object?>(nameof(HeaderAction));

    /// <summary>Identifies the <see cref="Body"/> property.</summary>
    public static new readonly StyledProperty<object?> BodyProperty =
        AvaloniaProperty.Register<Card, object?>(nameof(Body));

    /// <summary>Identifies the <see cref="BodyText"/> property.</summary>
    public static readonly StyledProperty<string?> BodyTextProperty =
        AvaloniaProperty.Register<Card, string?>(nameof(BodyText));

    /// <summary>Identifies the <see cref="ShowMedia"/> property.</summary>
    public static readonly StyledProperty<bool> ShowMediaProperty =
        AvaloniaProperty.Register<Card, bool>(nameof(ShowMedia));

    /// <summary>Identifies the <see cref="MediaSource"/> property.</summary>
    public static readonly StyledProperty<IImage?> MediaSourceProperty =
        AvaloniaProperty.Register<Card, IImage?>(nameof(MediaSource));

    /// <summary>Identifies the <see cref="MediaHeight"/> property.</summary>
    public static readonly StyledProperty<double> MediaHeightProperty =
        AvaloniaProperty.Register<Card, double>(nameof(MediaHeight), 180d);

    /// <summary>Identifies the <see cref="PrimaryActionText"/> property.</summary>
    public static readonly StyledProperty<string?> PrimaryActionTextProperty =
        AvaloniaProperty.Register<Card, string?>(nameof(PrimaryActionText));

    /// <summary>Identifies the <see cref="SecondaryActionText"/> property.</summary>
    public static readonly StyledProperty<string?> SecondaryActionTextProperty =
        AvaloniaProperty.Register<Card, string?>(nameof(SecondaryActionText));

    /// <summary>Identifies the <see cref="ActionColor"/> property.</summary>
    public static readonly StyledProperty<LoamColor> ActionColorProperty =
        AvaloniaProperty.Register<Card, LoamColor>(nameof(ActionColor), LoamColor.Primary);

    /// <summary>Creates a card.</summary>
    public Card()
    {
        Actions.CollectionChanged += OnGeneratedContentChanged;
        _generatedPrimaryAction.Click += (_, _) => PrimaryActionClick?.Invoke(this, EventArgs.Empty);
        _generatedSecondaryAction.Click += (_, _) => SecondaryActionClick?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raised when the generated primary action is clicked.</summary>
    public event EventHandler? PrimaryActionClick;

    /// <summary>Raised when the generated secondary action is clicked.</summary>
    public event EventHandler? SecondaryActionClick;

    /// <summary>Header title for the standard card layout.</summary>
    public new string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Header subtitle for the standard card layout.</summary>
    public new string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Leading header visual, usually an <see cref="Avatar"/>.</summary>
    public object? HeaderAvatar
    {
        get => GetValue(HeaderAvatarProperty);
        set => SetValue(HeaderAvatarProperty, value);
    }

    /// <summary>Trailing header visual, usually an <see cref="IconButton"/>.</summary>
    public object? HeaderAction
    {
        get => GetValue(HeaderActionProperty);
        set => SetValue(HeaderActionProperty, value);
    }

    /// <summary>Body content for the standard card layout.</summary>
    public new object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>Convenience body text for the standard card layout.</summary>
    public string? BodyText
    {
        get => GetValue(BodyTextProperty);
        set => SetValue(BodyTextProperty, value);
    }

    /// <summary>Whether the standard card layout includes a media band even when no image source is set.</summary>
    public bool ShowMedia
    {
        get => GetValue(ShowMediaProperty);
        set => SetValue(ShowMediaProperty, value);
    }

    /// <summary>Image source for the standard card media band.</summary>
    public IImage? MediaSource
    {
        get => GetValue(MediaSourceProperty);
        set => SetValue(MediaSourceProperty, value);
    }

    /// <summary>Height for the standard card media band.</summary>
    public double MediaHeight
    {
        get => GetValue(MediaHeightProperty);
        set => SetValue(MediaHeightProperty, value);
    }

    /// <summary>Generated primary action text. Leave unset to use <see cref="Actions"/>.</summary>
    public string? PrimaryActionText
    {
        get => GetValue(PrimaryActionTextProperty);
        set => SetValue(PrimaryActionTextProperty, value);
    }

    /// <summary>Generated secondary action text. Leave unset to use <see cref="Actions"/>.</summary>
    public string? SecondaryActionText
    {
        get => GetValue(SecondaryActionTextProperty);
        set => SetValue(SecondaryActionTextProperty, value);
    }

    /// <summary>Color for generated primary and secondary actions.</summary>
    public LoamColor ActionColor
    {
        get => GetValue(ActionColorProperty);
        set => SetValue(ActionColorProperty, value);
    }

    /// <summary>Action controls rendered in the standard card action row.</summary>
    public AvaloniaList<Control> Actions { get; } = [];

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Paper);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty && !_updatingContent)
        {
            _usingGeneratedContent = false;
            _hasCustomContent = Content is not null;
            DualContent.WarnIfConflicting(_hasCustomContent, HasGeneratedContent, GetType().Name);
            return;
        }

        if (change.Property == TitleProperty ||
            change.Property == SubtitleProperty ||
            change.Property == HeaderAvatarProperty ||
            change.Property == HeaderActionProperty ||
            change.Property == BodyProperty ||
            change.Property == BodyTextProperty ||
            change.Property == ShowMediaProperty ||
            change.Property == MediaSourceProperty ||
            change.Property == MediaHeightProperty ||
            change.Property == PrimaryActionTextProperty ||
            change.Property == SecondaryActionTextProperty ||
            change.Property == ActionColorProperty)
        {
            SyncGeneratedContent();
        }
    }

    private void OnGeneratedContentChanged(object? sender, NotifyCollectionChangedEventArgs e) => SyncGeneratedContent();

    private void SyncGeneratedContent()
    {
        if (!HasGeneratedContent)
        {
            return;
        }

        DualContent.WarnIfConflicting(_hasCustomContent, hasGeneratedContent: true, GetType().Name);

        if (_hasCustomContent && !_usingGeneratedContent)
        {
            return;
        }

        _generatedRoot.Children.Clear();
        _generatedRoot.Orientation = Orientation.Vertical;

        if (HasHeader)
        {
            _generatedHeader.Title = Title;
            _generatedHeader.Subtitle = Subtitle;
            _generatedHeader.Avatar = HeaderAvatar;
            _generatedHeader.Action = HeaderAction;
            _generatedRoot.Children.Add(_generatedHeader);
        }

        if (ShowMedia || MediaSource is not null)
        {
            _generatedMedia.Source = MediaSource;
            _generatedMedia.MediaHeight = MediaHeight;
            _generatedRoot.Children.Add(_generatedMedia);
        }

        if (Body is not null)
        {
            _generatedBody.Child = Body as Control ?? new ContentControl { Content = Body };
            _generatedRoot.Children.Add(_generatedBody);
        }
        else if (!string.IsNullOrWhiteSpace(BodyText))
        {
            _generatedBody.Child = new Text
            {
                Text = BodyText,
                Typo = Typo.Body2,
                Color = LoamColor.Secondary,
                TextWrapping = TextWrapping.Wrap,
            };
            _generatedRoot.Children.Add(_generatedBody);
        }

        if (HasActions)
        {
            _generatedActionRow.Children.Clear();
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

            _generatedActions.Child = _generatedActionRow;
            _generatedRoot.Children.Add(_generatedActions);
        }

        _usingGeneratedContent = true;
        _hasCustomContent = false;
        _updatingContent = true;
        try
        {
            Content = _generatedRoot;
        }
        finally
        {
            _updatingContent = false;
        }
    }

    private bool HasHeader =>
        HeaderAvatar is not null ||
        HeaderAction is not null ||
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Subtitle);

    private bool HasGeneratedContent =>
        _usingGeneratedContent ||
        HasHeader ||
        Body is not null ||
        !string.IsNullOrWhiteSpace(BodyText) ||
        ShowMedia ||
        MediaSource is not null ||
        HasActions;

    private bool HasActions =>
        Actions.Count > 0 ||
        !string.IsNullOrWhiteSpace(PrimaryActionText) ||
        !string.IsNullOrWhiteSpace(SecondaryActionText);
}

/// <summary>The padded body of a <see cref="Card"/>, mirroring the reference API's <c>CardContent</c>.</summary>
public class CardContent : Decorator
{
    /// <summary>Creates the content area with default padding.</summary>
    public CardContent() => Padding = new Thickness(16);
}

/// <summary>The action bar of a <see cref="Card"/> (host buttons here), mirroring the reference API's <c>CardActions</c>.</summary>
public class CardActions : Decorator
{
    /// <summary>Creates the actions area with default padding.</summary>
    public CardActions() => Padding = new Thickness(8);
}
