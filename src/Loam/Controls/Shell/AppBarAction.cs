using Loam;

namespace Loam.Controls;

/// <summary>A simple icon action rendered by <see cref="AppBar"/>.</summary>
public sealed class AppBarAction
{
    /// <summary>Icon path data.</summary>
    public string? Icon { get; init; }

    /// <summary>Accessible label for the action.</summary>
    public string? Label { get; init; }

    /// <summary>Whether the generated action button is enabled.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>Action button variant.</summary>
    public Variant Variant { get; init; } = Variant.Text;

    /// <summary>Action button color.</summary>
    public LoamColor Color { get; init; } = LoamColor.Inherit;

    /// <summary>Action button size.</summary>
    public LoamSize Size { get; init; } = LoamSize.Medium;

    /// <summary>Invoked when the action button is clicked.</summary>
    public Action? OnClick { get; init; }
}
