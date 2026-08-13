using FsCheck;
using FsCheck.Fluent;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Tests.Characters;

/// <summary>Property-based coverage of the "sparse directed records" model (Phase 5 item 5): <see
/// cref="RelationshipKey"/>'s forward and reverse forms are never equal for distinct Characters, its
/// ordering is the plain lexicographic order over (From, To), and the two directions of a pair can
/// independently hold different <see cref="Relationship"/> values without that asymmetry itself being
/// treated as a defect.</summary>
public sealed class RelationshipAsymmetryPropertyTests
{
    private static RuntimeId<Character> MakeId(int offset)
    {
        var counter = new RuntimeIdCounter<Character>();
        var id = counter.Issue();
        for (var i = 0; i < offset; i++)
            id = counter.Issue();
        return id;
    }

    private static int Normalize(int raw) => ((raw % 201) + 201) % 201 - 100;

    [FsCheck.NUnit.Property]
    public FsCheck.Property TheReverseKeyOfADistinctPairIsNeverEqualToTheForwardKey(NonNegativeInt rawA, NonNegativeInt rawB)
    {
        var a = MakeId(rawA.Get % 200);
        var b = MakeId(rawB.Get % 200);
        if (a == b)
            return true.ToProperty();

        var forward = new RelationshipKey(a, b);
        var backward = new RelationshipKey(b, a);

        return (!forward.Equals(backward)).ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property ComparisonAgreesWithLexicographicOrderOnTheUnderlyingIds(
        NonNegativeInt rawA1, NonNegativeInt rawB1, NonNegativeInt rawA2, NonNegativeInt rawB2)
    {
        var a1 = MakeId(rawA1.Get % 100);
        var b1 = MakeId(rawB1.Get % 100);
        var a2 = MakeId(rawA2.Get % 100);
        var b2 = MakeId(rawB2.Get % 100);

        var left = new RelationshipKey(a1, b1);
        var right = new RelationshipKey(a2, b2);

        var expectedSign = a1.Value != a2.Value
            ? Math.Sign(a1.Value.CompareTo(a2.Value))
            : Math.Sign(b1.Value.CompareTo(b2.Value));
        var actualSign = Math.Sign(left.CompareTo(right));

        return (actualSign == expectedSign).ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property BothDirectionsOfAPairHoldIndependentOpinionsWithoutTrippingReferentialIntegrity(
        int rawForwardOpinion, int rawBackwardOpinion)
    {
        var forwardOpinion = Normalize(rawForwardOpinion);
        var backwardOpinion = Normalize(rawBackwardOpinion);

        var state = new WorldState(new GameDate(0));
        var a = state.CharacterIds.Issue();
        var b = state.CharacterIds.Issue();
        state.Characters.Add(a, CharacterTestFixtures.Minimal(a));
        state.Characters.Add(b, CharacterTestFixtures.Minimal(b));

        state.Relationships.Add(
            new RelationshipKey(a, b),
            new Relationship(forwardOpinion, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));
        state.Relationships.Add(
            new RelationshipKey(b, a),
            new Relationship(backwardOpinion, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null));

        state.Relationships.TryGet(new RelationshipKey(a, b), out var forward);
        state.Relationships.TryGet(new RelationshipKey(b, a), out var backward);
        var violations = new RelationshipReferentialIntegrityCheck().Check(state).ToArray();

        return (forward.Opinion == forwardOpinion &&
                backward.Opinion == backwardOpinion &&
                violations.Length == 0).ToProperty();
    }
}
