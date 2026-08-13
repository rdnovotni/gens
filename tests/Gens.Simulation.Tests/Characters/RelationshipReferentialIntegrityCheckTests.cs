using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class RelationshipReferentialIntegrityCheckTests
{
    [Test]
    public void NoViolationsWhenEveryRelationshipReferencesRegisteredDistinctCharacters()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));
        state.Relationships.Add(
            new RelationshipKey(characterId, targetId),
            new Relationship(10, BondTag.Friend, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));

        var violations = new RelationshipReferentialIntegrityCheck().Check(state).ToArray();

        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void AnAsymmetricOneDirectionalRelationshipIsNotAViolation()
    {
        // A holds a Relationship toward B; B holds nothing toward A at all. Directed asymmetry is the
        // documented model (Phase 5 item 5), not a defect this check should ever flag.
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));
        state.Relationships.Add(
            new RelationshipKey(characterId, targetId),
            new Relationship(-40, BondTag.Rival, RelationshipOrigin.Political, new GameDate(0), new GameDate(0), null));

        var violations = new RelationshipReferentialIntegrityCheck().Check(state).ToArray();

        Assert.That(violations, Is.Empty);
    }

    [Test]
    public void FlagsARelationshipReferencingAMissingSourceCharacter()
    {
        var state = new WorldState(new GameDate(10));
        var missingSourceId = state.CharacterIds.Issue();
        var targetId = state.CharacterIds.Issue();
        state.Characters.Add(targetId, CharacterTestFixtures.Minimal(targetId));
        state.Relationships.Add(
            new RelationshipKey(missingSourceId, targetId),
            new Relationship(10, BondTag.Friend, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));

        var violations = new RelationshipReferentialIntegrityCheck().Check(state).ToArray();

        Assert.That(violations, Has.Length.EqualTo(1));
        Assert.That(violations[0].InvariantId, Is.EqualTo("characters.relationships.referentialIntegrity"));
    }

    [Test]
    public void FlagsARelationshipReferencingAMissingTargetCharacter()
    {
        var state = new WorldState(new GameDate(10));
        var characterId = state.CharacterIds.Issue();
        var missingTargetId = state.CharacterIds.Issue();
        state.Characters.Add(characterId, CharacterTestFixtures.Minimal(characterId));
        state.Relationships.Add(
            new RelationshipKey(characterId, missingTargetId),
            new Relationship(10, BondTag.Friend, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));

        var violations = new RelationshipReferentialIntegrityCheck().Check(state).ToArray();

        Assert.That(violations, Has.Length.EqualTo(1));
    }
}
