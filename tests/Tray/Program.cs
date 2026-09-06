using System.Reflection;
using YouTubeBackground;
internal static class TrayTests
{
    static void Pump() { for (int i = 0; i < 15; i++) { Application.DoEvents(); Thread.Sleep(10); } }
    static SettingsForm Window() => Application.OpenForms.OfType<SettingsForm>().Single();
    static void Check(bool condition, string reason) { if (!condition) throw new Exception(reason); }
    [STAThread] static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using var app = new TrayApp();
        try {
            var tray = (NotifyIcon)typeof(TrayApp).GetField("tray", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(app)!;
            var menu = tray.ContextMenuStrip!.Items.OfType<ToolStripMenuItem>().Single(i => i.Text == "Configurações…");
            menu.PerformClick(); Pump();
            var first = Window(); Check(first.Visible && first.ShowInTaskbar, "settings menu did not show a reachable window");
            Check(Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(first.Bounds)), "settings outside screen");
            menu.PerformClick(); Pump(); Check(ReferenceEquals(first, Window()), "duplicate settings window");
            first.WindowState = FormWindowState.Minimized;
            menu.PerformClick(); Pump(); Check(Window().WindowState == FormWindowState.Normal, "not restored");
            first.Close(); Pump(); Check(!Application.OpenForms.OfType<SettingsForm>().Any(), "window did not close");
            menu.PerformClick(); Pump(); var reopened = Window(); Check(reopened.Visible && !ReferenceEquals(first, reopened), "cannot reopen");
            reopened.Close(); Pump();
            HotkeyWindow.RequestSettings(); Pump(); Check(Window().Visible, "second-instance settings request failed");
            Window().Close(); Pump();
            Console.WriteLine("PASS: actual tray menu handler opens visible settings; same window reused; minimized window restored; close/reopen; existing-instance request.");
        } finally { app.ExitThread(); Pump(); }
    }
}
