# YouTube Background 1.2.3 — Windows + Chrome

## Instalação recomendada: executável

Abra **YouTube-Background-Instalador-v1.2.3.exe** e avance pelo assistente. Não é preciso extrair um ZIP ou instalar o .NET. O instalador atualiza versões anteriores preservando os atalhos, registra o programa na lista de aplicativos do Windows e cria atalhos no menu Iniciar.

Depois da cópia dos arquivos, o assistente apresenta duas etapas: **Conecte ao Chrome**, com as instruções para carregar a extensão e botões para abrir o Chrome e copiar o caminho; e **Escolha seus atalhos**, com um botão para abrir as configurações do aplicativo.

O aplicativo, o instalador e o guia instalado usam o idioma de exibição do Windows: português, espanhol ou inglês. Para outros idiomas, usam inglês. A escolha é refeita em cada atualização, sem reutilizar o idioma anterior. As configurações, mensagens e a bandeja do aplicativo também são traduzidas; reinicie o aplicativo após mudar o idioma de exibição do Windows. O README do projeto apresenta inglês, português e espanhol, nessa ordem.

A extensão continua sendo adicionada manualmente pelo usuário no Chrome. As etapas podem ser feitas durante o assistente ou depois, pelo atalho **YouTube Background - Guia de instalação** no menu Iniciar. Quem já tem a extensão carregada pode mantê-la.

O novo ícone vermelho com duas setas identifica o aplicativo na bandeja, nas configurações, no menu Iniciar e no instalador.

Para desinstalar a versão instalada pelo executável, use **Configurações do Windows → Aplicativos → YouTube Background → Desinstalar**. Remova também a extensão no Chrome. As preferências são preservadas para uma reinstalação.

Os procedimentos com ZIP abaixo continuam disponíveis para os pacotes antigos e para desenvolvimento.

A versão 1.1.1 corrige a abertura de Configurações: novos cliques trazem a janela existente à frente, e a janela pode ser fechada e reaberta. Você também pode abrir **YouTube Background** pelo menu Iniciar para acessar as configurações, inclusive quando o programa já estiver rodando.

Controle o vídeo do YouTube por atalhos, mesmo trabalhando em outro programa ou com o Chrome minimizado.

## Instalar e começar

1. Extraia **todo** o arquivo ZIP para uma pasta. Não execute os arquivos de dentro do ZIP.
2. Abra **Instalar.cmd**. Não é necessário executar como administrador. O aplicativo será copiado para `%LOCALAPPDATA%\YouTubeBackground` e iniciado perto do relógio.
3. No Chrome, abra `chrome://extensions` e habilite **Modo do desenvolvedor**.
4. Clique em **Carregar sem compactação**. Selecione a pasta `%LOCALAPPDATA%\YouTubeBackground\extension`. Você pode colar esse caminho no campo de endereço do seletor de pastas.
5. Confira o identificador da extensão: `jobnoaknnkhgmfjfbfkelbpfgabdogfb`.
6. Recarregue as abas do YouTube que já estavam abertas e inicie um vídeo.
7. Abra outro programa e pressione **Ctrl + Alt + ←** ou **Ctrl + Alt + →**. O vídeo deve retroceder ou avançar 5 segundos, sem trocar de janela.

O aplicativo funciona em Windows 10/11 de 64 bits. O runtime .NET acompanha o pacote. O Chrome precisa permanecer aberto. Não é preciso ativar atalhos globais nas configurações da extensão: o aplicativo do Windows recebe as teclas.

Este é um pacote de teste, sem assinatura digital ou publicação em loja. O Windows pode mostrar a origem como editor desconhecido. Em computadores administrados, políticas locais podem impedir scripts, aplicativos ou extensões de desenvolvedor.

## Usar em janela anônima

Use a extensão 1.0.1 ou posterior. Depois de atualizar os arquivos da extensão, clique em **Recarregar** em `chrome://extensions`. Abra **Detalhes** da extensão YouTube Background e ative **Permitir em modo anônimo**. Essa autorização precisa ser feita manualmente no Chrome. Recarregue as abas do YouTube abertas na janela anônima e inicie um vídeo.

