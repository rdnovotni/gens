using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Companions;
using Gens.Simulation.Economy;
using Gens.Simulation.Hazards;

namespace Gens.Simulation.Activities;

/// <summary>
/// Every numeric the Activity Engine reads (Phase 17 item 4). The design doc's own §12 leaves "all
/// numeric sizing" open — Scale thresholds, Quality weighting, Witness-Pool risk scaling — so every
/// value here is an explicit, untuned first pass, kept in one place for later balancing, matching <see
/// cref="Romance.RomanceCatalog"/>'s identical convention.
/// </summary>
public static class ActivityCatalog
{
    // ---- Scale (§5.1) ----------------------------------------------------------------------------

    public const int IntimateMaxGuests = 5;
    public const int ModestMaxGuests = 12;
    public const int GrandMaxGuests = 30;

    /// <summary>Civic spaces whose own public character makes any Activity held there inherently
    /// Lavish — §5.1's "a Circus-wide public spectacle is inherently Lavish".</summary>
    public static readonly IReadOnlyList<string> InherentlyLavishCivicVenues = new[] { "circus", "amphitheatre", "theatre" };

    /// <summary>§5.1: set from Guest List size and Venue, never rolled. A civic space lifts any
    /// gathering to at least Grand (and the arenas to Lavish) however few guests were asked.</summary>
    public static ActivityScaleTier DeriveScale(int guestCount, ActivityVenueKind venueKind, string venueKey)
    {
        var bySize = guestCount <= IntimateMaxGuests ? ActivityScaleTier.Intimate
            : guestCount <= ModestMaxGuests ? ActivityScaleTier.Modest
            : guestCount <= GrandMaxGuests ? ActivityScaleTier.Grand
            : ActivityScaleTier.Lavish;

        var venueMinimum = venueKind != ActivityVenueKind.CivicSpace ? ActivityScaleTier.Intimate
            : InherentlyLavishCivicVenues.Contains(venueKey, StringComparer.Ordinal) ? ActivityScaleTier.Lavish
            : ActivityScaleTier.Grand;

        return bySize >= venueMinimum ? bySize : venueMinimum;
    }

    /// <summary>§9's "Scale's Witness-Pool amplification": the percentage an in-Phase Interaction's
    /// or incident's relationship swing is scaled to, because more people saw it.</summary>
    public static int WitnessAmplificationPercent(ActivityScaleTier scale) => scale switch
    {
        ActivityScaleTier.Intimate => 100,
        ActivityScaleTier.Modest => 125,
        ActivityScaleTier.Grand => 150,
        _ => 200,
    };

    /// <summary>§5.3's "Scale buys reach and stakes": the multiplier on the host's Quality-driven
    /// Dignitas payoff — in either direction.</summary>
    public static int DignitasStakesMultiplier(ActivityScaleTier scale) => scale switch
    {
        ActivityScaleTier.Intimate => 1,
        ActivityScaleTier.Modest => 1,
        ActivityScaleTier.Grand => 2,
        _ => 3,
    };

    // ---- Quality (§5.2) --------------------------------------------------------------------------

    public const int RespectableQualityThreshold = 35;
    public const int RefinedQualityThreshold = 60;
    public const int LegendaryQualityThreshold = 80;

    /// <summary>Per Venue tier above 1 — §2's "the Venue's own existing tier... feeds directly into Quality".</summary>
    public const int VenueTierQualityBonus = 5;

    /// <summary>Per currently-filled Phase-quality operator (§10's Companions bullet), capped.</summary>
    public const int HostOperatorQualityBonus = 5;
    public const int HostOperatorQualityBonusCap = 10;

    /// <summary>The existing Senior Positions §10 names as natural Phase-quality operators for a
    /// generic gathering (the Xenodochus/Archimagirus roles map onto Master of Hospitality/Head Cook;
    /// the Editor Muneris is a Games-specific operator left to that Type).</summary>
    public static readonly IReadOnlyList<SeniorPositionTitle> HostOperatorTitles = new[]
    {
        SeniorPositionTitle.Symposiarch,
        SeniorPositionTitle.MasterOfHospitality,
        SeniorPositionTitle.HeadCook,
    };

    public const int DisruptionQualityPenalty = 8;
    public const int DisputeQualityPenalty = 3;
    public const int ToastQualityBonus = 2;

    public static ActivityQualityTier QualityTierFor(int score) =>
        score >= LegendaryQualityThreshold ? ActivityQualityTier.Legendary
        : score >= RefinedQualityThreshold ? ActivityQualityTier.Refined
        : score >= RespectableQualityThreshold ? ActivityQualityTier.Respectable
        : ActivityQualityTier.Modest;

