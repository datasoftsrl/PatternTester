using Avalonia.Controls;
using Avalonia.Interactivity;
using PatternTester.App.ViewModels;

namespace PatternTester.App;


public partial class MonitorInfoWindow : Window
{
    public MonitorInfoWindow()
    {
        InitializeComponent();
    }

    public MonitorInfoWindow(MonitorInfoViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CloseClick(object? sender, RoutedEventArgs e)
    {
        if (Owner is MainWindow mainWindow)
        {
            mainWindow.CloseMonitorInfo();
        }
        else
        {
            Close();
        }
    }
}
