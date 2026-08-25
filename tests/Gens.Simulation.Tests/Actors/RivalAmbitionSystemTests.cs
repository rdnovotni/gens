using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.Schemes;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharacterTestFixtures = Gens.Simulation.Tests.Characters.CharacterTestFixtures;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 4/1 coverage for the Noteworthy-tier decision loop.</summary>
public sealed class RivalAmbitionSystemTests
{
    private static readonly DefinitionId<Trait> BoldTrait = new("bold-test");

    private static TraitCatalog BuildTraitCatalog() => new(new[]
    {
        new TraitDefinition(BoldTrait, TraitCategory.Congenital, PersonalityAxis.Boldness, 25),
    });

    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("actors.rivalAmbition", seed, 1);
        return streams;
    }

    private static (WorldState State, LivingWorldActor Actor, LivingWorldActor Target) SetUpNoteworthyActorWithATarget(
        int ambition, bool bold)
    {
        var state = new WorldState(new GameDate(0));
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);

        var actor = RivalHouseCreationService.CreateAncientSeed(
            state, "Aemilia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        var target = RivalHouseCreationService.CreateAncientSeed(
            state, "Cornelia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, actor.RegionId, actor.HomeSettlementId);

        // A tracked HouseStanding entry is what makes `target` a candidate at all (package 7's
        // deliberately bounded "only actors already related to" target search).
        state.HouseStandings.Add(HouseStandingKey.Between(actor.ActorId, target.ActorId), new HouseStanding(HouseStandingLevel.Neutral));

        var headId = state.CharacterIds.Issue();
        var head = CharacterTestFixtures.Minimal(
            headId,
            condition: new Condition(80, 0, 50, ambition, 50),
            traits: bold ? new[] { BoldTrait } : Array.Empty<DefinitionId<Trait>>());
        state.Characters.Add(headId, head);

        state.Actors.Remove(actor.ActorId);
        var withHead = actor with { HeadCharacterId = headId, Tier = LivingWorldActorTier.Noteworthy };
        state.Actors.Add(actor.ActorId, withHead);

        return (state, withHead, target);
    }

    [Test]
    public void DeclaresTheRelationshipsActorsPhaseAndPrerequisite()
    {
        var system = new RivalAmbitionSystem(RivalHouseActionDefinitions.BuildCatalog(), SchemeActionDefinitions.BuildCatalog(), BuildTraitCatalog(), "actors.rivalAmbition");

        Assert.Multiple(() =>
        {
            Assert.That(system.Phase, Is.EqualTo(TickPhase.RelationshipsActors));
            Assert.That(system.Prerequisites, Does.Contain("actors.backgroundHouseDrift"));
        });
    }

    [Test]
    public void ANeverContactedHeadWithNoTrackedStandingNeverActs()
    {
        var state = new WorldState(new GameDate(0));
        var netWorth = new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null);
        var military = new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest);
        var actor = RivalHouseCreationService.CreateAncientSeed(
            state, "Aemilia", LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            0, netWorth, military, state.RegionIds.Issue(), state.SettlementIds.Issue());
        var headId = state.CharacterIds.Issue();
        var head = CharacterTestFixtures.Minimal(headId, condition: new Condition(80, 0, 50, 100, 50));
        state.Characters.Add(headId, head);
        state.Actors.Remove(actor.ActorId);
        state.Actors.Add(actor.ActorId, actor with { HeadCharacterId = headId, Tier = LivingWorldActorTier.Noteworthy });

        var system = new RivalAmbitionSystem(RivalHouseActionDefinitions.BuildCatalog(), SchemeActionDefinitions.BuildCatalog(), BuildTraitCatalog(), "actors.rivalAmbition");
        var events = system.Tick(state, new MonthlyTickContext(new GameDate(0), Streams(1)));

        Assert.That(events, Is.Empty);
    }

    [Test]
    public void AMaximallyAmbitiousBoldHeadEventuallyChangesStandingWithItsTarget()
    {
        var (state, actor, target) = SetUpNoteworthyActorWithATarget(ambition: 100, bold: true);
        var system = new RivalAmbitionSystem(RivalHouseActionDefinitions.BuildCatalog(), SchemeActionDefinitions.BuildCatalog(), BuildTraitCatalog(), "actors.rivalAmbition");

        var initialStanding = HouseStandingResolver.GetEffectiveStanding(state, actor.ActorId, target.ActorId);
        var streams = Streams(0);
        var acted = false;
        for (var month = 0; month < 50 && !acted; month++)
        {
            system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            acted = HouseStandingResolver.GetEffectiveStanding(state, actor.ActorId, target.ActorId) != initialStanding;
        }

        Assert.That(acted, Is.True);
    }

    [Test]
    public void AFeudingPairWithBothStandingMovesBlockedInitiatesASchemeInstead()
    {
        var (state, actor, target) = SetUpNoteworthyActorWithATarget(ambition: 100, bold: true);

        // Give the target a head Character too, so it's a valid scheme target, and force both
        // House Standing directions to be ineligible: DeclareRivalry via AlreadyAtExtreme (already
        // Feuding), SeekAlliance via an active Ancestral Grudge.
        var targetHeadId = state.CharacterIds.Issue();
        state.Characters.Add(targetHeadId, CharacterTestFixtures.Minimal(targetHeadId, praenomen: "Quintus"));
        state.Actors.Remove(target.ActorId);
        state.Actors.Add(target.ActorId, target with { HeadCharacterId = targetHeadId, Tier = LivingWorldActorTier.Noteworthy });

        var key = HouseStandingKey.Between(actor.ActorId, target.ActorId);
        state.HouseStandings.Remove(key);
        state.HouseStandings.Add(key, new HouseStanding(HouseStandingLevel.Feuding, new AncestralGrudge("engagement_placeholder", new GameDate(0))));

        var system = new RivalAmbitionSystem(RivalHouseActionDefinitions.BuildCatalog(), SchemeActionDefinitions.BuildCatalog(), BuildTraitCatalog(), "actors.rivalAmbition");
        var streams = Streams(0);
        var initiated = false;
        for (var month = 0; month < 50 && !initiated; month++)
        {
            var events = system.Tick(state, new MonthlyTickContext(new GameDate(month), streams));
            initiated = events.Any(e => e is SchemeInitiatedEvent);
        }

        Assert.That(initiated, Is.True);
    }

    [Test]
    public void SameSeedProducesTheSameOutcomeEveryTime()
    {
        var (stateA, actorA, targetA) = SetUpNoteworthyActorWithATarget(ambition: 100, bold: true);
        var systemA = new RivalAmbitionSystem(RivalHouseActionDefinitions.BuildCatalog(), SchemeActionDefinitions.BuildCatalog(), BuildTraitCatalog(), "actors.rivalAmbition");
        systemA.Tick(stateA, new MonthlyTickContext(new GameDate(0), Streams(5)));
        var standingA = HouseStandingResolver.GetEffectiveStanding(stateA, actorA.ActorId, targetA.ActorId);

        var (stateB, actorB, targetB) = SetUpNoteworthyActorWithATarget(ambition: 100, bold: true);
        var systemB = new RivalAmbitionSystem(RivalHouseActionDefinitions.BuildCatalog(), SchemeActionDefinitions.BuildCatalog(), BuildTraitCatalog(), "actors.rivalAmbition");
        systemB.Tick(stateB, new MonthlyTickContext(new GameDate(0), Streams(5)));
        var standingB = HouseStandingResolver.GetEffectiveStanding(stateB, actorB.ActorId, targetB.ActorId);

        Assert.That(standingB, Is.EqualTo(standingA));
    }
}