    /// <summary>§9's Quality base payoff, before <see cref="DignitasStakesMultiplier"/>: a badly-run
    /// gathering costs standing, a Legendary one earns real standing.</summary>
    public static int BaseHostDignitas(ActivityQualityTier quality) => quality switch
    {
        ActivityQualityTier.Modest => -2,
        ActivityQualityTier.Respectable => 1,
        ActivityQualityTier.Refined => 3,
        _ => 6,
    };

    /// <summary>Each attending guest's opinion swing toward the host at resolution — Food Culture's
    /// "banquet quality feeds a dinner's relationship effect" (§9).</summary>
    public static int GuestOpinionDelta(ActivityQualityTier quality) => quality switch
    {
        ActivityQualityTier.Modest => -5,
        ActivityQualityTier.Respectable => 2,
        ActivityQualityTier.Refined => 5,
        _ => 10,
    };

    // ---- Invitation / RSVP (§4.1) -----------------------------------------------------------------

    /// <summary>An invitee accepts when their RSVP score reaches this (§4.1: reads opinion, standing,
    /// Faction/Culture).</summary>
    public const int RsvpAcceptThreshold = 0;

    public const int RsvpKinBonus = 25;
    public const int RsvpSpouseBonus = 50;
    public const int RsvpFriendBonus = 20;
    public const int RsvpPatronageBonus = 15;
    public const int RsvpRivalPenalty = 30;

    /// <summary>§4.1's "a Traditionalist Decurion accepts a properly Roman gathering more readily than a
    /// heavily Hellenized one": sharing the host's Culture helps, differing from it hurts, by the same amount.</summary>
    public const int RsvpCultureAffinity = 10;

    /// <summary>A bigger gathering is a bigger draw.</summary>
    public static int RsvpScalePrestigeBonus(ActivityScaleTier scale) => scale switch
    {
        ActivityScaleTier.Grand => 5,
        ActivityScaleTier.Lavish => 10,
        _ => 0,
    };

    // ---- Exclusion (§4.2) ------------------------------------------------------------------------

    /// <summary>§4.2's Insult-equivalent opinion hit for a pointed exclusion. Intimate gatherings never
    /// snub anyone — nobody reasonably expects to be asked to a five-guest dinner.</summary>
    public static int ExclusionOpinionPenalty(ActivityScaleTier scale) => scale switch
    {
        ActivityScaleTier.Intimate => 0,
        ActivityScaleTier.Modest => 10,
        ActivityScaleTier.Grand => 15,
        _ => 20,
    };

    // ---- Phases (§6) -----------------------------------------------------------------------------

    /// <summary>Per-Phase chance of a contextual incident (§6.2), rising with Scale.</summary>
    public static int PhaseIncidentChancePercent(ActivityScaleTier scale) => 25 + ((int)scale * 5);

    /// <summary>Incident-kind roll bands out of 100: below <see cref="ToastBandUpper"/> a Toast, below
    /// <see cref="DisputeBandUpper"/> a Dispute, otherwise a Flirtation.</summary>
    public const int ToastBandUpper = 40;
    public const int DisputeBandUpper = 70;

    public const int DisputeOpinionDelta = -8;
    public const int ToastOpinionDelta = 5;
    public const int FlirtationAffectionDelta = 5;
    public const int FlirtationAttractionDelta = 8;

    /// <summary>A troublemaker's disruption: the opinion swing between them and the host, both ways.</summary>
    public const int DisruptionOpinionDelta = -10;

    // ---- NPC hosting (§8) ------------------------------------------------------------------------

    public const int NpcHostingMonthlyChancePercent = 4;
    public const int NpcInviteOpinionThreshold = 20;
    public const int NpcMaxGuests = 8;
    public const int NpcPlanningLeadMonths = 1;

    /// <summary>A band-tracked rival house's Quality inputs, read from its wealth band (§8's NPC Phase
    /// depth open question answered as "the same engine, inputs summarized from the band").</summary>
    public static int NpcQualityInputScore(HouseholdWealthBand band) => band switch
    {
        HouseholdWealthBand.Ruined => 25,
        HouseholdWealthBand.Modest => 45,
        HouseholdWealthBand.Comfortable => 65,
        _ => 80,
    };

    // ---- Witness Pool (§7) / interruption (§3) ---------------------------------------------------

    /// <summary>Extra monthly Scheme discovery risk when initiator and target are both attending the
    /// same in-progress Activity — §7's shared Witness Pool, scaled by how many people are watching.</summary>
    public static int SharedActivityDiscoveryRiskBonus(ActivityScaleTier scale) => scale switch
    {
        ActivityScaleTier.Intimate => 2,
        ActivityScaleTier.Modest => 5,
        ActivityScaleTier.Grand => 8,
        _ => 12,
    };

    /// <summary>A Natural Disaster in the Venue's settlement at or above this severity interrupts an
    /// in-progress Activity (§3's "genuinely interrupted by an unrelated event").</summary>
    public const DisasterSeverity InterruptingDisasterSeverity = DisasterSeverity.Severe;
}
