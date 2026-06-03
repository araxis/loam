using Avalonia;
using Avalonia.Controls;
using Loam;
using Loam.Theming;

namespace Loam.Controls;

/// <summary>
/// Centers and width-caps its content, mirroring MudBlazor's <c>MudContainer</c>. The cap is a
/// breakpoint (MudBlazor's <c>MaxWidth</c> enum); <see cref="Gutters"/> adds horizontal padding.
/// </summary>
public class Container : Decorator
{
    /// <summary>Identifies the <see cref="MaxWidthBreakpoint"/> property.</summary>
    public static readonly StyledProperty<Breakpoint> MaxWidthBreakpointProperty =
        AvaloniaProperty.Register<Container, Breakpoint>(nameof(MaxWidthBreakpoint), Breakpoint.Lg);

    /// <summary>Identifies the <see cref="Gutters"/> property.</summary>
    public static readonly StyledProperty<bool> GuttersProperty =
        AvaloniaProperty.Register<Container, bool>(nameof(Gutters), true);

    private const double GutterSize = 16;

    static Container() =>
        AffectsMeasure<Container>(MaxWidthBreakpointProperty, GuttersProperty);

    /// <summary>The breakpoint the content width is capped at. Mirrors MudBlazor's <c>MaxWidth</c>.</summary>
    public Breakpoint MaxWidthBreakpoint
    {
        get => GetValue(MaxWidthBreakpointProperty);
        set => SetValue(MaxWidthBreakpointProperty, value);
    }

    /// <summary>Adds horizontal padding around the content. Mirrors MudBlazor's <c>Gutters</c>.</summary>
    public bool Gutters
    {
        get => GetValue(GuttersProperty);
        set => SetValue(GuttersProperty, value);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Child is null)
        {
            return default;
        }

        var gutter = Gutters ? GutterSize : 0;
        var contentWidth = ContentWidth(availableSize.Width);
        Child.Measure(new Size(Math.Max(0, contentWidth - (gutter * 2)), availableSize.Height));

        var width = double.IsInfinity(availableSize.Width)
            ? Child.DesiredSize.Width + (gutter * 2)
            : availableSize.Width;
        return new Size(width, Child.DesiredSize.Height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Child is null)
        {
            return finalSize;
        }

        var gutter = Gutters ? GutterSize : 0;
        var contentWidth = ContentWidth(finalSize.Width);
        var x = Math.Max(0, (finalSize.Width - contentWidth) / 2) + gutter;
        Child.Arrange(new Rect(x, 0, Math.Max(0, contentWidth - (gutter * 2)), finalSize.Height));
        return finalSize;
    }

    private double ContentWidth(double available)
    {
        var max = Breakpoints.MaxWidth(MaxWidthBreakpoint);
        if (double.IsInfinity(available))
        {
            return double.IsInfinity(max) ? available : max;
        }

        return double.IsInfinity(max) ? available : Math.Min(available, max);
    }
}
