using Gens.Simulation.Actors;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 10 item 5 save round-trip coverage, mirroring <see cref="ActorsSaveRoundTripTests"/>'s
/// identical pattern.</summary>
public sealed class HouseStandingSaveRoundTripTests
{
    [Test]
    public void HouseStandingRivalDossierAndRegionalFamiliesEntryRoundTripThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var actorA = state.ActorIds.Issue();
        var actorB = state.ActorIds.Issue();

        var key = HouseStandingKey.Between(actorA, actorB);
        var standing = new HouseStanding(
            HouseStandingLevel.Feuding,
            new AncestralGrudge("engagement_placeholder", new GameDate(3)));
        state.HouseStandings.Add(key, standing);

        var dossier = new RivalDossier(
            actorA,
            "A long-standing rivalry over a contested plot.",
            "the Unyielding",
            new GameDate(5),
            new[] { state.ChronicleEntryIds.Issue(), state.ChronicleEntryIds.Issue() });
        state.RivalDossiers.Add(actorA, dossier);

        var regionalEntry = new RegionalFamiliesEntry(actorB, "Gens Cornelia", LivingWorldActorStandingTrend.Rising, EconomicIdentityTag.Mercantile);
        state.RegionalFamiliesEntries.Add(actorB, regionalEntry);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.HouseStandings.TryGet(key, out var restoredStanding), Is.True);
            Assert.That(restoredStanding, Is.EqualTo(standing));
            Assert.That(restored.RivalDossiers.TryGet(actorA, out var restoredDossier), Is.True);
            Assert.That(restoredDossier!.ActorId, Is.EqualTo(dossier.ActorId));
            Assert.That(restoredDossier.Summary, Is.EqualTo(dossier.Summary));
            Assert.That(restoredDossier.HeadComboTitle, Is.EqualTo(dossier.HeadComboTitle));
            Assert.That(restoredDossier.LastUpdatedDate, Is.EqualTo(dossier.LastUpdatedDate));
            // Compared directly (not as a RivalDossier field) so NUnit's collection-aware comparison
            // applies: RivalDossier's own record-generated Equals compares this IReadOnlyList field by
            // reference, which two independently-deserialized arrays never satisfy even when equal
            // element-for-element.
            Assert.That(restoredDossier.RecentChronicleEntries, Is.EqualTo(dossier.RecentChronicleEntries));
            Assert.That(restored.RegionalFamiliesEntries.TryGet(actorB, out var restoredEntry), Is.True);
            Assert.That(restoredEntry, Is.EqualTo(regionalEntry));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }

    [Test]
    public void AnEmptyPreExistingSaveFixtureStillLoadsWithoutAnyHouseStandingData()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Saves", "Fixtures", "v1-empty-campaign.gens");
        var loaded = SaveReader.Read(path);

        Assert.Multiple(() =>
        {
            Assert.That(loaded.State.HouseStandings.Count, Is.EqualTo(0));
            Assert.That(loaded.State.RivalDossiers.Count, Is.EqualTo(0));
            Assert.That(loaded.State.RegionalFamiliesEntries.Count, Is.EqualTo(0));
        });
    }
}
