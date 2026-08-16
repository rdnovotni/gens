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
            if (queue.Projects.Count == 0)
                continue;
            var head = queue.Projects[0];

            var laborAvailable = EstateLookup.HasLaborAvailable(state, holdingId);
            var completed = queue.AdvanceMonth(laborAvailable);
            if (completed is not null)
            {
                var buildingId = state.BuildingIds.Issue();
                var building = new BuildingInstance(buildingId, completed.PlotId, completed.Definition);
                state.Buildings.Add(buildingId, building);

                events.Add(new BuildingConstructionCompletedEvent(
                    state.EventIds.Issue(), context.Date, buildingId, holdingId, completed.PlotId, completed.Definition.Id));
                continue;
            }

            if (!laborAvailable)
                continue;

            // Advanced by one month without finishing: report the same labor-consumption fact a
            // completion event reports, just short of the terminal month (Phase 6 item 8's "construction
            // events even before the economy consumes them").
            var progressed = queue.Projects[0];
            events.Add(new BuildingConstructionProgressedEvent(
                state.EventIds.Issue(), context.Date, holdingId, head.PlotId, head.Definition.Id,
                progressed.CompletedMonths, head.Definition.ConstructionMonths));
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

/// <summary>Emitted whenever a <see cref="ConstructionSchedule"/> head project consumes a month of labor
/// without yet reaching completion (Phase 6 item 8's "emit complete ledger-ready... construction...
/// events even before the economy consumes them" — a ledger consumer needs to see labor committed to an
/// unfinished project every month, not only the month it finally finishes). Not emitted the month a
/// project completes (<see cref="BuildingConstructionCompletedEvent"/> covers that month instead) or a
/// month <see cref="EstateLookup.HasLaborAvailable"/> is <see langword="false"/> (the queue paused; no
/// labor moved).</summary>
public sealed record BuildingConstructionProgressedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Holding> HoldingId,
    RuntimeId<Plot> PlotId,
    DefinitionId<Building> DefinitionId,
    int CompletedMonths,
    int TotalMonths) : IDomainEvent
{
    public string Type => "buildings.constructionProgressed";
    public int SchemaVersion => 1;

    public IReadOnlyList<string> SubjectIds => new[] { HoldingId.ToTaggedString(), PlotId.ToTaggedString() };

    public Visibility Visibility => Visibility.Public;
    public string? CausationId => null;
}
