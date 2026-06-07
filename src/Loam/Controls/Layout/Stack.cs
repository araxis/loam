using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Loam.Controls;

/// <summary>
/// A spaced stack of children, mirroring the reference API's <c>Stack</c>. Vertical by default;
/// <see cref="Row"/> lays out horizontally. Spacing reuses <see cref="StackPanel.Spacing"/>.
/// </summary>
[Obsolete(
    "Loam.Controls.Stack is a thin wrapper over Avalonia.Controls.StackPanel and will be removed in a " +
    "future release. Use StackPanel (set Orientation and Spacing). See https://araxis.github.io/loam/migration/v2-to-v3.",
    error: false,
    DiagnosticId = "LOAM0003",
    UrlFormat = "https://araxis.github.io/loam/migration/v2-to-v3")]
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Mirrors the reference API's Stack.")]
public class Stack : StackPanel
{
    /// <summary>Identifies the <see cref="Row"/> property.</summary>
    public static readonly StyledProperty<bool> RowProperty =
        AvaloniaProperty.Register<Stack, bool>(nameof(Row));

    /// <summary>Creates the stack with a sensible default spacing.</summary>
    public Stack()
    {
        Spacing = 8;
        AutomationProperties.SetName(this, "Stack");
    }

    /// <summary>Lays out horizontally instead of vertically. Mirrors the reference API's <c>Row</c>.</summary>
    public bool Row
    {
        get => GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RowProperty)
        {
            Orientation = Row ? Orientation.Horizontal : Orientation.Vertical;
        }
    }
}
