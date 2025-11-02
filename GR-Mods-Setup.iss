; Script Inno Setup pour GR Mods
; Cet installeur installe l'application GR Mods sur le système

#define MyAppName "GR Mods"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "LightZirconite"
#define MyAppExeName "GR-Mods.exe"
#define MyAppURL "https://github.com/LightZirconite/GR-Mods"

[Setup]
; Informations de base
AppId={{A1B2C3D4-E5F6-7890-ABCD-123456789ABC}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=Installer
OutputBaseFilename=GR-Mods-Setup-{#MyAppVersion}
SetupIconFile=assets\GR-Mods.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Fichiers de l'application depuis le dossier publish
Source: "GTA5Launcher\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: N'utilisez pas "Flags: ignoreversion" sur les fichiers système

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runascurrentuser

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsAdmin then
  begin
    MsgBox('Ce programme nécessite des droits administrateur pour fonctionner correctement.' + #13#10 + 
           'Veuillez exécuter l''installateur en tant qu''administrateur.', mbError, MB_OK);
    Result := False;
  end;
end;
