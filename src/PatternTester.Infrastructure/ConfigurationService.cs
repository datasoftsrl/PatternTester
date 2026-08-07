using System.Text.Json;
using System.Text.Json.Serialization;
using PatternTester.Core.Models;
using PatternTester.Core.Patterns;

namespace PatternTester.Infrastructure;

public sealed class ConfigurationService
{
    private readonly string _path;

    public ConfigurationService()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _path = System.IO.Path.Combine(root, "PatternTester", "patterntester.json");
    }

    public string Path => _path;

    public int Load(DisplaySettings display, PatternCatalog catalog, ApplicationSettings settings)
    {
        if (!File.Exists(_path))
        {
            Console.WriteLine($"Configuration file not found: {_path}");
            return 0;
        }

        try
        {
            var jsonText = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize(jsonText, ConfigurationJsonContext.Default.ConfigurationData);

            if (data is null)
                return 0;

            if (data.Settings is not null)
            {
                settings.StartupMode = data.Settings.StartupMode;
                settings.Theme = data.Settings.Theme == "light" ? "light" : "dark";
                settings.LanguageCode = string.IsNullOrWhiteSpace(data.Settings.LanguageCode) ? "it" : data.Settings.LanguageCode;
                settings.AutoShowPattern = data.Settings.AutoShowPattern;
                settings.SaveMode = data.Settings.SaveMode;

                settings.DefaultPatternIndex = Math.Clamp(
                    data.Settings.DefaultPatternIndex, 0, 10);

                settings.DefaultMonitor = Math.Max(
                    1, data.Settings.DefaultMonitor);

                settings.DefaultHorizontalScreens = Math.Clamp(
                    data.Settings.DefaultHorizontalScreens, 1, 40);

                settings.DefaultVerticalScreens = Math.Clamp(
                    data.Settings.DefaultVerticalScreens, 1, 40);
            }

            if (settings.StartupMode == "defaults")
            {
                display.TargetMonitor = settings.DefaultMonitor;
                display.HorizontalScreens = settings.DefaultHorizontalScreens;
                display.VerticalScreens = settings.DefaultVerticalScreens;

                return settings.DefaultPatternIndex;
            }

            if (data.Display is not null)
            {
                display.TargetMonitor = data.Display.TargetMonitor;
                display.HorizontalScreens = data.Display.HorizontalScreens;
                display.VerticalScreens = data.Display.VerticalScreens;
                display.MaxScreens = data.Display.MaxScreens;
            }

            Apply(data, catalog);

            return Math.Clamp(data.SelectedPatternIndex, 0, 10);
            
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Configuration ignored: {ex.Message}");
            return 0;
        }
    }

    public void Save(DisplaySettings display, PatternCatalog catalog, int selectedPatternIndex, ApplicationSettings settings)
    {
        var data = new ConfigurationData
        {
            SelectedPatternIndex = selectedPatternIndex,
            
            Settings = new SettingsData
            {
                StartupMode = settings.StartupMode,
                Theme = settings.Theme,
                LanguageCode = settings.LanguageCode,
                AutoShowPattern = settings.AutoShowPattern,
                SaveMode = settings.SaveMode,
                DefaultPatternIndex = settings.DefaultPatternIndex,
                DefaultMonitor = settings.DefaultMonitor,
                DefaultHorizontalScreens = settings.DefaultHorizontalScreens,
                DefaultVerticalScreens = settings.DefaultVerticalScreens
            },
            
            Display = new DisplayData
            {
                TargetMonitor = display.TargetMonitor,
                HorizontalScreens = display.HorizontalScreens,
                VerticalScreens = display.VerticalScreens,
                MaxScreens = display.MaxScreens
            },
            Geometry = new GeometryData
            {
                Lines = catalog.Items.OfType<GeometryPattern>().First().Lines,
                Diagonal = catalog.Items.OfType<GeometryPattern>().First().DiagonalLines,
                Circle = catalog.Items.OfType<GeometryPattern>().First().Circle,
                MaxLines = catalog.Items.OfType<GeometryPattern>().First().MaxLines
            },
            SingleColor = new SingleColorData { Color = catalog.Items.OfType<SingleColorPattern>().First().Color.ToString() },
            Gray = new GrayData { White = catalog.Items.OfType<GrayPattern>().First().White },
            
            Gamma = new GammaData
            {
                Value = catalog.Items.OfType<GammaPattern>().First().Value,
                Cells = catalog.Items.OfType<GammaPattern>().First().Cells
            },
            
            Bars = new BarsData
            {
                Color = catalog.Items.OfType<BarsPattern>().First().Color.ToString(),
                Direction = catalog.Items.OfType<BarsPattern>().First().Direction.ToConfigString(),
                BarsNum = catalog.Items.OfType<BarsPattern>().First().Number,
                MaxBarsNum = catalog.Items.OfType<BarsPattern>().First().MaxNumber
            },
            ColorBars = new DirectionData { Direction = catalog.Items.OfType<ColorBarsPattern>().First().Direction.ToConfigString() },
            GradientToBlack = new GradientToBlackData
            {
                Color = catalog.Items.OfType<GradientToBlackPattern>().First().Color.ToString(),
                Direction = catalog.Items.OfType<GradientToBlackPattern>().First().Direction.ToConfigString()
            },
            Gradient2Colors = new Gradient2ColorsData
            {
                StartColor = catalog.Items.OfType<GradientTwoColorsPattern>().First().StartColor.ToString(),
                EndColor = catalog.Items.OfType<GradientTwoColorsPattern>().First().EndColor.ToString(),
                Direction = catalog.Items.OfType<GradientTwoColorsPattern>().First().Direction.ToConfigString()
            },
            Chessboard = new ChessboardData
            {
                Squares = catalog.Items.OfType<ChessboardPattern>().First().Squares,
                MaxSquares = catalog.Items.OfType<ChessboardPattern>().First().MaxSquares
            },
            Phase = new DirectionData { Direction = catalog.Items.OfType<PhasePattern>().First().Direction.ToConfigString() }, 
            ColorTemperature = new ColorTemperatureData { Kelvin = catalog.Items.OfType<ColorTemperaturePattern>().First().Temperature }
        };

        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_path)!);

        var json = JsonSerializer.Serialize(data, ConfigurationJsonContext.Default.ConfigurationData);
        var temporaryPath = _path + ".tmp";

        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, _path, true);
    }

    private static void Apply(ConfigurationData data, PatternCatalog catalog)
    {
        var geometry = catalog.Items.OfType<GeometryPattern>().First();
        if (data.Geometry is not null)
        {
            geometry.Lines = Math.Clamp(data.Geometry.Lines, 0, data.Geometry.MaxLines > 0 ? data.Geometry.MaxLines : geometry.MaxLines);
            geometry.DiagonalLines = data.Geometry.Diagonal;
            geometry.Circle = data.Geometry.Circle;
            geometry.MaxLines = Math.Max(1, data.Geometry.MaxLines);
        }

        if (data.SingleColor is not null) catalog.Items.OfType<SingleColorPattern>().First().Color = RgbColor.Parse(data.SingleColor.Color, RgbColor.Black);
        
        if (data.Gray is not null) catalog.Items.OfType<GrayPattern>().First().White = Math.Clamp(data.Gray.White, 0, 100);
        
        if (data.Gamma is not null)
        {
            var gamma = catalog.Items.OfType<GammaPattern>().First();

            gamma.Value = Math.Clamp(data.Gamma.Value, 1.0, 3.5);
            gamma.Cells = Math.Clamp(data.Gamma.Cells, 4, 128);
        }
        
        var bars = catalog.Items.OfType<BarsPattern>().First();
        if (data.Bars is not null)
        {
            bars.Color = RgbColor.Parse(data.Bars.Color, RgbColor.White);
            bars.Direction = PatternDirectionExtensions.Parse(data.Bars.Direction);
            bars.MaxNumber = Math.Max(1, data.Bars.MaxBarsNum);
            bars.Number = Math.Clamp(data.Bars.BarsNum, 1, bars.MaxNumber);
        }

        if (data.ColorBars is not null) catalog.Items.OfType<ColorBarsPattern>().First().Direction = PatternDirectionExtensions.Parse(data.ColorBars.Direction);

        var gtb = catalog.Items.OfType<GradientToBlackPattern>().First();
        if (data.GradientToBlack is not null)
        {
            gtb.Color = RgbColor.Parse(data.GradientToBlack.Color, RgbColor.White);
            gtb.Direction = PatternDirectionExtensions.Parse(data.GradientToBlack.Direction);
        }

        var g2 = catalog.Items.OfType<GradientTwoColorsPattern>().First();
        if (data.Gradient2Colors is not null)
        {
            g2.StartColor = RgbColor.Parse(data.Gradient2Colors.StartColor, RgbColor.Red);
            g2.EndColor = RgbColor.Parse(data.Gradient2Colors.EndColor, RgbColor.Blue);
            g2.Direction = PatternDirectionExtensions.Parse(data.Gradient2Colors.Direction);
        }

        var chess = catalog.Items.OfType<ChessboardPattern>().First();
        if (data.Chessboard is not null)
        {
            chess.MaxSquares = Math.Max(1, data.Chessboard.MaxSquares);
            chess.Squares = Math.Clamp(data.Chessboard.Squares, 1, chess.MaxSquares);
        }

        if (data.Phase is not null) catalog.Items.OfType<PhasePattern>().First().Direction = PatternDirectionExtensions.Parse(data.Phase.Direction);

        if (data.ColorTemperature is not null) { catalog.Items.OfType<ColorTemperaturePattern>().First().Temperature = Math.Clamp(data.ColorTemperature.Kelvin, 2400, 9550); }
    }

