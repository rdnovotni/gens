using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Clientela;

/// <summary>
/// A patron calling in a favor from one of their Clientela clients (Phase 12 item 2; §4.2's "what
/// favor they can actually perform when called on"). This is exactly the integration <see
/// cref="FavorObligation"/>'s own doc comment flagged as deliberately left to item 2: it opens and
/// immediately resolves a <see cref="FavorObligation"/> (the client performing the favor on the spot,
/// rather than a debt the patron waits on) using that item's own record and event shapes — <see
/// cref="Reputation.FavorGrantedEvent"/> and <see cref="Reputation.FavorSettledEvent"/> — instead of a
/// bespoke Clientela-only ledger. What this command adds on top is §4.2's own reciprocity rule: calling
/// in a favor within <see cref="ClientelaCatalog.FavorCooldownMonths"/> of the last one is "too often
/// without reciprocation" and costs the client's opinion of the patron (<see
/// cref="ClientelaCatalog.OverdrawnOpinionPenalty"/>); spaced-out calls cost nothing.
/// </summary>
public sealed record CallInClientFavorCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> PatronHouseholdId,
    RuntimeId<Character> ClientId,
    string Kind) : ICommand;

/// <summary>Emitted whenever a <see cref="CallInClientFavorCommand"/> is accepted, alongside the
/// generic <see cref="FavorGrantedEvent"/>/<see cref="FavorSettledEvent"/> pair every favor produces —
/// this event carries the Clientela-specific fact those two don't: whether the call-in was overdrawn
/// and, if so, the opinion cost actually applied. Same private, two-party <see cref="Visibility"/> as
/// the underlying favor events.</summary>
public sealed record ClientFavorCalledInEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Character> PatronHeadId,
    RuntimeId<Character> ClientId,
    RuntimeId<FavorObligation> FavorId,
    bool Overdrawn,
    int OpinionDelta,
    string? CausationId) : IDomainEvent
{
    public string Type => "clientela.clientFavorCalledIn";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { PatronHeadId.ToTaggedString(), ClientId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(PatronHeadId.ToTaggedString(), ClientId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="CallInClientFavorCommand"/> (ADR 0006).</summary>
public static class CallInClientFavorCommands
{
    public static readonly ValidationErrorCode UnknownClient = new("clientela.callInFavor.unknownClient");
    public static readonly ValidationErrorCode NotYourClient = new("clientela.callInFavor.notYourClient");
    public static readonly ValidationErrorCode ClientDeceased = new("clientela.callInFavor.clientDeceased");
    public static readonly ValidationErrorCode EmptyKind = new("clientela.callInFavor.emptyKind");
    public static readonly ValidationErrorCode PatronHasNoHead = new("clientela.callInFavor.patronHasNoHead");

    public static readonly CommandPipeline<WorldState, CallInClientFavorCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, CallInClientFavorCommand command)
    {
        if (!state.ClientelaEntries.TryGet(command.ClientId, out var entry))
            return UnknownClient;
        if (entry!.PatronHouseholdId != command.PatronHouseholdId)
            return NotYourClient;
        if (string.IsNullOrWhiteSpace(command.Kind))
            return EmptyKind;
        if (!state.Characters.TryGet(command.ClientId, out var client) || !client!.IsAlive)
            return ClientDeceased;
        if (!state.HouseholdHeadships.TryGet(command.PatronHouseholdId, out _))
            return PatronHasNoHead;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, CallInClientFavorCommand command)
    {
        state.ClientelaEntries.TryGet(command.ClientId, out var entry);
        state.HouseholdHeadships.TryGet(command.PatronHouseholdId, out var headship);
        var patronHeadId = headship!.HeadCharacterId;

        // The favor is granted and immediately marked Repaid: unlike GrantFavorCommand's own
        // open-ended debt, a Clientela call-in represents the client performing the favor there and
        // then, not a promise collected later.
        var favorId = state.FavorObligationIds.Issue();
        state.FavorObligations.Add(
            favorId,
            new FavorObligation(
                favorId, command.ClientId, patronHeadId, command.Kind, command.SubmittedDate,
                FavorStatus.Repaid, command.SubmittedDate));

        var overdrawn = entry!.LastFavorCalledDate is { } last &&
            command.SubmittedDate.TotalMonths - last.TotalMonths < ClientelaCatalog.FavorCooldownMonths;
        var opinionDelta = 0;
        if (overdrawn)
        {
            opinionDelta = ClientelaCatalog.OverdrawnOpinionPenalty;
            ClientelaBondHelper.AdjustOpinion(state, command.ClientId, patronHeadId, opinionDelta, command.SubmittedDate);
        }

        state.ClientelaEntries.Remove(command.ClientId);
        state.ClientelaEntries.Add(command.ClientId, entry with { LastFavorCalledDate = command.SubmittedDate });

        return new IDomainEvent[]
        {
            new FavorGrantedEvent(
                state.EventIds.Issue(), command.SubmittedDate, favorId, command.ClientId, patronHeadId, command.Kind,
                command.CommandId.ToTaggedString()),
            new FavorSettledEvent(
                state.EventIds.Issue(), command.SubmittedDate, favorId, command.ClientId, patronHeadId,
                FavorResolution.Repaid, command.CommandId.ToTaggedString()),
            new ClientFavorCalledInEvent(
                state.EventIds.Issue(), command.SubmittedDate, patronHeadId, command.ClientId, favorId, overdrawn,
                opinionDelta, command.CommandId.ToTaggedString()),
        };
    }
}
