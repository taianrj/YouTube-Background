using System.Globalization;

namespace YouTubeBackground;

// Use the Windows display language inherited by CurrentUICulture, including regional variants.
// No language preference is written to the user's settings.
internal static class Ui
{
    internal static string LanguageFor(CultureInfo culture) => culture.TwoLetterISOLanguageName switch {
        "pt" => "pt", "es" => "es", _ => "en"
    };
    internal static readonly IReadOnlyDictionary<string, (string En, string Pt, string Es)> Messages =
        new Dictionary<string, (string En, string Pt, string Es)> {
        ["Mute"] = ("Mute", "Silenciar", "Silenciar"),
        ["VolumeDown"] = ("Volume −", "Volume −", "Volumen −"),
        ["VolumeUp"] = ("Volume +", "Volume +", "Volumen +"),
        ["NextTrack"] = ("Next track", "Próxima faixa", "Pista siguiente"),
        ["PreviousTrack"] = ("Previous track", "Faixa anterior", "Pista anterior"),
        ["StopMedia"] = ("Stop media", "Parar mídia", "Detener medios"),
        ["PlayPause"] = ("Play / pause", "Reproduzir / pausar", "Reproducir / pausar"),
        ["AppNotResponding"] = ("The app is not responding yet. Try opening Settings again.", "O aplicativo ainda não está respondendo. Tente abrir Configurações novamente.", "La aplicación aún no responde. Intenta abrir Ajustes de nuevo."),
        ["Disconnected"] = ("Chrome disconnected", "Chrome desconectado", "Chrome desconectado"),
        ["NoVideo"] = ("No video playing", "Nenhum vídeo em reprodução", "Ningún vídeo en reproducción"),
        ["WaitingCommand"] = ("Waiting for a command", "Aguardando comando", "Esperando un comando"),
        ["Suspend"] = ("Suspend hotkeys", "Suspender atalhos", "Suspender atajos"),
        ["SettingsMenu"] = ("Settings…", "Configurações…", "Ajustes…"),
        ["Exit"] = ("Exit", "Sair", "Salir"),
        ["UnavailableTitle"] = ("Hotkey unavailable", "Atalho indisponível", "Atajo no disponible"),
        ["UnavailableBody"] = ("A hotkey is in use. Open Settings to choose another combination.", "Um dos atalhos está ocupado. Abra Configurações para escolher outra combinação.", "Un atajo está ocupado. Abre Ajustes para elegir otra combinación."),
        ["SettingsOpen"] = ("Settings open — hotkeys suspended", "Configurações abertas — atalhos suspensos", "Ajustes abiertos — atajos suspendidos"),
        ["Suspended"] = ("Hotkeys suspended", "Atalhos suspensos", "Atajos suspendidos"),
        ["Occupied"] = ("Hotkey in use — open Settings", "Atalho ocupado — abra Configurações", "Atajo ocupado — abre Ajustes"),
        ["Connected"] = ("Chrome connected", "Chrome conectado", "Chrome conectado"),
        ["ConnectionLost"] = ("Connection lost; command discarded", "Conexão interrompida; comando descartado", "Conexión interrumpida; comando descartado"),
        ["SeekSent"] = ("Last skip sent to the video", "Último salto enviado ao vídeo", "Último salto enviado al vídeo"),
        ["NotExecuted"] = ("Command not executed", "Comando não executado", "Comando no ejecutado"),
        ["OccupiedSave"] = ("Hotkey in use by another app. Choose a different combination.", "Atalho ocupado por outro aplicativo. Escolha uma combinação diferente.", "Otra aplicación utiliza el atajo. Elige otra combinación."),
        ["SaveFailed"] = ("Could not save: ", "Não foi possível salvar: ", "No se pudo guardar: "),
        ["SettingsFailed"] = ("Could not open Settings.\n\n", "Não foi possível abrir Configurações.\n\n", "No se pudo abrir Ajustes.\n\n"),
        ["Help"] = ("Click Record and press a key, a combination or turn the dial.\nIf the dial sends Volume +/−, each direction can be recorded separately.\nWhile these hotkeys are active, they replace volume control.", "Clique em Gravar e pressione uma tecla, combinação ou gire o controle.\nSe o giro enviar Volume +/−, cada direção pode ser gravada separadamente.\nEnquanto esses atalhos estiverem ativos, eles substituem o controle de volume.", "Pulsa Grabar y pulsa una tecla, combinación o gira el control.\nSi envía Volumen +/−, puedes grabar cada dirección por separado.\nMientras estos atajos estén activos, sustituyen el control de volumen."),
        ["SettingsTitle"] = ("YouTube Background — Settings", "YouTube Background — Configurações", "YouTube Background — Ajustes"),
        ["Startup"] = ("Start when signing in to Windows", "Iniciar ao entrar no Windows", "Iniciar al entrar en Windows"),
        ["Record"] = ("Record", "Gravar", "Grabar"),
        ["CancelledInline"] = ("Recording cancelled. ", "Gravação cancelada. ", "Grabación cancelada. "),
        ["Back"] = ("Skip backward", "Retroceder", "Retroceder"),
        ["Forward"] = ("Skip forward", "Avançar", "Avanzar"),
        ["Seconds"] = ("Seconds per skip", "Segundos por salto", "Segundos por salto"),
        ["StartupLabel"] = ("Startup", "Inicialização", "Inicio"),
        ["Save"] = ("Save", "Salvar", "Guardar"),
        ["DifferentHotkeys"] = ("Choose two different, valid hotkeys.", "Escolha dois atalhos válidos e diferentes.", "Elige dos atajos válidos y diferentes."),
        ["Settings"] = ("Settings", "Configurações", "Ajustes"),
        ["Timeout"] = ("No key detected. The dial may need configuration in the keyboard software.\n", "Nenhuma tecla detectada. O controle pode precisar de configuração no software do teclado.\n", "No se detectó ninguna tecla. Puede que debas configurar el control en el software del teclado.\n"),
        ["Deactivated"] = ("Recording cancelled after switching windows.\n", "Gravação cancelada ao trocar de janela.\n", "Grabación cancelada al cambiar de ventana.\n"),
        ["CaptureFailed"] = ("Could not start recording: ", "Não foi possível iniciar a captura: ", "No se pudo iniciar la grabación: "),
        ["Cancel"] = ("Cancel", "Cancelar", "Cancelar"),
        ["Recording"] = ("Waiting… press the hotkey or turn the dial once.\nEsc cancels. Recording stops automatically after 15 seconds.", "Aguardando… pressione o atalho ou gire o controle uma vez.\nEsc cancela. A gravação termina automaticamente após 15 segundos.", "Esperando… pulsa el atajo o gira el control una vez.\nEsc cancela. La grabación termina automáticamente tras 15 segundos."),
        ["Cancelled"] = ("Recording cancelled.\n", "Gravação cancelada.\n", "Grabación cancelada.\n"),
        ["Detected"] = ("Detected: {0}. Click Save to apply.\n", "Detectado: {0}. Clique em Salvar para aplicar.\n", "Detectado: {0}. Pulsa Guardar para aplicar.\n"),
        ["VideoUnavailable"] = ("Video unavailable, paused or showing an ad.", "Vídeo indisponível, pausado ou anúncio.", "Vídeo no disponible, pausado o con anuncio."),
        ["NoSeekRange"] = ("Video has no seekable range.", "Vídeo sem intervalo navegável.", "El vídeo no tiene un intervalo navegable."),
        ["SeekFailed"] = ("Could not seek in the video.", "Não foi possível navegar no vídeo.", "No se pudo saltar en el vídeo."),
        ["TargetChanged"] = ("The selected video changed or stopped.", "O vídeo selecionado mudou ou parou.", "El vídeo seleccionado cambió o se detuvo."),
        ["TabUnavailable"] = ("The tab is unavailable.", "A aba não está disponível.", "La pestaña no está disponible."),
    };
    internal static string Text(string key) => LanguageFor(CultureInfo.CurrentUICulture) switch {
        "pt" => Messages[key].Pt, "es" => Messages[key].Es, _ => Messages[key].En
    };
    internal static string Format(string key, params object[] values) =>
        string.Format(CultureInfo.CurrentUICulture, Text(key), values);

    // Translate the existing extension protocol without changing its identity or wire format.
    internal static string CommandFailure(string? reason) => Text(reason switch {
        "Vídeo indisponível, pausado ou anúncio." => "VideoUnavailable",
        "Vídeo sem intervalo navegável." => "NoSeekRange",
        "Não foi possível navegar no vídeo." => "SeekFailed",
        "O vídeo selecionado mudou ou parou." => "TargetChanged",
        "A aba não está disponível." => "TabUnavailable",
        _ => "NotExecuted"
    });
}
