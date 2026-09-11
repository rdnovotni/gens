using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Companions;

/// <summary>
/// The monthly Companions &amp; Court Positions vacancy tick (Phase 17 item 1): ends any active <see
/// cref="OverseerAssignment"/> or <see cref="SeniorPositionAssignment"/> whose holder died or left the
/// household — no loss reason, a vacancy, mirroring <see
/// cref="Magistracies.MagistracyTermSystem"/>'s own "a dead Character can't meaningfully continue
/// holding a seat" addition. A <see cref="SeniorPositionTitle.Procurator"/> vacated this way also ends
/// its linked <see cref="Stewardship.StewardshipAssignment"/> (<see cref="ProcuratorLink"/>), the same
/// cleanup <see cref="VacateSeniorPositionCommand"/> performs for a deliberate vacate. Runs in <see
/// cref="TickPhase.RelationshipsActors"/>, the same phase every other office-holding tick in this
/// codebase runs in, and before <see cref="RationalisBonusSystem"/> (its own <see
/// cref="IMonthlySystem{TState}.Prerequisites"/>) so that system sees this month's vacancies rather than
/// a stale, already-vacated holder.
/// </summary>
public sealed class PositionVacancySystem : IMonthlySystem<WorldState>
{
    public string Id => "companions.positionVacancy";
    public TickPhase Phase => TickPhase.RelationshipsActors;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "overseerAssignments", "seniorPositionAssignments", "characters", "stewardshipAssignments" };
    public IReadOnlyCollection<string> Writes { get; } = new[] { "overseerAssignments", "seniorPositionAssignments", "stewardshipAssignments", "eventIds" };
    public IReadOnlyCollection<string> Prerequisites { get; } = Array.Empty<string>();

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        foreach (var entry in state.OverseerAssignments.InAscendingOrder().Where(e => OverseerResolver.IsActive(e.Value)).ToArray())
        {
            var record = entry.Value;
            if (!IsVacated(state, record.HolderId, record.HouseholdId))
                continue;

            state.OverseerAssignments.Remove(record.RecordId);
            state.OverseerAssignments.Add(record.RecordId, record with { EndDate = context.Date });
            events.Add(new OverseerVacatedEvent(state.EventIds.Issue(), context.Date, record.RecordId, record.HolderId, record.Role, CausationId: null));
        }

        foreach (var entry in state.SeniorPositionAssignments.InAscendingOrder().Where(e => SeniorPositionResolver.IsActive(e.Value)).ToArray())
        {
            var record = entry.Value;
            if (!IsVacated(state, record.HolderId, record.HouseholdId))
                continue;

            state.SeniorPositionAssignments.Remove(record.RecordId);
            state.SeniorPositionAssignments.Add(record.RecordId, record with { EndDate = context.Date });
            events.Add(new SeniorPositionVacatedEvent(state.EventIds.Issue(), context.Date, record.RecordId, record.HolderId, record.Title, CausationId: null));

            if (record.Title == SeniorPositionTitle.Procurator)
                events.AddRange(ProcuratorLink.EndLinkedStewardship(state, record, context.Date, causationId: null));
        }

        return events;
    }

    private static bool IsVacated(WorldState state, RuntimeId<Character> holderId, RuntimeId<Household> householdId)
    {
        state.Characters.TryGet(holderId, out var holder);
        return holder is null || !holder.IsAlive || holder.Household != householdId;
    }
}
