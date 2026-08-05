using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;

namespace PatternTester.App;

/// <summary>
/// The fullscreen "MONITOR n" overlay shown briefly on every connected
/// display by View → Identify Monitors. Small in scope but the result
/// of a few hard-won lessons worth preserving as comments here (see
/// each member below): a data-binding pitfall that cost real debugging
/// time, and a native X11 fallback needed because Avalonia's own
/// cross-platform window-placement APIs aren't reliable enough for a
/// borderless, always-on-top overlay on Linux.
/// </summary>
public partial class MonitorIdentificationWindow : Window
{
    public MonitorIdentificationWindow()
    {
        InitializeComponent();
        DataContext = "MONITOR";
    }

    private sealed class MonitorIdentificationViewModel
    {
        public string Title { get; init; } = "";
        public string Details { get; init; } = "";
    }

    /// <param name="details">
    /// Pre-formatted resolution/refresh-rate text (e.g.
    /// "1920x1080@60"), computed by the caller BEFORE this window is
    /// constructed — not looked up lazily in here. This is deliberate:
    /// an earlier version of this class re-assigned DataContext a
    /// second time, inside ShowOnScreen, after the window had already
    /// been constructed and shown once with a simpler DataContext. That
    /// second assignment silently failed to update the visible text
    /// (the exact mechanism was never conclusively identified — the
    /// working theory is a binding-refresh quirk specific to reusing a
    /// classic/reflection binding across two different DataContext
    /// values on the same already-realized control). Setting the full,
    /// final DataContext exactly ONCE, here in the constructor, is the
    /// only version that has been confirmed to reliably work — do not
    /// "simplify" this by moving details-gathering into ShowOnScreen
    /// without re-testing carefully on Windows.
    /// </param>
    public MonitorIdentificationWindow(int monitorNumber, string details)
    {
        InitializeComponent();

        DataContext = new MonitorIdentificationViewModel
        {
            Title = $"MONITOR {monitorNumber}",
            Details = details
        };
    }

    public void ShowOnScreen(Screen screen)
    {
        var bounds = screen.Bounds;

        // NOTE: bounds.Width/Height are in PHYSICAL pixels, while
        // Window.Width/Height are in LOGICAL pixels (DIPs) — at display
        // scaling other than 100% these two units diverge, and using
        // physical pixels directly (as done here) makes this overlay
        // slightly the wrong size relative to the physical screen. This
        // is the same class of bug that was found and fixed in
        // PatternWindow (dividing by screen.Scaling) — it was
        // deliberately NOT carried over to this file, so this overlay
        // currently still has it. See KNOWN_ISSUES for the current
        // status before "fixing" this without checking history first.
        Width = bounds.Width;
        Height = bounds.Height;

        Show();
        Activate();

        if (OperatingSystem.IsLinux())
        {
            // Avalonia's own Position/Width/Height setters are not
            // reliable enough on X11 for a borderless, topmost,
            // precisely-positioned overlay like this one — falling
            // through to raw Xlib calls (via the four DllImports below)
            // is what actually gets the window placed pixel-exact on
            // the target screen. This mirrors, in spirit, the native
            // Win32 SetWindowPos fallback PatternWindow uses on
            // Windows for the same underlying reason: the cross-
            // platform windowing API doesn't cover every platform's
            // quirks for this specific kind of window.
            MoveWindowUsingX11(
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height);
        }
        else
        {
            Position = bounds.Position;
            Width = bounds.Width;
            Height = bounds.Height;
        }

        Topmost = true;
        Activate();
    }

    /// <summary>
    /// Positions and raises the window directly via Xlib, bypassing
    /// Avalonia's own window-management APIs entirely for this one
    /// operation. <see cref="TryGetPlatformHandle"/> gives us the
    /// window's native X11 handle; everything else here is a thin,
    /// synchronous wrapper around the four Xlib calls actually needed
    /// (open a connection, move+resize, raise to the top of the stack,
    /// flush the request queue so it takes effect immediately rather
    /// than waiting for the next natural flush).
    /// </summary>
    private void MoveWindowUsingX11(
        int x,
        int y,
        int width,
        int height)
    {
        var handle = TryGetPlatformHandle();

        if (handle is null)
            return;

        var display = XOpenDisplay(IntPtr.Zero);

        if (display == IntPtr.Zero)
            return;

        try
        {
            XMoveResizeWindow(
                display,
                handle.Handle,
                x,
                y,
                (uint)width,
                (uint)height);

            XRaiseWindow(display, handle.Handle);
            XFlush(display);
        }
        finally
        {
            // Always close the display connection we opened, even if a
            // call above throws — an Xlib connection is a real
            // resource (an open socket to the X server), not something
            // the .NET GC will ever reclaim on its own.
            XCloseDisplay(display);
        }
    }

    // Minimal Xlib P/Invoke surface — just the handful of functions
    // this class actually needs, not a general-purpose X11 binding.
    // libX11.so.6 is part of the base X11 client libraries and can be
    // assumed present on any system with a running X server (including
    // XWayland sessions), which is the same assumption the rest of the
    // Linux-specific code in this project (LinuxMonitorInfoService's
    // reliance on xrandr) already makes.

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XMoveResizeWindow(
        IntPtr display,
        IntPtr window,
        int x,
        int y,
        uint width,
        uint height);

    [DllImport("libX11.so.6")]
    private static extern int XRaiseWindow(
        IntPtr display,
        IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XFlush(
        IntPtr display);
}
