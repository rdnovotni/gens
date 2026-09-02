using Gens.Simulation.Actors;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.NotableBusinesses;
using Gens.Simulation.RealEstate;
using Gens.Simulation.Reputation;
using Gens.Simulation.Societates;
using Gens.Simulation.State;

namespace Gens.Simulation.PublicContracts;

/// <summary>§5's real bidder roster: "the player's own household, a Notable Business..., a Societas
/// formed specifically to pool capital..., a Merchant Family/Equestrian household, or a generated Rival
/// House competitor." This codebase tracks exactly two real household-like entities — the player's own
/// household and a Rival Gens <see cref="LivingWorldActor"/> (see <see
/// cref="MerchantFamilies.EquestrianStatusQuery"/>'s own identical finding) — so §5's "Merchant
/// Family/Equestrian household" and "generated Rival House competitor" are the same real kind here,
/// distinguished only by whichever of those two happens to clear <see
/// cref="MerchantFamilies.EquestrianStatusQuery"/>'s own threshold, not a fifth bidder kind this
/// codebase has nothing to back. A <see cref="PropertyOwnerRef"/>-shaped Kind+string pair, matching that
/// type's own "no single <c>RuntimeId&lt;T&gt;</c> could name every bidder kind" reasoning, extended
/// with the two real Phase 15 item 2/4 entities (<see cref="Societates.Societas"/>, <see
/// cref="NotableBusiness"/>) <see cref="PropertyOwnerRef"/> itself only ever tracked as narrative-only
/// placeholders (Societas) or not at all (NotableBusiness) at the time it was built.</summary>
public enum ContractBidderKind
{
    /// <summary>The player's own household — <see cref="ContractBidderRef.BidderId"/> is a <see
    /// cref="RuntimeId{Household}"/>'s tagged string.</summary>
    PlayerHousehold,

    /// <summary>A Rival Gens (or, once it clears <see
    /// cref="MerchantFamilies.EquestrianStatusQuery"/>'s threshold, that same house read as a Merchant
    /// Family/Equestrian bidder) — <see cref="ContractBidderRef.BidderId"/> is a <see
    /// cref="RuntimeId{Actor}"/>'s tagged string.</summary>
    RivalHouse,

    /// <summary>A Notable Business, bidding via its own §7 Government Contracts hook (Phase 15 item 4) —
    /// <see cref="ContractBidderRef.BidderId"/> is a <see cref="RuntimeId{NotableBusiness}"/>'s tagged
    /// string.</summary>
    NotableBusiness,

    /// <summary>A Societas formed to pool capital for a large bid (Phase 15 item 2) — <see
    /// cref="ContractBidderRef.BidderId"/> is a <see cref="RuntimeId{Societas}"/>'s tagged
    /// string.</summary>
    Societas,
}

/// <summary>A tagged bidder reference — <see cref="ContractBidderKind"/> + string ID, round-tripping the
/// same way <see cref="PropertyOwnerRef"/> does.</summary>
public readonly record struct ContractBidderRef(ContractBidderKind Kind, string BidderId)
{
    private const string Separator = ":";

    public static ContractBidderRef ForPlayerHousehold(RuntimeId<Household> householdId) =>
        new(ContractBidderKind.PlayerHousehold, householdId.ToTaggedString());

    public static ContractBidderRef ForRivalHouse(RuntimeId<Actor> actorId) =>
        new(ContractBidderKind.RivalHouse, actorId.ToTaggedString());

    public static ContractBidderRef ForNotableBusiness(RuntimeId<NotableBusiness> businessId) =>
        new(ContractBidderKind.NotableBusiness, businessId.ToTaggedString());

    public static ContractBidderRef ForSocietas(RuntimeId<Societas> societasId) =>
        new(ContractBidderKind.Societas, societasId.ToTaggedString());

    public string ToTaggedString() => $"{Kind}{Separator}{BidderId}";

    public static ContractBidderRef Parse(string tagged)
    {
        if (string.IsNullOrWhiteSpace(tagged))
            throw new ArgumentException("A tagged bidder ID is required.", nameof(tagged));

        var splitIndex = tagged.IndexOf(Separator, StringComparison.Ordinal);
        if (splitIndex < 0 || !Enum.TryParse<ContractBidderKind>(tagged[..splitIndex], out var kind))
            throw new FormatException($"'{tagged}' is not a recognized {nameof(ContractBidderRef)}.");

        return new ContractBidderRef(kind, tagged[(splitIndex + Separator.Length)..]);
    }
}

