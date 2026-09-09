using Gens.Client.Desktop.Diagnostics;
using Gens.Client.Desktop.Platform;
using Gens.Runtime;
using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class DiagnosticsTests
{
    private string directory = null!;
    [SetUp] public void SetUp() { directory = Path.Combine(Path.GetTempPath(), "gens-diagnostics-tests", Guid.NewGuid().ToString("N")); }
    [TearDown] public void TearDown() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

    // Same structural-failure technique as SettingsAndPathsTests.UnwritableSettingsRootFailsSaveWithoutThrowingOrLosingPriorState:
    // a plain file occupies the directory slot so Directory.CreateDirectory fails regardless of the running user's privileges.
    [Test]
    public void UnwritableLogsRootDoesNotThrowOnConstructOrLog()
    {
        var paths = new DesktopApplicationPaths(directory);
        Directory.CreateDirectory(paths.UserData);
        File.WriteAllText(paths.Logs, "a file blocking the logs directory");

        StructuredFileLogger? logger = null;
        Assert.DoesNotThrow(() => logger = new StructuredFileLogger(paths));
        Assert.DoesNotThrow(() => logger!.Log(AppLogCategory.Client, RuntimeLogLevel.Information, "should not throw even though the log file can never be written"));
        Assert.That(logger!.Recent, Has.Count.EqualTo(1));
        logger.Dispose();
    }

    [Test]
    public void UnwritableCrashReportsRootDoesNotThrowAndReturnsNull()
    {
        var paths = new DesktopApplicationPaths(directory);
        Directory.CreateDirectory(paths.UserData);
        File.WriteAllText(paths.CrashReports, "a file blocking the crash-reports directory");
        using var logger = new StructuredFileLogger(paths);

        string? report = null;
        Assert.DoesNotThrow(() => report = new CrashReporter(paths, logger).Capture(new InvalidOperationException("boom"), "roster", "SDL3", "SkiaSharp", "null", "software"));
        Assert.That(report, Is.Null);
    }

    [Test]
    public void LoggerRotatesAndCrashReporterCapturesUnderWritableRoot()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        using var logger = new StructuredFileLogger(paths, maxBytes: 1);
        logger.Log(AppLogCategory.Client, RuntimeLogLevel.Information, "first line forces rotation on the next write");
        logger.Log(AppLogCategory.Client, RuntimeLogLevel.Information, "second line lands after rotation");
        Assert.That(Directory.GetFiles(paths.Logs, "gens-*.log"), Has.Length.EqualTo(1));

        string? report = new CrashReporter(paths, logger).Capture(new InvalidOperationException("boom"), "roster", "SDL3", "SkiaSharp", "null", "software");
        Assert.That(report, Is.Not.Null);
        Assert.That(File.Exists(report!), Is.True);
    }
}
