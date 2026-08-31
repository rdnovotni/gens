using Gens.Simulation.History;
using Gens.Simulation.Identity;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.History;

public sealed class HistoricalTimelineEntryDefinitionTests
{
    private static readonly DefinitionId<HistoricalTimelineEntryDefinition> Id = new("test-entry");

    private static HistoricalTimelineEntryDefinition Build(
        string? realWorldName = "Test Event",
        IReadOnlyList<string>? regionRelevance = null,
        GameDate? date = null) =>
        new(
            Id,
            date ?? HistoricalTimelineRange.Start,
            HistoricalEventType.Other,
            realWorldName!,
            regionRelevance ?? new[] { "Roman" },
            Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(),
            linkedEventDefinitionRef: null,
            divergenceEligible: false);

    [Test]
    public void ValidEntryConstructsSuccessfully()
    {
        var entry = Build();

        Assert.That(entry.Id, Is.EqualTo(Id));
    }

    [Test]
    public void EmptyRealWorldNameThrows()
    {
        Assert.Throws<ArgumentException>(() => Build(realWorldName: ""));
    }

    [Test]
    public void DateBeforeRangeStartThrows()
    {
        var beforeStart = new GameDate(HistoricalTimelineRange.Start.TotalMonths - 1);

        Assert.Throws<ArgumentException>(() => Build(date: beforeStart));
    }

    [Test]
    public void DateAtOrAfterRangeEndThrows()
    {
        Assert.Throws<ArgumentException>(() => Build(date: HistoricalTimelineRange.End));
    }

    [Test]
    public void DateAtRangeStartIsValid()
    {
        Assert.DoesNotThrow(() => Build(date: HistoricalTimelineRange.Start));
    }

    [Test]
    public void DateJustBeforeRangeEndIsValid()
    {
        var justBeforeEnd = new GameDate(HistoricalTimelineRange.End.TotalMonths - 1);

        Assert.DoesNotThrow(() => Build(date: justBeforeEnd));
    }

    [Test]
    public void EmptyRegionRelevanceThrows()
    {
        Assert.Throws<ArgumentException>(() => Build(regionRelevance: Array.Empty<string>()));
    }

    [Test]
    public void WhitespaceRegionRelevanceEntryThrows()
    {
        Assert.Throws<ArgumentException>(() => Build(regionRelevance: new[] { "Roman", "   " }));
    }

    [Test]
    public void NullInvolvedFigureIdsThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new HistoricalTimelineEntryDefinition(
            Id, HistoricalTimelineRange.Start, HistoricalEventType.Other, "Test Event", new[] { "Roman" },
            null!, linkedEventDefinitionRef: null, divergenceEligible: false));
    }

    [Test]
    public void EmptyInvolvedFigureIdsIsAllowed()
    {
        Assert.DoesNotThrow(() => new HistoricalTimelineEntryDefinition(
            Id, HistoricalTimelineRange.Start, HistoricalEventType.Other, "Test Event", new[] { "Roman" },
            System.Array.Empty<DefinitionId<NamedHistoricalFigureDefinition>>(), linkedEventDefinitionRef: null, divergenceEligible: false));
    }
}

public sealed class NamedHistoricalFigureDefinitionTests
{
    private static readonly DefinitionId<NamedHistoricalFigureDefinition> Id = new("test-figure");

    [Test]
    public void ValidFigureConstructsSuccessfully()
    {
        var figure = new NamedHistoricalFigureDefinition(
            Id, "Test Figure", HistoricalFigureRole.Senator,
            realAccessionOrStartYear: null, realDeathOrEndYear: null);

        Assert.That(figure.RealName, Is.EqualTo("Test Figure"));
    }

    [Test]
    public void EmptyRealNameThrows()
    {
        Assert.Throws<ArgumentException>(() => new NamedHistoricalFigureDefinition(
            Id, "", HistoricalFigureRole.Senator, null, null));
    }

    [Test]
    public void StartAfterEndThrows()
    {
        var start = HistoricalYear.ToGameDate(44, isBce: true);
        var end = HistoricalYear.ToGameDate(133, isBce: true);

        Assert.Throws<ArgumentException>(() => new NamedHistoricalFigureDefinition(
            Id, "Test Figure", HistoricalFigureRole.Senator, start, end));
    }

    [Test]
    public void StartEqualToEndIsAllowed()
    {
        var year = HistoricalYear.ToGameDate(69, isBce: false);

        Assert.DoesNotThrow(() => new NamedHistoricalFigureDefinition(
            Id, "Test Figure", HistoricalFigureRole.HeadOfState, year, year));
    }

    [Test]
    public void StartBeforeEndIsAllowed()
    {
        var start = HistoricalYear.ToGameDate(133, isBce: true);
        var end = HistoricalYear.ToGameDate(44, isBce: true);

        Assert.DoesNotThrow(() => new NamedHistoricalFigureDefinition(
            Id, "Test Figure", HistoricalFigureRole.Senator, start, end));
    }
}
