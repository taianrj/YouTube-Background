# Validação do YouTube Background

## Aplicativo e instalador 1.2.3 — interface traduzida (07/09/2026)

- Configurações, instruções de gravação, avisos, teclas multimídia e bandeja em inglês, português e espanhol. Seleção por `CurrentUICulture`, usando idioma primário `pt`/`es` e inglês para os demais. Nenhuma preferência de idioma foi adicionada; motivos conhecidos da extensão são traduzidos na apresentação, sem mudar o protocolo ou a extensão 1.0.1.
- `dotnet run --project tests/Localization/LocalizationTests.csproj -- work/localization`: 264 verificações passaram, cobrindo variantes regionais, fallback, mensagens, argumentos, nomes das teclas, erros da extensão, preferências serializadas invariantes e layout das configurações. A fábrica do menu usada pela bandeja foi testada com callbacks isolados, incluindo Configurações, suspensão e Sair.
- Formulários e menus de teste invisíveis foram renderizados em `work/localization/`; capturas representativas das configurações e dos menus nos três idiomas foram inspecionadas, sem cortes observados. Os testes não ativam hooks, não escrevem preferências e não conectam ao host. As mensagens de gravação foram exercitadas como estados visuais, sem captura de teclado físico.
- `build.ps1` passou: 12 testes JavaScript (incluindo traduções do instalador), 12 verificações de protocolo e publicação autocontida em `package/`. Guias, README, AGENTS.md e versão do instalador sincronizados com 1.2.3. Testes antigos que localizavam botões pelo texto em português foram adaptados.
- Inno Setup 6.7.3 gerou `dist/YouTube-Background-Instalador-v1.2.3.exe`; versão do instalador e do aplicativo publicado conferidas e SHA-256 salvo em `dist/`. `git diff --check` passou.
- Aplicativo instalado encontrado em execução; não foi interrompido. O fluxo completo do ícone da bandeja, testes de captura, inicialização oculta e instalação/remoção não foram repetidos nesta alteração. A lógica de foco, registro dos atalhos e persistência foi preservada. Não houve alteração de visibilidade nem publicação de Release no GitHub.

## Instalador 1.2.2 — idiomas (07/09/2026)

- Instalador e aplicativo versionados como 1.2.2; extensão permanece em 1.0.1. README em inglês, português e espanhol; guias HTML e mensagens do instalador nos três idiomas. A interface do aplicativo Windows continua em português.
- `build.ps1` passou: 11 testes de lógica JavaScript, 1 teste de completude das traduções (incluindo referências e argumentos) e 12 verificações do protocolo. Aplicativo e host publicados em `package/`.
- Inno Setup 6.7.3 compilou `dist/YouTube-Background-Instalador-v1.2.2.exe`, incluindo os três guias e o manifest atualizado. Versões dos executáveis e hash SHA-256 do instalador conferidos.
- Revisão visual de boas-vindas, página Chrome e página de atalhos nos três idiomas: textos e botões legíveis, sem cortes observados. Foi usada uma prévia gerada em `work/` a partir do script, com páginas antecipadas, caminho de teste por `{localappdata}`, arquivos de instalação omitidos e avanço da página de instalação bloqueado. Essa prévia não instala o programa; não equivale a uma instalação completa do executável de distribuição.
- Português escolhido automaticamente no Windows pt-BR; inglês e espanhol selecionados por `/LANG=english` e `/LANG=spanish` na prévia. Idiomas regionais e fallback para inglês seguem a detecção nativa documentada do Inno Setup: correspondência completa, depois idioma primário, depois primeira entrada. Configuração: `LanguageDetectionMethod=uilanguage`, inglês primeiro, `ShowLanguageDialog=no`, `UsePreviousLanguage=no`. Não foi alterado o idioma do Windows para testar outras localidades.
- Referências locais do README e `git diff --check` conferidos. Não houve instalação/remoção desta versão, alteração de visibilidade no GitHub ou publicação de Release. Validação de modo anônimo com vídeo real e de teclado físico continua pendente.

## Instalador 1.2.1 (07/09/2026)

