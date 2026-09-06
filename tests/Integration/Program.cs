using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

internal static class Integration
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string? className, string name);
    [DllImport("user32.dll")] static extern bool PostMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    static async Task Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--trigger") {
            var window = FindWindowEx(new IntPtr(-3), IntPtr.Zero, null, "YouTubeBackground");
            if (window == IntPtr.Zero || !PostMessage(window, 0x0312, new IntPtr(int.Parse(args[1])), IntPtr.Zero)) throw new Exception("Aplicativo de teste indisponível.");
            return;
        }
        string root = Path.GetFullPath(args[0]);
        if (FindWindowEx(new IntPtr(-3), IntPtr.Zero, null, "YouTubeBackground") != IntPtr.Zero) throw new Exception("Feche o aplicativo antes deste teste.");
        using var app = Process.Start(new ProcessStartInfo(Path.Combine(root, "app", "YouTubeBackground.exe")) { UseShellExecute = false, CreateNoWindow = true })!;
        Process? host = null;
        try {
            IntPtr hwnd = IntPtr.Zero;
            for (int i = 0; i < 50 && hwnd == IntPtr.Zero; i++) { await Task.Delay(100); hwnd = FindWindowEx(new IntPtr(-3), IntPtr.Zero, null, "YouTubeBackground"); }
            if (hwnd == IntPtr.Zero) throw new Exception("Aplicativo não criou janela de mensagens.");
            host = Process.Start(new ProcessStartInfo(Path.Combine(root, "host", "YouTubeBackground.Host.exe")) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true })!;
            async Task State(bool playing) {
                object? target = playing ? new { tabId = 42, playing = true, ad = false, startedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), title = "Vídeo de integração" } : null;
                byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(new { v = 1, type = "state", target });
                byte[] header = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(header, bytes.Length);
                await host.StandardInput.BaseStream.WriteAsync(header); await host.StandardInput.BaseStream.WriteAsync(bytes); await host.StandardInput.BaseStream.FlushAsync();
                await Task.Delay(350);
            }
            async Task<JsonElement> ReadCommand() {
                using var timeout = new CancellationTokenSource(5000);
                byte[] header = new byte[4]; await host.StandardOutput.BaseStream.ReadExactlyAsync(header, timeout.Token);
                byte[] body = new byte[BinaryPrimitives.ReadInt32LittleEndian(header)]; await host.StandardOutput.BaseStream.ReadExactlyAsync(body, timeout.Token);
                using var doc = JsonDocument.Parse(body); return doc.RootElement.Clone();
            }
            await State(true);
            var foreground = GetForegroundWindow();
            PostMessage(hwnd, 0x0312, new IntPtr(1), IntPtr.Zero);
            var back = await ReadCommand();
            if (back.GetProperty("delta").GetInt32() >= 0 || back.GetProperty("tabId").GetInt32() != 42) throw new Exception("Retrocesso incorreto.");
            PostMessage(hwnd, 0x0312, new IntPtr(2), IntPtr.Zero);
            var forward = await ReadCommand();
            if (forward.GetProperty("delta").GetInt32() <= 0) throw new Exception("Avanço incorreto.");
            if (GetForegroundWindow() != foreground) throw new Exception("O foco foi alterado durante o teste.");
            await State(false);
            PostMessage(hwnd, 0x0312, new IntPtr(1), IntPtr.Zero);
            await Task.Delay(300);
            await State(true);
            PostMessage(hwnd, 0x0312, new IntPtr(2), IntPtr.Zero);
            var afterPause = await ReadCommand();
            if (afterPause.GetProperty("delta").GetInt32() <= 0) throw new Exception("Vídeo pausado recebeu comando.");
            host.StandardInput.Close();
            await host.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Console.WriteLine("PASS: app + named pipe + native host framing; forward/back; no command while paused; focus preserved; host exits on EOF.");
        } finally {
            if (host is not null) { if (!host.HasExited) host.Kill(); host.Dispose(); }
            if (!app.HasExited) { app.Kill(); await app.WaitForExitAsync(); }
        }
    }
}
