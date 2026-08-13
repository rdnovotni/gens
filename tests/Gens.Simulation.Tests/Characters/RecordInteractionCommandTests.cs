using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class RecordInteractionCommandTests
{
    [Test]
    public void FirstInteractionCreatesARelationshipAndEmitsAnEvent()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));

        var command = new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            characterId, targetId, OpinionDelta: 15, BondsGranted: BondTag.Friend, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Encounter);
        var result = RecordInteractionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.True);
        var raised = result.Events.Single();
        Assert.That(raised, Is.InstanceOf<RelationshipInteractionRecordedEvent>());
        var recorded = (RelationshipInteractionRecordedEvent)raised;
        Assert.That(recorded.OpinionBefore, Is.EqualTo(0));
        Assert.That(recorded.OpinionAfter, Is.EqualTo(15));

        var key = new RelationshipKey(characterId, targetId);
        Assert.That(state.Relationships.TryGet(key, out var relationship), Is.True);
        Assert.That(relationship.Opinion, Is.EqualTo(15));
        Assert.That(relationship.HasBond(BondTag.Friend), Is.True);
        Assert.That(relationship.Origin, Is.EqualTo(RelationshipOrigin.Encounter));
        Assert.That(relationship.FormedDate, Is.EqualTo(new GameDate(10)));
        Assert.That(relationship.LastMeaningfulInteractionDate, Is.EqualTo(new GameDate(10)));
        Assert.That(relationship.ProvenanceEventId, Is.EqualTo(recorded.EventId.ToTaggedString()));
    }

    [Test]
    public void ThisCommandOnlyAffectsTheOneDirectedRelationship()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));

        var command = new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            characterId, targetId, OpinionDelta: 15, BondsGranted: BondTag.None, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Encounter);
        RecordInteractionCommands.Pipeline.Execute(state, command);

        Assert.That(state.Relationships.TryGet(new RelationshipKey(characterId, targetId), out _), Is.True);
        Assert.That(state.Relationships.TryGet(new RelationshipKey(targetId, characterId), out _), Is.False);
    }

    [Test]
    public void ASecondInteractionAccumulatesOpinionAndPreservesTheOriginalOriginAndFormedDate()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));

        RecordInteractionCommands.Pipeline.Execute(state, new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            characterId, targetId, OpinionDelta: 10, BondsGranted: BondTag.Friend, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Encounter));

        var second = RecordInteractionCommands.Pipeline.Execute(state, new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(14), null,
            characterId, targetId, OpinionDelta: 20, BondsGranted: BondTag.None, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Political));

        Assert.That(second.Accepted, Is.True);
        state.Relationships.TryGet(new RelationshipKey(characterId, targetId), out var relationship);
        Assert.That(relationship.Opinion, Is.EqualTo(30));
        Assert.That(relationship.Origin, Is.EqualTo(RelationshipOrigin.Encounter));
        Assert.That(relationship.FormedDate, Is.EqualTo(new GameDate(10)));
        Assert.That(relationship.LastMeaningfulInteractionDate, Is.EqualTo(new GameDate(14)));
    }

    [Test]
    public void OpinionClampsAtTheMaximum()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));

        RecordInteractionCommands.Pipeline.Execute(state, new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            characterId, targetId, OpinionDelta: 90, BondsGranted: BondTag.None, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Encounter));
        RecordInteractionCommands.Pipeline.Execute(state, new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(11), null,
            characterId, targetId, OpinionDelta: 90, BondsGranted: BondTag.None, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Encounter));

        state.Relationships.TryGet(new RelationshipKey(characterId, targetId), out var relationship);
        Assert.That(relationship.Opinion, Is.EqualTo(Relationship.MaxOpinion));
    }

    [Test]
    public void BondsRevokedRemovesOnlyTheNamedFlags()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));

        RecordInteractionCommands.Pipeline.Execute(state, new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            characterId, targetId, OpinionDelta: 0, BondsGranted: BondTag.Friend | BondTag.Patron, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Political));

        RecordInteractionCommands.Pipeline.Execute(state, new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(11), null,
            characterId, targetId, OpinionDelta: 0, BondsGranted: BondTag.None, BondsRevoked: BondTag.Patron,
            Origin: RelationshipOrigin.Political));

        state.Relationships.TryGet(new RelationshipKey(characterId, targetId), out var relationship);
        Assert.That(relationship.HasBond(BondTag.Friend), Is.True);
        Assert.That(relationship.HasBond(BondTag.Patron), Is.False);
    }

    [Test]
    public void ValidationRejectsInteractingWithOneself()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));

        var command = new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            characterId, characterId, OpinionDelta: 5, BondsGranted: BondTag.None, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Encounter);
        var result = RecordInteractionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(RecordInteractionCommands.SelfInteraction));
    }

    [Test]
    public void ValidationRejectsADeceasedTarget()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(
            targetId, deathRecord: new DeathRecord(new GameDate(5), DeathCause.Disease, 20)));

        var command = new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            characterId, targetId, OpinionDelta: 5, BondsGranted: BondTag.None, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Encounter);
        var result = RecordInteractionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(RecordInteractionCommands.TargetDeceased));
    }

    [Test]
    public void ValidationRejectsAnInteractionWithNothingToRecord()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));

        var command = new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            characterId, targetId, OpinionDelta: 0, BondsGranted: BondTag.None, BondsRevoked: BondTag.None,
            Origin: RelationshipOrigin.Encounter);
        var result = RecordInteractionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(RecordInteractionCommands.NothingToRecord));
    }

    [Test]
    public void ValidationRejectsGrantingAndRevokingTheSameBondAtOnce()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));

        var command = new RecordInteractionCommand(
            state.CommandIds.Issue(), "player", new GameDate(10), null,
            characterId, targetId, OpinionDelta: 0, BondsGranted: BondTag.Friend, BondsRevoked: BondTag.Friend,
            Origin: RelationshipOrigin.Encounter);
        var result = RecordInteractionCommands.Pipeline.Execute(state, command);

        Assert.That(result.Accepted, Is.False);
        Assert.That(result.Error, Is.EqualTo(RecordInteractionCommands.ConflictingBondChange));
    }
}