public sealed class ConfigurationData
    {
        public DisplayData? Display { get; set; }
        public int SelectedPatternIndex { get; set; } = 0;
        public GeometryData? Geometry { get; set; }
        public SingleColorData? SingleColor { get; set; }
        public GrayData? Gray { get; set; }
        public GammaData? Gamma { get; set; }
        public BarsData? Bars { get; set; }
        public DirectionData? ColorBars { get; set; }
        public GradientToBlackData? GradientToBlack { get; set; }
        public Gradient2ColorsData? Gradient2Colors { get; set; }
        public ChessboardData? Chessboard { get; set; }
        public DirectionData? Phase { get; set; }
        public ColorTemperatureData? ColorTemperature { get; set; }
        public SettingsData? Settings { get; set; }
    }

    public sealed class SettingsData { public string StartupMode { get; set; } = "last"; public string Theme { get; set; } = "dark"; public string LanguageCode { get; set; } = "it"; public bool AutoShowPattern { get; set; } = false; public string SaveMode { get; set; } = "on_exit"; public int DefaultPatternIndex { get; set; } = 0; public int DefaultMonitor { get; set; } = 1; public int DefaultHorizontalScreens { get; set; } = 3; public int DefaultVerticalScreens { get; set; } = 3; }
    public sealed class DisplayData { public int TargetMonitor { get; set; } = 1; public int HorizontalScreens { get; set; } = 3; public int VerticalScreens { get; set; } = 3; public int MaxScreens { get; set; } = 40; }
    public sealed class GeometryData { public int Lines { get; set; } = 10; public bool Diagonal { get; set; } = false; public bool Circle { get; set; } = false; public int MaxLines { get; set; } = 16; }
    public sealed class SingleColorData { public string Color { get; set; } = "black"; }
    public sealed class GrayData { public double White { get; set; } = 50; }
    public sealed class GammaData { public double Value { get; set; } = 2.2; public int Cells { get; set; } = 16; }
    public sealed class BarsData { public string Color { get; set; } = "white"; public string Direction { get; set; } = "from_left"; public int BarsNum { get; set; } = 8; public int MaxBarsNum { get; set; } = 32; }
    public sealed class DirectionData { public string Direction { get; set; } = "from_left"; }
    public sealed class GradientToBlackData { public string Color { get; set; } = "white"; public string Direction { get; set; } = "from_left"; }
    public sealed class Gradient2ColorsData { public string StartColor { get; set; } = "red"; public string EndColor { get; set; } = "blue"; public string Direction { get; set; } = "from_left"; }
    public sealed class ChessboardData { public int Squares { get; set; } = 10; public int MaxSquares { get; set; } = 20; }
    public sealed class ColorTemperatureData { public int Kelvin { get; set; } = 6500; }

}
