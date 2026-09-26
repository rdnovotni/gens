using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Activities;

namespace Gens.Simulation.Feasts;

/// <summary>Informational-only guidance for one <see cref="FeastPurpose"/> (§6: "each Purpose can carry
/// its own natural default Guest List composition and expected Scale... without changing any of the
/// underlying mechanics — Purpose shapes expectation and reception, not the formula itself"). Read by
/// presentation/UI as a hint; nothing in <see cref="PlanFeastCommands"/> or <see
/// cref="FeastSeatingResolutionSystem"/> enforces it.</summary>
public readonly record struct FeastPurposeGuidance(int TypicalMinGuests, int TypicalMaxGuests, ActivityScaleTier TypicalScale);

/// <summary>
/// Every numeric and definition the Feast domain owns (Phase 17 item 5). Mirrors <see
/// cref="ActivityCatalog"/>'s own role and its identical "explicit, untuned first pass, kept in one
/// place for later balancing" convention — no playtesting data exists yet for any of this game's
/// numbers, Feasts included.
/// </summary>
public static class FeastCatalog
{
    // ---- Phases (§3) -------------------------------------------------------------------------------

    /// <summary>The Feast's own override of the Activity Engine's generic default Phase sequence
    /// (§3: Arrival &amp; Seating, the Meal, the Comissatio, Departure). The Meal's own key is
    /// deliberately the literal string <see cref="ActivityPhaseKeys.MainEvent"/> — not a Feast-specific
    /// name — so <c>ActivityPhaseRunner.DisruptionPhaseIndex</c>'s existing exact-string lookup still
    /// finds it and targets a troublemaker's Disruption at the Meal. Without this reuse, a 4-Phase
    /// sequence with no <c>"mainEvent"</c> key would silently fall back to that method's own
    /// <c>Phases.Count / 2</c> midpoint (index 2, the Comissatio) instead — the wrong Phase per §3.
    /// Phase labels shown to players are unaffected; only the internal key string is shared.</summary>
    public static class FeastPhaseKeys
    {
        public const string ArrivalAndSeating = "arrivalAndSeating";
        public const string Meal = ActivityPhaseKeys.MainEvent;
        public const string Comissatio = "comissatio";
        public const string Departure = "departure";

        public static readonly IReadOnlyList<string> All = new[] { ArrivalAndSeating, Meal, Comissatio, Departure };
    }

    /// <summary>§2's six slots filled in: Quick duration (§2.5: "Quick by default... nested inside a
    /// larger Extended Activity once those future Activity Types exist" — Wedding/Triumph don't exist
    /// yet, so nesting is out of scope here); Villa room or outdoor Venue only (§2.3's venue list —
    /// Triclinium, Oecus, Andron, Gallic Feasting Hall, Viridarium — names no civic space); and Quality
    /// inputs reusing the Activity Engine's own generic 3-input shape verbatim (§5: "no new formula" —
    /// see this class's own file-level remarks on the corrected Food Culture premise).</summary>
    public static readonly ActivityTypeDefinition FeastType = new(
        "feast",
        ActivityDurationMode.Quick,
        FeastPhaseKeys.All,
        new[] { ActivityVenueKind.VillaRoom, ActivityVenueKind.Outdoor },
        new[]
        {
            new ActivityQualityInputDefinition(ActivityTypeCatalog.ProvisioningInput, 2),
            new ActivityQualityInputDefinition(ActivityTypeCatalog.HospitalityInput, 1),
            new ActivityQualityInputDefinition(ActivityTypeCatalog.EntertainmentInput, 1),
        });

    // ---- Venue keys (§2.3) --------------------------------------------------------------------------

