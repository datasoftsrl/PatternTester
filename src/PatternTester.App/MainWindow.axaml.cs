using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using PatternTester.App.ViewModels;
using PatternTester.App.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;

namespace PatternTester.App;

public partial class MainWindow : Window
{
	private PatternWindow? _patternWindow;
    private MonitorInfoWindow? _monitorInfoWindow;
    private readonly LinuxMonitorInfoService _linuxMonitorInfoService = new();
    private readonly WindowsMonitorInfoService _windowsMonitorInfoService = new();
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;
   
	private bool _suspendPatternWindow;
	
	public MainWindow()
	{
		InitializeComponent();
	}
	
    public MainWindow(MainWindowViewModel viewModel)
    {
        
        InitializeComponent();
        DataContext = viewModel;

        ApplyTheme();

        Opened += OnOpened;
        Closed += OnClosed;

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// Applica il tema (chiaro/scuro) letto da ViewModel.Settings.Theme
    /// all'intera applicazione. Va richiamato sia all'avvio (dopo il
    /// caricamento della configurazione) sia ogni volta che l'utente
    /// lo cambia da menu/impostazioni.
    /// </summary>
    private void ApplyTheme()
    {
        if (Application.Current is null)
            return;

        Application.Current.RequestedThemeVariant = ViewModel.UseLightTheme
            ? Avalonia.Styling.ThemeVariant.Light
            : Avalonia.Styling.ThemeVariant.Dark;
    }

    private void OnViewModelPropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(MainWindowViewModel.UseDarkTheme)
            || args.PropertyName == nameof(MainWindowViewModel.UseLightTheme))
        {
            ApplyTheme();
            return;
        }

        if (args.PropertyName == nameof(MainWindowViewModel.ShowMonitorInfo))
        {
            if (ViewModel.ShowMonitorInfo)
            {
                ShowMonitorInfo();
            }
            else
            {
                _monitorInfoWindow?.Hide();

                // Rilascia il Topmost solo quando la finestra viene
                // nascosta, non subito dopo averla mostrata (vedi
                // ShowMonitorInfo).
                if (_monitorInfoWindow is not null)
                    _monitorInfoWindow.Topmost = false;

                // Senza questo, chiudendo Info Monitor la finestra
                // principale puo' finire di nuovo dietro al pattern
                // (che resta sempre Topmost): Windows ricalcola lo
                // z-order quando la finestra Topmost "Info Monitor"
                // scompare, e MainWindow non viene automaticamente
                // riportata sopra. La riaffermiamo esplicitamente qui,
                // con lo stesso schema usato in ShowMonitorInfo().
                Show();
                Activate();

                if (OperatingSystem.IsWindows())
                {
                    Topmost = true;
                    Topmost = false;
                    Topmost = true;
                }
            }

            return;
        }

		if (args.PropertyName == nameof(MainWindowViewModel.TestMode))
		{
			if (ViewModel.TestMode)
			{
				// L'utente ha riattivato Show Pattern:
				// termina la sospensione.
				_suspendPatternWindow = false;

				ShowLivePattern();
			}
			else
			{
				_suspendPatternWindow = false;

				_patternWindow?.Hide();
			}

			Show();
			Activate();

			return;
		}

  		if (args.PropertyName == nameof(MainWindowViewModel.SelectedMonitor))
		{
			PositionOnSelectedMonitor();

			if (_patternWindow is not null && _patternWindow.IsVisible)
				_patternWindow.ShowOnMonitor(ViewModel.SelectedMonitor);

			if (ViewModel.ShowMonitorInfo)
				ShowMonitorInfo();

			return;
		}

