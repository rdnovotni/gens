using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Gens.Client.Desktop.Platform;
using Gens.Runtime;

namespace Gens.Client.Desktop.Diagnostics;

public enum AppLogCategory { Simulation, Application, Runtime, Platform, Graphics, UI, Scene2D, Audio, Art, Save, Localization, Accessibility, Client, Performance }

public sealed record ReleaseMetadata(string Version, string CommitSha, string Channel)
{
    public static ReleaseMetadata Current { get; } = new(
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "0.0.0",
        Environment.GetEnvironmentVariable("GENS_COMMIT_SHA") ?? "development",
        Environment.GetEnvironmentVariable("GENS_BUILD_CHANNEL") ?? "local");
    public string Display => $"{Version} ({Channel}, {CommitSha[..Math.Min(CommitSha.Length, 8)]})";
}

public sealed class StructuredFileLogger : IRuntimeLogger, IDisposable
{
    private static readonly Regex Sensitive = new("(?i)(token|secret|api[-_]?key|authorization)(\\s*[:=]\\s*)([^\\s,;]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly object gate = new();
    private readonly string directory;
    private readonly long maxBytes;
    private readonly int retainedFiles;
    private readonly Queue<string> recent = new();
    private StreamWriter writer;

    public StructuredFileLogger(IApplicationPaths paths, long maxBytes = 2 * 1024 * 1024, int retainedFiles = 7)
    {
        directory = paths.Logs; this.maxBytes = maxBytes; this.retainedFiles = retainedFiles; Directory.CreateDirectory(directory); writer = Open();
    }
    public IReadOnlyList<string> Recent { get { lock (gate) return recent.ToArray(); } }
    public void Log(RuntimeLogLevel level, string message, Exception? exception = null) => Log(AppLogCategory.Runtime, level, message, exception);
    public void Log(AppLogCategory category, RuntimeLogLevel level, string message, Exception? exception = null)
    {
        lock (gate)
        {
            if (writer.BaseStream.Length >= maxBytes) Rotate();
            string safeMessage = Redact(message);
            string line = JsonSerializer.Serialize(new { timestamp = DateTimeOffset.UtcNow, level = level.ToString(), category = category.ToString(), message = safeMessage, exception = exception is null ? null : Redact(exception.ToString()) });
            writer.WriteLine(line); writer.Flush(); recent.Enqueue(line); while (recent.Count > 200) recent.Dequeue();
        }
    }
    public void Dispose() { lock (gate) writer.Dispose(); }
    public static string Redact(string value) => Sensitive.Replace(value, "$1$2[REDACTED]");
    private StreamWriter Open() => new(new FileStream(Path.Combine(directory, "gens.log"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite)) { AutoFlush = true };
    private void Rotate()
    {
        writer.Dispose(); string archived = Path.Combine(directory, $"gens-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}.log"); File.Move(Path.Combine(directory, "gens.log"), archived, true);
        foreach (FileInfo stale in new DirectoryInfo(directory).GetFiles("gens-*.log").OrderByDescending(static file => file.CreationTimeUtc).Skip(retainedFiles - 1)) stale.Delete(); writer = Open();
    }
}

public sealed class CrashReporter(IApplicationPaths paths, StructuredFileLogger logger)
{
    private static readonly JsonSerializerOptions ReportJsonOptions = new() { WriteIndented = true };
    public string Capture(Exception exception, string activeScreen, string platformBackend, string graphicsBackend, string audioBackend, string rendererMode)
    {
        Directory.CreateDirectory(paths.CrashReports);
        string id = $"crash-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}-{Guid.NewGuid():N}";
        string path = Path.Combine(paths.CrashReports, id + ".json");
        var report = new
        {
            id,
            capturedUtc = DateTimeOffset.UtcNow,
            release = ReleaseMetadata.Current,
            os = RuntimeInformation.OSDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            runtime = RuntimeInformation.FrameworkDescription,
            platformBackend,
            graphicsBackend,
            audioBackend,
            activeScreen,
            rendererMode,
            saveFormatVersion = 2,
            recentLogs = logger.Recent,
            exception = StructuredFileLogger.Redact(exception.ToString()),
        };
        File.WriteAllText(path, JsonSerializer.Serialize(report, ReportJsonOptions)); return path;
    }
}
