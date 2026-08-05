# Developer Guide

This guide describes the internal structure of PatternTester and how to work on it.

---

# Requirements

- **.NET SDK** — exact version pinned in [`global.json`](https://github.com/datasoftsrl/PatternTester/blob/main/global.json) (`rollForward: latestFeature`, so any compatible newer feature release also works)
- **Avalonia UI** 12.x (via NuGet, restored automatically)
- **Git**
- An editor: Visual Studio Community, VS Code + C# Dev Kit, or JetBrains Rider all work; the project has no editor-specific files
- On Windows, for the installer only: [Inno Setup](https://jrsoftware.org/isdl.php) (free)

PatternTester targets `net10.0`, plain (not `net10.0-windows`), so the solution builds and runs on both Windows and Linux from the same codebase.

---

# Repository Layout

```
src/
  PatternTester.App/            Avalonia UI application (entry point)
  PatternTester.Core/           Pattern definitions, models, pattern catalog
  PatternTester.Infrastructure/ Configuration persistence
  PatternTester.Rendering/      Pattern drawing/canvas logic
tests/
  PatternTester.Core.Tests/     Unit tests for PatternTester.Core
installer/                      Inno Setup script + packaging PowerShell script
docs/                           This documentation (MkDocs Material)
assets/                         Repository-level images (icon, README screenshots)
```

Each project has a single responsibility: `Core` has no UI or platform dependency, `Rendering` turns pattern definitions into pixels, `Infrastructure` only knows how to read/write the JSON settings file, and `App` wires everything together with Avalonia.

---

# `PatternTester.App` Layout

```
App.axaml / App.axaml.cs        Application entry point, theme setup
MainWindow.axaml(.cs)           Main control window (menus, pattern config panel)
PatternWindow.axaml(.cs)        Fullscreen pattern display window
SettingsWindow.axaml(.cs)       Tabbed settings dialog
MonitorInfoWindow.axaml(.cs)    Detailed per-monitor information panel
MonitorIdentificationWindow.*   Fullscreen "MONITOR n" overlay used by Identify Monitors
AboutWindow.axaml(.cs)          About dialog
ViewModels/                     MainWindowViewModel and supporting view-model types
Services/                       Platform services (see below)
Styles/Theme.axaml              Shared design tokens (colors, card style, base font size)
Languages/it.json, en.json      UI string tables
Assets/                         Icon (.ico/.png) and built-in pattern preview images
```

## Services

| Service | Purpose |
|---|---|
| `LinuxMonitorInfoService` | Populates **Info Monitor** fields on Linux by shelling out to `xrandr`/`xdpyinfo` |
| `WindowsMonitorInfoService` | Same purpose on Windows, via P/Invoke (`EnumDisplaySettings`, `GetDeviceCaps`) — no external process needed |
| `Win32WindowPlacement` | Native `SetWindowPos` fallback used to position the fullscreen pattern window on Windows, in addition to Avalonia's own `Window.Position`/`Screens` APIs |
| `LocalizationService` | Loads `Languages/*.json` and exposes translated strings as dynamic resources |

`LinuxMonitorInfoService` and `WindowsMonitorInfoService` are **not** behind a shared interface; the caller (`MainWindow.axaml.cs`) branches on `OperatingSystem.IsWindows()` and calls the appropriate one directly. Each method degrades gracefully (returns `"N/D"`/`"N/A"` for a field) rather than throwing, so a missing platform API never crashes the info panel.

---

# Theming

`Styles/Theme.axaml` is merged into `App.axaml` **after** `<FluentTheme />`, so its resources take precedence:

- `SystemAccentColor` and its light/dark variants are overridden with the application's own accent color — this alone re-colors every FluentTheme control that uses the accent (buttons, sliders, toggle switches, menu highlights) without needing custom `ControlTemplate`s.
- Colors that must differ between light and dark mode (card/header backgrounds, muted text) live inside `ResourceDictionary.ThemeDictionaries`, keyed `"Dark"` / `"Light"`, and are referenced with `DynamicResource` so they switch live.
- A global `Style Selector="TextBlock"` sets the base font size for the whole app; any window/control with an explicit local `FontSize` overrides it, since local values always win over style setters in Avalonia.
- A global `Style Selector="Window"` sets `Background="{DynamicResource AppWindowBackgroundBrush}"` on every window, replacing FluentTheme's plain white light-mode background with a softer off-white.

The active theme is controlled by `Application.Current.RequestedThemeVariant` (`ThemeVariant.Light` / `.Dark`), toggled from `MainWindowViewModel.UseDarkTheme` / `UseLightTheme` and applied in `MainWindow.ApplyTheme()`.

To add a new shared design token: add it to `Theme.axaml` (inside `ThemeDictionaries` if it must differ per theme, or as a plain resource otherwise) and reference it elsewhere with `{DynamicResource YourKey}` — never hardcode a color for anything that should adapt to the theme.

---

# Configuration Persistence

`PatternTester.Infrastructure.ConfigurationService` reads and writes a single JSON file:

- Windows: `%APPDATA%\PatternTester\patterntester.json`
- Linux: `~/.config/PatternTester/patterntester.json`

It hydrates a `DisplaySettings` object (current pattern, monitor, columns/rows) and an `ApplicationSettings` object (theme, startup mode, save mode, language, default values) at startup, and serializes them back on save. `MainWindowViewModel`'s constructor calls `Load()` and then must explicitly re-run the "highlight the active option" logic (`UpdateColumnSelection`, `UpdateRowSelection`, and — via `RefreshMonitors()` — `UpdateMonitorSelection`) for any menu item that mirrors a loaded value, because the constructor sets backing fields directly rather than through the property setters that normally trigger those updates.

When adding a new persisted setting: add the property to `ApplicationSettings` (or `DisplaySettings`), add it to the private `SettingsData`/equivalent DTO in `ConfigurationService`, and wire both `Load()` and `Save()`.

---

# Multi-Monitor and DPI Handling

This is the area most worth understanding before touching window-placement code.

- Avalonia's `Screen.Bounds` is expressed in **physical pixels**.
- Avalonia's `Window.Width` / `Window.Height` are expressed in **logical pixels (DIPs)**.
- At 100% display scaling the two coincide, which can hide bugs during casual testing; at any other scaling (125%, 150%, ...) a window sized directly from `Screen.Bounds.Width/Height` ends up larger or smaller than the physical screen, clipping content at the edges.

The fix applied throughout `PatternWindow` and `MonitorIdentificationWindow` is to always divide by `Screen.Scaling` when assigning `Width`/`Height`:

```csharp
Width  = screen.Bounds.Width  / screen.Scaling;
Height = screen.Bounds.Height / screen.Scaling;
```

`Window.Position` (a `PixelPoint`) is already in physical pixels and needs no conversion.

On Windows, `PatternWindow` also re-applies its position/size through `Win32WindowPlacement` (native `SetWindowPos`) after `Show()`, as a defensive measure against a known Avalonia Win32-backend quirk where a window's initial placement can be reset during creation. `app.manifest` declares Per-Monitor-V2 DPI awareness, which is required for Windows to report correct per-monitor scaling in the first place — without it, Windows applies DPI virtualization and the numbers above are unreliable.

**Menu selection binding pitfall:** menu items generated from an `ItemsSource` (e.g. the Monitor/Columns/Rows submenus) with a `Click` handler attached to the *parent* `MenuItem` will report the parent as `sender` when the click bubbles up, not the clicked child. Handlers for those menus read `e.Source` instead of `sender`, or (preferred, used for Columns/Rows/Monitor) attach `Click` directly to each generated `ToggleButton` via the `DataTemplate`, so `sender` is correct without relying on bubbling at all.

---

# Building

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/PatternTester.App
```

# Publishing a Windows Executable

```powershell
dotnet publish src/PatternTester.App/PatternTester.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

# Building the Windows Installer

`installer/build-and-package.ps1` automates the two steps above plus Inno Setup compilation:

```powershell
.\installer\build-and-package.ps1
```

Optional parameters: `-Version "1.2.0"`, `-SkipInstaller` (publish only). It looks for `ISCC.exe` (Inno Setup's command-line compiler) and offers to install it via `winget` if missing. The generated installer:

- copies the self-contained executable, `LICENSE.txt`, `CHANGELOG.txt` and `KNOWN_ISSUES.txt` into the install directory;
- shows the GPL-3.0 license (`LicenseFile=`) before installation, translated setup UI (Italian/English via `[Languages]`);
- offers optional "View changelog" / "View known issues" checkboxes on the final page.

`installer/PatternTester.iss` and `installer/build-and-package.ps1` both resolve paths relative to their own location, so they work whether `installer/` sits at the repository root or is moved (as long as the two files stay together and `src/` stays one level above).

---

# Coding Guidelines

- Keep methods short and single-purpose.
- Prefer composition over inheritance.
- Avoid duplicated logic between the Windows and Linux code paths where the underlying data is the same shape (see `WindowsMonitorInfoService`/`LinuxMonitorInfoService` for the accepted exception: platform-specific data sources with a shared consumer).
- All source code comments must be written in English (Italian is fine in commit messages/PRs/discussions, not in code).
- Never hardcode a themeable color; use `Theme.axaml` resources.
- When adding a window, follow the existing pattern: apply `Classes="card"` to grouping `Border`s, `Classes="section-title"` to section headers, and reference `Icon="avares://PatternTester.App/Assets/icon.png"` (the `.ico` is Windows-executable-icon only, not for in-app window icons — Avalonia's own image decoder handles `.png` more reliably than multi-resolution `.ico`).

---

# Version Control

- Feature branches, small commits, descriptive messages.
- Pull requests for anything touching shared infrastructure (`Theme.axaml`, `ConfigurationService`, `ApplicationSettings`).
- Update this documentation when a change affects architecture, persisted settings, or platform-specific behavior.

---

# Known Limitations

See [`installer/KNOWN_ISSUES.txt`](https://github.com/datasoftsrl/PatternTester/blob/main/installer/KNOWN_ISSUES.txt) for the current list (per-channel color depth unavailable on Windows without EDID parsing, refresh-rate lookup depending on Avalonia's reported device name matching Windows' own naming, etc.).
