namespace PatternTester.App.ViewModels;

public sealed class MonitorInfoViewModel
{
    public string MonitorNumber { get; init; } = "N/D";
    public string DisplayName { get; init; } = "N/D";
    public string Resolution { get; init; } = "N/D";
    public string RefreshRate { get; init; } = "N/D";
    public string WorkingArea { get; init; } = "N/D";
    public string PhysicalSize { get; init; } = "N/D";
    public string Scaling { get; init; } = "N/D";
    public string Dpi { get; init; } = "N/D";
    public string ColorDepth { get; init; } = "N/D";
    public string PanelColorDepth { get; init; } = "N/D";
    public string Orientation { get; init; } = "N/D";
    public string IsPrimary { get; init; } = "N/D";

    public string OperatingSystem { get; init; } = "N/D";
    public string DesktopEnvironment { get; init; } = "N/D";
    public string SessionType { get; init; } = "N/D";
    public string Kernel { get; init; } = "N/D";
    public string Architecture { get; init; } = "N/D";
}
