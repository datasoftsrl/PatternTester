using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PatternTester.App.Services;
using PatternTester.App.ViewModels;
using PatternTester.Rendering;

namespace PatternTester.App;

public partial class PatternWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly MainWindow _mainWindow;
    
    public PatternWindow()
	{
		InitializeComponent();
	}

    public PatternWindow(MainWindowViewModel viewModel, MainWindow mainWindow)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _mainWindow = mainWindow;
        Canvas.Catalog = viewModel.Catalog;
        Topmost = true;

        // Necessario su Windows: senza Manual esplicito, il backend Win32
        // puo' ignorare la Position impostata prima della prima Show()
        // e posizionare la finestra sul monitor primario.
        WindowStartupLocation = WindowStartupLocation.Manual;

        PointerPressed += OnPointerPressed;
        _viewModel.PropertyChanged += (_, _) => Refresh();
    }

	public void ShowOnMonitor(int monitorNumber)
	{
		var screens = _mainWindow.Screens.All;
		if (screens.Count == 0)
			return;

		var index = Math.Clamp(monitorNumber - 1, 0, screens.Count - 1);
		var screen = screens[index];

		Canvas.HorizontalScreens = _viewModel.HorizontalScreens;
		Canvas.VerticalScreens = _viewModel.VerticalScreens;
		Canvas.CurrentPatternIndex = _viewModel.SelectedPatternIndex;

		Position = screen.Bounds.Position;
		Width = screen.Bounds.Width;
		Height = screen.Bounds.Height;

		Topmost = true;
		Show();

		// Su Windows la Position impostata prima di Show() puo' essere
		// scartata dal backend Win32 durante la creazione della finestra
		// nativa (soprattutto con DPI/scaling diversi tra i monitor).
		// La riapplichiamo subito dopo la Show() per essere sicuri che
		// la finestra finisca davvero sul monitor selezionato.
		Position = screen.Bounds.Position;
		Width = screen.Bounds.Width;
		Height = screen.Bounds.Height;

		var bounds = screen.Bounds;
		ApplyNativePlacement(bounds.X, bounds.Y, bounds.Width, bounds.Height);

		// Un secondo ripristino asincrono, eseguito dopo il primo layout
		// pass, copre i casi limite in cui il sistema operativo rimuove
		// ancora la posizione (tipico con scaling misto tra monitor).
		Dispatcher.UIThread.Post(() =>
		{
			Position = screen.Bounds.Position;
			Width = screen.Bounds.Width;
			Height = screen.Bounds.Height;
			ApplyNativePlacement(bounds.X, bounds.Y, bounds.Width, bounds.Height);
		}, DispatcherPriority.Loaded);

		Refresh();
	}

	/// <summary>
	/// Su Windows, forza il posizionamento della finestra nativa (HWND)
	/// tramite SetWindowPos, bypassando Avalonia.Window.Position che in
	/// alcuni setup multi-monitor riporta erroneamente la finestra sul
	/// monitor primario nonostante WindowStartupLocation.Manual (bug noto
	/// del backend Win32 di Avalonia). Su Linux/macOS non fa nulla: la
	/// gestione standard di Avalonia funziona correttamente.
	/// </summary>
	private void ApplyNativePlacement(int x, int y, int width, int height)
	{
		if (!OperatingSystem.IsWindows())
			return;

		var handle = TryGetPlatformHandle();

		if (handle is null)
			return;

		Win32WindowPlacement.MoveTo(handle.Handle, x, y, width, height, topmost: true);
	}

	public void MoveToMonitor(int monitorNumber)
	{
		var screens = _mainWindow.Screens.All;

		if (screens.Count == 0)
			return;

		var index = Math.Clamp(monitorNumber - 1, 0, screens.Count - 1);
		var screen = screens[index];

		Canvas.HorizontalScreens = _viewModel.HorizontalScreens;
		Canvas.VerticalScreens = _viewModel.VerticalScreens;
		Canvas.CurrentPatternIndex = _viewModel.SelectedPatternIndex;

		Position = screen.Bounds.Position;
		Width = screen.Bounds.Width;
		Height = screen.Bounds.Height;

		var bounds = screen.Bounds;
		ApplyNativePlacement(bounds.X, bounds.Y, bounds.Width, bounds.Height);
	}



	public void Refresh()
	{
		Canvas.HorizontalScreens = _viewModel.HorizontalScreens;
		Canvas.VerticalScreens = _viewModel.VerticalScreens;
		Canvas.CurrentPatternIndex = _viewModel.SelectedPatternIndex;
		Canvas.Refresh();

		if (_viewModel.TestMode && !IsVisible)
		{
			ShowOnMonitor(_viewModel.SelectedMonitor);
		}
	}
	
	
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            // Su Windows, Topmost mantiene la finestra sopra a tutto in
            // modo molto piu' rigido rispetto a Linux: va disattivato
            // esplicitamente PRIMA di nascondere la finestra, altrimenti
            // la finestra dei parametri puo' restare bloccata dietro.
            Topmost = false;
            Hide();
            _mainWindow.ReturnFromPattern();
            e.Handled = true;
        }
    }
}
