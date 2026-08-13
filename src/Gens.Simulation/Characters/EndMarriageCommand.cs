using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Ends the submitting Character's currently open marriage as a divorce or annulment
/// (<c>gens-familia-design.md</c> §5.1). <see cref="MarriageEndReason.Death"/> is deliberately not a
/// valid <see cref="Reason"/> here — a spouse's death only ever closes a marriage via <see
/// cref="CharacterLifecycleSystem"/>, never via a submitted command.</summary>
public sealed record EndMarriageCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    MarriageEndReason Reason) : ICommand;

/// <summary>The validate/mutate pipeline for <see cref="EndMarriageCommand"/> (ADR 0006). Emits the
/// same <see cref="MarriageEndedEvent"/> shape <see cref="CharacterLifecycleSystem"/> uses for a
/// death-triggered closure, but with this command's ID as <see
/// cref="MarriageEndedEvent.CausationId"/>.</summary>
public static class EndMarriageCommands
{
    public static readonly ValidationErrorCode CharacterNotFound = new("characters.marriage.characterNotFound");
    public static readonly ValidationErrorCode NotMarried = new("characters.marriage.notMarried");
    public static readonly ValidationErrorCode DeathNotAllowed = new("characters.marriage.deathNotAllowed");

    public static readonly CommandPipeline<WorldState, EndMarriageCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, EndMarriageCommand command)
    {
        if (command.Reason == MarriageEndReason.Death)
            return DeathNotAllowed;
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (character.CurrentSpouseId is null)
            return NotMarried;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, EndMarriageCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        var spouseId = character.CurrentSpouseId!.Value;

        var updatedCharacter = CloseOpenMarriage(character, spouseId, command.SubmittedDate, command.Reason);
        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(command.CharacterId, updatedCharacter);

        if (state.Characters.TryGet(spouseId, out var spouse))
        {
            var updatedSpouse = CloseOpenMarriage(spouse, command.CharacterId, command.SubmittedDate, command.Reason);
            state.Characters.Remove(spouseId);
            state.Characters.Add(spouseId, updatedSpouse);
        }

        return new IDomainEvent[]
        {
            new MarriageEndedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, spouseId, command.Reason,
                command.CommandId.ToTaggedString()),
        };
    }

    private static Character CloseOpenMarriage(
        Character character, RuntimeId<Character> spouseId, GameDate endDate, MarriageEndReason reason)
    {
        var history = character.MaritalHistory
            .Select(record => record.SpouseId == spouseId && record.EndDate is null
                ? new MarriageRecord(record.SpouseId, record.StartDate, endDate, reason)
                : record)
            .ToArray();
        return character with { MaritalHistory = history };
    }
}
