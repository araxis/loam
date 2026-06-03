using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam.Theming;

namespace Loam.Gallery;

/// <summary>
/// Phase 2 showcase: renders the Loam design system (palette, typography, elevation) entirely from
/// theme tokens, and drives runtime theming — a light/dark toggle plus live primary-color presets
/// that call <see cref="LoamTheme.SetPrimary"/>.
/// </summary>
public sealed class DesignSystemView : UserControl
{
    public DesignSystemView()
    {
        var root = new DockPanel();
        root.Children.Add(BuildToolbar());
        root.Children.Add(BuildBody());

        var background = new Border { Child = root };
        background.Bind(Border.BackgroundProperty, background.GetResourceObservable(LoamTokens.Background));
        Content = background;
    }

    private static Border BuildToolbar()
    {
        var title = new TextBlock
        {
            Text = "Loam Design System",
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 18,
            FontWeight = FontWeight.Medium,
        };
        title.Bind(TextBlock.ForegroundProperty,
            title.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.AppbarText))));

        var presets = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Spacing = 6 };
        foreach (var hex in new[] { "#594AE2", "#2196F3", "#00C853", "#F44336", "#FF9800", "#9C27B0" })
        {
            presets.Children.Add(PrimaryPreset(Color.Parse(hex)));
        }

        var toggle = new Button { Content = "Toggle light / dark", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
        toggle.Click += (_, _) => ToggleVariant();

        var grid = new Avalonia.Controls.Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"), Margin = new Thickness(16, 0) };
        Avalonia.Controls.Grid.SetColumn(title, 0);
        Avalonia.Controls.Grid.SetColumn(presets, 1);
        Avalonia.Controls.Grid.SetColumn(toggle, 2);
        grid.Children.Add(title);
        grid.Children.Add(presets);
        grid.Children.Add(toggle);

        var bar = new Border { Height = 64, Child = grid };
        bar.Bind(Border.BackgroundProperty, bar.GetResourceObservable(LoamTokens.Palette(nameof(LoamPalette.AppbarBackground))));
        bar.Bind(Border.BoxShadowProperty, bar.GetResourceObservable(LoamTokens.Elevation(4)));
        DockPanel.SetDock(bar, Dock.Top);
        return bar;
    }

    private static ScrollViewer BuildBody()
    {
        var stack = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 28,
            Children =
            {
                Section("Palette", BuildPalette()),
                Section("Typography", BuildTypography()),
                Section("Elevation", BuildElevation()),
            },
        };
        return new ScrollViewer { Content = stack };
    }

    private static StackPanel Section(string title, Control body)
    {
        var heading = new TextBlock { Text = title, FontSize = 20, FontWeight = FontWeight.Medium, Margin = new Thickness(0, 0, 0, 12) };
        heading.Bind(TextBlock.ForegroundProperty, heading.GetResourceObservable(LoamTokens.TextPrimary));
        return new StackPanel { Children = { heading, body } };
    }

    private static WrapPanel BuildPalette()
    {
        var wrap = new WrapPanel();
        foreach (var name in new[]
                 {
                     "Primary", "Secondary", "Tertiary", "Info", "Success", "Warning",
                     "Error", "Dark", "Surface", "Background", "AppbarBackground",
                 })
        {
            wrap.Children.Add(Swatch(name));
        }

        return wrap;
    }

    private static StackPanel Swatch(string paletteName)
    {
        var box = new Border
        {
            Width = 108,
            Height = 56,
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(1),
        };
        box.Bind(Border.BackgroundProperty, box.GetResourceObservable(LoamTokens.Palette(paletteName)));
        box.Bind(Border.BorderBrushProperty, box.GetResourceObservable(LoamTokens.Divider));

        var label = new TextBlock { Text = paletteName, FontSize = 12, Margin = new Thickness(0, 4, 0, 0) };
        label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable(LoamTokens.TextSecondary));

        return new StackPanel { Width = 108, Margin = new Thickness(0, 0, 12, 12), Children = { box, label } };
    }

    private static StackPanel BuildTypography()
    {
        var stack = new StackPanel { Spacing = 2 };
        foreach (var (name, _) in LoamThemeData.Default.Typography.Scales)
        {
            stack.Children.Add(TypeSample(name));
        }

        return stack;
    }

    private static TextBlock TypeSample(string name)
    {
        var sample = new TextBlock
        {
            Text = $"{name} · The quick brown fox jumps over the lazy dog",
            Margin = new Thickness(0, 2, 0, 2),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        sample.Bind(TextBlock.FontSizeProperty, sample.GetResourceObservable(LoamTokens.TypographyFontSize(name)));
        sample.Bind(TextBlock.FontWeightProperty, sample.GetResourceObservable(LoamTokens.TypographyFontWeight(name)));
        sample.Bind(TextBlock.FontFamilyProperty, sample.GetResourceObservable(LoamTokens.FontFamily));
        sample.Bind(TextBlock.ForegroundProperty, sample.GetResourceObservable(LoamTokens.TextPrimary));
        return sample;
    }

    private static WrapPanel BuildElevation()
    {
        var wrap = new WrapPanel();
        foreach (var level in new[] { 0, 1, 2, 3, 4, 6, 8, 12, 16, 24 })
        {
            wrap.Children.Add(ElevationSample(level));
        }

        return wrap;
    }

    private static Border ElevationSample(int level)
    {
        var box = new Border { Width = 84, Height = 64, Margin = new Thickness(0, 8, 16, 8), CornerRadius = new CornerRadius(4) };
        box.Bind(Border.BackgroundProperty, box.GetResourceObservable(LoamTokens.Surface));
        box.Bind(Border.BoxShadowProperty, box.GetResourceObservable(LoamTokens.Elevation(level)));

        var label = new TextBlock { Text = $"e{level}", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
        label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable(LoamTokens.TextSecondary));
        box.Child = label;
        return box;
    }

    private static Border PrimaryPreset(Color color)
    {
        var swatch = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(color),
            Cursor = new Cursor(StandardCursorType.Hand),
        };
        swatch.PointerPressed += (_, _) => CurrentLoamTheme()?.SetPrimary(color);
        return swatch;
    }

    private static void ToggleVariant()
    {
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant =
                app.ActualThemeVariant == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
        }
    }

    private static LoamTheme? CurrentLoamTheme() =>
        Application.Current?.Styles.OfType<LoamTheme>().FirstOrDefault();
}
