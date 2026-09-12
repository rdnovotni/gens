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
/// prerequisite so a trip that transitions to <see cref="TravelTripStatus.Arrived"/> this same month is
/// already visible here (ADR 0004/0005's deterministic same-phase ordering). For every <see
/// cref="StudyAbroadJourney"/> whose <see cref="TravelTrip"/> is <see cref="TravelTripStatus.Arrived"/>:
/// draws the institution's <see cref="InstitutionOfRenown.CostPerMonth"/> from the sponsoring household
/// (mirrors <see cref="Religion.FavorCycleSystem"/>'s unconditional draw shape) and accrues <see
/// cref="StudyAbroadJourney.MonthsAtInstitution"/>. Once that reaches <see
/// cref="InstitutionOfRenown.JourneyDurationMonths"/>: grants a <see cref="CharacterInstitutionCredential"/>,
/// applies a one-time sharp Culture-drift push toward the institution's <see
/// cref="InstitutionOfRenown.PrimeCulturalAssociation"/> (this implementation's own reading of §4's
/// "sharply accelerated... during a Study Abroad Journey" as a lump-sum push applied on completion rather
/// than an ongoing per-month multiplier layered onto <see cref="CulturalDriftSystem"/>'s own separate
/// tick, since a Journey's own fixed <see cref="InstitutionOfRenown.JourneyDurationMonths"/> already
/// bounds how long the acceleration should apply), and transitions the trip directly to <see
/// cref="TravelTripStatus.Returning"/> — the same transition <see cref="BeginReturnCommand.Mutate"/>
/// performs, applied by this system directly per <see cref="Religion.FavorCycleSystem"/>'s "systems write
/// WorldState directly" convention. The return leg reuses the same outbound risk/duration profile,
/// matching <see cref="TravelProgressSystem"/>'s own existing default.
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

        // Materialize first: completing a Journey removes the entry being iterated.
        foreach (var entry in state.StudyAbroadJourneys.InAscendingOrder().ToArray())
        {
            var journey = entry.Value;
            if (!state.TravelTrips.TryGet(journey.TripId, out var trip) || trip!.Status != TravelTripStatus.Arrived)
                continue;
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

            CharacterInstitutionCredentialResolver.Grant(state, journey.CharacterId, journey.InstitutionId, context.Date);
            ApplyStudyAbroadDriftPush(state, journey.CharacterId, institution.PrimeCulturalAssociation);

            state.TravelTrips.Remove(journey.TripId);
            state.TravelTrips.Add(journey.TripId, trip with { MonthsElapsed = 0, Status = TravelTripStatus.Returning, EncounterCompleted = true });
            state.StudyAbroadJourneys.Remove(journey.TripId);

            events.Add(new StudyAbroadCompletedEvent(
                state.EventIds.Issue(), context.Date, journey.TripId, journey.CharacterId, journey.InstitutionId, CausationId: null));
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
