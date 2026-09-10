using Gens.Simulation.Interactions;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Saves;

/// <summary>Phase 16 item 1 save round-trip coverage, mirroring <see cref="SchemeSaveRoundTripTests"/>'s
/// identical pattern.</summary>
public sealed class SpyPlacementSaveRoundTripTests
{
    [Test]
    public void SpyPlacementRoundTripsThroughTheDtoAndCanonicalJson()
    {
        var state = new WorldState(new GameDate(5));
        var spyId = state.CharacterIds.Issue();
        var sponsorId = state.CharacterIds.Issue();
        var targetActorId = state.ActorIds.Issue();
        var placementId = state.SpyPlacementIds.Issue();

        var placement = SpyPlacement.Create(
            placementId, spyId, sponsorId, targetActorId, SpyPlacementType.PersistentNetwork, concealmentQuality: 65, new GameDate(2)) with
        {
            DiscoveryRisk = 40,
            MonthsActive = 3,
            LastProgressedDate = new GameDate(5),
        };
        state.SpyPlacements.Add(placementId, placement);

        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.SpyPlacements.TryGet(placementId, out var restoredPlacement), Is.True);
            Assert.That(restoredPlacement, Is.EqualTo(placement));
        });

        var bytesA = CanonicalJson.SerializeToCanonicalBytes(dto);
        var bytesB = CanonicalJson.SerializeToCanonicalBytes(WorldStateMapper.ToDto(restored));
        Assert.That(bytesB, Is.EqualTo(bytesA));
    }
}
