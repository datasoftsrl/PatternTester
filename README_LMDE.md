# PatternTester — Debian / LMDE 7 Build and Packaging Guide

This document describes how to build and package **PatternTester 1.0.0**
for **Linux Mint Debian Edition 7 (LMDE 7)**.

## 1. Target environment

Recommended build environment:

- Linux Mint Debian Edition 7 (LMDE 7)
- x86_64 / amd64
- .NET SDK 10
- Git
- Debian packaging tools

Check the system:

``` bash
cat /etc/os-release
uname -m
```

For an x86_64 build, the architecture should normally be `x86_64`.

## 2. Install the .NET 10 SDK

LMDE 7 is Debian-based. The current Microsoft documentation lists Debian
13 as supported for .NET 10.

Install the Microsoft package repository:

``` bash
wget https://packages.microsoft.com/config/debian/13/packages-microsoft-prod.deb   -O packages-microsoft-prod.deb

sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
```

Install the SDK:

``` bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

Verify:

``` bash
dotnet --version
dotnet --list-sdks
```

## 3. Install build and packaging tools

``` bash
sudo apt-get update
sudo apt-get install -y git build-essential dpkg-dev fakeroot
```

Optional validation tools:

``` bash
sudo apt-get install -y lintian devscripts
```

## 4. Clone the repository

``` bash
git clone https://github.com/datasoftsrl/PatternTester.git
cd PatternTester
```

The initial release version is:

``` text
1.0.0
```

## 5. Restore, build and test

Restore:

``` bash
dotnet restore
```

Debug build:

``` bash
dotnet build
```

Release build:

``` bash
dotnet build -c Release
```

Run tests:

``` bash
dotnet test -c Release
```

Do not create a release package if the tests fail.

## 6. Publish for Debian/LMDE 7 x64

The target runtime identifier is `linux-x64`.

### Framework-dependent publish

Use this when the target system already provides the required .NET
runtime:

``` bash
dotnet publish src/PatternTester.App/PatternTester.App.csproj     -c Release     -r linux-x64     --self-contained false     -o publish/linux-x64
```

### Self-contained publish

This is the recommended starting point for a simple distributable
package because the .NET runtime is included:

``` bash
dotnet publish src/PatternTester.App/PatternTester.App.csproj     -c Release     -r linux-x64     --self-contained true     -o publish/linux-x64
```

Self-contained packages are larger but reduce runtime prerequisites on
the target machine.

## 7. Verify the published application

Run:

``` bash
chmod +x publish/linux-x64/PatternTester.App
./publish/linux-x64/PatternTester.App
```

Before packaging, verify:

1.  The application starts.
2.  Multi-monitor configuration works.
3.  Pattern selection works.
4.  Pattern settings work.
5.  Settings opens correctly.
6.  Italian and English can be selected.
7.  The selected language remains active after restarting.
8.  Help / Guide opens the correct external HTML file.
9.  About displays version `1.0.0`.
10. Website and GitHub links open in the default browser.
11. Configuration saving and restoring works.

## 8. External language and help files

PatternTester uses external language resources and help files.

The package must include the contents of:

``` text
src/PatternTester.App/Languages/
```

and the corresponding HTML help files.

Do not remove these files from the published application.

The language system is intentionally external so that additional
language files can be introduced without recompiling the application,
provided they follow the expected JSON structure and resource keys.

## 9. Prepare the Debian package

Create the package tree:

``` bash
rm -rf package

mkdir -p package/patterntester/opt/patterntester
mkdir -p package/patterntester/usr/share/applications
mkdir -p package/patterntester/DEBIAN

cp -a publish/linux-x64/.     package/patterntester/opt/patterntester/
```

The application files are installed under:

``` text
/opt/patterntester/
```

## 10. Debian control file

Create:

``` text
package/patterntester/DEBIAN/control
```

Use:

``` text
Package: patterntester
Version: 1.0.0
Section: graphics
Priority: optional
Architecture: amd64
Maintainer: DataSoft Srl
Description: Pattern Test Generator
 Display test pattern generator for multi-monitor setups.
