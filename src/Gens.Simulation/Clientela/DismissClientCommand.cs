using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Clientela;

/// <summary>Removes a Character from a household's Clientela roster (Phase 12 item 2) — the ordinary
/// end to a patronage tie, distinct from <see cref="ClientPoachingSystem"/>'s own involuntary flip to a
/// rival.</summary>
public sealed record DismissClientCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> ClientId) : ICommand;

/// <summary>Emitted whenever a <see cref="DismissClientCommand"/> is accepted. Same private, two-party
/// <see cref="Visibility"/> as <see cref="ClientRecruitedEvent"/>.</summary>
public sealed record ClientDismissedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> PatronHouseholdId,
    RuntimeId<Character> ClientId,
    string? CausationId) : IDomainEvent
{
    public string Type => "clientela.clientDismissed";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ClientId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(ClientId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="DismissClientCommand"/> (ADR 0006).</summary>
public static class DismissClientCommands
{
    public static readonly ValidationErrorCode NotAClient = new("clientela.dismissClient.notAClient");

    public static readonly CommandPipeline<WorldState, DismissClientCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, DismissClientCommand command) =>
        !state.ClientelaEntries.TryGet(command.ClientId, out _) ? NotAClient : null;

    private static IDomainEvent[] Mutate(WorldState state, DismissClientCommand command)
    {
        state.ClientelaEntries.TryGet(command.ClientId, out var entry);
        state.ClientelaEntries.Remove(command.ClientId);

        // Only clean up the relationship-web bond when the patron still has a recorded head — an
        // already-vacant headship (a rare edge case: the patron household's own head died and no new
        // one has been established yet) simply leaves the stale bond tags to decay/be overwritten
        // naturally, matching how RelationshipDecaySystem already tolerates stale bonds elsewhere.
        if (state.HouseholdHeadships.TryGet(entry!.PatronHouseholdId, out var headship))
            ClientelaBondHelper.BreakBond(state, headship!.HeadCharacterId, command.ClientId, command.SubmittedDate);

        return new IDomainEvent[]
        {
            new ClientDismissedEvent(
                state.EventIds.Issue(), command.SubmittedDate, entry.PatronHouseholdId, command.ClientId,
                command.CommandId.ToTaggedString()),
        };
    }
}
