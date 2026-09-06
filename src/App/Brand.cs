using System.Reflection;
namespace YouTubeBackground;
internal static class Brand
{
    public static readonly Icon Icon = Load();
    private static Icon Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("YouTubeBackground.app.ico")!;
        using var icon = new Icon(stream, 32, 32);
        return (Icon)icon.Clone();
    }
}
