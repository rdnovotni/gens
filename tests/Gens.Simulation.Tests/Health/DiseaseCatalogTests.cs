using Gens.Simulation.Health;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Health;

public sealed class DiseaseCatalogTests
{
    [Test]
    public void BuildConditionCatalogContainsAllElevenNamedDiseases()
    {
        var catalog = DiseaseCatalog.BuildConditionCatalog();
        Assert.That(catalog.Count, Is.EqualTo(11));
    }

    [TestCase("disease-roman-fever")]
    [TestCase("disease-the-flux")]
    [TestCase("disease-ophthalmia")]
    [TestCase("disease-consumption")]
    [TestCase("disease-leprosy")]
    [TestCase("disease-gout")]
    [TestCase("disease-saturnism")]
    public void EveryEndemicDiseaseIsChronicAndHasNoCure(string id)
    {
        var catalog = DiseaseCatalog.BuildConditionCatalog();
        var definition = catalog.Get(new(id));
        Assert.That(definition.Category, Is.EqualTo(HealthConditionCategory.Chronic));
        Assert.That(definition.HasCure, Is.False);
    }

    [TestCase("disease-pestilence", false)]
    [TestCase("disease-pox", false)]
    [TestCase("disease-camp-fever", false)]
    [TestCase("disease-enteric-fever", true)]
    public void EveryEpidemicDiseaseIsAcuteWithTheDocumentedCureStatus(string id, bool hasCure)
    {
        var catalog = DiseaseCatalog.BuildConditionCatalog();
        var definition = catalog.Get(new(id));
        Assert.That(definition.Category, Is.EqualTo(HealthConditionCategory.Acute));
        Assert.That(definition.HasCure, Is.EqualTo(hasCure));
    }

    [Test]
    public void SevenEndemicProfilesAndFourEpidemicProfilesAreRegistered()
    {
        Assert.That(DiseaseCatalog.EndemicProfiles, Has.Count.EqualTo(7));
        Assert.That(DiseaseCatalog.EpidemicProfiles, Has.Count.EqualTo(4));
    }

    [Test]
    public void OnlyLeprosyCarriesSocialExclusion()
    {
        foreach (var profile in DiseaseCatalog.EndemicProfiles)
        {
            var expected = profile.ConditionId == DiseaseCatalog.Leprosy;
            Assert.That(profile.SocialExclusion, Is.EqualTo(expected), profile.ConditionId.Value);
        }
    }

    [Test]
    public void OnlyEntericFeverIsWaterborne()
    {
        foreach (var profile in DiseaseCatalog.EpidemicProfiles)
        {
            var expected = profile.ConditionId == DiseaseCatalog.EntericFever ? EpidemicVector.Waterborne : EpidemicVector.PersonToPerson;
            Assert.That(profile.Vector, Is.EqualTo(expected), profile.ConditionId.Value);
        }
    }

    [Test]
    public void TryGetEndemicProfileFindsARegisteredDiseaseAndFailsForAnUnregisteredOne()
    {
        Assert.That(DiseaseCatalog.TryGetEndemicProfile(DiseaseCatalog.Gout, out var profile), Is.True);
        Assert.That(profile.Driver, Is.EqualTo(EndemicExposureDriver.LavishDiet));
        Assert.That(DiseaseCatalog.TryGetEndemicProfile(DiseaseCatalog.Pestilence, out _), Is.False);
    }

    [Test]
    public void TryGetEpidemicProfileFindsARegisteredDiseaseAndFailsForAnUnregisteredOne()
    {
        Assert.That(DiseaseCatalog.TryGetEpidemicProfile(DiseaseCatalog.EntericFever, out var profile), Is.True);
        Assert.That(profile.Vector, Is.EqualTo(EpidemicVector.Waterborne));
        Assert.That(DiseaseCatalog.TryGetEpidemicProfile(DiseaseCatalog.Gout, out _), Is.False);
    }
}
