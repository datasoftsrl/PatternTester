using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PatternTester.App.ViewModels;
using PatternTester.App.Services;
using PatternTester.Infrastructure;
using PatternTester.Core.Patterns;

namespace PatternTester.App;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
		// Disabilita GCM e silenzia i log di Chromium a livello di processo globale (Windows e Linux)
		Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--disable-gcm --disable-device-discovery-notifications");
		Environment.SetEnvironmentVariable("CHROME_LOG_LEVEL", "3");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var catalog = new PatternCatalog();
            var configuration = new ConfigurationService();
            var localization = new LocalizationService();
            desktop.MainWindow = new MainWindow(new MainWindowViewModel(catalog, configuration, localization));
        }
        base.OnFrameworkInitializationCompleted();
        
        
    }
}
