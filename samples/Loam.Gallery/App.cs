using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Loam.Theming;

namespace Loam.Gallery;

public sealed class App : Application
{
    public override void Initialize()
    {
        // SCAFFOLDING: FluentTheme supplies templates for the window shell and built-in controls
        // while Loam is still growing. TODO(Phase 3+): drop FluentTheme once Loam themes the shell.
        Styles.Add(new FluentTheme());

        // The real subject: Loam's pure-C# theming engine layered on top.
        Styles.Add(new LoamTheme());

        // Satellite control themes (3.1 package split): pickers, data controls, charts.
        Styles.Add(new LoamPickers());
        Styles.Add(new LoamData());
        Styles.Add(new LoamCharts());

        RequestedThemeVariant = ThemeVariant.Light;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
