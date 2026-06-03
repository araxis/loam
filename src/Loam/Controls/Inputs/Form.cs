using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Loam.Controls;

/// <summary>
/// A lightweight form container, mirroring MudBlazor's <c>MudForm</c>. Put your inputs inside its
/// <see cref="Avalonia.Controls.Decorator.Child"/>; <see cref="Validate"/> runs each
/// <see cref="TextField"/>'s validation (Required + custom validator) and updates <see cref="IsValid"/>.
/// </summary>
public class Form : Decorator
{
    /// <summary>Identifies the <see cref="IsValid"/> property.</summary>
    public static readonly StyledProperty<bool> IsValidProperty =
        AvaloniaProperty.Register<Form, bool>(nameof(IsValid), defaultValue: true);

    /// <summary>Whether all fields passed validation at the last <see cref="Validate"/>.</summary>
    public bool IsValid
    {
        get => GetValue(IsValidProperty);
        private set => SetValue(IsValidProperty, value);
    }

    /// <summary>Validates every <see cref="TextField"/> in the form and returns whether all are valid.</summary>
    public bool Validate()
    {
        var valid = true;
        foreach (var field in this.GetVisualDescendants().OfType<TextField>())
        {
            if (field.Validate() is not null)
            {
                valid = false;
            }
        }

        IsValid = valid;
        return valid;
    }
}
