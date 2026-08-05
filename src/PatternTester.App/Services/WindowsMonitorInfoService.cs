using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PatternTester.App.Services;

/// <summary>
/// Legge le informazioni di sistema e del monitor su Windows, come
/// equivalente di LinuxMonitorInfoService (che si appoggia a xrandr,
/// xdpyinfo, uname e variabili XDG_*, tutte cose che non esistono su
/// Windows). Usa solo API Win32 native (user32/gdi32) e la classe
/// Environment/RuntimeInformation della BCL, senza dipendenze NuGet
/// aggiuntive.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsMonitorInfoService
{
    /// <param name="deviceName">
    /// Nome del device cosi' come riportato da Avalonia (Screen.DisplayName),
    /// tipicamente nel formato "\\.\DISPLAY1". Se null/vuoto o se le API
    /// falliscono con questo nome, si tenta un fallback basato sull'indice.
    /// </param>
    /// <param name="monitorIndex">Indice 0-based del monitor selezionato.</param>
    public WindowsMonitorInfo Read(string? deviceName, int monitorIndex)
    {
        var result = new WindowsMonitorInfo
        {
            OperatingSystem = GetOperatingSystemName(),
            DesktopEnvironment = "Windows Desktop (Explorer)",
            SessionType = Environment.GetEnvironmentVariable("SESSIONNAME") ?? "N/D",
            Kernel = Environment.OSVersion.VersionString,
            Architecture = RuntimeInformation.OSArchitecture.ToString()
        };

        var candidates = BuildDeviceNameCandidates(deviceName, monitorIndex);

        foreach (var candidate in candidates)
        {
            if (TryReadDisplaySettings(candidate, result) | TryReadPhysicalSize(candidate, result))
            {
                // Trovato un nome di device che funziona: non serve
                // provare gli altri candidati di fallback.
                break;
            }
        }

        return result;
    }

    private static IEnumerable<string> BuildDeviceNameCandidates(string? deviceName, int monitorIndex)
    {
        if (!string.IsNullOrWhiteSpace(deviceName))
            yield return deviceName;

        // Fallback: se Avalonia non riporta un DisplayName utilizzabile,
        // proviamo il nome standard di Windows per posizione (indice
        // 0-based -> DISPLAY1, DISPLAY2, ...).
        yield return $@"\\.\DISPLAY{monitorIndex + 1}";
    }

    private static bool TryReadDisplaySettings(string deviceName, WindowsMonitorInfo result)
    {
        var mode = new DEVMODE { dmSize = (short)Marshal.SizeOf<DEVMODE>() };

        if (!EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref mode))
            return false;

        var found = false;

        if (mode.dmDisplayFrequency > 1)
        {
            result.RefreshRate = $"{mode.dmDisplayFrequency} Hz";
            found = true;
        }

        if (mode.dmBitsPerPel > 0)
        {
            result.ColorDepth = $"{mode.dmBitsPerPel} bit";
            found = true;
        }

        return found;
    }

    private static bool TryReadPhysicalSize(string deviceName, WindowsMonitorInfo result)
    {
        var hdc = CreateDC(deviceName, deviceName, null, IntPtr.Zero);

        if (hdc == IntPtr.Zero)
            return false;

        try
        {
            var widthMm = GetDeviceCaps(hdc, HORZSIZE);
            var heightMm = GetDeviceCaps(hdc, VERTSIZE);

            if (widthMm <= 0 || heightMm <= 0)
                return false;

            result.PhysicalSize = $"{widthMm} × {heightMm} mm";
            return true;
        }
        finally
        {
            DeleteDC(hdc);
        }
    }

    private static string GetOperatingSystemName()
    {
        var version = Environment.OSVersion.Version;

        // Windows 11 condivide la major/minor version (10.0) con Windows
        // 10: si distinguono solo dal numero di build (>= 22000).
        var friendlyName = version.Build >= 22000 ? "Windows 11" : "Windows 10";

        return $"{friendlyName} (build {version.Build})";
    }

    // --- P/Invoke ---

    private const int ENUM_CURRENT_SETTINGS = -1;
    private const int HORZSIZE = 4;
    private const int VERTSIZE = 6;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool EnumDisplaySettings(
        string deviceName, int modeNum, ref DEVMODE devMode);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CreateDC(
        string driver, string device, string? output, IntPtr initData);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int index);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);
}

public sealed class WindowsMonitorInfo
{
    public string RefreshRate { get; set; } = "N/D";
    public string PhysicalSize { get; set; } = "N/D";
    public string ColorDepth { get; set; } = "N/D";

    // Il numero di bit per canale (bpc) del pannello richiederebbe il
    // parsing dell'EDID grezzo (non esposto da API Win32 standard senza
    // driver/tool aggiuntivi): lasciato N/D di proposito, a differenza
    // di Linux dove xrandr --verbose lo espone direttamente.
    public string PanelColorDepth { get; set; } = "N/D";

    public string OperatingSystem { get; set; } = "N/D";
    public string DesktopEnvironment { get; set; } = "N/D";
    public string SessionType { get; set; } = "N/D";
    public string Kernel { get; set; } = "N/D";
    public string Architecture { get; set; } = "N/D";
}
