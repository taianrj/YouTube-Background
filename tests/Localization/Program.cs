using System.Globalization;
using System.Reflection;
using System.Text.Json;
using YouTubeBackground;
using Shortcut = YouTubeBackground.Shortcut;

internal static class LocalizationTests
{
    static int checks;
    static void Check(bool condition, string message) { checks++; if (!condition) throw new Exception(message); }
    static IEnumerable<Control> Descendants(Control root) {
        foreach (Control child in root.Controls) { yield return child; foreach (var nested in Descendants(child)) yield return nested; }
    }
    [STAThread] static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        var originalCulture = CultureInfo.CurrentUICulture;
        var serialized = JsonSerializer.Serialize(Settings.Default);
        var output = args.FirstOrDefault();
        if (output is not null) Directory.CreateDirectory(output);
        try {
            foreach (var (culture, expected) in new[] { ("en-US", "en"), ("en-GB", "en"), ("pt-BR", "pt"), ("pt-PT", "pt"), ("es-ES", "es"), ("es-MX", "es"), ("fr-FR", "en"), ("ja-JP", "en") })
                Check(Ui.LanguageFor(CultureInfo.GetCultureInfo(culture)) == expected, "Language selection: " + culture);
            foreach (var culture in new[] { "en-US", "pt-BR", "es-ES", "fr-FR" }) {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
                foreach (var key in Ui.Messages.Keys) Check(!string.IsNullOrWhiteSpace(Ui.Text(key)), culture + ": " + key);
                Check(Ui.Format("Detected", "Ctrl + A").Contains("Ctrl + A"), "Recording placeholder");
                Check(Ui.CommandFailure("O vídeo selecionado mudou ou parou.") == Ui.Text("TargetChanged"), "Wire error translation");
                Check(Ui.CommandFailure("unknown peer message") == Ui.Text("NotExecuted"), "Unknown error fallback");
                Check(new Shortcut(0, 176).ToString() == Ui.Text("NextTrack"), "Media key translation");
                Check(JsonSerializer.Serialize(Settings.Default) == serialized, "Language changed stored preferences");
                int settingsClicks = 0, exitClicks = 0;
                using (var menu = TrayApp.CreateMenu(new(Ui.Text("Disconnected")) { Enabled = false },
                    new(Ui.Text("NoVideo")) { Enabled = false }, new(Ui.Text("WaitingCommand")) { Enabled = false },
                    new(Ui.Text("Suspend")) { CheckOnClick = true }, () => settingsClicks++, () => exitClicks++)) {
                    var items = menu.Items.OfType<ToolStripMenuItem>().ToArray();
                    Check(items.Select(i => i.Text).SequenceEqual(new[] { Ui.Text("Disconnected"), Ui.Text("NoVideo"),
                        Ui.Text("WaitingCommand"), Ui.Text("SettingsMenu"), Ui.Text("Suspend"), Ui.Text("Exit") }), "Tray labels");
                    items[3].PerformClick(); items[4].PerformClick(); items[5].PerformClick();
                    Check(settingsClicks == 1 && exitClicks == 1 && items[4].Checked, "Tray actions retained");
                    if (output is not null) {
                        menu.Opacity = 0;
                        menu.Show(new Point(-30000, -30000));
                        Application.DoEvents();
                        using var bitmap = new Bitmap(menu.Width, menu.Height);
                        menu.DrawToBitmap(bitmap, new Rectangle(Point.Empty, menu.Size));
                        bitmap.Save(Path.Combine(output, culture + "-Tray.png"));
                        menu.Close();
                    }
                }
                using var form = new SettingsForm(Settings.Default);
                form.ShowInTaskbar = false;
                form.Opacity = 0;
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-30000, -30000);
                form.Show();
                Application.DoEvents();
                form.PerformLayout();
                var controls = Descendants(form).ToArray();
                Check(form.Text == Ui.Text("SettingsTitle"), "Window title");
                Check(controls.OfType<Button>().Count(b => b.Text == Ui.Text("Record")) == 2, "Record buttons");
                var save = controls.OfType<Button>().Single(b => b.Text == Ui.Text("Save"));
                var hint = (Label)typeof(SettingsForm).GetField("hint", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(form)!;
                foreach (var state in new[] { "Help", "Recording", "Timeout", "Cancelled", "Deactivated" }) {
                    hint.Text = Ui.Text(state) + (state is "Timeout" or "Cancelled" or "Deactivated" ? Ui.Text("Help") : "");
                    form.PerformLayout();
                    Check(hint.Bottom <= save.Top && save.Bottom <= form.ClientSize.Height, culture + ": clipped " + state);
                    if (output is not null) {
                        using var bitmap = new Bitmap(form.Width, form.Height);
                        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
                        bitmap.Save(Path.Combine(output, culture + "-" + state + ".png"));
                    }
                }
            }
        } finally { CultureInfo.CurrentUICulture = originalCulture; }
        Console.WriteLine($"PASS: {checks} localization checks; language fallback, messages, media keys, wire errors, unchanged preferences and settings layout. No hooks or settings writes.");
    }
}
