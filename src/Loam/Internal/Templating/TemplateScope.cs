using Avalonia;
using Avalonia.Controls;

namespace Loam.Internal.Templating;

/// <summary>
/// Thin, project-local fluent helpers for authoring control templates in pure C#
/// (ADR-0002). These wrap official Avalonia APIs only — no hidden concepts.
/// </summary>
internal static class TemplateScope
{
    /// <summary>
    /// Assigns a name to a template element and registers it in the template name scope so the
    /// owning control (and <see cref="Avalonia.Controls.Primitives.TemplatedControl.OnApplyTemplate"/>)
    /// can resolve it by name.
    /// </summary>
    public static T Named<T>(this T element, string name, INameScope scope)
        where T : StyledElement
    {
        element.Name = name;
        scope.Register(name, element);
        return element;
    }
}
