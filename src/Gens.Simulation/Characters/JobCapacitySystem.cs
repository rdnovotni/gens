using Gens.Simulation.Buildings;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Numerics;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly job-capacity tick (Phase 7 item 2; <c>gens-settlement-demographics-design.md</c> §4.1
/// "Where Jobs Come From" and §4.2 "The Employment Ratio"). For every Settlement, sums the <see
/// cref="BuildingDefinition.BackgroundJobCapacity"/> of every completed, non-ruined Building on one of
/// its Plots by <see cref="BuildingDefinition.Sector"/> — Agriculture feeds Coloni, Industry feeds
/// Opifices, Commerce feeds Negotiatores <em>and</em> a matching Operarii baseline (§4.1's "plus a
/// baseline of Operarii"), Religion feeds Aeditui — then adds a population/stage-driven Curiales figure
/// on top, since §4.1 places Curiales capacity outside any single building category. Deliberately does
/// not require <see cref="BuildingInstance.IsOperational"/> (fully staffed): §4.1's whole point is that
/// background pops are <em>not</em> staffing the player's own buildings, so a building's mere completed
/// presence — not its own staffing state — is what creates background job openings.
///
/// Writes the result straight into each existing <see cref="PopGroup.EmploymentRatio"/> (§4.2's
/// <c>slots / size</c>) rather than persisting a separate capacity partition: every input (Buildings,
/// Plots, Settlements, PopGroups) is already authoritative state, so the capacity figure is fully
/// re-derivable each tick and does not need its own save-format entry. A group's capacity is always
/// recomputed — including down to zero once its last contributing Building is lost to <see
/// cref="BuildingCondition.Ruined"/> — but only actually written when a <see cref="PopGroup"/> already
/// exists for that (settlement, group type) pair; this system never creates one. <see
/// cref="BackgroundJobCapacityComputedEvent"/> still records the computed slot counts per settlement,
/// ledger-ready, matching <see cref="Buildings.ProductionResolvedEvent"/>'s "always emit" convention.
///
/// Two deliberate simplifications, left for a later Phase 7 item: Operarii's own §3 description also
/// names Industry and "Civic construction" as background-job sources alongside Commerce, but Industry
/// already maps fully to Opifices and no Civic-building/construction-labor category exists in code yet
/// — only the Commerce baseline is implemented. And Curiales' formula uses <see
/// cref="Settlement.Stage"/> alone rather than "Dignitas/stage" (§4.1): no Dignitas field exists yet
/// anywhere in this codebase (open question P35's cross-reference), so Stage is the only half of that
/// pair currently available.
/// </summary>
public sealed class JobCapacitySystem : IMonthlySystem<WorldState>
{
    /// <summary>Curiales capacity as a fraction of total settlement population, in basis points (parts
    /// per 1,000), rising with <see cref="SettlementStage"/> — a modest, growing pool of upward-mobility
    /// slots as a settlement matures, standing in for "Dignitas/stage" until Dignitas exists.</summary>
    private static readonly IReadOnlyDictionary<SettlementStage, int> CurialesBasisPoints =
        new Dictionary<SettlementStage, int>
        {
            [SettlementStage.Villa] = 20,
            [SettlementStage.Vicus] = 30,
            [SettlementStage.Town] = 50,
            [SettlementStage.City] = 80,
        };

    public string Id => "characters.jobCapacity";
    public TickPhase Phase => TickPhase.EmploymentNeeds;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "buildings", "plots", "settlements", "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "popGroups", "eventIds" };
    // TickPhase.EmploymentNeeds always runs after TickPhase.Production completes (phase ordering
    // alone guarantees this, matching LaborOutputSystem's identical reasoning) — no same-phase
    // prerequisite is needed for this month's completed Buildings to already be on record.
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();
        var capacityBySettlement = SumBuildingCapacityBySettlement(state);

        foreach (var settlementEntry in state.Settlements.InAscendingOrder().ToArray())
        {
            var settlementId = settlementEntry.Key;
            var settlement = settlementEntry.Value;
            capacityBySettlement.TryGetValue(settlementId, out var sectorCapacity);

            // Every sector-driven group always gets an entry, even at zero — a settlement that lost
            // its last Agriculture building must see Coloni capacity drop to zero, not silently keep
            // whatever ratio a now-vanished building last produced.
            var capacityByGroup = new Dictionary<PopGroupType, int>
            {
                [PopGroupType.Coloni] = 0,
                [PopGroupType.Operarii] = 0,
                [PopGroupType.Opifices] = 0,
                [PopGroupType.Negotiatores] = 0,
                [PopGroupType.Aeditui] = 0,
            };
            if (sectorCapacity is not null)
            {
                foreach (var (sector, slots) in sectorCapacity)
                {
                    if (PrimaryGroupFor(sector) is { } primaryGroup)
                        AddCapacity(capacityByGroup, primaryGroup, slots);
                    if (sector == BuildingSector.Commerce)
                        AddCapacity(capacityByGroup, PopGroupType.Operarii, slots);
                }
            }

            var totalPopulation = 0;
            foreach (var popGroupEntry in state.PopGroups.InAscendingOrder())
            {
                if (popGroupEntry.Key.SettlementId == settlementId)
                    totalPopulation += popGroupEntry.Value.Size;
            }
            capacityByGroup[PopGroupType.Curiales] = CurialesCapacity(totalPopulation, settlement.Stage);

            foreach (var (groupType, capacity) in capacityByGroup)
            {
                var key = new PopGroupKey(settlementId, groupType);
                if (!state.PopGroups.TryGet(key, out var popGroup))
                    continue;

                var ratio = popGroup.Size > 0
                    ? Fixed64.Divide(Fixed64.FromInt(capacity), Fixed64.FromInt(popGroup.Size))
                    : Fixed64.One;
                if (ratio == popGroup.EmploymentRatio)
                    continue;

                state.PopGroups.Remove(key);
                state.PopGroups.Add(key, popGroup with { EmploymentRatio = ratio });
            }

            var lines = capacityByGroup
                .OrderBy(pair => pair.Key)
                .Select(pair => new BackgroundJobCapacityLine(pair.Key, pair.Value))
                .ToArray();
            events.Add(new BackgroundJobCapacityComputedEvent(state.EventIds.Issue(), context.Date, settlementId, lines));
        }

        return events;
    }

    private static Dictionary<RuntimeId<Settlement>, Dictionary<BuildingSector, int>> SumBuildingCapacityBySettlement(WorldState state)
    {
        var result = new Dictionary<RuntimeId<Settlement>, Dictionary<BuildingSector, int>>();

        foreach (var entry in state.Buildings.InAscendingOrder())
        {
            var building = entry.Value;
            if (building.Condition == BuildingCondition.Ruined)
                continue;
            if (building.Definition.Sector == BuildingSector.None || building.Definition.BackgroundJobCapacity == 0)
                continue;
            if (!state.Plots.TryGet(building.PlotId, out var plot))
                continue;

            if (!result.TryGetValue(plot.SettlementId, out var bySector))
            {
                bySector = new Dictionary<BuildingSector, int>();
                result[plot.SettlementId] = bySector;
            }

            AddCapacity(bySector, building.Definition.Sector, building.Definition.BackgroundJobCapacity);
        }

        return result;
    }

    private static void AddCapacity<TKey>(Dictionary<TKey, int> capacity, TKey key, int slots) where TKey : notnull
    {
        capacity.TryGetValue(key, out var existing);
        capacity[key] = existing + slots;
    }

    private static PopGroupType? PrimaryGroupFor(BuildingSector sector) => sector switch
    {
        BuildingSector.Agriculture => PopGroupType.Coloni,
        BuildingSector.Industry => PopGroupType.Opifices,
        BuildingSector.Commerce => PopGroupType.Negotiatores,
        BuildingSector.Religion => PopGroupType.Aeditui,
        _ => null,
    };

    private static int CurialesCapacity(int totalPopulation, SettlementStage stage) =>
        (int)((long)totalPopulation * CurialesBasisPoints[stage] / 1000);
}

/// <summary>One Settlement's computed background job capacity for one pop group this month (§4.1/§4.2),
/// part of <see cref="BackgroundJobCapacityComputedEvent"/>.</summary>
public sealed record BackgroundJobCapacityLine(PopGroupType GroupType, int AvailableSlots);

/// <summary>Emitted for every Settlement, every month, whether or not any capacity changed — ledger-ready
/// groundwork for the employment-matching system Phase 7 item 3 will add, matching <see
/// cref="Buildings.ProductionResolvedEvent"/>'s and <see cref="LaborOutputComputedEvent"/>'s identical
/// "always emitted" convention.</summary>
public sealed record BackgroundJobCapacityComputedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    IReadOnlyList<BackgroundJobCapacityLine> Capacities) : IDomainEvent
{
    public string Type => "characters.backgroundJobCapacityComputed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
