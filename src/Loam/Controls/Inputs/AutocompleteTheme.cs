using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Loam.Internal.Templating;

namespace Loam.Controls;

/// <summary>Builds the <see cref="Autocomplete"/> theme: a single <see cref="TextField"/> (<c>PART_Field</c>) whose label/placeholder/variant/color forward from the autocomplete; the suggestion popup is driven by the control.</summary>
internal static class AutocompleteTheme
{
    public static ControlTheme Create() =>
        new(typeof(Autocomplete))
        {
            Setters = { new Setter(TemplatedControl.TemplateProperty, BuildTemplate()) },
        };

    private static FuncControlTemplate<Autocomplete> BuildTemplate() =>
        new((autocomplete, scope) =>
        {
            var field = new TextField().Named("PART_Field", scope);
            field.Bind(TextField.LabelProperty, autocomplete.GetObservable(Autocomplete.LabelProperty));
            field.Bind(TextField.PlaceholderProperty, autocomplete.GetObservable(Autocomplete.PlaceholderProperty));
            field.Bind(TextField.VariantProperty, autocomplete.GetObservable(Autocomplete.VariantProperty));
            field.Bind(TextField.ColorProperty, autocomplete.GetObservable(Autocomplete.ColorProperty));
            field.Bind(TextField.HelperTextProperty, autocomplete.GetObservable(Autocomplete.HelperTextProperty));
            field.Bind(TextField.ErrorTextProperty, autocomplete.GetObservable(Autocomplete.ErrorTextProperty));
            field.Bind(TextField.ErrorProperty, autocomplete.GetObservable(Autocomplete.ErrorProperty));
            field.Bind(TextField.ShrinkLabelProperty, autocomplete.GetObservable(Autocomplete.ShrinkLabelProperty));

            var popup = new Popup
            {
                IsLightDismissEnabled = true,
                OverlayDismissEventPassThrough = true,
                Placement = PlacementMode.BottomEdgeAlignedLeft,
                PlacementTarget = field,
            }.Named("PART_Popup", scope);

            return new StackPanel { Children = { field, popup } };
        });
}
