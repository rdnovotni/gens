using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class SetSettlementQuarantineCommandTests
{
    [Test]
    public void DeclaringQuarantineOnAnActiveOutbreakSetsTheFlagAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var outbreakId = state.EpidemicOutbreakIds.Issue();
        state.EpidemicOutbreaks.Add(outbreakId, EpidemicOutbreak.Create(outbreakId, settlementId, DiseaseCatalog.Pestilence, new GameDate(9)));

        var command = new SetSettlementQuarantineCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, outbreakId, true);
        var result = SetSettlementQuarantineCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        state.EpidemicOutbreaks.TryGet(outbreakId, out var updated);
        Assert.That(updated.SettlementQuarantineActive, Is.True);
        var applied = (SettlementQuarantineChangedEvent)result.Events.Single();
        Assert.That(applied.SettlementId, Is.EqualTo(settlementId));
    }

    [Test]
    public void ValidationRejectsAMissingOutbreak()
    {
        var state = new WorldState(new GameDate(10));
        var command = new SetSettlementQuarantineCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            new RuntimeIdCounter<EpidemicOutbreak>().Issue(), true);
        var result = SetSettlementQuarantineCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(SetSettlementQuarantineCommands.OutbreakNotFound));
    }

    [Test]
    public void ValidationRejectsAnEndedOutbreak()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var outbreakId = state.EpidemicOutbreakIds.Issue();
        var ended = EpidemicOutbreak.Create(outbreakId, settlementId, DiseaseCatalog.Pestilence, new GameDate(9))
            with
        { Status = EpidemicOutbreakStatus.Ended, ResolvedDate = new GameDate(10) };
        state.EpidemicOutbreaks.Add(outbreakId, ended);

        var command = new SetSettlementQuarantineCommand(state.CommandIds.Issue(), "player", new GameDate(10), null, outbreakId, true);
        var result = SetSettlementQuarantineCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(SetSettlementQuarantineCommands.OutbreakNotActive));
    }
}
