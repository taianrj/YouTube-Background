using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace YouTubeBackground;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        using var mutex = new Mutex(true, Protocol.PipeName + "-app", out bool first);
        if (!first) { HotkeyWindow.RequestSettings(); return; }
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApp(args.Contains("--settings")));
    }
}

public sealed record Shortcut(uint Modifiers, int Key)
{
    public override string ToString() => ((Modifiers & 2) != 0 ? "Ctrl + " : "") + ((Modifiers & 1) != 0 ? "Alt + " : "") + ((Modifiers & 4) != 0 ? "Shift + " : "") + ((Modifiers & 8) != 0 ? "Win + " : "") + (Key switch {
        173 => "Silenciar", 174 => "Volume −", 175 => "Volume +", 176 => "Próxima faixa", 177 => "Faixa anterior", 178 => "Parar mídia", 179 => "Reproduzir / pausar", 37 => "←", 39 => "→", _ => ((Keys)Key).ToString()
    });
    public static bool IsModifier(int key) => key is 16 or 17 or 18 or 91 or 92 or >= 160 and <= 165;
    public bool IsMedia => Key is >= 173 and <= 179;
    public bool Valid => (Modifiers & ~15u) == 0 && Key >= 8 && Key <= 254 && !IsModifier(Key);
}
public sealed record Settings(Shortcut Back, Shortcut Forward, int Seconds, bool Startup)
{
    public static Settings Default => new(new(3, (int)Keys.Left), new(3, (int)Keys.Right), 5, true);
    public static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YouTubeBackground");
    public static string FileName => Path.Combine(Folder, "settings.json");
    public static Settings Load()
    {
        try {
            var s = JsonSerializer.Deserialize<Settings>(File.ReadAllText(FileName));
            if (s is not null && s.Back.Valid && s.Forward.Valid && s.Back != s.Forward && s.Seconds is >= 1 and <= 120) return s;
        } catch { }
        return Default;
    }
    public void Save()
    {
        Directory.CreateDirectory(Folder);
        File.WriteAllText(FileName + ".tmp", JsonSerializer.Serialize(this));
        File.Move(FileName + ".tmp", FileName, true);
        using var run = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        if (Startup) run.SetValue("YouTubeBackground", "\"" + Application.ExecutablePath + "\"");
        else run.DeleteValue("YouTubeBackground", false);
    }
}

internal sealed class HotkeyWindow : NativeWindow, IDisposable
{
    [DllImport("user32.dll", SetLastError = true)] static extern bool RegisterHotKey(IntPtr handle, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr handle, int id);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string? className, string title);
    public static void RequestSettings() {
        // The first process owns the mutex before its message window is ready.
        for (int attempt = 0; attempt < 50; attempt++) {
            var window = FindWindowEx(new IntPtr(-3), IntPtr.Zero, null, "YouTubeBackground");
            if (window != IntPtr.Zero) { PostMessage(window, 0x8001, IntPtr.Zero, IntPtr.Zero); return; }
            Thread.Sleep(100);
        }
        MessageBox.Show("O aplicativo ainda não está respondendo. Tente abrir Configurações novamente.", "YouTube Background");
    }
    private KeyboardHook? media;
    public event Action<int>? Pressed;
    public event Action? SettingsRequested;
    public HotkeyWindow() { CreateHandle(new CreateParams { Caption = "YouTubeBackground", Parent = new IntPtr(-3) }); }
    public bool Register(Settings s)
    {
        Clear();
        if (!s.Back.Valid || !s.Forward.Valid || s.Back == s.Forward) return false;
        if (!s.Back.IsMedia && !RegisterHotKey(Handle, 1, s.Back.Modifiers | 0x4000, (uint)s.Back.Key)) return false;
        if (!s.Forward.IsMedia && !RegisterHotKey(Handle, 2, s.Forward.Modifiers | 0x4000, (uint)s.Forward.Key)) { Clear(); return false; }
        if (s.Back.IsMedia || s.Forward.IsMedia) {
            try {
                media = new KeyboardHook { OnKey = shortcut => {
                    int id = shortcut == s.Back ? 1 : shortcut == s.Forward ? 2 : 0;
                    if (id == 0) return false;
                    PostMessage(Handle, 0x0312, new IntPtr(id), IntPtr.Zero); return true;
                } };
            } catch { Clear(); return false; }
        }
        return true;
    }
    public void Clear() { media?.Dispose(); media = null; UnregisterHotKey(Handle, 1); UnregisterHotKey(Handle, 2); }
    protected override void WndProc(ref Message m) { if (m.Msg == 0x0312) Pressed?.Invoke(m.WParam.ToInt32()); if (m.Msg == 0x8001) SettingsRequested?.Invoke(); base.WndProc(ref m); }
    public void Dispose() { Clear(); DestroyHandle(); }
}

