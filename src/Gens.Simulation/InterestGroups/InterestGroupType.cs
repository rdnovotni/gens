using Gens.Simulation.Economy;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.InterestGroups;

/// <summary>§2's five real historical coalition types (Phase 12 item 6; <c>gens-interest-groups-design.md</c>).
/// Every real category the design doc names is represented, matching <see
/// cref="Legal.LegalCase.CaseType"/>'s own "every real design-doc source represented, only some
/// reachable" precedent — see <see cref="InterestGroupResolver"/>'s own doc comment for exactly which
/// one this item can actually check membership for.</summary>
public enum InterestGroupType
{
    LandownersVsLandless,
    CreditorsVsDebtors,
    PublicaniEquestrian,
    Veterans,
    ProvincialInterest,
}

/// <summary>
/// Membership is derived, never separately tracked (§4: "read directly from existing household data
/// rather than requiring a separate join action"), so this domain adds no new <see cref="WorldState"/>
/// partition at all — only this read-side resolver over data other, already-shipped domains already
/// own.
///
/// <b>Only <see cref="InterestGroupType.CreditorsVsDebtors"/> is checkable against real per-household
/// data</b>, and only its Debtor half: a household is a real member the moment it holds any <see
/// cref="DebtRecord"/> of its own that is neither <see cref="DebtStatus.Forgiven"/> nor resolved (<see
/// cref="DebtResolution.AssetSeizure"/>). The design doc's own "Creditors vs. Debtors" framing assumes a
/// household can sit on either side, but <see cref="DebtRecord.LenderIsPlayer"/> "always reads false in
/// this implementation" (that record's own doc comment) — every debt's counterparty is the settlement
/// Treasury, never another household — so there is no real opposing household-level Creditor bloc to
/// organize here at all; this resolver deliberately checks only the real Debtor half.
///
/// The other four types are named for schema completeness but every one of them depends on a system
/// this codebase does not build: <see cref="InterestGroupType.LandownersVsLandless"/> and <see
/// cref="InterestGroupType.PublicaniEquestrian"/> need Policies &amp; Edicts' Land Redistribution/
/// Tabulae Novae and a Publicanus Contract respectively, neither of which exist anywhere in this
/// codebase (Policies &amp; Edicts is Phase 12 item 9, not yet started); <see
/// cref="InterestGroupType.Veterans"/> needs a household-level veteran flag distinct from Settlement
/// Demographics' own settlement-aggregate <see cref="Characters.PopGroupType.Veterans"/> pop group,
/// which this codebase has no household-scoped equivalent of; <see
/// cref="InterestGroupType.ProvincialInterest"/> needs Reputation Duality and Starting Regions content,
/// neither of which exist in code yet. <see cref="IsMember"/> throws for all four, matching this
/// codebase's existing "an unreachable enum branch is a real, named gap, not a silent false" discipline
/// rather than returning a misleadingly confident <c>false</c>.
/// </summary>
public static class InterestGroupResolver
{
    public static bool IsMember(WorldState state, RuntimeId<Household> householdId, InterestGroupType groupType) =>
        groupType switch
        {
            InterestGroupType.CreditorsVsDebtors => IsActiveDebtor(state, householdId),
            InterestGroupType.LandownersVsLandless => throw Unreachable(groupType, "Policies & Edicts' Land Redistribution Edict"),
            InterestGroupType.PublicaniEquestrian => throw Unreachable(groupType, "a Publicanus Contract"),
            InterestGroupType.Veterans => throw Unreachable(groupType, "a household-scoped veteran flag"),
            InterestGroupType.ProvincialInterest => throw Unreachable(groupType, "Reputation Duality"),
            _ => throw new ArgumentOutOfRangeException(nameof(groupType), groupType, "Unknown InterestGroupType."),
        };

    private static bool IsActiveDebtor(WorldState state, RuntimeId<Household> householdId)
    {
        foreach (var entry in state.DebtRecords.InAscendingOrder())
        {
            var record = entry.Value;
            if (record.DebtorHouseholdId == householdId && record.Status != DebtStatus.Forgiven &&
                record.Resolution == DebtResolution.None)
                return true;
        }

        return false;
    }

    private static NotSupportedException Unreachable(InterestGroupType groupType, string missingSystem) =>
        new($"{groupType} has no real, checkable membership data yet — it depends on {missingSystem}, which does not exist in this codebase.");
}
