using Gens.Simulation.History;
using NUnit.Framework;

namespace Gens.Simulation.Tests.History;

public sealed class HistoricalYearTests
{
    [Test]
    public void FortyFourBceRoundTripsToTheAssassinationYearLabel()
    {
        var date = HistoricalYear.ToGameDate(44, isBce: true);

        Assert.That(date.ToDisplayYearLabel(), Is.EqualTo("44 BCE"));
    }

    [Test]
    public void SeventyNineCeRoundTripsToTheVesuviusYearLabel()
    {
        var date = HistoricalYear.ToGameDate(79, isBce: false);

        Assert.That(date.ToDisplayYearLabel(), Is.EqualTo("79 CE"));
    }

    [Test]
    public void OneBceAndOneCeAreAdjacentWithNoYearZeroBetweenThem()
    {
        var oneBce = HistoricalYear.ToGameDate(1, isBce: true);
        var oneCe = HistoricalYear.ToGameDate(1, isBce: false);

        Assert.Multiple(() =>
        {
            Assert.That(oneBce.ToDisplayYearLabel(), Is.EqualTo("1 BCE"));
            Assert.That(oneCe.ToDisplayYearLabel(), Is.EqualTo("1 CE"));
            Assert.That(oneCe.TotalMonths, Is.EqualTo(oneBce.TotalMonths + 12));
        });
    }

    [Test]
    public void OneThirtyThreeBceRoundTripsToTheRangeOpeningYearLabel()
    {
        var date = HistoricalYear.ToGameDate(133, isBce: true);

        Assert.That(date.ToDisplayYearLabel(), Is.EqualTo("133 BCE"));
    }

    [Test]
    public void MonthOfYearDefaultsToJanuary()
    {
        var date = HistoricalYear.ToGameDate(79, isBce: false);

        Assert.That(date.ToCalendar().MonthOfYear, Is.EqualTo(1));
    }

    [Test]
    public void ExplicitMonthOfYearIsHonored()
    {
        var date = HistoricalYear.ToGameDate(79, isBce: false, monthOfYear: 8);

        Assert.That(date.ToCalendar().MonthOfYear, Is.EqualTo(8));
    }

    [Test]
    public void DisplayYearBelowOneThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => HistoricalYear.ToGameDate(0, isBce: true));
    }

    [Test]
    public void MonthOfYearOutOfRangeThrows()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => HistoricalYear.ToGameDate(44, isBce: true, monthOfYear: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => HistoricalYear.ToGameDate(44, isBce: true, monthOfYear: 13));
        });
    }

    [Test]
    public void LaterBceYearsProduceEarlierDatesThanEarlierBceYears()
    {
        // "133 BC" is chronologically earlier than "44 BC" despite the larger display magnitude.
        var oneThirtyThreeBce = HistoricalYear.ToGameDate(133, isBce: true);
        var fortyFourBce = HistoricalYear.ToGameDate(44, isBce: true);

        Assert.That(oneThirtyThreeBce.TotalMonths, Is.LessThan(fortyFourBce.TotalMonths));
    }
}
