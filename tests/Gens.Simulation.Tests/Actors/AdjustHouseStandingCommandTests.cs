using Gens.Simulation.Actors;
using Gens.Simulation.Commands;
using Gens.Simulation.Economy;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 5/6 coverage for <see cref="AdjustHouseStandingCommand"/>, mirroring <see
/// cref="Policies.ChangeRitesBudgetCommandTests"/>'s accept/reject shape.</summary>
public sealed class AdjustHouseStandingCommandTests
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

    private static AdjustHouseStandingCommand MakeCommand(
        WorldState state, LivingWorldActor initiator, LivingWorldActor target, HouseStandingAdjustmentDirection direction) =>
        new(state.CommandIds.Issue(), initiator.ActorId.ToTaggedString(), state.Date, null, initiator.ActorId, target.ActorId, direction);

    [Test]
    public void TowardRivalryStepsNeutralToRivalrous()
    {
        var (state, a, b) = TwoActors();

        var result = AdjustHouseStandingCommands.Pipeline.Execute(
            state, MakeCommand(state, a, b, HouseStandingAdjustmentDirection.TowardRivalry));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(HouseStandingResolver.GetEffectiveStanding(state, a.ActorId, b.ActorId), Is.EqualTo(HouseStandingLevel.Rivalrous));
        });
    }

    [Test]
    public void TowardAllianceStepsNeutralToAllied()
    {
        var (state, a, b) = TwoActors();

        var result = AdjustHouseStandingCommands.Pipeline.Execute(
            state, MakeCommand(state, a, b, HouseStandingAdjustmentDirection.TowardAlliance));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.True);
            Assert.That(HouseStandingResolver.GetEffectiveStanding(state, a.ActorId, b.ActorId), Is.EqualTo(HouseStandingLevel.Allied));
        });
    }

    [Test]
    public void RejectsTheSameActorAsInitiatorAndTarget()
    {
        var (state, a, _) = TwoActors();

        var result = AdjustHouseStandingCommands.Pipeline.Execute(
            state, MakeCommand(state, a, a, HouseStandingAdjustmentDirection.TowardRivalry));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(AdjustHouseStandingCommands.SameActor));
        });
    }

    [Test]
    public void RejectsAlreadyAtTheExtreme()
    {
        var (state, a, b) = TwoActors();
        state.HouseStandings.Add(HouseStandingKey.Between(a.ActorId, b.ActorId), new HouseStanding(HouseStandingLevel.Feuding));

        var result = AdjustHouseStandingCommands.Pipeline.Execute(
            state, MakeCommand(state, a, b, HouseStandingAdjustmentDirection.TowardRivalry));

        Assert.Multiple(() =>
        {
            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Error, Is.EqualTo(AdjustHouseStandingCommands.AlreadyAtExtreme));
        });
    }

    [Test]
    public void PreservesAnExistingAncestralGrudgeAcrossAStandingChange()
    {
        var (state, a, b) = TwoActors();
        var grudge = new AncestralGrudge("engagement_placeholder", new GameDate(1));
        state.HouseStandings.Add(HouseStandingKey.Between(a.ActorId, b.ActorId), new HouseStanding(HouseStandingLevel.Rivalrous, grudge));

        AdjustHouseStandingCommands.Pipeline.Execute(state, MakeCommand(state, a, b, HouseStandingAdjustmentDirection.TowardAlliance));

        Assert.That(state.HouseStandings.TryGet(HouseStandingKey.Between(a.ActorId, b.ActorId), out var stored), Is.True);
        Assert.That(stored!.Grudge, Is.EqualTo(grudge));
    }

    [Test]
    public void AcceptedCommandEmitsAHouseStandingChangedEvent()
    {
        var (state, a, b) = TwoActors();

        var result = AdjustHouseStandingCommands.Pipeline.Execute(
            state, MakeCommand(state, a, b, HouseStandingAdjustmentDirection.TowardRivalry));

        Assert.That(result.Events, Has.Count.EqualTo(1));
        var evt = (HouseStandingChangedEvent)result.Events[0];
        Assert.Multiple(() =>
        {
            Assert.That(evt.PreviousStanding, Is.EqualTo(HouseStandingLevel.Neutral));
            Assert.That(evt.NewStanding, Is.EqualTo(HouseStandingLevel.Rivalrous));
            Assert.That(evt.Visibility, Is.EqualTo(Visibility.Public));
        });
    }
}
