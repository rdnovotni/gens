using Gens.Application.Campaign;
using Gens.Client.Desktop.App;
using Gens.Client.Desktop.Platform;
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
        Assert.Multiple(() => { Assert.That(app.CurrentCampaign, Is.Not.Null); Assert.That(app.CurrentScreen, Is.EqualTo(ScreenId.HouseholdRoster)); Assert.That(app.Roster().Rows, Is.Not.Empty); });
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
        Assert.That(app.CurrentCampaign, Is.Null); app.StartNew("campania", "hard", 2);
        Assert.Multiple(() => { Assert.That(app.CurrentCampaign, Is.Not.Null); Assert.That(app.Presentation, Is.Not.Null); Assert.That(app.SelectedCharacterId, Is.Null); Assert.That(app.Roster().Rows[0].CharacterId, Is.EqualTo(first)); });
    }

    [Test]
    public void SettingsAreVersionedAndPersisted()
    {
        app.SetUiScale(1.75f); app.SetReducedMotion(true); app.SetConsoleEnabled(true);
        var loaded = new DesktopApplicationController(new DesktopApplicationPaths(directory));
        Assert.Multiple(() => { Assert.That(loaded.Settings.Version, Is.EqualTo(1)); Assert.That(loaded.Settings.Display.UiScale, Is.EqualTo(1.75f)); Assert.That(loaded.Settings.Accessibility.ReducedMotion, Is.True); Assert.That(loaded.Settings.Developer.ConsoleEnabled, Is.True); });
    }

    [Test]
    public void DeveloperConsoleUsesSessionDiagnosticsAndCommands()
    {
        app.StartNew("latium", "standard"); string hash = app.ExecuteConsoleCommand("hash"); string replay = app.ExecuteConsoleCommand("replay"); string query = app.ExecuteConsoleCommand("query roster"); string submit = app.ExecuteConsoleCommand("submit rites");
        Assert.Multiple(() => { Assert.That(hash, Has.Length.EqualTo(16)); Assert.That(replay, Is.EqualTo($"Replay verified: {hash}.")); Assert.That(query, Does.Contain("char_")); Assert.That(submit, Is.EqualTo("Accepted.")); });
    }
}
