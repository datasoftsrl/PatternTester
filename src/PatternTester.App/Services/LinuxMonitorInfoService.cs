using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace PatternTester.App.Services;

/// <summary>
/// Populates the "Info Monitor" panel on Linux by shelling out to
/// <c>xrandr</c>/<c>xdpyinfo</c> and text-parsing their output — the
/// Linux counterpart to <c>WindowsMonitorInfoService</c>, which instead
/// calls Win32 APIs directly. There is no equivalent of those APIs on
/// Linux/X11 without extra native library dependencies, so parsing the
/// standard command-line tools' human-readable output is the pragmatic
/// choice here, at the cost of being inherently more fragile than a
/// real API: it depends on the exact wording/formatting these tools
/// happen to use, which is not a stable, versioned contract the way an
/// OS API is. Every parse step below degrades to "N/D" rather than
/// throwing if the expected text isn't found, so a future xrandr output
/// format change would silently blank a field instead of crashing the
/// info panel.
/// </summary>
public sealed class LinuxMonitorInfoService
{
    /// <param name="displayName">
    /// The X11 output name (e.g. "eDP-1", "HDMI-1") to look up within
    /// xrandr's output. This must match how xrandr itself names the
    /// output — Avalonia's own reported screen name is expected to
    /// already be in this form on Linux.
    /// </param>
    public LinuxMonitorInfo Read(string displayName)
    {
        var xrandr = RunCommand("xrandr", "--verbose");
        var xdpyinfo = RunCommand("xdpyinfo", "");

        var result = new LinuxMonitorInfo
        {
            OperatingSystem = ReadOsRelease(),
            DesktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? "N/D",
            SessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE") ?? "N/D",
            Kernel = RunCommand("uname", "-r").Trim(),
            Architecture = RunCommand("uname", "-m").Trim()
        };

        ParseXrandr(xrandr, displayName, result);
        ParseXdpyinfo(xdpyinfo, result);

        return result;
    }

