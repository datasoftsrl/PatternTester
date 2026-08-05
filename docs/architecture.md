# Software Architecture

PatternTester is split into four .NET projects, each with a single, narrow responsibility. This page gives the high-level picture; see the [Developer Guide](developer.md) for file-by-file detail and the reasoning behind specific design decisions (theming, DPI handling, configuration persistence).

---

## Project Overview

```
PatternTester.Core            → what a pattern IS (no UI, no platform code)
        ↓
PatternTester.Rendering       → turns a pattern into pixels (Avalonia canvas)
        ↓
PatternTester.App             → windows, menus, view models, platform services
        ↑
PatternTester.Infrastructure  → reads/writes the JSON settings file
```

`Infrastructure` is consumed by `App` directly (it has no dependency of its own on `Core` or `Rendering`); the arrow above reflects data flow, not a compile-time dependency chain.

---

## `PatternTester.Core`

Defines `PatternBase` (the abstract class every pattern derives from) and the 11 concrete pattern classes, plus `PatternCatalog`, the ordered list of all built-in patterns. Has no dependency on Avalonia or any UI framework, so it is unit-testable in isolation — see `tests/PatternTester.Core.Tests`. Full public surface documented in the [API Reference](api.md).

## `PatternTester.Rendering`

`PatternCanvas`, the Avalonia control that hosts a `PatternBase` and calls its `Render` method once per grid cell, handling the column/row layout (see [Display → Columns/Rows](user.md#columns-and-rows-split-screen-patterns)) and the optional cell border. This is the one place where "what a pattern draws" (`Core`) meets an actual UI framework.

## `PatternTester.Infrastructure`

`ConfigurationService`, `ApplicationSettings` and `DisplaySettings`: reading and writing the single per-user JSON configuration file (`%APPDATA%\PatternTester\patterntester.json` on Windows, `~/.config/PatternTester/patterntester.json` on Linux). No UI or pattern-rendering knowledge — just persistence.

## `PatternTester.App`

The Avalonia application itself:

- **Windows** (`MainWindow`, `PatternWindow`, `SettingsWindow`, `MonitorInfoWindow`, `MonitorIdentificationWindow`, `AboutWindow`) — one `.axaml`/`.axaml.cs` pair per window, each fairly self-contained.
- **`ViewModels/`** — primarily `MainWindowViewModel`, which owns the current pattern/monitor/layout selection, the loaded `ApplicationSettings`/`DisplaySettings`, and exposes them as bindable properties for the main window and the settings dialog.
- **`Services/`** — platform-specific functionality that doesn't belong in `Core` or `Infrastructure`: `WindowsMonitorInfoService` and `LinuxMonitorInfoService` (populate Info Monitor), `Win32WindowPlacement` (native fallback for positioning the fullscreen pattern window on Windows), `LocalizationService` (loads `Languages/*.json`).
- **`Styles/Theme.axaml`** — shared design tokens (accent color, card style, base font size, light/dark-specific colors), merged after Avalonia's own `FluentTheme` so it can override the defaults without needing custom control templates.

---

## Design Goals

- **Platform independence where it doesn't cost accuracy** — `Core` and `Rendering` are fully cross-platform; platform-specific code is isolated behind small, single-purpose services in `App/Services`, each with its own Windows or Linux implementation rather than a leaky shared abstraction.
- **Testability** — `Core` has no external dependencies, which is what makes `PatternTester.Core.Tests` possible without mocking a UI or a filesystem.
- **Fail soft, not hard** — platform services return a placeholder value (`"N/D"`/`"N/A"`) for data they can't read rather than throwing, so a missing Win32 API or an unusual Linux driver never crashes a window.
