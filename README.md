# YouTube Background

<img src="assets/icon.png" alt="Ícone YouTube Background" width="88">

Controle o YouTube por atalhos globais no Windows, mesmo usando outro programa ou outra aba. Aplicativo com ícone na bandeja, extensão Chrome e instalador guiado em português.

## Funcionalidades

- Avançar/retroceder 5 segundos com `Ctrl + Alt + ←/→`, configuráveis entre 1 e 120 segundos.
- Gravar combinações, teclas individuais e eventos multimídia, incluindo Volume +/− de controles giratórios compatíveis.
- Escolher o último vídeo iniciado entre os que estão em reprodução, sem mudar o foco da janela.
- Suspender atalhos, inicializar com Windows e preservar configurações em atualizações.
- Instalador `.exe` com orientação para carregar a extensão e gravar atalhos; desinstalação pela lista de aplicativos do Windows.

**Versão atual:** 1.2.0. Windows 10/11 x64 e Chrome em perfil normal. YouTube Music, modo anônimo e vídeos incorporados não fazem parte desta versão.

## Instalar e usar

Execute o instalador gerado e siga as etapas. A extensão é adicionada manualmente em `chrome://extensions`, usando **Modo do desenvolvedor → Carregar sem compactação**, com a pasta `%LOCALAPPDATA%\YouTubeBackground\extension`.

O guia completo está em [LEIA-ME.md](LEIA-ME.md), e a versão HTML incluída no instalador está em [setup/guide.html](setup/guide.html). Os instaladores não são armazenados no Git; podem ser gerados a partir deste código.

Ao configurar Volume +/− como atalho, esses eventos passam a controlar o vídeo até suspender os atalhos. O controle físico precisa emitir eventos reconhecidos pelo Windows.

## Desenvolvimento

Requisitos: **Windows x64**, **.NET SDK 10** e **Node.js**. Para o instalador, use **Inno Setup 6.7.3** ou compatível. Não há pacotes npm nem dependências NuGet de terceiros no aplicativo.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
.\build-installer.ps1 -Compiler 'C:\caminho\Inno Setup 6\ISCC.exe' -OutputDirectory .\dist
```

O primeiro comando executa testes de lógica/protocolo e publica o aplicativo e o auxiliar com runtime incluído em `package/`. O segundo gera o instalador em `dist/`. A solução [YouTubeBackground.slnx](YouTubeBackground.slnx) reúne os projetos C#.

## Organização

| Pasta | Conteúdo |
| --- | --- |
| `src/App` | Aplicativo WinForms, bandeja, teclas globais e configurações |
| `src/Host` | Ponte Native Messaging entre Chrome e aplicativo |
| `src/Shared` | Protocolo e named pipe |
| `extension` | Extensão Manifest V3, seleção do vídeo e saltos |
| `setup` | Instalador Inno Setup e guia do usuário |
| `assets` | Ícone vetorial, ICO e imagens do instalador |
| `tests` | Lógica, protocolo, entrada Windows, bandeja e navegador |

## Testes e contexto

[VALIDACAO.md](VALIDACAO.md) registra testes realizados e limitações. [docs/PROJECT_STATUS.md](docs/PROJECT_STATUS.md) preserva as decisões e o estado do projeto. [AGENTS.md](AGENTS.md) orienta o Codex em futuras tarefas.

Testes de interface exigem desktop interativo e podem conflitar com uma instância instalada. Consulte as instruções antes de rodá-los. O teste de navegador usa um perfil separado e mídia controlada; `--live` usa um vídeo público real.

As preferências ficam apenas no computador do usuário. O aplicativo não envia títulos ou histórico para servidores externos. A conexão local aceita somente a extensão com ID estável `jobnoaknnkhgmfjfbfkelbpfgabdogfb`.
