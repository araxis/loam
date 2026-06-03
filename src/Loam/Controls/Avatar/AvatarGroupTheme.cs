using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="AvatarGroup"/> theme: a horizontal strip (<c>PART_Items</c>) the control fills with overlapping avatars.</summary>
internal static class AvatarGroupTheme
{
    public static ControlTheme Create() =>
        new(typeof(AvatarGroup))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<AvatarGroup> BuildTemplate() =>
        new((group, scope) =>
            new StackPanel { Orientation = Orientation.Horizontal }.Named("PART_Items", scope));
}
