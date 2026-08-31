using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Correspondence;

/// <summary>§6's Inbox response: "a real, felt consequence for every response, including no response
/// at all." A deliberate command, not a flag flip — matching this codebase's own "a response is a real
/// command" convention (see this item's own task brief). §6's own "including no response at all"
/// framing means simply never submitting this command against a given <see cref="Letter"/> is itself a
/// legitimate, real choice this engine has to support by construction — nothing here forces a
/// response, and <see cref="Letter.RequiresResponse"/> staying unanswered is not an error state.</summary>
public sealed record RespondToLetterCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Letter> LetterId,
    LetterAction ResponseAction) : ICommand;

/// <summary>Emitted whenever a <see cref="RespondToLetterCommand"/> is accepted.</summary>
public sealed record LetterRespondedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Letter> LetterId,
    string RecipientCharacterOrActorId,
    LetterAction ResponseAction,
    string? CausationId) : IDomainEvent
{
    public string Type => "correspondence.responded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { RecipientCharacterOrActorId };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="RespondToLetterCommand"/> (ADR 0006). §10's
/// integration hooks (a felt Clientela/Dignitas consequence per §6) are explicitly this pipeline's
/// caller's own job, not this command's — matching <see cref="LetterAction"/>'s own doc comment for
/// why a not-yet-built target system's real payload stays out of this item's scope; this pipeline only
/// records that a response happened and what it was.</summary>
public static class RespondToLetterCommands
{
    public static readonly ValidationErrorCode LetterNotFound = new("correspondence.respond.letterNotFound");
    public static readonly ValidationErrorCode NotInbound = new("correspondence.respond.notInbound");
    public static readonly ValidationErrorCode NotYetDelivered = new("correspondence.respond.notYetDelivered");
    public static readonly ValidationErrorCode DoesNotRequireResponse = new("correspondence.respond.doesNotRequireResponse");
    public static readonly ValidationErrorCode AlreadyResponded = new("correspondence.respond.alreadyResponded");
    public static readonly ValidationErrorCode LetterNeverArrived = new("correspondence.respond.letterNeverArrived");

    public static readonly CommandPipeline<WorldState, RespondToLetterCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RespondToLetterCommand command)
    {
        if (!state.Letters.TryGet(command.LetterId, out var letter))
            return LetterNotFound;
        if (letter.Direction != LetterDirection.Inbound)
            return NotInbound;
        if (letter.Status == LetterStatus.InTransit)
            return NotYetDelivered;
        if (!letter.RequiresResponse)
            return DoesNotRequireResponse;
        if (letter.Responded)
            return AlreadyResponded;
        if (letter.Outcome == LetterOutcome.Intercepted)
            return LetterNeverArrived;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, RespondToLetterCommand command)
    {
        state.Letters.TryGet(command.LetterId, out var letter);
        state.Letters.Remove(command.LetterId);
        state.Letters.Add(
            command.LetterId,
            letter with { Responded = true, ResponseAction = command.ResponseAction, Status = LetterStatus.Answered });

        return new IDomainEvent[]
        {
            new LetterRespondedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.LetterId,
                letter.RecipientCharacterOrActorId, command.ResponseAction, command.CommandId.ToTaggedString()),
        };
    }
}
