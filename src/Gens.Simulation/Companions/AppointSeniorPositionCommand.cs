using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Companions;

/// <summary>
/// Seats a Character in a <see cref="SeniorPositionTitle"/> (Phase 17 item 1; §5-§6) — either directly
/// (a tier-skipping appointment, §6: "there is no requirement to have already held an Overseer seat")
/// or as a promotion out of an existing <see cref="OverseerAssignment"/> (<paramref
/// name="PromotedFromOverseerRecordId"/> set, ending that source record in the same mutate step). There
/// is deliberately no separate <c>PromoteCommand</c>: §6 never requires holding the lower tier first,
/// so a dedicated promotion command would only duplicate this one's own validation for no behavioral
/// difference beyond the provenance field. A promotion is <see cref="Chronicle.ChronicleProjector"/>'s
/// own <see cref="Chronicle.ChronicleTier.Notable"/> material; a direct appointment is quieter <see
/// cref="Chronicle.ChronicleTier.Minor"/> material — the contrast with <see
/// cref="AppointOverseerCommand"/>'s own not-projected-at-all <see cref="OverseerAssignedEvent"/>.
///
/// <see cref="Title"/> can never be <see cref="SeniorPositionTitle.Procurator"/> here — that title's
/// own <see cref="SeniorPositionAssignment.OversightSettlementId"/> requires a target settlement this
/// command's own shape has no field for, so §5.3's Procurator appointment always goes through <see
/// cref="AppointSecondSettlementProcuratorCommand"/> instead, which reuses this file's own <see
/// cref="AppointSeniorPositionCommands.ValidateShared"/> for every check the two commands share.
/// </summary>
public sealed record AppointSeniorPositionCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Household> HouseholdId,
    SeniorPositionTitle Title,
    RuntimeId<Building>? TiedBuildingId,
    RuntimeId<OverseerAssignment>? PromotedFromOverseerRecordId) : ICommand;

