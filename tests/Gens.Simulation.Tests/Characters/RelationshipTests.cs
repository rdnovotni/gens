using Gens.Simulation.Characters;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class RelationshipTests
{
    [Test]
    public void ConstructorRejectsOpinionBelowMinimum()
    {
        Assert.That(
            () => new Relationship(-101, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorRejectsOpinionAboveMaximum()
    {
        Assert.That(
            () => new Relationship(101, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ConstructorAcceptsTheFullOpinionRange()
    {
        Assert.That(
            () => new Relationship(Relationship.MinOpinion, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null),
            Throws.Nothing);
        Assert.That(
            () => new Relationship(Relationship.MaxOpinion, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null),
            Throws.Nothing);
    }

    [Test]
    public void ConstructorRejectsALastInteractionBeforeTheFormedDate()
    {
        Assert.That(
            () => new Relationship(0, BondTag.None, RelationshipOrigin.Encounter, new GameDate(10), new GameDate(9), null),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void HasBondIsTrueOnlyWhenEveryRequestedFlagIsSet()
    {
        var relationship = new Relationship(
            0, BondTag.Friend | BondTag.Patron, RelationshipOrigin.Political, new GameDate(0), new GameDate(0), null);

        Assert.That(relationship.HasBond(BondTag.Friend), Is.True);
        Assert.That(relationship.HasBond(BondTag.Patron), Is.True);
        Assert.That(relationship.HasBond(BondTag.Friend | BondTag.Patron), Is.True);
        Assert.That(relationship.HasBond(BondTag.Rival), Is.False);
        Assert.That(relationship.HasBond(BondTag.Friend | BondTag.Rival), Is.False);
    }

    [Test]
    public void IsEmptyIsTrueOnlyForNeutralOpinionAndNoBonds()
    {
        var neutralNoBonds = new Relationship(0, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null);
        var neutralWithBond = new Relationship(0, BondTag.Rival, RelationshipOrigin.Political, new GameDate(0), new GameDate(0), null);
        var nonNeutralNoBonds = new Relationship(5, BondTag.None, RelationshipOrigin.Encounter, new GameDate(0), new GameDate(0), null);

        Assert.That(neutralNoBonds.IsEmpty, Is.True);
        Assert.That(neutralWithBond.IsEmpty, Is.False);
        Assert.That(nonNeutralNoBonds.IsEmpty, Is.False);
    }
}
