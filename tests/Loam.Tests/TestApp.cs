using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using Loam.Tests;
using Loam.Theming;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Loam.Tests;

public sealed class TestApp : Application
{
    public override void Initialize()
    {
        // Fluent supplies Window/built-in templates for the headless host; Loam is the subject.
        Styles.Add(new FluentTheme());
        Styles.Add(new LoamTheme());
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