- Aplicativo atualizado para 1.2.1, extensão mantida em 1.0.1. Documentação e versão do instalador sincronizadas; assistente orienta recarregar a extensão e autorizar o modo anônimo.
- `build.ps1` passou: 11 testes JavaScript, 12 verificações de protocolo e publicação autocontida em `package/`. Versão do executável publicado conferida: 1.2.1.0.
- Instalador gerado com Inno Setup 6.7.3 em `dist/YouTube-Background-Instalador-v1.2.1.exe`. Compilador preparado em modo portátil a partir do download oficial, com assinatura válida de Pyrsys B.V. O instalador do aplicativo continua sem assinatura digital própria.
- Compilação confirmou inclusão dos quatro arquivos da extensão, executáveis, ícone, guia atualizado e script de encerramento. `git diff --check` passou.
- Não foi executada instalação/remoção nem validação visual do assistente 1.2.1 nesta entrega. Os testes históricos abaixo não equivalem a uma nova validação de atualização. O teste em janela anônima com YouTube real permanece pendente.

## Extensão 1.0.1 — acesso opcional ao modo anônimo (07/09/2026)

- Manifest alterado de `not_allowed` para `spanning`, preservando a chave pública, o ID e as permissões. Guias atualizados para a autorização manual no Chrome e a seleção compartilhada entre janelas normais e anônimas.
- `build.ps1`: 11 testes JavaScript e 12 verificações de protocolo passaram; executáveis e extensão publicados em `package/`. Aplicativo e instalador permanecem na versão 1.2.0; nenhum novo instalador foi gerado.
- `node tests/browser-smoke.cjs work/browser-incognito-change-retry`: passou no Chrome 152.0.7977.77, em perfil isolado normal, com mídia simulada em página interceptada. Verificou ID da extensão, saltos a partir de outra aba, rejeição em pausa/anúncio e remoção de aba fechada.
- A primeira tentativa de build falhou por acesso ao SDK e a primeira tentativa de navegador falhou no processo de GPU sob restrição do ambiente; ambas passaram após execução com a permissão necessária.
- Não foi realizado teste em janela anônima, com YouTube real ou com teclado físico nesta alteração. A autorização manual e o fluxo completo em janela anônima ainda precisam de validação. A instalação existente do usuário não foi modificada.

## Instalador executável e identidade visual

- Aplicativo compilado com ícone próprio embutido; ICO inclui nove resoluções, de 16 a 256 pixels.
- Instalador compilado com Inno Setup 6.7.3, preparado em modo portátil a partir da distribuição oficial com assinatura digital validada. O instalador gerado continua sem assinatura digital própria.
- Telas de boas-vindas, Chrome, atalhos e conclusão percorridas e capturadas para inspeção visual.
- Atualização de 1.1.1 para 1.2.0 concluída na instalação existente. Preferências comparadas por hash antes/depois e preservadas.
- Botão Copiar caminho verificado contra a pasta instalada da extensão. Botão Abrir configurações abriu a janela do aplicativo.
- Manifesto Native Messaging validado como JSON e caminho do auxiliar confirmado. Entrada de desinstalação registrada na lista de aplicativos do Windows com versão 1.2.0.
- Onze testes JavaScript e doze verificações do protocolo C# passaram na compilação da versão.

A remoção completa não foi executada nesta sessão para manter o programa instalado. Os botões de abertura do Chrome e da pasta foram conferidos no código; a etapa manual de carregar a extensão no Chrome continua sendo feita pelo usuário. O fluxo visual permite continuar sem essa etapa, mas informa que os atalhos só controlarão o vídeo após a extensão estar carregada.

## Correção 1.1.1: janela de Configurações

Causa confirmada na instalação: a inicialização com janela oculta (`SW_HIDE`) também afetava a primeira exibição das Configurações. A correção solicita explicitamente a visibilidade dessa janela. A versão publicada foi instalada e testada com inicialização oculta e solicitação imediata de Configurações: janela visível na primeira tentativa. O instalador agora aguarda o encerramento do processo antes de substituir os arquivos.

O teste do handler real do menu da bandeja passou: abertura de janela visível e presente na barra de tarefas, posicionamento dentro de uma tela, reutilização em cliques repetidos, restauração, fechamento e reabertura. A solicitação enviada por outra instância do executável também abriu a janela. Teste reproduzível: `dotnet run --project tests/Tray/TrayTests.csproj`. Feche o aplicativo instalado antes de executá-lo, para evitar conflito de atalhos.

## Atualização 1.1: gravação de atalhos

Treze verificações de entrada passaram usando `SendInput` real do Windows com eventos consumidos pelos hooks de teste: gravação de Volume +/−, combinação com Ctrl, rejeição de modificador isolado, manutenção dos padrões anteriores, supressão de repetição, pulsos distintos, despacho de avanço/retrocesso e suspensão. A tela foi renderizada e inspecionada; os dois botões Gravar foram acionados pelo teste, e Esc preservou o atalho anterior.

