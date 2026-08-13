using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class TraitDefinitionTests
{
    private static readonly DefinitionId<Trait> Id = new("some-trait");
    private static readonly DefinitionId<Trait> Other = new("other-trait");

    [Test]
    public void AllowsATraitWithNoAxisNudgeNoOpposesAndNoSpectrum()
    {
        var definition = new TraitDefinition(Id, TraitCategory.Congenital);

        Assert.That(definition.Axis, Is.Null);
        Assert.That(definition.Opposes, Is.Null);
        Assert.That(definition.Spectrum, Is.Null);
    }

    [Test]
    public void RejectsAnAxisWithoutAnAxisNudge() =>
        Assert.That(
            () => new TraitDefinition(Id, TraitCategory.Congenital, axis: PersonalityAxis.Boldness),
            Throws.ArgumentException);

    [Test]
    public void RejectsAnAxisNudgeWithoutAnAxis() =>
        Assert.That(
            () => new TraitDefinition(Id, TraitCategory.Congenital, axisNudge: 10),
            Throws.ArgumentException);

    [Test]
    public void RejectsAZeroAxisNudge() =>
        Assert.That(
            () => new TraitDefinition(Id, TraitCategory.Congenital, axis: PersonalityAxis.Boldness, axisNudge: 0),
            Throws.TypeOf<ArgumentOutOfRangeException>());

    [Test]
    public void RejectsAnAxisNudgeOutOfRange() =>
        Assert.That(
            () => new TraitDefinition(Id, TraitCategory.Congenital, axis: PersonalityAxis.Boldness, axisNudge: 50),
            Throws.TypeOf<ArgumentOutOfRangeException>());

    [Test]
    public void RejectsSelfOpposition() =>
        Assert.That(
            () => new TraitDefinition(Id, TraitCategory.Congenital, opposes: Id),
            Throws.ArgumentException);

    [Test]
    public void RejectsBothOpposesAndSpectrumTogether() =>
        Assert.That(
            () => new TraitDefinition(Id, TraitCategory.Congenital, opposes: Other, spectrum: "piety", tierPosition: 0),
            Throws.ArgumentException);

    [Test]
    public void RejectsASpectrumWithoutATierPosition() =>
        Assert.That(
            () => new TraitDefinition(Id, TraitCategory.Congenital, spectrum: "piety"),
            Throws.ArgumentException);

    [Test]
    public void RejectsATierPositionWithoutASpectrum() =>
        Assert.That(
            () => new TraitDefinition(Id, TraitCategory.Congenital, tierPosition: 0),
            Throws.ArgumentException);

    [Test]
    public void RejectsATierPositionOutOfRange() =>
        Assert.That(
            () => new TraitDefinition(Id, TraitCategory.Congenital, spectrum: "piety", tierPosition: 4),
            Throws.TypeOf<ArgumentOutOfRangeException>());
}
