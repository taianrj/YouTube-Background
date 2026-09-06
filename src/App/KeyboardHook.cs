using System.Runtime.InteropServices;

namespace YouTubeBackground;

// Only configured multimedia events, or a user-initiated recording, are consumed.
// No keystrokes are stored or logged. Callbacks must remain short.
internal sealed class KeyboardHook : IDisposable
{
    private delegate IntPtr HookProc(int code, IntPtr message, IntPtr data);
    [DllImport("user32.dll", SetLastError = true)] static extern IntPtr SetWindowsHookEx(int id, HookProc callback, IntPtr module, uint thread);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);
    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] static extern IntPtr GetModuleHandle(string? module);
    private readonly HookProc callback;
    private readonly HashSet<int> modifiers = [];
    private readonly HashSet<int> consumed = [];
    private IntPtr handle;
    public Func<Shortcut, bool>? OnKey;
    public KeyboardHook()
    {
        callback = Handle;
        foreach (int key in new[] { 160, 161, 162, 163, 164, 165, 91, 92 }) if (GetAsyncKeyState(key) < 0) modifiers.Add(key);
        handle = SetWindowsHookEx(13, callback, GetModuleHandle(null), 0);
        if (handle == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }
    private IntPtr Handle(int code, IntPtr message, IntPtr data)
    {
        if (code >= 0) {
            int key = Marshal.ReadInt32(data), msg = message.ToInt32();
            bool down = msg is 0x100 or 0x104, up = msg is 0x101 or 0x105;
            if (Shortcut.IsModifier(key)) { if (down) modifiers.Add(key); if (up) modifiers.Remove(key); }
            else if (up && consumed.Remove(key)) return new IntPtr(1);
            else if (down) {
                if (consumed.Contains(key)) return new IntPtr(1);
                uint mods = (modifiers.Overlaps(new[] { 16,160,161 }) ? 4u : 0) |
                    (modifiers.Overlaps(new[] { 17,162,163 }) ? 2u : 0) |
                    (modifiers.Overlaps(new[] { 18,164,165 }) ? 1u : 0) |
                    (modifiers.Overlaps(new[] { 91,92 }) ? 8u : 0);
                try { if (OnKey?.Invoke(new Shortcut(mods, key)) == true) { consumed.Add(key); return new IntPtr(1); } }
                catch { /* Never allow an exception to cross the native callback boundary. */ }
            }
        }
        return CallNextHookEx(handle, code, message, data);
    }
    public void Dispose() { if (handle != IntPtr.Zero) { UnhookWindowsHookEx(handle); handle = IntPtr.Zero; } GC.KeepAlive(callback); }
}
