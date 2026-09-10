using System.Text.Json;
using Gens.Art;
using Gens.Client.Desktop.Platform;
using Gens.UI;

namespace Gens.Client.Desktop.Settings;

public sealed record DesktopSettings
{
    public const int CurrentVersion = 2;
    public int Version { get; init; } = CurrentVersion;
    public DisplaySettings Display { get; init; } = new();
    public AudioSettings Audio { get; init; } = new();
    public AccessibilitySettings Accessibility { get; init; } = new();
    public LanguageSettings Language { get; init; } = new();
    public DeveloperSettings Developer { get; init; } = new();
    public ArtSettings Art { get; init; } = new();
    public PrivacySettings Privacy { get; init; } = new();
}
public sealed record DisplaySettings { public float UiScale { get; init; } = 1f; }
public sealed record AudioSettings
{
    public float MasterVolume { get; init; } = 1f;
    public float MusicVolume { get; init; } = .8f;
    public float AmbienceVolume { get; init; } = .8f;
    public float EffectsVolume { get; init; } = 1f;
    public float UiVolume { get; init; } = 1f;
    public bool Muted { get; init; }
}
public sealed record AccessibilitySettings { public bool ReducedMotion { get; init; } public MotionMode Motion { get; init; } = MotionMode.Full; public bool HighContrast { get; init; } }
public sealed record LanguageSettings { public string Locale { get; init; } = "en"; }
public sealed record DeveloperSettings { public bool ConsoleEnabled { get; init; } }
public enum ConsentState { NotAsked, Granted, Denied }
public sealed record PrivacySettings
{
    public ConsentState UsageTelemetry { get; init; }
    public ConsentState CrashReports { get; init; }
}
public sealed class SettingsChangedEventArgs(DesktopSettings previous, DesktopSettings current, string domain) : EventArgs
{
    public DesktopSettings Previous { get; } = previous;
    public DesktopSettings Current { get; } = current;
    public string Domain { get; } = domain;
}

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IApplicationPaths paths;
    private readonly Action<string, Exception?> log;
    public SettingsService(IApplicationPaths paths, Action<string, Exception?>? log = null) { this.paths = paths; this.log = log ?? ((_, _) => { }); Current = Load(); }
    public DesktopSettings Current { get; private set; }
    public event EventHandler<SettingsChangedEventArgs>? Changed;

    public DesktopSettings Load()
    {
        try
        {
            if (!File.Exists(paths.SettingsFile)) return new();
            string json = File.ReadAllText(paths.SettingsFile);
            using JsonDocument document = JsonDocument.Parse(json);
            int version = document.RootElement.TryGetProperty("version", out JsonElement element) ? element.GetInt32() : 1;
            DesktopSettings value = JsonSerializer.Deserialize<DesktopSettings>(json, JsonOptions) ?? throw new JsonException("Settings root is null.");
            return Validate(Migrate(value, version));
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            log("Settings could not be loaded; safe defaults are active.", ex); PreserveInvalidFile(); return new();
        }
    }

    public void Update(DesktopSettings value, string domain)
    {
        DesktopSettings validated = Validate(value with { Version = DesktopSettings.CurrentVersion });
        DesktopSettings previous = Current; Current = validated;
        try { Save(validated); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log($"Settings could not be saved to {paths.SettingsFile}; the change was not persisted.", ex); Current = previous; return;
        }
        Changed?.Invoke(this, new(previous, validated, domain));
    }

    public void Save(DesktopSettings settings)
    {
        Directory.CreateDirectory(paths.Settings);
        string temporary = paths.SettingsFile + ".tmp";
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(settings, JsonOptions);
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough)) { stream.Write(bytes); stream.Flush(true); }
        if (File.Exists(paths.SettingsFile))
        {
            try { File.Replace(temporary, paths.SettingsFile, paths.SettingsFile + ".bak", true); }
            catch (PlatformNotSupportedException) { File.Move(temporary, paths.SettingsFile, true); }
        }
        else File.Move(temporary, paths.SettingsFile);
    }

    private static DesktopSettings Migrate(DesktopSettings value, int version) => version switch
    {
        1 => value with { Version = DesktopSettings.CurrentVersion, Accessibility = value.Accessibility with { Motion = value.Accessibility.ReducedMotion ? MotionMode.Reduced : MotionMode.Full } },
        DesktopSettings.CurrentVersion => value,
        _ => throw new JsonException($"Unsupported settings version {version}."),
    };
    private static DesktopSettings Validate(DesktopSettings value) => value with
    {
        Display = value.Display with { UiScale = Math.Clamp(value.Display.UiScale, 1f, 2f) },
        Audio = value.Audio with
        {
            MasterVolume = Clamp(value.Audio.MasterVolume),
            MusicVolume = Clamp(value.Audio.MusicVolume),
            AmbienceVolume = Clamp(value.Audio.AmbienceVolume),
            EffectsVolume = Clamp(value.Audio.EffectsVolume),
            UiVolume = Clamp(value.Audio.UiVolume),
        },
        Language = value.Language with { Locale = string.IsNullOrWhiteSpace(value.Language.Locale) ? "en" : value.Language.Locale },
    };
    private static float Clamp(float value) => Math.Clamp(float.IsFinite(value) ? value : 0, 0, 1);
    private void PreserveInvalidFile()
    {
        try { if (File.Exists(paths.SettingsFile)) File.Move(paths.SettingsFile, paths.SettingsFile + $".corrupt-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}", false); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}

[Obsolete("Use SettingsService.")]
public sealed class DesktopSettingsStore
{
    private readonly SettingsService service;
    public DesktopSettingsStore(IApplicationPaths paths) => service = new(paths);
    public DesktopSettings Load() => service.Current;
    public void Save(DesktopSettings settings) => service.Save(settings);
}
