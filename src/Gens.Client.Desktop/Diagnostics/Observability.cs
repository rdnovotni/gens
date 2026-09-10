using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Gens.Client.Desktop.Platform;
using Gens.Client.Desktop.Settings;
using Gens.Runtime;

namespace Gens.Client.Desktop.Diagnostics;

/// <summary>
/// Aggregate, allowlisted gameplay counters for tuning. It deliberately has no API for arbitrary
/// properties, identifiers, text, monetary values, seeds, hashes, or individual event timelines.
/// </summary>
public sealed record BalanceTelemetry(
    int SessionsStarted = 0,
    int CampaignsStarted = 0,
    int CampaignsLoaded = 0,
    int MonthsAdvanced = 0,
    int CommandsAccepted = 0,
    int CommandsRejected = 0,
    int SavesSucceeded = 0,
    int SavesFailed = 0,
    int LoadsFailed = 0);

public enum BalanceMetric
{
    SessionStarted,
    CampaignStarted,
    CampaignLoaded,
    MonthAdvanced,
    CommandAccepted,
    CommandRejected,
    SaveSucceeded,
    SaveFailed,
    LoadFailed,
}

public sealed class PrivacyTelemetry
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly string file;
    private ConsentState consent;

    public PrivacyTelemetry(IApplicationPaths paths, ConsentState consent)
    {
        file = Path.Combine(paths.UserData, "anonymous-telemetry.json");
        this.consent = consent;
        Snapshot = consent == ConsentState.Granted ? Load() : new();
    }

    public BalanceTelemetry Snapshot { get; private set; }
    public bool IsEnabled => consent == ConsentState.Granted;

    public void SetConsent(ConsentState value)
    {
        consent = value;
        if (value != ConsentState.Granted)
        {
            Snapshot = new();
            try { if (File.Exists(file)) File.Delete(file); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        else
        {
            Record(BalanceMetric.SessionStarted);
        }
    }

    public void Record(BalanceMetric metric)
    {
        if (!IsEnabled) return;
        Snapshot = metric switch
        {
            BalanceMetric.SessionStarted => Snapshot with { SessionsStarted = Snapshot.SessionsStarted + 1 },
            BalanceMetric.CampaignStarted => Snapshot with { CampaignsStarted = Snapshot.CampaignsStarted + 1 },
            BalanceMetric.CampaignLoaded => Snapshot with { CampaignsLoaded = Snapshot.CampaignsLoaded + 1 },
            BalanceMetric.MonthAdvanced => Snapshot with { MonthsAdvanced = Snapshot.MonthsAdvanced + 1 },
            BalanceMetric.CommandAccepted => Snapshot with { CommandsAccepted = Snapshot.CommandsAccepted + 1 },
            BalanceMetric.CommandRejected => Snapshot with { CommandsRejected = Snapshot.CommandsRejected + 1 },
            BalanceMetric.SaveSucceeded => Snapshot with { SavesSucceeded = Snapshot.SavesSucceeded + 1 },
            BalanceMetric.SaveFailed => Snapshot with { SavesFailed = Snapshot.SavesFailed + 1 },
            BalanceMetric.LoadFailed => Snapshot with { LoadsFailed = Snapshot.LoadsFailed + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(metric), metric, null),
        };
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            string temporary = file + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(Snapshot, JsonOptions));
            File.Move(temporary, file, true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    public void Delete() => SetConsent(ConsentState.Denied);

    private BalanceTelemetry Load()
    {
        try { return File.Exists(file) ? JsonSerializer.Deserialize<BalanceTelemetry>(File.ReadAllText(file), JsonOptions) ?? new() : new(); }
        catch (JsonException) { return new(); }
        catch (IOException) { return new(); }
    }
}

public sealed record AnonymousCrashReport(
    int SchemaVersion,
    string ReportId,
    DateTimeOffset OccurredAtUtc,
    string ApplicationVersion,
    string OperatingSystem,
    string ProcessArchitecture,
    string FailureKind,
    string ExceptionType,
    IReadOnlyList<string> StackFrames);

/// <summary>Captures only structured, scrubbed crash data. Exception messages and source paths are never persisted.</summary>
public sealed class CrashReportStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly string directory;
    private ConsentState consent;

    public CrashReportStore(IApplicationPaths paths, ConsentState consent = ConsentState.NotAsked)
    {
        directory = Path.Combine(paths.CrashReports, "anonymous");
        this.consent = consent;
    }

    public int PendingCount => ReportFiles.Count;
    public IReadOnlyList<string> ReportFiles
    {
        get
        {
            try { return Directory.Exists(directory) ? Directory.GetFiles(directory, "*.json").Order(StringComparer.Ordinal).ToArray() : []; }
            catch (IOException) { return []; }
            catch (UnauthorizedAccessException) { return []; }
        }
    }

    public void SetConsent(ConsentState value)
    {
        consent = value;
        if (value == ConsentState.Denied) DeleteAll();
    }

    public string? Capture(Exception exception, string failureKind)
    {
        if (consent != ConsentState.Granted) return null;
        ArgumentNullException.ThrowIfNull(exception);
        try
        {
            Directory.CreateDirectory(directory);
            string id = Guid.NewGuid().ToString("N");
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset coarseTime = new(now.Year, now.Month, now.Day, now.Hour, 0, 0, TimeSpan.Zero);
            var report = new AnonymousCrashReport(
                1,
                id,
                coarseTime,
                Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "unknown",
                OperatingSystem.IsWindows() ? "windows" : OperatingSystem.IsMacOS() ? "macos" : OperatingSystem.IsLinux() ? "linux" : "other",
                RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
                NormalizeToken(failureKind),
                exception.GetType().Name,
                ScrubbedFrames(exception));
            string path = Path.Combine(directory, $"crash-{now:yyyyMMdd}-{id[..8]}.json");
            File.WriteAllText(path, JsonSerializer.Serialize(report, JsonOptions));
            return path;
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    public void DeleteAll()
    {
        foreach (string file in ReportFiles)
        {
            try { File.Delete(file); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static List<string> ScrubbedFrames(Exception exception)
    {
        var frames = new List<string>();
        for (Exception? current = exception; current is not null && frames.Count < 40; current = current.InnerException)
        {
            foreach (StackFrame frame in new StackTrace(current, false).GetFrames() ?? [])
            {
                MethodBase? method = frame.GetMethod();
                if (method is null) continue;
                string type = method.DeclaringType?.FullName ?? "unknown";
                frames.Add($"{type}.{method.Name}");
                if (frames.Count == 40) break;
            }
        }
        return frames;
    }

    private static string NormalizeToken(string value) => new(value.Where(static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_').Take(40).ToArray());
}

public sealed class CrashHandler(IApplicationPaths paths) : IDisposable
{
    public void Install()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandled;
        TaskScheduler.UnobservedTaskException += OnUnobservedTask;
    }

    public void Dispose()
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandled;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTask;
    }

    private void OnUnhandled(object sender, UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is Exception exception) Capture(exception, "unhandled");
    }

    private void OnUnobservedTask(object? sender, UnobservedTaskExceptionEventArgs args) => Capture(args.Exception, "unobserved-task");

    public string? Capture(Exception exception, string kind)
    {
        try
        {
            ConsentState consent = new SettingsService(paths).Current.Privacy.CrashReports;
            return new CrashReportStore(paths, consent).Capture(exception, kind);
        }
        catch (Exception captureFailure) when (captureFailure is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }
}

public sealed record DiagnosticsManifest(
    int SchemaVersion,
    DateTimeOffset ExportedAtUtc,
    string ApplicationVersion,
    string OperatingSystem,
    string ProcessArchitecture,
    RuntimeDiagnosticsSnapshot? Runtime,
    BalanceTelemetry? Balance,
    int CrashReportCount);

public sealed record RuntimeDiagnosticsSnapshot(
    ulong FramesPresented,
    ulong EventsDispatched,
    ulong WaitCount,
    double LastFrameMilliseconds,
    string Platform,
    string GraphicsBackend,
    string RendererMode);

public sealed class DiagnosticsExporter(IApplicationPaths paths, PrivacyTelemetry telemetry, CrashReportStore crashes)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public string Export(RuntimeDiagnostics? runtime = null)
    {
        Directory.CreateDirectory(paths.Logs);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset coarseTime = new(now.Year, now.Month, now.Day, now.Hour, 0, 0, TimeSpan.Zero);
        string path = Path.Combine(paths.Logs, $"gens-diagnostics-{now:yyyyMMdd}-{Guid.NewGuid():N}.zip");
        RuntimeDiagnosticsSnapshot? runtimeSnapshot = runtime is null ? null : new(
            runtime.FramesPresented, runtime.EventsDispatched, runtime.WaitCount, runtime.LastFrameDuration.TotalMilliseconds,
            runtime.PlatformName, runtime.GraphicsBackendName, runtime.RendererMode.ToString());
        var manifest = new DiagnosticsManifest(
            1,
            coarseTime,
            Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "unknown",
            OperatingSystem.IsWindows() ? "windows" : OperatingSystem.IsMacOS() ? "macos" : OperatingSystem.IsLinux() ? "linux" : "other",
            RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
            runtimeSnapshot,
            telemetry.IsEnabled ? telemetry.Snapshot : null,
            crashes.ReportFiles.Count);
        using ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create);
        WriteJson(archive, "manifest.json", manifest);
        foreach (string crash in crashes.ReportFiles)
        {
            ZipArchiveEntry entry = archive.CreateEntry($"crashes/{Path.GetFileName(crash)}", CompressionLevel.Optimal);
            using Stream destination = entry.Open();
            using FileStream source = File.OpenRead(crash);
            source.CopyTo(destination);
        }
        return path;
    }

    private static void WriteJson<T>(ZipArchive archive, string name, T value)
    {
        ZipArchiveEntry entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using Stream stream = entry.Open();
        JsonSerializer.Serialize(stream, value, JsonOptions);
    }
}
