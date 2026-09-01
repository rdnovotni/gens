using Gens.Simulation.Hazards;
using Gens.Simulation.Land;
using Gens.Simulation.Saves;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using NUnit.Framework;

namespace Gens.Simulation.Tests.Hazards;

public sealed class HazardsSaveRoundTripTests
{
    [Test]
    public void DisasterEventsAndDormantVolcanoesRoundTripThroughTheDtoAndDeterministicHashStaysStable()
    {
        var state = new WorldState(new GameDate(20));
        var regionId = state.RegionIds.Issue();
        var settlementId = state.SettlementIds.Issue();
        state.Settlements.Add(settlementId, Settlement.Create(settlementId, regionId));
        var plotId = state.PlotIds.Issue();
        state.Plots.Add(plotId, Plot.Create(plotId, settlementId, TerrainType.Hills, capacity: 1));

        var ordinaryEventId = state.DisasterEventIds.Issue();
        state.DisasterEvents.Add(ordinaryEventId, DisasterEvent.Create(
            ordinaryEventId, settlementId, new GameDate(15), HazardType.Fire, DisasterSeverity.Severe,
            triggeredByCompounding: false, buildingsDamaged: 2, populationLost: 0, perennialCropSetback: false));

        var chainedFloodId = state.DisasterEventIds.Issue();
        state.DisasterEvents.Add(chainedFloodId, DisasterEvent.Create(
            chainedFloodId, settlementId, new GameDate(16), HazardType.Flood, DisasterSeverity.Moderate,
            triggeredByCompounding: true, buildingsDamaged: 1, populationLost: 0, perennialCropSetback: false));

        var catastrophicFrostId = state.DisasterEventIds.Issue();
        state.DisasterEvents.Add(catastrophicFrostId, DisasterEvent.Create(
            catastrophicFrostId, settlementId, new GameDate(17), HazardType.Frost, DisasterSeverity.Catastrophic,
            triggeredByCompounding: false, buildingsDamaged: 3, populationLost: 5, perennialCropSetback: true));

        state.DormantVolcanoes.Add(plotId, DormantVolcano.Create(plotId, settlementId) with
        {
            HasErupted = true,
            PostEruptionFertilityBoostActive = true,
        });

        var beforeHash = StateHasher.Hash(state);
        var dto = WorldStateMapper.ToDto(state);
        var restored = WorldStateMapper.ToWorldState(dto);

        Assert.Multiple(() =>
        {
            Assert.That(restored.DisasterEventIds.Peek, Is.EqualTo(state.DisasterEventIds.Peek));
            Assert.That(restored.DisasterEvents.Count, Is.EqualTo(3));

            restored.DisasterEvents.TryGet(catastrophicFrostId, out var restoredFrost);
            Assert.That(restoredFrost.Severity, Is.EqualTo(DisasterSeverity.Catastrophic));
            Assert.That(restoredFrost.PerennialCropSetback, Is.True);
            Assert.That(restoredFrost.PopulationLost, Is.EqualTo(5));

            restored.DisasterEvents.TryGet(chainedFloodId, out var restoredFlood);
            Assert.That(restoredFlood.TriggeredByCompounding, Is.True);

            Assert.That(restored.DormantVolcanoes.Count, Is.EqualTo(1));
            restored.DormantVolcanoes.TryGet(plotId, out var restoredVolcano);
            Assert.That(restoredVolcano.HasErupted, Is.True);
            Assert.That(restoredVolcano.PostEruptionFertilityBoostActive, Is.True);
            Assert.That(restoredVolcano.SettlementId, Is.EqualTo(settlementId));

            Assert.That(StateHasher.Hash(restored), Is.EqualTo(beforeHash));
        });
    }
}
