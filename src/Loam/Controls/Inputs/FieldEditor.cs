using System;
using Avalonia.Controls;

namespace Loam.Controls;

/// <summary>Helpers for composing custom editors inside a <see cref="Field"/>.</summary>
public static class FieldEditor
{
    /// <summary>
    /// Removes Avalonia's native <see cref="TextBox"/> background, border, and focus adorner so the
    /// surrounding <see cref="Field"/> owns the input chrome.
    /// </summary>
    /// <param name="textBox">The text box hosted inside a field-style control.</param>
    /// <returns>The same <paramref name="textBox"/> instance for fluent object construction.</returns>
    public static TextBox MakeChromeless(TextBox textBox)
    {
        ArgumentNullException.ThrowIfNull(textBox);
        FieldChrome.ResetInnerTextBox(textBox);
        return textBox;
    }
}
