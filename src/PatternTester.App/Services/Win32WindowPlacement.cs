using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PatternTester.App.Services;

/// <summary>
/// Posiziona una finestra nativa Windows usando direttamente SetWindowPos,
/// bypassando la logica di posizionamento di Avalonia (Window.Position),
/// che su Windows in setup multi-monitor puo' riportare la finestra
/// sul monitor primario nonostante WindowStartupLocation.Manual e una
/// PixelPoint esplicita (bug noto, es. AvaloniaUI/Avalonia#19255).
/// Da usare SOLO su Windows: verificare OperatingSystem.IsWindows()
/// prima di chiamare MoveTo.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class Win32WindowPlacement
{
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const uint SWP_NOACTIVATE = 0x0010;

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    /// <summary>
    /// Sposta e ridimensiona la finestra nativa alle coordinate fisiche
    /// (pixel reali dello schermo, gli stessi restituiti da Screen.Bounds
    /// in Avalonia) del monitor desiderato, forzandola in primo piano.
    /// </summary>
    public static void MoveTo(IntPtr hWnd, int x, int y, int width, int height, bool topmost)
    {
        if (hWnd == IntPtr.Zero)
            return;

        var insertAfter = topmost ? HWND_TOPMOST : HWND_NOTOPMOST;

        SetWindowPos(
            hWnd,
            insertAfter,
            x,
            y,
            width,
            height,
            SWP_SHOWWINDOW | SWP_NOACTIVATE);
    }
}
