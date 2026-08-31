using Gens.Simulation.Events;
using Gens.Simulation.History;
using Gens.Simulation.Identity;
using NUnit.Framework;

namespace Gens.Simulation.Tests.History;

public sealed class HistoricalTimelineCatalogTests
{
    [Test]
    public void ConstructorRejectsDuplicateIds()
    {
        var figures = SampleHistoricalTimelineDefinitions.BuildFigureCatalog();
        var entry = new HistoricalTimelineEntryDefinition(
            new DefinitionId<HistoricalTimelineEntryDefinition>("dup"), HistoricalTimelineRange.Start,
            HistoricalEventType.Other, "Test", new[] { "Roman" },
            Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(), null, false);

        Assert.Throws<ArgumentException>(() => new HistoricalTimelineCatalog(new[] { entry, entry }, figures));
    }

    [Test]
    public void ConstructorRejectsAnUnknownInvolvedFigure()
    {
        var figures = new NamedHistoricalFigureCatalog(Array.Empty<NamedHistoricalFigureDefinition>());
        var entry = new HistoricalTimelineEntryDefinition(
            new DefinitionId<HistoricalTimelineEntryDefinition>("orphan"), HistoricalTimelineRange.Start,
            HistoricalEventType.Other, "Test", new[] { "Roman" },
            new[] { new DefinitionId<NamedHistoricalFigureDefinition>("unregistered-figure") }, null, false);

        Assert.Throws<ArgumentException>(() => new HistoricalTimelineCatalog(new[] { entry }, figures));
    }

    [Test]
    public void ConstructorRejectsAnUnknownLinkedEventWhenAnEventCatalogIsSupplied()
    {
        var figures = SampleHistoricalTimelineDefinitions.BuildFigureCatalog();
        var entry = new HistoricalTimelineEntryDefinition(
            new DefinitionId<HistoricalTimelineEntryDefinition>("bad-link"), HistoricalTimelineRange.Start,
            HistoricalEventType.Other, "Test", new[] { "Roman" },
            Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(),
            new DefinitionId<EventDefinition>("unregistered-event"), false);

        Assert.Throws<ArgumentException>(() => new HistoricalTimelineCatalog(new[] { entry }, figures, SampleEventDefinitions.BuildCatalog()));
    }

    [Test]
    public void ConstructorAllowsAnUnresolvedLinkedEventWhenNoEventCatalogIsSupplied()
    {
        var figures = SampleHistoricalTimelineDefinitions.BuildFigureCatalog();
        var entry = new HistoricalTimelineEntryDefinition(
            new DefinitionId<HistoricalTimelineEntryDefinition>("unchecked-link"), HistoricalTimelineRange.Start,
            HistoricalEventType.Other, "Test", new[] { "Roman" },
            Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(),
            new DefinitionId<EventDefinition>("unregistered-event"), false);

        Assert.DoesNotThrow(() => new HistoricalTimelineCatalog(new[] { entry }, figures));
    }

    [Test]
    public void SampleCatalogResolvesItsRealLinkedEventDefinition()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog(SampleEventDefinitions.BuildCatalog());

        Assert.That(catalog.Get(SampleHistoricalTimelineDefinitions.SampleDivergenceEligibleEntry).LinkedEventDefinitionRef,
            Is.EqualTo(SampleEventDefinitions.DomesticMurmur));
    }

    [Test]
    public void ChronologicalOrdersEntriesByDateRegardlessOfDeclarationOrder()
    {
        var figures = SampleHistoricalTimelineDefinitions.BuildFigureCatalog();
        var later = new HistoricalTimelineEntryDefinition(
            new DefinitionId<HistoricalTimelineEntryDefinition>("later"), HistoricalYear.ToGameDate(44, isBce: true),
            HistoricalEventType.Other, "Later", new[] { "Roman" }, Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(), null, false);
        var earlier = new HistoricalTimelineEntryDefinition(
            new DefinitionId<HistoricalTimelineEntryDefinition>("earlier"), HistoricalYear.ToGameDate(133, isBce: true),
            HistoricalEventType.Other, "Earlier", new[] { "Roman" }, Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(), null, false);

        var catalog = new HistoricalTimelineCatalog(new[] { later, earlier }, figures);

        var chronological = catalog.Chronological();

        Assert.Multiple(() =>
        {
            Assert.That(chronological, Has.Count.EqualTo(2));
            Assert.That(chronological[0].Id.Value, Is.EqualTo("earlier"));
            Assert.That(chronological[1].Id.Value, Is.EqualTo("later"));
        });
    }

    [Test]
    public void GetThrowsForAnUnregisteredId()
    {
        var catalog = SampleHistoricalTimelineDefinitions.BuildCatalog();

        Assert.Throws<KeyNotFoundException>(() => catalog.Get(new DefinitionId<HistoricalTimelineEntryDefinition>("unregistered")));
    }

    // ---- Real authored content -----------------------------------------------------------------

    [Test]
    public void TheRealHistoricalFigureRosterHasAllFortyThreeNamedFigures()
    {
        var figures = KnownWorldHistoricalFigures.BuildCatalog();

        Assert.That(figures.All().Count(), Is.EqualTo(43));
    }

    [Test]
    public void TheRealHistoricalTimelineBuildsCleanlyAndCrossReferencesResolve()
    {
        var catalog = KnownWorldHistoricalTimeline.BuildCatalog();

        Assert.That(catalog.All().Count(), Is.EqualTo(85));
    }

    [Test]
    public void EveryRealEntryFallsWithinTheSupportedRange()
    {
        var catalog = KnownWorldHistoricalTimeline.BuildCatalog();

        Assert.That(catalog.All().All(entry => HistoricalTimelineRange.Contains(entry.Date)), Is.True);
    }

    [Test]
    public void RealEntriesOfEligibleTypesAreDivergenceEligibleAndOthersAreNot()
    {
        var catalog = KnownWorldHistoricalTimeline.BuildCatalog();

        Assert.Multiple(() =>
        {
            foreach (var entry in catalog.All())
            {
                var shouldBeEligible = entry.EventType is HistoricalEventType.ImperialSuccession
                    or HistoricalEventType.WarOrRevolt or HistoricalEventType.PoliticalTrial;
                Assert.That(entry.DivergenceEligible, Is.EqualTo(shouldBeEligible), entry.Id.Value);
            }
        });
    }

    [Test]
    public void TheRealTimelineIsChronologicallyOrdered()
    {
        var catalog = KnownWorldHistoricalTimeline.BuildCatalog();

        var totalMonths = catalog.Chronological().Select(entry => entry.Date.TotalMonths).ToArray();
        var sorted = totalMonths.OrderBy(value => value).ToArray();

        Assert.That(totalMonths, Is.EqualTo(sorted));
    }
}
