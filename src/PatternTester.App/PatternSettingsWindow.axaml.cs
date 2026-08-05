using Avalonia.Controls;
using PatternTester.App.ViewModels;

namespace PatternTester.App;

public partial class PatternSettingsWindow : Window
{
	public PatternSettingsWindow()
	{
		InitializeComponent();
	}
    public PatternSettingsWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