/// <summary>Emitted whenever an <see cref="AppointSeniorPositionCommand"/> (or <see
/// cref="AppointSecondSettlementProcuratorCommand"/>) is accepted.</summary>
public sealed record SeniorPositionAssignedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<SeniorPositionAssignment> RecordId,
    RuntimeId<Character> HolderId,
    SeniorPositionTitle Title,
    RuntimeId<Household> HouseholdId,
    RuntimeId<OverseerAssignment>? PromotedFromOverseerRecordId,
    string? CausationId) : IDomainEvent
{
    public string Type => "companions.seniorPositionAssigned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AppointSeniorPositionCommand"/> (ADR 0006), and
/// the shared validation/construction <see cref="AppointSecondSettlementProcuratorCommand"/> reuses.</summary>
public static class AppointSeniorPositionCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("companions.appointSeniorPosition.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("companions.appointSeniorPosition.characterDeceased");
    public static readonly ValidationErrorCode NotHouseholdMember = new("companions.appointSeniorPosition.notHouseholdMember");
    public static readonly ValidationErrorCode IneligibleLifecycleStage = new("companions.appointSeniorPosition.ineligibleLifecycleStage");
    public static readonly ValidationErrorCode ProcuratorRequiresDedicatedCommand =
        new("companions.appointSeniorPosition.procuratorRequiresAppointProcuratorCommand");
    public static readonly ValidationErrorCode TiedBuildingRequired = new("companions.appointSeniorPosition.tiedBuildingRequired");
    public static readonly ValidationErrorCode TiedBuildingNotApplicable = new("companions.appointSeniorPosition.tiedBuildingNotApplicable");
    public static readonly ValidationErrorCode AlreadyHoldsSeniorPosition = new("companions.appointSeniorPosition.alreadyHoldsSeniorPosition");
    public static readonly ValidationErrorCode PositionAlreadyFilled = new("companions.appointSeniorPosition.positionAlreadyFilled");
    public static readonly ValidationErrorCode InsufficientAttributeStanding = new("companions.appointSeniorPosition.insufficientAttributeStanding");
    public static readonly ValidationErrorCode InsufficientLoyalty = new("companions.appointSeniorPosition.insufficientLoyalty");
    public static readonly ValidationErrorCode PromotionSourceNotFound = new("companions.appointSeniorPosition.promotionSourceNotFound");
    public static readonly ValidationErrorCode PromotionSourceNotActive = new("companions.appointSeniorPosition.promotionSourceNotActive");
    public static readonly ValidationErrorCode PromotionSourceHolderMismatch = new("companions.appointSeniorPosition.promotionSourceHolderMismatch");

    public static readonly CommandPipeline<WorldState, AppointSeniorPositionCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AppointSeniorPositionCommand command)
    {
        if (command.Title == SeniorPositionTitle.Procurator)
            return ProcuratorRequiresDedicatedCommand;

        return ValidateShared(
            state, command.CharacterId, command.HouseholdId, command.Title, command.TiedBuildingId,
            command.PromotedFromOverseerRecordId, command.SubmittedDate, checkPositionAlreadyFilled: true);
    }

    /// <summary>Every check <see cref="AppointSeniorPositionCommand"/> and <see
    /// cref="AppointSecondSettlementProcuratorCommand"/> share. <paramref name="checkPositionAlreadyFilled"/> is false
    /// only for <see cref="AppointSecondSettlementProcuratorCommand"/>'s own call: §5.3 allows several concurrent
    /// Procurators per household (one per distant settlement), so that command checks its own
    /// settlement-scoped uniqueness (<see cref="SeniorPositionResolver.ActiveProcuratorRecord"/>)
    /// instead of this method's plain per-<see cref="SeniorPositionTitle"/> gate.</summary>
    internal static ValidationErrorCode? ValidateShared(
        WorldState state,
        RuntimeId<Character> characterId,
        RuntimeId<Household> householdId,
        SeniorPositionTitle title,
        RuntimeId<Building>? tiedBuildingId,
        RuntimeId<OverseerAssignment>? promotedFromOverseerRecordId,
        GameDate submittedDate,
        bool checkPositionAlreadyFilled)
    {
        if (!state.Characters.TryGet(characterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;
        if (character.Household != householdId)
            return NotHouseholdMember;
        if (character.GetLifecycleStage(submittedDate) < LifecycleStage.Adult)
            return IneligibleLifecycleStage;

        var roomTied = CompanionsCatalog.IsRoomTied(title);
        if (roomTied && tiedBuildingId is null)
            return TiedBuildingRequired;
        if (!roomTied && tiedBuildingId is not null)
            return TiedBuildingNotApplicable;

        if (SeniorPositionResolver.ActiveRecordForCharacter(state, characterId) is not null)
            return AlreadyHoldsSeniorPosition;
        if (checkPositionAlreadyFilled && SeniorPositionResolver.ActiveRecordForTitle(state, householdId, title) is not null)
            return PositionAlreadyFilled;

        var attribute = CompanionsCatalog.ValueOf(character.GetEffectiveAttributes(), CompanionsCatalog.AttributeOf(title));
        if (attribute < CompanionsCatalog.SeniorPositionAttributeThreshold)
            return InsufficientAttributeStanding;
        if (character.Condition.Loyalty < CompanionsCatalog.SeniorPositionLoyaltyThreshold)
            return InsufficientLoyalty;

        if (promotedFromOverseerRecordId is { } sourceId)
        {
            if (!state.OverseerAssignments.TryGet(sourceId, out var source))
                return PromotionSourceNotFound;
            if (!OverseerResolver.IsActive(source!))
                return PromotionSourceNotActive;
            if (source!.HolderId != characterId)
                return PromotionSourceHolderMismatch;
        }

        return null;
    }

    /// <summary>Builds the <see cref="SeniorPositionAssignment"/> record itself, shared by this file's
    /// own <see cref="Mutate"/> and <see cref="AppointSecondSettlementProcuratorCommand"/>'s (which supplies a non-null
    /// <paramref name="oversightSettlementId"/> this command's own <see
    /// cref="AppointSeniorPositionCommand"/> shape has no field for).</summary>
    internal static SeniorPositionAssignment BuildRecord(
        RuntimeId<SeniorPositionAssignment> recordId,
        RuntimeId<Character> characterId,
        SeniorPositionTitle title,
        RuntimeId<Household> householdId,
        RuntimeId<Building>? tiedBuildingId,
        RuntimeId<Settlement>? oversightSettlementId,
        GameDate date,
        RuntimeId<OverseerAssignment>? promotedFromOverseerRecordId) =>
        new(
            recordId, characterId, title, householdId, CompanionsCatalog.ScopeOf(title), tiedBuildingId,
            oversightSettlementId, date, PromotedFromOverseerRecordId: promotedFromOverseerRecordId);

    /// <summary>Ends the source <see cref="OverseerAssignment"/> a promotion supersedes, if any — shared
    /// by this file's own <see cref="Mutate"/> and <see cref="AppointSecondSettlementProcuratorCommand"/> (Procurator
    /// promotions are not modeled by §5.3, but the helper is harmless to share regardless).</summary>
    internal static void EndPromotionSource(WorldState state, RuntimeId<OverseerAssignment>? promotedFromOverseerRecordId, GameDate date)
    {
        if (promotedFromOverseerRecordId is not { } sourceId)
            return;

        state.OverseerAssignments.TryGet(sourceId, out var source);
        state.OverseerAssignments.Remove(sourceId);
        state.OverseerAssignments.Add(sourceId, source! with { EndDate = date });
    }

    private static IDomainEvent[] Mutate(WorldState state, AppointSeniorPositionCommand command)
    {
        var recordId = state.SeniorPositionAssignmentIds.Issue();
        state.SeniorPositionAssignments.Add(
            recordId,
            BuildRecord(
                recordId, command.CharacterId, command.Title, command.HouseholdId, command.TiedBuildingId,
                oversightSettlementId: null, command.SubmittedDate, command.PromotedFromOverseerRecordId));

        EndPromotionSource(state, command.PromotedFromOverseerRecordId, command.SubmittedDate);

        return new IDomainEvent[]
        {
            new SeniorPositionAssignedEvent(
                state.EventIds.Issue(), command.SubmittedDate, recordId, command.CharacterId, command.Title,
                command.HouseholdId, command.PromotedFromOverseerRecordId, command.CommandId.ToTaggedString()),
        };
    }
}
