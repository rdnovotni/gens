using Gens.Application.Campaign;
using Gens.Client.Desktop.App;
using Gens.Client.Desktop.Platform;
using Gens.Client.Desktop.Settings;
using Gens.UI;
using NUnit.Framework;

namespace Gens.Client.Desktop.Tests;

[TestFixture]
public sealed class DesktopApplicationFlowTests
{
    private string directory = null!;
    private DesktopApplicationController app = null!;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "gens-desktop-tests", Guid.NewGuid().ToString("N"));
        app = new(new DesktopApplicationPaths(directory));
    }

    [TearDown]
    public void TearDown() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

    [Test]
    public void MainMenuNewGameEntersPlayableRoster()
    {
        Assert.That(app.CurrentScreen, Is.EqualTo(ScreenId.MainMenu));
        app.Navigate(ScreenId.NewGameSetup); app.StartNew("latium", "standard");
        Assert.Multiple(() => { Assert.That(app.HasActiveCampaign, Is.True); Assert.That(app.CurrentScreen, Is.EqualTo(ScreenId.HouseholdRoster)); Assert.That(app.Roster().Rows, Is.Not.Empty); });
    }

    [Test]
    public void RosterCharacterBackPreservesNavigation()
    {
        app.StartNew("latium", "standard"); string id = app.Roster().Rows[0].CharacterId;
        app.OpenCharacter(id); Assert.That(app.Character().CharacterId, Is.EqualTo(id)); app.Back();
        Assert.That(app.CurrentScreen, Is.EqualTo(ScreenId.HouseholdRoster));
    }

    [Test]
    public void EstateNavigationProducesProjectionContent()
    {
        app.StartNew("latium", "standard"); app.Navigate(ScreenId.EstateSettlement);
        Assert.Multiple(() => { Assert.That(app.CurrentScreen, Is.EqualTo(ScreenId.EstateSettlement)); Assert.That(app.Estate().SettlementStage, Is.Not.Empty); });
    }

    [Test]
    public void MonthAdvanceRefreshesDateReportAndHashDeterministically()
    {
        app.StartNew("latium", "standard", 77); string beforeDate = app.InkBar().Date; ulong before = app.StateHash!.Value;
        app.AdvanceMonth(); ulong after = app.StateHash!.Value;
        var second = new DesktopApplicationController(new DesktopApplicationPaths(Path.Combine(directory, "second"))); second.StartNew("latium", "standard", 77); second.AdvanceMonth();
        Assert.Multiple(() => { Assert.That(app.InkBar().Date, Is.Not.EqualTo(beforeDate)); Assert.That(after, Is.Not.EqualTo(before)); Assert.That(app.CurrentScreen, Is.EqualTo(ScreenId.MonthlyReport)); Assert.That(second.StateHash, Is.EqualTo(after)); });
    }

    [Test]
    public void WaxSealCancelPreservesHashAndConfirmSubmitsCommand()
    {
        app.StartNew("latium", "standard"); ulong before = app.StateHash!.Value; app.RequestAction(CampaignHouseholdAction.FundFestival);
        Assert.That(app.Modal?.Kind, Is.EqualTo(ModalKind.WaxSeal)); app.CancelModal();
        Assert.That(app.StateHash, Is.EqualTo(before));
        app.RequestAction(CampaignHouseholdAction.FundFestival); app.ConfirmModal();
        Assert.That(app.StateHash, Is.Not.EqualTo(before));
    }

    [Test]
    public void SaveLoadRestoresExactHashAndContinuation()
    {
        app.StartNew("latium", "standard", 19); app.AdvanceMonth(); ulong saved = app.StateHash!.Value; app.Save(); app.ConfirmModal(); app.RequestMainMenu(); app.ConfirmModal(); app.Load();
        Assert.That(app.StateHash, Is.EqualTo(saved)); app.AdvanceMonth(); ulong continued = app.StateHash!.Value;
        var expected = new DesktopApplicationController(new DesktopApplicationPaths(Path.Combine(directory, "expected"))); expected.StartNew("latium", "standard", 19); expected.AdvanceMonth(); expected.AdvanceMonth();
        Assert.That(continued, Is.EqualTo(expected.StateHash));
    }

    [Test]
    public void RepeatedCampaignLifecycleDropsStalePresentation()
    {
        app.StartNew("latium", "standard", 1); string first = app.Roster().Rows[0].CharacterId; app.RequestMainMenu(); app.ConfirmModal();
        Assert.That(app.HasActiveCampaign, Is.False); app.StartNew("campania", "hard", 2);
        Assert.Multiple(() => { Assert.That(app.HasActiveCampaign, Is.True); Assert.That(app.Presentation, Is.Not.Null); Assert.That(app.SelectedCharacterId, Is.Null); Assert.That(app.Roster().Rows[0].CharacterId, Is.EqualTo(first)); });
    }

    [Test]
    public void SettingsAreVersionedAndPersisted()
    {
        Assert.That(app.Settings.Art.AiGenerationEnabled, Is.False);
        app.SetUiScale(1.75f); app.SetReducedMotion(true); app.SetConsoleEnabled(true); app.SetAiArtEnabled(true);
        var loaded = new DesktopApplicationController(new DesktopApplicationPaths(directory));
        Assert.Multiple(() => { Assert.That(loaded.Settings.Version, Is.EqualTo(DesktopSettings.CurrentVersion)); Assert.That(loaded.Settings.Display.UiScale, Is.EqualTo(1.75f)); Assert.That(loaded.Settings.Accessibility.ReducedMotion, Is.True); Assert.That(loaded.Settings.Developer.ConsoleEnabled, Is.True); Assert.That(loaded.Settings.Art.AiGenerationEnabled, Is.True); Assert.That(loaded.Settings.Art.Provider, Is.EqualTo("mock")); });
    }

    [Test]
    public void VersionOneSettingsMigrateAndCorruptSettingsArePreserved()
    {
        string settingsDirectory = Path.Combine(directory, "settings"); Directory.CreateDirectory(settingsDirectory);
        File.WriteAllText(Path.Combine(settingsDirectory, "settings.json"), "{\"version\":1,\"display\":{\"uiScale\":1.5},\"accessibility\":{\"reducedMotion\":true}}");
        var migrated = new DesktopApplicationController(new DesktopApplicationPaths(directory));
        Assert.Multiple(() => { Assert.That(migrated.Settings.Version, Is.EqualTo(2)); Assert.That(migrated.Settings.Accessibility.Motion, Is.EqualTo(MotionMode.Reduced)); });
        File.WriteAllText(Path.Combine(settingsDirectory, "settings.json"), "{broken");
        var recovered = new DesktopApplicationController(new DesktopApplicationPaths(directory));
        Assert.Multiple(() => { Assert.That(recovered.Settings, Is.EqualTo(new DesktopSettings())); Assert.That(Directory.GetFiles(settingsDirectory, "*.corrupt-*").Length, Is.EqualTo(1)); });
    }

    [Test]
    public void UnicodeApplicationPathsRoundTripAndTraversalIsRejected()
    {
        string unicodeRoot = Path.Combine(directory, "用户-δοκιμή"); var paths = new DesktopApplicationPaths(unicodeRoot); paths.EnsureRequiredDirectories();
        var service = new SettingsService(paths); service.Update(new DesktopSettings { Display = new() { UiScale = 1.25f } }, "Display");
        Assert.Multiple(() => { Assert.That(new SettingsService(paths).Current.Display.UiScale, Is.EqualTo(1.25f)); Assert.That(paths.ResolveSafePath(paths.Screenshots, "capture.png"), Does.StartWith(paths.Screenshots)); Assert.Throws<InvalidOperationException>(() => paths.ResolveSafePath(paths.Cache, "../escape")); });
    }

    [Test]
    public void UnicodeApplicationPathsSaveLoadAndLogRoundTrip()
    {
        string unicodeRoot = Path.Combine(directory, "用户-δοκιμή"); var paths = new DesktopApplicationPaths(unicodeRoot); paths.EnsureRequiredDirectories();
        var native = new DesktopApplicationController(paths); native.StartNew("latium", "standard");
        ulong hashBeforeSave = native.ExecuteConsoleCommand("hash") is { } h ? Convert.ToUInt64(h, 16) : 0;
        native.Save();
        var reloaded = new DesktopApplicationController(paths); reloaded.Load();
        Assert.Multiple(() =>
        {
            Assert.That(reloaded.HasActiveCampaign, Is.True);
            Assert.That(Convert.ToUInt64(reloaded.ExecuteConsoleCommand("hash"), 16), Is.EqualTo(hashBeforeSave));
            Assert.That(File.Exists(paths.Quicksave), Is.True);
        });

        using var logger = new Gens.Client.Desktop.Diagnostics.StructuredFileLogger(paths);
        logger.Log(Gens.Client.Desktop.Diagnostics.AppLogCategory.Client, Gens.Runtime.RuntimeLogLevel.Information, "unicode root logging round trip");
        Assert.That(File.Exists(Path.Combine(paths.Logs, "gens.log")), Is.True);

        string? report = new Gens.Client.Desktop.Diagnostics.CrashReporter(paths, logger).Capture(new InvalidOperationException("test"), "roster", "SDL3", "SkiaSharp", "null", "software");
        Assert.That(report, Is.Not.Null.And.Matches(@"^.*crash-.*\.json$"));
        Assert.That(File.Exists(report), Is.True);
    }

    [Test]
    public void SaveAsCreatesAnIndependentlyLoadableSlotDistinctFromQuicksave()
    {
        app.StartNew("latium", "standard", 31); app.AdvanceMonth(); ulong saved = app.StateHash!.Value;
        app.SaveAs("The Aemilii"); app.ConfirmModal();
        Gens.Client.Desktop.Saves.SaveSlotMetadata namedSlot = app.SaveSlots.Single(s => !s.IsQuicksave);
        Assert.Multiple(() => { Assert.That(namedSlot.DisplayName, Is.EqualTo("The Aemilii")); Assert.That(namedSlot.FileName, Is.Not.EqualTo("quicksave.gens")); });

        app.RequestMainMenu(); app.ConfirmModal(); app.LoadSlot(namedSlot.SlotId);
        Assert.Multiple(() => { Assert.That(app.HasActiveCampaign, Is.True); Assert.That(app.StateHash, Is.EqualTo(saved)); });
    }

    [Test]
    public void DeleteSlotRemovesFileAndIndexEntryButRefusesOnQuicksave()
    {
        app.StartNew("latium", "standard", 32); app.SaveAs("Temporary"); app.ConfirmModal();
        Gens.Client.Desktop.Saves.SaveSlotMetadata namedSlot = app.SaveSlots.Single(s => !s.IsQuicksave);
        app.DeleteSlot(namedSlot.SlotId);
        Assert.That(app.SaveSlots.Any(s => s.SlotId == namedSlot.SlotId), Is.False);

        app.Save(); app.ConfirmModal();
        Gens.Client.Desktop.Saves.SaveSlotMetadata quicksave = app.SaveSlots.Single(s => s.IsQuicksave);
        app.DeleteSlot(quicksave.SlotId);
        Assert.That(app.SaveSlots.Any(s => s.IsQuicksave), Is.True);
    }

    [Test]
    public void LoadingAChecksumCorruptedSaveSurfacesRecoveryModalInsteadOfCrashing()
    {
        app.StartNew("latium", "standard", 33); app.SaveAs("Corruptible"); app.ConfirmModal();
        Gens.Client.Desktop.Saves.SaveSlotMetadata namedSlot = app.SaveSlots.Single(s => !s.IsQuicksave);
        string path = Path.Combine(directory, "saves", namedSlot.FileName);
        using (var archive = System.IO.Compression.ZipFile.Open(path, System.IO.Compression.ZipArchiveMode.Update))
        {
            var entry = archive.GetEntry(Gens.Simulation.Saves.SaveFormat.WorldEntry)!;
            using var stream = entry.Open(); stream.SetLength(0);
            byte[] corrupted = System.Text.Encoding.UTF8.GetBytes("{\"corrupted\":true}");
            stream.Write(corrupted, 0, corrupted.Length);
        }

        app.LoadSlot(namedSlot.SlotId);
        Assert.That(app.Modal?.Kind, Is.EqualTo(ModalKind.SaveRecovery));
    }

    [Test]
    public void PlaytimeAccumulatesAcrossSavesAndAttributesOnlyUnaccountedTime()
    {
        app.StartNew("latium", "standard", 34);
        app.AccumulatePlaytime(TimeSpan.FromMinutes(2)); app.SaveAs("First"); app.ConfirmModal();
        Gens.Client.Desktop.Saves.SaveSlotMetadata first = app.SaveSlots.Single(s => !s.IsQuicksave);
        Assert.That(first.PlaytimeSeconds, Is.EqualTo(120));

        app.AccumulatePlaytime(TimeSpan.FromMinutes(3)); app.Save(); app.ConfirmModal();
        Gens.Client.Desktop.Saves.SaveSlotMetadata quicksave = app.SaveSlots.Single(s => s.IsQuicksave);
        Assert.That(quicksave.PlaytimeSeconds, Is.EqualTo(180));
    }

    [Test]
    public void DeveloperConsoleUsesSessionDiagnosticsAndCommands()
    {
        app.StartNew("latium", "standard"); string hash = app.ExecuteConsoleCommand("hash"); string replay = app.ExecuteConsoleCommand("replay"); string query = app.ExecuteConsoleCommand("query roster"); string submit = app.ExecuteConsoleCommand("submit rites");
        Assert.Multiple(() => { Assert.That(hash, Has.Length.EqualTo(16)); Assert.That(replay, Is.EqualTo($"Replay verified: {hash}.")); Assert.That(query, Does.Contain("char_")); Assert.That(submit, Is.EqualTo("Accepted.")); });
    }
}