    /// <summary>
    /// <c>xrandr --verbose</c> output has one "&lt;name&gt; connected ..."
    /// header line per physical output, followed by an indented block of
    /// available display modes for that output — one of which is marked
    /// "*current" (the mode X11 is presently using). This method finds
    /// the header line matching <paramref name="displayName"/>, then
    /// scans forward through its indented block for the "*current" line
    /// to read the live resolution/refresh rate, stopping as soon as
    /// indentation ends (which marks the start of the NEXT output's own
    /// block, i.e. we've walked past the end of this one).
    /// </summary>
    private static void ParseXrandr(
        string output,
        string displayName,
        LinuxMonitorInfo result)
    {
        if (string.IsNullOrWhiteSpace(output))
            return;

        var lines = output.Split('\n');

        var monitorLineIndex = -1;

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(displayName, StringComparison.OrdinalIgnoreCase) &&
                lines[i].Contains(" connected", StringComparison.OrdinalIgnoreCase))
            {
                monitorLineIndex = i;
                break;
            }
        }

        if (monitorLineIndex < 0)
            return;

        var monitorLine = lines[monitorLineIndex];

        // The header line itself carries physical size ("NNNmm x MMMmm")
        // and the CURRENTLY ACTIVE geometry as "WxH+X+Y" (offset can be
        // negative in multi-monitor layouts, hence the optional '-').
        var physicalMatch = Regex.Match(
            monitorLine,
            @"(\d+)mm\s+x\s+(\d+)mm");

        if (physicalMatch.Success)
        {
            result.PhysicalSize =
                $"{physicalMatch.Groups[1].Value} × {physicalMatch.Groups[2].Value} mm";
        }

        var resolutionMatch = Regex.Match(
            monitorLine,
            @"\s(\d+)x(\d+)\+\-?\d+\+\-?\d+");

        if (resolutionMatch.Success)
        {
            result.Resolution =
                $"{resolutionMatch.Groups[1].Value} × {resolutionMatch.Groups[2].Value} px";
        }

        for (var i = monitorLineIndex + 1; i < lines.Length; i++)
        {
            var line = lines[i];

            // Any line that ISN'T indented belongs to the next output's
            // own header — we've left this monitor's block, so stop.
            if (!line.StartsWith(" ") && !line.StartsWith("\t"))
                break;

            // A mode line looks like:
            //   1920x1080     60.00*+  59.94    50.00    59.97
            // i.e. a resolution, then a list of refresh rates, with the
            // CURRENTLY ACTIVE one marked by a trailing '*' directly on
            // the number (an additional trailing '+' marks it as also
            // the preferred mode) — xrandr does not print the word
            // "current" anywhere; matching for that literal text was a
            // bug that made this branch never fire on real output.
            var currentRateMatch = Regex.Match(
                line,
                @"(\d+(?:\.\d+)?)\*");

            if (currentRateMatch.Success)
            {
                result.RefreshRate = $"{currentRateMatch.Groups[1].Value} Hz";

                var modeMatch = Regex.Match(
                    line,
                    @"^\s*(\d+)x(\d+)");

                if (modeMatch.Success)
                {
                    result.Resolution =
                        $"{modeMatch.Groups[1].Value} × {modeMatch.Groups[2].Value} px";
                }

                continue;
            }

            // Panel bit depth is reported inconsistently across drivers
            // and xrandr versions — sometimes as an explicit "dithering
            // depth:" line, sometimes only inferable from a "Supported:
            // ... bpc" capabilities line. Both are checked; whichever is
            // present (there's no guarantee either will be) wins.
            if (line.Contains("dithering depth:", StringComparison.OrdinalIgnoreCase))
            {
                result.PanelColorDepth =
                    line[(line.IndexOf(':') + 1)..].Trim();
            }

            if (line.Contains("supported:", StringComparison.OrdinalIgnoreCase) &&
                line.Contains("bpc", StringComparison.OrdinalIgnoreCase))
            {
                result.PanelColorDepth = line.Trim();
            }
        }
    }

    /// <summary>xdpyinfo reports the X11 SERVER's root window color depth — a session-wide value, not per-monitor, which is the best Linux/X11 equivalent of the per-monitor color-depth WindowsMonitorInfoService reads via EnumDisplaySettings.</summary>
    private static void ParseXdpyinfo(
        string output,
        LinuxMonitorInfo result)
    {
        if (string.IsNullOrWhiteSpace(output))
            return;

        foreach (var line in output.Split('\n'))
        {
            if (line.Contains("depth of root window:", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(line, @"(\d+)\s+planes");

                if (match.Success)
                    result.ColorDepth = $"{match.Groups[1].Value} bit";
            }
        }
    }

    /// <summary>/etc/os-release's PRETTY_NAME is the closest Linux equivalent of the Windows "ProductName" registry value — the standard, distro-agnostic way to get a human-readable OS name.</summary>
    private static string ReadOsRelease()
    {
        try
        {
            if (!File.Exists("/etc/os-release"))
                return "N/D";

            var lines = File.ReadAllLines("/etc/os-release");

            var prettyName = lines.FirstOrDefault(
                x => x.StartsWith("PRETTY_NAME=", StringComparison.Ordinal));

            if (prettyName is null)
                return "N/D";

            return prettyName["PRETTY_NAME=".Length..]
                .Trim()
                .Trim('"');
        }
        catch
        {
            return "N/D";
        }
    }

    /// <summary>
    /// Runs an external command and returns its stdout, or an empty
    /// string on ANY failure (command not found, not an X11 session,
    /// insufficient permissions, etc.) — deliberately swallowed rather
    /// than propagated, since a missing diagnostic tool should blank a
    /// few info fields, not crash the window that's showing them.
    /// </summary>
    private static string RunCommand(string command, string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);

            if (process is null)
                return string.Empty;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }
        catch
        {
            return string.Empty;
        }
    }
}

/// <summary>Mirrors WindowsMonitorInfo's shape so MainWindow.axaml.cs can populate the same MonitorInfoViewModel fields from either platform's service without a shared interface (see the Services section of the Developer Guide for why there isn't one).</summary>
public sealed class LinuxMonitorInfo
{
    public string Resolution { get; set; } = "N/D";
    public string RefreshRate { get; set; } = "N/D";
    public string PhysicalSize { get; set; } = "N/D";
    public string PanelColorDepth { get; set; } = "N/D";
    public string ColorDepth { get; set; } = "N/D";

    public string OperatingSystem { get; set; } = "N/D";
    public string DesktopEnvironment { get; set; } = "N/D";
    public string SessionType { get; set; } = "N/D";
    public string Kernel { get; set; } = "N/D";
    public string Architecture { get; set; } = "N/D";
}
