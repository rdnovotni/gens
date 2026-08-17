using Gens.Simulation.Identity;
using Gens.Simulation.Land;

namespace Gens.Simulation.Ledger;

/// <summary>
/// The <see cref="WorldState.LedgerAccounts"/> key (Phase 8 items 1-2): which <see
/// cref="LedgerAccountKind"/> plus a tagged-string owner reference, matching how <see
/// cref="Holding.OwnerId"/> already stores cross-cutting ownership as a plain string rather than a
/// single narrow <c>RuntimeId&lt;T&gt;</c> type (an account owner can be a Household, an Actor, a
/// Settlement, or the fixed <see cref="Mint"/> sentinel, so no single <c>RuntimeId&lt;T&gt;</c>
/// phantom type could name them all). No <see cref="RuntimeId{T}"/> is issued for the account itself —
/// unlike <see cref="LedgerTransaction"/>, an account is a mutable running balance keyed by its
/// owner, not a counted entity in its own right, matching how <see cref="Characters.PopGroup"/> is
/// keyed by <see cref="Characters.PopGroupKey"/> rather than its own <c>RuntimeId</c>.
/// </summary>
public readonly record struct LedgerAccountKey(LedgerAccountKind Kind, string OwnerId) : IComparable<LedgerAccountKey>
{
    /// <summary>The single named external/system account new money enters or leaves through (Phase 8's
    /// conservation requirement) — a fixed, well-known ID rather than one issued per campaign, since
    /// there is exactly one of it.</summary>
    public const string MintOwnerId = "mint";

    public static LedgerAccountKey ForHousehold(RuntimeId<Household> householdId) =>
        new(LedgerAccountKind.Household, householdId.ToTaggedString());

    public static LedgerAccountKey ForActor(RuntimeId<Actor> actorId) =>
        new(LedgerAccountKind.Actor, actorId.ToTaggedString());

    public static LedgerAccountKey ForSettlementTreasury(RuntimeId<Settlement> settlementId) =>
        new(LedgerAccountKind.SettlementTreasury, settlementId.ToTaggedString());

    /// <summary>The one system-kind account this campaign uses (<see cref="MintOwnerId"/>), per
    /// <see cref="LedgerAccountKind"/>'s system value.</summary>
    public static readonly LedgerAccountKey Mint = new(LedgerAccountKind.System, MintOwnerId);

    public int CompareTo(LedgerAccountKey other)
    {
        var kindCompare = Kind.CompareTo(other.Kind);
        return kindCompare != 0 ? kindCompare : string.CompareOrdinal(OwnerId, other.OwnerId);
    }

    public static bool operator <(LedgerAccountKey left, LedgerAccountKey right) => left.CompareTo(right) < 0;
    public static bool operator >(LedgerAccountKey left, LedgerAccountKey right) => left.CompareTo(right) > 0;
    public static bool operator <=(LedgerAccountKey left, LedgerAccountKey right) => left.CompareTo(right) <= 0;
    public static bool operator >=(LedgerAccountKey left, LedgerAccountKey right) => left.CompareTo(right) >= 0;
}
