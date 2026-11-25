using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PatternTester.App;

public partial class AboutWindow : Window
{
public string AppVersion { get; } = ResolveVersion();

/// <summary>
/// AssemblyName.Version reflects the assembly's AssemblyVersion, which
/// MinVer deliberately pins to "{Major}.0.0.0" (e.g. always "1.0.0.0")
/// rather than the real calculated version, specifically to avoid
/// binding-redirect churn between patch/minor releases — this is MinVer's
/// documented default behavior, not a bug in the tag/prefix setup (see
/// docs/developer.md, "Versioning"). Reading it here was why this window
/// kept showing "1.0.0" no matter how many tags followed v1.0.0.
///
/// AssemblyInformationalVersionAttribute instead carries MinVer's full
/// calculated SemVer (e.g. "1.0.4", or "1.0.5-alpha.0.3" for a dev build
/// ahead of the last tag), which is what a human expects to see in an
/// About dialog — so that is what we read here instead.
/// </summary>
private static string ResolveVersion()
{
    var informational = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion;

    if (!string.IsNullOrWhiteSpace(informational))
    {
        // MinVer appends "+<git-sha>" build metadata to this field
        // (e.g. "1.0.4+a1b2c3d"); strip it for a clean, user-facing
        // version string. Any prerelease suffix (the "-alpha.0.3" part)
        // is kept, since for an untagged dev build it is the only
        // visible signal that this isn't an exact tagged release.
        var plusIndex = informational.IndexOf('+');
        return plusIndex >= 0 ? informational[..plusIndex] : informational;
    }

    return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
}

public AboutWindow()
{
    InitializeComponent();
    DataContext = this;
}

private void WebsiteClick(object? sender, RoutedEventArgs e)
{
    OpenUrl("https://www.datasoftweb.com/");
}

private void GitHubClick(object? sender, RoutedEventArgs e)
{
    OpenUrl("https://github.com/datasoftsrl/PatternTester");
}

private static void OpenUrl(string url)
{
    Process.Start(new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
}

private void CloseClick(object? sender, RoutedEventArgs e)
{
    Close();
}

}
