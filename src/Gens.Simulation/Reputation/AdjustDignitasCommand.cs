using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Reputation;

/// <summary>
/// The one command path (rule 2) every future Dignitas-moving system routes through — a Politics &amp;
/// Patronage Salutatio, a won magistracy, a Legal &amp; Court verdict, a Scandal, a defaulted debt — all
/// of them, per <c>gens-politics-patronage-design.md</c> §2, move the same single household-level
/// number, and all of them will submit this same command rather than each poking <see
/// cref="HouseholdReputation"/> directly. No such caller exists yet in this codebase (each named
/// trigger belongs to a later Phase 12 item, or to already-closed Phase 9/11 items this pass
/// deliberately does not reopen — see <see cref="HouseholdReputation"/>'s own doc comment) — this item
/// only builds the shared primitive itself, exercised directly by tests standing in for those future
/// callers. <paramref name="Reason"/> is a plain, free-form string rather than a closed enum: no single
/// reason catalog exists yet across every future source, matching <see
/// cref="Actors.LivingWorldActorMilitaryStrength.ResolvedForceId"/>'s identical "reference something
/// that does not exist yet as a plain string" convention.
/// </summary>
public sealed record AdjustDignitasCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    int Delta,
    string Reason) : ICommand;

/// <summary>
/// Emitted whenever an <see cref="AdjustDignitasCommand"/> is accepted. <see cref="Visibility"/> is
/// always <see cref="Commands.Visibility.Public"/>: per <c>gens-politics-patronage-design.md</c> §2,
/// Dignitas is "read constantly by other systems... without this document needing to duplicate those
/// read sites" and, per <c>gens-celebrities-influential-figures-design.md</c> §4, is "legible to
/// Curiales, Rival Houses, and the political class" by definition — it is a standing figure other
/// actors are simply assumed to know, not a fact that has to propagate through contact or
/// correspondence first. This is the deliberate contrast with <see cref="GrantFavorCommand"/>'s
/// private, two-party <see cref="FavorGrantedEvent"/>: the same reputation-and-obligation primitive
/// this item builds surfaces at two genuinely different audience scopes, using the same <see
/// cref="Commands.Visibility"/> mechanism every other system in this codebase already reads knowledge
/// through (ADR 0008), rather than a parallel audience model invented just for Dignitas.
/// </summary>
public sealed record DignitasChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    int PreviousDignitas,
    int NewDignitas,
    string Reason,
    string? CausationId) : IDomainEvent
{
    public string Type => "reputation.dignitasChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="AdjustDignitasCommand"/> (ADR 0006).</summary>
public static class AdjustDignitasCommands
{
    public static readonly ValidationErrorCode ZeroDelta = new("reputation.adjustDignitas.zeroDelta");

    public static readonly CommandPipeline<WorldState, AdjustDignitasCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AdjustDignitasCommand command) =>
        command.Delta == 0 ? ZeroDelta : null;

    private static IDomainEvent[] Mutate(WorldState state, AdjustDignitasCommand command)
    {
        var previous = DignitasResolver.Current(state, command.HouseholdId);
        DignitasResolver.Apply(state, command.HouseholdId, command.Delta);
        var next = previous + command.Delta;

        return new IDomainEvent[]
        {
            new DignitasChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, previous, next,
                command.Reason, command.CommandId.ToTaggedString()),
        };
    }
}
