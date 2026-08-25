using Gens.Simulation.Actions;
using Gens.Simulation.Actors;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 5/6 coverage for <see cref="RivalHouseActionDefinitions"/>.</summary>
public sealed class RivalHouseActionDefinitionsTests
{
    private static (WorldState State, LivingWorldActor A, LivingWorldActor B) TwoActors()
    {
        var state = new WorldState(new GameDate(0));
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        var a = RivalHouseCreationService.CreateAncientSeed(
            state, "Aemilia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        var b = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        return (state, a, b);
    }

    [Test]
    public void BuildCatalogRegistersBothDirections()
    {
        var catalog = RivalHouseActionDefinitions.BuildCatalog();

        Assert.Multiple(() =>
        {
            Assert.That(catalog.TryGet(RivalHouseActionDefinitions.SeekAlliance, out var seek), Is.True);
            Assert.That(seek!.TargetKind, Is.EqualTo(ActionTargetKind.Actor));
            Assert.That(catalog.TryGet(RivalHouseActionDefinitions.DeclareRivalry, out var declare), Is.True);
            Assert.That(declare!.Confirmation, Is.EqualTo(ActionConfirmationSeverity.WaxSeal));
        });
    }

    [Test]
    public void ToDirectionMapsBothKnownIds()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RivalHouseActionDefinitions.ToDirection(RivalHouseActionDefinitions.SeekAlliance),
                Is.EqualTo(HouseStandingAdjustmentDirection.TowardAlliance));
            Assert.That(RivalHouseActionDefinitions.ToDirection(RivalHouseActionDefinitions.DeclareRivalry),
                Is.EqualTo(HouseStandingAdjustmentDirection.TowardRivalry));
        });
    }

    [Test]
    public void ToDirectionThrowsForAnUnknownId()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RivalHouseActionDefinitions.ToDirection(new DefinitionId<ActionDefinition>("not-registered")));
    }

    [Test]
    public void DeclareRivalryIsIneligibleWhenAlreadyFeuding()
    {
        var (state, a, b) = TwoActors();
        state.HouseStandings.Add(HouseStandingKey.Between(a.ActorId, b.ActorId), new HouseStanding(HouseStandingLevel.Feuding));
        var catalog = RivalHouseActionDefinitions.BuildCatalog();
        var definition = catalog.Get(RivalHouseActionDefinitions.DeclareRivalry);
        var invocation = new ActionInvocation(a.ActorId.ToTaggedString(), b.ActorId.ToTaggedString(), new GameDate(0));

        Assert.That(definition.Eligibility(state, invocation), Is.EqualTo(AdjustHouseStandingCommands.AlreadyAtExtreme));
    }

    [Test]
    public void SeekAllianceIsEligibleForAFreshNeutralPair()
    {
        var (state, a, b) = TwoActors();
        var catalog = RivalHouseActionDefinitions.BuildCatalog();
        var definition = catalog.Get(RivalHouseActionDefinitions.SeekAlliance);
        var invocation = new ActionInvocation(a.ActorId.ToTaggedString(), b.ActorId.ToTaggedString(), new GameDate(0));

        Assert.That(definition.Eligibility(state, invocation), Is.Null);
    }
}
