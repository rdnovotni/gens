using FsCheck;
using FsCheck.Fluent;
using Gens.Simulation.Characters;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Tests.Characters;

/// <summary>Property-based coverage of <see cref="RelationshipReferentialIntegrityCheck"/> across the
/// full range of <see cref="Relationship"/> content: a relationship between two registered, distinct
/// Characters is never flagged regardless of opinion or bonds, while a self-directed relationship or
/// one naming an unregistered Character is always flagged, exactly once, regardless of its content.</summary>
public sealed class RelationshipReferentialIntegrityPropertyTests
{
    private static int NormalizeOpinion(int raw) => ((raw % 201) + 201) % 201 - 100;

    private static Relationship MakeRelationship(int opinion, int rawBonds) =>
        new(opinion, (BondTag)(rawBonds & 0x1FFFF), RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null);

    [FsCheck.NUnit.Property]
    public FsCheck.Property ARelationshipBetweenTwoRegisteredDistinctCharactersIsNeverFlagged(int rawOpinion, int rawBonds)
    {
        var relationship = MakeRelationship(NormalizeOpinion(rawOpinion), rawBonds);

        var state = new WorldState(new GameDate(0));
        var a = state.CharacterIds.Issue();
        var b = state.CharacterIds.Issue();
        state.Characters.Add(a, CharacterTestFixtures.Minimal(a));
        state.Characters.Add(b, CharacterTestFixtures.Minimal(b));
        state.Relationships.Add(new RelationshipKey(a, b), relationship);

        var violations = new RelationshipReferentialIntegrityCheck().Check(state).ToArray();

        return (violations.Length == 0).ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property ASelfDirectedRelationshipIsAlwaysFlaggedExactlyOnce(int rawOpinion, int rawBonds)
    {
        var relationship = MakeRelationship(NormalizeOpinion(rawOpinion), rawBonds);

        var state = new WorldState(new GameDate(0));
        var a = state.CharacterIds.Issue();
        state.Characters.Add(a, CharacterTestFixtures.Minimal(a));
        state.Relationships.Add(new RelationshipKey(a, a), relationship);

        var violations = new RelationshipReferentialIntegrityCheck().Check(state).ToArray();

        return (violations.Length == 1).ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property ARelationshipFromAnUnregisteredSourceIsAlwaysFlaggedExactlyOnce(int rawOpinion, int rawBonds)
    {
        var relationship = MakeRelationship(NormalizeOpinion(rawOpinion), rawBonds);

        var state = new WorldState(new GameDate(0));
        var missingSource = state.CharacterIds.Issue();
        var target = state.CharacterIds.Issue();
        state.Characters.Add(target, CharacterTestFixtures.Minimal(target));
        state.Relationships.Add(new RelationshipKey(missingSource, target), relationship);

        var violations = new RelationshipReferentialIntegrityCheck().Check(state).ToArray();

        return (violations.Length == 1 && violations[0].InvariantId == "characters.relationships.referentialIntegrity").ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property ARelationshipToAnUnregisteredTargetIsAlwaysFlaggedExactlyOnce(int rawOpinion, int rawBonds)
    {
        var relationship = MakeRelationship(NormalizeOpinion(rawOpinion), rawBonds);

        var state = new WorldState(new GameDate(0));
        var source = state.CharacterIds.Issue();
        var missingTarget = state.CharacterIds.Issue();
        state.Characters.Add(source, CharacterTestFixtures.Minimal(source));
        state.Relationships.Add(new RelationshipKey(source, missingTarget), relationship);

        var violations = new RelationshipReferentialIntegrityCheck().Check(state).ToArray();

        return (violations.Length == 1).ToProperty();
    }
}
