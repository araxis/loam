using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Loam.Theming;

namespace Loam.Gallery;

/// <summary>
/// Design-system showcase: renders Loam foundations entirely from
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

    private static Loam.Controls.AppBar BuildToolbar()
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
        foreach (var hex in new[] { "#6750A4", "#006A6A", "#386A20", "#B3261E", "#765A00", "#7D5260" })
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

        var bar = new Loam.Controls.AppBar { Content = grid, Elevation = 4 };
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
                Section("Color roles", BuildPalette()),
                Section("Typography", BuildTypography()),
                Section("Shape", BuildShape()),
                Section("Spacing and motion", BuildSpacingAndMotion()),
                Section("Tonal elevation", BuildElevation()),
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
                     nameof(LoamColorScheme.Primary), nameof(LoamColorScheme.OnPrimary),
                     nameof(LoamColorScheme.PrimaryContainer), nameof(LoamColorScheme.OnPrimaryContainer),
                     nameof(LoamColorScheme.Secondary), nameof(LoamColorScheme.SecondaryContainer),
                     nameof(LoamColorScheme.Tertiary), nameof(LoamColorScheme.TertiaryContainer),
                     nameof(LoamColorScheme.Error), nameof(LoamColorScheme.ErrorContainer),
                     nameof(LoamColorScheme.Surface), nameof(LoamColorScheme.SurfaceContainer),
                     nameof(LoamColorScheme.SurfaceContainerHigh), nameof(LoamColorScheme.Outline),
                 })
        {
            wrap.Children.Add(Swatch(name));
        }

        return wrap;
    }

    private static StackPanel Swatch(string role)
    {
        var box = new Border
        {
            Width = 108,
            Height = 56,
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
        };
        box.Bind(Border.BackgroundProperty, box.GetResourceObservable(LoamTokens.ColorScheme(role)));
        box.Bind(Border.BorderBrushProperty, box.GetResourceObservable(LoamTokens.ColorOutlineVariant));

        var label = new TextBlock { Text = role, FontSize = 12, Margin = new Thickness(0, 4, 0, 0) };
        label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable(LoamTokens.ColorOnSurfaceVariant));

        return new StackPanel { Width = 108, Margin = new Thickness(0, 0, 12, 12), Children = { box, label } };
    }

    private static StackPanel BuildTypography()
    {
        var stack = new StackPanel { Spacing = 2 };
        foreach (var name in new[]
                 {
                     nameof(LoamTypography.DisplayLarge), nameof(LoamTypography.DisplayMedium),
                     nameof(LoamTypography.DisplaySmall), nameof(LoamTypography.HeadlineLarge),
                     nameof(LoamTypography.HeadlineMedium), nameof(LoamTypography.HeadlineSmall),
                     nameof(LoamTypography.TitleLarge), nameof(LoamTypography.TitleMedium),
                     nameof(LoamTypography.TitleSmall), nameof(LoamTypography.BodyLarge),
                     nameof(LoamTypography.BodyMedium), nameof(LoamTypography.BodySmall),
                     nameof(LoamTypography.LabelLarge), nameof(LoamTypography.LabelMedium),
                     nameof(LoamTypography.LabelSmall),
                 })
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
        sample.Bind(TextBlock.ForegroundProperty, sample.GetResourceObservable(LoamTokens.ColorOnSurface));
        return sample;
    }

    private static WrapPanel BuildShape()
    {
        var wrap = new WrapPanel();
        foreach (var name in new[]
                 {
                     nameof(LoamShape.None), nameof(LoamShape.ExtraSmall), nameof(LoamShape.Small),
                     nameof(LoamShape.Medium), nameof(LoamShape.Large), nameof(LoamShape.ExtraLarge),
                     nameof(LoamShape.ExtraExtraLarge), nameof(LoamShape.Full),
                 })
        {
            wrap.Children.Add(ShapeSample(name));
        }

        return wrap;
    }

    private static Border ShapeSample(string name)
    {
        var label = new TextBlock
        {
            Text = name,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
        };
        label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable(LoamTokens.ColorOnPrimaryContainer));

        var box = new Border { Width = 108, Height = 56, Margin = new Thickness(0, 0, 12, 12), Child = label };
        box.Bind(Border.CornerRadiusProperty, box.GetResourceObservable($"Loam.Shape.{name}"));
        box.Bind(Border.BackgroundProperty, box.GetResourceObservable(LoamTokens.ColorPrimaryContainer));
        return box;
    }

    private static WrapPanel BuildSpacingAndMotion()
    {
        var wrap = new WrapPanel();
        foreach (var name in new[]
                 {
                     nameof(LoamSpacing.Small), nameof(LoamSpacing.Medium), nameof(LoamSpacing.Large),
                     nameof(LoamSpacing.ExtraLarge), nameof(LoamMotion.Short3), nameof(LoamMotion.Medium1),
                     nameof(LoamMotion.Long1),
                 })
        {
            wrap.Children.Add(TokenChip(name, name.StartsWith("Short", StringComparison.Ordinal) ||
                name.StartsWith("Medium", StringComparison.Ordinal) ||
                name.StartsWith("Long", StringComparison.Ordinal)
                    ? $"Loam.Motion.Duration.{name}"
                    : $"Loam.Spacing.{name}"));
        }

        return wrap;
    }

    private static Border TokenChip(string labelText, string token)
    {
        var label = new TextBlock { Text = labelText, FontSize = 12, Margin = new Thickness(12, 6) };
        label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable(LoamTokens.ColorOnSurface));

        var chip = new Border
        {
            Margin = new Thickness(0, 0, 8, 8),
            BorderThickness = new Thickness(1),
            Child = label,
        };
        chip.Bind(Border.BackgroundProperty, chip.GetResourceObservable(LoamTokens.ColorSurfaceContainer));
        chip.Bind(Border.BorderBrushProperty, chip.GetResourceObservable(LoamTokens.ColorOutlineVariant));
        chip.Bind(Border.CornerRadiusProperty, chip.GetResourceObservable(LoamTokens.ShapeSmall));
        _ = token;
        return chip;
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
        var tonalLevel = Math.Min(level, 5);
        var box = new Border { Width = 84, Height = 64, Margin = new Thickness(0, 8, 16, 8) };
        box.Bind(Border.CornerRadiusProperty, box.GetResourceObservable(LoamTokens.ShapeMedium));
        box.Bind(Border.BackgroundProperty, box.GetResourceObservable(LoamTokens.TonalElevation(tonalLevel)));
        box.Bind(Border.BoxShadowProperty, box.GetResourceObservable(LoamTokens.Elevation(level)));

        var label = new TextBlock { Text = $"e{level}", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 12 };
        label.Bind(TextBlock.ForegroundProperty, label.GetResourceObservable(LoamTokens.ColorOnSurfaceVariant));
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
