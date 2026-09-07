namespace Gens.Client.Desktop.Platform;

public interface IApplicationPaths
{
    string UserData { get; }
    string Saves { get; }
    string Logs { get; }
    string Cache { get; }
    string SettingsFile { get; }
}

public sealed class DesktopApplicationPaths : IApplicationPaths
{
    public DesktopApplicationPaths(string? root = null)
    {
        UserData = root ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gens");
        Saves = Path.Combine(UserData, "saves");
        Logs = Path.Combine(UserData, "logs");
        Cache = Path.Combine(UserData, "cache");
        SettingsFile = Path.Combine(UserData, "settings.json");
    }

    public string UserData { get; }
    public string Saves { get; }
    public string Logs { get; }
    public string Cache { get; }
    public string SettingsFile { get; }
    public string Quicksave => Path.Combine(Saves, "quicksave.gens");
    public void EnsureRequiredDirectories() { Directory.CreateDirectory(UserData); Directory.CreateDirectory(Saves); Directory.CreateDirectory(Logs); Directory.CreateDirectory(Cache); }
}
