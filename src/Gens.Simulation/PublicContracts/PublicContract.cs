using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.PublicContracts;

/// <summary>§4's three real contract types, matching the data model's own <c>"publicani" |
/// "redemptor" | "militarySupply" | "provincialSupply"</c> four-value vocabulary directly (§4.3 names
/// Military and Provincial Supply as one contract type in prose, but the data model already splits them
/// into two values — this item keeps that split, since a legion's own supply need and a province's
/// administrative one are genuinely different settlements/buyers even when the bidding mechanism is
/// identical). <see cref="Publicani"/> is §4.1's recap of Land Ownership &amp; Real Estate §8 — this
/// item supplies its award mechanism, honestly narrowed: §8 itself was never built (confirmed unbuilt by
/// direct search, matching this phase's own item 1 progress note), so no Collection Intensity dial or
/// Publicanus Contract entity exists for this type to extend; it is scored and awarded exactly like the
/// other two, with no dial of its own.</summary>
public enum PublicContractType
{
    Publicani,
    Redemptor,
    MilitarySupply,
    ProvincialSupply,
}

/// <summary>A contract's own lifecycle stage — open for bids, or currently held.</summary>
public enum PublicContractStatus
{
    OpenForBidding,
    Awarded,
}

/// <summary>
/// §8's <c>PublicContract</c> data model (Phase 15 item 6). Kept forever once opened, matching <see
/// cref="Magistracies.MagistracyRecord"/>'s and every other Phase 12+ "real record as its own tag"
/// entity's identical "resolved or not, kept for the campaign's lifetime" convention — a re-bid at the
/// next Lustrum replaces this same record's holder-facing fields in place (remove-then-re-add, per that
/// convention) rather than closing this one and opening a fresh contract, since the contract itself
/// (the standing state obligation) persists across a re-bid even when its holder changes.
/// </summary>
/// <param name="CurrentHolder">Null while <see cref="PublicContractStatus.OpenForBidding"/>; the
/// winning <see cref="ContractBid.Bidder"/> once <see cref="AwardPublicContractCommand"/>
/// awards it.</param>
/// <param name="ContractValue">§5's Price input, frozen at award time from the winning bid's own <see
/// cref="ContractBid.PriceOffered"/> — <see cref="Money.Zero"/> while open. The anchor §6.1's cutting-
/// corners margin gain and §6.2's restitution both scale against.</param>
/// <param name="AwardedViaLustrum">§3's "mandatory re-bidding of every standing contract... whether or
/// not its current holder is performing well" versus §3's own ad hoc issuance for urgent need — set by
/// <see cref="AwardPublicContractCommand"/>'s own caller-supplied flag.</param>
/// <param name="IsCuttingCorners">§6.1 — hidden (mechanically reachable, not narratively announced)
/// until <see cref="FraudDiscovered"/> flips true.</param>
/// <param name="FraudDiscoveryRisk">§6.1's 0-<see
/// cref="PublicContractsCatalog.FraudDiscoveryRiskThreshold"/> race, advanced monthly by <see
/// cref="ContractFraudDiscoverySystem"/> while <see cref="IsCuttingCorners"/> and not yet <see
/// cref="FraudDiscovered"/>.</param>
public sealed record PublicContract(
    RuntimeId<PublicContract> ContractId,
    PublicContractType Type,
    RuntimeId<Settlement> SettlementId,
    PublicContractStatus Status,
    ContractBidderRef? CurrentHolder,
    Money ContractValue,
    GameDate OpenedDate,
    GameDate? AwardedDate,
    bool AwardedViaLustrum,
    bool IsCuttingCorners,
    bool FraudDiscovered,
    int FraudDiscoveryRisk);

/// <summary>Read-side helpers over <see cref="WorldState.PublicContracts"/>, matching <see
/// cref="Magistracies.MagistracyResolver"/>'s identical "a small, hand-curated collection doesn't need a
/// maintained secondary index yet" linear-scan convention.</summary>
public static class PublicContractResolver
{
    public static bool TryGetCurrent(WorldState state, RuntimeId<PublicContract> contractId, out PublicContract contract) =>
        state.PublicContracts.TryGet(contractId, out contract!);

    /// <summary>Every <see cref="ContractBid"/> still <see cref="ContractBidOutcome.Pending"/> against a
    /// specific contract — the roster <see cref="AwardPublicContractCommand"/> scores.</summary>
    public static IReadOnlyList<ContractBid> PendingBids(WorldState state, RuntimeId<PublicContract> contractId)
    {
        var bids = new List<ContractBid>();
        foreach (var entry in state.ContractBids.InAscendingOrder())
            if (entry.Value.ContractId == contractId && entry.Value.Outcome == ContractBidOutcome.Pending)
                bids.Add(entry.Value);
        return bids;
    }
}
