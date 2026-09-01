using Gens.Simulation.Health;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class SetSanitationInvestmentCommandTests
{
    [Test]
    public void ValidRequestSetsTheTierAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));

        var command = new SetSanitationInvestmentCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, settlementId, SanitationInvestmentTier.Comprehensive);
        var result = SetSanitationInvestmentCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        Assert.That(SanitationQueries.EffectiveTier(state, settlementId), Is.EqualTo(SanitationInvestmentTier.Comprehensive));
        var applied = (SanitationInvestmentChangedEvent)result.Events.Single();
        Assert.That(applied.Tier, Is.EqualTo(SanitationInvestmentTier.Comprehensive));
    }

    [Test]
    public void ChangingTheTierAgainOverwritesTheEarlierChoice()
    {
        var state = new WorldState(new GameDate(10));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        SetSanitationInvestmentCommands.Pipeline.Execute(state, new SetSanitationInvestmentCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null, settlementId, SanitationInvestmentTier.Standard));

        SetSanitationInvestmentCommands.Pipeline.Execute(state, new SetSanitationInvestmentCommand(
            state.CommandIds.Issue(), "player", new GameDate(11), null, settlementId, SanitationInvestmentTier.Minimal));

        Assert.That(SanitationQueries.EffectiveTier(state, settlementId), Is.EqualTo(SanitationInvestmentTier.Minimal));
        Assert.That(state.SettlementSanitationInvestments.Count, Is.EqualTo(1));
    }

    [Test]
    public void ValidationRejectsAMissingSettlement()
    {
        var state = new WorldState(new GameDate(10));
        var command = new SetSanitationInvestmentCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            new RuntimeIdCounter<Settlement>().Issue(), SanitationInvestmentTier.Standard);
        var result = SetSanitationInvestmentCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(SetSanitationInvestmentCommands.SettlementNotFound));
    }

    [Test]
    public void ASettlementWithNoInvestmentDefaultsToMinimal()
    {
        var state = new WorldState(new GameDate(10));
        var settlementId = new RuntimeIdCounter<Settlement>().Issue();
        Assert.That(SanitationQueries.EffectiveTier(state, settlementId), Is.EqualTo(SanitationInvestmentTier.Minimal));
    }
}
