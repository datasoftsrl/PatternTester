using PatternTester.App.Services;
using System.Collections.ObjectModel;
using PatternTester.Core.Models;
using PatternTester.Core.Patterns;
using PatternTester.Infrastructure;


namespace PatternTester.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    public ApplicationSettings Settings { get; } = new();
    
	public bool StartupUseLast
	{
		get => Settings.StartupMode == "last";
		set
		{
			if (!value)
				return;

			Settings.StartupMode = "last";
			Raise(nameof(StartupUseLast));
			Raise(nameof(StartupUseDefaults));
		}
	}

	public bool StartupUseDefaults
	{
		get => Settings.StartupMode == "defaults";
		set
		{
			if (!value)
				return;

			Settings.StartupMode = "defaults";
			Raise(nameof(StartupUseDefaults));
			Raise(nameof(StartupUseLast));
		}
	}

	public bool UseDarkTheme
	{
		get => Settings.Theme == "dark";
		set
		{
			if (!value)
				return;

			Settings.Theme = "dark";
			Raise(nameof(UseDarkTheme));
			Raise(nameof(UseLightTheme));
		}
	}

	public bool UseLightTheme
	{
		get => Settings.Theme == "light";
		set
		{
			if (!value)
				return;

			Settings.Theme = "light";
			Raise(nameof(UseLightTheme));
			Raise(nameof(UseDarkTheme));
		}
	}

	public bool SaveOnExit
	{
		get => Settings.SaveMode == "on_exit";
		set
		{
			if (!value)
				return;

			Settings.SaveMode = "on_exit";
			Raise(nameof(SaveOnExit));
			Raise(nameof(SaveOnChange));
			Raise(nameof(SaveManual));
		}
	}

	public bool SaveOnChange
	{
		get => Settings.SaveMode == "on_change";
		set
		{
			if (!value)
				return;

			Settings.SaveMode = "on_change";
			Raise(nameof(SaveOnChange));
			Raise(nameof(SaveOnExit));
			Raise(nameof(SaveManual));
		}
	}

	public bool SaveManual
	{
		get => Settings.SaveMode == "manual";
		set
		{
			if (!value)
				return;

			Settings.SaveMode = "manual";
			Raise(nameof(SaveManual));
			Raise(nameof(SaveOnExit));
			Raise(nameof(SaveOnChange));
		}
	}



    private bool _showPattern;
    private bool _showMonitorInfo;
    private readonly ConfigurationService _configuration;
    private readonly LocalizationService _localization;
    public LocalizationService Localization => _localization;
    private int _selectedMonitor = 1;
    private int _horizontalScreens = 3;
    private int _verticalScreens = 3;
    private int _selectedPatternIndex;
    private string _singleColorName = "black";
    private string _barsColorName = "white";
    private string _gradientColorName = "white";
    private string _gradientStartColorName = "red";
    private string _gradientEndColorName = "blue";

    public MainWindowViewModel(PatternCatalog catalog, ConfigurationService configuration, LocalizationService localization)
    {
        Catalog = catalog;
        _configuration = configuration;
        _localization = localization;
        Display = new DisplaySettings();
               
        _selectedPatternIndex = _configuration.Load(Display, Catalog, Settings);
        
        _selectedMonitor = Display.TargetMonitor;
        
        _horizontalScreens = Display.HorizontalScreens;
        _verticalScreens = Display.VerticalScreens;

        // Senza queste due chiamate, righe/colonne caricate da
        // configurazione risultavano corrette nei valori ma NON
        // evidenziate nel menu Display al riavvio (bypassavano i
        // setter pubblici HorizontalScreens/VerticalScreens, gli unici
        // che aggiornano IsSelected sulle relative ObservableCollection).
        UpdateColumnSelection();
        UpdateRowSelection();        _singleColorName = Catalog.Items.OfType<SingleColorPattern>().First().Color.ToString();
        _barsColorName = Catalog.Items.OfType<BarsPattern>().First().Color.ToString();
        _gradientColorName = Catalog.Items.OfType<GradientToBlackPattern>().First().Color.ToString();
        _gradientStartColorName = Catalog.Items.OfType<GradientTwoColorsPattern>().First().StartColor.ToString();
        _gradientEndColorName = Catalog.Items.OfType<GradientTwoColorsPattern>().First().EndColor.ToString();

        LanguageOptions = new ObservableCollection<LanguageOptionItem>(_localization.AvailableLanguages);
        var language = LanguageOptions.FirstOrDefault(x => string.Equals(x.Code, Settings.LanguageCode, StringComparison.OrdinalIgnoreCase))
                       ?? LanguageOptions.FirstOrDefault(x => x.Code == "it")
                       ?? LanguageOptions.FirstOrDefault();
        _selectedLanguage = language;
        if (language is not null)
        {
            _localization.Apply(language.Code);
            Settings.LanguageCode = language.Code;
        }

        UpdatePatternNames();
    }

    public ObservableCollection<LanguageOptionItem> LanguageOptions { get; }
    private LanguageOptionItem? _selectedLanguage;
    public LanguageOptionItem? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (value is null || !SetProperty(ref _selectedLanguage, value))
                return;

            Settings.LanguageCode = value.Code;
            _localization.Apply(value.Code);
            UpdatePatternNames();
        }
    }

    public void SelectLanguageCode(string? code)
    {
        var language = LanguageOptions.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))
                       ?? LanguageOptions.FirstOrDefault(x => x.Code == "it")
                       ?? LanguageOptions.FirstOrDefault();
        if (language is not null)
            SelectedLanguage = language;
    }

    public PatternCatalog Catalog { get; }
    public DisplaySettings Display { get; }
    public ObservableCollection<int> MonitorOptions { get; } = [1];

	// Collezione "gemella" di MonitorOptions, usata solo dal menu Display
	// per evidenziare il monitor attualmente attivo (stesso pattern gia'
	// usato per ColumnOptions/RowOptions). Tenuta separata da
	// MonitorOptions per non toccare il binding SelectedItem gia'
	// esistente in SettingsWindow (che si aspetta un int, non un
	// ScreenOptionItem).
	public ObservableCollection<ScreenOptionItem> MonitorMenuOptions { get; } = new();

	public ObservableCollection<ScreenOptionItem> ColumnOptions { get; } =
		new(Enumerable.Range(1, 64).Select(x => new ScreenOptionItem(x)));

	public ObservableCollection<ScreenOptionItem> RowOptions { get; } =
		new(Enumerable.Range(1, 64).Select(x => new ScreenOptionItem(x)));    
	
	public ObservableCollection<int> ScreenOptions { get; } =
		new(Enumerable.Range(1, 64));
		
	public ObservableCollection<int> ChessboardOptions { get; } =
		new(Enumerable.Range(2, 15));
    
    public ObservableCollection<string> PatternNames { get; } = new();

    private void UpdatePatternNames()
    {
        var keys = new[]
        {
            "PatternName.Geometry", "PatternName.SingleColor", "PatternName.Gray",
            "PatternName.Gamma", "PatternName.Bars", "PatternName.ColorBars",
            "PatternName.GradientToBlack", "PatternName.TwoColorsGradient",
            "PatternName.Chessboard", "PatternName.Phase", "PatternName.ColorTemperature"
        };

        PatternNames.Clear();
        foreach (var key in keys)
            PatternNames.Add(_localization.Translate(key));
    }
    
    
	public bool TestMode
	{
		get => _showPattern;
		set => SetProperty(ref _showPattern, value);
	}    

   
	public bool ShowMonitorInfo
	{
		get => _showMonitorInfo;
		set => SetProperty(ref _showMonitorInfo, value);
	}
    
    public ObservableCollection<string> Directions { get; } = new(new[] { "from_left", "from_right", "from_top", "from_bottom" });
    public ObservableCollection<string> PresetColors { get; } = new(new[] { "black", "white", "red", "green", "blue", "magenta", "yellow", "cyan" });
	public int SelectedMonitor
	{
		get => _selectedMonitor;
		set
		{
			if (SetProperty(ref _selectedMonitor, value))
			{
				Display.TargetMonitor = value;
				Raise(nameof(SelectedMonitorItem));
				UpdateMonitorSelection();
			}
		}
	}    
	 
	public int? SelectedMonitorItem
	{
		get => MonitorOptions.Contains(_selectedMonitor)
			? _selectedMonitor
			: null;

		set
		{
			if (value.HasValue && MonitorOptions.Contains(value.Value))
				SelectedMonitor = value.Value;
		}
	}

	public int HorizontalScreens
	{
		get => _horizontalScreens;
		set
		{
			if (SetProperty(ref _horizontalScreens, value))
			{
				Display.HorizontalScreens = value;
				UpdateColumnSelection();
				Raise(nameof(CellCount));
			}
		}
	}

	public int VerticalScreens
	{
		get => _verticalScreens;
		set
		{
			if (SetProperty(ref _verticalScreens, value))
			{
				Display.VerticalScreens = value;
				UpdateRowSelection();
				Raise(nameof(CellCount));
			}
		}
	}

	private void UpdateColumnSelection()
	{
		foreach (var option in ColumnOptions)
			option.IsSelected = option.Value == HorizontalScreens;
	}

	private void UpdateRowSelection()
	{
		foreach (var option in RowOptions)
			option.IsSelected = option.Value == VerticalScreens;
	}

	private void UpdateMonitorSelection()
	{
		foreach (var option in MonitorMenuOptions)
			option.IsSelected = option.Value == SelectedMonitor;
	}

	public int CellCount => HorizontalScreens * VerticalScreens;
	
    public int SelectedPatternIndex
    {
        get => _selectedPatternIndex;
        set
        {
            if (SetProperty(ref _selectedPatternIndex, value))
            {
                Raise(nameof(IsGeometrySelected)); Raise(nameof(IsSingleColorSelected)); Raise(nameof(IsGraySelected)); Raise(nameof(IsGammaSelected));
                Raise(nameof(IsBarsSelected)); Raise(nameof(IsColorBarsSelected)); Raise(nameof(IsGradientSelected));
                Raise(nameof(IsTwoColorsGradientSelected)); Raise(nameof(IsChessboardSelected)); Raise(nameof(IsPhaseSelected)); Raise(nameof(IsColorTemperatureSelected));
                Raise(nameof(IsOverscanSelected));
                
            }
        }
    }

    public bool IsGeometrySelected => SelectedPatternIndex == 0;
    public bool IsSingleColorSelected => SelectedPatternIndex == 1;
    public bool IsGraySelected => SelectedPatternIndex == 2;
    public bool IsGammaSelected => SelectedPatternIndex == 3;
    public bool IsBarsSelected => SelectedPatternIndex == 4;
    public bool IsColorBarsSelected => SelectedPatternIndex == 5;
    public bool IsGradientSelected => SelectedPatternIndex == 6;
    public bool IsTwoColorsGradientSelected => SelectedPatternIndex == 7;
    public bool IsChessboardSelected => SelectedPatternIndex == 8;
    public bool IsPhaseSelected => SelectedPatternIndex == 9;
    public bool IsColorTemperatureSelected => SelectedPatternIndex == 10;
    public bool IsOverscanSelected => SelectedPatternIndex == 11;
   
    public GeometryPattern Geometry => Catalog.Items.OfType<GeometryPattern>().First();
    public SingleColorPattern SingleColor => Catalog.Items.OfType<SingleColorPattern>().First();
    public GrayPattern Gray => Catalog.Items.OfType<GrayPattern>().First();
    public GammaPattern Gamma => Catalog.Items.OfType<GammaPattern>().First();
    public BarsPattern Bars => Catalog.Items.OfType<BarsPattern>().First();
    public ColorBarsPattern ColorBars => Catalog.Items.OfType<ColorBarsPattern>().First();
    public GradientToBlackPattern GradientToBlack => Catalog.Items.OfType<GradientToBlackPattern>().First();
    public GradientTwoColorsPattern GradientTwoColors => Catalog.Items.OfType<GradientTwoColorsPattern>().First();
    public ChessboardPattern Chessboard => Catalog.Items.OfType<ChessboardPattern>().First();
    public PhasePattern Phase => Catalog.Items.OfType<PhasePattern>().First();
    public ColorTemperaturePattern ColorTemperature => Catalog.Items.OfType<ColorTemperaturePattern>().First();
    public OverscanPattern Overscan => Catalog.Items.OfType<OverscanPattern>().First();
   

      public double OverscanActionSafePercent
    {
        get => Overscan.ActionSafePercent;
        set
        {
            var clamped = Math.Clamp(value, 0, 45);

            if (Overscan.ActionSafePercent != clamped)
            {
                Overscan.ActionSafePercent = clamped;
                Raise();
            }
        }
    }

    public double OverscanTitleSafePercent
    {
        get => Overscan.TitleSafePercent;
        set
        {
            var clamped = Math.Clamp(value, 0, 45);

            if (Overscan.TitleSafePercent != clamped)
            {
                Overscan.TitleSafePercent = clamped;
                Raise();
            }
        }
    }

    public int GeometryLines { get => Geometry.Lines; set { Geometry.Lines = value; Raise(); } }
    public bool GeometryDiagonal { get => Geometry.DiagonalLines; set { Geometry.DiagonalLines = value; Raise(); } }
    public bool GeometryCircle { get => Geometry.Circle; set { Geometry.Circle = value; Raise(); } }
    public double GrayWhite { get => Gray.White; set { Gray.White = value; Raise(); } }
    public double GammaValue { get => Gamma.Value; set { Gamma.Value = Math.Clamp(Math.Round(value, 1), 1.0, 3.5); Raise(); }}
    public int GammaCells { get => Gamma.Cells; set { Gamma.Cells = Math.Clamp(value, 4, 128); Raise(); }}
    public int BarsNumber { get => Bars.Number; set { Bars.Number = value; Raise(); } }
    public string BarsDirection { get => Bars.Direction.ToConfigString(); set { Bars.Direction = PatternDirectionExtensions.Parse(value); Raise(); } }
    public string ColorBarsDirection { get => ColorBars.Direction.ToConfigString(); set { ColorBars.Direction = PatternDirectionExtensions.Parse(value); Raise(); } }
    public string GradientDirection { get => GradientToBlack.Direction.ToConfigString(); set { GradientToBlack.Direction = PatternDirectionExtensions.Parse(value); Raise(); } }
    public string Gradient2Direction { get => GradientTwoColors.Direction.ToConfigString(); set { GradientTwoColors.Direction = PatternDirectionExtensions.Parse(value); Raise(); } }
    public int ChessboardSquares { get => Chessboard.Squares; set { Chessboard.Squares = Math.Clamp(value, 2, 16); Raise(); } }
    public string PhaseDirection { get => Phase.Direction.ToConfigString(); set { Phase.Direction = PatternDirectionExtensions.Parse(value); Raise(); } }

    public string SingleColorName
    {
        get => _singleColorName;
        set { if (SetProperty(ref _singleColorName, value)) { SingleColor.Color = RgbColor.Parse(value, RgbColor.Black); Raise(nameof(SingleColor)); } }
    }
    public string BarsColorName
    {
        get => _barsColorName;
        set { if (SetProperty(ref _barsColorName, value)) { Bars.Color = RgbColor.Parse(value, RgbColor.White); Raise(nameof(Bars)); } }
    }
    public string GradientColorName
    {
        get => _gradientColorName;
        set { if (SetProperty(ref _gradientColorName, value)) { GradientToBlack.Color = RgbColor.Parse(value, RgbColor.White); Raise(nameof(GradientToBlack)); } }
    }
    public string GradientStartColorName
    {
        get => _gradientStartColorName;
        set { if (SetProperty(ref _gradientStartColorName, value)) { GradientTwoColors.StartColor = RgbColor.Parse(value, RgbColor.Red); Raise(nameof(GradientTwoColors)); } }
    }
    public string GradientEndColorName
    {
        get => _gradientEndColorName;
        set { if (SetProperty(ref _gradientEndColorName, value)) { GradientTwoColors.EndColor = RgbColor.Parse(value, RgbColor.Blue); Raise(nameof(GradientTwoColors)); } }
    }

	
	public void RefreshMonitors(int count, int primaryIndex)
	{
		MonitorOptions.Clear();
		MonitorMenuOptions.Clear();

		count = Math.Max(1, count);

		for (var i = 1; i <= count; i++)
		{
			MonitorOptions.Add(i);
			MonitorMenuOptions.Add(new ScreenOptionItem(i));
		}

		var monitor = _selectedMonitor;

		if (monitor < 1 || monitor > count)
			monitor = Math.Clamp(primaryIndex, 1, count);

		_selectedMonitor = monitor;
		Display.TargetMonitor = monitor;

		Raise(nameof(SelectedMonitor));
		Raise(nameof(SelectedMonitorItem));
		UpdateMonitorSelection();
	}


       public void Save() => _configuration.Save(Display, Catalog, SelectedPatternIndex, Settings);
       
       
       public int ColorTemperatureKelvin
		{
			get => ColorTemperature.Temperature;
			set
			{
				var temperature = Math.Clamp(value, 2400, 9500);

				if (ColorTemperature.Temperature != temperature)
				{
					ColorTemperature.Temperature = temperature;
					Raise();
				}
			}
		}

}
