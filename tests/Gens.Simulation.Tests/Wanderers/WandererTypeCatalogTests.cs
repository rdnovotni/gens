using Gens.Simulation.Characters;
using Gens.Simulation.Ledger;
using Gens.Simulation.Regions;
using Gens.Simulation.Wanderers;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Wanderers;

public sealed class WandererTypeCatalogTests
{
    private static WandererTypeProfile Profile(
        WandererType type = WandererType.Physician,
        GazetteerRole[]? roles = null,
        Money? hostFee = null,
        Money? recruitFee = null,
        int hostDignitasGain = 3,
        int engagementFameGain = 3,
        DutySlot? dutySlot = null) =>
        new(
            type,
            roles ?? new[] { GazetteerRole.MarketHub },
            prefersHighProminence: false,
            hostFee ?? Money.FromDenarii(10),
            recruitFee ?? Money.FromDenarii(50),
            hostDignitasGain,
            engagementFameGain,
            dutySlot);

    [Test]
    public void TheDefaultRosterAuthorsAllSixDesignDocumentTypes()
    {
        var catalog = WandererTypeCatalog.BuildDefault();

        Assert.That(catalog.Count, Is.EqualTo(Enum.GetValues<WandererType>().Length));
        foreach (var type in Enum.GetValues<WandererType>())
            Assert.That(catalog.TryGet(type, out _), Is.True, $"No profile authored for {type}.");
    }

    [Test]
    public void EveryAuthoredTypeCostsMoreToRecruitThanToHostAndIsNeverFree()
    {
        foreach (var profile in WandererTypeCatalog.BuildDefault().All())
        {
            Assert.That(profile.HostFee, Is.GreaterThan(Money.Zero), $"{profile.Type} hosts for free.");
            Assert.That(profile.RecruitFee, Is.GreaterThan(profile.HostFee), $"{profile.Type} recruits no dearer than it hosts.");
        }
    }

    [Test]
    public void OnlyThePhysicianAndArchitectMapToARealFamiliaDutySlot()
    {
        var catalog = WandererTypeCatalog.BuildDefault();

        Assert.Multiple(() =>
        {
            Assert.That(catalog.Get(WandererType.Physician).RecruitedDutySlot, Is.EqualTo(DutySlot.Physician));
            Assert.That(catalog.Get(WandererType.ArchitectEngineer).RecruitedDutySlot, Is.EqualTo(DutySlot.Craftsman));
            Assert.That(catalog.Get(WandererType.PhilosopherRhetorician).RecruitedDutySlot, Is.Null);
            Assert.That(catalog.Get(WandererType.MerchantPeddler).RecruitedDutySlot, Is.Null);
            Assert.That(catalog.Get(WandererType.Entertainer).RecruitedDutySlot, Is.Null);
            Assert.That(catalog.Get(WandererType.HolyManAstrologer).RecruitedDutySlot, Is.Null);
        });
    }

    [Test]
    public void ADuplicateTypeIsRejectedAtConstruction()
    {
        var duplicate = new[] { Profile(), Profile() };

        Assert.That(() => new WandererTypeCatalog(duplicate), Throws.ArgumentException);
    }

    [Test]
    public void AnUnauthoredTypeThrowsOnGet()
    {
        var catalog = new WandererTypeCatalog(new[] { Profile(WandererType.Physician) });

        Assert.That(() => catalog.Get(WandererType.Entertainer), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void AProfileRequiresAtLeastOnePreferredRole()
    {
        Assert.That(() => Profile(roles: Array.Empty<GazetteerRole>()), Throws.ArgumentException);
    }

    [Test]
    public void AProfileRejectsRepeatedPreferredRoles()
    {
        Assert.That(
            () => Profile(roles: new[] { GazetteerRole.MarketHub, GazetteerRole.MarketHub }),
            Throws.ArgumentException);
    }

    [Test]
    public void AProfileRejectsANegativeFeeAndANonPositiveBenefit()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => Profile(hostFee: Money.FromDenarii(-1)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => Profile(recruitFee: Money.FromDenarii(-1)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => Profile(hostDignitasGain: 0), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => Profile(engagementFameGain: 0), Throws.TypeOf<ArgumentOutOfRangeException>());
        });
    }
}
