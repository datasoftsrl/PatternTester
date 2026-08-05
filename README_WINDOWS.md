# PatternTester — Windows Guide

### 1. Prerequisites

Free/open source tools required:

| Tool | Purpose | Download |
|---|---|---|
| **.NET SDK** (version required in `global.json`) | Build and run the project | https://dotnet.microsoft.com/download |
| **Git** | Clone the repository | https://git-scm.com/download/win |
| **Editor** (VS Code + C# Dev Kit, or Visual Studio Community) | Edit the code | https://code.visualstudio.com |
| **Inno Setup** | Build the Windows installer | https://jrsoftware.org/isdl.php |

Quick install via `winget` (PowerShell, run as administrator):
```powershell
winget install --id Git.Git -e
winget install --id Microsoft.DotNet.SDK.10 -e
winget install --id Microsoft.VisualStudioCode -e
winget install --id JRSoftware.InnoSetup -e
```

Verify installation:
```powershell
dotnet --version
git --version
```

### 2. Clone the repository

```powershell
git clone https://github.com/datasoftsrl/PatternTester.git
cd PatternTester
type global.json    # check the exact .NET SDK version required
```

### 3. Build and run (development mode)

```powershell
dotnet restore
dotnet build
dotnet run --project src/PatternTester.App
```

If `dotnet build` complains about a missing SDK version, download exactly the one listed in `global.json` from https://dotnet.microsoft.com/download (multiple SDK versions can coexist without conflicts).

### 4. Create the standalone executable and the installer

The repository includes a script that automates everything: self-contained build (no .NET required on the target machine) + installer creation with Inno Setup.

Files involved:
- `installer/build-and-package.ps1`
- `installer/PatternTester.iss` — Inno Setup configuration script

Run it (works from any directory — paths are resolved relative to the script's own location, not the current directory):
```powershell
.\installer\build-and-package.ps1
```

What the script does, in order:
1. Cleans previous builds
2. Runs `dotnet publish` in self-contained, single-file mode, for `win-x64`
3. Checks whether Inno Setup is installed; if missing, tries to install it automatically via `winget`
4. Builds the installer with Inno Setup

**Generated output** (in the repository root, not inside `installer/`):
- `publish\PatternTester.App.exe` — standalone executable (just copy it, no installation needed)
- `installer-output\PatternTester-Setup-1.0.0.exe` — full installer with Desktop/Start Menu shortcuts, GPL-3.0 license page, translated (Italian/English) setup wizard, and optional changelog/known-issues pages, and automatic uninstall

Optional script parameters:
```powershell
.\installer\build-and-package.ps1 -Version "1.1.0"    # set a different version
.\installer\build-and-package.ps1 -SkipInstaller       # publish only, skip installer creation
```

### 5. Deploying to new machines

To install the application on a new Windows PC, just copy the `PatternTester-Setup-<version>.exe` file (generated in step 4) and run it. It is **self-contained**: there's no need to install .NET separately on the target machine.

### 6. Multi-monitor technical note

The project includes an `app.manifest` (under `src/PatternTester.App/`) declaring **Per-Monitor V2 DPI awareness** compatibility, required for correct window placement on multi-monitor setups, especially with mismatched scaling/DPI between screens. If new executable projects are added to the repository in the future, make sure to attach an equivalent manifest to them as well, referencing it in the corresponding `.csproj` with:
```xml
<ApplicationManifest>app.manifest</ApplicationManifest>
```

### 7. Documentation site (MkDocs)

The `docs/` folder is a [MkDocs](https://www.mkdocs.org/) + [Material theme](https://squidfunk.github.io/mkdocs-material/) site (User Guide, Developer Guide, API reference, pattern reference, etc.), configured by `mkdocs.yml` in the repository root.

**Prerequisites:** Python 3.9+ (comes with `pip`). Get it from https://www.python.org/downloads/windows/ or `winget install --id Python.Python.3.12 -e` if you don't already have it.

Install MkDocs and the Material theme:
```powershell
pip install mkdocs mkdocs-material
```

**Live preview while editing** (auto-reloads in the browser on every save):
```powershell
mkdocs serve
```
Open http://127.0.0.1:8000 in a browser.

**Build the static site** (output goes to `site/`, not committed to the repository):
```powershell
mkdocs build --strict
```
`--strict` turns broken internal links or navigation entries pointing at missing files into build errors instead of silent warnings — always run it before committing documentation changes, since a normal `mkdocs build` (or `mkdocs serve`) will happily start even with broken links.

New pages must be added to the `nav:` section of `mkdocs.yml`, or they exist on disk but are unreachable from the site's navigation menu (MkDocs will list them as "not included in nav" in the build output — that's informational, not an error, but usually a sign a page was forgotten).

### 8. Common issues

| Symptom | Likely cause | Fix |
|---|---|---|
| `dotnet build` fails with "SDK not found" | The SDK version required by `global.json` isn't installed | Install the exact version listed |
| `ISCC.exe not found` during `build-and-package.ps1` | Inno Setup not installed | Install it from https://jrsoftware.org/isdl.php or via `winget install --id JRSoftware.InnoSetup` |
| Installer asks for administrator rights | Expected behavior (installs into Program Files) | Run as administrator |
| Brace/compile errors after pasting code from chat | File was only partially copied | Replace the entire file content, don't merge manually |
| `mkdocs: command not found` after `pip install` | `pip`'s script directory isn't on `PATH` | Run via `python -m mkdocs serve` instead, or add Python's `Scripts` folder to `PATH` |
