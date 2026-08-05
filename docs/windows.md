# Windows

Windows-specific notes for PatternTester.

---

## Supported Versions

PatternTester targets Windows 10 and Windows 11, x64. It ships as a self-contained executable (the .NET runtime is bundled), so no separate runtime installation is needed.

---

## Monitor Detection and DPI

PatternTester detects every connected display automatically, including its resolution, position and per-monitor scaling percentage. The application manifest declares Per-Monitor-V2 DPI awareness, which Windows requires in order to report accurate, independent scaling for each monitor — without it, monitors with different scaling percentages (e.g. one at 100% and another at 125%) would not be handled correctly.

Fullscreen patterns always render at the target monitor's native resolution, regardless of its scaling percentage.

---

## Monitor Identification

**View → Identify Monitors** briefly shows a fullscreen overlay on every connected display with its number and resolution, plus refresh rate on Windows. Example:

```
MONITOR 2
2560x1440@144
```

---

## Info Monitor

On Windows, the **Info Monitor** panel reads resolution, refresh rate, physical size and color depth directly via the Win32 display APIs — no external tools required. Per-channel color depth (bits per channel) is not available through these APIs without parsing the monitor's raw EDID data, which PatternTester does not currently do; that field shows "N/A".

---

## Known Limitations

- Refresh-rate and physical-size lookup relies on matching the display device name Avalonia reports for each screen against Windows' own naming; in rare non-standard multi-adapter configurations this match can fail, in which case the affected fields fall back to "N/A" rather than showing an incorrect value.
- Some USB or virtual display adapters may report incomplete monitor information to Windows itself, independent of PatternTester.

See also: [Installing on Windows](user/installation.md#windows), [Building the installer](developer.md#building-the-windows-installer).
