#nullable enable

using System;
using System.IO;
using System.Linq;
using Gens.Presentation.Shell;
using Gens.Presentation.Tests.Support;
using Gens.Simulation.Campaign;
using Gens.Simulation.Characters;
using Gens.Simulation.Queries;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Presentation.Tests.EditMode;

/// <summary>Covers <see cref="CampaignShell"/> itself (Phase 9 item 5): the two ADR 0013 entry points
/// (<see cref="CampaignShell.Query{TProjection}"/>, <see cref="CampaignShell.Submit{TCommand}"/>),
/// month advance, and save/load/replay-diagnostics — everything <c>GensUIController</c> relies on the
/// shell for.</summary>
public sealed class CampaignShellTests
{
    [Test]
    public void BootstrapIssuesTheFirstHouseholdAndSettlementIds()
    {
        var shell = CampaignTestFixtures.Bootstrap();

        Assert.That(shell.HouseholdId.ToTaggedString(), Is.EqualTo("household_0000000"));
        Assert.That(shell.SettlementId.ToTaggedString(), Is.EqualTo("settlement_0000000"));
    }

    [Test]
    public void BootstrapReturnsInitialHistoryContainingTheBootstrapEvent()
    {
        var config = CampaignTestFixtures.BuildConfig();

        CampaignShell.Bootstrap(config, out var initialHistory);

        Assert.That(initialHistory.Select(e => e.Type), Does.Contain("campaign.bootstrapped"));
    }

    [Test]
    public void QueryExecutesAgainstTheShellsOwnStateForTheGivenObserver()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        CampaignTestFixtures.AddAdultHouseholdMember(shell);

        var projection = shell.Query(new HouseholdRosterQuery(shell.HouseholdId), "player");

        Assert.That(projection.Members, Has.Count.EqualTo(1));
    }

    [Test]
    public void QueryThrowsOnNullQuery()
    {
        var shell = CampaignTestFixtures.Bootstrap();

        Assert.That(
            () => shell.Query<HouseholdRosterProjection>(null!, "player"),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void SubmitRunsTheCommandThroughItsOwnPipelineAndMutatesState()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var characterId = CampaignTestFixtures.AddAdultHouseholdMember(shell);

        var command = new AssignDutyCommand(
            shell.State.CommandIds.Issue(), shell.HouseholdId.ToTaggedString(), shell.State.Date, null,
            characterId, shell.HouseholdId, DutySlot.FieldHand);
        var result = shell.Submit(AssignDutyCommands.Pipeline, command);

        Assert.That(result.Accepted, Is.True);
        var roster = shell.Query(new HouseholdRosterQuery(shell.HouseholdId), "player");
        Assert.That(roster.Members.Single().DutySlot, Is.EqualTo(DutySlot.FieldHand));
    }

    [Test]
    public void SubmitThrowsOnNullPipeline()
    {
        var shell = CampaignTestFixtures.Bootstrap();

        Assert.That(
            () => shell.Submit<AssignDutyCommand>(null!, null!),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void AdvanceMonthTicksTheSuppliedSystemsAndMovesTheCalendarForwardOneMonth()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var before = shell.State.Date;

        shell.AdvanceMonth(new IMonthlySystem<WorldState>[] { new ScheduledActionSystem() });

        Assert.That(shell.State.Date.TotalMonths, Is.EqualTo(before.TotalMonths + 1));
    }

    [Test]
    public void AdvanceMonthThrowsOnNullSystems()
    {
        var shell = CampaignTestFixtures.Bootstrap();

        Assert.That(() => shell.AdvanceMonth(null!), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void SaveThenLoadReproducesTheSameHouseholdAndRoster()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        CampaignTestFixtures.AddAdultHouseholdMember(shell);
        var path = TempSavePath();

        try
        {
            shell.Save(path, "0.0.0-test");
            var loaded = CampaignShell.Load(path, out var manifest);

            Assert.That(manifest.ContentPackHash, Is.EqualTo(shell.ContentPackHash));
            Assert.That(loaded.HouseholdId, Is.EqualTo(shell.HouseholdId));
            var roster = loaded.Query(new HouseholdRosterQuery(loaded.HouseholdId), "player");
            Assert.That(roster.Members, Has.Count.EqualTo(1));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void VerifyDeterministicReplayMatchesForAnUntouchedCampaign()
    {
        var shell = CampaignTestFixtures.Bootstrap();
        var path = TempSavePath();

        try
        {
            var result = shell.VerifyDeterministicReplay(path, "0.0.0-test");

            Assert.That(result.Matches, Is.True);
            Assert.That(result.HashBeforeSave, Is.EqualTo(result.HashAfterReload));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string TempSavePath() =>
        Path.Combine(Path.GetTempPath(), $"gens-shell-test-{Guid.NewGuid():N}.gens");
}
