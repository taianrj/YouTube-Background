# Instruções para o Codex

## Objetivo e comunicação

Este repositório contém o **YouTube Background**, aplicativo Windows que controla um vídeo do YouTube com atalhos globais, usando uma extensão Chrome e Native Messaging. Comunique-se em português do Brasil. Leia `README.md`, `docs/PROJECT_STATUS.md` e a implementação relevante antes de alterar o comportamento.

O repositório é a fonte de verdade. Não continue editando cópias antigas extraídas de ZIP ou pastas temporárias da conversa original. Faça alterações pequenas, preserve trabalho existente e explique o resultado e a validação.

## Arquitetura

- `src/App`: C#/.NET 10, Windows Forms, instância única, bandeja, configuração e gravação dos atalhos.
- `src/Host`: executável de Native Messaging. **stdout é reservado ao protocolo binário**; diagnóstico apenas em stderr.
- `src/Shared`: versão e validação das mensagens, identidade do named pipe por usuário e sessão.
- `extension`: Manifest V3, JavaScript sem dependências; observa o player, escolhe o último vídeo iniciado em reprodução e executa deslocamentos.
- `setup`: instalador Inno Setup com duas etapas posteriores à instalação: extensão Chrome e atalhos. `assets` contém o ícone original e seu gerador.

## Invariantes do produto

- Nunca mudar o foco, ativar a aba ou simular cliques no navegador para executar um salto.
- Não controlar vídeo pausado, encerrado, aba fechada ou anúncio identificado. Vídeos silenciados continuam elegíveis.
- Revalidar o alvo e a expiração imediatamente antes do salto; não guardar comandos para executar após reconexão.
- Respeitar limites do vídeo e intervalos `seekable` de transmissões com DVR.
- Manter `RegisterHotKey` com `MOD_NOREPEAT` para teclas comuns. Hooks multimídia devem ser rápidos, consumir apenas eventos configurados e não registrar histórico de teclas.
- Suspender os atalhos enquanto Configurações está aberta; restaurá-los ao fechar. Gravação: botão explícito, Esc cancela, timeout de 15 segundos, valores antigos preservados no cancelamento.
- Preservar **a chave pública do manifest** e o ID `jobnoaknnkhgmfjfbfkelbpfgabdogfb`. O host deve permitir apenas essa extensão.
- Configurações e integração são por usuário, sem administrador. Preservar preferências em atualizações. Não ampliar permissões da extensão além de `https://www.youtube.com/*` sem necessidade de produto.

## Compilação e testes

Em PowerShell, na raiz:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
.\build-installer.ps1 -Compiler 'C:\caminho\Inno Setup 6\ISCC.exe' -OutputDirectory .\dist
```

Requisitos: Windows x64, SDK .NET 10, Node.js e Inno Setup 6.7.3 compatível. O build básico executa 11 testes JavaScript, 12 verificações do protocolo e publica executáveis autocontidos em `package/`.

Testes adicionais dependem de desktop interativo e podem capturar teclas, abrir janelas ou disputar atalhos. Use-os quando relevantes e verifique se o aplicativo instalado está ativo antes de executar:

```powershell
dotnet run --project tests/Integration/Integration.csproj -- package
dotnet run --project tests/Input/InputTests.csproj -- work/settings-test.png
dotnet run --project tests/Tray/TrayTests.csproj
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/hidden-start.ps1
node tests/browser-smoke.cjs work/browser-test-profile
```

Crie `work/` antes do teste que salva imagem. `--live` no teste de navegador acessa um vídeo público. `tests/native-browser.ps1` altera temporariamente o registro do host e recusa instalações existentes. Não rode testes de instalação/remoção sobre uma instalação do usuário sem considerar o escopo autorizado. Informe precisamente quando o teste usa mídia simulada, player real ou hardware físico.

## Regressões importantes

- Configurações precisa abrir mesmo se o processo começou com `SW_HIDE`. Preserve a exibição explícita com `SetWindowPos`, a reativação da janela existente e o tratamento de solicitações recebidas durante a inicialização.
- Não voltar a usar um bloqueio modal que ignore novos cliques em Configurações.
- O instalador deve aguardar o fim do processo antes de substituir arquivos. No Inno Setup, `{app}` ainda não está disponível em `InitializeWizard`; use `{localappdata}` nessa fase e atualize o caminho após inicialização.
- Uma captura apenas do formulário isolado não comprova que o menu da bandeja ou o executável iniciado oculto funcionam. Use o teste correspondente.

## Entregas e revisão

- Atualize versões do aplicativo, instalador e documentação em conjunto quando gerar uma nova versão. A versão da extensão só precisa mudar se ela for alterada.
- Não versionar executáveis, instaladores, `bin`, `obj`, perfis do Chrome, logs, credenciais, preferências pessoais ou ferramentas baixadas. Versionar os ícones e imagens usados pelo build.
- Gere pacotes em `package/` e `dist/`; use `work/` para testes e capturas. Não publique Releases ou altere a visibilidade do repositório sem pedido do usuário.
- Antes de concluir, execute verificações apropriadas à mudança e registre limitações reais. Não alegue suporte ao controle físico de um teclado sem testá-lo.

## Code Review Rules

Priorize perda de foco, comandos atrasados, alvo incorreto, captura indevida de teclas, quebra de atualização/remoção por usuário e perda de preferências. Exija que alterações no ciclo de vida das janelas sejam verificadas também no executável publicado, quando afetarem a inicialização oculta.
