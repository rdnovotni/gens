using Gens.Simulation.Identity;
using Gens.Simulation.Legal;
using Gens.Simulation.Scandal;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Edicts;

/// <summary>
/// One issued Edict (Phase 12 item 9; §9's own <c>Edict</c> data-model sketch). Kept forever once
/// issued, matching <see cref="ScandalRecord"/>'s and <see cref="LegalCase"/>'s identical "kept for the
/// campaign's lifetime" convention — <see cref="Doctrine.HouseholdDoctrineType.DomusDura"/>'s own
/// "at least one Proscription issued" signal (<see cref="EdictResolver.HasIssuedProscription"/>) needs a
/// household's full Edict history, not just its latest one.
///
/// <b>Household-level issuer, matching <see cref="Reputation.AdjustDignitasCommand"/>'s and <see
/// cref="LegalCase"/>'s own identical convention</b> — §9's own sketch leaves the issuer untyped, and
/// this item resolves every Edict at the same <see cref="Household"/> granularity Dignitas itself
/// already moves at.
/// </summary>
/// <param name="ScandalId">§5.1's Reception, "capable of escalating into a Scheme, Legal &amp; Court
/// case, or Private Feud" — this item routes every real Edict's backlash through Phase 12 item 7's own
/// Scandal engine (per that item's own §1 framing: "not a new consequence system, but the shared
/// engine... a handful of already-shipped Phase 12 moments have been quietly waiting for" — Edicts is
/// exactly such a moment) rather than inventing a second backlash primitive, so this is always set once
/// an Edict is issued.</param>
/// <param name="LegalCaseId">§5.6's own "a plausible Legal &amp; Court challenge to the grant's own
/// validity" — set only when <see cref="GrantCitizenshipEdictCommand"/>'s own optional challenger
/// household actually resolves and files one; null for every other Edict type and for a Citizenship
/// Grant nobody challenges.</param>
/// <param name="DemonstrationEffectTriggered">§5.7's own "every regional Rival House shifts toward
/// Wary or Hostile, not just the target" — true only for a real <see cref="IssueProscriptionCommand"/>
/// that actually found at least one other real, tracked Rival House Actor to shift standing against.</param>
public sealed record EdictRecord(
    RuntimeId<EdictRecord> EdictId,
    RuntimeId<Household> IssuingHouseholdId,
    EdictType Type,
    GameDate IssuedDate,
    int InfluenceCost,
    int DignitasCostToIssue,
    RuntimeId<ScandalRecord> ScandalId,
    RuntimeId<LegalCase>? LegalCaseId = null,
    bool DemonstrationEffectTriggered = false);

/// <summary>Read-side helpers over <see cref="WorldState.EdictRecords"/>, matching <see
/// cref="Legal.LegalCaseResolver"/>'s identical "a small, hand-curated collection doesn't need a
/// maintained secondary index yet" linear-scan convention.</summary>
public static class EdictResolver
{
    /// <summary>Whether <paramref name="householdId"/> has ever issued a real <see
    /// cref="EdictType.Proscription"/> — <see cref="Doctrine.DoctrineResolutionSystem"/>'s own Domus
    /// Dura signal (§3.2: "at least one Proscription issued").</summary>
    public static bool HasIssuedProscription(WorldState state, RuntimeId<Household> householdId)
    {
        foreach (var entry in state.EdictRecords.InAscendingOrder())
            if (entry.Value.IssuingHouseholdId == householdId && entry.Value.Type == EdictType.Proscription)
                return true;

        return false;
    }
}