```

For a self-contained build, no .NET runtime dependency needs to be
declared.

For a framework-dependent build, verify the exact runtime package
dependency on the target LMDE 7 system before adding it to the control
file.

## 11. Desktop launcher

Create:

``` text
package/patterntester/usr/share/applications/patterntester.desktop
```

Use:

``` ini
[Desktop Entry]
Name=PatternTester
Comment=Display test pattern generator
Exec=/opt/patterntester/PatternTester.App
Terminal=false
Type=Application
Categories=Graphics;Utility;
```

If the published executable has a different name, update `Exec=`
accordingly.

## 12. Build the `.deb`

``` bash
dpkg-deb --build package/patterntester     PatternTester-1.0.0-amd64.deb
```

Inspect it:

``` bash
dpkg-deb --info PatternTester-1.0.0-amd64.deb
dpkg-deb --contents PatternTester-1.0.0-amd64.deb
```

If installed, also run:

``` bash
lintian PatternTester-1.0.0-amd64.deb
```

## 13. Install and test the package

Install:

``` bash
sudo apt install ./PatternTester-1.0.0-amd64.deb
```

Verify:

``` bash
dpkg -l | grep patterntester
```

Then launch PatternTester from the desktop menu or directly:

``` bash
/opt/patterntester/PatternTester.App
```

## 14. Remove the package

Remove:

``` bash
sudo apt remove patterntester
```

Purge:

``` bash
sudo apt purge patterntester
```

## 15. Recommended release procedure

From a clean repository:

``` bash
git status

dotnet restore
dotnet build -c Release
dotnet test -c Release

rm -rf publish/linux-x64

dotnet publish src/PatternTester.App/PatternTester.App.csproj     -c Release     -r linux-x64     --self-contained true     -o publish/linux-x64
```

Test the published application manually.

Then create and inspect the Debian package:

``` bash
dpkg-deb --build package/patterntester     PatternTester-1.0.0-amd64.deb

dpkg-deb --info PatternTester-1.0.0-amd64.deb
dpkg-deb --contents PatternTester-1.0.0-amd64.deb
lintian PatternTester-1.0.0-amd64.deb
```

## 16. Clean generated files

``` bash
dotnet clean
rm -rf publish
rm -rf package
rm -f PatternTester-*.deb
```

## 17. Release notes

- Initial application version: **1.0.0**
- Target distribution: **LMDE 7**
- Target architecture: **amd64**
- .NET target: **.NET 10**
- Linux runtime identifier: **linux-x64**
- UI framework: **Avalonia 12.1.0**
- External language JSON files must be included.
- External HTML help files must be included.
- Do not package `bin/` or `obj/` as runtime application content.
- Test the published application before building the final `.deb`.
- A Linux x64 package is architecture-specific and must not be treated
  as an Arm64 package.

## 18. Documentation site (MkDocs)

The `docs/` folder is a [MkDocs](https://www.mkdocs.org/) + Material
theme site (User Guide, Developer Guide, API reference, pattern
reference, etc.), configured by `mkdocs.yml` in the repository root.
Building it has no relationship to building the application itself —
it's independent tooling, only needed when editing documentation.

Install Python 3 and pip if not already present:

``` bash
sudo apt-get install -y python3 python3-pip
```

Install MkDocs and the Material theme:

``` bash
pip install --break-system-packages mkdocs mkdocs-material
```

(`--break-system-packages` is required on Debian/LMDE's system Python
due to PEP 668; using a virtual environment instead is equally valid
if preferred — `python3 -m venv .venv && source .venv/bin/activate`
before the `pip install` above.)

Live preview while editing (auto-reloads in the browser on every save):

``` bash
mkdocs serve
```

Open `http://127.0.0.1:8000` in a browser.

Build the static site (output goes to `site/`, not committed to the
repository):

``` bash
mkdocs build --strict
```

`--strict` turns broken internal links or navigation entries pointing
at missing files into build errors instead of silent warnings — always
run it before committing documentation changes, since a normal
`mkdocs build` (or `mkdocs serve`) will happily start even with broken
links present.

New pages must be added to the `nav:` section of `mkdocs.yml`, or they
exist on disk but stay unreachable from the site's navigation menu.

## References

- Linux Mint package repository: https://packages.linuxmint.com/
- .NET on Debian:
  https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian
- .NET publishing:
  https://learn.microsoft.com/en-us/dotnet/core/deploying/
- Single-file deployment:
  https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview
- `dotnet publish`:
  https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
- PatternTester repository: https://github.com/datasoftsrl/PatternTester
- MkDocs: https://www.mkdocs.org/
- Material for MkDocs: https://squidfunk.github.io/mkdocs-material/
