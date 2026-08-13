using FsCheck;
using FsCheck.Fluent;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;

namespace Gens.Simulation.Tests.Characters;

/// <summary>Property-based coverage of <see cref="TraitCatalog.CheckExclusivity"/>: opposed pairs and
/// tiered-spectrum membership exclude regardless of which unrelated traits are already held or in what
/// order, an already-held trait is always rejected, and traits with no relationship to one another
/// never conflict.</summary>
public sealed class TraitExclusivityPropertyTests
{
    [FsCheck.NUnit.Property]
    public FsCheck.Property AnyTwoDistinctMembersOfTheSameSpectrumMutuallyExclude(
        PositiveInt spectrumSizeRaw, NonNegativeInt indexARaw, NonNegativeInt indexBRaw)
    {
        var size = Math.Clamp(spectrumSizeRaw.Get, 2, 12);
        var indexA = indexARaw.Get % size;
        var indexB = indexBRaw.Get % size;
        if (indexA == indexB)
            return true.ToProperty();

        var ids = new DefinitionId<Trait>[size];
        var definitions = new List<TraitDefinition>();
        for (var i = 0; i < size; i++)
        {
            ids[i] = new DefinitionId<Trait>($"spectrum-member-{i}");
            definitions.Add(new TraitDefinition(ids[i], TraitCategory.Formative, spectrum: "test-spectrum", tierPosition: i % 4));
        }

        var catalog = new TraitCatalog(definitions);
        var forward = catalog.CheckExclusivity(new[] { ids[indexA] }, ids[indexB]);
        var backward = catalog.CheckExclusivity(new[] { ids[indexB] }, ids[indexA]);

        return (forward is { Reason: TraitExclusivityReason.SameSpectrum } &&
                backward is { Reason: TraitExclusivityReason.SameSpectrum }).ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property AnOpposedPairExcludesRegardlessOfHowManyUnrelatedTraitsAreAlreadyHeld(NonNegativeInt extraCountRaw)
    {
        var extraCount = extraCountRaw.Get % 10;

        var a = new DefinitionId<Trait>("opposed-a");
        var b = new DefinitionId<Trait>("opposed-b");
        var definitions = new List<TraitDefinition>
        {
            new(a, TraitCategory.Congenital, opposes: b),
            new(b, TraitCategory.Congenital, opposes: a),
        };
        var existing = new List<DefinitionId<Trait>>();
        for (var i = 0; i < extraCount; i++)
        {
            var extraId = new DefinitionId<Trait>($"unrelated-{i}");
            definitions.Add(new TraitDefinition(extraId, TraitCategory.Congenital));
            existing.Add(extraId);
        }
        existing.Add(a);

        var catalog = new TraitCatalog(definitions);
        var violation = catalog.CheckExclusivity(existing, b);

        return (violation is { Reason: TraitExclusivityReason.OpposedPair, ConflictingTraitId: var conflict } && conflict == a)
            .ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property AnAlreadyHeldTraitIsAlwaysRejectedRegardlessOfHowManyUnrelatedTraitsPrecedeIt(NonNegativeInt extraCountRaw)
    {
        var extraCount = extraCountRaw.Get % 10;

        var held = new DefinitionId<Trait>("held-trait");
        var definitions = new List<TraitDefinition> { new(held, TraitCategory.Congenital) };
        var existing = new List<DefinitionId<Trait>>();
        for (var i = 0; i < extraCount; i++)
        {
            var extraId = new DefinitionId<Trait>($"unrelated-{i}");
            definitions.Add(new TraitDefinition(extraId, TraitCategory.Congenital));
            existing.Add(extraId);
        }
        existing.Add(held);

        var catalog = new TraitCatalog(definitions);
        var violation = catalog.CheckExclusivity(existing, held);

        return (violation is { Reason: TraitExclusivityReason.AlreadyHeld }).ToProperty();
    }

    [FsCheck.NUnit.Property]
    public FsCheck.Property StandaloneTraitsWithNoDeclaredRelationshipNeverConflict(NonNegativeInt countRaw)
    {
        var count = countRaw.Get % 10 + 2;

        var ids = new DefinitionId<Trait>[count];
        var definitions = new List<TraitDefinition>();
        for (var i = 0; i < count; i++)
        {
            ids[i] = new DefinitionId<Trait>($"standalone-{i}");
            definitions.Add(new TraitDefinition(ids[i], TraitCategory.Congenital));
        }

        var catalog = new TraitCatalog(definitions);
        var existing = ids.Take(count - 1).ToArray();
        var candidate = ids[count - 1];
        var violation = catalog.CheckExclusivity(existing, candidate);

        return (violation is null).ToProperty();
    }
}
