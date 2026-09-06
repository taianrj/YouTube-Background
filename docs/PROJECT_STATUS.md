# Estado e decisões do projeto

## Origem e objetivo

Projeto iniciado em 06/09/2026 a pedido de Taian para permitir que um amigo controle o YouTube enquanto trabalha em outra janela. Foi inicialmente desenvolvido em uma conversa sem projeto e migrado para este repositório. A partir da migração, este repositório é a fonte de verdade.

## Versões entregues

- **1.0:** aplicativo WinForms + host + extensão; saltos, configurações e distribuição por ZIP.
- **1.1:** botões Gravar e eventos de volume/multimídia, incluindo sinais enviados por controles giratórios compatíveis.
- **1.1.1:** correção da primeira janela oculta por `SW_HIDE`, abertura pelo menu Iniciar, recuperação da janela existente e espera pelo encerramento durante atualização.
- **1.2.0:** ícone próprio vermelho com setas e instalador Inno Setup com etapas para Chrome e atalhos; atualização local testada preservando preferências.

## Decisões preservadas

1. Aplicativo na sessão do usuário com bandeja, e não serviço do Windows. O instalador não exige administrador.
2. Chrome usa extensão Manifest V3 e Native Messaging. O host usa named pipe por usuário e sessão; não abre porta TCP.
3. Alvo é o vídeo atualmente reproduzindo; empate favorece o último iniciado. Não controlar vídeo pausado. Não trocar foco nem aba para executar comandos.
4. A chave pública do manifest fixa o ID da extensão. Sua alteração quebra a integração instalada.
5. Atalhos padrão Ctrl + Alt + setas e saltos de 5 segundos. Volume atribuído ao vídeo deixa de alterar volume até suspender os atalhos.
6. Instalação manual da extensão continua necessária: o assistente ensina e abre as ferramentas, mas não afirma instalar a extensão automaticamente.
7. Desinstalador do executável preserva preferências. As instruções antigas de ZIP ficam documentadas para compatibilidade histórica.

## Validação existente

Onze testes JavaScript, doze verificações de protocolo, testes Windows de captura de teclas, interface da bandeja, inicialização oculta e integração de Native Messaging. Vídeo real do YouTube foi controlado a partir de outra aba. Instalador 1.2 foi percorrido e testado na atualização local. Consulte `VALIDACAO.md` para distinguir testes atuais e históricos.

## Limitações e próximos trabalhos possíveis

- O controle giratório físico do amigo ainda não foi testado; a validação de entrada usou eventos simulados do Windows.
- Sem assinatura digital própria e sem publicação da extensão na Chrome Web Store.
- Dependência dos seletores do player e de anúncios do YouTube; futuras mudanças no site podem exigir ajustes.
- Não há certificação separada em todas as versões suportadas de Windows nem teste de desinstalação completa da versão 1.2 nesta sessão.
- Instaladores e executáveis de distribuição ficam fora do histórico Git. Uma Release pode ser criada futuramente quando solicitada.
