using Gens.Simulation.Identity;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Epithets;

/// <summary>
/// A household's real, permanent decision to formally adopt an earned <see cref="Agnomen"/> as a
/// standing part of the family's own cognomen going forward (Phase 11 item 5; <c>gens-epithets-
/// nicknames-titles-design.md</c> §5/§9's <c>InheritedCognomenDecision</c> data model) — the real
/// historical <c>Scipio Africanus</c> pattern, distinct from ordinary property/title succession. Kept
/// forever once recorded, matching <see cref="Succession.SuccessionDispute"/>'s identical "resolved or
/// not, kept for the campaign's lifetime" convention. This item only ever produces an <em>adopting</em>
/// decision — <see cref="AdoptedAsPermanentCognomen"/> is always <c>true</c> for any entry that exists:
/// declining is simply never submitting <see cref="AdoptAgnomenAsCognomenCommand"/> at all, so there is
/// no real "decline" case to record.
/// </summary>
public sealed record InheritedCognomenDecision(
    RuntimeId<InheritedCognomenDecision> DecisionId,
    RuntimeId<Agnomen> OriginalAgnomenId,
    RuntimeId<Household> DecidingHouseholdId,
    bool AdoptedAsPermanentCognomen,
    GameDate EffectiveFromDate);

/// <summary>Resolves which cognomen, if any, a household's own adopted <see
/// cref="InheritedCognomenDecision"/> currently overrides a newborn's generated cognomen with (Phase 11
/// item 5) — read by <see cref="Characters.BirthCharacterCommands"/> at birth, realizing §5's own "changes
/// how every subsequent generation is actually named."</summary>
public static class InheritedCognomenResolver
{
    /// <summary>The most recently adopted decision for <paramref name="householdId"/>, or <c>null</c>
    /// if the household has never adopted one — later adoptions supersede earlier ones rather than
    /// stacking, matching how a real family's own cognomen is one standing name at a time. Takes the
    /// last matching entry found while scanning in ascending <see
    /// cref="Identity.RuntimeId{T}"/> order (ADR 0004) — issuance order, so it stays correct even when
    /// two decisions share the same <see cref="InheritedCognomenDecision.EffectiveFromDate"/> (multiple
    /// commands accepted within the same month), rather than comparing that date directly.</summary>
    public static string? CurrentCognomen(WorldState state, RuntimeId<Household> householdId)
    {
        InheritedCognomenDecision? latest = null;
        foreach (var entry in state.InheritedCognomenDecisions.InAscendingOrder())
        {
            var decision = entry.Value;
            if (decision.DecidingHouseholdId != householdId || !decision.AdoptedAsPermanentCognomen)
                continue;
            latest = decision;
        }

        if (latest is null)
            return null;

        return state.Agnomens.TryGet(latest.OriginalAgnomenId, out var agnomen) ? agnomen!.Name : null;
    }
}
