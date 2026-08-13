using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Characters;

public sealed class TraitCatalogTests
{
    private static readonly DefinitionId<Trait> Bold = new("bold");
    private static readonly DefinitionId<Trait> Cautious = new("cautious");
    private static readonly DefinitionId<Trait> Honest = new("honest");
    private static readonly DefinitionId<Trait> Impious = new("impious");
    private static readonly DefinitionId<Trait> Devout = new("devout");
    private static readonly DefinitionId<Trait> Zealous = new("zealous");
    private static readonly DefinitionId<Trait> Diligent = new("diligent");

    private static TraitCatalog BuildCatalog() => new(new[]
    {
        new TraitDefinition(Bold, TraitCategory.Congenital, PersonalityAxis.Boldness, 10, opposes: Cautious),
        new TraitDefinition(Cautious, TraitCategory.Congenital, PersonalityAxis.Boldness, -10, opposes: Bold),
        new TraitDefinition(Honest, TraitCategory.Congenital, PersonalityAxis.Honor, 25),
        new TraitDefinition(Impious, TraitCategory.Formative, PersonalityAxis.Zealotry, -25, spectrum: "piety", tierPosition: 0),
        new TraitDefinition(Devout, TraitCategory.Formative, PersonalityAxis.Zealotry, 10, spectrum: "piety", tierPosition: 2),
        new TraitDefinition(Zealous, TraitCategory.Formative, PersonalityAxis.Zealotry, 25, spectrum: "piety", tierPosition: 3),
        new TraitDefinition(Diligent, TraitCategory.Congenital),
    });

    [Test]
    public void AllowsAnUnrelatedCandidateTrait()
    {
        var catalog = BuildCatalog();

        var violation = catalog.CheckExclusivity(new[] { Honest }, Diligent);

        Assert.That(violation, Is.Null);
    }

    [Test]
    public void RejectsAnOpposedPairInEitherDirection()
    {
        var catalog = BuildCatalog();

        var forward = catalog.CheckExclusivity(new[] { Bold }, Cautious);
        var backward = catalog.CheckExclusivity(new[] { Cautious }, Bold);

        Assert.That(forward!.Value.Reason, Is.EqualTo(TraitExclusivityReason.OpposedPair));
        Assert.That(backward!.Value.Reason, Is.EqualTo(TraitExclusivityReason.OpposedPair));
    }

    [Test]
    public void RejectsASecondTagFromTheSameTieredSpectrum()
    {
        var catalog = BuildCatalog();

        var violation = catalog.CheckExclusivity(new[] { Impious }, Devout);

        Assert.That(violation!.Value.Reason, Is.EqualTo(TraitExclusivityReason.SameSpectrum));
        Assert.That(violation.Value.ConflictingTraitId, Is.EqualTo(Impious));
    }

    [Test]
    public void RejectsATraitAlreadyHeld()
    {
        var catalog = BuildCatalog();

        var violation = catalog.CheckExclusivity(new[] { Bold, Honest }, Honest);

        Assert.That(violation!.Value.Reason, Is.EqualTo(TraitExclusivityReason.AlreadyHeld));
    }

    [Test]
    public void AxisScoreSumsEveryHeldTraitsNudgeOnThatAxis()
    {
        var catalog = BuildCatalog();

        var score = catalog.GetAxisScore(new[] { Devout, Zealous }, PersonalityAxis.Zealotry);

        Assert.That(score, Is.EqualTo(35));
    }

    [Test]
    public void AxisScoreIgnoresTraitsThatDoNotNudgeThatAxis()
    {
        var catalog = BuildCatalog();

        var score = catalog.GetAxisScore(new[] { Bold, Diligent }, PersonalityAxis.Honor);

        Assert.That(score, Is.EqualTo(0));
    }

    [Test]
    public void AxisScoreClampsToTheAxisRange()
    {
        var definitions = Enumerable.Range(0, 5)
            .Select(i => new TraitDefinition(new DefinitionId<Trait>($"honest-{i}"), TraitCategory.Congenital, PersonalityAxis.Honor, 25))
            .ToArray();
        var catalog = new TraitCatalog(definitions);

        var score = catalog.GetAxisScore(definitions.Select(static d => d.Id), PersonalityAxis.Honor);

        Assert.That(score, Is.EqualTo(PersonalityAxes.MaxScore));
    }

    [Test]
    public void RejectsDuplicateDefinitionIdsAtConstruction() =>
        Assert.That(
            () => new TraitCatalog(new[]
            {
                new TraitDefinition(Bold, TraitCategory.Congenital),
                new TraitDefinition(Bold, TraitCategory.Congenital),
            }),
            Throws.ArgumentException);
}
