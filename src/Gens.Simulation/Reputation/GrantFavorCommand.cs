using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Reputation;

/// <summary>Opens a new <see cref="FavorObligation"/> from <see cref="GrantorId"/> to <see
/// cref="BeneficiaryId"/> (Phase 12 item 1). Actor-agnostic like every other command in this codebase
/// (rule 2) — a player action, a future Clientela system's automated favor grant, and NPC-to-NPC
/// bookkeeping all submit this exact command.</summary>
public sealed record GrantFavorCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> GrantorId,
    RuntimeId<Character> BeneficiaryId,
    string Kind) : ICommand;

/// <summary>Emitted whenever a <see cref="GrantFavorCommand"/> is accepted. Deliberately <see
/// cref="Commands.Visibility.Private"/> to the two named Characters — a favor is a fact between a
/// patron and a client (or any two individuals), not a standing public figure the way <see
/// cref="AdjustDignitasCommand"/>'s Dignitas total is (see that command's own <see
/// cref="DignitasChangedEvent"/> doc comment for the direct contrast). Whether a third party ever comes
/// to know a specific favor was granted is exactly the kind of provenance/knowledge-propagation question
/// ADR 0008 defers to a later phase with real event payloads to propagate.</summary>
public sealed record FavorGrantedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<FavorObligation> FavorId,
    RuntimeId<Character> GrantorId,
    RuntimeId<Character> BeneficiaryId,
    string Kind,
    string? CausationId) : IDomainEvent
{
    public string Type => "reputation.favorGranted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { GrantorId.ToTaggedString(), BeneficiaryId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(GrantorId.ToTaggedString(), BeneficiaryId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="GrantFavorCommand"/> (ADR 0006).</summary>
public static class GrantFavorCommands
{
    public static readonly ValidationErrorCode SameCharacter = new("reputation.grantFavor.sameCharacter");
    public static readonly ValidationErrorCode EmptyKind = new("reputation.grantFavor.emptyKind");
    public static readonly ValidationErrorCode UnknownCharacter = new("reputation.grantFavor.unknownCharacter");

    public static readonly CommandPipeline<WorldState, GrantFavorCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, GrantFavorCommand command)
    {
        if (command.GrantorId == command.BeneficiaryId)
            return SameCharacter;
        if (string.IsNullOrWhiteSpace(command.Kind))
            return EmptyKind;
        if (!state.Characters.TryGet(command.GrantorId, out _) || !state.Characters.TryGet(command.BeneficiaryId, out _))
            return UnknownCharacter;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, GrantFavorCommand command)
    {
        var favorId = state.FavorObligationIds.Issue();
        state.FavorObligations.Add(
            favorId,
            new FavorObligation(
                favorId, command.GrantorId, command.BeneficiaryId, command.Kind, command.SubmittedDate, FavorStatus.Outstanding));

        return new IDomainEvent[]
        {
            new FavorGrantedEvent(
                state.EventIds.Issue(), command.SubmittedDate, favorId, command.GrantorId, command.BeneficiaryId,
                command.Kind, command.CommandId.ToTaggedString()),
        };
    }
}
