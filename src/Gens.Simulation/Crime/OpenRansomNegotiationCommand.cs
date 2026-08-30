using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Crime;

/// <summary>§10: opens a ransom negotiation over a Detained captive "of sufficient standing," sized to
/// the captive's own household Dignitas. The caller supplies the actual opening demand (mirroring <see
/// cref="Legal.OfferBribeCommand"/>'s own "the caller names the Denarii amount" shape) — <see
/// cref="RansomCatalogSuggestion.SuggestedDemand"/> is offered as a real, non-mandatory sizing helper
/// rather than a hard formula the command itself enforces, since §13 leaves "Ransom pricing" fully
/// unsized.</summary>
public sealed record OpenRansomNegotiationCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Character> CaptiveCharacterId,
    RuntimeId<Household> CapturingHouseholdId,
    RuntimeId<Household> TargetHouseholdId,
    Money AmountOffered) : ICommand;

/// <summary>Emitted whenever an <see cref="OpenRansomNegotiationCommand"/> is accepted.</summary>
public sealed record RansomNegotiationOpenedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<RansomNegotiation> NegotiationId,
    RuntimeId<Character> CaptiveCharacterId,
    RuntimeId<Household> CapturingHouseholdId,
    RuntimeId<Household> TargetHouseholdId,
    Money AmountOffered,
    string? CausationId) : IDomainEvent
{
    public string Type => "crime.ransomNegotiationOpened";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CapturingHouseholdId.ToTaggedString(), TargetHouseholdId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>A real, non-mandatory sizing helper for a ransom's opening demand (§10: "sized to the
/// captive's own Dignitas and their house's own wealth"). Wealth is left out — no per-household net
/// worth figure independent of <see cref="Economy.NetWorth"/>'s own broader assessment machinery is
/// reachable from this domain cheaply, and Dignitas alone already gives a real, if partial, "how much
/// is this person actually worth ransoming" signal.</summary>
public static class RansomCatalogSuggestion
{
    public static Money SuggestedDemand(WorldState state, RuntimeId<Household> captiveHouseholdId)
    {
        var dignitas = DignitasResolver.Current(state, captiveHouseholdId);
        var suggested = Money.FromDenarii(Math.Max(0, dignitas) * CrimeCatalog.RansomDenariiPerDignitasPoint);
        return suggested < CrimeCatalog.MinimumRansomDemand ? CrimeCatalog.MinimumRansomDemand : suggested;
    }
}

/// <summary>The validate/mutate pipeline for <see cref="OpenRansomNegotiationCommand"/> (ADR 0006).</summary>
public static class OpenRansomNegotiationCommands
{
    public static readonly ValidationErrorCode CaptiveNotFound = new("crime.openRansomNegotiation.captiveNotFound");
    public static readonly ValidationErrorCode NotDetained = new("crime.openRansomNegotiation.notDetained");
    public static readonly ValidationErrorCode AlreadyNegotiating = new("crime.openRansomNegotiation.alreadyNegotiating");
    public static readonly ValidationErrorCode NonPositiveAmount = new("crime.openRansomNegotiation.nonPositiveAmount");

    public static readonly CommandPipeline<WorldState, OpenRansomNegotiationCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, OpenRansomNegotiationCommand command)
    {
        if (!state.Characters.TryGet(command.CaptiveCharacterId, out var captive) || !captive!.IsAlive)
            return CaptiveNotFound;
        if (DetentionResolver.ActiveFor(state, command.CaptiveCharacterId) is null)
            return NotDetained;
        if (RansomNegotiationResolver.ActiveFor(state, command.CaptiveCharacterId) is not null)
            return AlreadyNegotiating;
        if (command.AmountOffered <= Money.Zero)
            return NonPositiveAmount;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, OpenRansomNegotiationCommand command)
    {
        var negotiationId = state.RansomNegotiationIds.Issue();
        state.RansomNegotiations.Add(
            negotiationId,
            new RansomNegotiation(
                negotiationId, command.CaptiveCharacterId, command.CapturingHouseholdId, command.TargetHouseholdId,
                command.AmountOffered, command.SubmittedDate));

        return new IDomainEvent[]
        {
            new RansomNegotiationOpenedEvent(
                state.EventIds.Issue(), command.SubmittedDate, negotiationId, command.CaptiveCharacterId,
                command.CapturingHouseholdId, command.TargetHouseholdId, command.AmountOffered,
                command.CommandId.ToTaggedString()),
        };
    }
}
