using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;
using CharactersNamePoolTestFixtures = Gens.Simulation.Tests.Characters.NamePoolTestFixtures;

namespace Gens.Simulation.Tests.Actors;

/// <summary>Phase 10 item 4 lazy head-Character generation coverage, mirroring <see
/// cref="Characters.PromoteToNamedCommandTests"/>'s generation-shape assertions.</summary>
public sealed class LivingWorldActorHeadGeneratorTests
{
    private static RandomStreamSet Streams(ulong seed)
    {
        var streams = new RandomStreamSet();
        streams.Add("actors.rivalHouseHeadGeneration", seed, 1);
        return streams;
    }

    private static LivingWorldActor CreateBackgroundActor(WorldState state, string nomen = "Valerius") =>
        RivalHouseCreationService.CreateAncientSeed(
            state, nomen, LivingWorldActorStandingTrend.Established, LivingWorldActorIdentity.None,
            dignitas: 10, new LivingWorldActorNetWorth(HouseholdWealthBand.Modest, Figure: null),
            new LivingWorldActorMilitaryStrength(MilitaryStrengthBand.Modest),
            state.RegionIds.Issue(), state.SettlementIds.Issue());

    [Test]
    public void GenerateHeadStampsTheActorAndRegistersTheCharacter()
    {
        var state = new WorldState(new GameDate(0));
        var actor = CreateBackgroundActor(state);

        var (updatedActor, head) = LivingWorldActorHeadGenerator.GenerateHead(
            state, Streams(1), "actors.rivalHouseHeadGeneration", actor.ActorId, new GameDate(0),
            LegalStatus.RomanCitizen, SocialClass.Senatorial, new DefinitionId<Culture>("roman"),
            CharactersNamePoolTestFixtures.Roman);

        Assert.Multiple(() =>
        {
            Assert.That(updatedActor.HeadCharacterId, Is.EqualTo(head.Id));
            Assert.That(state.Actors.TryGet(actor.ActorId, out var stored), Is.True);
            Assert.That(stored!.HeadCharacterId, Is.EqualTo(head.Id));
            Assert.That(state.Characters.TryGet(head.Id, out var storedHead), Is.True);
            Assert.That(storedHead, Is.EqualTo(head));
            var expectedNomen = head.Sex == Sex.Female ? CharacterNameGenerator.Feminize("Valerius") : "Valerius";
            Assert.That(head.Nomen, Is.EqualTo(expectedNomen));
            Assert.That(head.Source, Is.EqualTo(CharacterSource.RivalGenerated));
            Assert.That(head.BackfilledHistory, Is.True);
        });
    }

    [Test]
    public void GenerateHeadThrowsWhenTheActorAlreadyHasAHead()
    {
        var state = new WorldState(new GameDate(0));
        var actor = CreateBackgroundActor(state);
        LivingWorldActorHeadGenerator.GenerateHead(
            state, Streams(1), "actors.rivalHouseHeadGeneration", actor.ActorId, new GameDate(0),
            LegalStatus.RomanCitizen, SocialClass.Senatorial, new DefinitionId<Culture>("roman"),
            CharactersNamePoolTestFixtures.Roman);

        Assert.Throws<InvalidOperationException>(() => LivingWorldActorHeadGenerator.GenerateHead(
            state, Streams(2), "actors.rivalHouseHeadGeneration", actor.ActorId, new GameDate(0),
            LegalStatus.RomanCitizen, SocialClass.Senatorial, new DefinitionId<Culture>("roman"),
            CharactersNamePoolTestFixtures.Roman));
    }

    [Test]
    public void SameSeedProducesTheSameHeadEveryTime()
    {
        var stateA = new WorldState(new GameDate(0));
        var actorA = CreateBackgroundActor(stateA);
        var (_, headA) = LivingWorldActorHeadGenerator.GenerateHead(
            stateA, Streams(7), "actors.rivalHouseHeadGeneration", actorA.ActorId, new GameDate(0),
            LegalStatus.RomanCitizen, SocialClass.Senatorial, new DefinitionId<Culture>("roman"),
            CharactersNamePoolTestFixtures.Roman);

        var stateB = new WorldState(new GameDate(0));
        var actorB = CreateBackgroundActor(stateB);
        var (_, headB) = LivingWorldActorHeadGenerator.GenerateHead(
            stateB, Streams(7), "actors.rivalHouseHeadGeneration", actorB.ActorId, new GameDate(0),
            LegalStatus.RomanCitizen, SocialClass.Senatorial, new DefinitionId<Culture>("roman"),
            CharactersNamePoolTestFixtures.Roman);

        Assert.Multiple(() =>
        {
            Assert.That(headB.Praenomen, Is.EqualTo(headA.Praenomen));
            Assert.That(headB.Sex, Is.EqualTo(headA.Sex));
            Assert.That(headB.Attributes, Is.EqualTo(headA.Attributes));
        });
    }
}
