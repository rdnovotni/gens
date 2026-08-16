using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Buildings;

/// <summary>
/// The monthly maintenance tick (Phase 6 item 7's "maintenance"): for every completed <see
/// cref="BuildingInstance"/> whose definition authors upkeep lines, resolves this month's upkeep
/// against its Holding's <see cref="Goods.Stockpile"/> — pays it in full when every line is available
/// (consuming that stock) or leaves it unpaid otherwise (no partial payment: an upkeep line is either
/// fully covered or the whole month's upkeep goes unpaid, matching <see
/// cref="ProductionSystem"/>'s identical all-or-nothing rule for a recipe's inputs) — then calls <see
/// cref="BuildingInstance.ApplyUpkeep"/> either way. A Building on a Plot with no resolvable Holding or
/// Stockpile is left untouched this tick: maintenance cannot be resolved without a storage constraint
/// to check it against. Runs before <see cref="ProductionSystem"/> so a Building whose condition drops
/// to <see cref="BuildingCondition.Ruined"/> this month already fails <see
/// cref="BuildingInstance.IsOperational"/> before production for that same month is evaluated.
/// </summary>
public sealed class MaintenanceSystem : IMonthlySystem<WorldState>
{
    public string Id => "buildings.maintenance";
    public TickPhase Phase => TickPhase.Production;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "buildings", "plots", "stockpiles" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "buildings", "stockpiles", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "buildings.construction" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.Buildings.InAscendingOrder().ToArray())
        {
            var building = entry.Value;
            if (building.Definition.Upkeep.Count == 0)
                continue;
            if (!EstateLookup.TryResolveStockpile(state, building.PlotId, out var holdingId, out var stockpile))
                continue;

            var paid = building.Definition.Upkeep
                .All(line => stockpile.AvailableQuantityOf(line.GoodId) >= line.Quantity);
            if (paid)
            {
                foreach (var line in building.Definition.Upkeep)
                    stockpile.ConsumeAvailable(line.GoodId, line.Quantity);
            }

            var previousCondition = building.Condition;
            building.ApplyUpkeep(paid);

            if (building.Condition != previousCondition)
            {
                events.Add(new BuildingUpkeepResolvedEvent(
                    state.EventIds.Issue(), context.Date, building.Id, holdingId, paid, previousCondition, building.Condition));
            }
        }

        return events;
    }
}

/// <summary>Emitted whenever a Building's <see cref="BuildingCondition"/> changes as a result of this
/// month's upkeep resolution (Phase 6 item 7's "maintenance"). Not emitted for a Building whose upkeep
/// was paid (no condition change) or whose condition was already <see
/// cref="BuildingCondition.Ruined"/> before this tick (<see cref="BuildingInstance.ApplyUpkeep"/> is a
/// no-op there).</summary>
public sealed record BuildingUpkeepResolvedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Building> BuildingId,
    RuntimeId<Holding> HoldingId,
    bool Paid,
    BuildingCondition PreviousCondition,
    BuildingCondition NewCondition) : IDomainEvent
{
    public string Type => "buildings.upkeepResolved";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BuildingId.ToTaggedString(), HoldingId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