/// <summary>Resolves a <see cref="ContractBidderRef"/> against this codebase's real entities — §5's
/// scoring inputs (Reliability), §5's Influence-spend precondition, and §6.2's prosecution precondition
/// all read through this one shared surface rather than each command switching on <see
/// cref="ContractBidderKind"/> independently.</summary>
public static class ContractBidderResolver
{
    /// <summary>Whether this bidder actually resolves against a real, live entity — the shared
    /// existence check every bid-submission command needs before anything else.</summary>
    public static bool Exists(WorldState state, ContractBidderRef bidder) => bidder.Kind switch
    {
        ContractBidderKind.PlayerHousehold => true, // no central household registry exists to check against (§2's own item 1 finding) — accepted as-given, matching every other PlayerHousehold-kind owner reference in this codebase.
        ContractBidderKind.RivalHouse => state.Actors.TryGet(RuntimeId<Actor>.Parse(bidder.BidderId), out _),
        ContractBidderKind.NotableBusiness => state.NotableBusinesses.TryGet(RuntimeId<NotableBusiness>.Parse(bidder.BidderId), out var business)
            && business!.Status == NotableBusinessStatus.Tracked,
        ContractBidderKind.Societas => state.Societates.TryGet(RuntimeId<Societas>.Parse(bidder.BidderId), out var societas) && societas!.IsActive,
        _ => false,
    };

    /// <summary>§5's Reliability input — "a Notable Business's Reputation..., a household's own
    /// accumulated Dignitas, or simple past-performance history." No per-bidder past-performance log
    /// exists yet (a real, named scope cut — see this domain's own roadmap writeup), so this reads
    /// exactly the two real standing figures §5 names first: <see
    /// cref="NotableBusiness.Reputation"/> directly for a business, <see
    /// cref="DignitasResolver"/>/<see cref="LivingWorldActor.Dignitas"/> for a household-like bidder,
    /// and a Societas's own average partner Dignitas (no Reputation field of its own exists on <see
    /// cref="Societates.Societas"/>) — 0 for any partner that does not itself resolve to one of those
    /// two, matching <see cref="MerchantFamilies.EquestrianStatusQuery"/>'s own identical "only some
    /// owner kinds resolve against a real figure" narrowing.</summary>
    public static int ReliabilityScore(WorldState state, ContractBidderRef bidder)
    {
        switch (bidder.Kind)
        {
            case ContractBidderKind.PlayerHousehold:
                return DignitasResolver.Current(state, RuntimeId<Household>.Parse(bidder.BidderId));

            case ContractBidderKind.RivalHouse:
                return state.Actors.TryGet(RuntimeId<Actor>.Parse(bidder.BidderId), out var actor) ? actor!.Dignitas : 0;

            case ContractBidderKind.NotableBusiness:
                return state.NotableBusinesses.TryGet(RuntimeId<NotableBusiness>.Parse(bidder.BidderId), out var business)
                    ? business!.Reputation
                    : 0;

            case ContractBidderKind.Societas:
                if (!state.Societates.TryGet(RuntimeId<Societas>.Parse(bidder.BidderId), out var societas))
                    return 0;
                var total = 0;
                var counted = 0;
                foreach (var partner in societas!.Partners)
                {
                    if (!TryResolveOwnerScore(state, partner.Owner, out var score))
                        continue;
                    total += score;
                    counted++;
                }

                return counted == 0 ? 0 : total / counted;

            default:
                return 0;
        }
    }

