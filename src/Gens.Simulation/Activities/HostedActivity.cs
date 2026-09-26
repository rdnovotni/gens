using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Identity;
using Gens.Simulation.Land;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Activities;

/// <summary>§3's two duration modes. Which one applies is a property of the <see
/// cref="ActivityTypeDefinition"/>, never a uniform rule the engine imposes.</summary>
public enum ActivityDurationMode
{
    /// <summary>Resolves entirely within its start month; every Phase runs as a sub-step of that tick.</summary>
    Quick,

    /// <summary>Spans several months, its Phases spread across the real duration — a standing state
    /// that can be interrupted (<see cref="InterruptActivityCommand"/>).</summary>
    Extended,
}

/// <summary>§5.1's Scale axis — how big, public, and visible. Derived at planning time from Guest List
/// size and Venue (<see cref="ActivityCatalog.DeriveScale"/>), never rolled.</summary>
public enum ActivityScaleTier
{
    Intimate,
    Modest,
    Grand,
    Lavish,
}

/// <summary>§5.2's Quality axis — how well the Activity was actually executed. Deliberately the same
/// four-tier output Food Culture's Banquet Quality established; orthogonal to <see
/// cref="ActivityScaleTier"/> (§5.3).</summary>
public enum ActivityQualityTier
{
    Modest,
    Respectable,
    Refined,
    Legendary,
}

/// <summary>An Activity's lifecycle. <see cref="Concluded"/>, <see cref="Cancelled"/>, and <see
/// cref="Interrupted"/> are terminal; the record is kept forever either way, matching every other
/// "resolved or not, kept for the campaign's lifetime" record in this codebase.</summary>
public enum ActivityStatus
{
    Planned,
    InProgress,
    Concluded,
    Cancelled,
    Interrupted,
}

/// <summary>§2's three Venue families: a Villa room (Triclinium, Oecus, Peristylium, Andron,
/// Diaeta, Xenodochium), a settlement civic space (Forum, Circus, Basilica), or an outdoor location
/// (a Hunt's ground).</summary>
public enum ActivityVenueKind
{
    VillaRoom,
    CivicSpace,
    Outdoor,
}

/// <summary>§2's Venue slot. <see cref="Tier"/> is the room's own existing tier for a <see
/// cref="ActivityVenueKind.VillaRoom"/> resolved against a real <see cref="Holding"/> (feeding Quality
/// directly, §5.2/§10's Villa bullet); every other venue, and a rival house's own untracked residence
/// (<see cref="HoldingId"/> null), is tier 1.</summary>
public readonly record struct ActivityVenue(
    ActivityVenueKind Kind,
    string VenueKey,
    RuntimeId<Settlement> SettlementId,
    RuntimeId<Holding>? HoldingId,
    int Tier);

/// <summary>One Type-specific Quality input (§5.2, §11's <c>qualityInputs</c>) — catering for a
/// Feast, game stock for a Hunt — as a 0-100 score. Which keys an Activity must supply, and how each is
/// weighted, is its <see cref="ActivityTypeDefinition"/>'s business.</summary>
public readonly record struct ActivityQualityInput(string Key, int Score);

/// <summary>What kind of real key moment (§9's Activity Record) a Phase produced.</summary>
public enum ActivityMomentKind
{
    /// <summary>An ordinary Interaction (Characters §9.1–9.7) initiated between attendees inside a
    /// Phase (§6.1), amplified by Scale.</summary>
    Interaction,

    /// <summary>A contextual Phase incident (§6.2) — see <see cref="ActivityIncidentKind"/>.</summary>
    Incident,

    /// <summary>A guest who accepted specifically to cause trouble (§4.1's sharper case) acting on it.</summary>
    Disruption,
}

/// <summary>§6.2's Phase-anchored incidents, each reusing an existing mechanic rather than inventing a
/// new one: a dispute and a toast move the ordinary relationship web; a flirtation moves the Romance
/// bond (and only between a pair <see cref="Romance.RomanceEligibility"/> already allows).</summary>
public enum ActivityIncidentKind
{
    Dispute,
    Toast,
    Flirtation,
}

/// <summary>One key moment recorded inside a Phase (§6, §11's <c>interactionsInitiated</c>/
/// <c>eventRollResults</c>).</summary>
/// <param name="Magnitude">The signed opinion (or affection) swing actually applied, after Scale's
/// witness amplification — read by <see cref="ActivityOutcomeResolver"/> as a Quality nudge.</param>
public sealed record ActivityMoment(
    ActivityMomentKind Kind,
    RuntimeId<Character> PrimaryCharacterId,
    RuntimeId<Character>? SecondaryCharacterId,
    ActivityIncidentKind? IncidentKind,
    int Magnitude);

/// <summary>One Phase (§6, §11's <c>ActivityPhase</c>). <see cref="ScheduledDate"/> is fixed at
/// planning time; <see cref="OccurredDate"/> is null until the Phase has actually run.</summary>
public sealed record ActivityPhase(
    string PhaseKey,
    int Sequence,
    GameDate ScheduledDate,
    GameDate? OccurredDate,
    IReadOnlyList<ActivityMoment> Moments)
{
    public bool HasOccurred => OccurredDate is not null;
}