internal sealed class Client : IDisposable
{
    public readonly NamedPipeServerStream Pipe;
    public readonly StreamWriter Writer;
    public JsonElement? Target;
    public readonly SemaphoreSlim Sending = new(1, 1);
    public Client(NamedPipeServerStream pipe) { Pipe = pipe; Writer = new(pipe, new UTF8Encoding(false), 4096, true) { AutoFlush = true }; }
    public void Dispose() { Pipe.Dispose(); Writer.Dispose(); }
}

internal sealed class TrayApp : ApplicationContext
{
    [DllImport("user32.dll", SetLastError = true)] static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int width, int height, uint flags);
    private Settings settings = Settings.Load();
    private readonly HotkeyWindow keys = new();
    private readonly Control dispatcher = new();
    private readonly NotifyIcon tray;
    private readonly ToolStripMenuItem status = new("Chrome desconectado") { Enabled = false };
    private readonly ToolStripMenuItem title = new("Nenhum vídeo em reprodução") { Enabled = false };
    private readonly ToolStripMenuItem lastResult = new("Aguardando comando") { Enabled = false };
    private readonly ToolStripMenuItem pause = new("Suspender atalhos") { CheckOnClick = true };
    private readonly ConcurrentDictionary<Guid, Client> clients = new();
    private readonly CancellationTokenSource stop = new();
    private bool registered, closing;
    private SettingsForm? settingsWindow;

    public TrayApp(bool openSettings = false)
    {
        _ = dispatcher.Handle;
        var menu = new ContextMenuStrip();
        menu.Items.AddRange([status, title, lastResult, new ToolStripSeparator()]);
        menu.Items.Add("Configurações…", null, (_, _) => UI(Configure));
        menu.Items.Add(pause);
        menu.Items.Add("Sair", null, (_, _) => ExitThread());
        tray = new NotifyIcon { Text = "YouTube Background", Icon = Brand.Icon, ContextMenuStrip = menu, Visible = true };
        tray.DoubleClick += (_, _) => UI(Configure);
        keys.SettingsRequested += () => UI(Configure);
        pause.CheckedChanged += (_, _) => {
            if (pause.Checked) { keys.Clear(); registered = false; }
            else if (settingsWindow is null) Register();
            UpdateStatus();
        };
        keys.Pressed += id => SendSeek(id == 1 ? -settings.Seconds : settings.Seconds);
        Register();
        _ = AcceptClients();
        if (openSettings) UI(Configure);
    }
    private void UI(Action action)
    {
        if (stop.IsCancellationRequested) return;
        try { dispatcher.BeginInvoke(action); } catch (InvalidOperationException) { }
    }
    private void Register()
    {
        registered = keys.Register(settings);
        if (!registered) tray.ShowBalloonTip(6000, "Atalho indisponível", "Um dos atalhos está ocupado. Abra Configurações para escolher outra combinação.", ToolTipIcon.Warning);
        UpdateStatus();
    }
    private (Client? client, JsonElement? target) Selected()
    {
        Client? chosen = null; JsonElement? selected = null; double latest = -1;
        foreach (var client in clients.Values) {
            if (client.Target is { } target && target.GetProperty("startedAt").GetDouble() > latest) {
                chosen = client; selected = target; latest = target.GetProperty("startedAt").GetDouble();
            }
        }
        return (chosen, selected);
    }
    private void UpdateStatus()
    {
        status.Text = settingsWindow is not null ? "Configurações abertas — atalhos suspensos" : pause.Checked ? "Atalhos suspensos" : !registered ? "Atalho ocupado — abra Configurações" : clients.IsEmpty ? "Chrome desconectado" : "Chrome conectado";
        var (_, selected) = Selected();
        title.Text = selected is { } t ? t.GetProperty("title").GetString() : "Nenhum vídeo em reprodução";
    }
    private async void SendSeek(int delta)
    {
        var (client, target) = Selected();
        if (!registered || pause.Checked || client is null || target is null) return;
        if (!client.Sending.Wait(0)) return;
        var message = new { v = 1, type = "seek", id = Guid.NewGuid().ToString("N"), tabId = target.Value.GetProperty("tabId").GetInt32(), delta, expiresAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1500 };
        try { await client.Writer.WriteLineAsync(JsonSerializer.Serialize(message)); }
        catch { lastResult.Text = "Conexão interrompida; comando descartado"; }
        finally { client.Sending.Release(); }
    }
    private async Task AcceptClients()
    {
        while (!stop.IsCancellationRequested) {
            var pipe = new NamedPipeServerStream(Protocol.PipeName, PipeDirection.InOut, 16, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            try {
                await pipe.WaitForConnectionAsync(stop.Token);
                _ = ReadClient(pipe);
            } catch { pipe.Dispose(); if (stop.IsCancellationRequested) break; }
        }
    }
    private async Task ReadClient(NamedPipeServerStream pipe)
    {
        var id = Guid.NewGuid(); var client = new Client(pipe);
        clients[id] = client; UI(UpdateStatus);
        try {
            using var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 4096, true);
            while (await reader.ReadLineAsync(stop.Token) is { } line) {
                if (Protocol.Valid(line, "state")) {
                    using var doc = JsonDocument.Parse(line);
                    var t = doc.RootElement.GetProperty("target");
                    JsonElement? valid = null;
                    if (t.ValueKind != JsonValueKind.Null) {
                        if (t.GetProperty("tabId").GetInt32() < 0 || !t.GetProperty("playing").GetBoolean() || t.GetProperty("ad").GetBoolean() || !double.IsFinite(t.GetProperty("startedAt").GetDouble()) || t.GetProperty("title").GetString()!.Length > 300) break;
                        valid = t.Clone();
                    }
                    UI(() => { client.Target = valid; UpdateStatus(); });
                } else if (Protocol.Valid(line, "result")) {
                    using var doc = JsonDocument.Parse(line);
                    var r = doc.RootElement;
                    var text = r.GetProperty("ok").GetBoolean() ? "Último salto enviado ao vídeo" : r.TryGetProperty("reason", out var reason) ? reason.GetString() : "Comando não executado";
                    UI(() => lastResult.Text = text);
                } else break;
            }
        } catch { /* A closed Chrome process or invalid peer simply disconnects. */ }
        finally { clients.TryRemove(id, out _); client.Dispose(); UI(UpdateStatus); }
    }
    private void Configure()
    {
        if (closing) return;
        try {
            if (settingsWindow is null || settingsWindow.IsDisposed) {
                keys.Clear(); registered = false;
                var form = new SettingsForm(settings);
                settingsWindow = form;
                form.ShowInTaskbar = true;
                form.StartPosition = FormStartPosition.Manual;
                var area = Screen.FromPoint(Cursor.Position).WorkingArea;
                form.Location = new(area.Left + Math.Max(0, (area.Width - form.Width) / 2), area.Top + Math.Max(0, (area.Height - form.Height) / 2));
                form.SaveRequested = next => {
                    bool available = keys.Register(next); keys.Clear();
                    if (!available) return "Atalho ocupado por outro aplicativo. Escolha uma combinação diferente.";
                    try { next.Save(); settings = next; return null; }
                    catch (Exception e) { return "Não foi possível salvar: " + e.Message; }
                };
                form.FormClosed += (_, _) => {
                    settingsWindow = null;
                    if (!closing) { if (!pause.Checked) Register(); UpdateStatus(); }
                };
            }
            settingsWindow.Show();
            if (settingsWindow.WindowState == FormWindowState.Minimized) settingsWindow.WindowState = FormWindowState.Normal;
            // Startup uses SW_HIDE for the background app. Explicitly show this user-requested
            // window: Windows may apply STARTUPINFO to its first ShowWindow call.
            if (!SetWindowPos(settingsWindow.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0040)) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            settingsWindow.BringToFront(); settingsWindow.Activate();
            UpdateStatus();
        } catch (Exception error) {
            var failed = settingsWindow; settingsWindow = null; failed?.Dispose();
            if (!pause.Checked) Register();
            UpdateStatus();
            MessageBox.Show("Não foi possível abrir Configurações.\n\n" + error.Message, "YouTube Background", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    protected override void ExitThreadCore()
    {
        closing = true; settingsWindow?.Close(); stop.Cancel(); keys.Dispose();
        foreach (var c in clients.Values) { try { c.Pipe.Dispose(); } catch {} }
        tray.Visible = false; tray.Dispose(); dispatcher.Dispose();
        base.ExitThreadCore();
    }
}

internal sealed class SettingsForm : Form
{
    public Func<Settings, string?>? SaveRequested;
    private KeyboardHook? recorder;
    private readonly System.Windows.Forms.Timer timeout = new() { Interval = 15000 };
    private readonly Label hint = new() { AutoSize = true, MaximumSize = new(540, 0) };
    private Button? activeButton;
    private bool recording;
    private const string Help = "Clique em Gravar e pressione uma tecla, combinação ou gire o controle.\nSe o giro enviar Volume +/−, cada direção pode ser gravada separadamente.\nEnquanto esses atalhos estiverem ativos, eles substituem o controle de volume.";
    public SettingsForm(Settings s)
    {
        Text = "YouTube Background — Configurações";
        Icon = Brand.Icon;
        AutoScaleMode = AutoScaleMode.Dpi; ClientSize = new(590, 345);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; StartPosition = FormStartPosition.CenterScreen;
        var back = new TextBox { ReadOnly = true, Text = s.Back.ToString(), Tag = s.Back, Width = 220 };
        var forward = new TextBox { ReadOnly = true, Text = s.Forward.ToString(), Tag = s.Forward, Width = 220 };
        var seconds = new NumericUpDown { Minimum = 1, Maximum = 120, Value = s.Seconds, Width = 220 };
        var startup = new CheckBox { Text = "Iniciar ao entrar no Windows", Checked = s.Startup, AutoSize = true };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 6 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        void Row(string label, Control c) { layout.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 6, 0, 0) }); layout.Controls.Add(c); }
        Control CaptureRow(TextBox box) {
            var panel = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            var button = new Button { Text = "Gravar", AutoSize = true };
            button.Click += (_, _) => { if (recording && activeButton == button) StopRecording("Gravação cancelada. " + Help); else StartRecording(box, button); };
            panel.Controls.Add(box); panel.Controls.Add(button); return panel;
        }
        Row("Retroceder", CaptureRow(back)); Row("Avançar", CaptureRow(forward)); Row("Segundos por salto", seconds); Row("Inicialização", startup);
        hint.Text = Help;
        layout.Controls.Add(hint); layout.SetColumnSpan(hint, 2);
        var save = new Button { Text = "Salvar", AutoSize = true };
        save.Click += (_, _) => {
            StopRecording(Help);
            var next = new Settings((Shortcut)back.Tag!, (Shortcut)forward.Tag!, (int)seconds.Value, startup.Checked);
            if (!next.Back.Valid || !next.Forward.Valid || next.Back == next.Forward) { MessageBox.Show(this, "Escolha dois atalhos válidos e diferentes."); return; }
            var error = SaveRequested?.Invoke(next);
            if (error is null) Close(); else MessageBox.Show(this, error, "Configurações", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        layout.Controls.Add(save); Controls.Add(layout);
        timeout.Tick += (_, _) => StopRecording("Nenhuma tecla detectada. O controle pode precisar de configuração no software do teclado.\n" + Help);
        Deactivate += (_, _) => { if (recording) StopRecording("Gravação cancelada ao trocar de janela.\n" + Help); };
    }
    private void StartRecording(TextBox box, Button button)
    {
        StopRecording(Help);
        try { recorder ??= new KeyboardHook(); }
        catch (Exception e) { hint.Text = "Não foi possível iniciar a captura: " + e.Message; return; }
        recording = true; activeButton = button; button.Text = "Cancelar";
        hint.Text = "Aguardando… pressione o atalho ou gire o controle uma vez.\nEsc cancela. A gravação termina automaticamente após 15 segundos.";
        recorder.OnKey = shortcut => {
            if (!recording) return false;
            if (!shortcut.Valid) return false;
            recording = false;
            bool cancel = shortcut.Key == 27 && shortcut.Modifiers == 0;
            BeginInvoke(() => {
                if (!cancel) { box.Tag = shortcut; box.Text = shortcut.ToString(); }
                StopRecording(cancel ? "Gravação cancelada.\n" + Help : "Detectado: " + shortcut + ". Clique em Salvar para aplicar.\n" + Help);
            });
            return true;
        };
        timeout.Start();
    }
    private void StopRecording(string message) { recording = false; timeout.Stop(); if (recorder is not null) recorder.OnKey = null; if (activeButton is not null) activeButton.Text = "Gravar"; activeButton = null; hint.Text = message; }
    protected override void Dispose(bool disposing) { if (disposing) { recorder?.Dispose(); timeout.Dispose(); } base.Dispose(disposing); }
}
