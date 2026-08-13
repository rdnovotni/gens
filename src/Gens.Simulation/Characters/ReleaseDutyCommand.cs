using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Vacates a Character's current Labor duty slot, if any (<c>gens-familia-design.md</c>
/// §4) — the only way a held <see cref="Character.Duty"/> clears, short of being overwritten by a
/// fresh <see cref="AssignDutyCommand"/> after this one runs.</summary>
public sealed record ReleaseDutyCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId) : ICommand;

/// <summary>Emitted whenever a <see cref="ReleaseDutyCommand"/> is accepted.</summary>
public sealed record DutyReleasedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Household> HouseholdId,
    DutySlot Slot,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.dutyReleased";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString(), HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="ReleaseDutyCommand"/> (ADR 0006).</summary>
public static class ReleaseDutyCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("characters.duty.characterNotFound");
    public static readonly ValidationErrorCode NoActiveDuty = new("characters.duty.noActiveDuty");

    public static readonly CommandPipeline<WorldState, ReleaseDutyCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, ReleaseDutyCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (character.Duty is null)
            return NoActiveDuty;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, ReleaseDutyCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        var duty = character.Duty!.Value;

        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(command.CharacterId, character with { Duty = null });

        return new IDomainEvent[]
        {
            new DutyReleasedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, duty.HouseholdId, duty.Slot,
                command.CommandId.ToTaggedString()),
        };
    }
}
