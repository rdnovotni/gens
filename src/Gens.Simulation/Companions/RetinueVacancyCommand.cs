using System.Linq;
#nullable enable
using System;
using Gens.Simulation.State;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Companions;

/// <summary>
/// Marks and clears <see cref="OverseerAssignment.OnLeaveSince"/>/<see
/// cref="SeniorPositionAssignment.OnLeaveSince"/> for a <see cref="TravelParty"/>'s retinue (Phase 17
/// item 1; §7's "a retinue member's Court Position back home is temporarily unstaffed while they
/// travel"). Not a full <see cref="Commands.ICommand"/>: unlike <see cref="AppointOverseerCommand"/> or
/// its siblings, this has no independent validation of its own to fail — it is a same-transaction side
/// effect of two commands/systems that already validated everything relevant (<see
/// cref="Travel.BeginTravelCommand"/>'s own Validate already confirms every party member exists and is
/// alive; <see cref="Travel.TravelProgressSystem"/>'s return leg needs no validation at all), so a
/// bare static-method pair is this codebase's own precedent for a shared mutation with nothing to
/// reject (matching this codebase's own "an internal helper doesn't need command ceremony" shape,
/// e.g. <see cref="Stewardship.StewardshipCommands"/>'s own private return-report builder).
/// </summary>
public static class RetinueVacancyCommands
{
    /// <summary>Sets <c>OnLeaveSince</c> on every retinue member's currently active Overseer/Senior
    /// Position — called from <see cref="Travel.BeginTravelCommands.Mutate"/>.</summary>
    public static void MarkOnLeave(WorldState state, TravelParty party, Time.GameDate date)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (party is null)
            throw new ArgumentNullException(nameof(party));

        foreach (var memberId in party.RetinueIds)
        {
            if (OverseerResolver.ActiveRecordForCharacter(state, memberId) is { OnLeaveSince: null } overseer)
            {
                state.OverseerAssignments.Remove(overseer.RecordId);
                state.OverseerAssignments.Add(overseer.RecordId, overseer with { OnLeaveSince = date });
            }

            if (SeniorPositionResolver.ActiveRecordForCharacter(state, memberId) is { OnLeaveSince: null } senior)
            {
                state.SeniorPositionAssignments.Remove(senior.RecordId);
                state.SeniorPositionAssignments.Add(senior.RecordId, senior with { OnLeaveSince = date });
            }
        }
    }

    /// <summary>Clears <c>OnLeaveSince</c> back to <c>null</c> for every retinue member's currently
    /// active Overseer/Senior Position — called from <see
    /// cref="Travel.TravelProgressSystem"/>'s return-leg completion.</summary>
    public static void ClearOnLeave(WorldState state, TravelParty party)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));
        if (party is null)
            throw new ArgumentNullException(nameof(party));

        foreach (var memberId in party.RetinueIds)
        {
            if (OverseerResolver.ActiveRecordForCharacter(state, memberId) is { OnLeaveSince: not null } overseer)
            {
                state.OverseerAssignments.Remove(overseer.RecordId);
                state.OverseerAssignments.Add(overseer.RecordId, overseer with { OnLeaveSince = null });
            }

            if (SeniorPositionResolver.ActiveRecordForCharacter(state, memberId) is { OnLeaveSince: not null } senior)
            {
                state.SeniorPositionAssignments.Remove(senior.RecordId);
                state.SeniorPositionAssignments.Add(senior.RecordId, senior with { OnLeaveSince = null });
            }
        }
    }
}