/// <summary>An Interaction an attendee intends to open inside a named Phase, submitted before the
/// Activity begins (<see cref="PerformActivityInteractionCommand"/> against a <see
/// cref="ActivityStatus.Planned"/> Activity) — §12's "set an overall intent and let routine Phases
/// resolve" answer to Quick Activities, whose Phases all run inside one tick.</summary>
public sealed record ActivityPlannedInteraction(
    RuntimeId<Character> InitiatorId,
    RuntimeId<Character> TargetId,
    string PhaseKey,
    int OpinionDelta,
    BondTag BondsGranted);

/// <summary>§9's aggregate Outcome plus its readable Activity Record (§11's <c>ActivityRecord</c>),
/// folded onto the Activity itself rather than kept as a parallel registry.</summary>
public sealed record ActivityOutcome(
    int QualityScore,
    ActivityQualityTier QualityTier,
    int HostDignitasDelta,
    int GuestOpinionDelta,
    int AttendeeCount,
    int WitnessCount,
    string NarrativeSummary);

/// <summary>
/// One deliberately convened, named gathering (Phase 17 item 4; <c>gens-activities-activity-engine-design.md</c>
/// §2, §11) — the shared engine record every future Activity Type (Feasts, Games, Weddings, a formal
/// Symposium) fills in rather than inventing its own gathering model. Keyed by the long-reserved
/// <see cref="Activity"/> runtime ID kind (<see cref="WorldState.ActivityIds"/>). Immutable like every
/// other <c>WorldState</c> record: every change replaces the entry (remove then re-add).
/// </summary>
/// <param name="TypeKey">§2's pluggable Type slot — an <see cref="ActivityTypeDefinition.Key"/>.</param>
/// <param name="HostHouseholdId">Set when a household hosts (the player's own, typically).</param>
/// <param name="HostActorId">Set when a Living World Actor hosts (§8's NPC-hosted Activity). Exactly
/// one of <see cref="HostHouseholdId"/>/<see cref="HostActorId"/> is set.</param>
/// <param name="EndDate">The last month the Activity occupies — equal to <see cref="StartDate"/> for a
/// Quick Activity.</param>
/// <param name="Budget">What the household host pays when the Activity begins (posted to the ledger).
/// Zero for an NPC host, whose economy is band-tracked, not ledgered.</param>
public sealed record HostedActivity(
    RuntimeId<Activity> Id,
    string TypeKey,
    RuntimeId<Character> HostCharacterId,
    RuntimeId<Household>? HostHouseholdId,
    RuntimeId<Actor>? HostActorId,
    ActivityVenue Venue,
    ActivityDurationMode DurationMode,
    GameDate PlannedDate,
    GameDate StartDate,
    GameDate EndDate,
    ActivityScaleTier Scale,
    IReadOnlyList<ActivityQualityInput> QualityInputs,
    Money Budget,
    ActivityStatus Status,
    IReadOnlyList<ActivityPhase> Phases,
    IReadOnlyList<ActivityPlannedInteraction> PlannedInteractions,
    ActivityOutcome? Outcome = null,
    string? TerminationReason = null)
{
    public bool HostIsNpc => HostActorId is not null;

    public bool IsOpen => Status is ActivityStatus.Planned or ActivityStatus.InProgress;

    /// <summary>The next Phase not yet run, or null once every Phase has occurred.</summary>
    public ActivityPhase? NextPhase => Phases.FirstOrDefault(phase => !phase.HasOccurred);
}

/// <summary>Read-side helpers over <see cref="WorldState.Activities"/>, matching the codebase's
/// "a small, hand-curated collection doesn't need a maintained secondary index yet" linear-scan
/// convention (e.g. <see cref="Companions.OverseerResolver"/>).</summary>
public static class ActivityResolver
{
    /// <summary>The Activity this Character is currently hosting (Planned or InProgress), if any.</summary>
    public static HostedActivity? OpenActivityHostedBy(WorldState state, RuntimeId<Character> hostCharacterId)
    {
        foreach (var entry in state.Activities.InAscendingOrder())
            if (entry.Value.IsOpen && entry.Value.HostCharacterId == hostCharacterId)
                return entry.Value;

        return null;
    }

    /// <summary>Every Activity currently <see cref="ActivityStatus.InProgress"/>, in ascending ID order.</summary>
    public static IEnumerable<HostedActivity> InProgress(WorldState state) =>
        state.Activities.InAscendingOrder()
            .Select(entry => entry.Value)
            .Where(activity => activity.Status == ActivityStatus.InProgress);

    /// <summary>Every Activity held during <paramref name="month"/> — still in progress, or concluded
    /// that month — in ascending ID order.</summary>
    public static IEnumerable<HostedActivity> HeldIn(WorldState state, GameDate month) =>
        state.Activities.InAscendingOrder()
            .Select(entry => entry.Value)
            .Where(activity => activity.Status == ActivityStatus.InProgress ||
                               (activity.Status == ActivityStatus.Concluded && activity.EndDate == month));

    /// <summary>Replaces an Activity's stored record (remove then re-add).</summary>
    public static void Replace(WorldState state, HostedActivity activity)
    {
        state.Activities.Remove(activity.Id);
        state.Activities.Add(activity.Id, activity);
    }
}
