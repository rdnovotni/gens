using System.Linq;
#nullable enable
using System;
using System.Collections.Generic;
using Gens.Simulation.Characters;
using Gens.Simulation.Commands;
using Gens.Simulation.Identity;
using Gens.Simulation.Ledger;
using Gens.Simulation.State;
using Gens.Simulation.Time;
using Gens.Simulation.Travel;

namespace Gens.Simulation.Education;

/// <summary>
/// The monthly Study Abroad Journey tick (Phase 17 item 2; §4). Runs in the same <see
/// cref="TickPhase.Lifecycle"/> phase as <see cref="TravelProgressSystem"/>, declared as this system's own
/// prerequisite so a trip that transitions status this same month (Arrival, or the return leg's own
/// Completed transition) is already visible here (ADR 0004/0005's deterministic same-phase ordering). For
/// every <see cref="StudyAbroadJourney"/>:
///
/// <list type="bullet">
/// <item>The traveler must still be alive (correctness fix, matching <see
/// cref="Companions.PositionVacancySystem"/>'s own "check the holder is still alive" convention) — a dead
/// or missing Character cancels the Journey outright, with no further charge, progress, or credential,
/// even one whose stay had already finished and was only waiting on its return leg.</item>
/// <item>While <see cref="StudyAbroadJourney.StudyCompleted"/> is false and the trip is <see
/// cref="TravelTripStatus.Arrived"/>: draws the institution's <see
/// cref="InstitutionOfRenown.CostPerMonth"/> from the sponsoring household (mirrors <see
/// cref="Religion.FavorCycleSystem"/>'s unconditional draw shape) and accrues <see
/// cref="StudyAbroadJourney.MonthsAtInstitution"/>. Once that reaches <see
/// cref="InstitutionOfRenown.JourneyDurationMonths"/>: applies a one-time sharp Culture-drift push toward
/// the institution's <see cref="InstitutionOfRenown.PrimeCulturalAssociation"/> (this implementation's own
/// reading of §4's "sharply accelerated... during a Study Abroad Journey" as a lump-sum push applied on
/// stay-completion rather than an ongoing per-month multiplier layered onto <see
/// cref="CulturalDriftSystem"/>'s own separate tick, since a Journey's own fixed <see
/// cref="InstitutionOfRenown.JourneyDurationMonths"/> already bounds how long the acceleration should
/// apply), transitions the trip directly to <see cref="TravelTripStatus.Returning"/> — the same transition
/// <see cref="BeginReturnCommand.Mutate"/> performs, applied by this system directly per <see
/// cref="Religion.FavorCycleSystem"/>'s "systems write WorldState directly" convention — and marks <see
/// cref="StudyAbroadJourney.StudyCompleted"/>, but deliberately does <em>not</em> yet grant the credential
/// or emit completion (see next bullet: correctness fix).</item>
/// <item>While <see cref="StudyAbroadJourney.StudyCompleted"/> is false and the trip is no longer <see
/// cref="TravelTripStatus.Arrived"/> (an early <c>RecallTravelCommand</c> or <c>BeginReturnCommand</c> cut
/// the stay short before it finished): the Journey is dropped with no credential — correctness fix; this
/// side record used to be left behind forever once the trip stopped being <see
/// cref="TravelTripStatus.Arrived"/>, since the original code only ever handled that one status.</item>
/// <item>While <see cref="StudyAbroadJourney.StudyCompleted"/> is true: waits for <see
/// cref="TravelProgressSystem"/> to actually finish the return leg (<see
/// cref="TravelTripStatus.Completed"/>) before granting the permanent <see
/// cref="CharacterInstitutionCredential"/> and emitting <see cref="StudyAbroadCompletedEvent"/> —
/// correctness fix: this used to happen the same month the stay itself finished, while the character was
/// still mid-return with an entire travel leg outstanding, letting them pass credential gates (and
/// Chronicle say they "returned") before they actually had, and letting them keep the credential even if
/// they died crossing home (now covered by this method's own alive-check instead).</item>
/// </list>
///
/// The return leg reuses the same outbound risk/duration profile, matching <see
/// cref="TravelProgressSystem"/>'s own existing default.
/// </summary>
public sealed class StudyAbroadProgressSystem : IMonthlySystem<WorldState>
{
    private static readonly LedgerAccountKey StudyAbroadSink = new(LedgerAccountKind.System, "education:studyAbroad");

    public string Id => "education.studyAbroadProgress";
    public TickPhase Phase => TickPhase.Lifecycle;
    public IReadOnlyCollection<string> Reads { get; } = new[] { "studyAbroadJourneys", "travelTrips", "characters" };
    public IReadOnlyCollection<string> Writes { get; } = new[]
    {
        "studyAbroadJourneys", "travelTrips", "characterInstitutionCredentials", "culturalDriftStates",
        "ledgerAccounts", "ledgerTransactions", "eventIds",
    };
    public IReadOnlyCollection<string> Prerequisites { get; } = new[] { "travel.progress" };

