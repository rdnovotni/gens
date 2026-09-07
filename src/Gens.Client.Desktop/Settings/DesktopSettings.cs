using System.Text.Json;
using Gens.Client.Desktop.Platform;

namespace Gens.Client.Desktop.Settings;

public sealed record DesktopSettings
{
    public int Version { get; init; } = 1;
    public DisplaySettings Display { get; init; } = new();
    public AccessibilitySettings Accessibility { get; init; } = new();
    public DeveloperSettings Developer { get; init; } = new();
}
public sealed record DisplaySettings { public float UiScale { get; init; } = 1f; }
public sealed record AccessibilitySettings { public bool ReducedMotion { get; init; } }
public sealed record DeveloperSettings { public bool ConsoleEnabled { get; init; } }

public sealed class DesktopSettingsStore(IApplicationPaths paths)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public DesktopSettings Load()
    {
        try
        {
            if (!File.Exists(paths.SettingsFile)) return new();
            DesktopSettings? value = JsonSerializer.Deserialize<DesktopSettings>(File.ReadAllText(paths.SettingsFile), JsonOptions);
            return value is { Version: 1 } && value.Display.UiScale is >= 1f and <= 2f ? value : new();
        }
        catch (JsonException) { return new(); }
        catch (IOException) { return new(); }
    }

    public void Save(DesktopSettings settings)
    {
        Directory.CreateDirectory(paths.UserData);
        string temporary = paths.SettingsFile + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(settings, JsonOptions));
        if (File.Exists(paths.SettingsFile))
            File.Replace(temporary, paths.SettingsFile, null);
        else
            File.Move(temporary, paths.SettingsFile);
    }
}
