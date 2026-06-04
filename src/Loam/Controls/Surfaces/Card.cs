using Avalonia;
using Avalonia.Controls;

namespace Loam.Controls;

/// <summary>A surface for grouping related content + actions, mirroring the reference API's <c>Card</c>. A <see cref="Paper"/> styled for card composition (use <see cref="CardContent"/>/<see cref="CardActions"/> inside).</summary>
public class Card : Paper
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Paper);
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
