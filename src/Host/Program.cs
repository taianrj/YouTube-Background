using System.Buffers.Binary;
using System.IO.Pipes;
using System.Text;
using YouTubeBackground;

// Chrome owns this process. stdout is exclusively the native messaging protocol.
using var stop = new CancellationTokenSource();
using var input = Console.OpenStandardInput();
using var output = Console.OpenStandardOutput();
try
{
    using var pipe = new NamedPipeClientStream(".", Protocol.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
    await pipe.ConnectAsync(5000, stop.Token);
    using var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 4096, true);
    using var writer = new StreamWriter(pipe, new UTF8Encoding(false), 4096, true) { AutoFlush = true };
    async Task FromChrome()
    {
        var header = new byte[4];
        while (true) {
            await input.ReadExactlyAsync(header, stop.Token);
            int size = BinaryPrimitives.ReadInt32LittleEndian(header);
            if (size < 1 || size > Protocol.MaxBytes) return;
            var bytes = new byte[size];
            await input.ReadExactlyAsync(bytes, stop.Token);
            var json = Encoding.UTF8.GetString(bytes);
            if (!Protocol.Valid(json, "state") && !Protocol.Valid(json, "result")) return;
            // Parse + serialize removes literal newlines before the line-based local transport.
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            await writer.WriteLineAsync(System.Text.Json.JsonSerializer.Serialize(doc.RootElement).AsMemory(), stop.Token);
        }
    }
    async Task ToChrome()
    {
        while (await reader.ReadLineAsync(stop.Token) is { } line) {
            if (!Protocol.Valid(line, "seek")) return;
            var bytes = Encoding.UTF8.GetBytes(line);
            var header = new byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(header, bytes.Length);
            await output.WriteAsync(header, stop.Token);
            await output.WriteAsync(bytes, stop.Token);
            await output.FlushAsync(stop.Token);
        }
    }
    await Task.WhenAny(FromChrome(), ToChrome());
    stop.Cancel();
}
catch (Exception ex) { Console.Error.WriteLine(ex.Message); }
