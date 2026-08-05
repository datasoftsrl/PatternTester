<#
.SYNOPSIS
    Compila PatternTester in eseguibile self-contained e genera un
    installer Windows (.exe) pronto per essere distribuito su nuove
    macchine, senza bisogno che abbiano il .NET Runtime installato.

.USAGE
    Questo script va tenuto in installer\, insieme a PatternTester.iss.
    Eseguilo da PowerShell, dalla cartella "installer" oppure da
    qualunque altra cartella (i percorsi sono calcolati automaticamente
    rispetto alla posizione dello script, non alla cartella corrente):
        .\build-and-package.ps1

    Parametri opzionali:
        .\build-and-package.ps1 -Version "1.1.0" -SkipInstaller
#>

param(
    [string]$Version = "1.0.0",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

# Lo script vive in installer\, quindi la root del repository è la
# cartella PADRE di dove si trova questo file.
$InstallerDir = $PSScriptRoot
$RepoRoot     = Split-Path $InstallerDir -Parent

$ProjectPath  = Join-Path $RepoRoot "src\PatternTester.App\PatternTester.App.csproj"
$PublishDir   = Join-Path $RepoRoot "publish"
$IssFile      = Join-Path $InstallerDir "PatternTester.iss"
$OutputDir    = Join-Path $RepoRoot "installer-output"

Write-Host "=== 1. Pulizia build precedenti ===" -ForegroundColor Cyan
if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }
dotnet clean $ProjectPath -c Release | Out-Null

Write-Host "=== 2. Publish self-contained (win-x64, single file) ===" -ForegroundColor Cyan
dotnet publish $ProjectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:Version=$Version `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish fallito, controlla gli errori sopra." -ForegroundColor Red
    exit 1
}

$ExePath = Join-Path $PublishDir "PatternTester.App.exe"
if (-not (Test-Path $ExePath)) {
    Write-Host "ATTENZIONE: eseguibile non trovato in $ExePath" -ForegroundColor Red
    exit 1
}

Write-Host "Eseguibile creato: $ExePath" -ForegroundColor Green

if ($SkipInstaller) {
    Write-Host "Installer saltato (-SkipInstaller). Fine." -ForegroundColor Yellow
    exit 0
}

Write-Host "=== 3. Verifica presenza Inno Setup (ISCC.exe) ===" -ForegroundColor Cyan

function Find-ISCC {
    $candidate = Get-Command iscc.exe -ErrorAction SilentlyContinue
    if ($candidate) { return $candidate.Source }

    $defaultPaths = @(
        "$Env:ProgramFiles(x86)\Inno Setup 7\ISCC.exe",
        "$Env:ProgramFiles\Inno Setup 7\ISCC.exe"
    )
    foreach ($p in $defaultPaths) {
        if (Test-Path $p) { return $p }
    }
    return $null
}

$IsccPath = Find-ISCC

if (-not $IsccPath) {
    Write-Host "Inno Setup non trovato. Provo a installarlo con winget..." -ForegroundColor Yellow

    if (Get-Command winget -ErrorAction SilentlyContinue) {
        winget install --id JRSoftware.InnoSetup -e --source winget `
            --accept-package-agreements --accept-source-agreements
        $IsccPath = Find-ISCC
    }

    if (-not $IsccPath) {
        Write-Host ""
        Write-Host "Impossibile trovare o installare Inno Setup automaticamente." -ForegroundColor Red
        Write-Host "Scaricalo manualmente (gratuito) da: https://jrsoftware.org/isdl.php"
        Write-Host "Poi rilancia questo script."
        exit 1
    }
}

Write-Host "Inno Setup trovato: $IsccPath" -ForegroundColor Green

Write-Host "=== 4. Generazione installer ===" -ForegroundColor Cyan
if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir | Out-Null }

& $IsccPath `
    "/DMyAppVersion=$Version" `
    $IssFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "Generazione installer fallita, controlla gli errori sopra." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Completato ===" -ForegroundColor Green
Write-Host "Eseguibile standalone : $ExePath"
Write-Host "Installer distribuibile: $OutputDir\PatternTester-Setup-$Version.exe"
Write-Host ""
Write-Host "Copia il file 'PatternTester-Setup-$Version.exe' sulla nuova macchina ed eseguilo:"
Write-Host "non serve installare .NET, e' tutto incluso (self-contained)."