    /// <summary>§5's Influence and §6.1's cutting-corners margin gain both need a real Ledger/Influence
    /// account to move — only a bidder that ultimately resolves to the player's own household actually
    /// has one of either resource this codebase tracks (<see cref="Clientela.HouseholdInfluence"/> and
    /// <see cref="Ledger.LedgerAccountKey.ForHousehold"/> are both household-scoped only). A Rival
    /// House's own <see cref="LivingWorldActor"/> has neither an Influence balance nor a Ledger account
    /// of this shape, so this deliberately narrows further than <see
    /// cref="NotableBusinessOwnerResolver.TryResolveHousehold"/>'s own PlayerHousehold-only precedent
    /// only by definition (that resolver already stops at exactly the same kind).</summary>
    public static bool TryResolveHousehold(WorldState state, ContractBidderRef bidder, out RuntimeId<Household> householdId)
    {
        switch (bidder.Kind)
        {
            case ContractBidderKind.PlayerHousehold:
                householdId = RuntimeId<Household>.Parse(bidder.BidderId);
                return true;

            case ContractBidderKind.NotableBusiness:
                if (state.NotableBusinesses.TryGet(RuntimeId<NotableBusiness>.Parse(bidder.BidderId), out var business) &&
                    NotableBusinessOwnerResolver.TryResolveHousehold(business!.Owner, out householdId))
                    return true;
                break;

            case ContractBidderKind.Societas:
                if (state.Societates.TryGet(RuntimeId<Societas>.Parse(bidder.BidderId), out var societas))
                {
                    foreach (var partner in societas!.Partners)
                    {
                        if (NotableBusinessOwnerResolver.TryResolveHousehold(partner.Owner, out householdId))
                            return true;
                    }
                }

                break;
        }

        householdId = default;
        return false;
    }

    /// <summary>§5's award-time Faction/Clientela skew needs the bidder's own real Character —
    /// resolves through <see cref="NotableBusinessOwnerResolver.TryResolveCharacter"/> directly (already
    /// general over any <see cref="PropertyOwnerRef"/>, not Notable-Business-specific) for every kind
    /// that ultimately traces back to one.</summary>
    public static bool TryResolveCharacter(WorldState state, ContractBidderRef bidder, out RuntimeId<Character> characterId)
    {
        switch (bidder.Kind)
        {
            case ContractBidderKind.PlayerHousehold:
                return NotableBusinessOwnerResolver.TryResolveCharacter(
                    state, PropertyOwnerRef.ForPlayerHousehold(RuntimeId<Household>.Parse(bidder.BidderId)), out characterId);

            case ContractBidderKind.RivalHouse:
                return NotableBusinessOwnerResolver.TryResolveCharacter(
                    state, PropertyOwnerRef.ForRivalGens(RuntimeId<Actor>.Parse(bidder.BidderId)), out characterId);

            case ContractBidderKind.NotableBusiness:
                if (state.NotableBusinesses.TryGet(RuntimeId<NotableBusiness>.Parse(bidder.BidderId), out var business))
                    return NotableBusinessOwnerResolver.TryResolveCharacter(state, business!.Owner, out characterId);
                break;

            case ContractBidderKind.Societas:
                if (state.Societates.TryGet(RuntimeId<Societas>.Parse(bidder.BidderId), out var societas))
                {
                    foreach (var partner in societas!.Partners)
                    {
                        if (NotableBusinessOwnerResolver.TryResolveCharacter(state, partner.Owner, out characterId))
                            return true;
                    }
                }

                break;
        }

        characterId = default;
        return false;
    }

    /// <summary>§6.2's "permanent or long-term disqualification from future contract bidding" —
    /// checked against every <see cref="ContractFraudRecord"/> naming this exact bidder, honoring <see
    /// cref="ContractFraudRecord.DisqualifiedUntilDate"/>'s own null-means-indefinite convention.</summary>
    public static bool IsDisqualified(WorldState state, ContractBidderRef bidder, Time.GameDate asOf)
    {
        foreach (var entry in state.PublicContractFraudRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (record.Holder != bidder || !record.DisqualifiedFromBidding)
                continue;
            if (record.DisqualifiedUntilDate is null || asOf.TotalMonths < record.DisqualifiedUntilDate.Value.TotalMonths)
                return true;
        }

        return false;
    }

    private static bool TryResolveOwnerScore(WorldState state, PropertyOwnerRef owner, out int score)
    {
        switch (owner.Kind)
        {
            case PropertyOwnerKind.PlayerHousehold:
                score = DignitasResolver.Current(state, RuntimeId<Household>.Parse(owner.OwnerId!));
                return true;
            case PropertyOwnerKind.RivalGens:
                score = state.Actors.TryGet(RuntimeId<Actor>.Parse(owner.OwnerId!), out var actor) ? actor!.Dignitas : 0;
                return true;
            default:
                score = 0;
                return false;
        }
    }
}
