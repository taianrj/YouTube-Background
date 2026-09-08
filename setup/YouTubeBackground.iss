#define AppVersion "1.2.3"
#ifndef ReleaseDir
  #define ReleaseDir "..\dist"
#endif
[Setup]
AppId={{828D4294-764D-45E7-889E-08D87664DB4D}
AppName=YouTube Background
AppVersion={#AppVersion}
AppPublisher=YouTube Background
DefaultDirName={localappdata}\YouTubeBackground
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
WizardStyle=modern
WizardSizePercent=115
SetupIconFile=..\assets\app.ico
WizardImageFile=..\assets\wizard.bmp
WizardSmallImageFile=..\assets\wizard-small.bmp
UninstallDisplayIcon={app}\app\YouTubeBackground.exe
OutputDir={#ReleaseDir}
OutputBaseFilename=YouTube-Background-Instalador-v{#AppVersion}
Compression=lzma2
SolidCompression=yes
CloseApplications=yes
RestartApplications=no
DisableWelcomePage=no
DisableFinishedPage=no
SetupLogging=yes
LanguageDetectionMethod=uilanguage
ShowLanguageDialog=no
UsePreviousLanguage=no

[Languages]
; English must remain first: Inno Setup uses it when the Windows UI language has no match.
Name: "english"; MessagesFile: "compiler:Default.isl,messages\english.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl,messages\brazilianportuguese.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl,messages\spanish.isl"

[Files]
Source: "..\package\app\YouTubeBackground.exe"; DestDir: "{app}\app"; Flags: ignoreversion
Source: "..\package\host\YouTubeBackground.Host.exe"; DestDir: "{app}\host"; Flags: ignoreversion
Source: "..\extension\*"; DestDir: "{app}\extension"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\assets\app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "guide.en.html"; DestDir: "{app}"; DestName: "guide.html"; Languages: english; Flags: ignoreversion
Source: "guide.html"; DestDir: "{app}"; Languages: brazilianportuguese; Flags: ignoreversion
Source: "guide.es.html"; DestDir: "{app}"; DestName: "guide.html"; Languages: spanish; Flags: ignoreversion
Source: "stop-app.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "stop-app.ps1"; Flags: dontcopy

[Registry]
Root: HKCU; Subkey: "Software\Google\Chrome\NativeMessagingHosts\com.youtubebackground.host"; ValueType: string; ValueName: ""; ValueData: "{app}\native-host.json"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "YouTubeBackground"; ValueData: """{app}\app\YouTubeBackground.exe"""; Flags: uninsdeletevalue; Check: EnableStartup

[Icons]
Name: "{userprograms}\YouTube Background"; Filename: "{app}\app\YouTubeBackground.exe"; Parameters: "--settings"; IconFilename: "{app}\app.ico"
Name: "{userprograms}\YouTube Background - {cm:GuideShortcut}"; Filename: "{app}\guide.html"; IconFilename: "{app}\app.ico"

[InstallDelete]
; Remove only our former guide shortcuts when an update changes language.
Type: files; Name: "{userprograms}\YouTube Background - Guia de instalação.lnk"
Type: files; Name: "{userprograms}\YouTube Background - Installation guide.lnk"
Type: files; Name: "{userprograms}\YouTube Background - Guía de instalación.lnk"

[UninstallRun]
Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\stop-app.ps1"" -InstallRoot ""{app}"""; Flags: runhidden waituntilterminated; RunOnceId: "StopYouTubeBackground"

[UninstallDelete]
Type: files; Name: "{app}\native-host.json"
Type: files; Name: "{app}\app\YouTubeBackground.pdb"
Type: files; Name: "{app}\host\YouTubeBackground.Host.pdb"
Type: files; Name: "{app}\uninstall.ps1"
Type: dirifempty; Name: "{app}\app"
Type: dirifempty; Name: "{app}\host"

[Code]
var
  ChromePage, KeysPage: TWizardPage;
  FolderEdit: TNewEdit;
  StartupEnabled, Started: Boolean;

function EnableStartup: Boolean;
begin
  Result := StartupEnabled;
end;

function InitializeSetup: Boolean;
begin
  StartupEnabled := (not FileExists(ExpandConstant('{localappdata}\YouTubeBackground\app\YouTubeBackground.exe'))) or
    RegValueExists(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'YouTubeBackground');
  Started := False;
  Result := True;
end;

procedure LabelOnPage(Page: TWizardPage; Top: Integer; Text: String);
var L: TNewStaticText;
begin
  L := TNewStaticText.Create(Page);
  L.Parent := Page.Surface;
  L.Left := 0; L.Top := ScaleY(Top); L.Width := Page.SurfaceWidth;
  L.AutoSize := False; L.Height := ScaleY(80); L.WordWrap := True;
  L.Caption := Text;
end;

procedure CopyFolder(Sender: TObject);
begin
  WizardForm.ActiveControl := FolderEdit;
  SendMessage(FolderEdit.Handle, $00B1, 0, -1);
  SendMessage(FolderEdit.Handle, $0301, 0, 0);
end;

procedure OpenFolder(Sender: TObject);
var Code: Integer;
begin
  ShellExec('open', ExpandConstant('{app}\extension'), '', '', SW_SHOWNORMAL, ewNoWait, Code);
end;

procedure OpenChrome(Sender: TObject);
var Chrome: String; Code: Integer;
begin
  if not RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe', '', Chrome) then
    if not RegQueryStringValue(HKLM, 'Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe', '', Chrome) then
      Chrome := ExpandConstant('{pf64}\Google\Chrome\Application\chrome.exe');
  if not FileExists(Chrome) then Chrome := ExpandConstant('{pf32}\Google\Chrome\Application\chrome.exe');
  if not FileExists(Chrome) then Chrome := ExpandConstant('{localappdata}\Google\Chrome\Application\chrome.exe');
  if not FileExists(Chrome) then
    MsgBox(CustomMessage('OpenChromeManually'), mbInformation, MB_OK)
  else if not Exec(Chrome, 'chrome://extensions/', '', SW_SHOWNORMAL, ewNoWait, Code) then
    MsgBox(CustomMessage('OpenChromeManually'), mbInformation, MB_OK);
end;

procedure OpenSettings(Sender: TObject);
var Code: Integer;
begin
  if not Exec(ExpandConstant('{app}\app\YouTubeBackground.exe'), '--settings', '', SW_SHOWNORMAL, ewNoWait, Code) then
    MsgBox(CustomMessage('OpenSettingsManually'), mbInformation, MB_OK);
end;

procedure AddButton(Page: TWizardPage; Left, Top, Width: Integer; Text: String; Handler: TNotifyEvent);
var B: TNewButton;
begin
  B := TNewButton.Create(Page);
  B.Parent := Page.Surface; B.Left := ScaleX(Left); B.Top := ScaleY(Top);
  B.Width := ScaleX(Width); B.Height := ScaleY(28); B.Caption := Text; B.OnClick := Handler;
end;

procedure InitializeWizard;
begin
  ChromePage := CreateCustomPage(wpInstalling, CustomMessage('ChromeTitle'), CustomMessage('ChromeSubtitle'));
  LabelOnPage(ChromePage, 0, CustomMessage('ChromeSteps'));
  FolderEdit := TNewEdit.Create(ChromePage);
  FolderEdit.Parent := ChromePage.Surface; FolderEdit.Left := 0; FolderEdit.Top := ScaleY(91);
  FolderEdit.Width := ChromePage.SurfaceWidth; FolderEdit.ReadOnly := True;
  FolderEdit.Text := ExpandConstant('{localappdata}\YouTubeBackground\extension');
  AddButton(ChromePage, 0, 125, 150, CustomMessage('OpenChrome'), @OpenChrome);
  AddButton(ChromePage, 160, 125, 150, CustomMessage('CopyPath'), @CopyFolder);
  AddButton(ChromePage, 320, 125, 140, CustomMessage('OpenFolder'), @OpenFolder);
  LabelOnPage(ChromePage, 172, CustomMessage('ChromeNote'));

  KeysPage := CreateCustomPage(ChromePage.ID, CustomMessage('KeysTitle'), CustomMessage('KeysSubtitle'));
  LabelOnPage(KeysPage, 0, CustomMessage('KeysSteps1'));
  LabelOnPage(KeysPage, 88, CustomMessage('KeysSteps2'));
  AddButton(KeysPage, 0, 145, 205, CustomMessage('OpenSettings'), @OpenSettings);
  LabelOnPage(KeysPage, 188, CustomMessage('KeysNote'));
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer;
begin
  Result := '';
  ExtractTemporaryFile('stop-app.ps1');
  if not Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'),
    '-NoProfile -ExecutionPolicy Bypass -File "' + ExpandConstant('{tmp}\stop-app.ps1') + '" -InstallRoot "' + ExpandConstant('{app}') + '"',
    '', SW_HIDE, ewWaitUntilTerminated, Code) then Result := CustomMessage('StopFailed')
  else if Code <> 0 then Result := CustomMessage('CloseBeforeInstall');
end;

procedure CurStepChanged(CurStep: TSetupStep);
var Lines: TArrayOfString; HostPath: String; Code: Integer;
begin
  if CurStep = ssPostInstall then begin
    HostPath := ExpandConstant('{app}\host\YouTubeBackground.Host.exe');
    StringChangeEx(HostPath, '\', '\\', True);
    SetArrayLength(Lines, 7);
    Lines[0] := '{';
    Lines[1] := '"name":"com.youtubebackground.host",';
    Lines[2] := '"description":"YouTube Background",';
    Lines[3] := '"path":"' + HostPath + '",';
    Lines[4] := '"type":"stdio",';
    Lines[5] := '"allowed_origins":["chrome-extension://jobnoaknnkhgmfjfbfkelbpfgabdogfb/"]';
    Lines[6] := '}';
    if not SaveStringsToUTF8FileWithoutBOM(ExpandConstant('{app}\native-host.json'), Lines, False) then
      RaiseException(CustomMessage('HostSetupFailed'));
    if not WizardSilent then begin
      Started := Exec(ExpandConstant('{app}\app\YouTubeBackground.exe'), '', '', SW_HIDE, ewNoWait, Code);
      if not Started then MsgBox(CustomMessage('StartManually'), mbInformation, MB_OK);
    end;
  end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = ChromePage.ID then FolderEdit.Text := ExpandConstant('{app}\extension');
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then begin
    RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'YouTubeBackground');
    if not UninstallSilent then MsgBox(CustomMessage('Uninstalled'), mbInformation, MB_OK);
  end;
end;
