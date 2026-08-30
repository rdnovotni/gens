using Gens.Simulation.Actors;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Collegia;

/// <summary>
/// Moves Money from a household's own Ledger account into a Collegium's Arca (Phase 12 item 3; §3's
/// membership dues, §4's patron funding cost — one shared command for both, since both are the same
/// real movement: a household's balance decreasing, the collegium's per-Actor account (<see
/// cref="LedgerAccountKey.ForActor(RuntimeId{Actors.Actor})"/>) increasing). Posted through <see
/// cref="LedgerService.Post"/> like every other real money movement in this codebase (ADR 0006) — the
/// Arca genuinely accumulates rather than draining into a fixed sink, since §3 frames it as "funding
/// mutual aid, the funerary guarantee, and any shared property," a real balance a future command could
/// spend back out of, unlike <see cref="Policies.FundFestivalCommand"/>'s own one-way festival sink.
/// </summary>
public sealed record FundCollegiumArcaCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<Household> FundingHouseholdId,
    RuntimeId<Actor> CollegiumId,
    Money Amount) : ICommand;

/// <summary>Emitted alongside the <see cref="LedgerTransactionPostedEvent"/> <see cref="LedgerService.Post"/>
/// itself produces, matching <see cref="Policies.FundFestivalCommand"/>'s identical "both the ledger
/// receipt and the domain-specific event" convention.</summary>
public sealed record CollegiumArcaFundedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Household> FundingHouseholdId,
    RuntimeId<Actor> CollegiumId,
    Money Amount,
    RuntimeId<LedgerTransaction> TransactionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "collegia.arcaFunded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { FundingHouseholdId.ToTaggedString(), CollegiumId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

/// <summary>The validate/mutate pipeline for <see cref="FundCollegiumArcaCommand"/> (ADR 0006).</summary>
public static class FundCollegiumArcaCommands
{
    public static readonly ValidationErrorCode AmountMustBePositive = new("collegia.fundArca.amountMustBePositive");
    public static readonly ValidationErrorCode CollegiumNotFound = new("collegia.fundArca.collegiumNotFound");
    public static readonly ValidationErrorCode InsufficientBalance = new("collegia.fundArca.insufficientBalance");

    public static readonly CommandPipeline<WorldState, FundCollegiumArcaCommand> Pipeline = new(
        validate: Validate,
        mutate: Mutate,
        issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, FundCollegiumArcaCommand command)
    {
        if (command.Amount <= Money.Zero)
            return AmountMustBePositive;
        if (!state.Collegia.TryGet(command.CollegiumId, out _))
            return CollegiumNotFound;

        var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(command.FundingHouseholdId), out var account)
            ? account!.Balance
            : Money.Zero;
        if (balance < command.Amount)
            return InsufficientBalance;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, FundCollegiumArcaCommand command)
    {
        var posted = LedgerService.Post(
            state, command.SubmittedDate, CollegiumCatalog.ArcaFundingCategory,
            new[]
            {
                new LedgerPosting(LedgerAccountKey.ForHousehold(command.FundingHouseholdId), -command.Amount),
                new LedgerPosting(LedgerAccountKey.ForActor(command.CollegiumId), command.Amount),
            },
            reference: $"collegiumArcaFunded:{command.CollegiumId.ToTaggedString()}");

        return new IDomainEvent[]
        {
            posted,
            new CollegiumArcaFundedEvent(
                state.EventIds.Issue(), command.SubmittedDate, command.FundingHouseholdId, command.CollegiumId,
                command.Amount, posted.TransactionId, command.CommandId.ToTaggedString()),
        };
    }
}
