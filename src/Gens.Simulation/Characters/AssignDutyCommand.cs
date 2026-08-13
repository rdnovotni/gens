using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Assigns a living, eligible household member to a Labor duty slot
/// (<c>gens-familia-design.md</c> §4).</summary>
public sealed record AssignDutyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Household> HouseholdId,
    DutySlot Slot) : ICommand;

/// <summary>Emitted whenever an <see cref="AssignDutyCommand"/> is accepted.</summary>
public sealed record DutyAssignedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Household> HouseholdId,
    DutySlot Slot,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.dutyAssigned";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AssignDutyCommand"/> (ADR 0006). Validation
/// covers every gate <c>gens-familia-design.md</c> §4 implies but doesn't specify numerically: the
/// assignee must belong to the household, be old enough (Adolescent+), competent enough at the slot's
/// <see cref="LaborSkills"/> stat, currently available (not already on a duty, not too fatigued to
/// take one on), physically co-located with the rest of the household, and the slot must have an open
/// seat (<see cref="DutySlotCatalog"/>).</summary>
public static class AssignDutyCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("characters.duty.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("characters.duty.characterDeceased");
    public static readonly ValidationErrorCode NotHouseholdMember = new("characters.duty.notHouseholdMember");
    public static readonly ValidationErrorCode TooYoung = new("characters.duty.tooYoung");
    public static readonly ValidationErrorCode AlreadyAssigned = new("characters.duty.alreadyAssigned");
    public static readonly ValidationErrorCode Unavailable = new("characters.duty.unavailable");
    public static readonly ValidationErrorCode InsufficientCompetence = new("characters.duty.insufficientCompetence");
    public static readonly ValidationErrorCode LocationConflict = new("characters.duty.locationConflict");
    public static readonly ValidationErrorCode SlotAtCapacity = new("characters.duty.slotAtCapacity");

    public static readonly CommandPipeline<WorldState, AssignDutyCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AssignDutyCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (character.Household != command.HouseholdId)
            return NotHouseholdMember;
        if (character.GetLifecycleStage(command.SubmittedDate) < LifecycleStage.Adolescent)
            return TooYoung;
        if (character.Duty is not null)
            return AlreadyAssigned;
        if (character.Condition.Fatigue >= DutySlotCatalog.MaxWorkableFatigue)
            return Unavailable;

        var effectiveSkill = DutySlotCatalog.RelevantSkillValue(character.GetEffectiveSkills(), command.Slot);
        if (effectiveSkill < DutySlotCatalog.MinimumCompetence)
            return InsufficientCompetence;

        var slotHolders = 0;
        RuntimeId<Settlement>? otherMemberLocation = null;
        foreach (var entry in state.Characters.InAscendingOrder())
        {
            var member = entry.Value;
            if (member.Id == character.Id || member.Household != command.HouseholdId)
                continue;

            otherMemberLocation ??= member.Location;
            if (member.Duty is { } duty && duty.HouseholdId == command.HouseholdId && duty.Slot == command.Slot)
                slotHolders++;
        }

        if (otherMemberLocation is { } location && location != character.Location)
            return LocationConflict;
        if (slotHolders >= DutySlotCatalog.Capacity(command.Slot))
            return SlotAtCapacity;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AssignDutyCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(
            command.CharacterId,
            character with { Duty = new DutyAssignment(command.HouseholdId, command.Slot, command.SubmittedDate) });

        return new IDomainEvent[]
        {
            new DutyAssignedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.HouseholdId,
                command.Slot, command.CommandId.ToTaggedString()),
        };
    }
}
