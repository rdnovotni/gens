using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Buildings;

/// <summary>
/// The monthly construction tick (Phase 6 item 7's "one construction queue"): advances every
/// Holding's <see cref="ConstructionSchedule"/> head project by one month (gated on <see
/// cref="EstateLookup.HasLaborAvailable"/>, matching <see cref="ConstructionSchedule.AdvanceMonth"/>'s
/// own "pauses without labor, resumes without losing progress" contract), and on completion adds the
/// finished <see cref="BuildingInstance"/> to <see cref="WorldState.Buildings"/>. Deliberately runs
/// first among this item's three systems (no same-phase prerequisite needed — nothing else in
/// <see cref="TickPhase.Production"/> writes <c>constructionSchedules</c>) so a building finished this
/// tick is already on record before <see cref="MaintenanceSystem"/> and <see cref="ProductionSystem"/>
/// run.
/// </summary>
public sealed class ConstructionSystem : IMonthlySystem<WorldState>
{
    public string Id => "buildings.construction";
    public TickPhase Phase => TickPhase.Production;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "constructionSchedules", "holdings", "characters" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "constructionSchedules", "buildings", "buildingIds", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: ConstructionSchedule is a mutable class mutated in place below, but the
        // registry itself is not structurally changed by this loop, matching the same defensive
        // precaution LaborOutputSystem/CharacterLifecycleSystem take for partitions they do mutate.
        var queues = state.ConstructionSchedules.InAscendingOrder().ToArray();

        foreach (var entry in queues)
        {
            var holdingId = entry.Key;
            var queue = entry.Value;
            var laborAvailable = EstateLookup.HasLaborAvailable(state, holdingId);
            var completed = queue.AdvanceMonth(laborAvailable);
            if (completed is null)
                continue;

            var buildingId = state.BuildingIds.Issue();
            var building = new BuildingInstance(buildingId, completed.PlotId, completed.Definition);
            state.Buildings.Add(buildingId, building);

            events.Add(new BuildingConstructionCompletedEvent(
                state.EventIds.Issue(), context.Date, buildingId, holdingId, completed.PlotId, completed.Definition.Id));
        }

        return events;
    }
}

/// <summary>Emitted whenever a <see cref="ConstructionSchedule"/> head project finishes its final month
/// and the resulting <see cref="BuildingInstance"/> is added to <see cref="WorldState.Buildings"/>
/// (Phase 6 item 7). This is the "construction" half of Phase 6 item 8's "emit complete ledger-ready
/// production, consumption, construction, and labor events."</summary>
public sealed record BuildingConstructionCompletedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Building> BuildingId,
    RuntimeId<Holding> HoldingId,
    RuntimeId<Plot> PlotId,
    DefinitionId<Building> DefinitionId) : IDomainEvent
{
    public string Type => "buildings.constructionCompleted";
    public int SchemaVersion => 1;

    public IReadOnlyList<string> SubjectIds => new[]
    {
        BuildingId.ToTaggedString(), HoldingId.ToTaggedString(), PlotId.ToTaggedString(),
    };

    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
