using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Magistracies;

/// <summary>
/// One Character's term in one <see cref="MagistracyOffice"/> at one settlement's Curia (Phase 12 item
/// 2; §11's <c>MagistracyRecord</c> sketch). Kept forever once created, active or not — matching <see
/// cref="Succession.SuccessionDispute"/>'s identical "resolved or not, kept for the campaign's lifetime"
/// convention, since a later system (a future Dynasty Chronicle hook, per §10's own cross-reference)
/// needs the full holding history, not just who currently sits in the seat. <see cref="TermEndDate"/>
/// null is the "currently active" flag — an ended record is never removed, only replaced (remove then
/// re-add under the same <see cref="RecordId"/>) to set it, matching <see
/// cref="Succession.HouseholdHeadship"/>'s identical immutable-record-partition convention.
/// </summary>
/// <param name="CoHolderId">The paired colleague's Character id, for <see
/// cref="MagistracyOffice.Duumvir"/> only — null until <see cref="PairDuumvirsCommand"/> links two
/// independently-won Duumvir seats together. Always null for every other office.</param>
public sealed record MagistracyRecord(
    RuntimeId<MagistracyRecord> RecordId,
    RuntimeId<Character> HolderId,
    MagistracyOffice Office,
    RuntimeId<Settlement> SettlementId,
    GameDate TermStartDate,
    GameDate? TermEndDate = null,
    MagistracyLossReason? LossReason = null,
    RuntimeId<Character>? CoHolderId = null);

/// <summary>Read-side helpers over <see cref="WorldState.MagistracyRecords"/>. Every query here is a
/// linear scan — matching <see cref="Clientela.ClientelaResolver"/>'s identical "a small, hand-curated
/// collection doesn't need a maintained secondary index yet" judgment call; a campaign's total office
/// history across every settlement is nowhere near population scale.</summary>
public static class MagistracyResolver
{
    public static bool IsActive(MagistracyRecord record) => record.TermEndDate is null;

    /// <summary>The active record for a specific holder/office/settlement combination, if any — used to
    /// reject a duplicate appointment/election and to check §5.5's "must already hold Decurion here"
    /// gate.</summary>
    public static MagistracyRecord? ActiveRecord(
        WorldState state, RuntimeId<Settlement> settlementId, MagistracyOffice office, RuntimeId<Character> holderId)
    {
        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (IsActive(record) && record.SettlementId == settlementId && record.Office == office && record.HolderId == holderId)
                return record;
        }

        return null;
    }

    /// <summary>How many active seats a given office currently has filled at a settlement — used by
    /// <see cref="Magistracies.AppointDecurionCommand"/> against <see
    /// cref="MagistracyCatalog.DecurionCuriaSeatCount"/>.</summary>
    public static int ActiveSeatCount(WorldState state, RuntimeId<Settlement> settlementId, MagistracyOffice office)
    {
        var count = 0;
        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
            if (IsActive(entry.Value) && entry.Value.SettlementId == settlementId && entry.Value.Office == office)
                count++;
        return count;
    }

    /// <summary>Every active Decurion at a settlement — §5.6's Curia body, deliberately modeled as a
    /// derived read over active <see cref="MagistracyOffice.Decurion"/> records rather than a second,
    /// denormalized <c>CuriaBody</c> store: "who currently holds Decurion here" already is the seat
    /// roster §11's own <c>CuriaBody.seats</c> sketch describes, so this item stores that fact exactly
    /// once. A seat's Faction (<see cref="Clientela.CharacterFactionResolver"/>) and opinion of the
    /// player (the ordinary <see cref="Relationship.Opinion"/> from that Decurion toward the player's
    /// own household head) are both already-modeled facts this resolver's caller reads separately,
    /// rather than this method duplicating them into a bespoke seat DTO.</summary>
    public static IReadOnlyList<MagistracyRecord> ActiveCuriaSeats(WorldState state, RuntimeId<Settlement> settlementId)
    {
        var seats = new List<MagistracyRecord>();
        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
            if (IsActive(entry.Value) && entry.Value.SettlementId == settlementId && entry.Value.Office == MagistracyOffice.Decurion)
                seats.Add(entry.Value);
        return seats;
    }

    /// <summary>How many active offices (any settlement, any of the four §5 offices) a household
    /// currently holds through its members — <see cref="Clientela.InfluenceCycleSystem"/>'s "plus held
    /// office" Influence generation term (§4.4). Resolves a holder's household via <see
    /// cref="Character.Household"/> directly rather than via <see cref="Succession.HouseholdHeadship"/>:
    /// office-holding belongs to the individual Character (a non-head family member can hold a
    /// magistracy in their own right), not only to whoever currently heads the household.
    ///
    /// <b>Scope note:</b> this is also the resolver Economy &amp; Finance's own flagged
    /// "requiresOffice" Tax Policy gate (§5.3: "holding the local Quaestorship is what actually
    /// satisfies that document's requiresOffice gate") should read once that gate exists — no such gate
    /// was found anywhere in <c>Economy</c> at the time this item was built (only <see
    /// cref="Economy.InsolvencyState"/> and Net Worth exist), so there is nothing to wire it to yet;
    /// a future Economy pass adding that gate should call <see cref="ActiveRecord"/> with <see
    /// cref="MagistracyOffice.QuaestorLocal"/> directly rather than this aggregate count.</summary>
    public static int ActiveOfficeCountForHousehold(WorldState state, RuntimeId<Household> householdId)
    {
        var count = 0;
        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
        {
            if (!IsActive(entry.Value))
                continue;
            if (state.Characters.TryGet(entry.Value.HolderId, out var holder) && holder!.Household == householdId)
                count++;
        }

        return count;
    }

    /// <summary>Phase 15 item 6's Censor eligibility gate (<c>gens-public-contracts-competitive-
    /// bidding-design.md</c> §2: "gated on having already held Duumvir at least once") — unlike <see
    /// cref="ActiveRecord"/>, this checks the full history (active <i>or</i> ended, any settlement), since
    /// §2's own "at least once" is a lifetime achievement, not a currently-held seat. Matches this
    /// resolver's own linear-scan convention.</summary>
    public static bool HasEverHeldOffice(WorldState state, RuntimeId<Character> holderId, MagistracyOffice office)
    {
        foreach (var entry in state.MagistracyRecords.InAscendingOrder())
            if (entry.Value.Office == office && entry.Value.HolderId == holderId)
                return true;
        return false;
    }
}
