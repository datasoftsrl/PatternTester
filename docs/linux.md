# Linux

Linux-specific notes for PatternTester. Developed and tested primarily on Linux Mint Debian Edition (LMDE) 7; other Debian-based distributions with a standard X11 desktop should work the same way.

---

## Display Server

Monitor information and window placement on Linux currently rely on `xrandr`, which requires an **X11** session (including XWayland, i.e. X11 applications running under a Wayland compositor via its X11 compatibility layer). Native-Wayland-only detection (without XWayland) is not implemented.

---

## Monitor Detection

PatternTester detects connected displays via `xrandr`/`xdpyinfo`, including resolution, position and, where reported by the driver, refresh rate. See [Info Monitor](user.md#info-monitor) in the User Guide for the exact fields shown.

---

## Monitor Identification

**View → Identify Monitors** works the same way as on Windows: a brief fullscreen overlay on each display shows its number and resolution. Refresh rate is currently only shown on Windows (see [`WindowsMonitorInfoService`](developer.md#services) in the Developer Guide) — on Linux this line shows resolution only.

---

## Configuration File

Settings are stored per-user at `~/.config/PatternTester/patterntester.json` (see [Settings](user.md#settings)).

---

## Installing

See [Installation → Linux](user/installation.md#linux-debian-lmde) for the `.deb` package, or [`README_DEBIAN.md`](https://github.com/datasoftsrl/PatternTester/blob/main/README_DEBIAN.md) to build it from source.

---

## Known Limitations

- Per-channel color depth and some extended monitor metadata depend on what `xrandr --verbose` exposes for a given driver/monitor combination; not every field is available on every system.
- Desktop environments other than the ones PatternTester has been tested against (Cinnamon on LMDE) should work, since the application only depends on standard X11/`xrandr` behavior rather than any desktop-environment-specific API, but have not been explicitly verified.
