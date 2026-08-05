# Changelog

## 1.0.0

### Multi-Monitor (Windows)

- Fixed a bug where the pattern was always shown on monitor 1, even after selecting a different monitor from the **Display** menu (the issue was in the menu's data binding, not in window placement).
- Added an application manifest with Per-Monitor-V2 DPI awareness, required for correct positioning on Windows systems with multiple monitors using different scaling percentages.
- Added a native Win32 placement fallback as extra protection against known Avalonia Windows-backend bugs in multi-monitor setups.
- Fixed a rendering bug affecting scaling percentages other than 100% (e.g. 125%): the pattern could appear clipped at the edges (for example, an incomplete border on the Geometry pattern).

### Info Monitor Window

- Full data now available on Windows too (resolution, refresh rate, physical size, operating system, etc.) — previously available on Linux only.
- Fixed duplicated/wrapped rows.
- The window now stays visible above the fullscreen pattern even with a single monitor, instead of ending up hidden behind it.

### Identify Monitors Window

- Added resolution (and, on Windows, refresh rate) below the monitor number, to make it easier to recognize each screen at a glance in multi-monitor setups.

### Appearance and Customization

- New application icon (color bars).
- Selectable light/dark application theme from Settings, with the preference saved across restarts.
- Settings window reorganized into tabs (Appearance, Startup, Default Values, Saving).
- Consistent typography across every window of the application.
- Active monitor/columns/rows are now highlighted in the **Display** menu, correctly restored on restart.

### About Window

- Removed duplicated rows (Developed by / License).
- Fixed alignment of all fields (Developed by, License, Version, Website, GitHub) onto a single column.

### Distribution

- Added an automated build/packaging script and a Windows installer (Inno Setup), producing a self-contained executable that does not require .NET to be preinstalled on the target machine.
- Installer shows the GPL-3.0 license before installation and offers optional changelog/known-issues pages at the end, with a fully translated (Italian/English) setup wizard.
