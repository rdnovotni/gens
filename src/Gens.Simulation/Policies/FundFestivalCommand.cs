using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Policies;

/// <summary>Funds a Festival (<c>gens-policies-edicts-design.md</c> §4: "Divine Favor, Dignitas —
/// Religion §5") — Phase 9 item 2's "one funded action": a one-off spend for an immediate, contained
/// payoff, distinct from a Standing Policy's continuous dial (§1's own "does it sit toggled, or does
/// it fire once?" test; this fires once). Religion (§6, future) is what will eventually turn the spend
/// into an actual Divine Favor/Dignitas payoff; this pass only moves the money through the ledger and
/// makes the spend real, readable state — matching <see
/// cref="Gens.Simulation.Economy.StandingContractService"/>'s own "Phase 9's not-yet-built actions
/// layer would call this" forward reference, now realized here for a funded action specifically.
/// </summary>
public sealed record FundFestivalCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    Money Amount) : ICommand;

/// <summary>Emitted whenever a <see cref="FundFestivalCommand"/> is accepted, alongside the <see
/// cref="LedgerTransactionPostedEvent"/> <see cref="LedgerService.Post"/> itself produces (matching
/// <see cref="Gens.Simulation.Economy.DebtSystem"/>'s identical "both the ledger receipt and the
/// domain-specific event" convention).</summary>
public sealed record FestivalFundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> HouseholdId,
    RuntimeId<Settlement> SettlementId,
    Money Amount,
    RuntimeId<LedgerTransaction> TransactionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "policies.festivalFunded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HouseholdId.ToTaggedString(), SettlementId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="FundFestivalCommand"/> (ADR 0006).</summary>
public static class FundFestivalCommands
{
    public static readonly ValidationErrorCode AmountMustBePositive = new("policies.fundFestival.amountMustBePositive");
    public static readonly ValidationErrorCode InsufficientTreasury = new("policies.fundFestival.insufficientTreasury");

    /// <summary>The named system sink a funded Festival's spend drains into — money genuinely consumed
    /// by the festivities rather than transferred to another campaign-owned account, matching <see
    /// cref="Gens.Simulation.Economy.StandingContractService.CommitToTradeRoute"/>'s identical "one
    /// named account, not an untracked leak" discipline.</summary>
    private static readonly LedgerAccountKey FestivalSink = new(LedgerAccountKind.System, "fundedaction:festival");

    public static readonly CommandPipeline<WorldState, FundFestivalCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FundFestivalCommand command)
    {
        if (command.Amount <= Money.Zero)
            return AmountMustBePositive;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.HouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < command.Amount)
            return InsufficientTreasury;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FundFestivalCommand command)
    {
        var posted = LedgerService.Post(
            state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.HouseholdId), -command.Amount),
                new LedgerPosting(FestivalSink, command.Amount),
            },
            reference: $"fundedAction:festival:{command.CommandId.ToTaggedString()}");

        return new IDomainEvent[]
        {
            posted,
            new FestivalFundedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.HouseholdId, command.SettlementId,
                command.Amount, posted.TransactionId, command.CommandId.ToTaggedString()),
        };
    }
}
