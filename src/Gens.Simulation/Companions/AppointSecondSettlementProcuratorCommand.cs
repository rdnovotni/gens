using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Stewardship;
using Gens.Simulation.Time;

namespace Gens.Simulation.Companions;

/// <summary>
/// Seats a Character as <see cref="SeniorPositionTitle.Procurator"/> over a second, distant settlement
/// (Phase 17 item 1; §5.3) and, in the same mutate step, appoints them as that settlement's <see
/// cref="StewardshipContext.SecondSettlementProcurator"/> <see cref="StewardshipAssignment"/> — §5.3's
/// "a genuine Court Position with a genuine Stewardship link, not two unrelated facts that happen to
/// share a Character." The two records are linked implicitly by <c>(HouseholdId, Context,
/// AppointeeCharacterId)</c> (<see cref="ProcuratorLink"/>), no new join field. §5.3's own "a weak or
/// disloyal Procurator... must surface to the player" is already <see
/// cref="Stewardship.StewardIncidentCatalog"/>'s job once this command creates the underlying <see
/// cref="StewardshipAssignment"/> — no new incident mechanism is added here.
/// </summary>
public sealed record AppointSecondSettlementProcuratorCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> TargetSettlementId) : ICommand;

/// <summary>Reusable, non-<see cref="ICommand"/> linkage between a <see cref="SeniorPositionTitle.Procurator"/>
/// <see cref="SeniorPositionAssignment"/> and the <see cref="StewardshipAssignment"/> <see
/// cref="AppointSecondSettlementProcuratorCommand"/> created alongside it — read by <see
/// cref="VacateSeniorPositionCommand"/> and <see cref="PositionVacancySystem"/>, the two places a
/// Procurator's own record can end.</summary>
public static class ProcuratorLink
{
    /// <summary>The active <see cref="StewardshipAssignment"/> linked to <paramref name="procurator"/>,
    /// if any — matched by <c>(HouseholdId, Context, AppointeeCharacterId)</c> per this class's own doc
    /// comment, since no dedicated join field exists.</summary>
    public static StewardshipAssignment? ActiveLinkedAssignment(WorldState state, SeniorPositionAssignment procurator)
    {
        foreach (var entry in state.StewardshipAssignments.InAscendingOrder())
        {
            var assignment = entry.Value;
            if (assignment.IsActive &&
                assignment.HouseholdId == procurator.HouseholdId &&
                assignment.Context == StewardshipContext.SecondSettlementProcurator &&
                assignment.AppointeeCharacterId == procurator.HolderId)
                return assignment;
        }

        return null;
    }

    /// <summary>Ends <paramref name="procurator"/>'s linked <see cref="StewardshipAssignment"/>, if
    /// still active, mirroring <see cref="StewardshipCommands.MutateEnd"/>'s own replace-with-<see
    /// cref="StewardshipAssignment.EndDate"/> mutation minus its <see cref="ReturnReport"/> generation
    /// (see <see cref="VacateSeniorPositionCommand"/>'s own doc comment for why).</summary>
    public static IReadOnlyList<IDomainEvent> EndLinkedStewardship(
        WorldState state, SeniorPositionAssignment procurator, GameDate date, string? causationId)
    {
        var assignment = ActiveLinkedAssignment(state, procurator);
        if (assignment is null)
            return Array.Empty<IDomainEvent>();

        state.StewardshipAssignments.Remove(assignment.AssignmentId);
        state.StewardshipAssignments.Add(assignment.AssignmentId, assignment with { EndDate = date });

        return new IDomainEvent[]
        {
            new StewardshipEndedEvent(state.EventIds.Issue(), date, assignment.AssignmentId, assignment.HouseholdId, causationId),
        };
    }
}

/// <summary>The validate/mutate pipeline for <see cref="AppointSecondSettlementProcuratorCommand"/> (ADR 0006).</summary>
public static class AppointSecondSettlementProcuratorCommands
{
    public static readonly ValidationErrorCode TargetSettlementIsHome = new("companions.appointProcurator.targetSettlementIsHome");
    public static readonly ValidationErrorCode TargetAlreadyHasProcurator = new("companions.appointProcurator.targetAlreadyHasProcurator");

    public static readonly CommandPipeline<WorldState, AppointSecondSettlementProcuratorCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AppointSecondSettlementProcuratorCommand command)
    {
        var shared = AppointSeniorPositionCommands.ValidateShared(
            state, command.CharacterId, command.HouseholdId, SeniorPositionTitle.Procurator, tiedBuildingId: null,
            promotedFromOverseerRecordId: null, command.SubmittedDate, checkPositionAlreadyFilled: false);
        if (shared is { } sharedError)
            return sharedError;

        state.Characters.TryGet(command.CharacterId, out var character);
        if (character!.Location == command.TargetSettlementId)
            return TargetSettlementIsHome;
        if (SeniorPositionResolver.ActiveProcuratorRecord(state, command.HouseholdId, command.TargetSettlementId) is not null)
            return TargetAlreadyHasProcurator;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AppointSecondSettlementProcuratorCommand command)
    {
        var recordId = state.SeniorPositionAssignmentIds.Issue();
        var record = AppointSeniorPositionCommands.BuildRecord(
            recordId, command.CharacterId, SeniorPositionTitle.Procurator, command.HouseholdId, tiedBuildingId: null,
            oversightSettlementId: command.TargetSettlementId, command.SubmittedDate, promotedFromOverseerRecordId: null);
        state.SeniorPositionAssignments.Add(recordId, record);

        var assignmentId = state.StewardshipAssignmentIds.Issue();
        var assignment = StewardshipAssignment.Create(
            assignmentId, command.HouseholdId, StewardshipContext.SecondSettlementProcurator, StewardshipMode.SingleSteward,
            command.CharacterId, councilMembers: null, councilHeadCharacterId: null,
            StewardshipAssignment.DefaultAutonomyLevel, command.SubmittedDate);
        state.StewardshipAssignments.Add(assignmentId, assignment);

        return new IDomainEvent[]
        {
            new SeniorPositionAssignedEvent(
                state.EventIds.Issue(), command.SubmittedDate, recordId, command.CharacterId, SeniorPositionTitle.Procurator,
                command.HouseholdId, PromotedFromOverseerRecordId: null, command.CommandId.ToTaggedString()),
            new StewardshipAssignedEvent(
                state.EventIds.Issue(), command.SubmittedDate, assignmentId, command.HouseholdId,
                StewardshipContext.SecondSettlementProcurator, command.CommandId.ToTaggedString()),
        };
    }
}
