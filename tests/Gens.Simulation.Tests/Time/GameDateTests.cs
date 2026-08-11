using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Time;

public sealed class GameDateTests
{
    [Test]
    public void EpochIsJanuaryOfAstronomicalYearMinus753()
    {
        var (year, month) = new GameDate(0).ToCalendar();
        Assert.That(year, Is.EqualTo(-753));
        Assert.That(month, Is.EqualTo(1));
    }

    [Test]
    public void MonthRolloverStaysWithinTheSameYear()
    {
        var (year, month) = new GameDate(11).ToCalendar();
        Assert.That(year, Is.EqualTo(-753));
        Assert.That(month, Is.EqualTo(12));
    }

    [Test]
    public void MonthRolloverAdvancesTheYear()
    {
        var (year, month) = new GameDate(12).ToCalendar();
        Assert.That(year, Is.EqualTo(-752));
        Assert.That(month, Is.EqualTo(1));
    }

    [Test]
    public void NegativeTotalMonthsFloorsCorrectly()
    {
        var (year, month) = new GameDate(-1).ToCalendar();
        Assert.That(year, Is.EqualTo(-754));
        Assert.That(month, Is.EqualTo(12));
    }

    [TestCase(0, 1)]
    [TestCase(-1, 2)]
    [TestCase(1, 1)]
    public void DisplayYearHasNoYearZero(int astronomicalYear, int expectedDisplayYear)
    {
        Assert.That(GameDate.ToDisplayYear(astronomicalYear), Is.EqualTo(expectedDisplayYear));
    }

    [Test]
    public void DisplayYearLabelCrossesBceToCe()
    {
        Assert.That(new GameDate(0).ToDisplayYearLabel(), Is.EqualTo("754 BCE"));

        // 753 years of 12 months lands on astronomical year 0 (1 BCE); one more month crosses to 1 CE.
        var oneBce = new GameDate(753 * 12);
        Assert.That(oneBce.ToDisplayYearLabel(), Is.EqualTo("1 BCE"));

        var oneCe = new GameDate(754 * 12);
        Assert.That(oneCe.ToDisplayYearLabel(), Is.EqualTo("1 CE"));
    }

    [Test]
    public void NextMonthStillThrowsOnOverflow()
    {
        var maxDate = new GameDate(int.MaxValue);
        Assert.That(() => maxDate.NextMonth(), Throws.TypeOf<OverflowException>());
    }
}
