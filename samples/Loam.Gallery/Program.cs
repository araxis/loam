using Avalonia;

namespace Loam.Gallery;

internal static class Program
{
    // Avalonia desktop entry point. Keep initialization in BuildAvaloniaApp so design tooling
    // and tests can reuse it.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
