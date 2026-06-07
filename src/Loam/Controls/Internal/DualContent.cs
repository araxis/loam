using System.Diagnostics;

namespace Loam.Controls.Internal;

/// <summary>
/// Helpers for the dual-mode controls that flip between a generated anatomy (built from typed
/// properties like <c>Title</c>/<c>Body</c>) and a custom inherited <c>Content</c> — e.g.
/// <see cref="Paper"/>, <see cref="Card"/>, <see cref="Drawer"/>.
/// </summary>
/// <remarks>
/// Precedence is fixed and explicit: <b>custom <c>Content</c> always wins</b>. If you set both the
/// inherited <c>Content</c> and any generated-anatomy property on the same instance, the generated
/// properties are ignored. Use one mode or the other.
/// </remarks>
internal static class DualContent
{
    /// <summary>
    /// Debug-only diagnostic that warns when an instance has both custom <c>Content</c> and
    /// generated-anatomy properties set (an ambiguous combination). Compiled out entirely in Release —
    /// the call and its argument evaluation are elided by <see cref="ConditionalAttribute"/>.
    /// </summary>
    [Conditional("DEBUG")]
    public static void WarnIfConflicting(bool hasCustomContent, bool hasGeneratedContent, string control)
    {
        if (hasCustomContent && hasGeneratedContent)
        {
            Debug.WriteLine(
                $"[Loam] {control}: both custom Content and generated-anatomy properties are set. " +
                "Custom Content takes precedence and the generated properties are ignored — set one or the other.");
        }
    }
}
