using Gens.Simulation.Characters;
using Gens.Simulation.Clientela;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.Legal;
using Gens.Simulation.Magistracies;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>
/// §2's Locatio Censoria in its awarding form, and §5's "genuine multi-actor process" made concrete: a
/// sitting Censor scores every <see cref="ContractBidOutcome.Pending"/> bid against a contract on §5's
/// three real inputs — Price (<see cref="ContractBid.PriceOffered"/>, converted to score via <see
/// cref="PublicContractsCatalog.PriceScoreDivisorDenarii"/>), Reliability (<see
/// cref="ContractBid.ReliabilityScore"/>'s own award-time snapshot), and Influence (<see
/// cref="ContractBid.InfluenceSpent"/> plus a bribe's own converted weight, reusing <see
/// cref="LegalCatalog.BriberyWeightPerTenDenarii"/>'s exact conversion per <see
/// cref="PublicContractsCatalog.BribeScoreWeightPerTenDenarii"/>) — then applies §5's own "not purely
/// mechanical arithmetic": a Faction-alignment bonus when the awarding Censor and a bidder's own resolved
/// Character share a <see cref="PoliticalFaction"/>, a Clientela bonus when they already carry a
/// directed <see cref="BondTag.Patron"/>/<see cref="BondTag.Client"/> bond, and — deterministically,
/// where the Censor carries <see cref="PublicContractsCatalog.CorruptCensorTraitId"/> — a favoritism
/// bonus toward whichever bidder offered the single largest bribe. The highest total score wins (ties
/// broken by ascending <see cref="ContractBid.BidId"/>, ADR 0004).
/// </summary>
public sealed record AwardPublicContractCommand(
    RuntimeId<Command> CommandId,
    string ActorId,
    GameDate SubmittedDate,
    string? CausationId,
    RuntimeId<PublicContract> ContractId,
    RuntimeId<Character> AwardingCensorId,
    bool IsLustrumRebid = false) : ICommand;

