using Loam;

namespace Loam.Theming;

/// <summary>
/// Responsive breakpoint thresholds (device-independent pixels), mirroring MudBlazor's defaults.
/// Loam evaluates breakpoints from a control's own available width (container-query style) rather
/// than a global viewport — self-contained and more reusable.
/// </summary>
public static class Breakpoints
{
    /// <summary>Small lower bound.</summary>
    public const double Sm = 600;
    /// <summary>Medium lower bound.</summary>
    public const double Md = 960;
    /// <summary>Large lower bound.</summary>
    public const double Lg = 1280;
    /// <summary>Extra-large lower bound.</summary>
    public const double Xl = 1920;
    /// <summary>Extra-extra-large lower bound.</summary>
    public const double Xxl = 2560;

    /// <summary>The breakpoint that a width falls into.</summary>
    public static Breakpoint FromWidth(double width) =>
        width >= Xxl ? Breakpoint.Xxl :
        width >= Xl ? Breakpoint.Xl :
        width >= Lg ? Breakpoint.Lg :
        width >= Md ? Breakpoint.Md :
        width >= Sm ? Breakpoint.Sm :
        Breakpoint.Xs;

    /// <summary>The max content width (px) for a container capped at a breakpoint; infinite for None/Always.</summary>
    public static double MaxWidth(Breakpoint breakpoint) => breakpoint switch
    {
        Breakpoint.Xs => Sm,
        Breakpoint.Sm => Sm,
        Breakpoint.Md => Md,
        Breakpoint.Lg => Lg,
        Breakpoint.Xl => Xl,
        Breakpoint.Xxl => Xxl,
        _ => double.PositiveInfinity,
    };
}
