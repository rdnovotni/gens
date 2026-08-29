using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Reputation;

/// <summary>How an outstanding <see cref="FavorObligation"/> is resolved (Phase 12 item 1) — called in
/// and actually honored, versus written off by the grantor without ever collecting.</summary>
public enum FavorResolution
{
    Repaid,
    Forgiven,
}

/// <summary>Resolves an <see cref="FavorStatus.Outstanding"/> <see cref="FavorObligation"/> as either
/// <see cref="FavorResolution.Repaid"/> or <see cref="FavorResolution.Forgiven"/> (Phase 12 item 1).
/// Deliberately does not itself move <see cref="Characters.Relationship.Opinion"/> — the design doc's
/// own "a favor drawn on too often without reciprocation costs the relationship-web opinion" (§4.2) is a
/// Clientela-specific policy judgment about frequency and reciprocity this generic primitive has no way
/// to make on its own; a future Clientela system reads this command's own <see cref="FavorSettledEvent"/>
/// and decides what, if anything, that means for opinion — matching <see
/// cref="FavorObligation"/>'s own doc comment for why this item stops at the ledger.</summary>
public sealed record SettleFavorCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<FavorObligation> FavorId,
    FavorResolution Resolution) : ICommand;

/// <summary>Emitted whenever a <see cref="SettleFavorCommand"/> is accepted. Same private,
/// two-party <see cref="Visibility"/> as <see cref="FavorGrantedEvent"/> — resolving a favor is no more
/// public a fact than granting one was.</summary>
public sealed record FavorSettledEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<FavorObligation> FavorId,
    RuntimeId<Characters.Character> GrantorId,
    RuntimeId<Characters.Character> BeneficiaryId,
    FavorResolution Resolution,
    string? CausationId) : IDomainEvent
{
    public string Type => "reputation.favorSettled";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { GrantorId.ToTaggedString(), BeneficiaryId.ToTaggedString() };
    public Visibility Visibility => Visibility.Private(GrantorId.ToTaggedString(), BeneficiaryId.ToTaggedString());
}

/// <summary>The validate/mutate pipeline for <see cref="SettleFavorCommand"/> (ADR 0006).</summary>
public static class SettleFavorCommands
{
    public static readonly ValidationErrorCode UnknownFavor = new("reputation.settleFavor.unknownFavor");
    public static readonly ValidationErrorCode NotOutstanding = new("reputation.settleFavor.notOutstanding");

    public static readonly CommandPipeline<WorldState, SettleFavorCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SettleFavorCommand command)
    {
        if (!state.FavorObligations.TryGet(command.FavorId, out var favor))
            return UnknownFavor;
        if (favor!.Status != FavorStatus.Outstanding)
            return NotOutstanding;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SettleFavorCommand command)
    {
        state.FavorObligations.TryGet(command.FavorId, out var favor);
        var newStatus = command.Resolution == FavorResolution.Repaid ? FavorStatus.Repaid : FavorStatus.Forgiven;

        state.FavorObligations.Remove(command.FavorId);
        state.FavorObligations.Add(command.FavorId, favor! with { Status = newStatus, ResolvedDate = command.SubmittedDate });

        return new IDomainEvent[]
        {
            new FavorSettledEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.FavorId, favor.GrantorId, favor.BeneficiaryId,
                command.Resolution, command.CommandId.ToTaggedString()),
        };
    }
}
