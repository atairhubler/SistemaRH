; Script de instalação — Inno Setup 6
; Gerado para SistemaRH (.NET 10 WPF, publicação self-contained)
;
; Antes de compilar:
;   1. Execute o comando de publicação abaixo para gerar a pasta E:\RH\Publicado
;      dotnet publish E:\RH\SistemaRH.Desktop\SistemaRH.Desktop.csproj ^
;        -c Release -r win-x64 --self-contained true ^
;        -p:PublishSingleFile=true -o E:\RH\Publicado
;   2. Abra este arquivo no Inno Setup Compiler e clique em Build > Compile

#define AppName      "Sistema RH"
#define AppVersion   "1.0.0"
#define AppPublisher "Ubler Tech"
#define AppExe       "SistemaRH.Desktop.exe"
#define SourceDir    "E:\RH\Publicado"
#define OutputDir    "E:\RH\Instalador"

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
DefaultDirName={autopf}\SistemaRH
DefaultGroupName={#AppName}
AllowNoIcons=yes
OutputDir={#OutputDir}
OutputBaseFilename=SistemaRH_Instalador_v{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
; O banco de dados e configurações ficam em AppData — não são removidos na desinstalação
UninstallDisplayName={#AppName}
CloseApplications=yes
CloseApplicationsFilter=*{#AppExe}*

[Languages]
Name: "pt"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de Trabalho"; GroupDescription: "Atalhos adicionais:"; Flags: unchecked

[Files]
; Copia todos os arquivos da pasta publicada (inclui o .exe self-contained com .NET embutido)
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}";           Filename: "{app}\{#AppExe}"
Name: "{group}\Desinstalar {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";     Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Iniciar {#AppName} agora"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "taskkill"; Parameters: "/f /im {#AppExe}"; Flags: runhidden skipifdoesntexist

; IMPORTANTE: os dados do usuário (banco, configs, backups) ficam em:
;   %AppData%\SistemaRH\
; A remoção desses dados é opcional — o usuário é perguntado durante a desinstalação.

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppDataDir: String;
  Resposta: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    AppDataDir := ExpandConstant('{userappdata}\SistemaRH');
    if DirExists(AppDataDir) then
    begin
      Resposta := MsgBox(
        'Deseja remover também os dados do usuário?' + #13#10 + #13#10 +
        'Isso inclui:' + #13#10 +
        '  • Banco de dados (sistemarh.db)' + #13#10 +
        '  • Configurações (config.json)' + #13#10 +
        '  • Tabelas INSS/IRRF (tabelas_folha.json)' + #13#10 +
        '  • Backups automáticos' + #13#10 + #13#10 +
        'Pasta: ' + AppDataDir + #13#10 + #13#10 +
        'ATENÇÃO: Esta ação é irreversível. Clique Não para preservar os dados.',
        mbConfirmation,
        MB_YESNO or MB_DEFBUTTON2  // "Não" é o botão padrão
      );
      if Resposta = IDYES then
        DelTree(AppDataDir, True, True, True);
    end;
  end;
end;
