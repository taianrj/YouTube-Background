using System.Text.Json;
using YouTubeBackground;
int count = 0;
void Check(bool condition) { count++; if (!condition) throw new Exception("Failed test " + count); }
Check(Protocol.Valid("{\"v\":1,\"type\":\"state\",\"target\":null}", "state"));
Check(!Protocol.Valid("{\"v\":2,\"type\":\"state\",\"target\":null}", "state"));
Check(!Protocol.Valid("{}", "state"));
Check(!Protocol.Valid("not json", "state"));
Check(!Protocol.Valid(new string('x', 65537), "state"));
var seek = new { v = 1, type = "seek", id = "test", tabId = 1, delta = 5, expiresAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1000 };
Check(Protocol.Valid(JsonSerializer.Serialize(seek), "seek"));
Check(!Protocol.Valid(JsonSerializer.Serialize(seek with { delta = 121 }), "seek"));
Check(!Protocol.Valid(JsonSerializer.Serialize(seek with { expiresAt = 0 }), "seek"));
Check(!Protocol.Valid(JsonSerializer.Serialize(seek with { delta = 0 }), "seek"));
Check(!Protocol.Valid(JsonSerializer.Serialize(seek with { tabId = -1 }), "seek"));
Check(Protocol.Valid("{\"v\":1,\"type\":\"result\",\"id\":\"x\",\"ok\":false}", "result"));
Check(!Protocol.Valid("{\"v\":1,\"type\":\"result\",\"id\":\"x\",\"ok\":3}", "result"));
Console.WriteLine($"Protocol: {count} assertions passed.");
