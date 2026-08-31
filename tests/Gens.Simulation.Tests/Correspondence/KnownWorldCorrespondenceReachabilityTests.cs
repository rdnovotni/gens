using Gens.Simulation.Correspondence;
using Gens.Simulation.Cultures;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Correspondence;

/// <summary>Covers the real (culture, reachability) content Phase 13 item 4 authors, closing item 3's
/// own explicitly-left-open seam (<see cref="CorrespondenceReachabilityCatalog"/>'s doc comment).</summary>
public sealed class KnownWorldCorrespondenceReachabilityTests
{
    [Test]
    public void GallicBritishAndGermanicAreOralTraditionPartialPerSevenOwnNamedExample()
    {
        var catalog = KnownWorldCorrespondenceReachability.BuildCatalog();
        Assert.Multiple(() =>
        {
            Assert.That(catalog.Resolve(KnownWorldCultures.Gallic), Is.EqualTo(CorrespondenceReachability.OralTraditionPartial));
            Assert.That(catalog.Resolve(KnownWorldCultures.British), Is.EqualTo(CorrespondenceReachability.OralTraditionPartial));
            Assert.That(catalog.Resolve(KnownWorldCultures.Germanic), Is.EqualTo(CorrespondenceReachability.OralTraditionPartial));
        });
    }

    [Test]
    public void NubianKushiteIsOralTraditionBlockedForMeroiticsOwnUndecipheredScript()
    {
        var catalog = KnownWorldCorrespondenceReachability.BuildCatalog();
        Assert.That(catalog.Resolve(KnownWorldCultures.NubianKushite), Is.EqualTo(CorrespondenceReachability.OralTraditionBlocked));
    }

    [Test]
    public void AnUnlistedCultureDefaultsToFullyLiteratePerTheCatalogsOwnHonestDefault()
    {
        var catalog = KnownWorldCorrespondenceReachability.BuildCatalog();
        Assert.Multiple(() =>
        {
            Assert.That(catalog.Resolve(KnownWorldCultures.Roman), Is.EqualTo(CorrespondenceReachability.FullyLiterate));
            Assert.That(catalog.Resolve(KnownWorldCultures.Parthian), Is.EqualTo(CorrespondenceReachability.FullyLiterate));
        });
    }
}
