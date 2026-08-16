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
///
/// The actual sector-summing and Curiales math lives in <see cref="BackgroundJobCapacityCalculator"/>
/// (factored out for Phase 7 item 3) so <see cref="EmploymentMatchingSystem"/> can read the same exact
/// slot counts this system uses, rather than back-deriving them from <see
/// cref="PopGroup.EmploymentRatio"/>'s rounded ratio.
/// </summary>
public sealed class JobCapacitySystem : IMonthlySystem<WorldState>
{
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
        var capacityBySettlement = BackgroundJobCapacityCalculator.SumBuildingCapacityBySettlement(state);

        foreach (var settlementEntry in state.Settlements.InAscendingOrder().ToArray())
        {
            var settlementId = settlementEntry.Key;
            var settlement = settlementEntry.Value;
            capacityBySettlement.TryGetValue(settlementId, out var sectorCapacity);

            var totalPopulation = 0;
            foreach (var popGroupEntry in state.PopGroups.InAscendingOrder())
            {
                if (popGroupEntry.Key.SettlementId == settlementId)
                    totalPopulation += popGroupEntry.Value.Size;
            }

            // Every sector-driven group always gets an entry, even at zero — a settlement that lost
            // its last Agriculture building must see Coloni capacity drop to zero, not silently keep
            // whatever ratio a now-vanished building last produced.
            var capacityByGroup = BackgroundJobCapacityCalculator.ComputeCapacityByGroup(
                sectorCapacity, totalPopulation, settlement.Stage);

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
}

/// <summary>One Settlement's computed background job capacity for one pop group this month (§4.1/§4.2),
/// part of <see cref="BackgroundJobCapacityComputedEvent"/>.</summary>
public sealed record BackgroundJobCapacityLine(PopGroupType GroupType, int AvailableSlots);

/// <summary>Emitted for every Settlement, every month, whether or not any capacity changed — ledger-ready
/// groundwork <see cref="EmploymentMatchingSystem"/> (Phase 7 item 3) builds on, matching <see
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
