# Installation

## Windows

1. Download `PatternTester-Setup-<version>.exe` from the [latest GitHub release](https://github.com/datasoftsrl/PatternTester/releases).
2. Run it and follow the setup wizard — choose your language, accept the GPL-3.0 license, and install.
3. The installer is **self-contained**: the .NET runtime is bundled with the application, so nothing else needs to be installed separately.
4. At the end of setup you can optionally view the changelog and known-issues notes, and launch PatternTester immediately.

After installation, PatternTester is available from the Start Menu and (if selected during setup) a Desktop shortcut.

### Building the installer yourself

See the [Developer Guide](../developer.md#building-the-windows-installer) if you'd rather build the installer from source instead of downloading a release.

---

## Linux (Debian / LMDE)

A prebuilt `.deb` package is produced from the same source using the standard Debian packaging tools. To install one:

```bash
sudo apt install ./PatternTester-<version>-amd64.deb
```

To build the package yourself from source (for example on LMDE 7), see [`README_DEBIAN.md`](https://github.com/datasoftsrl/PatternTester/blob/main/README_DEBIAN.md) in the repository — it covers installing the .NET 10 SDK, the required packaging tools (`dpkg-dev`, `fakeroot`), and the full `dotnet publish` + `dpkg-deb --build` sequence used to produce the `.deb`.

---

## Updating

- **Windows:** run a newer `PatternTester-Setup-<version>.exe`; it installs over the existing version.
- **Linux:** install a newer `.deb` the same way as above; `apt` handles the upgrade.

Your configuration (last-used pattern/monitor, theme, saved defaults) is stored separately from the application files and is preserved across updates — see [Settings](../user.md#settings) in the User Guide for the exact file location.

---

## Uninstall

**Windows:** Settings → Apps → PatternTester → Uninstall (or use the shortcut created in the Start Menu group).

**Linux:**
```bash
sudo apt remove patterntester
```