    /// <summary>The default Venue key (§2.3), matching <see
    /// cref="NpcActivityHostingSystem.NpcResidenceVenueKey"/>'s own identical convention: a free-text
    /// string, never validated against a Villa room catalog, because no such catalog exists anywhere in
    /// this codebase (<c>Villa.cs</c>'s own doc comment: "Room effects and decoration content
    /// deliberately remain outside the campaign model") — a pre-existing gap from Phase 17 item 4 itself,
    /// not this item's job to fix.</summary>
    public const string DefaultVenueKey = "triclinium";

    // ---- Seating (§4) --------------------------------------------------------------------------------

    /// <summary>§4's seat of honor: the middle couch's highest position.</summary>
    public static readonly (FeastCouch Couch, FeastCouchPosition Position) LocusConsularis = (FeastCouch.Medius, FeastCouchPosition.Highest);

    /// <summary>The 9 physical seats' fixed status ranking, 1 (the locus consularis) through 9 (worst).
    /// Couch order (most to least prestigious) is Medius, Summus, Imus, matching the historical Roman
    /// convention the design doc's own §4 draws on; within a couch, Highest, Middle, Lowest.</summary>
    public static int SeatRank(FeastCouch couch, FeastCouchPosition position)
    {
        var couchRank = couch switch
        {
            FeastCouch.Medius => 0,
            FeastCouch.Summus => 1,
            _ => 2,
        };
        var positionRank = position switch
        {
            FeastCouchPosition.Highest => 0,
            FeastCouchPosition.Middle => 1,
            _ => 2,
        };
        return (couchRank * 3) + positionRank + 1;
    }

    /// <summary>How far a guest's actual seat rank may diverge from their expected rank (§4's own
    /// standing-among-attendees order) before a seating reads as a felt Insult or a deliberate Honor
    /// rather than an ordinary, unremarkable placement.</summary>
    public const int SeatingJudgmentSlack = 1;

    /// <summary>Per rank of gap beyond <see cref="SeatingJudgmentSlack"/>, capped at <see
    /// cref="MaxSeatingOpinionMagnitude"/>/<see cref="MaxSeatingDignitasMagnitude"/>.</summary>
    public const int UnderSeatingOpinionPenaltyPerRank = 4;
    public const int UnderSeatingDignitasPenaltyPerRank = 1;
    public const int OverSeatingOpinionBonusPerRank = 3;
    public const int OverSeatingDignitasBonusPerRank = 1;
    public const int OverSeatingEnvyOpinionPenaltyPerRank = 3;

    public const int MaxSeatingOpinionMagnitude = 20;
    public const int MaxSeatingDignitasMagnitude = 6;

    /// <summary>§9's Scandal source: an under-seated guest whose own expected rank was at or above this
    /// (top-tier expected standing among the Guest List) is a "proud guest" whose insult can escalate.</summary>
    public const int SevereInsultExpectedPositionThreshold = 3;

    /// <summary>The minimum rank gap, beyond <see cref="SeatingJudgmentSlack"/>, for a severe under-seating.</summary>
    public const int SevereInsultGapThreshold = 3;

    // ---- Purpose guidance (§6, informational only) ----------------------------------------------------

    public static readonly IReadOnlyDictionary<FeastPurpose, FeastPurposeGuidance> PurposeGuidance =
        new Dictionary<FeastPurpose, FeastPurposeGuidance>
        {
            [FeastPurpose.PatronageDinner] = new(4, 15, ActivityScaleTier.Modest),
            [FeastPurpose.FuneralFeast] = new(10, 30, ActivityScaleTier.Grand),
            [FeastPurpose.WeddingFeast] = new(15, 40, ActivityScaleTier.Grand),
            [FeastPurpose.ReligiousFestival] = new(10, 30, ActivityScaleTier.Grand),
            [FeastPurpose.TriumphalBanquet] = new(20, 60, ActivityScaleTier.Lavish),
            [FeastPurpose.CompetitiveEuergetismFeast] = new(20, 60, ActivityScaleTier.Lavish),
            [FeastPurpose.OrdinarySocial] = new(2, 12, ActivityScaleTier.Intimate),
        };
}
