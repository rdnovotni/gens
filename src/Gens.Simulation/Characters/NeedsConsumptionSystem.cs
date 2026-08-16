using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>
/// The monthly needs-demand/consumption tick (Roadmap Phase 7 item 4's first hook;
/// <c>gens-settlement-demographics-design.md</c> §6.1). For every <see cref="PopGroup"/>, reports its
/// <see cref="NeedsConsumptionCalculator.ComputeDemand"/> grain-equivalent demand — the background
/// population's demand-side contribution to a Resources &amp; Goods market that doesn't exist yet
/// (Phase 8), so this system only measures and reports; it does not debit any Stockpile. Purely
/// read-derived like <see cref="BackgroundJobCapacityComputedEvent"/> and <see
/// cref="EmploymentMatchedEvent"/> before it — nothing here is persisted beyond the "always emitted"
/// event.
/// </summary>
public sealed class NeedsConsumptionSystem : IMonthlySystem<WorldState>
{
    public string Id => "characters.needsConsumption";
    public TickPhase Phase => TickPhase.EmploymentNeeds;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "settlements", "popGroups" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "eventIds" };
    // Chained after EmploymentMatchingSystem purely to match the roadmap's stated dependency order
    // for Phase 7 item 4's systems (job capacity -> employment matching -> needs/consumption ->
    // housing capacity -> contentment -> ...); this system's own math never actually reads employment
    // data.
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "characters.employmentMatching" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var settlementEntry in state.Settlements.InAscendingOrder())
        {
            var settlementId = settlementEntry.Key;
            var lines = new List<NeedsConsumptionLine>();

            foreach (var popGroupEntry in state.PopGroups.InAscendingOrder())
            {
                if (popGroupEntry.Key.SettlementId != settlementId)
                    continue;
                var group = popGroupEntry.Value;
                var demand = NeedsConsumptionCalculator.ComputeDemand(group.NeedsProfile, group.Size);
                lines.Add(new NeedsConsumptionLine(group.GroupType, NeedsConsumptionCalculator.ConsumptionGood, demand));
            }

            events.Add(new NeedsConsumptionComputedEvent(state.EventIds.Issue(), context.Date, settlementId, lines));
        }

        return events;
    }
}

/// <summary>One Settlement's computed monthly grain-equivalent demand for one pop group (§6.1), part
/// of <see cref="NeedsConsumptionComputedEvent"/>.</summary>
public sealed record NeedsConsumptionLine(PopGroupType GroupType, DefinitionId<Good> GoodId, long Quantity);

/// <summary>Emitted for every Settlement, every month, whether or not any group's PopGroups changed —
/// ledger-ready groundwork for Phase 8's market to eventually consume, matching <see
/// cref="BackgroundJobCapacityComputedEvent"/>'s identical "always emitted" convention.</summary>
public sealed record NeedsConsumptionComputedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Settlement> SettlementId,
    IReadOnlyList<NeedsConsumptionLine> Lines) : IDomainEvent
{
    public string Type => "characters.needsConsumptionComputed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
