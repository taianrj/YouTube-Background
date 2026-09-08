# YouTube Background

<img src="assets/icon.png" alt="YouTube Background icon" width="88">

[English](#english) · [Português](#português) · [Español](#español)

## English

Control YouTube with global Windows hotkeys while working in another app or tab, without changing window focus.

**Application / installer: 1.2.3 · Chrome extension: 1.0.1**

### Features and requirements

- Windows 10/11 x64 and Google Chrome. The .NET runtime is included.
- Skip backward/forward with `Ctrl + Alt + ←/→`: 5 seconds by default, configurable from 1 to 120 seconds.
- Record key combinations, individual keys and media keys, including Volume +/− from compatible keyboard dials.
- Control the most recently started video among those still playing, including muted videos. Paused videos and detected ads are excluded; live streams need DVR.
- Tray icon, suspend hotkeys, optional startup with Windows and preferences preserved during installer updates.
- The app, installer and installed guide follow the Windows display language: English, Portuguese or Spanish, with English for other languages.

### Install and use

1. Run `YouTube-Background-Instalador-v1.2.3.exe` and follow the wizard. No administrator access is needed.
2. Open `chrome://extensions`, enable **Developer mode**, choose **Load unpacked** and select `%LOCALAPPDATA%\YouTubeBackground\extension`.
3. Verify the ID: `jobnoaknnkhgmfjfbfkelbpfgabdogfb`. After updating, click **Reload** on the extension; you do not need to remove it.
4. Reload open YouTube tabs, play a video, switch to another app and use the hotkeys.
5. For Incognito windows, enable **Details → Allow in Incognito** manually in the extension. Reload the YouTube tab in that window.

The newest playing video across normal and authorized Incognito windows receives commands. Its title can appear in the tray menu, including for Incognito videos. Titles are not stored in a history or sent to external servers; communication with the app is local.

To change hotkeys, open **Settings**, click **Record**, press the desired key and click **Save**. Assigning Volume +/− makes those keys control the video; **Suspend hotkeys** restores volume control. Physical dials must emit supported Windows key events.

See the [English installation guide](setup/guide.en.html). Remove the app through **Windows Settings → Apps** and remove the extension in Chrome. The executable uninstaller preserves preferences. Installers are generated in `dist/`; binaries are not committed to Git.

### Build and validation

Requires Windows x64, **.NET SDK 10**, **Node.js** and **Inno Setup 6.7.3** or compatible. No npm packages or third-party NuGet dependencies are required by the app.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
.\build-installer.ps1 -Compiler 'C:\path\Inno Setup 6\ISCC.exe' -OutputDirectory .\dist
```

The build runs JavaScript and protocol checks, then publishes self-contained executables and the extension to `package/`. The second command creates the installer in `dist/`. C# projects are in [YouTubeBackground.slnx](YouTubeBackground.slnx).

| Directory | Contents |
| --- | --- |
| `src/App`, `src/Host`, `src/Shared` | Windows app, Native Messaging bridge and shared protocol |
| `extension` | Manifest V3 extension, video selection and seeking |
| `setup`, `assets` | Installer, translations, guides and build artwork |
| `tests` | Logic, protocol, Windows input, tray and browser tests |

See [validation and limitations](VALIDACAO.md), [project status](docs/PROJECT_STATUS.md) and [contributor instructions](AGENTS.md), currently in Portuguese. UI tests need an interactive desktop and can conflict with an installed instance. Browser tests use an isolated profile and simulated media; `--live` uses a real public video.

Incognito playback with a real YouTube video and physical keyboard dials have not yet been validated. YouTube Music, embedded videos and other browsers are outside the supported scope. The app is unsigned and the extension is not on the Chrome Web Store.

## Português

Controle o YouTube com atalhos globais do Windows enquanto trabalha em outro aplicativo ou aba, sem mudar o foco da janela.

**Aplicativo / instalador: 1.2.3 · Extensão Chrome: 1.0.1**

### Funcionalidades e requisitos

- Windows 10/11 x64 e Google Chrome. O runtime .NET acompanha o pacote.
- Avance/retroceda com `Ctrl + Alt + ←/→`: 5 segundos por padrão, configuráveis entre 1 e 120 segundos.
- Grave combinações, teclas individuais e teclas multimídia, incluindo Volume +/− de controles giratórios compatíveis.
- Controle o último vídeo iniciado entre os que continuam em reprodução, inclusive silenciados. Vídeos pausados e anúncios identificados são excluídos; transmissões ao vivo precisam de DVR.
- Ícone na bandeja, suspensão de atalhos, inicialização opcional com Windows e preferências preservadas nas atualizações pelo instalador.
- O aplicativo, o instalador e o guia instalado seguem o idioma de exibição do Windows: inglês, português ou espanhol, com inglês para outros idiomas.

### Instalar e usar

1. Execute `YouTube-Background-Instalador-v1.2.3.exe` e siga o assistente. Não precisa de administrador.
2. Abra `chrome://extensions`, ative **Modo do desenvolvedor**, clique em **Carregar sem compactação** e selecione `%LOCALAPPDATA%\YouTubeBackground\extension`.
3. Confira o ID: `jobnoaknnkhgmfjfbfkelbpfgabdogfb`. Após atualizar, clique em **Recarregar** na extensão; não é necessário removê-la.
4. Recarregue as abas do YouTube, reproduza um vídeo, abra outro aplicativo e use os atalhos.
5. Para janelas anônimas, ative manualmente **Detalhes → Permitir em modo anônimo** na extensão. Recarregue a aba do YouTube nessa janela.

O último vídeo iniciado em reprodução entre janelas normais e anônimas autorizadas recebe os comandos. Seu título pode aparecer na bandeja, inclusive em modo anônimo. Títulos não são gravados em histórico nem enviados a servidores externos; a comunicação com o aplicativo é local.

Para mudar os atalhos, abra **Configurações**, clique em **Gravar**, pressione a tecla desejada e clique em **Salvar**. Ao atribuir Volume +/−, essas teclas controlam o vídeo; **Suspender atalhos** restaura o volume. Controles físicos precisam emitir eventos de teclado reconhecidos pelo Windows.

Consulte o [guia de instalação](setup/guide.html) e o [manual detalhado](LEIA-ME.md). Remova o aplicativo em **Configurações do Windows → Aplicativos** e remova a extensão no Chrome. O desinstalador executável preserva as preferências. Os instaladores são gerados em `dist/`; binários não são versionados no Git.

### Compilação e validação

Requisitos: Windows x64, **SDK .NET 10**, **Node.js** e **Inno Setup 6.7.3** ou compatível. O aplicativo não exige pacotes npm nem dependências NuGet de terceiros.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
.\build-installer.ps1 -Compiler 'C:\caminho\Inno Setup 6\ISCC.exe' -OutputDirectory .\dist
```

O build executa verificações JavaScript e de protocolo e publica os executáveis autocontidos e a extensão em `package/`. O segundo comando gera o instalador em `dist/`. Os projetos C# estão em [YouTubeBackground.slnx](YouTubeBackground.slnx).

| Pasta | Conteúdo |
| --- | --- |
| `src/App`, `src/Host`, `src/Shared` | Aplicativo Windows, ponte Native Messaging e protocolo compartilhado |
| `extension` | Extensão Manifest V3, seleção do vídeo e saltos |
| `setup`, `assets` | Instalador, traduções, guias e imagens do build |
| `tests` | Testes de lógica, protocolo, entrada Windows, bandeja e navegador |

Consulte [validações e limitações](VALIDACAO.md), [estado do projeto](docs/PROJECT_STATUS.md) e [instruções de contribuição](AGENTS.md). Testes de interface exigem desktop interativo e podem conflitar com uma instância instalada. Testes de navegador usam perfil isolado e mídia simulada; `--live` usa vídeo público real.

O modo anônimo com vídeo real do YouTube e o controle giratório físico ainda não foram validados. YouTube Music, vídeos incorporados e outros navegadores ficam fora do escopo suportado. O aplicativo não tem assinatura digital e a extensão não está na Chrome Web Store.

## Español

Controla YouTube con atajos globales de Windows mientras trabajas en otra aplicación o pestaña, sin cambiar el foco de la ventana.

**Aplicación / instalador: 1.2.3 · Extensión de Chrome: 1.0.1**

### Funciones y requisitos

- Windows 10/11 x64 y Google Chrome. Se incluye el entorno de ejecución de .NET.
- Retrocede/avanza con `Ctrl + Alt + ←/→`: 5 segundos de forma predeterminada, configurables entre 1 y 120 segundos.
- Graba combinaciones, teclas individuales y teclas multimedia, incluido Volumen +/− de controles giratorios compatibles.
- Controla el último vídeo iniciado entre los que siguen reproduciéndose, incluidos los silenciados. Se excluyen los vídeos pausados y los anuncios detectados; los directos necesitan DVR.
- Icono en la bandeja, suspensión de atajos, inicio opcional con Windows y preferencias conservadas al actualizar con el instalador.
- La aplicación, el instalador y la guía instalada siguen el idioma de Windows: inglés, portugués o español, con inglés para los demás idiomas.

### Instalación y uso

1. Ejecuta `YouTube-Background-Instalador-v1.2.3.exe` y sigue el asistente. No requiere permisos de administrador.
2. Abre `chrome://extensions`, activa **Modo de desarrollador**, pulsa **Cargar descomprimida** y selecciona `%LOCALAPPDATA%\YouTubeBackground\extension`.
3. Comprueba el ID: `jobnoaknnkhgmfjfbfkelbpfgabdogfb`. Tras actualizar, pulsa **Recargar** en la extensión; no hace falta eliminarla.
4. Recarga las pestañas de YouTube, reproduce un vídeo, abre otra aplicación y utiliza los atajos.
5. Para ventanas de incógnito, activa manualmente **Detalles → Permitir en incógnito** en la extensión. Recarga la pestaña de YouTube de esa ventana.

El último vídeo iniciado que sigue reproduciéndose entre las ventanas normales y de incógnito autorizadas recibe los comandos. Su título puede aparecer en la bandeja, incluso en modo incógnito. Los títulos no se guardan en un historial ni se envían a servidores externos; la comunicación con la aplicación es local.

Para cambiar los atajos, abre **Ajustes**, pulsa **Grabar**, pulsa la tecla deseada y selecciona **Guardar**. Al asignar Volumen +/−, esas teclas controlan el vídeo; **Suspender atajos** restaura el volumen. Los controles físicos deben emitir eventos de teclado reconocidos por Windows.

Consulta la [guía de instalación en español](setup/guide.es.html). Elimina la aplicación desde **Configuración de Windows → Aplicaciones** y elimina la extensión en Chrome. El desinstalador ejecutable conserva las preferencias. Los instaladores se generan en `dist/`; los binarios no se incluyen en Git.

### Compilación y validación

Requiere Windows x64, **SDK de .NET 10**, **Node.js** e **Inno Setup 6.7.3** o compatible. La aplicación no requiere paquetes npm ni dependencias NuGet de terceros.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
.\build-installer.ps1 -Compiler 'C:\ruta\Inno Setup 6\ISCC.exe' -OutputDirectory .\dist
```

La compilación ejecuta las comprobaciones de JavaScript y del protocolo y publica los ejecutables autocontenidos y la extensión en `package/`. El segundo comando genera el instalador en `dist/`. Los proyectos C# están en [YouTubeBackground.slnx](YouTubeBackground.slnx).

| Carpeta | Contenido |
| --- | --- |
| `src/App`, `src/Host`, `src/Shared` | Aplicación Windows, puente Native Messaging y protocolo compartido |
| `extension` | Extensión Manifest V3, selección de vídeo y saltos |
| `setup`, `assets` | Instalador, traducciones, guías e imágenes de compilación |
| `tests` | Pruebas de lógica, protocolo, entrada Windows, bandeja y navegador |

Consulta las [validaciones y limitaciones](VALIDACAO.md), el [estado del proyecto](docs/PROJECT_STATUS.md) y las [instrucciones de contribución](AGENTS.md), actualmente en portugués. Las pruebas de interfaz requieren un escritorio interactivo y pueden entrar en conflicto con una instancia instalada. Las pruebas del navegador usan un perfil aislado y medios simulados; `--live` utiliza un vídeo público real.

Aún no se han validado el modo incógnito con un vídeo real de YouTube ni el control giratorio físico. YouTube Music, vídeos insertados en otros sitios y otros navegadores quedan fuera del alcance compatible. La aplicación no tiene firma digital y la extensión no está en Chrome Web Store.
