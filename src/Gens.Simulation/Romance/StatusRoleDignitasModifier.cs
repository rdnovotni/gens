using System.Linq;
#nullable enable
using System;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.State;

namespace Gens.Simulation.Romance;

/// <summary>
/// §13's shared, pure <see cref="LegalStatus"/>-comparison Dignitas modifier — called from both <see
/// cref="AffairDiscoverySystem"/> (at discovery) and <see cref="AdulteryResolutionHook"/> (at
/// conviction, stacked additively on top of that hook's own ordinary consequences), so neither call site
/// reimplements this reading twice.
///
/// <b>This item's own decisive §13 reading (per §17's own "make a call rather than leave this open"
/// guidance):</b> §13 frames the real social risk as landing on "the elite party, in a role status
/// perceives as subordinate" — not simply "whichever party outranks the other pays more." Read plainly,
/// that is exactly the higher-<see cref="LegalStatus"/>-ranked party of the pair: a Roman Citizen
/// carrying on with a Freedman or Peregrine is the one whose own standing takes the real hit for crossing
/// a status line beneath their own rank, while the lower-ranked party has comparatively little standing
/// to lose by the same measure. So this calculator's penalty always lands on the higher-ranked party's
/// own household — <see cref="DetermineHigherRankedParty"/> is the small, additional public surface this
/// type exposes (beyond <see cref="Calculate"/>'s own fixed signature) so both call sites can resolve
/// which household that is without duplicating the rank comparison themselves. Applied identically
/// regardless of which party is which <see cref="Character.Sex"/> — this type never reads <see
/// cref="Character.Sex"/> at all, matching §13's own "applied identically whichever sexes are involved"
/// framing directly, by construction rather than by a branch that happens to do nothing.
/// </summary>
public static class StatusRoleDignitasModifier
{
    /// <summary>The bigger of the two parties' rank gap, times <see
    /// cref="RomanceCatalog.StatusRoleDignitasModifierMagnitude"/>, returned as an already-negative
    /// Dignitas delta (0 when both parties share the same rank — no gap, no modifier). Callers apply
    /// this via <see cref="Reputation.AdjustDignitasCommand"/> against whichever household <see
    /// cref="DetermineHigherRankedParty"/> names, additively on top of whatever else that call site
    /// already applies.</summary>
    public static int Calculate(
        WorldState state, RuntimeId<Character> offenderCharacterId, RuntimeId<Character> thirdPartyCharacterId)
    {
        var gap = Math.Abs(Rank(ResolveStatus(state, offenderCharacterId)) - Rank(ResolveStatus(state, thirdPartyCharacterId)));
        return -(gap * RomanceCatalog.StatusRoleDignitasModifierMagnitude);
    }

    /// <summary>Which of the two parties this modifier's penalty lands on — see this type's own doc
    /// comment for why it is always the higher-<see cref="LegalStatus"/>-ranked party. Returns
    /// <c>null</c> when both parties rank equally (no higher party to name — <see cref="Calculate"/>
    /// itself already returns 0 for that same case, so callers never need to apply a zero delta against
    /// a null household anyway) or when the ranks are equal because one or both Characters could not be
    /// resolved at all.</summary>
    public static RuntimeId<Character>? DetermineHigherRankedParty(
        WorldState state, RuntimeId<Character> offenderCharacterId, RuntimeId<Character> thirdPartyCharacterId)
    {
        var offenderRank = Rank(ResolveStatus(state, offenderCharacterId));
        var thirdPartyRank = Rank(ResolveStatus(state, thirdPartyCharacterId));

        if (offenderRank == thirdPartyRank)
            return null;

        return offenderRank > thirdPartyRank ? offenderCharacterId : thirdPartyCharacterId;
    }

    private static LegalStatus ResolveStatus(WorldState state, RuntimeId<Character> characterId) =>
        state.Characters.TryGet(characterId, out var character) && character is not null
            ? character.LegalStatus
            : LegalStatus.Peregrine;

    /// <summary>A simple, local ordinal reading of <see cref="LegalStatus"/> — no ranking is defined
    /// anywhere else in this codebase (confirmed by inspection), so this type defines its own rather than
    /// promoting one onto the enum itself for a single caller's use. <see cref="LegalStatus.Enslaved"/> is
    /// included only for completeness; it is never actually reached here, since <see
    /// cref="RomanceEligibility.CheckPair"/> already excludes every Enslaved pairing from this module
    /// entirely.</summary>
    private static int Rank(LegalStatus status) => status switch
    {
        LegalStatus.RomanCitizen => 3,
        LegalStatus.LatinRights => 2,
        LegalStatus.Freedman => 1,
        LegalStatus.Peregrine => 0,
        LegalStatus.Enslaved => -1,
        _ => 0,
    };
}
