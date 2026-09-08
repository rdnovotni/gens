using System.Text.Json;
using Gens.Client.Desktop.Diagnostics;
using Gens.Client.Desktop.Platform;
using Gens.Runtime;
using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class ProductionServiceTests
{
    private string directory = null!;
    [SetUp] public void SetUp() { directory = Path.Combine(Path.GetTempPath(), "gens-production-tests", Guid.NewGuid().ToString("N")); }
    [TearDown] public void TearDown() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

    [Test]
    public void StructuredLogsRotateAndRedactSecrets()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories();
        using (var logger = new StructuredFileLogger(paths, 100, 3)) for (int i = 0; i < 12; i++) logger.Log(AppLogCategory.Client, RuntimeLogLevel.Information, $"entry={i} token=private-value padding-padding-padding");
        FileInfo[] logs = new DirectoryInfo(paths.Logs).GetFiles("*.log"); string text = string.Join("\n", logs.Select(static file => File.ReadAllText(file.FullName)));
        Assert.Multiple(() => { Assert.That(logs.Length, Is.LessThanOrEqualTo(3)); Assert.That(text, Does.Not.Contain("private-value")); Assert.That(text, Does.Contain("[REDACTED]")); });
    }

    [Test]
    public void CrashReportContainsMetadataButNotSecretOrCampaignState()
    {
        var paths = new DesktopApplicationPaths(directory); paths.EnsureRequiredDirectories(); using var logger = new StructuredFileLogger(paths);
        logger.Log(AppLogCategory.Client, RuntimeLogLevel.Error, "authorization=Bearer-secret");
        string reportPath = new CrashReporter(paths, logger).Capture(new InvalidOperationException("controlled"), "Settings", "SDL3", "Skia", "Null", "software");
        using JsonDocument report = JsonDocument.Parse(File.ReadAllText(reportPath)); string text = report.RootElement.GetRawText();
        Assert.Multiple(() => { Assert.That(text, Does.Contain("controlled")); Assert.That(text, Does.Contain("saveFormatVersion")); Assert.That(text, Does.Not.Contain("Bearer-secret")); Assert.That(text, Does.Not.Contain("world.json")); });
    }
}
