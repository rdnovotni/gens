using Gens.Simulation.Hazards;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Hazards;

public sealed class HazardExposureCalculatorTests
{
    [Test]
    public void FireExposureRisesWithBuildingDensityAndDrySeasonBonus()
    {
        var sparse = HazardExposureCalculator.FireExposure(0.1, drySeasonBonus: 0);
        var dense = HazardExposureCalculator.FireExposure(0.9, drySeasonBonus: 0);
        Assert.That(dense, Is.GreaterThan(sparse));

        var withDrySeason = HazardExposureCalculator.FireExposure(0.5, DisasterCompoundingCalculator.DrySeasonFireExposureBonus(true));
        var withoutDrySeason = HazardExposureCalculator.FireExposure(0.5, DisasterCompoundingCalculator.DrySeasonFireExposureBonus(false));
        Assert.That(withDrySeason, Is.GreaterThan(withoutDrySeason));
    }

    [Test]
    public void FloodExposureRisesWithRiverAdjacencyAndFallingForestCover()
    {
        var landlocked = HazardExposureCalculator.FloodExposure(0.0, forestCoverFraction: 0.5);
        var riverside = HazardExposureCalculator.FloodExposure(0.8, forestCoverFraction: 0.5);
        Assert.That(riverside, Is.GreaterThan(landlocked));

        var forested = HazardExposureCalculator.FloodExposure(0.8, forestCoverFraction: 1.0);
        var deforested = HazardExposureCalculator.FloodExposure(0.8, forestCoverFraction: 0.0);
        Assert.That(deforested, Is.GreaterThan(forested));
    }

    [Test]
    public void EarthquakeExposureIsAFlatBaseline()
    {
        Assert.That(HazardExposureCalculator.EarthquakeExposure(), Is.EqualTo(HazardExposureCalculator.EarthquakeExposure()));
        Assert.That(HazardExposureCalculator.EarthquakeExposure(), Is.InRange(0, 100));
    }

    [Test]
    public void DroughtFamineExposureRisesDuringTheDrySeason()
    {
        Assert.That(HazardExposureCalculator.DroughtFamineExposure(true), Is.GreaterThan(HazardExposureCalculator.DroughtFamineExposure(false)));
    }

    [Test]
    public void StormExposureRisesWithCoastalFraction()
    {
        var inland = HazardExposureCalculator.StormExposure(0.0);
        var coastal = HazardExposureCalculator.StormExposure(1.0);
        Assert.That(coastal, Is.GreaterThan(inland));
    }

    [Test]
    public void LandslideExposureRisesWithHillsFractionAndFallingForestCover()
    {
        var flat = HazardExposureCalculator.LandslideExposure(0.0, forestCoverFraction: 0.5);
        var hilly = HazardExposureCalculator.LandslideExposure(0.8, forestCoverFraction: 0.5);
        Assert.That(hilly, Is.GreaterThan(flat));

        var forested = HazardExposureCalculator.LandslideExposure(0.8, forestCoverFraction: 1.0);
        var deforested = HazardExposureCalculator.LandslideExposure(0.8, forestCoverFraction: 0.0);
        Assert.That(deforested, Is.GreaterThan(forested));
    }

    [Test]
    public void EveryExposureScoreStaysWithinZeroToOneHundred()
    {
        Assert.That(HazardExposureCalculator.FireExposure(5.0, 100), Is.InRange(0, 100));
        Assert.That(HazardExposureCalculator.FloodExposure(5.0, -5.0), Is.InRange(0, 100));
        Assert.That(HazardExposureCalculator.LandslideExposure(5.0, -5.0), Is.InRange(0, 100));
        Assert.That(HazardExposureCalculator.StormExposure(5.0), Is.InRange(0, 100));
        Assert.That(HazardExposureCalculator.BlightInfestationExposure(), Is.InRange(0, 100));
        Assert.That(HazardExposureCalculator.FrostExposure(), Is.InRange(0, 100));
    }
}
