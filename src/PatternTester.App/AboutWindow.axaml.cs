using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PatternTester.App;

public partial class AboutWindow : Window
{
public string AppVersion { get; } =
Assembly.GetExecutingAssembly()
.GetName()
.Version?
.ToString(3) ?? "1.0.0";

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
