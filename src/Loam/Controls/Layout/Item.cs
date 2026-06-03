using Avalonia;
using Avalonia.Controls;
using Loam;

namespace Loam.Controls;

/// <summary>
/// A child of <see cref="Grid"/> that spans a number of the 12 columns per breakpoint, mirroring
/// MudBlazor's <c>MudItem</c> (PascalCase span props here vs MudBlazor's lowercase). Holds a single
/// <see cref="Avalonia.Controls.Decorator.Child"/>. A span resolves to the value at the current
/// breakpoint or the nearest smaller one that is set; unset defaults to a full row (12).
/// </summary>
public class Item : Decorator
{
    /// <summary>Identifies the <see cref="Xs"/> property.</summary>
    public static readonly StyledProperty<int> XsProperty = AvaloniaProperty.Register<Item, int>(nameof(Xs));

    /// <summary>Identifies the <see cref="Sm"/> property.</summary>
    public static readonly StyledProperty<int> SmProperty = AvaloniaProperty.Register<Item, int>(nameof(Sm));

    /// <summary>Identifies the <see cref="Md"/> property.</summary>
    public static readonly StyledProperty<int> MdProperty = AvaloniaProperty.Register<Item, int>(nameof(Md));

    /// <summary>Identifies the <see cref="Lg"/> property.</summary>
    public static readonly StyledProperty<int> LgProperty = AvaloniaProperty.Register<Item, int>(nameof(Lg));

    /// <summary>Identifies the <see cref="Xl"/> property.</summary>
    public static readonly StyledProperty<int> XlProperty = AvaloniaProperty.Register<Item, int>(nameof(Xl));

    /// <summary>Identifies the <see cref="Xxl"/> property.</summary>
    public static readonly StyledProperty<int> XxlProperty = AvaloniaProperty.Register<Item, int>(nameof(Xxl));

    static Item() =>
        // Invalidating the item's measure bubbles up to re-run the parent Grid's layout.
        AffectsMeasure<Item>(XsProperty, SmProperty, MdProperty, LgProperty, XlProperty, XxlProperty);

    /// <summary>Columns at the extra-small breakpoint (1–12).</summary>
    public int Xs { get => GetValue(XsProperty); set => SetValue(XsProperty, value); }

    /// <summary>Columns at the small breakpoint (1–12).</summary>
    public int Sm { get => GetValue(SmProperty); set => SetValue(SmProperty, value); }

    /// <summary>Columns at the medium breakpoint (1–12).</summary>
    public int Md { get => GetValue(MdProperty); set => SetValue(MdProperty, value); }

    /// <summary>Columns at the large breakpoint (1–12).</summary>
    public int Lg { get => GetValue(LgProperty); set => SetValue(LgProperty, value); }

    /// <summary>Columns at the extra-large breakpoint (1–12).</summary>
    public int Xl { get => GetValue(XlProperty); set => SetValue(XlProperty, value); }

    /// <summary>Columns at the extra-extra-large breakpoint (1–12).</summary>
    public int Xxl { get => GetValue(XxlProperty); set => SetValue(XxlProperty, value); }

    /// <summary>The effective column span at a breakpoint (1–12; 12 if nothing is set).</summary>
    public int ResolveSpan(Breakpoint breakpoint)
    {
        var value = breakpoint switch
        {
            Breakpoint.Xxl => Pick(Xxl, Xl, Lg, Md, Sm, Xs),
            Breakpoint.Xl => Pick(Xl, Lg, Md, Sm, Xs),
            Breakpoint.Lg => Pick(Lg, Md, Sm, Xs),
            Breakpoint.Md => Pick(Md, Sm, Xs),
            Breakpoint.Sm => Pick(Sm, Xs),
            _ => Pick(Xs),
        };

        return value > 0 ? Math.Clamp(value, 1, 12) : 12;
    }

    private static int Pick(params int[] values)
    {
        foreach (var value in values)
        {
            if (value > 0)
            {
                return value;
            }
        }

        return 0;
    }
}
