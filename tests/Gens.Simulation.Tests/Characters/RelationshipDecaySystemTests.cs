using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Random;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class RelationshipDecaySystemTests
{
    [Test]
    public void PositiveOpinionDecaysOnePointTowardZeroPerMonth()
    {
        var state = new WorldState(new GameDate(10));
        var (characterId, targetId) = AddCharacterPair(state);
        var key = new RelationshipKey(characterId, targetId);
        state.Relationships.Add(key, new Relationship(10, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));

        new RelationshipDecaySystem().Tick(state, Context(new GameDate(10)));

        state.Relationships.TryGet(key, out var relationship);
        Assert.That(relationship.Opinion, Is.EqualTo(9));
    }

    [Test]
    public void NegativeOpinionDecaysOnePointTowardZeroPerMonth()
    {
        var state = new WorldState(new GameDate(10));
        var (characterId, targetId) = AddCharacterPair(state);
        var key = new RelationshipKey(characterId, targetId);
        state.Relationships.Add(key, new Relationship(-10, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));

        new RelationshipDecaySystem().Tick(state, Context(new GameDate(10)));

        state.Relationships.TryGet(key, out var relationship);
        Assert.That(relationship.Opinion, Is.EqualTo(-9));
    }

    [Test]
    public void NoDecayOccursInTheSameMonthAsTheLastMeaningfulInteraction()
    {
        var state = new WorldState(new GameDate(10));
        var (characterId, targetId) = AddCharacterPair(state);
        var key = new RelationshipKey(characterId, targetId);
        state.Relationships.Add(key, new Relationship(10, BondTag.None, RelationshipOrigin.Encounter, new GameDate(10), new GameDate(10), null));

        new RelationshipDecaySystem().Tick(state, Context(new GameDate(10)));

        state.Relationships.TryGet(key, out var relationship);
        Assert.That(relationship.Opinion, Is.EqualTo(10));
    }

    [Test]
    public void StickyFamilyBondsAreExemptFromOpinionDecay()
    {
        var state = new WorldState(new GameDate(10));
        var (characterId, targetId) = AddCharacterPair(state);
        var key = new RelationshipKey(characterId, targetId);
        state.Relationships.Add(key, new Relationship(10, BondTag.Parent, RelationshipOrigin.Family, new GameDate(0), new GameDate(0), null));

        new RelationshipDecaySystem().Tick(state, Context(new GameDate(10)));

        state.Relationships.TryGet(key, out var relationship);
        Assert.That(relationship.Opinion, Is.EqualTo(10));
    }

    [Test]
    public void ANeutralBondlessRelationshipIsPrunedRatherThanKept()
    {
        var state = new WorldState(new GameDate(10));
        var (characterId, targetId) = AddCharacterPair(state);
        var key = new RelationshipKey(characterId, targetId);
        state.Relationships.Add(key, new Relationship(0, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));

        new RelationshipDecaySystem().Tick(state, Context(new GameDate(10)));

        Assert.That(state.Relationships.TryGet(key, out _), Is.False);
    }

    [Test]
    public void ANeutralRelationshipWithAStandingBondIsNotPruned()
    {
        var state = new WorldState(new GameDate(10));
        var (characterId, targetId) = AddCharacterPair(state);
        var key = new RelationshipKey(characterId, targetId);
        state.Relationships.Add(key, new Relationship(0, BondTag.Rival, RelationshipOrigin.Political, new GameDate(0), new GameDate(0), null));

        new RelationshipDecaySystem().Tick(state, Context(new GameDate(10)));

        Assert.That(state.Relationships.TryGet(key, out _), Is.True);
    }

    [Test]
    public void DecayingToExactlyZeroWithNoBondsPrunesTheRecordTheSameTick()
    {
        var state = new WorldState(new GameDate(10));
        var (characterId, targetId) = AddCharacterPair(state);
        var key = new RelationshipKey(characterId, targetId);
        state.Relationships.Add(key, new Relationship(1, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));

        new RelationshipDecaySystem().Tick(state, Context(new GameDate(10)));

        Assert.That(state.Relationships.TryGet(key, out _), Is.False);
    }

    [Test]
    public void TickEmitsNoEvents()
    {
        var state = new WorldState(new GameDate(10));
        var (characterId, targetId) = AddCharacterPair(state);
        state.Relationships.Add(
            new RelationshipKey(characterId, targetId),
            new Relationship(10, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));

        var events = new RelationshipDecaySystem().Tick(state, Context(new GameDate(10)));

        Assert.That(events, Is.Empty);
    }

    private static (RuntimeId<Character>, RuntimeId<Character>) AddCharacterPair(WorldState state)
    {
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));
        return (characterId, targetId);
    }

    private static MonthlyTickContext Context(GameDate date) => new(date, new RandomStreamSet());
}
