using System.Runtime.InteropServices;
using YouTubeBackground;
using Shortcut = YouTubeBackground.Shortcut;

internal static class InputTests
{
    [StructLayout(LayoutKind.Sequential)] struct Input { public uint Type; public InputUnion Data; }
    [StructLayout(LayoutKind.Explicit)] struct InputUnion { [FieldOffset(0)] public Keyboard Keyboard; [FieldOffset(0)] public Mouse Mouse; }
    [StructLayout(LayoutKind.Sequential)] struct Keyboard { public ushort Key, Scan; public uint Flags, Time; public UIntPtr Extra; }
    [StructLayout(LayoutKind.Sequential)] struct Mouse { public int X, Y; public uint Data, Flags, Time; public UIntPtr Extra; }
    [DllImport("user32.dll", SetLastError = true)] static extern uint SendInput(uint count, Input[] input, int size);
    static int checks;
    static void Check(bool condition, string name) { checks++; if (!condition) throw new Exception(name); }
    static void Key(int key, bool up = false)
    {
        Input[] input = [new Input { Type = 1, Data = new InputUnion { Keyboard = new Keyboard { Key = (ushort)key, Flags = up ? 2u : 0 } } }];
        if (SendInput(1, input, Marshal.SizeOf<Input>()) != 1) throw new Exception("SendInput failed");
        Pump();
    }
    static void Pump() { for (int i = 0; i < 8; i++) { Application.DoEvents(); Thread.Sleep(10); } }
    [STAThread] static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Check(new Shortcut(0, 174).Valid, "volume without modifier");
        Check(!new Shortcut(0, 162).Valid, "modifier alone invalid");
        Check(Settings.Default.Back.Valid, "old defaults preserved");
        var captured = new List<Shortcut>();
        // Failsafe sink consumes test media events even while the tested binding is disabled.
        using var sink = new KeyboardHook { OnKey = s => s.IsMedia };
        using (var recorder = new KeyboardHook { OnKey = s => { captured.Add(s); return true; } }) {
            Key(174); Key(174); Key(174, true);
            Check(captured.Count == 1 && captured[0] == new Shortcut(0, 174), "capture + no repeat");
            Key(175); Key(175, true);
            Check(captured[1] == new Shortcut(0, 175), "opposite knob direction");
            Key(162);
            try { Key(175); Key(175, true); }
            finally { Key(162, true); }
            Check(captured[2] == new Shortcut(2, 175), "modifier tracked");
        }
        using (var hotkeys = new HotkeyWindow()) {
            var commands = new List<int>(); hotkeys.Pressed += commands.Add;
            Check(hotkeys.Register(new Settings(new(0,174), new(0,175), 5, false)), "media bindings registered");
            Key(174); Key(174); Key(174, true); Key(175); Key(175, true); Key(175); Key(175, true);
            Check(commands.SequenceEqual(new[] {1,2,2}), "runtime directions, no repeat, distinct pulses");
            hotkeys.Clear(); Key(175); Key(175, true);
            Check(commands.Count == 3, "suspension releases media binding");
        }
        using (var form = new SettingsForm(Settings.Default)) {
            form.ShowInTaskbar = false; form.Opacity = 0;
            form.StartPosition = FormStartPosition.Manual; form.Location = new(-30000, -30000);
            form.Show(); Pump(); form.PerformLayout();
            IEnumerable<Control> Descendants(Control root) { foreach (Control child in root.Controls) { yield return child; foreach (var nested in Descendants(child)) yield return nested; } }
            var recordButtons = Descendants(form).OfType<Button>().Where(b => b.Text == "Gravar").ToArray();
            var boxes = Descendants(form).OfType<TextBox>().ToArray();
            Check(recordButtons.Length == 2, "two recording buttons");
            recordButtons[0].PerformClick(); Key(174); Key(174, true);
            Check((Shortcut)boxes[0].Tag! == new Shortcut(0,174), "record button stores volume down");
            recordButtons[1].PerformClick(); Key(27); Key(27, true);
            Check((Shortcut)boxes[1].Tag! == Settings.Default.Forward, "Escape retains previous shortcut");
            recordButtons[1].PerformClick(); Key(175); Key(175, true);
            Check((Shortcut)boxes[1].Tag! == new Shortcut(0,175), "record button stores volume up");
            using var bitmap = new System.Drawing.Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0,0,form.Width,form.Height));
            bitmap.Save(Path.GetFullPath(args[0]));
            form.Hide();
        }
        Console.WriteLine($"PASS: {checks} input checks using Windows SendInput; capture, modifiers, media dispatch, repeat suppression and suspension. UI rendered.");
    }
}
