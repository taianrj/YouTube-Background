#define AppVersion "1.2.0"
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
OutputBaseFilename=YouTube-Background-Instalador-v1.2.0
Compression=lzma2
SolidCompression=yes
CloseApplications=yes
RestartApplications=no
DisableWelcomePage=no
DisableFinishedPage=no
SetupLogging=yes

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Messages]
WelcomeLabel1=Controle o YouTube de qualquer janela
WelcomeLabel2=Este assistente vai instalar o YouTube Background e mostrar como conectar a extensão ao Chrome e gravar seus atalhos.%n%nVocê não precisa extrair arquivos nem instalar o .NET.%n%nA extensão é adicionada manualmente ao Chrome em uma etapa guiada após a instalação.
FinishedLabel=O aplicativo foi instalado.%n%nSe ainda não carregou a extensão, siga o Guia de instalação no menu Iniciar. Depois, reproduza um vídeo no YouTube e teste seus atalhos em outra janela.%n%nO ícone vermelho com duas setas fica perto do relógio, inclusive na lista de ícones ocultos.

[Files]
Source: "..\package\app\YouTubeBackground.exe"; DestDir: "{app}\app"; Flags: ignoreversion
Source: "..\package\host\YouTubeBackground.Host.exe"; DestDir: "{app}\host"; Flags: ignoreversion
Source: "..\extension\*"; DestDir: "{app}\extension"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\assets\app.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "guide.html"; DestDir: "{app}"; Flags: ignoreversion
Source: "stop-app.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "stop-app.ps1"; Flags: dontcopy

[Registry]
Root: HKCU; Subkey: "Software\Google\Chrome\NativeMessagingHosts\com.youtubebackground.host"; ValueType: string; ValueName: ""; ValueData: "{app}\native-host.json"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "YouTubeBackground"; ValueData: """{app}\app\YouTubeBackground.exe"""; Flags: uninsdeletevalue; Check: EnableStartup

[Icons]
Name: "{userprograms}\YouTube Background"; Filename: "{app}\app\YouTubeBackground.exe"; Parameters: "--settings"; IconFilename: "{app}\app.ico"
Name: "{userprograms}\YouTube Background - Guia de instalação"; Filename: "{app}\guide.html"; IconFilename: "{app}\app.ico"

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
    MsgBox('Abra o Google Chrome e digite chrome://extensions na barra de endereços.', mbInformation, MB_OK)
  else if not Exec(Chrome, 'chrome://extensions/', '', SW_SHOWNORMAL, ewNoWait, Code) then
    MsgBox('No Chrome, digite chrome://extensions na barra de endereços.', mbInformation, MB_OK);
end;

procedure OpenSettings(Sender: TObject);
var Code: Integer;
begin
  if not Exec(ExpandConstant('{app}\app\YouTubeBackground.exe'), '--settings', '', SW_SHOWNORMAL, ewNoWait, Code) then
    MsgBox('Abra YouTube Background pelo menu Iniciar para configurar os atalhos.', mbInformation, MB_OK);
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
  ChromePage := CreateCustomPage(wpInstalling, 'Conecte ao Chrome', 'Etapa 1 de 2: adicione a extensão do YouTube Background');
  LabelOnPage(ChromePage, 0, '1. Abra chrome://extensions e ative Modo do desenvolvedor.' + #13#10 +
    '2. Clique em Carregar sem compactação.' + #13#10 +
    '3. Selecione a pasta abaixo. Você pode colar o caminho no seletor.' + #13#10 +
    '4. Recarregue as abas do YouTube que já estavam abertas.');
  FolderEdit := TNewEdit.Create(ChromePage);
  FolderEdit.Parent := ChromePage.Surface; FolderEdit.Left := 0; FolderEdit.Top := ScaleY(91);
  FolderEdit.Width := ChromePage.SurfaceWidth; FolderEdit.ReadOnly := True;
  FolderEdit.Text := ExpandConstant('{localappdata}\YouTubeBackground\extension');
  AddButton(ChromePage, 0, 125, 150, 'Abrir Chrome', @OpenChrome);
  AddButton(ChromePage, 160, 125, 150, 'Copiar caminho', @CopyFolder);
  AddButton(ChromePage, 320, 125, 140, 'Abrir pasta', @OpenFolder);
  LabelOnPage(ChromePage, 172, 'Já instalou a extensão em uma versão anterior? Mantenha a mesma extensão.' + #13#10#13#10 +
    'Você pode fazer isso agora ou continuar e usar o guia no menu Iniciar depois. O controle do vídeo só funciona após adicionar a extensão.');

  KeysPage := CreateCustomPage(ChromePage.ID, 'Escolha seus atalhos', 'Etapa 2 de 2: configure e teste sem sair do assistente');
  LabelOnPage(KeysPage, 0, 'Os padrões são Ctrl + Alt + seta esquerda/direita, com saltos de 5 segundos.' + #13#10#13#10 +
    '1. Clique em Abrir configurações.' + #13#10 +
    '2. Clique em Gravar ao lado de Retroceder ou Avançar.');
  LabelOnPage(KeysPage, 88, '3. Pressione uma tecla/combinação ou gire o controle do teclado.' + #13#10 +
    '4. Ajuste os segundos e clique em Salvar.');
  AddButton(KeysPage, 0, 145, 205, 'Abrir configurações', @OpenSettings);
  LabelOnPage(KeysPage, 188, 'Se o controle enviar Volume +/−, ele passa a controlar o vídeo enquanto os atalhos estiverem ativos. Suspender atalhos devolve a função de volume.' + #13#10#13#10 +
    'Para testar: reproduza um vídeo, abra outra janela e use o atalho.');
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer;
begin
  Result := '';
  ExtractTemporaryFile('stop-app.ps1');
  if not Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'),
    '-NoProfile -ExecutionPolicy Bypass -File "' + ExpandConstant('{tmp}\stop-app.ps1') + '" -InstallRoot "' + ExpandConstant('{app}') + '"',
    '', SW_HIDE, ewWaitUntilTerminated, Code) then Result := 'Não foi possível encerrar o aplicativo anterior.'
  else if Code <> 0 then Result := 'Feche o YouTube Background e tente instalar novamente.';
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
      RaiseException('Não foi possível configurar a conexão com o Chrome. Execute o instalador novamente.');
    if not WizardSilent then begin
      Started := Exec(ExpandConstant('{app}\app\YouTubeBackground.exe'), '', '', SW_HIDE, ewNoWait, Code);
      if not Started then MsgBox('O aplicativo foi instalado. Abra YouTube Background pelo menu Iniciar.', mbInformation, MB_OK);
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
    if not UninstallSilent then MsgBox('Aplicativo removido. Remova também a extensão em chrome://extensions.' + #13#10 +
      'Suas preferências foram preservadas para uma futura reinstalação.', mbInformation, MB_OK);
  end;
end;
