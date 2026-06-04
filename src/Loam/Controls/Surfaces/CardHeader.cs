using Avalonia;
using Avalonia.Controls.Primitives;

namespace Loam.Controls;

/// <summary>
/// The header row of a <see cref="Card"/>, mirroring the reference API's <c>CardHeader</c>. Lays out an
/// optional leading <see cref="Avatar"/>, a <see cref="Title"/>/<see cref="Subtitle"/> stack, and a
/// trailing <see cref="Action"/> (e.g. an icon button).
/// </summary>
public class CardHeader : TemplatedControl
{
    /// <summary>Identifies the <see cref="Avatar"/> property.</summary>
    public static readonly StyledProperty<object?> AvatarProperty =
        AvaloniaProperty.Register<CardHeader, object?>(nameof(Avatar));

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<CardHeader, string?>(nameof(Title));

    /// <summary>Identifies the <see cref="Subtitle"/> property.</summary>
    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<CardHeader, string?>(nameof(Subtitle));

    /// <summary>Identifies the <see cref="Action"/> property.</summary>
    public static readonly StyledProperty<object?> ActionProperty =
        AvaloniaProperty.Register<CardHeader, object?>(nameof(Action));

    /// <summary>A leading visual (typically an <see cref="Avatar"/>). Mirrors the reference API's <c>CardHeaderAvatar</c>.</summary>
    public object? Avatar
    {
        get => GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    /// <summary>The header title. Mirrors the reference API's <c>CardHeaderContent</c> title.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The header subtitle.</summary>
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>A trailing visual (typically an icon button). Mirrors the reference API's <c>CardHeaderActions</c>.</summary>
    public object? Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CardHeader);
}

/// <summary>
/// A media band for a <see cref="Card"/>, mirroring the reference API's <c>CardMedia</c>. Shows
/// <see cref="Source"/> stretched to fill a band of <see cref="MediaHeight"/> pixels (with a neutral
/// placeholder behind it when no image is set).
/// </summary>
public class CardMedia : TemplatedControl
{
    /// <summary>Identifies the <see cref="Source"/> property.</summary>
    public static readonly StyledProperty<Avalonia.Media.IImage?> SourceProperty =
        AvaloniaProperty.Register<CardMedia, Avalonia.Media.IImage?>(nameof(Source));

    /// <summary>Identifies the <see cref="MediaHeight"/> property.</summary>
    public static readonly StyledProperty<double> MediaHeightProperty =
        AvaloniaProperty.Register<CardMedia, double>(nameof(MediaHeight), 180);

    /// <summary>The image to display. Mirrors the reference API's <c>Image</c>.</summary>
    public Avalonia.Media.IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>The media band height in pixels. Mirrors the reference API's <c>Height</c>.</summary>
    public double MediaHeight
    {
        get => GetValue(MediaHeightProperty);
        set => SetValue(MediaHeightProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CardMedia);
}
