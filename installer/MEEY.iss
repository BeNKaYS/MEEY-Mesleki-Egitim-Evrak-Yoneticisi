#define MyAppName "MEEY - Mesleki Eğitim Evrak Yöneticisi"
#define MyAppVersion "1.0.3"
#define MyAppPublisher "Sercan Özdemir"
#define MyAppURL "https://github.com/BeNKaYS/MEEY-Mesleki-Egitim-Evrak-Yoneticisi"
#define MyAppExeName "MEEY.exe"
#define MySourceDir "..\MEEY\bin\Release\net6.0-windows\win-x64\publish"

[Setup]
AppId={{E4A3E45C-EA0B-4A12-9C23-6A8D3C89F7B1}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\MEEY
DefaultGroupName=MEEY
DisableProgramGroupPage=yes
OutputDir=..\PublishOutput\Installer
OutputBaseFilename=MEEY-Setup-v1.0.3-win-x64
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=..\MEEY\ico\MEEY_desktop.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur"; GroupDescription: "Ek görevler:"; Flags: unchecked

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\MEEY.db"; DestDir: "{app}"; Flags: ignoreversion; Check: FileExists(ExpandConstant('{src}\..\MEEY.db'))

[Icons]
Name: "{autoprograms}\MEEY"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\MEEY"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "MEEY uygulamasını çalıştır"; Flags: nowait postinstall skipifsilent
