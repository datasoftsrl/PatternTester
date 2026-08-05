# User Guide

Welcome to the PatternTester User Guide.

This document explains how to install, configure and use PatternTester for professional monitor evaluation.

---

# Introduction

PatternTester is a cross-platform (Windows and Linux) monitor test pattern generator, designed to evaluate display characteristics such as geometry, gamma, color accuracy, sharpness and uniformity. It supports multiple monitors simultaneously, each with independent configuration.

Typical use cases:

- Display calibration and quality control
- Comparing multiple monitors side by side
- AV installation testing
- Enthusiast/professional display evaluation

---

# Main Window

The main window is the control panel: it lets you choose a pattern, configure its parameters, and choose which monitor to show it on. The actual test pattern is displayed full-screen in a separate window, so the main window stays available to adjust settings while you look at the result.

## Menu Bar

| Menu | Contains |
|---|---|
| **File** | Save configuration, Settings, Exit |
| **Display** | Monitor selection, number of columns/rows for the split-screen patterns |
| **Pattern** | Choice of test pattern (see [Pattern Reference](patterns.md)) |
| **View** | Show pattern (fullscreen), Info Monitor, Identify Monitors |
| **Help** | User guide, About |

## Pattern Configuration Panel

Below the menu, a panel shows the parameters for the currently selected pattern (for example, gamma value, grid size, number of color bars, gradient direction). Only the fields relevant to the active pattern are shown; the rest stay hidden.

---

# Choosing a Monitor

Open **Display → Monitor** to pick which connected screen the pattern will appear on. The monitor currently selected is highlighted. This works the same way whether one or several monitors are connected, and the choice is remembered the next time you launch PatternTester (see [Settings](#settings)).

## Identifying Monitors

If you are not sure which physical screen corresponds to which number in the menu, use **View → Identify Monitors**. A large label briefly appears on every connected screen, showing its number, resolution and (on Windows) refresh rate — for example `MONITOR 2` followed by `1920x1080@60`. This is especially useful right after connecting a new monitor or in installations with several identical screens.

## Info Monitor

**View → Info Monitor** opens a panel with detailed information about the selected screen: resolution, refresh rate, physical size, color depth, scaling, orientation, and information about the operating system session. Available fields differ slightly between Windows and Linux, depending on what each platform exposes.

---

# Columns and Rows (Split-Screen Patterns)

Some patterns (for example the chessboard) are drawn as a grid. **Display → Columns** and **Display → Rows** control how many horizontal and vertical divisions are used. The active values are highlighted in the menu, the same way the active monitor is.

---

# Fullscreen Display

**View → Show Pattern** displays the currently selected pattern fullscreen on the chosen monitor, above every other window. To go back to the controls:

- **Right-click** anywhere on the pattern, or
- Untick **View → Show Pattern** again from the main window.

The pattern window automatically matches the exact resolution and position of the target monitor, including on systems where different monitors use different Windows display-scaling percentages.

---

# Settings

**File → Settings** opens a dedicated window, organized in tabs:

| Tab | Contents |
|---|---|
| **Appearance** | Light or dark application theme |
| **Startup** | Whether to reopen with the last-used configuration or with defaults; whether to show the pattern automatically at startup |
| **Default Values** | Default pattern, monitor, language, columns and rows used when "start with defaults" is selected |
| **Saving** | When to persist the configuration: on exit, on every change, or only when **File → Save** is used manually |

Settings (and, depending on the chosen save mode, the last-used pattern/monitor/layout) are stored in a per-user configuration file:

- **Windows:** `%APPDATA%\PatternTester\patterntester.json`
- **Linux:** `~/.config/PatternTester/patterntester.json`

---

# Language

PatternTester ships with Italian and English translations, selectable from **Settings → Default Values → Language**.

---

# Typical Workflow

1. Connect and, if needed, identify your monitor(s) (**View → Identify Monitors**).
2. Select the target monitor (**Display → Monitor**).
3. Choose a pattern (**Pattern** menu) and adjust its parameters in the main panel.
4. Show it fullscreen (**View → Show Pattern**).
5. Evaluate the display; right-click to return to the controls.
6. Repeat for each pattern you want to check, and for each monitor.

---

# Troubleshooting

## The pattern looks cut off or larger than the screen

This is almost always a Windows display-scaling issue (e.g. 125% or 150% scaling). PatternTester accounts for per-monitor scaling automatically; if you still see this, check **Info Monitor** for the scaling percentage reported for that screen, and make sure you are running the latest version.

## The pattern appears on the wrong monitor

Use **View → Identify Monitors** to confirm the physical numbering, then re-select the correct one from **Display → Monitor**.

## The "Info Monitor" window is missing information

Some fields (in particular per-channel color depth) are not exposed by Windows without parsing the monitor's raw EDID data, which PatternTester does not currently do; these fields show "N/A". On Linux, the same information is normally available through `xrandr`.

## After closing "Info Monitor", the main window ends up behind the pattern

Make sure you are on the latest release — this was a known issue in earlier builds, fixed by keeping the main window on top while returning from the info panel.

---

# See Also

- [Pattern Reference](patterns.md) — what each pattern tests and how to read it
- [Windows notes](windows.md)
- [Linux notes](linux.md)
- [FAQ](faq.md)
