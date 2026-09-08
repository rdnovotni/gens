namespace Gens.Client.Desktop.Platform;

public interface IApplicationPaths
{
    string UserData { get; }
    string Saves { get; }
    string Settings { get; }
    string Logs { get; }
    string Cache { get; }
    string GeneratedArt { get; }
    string Screenshots { get; }
    string CrashReports { get; }
    string Mods { get; }
    string SettingsFile { get; }
    string Quicksave { get; }
    void EnsureRequiredDirectories();
    string ResolveSafePath(string root, string untrustedRelativePath);
}

public sealed class DesktopApplicationPaths : IApplicationPaths
{
    public DesktopApplicationPaths(string? root = null)
    {
        UserData = Path.GetFullPath(root ?? DefaultUserDataRoot());
        Saves = Child("saves"); Settings = Child("settings"); Logs = Child("logs"); Cache = Child("cache");
        GeneratedArt = Child("generated-art"); Screenshots = Child("screenshots"); CrashReports = Child("crash-reports"); Mods = Child("mods");
        SettingsFile = Path.Combine(Settings, "settings.json"); Quicksave = Path.Combine(Saves, "quicksave.gens");
    }

    public string UserData { get; }
    public string Saves { get; }
    public string Settings { get; }
    public string Logs { get; }
    public string Cache { get; }
    public string GeneratedArt { get; }
    public string Screenshots { get; }
    public string CrashReports { get; }
    public string Mods { get; }
    public string SettingsFile { get; }
    public string Quicksave { get; }

    public void EnsureRequiredDirectories()
    {
        foreach (string path in new[] { UserData, Saves, Settings, Logs, Cache, GeneratedArt, Screenshots, CrashReports, Mods }) Directory.CreateDirectory(path);
    }

    public string ResolveSafePath(string root, string untrustedRelativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(untrustedRelativePath);
        string fullRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        string candidate = Path.GetFullPath(Path.Combine(root, untrustedRelativePath));
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!candidate.StartsWith(fullRoot, comparison)) throw new InvalidOperationException("The requested path escapes its application directory.");
        return candidate;
    }

    private string Child(string name) => Path.Combine(UserData, name);
    private static string DefaultUserDataRoot()
    {
        if (OperatingSystem.IsWindows()) return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Gens");
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (OperatingSystem.IsMacOS()) return Path.Combine(userProfile, "Library", "Application Support", "Gens");
        string? xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        return Path.Combine(string.IsNullOrWhiteSpace(xdg) ? Path.Combine(userProfile, ".local", "share") : xdg, "gens");
    }
}
