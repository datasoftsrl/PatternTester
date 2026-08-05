namespace PatternTester.Core.Models;

/// <summary>
/// The part of the current session state that describes WHERE and HOW
/// the pattern is being shown (as opposed to <c>ApplicationSettings</c>,
/// which describes user preferences about the app itself). Persisted
/// and restored by <c>PatternTester.Infrastructure.ConfigurationService</c>.
/// </summary>
public sealed class DisplaySettings
{
    /// <summary>1-based index of the selected monitor, matching the numbering shown in the Display menu and by "Identify Monitors".</summary>
    public int TargetMonitor { get; set; } = 1;

    public int HorizontalScreens { get; set; } = 3;
    public int VerticalScreens { get; set; } = 3;

    /// <summary>Upper bound offered for <see cref="HorizontalScreens"/>/<see cref="VerticalScreens"/> in the UI; not a hard technical limit, just a sane cap on the grid size.</summary>
    public int MaxScreens { get; set; } = 40;
}
