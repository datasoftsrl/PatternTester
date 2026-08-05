using Avalonia.Controls;
using Avalonia.Interactivity;
using PatternTester.App.ViewModels;

namespace PatternTester.App;

public partial class SettingsWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly string _initialLanguageCode;
    private bool _accepted;
    
     public SettingsWindow()
    {
        InitializeComponent();
     }

    public SettingsWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _initialLanguageCode = viewModel.Settings.LanguageCode;
        DataContext = viewModel;
        Closing += OnClosing;
    }

    private void OkClick(object? sender, RoutedEventArgs e)
    {
        _accepted = true;
        _viewModel.Save();
        Close();
    }

    private void CancelClick(object? sender, RoutedEventArgs e)
    {
        _viewModel.SelectLanguageCode(_initialLanguageCode);
        Close();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (!_accepted)
            _viewModel.SelectLanguageCode(_initialLanguageCode);
    }
}
