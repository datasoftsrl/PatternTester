# Frequently Asked Questions

---

## Why doesn't the pattern fill the screen, or looks cut off?

This is almost always a Windows display-scaling issue (e.g. 125% or 150%). Check the scaling percentage reported for that monitor in **Info Monitor**, and make sure you're on the latest release — see [Troubleshooting](user.md#troubleshooting) in the User Guide for details.

---

## Why are monitors numbered differently than I expect?

The operating system determines monitor order, not PatternTester. Use **View → Identify Monitors** to see the actual numbering before selecting a monitor from the **Display** menu.

---

## Is refresh rate shown for every monitor?

In the **Info Monitor** panel, yes, on both Windows and Linux (via the Win32 display APIs on Windows, via `xrandr` on Linux) — though on Linux it depends on what the driver reports. In the quick **Identify Monitors** overlay specifically, refresh rate is currently shown on Windows only; on Linux that overlay shows resolution only.

---

## Does PatternTester support CRT monitors?

Yes, as long as the operating system and graphics driver recognize the display and report a usable resolution/refresh rate — PatternTester itself has no CRT-specific restriction.

---

## Does PatternTester support multiple monitors?

Yes — multi-monitor support, including monitors with different Windows scaling percentages, is a core design goal, not an afterthought.

---

## Is internet access required?

No. PatternTester runs entirely offline; nothing is ever sent over the network.

---

## Is PatternTester open source?

Yes, under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html). Source code is on [GitHub](https://github.com/datasoftsrl/PatternTester).

---

## Which platforms are supported?

Windows (10/11, x64) and Linux (Debian-based, tested on LMDE 7, X11/XWayland) are both fully supported today — see [Windows notes](windows.md) and [Linux notes](linux.md) for platform-specific details. Because the UI is built with Avalonia, other platforms are technically possible in the future, but none are currently planned or tested.

---

## Where is my configuration stored?

- Windows: `%APPDATA%\PatternTester\patterntester.json`
- Linux: `~/.config/PatternTester/patterntester.json`

See [Settings](user.md#settings) for what's stored and when it's saved.

---

## Can I add a language other than Italian or English?

Yes — see [External Languages](PatternTester_Lingue.md). No recompilation is needed; PatternTester picks up new `.json` language files automatically at the next launch.
