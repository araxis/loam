using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Loam;
using Loam.Controls.Internal;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>Presentation options for <see cref="Tooltip"/>.</summary>
public sealed record TooltipOptions
{
    /// <summary>Optional title shown above the tooltip text.</summary>
    public string? Title { get; init; }

    /// <summary>Tooltip elevation.</summary>
    public int Elevation { get; init; } = 4;

    /// <summary>Tooltip padding.</summary>
    public Thickness Padding { get; init; } = new(8, 4);

    /// <summary>Text typography role.</summary>
    public Typo Typo { get; init; } = Typo.Caption;

    /// <summary>Text color.</summary>
    public LoamColor Color { get; init; } = LoamColor.Default;

    /// <summary>Help text exposed for automation. Defaults to the tooltip text.</summary>
    public string? HelpText { get; init; }

    /// <summary>Optional popup placement. When unset, Avalonia keeps its default tooltip placement.</summary>
    public PlacementMode? Placement { get; init; }

    /// <summary>Optional horizontal popup offset.</summary>
    public double? HorizontalOffset { get; init; }

    /// <summary>Optional vertical popup offset.</summary>
    public double? VerticalOffset { get; init; }

    /// <summary>Optional delay, in milliseconds, before the tooltip opens.</summary>
    public int? ShowDelay { get; init; }

    /// <summary>Optional delay, in milliseconds, between quickly shown tooltips.</summary>
    public int? BetweenShowDelay { get; init; }

    /// <summary>Whether the tooltip can show when the target control is disabled. When unset, Avalonia keeps its default.</summary>
    public bool? ShowOnDisabled { get; init; }

    /// <summary>Enables or disables the Avalonia tooltip service for the target.</summary>
    public bool ServiceEnabled { get; init; } = true;
}

/// <summary>
/// Attaches a Loam-styled tooltip to a control, mirroring the reference API's <c>Tooltip</c>. Wraps
/// Avalonia's <see cref="ToolTip"/> with a small elevated <see cref="Paper"/>.
/// </summary>
public static class Tooltip
{
    /// <summary>Sets a text tooltip on <paramref name="control"/>.</summary>
    public static void Set(Control control, string text) => Set(control, text, new TooltipOptions());

    /// <summary>Sets a text tooltip with presentation options on <paramref name="control"/>.</summary>
    public static void Set(Control control, string text, TooltipOptions options)
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(options);

        AutomationProperties.SetHelpText(control, options.HelpText ?? text);
        Control content = new Text { Text = text, Typo = options.Typo, Color = options.Color };
        if (!string.IsNullOrWhiteSpace(options.Title))
        {
            content = new StackPanel
            {
                Spacing = 2,
                Children =
                {
                    new Text { Text = options.Title, Typo = Typo.LabelMedium, Color = options.Color },
                    content,
                },
            };
        }

        var paper = new Paper
        {
            Elevation = options.Elevation,
            Padding = options.Padding,
            Content = content,
        };
        InteractionAssist.ApplyZIndex(paper, LoamTokens.ZIndex(nameof(LoamZIndex.Tooltip)), LoamZIndex.Default.Tooltip);
        ToolTip.SetTip(control, paper);
        ApplyOptions(control, options);
    }

    /// <summary>Removes the tooltip and related automation help text from <paramref name="control"/>.</summary>
    public static void Clear(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);

        ToolTip.SetIsOpen(control, false);
        ToolTip.SetTip(control, null);
        control.ClearValue(AutomationProperties.HelpTextProperty);
    }

    private static void ApplyOptions(Control control, TooltipOptions options)
    {
        if (options.Placement is { } placement)
        {
            ToolTip.SetPlacement(control, placement);
        }

        if (options.HorizontalOffset is { } horizontalOffset)
        {
            ToolTip.SetHorizontalOffset(control, horizontalOffset);
        }

        if (options.VerticalOffset is { } verticalOffset)
        {
            ToolTip.SetVerticalOffset(control, verticalOffset);
        }

        if (options.ShowDelay is { } showDelay)
        {
            ToolTip.SetShowDelay(control, showDelay);
        }

        if (options.BetweenShowDelay is { } betweenShowDelay)
        {
            ToolTip.SetBetweenShowDelay(control, betweenShowDelay);
        }

        if (options.ShowOnDisabled is { } showOnDisabled)
        {
            ToolTip.SetShowOnDisabled(control, showOnDisabled);
        }

        ToolTip.SetServiceEnabled(control, options.ServiceEnabled);
        if (!options.ServiceEnabled)
        {
            ToolTip.SetIsOpen(control, false);
        }
    }
}
