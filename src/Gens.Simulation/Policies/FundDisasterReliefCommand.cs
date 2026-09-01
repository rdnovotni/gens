using Gens.Simulation.Commands;
using Gens.Simulation.Hazards;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Policies;

/// <summary>Funds §6.2's own real, named ninth Funded Action: **Disaster Relief**, "triggered by physical
/// disaster damage" against one already-fired <see cref="DisasterEvent"/> — the design document's own
/// item-3/item-5 deferred gap (<c>Hazards.NaturalDisasterSystem</c>'s own doc comment named it exactly),
/// closed here in the same shape <see cref="FundFestivalCommand"/> already established for this
/// codebase's one other authored Funded Action: a one-off Household spend, posted through the real
/// Ledger, immediately and visibly consumed rather than parked in a second campaign-owned account. §6.2's
/// own "rarely justifies the political theater... concentrated at Severe and Catastrophic severity" is
/// enforced directly: this command is only eligible against a <see cref="DisasterSeverity.Severe"/> or
/// <see cref="DisasterSeverity.Catastrophic"/> Event, and only once per Event (<see
/// cref="DisasterEvent.ReliefFunded"/>) — a second relief response to the same disaster would read as the
/// "oddly generous non-event" §6.2 explicitly warns against. The real patronage payoff is a Dignitas gain
/// through <see cref="DignitasResolver.Apply"/>, the same disclosed Cultural-Prestige-to-Dignitas
/// substitution <c>Wanderers.HostWandererCommand</c>'s own doc comment already made — a visible relief
/// response is patronage, and Dignitas is this codebase's one real "moved by nearly everything"
/// household-standing primitive.</summary>
public sealed record FundDisasterReliefCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<DisasterEvent> DisasterEventId,
    Money Amount) : ICommand;

/// <summary>Emitted whenever a <see cref="FundDisasterReliefCommand"/> is accepted, alongside the <see
/// cref="LedgerTransactionPostedEvent"/> <see cref="LedgerService.Post"/> itself produces, matching <see
/// cref="FestivalFundedEvent"/>'s identical "both the ledger receipt and the domain-specific event"
/// convention.</summary>
public sealed record DisasterReliefFundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<DisasterEvent> DisasterEventId,
    Money Amount,
    int DignitasGained,
    RuntimeId<LedgerTransaction> TransactionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "policies.disasterReliefFunded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="FundDisasterReliefCommand"/> (ADR 0006).</summary>
public static class FundDisasterReliefCommands
{
    public static readonly ValidationErrorCode AmountMustBePositive = new("policies.fundDisasterRelief.amountMustBePositive");
    public static readonly ValidationErrorCode InsufficientTreasury = new("policies.fundDisasterRelief.insufficientTreasury");
    public static readonly ValidationErrorCode DisasterEventNotFound = new("policies.fundDisasterRelief.disasterEventNotFound");
    public static readonly ValidationErrorCode SeverityTooLow = new("policies.fundDisasterRelief.severityTooLow");
    public static readonly ValidationErrorCode AlreadyFunded = new("policies.fundDisasterRelief.alreadyFunded");

    /// <summary>§6.2's own real Dignitas payoff — this implementation's own invented figure (no relief
    /// patronage value is sized anywhere in the design corpus), a flat gain rather than spend-scaled,
    /// matching <c>Wanderers.HostWandererCommand</c>'s own "the visible act of patronage, not its exact
    /// price tag" framing for its identical Dignitas-through-engagement payoff.</summary>
    public const int DignitasGain = 8;

    private static readonly LedgerAccountKey DisasterReliefSink = new(LedgerAccountKind.System, "fundedaction:disasterrelief");

    public static readonly CommandPipeline<WorldState, FundDisasterReliefCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FundDisasterReliefCommand command)
    {
        if (command.Amount <= Money.Zero)
            return AmountMustBePositive;

        if (!state.DisasterEvents.TryGet(command.DisasterEventId, out var disasterEvent))
            return DisasterEventNotFound;
        if (disasterEvent!.Severity is not (DisasterSeverity.Severe or DisasterSeverity.Catastrophic))
            return SeverityTooLow;
        if (disasterEvent.ReliefFunded)
            return AlreadyFunded;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.HouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < command.Amount)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FundDisasterReliefCommand command)
    {
        state.DisasterEvents.TryGet(command.DisasterEventId, out var disasterEvent);
        state.DisasterEvents.Remove(command.DisasterEventId);
        state.DisasterEvents.Add(command.DisasterEventId, disasterEvent! with { ReliefFunded = true });

        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -command.Amount),
                new LedgerPosting(DisasterReliefSink, command.Amount),
            },
            reference: $"fundedAction:disasterRelief:{command.CommandId.ToTaggedString()}");

        DignitasResolver.Apply(state, command.HouseholdId, DignitasGain);

        return new IDomainEvent[]
        {
            posted,
            new DisasterReliefFundedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, disasterEvent.SettlementId,
                command.DisasterEventId, command.Amount, DignitasGain, posted.TransactionId, command.CommandId.ToTaggedString()),
        };
    }
}
