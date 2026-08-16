using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly housing/land-capacity tick (Roadmap Phase 7 item 4;
/// <c>gens-settlement-demographics-design.md</c> §7). For every Settlement, computes a <see
/// cref="HousingAndLandCapacity"/> (urban from completed, non-ruined Buildings' <see
/// cref="Buildings.BuildingDefinition.ResidentialCapacity"/>; rural from unclaimed Fertile-Plain Plots,
/// §7.2) and writes each affected <see cref="PopGroup"/>'s <see cref="PopGroup.HousingSatisfaction"/>
/// from it, mirroring <see cref="JobCapacitySystem"/>'s "always recomputed, only written when a
/// PopGroup already exists" shape. <see cref="PopGroupType.NonHouseholdEnslaved"/> is left untouched:
/// its housing is its owner household's concern, not this ambient background-population system's
/// (§9's "doesn't get its own... mechanic" carve-out, same as <see
/// cref="EmploymentMatchingSystem"/>'s wage table already excludes it).
/// </summary>
public sealed class HousingCapacitySystem : IMonthlySystem<WorldState>
{
    public string Id => "characters.housingCapacity";
    public TickPhase Phase => TickPhase.EmploymentNeeds;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "buildings", "plots", "settlements", "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "popGroups", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.needsConsumption" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var residentialCapacityBySettlement = HousingCapacityCalculator.SumResidentialCapacityBySettlement(state);

        foreach (var settlementEntry in state.Settlements.InAscendingOrder().ToArray())
        {
            var settlementId = settlementEntry.Key;
            residentialCapacityBySettlement.TryGetValue(settlementId, out var urbanCapacity);
            var ruralCapacity = HousingCapacityCalculator.ComputeRuralCapacity(state, settlementId);

            var groupsHere = state.PopGroups.InAscendingOrder()
                .Where(entry => entry.Key.SettlementId == settlementId)
                .ToDictionary(entry => entry.Key.GroupType, entry => entry.Value.Size);

            var urbanPopulation = HousingCapacityCalculator.UrbanGroups.Sum(type => groupsHere.GetValueOrDefault(type));
            var ruralPopulation = HousingCapacityCalculator.RuralGroups.Sum(type => groupsHere.GetValueOrDefault(type));
            var urbanSatisfaction = HousingCapacityCalculator.SharedSatisfaction(urbanCapacity, urbanPopulation);
            var ruralSatisfaction = HousingCapacityCalculator.SharedSatisfaction(ruralCapacity, ruralPopulation);

            WriteSatisfaction(state, settlementId, HousingCapacityCalculator.UrbanGroups, urbanSatisfaction);
            WriteSatisfaction(state, settlementId, HousingCapacityCalculator.RuralGroups, ruralSatisfaction);

            events.Add(new HousingCapacityComputedEvent(
                state.EventIds.Issue(), context.Date, settlementId, urbanCapacity, ruralCapacity, urbanSatisfaction, ruralSatisfaction));
        }

        return events;
    }

    private static void WriteSatisfaction(
        WorldState state, RuntimeId<Settlement> settlementId, IReadOnlyList<PopGroupType> groupTypes, Fixed64 satisfaction)
    {
        foreach (var groupType in groupTypes)
        {
            var key = new PopGroupKey(settlementId, groupType);
            if (!state.PopGroups.TryGet(key, out var group) || group.HousingSatisfaction == satisfaction)
                continue;

            state.PopGroups.Remove(key);
            state.PopGroups.Add(key, group with { HousingSatisfaction = satisfaction });
        }
    }
}

/// <summary>Emitted for every Settlement, every month, whether or not capacity changed — ledger-ready
/// groundwork matching <see cref="BackgroundJobCapacityComputedEvent"/>'s identical "always emitted"
/// convention.</summary>
public sealed record HousingCapacityComputedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    int UrbanCapacity,
    int RuralCapacity,
    Fixed64 UrbanSatisfaction,
    Fixed64 RuralSatisfaction) : IDomainEvent
{
    public string Type => "characters.housingCapacityComputed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
