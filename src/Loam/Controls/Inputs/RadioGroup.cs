using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Loam.Controls;

/// <summary>
/// Coordinates a set of <see cref="Radio"/> options and binds the selected one's
/// <see cref="Radio.Value"/>, mirroring the reference API's <c>RadioGroup</c>. Put the radios inside its
/// <see cref="Avalonia.Controls.Decorator.Child"/>; <see cref="Value"/> tracks the chosen option.
/// </summary>
public class RadioGroup : Decorator
{
    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<RadioGroup, object?>(nameof(Value), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>The selected option's value.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        foreach (var radio in this.GetVisualDescendants().OfType<Radio>())
        {
            radio.IsCheckedChanged -= OnRadioChecked;
            radio.IsCheckedChanged += OnRadioChecked;
        }

        SyncChecked();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            SyncChecked();
        }
    }

    private void OnRadioChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is Radio radio && radio.IsChecked == true)
        {
            Value = radio.Value;
        }
    }

    private void SyncChecked()
    {
        foreach (var radio in this.GetVisualDescendants().OfType<Radio>())
        {
            radio.IsChecked = Equals(radio.Value, Value);
        }
    }
}
