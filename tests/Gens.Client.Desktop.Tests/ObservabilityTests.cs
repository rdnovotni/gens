using System.IO.Compression;
using Gens.Application.Campaign;
using Gens.Client.Desktop.App;
using Gens.Client.Desktop.Diagnostics;
using Gens.Client.Desktop.Platform;
using Gens.Client.Desktop.Settings;
using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class ObservabilityTests
{
    private string directory = null!;
    private DesktopApplicationPaths paths = null!;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "gens-observability-tests", Guid.NewGuid().ToString("N"));
        paths = new(directory);
        paths.EnsureRequiredDirectories();
    }

    [TearDown]
    public void TearDown() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

    [Test]
    public void CollectionIsOffUntilExplicitConsentAndOptOutDeletesCounters()
    {
        var telemetry = new PrivacyTelemetry(paths, ConsentState.NotAsked);
        telemetry.Record(BalanceMetric.MonthAdvanced);
        Assert.That(File.Exists(Path.Combine(directory, "anonymous-telemetry.json")), Is.False);

        telemetry.SetConsent(ConsentState.Granted);
        telemetry.Record(BalanceMetric.MonthAdvanced);
        Assert.Multiple(() =>
        {
            Assert.That(telemetry.Snapshot.SessionsStarted, Is.EqualTo(1));
            Assert.That(telemetry.Snapshot.MonthsAdvanced, Is.EqualTo(1));
        });

        telemetry.SetConsent(ConsentState.Denied);
        Assert.Multiple(() =>
        {
            Assert.That(telemetry.Snapshot, Is.EqualTo(new BalanceTelemetry()));
            Assert.That(File.Exists(Path.Combine(directory, "anonymous-telemetry.json")), Is.False);
        });
    }

    [Test]
    public void CrashReportOmitsMessagesPathsAndUserData()
    {
        var store = new CrashReportStore(paths, ConsentState.Granted);
        Exception exception = CaptureExceptionContaining("PRIVATE-NAME C:\\Users\\private\\save.gens");
        string reportPath = store.Capture(exception, "fatal startup")!;
        string json = File.ReadAllText(reportPath);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Not.Contain("PRIVATE-NAME"));
            Assert.That(json, Does.Not.Contain("Users"));
            Assert.That(json, Does.Not.Contain("save.gens"));
            Assert.That(json, Does.Contain("InvalidOperationException"));
            Assert.That(json, Does.Contain("fatalstartup"));
        });
    }

    [Test]
    public void DiagnosticsExportContainsOnlyAllowlistedFiles()
    {
        File.WriteAllText(paths.SettingsFile, "private settings");
        File.WriteAllText(Path.Combine(paths.Saves, "private.gens"), "private save");
        File.WriteAllText(Path.Combine(paths.Logs, "console.log"), "private log");
        var telemetry = new PrivacyTelemetry(paths, ConsentState.Granted);
        telemetry.Record(BalanceMetric.CampaignStarted);
        var crashes = new CrashReportStore(paths, ConsentState.Granted);
        crashes.Capture(CaptureExceptionContaining("PRIVATE"), "fatal");

        string export = new DiagnosticsExporter(paths, telemetry, crashes).Export();
        using ZipArchive archive = ZipFile.OpenRead(export);
        string[] names = archive.Entries.Select(static entry => entry.FullName).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(names, Does.Contain("manifest.json"));
            Assert.That(names.Count(static name => name.StartsWith("crashes/", StringComparison.Ordinal)), Is.EqualTo(1));
            Assert.That(names, Has.None.Contains("settings"));
            Assert.That(names, Has.None.Contains("save"));
            Assert.That(names, Has.None.Contains("console"));
        });
    }

    [Test]
    public void ControllerRecordsOnlyAggregateBalanceOutcomesAfterConsent()
    {
        var controller = new DesktopApplicationController(paths);
        Assert.That(controller.Modal?.Kind, Is.EqualTo(ModalKind.PrivacyConsent));
        controller.AcceptPrivacyConsent();
        controller.StartNew("latium", "standard", 987654321);
        controller.AdvanceMonth();
        controller.RequestAction(CampaignHouseholdAction.FundFestival);
        controller.ConfirmModal();

        BalanceTelemetry counters = controller.BalanceTelemetry;
        string persisted = File.ReadAllText(Path.Combine(directory, "anonymous-telemetry.json"));
        Assert.Multiple(() =>
        {
            Assert.That(counters.CampaignsStarted, Is.EqualTo(1));
            Assert.That(counters.MonthsAdvanced, Is.EqualTo(1));
            Assert.That(counters.CommandsAccepted + counters.CommandsRejected, Is.EqualTo(1));
            Assert.That(persisted, Does.Not.Contain("latium"));
            Assert.That(persisted, Does.Not.Contain("987654321"));
        });
    }

    private static Exception CaptureExceptionContaining(string privateMessage)
    {
        try { throw new InvalidOperationException(privateMessage); }
        catch (Exception exception) { return exception; }
    }
}
