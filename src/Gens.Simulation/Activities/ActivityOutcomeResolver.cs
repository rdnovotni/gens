using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Chronicle;
using Gens.Simulation.Commands;
using Gens.Simulation.Companions;
using Gens.Simulation.Identity;
using Gens.Simulation.Reputation;
using Gens.Simulation.State;
using Gens.Simulation.Time;

namespace Gens.Simulation.Activities;

/// <summary>Emitted when an Activity concludes and its Outcome resolves (§9). Public: how the great
/// gathering went is exactly the kind of thing everyone hears about.</summary>
public sealed record ActivityConcludedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<Activity> ActivityId,
    string TypeKey,
    RuntimeId<Character> HostCharacterId,
    RuntimeId<Household>? HostHouseholdId,
    RuntimeId<Actor>? HostActorId,
    ActivityScaleTier Scale,
    ActivityQualityTier Quality,
    int HostDignitasDelta,
    int AttendeeCount,
    string NarrativeSummary) : IDomainEvent
{
    public string Type => "activities.concluded";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { HostCharacterId.ToTaggedString() };
    public string? CausationId => null;
    public Visibility Visibility => Visibility.Public;
}

/// <summary>
/// §9's Resolution &amp; Outcome: Quality's base payoff, Scale's stakes, and one readable Activity
/// Record. Quality is Food Culture's three-input Banquet Quality formula generalized (§5.2): the
/// Type's weighted inputs, plus the Venue's tier (§2), plus the host's own Phase-quality operators
/// (§10's Companions bullet — a filled Symposiarch, Master of Hospitality, or Head Cook), nudged by
/// what actually happened in the Phases (a troublemaker's disruption, a dispute, a toast). The
/// host's Dignitas moves through the same <see cref="AdjustDignitasCommand"/> every other source uses
/// (an NPC host's own band-tracked <see cref="Actors.LivingWorldActor.Dignitas"/> instead); every
/// attending guest's opinion of the host moves through the ordinary <see cref="RecordInteractionCommand"/>.
/// </summary>
public static class ActivityOutcomeResolver
{
    public static int QualityScore(WorldState state, HostedActivity activity)
    {
        var type = ActivityTypeCatalog.Get(activity.TypeKey);
        var weighted = 0;
        var totalWeight = 0;
        foreach (var definition in type.QualityInputs)
        {
            var input = activity.QualityInputs.First(candidate => string.Equals(candidate.Key, definition.Key, StringComparison.Ordinal));
            weighted += input.Score * definition.Weight;
            totalWeight += definition.Weight;
        }

        var score = weighted / totalWeight;
        score += (activity.Venue.Tier - 1) * ActivityCatalog.VenueTierQualityBonus;

        if (activity.HostHouseholdId is { } householdId)
        {
            var operators = ActivityCatalog.HostOperatorTitles.Count(title => SeniorPositionResolver.IsCurrentlyFilled(state, householdId, title));
            score += Math.Min(operators * ActivityCatalog.HostOperatorQualityBonus, ActivityCatalog.HostOperatorQualityBonusCap);
        }

        foreach (var moment in activity.Phases.SelectMany(phase => phase.Moments))
        {
            score += moment switch
            {
                { Kind: ActivityMomentKind.Disruption } => -ActivityCatalog.DisruptionQualityPenalty,
                { Kind: ActivityMomentKind.Incident, IncidentKind: ActivityIncidentKind.Dispute } => -ActivityCatalog.DisputeQualityPenalty,
                { Kind: ActivityMomentKind.Incident, IncidentKind: ActivityIncidentKind.Toast } => ActivityCatalog.ToastQualityBonus,
                _ => 0,
            };
        }

        return Math.Clamp(score, 0, 100);
    }

