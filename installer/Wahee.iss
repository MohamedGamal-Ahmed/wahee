; Inno Setup script template for Wahee
#define MyAppName "Wahee"
#define MyAppVersion "1.0.6"
#define MyAppPublisher "Mohamed Gamal"
#define MyAppExeName "Wahee.exe"

[Setup]
AppId={{B6C8498C-0D6E-45A5-9B2C-4AB8C3E21234}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Wahee
DefaultGroupName=Wahee
OutputDir=..\artifacts\installer
OutputBaseFilename=Wahee-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\artifacts\publish\1.0.6\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Wahee"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Wahee"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Wahee"; Flags: nowait postinstall skipifsilent
