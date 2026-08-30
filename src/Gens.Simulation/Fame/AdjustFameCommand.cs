using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Fame;

/// <summary>
/// The one command path (rule 2) every future Fame-moving system routes through — a celebrated
/// prosecution, a triumphant general's return, a champion gladiator's arena win, a courtesan's
/// notoriety — all of them, per §3's own source list, move the same single Character-level score, and
/// all of them will submit this same command rather than each poking <see cref="CharacterFame"/>
/// directly. No such caller exists yet in this codebase: every named source in <see
/// cref="FameSourceType"/> belongs to a system this codebase has not built (see that enum's own doc
/// comment for exactly which one each source needs) — this item only builds the shared primitive
/// itself, exercised directly by tests standing in for those future callers, matching <see
/// cref="Reputation.AdjustDignitasCommand"/>'s identical "the primitive ships, the callers don't exist
/// yet" precedent.
/// </summary>
public sealed record AdjustFameCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CharacterId,
    int Delta,
    FameSourceType SourceType) : ICommand;

/// <summary>
/// Emitted whenever an <see cref="AdjustFameCommand"/> is accepted. <see cref="Visibility"/> is always
/// <see cref="Commands.Visibility.Public"/>: per §4, Fame's real audience is "the crowd" — Settlement
/// Demographics' own aggregate pop groups and Notable Households' own sampled population — a public
/// fact by definition, the same "legible... without this needing to propagate through contact first"
/// reasoning <see cref="Reputation.DignitasChangedEvent"/>'s own doc comment already used for Dignitas,
/// just addressed to a wider, less individually-tracked audience.
/// </summary>
public sealed record FameChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> CharacterId,
    int PreviousFame,
    int NewFame,
    FameSourceType SourceType,
    string? CausationId) : IDomainEvent
{
    public string Type => "fame.fameChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AdjustFameCommand"/> (ADR 0006).</summary>
public static class AdjustFameCommands
{
    public static readonly ValidationErrorCode ZeroDelta = new("fame.adjustFame.zeroDelta");
    public static readonly ValidationErrorCode UnknownCharacter = new("fame.adjustFame.unknownCharacter");

    public static readonly CommandPipeline<WorldState, AdjustFameCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AdjustFameCommand command)
    {
        if (command.Delta == 0)
            return ZeroDelta;
        if (!state.Characters.TryGet(command.CharacterId, out _))
            return UnknownCharacter;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AdjustFameCommand command)
    {
        var previous = FameResolver.Current(state, command.CharacterId);
        FameResolver.Apply(state, command.CharacterId, command.Delta);
        var next = Math.Clamp(previous + command.Delta, 0, 100);

        return new IDomainEvent[]
        {
            new FameChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.CharacterId, previous, next,
                command.SourceType, command.CommandId.ToTaggedString()),
        };
    }
}
