using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>§8's own outcome vocabulary — "won" | "lost" reads directly; §8's third listed value,
/// <c>"lostContestedLegally"</c>, is not a distinct value here: §5.1's own "a losing bidder... has a
/// real, standing motive to investigate, and... a real legal avenue if they find something" resolves
/// through an ordinary §6.2 repetundae prosecution against whoever won, not a special status on the
/// loser's own bid record — the loser's bid stays <see cref="Lost"/> regardless of whether they later
/// pursue that avenue, matching this domain's own "the flag is real, nothing yet forces the narrower
/// third state" reading of the sketch.</summary>
public enum ContractBidOutcome
{
    Pending,
    Won,
    Lost,
}

/// <summary>
/// §5's/§8's <c>ContractBid</c> data model (Phase 15 item 6). <see cref="ReliabilityScore"/> is a
/// snapshot taken at submission time (via <see cref="ContractBidderResolver.ReliabilityScore"/>), not a
/// live read — §5.1's "a losing bidder who believes the process was corrupted" needs the actual number
/// the Censor saw at award time to stay stable for later inspection, not a figure that could have since
/// drifted.
/// </summary>
public sealed record ContractBid(
    RuntimeId<ContractBid> BidId,
    RuntimeId<PublicContract> ContractId,
    ContractBidderRef Bidder,
    Money PriceOffered,
    int ReliabilityScore,
    int InfluenceSpent,
    bool BribeAttempted,
    Money BribeAmount,
    ContractBidOutcome Outcome,
    GameDate SubmittedDate);

/// <summary>§5's bid submission — any bidder eligible per <see cref="ContractBidderResolver.Exists"/>
/// and not currently disqualified per §6.2 (<see cref="ContractBidderResolver.IsDisqualified"/>) may
/// submit against any <see cref="PublicContractStatus.OpenForBidding"/> contract. <see
/// cref="InfluenceSpent"/>/<see cref="BribeAmount"/> both require the bidder to resolve to a real
/// household (<see cref="ContractBidderResolver.TryResolveHousehold"/>) — see that resolver's own doc
/// comment for why a Rival House bidder can offer neither (no Influence balance or Ledger account of
/// that shape exists for a bare <see cref="Actors.LivingWorldActor"/>).</summary>
public sealed record SubmitContractBidCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<PublicContract> ContractId,
    ContractBidderRef Bidder,
    Money PriceOffered,
    int InfluenceSpent = 0,
    Money? BribeAmount = null) : ICommand;

public sealed record ContractBidSubmittedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<ContractBid> BidId,
    RuntimeId<PublicContract> ContractId,
    string BidderTag,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicContracts.bidSubmitted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { BidId.ToTaggedString(), ContractId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class SubmitContractBidCommands
{
    public static readonly ValidationErrorCode ContractNotFound = new("publicContracts.submitBid.contractNotFound");
    public static readonly ValidationErrorCode ContractNotOpen = new("publicContracts.submitBid.contractNotOpen");
    public static readonly ValidationErrorCode BidderNotFound = new("publicContracts.submitBid.bidderNotFound");
    public static readonly ValidationErrorCode BidderDisqualified = new("publicContracts.submitBid.bidderDisqualified");
    public static readonly ValidationErrorCode NegativePrice = new("publicContracts.submitBid.negativePrice");
    public static readonly ValidationErrorCode NegativeInfluence = new("publicContracts.submitBid.negativeInfluence");
    public static readonly ValidationErrorCode InfluenceRequiresHousehold = new("publicContracts.submitBid.influenceRequiresHousehold");
    public static readonly ValidationErrorCode InsufficientInfluence = new("publicContracts.submitBid.insufficientInfluence");
    public static readonly ValidationErrorCode NegativeBribe = new("publicContracts.submitBid.negativeBribe");
    public static readonly ValidationErrorCode BribeRequiresHousehold = new("publicContracts.submitBid.bribeRequiresHousehold");
    public static readonly ValidationErrorCode InsufficientTreasuryForBribe = new("publicContracts.submitBid.insufficientTreasuryForBribe");

    private static readonly LedgerAccountKey BriberySink = new(LedgerAccountKind.System, "publicContracts:bribery");

    public static readonly CommandPipeline<WorldState, SubmitContractBidCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, SubmitContractBidCommand command)
    {
        if (!state.PublicContracts.TryGet(command.ContractId, out var contract))
            return ContractNotFound;
        if (contract!.Status != PublicContractStatus.OpenForBidding)
            return ContractNotOpen;
        if (!ContractBidderResolver.Exists(state, command.Bidder))
            return BidderNotFound;
        if (ContractBidderResolver.IsDisqualified(state, command.Bidder, command.SubmittedDate))
            return BidderDisqualified;
        if (command.PriceOffered < Money.Zero)
            return NegativePrice;
        if (command.InfluenceSpent < 0)
            return NegativeInfluence;

        var hasHousehold = ContractBidderResolver.TryResolveHousehold(state, command.Bidder, out var householdId);

        if (command.InfluenceSpent > 0)
        {
            if (!hasHousehold)
                return InfluenceRequiresHousehold;
            if (command.InfluenceSpent > Clientela.InfluenceResolver.Current(state, householdId))
                return InsufficientInfluence;
        }

        var bribeAmount = command.BribeAmount ?? Money.Zero;
        if (bribeAmount < Money.Zero)
            return NegativeBribe;
        if (bribeAmount > Money.Zero)
        {
            if (!hasHousehold)
                return BribeRequiresHousehold;
            var balance = state.LedgerAccounts.TryGet(LedgerAccountKey.ForHousehold(householdId), out var account) ? account!.Balance : Money.Zero;
            if (balance < bribeAmount)
                return InsufficientTreasuryForBribe;
        }

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, SubmitContractBidCommand command)
    {
        var events = new List<IDomainEvent>();
        var bribeAmount = command.BribeAmount ?? Money.Zero;

        if (ContractBidderResolver.TryResolveHousehold(state, command.Bidder, out var householdId))
        {
            if (command.InfluenceSpent > 0)
                Clientela.InfluenceResolver.Apply(state, householdId, -command.InfluenceSpent);

            if (bribeAmount > Money.Zero)
            {
                events.Add(LedgerService.Post(
                    state, command.SubmittedDate, LedgerTransactionCategory.Gifts,
                    new[]
                    {
                        new LedgerPosting(LedgerAccountKey.ForHousehold(householdId), -bribeAmount),
                        new LedgerPosting(BriberySink, bribeAmount),
                    },
                    reference: $"publicContracts:bribe:{command.CommandId.ToTaggedString()}"));
            }
        }

        var reliability = ContractBidderResolver.ReliabilityScore(state, command.Bidder);

        var bidId = state.ContractBidIds.Issue();
        var bid = new ContractBid(
            bidId, command.ContractId, command.Bidder, command.PriceOffered, reliability, command.InfluenceSpent,
            bribeAmount > Money.Zero, bribeAmount, ContractBidOutcome.Pending, command.SubmittedDate);
        state.ContractBids.Add(bidId, bid);

        events.Add(new ContractBidSubmittedEvent(state.EventIds.Issue(), command.SubmittedDate, bidId, command.ContractId, command.Bidder.ToTaggedString(), command.CommandId.ToTaggedString()));
        return events.ToArray();
    }
}