O último vídeo iniciado entre os que estão em reprodução nas janelas normais e anônimas recebe os atalhos, sem mudar o foco. O título do vídeo anônimo também pode aparecer no menu da bandeja do aplicativo; ele não é gravado em histórico. Desative **Permitir em modo anônimo** para retirar esse acesso.

## Usar e configurar

Procure o ícone **YouTube Background** na bandeja do Windows, inclusive na lista de ícones ocultos. Clique com o botão direito para consultar a conexão, o título do alvo e o resultado do último comando.

- **Configurações:** escolha os atalhos e um intervalo inteiro entre 1 e 120 segundos. Clique em **Gravar** ao lado de Retroceder ou Avançar e pressione a tecla/combinação desejada ou gire o controle do teclado. Confira o nome detectado e clique em **Salvar**. Os atalhos ficam temporariamente desativados enquanto essa janela está aberta.
- **Iniciar ao entrar no Windows:** ativado inicialmente; pode ser desativado nas configurações. Também é possível abrir o aplicativo pelo menu Iniciar.
- **Suspender atalhos:** libera as combinações para outros aplicativos até você retomar. Essa suspensão é temporária e não persiste ao reiniciar.
- **Sair:** encerra o aplicativo e libera os atalhos. A extensão permanece instalada; ao abrir novamente o aplicativo, a conexão é restabelecida automaticamente.

Cada pressionamento gera um salto. Segurar as teclas não gera repetição. A combinação escolhida é reservada pelo aplicativo enquanto os atalhos estão ativos, inclusive se não houver vídeo tocando. Por isso, evite combinações frequentes em outros programas.

### Usar o controle giratório do teclado

1. Abra **Configurações** e clique em **Gravar** ao lado de **Retroceder**.
2. Gire o controle uma vez para a esquerda. Se ele enviar uma tecla de volume, aparecerá **Volume −**.
3. Clique em **Gravar** ao lado de **Avançar** e gire para a direita; deve aparecer **Volume +**.
4. Clique em **Salvar** e teste com um vídeo tocando no YouTube.

Também são aceitas teclas de mídia (próxima/anterior faixa, reproduzir/pausar, parar e silenciar), teclas individuais e combinações com Ctrl, Alt, Shift ou Win. **Esc** cancela a gravação. Ela também é cancelada ao trocar de janela ou após 15 segundos sem detectar uma tecla, preservando o valor anterior.

O programa reconhece o evento de teclado enviado ao Windows, não o modelo físico do controle. Alguns controles enviam eventos exclusivos do fabricante ou rolagem do mouse, que esta versão não captura. Nesse caso, use o software do teclado para atribuir teclas ao giro. A tecla Fn normalmente é tratada pelo próprio teclado e pode não ser detectada.

Ao atribuir **Volume +/−**, esses eventos passam a controlar o vídeo e deixam de ajustar o volume enquanto os atalhos estão ativos. Isso vale também para outras teclas/dispositivos que enviem o mesmo evento. **Suspender atalhos** ou **Sair** devolve o comportamento normal. O controle físico mostrado na foto ainda não foi validado.

### Atualizar da versão 1.0

Extraia o ZIP **Windows-x64-v1.1.1** e execute **Instalar.cmd**. O instalador substitui o aplicativo e preserva as configurações. A extensão não mudou nesta atualização; mantenha a que já está carregada. O ZIP **Codigo-Fonte** destina-se a desenvolvimento, não à instalação direta.

Se dois vídeos estiverem tocando, o iniciado mais recentemente recebe o comando. Vídeos silenciados também contam. Vídeos pausados e anúncios identificados não são alvos. Quando nenhum vídeo estiver em reprodução, nada acontece. O alvo é revalidado antes do salto; se mudou ou parou, o comando é descartado.

## Solucionar problemas