        if (_patternWindow is not null && _patternWindow.IsVisible)
        {
            _patternWindow.Refresh();
        }
    }

    private void ShowLivePattern()
	{
		// Pattern temporarily suspended (Identify Monitor).
		if (_suspendPatternWindow)
			return;

		ViewModel.Save();

		_patternWindow ??= new PatternWindow(ViewModel, this);

		_patternWindow.ShowOnMonitor(ViewModel.SelectedMonitor);
		
	}

	private void ShowMonitorInfo()
	{
		var allScreens = Screens.All;

		if (allScreens.Count == 0)
			return;

		var monitorIndex = ViewModel.SelectedMonitor - 1;

		if (monitorIndex < 0 || monitorIndex >= allScreens.Count)
			monitorIndex = 0;

		var screen = allScreens[monitorIndex];
		var bounds = screen.Bounds;
		var workingArea = screen.WorkingArea;

		MonitorInfoViewModel info;

		if (OperatingSystem.IsWindows())
		{
			var winInfo = _windowsMonitorInfoService.Read(screen.DisplayName, monitorIndex);

			info = new MonitorInfoViewModel
			{
				MonitorNumber = (monitorIndex + 1).ToString(),
				DisplayName = screen.DisplayName ?? "N/D",

				Resolution = $"{bounds.Width} × {bounds.Height} px",
				RefreshRate = winInfo.RefreshRate,
				WorkingArea = $"{workingArea.Width} × {workingArea.Height} px",
				PhysicalSize = winInfo.PhysicalSize,
				Scaling = $"{screen.Scaling:P0}",
				Dpi = $"{screen.Scaling * 96:0} DPI",
				ColorDepth = winInfo.ColorDepth,
				PanelColorDepth = winInfo.PanelColorDepth,
				Orientation = screen.CurrentOrientation.ToString(),
				IsPrimary = screen.IsPrimary ? "Sì" : "No",

				OperatingSystem = winInfo.OperatingSystem,
				DesktopEnvironment = winInfo.DesktopEnvironment,
				SessionType = winInfo.SessionType,
				Kernel = winInfo.Kernel,
				Architecture = winInfo.Architecture
			};
		}
		else
		{
			var xrandr = RunCommand("xrandr", "--current");
			var xrandrVerbose = RunCommand("xrandr", "--verbose");
			var xdpyinfo = RunCommand("xdpyinfo", "");

			info = new MonitorInfoViewModel
			{
				MonitorNumber = (monitorIndex + 1).ToString(),
				DisplayName = screen.DisplayName ?? "N/D",

				Resolution = $"{bounds.Width} × {bounds.Height} px",

				RefreshRate = GetRefreshRate(xrandr),

				WorkingArea = $"{workingArea.Width} × {workingArea.Height} px",

				PhysicalSize = GetPhysicalSize(xrandr),

				Scaling = $"{screen.Scaling:P0}",

				Dpi = $"{screen.Scaling * 96:0} DPI",

				ColorDepth = GetColorDepth(xdpyinfo),

				PanelColorDepth = GetPanelColorDepth(xrandrVerbose),

				Orientation = screen.CurrentOrientation.ToString(),

				IsPrimary = screen.IsPrimary ? "Sì" : "No",

				OperatingSystem = GetOperatingSystem(),

				DesktopEnvironment =
					Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")
					?? Environment.GetEnvironmentVariable("XDG_SESSION_DESKTOP")
					?? "N/D",

				SessionType =
					Environment.GetEnvironmentVariable("XDG_SESSION_TYPE")
					?? "N/D",

				Kernel = GetKernel(),

				Architecture = RuntimeInformation.OSArchitecture.ToString()
			};
		}
		

		if (_monitorInfoWindow is null)
		{
            _monitorInfoWindow = new MonitorInfoWindow(info);
            
			_monitorInfoWindow.Closed += (_, _) =>
			{
				_monitorInfoWindow = null;

				if (ViewModel.ShowMonitorInfo)
					ViewModel.ShowMonitorInfo = false;
			};
		}		
		else
		{
			_monitorInfoWindow.DataContext = info;
		}

		if (!_monitorInfoWindow.IsVisible)
			_monitorInfoWindow.Show(this);

		// La finestra Info Monitor deve restare SOPRA al pattern (che e'
		// sempre Topmost) finche' e' visibile: con un solo monitor i due
		// occupano lo stesso schermo, quindi se il Topmost viene tolto
		// subito dopo l'Activate() la finestra Info torna dietro al
		// pattern nell'istante successivo. Percio' qui non lo resettiamo:
		// viene rilasciato solo alla chiusura/nascondimento (vedi sopra).
		_monitorInfoWindow.Topmost = true;
		_monitorInfoWindow.Activate();

		// Porta in primo piano anche la finestra principale, cosi'
		// restano entrambe visibili e utilizzabili sopra al pattern,
		// come richiesto per semplificare l'uso con un solo monitor.
		Show();
		Activate();

		if (OperatingSystem.IsWindows())
		{
			Topmost = true;
			Topmost = false;
			Topmost = true;
		}
	}

	private static string RunCommand(string command, string arguments)
	{
		try
		{
			using var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = command,
					Arguments = arguments,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();

			var output = process.StandardOutput.ReadToEnd();

			process.WaitForExit(2000);

			return output;
		}
		catch
		{
			return string.Empty;
		}
	}

	private static string GetRefreshRate(string xrandr)
	{
		if (string.IsNullOrWhiteSpace(xrandr))
			return "N/D";

		var match = Regex.Match(
			xrandr,
			@"\s(\d+(?:\.\d+)?)\s*\*",
			RegexOptions.Multiline);

		return match.Success
			? $"{match.Groups[1].Value} Hz"
			: "N/D";
	}

	private static string GetPhysicalSize(string xrandr)
	{
		if (string.IsNullOrWhiteSpace(xrandr))
			return "N/D";

		var match = Regex.Match(
			xrandr,
			@"connected(?:\s+primary)?[^\n]*\s(\d+)mm\s+x\s+(\d+)mm",
			RegexOptions.Multiline);

		return match.Success
			? $"{match.Groups[1].Value} × {match.Groups[2].Value} mm"
			: "N/D";
	}

	private static string GetColorDepth(string xdpyinfo)
	{
		if (string.IsNullOrWhiteSpace(xdpyinfo))
			return "N/D";

		var match = Regex.Match(
			xdpyinfo,
			@"depth of root window:\s*(\d+)",
			RegexOptions.IgnoreCase);

		return match.Success
			? $"{match.Groups[1].Value} bit"
			: "N/D";
	}

	private static string GetPanelColorDepth(string xrandr)
	{
		if (string.IsNullOrWhiteSpace(xrandr))
			return "N/D";

		if (xrandr.Contains("8 bpc"))
			return "8 bpc (max)";

		if (xrandr.Contains("6 bpc"))
			return "6 bpc";

		return "N/D";
	}

	private static string GetOperatingSystem()
	{
		try
		{
			if (File.Exists("/etc/os-release"))
			{
				var line = File.ReadLines("/etc/os-release")
					.FirstOrDefault(x => x.StartsWith("PRETTY_NAME="));

				if (line is not null)
				{
					return line
						.Split('=', 2)[1]
						.Trim('"');
				}
			}
		}
		catch
		{
		}

		return RuntimeInformation.OSDescription;
	}

	private static string GetKernel()
	{
		var result = RunCommand("uname", "-r");

		return string.IsNullOrWhiteSpace(result)
			? "N/D"
			: result.Trim();
	}

		private void OnOpened(object? sender, EventArgs e)
		{
			RefreshMonitorListAndPlacement();

			if (ViewModel.Settings.AutoShowPattern)
				ViewModel.TestMode = true;
		}

    private void RefreshMonitorListAndPlacement()
    {
        var screens = Screens;
        var allScreens = screens.All;

        var primaryIndex = 1;

        if (screens.Primary is not null)
        {
            for (var i = 0; i < allScreens.Count; i++)
            {
                if (ReferenceEquals(allScreens[i], screens.Primary))
                {
                    primaryIndex = i + 1;
                    break;
                }
            }
        }

        ViewModel.RefreshMonitors(allScreens.Count, primaryIndex);
        PositionOnSelectedMonitor();
    }

 
	private void PositionOnSelectedMonitor()
	{
		var allScreens = Screens.All;

		if (allScreens.Count == 0)
			return;

		int mainMonitorIndex;

		if (allScreens.Count == 1)
		{
			// Un solo monitor: la MainWindow va sul monitor principale.
			mainMonitorIndex = 0;
		}
		else
		{
			// Più monitor:
			// la MainWindow DEVE stare su un monitor diverso
			// da quello che visualizza il pattern.
			var patternMonitorIndex = ViewModel.SelectedMonitor - 1;

			if (patternMonitorIndex < 0 || patternMonitorIndex >= allScreens.Count)
				patternMonitorIndex = 0;

			// Preferiamo il monitor principale, purché non sia
			// quello che visualizza il pattern.
			var primaryIndex = 0;

			if (Screens.Primary is not null)
			{
				for (var i = 0; i < allScreens.Count; i++)
				{
					if (ReferenceEquals(allScreens[i], Screens.Primary))
					{
						primaryIndex = i;
						break;
					}
				}
			}

			if (primaryIndex != patternMonitorIndex)
			{
				mainMonitorIndex = primaryIndex;
			}
			else
			{
				// Il pattern è sul monitor principale:
				// scegliamo il primo monitor disponibile diverso.
				mainMonitorIndex = 0;

				for (var i = 0; i < allScreens.Count; i++)
				{
					if (i != patternMonitorIndex)
					{
						mainMonitorIndex = i;
						break;
					}
				}
			}
		}

		var screen = allScreens[mainMonitorIndex];
		var bounds = screen.WorkingArea;

		Position = new PixelPoint(
			bounds.X + (int)((bounds.Width - Width) / 2),
			bounds.Y + (int)((bounds.Height - Height) / 2));
	} 

    private void OnClosed(object? sender, EventArgs e)
    {
        ViewModel.Save();
        _patternWindow?.Close();
        _monitorInfoWindow?.Close();
    }

	private void PatternMenuClick(object? sender, RoutedEventArgs e)
	{
		if (sender is not MenuItem item)
			return;

		if (!int.TryParse(item.Tag?.ToString(), out var patternIndex))
			return;

		ViewModel.SelectedPatternIndex = patternIndex;

		PositionOnSelectedMonitor();
		Show();
		Activate();

		if (OperatingSystem.IsWindows())
		{
			Topmost = true;
			Topmost = false;
			Topmost = true;
		}
	}

    private void MonitorMenuClick(object? sender, RoutedEventArgs e)
    {
        // IMPORTANTE: non usare "sender" qui. Il Click e' agganciato al
        // MenuItem padre "Monitor" (perche' i figli sono generati da
        // ItemsSource senza un ItemTemplate/handler proprio), quindi con
        // il bubbling degli eventi "sender" e' sempre il padre, non la
        // voce di menu (1, 2, ...) effettivamente cliccata.
        // "e.Source" e' invece l'elemento che ha originato l'evento.
        if (e.Source is not MenuItem item)
            return;

        if (item.DataContext is int monitor)
            ViewModel.SelectedMonitorItem = monitor;
    }

	private void ColumnsMenuClick(object? sender, RoutedEventArgs e)
	{
		if (sender is ToggleButton button &&
			button.DataContext is ScreenOptionItem option)
		{
			ViewModel.HorizontalScreens = option.Value;
		}
	}

	private void RowsMenuClick(object? sender, RoutedEventArgs e)
	{
		if (sender is ToggleButton button &&
			button.DataContext is ScreenOptionItem option)
		{
			ViewModel.VerticalScreens = option.Value;
		}
	}

    private void TestModeMenuClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.TestMode = !ViewModel.TestMode;
    }

	internal void CloseMonitorInfo()
	{
		ViewModel.ShowMonitorInfo = false;
	}

    private void MonitorInfoMenuClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ShowMonitorInfo = !ViewModel.ShowMonitorInfo;
    }

    private void SaveClick(object? sender, RoutedEventArgs e)
        => ViewModel.Save();

    private async void SettingsClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsWindow(ViewModel)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        await dialog.ShowDialog(this);
    }

    private async void AboutClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new AboutWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        await dialog.ShowDialog(this);
    }

    private void HelpClick(object? sender, RoutedEventArgs e)
	{
		var guidePath = ViewModel.Localization.GetGuidePath();

		if (!File.Exists(guidePath))
			return;

		if (OperatingSystem.IsLinux())
		{
			// UseShellExecute=true su Linux lancia il browser via
			// xdg-open EREDITANDO stdout/stderr del terminale da cui
			// e' partito PatternTester (Process.Start non permette di
			// reindirizzare gli stream quando UseShellExecute=true).
			// Il browser (specie se Chromium-based) scrive parecchio
			// rumore diagnostico proprio (es. log interni di TensorFlow
			// Lite/Google Cloud Messaging usati da alcune sue funzioni
			// ML) che finisce nel nostro terminale senza che
			// PatternTester stesso stia facendo nulla di sbagliato.
			// Invocando xdg-open esplicitamente attraverso una shell,
			// con stdout/stderr rediretti a /dev/null, il terminale
			// resta pulito.
			Process.Start(new ProcessStartInfo
			{
				FileName = "/bin/sh",
				ArgumentList = { "-c", $"xdg-open '{guidePath}' >/dev/null 2>&1" },
				UseShellExecute = false
			});

			return;
		}

		Process.Start(new ProcessStartInfo
		{
			FileName = guidePath,
			UseShellExecute = true
		});
	}

    private void ExitClick(object? sender, RoutedEventArgs e)
        => Close();

	internal void ReturnFromPattern()
	{
		PositionOnSelectedMonitor();

		Show();
		Activate();

		if (OperatingSystem.IsWindows())
		{
			Topmost = true;
			Topmost = false;
			Topmost = true;
		}
	}
	private async void IdentifyMonitorsClick(object? sender, RoutedEventArgs e)
	{
		var screens = Screens.All;

		var patternWasVisible =
			_patternWindow is not null && _patternWindow.IsVisible;

		if (_patternWindow is not null && _patternWindow.IsVisible)
		{
    			_patternWindow.Hide();
		}

		if (screens.Count == 0)
			return;
	
		_suspendPatternWindow = true;

		Hide();

		try
		{
			for (var i = 0; i < screens.Count; i++)
			{
				var screen = screens[i];

				var resolution =
					$"{screen.Bounds.Width}×{screen.Bounds.Height}";

				var refreshRate = "N/D";

				if (OperatingSystem.IsWindows())
				{
					var info = _windowsMonitorInfoService.Read(
						screen.DisplayName,
						i);

					refreshRate = info.RefreshRate;
				}
				else if (OperatingSystem.IsLinux())
				{
					// Stesso identico meccanismo gia' usato (e
					// verificato funzionante) da Info Monitor — non la
					// classe LinuxMonitorInfoService, che risulta non
					// collegata/non corretta per questo scopo.
					var xrandr = RunCommand("xrandr", "--current");
					refreshRate = GetRefreshRate(xrandr);
				}

				// Append the refresh rate only when it was actually
				// found: xrandr doesn't always report one (depends on
				// the driver), and appending "N/D" would just make the
				// overlay look broken instead of simply omitting it.
				var details =
					!string.IsNullOrWhiteSpace(refreshRate) && refreshRate != "N/D"
						? $"{resolution} @ {refreshRate}"
						: resolution;

				var window = new MonitorIdentificationWindow(
					i + 1,
					details);

				window.ShowOnScreen(screen);

				await Task.Delay(TimeSpan.FromSeconds(5));

				window.Close();

				await Task.Delay(TimeSpan.FromMilliseconds(300));
			}
		}
		finally
		{
			Show();

			Topmost = true;
			Activate();
		}
	}

}