Esses testes não usam o controle físico da foto. A compatibilidade depende de ele enviar uma tecla reconhecida pelo Windows; eventos HID exclusivos do fabricante e roda de mouse não são capturados nesta versão.

Para reproduzir os testes adicionais no código-fonte:

```powershell
dotnet run --project tests/Input/InputTests.csproj -- settings-test.png
```

O teste simula eventos de teclado e abre uma janela invisível temporária. Não modifica configurações persistidas. As verificações abaixo documentam a integração de base já validada na versão 1.0; o transporte e a extensão não foram alterados em 1.1.

Executada em 6 de setembro de 2026, no ambiente Windows disponível, com .NET SDK 10.0.303 e Chrome 152.0.7977.77.

## Verificações aprovadas

| Verificação | Resultado |
| --- | --- |
| Publicação Windows x64 de aplicativo e auxiliar, com runtime incluído | Aprovada |
| JavaScript: seleção, empates, pausados, anúncios, limites, DVR, lacunas e comandos expirados | Aprovada |
| Content script: salto, pausa, anúncio, expiração, troca do elemento e ausência de DVR | Aprovada |
| Total de testes automatizados JavaScript | 11 aprovados |
| Validação C# de versão, tamanho, estrutura e prazo das mensagens | 12 verificações aprovadas |
| Aplicativo + named pipe + auxiliar Native Messaging | Avanço e retrocesso aprovados |
| Ausência de comandos sem vídeo em reprodução | Aprovada no teste de integração |
| Preservação do foco da janela pelo aplicativo | Aprovada no teste de integração |
| Encerramento do auxiliar quando a entrada do Chrome fecha | Aprovado |
| Carregamento real da extensão e estabilidade do ID | Aprovados em perfil temporário do Chrome |
| Mídia real em página de teste: salto com outra aba ativa, rejeição de pausa/anúncio e fechamento da aba | Aprovados |
| Vídeo público real do YouTube: salto com outra aba ativa | Aprovado |
| Caminho completo aplicativo → auxiliar → extensão → vídeo real do YouTube | Aprovado com registro temporário do auxiliar |

No último teste, o vídeo “Big Buck Bunny 60fps 4K - Official Blender Foundation Short Film” avançou 5 segundos a partir do aplicativo Windows. O teste usou um perfil separado do Chrome e removeu o registro temporário do auxiliar ao terminar. Não instalou a extensão no perfil habitual do usuário nem habilitou inicialização automática.

Os testes do aplicativo dispararam a mensagem `WM_HOTKEY` diretamente na janela de mensagens. Isso testa o tratamento do comando e a integração, mas não simula o pressionamento físico das teclas. A captura usa a API real `RegisterHotKey` com `MOD_NOREPEAT`.

## Verificação manual no computador de destino

1. Executar `Instalar.cmd`, carregar a extensão e recarregar o YouTube.
2. Com outro aplicativo em foco, verificar os dois atalhos. Repetir com Chrome minimizado.
3. Segurar as teclas e confirmar apenas um salto por pressionamento.
4. Abrir dois vídeos, alternar a reprodução e confirmar a seleção do último iniciado. Pausar ambos e confirmar ausência de salto.
5. Alterar teclas e intervalo; reiniciar o aplicativo e conferir persistência. Tentar uma combinação já reservada e conferir a indicação de conflito.
6. Suspender os atalhos e sair; confirmar liberação das teclas. Reabrir o aplicativo e reiniciar o Chrome; conferir reconexão.
7. Conferir inicialização no próximo login, atualização pelo instalador e remoção por `Desinstalar.cmd`.
8. Se necessário, testar uma transmissão ao vivo com DVR e anúncios reais. Os limites e a identificação de anúncios foram testados com cenários controlados, não com transmissão/anúncio ao vivo nesta sessão.

Esses passos ainda dependem da interação com o Windows do usuário, das combinações reservadas por outros programas e das políticas locais. Não há certificação separada em Windows 10 e Windows 11.

## Reproduzir os testes

Na pasta do código-fonte:

```powershell
.\build.ps1
dotnet run --project tests/Integration/Integration.csproj -- package
node tests/browser-smoke.cjs browser-test-profile
node tests/browser-smoke.cjs browser-test-profile --live
```

O teste completo com registro temporário é `tests/native-browser.ps1`. Ele exige que não exista instalação anterior e que o aplicativo esteja fechado; cria e remove apenas a chave de Native Messaging da aplicação. A opção `--live` acessa um vídeo público, podendo ser afetada pela conexão, anúncios e restrições do YouTube.
