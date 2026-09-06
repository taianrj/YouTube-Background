using System.Security.Principal;
using System.Text.Json;

namespace YouTubeBackground;
public static class Protocol
{
    public const int MaxBytes = 65536;
    public static string PipeName => "YouTubeBackground-" + WindowsIdentity.GetCurrent().User!.Value + "-" + System.Diagnostics.Process.GetCurrentProcess().SessionId;
    public static bool Valid(string json, string type)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(json) > MaxBytes) return false;
        try {
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;
            if (r.GetProperty("v").GetInt32() != 1 || r.GetProperty("type").GetString() != type) return false;
            if (type == "state") {
                var t = r.GetProperty("target");
                return t.ValueKind == JsonValueKind.Null || (t.GetProperty("tabId").GetInt32() >= 0 &&
                    t.GetProperty("playing").GetBoolean() && !t.GetProperty("ad").GetBoolean() &&
                    double.IsFinite(t.GetProperty("startedAt").GetDouble()) &&
                    t.GetProperty("title").GetString() is { Length: <= 300 });
            }
            if (type == "result") return r.GetProperty("id").GetString() is { Length: > 0 and <= 64 } &&
                r.GetProperty("ok").ValueKind is JsonValueKind.True or JsonValueKind.False;
            if (type == "seek") {
                long expires = r.GetProperty("expiresAt").GetInt64(), now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                int delta = r.GetProperty("delta").GetInt32();
                return r.GetProperty("id").GetString() is { Length: > 0 and <= 64 } &&
                    r.GetProperty("tabId").GetInt32() >= 0 && delta is >= -120 and <= 120 && delta != 0 && expires >= now && expires <= now + 3000;
            }
            return false;
        } catch { return false; }
    }
}
