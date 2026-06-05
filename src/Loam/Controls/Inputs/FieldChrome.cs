using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Loam.Internal.Templating;
using Loam.Theming;

namespace Loam.Controls;

internal static class FieldChrome
{
    private static readonly string[] InnerTextBoxBrushKeys =
    [
        "TextControlBackground",
        "TextControlBackgroundPointerOver",
        "TextControlBackgroundFocused",
        "TextControlBackgroundDisabled",
        "TextControlBorderBrush",
        "TextControlBorderBrushPointerOver",
        "TextControlBorderBrushFocused",
        "TextControlBorderBrushDisabled",
        "TextBoxBackground",
        "TextBoxBackgroundPointerOver",
        "TextBoxBackgroundFocused",
        "TextBoxBorderBrush",
        "TextBoxBorderBrushPointerOver",
        "TextBoxBorderBrushFocused",
    ];

    public static void ResetInnerTextBox(TextBox textBox)
    {
        foreach (var key in InnerTextBoxBrushKeys)
        {
            textBox.Resources[key] = Brushes.Transparent;
        }

        textBox.PointerEntered -= OnInnerTextBoxVisualStateChanged;
        textBox.PointerExited -= OnInnerTextBoxVisualStateChanged;
        textBox.GotFocus -= OnInnerTextBoxVisualStateChanged;
        textBox.LostFocus -= OnInnerTextBoxVisualStateChanged;
        textBox.PointerEntered += OnInnerTextBoxVisualStateChanged;
        textBox.PointerExited += OnInnerTextBoxVisualStateChanged;
        textBox.GotFocus += OnInnerTextBoxVisualStateChanged;
        textBox.LostFocus += OnInnerTextBoxVisualStateChanged;

        ApplyInnerTextBoxChrome(textBox);
    }

    private static void ApplyInnerTextBoxChrome(TextBox textBox)
    {
        textBox.Background = Brushes.Transparent;
        textBox.BorderBrush = Brushes.Transparent;
        textBox.BorderThickness = default;
        textBox.FocusAdorner = null;
        textBox.Padding = default;
        textBox.MinHeight = 24;
        textBox.ApplyTemplate();

        foreach (var border in textBox.GetVisualDescendants().OfType<Border>().Where(b => b.Name == "PART_BorderElement"))
        {
            border.Background = Brushes.Transparent;
            border.BorderBrush = Brushes.Transparent;
            border.BorderThickness = default;
            border.Padding = default;
        }
    }

    private static void OnInnerTextBoxVisualStateChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            Dispatcher.UIThread.Post(() => ApplyInnerTextBoxChrome(textBox), DispatcherPriority.Render);
        }
    }

    public static Border BuildLabelHost(Text label, Control owner, Avalonia.Controls.INameScope scope)
    {
        label.Margin = default;
        var metrics = ReadFieldMetrics(owner);

        var host = new Border
        {
            Child = label,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false,
            IsVisible = false,
            Margin = new Thickness(metrics.LabelX, 0, 0, 0),
            Padding = new Thickness(metrics.FloatingLabelHorizontalPadding, 0),
        }.Named("PART_LabelHost", scope);

        host.Bind(Border.BackgroundProperty, owner.GetResourceObservable(LoamTokens.ColorSurfaceContainer));
        return host;
    }

    public static void ApplyLabelLayout(Control owner, Border? inputBorder, Border? labelHost, bool showLabel)
    {
        var metrics = ReadFieldMetrics(owner);
        if (labelHost is not null)
        {
            labelHost.IsVisible = showLabel;
            labelHost.Margin = new Thickness(metrics.LabelX, 0, 0, 0);
            labelHost.Padding = new Thickness(metrics.FloatingLabelHorizontalPadding, 0);
        }

        if (inputBorder is not null)
        {
            inputBorder.Margin = showLabel ? new Thickness(0, metrics.FloatingLabelTopMargin, 0, 0) : default;
        }
    }

    public static void Apply(
        Control host,
        Border inputBorder,
        Variant variant,
        LoamColor color,
        bool error,
        bool focused,
        bool enabled,
        ref IDisposable? borderBrush,
        ref IDisposable? background,
        Thickness? textPadding = null,
        Thickness? filledPadding = null,
        Thickness? outlinedPadding = null)
    {
        var metrics = ReadFieldMetrics(host);
        var shape = ReadShape(host);
        var disabledOpacity = ResourceOrDefault(host, LoamTokens.StateDisabledOpacity, LoamStateLayer.Default.DisabledOpacity);

        host.Opacity = enabled ? 1 : disabledOpacity;

        var paletteName = color.ToPaletteName();
        var accent = paletteName is null ? LoamTokens.Primary : LoamTokens.Palette(paletteName);
        var brushKey = error ? LoamTokens.Error
            : focused ? accent
            : LoamTokens.ColorOutline;
        var outlineWidth = focused || error ? metrics.ActiveOutlineWidth : metrics.OutlineWidth;

        borderBrush?.Dispose();
        borderBrush = inputBorder.Bind(Border.BorderBrushProperty, host.GetResourceObservable(brushKey));

        background?.Dispose();
        background = null;

        switch (variant)
        {
            case Variant.Filled:
                inputBorder.MinHeight = metrics.FilledHeight;
                inputBorder.CornerRadius = new CornerRadius(
                    shape.ExtraSmall.TopLeft,
                    shape.ExtraSmall.TopRight,
                    0,
                    0);
                inputBorder.BorderThickness = new Thickness(0, 0, 0, outlineWidth);
                inputBorder.Padding = filledPadding ?? metrics.FilledPadding;
                background = inputBorder.Bind(Border.BackgroundProperty,
                    host.GetResourceObservable(LoamTokens.ColorSurfaceContainerHighest));
                break;
            case Variant.Text:
                inputBorder.MinHeight = metrics.TextHeight;
                inputBorder.CornerRadius = default;
                inputBorder.BorderThickness = new Thickness(0, 0, 0, outlineWidth);
                inputBorder.Padding = textPadding ?? metrics.TextPadding;
                inputBorder.Background = Brushes.Transparent;
                break;
            default:
                inputBorder.MinHeight = metrics.OutlinedHeight;
                inputBorder.CornerRadius = shape.ExtraSmall;
                inputBorder.BorderThickness = new Thickness(outlineWidth);
                inputBorder.Padding = outlinedPadding ?? metrics.OutlinedPadding;
                inputBorder.Background = Brushes.Transparent;
                break;
        }
    }

    private static LoamFieldMetrics ReadFieldMetrics(Control host)
    {
        var defaults = LoamFieldMetrics.Default;
        return defaults with
        {
            OutlinedHeight = ResourceOrDefault(host, LoamTokens.FieldOutlinedHeight, defaults.OutlinedHeight),
            FilledHeight = ResourceOrDefault(host, LoamTokens.FieldFilledHeight, defaults.FilledHeight),
            TextHeight = ResourceOrDefault(host, LoamTokens.FieldTextHeight, defaults.TextHeight),
            OutlineWidth = ResourceOrDefault(host, LoamTokens.FieldOutlineWidth, defaults.OutlineWidth),
            ActiveOutlineWidth = ResourceOrDefault(host, LoamTokens.FieldActiveOutlineWidth, defaults.ActiveOutlineWidth),
            OutlinedPadding = ResourceOrDefault(host, LoamTokens.FieldOutlinedPadding, defaults.OutlinedPadding),
            FilledPadding = ResourceOrDefault(host, LoamTokens.FieldFilledPadding, defaults.FilledPadding),
            TextPadding = ResourceOrDefault(host, LoamTokens.FieldTextPadding, defaults.TextPadding),
            LabelX = ResourceOrDefault(host, LoamTokens.FieldLabelX, defaults.LabelX),
            FloatingLabelTopMargin = ResourceOrDefault(host, LoamTokens.FieldFloatingLabelTopMargin, defaults.FloatingLabelTopMargin),
            FloatingLabelHorizontalPadding = ResourceOrDefault(
                host,
                LoamTokens.FieldFloatingLabelHorizontalPadding,
                defaults.FloatingLabelHorizontalPadding),
            IconSpacing = ResourceOrDefault(host, LoamTokens.FieldIconSpacing, defaults.IconSpacing),
            HelperTopSpacing = ResourceOrDefault(host, LoamTokens.FieldHelperTopSpacing, defaults.HelperTopSpacing),
        };
    }

    private static LoamShape ReadShape(Control host)
    {
        var defaults = LoamShape.Default;
        return defaults with
        {
            ExtraSmall = ResourceOrDefault(host, LoamTokens.ShapeExtraSmall, defaults.ExtraSmall),
            Small = ResourceOrDefault(host, LoamTokens.ShapeSmall, defaults.Small),
        };
    }

    private static T ResourceOrDefault<T>(Control host, string key, T fallback)
    {
        return host.TryGetResource(key, host.ActualThemeVariant, out var value) && value is T typed
            ? typed
            : fallback;
    }
}
