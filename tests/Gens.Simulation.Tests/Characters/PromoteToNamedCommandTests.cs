using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class PromoteToNamedCommandTests
{
    private const string StreamName = "test-promotion";

    private static PromoteToNamedCommand MakeCommand(WorldState state, RuntimeId<Settlement> settlementId, CharacterSource source = CharacterSource.LaborDutyHire) =>
        new(
            state.CommandIds.Issue(), "player", new GameDate(300), null,
            settlementId, PopGroupType.Operarii, source, null,
            LegalStatus.Freedman, null, new DefinitionId<Culture>("roman"),
            NamePoolTestFixtures.Roman, StreamName);

    private static WorldState SeedState(out RuntimeId<Settlement> settlementId, int popGroupSize = 5)
    {
        var state = new WorldState(new GameDate(300));
        settlementId = state.SettlementIds.Issue();
        state.PopGroups.Add(
            new PopGroupKey(settlementId, PopGroupType.Operarii),
            PopGroup.Create(settlementId, PopGroupType.Operarii, popGroupSize));
        return state;
    }

    [Test]
    public void ValidPromotionAddsANamedCharacterDecrementsThePopGroupAndEmitsCharacterPromotedEvent()
    {
        var state = SeedState(out var settlementId);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = PromoteToNamedCommands.CreatePipeline(streams);
        var command = MakeCommand(state, settlementId);

        var result = pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        var promoted = (CharacterPromotedEvent)result.Events.Single();
        Assert.That(state.Characters.TryGet(promoted.CharacterId, out var character), Is.True);
        Assert.That(character.Source, Is.EqualTo(CharacterSource.LaborDutyHire));
        Assert.That(character.BackfilledHistory, Is.True);
        Assert.That(character.Location, Is.EqualTo(settlementId));
        Assert.That(character.InstantiatedAtMonth, Is.EqualTo(300));

        Assert.That(state.PopGroups.TryGet(new PopGroupKey(settlementId, PopGroupType.Operarii), out var group), Is.True);
        Assert.That(group.Size, Is.EqualTo(4));
    }

    [Test]
    public void PromotedCharacterHasAnAdultBirthDateStrictlyBeforeTheSubmittedDate()
    {
        var state = SeedState(out var settlementId);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = PromoteToNamedCommands.CreatePipeline(streams);
        var command = MakeCommand(state, settlementId);

        var result = pipeline.Execute(state, command);

        var promoted = (CharacterPromotedEvent)result.Events.Single();
        state.Characters.TryGet(promoted.CharacterId, out var character);
        var ageYears = character.AgeInYears(new GameDate(300));
        Assert.That(ageYears, Is.GreaterThanOrEqualTo(18));
        Assert.That(ageYears, Is.LessThanOrEqualTo(65));
    }

    [Test]
    public void RepeatedPromotionsFromTheSamePopGroupEventuallyExhaustIt()
    {
        var state = SeedState(out var settlementId, popGroupSize: 2);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = PromoteToNamedCommands.CreatePipeline(streams);

        var first = pipeline.Execute(state, MakeCommand(state, settlementId));
        var second = pipeline.Execute(state, MakeCommand(state, settlementId));
        var third = pipeline.Execute(state, MakeCommand(state, settlementId));

        Assert.That(first.Accepted, Is.True);
        Assert.That(second.Accepted, Is.True);
        Assert.That(third.Accepted, Is.False);
        Assert.That(third.Error, Is.EqualTo(PromoteToNamedCommands.PopGroupEmpty));
        Assert.That(state.Characters.Count, Is.EqualTo(2));
    }

    [Test]
    public void ValidationRejectsAMissingPopGroup()
    {
        var state = new WorldState(new GameDate(300));
        var settlementId = state.SettlementIds.Issue();
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = PromoteToNamedCommands.CreatePipeline(streams);

        var result = pipeline.Execute(state, MakeCommand(state, settlementId));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(PromoteToNamedCommands.PopGroupNotFound));
    }

    [Test]
    public void ValidationRejectsFamiliaAsAPromotionSource()
    {
        var state = SeedState(out var settlementId);
        var streams = new RandomStreamSet();
        streams.Add(StreamName, 7, 1);
        var pipeline = PromoteToNamedCommands.CreatePipeline(streams);

        var result = pipeline.Execute(state, MakeCommand(state, settlementId, CharacterSource.Familia));

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(PromoteToNamedCommands.InvalidSource));
    }

    [Test]
    public void SameSeedAndCommandSequenceProduceAnIdenticalPromotedCharacter()
    {
        WorldState Run()
        {
            var state = SeedState(out var settlementId);
            var streams = new RandomStreamSet();
            streams.Add(StreamName, 7, 1);
            var pipeline = PromoteToNamedCommands.CreatePipeline(streams);
            pipeline.Execute(state, MakeCommand(state, settlementId));
            return state;
        }

        var stateA = Run();
        var stateB = Run();

        Assert.That(StateHasher.Hash(stateA), Is.EqualTo(StateHasher.Hash(stateB)));
    }
}