    public IReadOnlyList<IDomainEvent> Tick(WorldState state, MonthlyTickContext context)
    {
        if (state is null)
            throw new ArgumentNullException(nameof(state));

        var events = new List<IDomainEvent>();

        // Materialize first: completing (or cancelling) a Journey removes the entry being iterated.
        foreach (var entry in state.StudyAbroadJourneys.InAscendingOrder().ToArray())
        {
            var journey = entry.Value;

            if (!state.TravelTrips.TryGet(journey.TripId, out var trip))
            {
                // Orphaned: the underlying trip no longer exists. Nothing left to progress or return.
                state.StudyAbroadJourneys.Remove(journey.TripId);
                continue;
            }

            if (!state.Characters.TryGet(journey.CharacterId, out var character) || !character!.IsAlive)
            {
                // The traveler died (or the Character record is gone) — cancel rather than keep charging
                // the household, accruing months, or (if the stay had already finished) granting a
                // credential to someone no longer alive to hold it.
                state.StudyAbroadJourneys.Remove(journey.TripId);
                continue;
            }

            if (journey.StudyCompleted)
            {
                if (trip!.Status != TravelTripStatus.Completed)
                    continue; // Still on the return leg — wait for TravelProgressSystem to finish it.

                CharacterInstitutionCredentialResolver.Grant(state, journey.CharacterId, journey.InstitutionId, context.Date);
                state.StudyAbroadJourneys.Remove(journey.TripId);
                events.Add(new StudyAbroadCompletedEvent(
                    state.EventIds.Issue(), context.Date, journey.TripId, journey.CharacterId, journey.InstitutionId, CausationId: null));
                continue;
            }

            if (trip!.Status != TravelTripStatus.Arrived)
            {
                // Left early (Recalled, or an early BeginReturnCommand) before the stay finished — there
                // is no completed study to credit; drop the side record instead of leaving it behind.
                state.StudyAbroadJourneys.Remove(journey.TripId);
                continue;
            }

            if (!KnownInstitutionsOfRenown.Catalog.TryGet(journey.InstitutionId, out var institution))
                continue;

            var posted = LedgerService.Post(
                state, context.Date, LedgerTransactionCategory.Gifts,
                new[]
                {
                    new LedgerPosting(LedgerAccountKey.ForHousehold(journey.HouseholdId), -institution.CostPerMonth),
                    new LedgerPosting(StudyAbroadSink, institution.CostPerMonth),
                },
                reference: $"education:studyAbroad:{journey.TripId.ToTaggedString()}:{context.Date.TotalMonths}");
            events.Add(posted);

            var monthsAtInstitution = journey.MonthsAtInstitution + 1;
            if (monthsAtInstitution < institution.JourneyDurationMonths)
            {
                state.StudyAbroadJourneys.Remove(journey.TripId);
                state.StudyAbroadJourneys.Add(journey.TripId, journey with { MonthsAtInstitution = monthsAtInstitution });
                continue;
            }

            ApplyStudyAbroadDriftPush(state, journey.CharacterId, institution.PrimeCulturalAssociation);

            state.TravelTrips.Remove(journey.TripId);
            state.TravelTrips.Add(journey.TripId, trip with { MonthsElapsed = 0, Status = TravelTripStatus.Returning, EncounterCompleted = true });
            state.StudyAbroadJourneys.Remove(journey.TripId);
            state.StudyAbroadJourneys.Add(
                journey.TripId, journey with { MonthsAtInstitution = monthsAtInstitution, StudyCompleted = true });
        }

        return events;
    }

    private static void ApplyStudyAbroadDriftPush(WorldState state, RuntimeId<Character> characterId, DefinitionId<Identity.Culture> targetCultureId)
    {
        var push = EducationCulturalDriftCatalog.StudyAbroadAccelerationMultiplier * EducationCulturalDriftCatalog.SlowDriftMonthsPerMonth;
        if (!CulturalDriftResolver.TryGet(state, characterId, out var drift) || drift.TargetCultureId != targetCultureId)
        {
            CulturalDriftResolver.SetTarget(state, characterId, targetCultureId);
            CulturalDriftResolver.TryGet(state, characterId, out drift);
        }

        state.CulturalDriftStates.Remove(characterId);
        state.CulturalDriftStates.Add(characterId, drift with { ProgressMonths = drift.ProgressMonths + push });
    }
}

/// <summary>Emitted when a <see cref="StudyAbroadJourney"/> completes and its credential is granted.</summary>
public sealed record StudyAbroadCompletedEvent(
    RuntimeId<DomainEventEntity> EventId,
    GameDate OccurredDate,
    RuntimeId<TravelTrip> TripId,
    RuntimeId<Character> CharacterId,
    DefinitionId<InstitutionOfRenown> InstitutionId,
    string? CausationId) : IDomainEvent
{
    public string Type => "education.studyAbroadCompleted";
    public int SchemaVersion => 1;
    public IReadOnlyList<string> SubjectIds => new[] { CharacterId.ToTaggedString() };
    public Visibility Visibility => Visibility.Public;
}