    public static void Resolve(WorldState state, RuntimeId<Activity> activityId, GameDate date, List<IDomainEvent> events)
    {
        state.Activities.TryGet(activityId, out var activity);
        var score = QualityScore(state, activity!);
        var quality = ActivityCatalog.QualityTierFor(score);
        var dignitasDelta = ActivityCatalog.BaseHostDignitas(quality) * ActivityCatalog.DignitasStakesMultiplier(activity!.Scale);
        var guestOpinionDelta = ActivityCatalog.GuestOpinionDelta(quality);
        var causation = activityId.ToTaggedString();

        if (activity.HostHouseholdId is { } householdId)
        {
            events.AddRange(AdjustDignitasCommands.Pipeline.Execute(
                state,
                new AdjustDignitasCommand(
                    state.CommandIds.Issue(), "system", date, causation, householdId, dignitasDelta,
                    $"a {activity.Scale} {activity.TypeKey} of {quality} quality")).Events);
        }
        else if (activity.HostActorId is { } actorId && state.Actors.TryGet(actorId, out var actor))
        {
            state.Actors.Remove(actorId);
            state.Actors.Add(actorId, actor! with { Dignitas = actor.Dignitas + dignitasDelta });
        }

        var guests = ActivityInvitationResolver.GuestList(state, activityId).ToArray();
        var attending = guests.Where(invitation => invitation.IsAttending && IsAlive(state, invitation.InviteeId)).ToArray();
        if (IsAlive(state, activity.HostCharacterId))
        {
            foreach (var guest in attending.Where(invitation => !invitation.AttendsToCauseTrouble))
            {
                events.AddRange(RecordInteractionCommands.Pipeline.Execute(
                    state,
                    new RecordInteractionCommand(
                        state.CommandIds.Issue(), "system", date, causation, guest.InviteeId, activity.HostCharacterId,
                        guestOpinionDelta, BondTag.None, BondTag.None, RelationshipOrigin.Encounter)).Events);
            }
        }

        state.Activities.TryGet(activityId, out var latest);
        var witnessCount = ActivityWitnessPool.Of(state, latest!).Count;
        var excludedCount = ActivityInvitationResolver.ForActivity(state, activityId).Count(invitation => invitation.WasExpectedInvite);
        var summary = Summarize(state, latest!, quality, attending.Length, guests.Length, excludedCount);

        var outcome = new ActivityOutcome(score, quality, dignitasDelta, guestOpinionDelta, attending.Length, witnessCount, summary);
        ActivityResolver.Replace(state, latest! with { Status = ActivityStatus.Concluded, Outcome = outcome });

        events.Add(new ActivityConcludedEvent(
            state.EventIds.Issue(), date, activityId, latest!.TypeKey, latest.HostCharacterId, latest.HostHouseholdId,
            latest.HostActorId, latest.Scale, quality, dignitasDelta, attending.Length, summary));
    }

    /// <summary>§9's "lightweight generated summary, mirroring Military &amp; Combat's own Battle
    /// Report... highlighting real key moments rather than a wall of per-guest logs".</summary>
    internal static string Summarize(
        WorldState state, HostedActivity activity, ActivityQualityTier quality, int attended, int invited, int excluded)
    {
        var moments = activity.Phases.SelectMany(phase => phase.Moments).ToArray();
        var highlights = new List<string>();
        AddCount(highlights, moments.Count(m => m.Kind == ActivityMomentKind.Interaction), "exchange", "exchanges");
        AddCount(highlights, moments.Count(m => m.IncidentKind == ActivityIncidentKind.Toast), "toast", "toasts");
        AddCount(highlights, moments.Count(m => m.IncidentKind == ActivityIncidentKind.Dispute), "dispute", "disputes");
        AddCount(highlights, moments.Count(m => m.IncidentKind == ActivityIncidentKind.Flirtation), "flirtation", "flirtations");
        AddCount(highlights, moments.Count(m => m.Kind == ActivityMomentKind.Disruption), "disruption", "disruptions");

        var text = $"A {activity.Scale} {activity.TypeKey} hosted by {ChronicleProjector.Name(state, activity.HostCharacterId)} " +
                   $"at the {activity.Venue.VenueKey}: {attended} of {invited} invited guests attended, and it was judged {quality}.";
        if (highlights.Count > 0)
            text += $" Key moments: {string.Join(", ", highlights)}.";
        if (excluded > 0)
            text += $" {excluded} pointedly left off the guest list.";
        return text;
    }

    private static void AddCount(List<string> highlights, int count, string singular, string plural)
    {
        if (count > 0)
            highlights.Add($"{count} {(count == 1 ? singular : plural)}");
    }

    private static bool IsAlive(WorldState state, RuntimeId<Character> characterId) =>
        state.Characters.TryGet(characterId, out var character) && character.IsAlive;
}
