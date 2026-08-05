# PatternTester

Cross-platform (Windows / Linux) monitor test pattern generator, built with C#/.NET and Avalonia UI. Displays fullscreen test patterns — geometry, gamma, color bars, gradients, and more — for display calibration, quality control and AV installation testing, with proper multi-monitor support including mixed per-monitor display scaling.

Released under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html).

---

## Features

- 11 built-in test patterns (see [Pattern Reference](docs/patterns.md))
- Full multi-monitor support, correctly handling monitors with different Windows display-scaling percentages
- Monitor identification overlay and a detailed per-monitor info panel (resolution, refresh rate, physical size, color depth, ...)
- Light and dark application theme, remembered across restarts
- Italian and English interface, extendable via a plain JSON file — no recompilation needed (see [External Languages](docs/PatternTester_Lingue.md))
- Self-contained Windows installer and a Debian/`.deb` package for Linux

---

## Stack

- C# / .NET 10
- Avalonia UI 12.1
- JSON configuration, per-user
- MVVM-oriented application layer

---

## Project Structure

```
src/
  PatternTester.Core/            Platform-independent pattern definitions and models
  PatternTester.Rendering/       Avalonia drawing implementation (the pattern canvas)
  PatternTester.Infrastructure/  Persistent configuration (settings file read/write)
  PatternTester.App/             Desktop UI: windows, view models, platform services
tests/
  PatternTester.Core.Tests/      Unit tests for the platform-independent pattern logic
installer/                       Windows installer (Inno Setup) + build/packaging script
docs/                            Documentation site (MkDocs)
```

See the [Developer Guide](docs/developer.md) for the reasoning behind this structure and for the trickier implementation details (DPI/multi-monitor handling, the theming system, configuration persistence).

---

## Quick Start

```bash
dotnet restore
dotnet build
dotnet run --project src/PatternTester.App
```

Platform-specific build, packaging and installation instructions:

- **Windows** — [`README_WINDOWS.md`](README_WINDOWS.md)
- **Linux (LMDE / Debian-based)** — [`README_LMDE.md`](README_LMDE.md)

---

## Documentation

Full documentation — user guide, developer guide, architecture, pattern reference, FAQ — is available as a [MkDocs](https://www.mkdocs.org/) site under [`docs/`](docs/index.md). To browse it locally:

```bash
pip install mkdocs mkdocs-material
mkdocs serve
```

then open http://127.0.0.1:8000.

---

## Testing

```bash
dotnet test
```

`PatternTester.Core` has no dependency on Avalonia or any UI framework, which is what makes it independently unit-testable — see [Architecture](docs/architecture.md) for why the project is split this way.

---

## Contributing

Feature branches, small commits, descriptive messages. Pull requests touching shared infrastructure (theming, configuration persistence, multi-monitor/DPI handling) should reference the relevant section of the [Developer Guide](docs/developer.md). Update the documentation alongside any change that affects behavior a user or another developer would need to know about.
