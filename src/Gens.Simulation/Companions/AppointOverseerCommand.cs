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
/// Seats a Character as the named <see cref="OverseerRole"/> at one <see cref="Buildings.BuildingInstance"/>
/// (Phase 17 item 1; §6). §6 frames this tier's bar as purely qualitative ("a modest standing check, no
/// grand ceremony"); this command turns that into concrete gates against real <see cref="Character"/>
/// fields — <see cref="Character.GetEffectiveAttributes"/> and <see cref="Condition.Loyalty"/>, the same
/// fields <see cref="Stewardship.StewardIncidentCatalog.LoyaltyRiskThreshold"/> already reads the same
/// way. §6 explicitly allows skipping straight to a Senior Position without ever holding an Overseer
/// seat first, so this command has no prior-<see cref="DutyAssignment"/>-or-Overseer tenure requirement,
/// only present standing. §3's "legal status never gates which tier someone can reach" means this
/// command deliberately carries no citizenship check, unlike <see
/// cref="Magistracies.AppointDecurionCommand"/>'s own <c>IneligibleLegalStatus</c>.
/// </summary>
public sealed record AppointOverseerCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Building> BuildingId,
    OverseerRole Role) : ICommand;

/// <summary>Emitted whenever an <see cref="AppointOverseerCommand"/> is accepted. Not projected to the
/// Dynasty Chronicle — §6 frames Overseer as a working mid-tier role, not the narrative material a
/// promotion to full Court standing is (see <see cref="AppointSeniorPositionCommands"/>'s own doc
/// comment for the contrast).</summary>
public sealed record OverseerAssignedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<OverseerAssignment> RecordId,
    RuntimeId<Character> HolderId,
    OverseerRole Role,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Building> BuildingId,
    string? CausationId) : IDomainEvent
{
    public string Type => "companions.overseerAssigned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HolderId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AppointOverseerCommand"/> (ADR 0006).</summary>
public static class AppointOverseerCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("companions.appointOverseer.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("companions.appointOverseer.characterDeceased");
    public static readonly ValidationErrorCode NotHouseholdMember = new("companions.appointOverseer.notHouseholdMember");
    public static readonly ValidationErrorCode IneligibleLifecycleStage = new("companions.appointOverseer.ineligibleLifecycleStage");
    public static readonly ValidationErrorCode BuildingNotFound = new("companions.appointOverseer.buildingNotFound");
    public static readonly ValidationErrorCode BuildingCategoryMismatch = new("companions.appointOverseer.buildingCategoryMismatch");
    public static readonly ValidationErrorCode AlreadyHoldsOverseerPosition = new("companions.appointOverseer.alreadyHoldsOverseerPosition");
    public static readonly ValidationErrorCode BuildingAlreadyHasOverseer = new("companions.appointOverseer.buildingAlreadyHasOverseer");
    public static readonly ValidationErrorCode InsufficientAttributeStanding = new("companions.appointOverseer.insufficientAttributeStanding");
    public static readonly ValidationErrorCode InsufficientLoyalty = new("companions.appointOverseer.insufficientLoyalty");

    public static readonly CommandPipeline<WorldState, AppointOverseerCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AppointOverseerCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character!.IsAlive)
            return CharacterDeceased;
        if (character.Household != command.HouseholdId)
            return NotHouseholdMember;
        if (character.GetLifecycleStage(command.SubmittedDate) < LifecycleStage.Adult)
            return IneligibleLifecycleStage;
        if (!state.Buildings.TryGet(command.BuildingId, out var building))
            return BuildingNotFound;
        if (building!.Definition.Sector != CompanionsCatalog.CategoryOf(command.Role))
            return BuildingCategoryMismatch;
        if (OverseerResolver.ActiveRecordForCharacter(state, command.CharacterId) is not null)
            return AlreadyHoldsOverseerPosition;
        if (OverseerResolver.HasActiveOverseer(state, command.BuildingId))
            return BuildingAlreadyHasOverseer;

        var attribute = CompanionsCatalog.ValueOf(character.GetEffectiveAttributes(), CompanionsCatalog.AttributeOf(command.Role));
        if (attribute < CompanionsCatalog.OverseerAttributeThreshold)
            return InsufficientAttributeStanding;
        if (character.Condition.Loyalty < CompanionsCatalog.OverseerLoyaltyThreshold)
            return InsufficientLoyalty;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AppointOverseerCommand command)
    {
        var recordId = state.OverseerAssignmentIds.Issue();
        state.OverseerAssignments.Add(
            recordId,
            new OverseerAssignment(
                recordId, command.CharacterId, command.Role, command.HouseholdId, command.BuildingId, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new OverseerAssignedEvent(
                state.EventIds.Issue(), command.SubmittedDate, recordId, command.CharacterId, command.Role,
                command.HouseholdId, command.BuildingId, command.CommandId.ToTaggedString()),
        };
    }
}
