using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Characters;

/// <summary>Dispatches a pursuit after a Character has fled (<c>gens-labor-slavery-design.md</c> §7:
/// "a player-initiated response with a limited window ... before the trail goes cold"). <see
/// cref="PursuerId"/> is optional metadata only (which household member/Companion was dispatched) —
/// this "basic" scope has no bounty-hunter/Companion success-weighting system to consult yet.</summary>
public sealed record InitiatePursuitCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character>? PursuerId = null) : ICommand;

/// <summary>Emitted whenever an <see cref="InitiatePursuitCommand"/> is accepted.</summary>
public sealed record PursuitInitiatedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    RuntimeId<Character>? PursuerId,
    string? CausationId) : IDomainEvent
{
    public string Type => "characters.pursuitInitiated";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="InitiatePursuitCommand"/> (ADR 0006).</summary>
public static class InitiatePursuitCommands
{
    /// <summary>Invented baseline (§12 leaves the pursuit window untuned) — how many months after
    /// fleeing a pursuit can still be initiated (§7: "a few months").</summary>
    public const int PursuitWindowMonths = 4;

    public static readonly ValidationErrorCode CharacterNotFound = new("characters.pursuit.characterNotFound");
    public static readonly ValidationErrorCode CharacterDeceased = new("characters.pursuit.characterDeceased");
    public static readonly ValidationErrorCode NotFled = new("characters.pursuit.notFled");
    public static readonly ValidationErrorCode AlreadyPursuing = new("characters.pursuit.alreadyPursuing");
    public static readonly ValidationErrorCode TrailHasGoneCold = new("characters.pursuit.trailHasGoneCold");

    public static readonly CommandPipeline<WorldState, InitiatePursuitCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, InitiatePursuitCommand command)
    {
        if (!state.Characters.TryGet(command.CharacterId, out var character))
            return CharacterNotFound;
        if (!character.IsAlive)
            return CharacterDeceased;
        if (character.Flight is not { } flight)
            return NotFled;
        if (character.Pursuit is not null)
            return AlreadyPursuing;
        if (command.SubmittedDate.TotalMonths - flight.FledDate.TotalMonths > PursuitWindowMonths)
            return TrailHasGoneCold;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, InitiatePursuitCommand command)
    {
        state.Characters.TryGet(command.CharacterId, out var character);
        state.Characters.Remove(command.CharacterId);
        state.Characters.Add(
            command.CharacterId,
            character with { Pursuit = new PursuitRecord(LaborFlightSystem.PursuitDurationMonths, command.PursuerId) });

        return new IDomainEvent[]
        {
            new PursuitInitiatedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, command.PursuerId,
                command.CommandId.ToTaggedString()),
        };
    }
}
