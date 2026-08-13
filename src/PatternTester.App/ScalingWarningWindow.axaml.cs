using Avalonia.Controls;
using Avalonia.Interactivity;
using PatternTester.App.Services;

namespace PatternTester.App;

/// <summary>
/// Modal warning shown at startup when one or more connected monitors
/// report a display scaling above 100%. PatternTester needs 1 pattern
/// pixel = 1 physical monitor pixel for colorimetric/geometric
/// verification to be meaningful; OS-level scaling above 100% breaks
/// that 1:1 mapping (see PatternWindow/MonitorIdentificationWindow
/// comments and installer/KNOWN_ISSUES.txt item 1 for the technical
/// background). Rather than silently producing a pattern that doesn't
/// match the monitor's real resolution, this dialog stops the user and
/// makes the choice explicit: fix the scaling and relaunch, or
/// acknowledge the risk and continue anyway.
///
/// Includes a "don't show again" checkbox (persisted by the caller into
/// ApplicationSettings.WarnOnMonitorScaling, also reachable later from
/// Settings → Avvio) but deliberately no attempt to change the OS
/// scaling automatically — there is no safe cross-platform way to do
/// that from here.
/// </summary>
public partial class ScalingWarningWindow : Window
{
    public ScalingWarningWindow()
    {
        InitializeComponent();
    }

    /// <param name="offendingMonitors">
    /// One line per affected monitor, e.g. "Monitor 2 — 150%". Already
    /// formatted by the caller (MainWindow), which is the one that
    /// knows the monitor numbering shown elsewhere in the UI.
    /// </param>
    public ScalingWarningWindow(
        IReadOnlyList<string> offendingMonitors,
        LocalizationService localization)
    {
        InitializeComponent();

        DataContext = new ScalingWarningViewModel
        {
            Title = localization.Translate("ScalingWarning.Title"),
            Message = localization.Translate("ScalingWarning.Message"),
            MonitorList = string.Join(Environment.NewLine, offendingMonitors),
            Suggestion = localization.Translate("ScalingWarning.Suggestion"),
            ContinueText = localization.Translate("ScalingWarning.Continue"),
            ExitText = localization.Translate("ScalingWarning.Exit"),
            DontShowAgainText = localization.Translate("ScalingWarning.DontShowAgain")
        };
    }

    /// <summary>
    /// True if the user chose to continue despite the scaling warning,
    /// false if they chose to exit the application. Read by the caller
    /// after ShowDialog completes.
    /// </summary>
    public bool ContinueRequested { get; private set; }

    /// <summary>
    /// True if the "don't show again" checkbox was ticked, regardless of
    /// whether the user then chose Continue or Exit — the preference is
    /// about future launches, not about the current one. The caller is
    /// responsible for persisting this into ApplicationSettings.
    /// </summary>
    public bool DontShowAgainRequested => DontShowAgainCheckBox.IsChecked == true;

    private void ContinueClick(object? sender, RoutedEventArgs e)
    {
        ContinueRequested = true;
        Close();
    }

    private void ExitClick(object? sender, RoutedEventArgs e)
    {
        ContinueRequested = false;
        Close();
    }

    public sealed class ScalingWarningViewModel
    {
        public string Title { get; init; } = "";
        public string Message { get; init; } = "";
        public string MonitorList { get; init; } = "";
        public string Suggestion { get; init; } = "";
        public string ContinueText { get; init; } = "";
        public string ExitText { get; init; } = "";
        public string DontShowAgainText { get; init; } = "";
    }
}
