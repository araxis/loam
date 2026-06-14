using System.Reflection;
using Loam.Theming;
using Shouldly;
using Xunit;

namespace Loam.Tests;

/// <summary>
/// Guards the 3.1 package split (ADR-0009): the chart, picker, and heavy data controls — plus their
/// registrars — must live in their satellite assemblies, and the core <c>Loam</c> assembly must not
/// carry them. These reflection checks fail fast if a moved type is accidentally re-added to core.
/// </summary>
public class PackagingTests
{
    private static readonly Assembly Core = typeof(LoamTheme).Assembly;

    public static TheoryData<Type, string> MovedTypes() => new()
    {
        // Charts -> Loam.Charts
        { typeof(Loam.Controls.PieChart), "Loam.Charts" },
        { typeof(Loam.Controls.BarChart), "Loam.Charts" },
        { typeof(Loam.Controls.LineChart), "Loam.Charts" },
        { typeof(LoamCharts), "Loam.Charts" },

        // Pickers -> Loam.Pickers
        { typeof(Loam.Controls.DatePicker), "Loam.Pickers" },
        { typeof(Loam.Controls.TimePicker), "Loam.Pickers" },
        { typeof(Loam.Controls.ColorPicker), "Loam.Pickers" },
        { typeof(Loam.Controls.DateRangePicker), "Loam.Pickers" },
        { typeof(Loam.Controls.MonthCalendar), "Loam.Pickers" },
        { typeof(LoamPickers), "Loam.Pickers" },

        // Data -> Loam.Data
        { typeof(Loam.Controls.DataGrid<>), "Loam.Data" },
        { typeof(Loam.Controls.SimpleTable), "Loam.Data" },
        { typeof(Loam.Controls.TreeView), "Loam.Data" },
        { typeof(Loam.Controls.TreeViewItem), "Loam.Data" },
        { typeof(Loam.Controls.Pagination), "Loam.Data" },
        { typeof(LoamData), "Loam.Data" },
    };

    [Theory]
    [MemberData(nameof(MovedTypes))]
    public void Moved_type_lives_in_its_satellite_assembly(Type type, string expectedAssembly)
    {
        type.Assembly.GetName().Name.ShouldBe(expectedAssembly);
    }

    [Theory]
    [MemberData(nameof(MovedTypes))]
    public void Core_assembly_does_not_carry_moved_type(Type type, string expectedAssembly)
    {
        _ = expectedAssembly;
        Core.GetType(type.FullName!).ShouldBeNull(
            $"{type.FullName} must not exist in the core Loam assembly after the package split.");
    }

    [Fact]
    public void Core_keeps_its_own_theme_registrar()
    {
        typeof(LoamTheme).Assembly.GetName().Name.ShouldBe("Loam");
    }
}