public sealed record PublicContractAwardedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<PublicContract> ContractId,
    RuntimeId<ContractBid> WinningBidId,
    string WinnerTag,
    Money AwardedValue,
    int BidsConsidered,
    string? CausationId) : IDomainEvent
{
    public string Type => "publicContracts.contractAwarded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { ContractId.ToTaggedString(), WinningBidId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}

public static class AwardPublicContractCommands
{
    public static readonly ValidationErrorCode ContractNotFound = new("publicContracts.awardContract.contractNotFound");
    public static readonly ValidationErrorCode ContractNotOpen = new("publicContracts.awardContract.contractNotOpen");
    public static readonly ValidationErrorCode NoActiveCensorAtSettlement = new("publicContracts.awardContract.noActiveCensorAtSettlement");
    public static readonly ValidationErrorCode NoPendingBids = new("publicContracts.awardContract.noPendingBids");

    public static readonly CommandPipeline<WorldState, AwardPublicContractCommand> Pipeline = new(
        validate: Validate, mutate: Mutate, issueSequenceNumber: static state => state.IssueCommandSequenceNumber());

    private static ValidationErrorCode? Validate(WorldState state, AwardPublicContractCommand command)
    {
        if (!state.PublicContracts.TryGet(command.ContractId, out var contract))
            return ContractNotFound;
        if (contract!.Status != PublicContractStatus.OpenForBidding)
            return ContractNotOpen;
        if (MagistracyResolver.ActiveRecord(state, contract.SettlementId, MagistracyOffice.Censor, command.AwardingCensorId) is null)
            return NoActiveCensorAtSettlement;
        if (PublicContractResolver.PendingBids(state, command.ContractId).Count == 0)
            return NoPendingBids;

        return null;
    }

    private static IDomainEvent[] Mutate(WorldState state, AwardPublicContractCommand command)
    {
        state.PublicContracts.TryGet(command.ContractId, out var contract);
        var bids = PublicContractResolver.PendingBids(state, command.ContractId);

        var censorFaction = CharacterFactionResolver.Current(state, command.AwardingCensorId);
        var censorIsCorrupt = state.Characters.TryGet(command.AwardingCensorId, out var censor) &&
            censor!.Traits.Contains(PublicContractsCatalog.CorruptCensorTraitId);

        RuntimeId<ContractBid>? largestBriberId = null;
        var largestBribe = Money.Zero;
        if (censorIsCorrupt)
        {
            foreach (var bid in bids)
            {
                if (bid.BribeAmount > largestBribe)
                {
                    largestBribe = bid.BribeAmount;
                    largestBriberId = bid.BidId;
                }
            }
        }

        ContractBid? winner = null;
        var winnerScore = int.MinValue;

        foreach (var bid in bids)
        {
            var score = Score(state, command.AwardingCensorId, censorFaction, largestBriberId, bid);
            if (winner is null || score > winnerScore || (score == winnerScore && bid.BidId.Value < winner.BidId.Value))
            {
                winner = bid;
                winnerScore = score;
            }
        }

        var events = new List<IDomainEvent>();
        foreach (var bid in bids)
        {
            var outcome = bid.BidId == winner!.BidId ? ContractBidOutcome.Won : ContractBidOutcome.Lost;
            state.ContractBids.Remove(bid.BidId);
            state.ContractBids.Add(bid.BidId, bid with { Outcome = outcome });
        }

        state.PublicContracts.Remove(command.ContractId);
        state.PublicContracts.Add(
            command.ContractId,
            contract! with
            {
                Status = PublicContractStatus.Awarded,
                CurrentHolder = winner!.Bidder,
                ContractValue = winner.PriceOffered,
                AwardedDate = command.SubmittedDate,
                AwardedViaLustrum = command.IsLustrumRebid,
            });

        events.Add(new PublicContractAwardedEvent(
            state.EventIds.Issue(), command.SubmittedDate, command.ContractId, winner.BidId, winner.Bidder.ToTaggedString(),
            winner.PriceOffered, bids.Count, command.CommandId.ToTaggedString()));

        return events.ToArray();
    }

    private static int Score(
        WorldState state, RuntimeId<Character> censorId, PoliticalFaction? censorFaction, RuntimeId<ContractBid>? largestBriberId, ContractBid bid)
    {
        var priceScore = -(int)(bid.PriceOffered.RawValue / Money.ScaleFactor / PublicContractsCatalog.PriceScoreDivisorDenarii);
        var bribeWeight = (int)Math.Min(
            bid.BribeAmount.RawValue / Money.ScaleFactor / 10 * PublicContractsCatalog.BribeScoreWeightPerTenDenarii,
            PublicContractsCatalog.MaxBribeScoreWeight);

        var score = bid.ReliabilityScore + bid.InfluenceSpent + bribeWeight + priceScore;

        if (ContractBidderResolver.TryResolveCharacter(state, bid.Bidder, out var bidderCharacterId))
        {
            var bidderFaction = CharacterFactionResolver.Current(state, bidderCharacterId);
            if (censorFaction is not null && bidderFaction == censorFaction)
                score += PublicContractsCatalog.FactionAlignmentScoreBonus;

            if (HasClientelaBond(state, censorId, bidderCharacterId))
                score += PublicContractsCatalog.ClientelaBondScoreBonus;
        }

        if (largestBriberId == bid.BidId)
            score += PublicContractsCatalog.CorruptCensorBribeFavoritismBonus;

        return score;
    }

    private static bool HasClientelaBond(WorldState state, RuntimeId<Character> a, RuntimeId<Character> b)
    {
        var forward = state.Relationships.TryGet(new RelationshipKey(a, b), out var forwardRel) &&
            (forwardRel!.Bonds & (BondTag.Patron | BondTag.Client)) != BondTag.None;
        var backward = state.Relationships.TryGet(new RelationshipKey(b, a), out var backwardRel) &&
            (backwardRel!.Bonds & (BondTag.Patron | BondTag.Client)) != BondTag.None;
        return forward || backward;
    }
}
