using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.NotableBusinesses;

/// <summary>§4's named Reputation movers, plus this item's own §5 addition — every real reason this
/// item actually moves <see cref="NotableBusiness.Reputation"/>.</summary>
public enum BusinessReputationChangeReason
{
    /// <summary>§4's "Reputation rises through consistent Quality output."</summary>
    QualityOutput,

    /// <summary>§4's "falls through supply failures" — also the reason <see
    /// cref="SupplierDisruptionSystem"/> and a failed Government Contract (§7) apply.</summary>
    SupplyFailure,

    /// <summary>§4's "price gouging."</summary>
    PriceGouging,

    /// <summary>§4's "a genuine business-specific Scandal — a new source this document adds to that
    /// system's own existing sourceType list." See <see cref="RecordBusinessScandalCommand"/>.</summary>
    BusinessScandal,

    /// <summary>§5's Named Competition — this item's own addition beyond §4's own four-item list, since
    /// §5's own worked example ("his own Reputation and income both take a real, felt hit") is a
    /// distinctly-named mechanic moving the same field. See <see
    /// cref="RecordBusinessRivalryActionCommand"/>.</summary>
    CompetitiveRivalry,
}

/// <summary>
/// §4's one real, new, small tracked value's own one command path (rule 2) — every mover named above
/// routes through this rather than poking <see cref="NotableBusiness.Reputation"/> directly, matching
/// <see cref="Reputation.AdjustDignitasCommand"/>'s identical "the one place this figure actually
/// moves" role. Clamped to <see cref="NotableBusinessesCatalog.MinReputation"/>/<see
/// cref="NotableBusinessesCatalog.MaxReputation"/> rather than left unbounded like Dignitas — §4 frames
/// Reputation as a fixed 0-100 scale, not an open-ended figure.
/// </summary>
public sealed record AdjustBusinessReputationCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId,
    int Delta,
    BusinessReputationChangeReason Reason) : ICommand;

public sealed record BusinessReputationChangedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<NotableBusiness> BusinessId,
    int PreviousReputation,
    int NewReputation,
    BusinessReputationChangeReason Reason,
    string? CausationId) : IDomainEvent
{
    public string Type => "notableBusinesses.reputationChanged";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BusinessId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class AdjustBusinessReputationCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("notableBusinesses.adjustReputation.businessNotFound");
    public static readonly ValidationErrorCode BusinessNotTracked = new("notableBusinesses.adjustReputation.businessNotTracked");

    public static readonly CommandPipeline<WorldState, AdjustBusinessReputationCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AdjustBusinessReputationCommand command)
    {
        if (!state.NotableBusinesses.TryGet(command.BusinessId, out var business))
            return BusinessNotFound;
        // §3's "no longer given extra simulation fidelity" — a Demoted business's own Reputation is
        // frozen exactly like a demoted Rival Gens' own Dignitas, matching
        // LivingWorldActorTieringService.DemoteIfQuiet's own doc comment.
        if (business!.Status != NotableBusinessStatus.Tracked)
            return BusinessNotTracked;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AdjustBusinessReputationCommand command)
    {
        state.NotableBusinesses.TryGet(command.BusinessId, out var business);
        var previous = business!.Reputation;
        var next = Math.Clamp(previous + command.Delta, NotableBusinessesCatalog.MinReputation, NotableBusinessesCatalog.MaxReputation);

        state.NotableBusinesses.Remove(command.BusinessId);
        state.NotableBusinesses.Add(command.BusinessId, business with { Reputation = next });

        return new IDomainEvent[]
        {
            new BusinessReputationChangedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.BusinessId, previous, next, command.Reason,
                command.CommandId.ToTaggedString()),
        };
    }
}

/// <summary>§9's Scandal cross-integration (Phase 15 item 4): "adds a new business-specific source to
/// [Scandal's] own existing sourceType list — a genuine addition, not a pre-existing entry — distinct
/// from any Scandal implicating the owner's own personal conduct." Wraps both halves of that framing in
/// one command: the real <see cref="ScandalRecord"/> (via <see cref="RecordScandalCommand"/>, with the
/// personal Dignitas penalty and Trait grant both suppressed — a business Scandal is deliberately
/// <i>not</i> routed onto the owner's own personal standing, per §4's own "distinct from the owner's own
/// personal standing" framing) and the real <see cref="NotableBusiness.Reputation"/> hit (via <see
/// cref="AdjustBusinessReputationCommand"/>). <see cref="RecordScandalCommand"/> is itself
/// household-scoped (<see cref="RuntimeId{Household}"/>), so the <see cref="ScandalRecord"/> half only
/// fires when the business's own owner actually resolves to one — a <see
/// cref="RealEstate.PropertyOwnerKind.PlayerHousehold"/>, matching <see
/// cref="NotableBusinessOwnerResolver.TryResolveHousehold"/>'s own narrowing. A <see
/// cref="RealEstate.PropertyOwnerKind.RivalGens"/> or <see
/// cref="RealEstate.PropertyOwnerKind.IndividualCharacter"/>-owned business still takes the real
/// Reputation hit; it simply has no household-scoped <see cref="ScandalRecord"/> to also
/// create.</summary>
public sealed record RecordBusinessScandalCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<NotableBusiness> BusinessId,
    ScandalSeverity Severity) : ICommand;

public static class RecordBusinessScandalCommands
{
    public static readonly ValidationErrorCode BusinessNotFound = new("notableBusinesses.recordScandal.businessNotFound");

    public static readonly CommandPipeline<WorldState, RecordBusinessScandalCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, RecordBusinessScandalCommand command) =>
        state.NotableBusinesses.TryGet(command.BusinessId, out _) ? null : BusinessNotFound;

    private static IDomainEvent[] Mutate(WorldState state, RecordBusinessScandalCommand command)
    {
        var events = new List<IDomainEvent>();
        state.NotableBusinesses.TryGet(command.BusinessId, out var business);

        if (NotableBusinessOwnerResolver.TryResolveHousehold(business!.Owner, out var householdId))
        {
            events.AddRange(RecordScandalCommands.Pipeline.Execute(
                state, new RecordScandalCommand(
                    state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                    householdId, ScandalSourceType.BusinessMisconduct, command.Severity,
                    ApplyOrdinaryDignitasPenalty: false, ApplyTraitGrant: false)).Events);
        }

        events.AddRange(AdjustBusinessReputationCommands.Pipeline.Execute(
            state, new AdjustBusinessReputationCommand(
                state.CommandIds.Issue(), command.ActorId, command.SubmittedDate, command.CommandId.ToTaggedString(),
                command.BusinessId, -NotableBusinessesCatalog.BusinessScandalReputationLoss,
                BusinessReputationChangeReason.BusinessScandal)).Events);

        return events.ToArray();
    }
}