- **Chrome desconectado:** confirme que a extensão está habilitada, o aplicativo está aberto e o ID corresponde ao indicado acima. Aguarde até 30 segundos pela reconexão. Se necessário, reinicie o Chrome.
- **Nenhum vídeo em reprodução:** inicie um vídeo em `https://www.youtube.com` e recarregue a aba se ela estava aberta antes da instalação. Abas descartadas pelo navegador não têm um vídeo ativo.
- **Atalho ocupado:** abra Configurações e escolha outra combinação. Drivers e outros programas podem reservar `Ctrl + Alt + setas`.
- **Ao vivo não salta:** a transmissão precisa oferecer DVR. O salto fica limitado à janela navegável disponibilizada pelo YouTube.
- **Não funciona após atualização:** execute novamente o instalador a partir do novo pacote e clique em recarregar a extensão em `chrome://extensions`. Recarregue também as abas do YouTube.
- **A extensão está em outro perfil:** carregue-a somente nos perfis que deseja controlar. Se instalada em vários perfis, o aplicativo considera o último vídeo iniciado entre as conexões ativas; cada conexão revalida seu próprio alvo.

YouTube Music, outros navegadores e vídeos incorporados em outros sites não fazem parte desta versão. A identificação do player e de anúncios depende da página do YouTube; alterações no site podem exigir uma atualização.

## Remover

1. Remova a extensão em `chrome://extensions`.
2. Execute **Desinstalar.cmd** na pasta extraída do pacote.

Isso remove o aplicativo instalado, o registro de integração com o Chrome, a inicialização automática, o atalho do menu Iniciar e as configurações. Para preservar as configurações, execute `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\uninstall.ps1 -KeepSettings` na pasta do pacote.

## Código-fonte e compilação

Para gerar o instalador, use Inno Setup 6.7.3 ou compatível, após executar `build.ps1`:

```powershell
.\build-installer.ps1 -Compiler 'C:\caminho\Inno Setup 6\ISCC.exe' -OutputDirectory .\dist
```

O projeto do instalador está em `setup/YouTubeBackground.iss`. O ícone vetorial está em `assets/icon.svg`; `assets/generate.ps1` gera o ICO com nove tamanhos e as imagens do assistente. O `.ico` e as imagens já acompanham o código-fonte.

O ZIP de código-fonte acompanha a entrega. Requisitos de desenvolvimento: .NET SDK 10 e Node.js com `node:test`. Na pasta do código, execute:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
dotnet run --project tests/Integration/Integration.csproj -- package
node tests/browser-smoke.cjs browser-test-profile --live
```

Feche o aplicativo antes do teste de integração. Ele inicia e encerra seus próprios processos e não instala nada no Registro. O teste de navegador usa um perfil separado; a opção `--live` acessa um vídeo público do YouTube. O script de build testa o protocolo e a lógica JavaScript e gera a pasta `package`. Consulte `VALIDACAO.md` para os resultados e para o teste completo com registro temporário do auxiliar.

Estrutura: `src/App` contém o aplicativo WinForms; `src/Host`, o auxiliar Native Messaging; `src/Shared`, a validação de mensagens; `extension`, a extensão Manifest V3; `tests`, os testes. A chave pública em `extension/manifest.json` fixa o ID: preserve-a nas atualizações.

O aplicativo usa `RegisterHotKey` com `MOD_NOREPEAT` para teclas comuns. A gravação e os atalhos multimídia usam `WH_KEYBOARD_LL`, consumindo somente o evento capturado/configurado e suprimindo repetição até a liberação da tecla. As teclas pressionadas não são registradas em histórico. Chrome e auxiliar trocam JSON UTF-8 com prefixo binário de tamanho de 4 bytes. O auxiliar e o aplicativo usam um named pipe restrito ao usuário atual e à sessão. Comandos incluem prazo de validade, deslocamento e ID da aba; não são armazenados para execução posterior. Preferências ficam em `%LOCALAPPDATA%\YouTubeBackground\settings.json`.

O aplicativo não envia dados a servidores externos. Os títulos dos vídeos circulam apenas entre a extensão e o aplicativo local e não são gravados em histórico.
