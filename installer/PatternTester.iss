; ============================================================
; Script Inno Setup per PatternTester
; Genera un installer .exe con:
;  - installazione in Program Files
;  - collegamento nel Menu Start
;  - collegamento sul Desktop (opzionale, scelto dall'utente)
;  - disinstallatore automatico (visibile in "App e funzionalita'")
; Compilare con: iscc PatternTester.iss
; (richiede Inno Setup Compiler installato: https://jrsoftware.org/isdl.php)
; ============================================================

#define MyAppName "PatternTester"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "Datasoft Srl"
#define MyAppExeName "PatternTester.App.exe"

; Cartella con l'output di "dotnet publish" (vedi publish.ps1)
#define PublishDir "..\publish"

[Setup]
AppId={{A6F2E9C1-6B4C-4C7B-9C21-3F1E5B2C7A11}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Richiede diritti amministrativi per scrivere in Program Files
PrivilegesRequired=admin
OutputDir=..\installer-output
OutputBaseFilename={#MyAppName}-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
LicenseFile=LICENSE.txt
WizardStyle=modern
; Se hai un'icona personalizzata, decommenta e imposta il percorso:
SetupIconFile=..\src\PatternTester.App\Assets\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesInstallIn64BitMode=x64compatible


[Languages]
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Copia TUTTO il contenuto della cartella di publish (exe + eventuali dll native)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "CHANGELOG.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "KNOWN_ISSUES.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\CHANGELOG.txt"; Description: "{cm:ViewChangelog}"; Flags: postinstall shellexec skipifsilent unchecked
Filename: "{app}\KNOWN_ISSUES.txt"; Description: "{cm:ViewKnownIssues}"; Flags: postinstall shellexec skipifsilent unchecked

[UninstallDelete]
; Rimuove eventuali file generati a runtime nella cartella di installazione
Type: filesandordirs; Name: "{app}"


[CustomMessages]
italian.ViewChangelog=Visualizza il changelog
english.ViewChangelog=View changelog
italian.ViewKnownIssues=Visualizza i problemi noti
english.ViewKnownIssues=View known issues

