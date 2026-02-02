; Inno Setup Script para PC Performance Optimizer
; Genera un instalador EXE auto-contenido con todo lo necesario

#define MyAppName "PC Performance Optimizer"
#define MyAppVersion "1.2"
#define MyAppPublisher "KYZ"
#define MyAppExeName "OptimizerApp.exe"
#define MyAppPublisherURL "https://example.com"
#define SourcePath "..\OptimizerApp\bin\Release\net10.0-windows\win-x64\publish\"

[Setup]
AppId={{12345678-1234-1234-1234-123456789012}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppPublisherURL}
AppSupportURL={#MyAppPublisherURL}
AppUpdatesURL={#MyAppPublisherURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
PrivilegesRequired=admin
OutputDir=.\bin\Release
OutputBaseFilename=OptimizerApp-v{#MyAppVersion}-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\OptimizerApp\Assets\AppIcon.ico
CloseApplications=yes
RestartApplications=no
ShowLanguageDialog=auto
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional Icons"; Flags: unchecked
Name: "quicklaunchicon"; Description: "Create a Quick Launch icon"; GroupDescription: "Additional Icons"; Flags: unchecked

[Files]
; Main application executable
Source: "{#SourcePath}{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; All runtime and dependency files - wildcard include
Source: "{#SourcePath}*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourcePath}*.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourcePath}*.json"; DestDir: "{app}"; Flags: ignoreversion
; Assets
Source: "..\OptimizerApp\Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs
; Language packs (será agregados en futuras versiones)
; Source: "{#SourcePath}cs\*"; DestDir: "{app}\cs"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}de\*"; DestDir: "{app}\de"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}es\*"; DestDir: "{app}\es"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}fr\*"; DestDir: "{app}\fr"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}it\*"; DestDir: "{app}\it"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}ja\*"; DestDir: "{app}\ja"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}ko\*"; DestDir: "{app}\ko"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}pl\*"; DestDir: "{app}\pl"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}pt-BR\*"; DestDir: "{app}\pt-BR"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}ru\*"; DestDir: "{app}\ru"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}tr\*"; DestDir: "{app}\tr"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}zh-Hans\*"; DestDir: "{app}\zh-Hans"; Flags: ignoreversion recursesubdirs
; Source: "{#SourcePath}zh-Hant\*"; DestDir: "{app}\zh-Hant"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\AppIcon.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\AppIcon.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKA; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppName}"; ValueType: string; ValueName: "DisplayName"; ValueData: "{#MyAppName}"; Flags: uninsdeletekey
